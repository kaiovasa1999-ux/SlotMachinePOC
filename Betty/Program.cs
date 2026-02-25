using Betty.Core.Models;
using Betty.Infrastructure;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

var settings = config
    .GetSection("GameSettings")
    .Get<GameSettings>();


if (settings == null)
    throw new InvalidOperationException("Missing game settings");


var repository = new InMemoryWalletRepository();
var wallet = new Wallet(repository);

var console = new MyConsole();
var game = new Game(wallet, settings, new MyRandom());
var application = new ApplicationInteractor(wallet, game, console);


//starts the how program
application.Run();