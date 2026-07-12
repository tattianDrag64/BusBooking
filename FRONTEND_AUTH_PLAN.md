# Frontend Auth — план реализации (JWT, localStorage-подход)

Прицельный план под `busbooking-frontend/`, чтобы можно было реализовать сессию и логин/регистрацию против `AuthController` (`/api/auth/signup`, `/api/auth/signin`, `/api/auth/refresh`, `/api/auth/revoke`). Контекст и альтернативы (BFF/httpOnly cookie) — в `FRONTEND_PLAN.md`, п.6. Здесь — выбранный для MVP путь: `localStorage`.

## Зачем `auth/` в первую очередь

Флоу бронирования (поиск → место → оформление) требует JWT начиная с шага «выбор места» (`GET /api/seats/trip/{tripId}`). Без сессии эта часть продукта физически недоступна — это единственная жёсткая зависимость, блокирующая самую ценную часть MVP.

## Этап 1 — Типы и API-обёртки

**`types/auth.ts`**
```ts
export interface TokenResponse {
  accessToken: string;
  refreshToken: string;
}
export interface SignUpPayload {
  email: string;
  password: string;
  // сверить с реальными полями SignUpVM (BusBooking/Models/SignUpVM.cs)
}
export interface SignInPayload {
  email: string;
  password: string;
}
```

**`api/auth.ts`** — обёртки над `apiFetch` (`api/client.ts`), по образцу `api/routes.ts`:
```ts
export function signUp(payload: SignUpPayload) {
  return apiFetch<TokenResponse>("/api/auth/signup", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });
}
// signIn — аналогично, POST /api/auth/signin
// refresh(refreshToken) — POST /api/auth/refresh
// revoke(accessToken, refreshToken) — POST /api/auth/revoke, Authorization: Bearer accessToken
```

Проверить перед началом: точные имена/типы полей `SignUpVM`/`SignInVM`/`RefreshRequestVM`/`TokenResponseVM` в бэкенде — не гадать по названию.

## Этап 2 — Хранение токена

**`auth/tokenStorage.ts`** — единая точка доступа к `localStorage`, не размазывать `localStorage.getItem` по компонентам:
```ts
const KEY = "pometco_tokens";
export function saveTokens(tokens: TokenResponse): void { ... }
export function getTokens(): TokenResponse | null { ... }
export function clearTokens(): void { ... }
```
Важно: только в клиентском коде (`"use client"`) — `localStorage` не существует в Server Component, обращение к нему там уронит рендер.

## Этап 3 — `AuthProvider` + `useAuth`

**`auth/AuthProvider.tsx`** (`"use client"`) — React Context: `{ user, accessToken, signIn, signUp, signOut, isAuthenticated }`.
При маунте — читает токены из `tokenStorage`, декодирует payload `accessToken` (JWT — `atob` средней части токена, без библиотек) чтобы достать `email`/`role`/`userId` в `user`.

**`auth/useAuth.ts`** — `useContext(AuthContext)`, кидает ошибку при вызове вне провайдера.

Обернуть `{children}` в `app/layout.tsx` в `<AuthProvider>` — один раз, глобально.

## Этап 4 — Автообновление токена при 401 *(необязательно для первой итерации)*

Расширить `api/client.ts`: если ответ `401` и это не сам `/auth/refresh` — вызвать `refresh()`, сохранить новые токены, повторить исходный запрос один раз. Если `refresh` тоже упал — `clearTokens()` и редирект на `/login`.

## Этап 5 — Страницы `login` / `signup`

Паттерн index+vm из `FRONTEND_STRUCTURE.md`:
- `app/login/page.tsx` — `"use client"`, только JSX формы
- `app/login/useLoginPageVM.ts` — хук: `useState` полей, `handleSubmit` → `useAuth().signIn(...)` → успех: `router.push("/")`, ошибка: сообщение в UI
- `app/signup/page.tsx` + `app/signup/useSignupPageVM.ts` — аналогично

## Этап 6 — Логаут и защищённые страницы *(необязательно для первой итерации)*

- `Header.tsx` — если `isAuthenticated`, показывать «Выйти» вместо ссылки на логин; `signOut()` → `revoke()` + `clearTokens()`.
- `auth/withAuthGuard.tsx` — обёртка для будущих `booking/`, `my-bookings/`: если `!isAuthenticated` после загрузки — редирект на `/login`. Можно временно заменить точечной проверкой `isAuthenticated` прямо в компоненте страницы, без отдельной обёртки.

## Порядок при нехватке времени

| # | Этап | Обязательность |
|---|------|-----------------|
| 1 | `tokenStorage.ts` + `api/auth.ts` | обязательно — без этого ничего не работает |
| 2 | `AuthProvider` / `useAuth` | обязательно |
| 3 | `login` / `signup` страницы | обязательно |
| 4 | Автообновление по 401 | можно пропустить — токен протухнет раньше, юзер перелогинится вручную |
| 5 | `withAuthGuard` | можно пропустить — проверять `isAuthenticated` прямо в компоненте страницы |

## Проверка в конце

1. `npm run dev`
2. Зарегистрироваться через `/signup`
3. Перезагрузить страницу — сессия не должна слетать (токен читается из `localStorage` при маунте `AuthProvider`)
4. Разлогиниться — `localStorage` очищен, защищённые страницы недоступны
