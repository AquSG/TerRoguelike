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
using static TerRoguelike.MainMenu.TerRoguelikeMenu;

namespace TerRoguelike.Packets
{
    public sealed class MakeNPCPuppetPacket : TerRoguelikePacket
    {
        public override PacketType MessageType => PacketType.MakeNPCPuppetSync;
        public static void Send(int npc, int npcType, Vector2 pos, int owner, int toClient = -1, int ignoreClient = -1)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
                return;

            var packet = NewPacket(PacketType.MakeNPCPuppetSync);

            packet.Write(npc);
            packet.Write(npcType);
            packet.Write(owner);
            packet.WriteVector2(pos);

            packet.Send(toClient, ignoreClient);
        }
        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            int who = packet.ReadInt32();
            int whoType = packet.ReadInt32();
            int owner = packet.ReadInt32();
            Vector2 pos = packet.ReadVector2();

            NPC npc = Main.npc[who];
            if (!npc.active || npc.type != whoType)
            {
                if (Main.dedServ)
                {
                    int myRoom = -1;
                    if (RoomSystem.RoomList != null && RoomSystem.RoomList.Count > 0)
                    {
                        for (int i = 0; i < RoomSystem.RoomList.Count; i++)
                        {
                            Room room = RoomSystem.RoomList[i];
                            if (room.GetRect().Contains(pos.ToPoint()))
                            {
                                myRoom = room.myRoom;
                            }
                        }
                    }
                    npc = SpawnManager.SpawnNPCTerRoguelike(NPC.GetSource_NaturalSpawn(), pos, whoType, myRoom, null);
                }
                else
                {
                    return;
                }
            }

            var modNPC = npc.ModNPC();
            if (modNPC == null)
                return;
            Player player = Main.player[owner];
            if (!player.active)
                return;
            var modPlayer = player.ModPlayer();
            if (modPlayer == null)
                return;
            modPlayer.MakeNPCPuppet(npc, modNPC, true);

            if (Main.dedServ)
                Send(who, whoType, pos, owner, -1, sender);
        }
    }
}
