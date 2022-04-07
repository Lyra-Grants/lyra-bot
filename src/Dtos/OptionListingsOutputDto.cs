using Nethereum.ABI.FunctionEncoding.Attributes;
using System.Numerics;

namespace LyraBot.Dtos
{
    [FunctionOutput]
    public class OptionListingOutputDto : IFunctionOutputDTO
    {
        [Parameter("uint256", "id", 1)]
        public virtual BigInteger Id { get; set; }
        [Parameter("uint256", "strike", 2)]
        public virtual BigInteger Strike { get; set; }
        [Parameter("uint256", "skew", 3)]
        public virtual BigInteger Skew { get; set; }
        [Parameter("uint256", "longCall", 4)]
        public virtual BigInteger LongCall { get; set; }
        [Parameter("uint256", "shortCall", 5)]
        public virtual BigInteger ShortCall { get; set; }
        [Parameter("uint256", "longPut", 6)]
        public virtual BigInteger LongPut { get; set; }
        [Parameter("uint256", "shortPut", 7)]
        public virtual BigInteger ShortPut { get; set; }
        [Parameter("uint256", "boardId", 8)]
        public virtual BigInteger BoardId { get; set; }
    }
}
