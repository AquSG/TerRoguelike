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
using static TerRoguelike.Systems.EnemyHealthBarSystem;

namespace TerRoguelike.Packets
{
    public sealed class EnemyHealthbarPacket : TerRoguelikePacket
    {
        public override PacketType MessageType => PacketType.EnemyHealthbarSync;
        public static void Send(List<int> trackedEnemies, int toClient = -1, int ignoreClient = -1)
        {
            if (!Main.dedServ)
                return;

            var packet = NewPacket(PacketType.EnemyHealthbarSync);

            int count = trackedEnemies.Count;
            packet.Write(count);
            for (int i = 0; i < count; i++)
            {
                packet.Write(trackedEnemies[i]);
            }

            packet.Send(toClient, ignoreClient);
        }
        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            int count = packet.ReadInt32();
            List<int> trackedEnemies = [];
            for (int i = 0; i < count; i++)
            {
                int who = packet.ReadInt32();
                if (who < 0)
                    continue;
                trackedEnemies.Add(who);
            }
            NPC first = Main.npc[trackedEnemies[0]];
            string name = first.active ? first.GivenOrTypeName : " ";
            enemyHealthBar = new(trackedEnemies, name);
        }
    }
}
