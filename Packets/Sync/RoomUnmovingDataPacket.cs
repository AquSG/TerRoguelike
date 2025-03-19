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

            bool returnEarly = RoomSystem.RoomList == null;
            packet.Write(returnEarly);
            if (returnEarly)
            {
                packet.Send(toClient, ignoreClient);
                return;
            }

            List<byte> packageRoomLisIDs = [];
            List<Vector2> roomPositionsList = [];
            List<Vector2> roomDimensionsList = [];
            for (int i = 0; i < RoomSystem.RoomList.Count; i++)
            {
                Room room = RoomSystem.RoomList[i];
                packageRoomLisIDs.Add((byte)room.ID);

                roomPositionsList.Add(room.RoomPosition);
                roomDimensionsList.Add(room.RoomDimensions);
            }

            int length = packageRoomLisIDs.Count;
            packet.Write(length);
            for (int i = 0; i < length; i++)
            {
                packet.Write(packageRoomLisIDs[i]);
            }
            
            for (int i = 0; i < length; i++)
            {
                packet.WriteVector2(roomPositionsList[i]);
                packet.WriteVector2(roomDimensionsList[i]);
            }

            int floorLength = RoomManager.FloorIDsInPlay.Count;
            packet.Write(length);
            for (int i = 0; i < floorLength; i++)
            {
                packet.Write(RoomManager.FloorIDsInPlay[i]);
            }

            packet.Send(toClient, ignoreClient);

            return;
            for (int i = 0; i < RoomSystem.RoomList.Count; i++)
            {
                Room room = RoomSystem.RoomList[i];
                if ((room.IsStartRoom || room.IsSanctuary) && !room.IsBossRoom)
                {
                    Rectangle sendRect = new Rectangle((int)room.RoomPosition.X, (int)room.RoomPosition.Y, (int)room.RoomDimensions.X, (int)room.RoomDimensions.Y);
                    for (int x = sendRect.X; x < sendRect.X + sendRect.Width; x += 145)
                    {
                        for (int y = sendRect.Y; y < sendRect.Y + sendRect.Height; y += 145)
                        {
                            int width = Math.Min(145, sendRect.Width - (x - sendRect.X));
                            int height = Math.Min(145, sendRect.Height - (y - sendRect.Y));
                            NetMessage.SendTileSquare(-1, sendRect.X, sendRect.Y, width, height);
                        }
                    }
                }
            }
        }
        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            Main.NewText(packet.BaseStream.Length);
            Main.NewText(packet.BaseStream.Position);
            TerRoguelikeWorld.IsTerRoguelikeWorld = packet.ReadBoolean();
            TerRoguelikeWorld.IsDeletableOnExit = packet.ReadBoolean();

            bool returnEarly = packet.ReadBoolean();
            if (returnEarly)
            {
                Main.NewText("return early");
                RoomSystem.RoomList = [];
                return;
            }

            RoomSystem.RoomList = [];

            int length = packet.ReadInt32();
            List<int> recievedRoomIDs = [];
            for (int i = 0; i < length; i++)
            {
                recievedRoomIDs.Add(packet.ReadByte());
            }

            for (int i = 0; i < recievedRoomIDs.Count; i++)
            {
                int roomID = recievedRoomIDs[i];
                RoomSystem.RoomList.Add(RoomID[roomID]);

                Room room = RoomSystem.RoomList[i];
                room.RoomPosition = packet.ReadVector2();
                room.RoomDimensions = packet.ReadVector2();
            }

            int floorLength = packet.ReadInt32();
            RoomManager.FloorIDsInPlay.Clear();
            for (int i = 0; i < floorLength; i++)
            {
                RoomManager.FloorIDsInPlay.Add(packet.ReadInt32());
            }
            Main.NewText(packet.BaseStream.Position);
        }
    }
}
