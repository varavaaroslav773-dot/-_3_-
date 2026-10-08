# Команды для тестирования

```powershell
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release --logger "console;verbosity=normal"
dotnet test --collect:"XPlat Code Coverage" --results-directory TestResults
```

Для Visual Studio: Test -> Test Explorer -> Run All.
