using LyraBot.ConfigurationSettings;
using System;
using System.Threading.Tasks;
using Tweetinvi;

namespace LyraBot.TwitterBot
{
    public class Twitter
    {
        private int _totalCostThreshold = 500;
        private static TwitterConfig _twitterConfig;
        private TwitterClient _twitterClient = null;

        public Twitter(AppConfig config)
        {
            _twitterConfig = config.TwitterConfig;
            _twitterClient = new TwitterClient(_twitterConfig.ConsumerKey, _twitterConfig.ConsumerSecret, _twitterConfig.AccessToken, _twitterConfig.AccessTokenSecret);
        }

        public async Task Tweet(TradeDto tradeDto)
        {
            try
            {
                if (tradeDto.TotalCost >= _totalCostThreshold || (tradeDto.ProfitableTrader && tradeDto.ProfitableTraderPosition < 26) || (tradeDto.Ens != string.Empty && tradeDto.Amount >= 250))
                {
                    Console.WriteLine($"Tweeting trade: {tradeDto.Synopsis()}");
                    var tweet = await _twitterClient.Tweets.PublishTweetAsync(tradeDto.GenerateTweet());
                    Console.WriteLine($"Tweeted with tweet Id: {tweet.Id}");
                }
                else
                {
                    Console.WriteLine($"Trade not tweeted: less than {_totalCostThreshold} or not profitable.");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
