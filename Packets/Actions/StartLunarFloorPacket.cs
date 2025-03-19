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
using static TerRoguelike.Systems.MusicSystem;
using TerRoguelike.Floors;

namespace TerRoguelike.Packets
{
    public sealed class StartLunarFloorPacket : TerRoguelikePacket
    {
        public override PacketType MessageType => PacketType.StartLunarFloorSync;
        public static void Send(int toClient = -1, int ignoreClient = -1)
        {
            if (!Main.dedServ)
                return;

            var packet = NewPacket(PacketType.StartLunarFloorSync);

            packet.Send(toClient, ignoreClient);
        }
        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            int vortex = ModContent.NPCType<VortexPillar>();
            int stardust = ModContent.NPCType<StardustPillar>();
            int nebula = ModContent.NPCType<NebulaPillar>();
            int solar = ModContent.NPCType<SolarPillar>();
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                var npc = Main.npc[i];
                if (!npc.active) continue;

                Vector2 chainStart = (RoomID[RoomDict["LunarBossRoom1"]].RoomPosition + (RoomID[RoomDict["LunarBossRoom1"]].RoomDimensions * 0.5f)) * 16f;

                if (npc.type == vortex || npc.type == stardust || npc.type == nebula || npc.type == solar)
                {
                    TerRoguelikeWorld.chainList.Add(new Chain(chainStart, npc.Center, 24, 120, i));
                }
            }
        }
    }
}
