# Структура фронтенда — busbooking-frontend (Next.js App Router)

Итоговая структура, собранная из `FRONTEND_PLAN.md` (карта API/страниц) и паттерна index+vm из референсного проекта (`ERP-Supply-Management/apps/web`), адаптированного под Next.js.

```
busbooking-frontend/
  app/
    layout.tsx                  ← корневой layout (аналог main.tsx в Vite) + глобальные провайдеры
    globals.css                 ← аналог index.css
    page.tsx                    ← Home — SSG, статика, без vm-файла (нет данных с бэкенда)
    about/
      page.tsx                  ← About Us — SSG, статика

    routes/
      page.tsx                  ← index: только JSX, Server Component
      routes.vm.ts               ← vm: async-функция getRoutesVM(), fetch с revalidate: 3600

    login/
      page.tsx                  ← index: JSX формы, Client Component ("use client")
      useLoginPageVM.ts          ← vm-хук: состояние формы, mutation на /api/auth/signin, редирект

    signup/
      page.tsx
      useSignupPageVM.ts

    search/
      page.tsx                  ← поиск рейсов, Client Component
      useSearchPageVM.ts         ← useQuery(['trips','search',params]) + состояние формы

    booking/[tripId]/seats/
      page.tsx
      useSeatsPageVM.ts          ← useQuery(seats) + useMutation(book) + выбранное место
      useSeatSelection.ts        ← отдельный узкий хук, если логика подсветки/валидации мест разрастётся

    my-bookings/
      page.tsx
      useMyBookingsPageVM.ts     ← useQuery(['bookings','my']), enabled: isAuthenticated

  auth/
    AuthProvider.tsx             ← контекст текущего юзера + токенов (client-side, JWT в httpOnly cookie через BFF либо localStorage — решить осознанно, см. FRONTEND_PLAN.md п.6)
    useAuth.ts                   ← хук чтения контекста
    withAuthGuard.tsx            ← обёртка/redirect для защищённых страниц

  api/
    client.ts                    ← базовый fetch-клиент, NEXT_PUBLIC_API_BASE_URL, обработка 401 → refresh
    auth.ts                      ← signIn/signUp/refresh/revoke — обёртки над /api/auth/*
    trips.ts                     ← searchTrips
    seats.ts                     ← getSeats, bookSeat
    bookings.ts                  ← getMyBookings
    routes.ts                    ← getRoutes

  components/
    ui/                          ← общие переиспользуемые компоненты (кнопки, карточки, лоадеры)
      Header.tsx / Footer.tsx     ← глобальные, рендерятся один раз в app/layout.tsx
      PageHero.tsx                 ← баннер-заголовок внутренней страницы (About и т.п.)
      NewsSection.tsx               ← блок "Latest News", статичные данные из lib/news.ts
    home/                         ← блоки, специфичные для Home (Hero, SearchWidget, RouteExplorer, WhyPometco, ContactSection)
    about/                        ← блоки, специфичные для About (MissionSection)
    routes/                       ← блоки, специфичные для страницы маршрутов (RouteMap — декоративная карта, ActiveRoutesSidebar)

  hooks/                         ← общие хуки, НЕ привязанные к одной странице
    useDebounce.ts
    useMediaQuery.ts

  lib/
    queryClient.ts                ← конфиг TanStack Query (QueryClientProvider для клиентских страниц)
    format.ts                     ← форматирование даты/цены
    news.ts                       ← статичные заглушки для NewsSection (нет API для новостей)

  types/
    __generated__/                ← типы, сгенерированные из Swagger-схемы бэкенда (опционально, вместо ручного дублирования VM-интерфейсов)
    route.ts                      ← RouteSummary
    news.ts                       ← NewsItem
    (ручные типы, не покрытые кодогенерацией)

  utils/                          ← чистые функции без побочных эффектов

  .env.local                      ← NEXT_PUBLIC_API_BASE_URL=http://localhost:5100
```

## Про имя файла: почему не index.tsx

В App Router имя `page.tsx` — служебное соглашение самого фреймворка (как `layout.tsx`, `loading.tsx`): по нему роутер физически находит маршрут, переименовать в `index.tsx` нельзя (в отличие от Pages Router/Vite, где `routes/index.tsx` — просто путь). Решили остаться на App Router — значит, `page.tsx` остаётся, но становится предельно тонким: только импорт vm-функции/хука и рендер. Вся логика/структура/разделение — как и планировалось, просто имя файла с "оболочкой" зафиксировано фреймворком, а не нами.

## Правило разделения index/vm

- **Server Component (публичная, некликабельная страница: Home, About, каталог маршрутов)** → `page.tsx` + `<name>.vm.ts` с `async function get<Name>VM()`. Хуков быть не может — нет клиентского рантайма.
- **Client Component (что угодно с состоянием/формой/мутацией: логин, поиск, выбор места, брони)** → `page.tsx` с `"use client"` + `use<Name>PageVM.ts` — хук, инкапсулирующий `useQuery`/`useMutation`/`useState`. `page.tsx` только рендерит то, что вернул хук.
- Если у страницы есть отдельная, самостоятельная под-логика (drag&drop, выбор места, валидация) — выносить её в свой узкий хук рядом (`useSeatSelection.ts`), а не раздувать основной VM-хук.

## Что НЕ переносим из ERP-проекта

- `router.tsx`, `main.tsx`, `vite-env.d.ts`, папка `pages/` как роутер — это специфика Vite/SPA (react-router). В Next.js App Router роутинг задаётся файловой структурой `app/` автоматически, эти файлы не нужны.
- `stores` (глобальный Zustand/Redux) — не заводим на старте: серверные данные покрывает TanStack Query, а локального UI-состояния (кроме auth-контекста) пока недостаточно, чтобы оправдать глобальный store.

## Порядок реализации (без изменений от FRONTEND_PLAN.md)

1. Скаффолдинг + Home/About/Routes (SSG/ISR, без авторизации)
2. `auth/` + `login`/`signup` (фундамент для всего, что дальше требует токена)
3. `search` → `booking/.../seats` (флоу бронирования без оплаты — оплата пока заглушка, см. FRONTEND_PLAN.md п.3.5)
4. `my-bookings`
5. Карта (Leaflet) — независимо, в любой момент
