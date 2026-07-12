# Переход MVC → Web API + JWT (полный, без гибрида)

Решение принято осознанно (2026-07-10): убрать старые MVC Views и cookie-аутентификацию **сейчас**, не дожидаясь готовности нового фронтенда (Next.js, Фаза 7 `ROADMAP.md`). Это не поэтапный, а разовый переход.

## Следствие, которое нужно держать в уме

**У сайта не будет браузерного UI, пока не появится фронтенд.** Все страницы перестали существовать — остался только JSON API. Для реального клиента это значит: до готовности Next.js-фронтенда показать в браузере нечего, кроме Swagger UI (`/swagger`) и ответов через curl/Postman.

---

## Шаг 1 — API-контроллеры — ЗАВЕРШЁН

Все контроллеры написаны, собираются чисто и проверены живым сквозным флоу (создание рейса → автогенерация мест → бронирование → повторное бронирование `409` → просмотр своих заказов).

- [+] `AuthController` (`api/auth`) — `signup`, `signin`, `refresh`, `revoke`
- [+] `TripsController` (`api/trips`) — `search` (GET, публичный), `ListTrips`/`Create`/`Edit`(PUT)/`Delete` (admin-only через `TripEditVM`, без прямого биндинга в `Trip`)
- [+] `SeatsController` (`api/seats`) — `GET trip/{tripId}` (автогенерация мест при первом обращении), `POST book` (создание `Order`+`OrderSeat`). Добавлена проверка, которой не было в оригинале: занятое место — явный `409`, а не тихий no-op с последующим падением на unique-индексе `OrderSeat.SeatDetailId`
- [+] `BookingsController` (`api/bookings`) — `GET my`, ответ через `OrderSummaryVM` (не сырая сущность `Order`)
- [+] `UsersController` (`api/users`) — `GET` (admin-only), ответ через `UserSummaryVM` (не сырая сущность `User` — иначе `PasswordHash` утёк бы в JSON)
- [+] Везде — явная схема `[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]`
- [+] Роли — `enum UserRole { Admin, Customer }` (`Entity/UserRole.cs`), не строки: `nameof(UserRole.Admin)` в `[Authorize]`, `user.Role.ToString()` в `TokenService`, `.HasConversion<string>()` в `ApplicationDbContext`. Устраняет класс багов "регистр роли не совпал → молчаливый 403"
- [+] `Program.cs`: enum'ы сериализуются в JSON строкой (`"Admin"`, не `0`) — `AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))`
- [+] Найден и исправлен попутный баг: `TripsController.Create`/`Edit` падали на `Npgsql.ArgumentException` (`Cannot write DateTime with Kind=Unspecified to PostgreSQL type 'timestamp with time zone'`), если клиент присылал дату без таймзоны — `System.Text.Json` даёт `Kind=Unspecified`, колонка `timestamptz` требует `Kind=Utc`. Фикс — `DateTime.SpecifyKind(model.X, DateTimeKind.Utc)` на входе в оба action

Старые MVC-контроллеры удалены по мере того, как новые их полностью заменяли: `UsersController.cs`, `BookingController.cs`, `SeatDetailController.cs`, `TripController.cs`.

## Шаг 2 — Проверка покрытия старого функционала — ЗАВЕРШЁН

- [+] Полный флоу проверен вручную через curl (не Swagger UI, но эквивалентно): signup/signin → создание рейса → места → бронирование → конфликт при повторном бронировании → список своих заказов
- [+] Нетривиальная логика `ChooseSeats` (автогенерация мест, создание `Order`+`OrderSeat`) — перенесена и проверена, ничего не потеряно

## Шаг 3 — Снести старые MVC-контроллеры и Views — ЗАВЕРШЁН

- [+] Старые контроллеры удалены (см. Шаг 1)
- [+] `Views/` удалена
- [+] `wwwroot/` удалена
- [+] `HomeController.cs` удалён целиком (был мёртвым кодом — `View()` без `Views/`, недостижим через роутинг), заодно `Models/ErrorViewModel.cs` (использовался только там)
- [+] `app.UseExceptionHandler("/Home/Error")` заменён на JSON-обработчик:
  ```csharp
  app.UseExceptionHandler(a => a.Run(async ctx =>
  {
      ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
      ctx.Response.ContentType = "application/json";
      await ctx.Response.WriteAsJsonAsync(new { message = "An unexpected error occurred." });
  }));
  ```
- [+] Проверено вживую: `dotnet build` чисто, приложение стартует, `/Home/Privacy` (старый мёртвый роут) теперь честный `404`, основные API-эндпоинты (`search`/`users`/`bookings/my`) отвечают `200`

## Шаг 4 — `Program.cs`: MVC-инфраструктура и cookie-схема — ЗАВЕРШЁН

- [+] `AddControllersWithViews()` → `AddControllers()`
- [+] `AddSession()`/`app.UseSession()` убраны
- [+] Cookie-схема убрана, осталась только JWT
- [+] `app.UseStaticFiles()` убран
- [+] `app.MapControllerRoute(...)` → `app.MapControllers()`
- [+] `using Microsoft.AspNetCore.Authentication.Cookies;` убран

## Шаг 5 — Финальная проверка

- [+] `dotnet build` — чисто
- [+] Полный флоу через curl с реальными токенами — работает (см. Шаг 1/2)
- [+] Через сам Swagger UI (`/swagger`) — открыт headless-браузером (Playwright), заскриншочен: все 5 групп (`Auth`, `Bookings`, `Seats`, `Trips`, `Users`) с точными путями, 0 ошибок консоли
- [+] `run-busbooking` skill (`.claude/skills/run-busbooking/SKILL.md`) переписан под JSON-API: `smoke-login.mjs`/`smoke-authed-pages.mjs` (скриншотили несуществующие Views) удалены; новый `smoke-api.mjs` — полный флоу через `fetch` (health → signin → search → users 401/200 → seats автогенерация → book → duplicate `409` → my bookings → refresh → revoke), прогнан вживую, все 11 проверок `[OK]`. `driver.mjs` оставлен только для скриншота Swagger UI

## Шаг 6 — CORS и Swagger — ЗАВЕРШЁН

`AddCors`/`UseCors("Frontend")`, `AddSwaggerGen`/`UseSwagger`/`UseSwaggerUI` — работают без изменений.

---

## Переход завершён полностью

Все шаги плана закрыты и проверены вживую. Следующий логичный шаг — вернуться к `ROADMAP.md` и продолжить с Фазы 2 (rate limiting/lockout на новых `api/auth/*` роутах) или Фазы 5 (бизнес-функционал).
