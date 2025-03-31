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
using TerRoguelike.MainMenu;

namespace TerRoguelike.Packets
{
    public sealed class RoomPacket : TerRoguelikePacket
    {
        public override PacketType MessageType => PacketType.RoomSync;
        public static void Send(int roomID, int toClient = -1, int ignoreClient = -1)
        {
            if (TerRoguelike.singleplayer)
                return;
            Room room = RoomID[roomID];

            var packet = NewPacket(PacketType.RoomSync);

            packet.Write(roomID);
            if (TerRoguelike.mpClient)
            {
                packet.Send();
                return;
            }
            packet.Write(room.initialized);
            packet.Write(room.escapeInitialized);
            packet.Write(room.awake);
            packet.Write(room.active);
            packet.Write(room.haltSpawns);
            packet.Write(room.roomTime);
            packet.Write(room.closedTime);
            packet.Write(room.waveStartTime);
            packet.Write(room.waveCount);
            packet.Write(room.currentWave);
            packet.Write(room.waveClearGraceTime);
            packet.Write(room.bossDead);
            packet.Write(room.anyAlive);
            packet.Write(room.lastTelegraphDuration);
            packet.Write(room.wallActive);
            packet.Write(TerRoguelikeWorld.currentStage);
            packet.Write(TerRoguelikeWorld.currentLoop);
            packet.Write((byte)TerRoguelikeMenu.difficulty);

            packet.Send(toClient, ignoreClient);
        }
        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            int id = packet.ReadInt32();
            if (Main.dedServ)
            {
                Send(id, sender);
                return;
            }
            Room room = RoomID[id];
            room.initialized = packet.ReadBoolean();
            room.escapeInitialized = packet.ReadBoolean();
            room.awake = packet.ReadBoolean();
            room.active = packet.ReadBoolean();
            room.haltSpawns = packet.ReadBoolean();
            room.roomTime = packet.ReadInt32();
            room.closedTime = packet.ReadInt32();
            room.waveStartTime = packet.ReadInt32();
            room.waveCount = packet.ReadInt32();
            room.currentWave = packet.ReadInt32();
            room.waveClearGraceTime = packet.ReadInt32();
            room.bossDead = packet.ReadBoolean();
            room.anyAlive = packet.ReadBoolean();
            room.lastTelegraphDuration = packet.ReadInt32();
            room.wallActive = packet.ReadBoolean();
            TerRoguelikeWorld.currentStage = packet.ReadInt32();
            TerRoguelikeWorld.currentLoop = packet.ReadInt32();
            TerRoguelikeMenu.difficulty = (TerRoguelikeMenu.Difficulty)packet.ReadByte();
        }
    }
}
