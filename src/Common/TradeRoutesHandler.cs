using System;
using System.Collections.Generic;
using Vintagestory.API.Server;
using TradeRoutesDeluxe.Common.Network;
using Vintagestory.API.Util;
using TradeRoutesDeluxe.Common.Utils;

namespace TradeRoutesDeluxe.Common
{
    public class TradeRoutesHandler
    {
        private TradeRoutesSystem System { get; }

        Dictionary<string, TradingPostNetwork> networks = new Dictionary<string, TradingPostNetwork>();

        public TradeRoutesHandler(TradeRoutesSystem system)
            => System = system;

        public void InitClient()
        {
            System.ClientChannel
                .SetMessageHandler<TradingPostNetwork>(OnNetworkCreation)
                .SetMessageHandler<Dictionary<string, TradingPostNetwork>>(OnNetworksLoad)
                ;
        }

        public void InitServer()
        {
            System.ServerAPI.Event.SaveGameLoaded += OnSaveGameLoaded;
            System.ServerAPI.Event.GameWorldSave += OnGameGettingSaved;
            System.ServerAPI.Event.PlayerJoin += OnPlayerJoin;
        }

        public string CreateOrAddLocation(IServerPlayer forPlayer, TradingPostLocation tradePosLoc, string networkId = null)
        {
            if (networkId == null)
            {
                networkId = Guid.NewGuid().ToString();
                TradingPostNetwork network = new TradingPostNetwork();
                network.NetworkId = networkId;
                network.Locations = new Dictionary<string, TradingPostLocation>();
                network.Locations.Add(tradePosLoc.PostId, tradePosLoc);

                this.networks.Add(network.NetworkId, network);
                System.ServerChannel.SendPacket(network, forPlayer);
            }
            else
            {
                if (!this.networks.ContainsKey(networkId)) return null;

                if (!this.networks[networkId].Locations.ContainsKey(tradePosLoc.PostId))
                {
                    this.networks[networkId].Locations.Add(tradePosLoc.PostId, tradePosLoc);
                    System.ServerChannel.SendPacket(this.networks[networkId], forPlayer);
                }
            }

            return this.networks[networkId].NetworkId;
        }

        public void RemoveTradingPost(string postId, string networkId)
        {
            if (this.networks.ContainsKey(networkId) && this.networks[networkId].Locations.ContainsKey(postId))
            {
                this.networks[networkId].Locations.Remove(postId);
            }
        }

        public void SyncInventories(string originBlockId, string networkId, byte[] slotBytes)
        {
            if (!this.networks.ContainsKey(networkId)) return;

            this.networks[networkId].slots = slotBytes;
            foreach (KeyValuePair<string, TradingPostLocation> location in this.networks[networkId].Locations)
            {
                if (location.Value.PostId == originBlockId || slotBytes == null) continue;
                if (System.ClientAPI != null)
                {
                    System.ClientAPI.Network.SendBlockEntityPacket(location.Value.BlockPosition.X, location.Value.BlockPosition.Y, location.Value.BlockPosition.Z, (int)EnumTradingPostPackets.SyncInventory, slotBytes);
                }
                else
                {
                    System.ServerAPI.Network.BroadcastBlockEntityPacket(location.Value.BlockPosition.X, location.Value.BlockPosition.Y, location.Value.BlockPosition.Z, (int)EnumTradingPostPackets.SyncInventory, slotBytes);
                }
            };
        }

        public byte[] GetTree(string networkId)
        {
            return this.networks?[networkId]?.slots ?? null;
        }

        private void OnNetworkCreation(TradingPostNetwork incomingNetwork)
        {
            if (this.networks.ContainsKey(incomingNetwork.NetworkId))
            {
                this.networks[incomingNetwork.NetworkId] = incomingNetwork;
                return;
            }
            this.networks.Add(incomingNetwork.NetworkId, incomingNetwork);
        }

        private void OnNetworksLoad(Dictionary<string, TradingPostNetwork> networksFromSave)
        {
            this.networks = networksFromSave;
        }

        private void OnGameGettingSaved()
        {
            System.ServerAPI.WorldManager.SaveGame.StoreData("tradingPostNetworksList", SerializerUtil.Serialize(this.networks));
        }

        private void OnPlayerJoin(IServerPlayer byPlayer)
        {
            System.ServerChannel.SendPacket<Dictionary<string, TradingPostNetwork>>(this.networks, byPlayer);
        }

        private void OnSaveGameLoaded()
        {
            try
            {
                byte[] data = System.ServerAPI.WorldManager.SaveGame.GetData("tradingPostNetworksList");
                if (data != null) this.networks = SerializerUtil.Deserialize<Dictionary<string, TradingPostNetwork>>(data);
            }
            catch (Exception e)
            {
                System.ServerAPI.World.Logger.Error("Failed loading tradingPostNetworksList. Resetting. Exception: {0}", e);
            }
            if (this.networks == null) this.networks = new Dictionary<string, TradingPostNetwork>();
        }

    }
}