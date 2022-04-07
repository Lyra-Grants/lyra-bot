using LyraBot.ConfigurationSettings;
using LyraBot.Dtos;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.JsonRpc.WebSocketStreamingClient;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.Reactive.Eth;
using Nethereum.RPC.Reactive.Eth.Subscriptions;
using Nethereum.Web3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using LyraBot.Traders;
using LyraBot.TwitterBot;
using Nethereum.ENS;
using System.Collections;

namespace LyraBot.LyraProg
{
    public class Lyra
    {
        private static string _optionMarketABI = Abi.optionMarketContractABI;
        private static LyraConfig _lyraConfig;
        private readonly Web3 _web3;
        private static Contract _contract;
        private static ContractHandler _contractHandler;
        private List<string> _profitableTraderAddresses = TraderData.ProfitableTraderAddresses;
        private List<BigInteger> _liveBoardIds = new();
        private List<OptionBoardsOutputDto> _boards = new();
        private List<BigInteger> _boardListingIds = new();
        private List<OptionListingOutputDto> _listings = new();
        private Twitter _twitter = null;
        private Enums.Token _token;
        private ENSService _ensService;
        private Hashtable _ensLookup = new();
        private List<string> _noEnsAddresses = new();
        private TelegramBot.TelegramBot _telegram = null;
      

        public Lyra(AppConfig config, Twitter twitter, TelegramBot.TelegramBot telegram, LyraParams lyraParams)
        {
            _lyraConfig = config.LyraConfig;
            Console.WriteLine("Web3 Url:" + _lyraConfig.Web3Url);

            var contractAddress = MapContractAddress(lyraParams.SelectedToken);

            _web3 = new Web3(_lyraConfig.Web3Url);
            _ensService = new ENSService(new Web3(_lyraConfig.MainNetWeb3Url));
            _token = lyraParams.SelectedToken;
            _contract = _web3.Eth.GetContract(_optionMarketABI, contractAddress);
            _contractHandler = _web3.Eth.GetContractHandler(contractAddress);
            _twitter = twitter;
            _telegram = telegram;
        }

        public async Task<string> ResolveEnsName(string address)
        {
            if(_ensLookup.ContainsKey(address))
            {
                return (string)_ensLookup[address];
            }
            
            if(_noEnsAddresses.Contains(address))
            {
                return null;
            }

            var ethName = await _ensService.ReverseResolveAsync(address);

            if(ethName != null)
            {
                if (!_ensLookup.ContainsKey(address))
                {
                    _ensLookup.Add(address, ethName);
                }
            }
            else
            {
                if (!_noEnsAddresses.Contains(address))
                {
                    _noEnsAddresses.Add(address);
                }
             
            }

            return ethName;
        }

        public void TokenInitialise(LyraParams lyraParams)
        {
            //Console.WriteLine("Set the token");
            //_token = lyraParams.SelectedToken;
            //var contractAddress = MapContractAddress(lyraParams.SelectedToken);
            //_contract = _web3.Eth.GetContract(_optionMarketABI, contractAddress);
            //_contractHandler = _web3.Eth.GetContractHandler(contractAddress);
            //Console.WriteLine("Token / Contract initilised");
        }

        public async Task InitialiseBoards()
        {
            Console.WriteLine("Initialise Boards");

            var liveBoardIds = await GetLiveBoardIds();
            _liveBoardIds.AddRange(liveBoardIds);

            var boards = await GetBoards(_liveBoardIds);
            _boards.AddRange(boards);

            var boardListings = await GetBoardsListings(_liveBoardIds);
            _boardListingIds.AddRange(boardListings);

            var boardOptionListings = await GetOptionListings(_boardListingIds);
            _listings.AddRange(boardOptionListings);

            //Console.WriteLine("Live Board Ids:" + string.Join(", ", _liveBoardIds));
            // Console.WriteLine("Boards:" + string.Join(", ", _boards.Select(x => UnixTimestampToDateTime((int)x.Expiry))));

            //Console.WriteLine("Board Listing Ids:" + string.Join(", ", _boards.Select(x => _boardListingIds)));
        }

        public void Reset()
        {
            _liveBoardIds = new List<BigInteger>();
            _boards = new List<OptionBoardsOutputDto>();
            _boardListingIds = new List<BigInteger>();
            _listings = new List<OptionListingOutputDto>();
            Console.WriteLine("Reset Boards");
        }

        public async Task<List<BigInteger>> GetLiveBoardIds()
        {
            Console.WriteLine("Get live Board Ids");
            return await _contractHandler.QueryAsync<GetLiveBoardsFunction, List<BigInteger>>();
        }

        public async Task<List<OptionBoardsOutputDto>> GetBoards(List<BigInteger> boardIds)
        {
            List<OptionBoardsOutputDto> boards = new();

            foreach (var boardId in boardIds)
            {
                boards.Add(await GetBoard(boardId));
            }

            return boards;
        }

        public async Task<OptionBoardsOutputDto> GetBoard(BigInteger boardId)
        {
            return await _contractHandler.QueryDeserializingToObjectAsync<OptionBoardsFunction, OptionBoardsOutputDto>(
                new OptionBoardsFunction() { ReturnValue1 = boardId });
        }

        public async Task<List<BigInteger>> GetBoardsListings(List<BigInteger> boardIds)
        {
            List<BigInteger> listIds = new();

            foreach (var boardId in boardIds)
            {
                listIds.AddRange(await GetBoardListingIds(boardId));
            }

            return listIds;
        }

        public async Task<List<BigInteger>> GetBoardListingIds(BigInteger boardId)
        {
            return await _contractHandler.QueryAsync<GetBoardListingsFunction, List<BigInteger>>(
                new GetBoardListingsFunction() { BoardId = boardId });
        }

        public async Task<List<OptionListingOutputDto>> GetOptionListings(List<BigInteger> listingIds)
        {
            List<OptionListingOutputDto> listIds = new();

            foreach (var listingId in listingIds)
            {
                listIds.Add(await GetOptionListings(listingId));
            }

            return listIds;
        }

        public async Task<OptionListingOutputDto> GetOptionListings(BigInteger listingId)
        {
            return await _contractHandler.QueryDeserializingToObjectAsync<OptionListingsFunction, OptionListingOutputDto>(
                new OptionListingsFunction() { ReturnValue1 = listingId });
        }

        public EthLogsObservableSubscription CreateSubscription<T>(StreamingWebSocketClient client) where T : IPositionEventDTO, IEventDTO, new()
        {
            var subscription = new EthLogsObservableSubscription(client);
            subscription.GetSubscriptionDataResponsesAsObservable()
              .Subscribe(async log =>
              {
                  try
                  {
                      if (log.IsLogForEvent<T>())
                      {
                          var eventDecoded = Event<T>.DecodeEvent(log);
                          if (eventDecoded != null)
                          {
                              var trade = await Map(eventDecoded, _token);
                              await _twitter.Tweet(trade);
                              await _telegram.Message(trade);
                          }
                      }
                  }
                  catch (Exception e)
                  {
                      Console.WriteLine(e);
                      throw;
                  }
              }
              );

            return subscription;
        }

        public async Task Test()
        {
            var trade = new TradeDto()
            {
                Asset = "Eth",
                IsLong = true,
                IsCall = true,
                Strike = 5000,
                Expiry = DateTime.Now,
                Amount = 10,
                TotalCost = 5000,
                TraderAddress = "TEST",
                Transaction = "TxTEST",
                PositionOpened = true,
                ProfitableTrader = true,
                ProfitableTraderPosition = 1,
                Ens = "test.eth",
            };

            await _telegram.Message(trade);
        }

        public async Task WatchPositionsOpened()
        {
            // position opened
            var positionOpenedEventHandler = _contract.GetEvent("PositionOpened");
            var poFilter = positionOpenedEventHandler.CreateFilterInput(BlockParameter.CreateEarliest(), BlockParameter.CreateLatest());

            //position closed
            var positionClosedEventHandler = _contract.GetEvent("PositionClosed");
            var pcFilter = positionClosedEventHandler.CreateFilterInput(BlockParameter.CreateEarliest(), BlockParameter.CreateLatest());


            using var client = new StreamingWebSocketClient(_lyraConfig.WebSocketClientString);

            var positionOpenedSubscription = CreateSubscription<PositionOpenedEventDTO>(client);
            var positionClosedSubscription = CreateSubscription<PositionClosedEventDTO>(client);


            await client.StartAsync();
            positionOpenedSubscription.GetSubscriptionDataResponsesAsObservable().Subscribe(log => Console.WriteLine($"Position Opened!: {log.Address}"));
            positionClosedSubscription.GetSubscriptionDataResponsesAsObservable().Subscribe(log => Console.WriteLine($"Position Closed!: {log.Address}"));

            await positionOpenedSubscription.SubscribeAsync(poFilter);
            await positionClosedSubscription.SubscribeAsync(pcFilter);


            while (true) //pinging to keep wss alive
            {
                var handler = new EthBlockNumberObservableHandler(client);
                handler.GetResponseAsObservable().Subscribe(x => Console.WriteLine($"Pinging Connection - Block: " + x.Value));
                await handler.SendRequestAsync();
                await Task.Delay(30000);
            }
        }

        public async Task<TradeDto> Map<T>(EventLog<T> source, Enums.Token token) where T : IPositionEventDTO
        {
            var listing = _listings.FirstOrDefault(x => x.Id == source?.Event?.ListingId);
            var board = _boards.FirstOrDefault(x => x.Id == listing?.BoardId);

            // boards have updated reset them
            if (board == null)
            {
                Reset();
                await InitialiseBoards();
                board = _boards.FirstOrDefault(x => x.Id == listing.BoardId);
            }
            var traderAddress = source.Event.Trader.ToLower();

            var traderPosition = MapProfitableTrader(traderAddress);
            var ens = await ResolveEnsName(traderAddress) ?? string.Empty;

            return new TradeDto()
            {
                PositionOpened = source.Log.IsLogForEvent<PositionOpenedEventDTO>(),
                ProfitableTrader = traderPosition > 0,
                ProfitableTraderPosition = traderPosition, 
                Asset = MapAsset(token),
                IsLong = IsLong(source.Event.TradeType),
                IsCall = IsCall(source.Event.TradeType),
                Strike = ConvertDouble(listing.Strike),
                Expiry = UnixTimestampToDateTime((int)board.Expiry),
                Amount = ConvertDouble(source.Event.Amount),
                TotalCost = ConvertDouble(source.Event.TotalCost),
                TraderAddress = traderAddress,
                Transaction = source.Log.TransactionHash,
                Ens = ens
            };
        }

        private int MapProfitableTrader(string traderAddress)
        {
            return _profitableTraderAddresses.IndexOf(traderAddress) + 1;
        }

        public string MapAsset(Enums.Token token)
        {
            if (token == Enums.Token.Btc)
            {
                return "BTC";
            }

            if (token == Enums.Token.Link)
            {
                return "LINK";
            }

            if (token == Enums.Token.Sol)
            {
                return "SOL";
            }

            return "ETH";
        }

        public bool IsLong(int tradeType)
        {
            return (tradeType == 0 || tradeType == 2);
        }

        public bool IsCall(int tradeType)
        {
            return (tradeType == 0 || tradeType == 1);
        }

        public DateTime UnixTimestampToDateTime(long unixTime)
        {
            DateTime unixStart = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            long unixTimeStampInTicks = unixTime * TimeSpan.TicksPerSecond;
            return new DateTime(unixStart.Ticks + unixTimeStampInTicks, DateTimeKind.Utc);
        }

        public double ConvertDouble(BigInteger value)
        {
            var result = (decimal)value / 1000000000000000000;
            return (double)result;
        }

        public string MapContractAddress(Enums.Token token)
        {
            if (token == Enums.Token.Btc)
            {
                Console.WriteLine("Btc Contract Address: " + _lyraConfig.BtcContractAddress);
                return _lyraConfig.BtcContractAddress;
            }

            if (token == Enums.Token.Link)
            {
                Console.WriteLine("Link Contract Address: " + _lyraConfig.LinkContractAddress);
                return _lyraConfig.LinkContractAddress;
            }

            if (token == Enums.Token.Sol)
            {
                Console.WriteLine("Sol Contract Address: " + _lyraConfig.SolContractAddress);
                return _lyraConfig.SolContractAddress;
            }

            Console.WriteLine("Eth Contract Address: " + _lyraConfig.EthContractAddress);
            return _lyraConfig.EthContractAddress;
        }

    }
}
