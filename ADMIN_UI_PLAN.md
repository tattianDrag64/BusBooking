# Фаза 7 (расширение) — Admin UI: план

Сейчас вся админка — Swagger напрямую (`ROADMAP.md` это явно фиксирует как «сложившийся паттерн, не отклонение», см. `CANCELLATION_REFUND_PLAN.md`, п.4). Задача — дать реальный UI поверх уже существующих backend admin-эндпоинтов, **плюс закрыть один реальный пробел**, без которого «создать маршрут» с фронтенда невозможно даже в теории (см. ниже).

## Текущее состояние (проверено чтением кода)

### Backend — что уже есть

| Ресурс | Эндпоинты | Файл |
|---|---|---|
| Trips | `GET /api/trips` (paginated), `POST`, `PUT /{id}`, `DELETE /{id}`, `POST /generate` — все `[Authorize(Roles = Admin)]` | `Controllers/TripsController.cs` |
| Schedules | `POST`, `GET /route/{routeId}`, `DELETE /{id}` — контроллер целиком под `[Authorize(Roles = Admin)]` | `Controllers/SchedulesController.cs` |
| Users | `GET /api/users` (список, read-only) — контроллер целиком под `[Authorize(Roles = Admin)]` | `Controllers/UsersController.cs` |
| Bookings (admin) | `POST /api/bookings/{orderId}/admin-cancel` (причина обязательна) | `Controllers/BookingsController.cs` |

### Backend — реальный пробел, который блокирует именно «создать маршрут»

- **`RoutesController` (`Controllers/RoutesController.cs`) — только `GET /api/routes`.** Нет `POST`/`PUT`/`DELETE`. Значит `RouteInfo` сейчас можно создать **только** через сид-данные в `Data/ApplicationDbContext.cs` (`HasData`) или миграцию — не через API вообще. Без этого «Create Route» в админке невозможен независимо от фронтенда.
- **Нет `BusesController` вовсе.** `Bus` (`Entity/Bus.cs` — `BusNumber`, `SeatsCount`, `RouteId`) имеет репозиторий (`IBusRepository`/`BusRepository`), но никакой HTTP-эндпоинт его не создаёт. Это важно, потому что:
  - `TripsController.Create` требует существующий `BusId`.
  - `TripsController.GenerateTrips` **молча пропускает** маршруты без назначенного автобуса (`skippedNoBus` в ответе) — то есть даже создав маршрут и расписание, без автобуса рейсы не сгенерируются.
- Вывод: полный цикл «создать маршрут → назначить автобус → создать расписание → сгенерировать рейсы» требует **сначала** добавить `RoutesController` (POST/PUT/DELETE) и `BusesController` (как минимум POST) на бэкенде — это не фронтенд-задача, но без неё фронтенд не сможет закрыть тот флоу, который прямо запрошен.

### Frontend — что уже есть, на что опираться

- **Роль пользователя уже доступна на клиенте**, ничего доставать с бэкенда дополнительно не нужно: `TokenService.GenerateAccessToken` (бэкенд) кладёт `ClaimTypes.Role = user.Role.ToString()` (`"Admin"`/`"Customer"`) в JWT; `auth/decodeAccessToken.ts` уже парсит этот claim в `AuthUser.role`. Значит `useAuth().user?.role === "Admin"` — готовая проверка, добавлять новый API-вызов не требуется.
- **Guard'а по роли ещё нет.** `FRONTEND_AUTH_PLAN.md` (Этап 6, помечен необязательным для первой итерации) описывал `auth/withAuthGuard.tsx` для проверки `isAuthenticated` — он так и не был реализован, все существующие защищённые страницы (`/bookings`, `/checkout/...`) проверяют `isAuthenticated` точечно в компоненте. Для админки одного `isAuthenticated` недостаточно — нужна проверка `role === "Admin"`, и делать её нужно на **каждой** странице `/admin/*`, значит самое время наконец завести переиспользуемую обёртку, а не копировать проверку в 5+ файлов.
- **Паттерн `index + vm`** (`ARCHITECTURE_NOTES.md`, 11.2) — использовать для всех интерактивных admin-страниц, ровно как `/bookings`/`/login`.
- **`api/*.ts` — один файл на контроллер** (`ARCHITECTURE_NOTES.md`, 11.3) — понадобятся новые `api/adminRoutes.ts` (или расширить `api/routes.ts`), `api/adminBuses.ts`, `api/adminTrips.ts`, `api/adminSchedules.ts`, `api/adminUsers.ts`, все через `authFetch` (JWT уже обязателен для всех этих эндпоинтов).
- **Нет ни одного файла `types/bus.ts`/админских VM-типов на фронтенде** — заводить с нуля по образцу `types/route.ts`.

## 1. Backend: закрыть пробел (обязательно, до фронтенда) — ✅ СДЕЛАНО

- [+] `RoutesController`: добавлены `POST`/`PUT /{id}`/`DELETE /{id}` (`RouteCreateVM { DepartureCity, ArrivalCity, Price }`), все `[Authorize(Roles = Admin)]`. Без проверки на дубликаты — осознанно, не блокирует MVP.
- [+] Новый `BusesController` (`api/buses`) — `GET`/`POST`/`DELETE`, весь контроллер под `[Authorize(Roles = Admin)]`. `POST` проверяет, что `RouteId` существует (`400`, не молчаливый провал).
- [+] Живая проверка полного цикла (curl): создать маршрут → создать автобус → создать расписание → `POST /api/trips/generate` → `created: 18, skippedNoBus: 0`. Плюс проверены `PUT`/`DELETE`/403 для не-админа/401 для неавторизованного.
- [+] **Доп. пробел, найденный уже в процессе фронтенда, не в исходном плане:** `BookingsController` не имел эндпоинта для листинга **всех** бронирований (только `GET /my`, скоуплено на текущего юзера) — без него `/admin/bookings` не из чего было бы строить. Добавлен `GET /api/bookings` (`[Authorize(Roles = Admin)]`) → `AdminOrderSummaryVM` (то же самое + `UserEmail`), `IOrderRepository.GetAllWithDetails()`. Проверено вживую.

## 2. Frontend: guard по роли — ✅ СДЕЛАНО

- [+] `auth/AdminGuard.tsx` (не `withAdminGuard.tsx` — обычный компонент-обёртка `<AdminGuard>{children}</AdminGuard>`, проще для layout-файла, чем HOC): пока `isLoading` — скелетон; `!isAuthenticated` → `/login?redirectTo=/admin`; `isAuthenticated && role !== "Admin"` → `/`.
- [+] Применён в `app/admin/layout.tsx` — один guard на весь раздел, плюс общая боковая навигация по разделам.

## 3. Frontend: структура раздела `/admin` — ✅ СДЕЛАНО

Реализовано ровно по спланированной структуре — по одной странице на ресурс, инлайн-форма создания над таблицей, без отдельных `/new`-роутов:

```
app/admin/
  layout.tsx              — AdminGuard + боковая навигация (Routes/Buses/Schedules/Trips/Users/Bookings)
  routes/page.tsx + useAdminRoutesPageVM.ts
  buses/page.tsx + useAdminBusesPageVM.ts
  schedules/page.tsx + useAdminSchedulesPageVM.ts
  trips/page.tsx + useAdminTripsPageVM.ts       — пагинация + "Generate trips" + удаление (без редактирования — не входило в план)
  users/page.tsx                                — read-only список, без VM-хука
  bookings/page.tsx + useAdminBookingsPageVM.ts — inline textarea для причины отмены (не window.confirm), кнопка видна только для Pending/Confirmed заказов
```

- [+] `types/adminOrder.ts`, `types/adminSchedule.ts`, `types/adminTrip.ts`, `types/adminUser.ts`, `types/bus.ts` — отдельные файлы (пересечение с публичными типами оказалось недостаточным для переиспользования — например, `AdminOrderSummary` содержит `userEmail`, которого нет в публичном `OrderSummary`).
- [+] `api/adminRoutes.ts`, `api/adminBuses.ts`, `api/adminSchedules.ts`, `api/adminTrips.ts`, `api/adminUsers.ts`, `api/adminBookings.ts` — по файлу на контроллер, как и планировалось.
- [+] Ссылка "Admin" в `Header.tsx` (десктоп и мобильное меню) — только при `user?.role === "Admin"`.

## 4. Что осознанно не входит в этот план (не блокирует MVP)

- Редактирование `RouteInfo`/`Bus` после создания — только создание + удаление на первой итерации (backend `PUT` для Route можно добавить, но фронтенд-форму редактирования — по необходимости, не заранее).
- Управление ролями пользователей (`UsersController` сейчас read-only, нет `PUT`/смены роли) — вне рамок этого плана, отдельная задача если понадобится.
- Аудит-лог admin-действий (`ROADMAP.md`, Фаза 2, ещё не сделан отдельным пунктом) — если будет сделан, admin UI ничего не должен менять для этого, он просто станет одним из источников действий, логируемых на бэкенде.

## Порядок выполнения — ✅ ВЕСЬ ПЛАН ВЫПОЛНЕН

Выполнено в спланированном порядке (backend → guard → страницы одна за другой → ссылка в `Header`). Финальная живая проверка — через Playwright, не вручную:
- Админ (`xyz@mail.com`/`1234`): ссылка "Admin" видна в хедере, реально создал маршрут через UI-форму на `/admin/routes` (появился в таблице), прошёлся по всем шести страниц раздела, удалил тестовый маршрут через UI.
- Обычный пользователь (`xyz1@mail.com`/`1234`): ссылка "Admin" в хедере **не** отображается, прямой заход на `/admin/routes` редиректит на `/`.
- Неавторизованный: заход на `/admin/routes` редиректит на `/login?redirectTo=/admin`.

`dotnet build` и `npx tsc --noEmit` чистые на каждом шаге. Тестовые данные, созданные в процессе проверки, удалены — в БД не осталось мусора от прогонов.
