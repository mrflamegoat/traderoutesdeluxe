using System.Collections.Generic;
using TradeRoutesDeluxe.Common;
using TradeRoutesDeluxe.Common.Network;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace TradeRoutesDeluxe
{
    public class TradeRoutesSystem : ModSystem
    {
        public static string MOD_ID = "traderoutesdeluxe";

        // Client
        public ICoreClientAPI ClientAPI { get; private set; }
        public IClientNetworkChannel ClientChannel { get; private set; }

        // Server
        public ICoreServerAPI ServerAPI { get; private set; }
        public IServerNetworkChannel ServerChannel { get; private set; }

        // Common
        public TradeRoutesHandler TradeRoutesHandler { get; private set; }

        public override bool ShouldLoad(EnumAppSide side)
        {
            return true;
        }

        public override void Start(ICoreAPI api)
        {

            TradeRoutesHandler = new TradeRoutesHandler(this);
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            ClientAPI = api;
            ClientChannel = api.Network.RegisterChannel(MOD_ID)
                .RegisterMessageType<TradingPostNetwork>()
                .RegisterMessageType<Dictionary<string, TradingPostNetwork>>()
            ;

            TradeRoutesHandler.InitClient();
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            ServerAPI = api;
            ServerChannel = api.Network.RegisterChannel(MOD_ID)
                .RegisterMessageType<TradingPostNetwork>()
                .RegisterMessageType<Dictionary<string, TradingPostNetwork>>()
            ;

            TradeRoutesHandler.InitServer();
        }
    }
}