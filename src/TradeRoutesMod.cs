using TradeRoutesDeluxe.Common.BlockEntities;
using TradeRoutesDeluxe.Common.Blocks;
using TradeRoutesDeluxe.Common.Items;
using Vintagestory.API.Common;

[assembly: ModInfo("TradeRouteDeluxe",
    Description = "Allows you to create Trade Routes between Trading Posts for automated item delivery.",
    Website = "soon",
    Authors = new[] { "MrFlamegoat" })]
[assembly: ModDependency("game", "1.14.7")]

namespace TradeRoutesDeluxe {

    public class TradeRoutesMod : ModSystem {

        public override void Start(ICoreAPI api) {
            base.Start(api);

            api.RegisterItemClass("ItemPostParchmentUnwritten", typeof(ItemPostParchmentUnwritten));
            api.RegisterItemClass("ItemPostParchmentWritten", typeof(ItemPostParchmentWritten));

            api.RegisterBlockClass("BlockTradingPost", typeof(BlockTradingPost));
            api.RegisterBlockEntityClass("BlockEntityTradingPost", typeof(BlockEntityTradingPost));

        }
    }
}
