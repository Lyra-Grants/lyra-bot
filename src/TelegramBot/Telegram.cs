using LyraBot.ConfigurationSettings;
using LyraBot.TwitterBot;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace LyraBot.TelegramBot
{
    public class TelegramBot
    {
        private static TelegramConfig _telegramConfig;
        private TelegramBotClient _telegramClient = null;

        public TelegramBot(AppConfig config)
        {
            _telegramConfig = config.TelegramConfig;
            _telegramClient = new TelegramBotClient(_telegramConfig.AccessToken);
        }

        public async Task Message(TradeDto tradeDto)
        {
            try
            {
                if(tradeDto.TotalCost >= 250 || tradeDto.ProfitableTrader)
                {
                    using var cts = new CancellationTokenSource();
                    var me = await _telegramClient.GetMeAsync();
                    var sentMessage = await _telegramClient.SendTextMessageAsync(chatId: _telegramConfig.ChannelName, text: tradeDto.GenerateTelegramMessage(), parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, disableWebPagePreview: true, cancellationToken: cts.Token); ;
                    cts.Cancel();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
