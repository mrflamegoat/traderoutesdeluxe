using ProtoBuf;
using Vintagestory.API.MathTools;

namespace TradeRoutesDeluxe.Common.Network {

    [ProtoContract]
    public class TradingPostLocation {

        [ProtoMember(1)]
        public string PostId;

        [ProtoMember(2)]
        public BlockPos BlockPosition;
    }
}