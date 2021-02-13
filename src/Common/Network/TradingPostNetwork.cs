using ProtoBuf;
using System.Collections.Generic;

namespace TradeRoutesDeluxe.Common.Network
{
    [ProtoContract]
    public class TradingPostNetwork
    {
        [ProtoMember(1)]
        public string NetworkId;

        [ProtoMember(2)]
        public Dictionary<string, TradingPostLocation> Locations;

        [ProtoMember(3)]
        public byte[] slots;
    }
}