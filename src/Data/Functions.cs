using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using System.Numerics;
using LyraBot.Dtos;

namespace LyraBot
{
    [Function("getLiveBoards", "uint256[]")]
    public class GetLiveBoardsFunction : FunctionMessage
    {}

    [Function("getBoardListings", "uint256[]")]
    public class GetBoardListingsFunction : FunctionMessage
    {
        [Parameter("uint256", "boardId", 1)]
        public virtual BigInteger BoardId { get; set; }
    }

    [Function("optionListings", typeof(OptionListingOutputDto))]
    public class OptionListingsFunction : FunctionMessage
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    [FunctionOutput]
    public class OptionBoardsOutputDto : IFunctionOutputDTO
    {
        [Parameter("uint256", "id", 1)]
        public virtual BigInteger Id { get; set; }
        [Parameter("uint256", "expiry", 2)]
        public virtual BigInteger Expiry { get; set; }
        [Parameter("uint256", "iv", 3)]
        public virtual BigInteger Iv { get; set; }
        [Parameter("bool", "frozen", 4)]
        public virtual bool Frozen { get; set; }
    }

    [Function("optionBoards", typeof(OptionBoardsOutputDto))]
    public class OptionBoardsFunction : FunctionMessage
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

}
