# Betty  Console Slote Machine Application
.NET 10 consle Applcation

## Commands
`deposit <amount>` · `withdrawal <amount>` · `bet <amount>` · `history` · `exit`

---

## Architecture
```
Betty.Core/           # Domain logic, interfaces, exceptions
Betty.Infrastructure/ # InMemoryWalletRepository (swappable with DbContext)
Betty.TEST/           # xUnit + Moq
```

- **Repository Pattern** — persistence abstracted behind `IWalletRepository`
- **Dependency Inversion** — `IWallet`, `IGame`, `IConsole`, `IRandom` enable full unit testing
- **IRandom** — makes game outcomes deterministic in tests
- **Centralized exceptions** — `WalletErrors` / `GameErrors` factories ensure consistent error messages
- **Global exception handler** — single catch point in `Run()` keeps handlers clean

---

## Run & Test
```bash
dotnet run --project Betty
dotnet test Betty.TEST
```
