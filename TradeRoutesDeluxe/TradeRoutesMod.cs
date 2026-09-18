using TradeRoutesDeluxe.Common.BlockEntities;
using TradeRoutesDeluxe.Common.Blocks;
using TradeRoutesDeluxe.Common.Items;
using Vintagestory.API.Common;

namespace TradeRoutesDeluxe {

    public class TradeRoutesMod : ModSystem {

        public override void Start(ICoreAPI api) {
            base.Start(api);

            api.RegisterItemClass("BlankParchment", typeof(BlankParchment));
            api.RegisterItemClass("WrittenParchment", typeof(WrittenParchment));

            api.RegisterBlockClass("BlockTradingPost", typeof(BlockTradingPost));
            api.RegisterBlockEntityClass("BlockEntityTradingPost", typeof(BlockEntityTradingPost));

        }
    }
}
