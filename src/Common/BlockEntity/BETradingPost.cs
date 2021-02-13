using System;
using System.IO;
using TradeRoutesDeluxe.Common.Items;
using TradeRoutesDeluxe.Common.Network;
using Vintagestory.API.Server;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;
using TradeRoutesDeluxe.Common.Utils;

namespace TradeRoutesDeluxe.Common.BlockEntities
{

    public class BlockEntityTradingPost : BlockEntityGenericContainer
    {

        private string blockEnityId;

        internal InventoryGeneric inventory;

        private string networkId;

        public string DialogTitle
        {
            get { return Lang.Get("block-tradingpost"); }
        }

        public override InventoryBase Inventory
        {
            get { return inventory; }
        }

        public override string InventoryClassName
        {
            get { return inventoryClassName; }
        }

        public BlockEntityTradingPost() : base() { }

        public override void Initialize(ICoreAPI api)
        {
            // No inventory? New block, create it!
            if (inventory == null)
            {
                InitInventory(Block);
            }

            // No blockEntityId? New block, create it!
            if (this.blockEnityId == null)
            {
                this.blockEnityId = Guid.NewGuid().ToString();
            }

            // Oh ffs read the above two comments.
            if (this.networkId != null)
            {
                // Block exists and has a network, load up it's inventory.
                this.SyncFromNetworkInventory(api);
            }

            base.Initialize(api);
        }

        // Black magic, don't touch.
        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            if (inventory == null)
            {
                if (tree.HasAttribute("forBlockId"))
                {
                    InitInventory(worldForResolving.GetBlock((ushort)tree.GetInt("forBlockId")));
                }
                else if (tree.HasAttribute("forBlockCode"))
                {
                    InitInventory(worldForResolving.GetBlock(new AssetLocation(tree.GetString("forBlockCode"))));
                }
            }

            this.networkId = tree.GetString("networkId");
            this.blockEnityId = tree.GetString("blockEntityId");
            base.FromTreeAttributes(tree, worldForResolving);
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            tree.SetString("networkId", this.networkId);
            tree.SetString("blockEntityId", this.blockEnityId);
            base.ToTreeAttributes(tree);
        }

        public override bool OnPlayerRightClick(IPlayer byPlayer, BlockSelection blockSel)
        {
            if (byPlayer?.Entity?.Controls?.Sneak == true)
            {
                ItemSlot hotbarSlot = byPlayer.InventoryManager.ActiveHotbarSlot;

                if (hotbarSlot?.Itemstack?.Item is ItemPostParchmentUnwritten)
                {
                    hotbarSlot.TakeOut(1);
                    hotbarSlot.MarkDirty();

                    if (byPlayer.Entity.World is IServerWorldAccessor)
                    {
                        if (this.networkId == null)
                        {
                            TradingPostLocation post = new TradingPostLocation();
                            post.PostId = this.blockEnityId;
                            post.BlockPosition = blockSel.Position;

                            this.networkId = Api.ModLoader.GetModSystem<TradeRoutesSystem>().TradeRoutesHandler.CreateOrAddLocation(byPlayer as IServerPlayer, post);

                            this.SyncToNetworkInventory();
                        }

                        ItemPostParchmentWritten writtenPaper = byPlayer.Entity.World.GetItem(new AssetLocation("traderoutesdeluxe:post_parchment_written")) as ItemPostParchmentWritten;

                        ItemStack stack = new ItemStack(writtenPaper, 1);
                        stack.Attributes.SetString("networkId", networkId);

                        if (!byPlayer.InventoryManager.TryGiveItemstack(stack, true))
                        {
                            byPlayer.Entity.World.SpawnItemEntity(stack, byPlayer.Entity.Pos.XYZ.Add(0, 0.5, 0));
                        }

                        return true;
                    }
                }

                if (hotbarSlot?.Itemstack?.Item is ItemPostParchmentWritten)
                {
                    this.networkId = hotbarSlot.Itemstack.Attributes.GetString("networkId");
                    if (this.networkId == null) return false;

                    if (byPlayer.Entity.World is IServerWorldAccessor)
                    {
                        TradingPostLocation post = new TradingPostLocation();
                        post.PostId = this.blockEnityId;
                        post.BlockPosition = blockSel.Position;
                        Api.ModLoader.GetModSystem<TradeRoutesSystem>().TradeRoutesHandler.CreateOrAddLocation(byPlayer as IServerPlayer, post, this.networkId);
                    }

                    this.SyncFromNetworkInventory(Api);

                    return true;
                }
            }
            else
            {
                if (this.networkId != null && Api.World is IServerWorldAccessor)
                {
                    byte[] localInventory = ByteBuilder.InventoryByteBuilder(this.inventory, "BlockEntityTradingPost", DialogTitle);

                    ((ICoreServerAPI)Api).Network.SendBlockEntityPacket(
                        (IServerPlayer)byPlayer,
                        Pos.X, Pos.Y, Pos.Z,
                        (int)EnumBlockContainerPacketId.OpenInventory,
                        localInventory
                    );

                    byPlayer.InventoryManager.OpenInventory(this.inventory);
                }
            }

            return true;
        }

        public override void OnReceivedClientPacket(IPlayer player, int packetid, byte[] data)
        {
            if (packetid == (int)EnumTradingPostPackets.SyncInventory)
            {
                handleNeutralPackets(packetid, data);
            }

            base.OnReceivedClientPacket(player, packetid, data);
        }

        public override void OnReceivedServerPacket(int packetid, byte[] data)
        {
            if (packetid == (int)EnumTradingPostPackets.SyncInventory)
            {
                handleNeutralPackets(packetid, data);
                MarkDirty();
            }

            base.OnReceivedServerPacket(packetid, data);
        }

        public override void OnBlockBroken()
        {
            Api.ModLoader.GetModSystem<TradeRoutesSystem>().TradeRoutesHandler.RemoveTradingPost(this.blockEnityId, this.networkId);
            Inventory.DiscardAll();

            base.OnBlockBroken();
        }

        private void OnSlotModified(int slot)
        {
            Api.World.BlockAccessor.GetChunkAtBlockPos(this.Pos)?.MarkModified();
            this.SyncToNetworkInventory();
        }

        private void InitInventory(Block Block)
        {
            // Ripped right out of the SurvivalMod code.
            this.inventory = new InventoryGeneric(quantitySlots, null, null, null);

            this.inventory.OnInventoryClosed += OnInvClosed;
            this.inventory.OnInventoryOpened += OnInvOpened;
            this.inventory.SlotModified += OnSlotModified;
        }

        private void handleNeutralPackets(int packetid, byte[] data)
        {
            switch (packetid)
            {
                case (int)EnumTradingPostPackets.SyncInventory:
                    this.SyncFromNetworkInventory(Api, data);
                    break;
                default:
                    break;
            }
        }

        // Pass API here because we attempt to use this before it's initialized to the blockentity.
        private void SyncFromNetworkInventory(ICoreAPI api, byte[] data = null)
        {
            byte[] serializedItems = data ?? null;
            if (serializedItems == null)
            {
                serializedItems = api.ModLoader.GetModSystem<TradeRoutesSystem>().TradeRoutesHandler.GetTree(this.networkId);
            }
            if (serializedItems == null) return;

            Inventory.FromTreeAttributes(TreeAttribute.CreateFromBytes(serializedItems));
        }

        protected void SyncToNetworkInventory()
        {
            TreeAttribute tree = new TreeAttribute();
            Inventory.ToTreeAttributes(tree);
            Api.ModLoader.GetModSystem<TradeRoutesSystem>().TradeRoutesHandler.SyncInventories(this.blockEnityId, this.networkId, tree.ToBytes());
        }
    }
}