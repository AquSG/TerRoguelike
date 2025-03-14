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

namespace TerRoguelike.Packets
{
    public sealed class PendingEnemyPacket : TerRoguelikePacket
    {
        public override PacketType MessageType => PacketType.PendingEnemySync;
        public static void Send(PendingEnemy enemy, int toClient = -1, int ignoreClient = -1)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
                return;

            var packet = NewPacket(PacketType.PendingEnemySync);

            packet.Write(enemy.NPCType);
            packet.WriteVector2(enemy.Position);
            packet.Write(enemy.RoomListID);
            packet.Write(enemy.TelegraphDuration);
            packet.Write(enemy.TelegraphSize);
            packet.Write(enemy.eliteVars.tainted);
            packet.Write(enemy.eliteVars.slugged);
            packet.Write(enemy.eliteVars.burdened);

            packet.Send(toClient, ignoreClient);
        }
        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            int type = packet.ReadInt32();
            Vector2 pos = packet.ReadVector2();
            int listID = packet.ReadInt32();
            int duration = packet.ReadInt32();
            float size = packet.ReadSingle();
            var elitevars = new EliteVars();
            elitevars.tainted = packet.ReadBoolean();
            elitevars.slugged = packet.ReadBoolean();
            elitevars.burdened = packet.ReadBoolean();

            SpawnManager.pendingEnemies.Add(new(type, pos, listID, duration, size, elitevars));
        }
    }
}
