# План доведения проекта до собираемого состояния

Контекст: репозиторий в процессе рефакторинга (переход от дублирующихся
`BusBookingDemo`/`BusBooking`/`BusBookingProject` неймспейсов к единой модели,
миграция PK с `int` на `Guid`, переход бронирования с `Booking` на `Order`+`OrderSeat`).
Ниже — точное текущее состояние этого клона (Windows,
`C:\Users\Asus\source\repos\tattianDrag64\BusBooking`), проверено `dotnet clean && dotnet build`.

**Текущий статус сборки: 0 ошибок, 7 предупреждений.** Все предупреждения разобраны
в разделе ниже.

## Сделано

### Консолидация Entity/Data (первый проход)

- Версии `Microsoft.EntityFrameworkCore*` в `.csproj` выровнены на `8.0.3` (была
  NU1605-несовместимость: ядро `8.0.1` vs `Proxies 8.0.3`).
- Все сущности собраны под один namespace `BusBookingDemo.Entity` (раньше часть жила
  в `BusBookingDemo.Entities`/`BusBooking.Entities`).
- `Route` → `RouteInfo` (класс и файл) — старое имя конфликтовало с
  `Microsoft.AspNetCore.Routing.Route`, неявно попадающим в scope в веб-проекте.
- Первичные ключи (`BaseEntity.Id`) и все FK-скаляры переведены с `int` на `Guid`.
- `SeatDetail` унаследован от `BaseEntity` (раньше не имел PK вообще).
- `[ForeignKey]`-атрибуты переставлены на навигационные свойства (были на FK-скалярах
  самих на себя — не работает).
- `required` убран с навигационных свойств везде, где он создавал форс на подстановку
  целого связанного объекта вместо создания по FK (`= null!` вместо этого).

### Удалён мёртвый код

- `Data/BusBokingDbContext.cs`, `Data/BusBookingDbContextFactory.cs` — неиспользуемый
  дубль `ApplicationDbContext`.
- `Repository/Repositories/**` целиком — неиспользуемая копия репозиториев с ссылками
  на несуществующий `BusBooking.Entities`.
- `Models/Authorization/LoginVM.cs`, `RegisterVM.cs` — namespace
  `BusBookingProject.Models.ViewModels.Authorization` (третий вариант имени проекта),
  нигде не подключены ни одним контроллером.

### `Booking` → `Order`+`OrderSeat` (архитектурное решение принято и реализовано)

- `ApplicationDbContext.cs`: `Booking`-конфиг убран; добавлены `DbSet<Order>`,
  `DbSet<RouteInfo>`; Fluent-конфиг для `Order`/`OrderSeat`/`Trip.Route`; уникальный
  индекс `OrderSeat.SeatDetailId` (защита от двойного бронирования); сид-данные
  переведены на фиксированные `Guid`-литералы, добавлен `HasData` для `RouteInfo`,
  `Bus` получил `RouteId`.
- `Entity/Booking.cs`, `Models/BookingVM.cs`, `Repository/BookingRepository.cs`,
  `IBookingRepository.cs`, `Views/Booking/MyBookings.cshtml` — удалены.
- `Repository/OrderRepository.cs`, `OrderSeatRepository.cs`, `RouteRepository.cs` +
  интерфейсы — реализованы (были заведены как заготовки с опечатками в
  namespace/generic-базе — поправлено).
- `IUnitOfWork`/`UnitOfWork` — `Booking` заменён на `Order`, `OrderSeat`, `Route`.
- `SeatDetailController.ChooseSeats` создаёт `Order`+`OrderSeat` (пока одно место на
  заказ — `SeatBookingVM.SelectedSeat` остаётся одиночным полем; переход на
  мультивыбор мест — отдельная более крупная правка VM+вьюхи).
- `BookingController.MyBookings` читает через `Order.GetOrdersByUser(...)`, `OrderVM`
  вместо `BookingVM`.
- Попутно переведены на `Guid` сигнатуры, молчаливо оставшиеся на `int`:
  `IBusRepository.GetSeatsCount`, `ISeatDetailRepository.GetSeatsByTrip`/`ReserveSeat`,
  `SeatBookingVM.TripId`/`SelectedSeat`, `SeatDetailVM.Id`.

### `TripController`/`UsersController` (сломались из-за параллельной правки `Trip.cs`,

убравшей старые строковые поля `Depart`/`Return`/`Time`/`Guest`)

- Опечатка в имени класса (`TripCoTontroller` → `TripController`) — похоже, артефакт
  редактирования вживую.
- `TripVM`/`TripCreateVM` переведены на `DepartureDate`/`ArrivalDate`/`IsReturnTrip`/
  `Price`; `BusId` — на `Guid`.
- `Trip.Create` теперь берёт `RouteId` из выбранного `Bus.RouteId` (в форме создания
  рейса нет отдельного выбора маршрута — он неявно следует из автобуса).
- `Index(TripVM)` POST — убрана нелогичная ветка "не нашли рейс — создать новый без
  Bus/Route" (без них `Trip` создать нельзя, это было мёртвое/сломанное поведение
  ещё до Guid-миграции).
- `SearchTrips`/`ListTrips` — фильтрация и проекция переведены на новые поля `Trip`.
- **Вьюхи, ссылавшиеся на убранные поля, удалены целиком** (`Views/Home/Index.cshtml`,
  `Views/Trip/Index.cshtml`, `Create.cshtml`, `Edit.cshtml`, `SearchTrips.cshtml`) —
  решение принято осознанно: фронтенд всё равно переписывается на другом стеке
  (roadmap Фаза 6, Next.js), вкладываться в разметку MVC-вьюх сейчас бессмысленно.
  Соответствующие action'ы (`Index`, `Create`, `SearchTrips`, `ListTrips`, `Edit` GET,
  `Login`) теперь возвращают `Ok(...)`/`BadRequest(...)` вместо `View(...)` — без
  редизайна под полноценный Web API (это уже roadmap Фаза 4), просто чтобы не звать
  вьюхи, которых больше нет.
- `UsersController.SignUp` — не хватало обязательных `FirstName`/`LastName`/`Role`
  при создании `User` — добавлено (`Role = "Customer"` по умолчанию).
- `User.Password` переименован в `User.PasswordHash` (параллельная правка, в духе
  roadmap Фазы 2 — "заменить открытый пароль на хэш") — `UsersController` синхронизирован.

## ✅ Все предупреждения уровня C# закрыты

`dotnet clean && dotnet build`: **0 ошибок, 0 предупреждений CS**. Все 12 пунктов,
описанных в прошлых проходах, исправлены:

- `SignInVM`/`TripCreateVM.From`/`To` — по ходу правки словили ту же ловушку дважды:
  `required` на VM, которая где-то в GET-экшене создаётся совсем пустой (`new SignInVM()`,
  `new TripCreateVM { BusList = ... }` без `From`/`To`), ломает компиляцию. В обоих
  случаях откатили `required` → дефолт `= string.Empty;`. `TripVM.From`/`To` — другая
  ситуация (нигде не создаётся вручную, только через model binding), там `required`
  оставлен как есть.
- `Repository<T>.Get(...)`/`IRepository<T>.Get(...)`, `OrderRepository.GetByCode`/
  `IOrderRepository.GetByCode` — возврат `T`/`Order` заменён на `T?`/`Order?` (честно
  отражает, что `FirstOrDefault()` может вернуть `null`; вызывающий код уже везде
  проверяет на `null`, так что это просто уточнение типа).
- `BusRepository.GetSeatsCount` — вместо молчаливого NRE при отсутствии автобуса теперь
  явный `?? throw new InvalidOperationException(...)`.
- `TripController.cs:111` — `User.Identity?.IsAuthenticated != true` вместо
  `!User.Identity.IsAuthenticated`.
- `Views/SeatDetail/ChooseSeats.cshtml` — `@seat.SeatNumber?.Substring(5)`.

### Оставшиеся ~22 предупреждения — другая категория, не код

Это NuGet-адвизори об уязвимостях в транзитивных пакетах (`NU1901`/`NU1902`/`NU1903`),
не связаны с C#-кодом вообще:

```
NU1901 (low):      Microsoft.Identity.Client, NuGet.Packaging, NuGet.Protocol
NU1902 (moderate):  Azure.Identity, Microsoft.Identity.Client
NU1903 (high):      Microsoft.Build, Microsoft.Extensions.Caching.Memory,
                    System.Security.Cryptography.Xml, System.Text.Json
```

Точное число в выводе `dotnet build` гуляет (22/25/27/29 между прогонами) — это не
нестабильность реальных проблем, а особенность MSBuild: одно и то же предупреждение
может печататься несколько раз за один `build` (restore + design-time evaluation +
сам build), финальный счётчик "N Warning(s)" — это его собственная (не всегда
интуитивная) агрегация. Ни один из этих пакетов не указан в `.csproj` напрямую под
такой версией — они приходят транзитивно через `Microsoft.AspNetCore.Identity.*` и
`Microsoft.VisualStudio.Web.CodeGeneration.Design`. Чинить их — отдельная задача
(`dotnet list package --vulnerable`, обновление/пересмотр зависящих от них пакетов),
не относится к сегодняшней правке кода.

## Открытые архитектурные решения (не блокеры)

### Переименование проекта `BusBookingDemo` → `BusBooking`

Не блокер, вопрос выбора. За: "Demo" перестаёт быть демкой, проект и так посреди
рефакторинга. Против: инвазивно (namespace во всех `.cs`, `.csproj`, `.sln`, assembly
name, возможно имя БД), риск руками что-то упустить.

Порядок, если решите делать:

```bash
git mv BusBookingDemo.sln BusBooking.sln
git mv BusBookingDemo BusBooking
git mv BusBooking/BusBookingDemo.csproj BusBooking/BusBooking.csproj
```

В `BusBooking.sln` поправить строку `Project(...)`, заменить оба вхождения
`BusBookingDemo` на `BusBooking` (GUID не трогать). Затем namespace/using по всему коду:

```powershell
Get-ChildItem -Recurse -Include *.cs,*.csproj BusBooking |
  ForEach-Object {
    (Get-Content $_.FullName -Raw) -replace 'BusBookingDemo', 'BusBooking' |
      Set-Content -Encoding UTF8 $_.FullName
  }
```

Надёжнее — через семантический rename в IDE, а не текстовый replace, если есть такая
возможность. Отдельно: БД в connection string (`Database=BusBookingDemoDB`) — dev-база
с тестовыми сидами, переименование ничего не потеряет.

### Booking vs Order — решено

`Order`+`OrderSeat` — окончательный выбор (мульти-местные покупки, один чек на заказ).
`Booking` полностью удалён, см. раздел "Сделано" выше.

## Рекомендуемый порядок дальнейших действий

1. Пройтись по 7 предупреждениям выше.
2. `dotnet clean && dotnet build` — контрольная точка: 0 ошибок, 0 предупреждений.
3. Решить (не обязательно сейчас) вопрос переименования проекта.
4. Дальше — по `ROADMAP.md`: Фаза 1 (функциональные баги логина/паролей — `Password`→
   `PasswordHash` уже начато), Фаза 2 (безопасность), Фаза 3 (Postgres).

## Разное / машинно-локальные шаги (не код, не в git)

- `dotnet dev-certs https --trust` — подтверждение dev-сертификата HTTPS, один раз на
  каждой машине, где локально запускаете проект с `https://`.
- Проверка `dotnet --version` в **новом** терминале VS Code после установки SDK —
  убедиться, что редактор видит тот же `PATH`, что и внешний терминал.

## Про `HasData` vs ручной сидер (справочно)

- `HasData` хорош для по-настоящему статичных справочников, обязательных в любой среде
  (например, фиксированный список автобусов/маршрутов). Плата — данные навсегда
  оседают в истории миграций, менять/убирать их потом неудобно.
- Тестовые пользователи `tati`/`anni` с паролем `1234` — dev-данные для локальной
  разработки, а не то, что должно жить в схеме БД навечно и попадать в прод-миграции.
  Разумнее вынести в отдельный сидер, вызываемый только в `Development`, а не в
  `HasData` — но сейчас они там, тронуть не просили.
