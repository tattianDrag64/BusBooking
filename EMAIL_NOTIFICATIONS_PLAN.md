# Фаза 5 — Email-подтверждение бронирования: план

`ROADMAP.md`, Фаза 5: «сейчас email вообще нигде не отправляется, инфраструктуры для писем в проекте нет» — нужно отправлять клиенту письмо с подтверждением бронирования (детали рейса, место, код заказа) после успешной оплаты.

## Текущее состояние (проверено чтением кода)

- Точка, где заказ становится `Confirmed` — `PaymentsController.ConfirmOrderForSession` (`Controllers/PaymentsController.cs`), вызывается из вебхука Stripe `checkout.session.completed`. Именно сюда нужно добавить отправку письма — это единственное место, где мы точно знаем, что оплата реально прошла.
- Заказ там загружается без деталей: `_unitOfWork.Order.Get(o => o.StripeSessionId == session.Id, tracked: true)` — нет `Include` для `User`/`Trip`/`Route`/`OrderSeats`. Для письма нужны: email клиента, `From`/`To`/`DepartureDate` рейса, номера мест, `OrderCode`, `TotalPrice`.
- `IOrderRepository` уже имеет похожий метод с `Include` — `GetByIdWithDetails(Guid id)` (`Repository/OrderRepository.cs`), но без `.Include(o => o.User)` и без `.ThenInclude(os => os.SeatDetail)` на `OrderSeats`. Нужен либо новый метод, либо расширение существующего — решить по месту (см. шаг 1).
- Пароль/платежи в проекте всегда идут через `IServiceManager` (`Services/ServiceManager/ServiceManager.cs`) — `Lazy<T>`, ручной `new T(...)` в конструкторе, без DI-контейнера напрямую. Новый `IEmailService` должен встраиваться в этот же паттерн, не в отдельный DI-регистрационный путь.
- Секреты (Stripe-ключи) идут через `dotnet user-secrets` в деве и переменные окружения в проде (`docker-compose.yml`, `.env`) — не через `appsettings.json`. То же самое сделать для SMTP-креденшлов.
- В проекте нет ни одной NuGet-зависимости для отправки почты. `System.Net.Mail.SmtpClient` в .NET помечен как устаревший для новых проектов — рекомендуемая замена сегодня: **MailKit** (`NuGet: MailKit`). Работает поверх обычного SMTP, так что подходит для любого провайдера, у которого есть SMTP-релей (SendGrid, Postmark, Amazon SES — у всех трёх есть SMTP-интерфейс, не только HTTP API) — не привязывает код к одному вендору.

## 1. NuGet-пакет и конфигурация

- `dotnet add BusBooking/BusBooking.csproj package MailKit`
- `appsettings.json` — новая секция (только структура, без реальных значений — они через user-secrets/env):
  ```json
  "Email": {
    "SmtpHost": "",
    "SmtpPort": 587,
    "SenderAddress": "",
    "SenderName": "BusBooking"
  }
  ```
- `dotnet user-secrets set "Email:SmtpHost" "..."` и т.д. для дева (по аналогии с `Jwt:SigningKey`/`Stripe:SecretKey`, см. `dotnet user-secrets list` в `BusBooking/`).
- `docker-compose.yml`/`.env.example` — добавить `Email__SmtpHost`, `Email__SmtpUser`, `Email__SmtpPassword` и т.д. по тому же паттерну, что `ConnectionStrings__DefaultConnection` уже используется.
- Логин/пароль SMTP — **не** в `appsettings.json` ни в каком виде (та же причина, что и для `Stripe:SecretKey`).

## 2. `IEmailService`/`EmailService`

- `Services/Interfaces/IEmailService.cs`:
  ```csharp
  Task SendBookingConfirmationAsync(Order order, CancellationToken cancellationToken = default);
  ```
  (принимает саму сущность `Order` с загруженными `User`/`Trip`/`OrderSeats.SeatDetail` — собирать текст письма внутри сервиса, не в контроллере).
- `Services/EmailService.cs` — MailKit `SmtpClient`, `MimeMessage`. Тело письма — простой текст/HTML-заглушка со всеми деталями (Не нужен полноценный шаблонизатор для MVP — конкатенация строк/`$"..."` достаточно, как и остальной код проекта сейчас не использует шаблонные движки).
- Зарегистрировать в `IServiceManager`/`ServiceManager` — **тем же способом**, что `IPasswordBreachChecker` (см. `PasswordBreachChecker`, только что добавлен для HIBP-проверки) — `Lazy<IEmailService> _emailService = new(() => new EmailService(configuration))`.
- **Важно (урок из `PasswordBreachChecker`):** `ServiceManager` — scoped (создаётся заново на каждый HTTP-запрос). Если `EmailService`/MailKit `SmtpClient` держит соединение на уровне поля инстанса — не переиспользовать как statically shared client (MailKit `SmtpClient` не потокобезопасен для конкурентного использования одним инстансом, в отличие от `HttpClient`) — открывать/закрывать SMTP-соединение на каждый вызов `SendBookingConfirmationAsync` — это нормально для объёма writes здесь (одно письмо на одну успешную оплату, не hot path).
- **Fail-open или fail-closed?** — решить самостоятельно, но исходя из паттерна `PasswordBreachChecker` (fail-open при недоступности внешнего сервиса) и общей идеи, что оплата уже прошла и не должна теряться/откатываться из-за письма: **не давать сбою отправки письма сломать подтверждение заказа**. Обернуть вызов в `try/catch` в `PaymentsController`, залогировать `ILogger` при ошибке, не бросать исключение наружу (webhook должен вернуть `200` Stripe'у независимо от письма, иначе Stripe будет ретраить весь вебхук из-за проблемы, не связанной с оплатой).

## 3. Данные для письма — расширить `IOrderRepository`

- Либо новый метод `GetByIdWithFullDetails` (или переиспользовать `GetByIdWithDetails`, добавив `.Include(o => o.User)` и `.ThenInclude(os => os.SeatDetail)` на `OrderSeats` — но `GetByIdWithDetails` уже используется в `BookingsController.Cancel`/`AdminCancel`, где лишние `Include` не вредят, просто не нужны — оценить, стоит ли расширять общий метод или завести отдельный, специфичный под письмо).
- `PaymentsController.ConfirmOrderForSession` сейчас грузит заказ через `_unitOfWork.Order.Get(o => o.StripeSessionId == session.Id, tracked: true)` — заменить на новый/расширенный метод с нужными `Include`, чтобы `order.User.Email`, `order.Trip.From/To/DepartureDate`, `order.OrderSeats[].SeatDetail.SeatNumber` были доступны без дополнительных запросов.

## 4. Вызов из `PaymentsController`

- В конце `ConfirmOrderForSession`, после `_unitOfWork.Save()` (письмо шлём только если сохранение в БД прошло успешно — не хотим слать письмо, а потом упасть на `SaveChanges`):
  ```csharp
  try
  {
      await _services.EmailService.SendBookingConfirmationAsync(order);
  }
  catch (Exception ex)
  {
      _logger.LogWarning(ex, "Failed to send booking confirmation email for order {OrderId}", order.Id);
  }
  ```
- `ConfirmOrderForSession` сейчас синхронный (`private void`) — как и `SignUp` до HIBP-проверки, потребуется сделать `async Task`, и `HandleWebhook` (уже `async Task<IActionResult>`) — `await ConfirmOrderForSessionAsync(session)`.
- `PaymentsController` пока не принимает `ILogger` в конструктор — добавить, по образцу `AuthController(..., ILogger<AuthController> logger)`.

## 5. Проверка вживую (обязательно, по конвенции проекта — см. любой предыдущий `*_PLAN.md`)

Реального SMTP-провайдера в деве не будет (как и реального `Stripe:SecretKey`) — значит, ожидаемое поведение в деве: попытка отправки либо падает с понятной ошибкой (нет настоящих кредов), либо используется тестовый SMTP-сервер. Варианты проверки, выбрать один:

1. **[Рекомендуется] [Mailtrap](https://mailtrap.io) / [Papercut SMTP](https://github.com/ChangemakerStudios/Papercut-SMTP) (локальный SMTP-сервер-заглушка, ловит письма без реальной отправки)** — поднять локально, направить `Email:SmtpHost`/`Port` на него, пройти полный цикл `signup → checkout → webhook` и убедиться, что письмо реально пришло с правильным содержимым (код заказа, рейс, места).
2. Реальный SMTP-провайдер с бесплатным тестовым лимитом (SendGrid/Mailtrap/Postmark sandbox) и реальный тестовый email-адрес.
3. Если ни один из вариантов недоступен — как минимум: искусственно вызвать webhook с валидной подписью на тестовый заказ, убедиться что `try/catch` вокруг письма не ломает подтверждение заказа при намеренно неверном `Email:SmtpHost` (`order.Status` всё равно становится `Confirmed`), залогированная ошибка видна в консоли.

Не отмечать пункт `[+]` в `ROADMAP.md`/этом файле без реальной проверки — по всем предыдущим пунктам план требовал живого теста, не просто чистой сборки.

## Порядок выполнения

1. NuGet-пакет `MailKit` + секция `Email` в `appsettings.json` + user-secrets для дева.
2. `IEmailService`/`EmailService`, регистрация в `ServiceManager`.
3. Расширить/добавить метод в `IOrderRepository` для загрузки деталей заказа под письмо.
4. `PaymentsController.ConfirmOrderForSession` → `async`, вызов `SendBookingConfirmationAsync` в `try/catch`, не ломающий подтверждение заказа при сбое.
5. Живая проверка (Mailtrap/Papercut или аналог) — полный цикл `signup → checkout → webhook → письмо получено`.
6. Отметить `[+]` в `ROADMAP.md` (Фаза 5, пункт «E-mail — реальная отправка») и в этом файле, с описанием, как именно проверено.
