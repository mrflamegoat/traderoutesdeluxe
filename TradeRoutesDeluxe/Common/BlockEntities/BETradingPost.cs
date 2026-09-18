using System;
using TradeRoutesDeluxe.Common.Items;
using TradeRoutesDeluxe.Common.Network;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace TradeRoutesDeluxe.Common.BlockEntities {

    public class BlockEntityTradingPost : BlockEntityOpenableContainer {

        public int quantitySlots = 16;
        public string inventoryClassName = TradeRoutesHandler.InventoryClassName;

        private string blockEnityId;
        private string networkId;

        private InventoryGeneric unlinkedInventory;

        private InventoryBase subscribedInventory;

        public static string DialogTitle {
            get { return Lang.Get("block-tradingpost"); }
        }

        public override InventoryBase Inventory {
            get {
                if (networkId != null) {
                    InventoryGeneric shared = Handler?.GetOrCreateInventory(networkId, quantitySlots, Api);
                    if (shared != null) return shared;
                }

                return unlinkedInventory;
            }
        }

        public override string InventoryClassName {
            get { return inventoryClassName; }
        }

        private TradeRoutesHandler Handler {
            get { return Api?.ModLoader.GetModSystem<TradeRoutesSystem>()?.TradeRoutesHandler; }
        }

        public BlockEntityTradingPost() : base() {
            unlinkedInventory = new InventoryGeneric(quantitySlots, null, null, null);
        }

        public override void Initialize(ICoreAPI api) {
            if (Block?.Attributes != null) {
                inventoryClassName = Block.Attributes["inventoryClassName"].AsString(inventoryClassName);
                quantitySlots = Block.Attributes["quantitySlots"].AsInt(quantitySlots);

                if (unlinkedInventory.Count != quantitySlots && unlinkedInventory.Empty) {
                    unlinkedInventory = new InventoryGeneric(quantitySlots, null, null, null);
                }
            }

            if (blockEnityId == null) blockEnityId = Guid.NewGuid().ToString();

            base.Initialize(api);

            Inventory.OnInventoryOpened -= OnInventoryOpened;
            Inventory.OnInventoryClosed -= OnInventoryClosed;

            if (networkId != null) AttachToNetwork();

            SubscribeTo(Inventory);
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving) {
            networkId = tree.GetString("networkId");
            blockEnityId = tree.GetString("blockEntityId");

            if (networkId != null) tree.RemoveAttribute("inventory");

            base.FromTreeAttributes(tree, worldForResolving);
        }

        public override void ToTreeAttributes(ITreeAttribute tree) {
            base.ToTreeAttributes(tree);

            if (networkId != null) {
                tree.RemoveAttribute("inventory");
                tree.SetString("networkId", networkId);
            }

            if (blockEnityId != null) tree.SetString("blockEntityId", blockEnityId);
        }

        public override bool OnPlayerRightClick(IPlayer byPlayer, BlockSelection blockSel) {
            if (byPlayer?.Entity?.Controls?.Sneak == true) {
                ItemSlot hotbarSlot = byPlayer.InventoryManager.ActiveHotbarSlot;

                if (hotbarSlot?.Itemstack?.Item is ItemPostParchmentUnwritten) {
                    if (byPlayer.Entity.World is IServerWorldAccessor) {
                        if (networkId == null) {
                            TradingPostLocation post = new() {
                                PostId = blockEnityId,
                                BlockPosition = blockSel.Position
                            };

                            LinkToNetwork(Handler.CreateOrAddLocation(byPlayer as IServerPlayer, post));
                        }

                        if (networkId == null) return false;

                        hotbarSlot.TakeOut(1);
                        hotbarSlot.MarkDirty();

                        ItemPostParchmentWritten writtenPaper = byPlayer.Entity.World.GetItem(new AssetLocation("traderoutesdeluxe:post_parchment_written")) as ItemPostParchmentWritten;

                        ItemStack stack = new(writtenPaper, 1);
                        stack.Attributes.SetString("networkId", networkId);

                        if (!byPlayer.InventoryManager.TryGiveItemstack(stack, true)) {
                            byPlayer.Entity.World.SpawnItemEntity(stack, byPlayer.Entity.Pos.XYZ.Add(0, 0.5, 0));
                        }
                    }

                    return true;
                }

                if (hotbarSlot?.Itemstack?.Item is ItemPostParchmentWritten) {
                    string incomingNetworkId = hotbarSlot.Itemstack.Attributes.GetString("networkId");
                    if (incomingNetworkId == null) return false;

                    if (byPlayer.Entity.World is IServerWorldAccessor) {
                        TradingPostLocation post = new() {
                            PostId = blockEnityId,
                            BlockPosition = blockSel.Position
                        };

                        string joinedNetworkId = Handler.CreateOrAddLocation(byPlayer as IServerPlayer, post, incomingNetworkId);

                        if (joinedNetworkId == null) {
                            joinedNetworkId = Handler.CreateOrAddLocation(byPlayer as IServerPlayer, post);

                            hotbarSlot.Itemstack.Attributes.SetString("networkId", joinedNetworkId);
                            hotbarSlot.MarkDirty();
                        }

                        LinkToNetwork(joinedNetworkId);
                    }

                    return true;
                }
            } else {
                if (Api.World is IServerWorldAccessor) {
                    byte[] data = BlockEntityContainerOpen.ToBytes("BlockEntityInventory", DialogTitle, 4, Inventory);

                    ((ICoreServerAPI)Api).Network.SendBlockEntityPacket(
                        (IServerPlayer)byPlayer,
                        Pos,
                        (int)EnumBlockContainerPacketId.OpenInventory,
                        data
                    );

                    byPlayer.InventoryManager.OpenInventory(Inventory);
                }
            }

            return true;
        }

        public override void OnBlockBroken(IPlayer byPlayer = null) {
            if (networkId != null) {
                InventoryBase shared = Inventory;
                bool wasLastPost = Handler?.RemoveTradingPost(blockEnityId, networkId) ?? false;

                if (wasLastPost && Api is ICoreServerAPI && shared != null && shared != unlinkedInventory) {
                    shared.DropAll(Pos.ToVec3d().Add(0.5, 0.5, 0.5));
                    Handler?.DeleteNetwork(networkId);
                }

                DetachFromNetwork();

                networkId = null;
            }

            base.OnBlockBroken(byPlayer);
        }

        public override void OnBlockUnloaded() {
            DetachFromNetwork();

            base.OnBlockUnloaded();
        }

        public override void OnBlockRemoved() {
            DetachFromNetwork();

            base.OnBlockRemoved();
        }

        private void LinkToNetwork(string newNetworkId) {
            if (newNetworkId == null || newNetworkId == networkId) return;

            DetachFromNetwork();
            networkId = newNetworkId;
            AttachToNetwork();
            SubscribeTo(Inventory);

            TransferUnlinkedContents();
            MarkDirty(true);
        }

        private void AttachToNetwork() {
            InventoryGeneric shared = Handler?.GetOrCreateInventory(networkId, quantitySlots, Api);
            if (shared == null) return;

            shared.LateInitialize(TradeRoutesHandler.InventoryIdFor(networkId), Api);
            shared.ResolveBlocksOrItems();

        }

        private void DetachFromNetwork() {
            UnsubscribeCurrent();
        }

        private void SubscribeTo(InventoryBase inventory) {
            if (inventory == null || inventory == subscribedInventory) return;

            UnsubscribeCurrent();

            inventory.OnInventoryOpened += OnInventoryOpened;
            inventory.OnInventoryClosed += OnInventoryClosed;
            inventory.OnInventoryClosed += DisposeDialog;

            subscribedInventory = inventory;
        }

        private void UnsubscribeCurrent() {
            if (subscribedInventory == null) return;

            subscribedInventory.OnInventoryOpened -= OnInventoryOpened;
            subscribedInventory.OnInventoryClosed -= OnInventoryClosed;
            subscribedInventory.OnInventoryClosed -= DisposeDialog;

            subscribedInventory = null;
        }

        private void DisposeDialog(IPlayer player) {
            invDialog?.Dispose();
            invDialog = null;
        }

        private void TransferUnlinkedContents() {
            InventoryBase shared = Inventory;
            if (shared == null || shared == unlinkedInventory || Api?.World == null) return;

            for (int i = 0; i < unlinkedInventory.Count; i++) {
                ItemSlot source = unlinkedInventory[i];
                if (source.Empty) continue;

                for (int j = 0; j < shared.Count; j++) {
                    if (!shared[j].Empty) continue;

                    source.TryPutInto(Api.World, shared[j], source.StackSize);
                    break;
                }
            }
        }
    }
}
