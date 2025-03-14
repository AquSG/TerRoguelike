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
    public sealed class RoomUnmovingDataPacket : TerRoguelikePacket
    {
        public override PacketType MessageType => PacketType.RoomUnmovingDataSync;
        public static void Send(int toClient = -1, int ignoreClient = -1)
        {
            if (Main.netMode != NetmodeID.Server)
                return;

            var packet = NewPacket(PacketType.RoomUnmovingDataSync);
            packet.Write(TerRoguelikeWorld.IsTerRoguelikeWorld);
            packet.Write(TerRoguelikeWorld.IsDeletableOnExit);

            if (RoomSystem.RoomList == null)
            {
                packet.Send(toClient, ignoreClient);
                return;
            }

            List<byte> packageRoomLisIDs = [];
            List<int> roomPositionsList = [];
            List<int> roomDimensionsList = [];
            for (int i = 0; i < RoomSystem.RoomList.Count; i++)
            {
                Room room = RoomSystem.RoomList[i];
                packageRoomLisIDs.Add((byte)room.ID);

                roomPositionsList.Add((int)room.RoomPosition.X);
                roomPositionsList.Add((int)room.RoomPosition.Y);

                roomDimensionsList.Add((int)room.RoomDimensions.X);
                roomDimensionsList.Add((int)room.RoomDimensions.Y);
            }
            ReadOnlySpan<byte> sentRoomList = packageRoomLisIDs.ToArray();
            int length = sentRoomList.Length;
            packet.Write(length);
            packet.Write(sentRoomList);
            
            for (int i = 0; i < length; i++)
            {
                int index = i * 2;
                packet.Write(roomPositionsList[index]);
                packet.Write(roomPositionsList[index + 1]);
                packet.Write(roomDimensionsList[index]);
                packet.Write(roomDimensionsList[index + 1]);
            }

            packet.Send(toClient, ignoreClient);
        }
        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            TerRoguelikeWorld.IsTerRoguelikeWorld = packet.ReadBoolean();
            TerRoguelikeWorld.IsDeletableOnExit = packet.ReadBoolean();

            RoomSystem.RoomList = [];

            int length = packet.ReadInt32();
            byte[] recievedRoomIDs = packet.ReadBytes(length);

            for (int i = 0; i < recievedRoomIDs.Length; i++)
            {
                int roomID = (int)recievedRoomIDs[i];
                RoomSystem.RoomList.Add(RoomID[roomID]);

                Room room = RoomSystem.RoomList[i];
                room.RoomPosition.X = packet.ReadInt32();
                room.RoomPosition.Y = packet.ReadInt32();
                room.RoomDimensions.X = packet.ReadInt32();
                room.RoomDimensions.Y = packet.ReadInt32();
            }
        }
    }
}
