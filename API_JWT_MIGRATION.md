# Переход MVC → Web API + JWT (полный, без гибрида)

Решение принято осознанно (2026-07-10): убрать старые MVC Views и cookie-аутентификацию **сейчас**, не дожидаясь готовности нового фронтенда (Next.js, Фаза 7 `ROADMAP.md`). Это не поэтапный, а разовый переход.

## Следствие, которое нужно держать в уме

**У сайта не будет браузерного UI, пока не появится фронтенд.** Все страницы (`/Users/SignIn`, `/Trip/...` и т.д.) перестанут существовать — останется только JSON API. Для реального клиента это значит: между этим переходом и готовностью Next.js-фронтенда показать ему в браузере будет нечего, кроме Swagger UI (`/swagger`) и ответов через curl/Postman. Раз это осознанный выбор — двигаемся дальше, просто не забывать сообщить об этом клиенту заранее, если он ожидает видеть рабочий сайт в процессе.

## Текущее состояние (проверено в этой сессии)

Уже сделано и работает:
- JWT-инфраструктура: `Jwt`-конфиг, `Jwt:SigningKey` в user-secrets, `RefreshToken` (сущность + миграция + репозиторий)
- `IUnitOfWork`/`UnitOfWork` — `Repository/UnitOfWork/`
- `ITokenService`/`IPasswordHasherService`/`IServiceManager` — `Services/`
- `Controllers/AuthController.cs` — `api/auth`: `signup`, `signin`, `refresh`, `revoke` (уже прямо в `Controllers/`, не в `Controllers/Api/` — раз гибрид отменяется, отдельная подпапка `Api/` больше не нужна вообще, все контроллеры будут API)
- `Program.cs` — сейчас dual-scheme (cookie + JWT), `AddControllersWithViews`, `AddSession`

Остались MVC-контроллеры (под снос): `UsersController.cs`, `TripController.cs`, `SeatDetailController.cs`, `BookingController.cs`, `HomeController.cs`.

---

## Шаг 1 — Написать недостающие API-контроллеры

Прежде чем сносить старые MVC-контроллеры — новые API-эквиваленты должны закрывать ту же функциональность, иначе просто потеряется код без замены.

- [+] `TripsController` (`api/trips`) — покрыт `TripController`: `search` (GET, публичный, без авторизации — решено), `ListTrips`/`Create`/`Edit`(PUT)/`Delete` (admin-only). `Edit` — через `TripEditVM`, без прямого биндинга в `Trip`. Проверено вживую: `search` без токена `200`, `list` без токена `401`, `list` с токеном админа `200`, `list` с токеном обычного юзера `403`
- [ ] `SeatsController` (`api/seats`) или методы внутри `TripsController` — покрыть `SeatDetailController.ChooseSeats` (GET — список мест на рейс, POST — бронирование места + создание `Order`/`OrderSeat`)
- [ ] `BookingsController` (`api/bookings`) — покрыть `BookingController.MyBookings` (список заказов текущего юзера — `ClaimTypes.NameIdentifier` из JWT, не из cookie)
- [ ] `UsersController` (`api/users`) — покрыть `UsersController.Index` (список юзеров, admin-only)
- [ ] На всех защищённых actions — явная схема: `[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]`

**Гоча, встреченная на `TripsController` и закрытая окончательно:** сначала был баг с регистром роли в JWT-claim (`"Admin"` из БД vs `"admin"`-литерал в `[Authorize(Roles=...)]`, скопированный со старой cookie-схемы) — ловил молчаливый `403`. Вместо точечного фикса регистра `User.Role` переведён с `string` на `enum UserRole { Admin, Customer }` (`Entity/UserRole.cs`), с `.HasConversion<string>()` в `ApplicationDbContext` (в БД по-прежнему читаемый текст). Теперь опечатка/рассинхрон регистра невозможны на уровне компиляции:
- В `[Authorize(Roles = ...)]` — `nameof(UserRole.Admin)` (не строковый литерал)
- В `TokenService` — `user.Role.ToString()` при генерации claim
- Везде во внутренней логике — `user.Role == UserRole.Admin`, не сравнение строк
- Миграция `ConvertRoleToEnum` — пустая (тип колонки в БД не изменился), уже применена

## Шаг 2 — Убедиться, что новый API покрывает старый функционал

- [ ] Через Swagger UI или curl пройти каждый эндпоинт выше хотя бы один раз вручную, сверяя с тем, что делал соответствующий старый MVC action
- [ ] Особое внимание — `SeatDetailController.ChooseSeats`: там нетривиальная логика (автогенерация мест при первом обращении к рейсу, создание `Order`+`OrderSeat` одной транзакцией) — не потерять при переносе

## Шаг 3 — Снести старые MVC-контроллеры и Views

- [ ] Удалить `Controllers/UsersController.cs`, `Controllers/TripController.cs`, `Controllers/SeatDetailController.cs`, `Controllers/BookingController.cs`
- [ ] `Controllers/HomeController.cs` — скорее всего тоже под снос (`Privacy`/`Error` — Views-специфичные actions); `Error`-обработку для API заменить на middleware (`app.UseExceptionHandler` уже есть, но сейчас редиректит на `/Home/Error` — Views-путь; заменить на JSON-обработчик ошибок)
- [ ] Удалить папку `Views/` целиком
- [ ] Удалить `wwwroot/css/`, `wwwroot/lib/` (Bootstrap и т.д.) — не нужны без Views. `wwwroot/` в принципе можно удалить целиком, если там нет ничего, что отдаётся API (обычно нет)

## Шаг 4 — `Program.cs`: убрать MVC-инфраструктуру и cookie-схему

- [ ] `AddControllersWithViews()` → `AddControllers()`
- [ ] Убрать блок `AddSession()`/`app.UseSession()` — сессии были нужны только `TempData` в `SeatDetailController`, которого больше нет
- [ ] Убрать `AddCookie(...)` и `DefaultScheme = CookieAuthenticationDefaults...` — оставить только:
  ```csharp
  builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options => { /* как было */ });
  ```
- [ ] Убрать `app.UseStaticFiles()`, если `wwwroot/` снесён
- [ ] Убрать `app.MapControllerRoute(...)` (MVC-роутинг по конвенции) — для API-контроллеров нужен `app.MapControllers()` вместо него
- [ ] Убрать `using Microsoft.AspNetCore.Authentication.Cookies;`, если больше нигде не используется

## Шаг 5 — Проверка

- [ ] `dotnet build` — чисто
- [ ] Через Swagger UI (`/swagger`) пройти весь флоу: `signup` → `signin` → получить токен → вызвать защищённый эндпоинт с `Authorization: Bearer ...` → `refresh` → `revoke`
- [ ] Обновить `run-busbooking` skill (`.claude/skills/run-busbooking/SKILL.md`) — старые URL вида `/Users/SignIn` и скриншот-флоу через форму больше не актуальны; драйвер нужно переписать под curl/fetch к JSON-эндпоинтам вместо Playwright-скриншотов страниц, которых не будет

## Шаг 6 — CORS и Swagger уже готовы

Сделаны раньше, менять не нужно — `AddCors`/`UseCors("Frontend")`, `AddSwaggerGen`/`UseSwagger`/`UseSwaggerUI` продолжают работать без изменений.

---

## Что было в старом плане и больше не актуально

Шаги про "гибрид", "не трогать `app.UseSession()`", "новые Api-контроллеры рядом со старыми, не удаляя их", "тестирование обоих флоу параллельно" — всё это снято, поскольку решение изменилось на полный, единовременный переход.
