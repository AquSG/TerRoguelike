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
    public sealed class PendingItemPacket : TerRoguelikePacket
    {
        public override PacketType MessageType => PacketType.PendingItemSync;
        public static void Send(PendingItem item, int toClient = -1, int ignoreClient = -1)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
                return;

            var packet = NewPacket(PacketType.PendingItemSync);

            packet.Write(item.ItemType);
            packet.WriteVector2(item.Position);
            packet.Write(item.ItemTier);
            packet.Write(item.setTelegraphDuration);
            packet.Write(item.TelegraphSize);
            packet.Write(item.Personal);

            packet.Send(toClient, ignoreClient);
        }
        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            int type = packet.ReadInt32();
            Vector2 pos = packet.ReadVector2();
            int tier = packet.ReadInt32();
            int duration = packet.ReadInt32();
            float size = packet.ReadSingle();
            bool personal = packet.ReadBoolean();

            SpawnManager.pendingItems.Add(new(type, pos, tier, duration, size, personal));
            SoundEngine.PlaySound(ItemSpawn with { Volume = 0.12f, Variants = [tier], MaxInstances = 10 }, pos);
        }
    }
}
