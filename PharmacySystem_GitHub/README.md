# PharmacySystem

Учебный проект: модульная информационная система для учёта лекарственных средств в аптеке.
Проект подготовлен для учебной практики по ПМ.02 и предназначен для размещения в GitHub.

## Стек
- C# 12 / .NET 8
- WPF
- Entity Framework Core 8
- SQLite
- xUnit
- coverlet.collector

## Структура
- `src/Pharmacy.Domain` — сущности предметной области.
- `src/Pharmacy.Application` — бизнес-логика и интерфейсы доступа к данным.
- `src/Pharmacy.Infrastructure` — EF Core и SQLite.
- `src/Pharmacy.Wpf` — пользовательский интерфейс.
- `tests/Pharmacy.Application.Tests` — модульные тесты.
- `tests/Pharmacy.IntegrationTests` — интеграционные тесты с тестовой SQLite БД.

## Запуск в Windows / Visual Studio
1. Установить .NET 8 SDK и Visual Studio с workload `.NET Desktop Development`.
2. Открыть `PharmacySystem.sln`.
3. Восстановить NuGet-пакеты.
4. Запустить `Pharmacy.Wpf`.

## Запуск тестов
```powershell
dotnet test --configuration Release
```

## Покрытие
```powershell
dotnet test --collect:"XPlat Code Coverage" --results-directory TestResults
```

Отчёт `coverage.cobertura.xml` формируется тестовой инфраструктурой coverlet. Для просмотра HTML-отчёта можно использовать ReportGenerator.

## Git
Пример первого коммита:
```powershell
git init
git add .
git commit -m "feat: add pharmacy accounting system and qa tests"
```
