using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace TradeRoutesDeluxe.Common.Items {

    public class WrittenParchment : Item {

        public const int NetworkIdDisplayLength = 7;

        public WrittenParchment() { }

        public override string GetHeldItemName(ItemStack itemStack) {
            string networkId = itemStack?.Attributes?.GetString("networkId");
            if (networkId == null) return base.GetHeldItemName(itemStack);

            return Lang.Get(Code.Domain + ":item-traderoute-named", ShortNetworkId(networkId));
        }

        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo) {
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

            string networkId = inSlot?.Itemstack?.Attributes?.GetString("networkId");
            if (networkId == null) return;

            dsc.AppendLine(Lang.Get(Code.Domain + ":traderoute-networkid", networkId));
        }

        public static string ShortNetworkId(string networkId) {
            if (networkId == null) return null;

            return networkId.Length <= NetworkIdDisplayLength ? networkId : networkId[..NetworkIdDisplayLength];
        }
    }
}
