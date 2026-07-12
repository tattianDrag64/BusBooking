# SonarCloud — базовая настройка (без тестов, только статический анализ)

`ROADMAP.md` (Фаза 9) отмечает этот пункт как "делать после появления тестов" — тестов пока нет (тоже пункт Фазы 9, не сделан). Договорились сделать голый статический анализ сейчас (дублирование, code smells, потенциальные баги), coverage-метрику добавить позже, когда появятся xUnit-тесты — это просто ещё один параметр в той же команде сканера, ничего не придётся переделывать.

## Почему это не могу сделать полностью сам

SonarCloud — внешний сервис, требует:
1. **Аккаунт SonarCloud** (обычно логин через GitHub) — я не могу его создать за тебя.
2. **Organization + Project** внутри SonarCloud, привязанные к `github.com/tattianDrag64/BusBooking`.
3. **Токен** (`SONAR_TOKEN`) — секрет уровня организации/проекта в SonarCloud, который нужно добавить в **GitHub → Settings → Secrets and variables → Actions** этого репозитория.

Всё это требует твоего логина в SonarCloud и доступа к настройкам GitHub-репозитория — я не аутентифицирован ни там, ни там в этой сессии (`gh auth status` подтверждает, что GitHub CLI тоже не залогинен).

Что я могу сделать сам — подготовить workflow-файл и конфиг сканера, чтобы после того, как ты создашь аккаунт/токен, всё заработало сразу без дополнительной возни.

## Особенность этого репозитория — монорепо с двумя языками

Backend (C#, `BusBooking/`) и frontend (TypeScript, `busbooking-frontend/`) в одном репозитории. SonarCloud это поддерживает без создания двух отдельных проектов — `dotnet-sonarscanner` (обёртка над `sonar-scanner`, нужна для C#-анализа с привязкой к MSBuild) сканирует **весь чекаут репозитория**, включая TS/TSX-файлы, если они не исключены — не нужно два отдельных запуска.

## Шаги — что нужно сделать тебе

1. Зайти на [sonarcloud.io](https://sonarcloud.io), залогиниться через GitHub.
2. **Import project** → выбрать `tattianDrag64/BusBooking` (может потребовать поставить SonarCloud GitHub App на репозиторий, если ещё не стоит).
3. SonarCloud создаст **Organization Key** и **Project Key** — обычно `tattiandrag64` и `tattiandrag64_busbooking` (или похожие, зависит от того, что предложит мастер настройки) — скопировать оба, они понадобятся в workflow-файле ниже.
4. **My Account → Security → Generate Token** — создать токен (Project Analysis Token, привязанный к этому проекту, не User Token — меньше прав, если утечёт).
5. В GitHub-репозитории: **Settings → Secrets and variables → Actions → New repository secret** → имя `SONAR_TOKEN`, значение — токен из шага 4.
6. В файле `.github/workflows/sonarcloud.yml` (уже создан, см. ниже) — заменить `<SONAR_ORG>` и `<SONAR_PROJECT_KEY>` на реальные значения из шага 3.
7. Закоммитить и запушить (или открыть PR) — сканирование запустится автоматически на пуш в `master` и на каждый PR.

## Что я уже сделал

- [+] `.github/workflows/sonarcloud.yml` — GitHub Actions workflow: чекаут, Java (нужна сканеру — он JVM-based под капотом), .NET 10 SDK, Node (для `npm ci` во фронтенде — чтобы TS-анализ видел реально установленные типы, не обязательно, но точнее), `dotnet tool install dotnet-sonarscanner`, `begin` → `dotnet build BusBooking.sln` → `end`. Исключения (`sonar.exclusions`) — `bin/`, `obj/`, `node_modules/`, `.next/` (сгенерированный код и артефакты сборки, не нужно анализировать).
- [+] Кэш `~/.sonar/cache` через `actions/cache` — сканер иначе перекачивает анализаторы на каждый запуск.
- [ ] Реальные `<SONAR_ORG>`/`<SONAR_PROJECT_KEY>` — подставить после шага 3 выше, я не могу их узнать без доступа к твоему SonarCloud-аккаунту.
- [ ] `SONAR_TOKEN` — секрет в GitHub, добавить после шага 4-5 выше.

## Когда появятся тесты (следующий пункт Фазы 9)

Добавить в workflow между `begin` и `dotnet build`:
```yaml
- name: Run tests with coverage
  run: dotnet test --collect:"XPlat Code Coverage"
```
и в `begin`-шаг — параметр `/d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml"` (или `cobertura`, в зависимости от формата, который выберет `dotnet test`). Тогда SonarCloud покажет и coverage-метрику, не только статический анализ — ровно то, что и было целью в формулировке пункта в `ROADMAP.md`.

## Проверка после того, как секреты добавлены

1. Запушить любой коммит в `master` (или открыть PR) — GitHub Actions вкладка репозитория должна показать запуск `SonarCloud` workflow.
2. Дождаться завершения (~2-3 минуты первый раз, дольше из-за скачивания анализаторов) — зелёная галочка или явная ошибка в логах шага `end` (частая причина ошибки — неверный `<SONAR_ORG>`/`<SONAR_PROJECT_KEY>` или невалидный токен).
3. Результат — на `sonarcloud.io/project/overview?id=<SONAR_PROJECT_KEY>` — Bugs/Vulnerabilities/Code Smells/Duplications по обоим языкам.
