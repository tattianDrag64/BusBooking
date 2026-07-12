# Фаза 7 — Frontend (Next.js): план реализации под существующий API

Привязка каждого пункта `ROADMAP.md` Фазы 7 к реально существующим сейчас эндпоинтам (`Controllers/AuthController.cs`, `TripsController.cs`, `SeatsController.cs`, `BookingsController.cs`, `UsersController.cs`). Бэкенд — чистый JSON API, JWT-only, без Views (см. `API_JWT_MIGRATION.md`).

## Текущая карта API (для справки)

| Метод | Путь | Доступ | Тело запроса | Ответ |
|---|---|---|---|---|
| POST | `/api/auth/signup` | публичный | `SignUpVM` | `TokenResponseVM` |
| POST | `/api/auth/signin` | публичный | `SignInVM` | `TokenResponseVM` |
| POST | `/api/auth/refresh` | публичный | `RefreshRequestVM` | `TokenResponseVM` |
| POST | `/api/auth/revoke` | JWT | `RefreshRequestVM` | `204` |
| GET | `/api/trips/search` | публичный | query: `from,to,departureDate,arrivalDate,isReturnTrip` | `TripCreateVM[]` |
| GET | `/api/trips` | admin | — | `TripCreateVM[]` |
| POST | `/api/trips` | admin | `TripCreateVM` | `Trip` |
| PUT | `/api/trips/{id}` | admin | `TripEditVM` | `204` |
| DELETE | `/api/trips/{id}` | admin | — | `204` |
| GET | `/api/seats/trip/{tripId}` | JWT | — | `SeatDetailVM[]` |
| POST | `/api/seats/book` | JWT | `BookSeatRequestVM` | `{ orderId, orderCode }` |
| GET | `/api/bookings/my` | JWT | — | `OrderSummaryVM[]` |
| GET | `/api/users` | admin | — | `UserSummaryVM[]` |
| GET | `/api/routes` | публичный | — | `RouteSummaryVM[]` |

## Пробел — ЗАКРЫТ

`RoutesController` (`api/routes`, публичный) написан и проверен вживую — отдаёт список `RouteInfo` (`DepartureCity`/`ArrivalCity`/`DepartureDay`/`ReturnDay`/`DepartureTime`/`ReturnTime`/`Price`) через `RouteSummaryVM`. Каталог маршрутов для публичных страниц теперь есть, чем питаться.

---

## 1. Скаффолдинг Next.js + TypeScript + Tailwind

```bash
npx create-next-app@latest busbooking-frontend --typescript --tailwind --app --eslint
```
- App Router (не Pages Router) — соответствует технологическому стеку в `ROADMAP.md`
- `NEXT_PUBLIC_API_BASE_URL=http://localhost:5100` (позже — прод-домен API) в `.env.local`

## 2. Публичные страницы (SSR/SSG)

| Страница | Источник данных | Рендеринг |
|---|---|---|
| Home | статика/маркетинг | SSG |
| About Us | статика | SSG |
| Маршруты (каталог направлений) | `GET /api/routes` (см. пробел выше) — или временно уникальные `from/to` из `trips/search` без фильтров | SSR или ISR (цены/направления меняются не каждую секунду — `revalidate: 3600` разумно) |

Ни один из этих эндпоинтов не требует токена — можно рендерить на сервере (`fetch` в Server Component) без прокидывания авторизации.

## 3. Флоу бронирования (поиск → место → оформление → оплата)

Клиентские компоненты (интерактивность, состояние формы, требует токен от середины флоу):

1. **Поиск** — форма (from/to/date/isReturnTrip) → `GET /api/trips/search`. Публичный, можно и без логина показывать результаты.
2. **Выбор рейса** → у пользователя должен быть `tripId` из результатов поиска.
3. **Выбор места** — `GET /api/seats/trip/{tripId}` (**требует JWT** — если юзер не залогинен, на этом шаге редирект на логин/регистрацию, не раньше). Автогенерация мест на бэкенде происходит при первом обращении — фронтенду ничего специально делать не нужно, просто показать, что вернулось.
4. **Оформление** — `POST /api/seats/book` с `{ tripId, seatId }` → возвращает `{ orderId, orderCode }`. Обработать `409` (место уже занято — обновить список мест и попросить выбрать другое) отдельно от прочих ошибок.
5. **Оплата** — **backend этого не поддерживает вообще, пока не сделана Фаза 5** (`ROADMAP.md`: "Оплата" всё ещё `[ ]`). Сейчас бронирование (`POST /api/seats/book`) создаёт `Order` без единой проверки денег. Фронтенд может сверстать экран оплаты как заглушку/визуально, но реальную интеграцию (Stripe и т.п.) подключать не к чему, пока не готов платёжный эндпоинт на бэкенде. **Не блокирует остальной фронтенд — просто помни, что это заглушка, а не рабочий платёж.**

## 4. TanStack Query

Ключи запросов и что за ними стоит:

```ts
useQuery(['trips', 'search', params], () => api.get('/api/trips/search', { params }))
useQuery(['seats', tripId], () => api.get(`/api/seats/trip/${tripId}`), { enabled: !!tripId })
useMutation((body) => api.post('/api/seats/book', body))
useQuery(['bookings', 'my'], () => api.get('/api/bookings/my'), { enabled: isAuthenticated })
```
`seats` — обязательно `staleTime` небольшой (свободные места меняются быстро, пока другие пользователи бронируют) — не кэшировать надолго, в отличие от `trips/search`.

## 5. Карта маршрутов (Leaflet + OpenStreetMap)

Не зависит от бэкенда напрямую — `trips/search`/будущий `api/routes` отдают только названия городов (`From`/`To` — строки), не координаты. Нужно либо:
- захардкодить координаты городов на фронтенде (конечный список направлений, которые реально обслуживает компания — самое простое), либо
- геокодировать через публичный Nominatim (OpenStreetMap) на клиенте по названию города.

Для MVP — захардкоженный словарь координат проще и не требует доп. запросов.

## 6. Логин/регистрация против JWT API

- `POST /api/auth/signup` → сохранить `accessToken`/`refreshToken`
- `POST /api/auth/signin` → то же самое
- **Хранение токенов — решить осознанно, не по умолчанию в `localStorage`.** `localStorage` уязвим к XSS (весь JS на странице может прочитать токен). Альтернатива — Next.js Route Handler как BFF (backend-for-frontend): фронтенд стучится в свой же `/api/...` (Next.js), который проксирует к реальному бэкенду и держит токены в `httpOnly` cookie, недоступной для JS. CORS уже настроен под `http://localhost:3000` (`Program.cs`) — оба варианта совместимы, но с `httpOnly` cookie через BFF нужно решить, использовать ли CORS вообще (запросы будут same-origin с точки зрения браузера, если фронтенд сам проксирует).
- **Обновление access-токена** — `POST /api/auth/refresh` вызывать из общего HTTP-интерцептора при получении `401` от любого защищённого эндпоинта, не руками на каждом месте использования.
- **Логаут** — `POST /api/auth/revoke` (требует текущий access-токен + refresh-токен в теле), затем очистить локальное хранилище/cookie.

---

## Порядок реализации (рекомендация)

1. Скаффолдинг + публичные страницы (не требуют авторизации, можно делать первыми и параллельно с бэкендом)
2. ~~`RoutesController` на бэкенде~~ — готово
3. Логин/регистрация (фундамент для всего, что дальше требует токена)
4. Флоу бронирования без оплаты (поиск → место → оформление, оплата — заглушка)
5. Карта — можно в любой момент, не зависит от остального
