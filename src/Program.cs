using LyraBot.ConfigurationSettings;
using LyraBot.LyraProg;
using LyraBot.TwitterBot;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;
using Utils;

namespace LyraBot
{
    public class Program
    {
        protected static IServiceProvider _ServiceProvider;
        private static int _defaultMaxAttempts = 5;

        public static async Task Main(string[] args)
        {
            var configuration = InitOptions<AppConfig>();
            Console.WriteLine("Let' go....");
            var token = MapArgs(args);
            // var maxRetryAttempts = MapMaxAttempts(args);
            var twitterEnabled = TwitterEnabled(args);
            var telegramEnabled = TelegramEnabled(args);

            var services = new ServiceCollection()
                .AddSingleton(configuration)
                .AddSingleton(_ => new LyraParams { SelectedToken = token, TelegramEnabled = telegramEnabled, TwitterEnabled = twitterEnabled})
                .AddSingleton<Lyra>()
                .AddSingleton<Twitter>()
                .AddSingleton<TelegramBot.TelegramBot>();


            _ServiceProvider = services.BuildServiceProvider();

            try
            {
               await RetryHelper.RetryOnExceptionAsync<Exception>
                  (_defaultMaxAttempts, async () =>
                  {
                      await Run();
                  });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Bot shut down. :(");
                Console.WriteLine(ex.Message);
            }
        }

        private static async Task Run()
        {

            var lyraEth = _ServiceProvider.GetService<Lyra>();
            lyraEth.Reset();
            await lyraEth.InitialiseBoards();
            await lyraEth.WatchPositionsOpened();
            Console.WriteLine("Running Bot");
        }

        private static T InitOptions<T>() where T : new()
        {
            var config = InitConfig();
            return config.Get<T>();
        }

        private static IConfigurationRoot InitConfig()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile($"appsettings.json", true, true)
                .AddEnvironmentVariables();

            return builder.Build();
        }

        private static int MapMaxAttempts(string[] args)
        {
            if (args == null || args.Length <= 2)
            {
                Console.WriteLine($"No args - default {_defaultMaxAttempts}");
                return _defaultMaxAttempts;
            }

            _ = int.TryParse(args[1], out int result);

            return result;
        }

        private static bool TwitterEnabled(string[] args)
        {
            if (args == null || args.Length <= 2)
            {
                return true;
            }

            _ = bool.TryParse(args[1], out bool result);

            return result;
        }

        private static bool TelegramEnabled(string[] args)
        {
            if (args == null || args.Length <= 3)
            {
                return true;
            }

            _ = bool.TryParse(args[2], out bool result);

            return result;
        }


        private static Enums.Token MapArgs(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                Console.WriteLine("No args - default Eth Bot");
                return Enums.Token.Eth;
            }

            var token = args[0].ToLower();

            if (token == "btc")
            {
                Console.WriteLine("Btc Bot");
                return Enums.Token.Btc;
            }

            if (token == "link")
            {
                Console.WriteLine("Link Bot");
                return Enums.Token.Link;
            }

            if (token == "sol")
            {
                Console.WriteLine("Sol Bot");
                return Enums.Token.Sol;
            }

            Console.WriteLine("Eth Bot");
            return Enums.Token.Eth;
        }
    }
}


