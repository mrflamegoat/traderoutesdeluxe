using System;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;
using TradeRoutesDeluxe.Common.Items;
using TradeRoutesDeluxe.Common.Network;
using Vintagestory.API.Server;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using TradeRoutesDeluxe.Common.Utils;

namespace TradeRoutesDeluxe.Common.BlockEntities
{

    public class BlockEntityTradingPost : BlockEntityGenericContainer
    {

        internal InventoryGeneric inventory;

        private string networkId;

        private string blockEnityId;

        public string DialogTitle
        {
            get { return Lang.Get("Trading Post"); }
        }

        public override InventoryBase Inventory
        {
            get { return inventory; }
        }

        public override string InventoryClassName
        {
            get { return inventoryClassName; }
        }

        public BlockEntityTradingPost() : base()
        {
        }

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

                // Probably not required.
                MarkDirty();
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

                    // All on the server, it'll communicate back to the client for updates.
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

                        MarkDirty();

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

                    // Probably redundant, but for whatever reason, this makes the client/server interaction seem more "smooth".
                    // Shut up. Anecdotal is good enough for me!
                    MarkDirty();

                    return true;
                }
            }
            else
            {
                if (this.networkId != null && Api.World is IServerWorldAccessor)
                {
                    // Note to self: This is exactly how they do it in SurvivalMod - but surely I can write a helper for this.
                    byte[] localInventory;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        BinaryWriter writer = new BinaryWriter(ms);
                        writer.Write("BlockEntityTradingPost");
                        writer.Write(DialogTitle);
                        writer.Write((byte)4);
                        TreeAttribute tree = new TreeAttribute();
                        this.inventory.ToTreeAttributes(tree);
                        tree.ToBytes(writer);
                        localInventory = ms.ToArray();
                    }

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
            // Theoretically, you don't need to do this - it seems MakeDirty attempts to refresh the client but I can't
            // get the client to recognize the item slot changes no matter what I do. So... double duty!
            if (packetid == (int)EnumTradingPostPackets.SyncInventory)
            {
                handleNeutralPackets(packetid, data);
                MarkDirty();
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

        private void InitInventory(Block Block)
        {
            // Ripped right out of the SurvivalMod code.
            inventory = new InventoryGeneric(quantitySlots, null, null, null);

            inventory.OnInventoryClosed += OnInvClosed;
            inventory.OnInventoryOpened += OnInvOpened;
            inventory.SlotModified += OnSlotModified;
        }

        // Pass API here because we attempt to use this before it's initialized to the blockentity.
        private void SyncFromNetworkInventory(ICoreAPI api, byte[] data = null)
        {
            byte[] serializedItems = data ?? null;
            if (serializedItems == null)
            {
                // Attempt to retrieve it manually. Good for force loading.
                serializedItems = api.ModLoader.GetModSystem<TradeRoutesSystem>().TradeRoutesHandler.GetTree(this.networkId);
            }
            if (serializedItems == null) return;

            // I originally wrote this in a long hand for loop to overwrite the slots.
            // 30+ hours into reading through code and writing tests - learned you could just do this.
            Inventory.FromTreeAttributes(TreeAttribute.CreateFromBytes(serializedItems));
        }

        protected void SyncToNetworkInventory()
        {
            List<ItemSlot> slots = new List<ItemSlot>();

            foreach (ItemSlot slot in Inventory)
            {
                slots.Add(slot);
            }

            TreeAttribute tree = new TreeAttribute();
            Inventory.SlotsToTreeAttributes(slots.ToArray(), tree);
            Api.ModLoader.GetModSystem<TradeRoutesSystem>().TradeRoutesHandler.SyncInventories(this.blockEnityId, this.networkId, tree.ToBytes());
        }

        private void OnSlotModified(int slot)
        {
            Api.World.BlockAccessor.GetChunkAtBlockPos(this.Pos)?.MarkModified();
            this.SyncToNetworkInventory();
        }

        public override void OnBlockBroken()
        {
            Api.ModLoader.GetModSystem<TradeRoutesSystem>().TradeRoutesHandler.RemoveTradingPost(this.blockEnityId, this.networkId);

            // The blockentities local storage is just to make syncing easier and cleaner. It does not
            // delete the network storage.
            Inventory.DiscardAll();

            base.OnBlockBroken();
        }
    }
}