using System;
using System.Collections.Generic;
using TradeRoutesDeluxe.Common.Network;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace TradeRoutesDeluxe.Common {

    public class TradeRoutesHandler(TradeRoutesSystem system) {

        public const string InventoryClassName = "traderoutespost";

        private TradeRoutesSystem System { get; } = system;

        Dictionary<string, TradingPostNetwork> networks = [];

        Dictionary<string, InventoryGeneric> inventories = [];

        Dictionary<string, byte[]> storedContents = [];

        public void InitClient() {
            System.ClientChannel
                .SetMessageHandler<TradingPostNetwork>(OnNetworkCreation)
                .SetMessageHandler<Dictionary<string, TradingPostNetwork>>(OnNetworksLoad)
                ;
        }

        public void InitServer() {
            System.ServerAPI.Event.SaveGameLoaded += OnSaveGameLoaded;
            System.ServerAPI.Event.GameWorldSave += OnGameGettingSaved;
            System.ServerAPI.Event.PlayerJoin += OnPlayerJoin;
        }

        public static string InventoryIdFor(string networkId) {
            return InventoryClassName + "-" + networkId;
        }

        public InventoryGeneric GetOrCreateInventory(string networkId, int quantitySlots, ICoreAPI api) {
            if (networkId == null || api == null) return null;

            if (inventories.TryGetValue(networkId, out InventoryGeneric inventory)) return inventory;

            inventory = new InventoryGeneric(quantitySlots, null, null, null);
            inventory.LateInitialize(InventoryIdFor(networkId), api);

            if (storedContents.TryGetValue(networkId, out byte[] contents) && contents != null) {
                inventory.FromTreeAttributes(TreeAttribute.CreateFromBytes(contents));
                inventory.ResolveBlocksOrItems();
                storedContents.Remove(networkId);
            }

            inventories[networkId] = inventory;
            return inventory;
        }

        public string CreateOrAddLocation(IServerPlayer forPlayer, TradingPostLocation tradePosLoc, string networkId = null) {
            if (networkId == null) {
                networkId = Guid.NewGuid().ToString();

                TradingPostNetwork network = new() {
                    NetworkId = networkId,
                    Locations = new Dictionary<string, TradingPostLocation>() { { tradePosLoc.PostId, tradePosLoc } }
                };

                networks.Add(network.NetworkId, network);
                System.ServerChannel.SendPacket(network, forPlayer);
            } else {
                if (!networks.TryGetValue(networkId, out TradingPostNetwork value)) return null;

                if (value.Locations.TryAdd(tradePosLoc.PostId, tradePosLoc)) {
                    System.ServerChannel.SendPacket(value, forPlayer);
                }
            }

            return networks[networkId].NetworkId;
        }

        public bool RemoveTradingPost(string postId, string networkId) {
            if (networkId == null || !networks.TryGetValue(networkId, out TradingPostNetwork value)) return false;
            value.Locations.Remove(postId);

            return value.Locations.Count == 0;
        }

        public void DeleteNetwork(string networkId) {
            if (networkId == null) return;

            networks.Remove(networkId);
            inventories.Remove(networkId);
            storedContents.Remove(networkId);
        }

        private void OnNetworkCreation(TradingPostNetwork incomingNetwork) {
            networks[incomingNetwork.NetworkId] = incomingNetwork;
        }

        private void OnNetworksLoad(Dictionary<string, TradingPostNetwork> networksFromSave) {
            networks = networksFromSave ?? [];
        }

        private void OnGameGettingSaved() {
            System.ServerAPI.WorldManager.SaveGame.StoreData("tradingPostNetworksList", SerializerUtil.Serialize(networks));

            Dictionary<string, byte[]> contents = [with(storedContents)];

            foreach (KeyValuePair<string, InventoryGeneric> inventory in inventories) {
                TreeAttribute tree = new();
                inventory.Value.ToTreeAttributes(tree);
                contents[inventory.Key] = tree.ToBytes();
            }

            System.ServerAPI.WorldManager.SaveGame.StoreData("tradingPostInventories", SerializerUtil.Serialize(contents));
        }

        private void OnPlayerJoin(IServerPlayer byPlayer) {
            System.ServerChannel.SendPacket(networks, byPlayer);
        }

        private void OnSaveGameLoaded() {
            try {
                byte[] data = System.ServerAPI.WorldManager.SaveGame.GetData("tradingPostNetworksList");
                if (data != null) networks = SerializerUtil.Deserialize<Dictionary<string, TradingPostNetwork>>(data);
            } catch (Exception e) {
                System.ServerAPI.World.Logger.Error("Failed loading tradingPostNetworksList. Resetting. Exception: {0}", e);
            }

            try {
                byte[] data = System.ServerAPI.WorldManager.SaveGame.GetData("tradingPostInventories");
                if (data != null) storedContents = SerializerUtil.Deserialize<Dictionary<string, byte[]>>(data);
            } catch (Exception e) {
                System.ServerAPI.World.Logger.Error("Failed loading tradingPostInventories. Resetting. Exception: {0}", e);
            }

            if (networks == null) networks = [];
            if (storedContents == null) storedContents = [];
        }

    }
}
