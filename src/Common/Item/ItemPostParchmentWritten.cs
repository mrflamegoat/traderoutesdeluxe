using System;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace TradeRoutesDeluxe.Common.Items
{
    public class ItemPostParchmentWritten : Item
    {
        public ItemPostParchmentWritten()
        {
        }
        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

            if (inSlot.Itemstack?.Attributes?.GetString("networkId") != null)
            {
                dsc.AppendLine("GUID ID: " + inSlot.Itemstack?.Attributes?.GetString("networkId").Substring(0, 7));
            }
        }

    }
}