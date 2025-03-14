using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerRoguelike.Rooms;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using TerRoguelike.Schematics;
using TerRoguelike.Managers;
using TerRoguelike.Systems;
using TerRoguelike.Items;
using TerRoguelike.Items.Common;
using TerRoguelike.Items.Uncommon;
using TerRoguelike.Items.Rare;
using Terraria.ModLoader.Core;
using TerRoguelike.NPCs.Enemy;
using TerRoguelike.NPCs.Enemy.Pillar;
using TerRoguelike.NPCs;
using static TerRoguelike.Schematics.SchematicManager;
using Microsoft.Xna.Framework.Graphics;
using TerRoguelike.Particles;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using ReLogic.Threading;
using System.Diagnostics;
using rail;
using System.IO;
using System.Reflection;
using TerRoguelike.World;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static TerRoguelike.NPCs.TerRoguelikeGlobalNPC;
using Terraria.ModLoader.IO;
using static TerRoguelike.Managers.ItemManager;
using Terraria.Audio;

namespace TerRoguelike.Packets
{
    public sealed class BasinPacket : TerRoguelikePacket
    {
        public override PacketType MessageType => PacketType.BasinSync;
        public static void Send(int toClient = -1, int ignoreClient = -1)
        {
            if (Main.netMode != NetmodeID.Server)
                return;

            var packet = NewPacket(PacketType.BasinSync);

            int basincount = TerRoguelikeWorld.itemBasins.Count;
            packet.Write(basincount);
            for (int i = 0; i < basincount; i++)
            {
                var basin = TerRoguelikeWorld.itemBasins[i];
                packet.WriteVector2(basin.position.ToVector2());
                packet.Write((byte)basin.tier);
                packet.Write(basin.itemDisplay);
            }

            packet.Send(toClient, ignoreClient);
        }
        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            TerRoguelikeWorld.itemBasins.Clear();
            int count = packet.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                Vector2 posv = packet.ReadVector2();
                Point pos = posv.ToPoint();
                ItemTier tier = (ItemTier)packet.ReadByte();
                int itemdisplay = packet.ReadInt32();
                ItemBasinEntity basin = new(pos, tier);
                basin.itemDisplay = itemdisplay;
                basin.GenerateItemOptions(Main.LocalPlayer);
                TerRoguelikeWorld.itemBasins.Add(basin);
            }
        }
    }
}
