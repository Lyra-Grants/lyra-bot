using System;
using System.Text;
using LyraBot.Utils;

namespace LyraBot.TwitterBot
{
    public class TradeDto
    {
        public string Asset { get; set; }
        public bool IsLong { get; set; }
        public bool IsCall { get; set; }
        public double Strike { get; set; }
        public DateTime Expiry { get; set; }
        public double Amount { get; set; }
        public double TotalCost { get; set; }
        public string TraderAddress { get; set; }
        public string Transaction { get; set; }
        public bool PositionOpened { get; set; }
        public bool ProfitableTrader { get; set; }
        public int ProfitableTraderPosition { get; set; }
        public string Ens { get; set; }

        public string Synopsis()
        {
            return $"{(PositionOpened ? "Position Opened" : "Position Closed")} ${Asset} {(IsLong ? "LONG" : "SHORT")} ${string.Format("{0:N0}", Strike)} {(IsCall ? "CALL" : "PUT")}";
        }

        public string GenerateTweet()
        {
            var sb = new StringBuilder();
            if (ProfitableTrader)
            {
                sb.Append($"💵💵💵 💵 #{ProfitableTraderPosition} Top Trader 💵 💵💵💵" + Environment.NewLine);
            }
            sb.Append(Synopsis() + Environment.NewLine);
            sb.Append($"Exp: {Expiry.ToString("dd MMM yy")}  |  Amount: {string.Format("{0:N2}", Amount)}  |  {AmountWording()}: ${string.Format("{0:N2}", TotalCost)} {TradeSize()}" + Environment.NewLine);
            sb.Append($"Trader: {(!String.IsNullOrEmpty(Ens) ? Ens : TraderAddress)}" + Environment.NewLine);
            sb.Append($"Trades: https://app.lyra.finance/trade/portfolio/positions?see={TraderAddress}" + Environment.NewLine);
            sb.Append($"Txn: https://optimistic.etherscan.io/tx/{Transaction}" + Environment.NewLine);
            return sb.ToString();
        }

        public string GenerateTelegramMessage()
        {
            var sb = new StringBuilder();
            var traderAddress = !string.IsNullOrEmpty(Ens) ? Ens : TraderAddress;
          

            if (ProfitableTrader)
            {
                sb.Append($"💵💵💵 #{ProfitableTraderPosition} Top Trader 💵💵💵" + Environment.NewLine);
            }
            sb.Append($"${Asset} {(IsLong ? "LONG" : "SHORT")} ${string.Format("{0:N0}", Strike)} {(IsCall ? "CALL" : "PUT")}" + Environment.NewLine);
            sb.Append($"{(PositionOpened ? "Position Opened" : "Position Closed")}" + Environment.NewLine);
            sb.Append($"-------------------------------------------------------" + Environment.NewLine);
            sb.Append($"Exp: {Expiry.ToString("dd MMM yy")}  |  Amount: {string.Format("{0:N2}", Amount)}" + Environment.NewLine);
            sb.Append($"{AmountWording()}: ${string.Format("{0:N2}", TotalCost)}" + Environment.NewLine);
            if (!string.IsNullOrEmpty(Ens))
            {
                sb.Append($"Trader: {Ens}" + Environment.NewLine);
            }
            else
            {
                sb.Append($"Trader: {TraderAddress.Truncate(20)}" + Environment.NewLine);
            }
            sb.Append($"Trades: <a href='https://app.lyra.finance/trade/portfolio/positions?see={TraderAddress}'>Open</a>  |  <a href='https://app.lyra.finance/trade/portfolio/positions?see={TraderAddress}&tab=1'>Expired</a>  |  <a href='https://app.lyra.finance/trade/portfolio/history?see={TraderAddress}'>History</a>" + Environment.NewLine);
            sb.Append($"-------------------------------------------------------" + Environment.NewLine);
            sb.Append($"<a href='https://zapper.fi/account/{traderAddress}'>Zapper</a>  |  <a href='https://debank.com/profile/{TraderAddress}'>Debank</a>  |  <a href='https://optimistic.etherscan.io/tx/{Transaction}'>Transaction</a>" + Environment.NewLine);
            sb.Append($"-------------------------------------------------------" + Environment.NewLine);
            return sb.ToString();

        }
        private string AmountWording()
        {
            var paid = "Total Paid";
            var received = "Total Rcvd";

            if (PositionOpened)
            {
                return IsLong ? paid : received;
            }

            return IsLong ? received : paid;
        }

        private string TradeSize()
        {
            if (TotalCost < 1500)
            {
                return "🐟";
            }
            else if (TotalCost >= 1500 && TotalCost < 10000)
            {
                return "🐬";
            }

            else if (TotalCost >= 10000)
            {
                return "🐋";
            }

            return string.Empty;
        }
    }
}
