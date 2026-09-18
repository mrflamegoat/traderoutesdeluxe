using System;
using Vintagestory.GameContent;

namespace TradeRoutesDeluxe.Common.BlockEntities {

    public class TradingPostContainer(InventorySupplierDelegate inventorySupplier, string treeAttrKey, Func<float> perishRateSupplier) : InWorldContainer(inventorySupplier, treeAttrKey) {

        private readonly Func<float> perishRateSupplier = perishRateSupplier;

        public override float GetPerishRate() {
            return perishRateSupplier?.Invoke() ?? TradeRoutesHandler.DefaultPerishRate;
        }

        public void StopAffectingInventory() {
            if (Inventory == null) return;

            Inventory.OnAcquireTransitionSpeed -= Inventory_OnAcquireTransitionSpeed;
        }
    }
}
