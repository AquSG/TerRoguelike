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
        public static bool firstReceive = true;
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

            packet.Write((byte)TerRoguelikeWorld.currentStage);
            packet.Write((byte)TerRoguelikeWorld.currentLoop);
            packet.Write(RoomSystem.runStarted);
            packet.Write(Main.spawnTileX);
            packet.Write(Main.spawnTileY);

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
            packet.Write((byte)length);
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
            packet.Write((byte)floorLength);
            for (int i = 0; i < floorLength; i++)
            {
                packet.Write((byte)RoomManager.FloorIDsInPlay[i]);
            }

            List<byte> allowedPlayers = [];
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (!player.active) continue;

                var modPlayer = player.ModPlayer();
                if (modPlayer == null) continue;

                if (!RoomSystem.runStarted)
                    modPlayer.allowedToExist = true;
                if (modPlayer.allowedToExist)
                    allowedPlayers.Add((byte)i);
            }

            packet.Write((byte)allowedPlayers.Count);
            for (int i = 0; i < allowedPlayers.Count; i++)
            {
                packet.Write(allowedPlayers[i]);
            }

            packet.Write(RoomSystem.runStarted);
            packet.Write(RoomSystem.playerCount);

            packet.Send(toClient, ignoreClient);

            SendStartRoomTiles();
        }
        public static void SendStartRoomTiles()
        {
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
            TerRoguelikeWorld.IsTerRoguelikeWorld = packet.ReadBoolean();
            TerRoguelikeWorld.IsDeletableOnExit = packet.ReadBoolean();

            bool returnEarly = packet.ReadBoolean();
            if (returnEarly)
            {
                RoomSystem.RoomList = [];
                return;
            }

            bool amIAllowed = false;
            if (Main.LocalPlayer.active)
            {
                var myModPlayer = Main.LocalPlayer.ModPlayer();
                if (myModPlayer != null)
                    amIAllowed = myModPlayer.allowedToExist;
            }

            TerRoguelikeWorld.currentStage = packet.ReadByte();
            TerRoguelikeWorld.currentLoop = packet.ReadByte();
            RoomSystem.runStarted = packet.ReadBoolean();
            Main.spawnTileX = packet.ReadInt32();
            Main.spawnTileY = packet.ReadInt32();

            RoomSystem.RoomList = [];

            int length = packet.ReadByte();
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

            int floorLength = packet.ReadByte();
            RoomManager.FloorIDsInPlay = [];
            for (int i = 0; i < floorLength; i++)
            {
                RoomManager.FloorIDsInPlay.Add(packet.ReadByte());
            }

            int playerCount = packet.ReadByte();
            List<int> allowedPlayers = [];
            for (int i = 0; i < playerCount; i++)
                allowedPlayers.Add(packet.ReadByte());

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (!player.active) continue;
                var modPlayer = player.ModPlayer();
                if (modPlayer == null) continue;

                if (allowedPlayers.Count == 0)
                {
                    modPlayer.allowedToExist = false;
                    continue;
                }
                if (i != allowedPlayers[0])
                {
                    modPlayer.allowedToExist = false;
                    continue;
                }

                allowedPlayers.RemoveAt(0);
                modPlayer.allowedToExist = true;
            }

            if (Main.LocalPlayer.active)
            {
                var myModPlayer = Main.LocalPlayer.ModPlayer();
                if (myModPlayer != null)
                {
                    if (!amIAllowed && myModPlayer.allowedToExist) // just got perms to exist
                        Main.LocalPlayer.Spawn(PlayerSpawnContext.ReviveFromDeath);
                }
            }

            RoomSystem.runStarted = packet.ReadBoolean();
            RoomSystem.playerCount = packet.ReadInt32();

            if (firstReceive)
            {
                if (TerRoguelike.mpClient)
                {
                    TerRoguelikeMenu.weaponSelectInPlayerMenu = true;
                    Player player = Main.LocalPlayer;
                    for (int i = 0; i < 58; i++)
                        player.inventory[i].TurnToAir();
                    for (int i = 0; i < player.armor.Length; i++)
                        player.armor[i].TurnToAir();
                    for (int i = 0; i < player.dye.Length; i++)
                        player.dye[i].TurnToAir();
                    for (int i = 0; i < player.miscEquips.Length; i++)
                        player.miscEquips[i].TurnToAir();
                    for (int i = 0; i < player.miscDyes.Length; i++)
                        player.miscDyes[i].TurnToAir();
                    player.trashItem.TurnToAir();

                    IEnumerable<Item> vanillaItems = [];
                    List<Item> startingItems = PlayerLoader.GetStartingItems(player, vanillaItems);
                    PlayerLoader.SetStartInventory(player, startingItems);
                }
                firstReceive = false;
            }
        }
    }
}
