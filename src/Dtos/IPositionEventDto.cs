using System.Numerics;

namespace LyraBot.Dtos
{
    public interface IPositionEventDTO
    {
        public string Trader { get; set; }
        public BigInteger ListingId { get; set; }
        public byte TradeType { get; set; }
        public BigInteger Amount { get; set; }
        public BigInteger TotalCost { get; set; }
    }
}
