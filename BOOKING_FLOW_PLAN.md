# Фаза 5 (бэкенд) + Фаза 7 (фронтенд) — флоу бронирования, план действий

Делаем параллельно, потому что большая часть Фазы 7 «Флоу бронирования» физически не может быть написана без того, что уже готово в Фазе 5 (Stripe Checkout, seat hold, `Order.Status` — всё `[+]` в `ROADMAP.md`), а из Фазы 5 в первую очередь блокирует именно тестирование фронтенда один конкретный баг (пункт 0 ниже). Остальные незакрытые пункты Фазы 5 (e-mail/SMS-уведомления, мультивалютность, водители, автогенерация рейсов, остановки) в этот план **не входят** — они не блокируют флоу бронирования и остаются в `ROADMAP.md` как есть.

## Текущее состояние (проверено чтением кода перед написанием плана)

- Бэкенд: `POST /api/trips/search`, `GET /api/seats/trip/{id}`, `POST /api/seats/book`, `POST /api/payments/checkout/{orderId}`, `GET /api/bookings/my` — все существуют и работают (см. `FRONTEND_PLAN.md` за картой API).
- `PaymentService` уже настроен на `Stripe:SuccessUrl = http://localhost:3000/booking/success`, `Stripe:CancelUrl = http://localhost:3000/booking/cancel` (`appsettings.json`) — **фронтенду нужны страницы именно по этим путям**, иначе Stripe после оплаты/отмены будет редиректить в никуда.
- Фронтенд: страниц флоу бронирования нет вообще (`ls app/` — только `about`, `login`, `signup`, `routes`, `page.tsx`). Нет ни одного клиента для JWT-защищённых эндпоинтов (`api/auth.ts` — единственный API-модуль, без auth-заголовка для остальных).
- `SearchWidget` уже собирает `departure`/`returnDate` в state, но при сабмите **отбрасывает обе даты** и уходит на `/routes?from=&to=` — это статический каталог `RouteInfo` (публичный, без дат), а не поиск конкретных `Trip`. Реальный поиск рейсов с датой — отдельная страница, которой пока нет.

---

## 0. Бэкенд-баг: место навсегда недоступно после истёкшего hold'а — ✅ СДЕЛАНО

Уже зафиксирован в `ROADMAP.md` (Фаза 5). Делаем первым, потому что без этого фикса ручное тестирование seat-picker'а на фронтенде за несколько итераций "убьёт" все тестовые места (сиды ограничены).

**Сделано.** `SeatsController.ExpireStaleHolds` теперь удаляет `OrderSeat` через `_unitOfWork.OrderSeat.RemoveRange(...)` вместе с переводом заказа в `Expired`. Проверено вживую полным циклом: booking → принудительное старение через прямой SQL → `GET /api/seats/trip/{id}` освобождает место и удаляет `OrderSeat` (подтверждено `SELECT count(*)`) → повторный `POST /api/seats/book` на то же место тем же пользователем — успех (раньше падало с `duplicate key value violates unique constraint`).

**Файлы:** `Data/ApplicationDbContext.cs` (`HasIndex(os => os.SeatDetailId).IsUnique()`), `Controllers/SeatsController.cs` (`ExpireStaleHolds`).

**Решение:** удалять `OrderSeat` вместе с истечением hold'а вместо того чтобы просто переводить `Order` в `Expired` — место должно снова стать бронируемым. Шаги:
- В `SeatsController.ExpireStaleHolds` (или в `ISeatDetailRepository.ReleaseExpiredHolds`) после того как заказ переведён в `Expired`, удалить соответствующие `OrderSeat` через `_unitOfWork.OrderSeat.RemoveRange(...)`.
- Migration не нужна (не меняется схема, только логика).
- Проверить вживую: hold → принудительно состарить → повторный `GET /api/seats/trip/{tripId}` → место снова свободно → **второй раз забронировать то же самое место успешно** (это и есть регресс-тест бага, раньше падал с `duplicate key`).

---

## 1. Фронтенд-фундамент: authFetch + типы + API-клиенты — ✅ СДЕЛАНО

Нужно один раз, дальше переиспользуется всеми страницами флоу.

**Сделано.** Все перечисленные ниже файлы созданы как описано, плюс `ApiError` (класс с полем `status`) добавлен в `api/client.ts` — понадобился для различения `409 Conflict` от прочих ошибок в п.3. `npx tsc --noEmit` чистый.

- **`api/authFetch.ts`** — обёртка над `apiFetch` (`api/client.ts`), которая берёт `accessToken` из `auth/tokenStorage.ts` и добавляет `Authorization: Bearer ...`. На `401` — редирект на `/login?redirectTo=<текущий путь>` (паттерн `redirectTo` уже есть в `useLoginPageVM.ts`, просто переиспользуем). Автообновление по `refresh` — сознательно **не** делаем в этом плане (уже отмечено как отложенный пункт в `FRONTEND_AUTH_PLAN.md`, не блокирует).
- **`types/trip.ts`** — `TripSearchResult` (зеркало `TripCreateVM`: `tripId`, `from`, `to`, `departureDate`, `arrivalDate`, `isReturnTrip`, `price`).
- **`types/seat.ts`** — `Seat` (зеркало `SeatDetailVM`: `id`, `seatNumber`, `isOccupied`).
- **`types/order.ts`** — `BookSeatResponse` (`orderId`, `orderCode`, `holdExpiresAt`), `OrderSummary` (зеркало `OrderSummaryVM`), `CheckoutSession` (`checkoutUrl`).
- **`api/trips.ts`** — `searchTrips({ from, to, departureDate, arrivalDate?, isReturnTrip })` → `GET /api/trips/search` (публичный, обычный `apiFetch`, без auth).
- **`api/seats.ts`** — `getSeatsForTrip(tripId)`, `bookSeat({ tripId, seatId })` — оба через `authFetch` (JWT-protected).
- **`api/payments.ts`** — `createCheckoutSession(orderId)` → `POST /api/payments/checkout/{orderId}`, через `authFetch`.
- **`api/bookings.ts`** — `getMyBookings()` → `GET /api/bookings/my`, через `authFetch`.

## 2. Страница результатов поиска рейсов — `/trips` — ✅ СДЕЛАНО

- Обновить `SearchWidget.tsx`: при сабмите передавать `departure`/`returnDate` в query (`departureDate`, `arrivalDate`, `isReturnTrip=!!returnDate`), навигация на `/trips?...` вместо `/routes?...`. Валидация — `from`/`to`/`departureDate` обязательны (как того требует `TripVM` на бэкенде), не давать сабмитнуть без них.
- `app/trips/page.tsx` — серверный компонент (публичный эндпоинт, можно SSR), паттерн как `app/routes/page.tsx`: читает `searchParams`, зовёт `searchTrips(...)`, рендерит список `TripSearchResult` карточками с ценой/временем и кнопкой **"Select seats"** → `/trips/[tripId]/seats`.
- Пустой результат — свой empty-state, как в `RouteExplorer`/`app/routes/page.tsx` ("No trips match this search").

**Сделано, с одной поправкой относительно исходного плана.** `TripsController.SearchTrips` при `isReturnTrip=true` фильтрует по точной `ArrivalDate` *этого же* рейса, а не по «желаемой дате обратного билета» — это не полноценный round-trip поиск (та фича отдельно отложена в `ROADMAP.md`, «Билет туда-обратно со скидкой»). Поэтому `SearchWidget` теперь всегда шлёт `isReturnTrip=false`, поле "Return" в форме остаётся в UI, но не участвует в запросе. Проверено вживую (`dotnet run` + `npm run dev`): `/trips?from=Sofia&to=Berlin&departureDate=2026-10-01` реально отдаёт карточки с городами/ценой с бэкенда, не заглушку.

## 3. Выбор мест — `/trips/[tripId]/seats` — ✅ СДЕЛАНО

- Клиентский компонент (`"use client"`) — обязательно интерактивный (выбор конкретного места), плюс требует JWT.
- Если не залогинен (`useAuth().isAuthenticated === false`) — редирект на `/login?redirectTo=/trips/{tripId}/seats` **до** обращения к `/api/seats/trip/{tripId}` (эндпоинт всё равно `401`-нёт, но лучше не показывать флеш неавторизованного состояния).
- `getSeatsForTrip(tripId)` на маунте — сетка мест, занятые/удержанные (`isOccupied: true` из VM уже учитывает и hold, и реальную занятость, см. `SeatsController.GetSeatsForTrip`) визуально disabled.
- Выбор одного места → кнопка "Book seat" → `bookSeat({ tripId, seatId })`.
- Обработка `409` (место перехвачено кем-то другим) — отдельно от прочих ошибок: показать сообщение, перезапросить список мест (не просто общий error-тост).
- Успех → редирект на `/checkout/[orderId]` с `orderId` из ответа.

**Сделано.** Реализовано как `page.tsx` (использует React 19 `use(params)` для распаковки `Promise<{tripId}>`) + `useSeatsPageVM.ts` (весь стейт/логика — паттерн index+vm). Проверено вживую: страница отвечает `200`, начальный HTML корректен (без ошибок в логах dev-сервера), показывает `"Loading seats"` пока клиентский JS проверяет авторизацию — сам факт клиентского редиректа на `/login` при отсутствии токена не проверялся браузером в этой сессии (curl не выполняет JS), логика идентична уже проверенному паттерну `redirectTo` в `useLoginPageVM.ts`.

**Апдейт: расширено до множественного выбора мест.** Изначально (и по этому плану) флоу поддерживал только одно место на заказ — `BookSeatRequestVM.SeatId: Guid`, `selectedSeatId: string | null` на фронтенде. По запросу пользователя расширено до нескольких мест в одном заказе:

- Бэкенд: `BookSeatRequestVM.SeatIds: List<Guid>`. `ISeatDetailRepository.HoldSeats` — batch-версия `HoldSeat`, **all-or-nothing**: если хоть одно из запрошенных мест недоступно, не удерживается ни одно (иначе заказ бы завис в наполовину забронированном состоянии). `SeatsController.BookSeat` валидирует, что все места принадлежат рейсу, вызывает `HoldSeats`, при конфликте возвращает `409` со списком конкретных недоступных мест. `Order.TotalPrice = trip.Price × количество мест`.
- Фронтенд: `useSeatsPageVM.selectedSeatIds: string[]` + `toggleSeat(id)` вместо одиночного `selectedSeatId`/`setSelectedSeatId`. Кнопка показывает количество выбранных мест ("Book 3 seats").
- `ConfirmOrderForSession`/`CancelOrder` (Фаза 5/`CANCELLATION_REFUND_PLAN.md`) изменений не потребовали — оба уже были написаны через `order.OrderSeats`/`OrderSeat.GetByOrderId` (коллекция), не единичное поле, поэтому многоместные заказы поддержали их «бесплатно».

Проверено вживую: бронирование 3 мест одним заказом → `TotalPrice = 45×3 = 135`, 3 строки `OrderSeats`; попытка забронировать занятое + свободное место вместе → `409` с точным списком конфликтующих id, при этом свободное место **осталось свободным** (all-or-nothing подтверждён — не осталось «утечки» hold'а на месте, которое должно было остаться доступным).

## 4. Чекаут — `/checkout/[orderId]` — ✅ СДЕЛАНО

- Клиентский компонент, JWT-protected (та же логика редиректа на `/login`, что и в п.3 — хотя на этот шаг просто так не попасть без п.3, но прямой переход по URL должен быть тоже защищён).
- На маунте — `createCheckoutSession(orderId)`, при успехе — `window.location.href = checkoutUrl` (реальный редирект на Stripe Checkout, не `router.push` — это внешний домен).
- **Ожидаемая ошибка в деве** — бэкенд без реального `Stripe:SecretKey` вернёт `502` с сообщением `"Payment provider is unavailable right now."` (уже задокументировано и проверено в `ROADMAP.md`, Фаза 5). Страница должна показывать это сообщение как есть, а не generic "something went wrong" — пользователю/тестировщику нужно понимать, что это ожидаемое дев-ограничение, а не баг фронтенда.

**Сделано, с попутным улучшением `api/client.ts`.** `apiFetch` раньше терял тело ответа при ошибке (кидал только `"API ... failed: 502"`) — добавлен разбор `{ message: string }` из JSON-тела ответа (через `res.clone().json()`, чтобы не потребить поток дважды), `ApiError.message` теперь содержит реальное сообщение бэкенда, если оно есть. Проверено вживую отдельным Node-скриптом против настоящего `502`-ответа `/api/payments/checkout/{orderId}` — тело подтверждено как `{ message: "Payment provider is unavailable right now. Please try again shortly." }`, парсинг соответствует. Страница `/checkout/[orderId]` (`page.tsx` + `useCheckoutPageVM.ts`) отвечает `200`, рендерит "Redirecting to payment…" без ошибок в dev-логах.

## 5. Возврат от Stripe — `/booking/success` и `/booking/cancel` — ✅ СДЕЛАНО

Пути жёстко зашиты в бэкенде (`appsettings.json`, см. выше) — **обязательно** такие, не на усмотрение фронтенда.

- `app/booking/success/page.tsx` — читает `?orderId=` из `searchParams`, показывает "Оплата прошла, номер заказа {orderCode}" (можно дёрнуть `GET /api/bookings/my` и найти заказ по `orderId`, либо просто показать общее сообщение с `orderId` без похода в API — проще для первой итерации).
- `app/booking/cancel/page.tsx` — "Оплата отменена", ссылка вернуться к поиску (`/trips`).
- **Важно:** реально долететь до этих страниц через настоящий Stripe Checkout в деве нельзя (нет реального `SecretKey`, см. п.4) — эти страницы тестируются заходом по прямому URL с ручным `?orderId=...`, не через живой Stripe-редирект. Это ограничение дев-окружения, не баг.

**Сделано, взят вариант без похода в API** (общее сообщение с `orderId`, без `GET /api/bookings/my`) — проще, ничего не блокирует, при желании можно усилить позже. Обе страницы — простые серверные компоненты (без `"use client"`, без auth-проверки — намеренно публичные, потому что реальный Stripe-редирект браузера не может приложить JWT). Проверено вживую: `?orderId=abc-123` корректно подставляется в текст на обеих страницах, ошибок в dev-логах нет.

## 6. Мои бронирования — `/bookings` — ✅ СДЕЛАНО

- Паттерн index+vm, как `app/login/` (`page.tsx` + `useBookingsPageVM.ts`).
- JWT-protected, редирект на `/login` если не авторизован.
- `getMyBookings()` → список карточек (`orderCode`, `from → to`, `departureDate`, `seatNumbers`, `totalPrice`).
- Ссылка на эту страницу — из `Header.tsx` (рядом с "Sign out", видна только `isAuthenticated`).

**Сделано.** Ссылка "My bookings" добавлена в `Header.tsx` и в десктопной, и в мобильной навигации, рядом с "Sign out", условие видимости то же (`isAuthenticated`). Проверено вживую: `/bookings` отвечает `200`, рендерится без ошибок (показывает "Loading…" до резолва auth-состояния на клиенте — ожидаемо, как и на остальных JWT-защищённых страницах).

---

## Порядок выполнения

1. Бэкенд-баг (п.0) — блокер для повторного тестирования, короткий.
2. `authFetch` + типы + API-клиенты (п.1) — фундамент, дальше идёт последовательно.
3. `/trips` (п.2) → `/trips/[tripId]/seats` (п.3) → `/checkout/[orderId]` (п.4) — это цепочка, каждый шаг проверяется вживую до перехода к следующему.
4. `/booking/success`+`/booking/cancel` (п.5) — параллельно с п.3–4, не зависит от них по коду.
5. `/bookings` (п.6) — можно в любой момент после п.1, не зависит от остального флоу.

После каждого пункта — сборка (`dotnet build` / `npm run build` + `tsc --noEmit`) и проверка вживую (реальный `dotnet run` + `npm run dev`, не только типы), отметка `[+]` здесь и в `ROADMAP.md`.
