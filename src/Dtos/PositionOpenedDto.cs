using Nethereum.ABI.FunctionEncoding.Attributes;
using System.Numerics;

namespace LyraBot.Dtos
{
    [Event("PositionOpened")]
    public class PositionOpenedEventDTO : IEventDTO, IPositionEventDTO
    {
        [Parameter("address", "trader", 1, true)]
        public virtual string Trader { get; set; }
        [Parameter("uint256", "listingId", 2, true)]
        public virtual BigInteger ListingId { get; set; }
        [Parameter("uint8", "tradeType", 3, true)]
        public virtual byte TradeType { get; set; }
        [Parameter("uint256", "amount", 4, false)]
        public virtual BigInteger Amount { get; set; }
        [Parameter("uint256", "totalCost", 5, false)]
        public virtual BigInteger TotalCost { get; set; }
    }
}
