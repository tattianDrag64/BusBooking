# Фаза 5 — Отмена бронирования и возврат: план

`ROADMAP.md`: «action для клиента (отменить свою бронь до дедлайна) и для админа (отменить чужую с указанием причины), завязанные на поле `Status`». Прямое продолжение только что закрытого флоу бронирования — на `/bookings` уже есть список заказов пользователя, `OrderStatus.Cancelled` уже существует в enum.

## Текущее состояние (проверено чтением кода)

- `Order` (`Entity/Order.cs`) — нет `StripePaymentIntentId` (только `StripeSessionId`) и нет поля для причины отмены.
- `OrderStatus` (`Entity/OrderStatus.cs`) — `PendingPayment`/`Confirmed`/`Cancelled`/`Expired`, **нет `Refunded`** (комментарий в `ROADMAP.md` уже отмечал, что он не нужен, пока нет отмены — теперь нужен).
- `IPaymentService` — только `CreateCheckoutSession`/`ConstructWebhookEvent`, нет метода возврата.
- `PaymentsController.ConfirmOrderForSession` (webhook) — не сохраняет `PaymentIntentId` сессии, только `order.Status = Confirmed`.
- Логика освобождения места при истечении hold'а (`SeatsController.ExpireStaleHolds`, только что исправлено) удаляет `OrderSeat` вместе с истечением — **отмену нужно делать тем же способом**, иначе тот же баг с уникальным индексом `OrderSeat.SeatDetailId` вернётся для отменённых бронирований.

## 1. Бэкенд: модель данных — ✅ СДЕЛАНО

- **Миграция:** добавить `Order.StripePaymentIntentId` (nullable `string`) и `Order.CancellationReason` (nullable `string`, для admin-отмены — «с указанием причины»).
- **`OrderStatus`:** добавить `Refunded` в enum.
- В `PaymentsController.ConfirmOrderForSession` — сохранять `order.StripePaymentIntentId = session.PaymentIntentId` при подтверждении оплаты (нужен для возврата без лишнего похода в Stripe API за сессией).

**Сделано.** `Refunded` добавлен в конец `OrderStatus` (`= 4`, хранится как `int`, добавление в конец не требует миграции для самого enum). Миграция `AddOrderCancellationFields` добавляет `Orders.CancellationReason`/`Orders.StripePaymentIntentId` (nullable `text`), применена к БД. Сохранение `PaymentIntentId` в вебхуке — в шаге 3, вместе с эндпоинтами отмены (логически одно изменение в `ConfirmOrderForSession`).

## 2. Бэкенд: возврат через Stripe — ✅ СДЕЛАНО

- `IPaymentService`: новый метод `RefundPayment(string paymentIntentId)` — использует `Stripe.RefundService`, по аналогии с `CreateCheckoutSession` (кидает `StripeException` наружу, ловится в контроллере как `502`, тот же паттерн, что уже есть).
- Реализация в `PaymentService.cs`.

**Сделано.** `RefundPayment` в `PaymentService.cs` — `RefundCreateOptions { PaymentIntent = paymentIntentId }` через `RefundService`, тот же стиль, что и `CreateCheckoutSession`. Реальный вызов Stripe API не тестировался (нет эндпоинта, который его вызывает — это шаг 3, плюс в деве всё равно нет настоящего `Stripe:SecretKey`), но сборка чистая и регрессионная проверка (`dotnet build` + живой `signin`, чтобы убедиться, что `ServiceManager`/`Lazy<PaymentService>` по-прежнему конструируется без ошибок) пройдена.

## 3. Бэкенд: эндпоинты отмены — ✅ СДЕЛАНО

Общая внутренняя логика (`BookingsController`, приватный метод `CancelOrder(Order order, string? reason)`), переиспользуемая обоими эндпоинтами:
1. Если `order.Status == Confirmed` (уже оплачен) → `RefundPayment(order.StripePaymentIntentId)` → `order.Status = Refunded`. Если `PaymentIntentId` не сохранён (старый заказ до этой фичи) — ошибка, не молчаливый провал.
2. Если `order.Status == PendingPayment` (ещё не оплачен) → просто `order.Status = Cancelled`, возврата не нужно.
3. Если уже `Cancelled`/`Refunded`/`Expired` → `409 Conflict`, повторно отменить нельзя.
4. Освободить место: удалить связанные `OrderSeat` + сбросить `SeatDetail.IsOccupied`/`IsReserved`/`ReservedUntil` — **тем же способом**, что и в исправленном `ExpireStaleHolds` (переиспользовать существующий `_unitOfWork.OrderSeat.RemoveRange(...)`, добавить сброс `SeatDetail` полей, которого в `ExpireStaleHolds` не было нужно, т.к. там hold и так истёк).

Два эндпоинта поверх общей логики:
- **`POST /api/bookings/{orderId}/cancel`** — JWT, для владельца заказа. Проверка `order.UserId == текущий пользователь`. Дедлайн — разрешено только если `order.Trip.DepartureDate > DateTime.UtcNow` (нельзя отменить уже уехавший рейс), иначе `409`.
- **`POST /api/bookings/{orderId}/admin-cancel`** — `[Authorize(Roles = Admin)]`, тело `{ reason: string }` (обязательно — «с указанием причины»), без проверки владельца и без дедлайна.

**Сделано.** Также обновлён `PaymentsController.ConfirmOrderForSession` — теперь принимает весь `Session` (не только `stripeSessionId`) и сохраняет `order.StripePaymentIntentId = session.PaymentIntentId` при подтверждении оплаты. Добавлены `ISeatDetailRepository.ReleaseSeats` (обратное `ConfirmSeats` — сброс `IsOccupied`/`IsReserved`/`ReservedUntil`) и `IOrderRepository.GetByIdWithDetails` (tracked, с `Include(Trip)`+`Include(OrderSeats)`).

Проверено вживую, 9 сценариев подряд на реальной БД:
1. Отмена `PendingPayment`-заказа → `204`, место освобождено, `OrderSeat` удалён (`SELECT count(*) = 0`).
2. Повторная отмена того же заказа → `409 "Order is already Cancelled"`.
3. Повторное бронирование того же места тем же пользователем после отмены → `200` (регресс-тест на баг с уникальным индексом — не вернулся).
4. Отмена заказа на уже уехавшем рейсе (дата состарена через SQL) → `409 "trip has already departed"`.
5. `admin-cancel` без `reason` в теле → `400` (валидация `[Required]` сработала).
6. `admin-cancel` с `reason` → `204`, причина реально сохранена в БД (`SELECT CancellationReason` подтверждён).
7. `admin-cancel` от не-админа → `403`.
8. Отмена `Confirmed`-заказа с поддельным `StripePaymentIntentId` (нет реального `Stripe:SecretKey` в деве) → возврат падает `StripeException` → `502` с чистым сообщением, **статус заказа остался `Confirmed`, не искажён частично** — подтверждена атомарность (мутации применяются только после успешного возврата).
9. Отмена `Confirmed`-заказа без `StripePaymentIntentId` вообще → `409 "no payment record for this order"`, без попытки похода в Stripe.

## 4. Фронтенд — ✅ СДЕЛАНО

- **`types/order.ts`** — добавить `"Refunded"` в возможные значения статуса (если статус вообще передаётся — сейчас `OrderSummaryVM` его не содержит, см. ниже).
- **Бэкенд-гэп, который нужно закрыть параллельно:** `OrderSummaryVM` (и соответственно `BookingsController.MyBookings`) сейчас **не отдаёт `Status`** — фронтенду не по чему показать "Cancelled"/"Refunded" или решить, показывать ли кнопку отмены. Добавить `Status` в `OrderSummaryVM`.
- **`api/bookings.ts`** — новая функция `cancelBooking(orderId)` → `POST /api/bookings/{orderId}/cancel`, через `authFetch`.
- **`app/bookings/page.tsx`** — кнопка "Cancel booking" на карточках со статусом `PendingPayment`/`Confirmed` (скрыта для уже `Cancelled`/`Refunded`/`Expired`), с подтверждением (`window.confirm` — простое решение для MVP, без кастомного модального компонента). После успешной отмены — обновить список (`getMyBookings()` заново или локально пометить статус).
- Admin-панели для `admin-cancel` на фронтенде **нет и не планируется в этом плане** — `UsersController`/`TripsController` тоже не имеют отдельного admin UI, вся админка сейчас — через Swagger напрямую (это уже сложившийся паттерн проекта, не отклонение).

**Сделано.** `OrderSummaryVM.Status` добавлен и заполняется в `BookingsController.MyBookings` — проверено вживую, сериализуется строкой (`"PendingPayment"`, не число), т.к. `Program.cs` уже настроен на `JsonStringEnumConverter`. `types/order.ts` получил `OrderStatus`-union и поле `status` на `OrderSummary`. `api/bookings.ts` → `cancelBooking`. `app/bookings/` (`useBookingsPageVM.ts` + `page.tsx`) — `loadBookings` вынесен в переиспользуемый `useCallback`, чтобы кнопка отмены могла перезапросить список после успеха; кнопка видна только для `PendingPayment`/`Confirmed` через `CANCELLABLE_STATUSES`, с `window.confirm` и состоянием "Cancelling…" на конкретной карточке (`cancellingId`). Проверено вживую: `npx tsc --noEmit` чистый, `npm run build` проходит, `/bookings` отвечает `200` без ошибок в dev-логах.

## Порядок выполнения

1. Миграция (поля + `Refunded` в enum) — фундамент, ничего не работает без неё.
2. `IPaymentService.RefundPayment` + реализация.
3. `BookingsController`: `ConfirmOrderForSession` — сохранение `PaymentIntentId`, затем оба эндпоинта отмены.
4. `OrderSummaryVM.Status` — закрыть гэп для фронтенда.
5. Фронтенд: `cancelBooking` + кнопка на `/bookings`.

После каждого шага — сборка + проверка вживую (как во всех предыдущих планах), отметка `[+]` здесь и в `ROADMAP.md`.
