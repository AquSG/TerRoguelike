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
using TerRoguelike.TerPlayer;
using TerRoguelike.MainMenu;

namespace TerRoguelike.Packets
{
    public sealed class TeleportToPositionPacket : TerRoguelikePacket
    {
        public override PacketType MessageType => PacketType.TeleportToPosition;
        public enum TeleportContext
        {
            Room,
            FloorTransition,
            NewFloor,
            Sanctuary,
            TrueBrain,
            StartRun,
            Misc,
        }
        public static void Send(Vector2 position, TeleportContext context, int targetRoomID = -1, int toClient = -1, int ignoreClient = -1)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
                return;

            if (Main.dedServ && (context == TeleportContext.NewFloor || context == TeleportContext.Sanctuary || context == TeleportContext.StartRun))
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

            var packet = NewPacket(PacketType.TeleportToPosition);

            packet.WriteVector2(position);
            packet.Write((byte)context);
            packet.Write(targetRoomID);

            if (context == TeleportContext.StartRun)
            {
                packet.Write(Main.spawnTileX);
                packet.Write(Main.spawnTileY);
                packet.Write(RoomSystem.playerCount);
            }

            packet.Send(toClient, ignoreClient);

            if (context == TeleportContext.NewFloor || context == TeleportContext.StartRun)
                RoomUnmovingDataPacket.SendStartRoomTiles();
        }
        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            Vector2 pos = packet.ReadVector2();
            TeleportContext context = (TeleportContext)packet.ReadByte();
            int roomID = packet.ReadInt32();

            if (context == TeleportContext.Room)
            {
                Room room = RoomID[roomID];
                if (!room.GetRect().Intersects(Main.LocalPlayer.getRect()))
                    Main.SetCameraLerp(0.17f, 10);
            }

            foreach (Player player in Main.ActivePlayers)
            {
                var modPlayer = player.ModPlayer();
                if (modPlayer == null) continue;
                if (context != TeleportContext.Room && player.dead && modPlayer.allowedToExist)
                    player.Spawn(PlayerSpawnContext.ReviveFromDeath);
                player.Center = pos;
                player.fallStart = (int)(player.position.Y / 16f);
            }
            
            if (context == TeleportContext.FloorTransition)
            {
                RoomSystem.FloorTransitionEffects();
            }
            if (context == TeleportContext.Sanctuary)
            {
                ExtraSoundSystem.ForceStopAllExtraSounds();
                RoomSystem.FloorTransitionEffects();
                var modPlayer = Main.LocalPlayer.ModPlayer();
                if (modPlayer != null)
                {
                    modPlayer.LunarCharmLogic(Main.LocalPlayer.Center);
                }
                if (modPlayer?.escaped != true)
                {
                    SetCalm(FloorID[FloorDict["Sanctuary"]].Soundtrack.CalmTrack);
                    SetCombat(FloorID[FloorDict["Sanctuary"]].Soundtrack.CombatTrack);
                    SetMusicMode(MusicStyle.AllCalm);
                    CombatVolumeInterpolant = 0;
                    CalmVolumeInterpolant = 0;
                    CalmVolumeLevel = FloorID[FloorDict["Sanctuary"]].Soundtrack.Volume;
                    CombatVolumeLevel = FloorID[FloorDict["Sanctuary"]].Soundtrack.Volume;
                }
            }
            if (context == TeleportContext.NewFloor)
            {
                ExtraSoundSystem.ForceStopAllExtraSounds();
                Room room = RoomID[roomID];
                if (Main.LocalPlayer.ModPlayer() != null)
                    RoomSystem.NewFloorEffects(room, Main.LocalPlayer.ModPlayer());
                RoomSystem.FloorTransitionEffects();

                Floor nextFloor = FloorID[room.AssociatedFloor];
                if (!TerRoguelikeWorld.escape)
                    TerRoguelikePlayer.HealthUpIndicator(Main.LocalPlayer);
                else
                {
                    var modPlayer = Main.LocalPlayer.ModPlayer();
                    if (modPlayer != null)
                    {
                        modPlayer.escapeArrowTime = 300;
                        var newFloorStartRoom = RoomSystem.RoomList.Find(x => x.ID == nextFloor.StartRoomID);
                        modPlayer.escapeArrowTarget = newFloorStartRoom.RoomPosition16 + Vector2.UnitY * newFloorStartRoom.RoomDimensions.Y * 8f;
                    }

                    for (int n = 0; n < Main.maxNPCs; n++)
                    {
                        NPC npc = Main.npc[n];
                        if (npc == null)
                            continue;
                        if (!npc.active)
                            continue;
                        if (npc.life <= 0)
                            continue;

                        TerRoguelikeGlobalNPC modNPC = npc.ModNPC();
                        if (!modNPC.isRoomNPC)
                            continue;
                        if (modNPC.sourceRoomListID < 0)
                            continue;

                        if (modNPC.sourceRoomListID > room.myRoom)
                            npc.active = false;
                    }
                    for (int t = room.myRoom + 1; t < RoomSystem.RoomList.Count; t++)
                    {
                        Room roomToClear = RoomSystem.RoomList[t];
                        if (!roomToClear.active)
                            continue;

                        for (int p = 0; p < roomToClear.NotSpawned.Length; p++)
                        {
                            roomToClear.NotSpawned[p] = false;
                        }
                    }
                    for (int s = 0; s < SpawnManager.pendingEnemies.Count; s++)
                    {
                        var pendingEnemy = SpawnManager.pendingEnemies[s];
                        if (pendingEnemy.RoomListID > room.myRoom)
                        {
                            pendingEnemy.spent = true;
                        }
                    }
                }
                if (nextFloor.Name != "Lunar" && !TerRoguelikeWorld.escape)
                {
                    SetCalm(nextFloor.Soundtrack.CalmTrack);
                    SetCombat(nextFloor.Soundtrack.CombatTrack);
                    SetMusicMode(nextFloor.Name == "Sanctuary" ? MusicStyle.AllCalm : MusicStyle.Dynamic);
                    CombatVolumeInterpolant = 0;
                    CalmVolumeInterpolant = 0;
                    CalmVolumeLevel = nextFloor.Soundtrack.Volume;
                    CombatVolumeLevel = nextFloor.Soundtrack.Volume;
                }
            }
            if (context == TeleportContext.TrueBrain)
            {
                Room room = RoomID[roomID];
                if (Main.LocalPlayer.ModPlayer() != null)
                    RoomSystem.NewFloorEffects(room, Main.LocalPlayer.ModPlayer());
                RoomSystem.FloorTransitionEffects();
                SetBossTrack(FinalBoss2Theme);
                CombatVolumeInterpolant = 0;
                CalmVolumeInterpolant = 0;
            }
            if (context == TeleportContext.StartRun)
            {
                RoomSystem.runStartMeter = 0;
                Main.spawnTileX = packet.ReadInt32();
                Main.spawnTileY = packet.ReadInt32();
                RoomSystem.playerCount = packet.ReadInt32();
                Main.BlackFadeIn = 255;
                var modPlayer = Main.LocalPlayer.ModPlayer();
                if (modPlayer != null)
                {
                    modPlayer.playthroughTime.Restart();
                }

                Room room = RoomID[roomID];
                RoomSystem.FloorTransitionEffects();

                Floor nextFloor = FloorID[room.AssociatedFloor];

                if (nextFloor.Name != "Lunar" && !TerRoguelikeWorld.escape)
                {
                    SetCalm(nextFloor.Soundtrack.CalmTrack);
                    SetCombat(nextFloor.Soundtrack.CombatTrack);
                    SetMusicMode(nextFloor.Name == "Sanctuary" ? MusicStyle.AllCalm : MusicStyle.Dynamic);
                    CombatVolumeInterpolant = 0;
                    CalmVolumeInterpolant = 0;
                    CalmVolumeLevel = nextFloor.Soundtrack.Volume;
                    CombatVolumeLevel = nextFloor.Soundtrack.Volume;
                }

                TerRoguelikeMenu.weaponSelectInPlayerMenu = true;
                IEnumerable<Item> vanillaItems = [];
                for (int i = 0; i < 58; i++)
                    Main.LocalPlayer.inventory[i].type = Main.LocalPlayer.inventory[i].stack = 0;
                List<Item> startingItems = PlayerLoader.GetStartingItems(Main.LocalPlayer, vanillaItems);
                PlayerLoader.SetStartInventory(Main.LocalPlayer, startingItems);
                Main.LocalPlayer.trashItem = new(ItemID.None, 0);
                TerRoguelikeMenu.desiredPlayer = Main.ActivePlayerFileData;
            }
        }
    }
}
