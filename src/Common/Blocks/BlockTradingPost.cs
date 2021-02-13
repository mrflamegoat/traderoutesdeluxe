using TradeRoutesDeluxe.Common.BlockEntities;
using Vintagestory.API.Common;

namespace TradeRoutesDeluxe.Common.Blocks {

    public class BlockTradingPost : Block {

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel) {
            BlockEntityTradingPost bedc = (BlockEntityTradingPost)world.BlockAccessor.GetBlockEntity(blockSel.Position);

            return bedc?.OnPlayerRightClick(byPlayer, blockSel) ?? base.OnBlockInteractStart(world, byPlayer, blockSel);
        }
    }
}
