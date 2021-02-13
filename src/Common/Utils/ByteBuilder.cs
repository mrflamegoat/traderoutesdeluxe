using System.IO;
using ProtoBuf;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;

namespace TradeRoutesDeluxe.Common.Utils
{
    public static class ByteBuilder
    {
        public static byte[] InventoryByteBuilder(IInventory incomingInventory, string blockName, string dialogTitle)
        {
            byte[] byteInventory;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(ms);
                writer.Write(blockName);
                writer.Write(dialogTitle);
                writer.Write((byte)4);

                TreeAttribute tree = new TreeAttribute();
                (incomingInventory as InventoryGeneric).ToTreeAttributes(tree);
                tree.ToBytes(writer);
                byteInventory = ms.ToArray();
            }

            return byteInventory;
        }
    }
}