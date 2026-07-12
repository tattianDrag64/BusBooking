# Фаза 5 — Расписание (`Schedule`) + автогенерация рейсов: план

Изначальная задача из `ROADMAP.md` («Автогенерация рейсов из `RouteInfo`») оказалась крупнее, чем казалось: у `RouteInfo` сейчас только **одно** время отправления в неделю в каждую сторону (`DepartureDay`+`DepartureTime`, `ReturnDay`+`ReturnTime`), а нужна модель «несколько времён отправления в день, каждый день» (пример из обсуждения: Chișinău→Comrat каждый день в 10:00, 13:00, 16:00). Решение — новая сущность `Schedule`, которая **заменяет** эти четыре поля в `RouteInfo`, а не дополняет их (осознанный выбор, не два источника правды).

## Блast radius (проверено чтением кода перед планом)

**Бэкенд:** `Entity/RouteInfo.cs` (сама сущность), `Data/ApplicationDbContext.cs` (сид-данные + fluent-конфиг), `Repository/RouteRepository.cs` (`GetAvaiableRoutes`/`RouteExists` фильтруют по этим полям), `Models/RouteSummaryVM.cs` + `Controllers/RoutesController.cs` (API-контракт). `TripsController`/`SeatsController`/`BookingsController` **не затронуты** — работают только с полями самого `Trip`.

**Фронтенд:** `types/route.ts` (`RouteSummary`/`Weekday`), три компонента читают `departureDay`/`departureTime`/`returnDay`/`returnTime` напрямую: `RouteExplorer.tsx` (фильтр по дню, сортировка ближайшего отправления, рендер), `app/routes/page.tsx` (рендер), `ActiveRoutesSidebar.tsx` (рендер). `SearchWidget.tsx`/`routes.vm.ts` затронуты только на уровне типа, не логики.

## 1. Бэкенд: сущность `Schedule` — ✅ СДЕЛАНО

Новая сущность (`Entity/Schedule.cs`):

```csharp
public class Schedule : BaseEntity
{
    public required Guid RouteId { get; set; }
    public virtual RouteInfo Route { get; set; } = null!;
    public TimeSpan DepartureTime { get; set; }
    public TimeSpan Duration { get; set; }       // для расчёта Trip.ArrivalDate = DepartureDate + Duration
    public DayOfWeek? DayOfWeek { get; set; }     // null = рейс каждый день; конкретное значение = только по этому дню недели
    public bool IsReturnTrip { get; set; }        // направление: false = DepartureCity→ArrivalCity, true = обратно
}
```

`RouteInfo`: убрать `DepartureDay`/`ReturnDay`/`DepartureTime`/`ReturnTime`, добавить `ICollection<Schedule> Schedules`. `Price` остаётся на `RouteInfo` как есть (не переносится на `Schedule` — не было запроса на разные цены по времени, не расширяю без необходимости).

Миграция: `RemoveColumn` на 4 старых поля, `CreateTable` для `Schedules`, FK на `Routes`. Сид-данные в `ApplicationDbContext.OnModelCreating` — заменить текущий `HasData` для `RouteInfo` (без day/time полей) на `HasData` для `RouteInfo` + отдельный `HasData` для 2 `Schedule` (Monday 08:00 outbound + Tuesday 18:00 return, `Duration` — с потолка, например 10 часов, раз раньше длительности не было вообще — уточнить у пользователя реальное значение при необходимости, для сида не критично).

**Сделано.** `Entity/Schedule.cs` создан, `RouteInfo` лишился 4 полей, получил `ICollection<Schedule> Schedules`. Миграция `AddScheduleEntity` (с ожидаемым EF-предупреждением "operation may result in data loss" — это удаление старых 4 колонок, безопасно на dev-БД с одним сид-маршрутом) применена: `DROP COLUMN` × 4 на `Routes`, `CREATE TABLE Schedules` + FK + индекс на `RouteId`, сид-данные перенесены в `Schedules` (2 строки, `Duration = 10 часов` — заглушка, реальную длительность можно поправить позже через `SchedulesController`, когда он появится в шаге 6).

## 2. Бэкенд: `RouteRepository` — ✅ СДЕЛАНО

- `RouteExists(routeId, dayOfWeek?, departureTime)` — проверка дубликата конкретного слота расписания (было `RouteExists(departureDay, departureTime)` на уровне маршрута, станет — на уровне `Schedule`).
- `GetAvaiableRoutes` — что означает «доступные маршруты» без единого дня/времени на маршрут? Скорее всего теряет смысл в текущем виде (маршрут либо есть, либо нет, дни/времена теперь у `Schedule`) — возможно, стоит просто удалить этот метод, если он нигде не используется вызывающим кодом (проверить перед удалением).

**Сделано, с уточнением.** Проверка (`grep`) подтвердила: оба метода (`GetAvaiableRoutes`/`RouteExists`) нигде не вызывались — мёртвый код. `RouteRepository`/`IRouteRepository` упрощены до пустых оберток над `Repository<RouteInfo>`. Логика проверки дубликата перенесена в новый `IScheduleRepository.ScheduleExists(routeId, dayOfWeek?, departureTime, isReturnTrip)` — концептуально это свойство `Schedule`, не `RouteInfo`, поэтому не осталась на месте старого метода с тем же именем. `IUnitOfWork`/`UnitOfWork` получили `Schedule`-репозиторий.

## 3. Бэкенд: API-контракт (`RouteSummaryVM`/`RoutesController`) — ✅ СДЕЛАНО

Текущий `RouteSummaryVM` — один `departureDay`/`departureTime`/`returnDay`/`returnTime` на маршрут. С `Schedule` у маршрута теперь **список** слотов. Варианты:

**A. `RouteSummaryVM.Schedules: ScheduleSummaryVM[]`** — каждый маршрут возвращает полный список времён (`{ dayOfWeek: DayOfWeek?, departureTime, isReturnTrip }`). Честно отражает реальность, но требует переписать 3 фронтенд-компонента (`RouteExplorer`, `app/routes/page.tsx`, `ActiveRoutesSidebar`) под список вместо одного значения — они сейчас фильтруют/сортируют/рендерят по единственному `departureDay`/`departureTime`.

**B. Оставить `RouteSummaryVM` как есть, вычисляя "ближайший" слот** — `RoutesController` берёт хронологически ближайший `Schedule` для каждого маршрута и мапит его в старые 4 поля. Минимальные изменения на фронтенде (0 файлов), но теряется информация (пользователь не увидит, что рейс ходит 3 раза в день, только ближайший).

**Рекомендация: A** — раз весь смысл фичи в множественных временах, скрывать это на публичной странице `/routes` было бы странно (сайт должен показывать реальное расписание). Но это требует правки фронтенда как часть этого же плана, не отдельно.

**Сделано, вариант A.** `Models/ScheduleSummaryVM.cs` (`DayOfWeek?`, `DepartureTime` строкой `hh:mm`, `IsReturnTrip`). `RouteSummaryVM.Schedules: IEnumerable<ScheduleSummaryVM>`. `RoutesController.GetRoutes()` — `GetAll(includeProperties: "Schedules")` вместо обычного `GetAll()`. Проверено вживую: `GET /api/routes` отдаёт `schedules: [{dayOfWeek: "Monday", departureTime: "08:00", isReturnTrip: false}, {dayOfWeek: "Tuesday", departureTime: "18:00", isReturnTrip: true}]`.

## 4. Фронтенд: `types/route.ts` + 3 компонента — ✅ СДЕЛАНО

- `types/route.ts`: `RouteSummary.departureDay/departureTime/returnDay/returnTime` → `schedules: ScheduleSummary[]`, `ScheduleSummary { dayOfWeek: Weekday | null; departureTime: string; isReturnTrip: boolean }`.
- `RouteExplorer.tsx`: фильтр по дню (`route.departureDay === day`) → `route.schedules.some(s => s.dayOfWeek === day || s.dayOfWeek === null)`; `nextDeparture` — сортировка по всем `departureTime` всех `schedules` всех маршрутов, не по одному полю на маршрут; рендер карточки — показать первое ближайшее время или короткий список.
- `app/routes/page.tsx`: рендер карточки — список времён вместо одной строки `{departureDay}, {departureTime}`.
- `ActiveRoutesSidebar.tsx`: то же — превью ближайшего времени или количества рейсов в день.

**Сделано.** Общая логика вынесена в новый `lib/schedule.ts` (не дублируется в трёх местах): `outboundSchedules`/`returnSchedules` (фильтр по `isReturnTrip`), `routeRunsOnDay` (для фильтра по дню, учитывает `dayOfWeek === null` как «каждый день»), `formatSchedules`/`formatScheduleSummary` (человекочитаемая строка вида `"Every day, 08:00, 13:00"`), `earliestDepartureTime`. Все 3 компонента переписаны на эти хелперы вместо прямого чтения старых полей.

**Найдена и исправлена смежная проблема при живой проверке:** после смены API-контракта `GET /api/routes` фронтенд поймал `TypeError: Cannot read properties of undefined (reading 'filter')` — Next.js dev-кэш (`.next/cache`, `revalidate: 3600` в `getRoutes()`) отдавал ответ, закэшированный ещё до миграции, со старой формой без `schedules`. Помогло `rm -rf .next` + рестарт dev-сервера. **Важно на будущее:** при любой смене формы ответа бэкенда во время локальной разработки с ISR-кэшем — чистить `.next/cache`, иначе фронтенд будет получать доказуемо неверные (устаревшие) данные несмотря на то, что бэкенд уже обновлён.

Проверено вживую после фикса: `/` и `/routes` рендерят `"Monday, 08:00"`/`"Every day"` и т.д. без ошибок, оба блока Departure/Return на `/routes` отображаются.

## 5. Бэкенд: сама автогенерация (изначальная цель) — ✅ СДЕЛАНО

`POST /api/trips/generate` (admin-only), тело `{ weeksAhead: int }` (например, дефолт 4):
- Для каждого `Schedule` (всех маршрутов): для каждого дня в окне `[сегодня, сегодня + weeksAhead*7]`, который соответствует `Schedule.DayOfWeek` (или **каждый** день, если `DayOfWeek == null`) — вычислить `departureDate = день + Schedule.DepartureTime`.
- Пропустить, если `Trip` с таким `RouteId`+`DepartureDate`+`IsReturnTrip` уже существует (аналог текущего `TripRepository.TripExists`, adaптировать сигнатуру).
- Выбрать `Bus` для маршрута — первый `Bus` с `Bus.RouteId == schedule.RouteId` (тот же принцип, что и сейчас при ручном создании — просто автоматически, а не руками через форму).
- Создать `Trip`: `From`/`To` (с учётом направления `IsReturnTrip` — переставить местами `DepartureCity`/`ArrivalCity`), `DepartureDate`, `ArrivalDate = DepartureDate + Schedule.Duration`, `Price = (double)RouteInfo.Price`, `IsReturnTrip`, `BusId`, `RouteId`.
- Вернуть сводку: сколько `Trip` создано, сколько пропущено как дубликаты.

**Сделано, с одним попутным фиксом.** `TripRepository.TripExists` сравнивал только `DepartureDate.Date` (без времени) — верно для старой модели (одно время в день на маршрут), но неверно для новой (несколько слотов в один день: 10:00/13:00/16:00 ошибочно считались бы дубликатами друг друга). Изменено на сравнение полного `DepartureDate`. `TripsController.GenerateTrips` реализован по плану, ответ — `{ created, skipped, skippedNoBus }` (добавлен `skippedNoBus` сверх плана — отдельно считает маршруты без назначенного автобуса, чтобы админ видел, где генерация не сработала и почему).

Проверено вживую 5 сценариями: (1) первый прогон на 4 недели — `created: 8` (маршрут Sofia↔Berlin, 1 еженедельный outbound + 1 return слот × 4 недели), `skippedNoBus: 2` (маршрут Comrat без автобуса); (2) повторный прогон сразу же — `created: 0, skipped: 8` (полная идемпотентность, дубликаты не создаются); (3) добавлен автобус на маршрут Comrat, прогон на 1 неделю — `created: 14` (2 ежедневных слота × 7 дней, ровно как в примере из обсуждения); (4) не-админ — `403`; (5) сгенерированный рейс реально работает — `GET /api/seats/trip/{id}` на сгенерированном `Trip` вернул корректно созданные места, доступные для бронирования (полная интеграция с уже существующим флоу).

## 6. Бэкенд: минимальный admin CRUD для `Schedule` — ✅ СДЕЛАНО

Без этого некому будет заводить новые слоты расписания (текущий пример «Комрат каждый день в 10/13/16» нужно как-то ввести). Новый `SchedulesController`, admin-only:
- `POST /api/schedules` — создать слот (`RouteId`, `DepartureTime`, `Duration`, `DayOfWeek?`, `IsReturnTrip`).
- `GET /api/schedules/route/{routeId}` — список слотов маршрута (для проверки перед генерацией).
- `DELETE /api/schedules/{id}`.

Без фронтенд-UI (как и `TripsController`/`UsersController` сейчас — админка целиком через Swagger, устоявшийся паттерн проекта).

**Сделано. По ходу найден и исправлен реальный баг** (не в плане изначально): `POST /api/schedules` падал `500 System.Text.Json.JsonException: A possible object cycle was detected`. Причина — EF Core автоматически связывает навигационные свойства для отслеживаемых в одном контексте сущностей: `_unitOfWork.Route.GetById(...)` (проверка существования маршрута) трекает `RouteInfo`, затем при `_unitOfWork.Schedule.Add(schedule)` EF сам проставляет `schedule.Route = route` и одновременно добавляет `schedule` в `route.Schedules` — получается цикл `Schedule.Route.Schedules[0].Route.Schedules[0]...`, который System.Text.Json не может сериализовать. Исправлено — контроллер возвращает не сырую `Schedule`-сущность, а плоский `ScheduleVM` (`Id`, `RouteId`, `DepartureTime`, `Duration`, `DayOfWeek`, `IsReturnTrip`), как и `RouteSummaryVM`/`ScheduleSummaryVM` из шага 3 (там цикла не было, потому что уже возвращались VM, а не сущности).

Проверено вживую 8 сценариями: создание 3 слотов «Chisinau→Comrat, каждый день, 10:00/13:00/16:00» (ровно пример из обсуждения) — первая попытка поймала `500` до фикса, но `Save()` уже прошёл (данные не потеряны, транзакция коммитится независимо от ошибки сериализации ответа) — после фикса подтверждено `409` на дубликат того же слота; список по маршруту отдаёт все 3; не-админ получает `403`; создание на несуществующий `RouteId` — `404`; удаление — `204`, повторное удаление того же — `404`; `RoutesController` (шаг 3) не задет изменениями.

## Порядок выполнения

1. `Schedule`-сущность + миграция + обновлённые сид-данные — фундамент.
2. `RouteRepository` под новую модель.
3. `RouteSummaryVM`/`RoutesController` (вариант A) — бэкенд-контракт.
4. Фронтенд: `types/route.ts` + 3 компонента.
5. `SchedulesController` (admin CRUD) — нужен, чтобы вообще было что генерировать.
6. `POST /api/trips/generate` — сама автогенерация, финальная цель.

Каждый шаг — сборка + проверка вживую + отметка `[+]` здесь и в `ROADMAP.md`, как во всех предыдущих планах.
