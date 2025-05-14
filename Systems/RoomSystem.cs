using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using TerRoguelike.Managers;
using TerRoguelike.TerPlayer;
using TerRoguelike.World;
using Terraria.ID;
using TerRoguelike.Systems;
using TerRoguelike.Projectiles;
using Terraria.ModLoader.IO;
using Microsoft.Xna.Framework;
using TerRoguelike.NPCs;
using static TerRoguelike.Schematics.SchematicManager;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.Audio;
using System.IO;
using Terraria.UI;
using TerRoguelike.MainMenu;
using TerRoguelike.NPCs.Enemy.Pillar;
using static TerRoguelike.World.TerRoguelikeWorld;
using static TerRoguelike.Systems.MusicSystem;
using Terraria.DataStructures;
using static TerRoguelike.Managers.TextureManager;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using TerRoguelike.UI;
using TerRoguelike.Tiles;
using Terraria.GameInput;
using TerRoguelike.Particles;
using TerRoguelike.NPCs.Enemy.Boss;
using Terraria.Graphics.Effects;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TerRoguelike.ILEditing;
using TerRoguelike.Items;
using TerRoguelike.Schematics;
using Terraria.Utilities.Terraria.Utilities;
using Terraria.UI.Chat;
using TerRoguelike.Utilities;
using System.Threading;
using TerRoguelike.Packets;
using Terraria.Localization;

namespace TerRoguelike.Systems
{
    public class RoomSystem : ModSystem //This file handles pretty much everything relating to updating rooms
    {
        public static List<Room> RoomList; //List of all rooms currently in play in the world
        public static List<HealingPulse> healingPulses = new List<HealingPulse>();
        public static List<AttackPlanRocketBundle> attackPlanRocketBundles = new List<AttackPlanRocketBundle>();
        public static List<RemedialHealingOrb> remedialHealingOrbs = [];
        public static bool obtainedRoomListFromServer = false;
        public static bool debugDrawNotSpawnedEnemies = false;
        public static List<StoredDraw> postDrawEverythingCache = [];
        public static bool postDrawAllBlack = false;
        public static int loopingDrama = 0;
        public static bool regeneratingWorld = false;
        public static int regeneratingWorldTime = 0;
        public static bool activatedTeleport => activatedTeleportCooldown > 0;
        public static int activatedTeleportCooldown = 0;
        public static float runStartMeter = 0;
        public static bool runStartTouched = false;
        public static bool runStarted = false;
        public static int playerCount = 1;
        internal static Mod calamityMod = null;
        internal static Mod japaneseTranslation = null;
        internal static Mod spanishTranslation = null;
        internal static Mod koreanTranslation = null;
        public static List<Mod> translationMods = [];
        public static bool translationModEnabled
        {
            get
            {
                foreach (Mod mod in translationMods)
                {
                    if (mod != null)
                        return true;
                }
                return false;
            }
        }
        public static void NewRoom(Room room)
        {
            RoomList.Add(room);
        }
        public override void PostUpdateWorld()
        {
            if (IsTerRoguelikeWorld)
            {
                if (ILEdits.dualContrastTileShader)
                {
                    Main.time = 16500;
                    Main.dayTime = false;
                    Star.starfallBoost = -1;
                }
                else
                {
                    Main.time = 34920;
                    Main.dayTime = true;
                }
                if (Main.netMode == NetmodeID.SinglePlayer)
                    runStarted = true;
            }

            postDrawAllBlack = false;

            if (escapeTime > 0 && escape)
            {
                if (quakeCooldown <= 0)
                {
                    quakeCooldown = Main.rand.Next(600, 1201);
                    quakeTime = setQuateTime;
                }
                else
                    quakeCooldown--;

                escapeTime--;
                if (escapeTime == 0 && !escaped)
                {
                    for (int i = 0; i < Main.maxPlayers; i++)
                    {
                        if (Main.player[i] == null)
                            continue;
                        if (!Main.player[i].active)
                            continue;

                        var modPlayer = Main.player[i]?.ModPlayer();
                        if (modPlayer == null)
                            continue;
                        if (modPlayer.escaped)
                            continue;

                        worldTeleportTime = 1;
                        modPlayer.escapeFail = true;

                        if (Main.player[i].dead)
                            continue;
                        Main.player[i].KillMe(PlayerDeathReason.LegacyDefault(), Main.rand.Next(10000, 25000), Main.rand.NextBool() ? -1 : 1);
                    }

                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (npc == null)
                            continue;
                        if (!npc.active)
                            continue;

                        TerRoguelikeGlobalNPC modNPC = npc.ModNPC();
                        if (modNPC == null)
                            continue;
                        if (modNPC.isRoomNPC)
                        {
                            npc.StrikeInstantKill();
                            npc.active = false;
                        }
                    }
                    for (int i = 0; i < SpawnManager.pendingEnemies.Count; i++)
                    {
                        SpawnManager.pendingEnemies[i].spent = true;
                    }
                }
            }
            
            SpawnManager.UpdateSpawnManager(); //Run all logic for all pending items and enemies being telegraphed
            UpdateHealingPulse(); //Used for uncommon healing item based on room time
            UpdateAttackPlanRocketBundles(); //Used for the attack plan item that handles future attack plan bundles
            UpdateChains();
            UpdateQuake();

            if (RoomList == null)
                return;

            if (RoomList.Count == 0 || regeneratingWorld)
                return;

            int loopCount = -1;
            bool otherPillarAwake = false;
            foreach (Room room in RoomList)
            {
                loopCount++;
                if (room == null)
                    continue;

                room.myRoom = loopCount; //updates the room's 'myRoom' to refer to it's index on RoomList
                bool tryEnter = false;
                for (int i = 0; i < Main.maxPlayers; i++) //Player collision with rooms
                {
                    Player player;
                    if (Main.netMode == NetmodeID.SinglePlayer)
                        player = Main.player[Main.myPlayer];
                    else
                        player = Main.player[i];
                    if (!player.active || player.dead)
                        continue;
                    TerRoguelikePlayer modPlayer = player.ModPlayer();
                    
                    bool roomXcheck = player.Center.X - (player.width / 2f) > (room.RoomPosition.X + 1f) * 16f - 1f && player.Center.X + (player.width / 2f) < (room.RoomPosition.X - 1f + room.RoomDimensions.X) * 16f + 1f;
                    bool roomYcheck = player.Center.Y - (player.height / 2f) > (room.RoomPosition.Y + 1f) * 16f && player.Center.Y + (player.height / 2f) < (room.RoomPosition.Y - (15f / 16f) + room.RoomDimensions.Y) * 16f;
                    if (roomXcheck && roomYcheck)
                    {
                        if (!room.entered)
                        {
                            tryEnter = true;
                        }
                        modPlayer.currentRoom = -1; //Current room is -1 unless the player is inside an active room in RoomList
                        if (room.AssociatedFloor != -1)
                            modPlayer.currentFloor = FloorID[room.AssociatedFloor]; //If player is inside a room with a valid value for an associated floor, set it to that.
                        if (room.AllowSettingPlayerCurrentRoom)
                        {
                            modPlayer.currentRoom = room.myRoom;
                        }
                        modPlayer.lastKnownRoom = room.myRoom;

                        if (modPlayer.currentFloor.ID == 10 && !lunarFloorInitialized)
                        {
                            InitializeLunarFloor();
                        }

                        if (escape && !room.IsStartRoom)
                        {
                            if (loopCount >= 2)
                            {
                                Room jumpstartRoom = RoomList[loopCount - 2];
                                if (!jumpstartRoom.initialized && !jumpstartRoom.IsBossRoom)
                                {
                                    jumpstartRoom.awake = true;
                                    jumpstartRoom.InitializeRoom();
                                    if (Main.dedServ)
                                        RoomPacket.Send(jumpstartRoom.ID);
                                }
                            }
                        }

                        bool allowAwake = true;
                        if (!TerRoguelike.singleplayer && room.IsPillarRoom && otherPillarAwake)
                            allowAwake = false;
                        if (!runStarted)
                            allowAwake = false;
                        
                        if (allowAwake)
                            room.awake = true;

                        if (!escape && room.IsPillarRoom && room.active && room.awake)
                            otherPillarAwake = true;

                        if (room.CanDescend(player, modPlayer) && !TerRoguelike.mpClient && !activatedTeleport) //New Floor Blue Wall Portal Teleport
                        {
                            activatedTeleportCooldown = 180;
                            room.Descend(player);
                            player.fallStart = (int)(player.position.Y / 16f);
                            FloorTransitionEffects();
                        }
                        if (room.CanAscend(player, modPlayer) && !TerRoguelike.mpClient && !activatedTeleport)
                        {
                            activatedTeleportCooldown = 180;
                            room.Ascend(player);
                            player.fallStart = (int)(player.position.Y / 16f);
                            FloorTransitionEffects();
                        }

                        if (room.closedTime == 1 && !TerRoguelikeMenu.RuinedMoonActive) // heal players on room clear so no waiting slog for natural life regen
                        {
                            player.statLife = player.statLifeMax2;
                        }
                    }

                    if (Main.netMode == NetmodeID.SinglePlayer) // don't loop through all players if in singleplayer lol
                        break;
                }

                room.Update();
                if (room.awake && tryEnter)
                {
                    room.entered = true;
                    room.OnEnter();
                    if (Main.dedServ)
                        RoomPacket.Send(room.ID);
                }
            }

            if (!runStarted && runStartMeter > 0)
            {
                if (!runStartTouched)
                {
                    runStartMeter -= 1f / 360f;
                    if (runStartMeter < 0)
                        runStartMeter = 0;
                }
                else
                {
                    if (runStartMeter >= 1)
                    {
                        runStarted = true;
                        playerCount = NPC.GetActivePlayerCount();
                        if (Main.dedServ)
                        {
                            runStartMeter = 0;
                            Main.spawnTileX = (Main.maxTilesX / 32) + 12;
                            Main.spawnTileY = (Main.maxTilesY / 2) + 12;
                            TeleportToPositionPacket.Send(new Point(Main.spawnTileX, Main.spawnTileY).ToWorldCoordinates(), TeleportToPositionPacket.TeleportContext.StartRun, RoomList[0].ID);
                        }
                        else
                        {
                            runStartMeter = 1;
                        }
                    }
                }
                runStartTouched = false;
            }
        }
        public override void PostUpdateTime()
        {
            if (ILEdits.dualContrastTileShader)
            {
                Star.starfallBoost = -1;
            }
        }
        #region Initialize Lunar Floor
        public static void InitializeLunarFloor()
        {
            SetMusicMode(MusicStyle.AllCalm);
            SetCalm(FinalStage);
            CalmVolumeLevel = 0.32f;
            CalmVolumeInterpolant = 0;
            CombatVolumeInterpolant = 0;

            if (lunarFloorInitialized)
                return;
            lunarFloorInitialized = true;
            if (TerRoguelike.mpClient)
                return;

            Vector2 chainStart = (RoomID[RoomDict["LunarBossRoom1"]].RoomPosition + (RoomID[RoomDict["LunarBossRoom1"]].RoomDimensions * 0.5f)) * 16f;
            int pillarCount = 0;
            for (int i = 0; i < RoomList.Count; i++)
            {
                if (pillarCount >= 4)
                    break;

                Room room = RoomList[i];
                if (room == null)
                    continue;

                if (room.ID == RoomDict["LunarPillarRoomTopLeft"])
                {
                    int spawnedNpc = NPC.NewNPC(NPC.GetSource_NaturalSpawn(), (int)((room.RoomPosition.X + (room.RoomDimensions.X * 0.5f)) * 16f), (int)((room.RoomPosition.Y + (room.RoomDimensions.Y * 0.5f)) * 16f) + 160, ModContent.NPCType<VortexPillar>());
                    Main.npc[spawnedNpc].ModNPC().isRoomNPC = true;
                    Main.npc[spawnedNpc].ModNPC().sourceRoomListID = i;
                    Main.npc[spawnedNpc].netUpdate = true;
                    chainList.Add(new Chain(chainStart, Main.npc[spawnedNpc].Center, 24, 120, spawnedNpc));
                    pillarCount++;
                    continue;
                }
                if (room.ID == RoomDict["LunarPillarRoomTopRight"])
                {
                    int spawnedNpc = NPC.NewNPC(NPC.GetSource_NaturalSpawn(), (int)((room.RoomPosition.X + (room.RoomDimensions.X * 0.5f)) * 16f), (int)((room.RoomPosition.Y + (room.RoomDimensions.Y * 0.5f)) * 16f) + 160, ModContent.NPCType<StardustPillar>());
                    Main.npc[spawnedNpc].ModNPC().isRoomNPC = true;
                    Main.npc[spawnedNpc].ModNPC().sourceRoomListID = i;
                    Main.npc[spawnedNpc].netUpdate = true;
                    chainList.Add(new Chain(chainStart, Main.npc[spawnedNpc].Center, 24, 120, spawnedNpc));
                    pillarCount++;
                    continue;
                }
                if (room.ID == RoomDict["LunarPillarRoomBottomLeft"])
                {
                    int spawnedNpc = NPC.NewNPC(NPC.GetSource_NaturalSpawn(), (int)((room.RoomPosition.X + (room.RoomDimensions.X * 0.5f)) * 16f), (int)((room.RoomPosition.Y + (room.RoomDimensions.Y * 0.5f)) * 16f) + 160, ModContent.NPCType<NebulaPillar>());
                    Main.npc[spawnedNpc].ModNPC().isRoomNPC = true;
                    Main.npc[spawnedNpc].ModNPC().sourceRoomListID = i;
                    Main.npc[spawnedNpc].netUpdate = true;
                    chainList.Add(new Chain(chainStart, Main.npc[spawnedNpc].Center, 24, 120, spawnedNpc));
                    pillarCount++;
                    continue;
                }
                if (room.ID == RoomDict["LunarPillarRoomBottomRight"])
                {
                    int spawnedNpc = NPC.NewNPC(NPC.GetSource_NaturalSpawn(), (int)((room.RoomPosition.X + (room.RoomDimensions.X * 0.5f)) * 16f), (int)((room.RoomPosition.Y + (room.RoomDimensions.Y * 0.5f)) * 16f) + 160, ModContent.NPCType<SolarPillar>());
                    Main.npc[spawnedNpc].ModNPC().isRoomNPC = true;
                    Main.npc[spawnedNpc].ModNPC().sourceRoomListID = i;
                    Main.npc[spawnedNpc].netUpdate = true;
                    chainList.Add(new Chain(chainStart, Main.npc[spawnedNpc].Center, 24, 120, spawnedNpc));
                    pillarCount++;
                    continue;
                }
            }

            StartLunarFloorPacket.Send();
        }
        #endregion

        #region Save and Load Data
        public override void SaveWorldData(TagCompound tag)
        {
            var isTerRoguelikeWorld = TerRoguelikeWorld.IsTerRoguelikeWorld;
            var isDeletableOnExit = TerRoguelikeWorld.IsDeletableOnExit;
            var isDebugWorld = TerRoguelikeWorld.IsDebugWorld;
            tag["isTerRoguelikeWorld"] = isTerRoguelikeWorld;
            tag["isDeletableOnExit"] = isDeletableOnExit;
            tag["isDebugWorld"] = isDebugWorld;

            if (itemBasins != null && itemBasins.Count > 0)
            {
                var basinPositions = new List<Point>();
                var basinTiers = new List<int>();

                foreach( var basin in itemBasins)
                {
                    basinPositions.Add(basin.position);
                    basinTiers.Add((int)basin.tier);
                }

                tag["basinPositions"] = basinPositions;
                tag["basinTiers"] = basinTiers;
            }

            if (RoomList == null)
                return;

            //Save all critical data, such as what rooms are in play, their positions, dimensions, and what floors are in play
            var roomIDs = new List<int>();
            var roomPositions = new List<Vector2>();
            var roomDimensions = new List<Vector2>();
            var floorIDsInPlay = RoomManager.FloorIDsInPlay;

            foreach (Room room in RoomList)
            {
                if (room == null)
                    continue;

                roomIDs.Add(room.ID);
                roomPositions.Add(room.RoomPosition);
                roomDimensions.Add(room.RoomDimensions);
            }
            tag["roomIDs"] = roomIDs;
            tag["roomPositions"] = roomPositions;
            tag["roomDimensions"] = roomDimensions;
            tag["floorIDsInPlay"] = floorIDsInPlay;
        }
        public override void LoadWorldData(TagCompound tag)
        {
            var isTerRoguelikeWorld = tag.GetBool("isTerRoguelikeWorld");
            var isDeletableOnExit = TerRoguelikeMenu.prepareForRoguelikeGeneration ? tag.GetBool("isDeletableOnExit") : false;
            var isDebugWorld = tag.GetBool("isDebugWorld");
            TerRoguelikeWorld.IsTerRoguelikeWorld = isTerRoguelikeWorld;
            TerRoguelikeWorld.IsDeletableOnExit = isDeletableOnExit;
            TerRoguelikeWorld.IsDebugWorld = isDebugWorld;
            TerRoguelikeWorld.currentStage = 0;
            TerRoguelikeMenu.prepareForRoguelikeGeneration = false;

            var basinPositions = tag.GetList<Point>("basinPositions");
            var basinTiers = tag.GetList<int>("basinTiers");
            if (basinPositions != null && basinPositions.Count > 0)
            {
                for (int i = 0; i < basinPositions.Count; i++)
                {
                    itemBasins.Add(new ItemBasinEntity(basinPositions[i], (ItemManager.ItemTier)basinTiers[i]));
                }
            }

            if (!TerRoguelikeWorld.IsTerRoguelikeWorld)
            {
                RoomList = new List<Room>();
                return;
            }

            if (SpawnManager.pendingEnemies != null)
                SpawnManager.pendingEnemies.Clear();
            else
                SpawnManager.pendingEnemies = [];

            if (SpawnManager.pendingItems != null)
                SpawnManager.pendingItems.Clear();
            else
                SpawnManager.pendingItems = [];

            if (SpawnManager.specialPendingItems != null)
                SpawnManager.specialPendingItems.Clear();
            else
                SpawnManager.specialPendingItems = [];

            RoomList = new List<Room>();
            int loopcount = 0;
            var roomIDs = tag.GetList<int>("roomIDs");
            var roomPositions = tag.GetList<Vector2>("roomPositions");
            var roomDimensions = tag.GetList<Vector2>("roomDimensions");
            var floorIDsInPlay = tag.GetList<int>("floorIDsInPlay");
            foreach (int id in roomIDs)
            {
                if (id == -1)
                    continue;

                RoomList.Add(RoomID[id]);
                RoomList[loopcount].RoomPosition = roomPositions[loopcount];
                RoomList[loopcount].RoomDimensions = roomDimensions[loopcount];
                ResetRoomID(id);
                loopcount++;
            }
            RoomManager.FloorIDsInPlay = [.. floorIDsInPlay];

            foreach (var floor in FloorID)
            {
                floor.Reset();
            }

            // for some reason vanilla isn't certain about where the spawn position is and some crazy camera lerp happens so I'm just gonna make that.. not happen
            Main.BlackFadeIn = 255;
            Main.SetCameraLerp(0, 0);

            if (promoteLoop)
            {
                currentLoop++;
                promoteLoop = false;
            }
            else
                currentLoop = 0;

            runStarted = false;
            RoomSystem.playerCount = 1;
        }
        #endregion

        /// <summary>
        /// Resets the given room ID to it's default values, aside from position and dimensions
        /// </summary>
        public static void ResetRoomID(int id)
        {
            Room room = RoomID[id];
            room.PreResetRoom();
            room.active = true;
            room.initialized = false;
            room.awake = false;
            room.roomTime = 0;
            room.closedTime = 0;
            room.waveCount = 0;
            room.waveStartTime = 0;
            room.currentWave = 0;
            room.waveClearGraceTime = 0;
            room.NPCSpawnPosition = new Vector2[Room.RoomSpawnCap];
            room.NPCToSpawn = new int[Room.RoomSpawnCap];
            room.TimeUntilSpawn = new int[Room.RoomSpawnCap];
            room.TelegraphDuration = new int[Room.RoomSpawnCap];
            room.TelegraphSize = new float[Room.RoomSpawnCap];
            room.NotSpawned = new bool[Room.RoomSpawnCap];
            room.AssociatedWave = new int[Room.RoomSpawnCap];
            room.anyAlive = true;
            room.roomClearGraceTime = -1;
            room.wallActive = false;
            room.haltSpawns = false;
            room.bossDead = false;
            room.entered = false;
        }
        public static void PostDrawWalls_PreNPCsBehindTiles(SpriteBatch spritebatch)
        {
            StartAlphaBlendSpritebatch(false);
            DrawChains();
            Main.spriteBatch.End();
        }
        public static void PostDrawWalls(SpriteBatch spriteBatch)
        {
            StartAlphaBlendSpritebatch(false);
            DrawRoomWalls(spriteBatch);
            ParticleManager.DrawParticles_BehindTiles();
            Main.spriteBatch.End();
        }
        public static void PostDrawEverything(SpriteBatch spritebatch)
        {
            StartVanillaSpritebatch(false);
            foreach (NPC npc in Main.ActiveNPCs)
            {
                try
                {
                    var modNPC = npc.ModNPC();
                    if (modNPC == null || !modNPC.drawAfterEverything)
                        continue;

                    Main.instance.DrawNPCDirect(Main.spriteBatch, npc, false, Main.screenPosition);
                }
                catch (Exception ex)
                {
                    TerRoguelike.Instance.Logger.Error(ex);
                }
            }
            Main.spriteBatch.End();

            ParticleManager.DrawParticles_AfterEverything();

            Rectangle cameraRect = new Rectangle((int)Main.Camera.ScaledPosition.X, (int)Main.Camera.ScaledPosition.Y, (int)Main.Camera.ScaledSize.X, (int)Main.Camera.ScaledSize.Y);
            cameraRect.Inflate((int)(cameraRect.Height * -0.06f), (int)(cameraRect.Height * -0.06f));
            StartVanillaSpritebatch(false);
            foreach (NPC npc in Main.ActiveNPCs)
            {
                try
                {
                    var modNPC = npc.ModNPC();
                    if (modNPC == null)
                        continue;

                    if (modNPC.isRoomNPC && modNPC.sourceRoomListID >= 0)
                    {
                        if (!Main.hideUI && !modNPC.hostileTurnedAlly && modNPC.overheadArrowTime > 0 && ModContent.GetInstance<TerRoguelikeConfig>().EnemyLocationArrow)
                        {
                            Texture2D arrowTex = TexDict["YellowArrow"];
                            float opacity = MathHelper.Clamp(modNPC.overheadArrowTime / 60f, 0, 1) * 0.7f + (0.3f * (float)Math.Cos(Main.GlobalTimeWrappedHourly * 3));
                            Vector2 pos = npc.Top + modNPC.drawCenter + (npc.gfxOffY) * Vector2.UnitY;
                            if (!cameraRect.Contains(pos.ToPoint()))
                            {
                                pos = cameraRect.ClosestPointInRect(pos);
                            }
                            float rot = (npc.Center - pos).ToRotation();
                            if (npc.Center == pos)
                                rot = MathHelper.PiOver2;
                            pos += (-32 + (14 * opacity)) * rot.ToRotationVector2();
                            Main.EntitySpriteDraw(arrowTex, pos - Main.screenPosition, null, Color.White * opacity * 0.9f, rot, arrowTex.Size() * 0.5f, 0.5f, SpriteEffects.None);
                        }
                    }
                }
                catch (Exception e)
                {
                    TerRoguelike.Instance.Logger.Error(e);
                }
            }
            Main.spriteBatch.End();

            Player player = Main.LocalPlayer;
            if (player != null)
            {
                var modPlayer = player.ModPlayer();
                if (modPlayer != null && modPlayer.escapeFail)
                {
                    StartAlphaBlendSpritebatch(false);
                    Main.EntitySpriteDraw(TextureAssets.MagicPixel.Value, Main.Camera.ScaledPosition - Main.screenPosition, null, Color.Black, 0, Vector2.Zero, new Vector2(Main.screenWidth, Main.screenHeight * 0.0011f) / ZoomSystem.ScaleVector * 1.1f, SpriteEffects.None);
                    Main.spriteBatch.End();
                }
            }
            if (worldTeleportTime > 0)
            {
                StartAlphaBlendSpritebatch(false);
                float worldTeleportOpacity = 1f;
                int fadeIn = 0;
                int fadeOut = 50;
                int totalTime = 60;
                if (worldTeleportTime < fadeIn)
                {
                    worldTeleportOpacity *= worldTeleportTime / (float)fadeIn;
                }
                if (totalTime - worldTeleportTime < fadeOut)
                {
                    worldTeleportOpacity *= ((totalTime - worldTeleportTime) / (float)fadeOut);
                }

                Main.EntitySpriteDraw(TextureAssets.MagicPixel.Value, Main.Camera.ScaledPosition - Main.screenPosition, null, Color.LightGray * worldTeleportOpacity, 0, Vector2.Zero, new Vector2(Main.screenWidth, Main.screenHeight * 0.0011f) / ZoomSystem.ScaleVector * 1.1f, SpriteEffects.None);

                Main.spriteBatch.End();
            }
            if (loopingDrama > 0)
            {
                StartAlphaBlendSpritebatch(false);
                float worldTeleportOpacity = 1f;
                int fadeIn = 90;
                int fadeOut = -1;
                int totalTime = 120;
                if (loopingDrama < fadeIn)
                {
                    worldTeleportOpacity *= loopingDrama / (float)fadeIn;
                }
                if (totalTime - loopingDrama < fadeOut)
                {
                    worldTeleportOpacity *= ((totalTime - loopingDrama) / (float)fadeOut);
                }

                Main.EntitySpriteDraw(TextureAssets.MagicPixel.Value, Main.Camera.ScaledPosition - Main.screenPosition, null, Color.White * worldTeleportOpacity, 0, Vector2.Zero, new Vector2(Main.screenWidth, Main.screenHeight * 0.0011f) / ZoomSystem.ScaleVector * 1.1f, SpriteEffects.None);

                Main.spriteBatch.End();
            }

            if (lunarGambitSceneTime > 0)
            {
                Vector2 drawPos = lunarGambitSceneDisplayPos;

                if (drawPos.Distance(Main.Camera.Center) < 2500)
                {
                    drawPos -= Main.screenPosition;
                    if (lunarGambitSceneTime > lunarGambitStartDuration + lunarGambitFloatOverDuration)
                    {
                        float portalScaleInterpolant = lunarGambitSceneScaleInterpolant;

                        StartAdditiveSpritebatch(false);

                        var glowTex = TexDict["CircularGlow"];
                        Main.EntitySpriteDraw(glowTex, drawPos, null, Color.LightCyan * 0.4f, 0, glowTex.Size() * 0.5f, 1.5f * portalScaleInterpolant, SpriteEffects.None);
                        Main.spriteBatch.End();

                        Effect portalEffect = Filters.Scene["TerRoguelike:SpecialPortal"].GetShader().Shader;
                        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, portalEffect, Main.GameViewMatrix.TransformationMatrix);

                        portalEffect.Parameters["noiseScale"].SetValue(0.75f);
                        portalEffect.Parameters["uvOff"].SetValue(new Vector2((float)Math.Sin(Main.GlobalTimeWrappedHourly * MathHelper.PiOver4 * 0.25f) * 1f, Main.GlobalTimeWrappedHourly * 0.5f));
                        portalEffect.Parameters["outerRing"].SetValue(MathHelper.Lerp(0.5f, 0.85f, portalScaleInterpolant));
                        portalEffect.Parameters["innerRing"].SetValue(MathHelper.Lerp(0.5f, 0.8f, portalScaleInterpolant));
                        portalEffect.Parameters["invisThreshold"].SetValue(0.35f);
                        portalEffect.Parameters["edgeBlend"].SetValue(0.08f);
                        portalEffect.Parameters["tint"].SetValue((Color.Lerp(Color.White, Color.Cyan, portalScaleInterpolant) * 0.8f).ToVector4());
                        portalEffect.Parameters["edgeTint"].SetValue((Color.White).ToVector4());
                        portalEffect.Parameters["finalFadeExponent"].SetValue(0.5f);
                        portalEffect.Parameters["edgeThresholdMulti"].SetValue(1.12f);
                        portalEffect.Parameters["centerThresholdMulti"].SetValue(0.001f);
                        portalEffect.Parameters["centerThresholdExponent"].SetValue(1.4f);

                        var tex = TexDict["BlobbyNoiseSmall"];

                        Main.EntitySpriteDraw(tex, drawPos, null, Color.White, 0, tex.Size() * 0.5f, new Vector2(0.5f, 0.5f) * portalScaleInterpolant, SpriteEffects.None);

                        Main.spriteBatch.End();
                    }
                    if (lunarGambitSceneTime <= lunarGambitStartDuration + lunarGambitFloatOverDuration)
                    {
                        LunarGambit item = new();
                        StartAlphaBlendSpritebatch(false);
                        item.DrawLunarGambit(drawPos, 1f, 0);
                        Main.spriteBatch.End();
                    }
                }
            }

            if (!(postDrawEverythingCache == null || postDrawEverythingCache.Count == 0))
            {
                Vector2 offset = -Main.screenPosition;
                if (postDrawAllBlack)
                {
                    for (int i = 0; i < Main.combatText.Length; i++)
                    {
                        Main.combatText[i].active = false;
                    }
                    StartAlphaBlendSpritebatch(false);
                    Main.EntitySpriteDraw(TextureAssets.MagicPixel.Value, Main.Camera.ScaledPosition - Main.screenPosition, null, Color.Lerp(Color.LightGray, Color.White, 0.8f), 0, Vector2.Zero, new Vector2(Main.screenWidth, Main.screenHeight * 0.0011f) / ZoomSystem.ScaleVector * 1.1f, SpriteEffects.None);
                    StartAlphaBlendSpritebatch();

                    Vector3 colorHSL = Main.rgbToHsl(Color.Black);
                    GameShaders.Misc["TerRoguelike:BasicTint"].UseOpacity(1f);
                    GameShaders.Misc["TerRoguelike:BasicTint"].UseColor(Main.hslToRgb(1 - colorHSL.X, colorHSL.Y, colorHSL.Z));
                    GameShaders.Misc["TerRoguelike:BasicTint"].Apply();
                }
                else
                    StartAlphaBlendSpritebatch(false);
                foreach (var draw in postDrawEverythingCache)
                {
                    draw.Draw(offset);
                }
                postDrawEverythingCache.Clear();
                Main.spriteBatch.End();
            }

            if (regeneratingWorld)
            {
                StartAlphaBlendSpritebatch(false);

                Main.EntitySpriteDraw(TextureAssets.MagicPixel.Value, Main.Camera.ScaledPosition - Main.screenPosition, null, Color.Black, 0, Vector2.Zero, new Vector2(Main.screenWidth, Main.screenHeight * 0.0011f) / ZoomSystem.ScaleVector * 1.1f, SpriteEffects.None);

                float thisZoom = 1f / ZoomSystem.zoomOverride;
                var font = FontAssets.DeathText.Value;
                string text = Language.GetOrRegister("Mods.TerRoguelike.Loading").Value;
                ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, text, Main.Camera.Center - Main.screenPosition, Color.White, 0, font.MeasureString(text) * 0.5f, new Vector2(thisZoom));

                float baseRot = RoomSystem.regeneratingWorldTime * 0.15f;
                Vector2 basePos = Main.Camera.Center - Main.screenPosition + new Vector2(0, 48) * thisZoom;
                int count = 80;
                var tex = TexDict["Circle"];
                for (int i = 0; i < count; i++)
                {
                    float completion = i / (float)count;
                    float opacity = (1 - completion) * 0.6f;
                    opacity -= 0.2f;
                    if (i == 0)
                        opacity = 1;
                    float rot = completion * -MathHelper.TwoPi + baseRot;
                    Vector2 pos = basePos + rot.ToRotationVector2() * 16 * thisZoom;
                    Main.EntitySpriteDraw(tex, pos, null, Color.White * opacity, 0, tex.Size() * 0.5f, 0.018f * thisZoom, SpriteEffects.None);
                }

                Main.spriteBatch.End();
            }
            

        }
        public static void DrawRoomWalls(SpriteBatch spriteBatch)
        {
            if (RoomList == null)
                return;

            spriteBatch.End();
            Texture2D lightTexture = TexDict["TemporaryBlock"];
            foreach (Room room in RoomList)
            {
                if (room == null)
                    continue;

                if (!room.StartCondition())
                    continue;

                if (room.closedTime > 60)
                    continue;

                if (room.wallActive && room.AllowWallDrawing)
                {
                    //Draw the pink borders indicating the bounds of the room
                    StartAdditiveSpritebatch(false);
                    int maxXDimensions = (int)room.RoomDimensions.X + room.WallInflateModifier.X * 2;
                    int maxYDimensions = (int)room.RoomDimensions.Y + room.WallInflateModifier.Y * 2;
                    for (float side = 0; side < 2; side++)
                    {
                        for (float i = -room.WallInflateModifier.X; i < room.RoomDimensions.X + room.WallInflateModifier.X; i++)
                        {
                            Vector2 targetBlock = room.RoomPosition + new Vector2(i, side * maxYDimensions - side);

                            if (Main.tile[targetBlock.ToPoint()].IsTileSolidGround(true))
                                continue;

                            Color color = Color.White;

                            if (room.active)
                                color.A = (byte)MathHelper.Clamp(MathHelper.Lerp(0, 255f, room.roomTime / 60f), 0, 255f);
                            else
                                color.A = (byte)MathHelper.Lerp(255, 0, room.closedTime / 60f);

                            Vector2 drawPosition = targetBlock * 16f - Main.screenPosition - new Vector2(-16f * side, -16f * side);
                            float rotation = MathHelper.Pi + (MathHelper.Pi * side);

                            Main.EntitySpriteDraw(lightTexture, drawPosition, null, color, rotation, lightTexture.Size(), 1f, SpriteEffects.None);
                        }
                        for (float i = -room.WallInflateModifier.Y; i < room.RoomDimensions.Y + room.WallInflateModifier.Y; i++)
                        {
                            Vector2 targetBlock = room.RoomPosition + new Vector2(side * maxXDimensions - side - room.WallInflateModifier.X, i);
                            if (Main.tile[targetBlock.ToPoint()].IsTileSolidGround(true))
                                continue;

                            Color color = Color.White;

                            if (room.active)
                                color.A = (byte)MathHelper.Clamp(MathHelper.Lerp(0, 255f, room.roomTime / 60f), 0, 255f);
                            else
                                color.A = (byte)MathHelper.Lerp(255, 0, room.closedTime / 60f);

                            Vector2 drawPosition = targetBlock * 16f - Main.screenPosition - new Vector2(-16f * side, (16f) * (side - 1f));
                            float rotation = MathHelper.PiOver2 + (MathHelper.Pi * side);

                            Main.EntitySpriteDraw(lightTexture, drawPosition, null, color, rotation, lightTexture.Size(), 1f, SpriteEffects.None);
                        }
                    }
                    spriteBatch.End();
                }
            }
            StartAlphaBlendSpritebatch(false);
        }
        public override void PostDrawTiles()
        {
            Player player = Main.player[Main.myPlayer];
            if (IsTerRoguelikeWorld && player.dead)
                player.respawnTimer = 65;

            DrawDeathScene();
            DrawPendingEnemies();

            ILEdits.fakeHoverItem = 0;
            if (itemBasins != null)
            {
                StartVanillaSpritebatch(false);
                for (int b = 0; b < TerRoguelikeWorld.itemBasins.Count; b++)
                {
                    var basin = TerRoguelikeWorld.itemBasins[b];
                    if (basin.nearby <= 0 || basin.itemDisplay == 0)
                    {
                        continue;
                    }

                    Vector2 drawPos = (basin.position.ToVector2() + new Vector2(1, 0)).ToWorldCoordinates(8, 0) + new Vector2(0, -32);
                    Vector2 itemDisplayDimensions = new Vector2(48, 48);
                    var hoverRect = new Rectangle((int)(drawPos.X - (itemDisplayDimensions.X * 0.5f)), (int)(drawPos.Y - (itemDisplayDimensions.Y * 0.5f)), (int)itemDisplayDimensions.X, (int)(itemDisplayDimensions.Y * 1.83f));
                    if (hoverRect.Contains(MouseWorldAfterZoom.ToPoint()))
                    {
                        ILEdits.fakeHoverItem = basin.itemDisplay;
                    }

                    float period = (Main.GlobalTimeWrappedHourly + basin.position.X + basin.position.Y);
                    drawPos.Y += (float)Math.Cos(period * 0.5f) * 4;
                    drawPos -= new Vector2(Main.offScreenRange);

                    Item item = new Item(basin.itemDisplay);
                    Texture2D itemTex;
                    float scale;
                    Rectangle rect;
                    Main.GetItemDrawFrame(item.type, out itemTex, out rect);
                    if (itemTex.Width < itemTex.Height)
                    {
                        scale = 1f / (rect.Height / itemDisplayDimensions.Y);
                    }
                    else
                    {
                        scale = 1f / (itemTex.Width / itemDisplayDimensions.X);
                    }
                    if (scale > 1f)
                        scale = 1f;

                    float opacity = 0.7f + (float)Math.Cos(period * 2) * 0.15f;
                    Color color = Color.White * opacity;
                    Main.EntitySpriteDraw(itemTex, drawPos - Main.screenPosition + new Vector2(Main.offScreenRange), rect, color, 0f, rect.Size() * 0.5f, scale, SpriteEffects.None, 0);
                }
                Main.spriteBatch.End();
            }

            if (escape && jstcPortalTime != 0)
            {
                float portalScaleInterpolant = MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp(Math.Abs(jstcPortalTime) / 60f, 0, 1));
                bool portalRot = jstcPortalScale.X > jstcPortalScale.Y;

                Effect portalEffect = Filters.Scene["TerRoguelike:SpecialPortal"].GetShader().Shader;

                var tex = TexDict["BlobbyNoiseSmall"];
                Vector2 finalScale = new Vector2(1f / tex.Height) * jstcPortalScale * 1.5f;
                if (portalRot)
                    finalScale = new Vector2(finalScale.Y, finalScale.X);

                for (int i = 0; i < 3; i++)
                {
                    float completion = i / 2f;
                    float inVcompletion = 1 - completion;
                    Vector2 loopOff = new Vector2(48 * completion, 0).RotatedBy(jstcPortalRot);
                    float loopScale = 1 - (i / 6f);

                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, portalEffect, Main.GameViewMatrix.TransformationMatrix);

                    float noiseScaleModif = MathHelper.Lerp(1, 0.5f, completion);
                    portalEffect.Parameters["noiseScale"].SetValue(0.75f * noiseScaleModif);
                    portalEffect.Parameters["uvOff"].SetValue(new Vector2((float)Math.Sin(Main.GlobalTimeWrappedHourly * MathHelper.PiOver4 * 0.25f) * 1f, Main.GlobalTimeWrappedHourly * 0.5f) * noiseScaleModif);
                    portalEffect.Parameters["outerRing"].SetValue(MathHelper.Lerp(0.5f, MathHelper.Lerp(0.8f, 0.65f, completion), portalScaleInterpolant));
                    portalEffect.Parameters["innerRing"].SetValue(0);
                    portalEffect.Parameters["invisThreshold"].SetValue(0.35f);
                    portalEffect.Parameters["edgeBlend"].SetValue(0.08f);
                    portalEffect.Parameters["tint"].SetValue((Color.Lerp(Color.White, Color.Yellow, portalScaleInterpolant) * 0.8f * MathHelper.Lerp(1, 0.6f, completion)).ToVector4());
                    portalEffect.Parameters["edgeTint"].SetValue((Color.White * MathHelper.Lerp(1, 0.7f, completion)).ToVector4());
                    portalEffect.Parameters["finalFadeExponent"].SetValue(0.5f);
                    portalEffect.Parameters["edgeThresholdMulti"].SetValue(0);
                    portalEffect.Parameters["centerThresholdMulti"].SetValue(0.001f);
                    portalEffect.Parameters["centerThresholdExponent"].SetValue(1.4f);

                    loopOff *= finalScale.Y * 1.25f;
                    if (i != 0)
                    {
                        loopOff += loopOff.SafeNormalize(Vector2.UnitY) * (float)Math.Cos(Main.GlobalTimeWrappedHourly * MathHelper.Pi) * 1.4f * i;
                    }
                    Main.EntitySpriteDraw(tex, jstcPortalPos - Main.screenPosition + loopOff, null, Color.White, portalRot ? MathHelper.PiOver2 : 0, tex.Size() * 0.5f, finalScale * portalScaleInterpolant * loopScale, SpriteEffects.None);

                    Main.spriteBatch.End();
                }

            }

            DrawSpecialPendingItems();
            DrawHealingPulse();

            ParticleManager.DrawParticles_Default();

            if (RoomList == null)
                return;

            Rectangle screenRect = new Rectangle((int)Main.screenPosition.X, (int)Main.screenPosition.Y, Main.screenWidth, Main.screenHeight);
            StartAlphaBlendSpritebatch(false);
            for (int i = 0; i < RoomList.Count; i++)
            {
                Room room = RoomList[i];
                if (room.GetRect().Intersects(screenRect))
                    room.PostDrawTilesRoom();
            }
            Main.spriteBatch.End();
        }
        #region New Floor Effects
        public static void NewFloorEffects(Room targetRoom, TerRoguelikePlayer modPlayer)
        {
            if (!targetRoom.ActivateNewFloorEffects)
                return;

            if (modPlayer.giftBox > 0)
            {
                modPlayer.GiftBoxLogic((targetRoom.RoomPosition + (targetRoom.RoomDimensions / 2f)) * 16f);
            }
            if (modPlayer.portableGenerator > 0)
            {
                modPlayer.portableGeneratorImmuneTime += 540 + (600 * modPlayer.portableGenerator);
                SoundEngine.PlaySound(SoundID.Item125 with { Volume = 0.5f });
            }
        }
        public static void FloorTransitionEffects()
        {
            worldTeleportTime = 1;
            SoundEngine.PlaySound(WorldTeleport with { Volume = 0.3f, Variants = [1] });

            if (attackPlanRocketBundles != null)
                attackPlanRocketBundles.Clear();
            Room.ClearSpecificProjectiles();

            var modPlayer = Main.LocalPlayer.ModPlayer();
            if (modPlayer != null)
            {
                modPlayer.soulOfLenaUses = 0;
                modPlayer.lenaVisualPosition = Vector2.Zero;
                modPlayer.droneBuddyVisualPosition = Vector2.Zero;
            }
        }
        #endregion

        #region Death Scene
        public void DrawDeathScene()
        {
            if (!Main.dedServ)
            {
                Player player = Main.player[Main.myPlayer];
                TerRoguelikePlayer modPlayer = player.GetModPlayer<TerRoguelikePlayer>();
                if (modPlayer.deathEffectTimer > 0)
                {
                    modPlayer.DoDeathEffect();
                }
                if (player.dead)
                {
                    modPlayer.deadTime++;
                }
            }
        }
        #endregion

        #region Pending Enemies
        /// <summary>
        /// Draw the first frame of each pending enemy's animation as an attempt to telegraph what is spawning there
        /// </summary>
        public void DrawPendingEnemies()
        {
            if (SpawnManager.pendingEnemies == null)
                return;
            if (SpawnManager.pendingEnemies.Count == 0)
                return;

            Color color = Color.HotPink;
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            Vector3 colorHSL = Main.rgbToHsl(color);
            GameShaders.Misc["TerRoguelike:BasicTint"].UseOpacity(0.4f);
            GameShaders.Misc["TerRoguelike:BasicTint"].UseColor(Main.hslToRgb(1 - colorHSL.X, colorHSL.Y, colorHSL.Z));
            GameShaders.Misc["TerRoguelike:BasicTint"].Apply();

            for (int i = 0; i < SpawnManager.pendingEnemies.Count; i++)
            {
                PendingEnemy enemy = SpawnManager.pendingEnemies[i];

                Color newColor = SpawnManager.GetEliteColor(enemy.eliteVars);
                if (newColor != color)
                {
                    color = newColor;

                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                    colorHSL = Main.rgbToHsl(color);
                    GameShaders.Misc["TerRoguelike:BasicTint"].UseOpacity(0.4f);
                    GameShaders.Misc["TerRoguelike:BasicTint"].UseColor(Main.hslToRgb(1 - colorHSL.X, colorHSL.Y, colorHSL.Z));
                    GameShaders.Misc["TerRoguelike:BasicTint"].Apply();
                }

                Texture2D texture = enemy.dummyTex;
                int frameCount = Main.npcFrameCount[enemy.NPCType];
                int height = (int)(texture.Height / frameCount);
                float completion = enemy.TelegraphDuration / (float)enemy.MaxTelegraphDuration;
                int cutoff = (int)(completion * height);
                Main.EntitySpriteDraw(texture, enemy.Position + new Vector2(0, cutoff) - Main.screenPosition, new Rectangle(0, cutoff, texture.Width, height - cutoff), color, 0f, new Vector2(texture.Width / 2f, texture.Height / frameCount / 2f), 1f, SpriteEffects.None);
            }

            Main.spriteBatch.End();
        }
        #endregion

        #region Special Pending Items
        public static void DrawSpecialPendingItems()
        {
            if (SpawnManager.specialPendingItems == null || SpawnManager.specialPendingItems.Count == 0)
                return;

            StartAlphaBlendSpritebatch(false);

            for (int i = 0; i < SpawnManager.specialPendingItems.Count; i++)
            {
                SpawnManager.specialPendingItems[i].DrawPreDunkInSoup();
            }

            Main.spriteBatch.End();
        }
        #endregion

        #region Healing Pulses
        public void UpdateHealingPulse()
        {
            if (healingPulses == null)
                healingPulses = new List<HealingPulse>();

            if (healingPulses.Count == 0)
                return;

            for (int p = 0; p < healingPulses.Count; p++)
            {
                HealingPulse pulse = healingPulses[p];
                if (pulse.Time == 30)
                {
                    SoundEngine.PlaySound(SoundID.DD2_BetsyWindAttack with { Volume = 0.3f });
                }
                if (pulse.Time == 0)
                {
                    for (int i = 0; i < Main.maxPlayers; i++)
                    {
                        Player player = Main.player[i];
                        if (player == null)
                            continue;
                        if (!player.active || player.dead)
                            continue;

                        TerRoguelikePlayer modPlayer = player.GetModPlayer<TerRoguelikePlayer>();
                        modPlayer.ScaleableHeal((int)(player.statLifeMax2 * 0.3f));
                        SoundEngine.PlaySound(SoundID.DD2_DarkMageHealImpact with { Volume = 1f });
                    }
                }
                pulse.Time--;
                healingPulses.RemoveAll(x => x.Time <= -30);
            }
        }
        public void DrawHealingPulse()
        {
            if (healingPulses == null)
                return;
            if (healingPulses.Count == 0)
                return;

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            for (int p = 0; p < healingPulses.Count; p++)
            {
                HealingPulse pulse = healingPulses[p];
                Texture2D telegraphBase = TexDict["InvisibleProj"];

                float scale;
                float opacity;
                if (pulse.Time > 2)
                {
                    float interpolant = (pulse.Time - 2) / 28f;
                    scale = MathHelper.Lerp(0.01f, 2f, interpolant);
                    opacity = MathHelper.Lerp(0.5f, 0f, interpolant);
                }
                else
                {
                    float interpolant = Math.Abs(pulse.Time - 2) / 32f;
                    scale = MathHelper.Lerp(0.01f, 20f, interpolant);
                    opacity = MathHelper.Lerp(1f, 0f, interpolant);
                }
                    
                
                GameShaders.Misc["TerRoguelike:CircularGradientWithEdge"].UseOpacity(0.5f * opacity);
                GameShaders.Misc["TerRoguelike:CircularGradientWithEdge"].UseColor(Color.Lerp(Color.Green, Color.GreenYellow, 0.5f));
                GameShaders.Misc["TerRoguelike:CircularGradientWithEdge"].UseSecondaryColor(Color.LightGreen);
                GameShaders.Misc["TerRoguelike:CircularGradientWithEdge"].UseSaturation(scale);

                GameShaders.Misc["TerRoguelike:CircularGradientWithEdge"].Apply();

                Vector2 drawPosition = pulse.Position - Main.screenPosition;
                Main.EntitySpriteDraw(telegraphBase, drawPosition, null, Color.White, 0, telegraphBase.Size() / 2f, scale * 156f, 0, 0);
            }
            Main.spriteBatch.End();
        }
        #endregion

        #region Remedial Healing Orbs
        public static void UpdateRemedialHealingOrbs()
        {
            int cap = 2000;
            if (remedialHealingOrbs.Count > cap)
                remedialHealingOrbs.RemoveRange(0, remedialHealingOrbs.Count - cap);

            bool updatesLeft = false;
            for (int u = 0; u < 1000; u++)
            {
                updatesLeft = false;
                for (int i = 0; i < remedialHealingOrbs.Count; i++)
                {
                    var orb = remedialHealingOrbs[i];
                    if (!orb.active)
                    {
                        remedialHealingOrbs.RemoveAt(i);
                        i--;
                        continue;
                    }

                    if (orb.currentUpdate < orb.maxUpdates)
                    {
                        orb.Update();
                        updatesLeft = true;
                    }

                    if (!orb.active)
                    {
                        remedialHealingOrbs.RemoveAt(i);
                        i--;
                        continue;
                    }
                }

                if (!updatesLeft || u == 999)
                {
                    for (int i = 0; i < remedialHealingOrbs.Count; i++)
                    {
                        var orb = remedialHealingOrbs[i];
                        orb.currentUpdate = 0;
                    }
                    break;
                }
            }
        }
        public static void DrawRemedialHealingOrbs()
        {
            if (remedialHealingOrbs == null)
                return;
            if (remedialHealingOrbs.Count == 0)
                return;

            StartVanillaSpritebatch(false);

            for (int i = 0; i < remedialHealingOrbs.Count; i++)
            {
                var orb = remedialHealingOrbs[i];
                if (!orb.active)
                    continue;

                orb.Draw();
            }

            Main.spriteBatch.End();
        }
        public class RemedialHealingOrb
        {
            public Texture2D texture => Main.dedServ ? null : TextureAssets.Projectile[ModContent.ProjectileType<Projectiles.RemedialHealingOrb>()].Value;
            public bool active = true;
            public int timeLeft;
            public int timeAlive = 0;
            public Vector2 position;
            public Vector2 velocity;
            public int width = 10;
            public int height = 10;
            public int owner;
            public int maxUpdates;
            public int currentUpdate = 0;
            public List<Vector2> oldPos = [];
            public int oldPosCap = 5;
            public Rectangle hitbox => new Rectangle((int)(position.X - (width * 0.5f)), (int)(position.Y - (height * 0.5f)), width, height);
            public RemedialHealingOrb(Vector2 Position, Vector2 Velocity, int TimeLeft, int Owner, int MaxUpdates = 1, bool netSend = true)
            {
                position = Position;
                velocity = Velocity;
                maxUpdates = MaxUpdates;
                timeLeft = TimeLeft * maxUpdates;
                owner = Owner;
                if (netSend)
                    RemedialOrbPacket.Send(this);
            }
            public void Update()
            {
                currentUpdate++;

                AI();
                
                position += velocity;
                oldPos.Insert(0, position);
                if (oldPos.Count > oldPosCap)
                    oldPos.RemoveAt(oldPosCap - 1);

                timeAlive++;
                timeLeft--;
                if (timeLeft <= 0)
                    active = false;
                    
            }
            public void AI()
            {
                velocity *= 0.885f;

                if (currentUpdate != maxUpdates)
                    return;

                var ownerModPlayer = Main.player[owner].ModPlayer();
                if (ownerModPlayer == null)
                    return;

                Rectangle rect = hitbox;
                foreach (Player player in Main.ActivePlayers)
                {
                    if (player.dead)
                        continue;

                    if (player.getRect().Intersects(rect))
                    {
                        TerRoguelikePlayer modPlayer = player.GetModPlayer<TerRoguelikePlayer>();
                        int healAmt = ownerModPlayer.remedialTapeworm * 1; // heal for how much tapeworm the original spawner player had
                        modPlayer.ScaleableHeal(healAmt); // however, scales based off of the touchee's healing effectiveness
                        timeLeft = 0;
                        active = false;
                        return;
                    }
                }
            }
            public void Draw()
            {
                Texture2D tex = texture;
                for (int i = 0; i < oldPos.Count; i++)
                {
                    float colorInterpolation = (float)Math.Cos(timeLeft / 32f + Main.GlobalTimeWrappedHourly / 20f + i / (float)oldPos.Count * MathHelper.Pi) * 0.5f + 0.5f;
                    Color color = Color.Lerp(Color.LightSeaGreen, Color.LimeGreen, colorInterpolation) * (i <= 1 ? 1f - (i * 0.05f) : 0.7f);
                    color.A = 0;
                    Vector2 drawPosition = oldPos[i] - Main.screenPosition;
                    Color outerColor = color;
                    Color innerColor = color * 0.5f;
                    float intensity = 0.8f + 0.15f * (float)Math.Cos(timeLeft * MathHelper.TwoPi / maxUpdates * 0.04f);
                    intensity *= i <= 1 ? 1f : MathHelper.Lerp(0.15f, 0.6f, 1f - i / (float)oldPos.Count);
                    if (timeLeft <= 60 * maxUpdates) //Shrinks to nothing when projectile is nearing death
                    {
                        intensity *= timeLeft / (60f * maxUpdates);
                    }
                    if (timeAlive < 30 * maxUpdates)
                    {
                        intensity *= MathHelper.Lerp(0.5f, 1f, timeAlive / (30f * maxUpdates));
                    }

                    Vector2 outerScale = new Vector2(1f) * intensity;
                    Vector2 innerScale = new Vector2(1f) * intensity * 0.7f;
                    outerColor *= intensity;
                    innerColor *= intensity;
                    Main.EntitySpriteDraw(tex, drawPosition, null, outerColor, 0f, tex.Size() * 0.5f, outerScale * 0.15f, SpriteEffects.None, 0);
                    Main.EntitySpriteDraw(tex, drawPosition, null, innerColor, 0f, tex.Size() * 0.5f, innerScale * 0.15f, SpriteEffects.None, 0);
                }
            }
        }
        #endregion

        #region Chains
        public void UpdateChains()
        {
            if (chainList == null)
                return;
            if (chainList.Count == 0)
                return;

            int vortex = ModContent.NPCType<VortexPillar>();
            int nebula = ModContent.NPCType<NebulaPillar>();
            int stardust = ModContent.NPCType<StardustPillar>();
            int solar = ModContent.NPCType<SolarPillar>();

            for (int i = 0; i < chainList.Count; i++)
            {
                Chain chain = chainList[i];
                if (chain.TimeLeft != chain.MaxTimeLeft)
                {
                    if ((int)(chain.Length * (chain.TimeLeft / (float)chain.MaxTimeLeft)) != (int)(chain.Length * ((chain.TimeLeft - 1) / (float)chain.MaxTimeLeft)))
                    {
                        Vector2 soundPos = chain.Start + ((chain.End - chain.Start) * ((int)(chain.Length * (chain.TimeLeft / (float)chain.MaxTimeLeft)) / (float)chain.Length));
                        if ((int)(chain.Length * (chain.TimeLeft / (float)chain.MaxTimeLeft)) == 1)
                        {
                            SoundEngine.PlaySound(SoundID.DD2_WitherBeastDeath with { Volume = 0.8f }, soundPos);
                            int wantedType = ModContent.NPCType<MoonLord>();
                            for (int n = 0; n < Main.maxNPCs; n++)
                            {
                                NPC npc = Main.npc[n];
                                if (npc == null || !npc.active)
                                    continue;
                                if (npc.type == wantedType && npc.localAI[3] == 0)
                                    npc.localAI[1] = 120;
                            }
                        }
                        else
                            SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact with { Volume = 0.8f }, soundPos);
                    }
                    chain.TimeLeft--;
                }
                if (chain.AttachedNPC != -1)
                {
                    if (!Main.npc[chain.AttachedNPC].active || !PillarTypeCheck(Main.npc[chain.AttachedNPC].type))
                    {
                        chain.TimeLeft--;
                        chain.AttachedNPC = -1;
                    }
                    else
                    {
                        chain.End = Main.npc[chain.AttachedNPC].Center;
                    }
                }
            }
            chainList.RemoveAll(x => x.TimeLeft <= 0);

            bool PillarTypeCheck(int type)
            {
                return type == vortex || type == nebula || type == stardust || type == solar;
            }
        }
        public static void DrawChains()
        {
            if (chainList == null)
                return;

            if (chainList.Count == 0)
                return;

            Texture2D chain1Tex = TexDict["Chain1"];
            Texture2D chain2Tex = TexDict["Chain2"];
            for (int i = 0; i < chainList.Count; i++)
            {
                Chain chain = chainList[i];
                Vector2 visualStart = chain.Start + ((chain.End - chain.Start).SafeNormalize(Vector2.UnitX) * (chain2Tex.Height * 0.5f));
                float rotation = (chain.End - visualStart).ToRotation();
                int visualLength = (int)(chain.Length * (chain.TimeLeft / (float)chain.MaxTimeLeft));
                for (int j = 0; j < visualLength; j++)
                {
                    Vector2 position = ((chain.End - visualStart) * (j / (float)chain.Length));
                    Main.EntitySpriteDraw(j % 2 == 0 ? chain2Tex : chain1Tex, visualStart + position - Main.screenPosition, null, Color.White, rotation + MathHelper.PiOver2, j % 2 == 0 ? chain2Tex.Size() * 0.5f : chain1Tex.Size() * 0.5f, 1f, SpriteEffects.None);
                }
            }
        }
        #endregion

        #region Attack Plan Rocket Bundles
        public void UpdateAttackPlanRocketBundles()
        {
            if (attackPlanRocketBundles == null)
                attackPlanRocketBundles = new List<AttackPlanRocketBundle>();
            if (attackPlanRocketBundles.Count == 0)
                return;

            for (int i = 0; i < attackPlanRocketBundles.Count; i++)
            {
                AttackPlanRocketBundle bundle = attackPlanRocketBundles[i];

                if (!RoomList[bundle.SourceRoom].active)
                {
                    bundle.Time = -1;
                    continue;
                }
                if (bundle.StartupTime < 30)
                {
                    bundle.StartupTime++;
                    continue;
                }

                if (bundle.Time % 12 == 0 && bundle.Count > 0)
                {
                    int ProjDamage = 70;
                    int owner = bundle.Owner;
                    if (owner >= 0)
                    {
                        ProjDamage += (Main.player[owner].ModPlayer().attackPlan - 1) * 15;
                    }
                    Projectile.NewProjectile(Projectile.GetSource_None(), bundle.Position + new Vector2(0, -32).RotatedBy(bundle.Rotation), (-Vector2.UnitY * 2.2f).RotatedBy(bundle.Rotation), ModContent.ProjectileType<PlanRocket>(), ProjDamage, 1f, bundle.Owner, -1);
                    bundle.Count--;
                    bundle.Rotation += MathHelper.PiOver4;
                }
                bundle.Time--;
            }
            attackPlanRocketBundles.RemoveAll(x => x.Time < 0);
        }
        #endregion

        #region Quake
        public void UpdateQuake()
        {
            if (quakeTime > 0)
            {
                Player player = Main.LocalPlayer;
                if (player == null)
                    return;

                if (quakeTime == setQuateTime)
                {
                    ExtraSoundSystem.ExtraSounds.Add(new ExtraSound(SoundEngine.PlaySound(EarthTremor with { Volume = 1f, Pitch = -0.5f, PitchVariance = 0.1f }, player.Center + new Vector2(Main.rand.NextFloat(-500, 500), -500)), 1, 120, 90, true));
                    ExtraSoundSystem.ExtraSounds.Add(new ExtraSound(SoundEngine.PlaySound(EarthPound with { Volume = 0.35f, Pitch = -0.5f, PitchVariance = 0.1f }, player.Center + new Vector2(Main.rand.NextBool() ? -500 : 500, -500)), 1, 120, 90, true));
                    ScreenshakeSystem.SetScreenshake(180, Main.rand.NextFloat(3, 4));
                }
                if (quakeTime % 5 == 0)
                {
                    Vector2 basePos = player.Center + new Vector2(Main.rand.NextFloat(-1050, 1050), -1500);
                    int debrisCount = Main.rand.Next(2, 5);
                    if (quakeTime < 75)
                        debrisCount /= 2;
                    int blackTileType = ModContent.TileType<Tiles.BlackTile>();
                    for (int i = 0; i < debrisCount; i++)
                    {
                        Vector2 startPos = basePos + new Vector2(Main.rand.NextFloat(-80, 80), 0);
                        Point startTilePoint = startPos.ToTileCoordinates();
                        Tile tile = ParanoidTileRetrieval(startTilePoint);
                        if (tile.TileType == blackTileType && tile.IsTileSolidGround(true))
                        {
                            for (int y = 1; y < 125; y++)
                            {
                                Tile checkTile = ParanoidTileRetrieval(startTilePoint + new Point(0, y));
                                if (!checkTile.IsTileSolidGround(true))
                                {
                                    Vector2 particlePos = new Vector2(startPos.X, ((int)(startPos.Y / 16) + y - 1) * 16);
                                    particlePos.Y -= 8;
                                    ParticleManager.AddParticle(new Debris(
                                        particlePos, Vector2.UnitY * Main.rand.NextFloat(-1.5f, 1.5f),
                                        Main.rand.Next(80, 120), Color.White, new Vector2(0.5f), Main.rand.Next(3), Main.rand.NextFloat(MathHelper.TwoPi),
                                        Main.rand.NextBool() ? SpriteEffects.None : SpriteEffects.FlipHorizontally, Main.rand.NextFloat(0.1f, 0.13f), 7f, 60, true),
                                        ParticleManager.ParticleLayer.BehindTiles);

                                    if (Main.rand.NextBool())
                                    {
                                        ParticleManager.AddParticle(new Ball(
                                        particlePos, (Vector2.UnitY * Main.rand.NextFloat(2f, 3.5f)).RotatedBy(Main.rand.NextFloat(-0.6f, 0.6f)), 60, Color.SandyBrown, new Vector2(Main.rand.NextFloat(0.075f, 0.15f)), 0, Main.rand.NextFloat(0.95f, 0.97f), 40, false, true),
                                        ParticleManager.ParticleLayer.BehindTiles);
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }

                quakeTime--;
            }
        }
        #endregion

        #region Post Update Everything
        public override void PostUpdateEverything()
        {
            if (chainList != null && chainList.Count >= 4)
            {
                StartLunarFloorPacket.Send();
            }
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                try
                {
                    PostUpdateWorld();
                }
                catch (Exception e)
                {
                    TerRoguelike.Instance.Logger.Error(e);
                }
                if (RequestBasinPacket.cooldown > 0)
                    RequestBasinPacket.cooldown--;
                if (TerPlayerPacket.cooldown > 0)
                    TerPlayerPacket.cooldown--;
                if (RequestRoomUmovingDataPacket.cooldown > 0)
                    RequestRoomUmovingDataPacket.cooldown--;
            }

            if (activatedTeleportCooldown > 0)
                activatedTeleportCooldown--;

            if (regeneratingWorld)
            {
                regeneratingWorldTime++;
                loopingDrama = 0;
                runStarted = false;

                Vector2 spawnPos = new Vector2(Main.spawnTileX, Main.spawnTileY) * 16;
                foreach (Player player in Main.ActivePlayers)
                {
                    player.Center = spawnPos;
                    var modPlayer = player.ModPlayer();
                    if (modPlayer != null && modPlayer.allowedToExist)
                    {
                        if (player.dead)
                            player.Spawn(PlayerSpawnContext.ReviveFromDeath);
                        else
                            player.Spawn(PlayerSpawnContext.RecallFromItem);
                    }
                }
                
                RegenerateWorldPacket.Send();
            }
            else if (regeneratingWorldTime > 0)
            {
                Vector2 spawnPos = new Vector2(Main.spawnTileX, Main.spawnTileY) * 16;
                foreach (Player player in Main.ActivePlayers)
                {
                    player.Center = spawnPos;
                    var modPlayer = player.ModPlayer();
                    if (modPlayer != null && modPlayer.allowedToExist)
                    {
                        if (player.dead)
                            player.Spawn(PlayerSpawnContext.ReviveFromDeath);
                        else
                            player.Spawn(PlayerSpawnContext.RecallFromItem);
                    }
                }

                Main.LocalPlayer.gfxOffY = 0;
                loopingDrama = 0;
                foreach (Player player in Main.ActivePlayers)
                {
                    var modPlayer = player.ModPlayer();
                    if (modPlayer == null)
                        return;
                    modPlayer.OnEnterWorld();
                    modPlayer.OnRespawn();
                }
                
                WorldGen.gen = false;
                MusicSystem.Initialized = false;
                Main.BlackFadeIn = 255;
                TerRoguelikeMenu.prepareForRoguelikeGeneration = false;
                regeneratingWorldTime = 0;
                if (TerRoguelikeWorld.currentLoop > 0)
                    runStarted = true;

                if (TerRoguelike.singleplayer)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        Point position = Main.LocalPlayer.Bottom.ToTileCoordinates();
                        position.Y += i;
                        if (TerRoguelikeUtils.IsTileSolidGround(Main.tile[position]))
                        {
                            Main.LocalPlayer.Bottom = position.ToVector2() * 16 + Vector2.UnitY * -1;
                            break;
                        }
                    }
                }
                if (Main.dedServ)
                {
                    ResetSectionManagerPacket.Send();
                    for (int i = 0; i < Netplay.Clients.Length; i++)
                    {
                        var client = Netplay.Clients[i];
                        if (client.IsConnected() && client.IsActive)
                        {
                            client.ResetSections();
                            RemoteClient.CheckSection(i, Main.player[i].position);
                        }
                    }
                    RoomUnmovingDataPacket.Send();
                    RegenerateWorldPacket.Send();
                }
                Main.Map.Clear();
            }

            ParticleManager.UpdateParticles();
            UpdateRemedialHealingOrbs();
            if (worldTeleportTime > 0)
            {
                worldTeleportTime++;
                if (worldTeleportTime > 60)
                    worldTeleportTime = 0;
            }

            if (escape)
            {
                try
                {
                    int furthest = 100;
                    foreach (Player player in Main.ActivePlayers)
                    {
                        var modPlayer = player.ModPlayer();
                        if (modPlayer.currentFloor.Stage < furthest)
                            furthest = modPlayer.currentFloor.Stage;
                    }
                    for (int i = 0; i < RoomManager.FloorIDsInPlay.Count; i++)
                    {
                        Floor floor = FloorID[RoomManager.FloorIDsInPlay[i]];
                        if (floor.Stage == furthest && floor.Stage >= 0 && floor.Stage <= 4)
                        {
                            floor.jstcUpdate();
                        }
                    }
                }
                catch (Exception e)
                {
                    TerRoguelike.Instance.Logger.Error(e);
                }
            }

            if (lunarGambitSceneTime > 0)
            {
                lunarGambitSceneTime++;

                Vector2 drawPos = lunarGambitSceneDisplayPos;

                if (drawPos.Distance(Main.Camera.Center) < 2500)
                {
                    if (lunarGambitSceneTime > lunarGambitStartDuration + lunarGambitFloatOverDuration)
                    {
                        if (SoundEngine.TryGetActiveSound(PortalSlot, out var portalSound) && portalSound.IsPlaying)
                        {
                            UpdatePortalSound(portalSound);
                        }
                        else
                        {
                            if (!CreditsSystem.creditsActive)
                                PortalSlot = SoundEngine.PlaySound(PortalLoop with { IsLooped = true, Volume = 0.64f, Pitch = -0.5f }, drawPos);

                            if (SoundEngine.TryGetActiveSound(PortalSlot, out var portalSound2) && portalSound2.IsPlaying)
                            {
                                UpdatePortalSound(portalSound2);
                            }
                        }
                        void UpdatePortalSound(ActiveSound sound)
                        {
                            float interpolant = MathHelper.Clamp((lunarGambitSceneTime - lunarGambitStartDuration - lunarGambitFloatOverDuration) / 180f, 0, 1);

                            sound.Volume = interpolant;
                            sound.Pitch = (float)Math.Cos(Main.GlobalTimeWrappedHourly * MathHelper.TwoPi) * 0.25f - 0.5f;
                        }
                        if (lunarGambitSceneTime > lunarGambitStartDuration + lunarGambitFloatOverDuration + 180)
                        {
                            if (loopingDrama <= 0)
                            {
                                foreach (Player player in Main.ActivePlayers)
                                {
                                    if (!player.dead && player.Center.Distance(drawPos) < 136)
                                    {
                                        loopingDrama++;
                                        SoundEngine.PlaySound(WorldTeleport with { Volume = 0.3f, Variants = [1], Pitch = -0.25f });
                                        CutsceneSystem.SetCutscene(drawPos, 120, 90, 2, 2.5f);
                                        ScreenshakeSystem.SetScreenshake(120, 6);
                                        break;
                                    }
                                }
                            }
                        }
                        float portalScaleInterpolant = lunarGambitSceneScaleInterpolant;
                        float portalRadius = 130 * portalScaleInterpolant;
                        Vector2 randVect = Main.rand.NextVector2CircularEdge(portalRadius, portalRadius);
                        Vector2 ballVel = randVect.RotatedBy(MathHelper.Pi * 0.4f * (randVect.X > 0 ? -1 : 1)) * 0.05f;
                        if (portalScaleInterpolant < 1)
                            ballVel += randVect.SafeNormalize(Vector2.UnitY) * 0.8f;
                        ParticleManager.AddParticle(new Ball(
                            drawPos + randVect, ballVel, 
                            30, Color.Lerp(Color.Cyan, Color.White, Main.rand.NextFloat(0.999999f - portalScaleInterpolant, 1)), 
                            new Vector2(0.14f) * MathHelper.Lerp(portalScaleInterpolant, 1, 0.4f), 0, 0.96f, 30));
                    }
                    if (lunarGambitSceneTime <= lunarGambitStartDuration + lunarGambitFloatOverDuration)
                    {
                        if (SoundEngine.TryGetActiveSound(PortalSlot, out var portalSound) && portalSound.IsPlaying)
                        {
                            UpdateMoonSound(portalSound);
                        }
                        else
                        {
                            if (!CreditsSystem.creditsActive)
                                PortalSlot = SoundEngine.PlaySound(AahLoop with { IsLooped = true, Volume = 0.07f, Pitch = 0f }, drawPos);

                            if (SoundEngine.TryGetActiveSound(PortalSlot, out var portalSound2) && portalSound2.IsPlaying)
                            {
                                UpdateMoonSound(portalSound2);
                            }
                        }
                        void UpdateMoonSound(ActiveSound sound)
                        {
                            if (lunarGambitSceneTime == lunarGambitStartDuration + lunarGambitFloatOverDuration)
                            {
                                sound.Stop();
                                return;
                            }
                                
                            sound.Position = drawPos;

                            float basePitch = 0;
                            basePitch = MathHelper.Lerp(basePitch, 1f, MathHelper.Clamp(lunarGambitSceneTime / 50f, 0, 1));
                            if (lunarGambitSceneTime >= lunarGambitStartDuration)
                            {
                                basePitch = MathHelper.Lerp(basePitch, -1f, MathHelper.Clamp((float)Math.Pow(1 - ((lunarGambitSceneTime - lunarGambitStartDuration - lunarGambitFloatOverDuration) / -70f), 3), 0, 1));
                            }
                            sound.Pitch = basePitch;

                            sound.Volume = lunarGambitSceneTime < 60 ? MathHelper.Clamp(lunarGambitSceneTime / 60f, 0, 1) : MathHelper.Clamp((lunarGambitSceneTime - lunarGambitStartDuration - lunarGambitFloatOverDuration) / -4f, 0, 1);
                        }

                        int randBoolValue = lunarGambitSceneTime > lunarGambitStartDuration + lunarGambitFloatOverDuration - 30 ? 5 : 2;
                        if (Main.rand.NextBool(randBoolValue))
                        {
                            ParticleManager.AddParticle(new Ball(
                                drawPos,
                                Main.rand.NextVector2CircularEdge(3, 3) * Main.rand.NextFloat(0.5f, 1f),
                                30, Color.Lerp(Color.White, Color.Cyan, Main.rand.NextFloat(0.3f, 1f)) * 1,
                                new Vector2(Main.rand.NextFloat(0.5f, 1f) * 0.12f), 0, 0.96f, 30, true, false), ParticleManager.ParticleLayer.Default);
                        }
                        if (lunarGambitSceneTime == lunarGambitStartDuration + lunarGambitFloatOverDuration)
                        {
                            SoundEngine.PlaySound(SoundID.NPCDeath43 with { Pitch = -0.35f, PitchVariance = 0, Volume = 0.5f}, drawPos);
                            ExtraSoundSystem.ExtraSounds.Add(new(SoundEngine.PlaySound(PortalSpawn with { Pitch = -0.7f, PitchVariance = 0, Volume = 1f }, drawPos), 1.5f));
                            for (int i = 0; i < 9; i++)
                            {
                                float rot = i / 9f * MathHelper.TwoPi + MathHelper.PiOver2;
                                Vector2 rotVect = rot.ToRotationVector2();
                                ParticleManager.AddParticle(new Debris(
                                    drawPos + rotVect * 5, (rotVect * new Vector2(3f, 2.4f) * Main.rand.NextFloat(0.4f, 1f) - Vector2.UnitY * 1.2f), 
                                    50, Color.Lerp(Color.Cyan, Color.White, Main.rand.NextFloat(0.4f)), new Vector2(Main.rand.NextFloat(0.55f, 0.72f)), 0, 
                                    Main.rand.NextFloat(MathHelper.TwoPi), SpriteEffects.None, 0.14f, 10, 30));

                                ParticleManager.AddParticle(new Ball(
                                    drawPos, rotVect.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f)) * 5 * Main.rand.NextFloat(0.4f, 1f), 60, Color.White * 0.5f, new Vector2(Main.rand.NextFloat(0.15f, 0.25f)), 0, 0.92f, 50));
                            }
                        }
                    }
                }
                else if (SoundEngine.TryGetActiveSound(PortalSlot, out var portalSound))
                {
                    portalSound.Stop();
                }

                if (loopingDrama > 0)
                {
                    if (loopingDrama == 120)
                    {
                        bool stayinworld = true;
                        if (stayinworld && !Main.dedServ)
                        {
                            if (!TerRoguelike.mpClient)
                            {
                                TerRoguelikeWorld.promoteLoop = true;
                                RegenerateWorld(true);
                            }
                            StartRoomGenerationPacket.Send(true);
                        }
                        /*
                        else
                        {
                            ZoomSystem.SetZoomAnimation(Main.GameZoomTarget, 2);
                            ZoomSystem.zoomOverride = Main.GameZoomTarget;
                            if (TerRoguelikeWorld.IsDeletableOnExit)
                            {
                                TerRoguelikeMenu.desiredPlayer = Main.ActivePlayerFileData;
                                TerRoguelikeMenu.wipeTempWorld = true;
                                TerRoguelikeMenu.prepareForRoguelikeGeneration = true;
                                TerRoguelikeWorld.promoteLoop = true;
                            }
                            SetCalm(Silence);
                            SetCombat(Silence);
                            SetMusicMode(MusicStyle.Silent);
                            WorldGen.SaveAndQuit();
                        }
                        */
                    }
                    loopingDrama++;
                }
            }
            if (escape && jstcPortalTime != 0)
            {
                float portalScaleInterpolant = MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp(Math.Abs(jstcPortalTime) / 60f, 0, 1));
                Vector2 finalScale = jstcPortalScale * 1.5f * portalScaleInterpolant;
                float rot = finalScale.X > finalScale.Y ? MathHelper.PiOver2 : 0;

                
                for (int i = 0; i < 2; i++)
                {
                    float rate = Math.Max(jstcPortalScale.X, jstcPortalScale.Y) / 250f;
                    if (Main.rand.NextFloat() < rate)
                    {
                        Vector2 randVect = Main.rand.NextVector2CircularEdge(finalScale.X * 0.4f, finalScale.Y * 0.4f);
                        Vector2 ballVel = -randVect * 0.1f;
                        ballVel = ballVel.ToRotation().AngleLerp(jstcPortalRot, Main.rand.NextFloat(0.5f, 0.9f)).ToRotationVector2() * Main.rand.NextFloat(2f, 6f);

                        ParticleManager.AddParticle(new Ball(
                            jstcPortalPos + randVect, ballVel,
                            30, Color.Lerp(Color.Yellow, Color.White, Main.rand.NextFloat(0.999999f - portalScaleInterpolant, 1)),
                            new Vector2(0.14f) * MathHelper.Lerp(portalScaleInterpolant, 1, 0.4f), 0, 0.96f, 30));
                    }
                }
            }

            if (itemBasins.Count > 0)
            {
                bool spawnParticles = (int)(Main.GlobalTimeWrappedHourly * 60) % 4 == 0;
                int basinTileType = ModContent.TileType<Tiles.ItemBasin>();
                bool interact = PlayerInput.Triggers.JustPressed.MouseRight && !Main.mapFullscreen;
                Player player = Main.LocalPlayer;
                if (player == null || !player.active || player.ModPlayer() == null)
                    return;
                var modPlayer = player.ModPlayer();
                if (Main.mapFullscreen)
                    modPlayer.selectedBasin = null;

                try
                {
                    for (int i = 0; i < itemBasins.Count; i++)
                    {
                        var basin = itemBasins[i];

                        if (Main.tile[basin.position.X, basin.position.Y].TileType != basinTileType)
                        {
                            itemBasins.RemoveAt(i);
                            i--;
                            continue;
                        }
                        if (basin.nearby > 0)
                        {
                            basin.nearby--;
                            if (spawnParticles)
                                basin.SpawnParticles();
                        }
                    }
                }
                catch (Exception ex)
                {
                    TerRoguelike.Instance.Logger.Error(ex);
                }
                
            }
            if (ItemBasinUI.stickMoveCooldown > 0)
                ItemBasinUI.stickMoveCooldown--;
        }
        #endregion

        #region Networking
        public override void NetSend(BinaryWriter writer)
        {
            RoomUnmovingDataPacket.Send();
        }
        public override void NetReceive(BinaryReader reader)
        {
            
        }
        #endregion
        public static void ClearWorldTerRoguelike()
        {
            foreach (var floor in FloorID)
            {
                floor.Reset();
            }
            runStartMeter = 0;
            runStartTouched = false;
            TerRoguelikePlayer.allDeadTime = 0;
            difficultyReceivedByServer = false;
            if (Main.netMode == NetmodeID.MultiplayerClient)
                obtainedRoomListFromServer = false;
            else
                obtainedRoomListFromServer = true;

            ZoomSystem.SetZoomAnimation(Main.GameZoomTarget, 2);
            ZoomSystem.zoomOverride = Main.GameZoomTarget;

            chainList.Clear();
            ParticleManager.ActiveParticles.Clear();

            CutsceneSystem.cutsceneTimer = 0;
            CutsceneSystem.cutsceneDisableControl = false;
            CreditsSystem.StopCredits();
            CutsceneSystem.cutsceneActive = false;
            Main.screenPosition = Main.Camera.UnscaledPosition;
            TerRoguelikeWorld.IsDebugWorld = false;
            TerRoguelikeWorld.IsDeletableOnExit = IsDeletableOnExit && regeneratingWorld;
            TerRoguelikeWorld.IsTerRoguelikeWorld = regeneratingWorld;
            TerRoguelikeWorld.lunarFloorInitialized = false;
            TerRoguelikeWorld.lunarBossSpawned = false;
            TerRoguelikeWorld.escape = false;
            TerRoguelikeWorld.escaped = false;
            itemBasins.Clear();
            worldTeleportTime = 0;
            sanctuaryTries = 0;
            sanctuaryCount = 0;
            quakeTime = 0;
            quakeCooldown = 0;
            postDrawEverythingCache.Clear();
            lunarGambitSceneTime = 0;
            lunarGambitSceneStartPos = Vector2.Zero;
            loopingDrama = 0;
            jstcPortalPos = Vector2.Zero;
            jstcPortalTime = 0;
            healingPulses.Clear();
            attackPlanRocketBundles.Clear();
            remedialHealingOrbs.Clear();
            ILEdits.dualContrastTileShader = false;
            SpawnManager.pendingEnemies.Clear();
            SpawnManager.pendingItems.Clear();
            ParticleManager.ActiveParticles.Clear();
            ParticleManager.ActiveParticlesAfterEverything.Clear();
            ParticleManager.ActiveParticlesAfterProjectiles.Clear();
            ParticleManager.ActiveParticlesBehindTiles.Clear();
            Room.forceLoopCalculation = -1;
            if (RoomList != null)
            {
                for (int i = 0; i < RoomList.Count; i++)
                    ResetRoomID(RoomList[i].ID);
            }

            TerRoguelikeWorldManagementSystem.currentlyGeneratingTerRoguelikeWorld = false;
            if (!regeneratingWorld)
                WorldGen.gen = false;

            if (SoundEngine.TryGetActiveSound(PortalSlot, out var portalSound))
            {
                portalSound.Stop();
            }
        }
        public override void ClearWorld()
        {
            ClearWorldTerRoguelike();
        }
        public static void RegenerateWorld(bool loop = false)
        {
            if (!TerRoguelikeWorld.IsTerRoguelikeWorld)
                return;

            TerRoguelikeWorld.promoteLoop = loop;
            TerRoguelikeWorld.currentStage = 0;
            if (TerRoguelikeWorld.promoteLoop)
            {
                TerRoguelikeWorld.promoteLoop = false;
                TerRoguelikeWorld.currentLoop++;
            }
            else
            {
                TerRoguelikeWorld.currentLoop = 0;
                runStarted = false;
                RoomSystem.playerCount = 1;
            }
                
            StageCountPacket.Send();

            TerRoguelikeMenu.prepareForRoguelikeGeneration = true;
            for (int i = 0; i < Main.maxNPCs; i++)
                Main.npc[i].active = false;
            for (int i = 0; i < Main.maxProjectiles; i++)
                Main.projectile[i].active = false;
            for (int i = 0; i < Main.maxDust; i++)
                Main.dust[i].active = false;
            for (int i = 0; i < Main.maxGore; i++)
                Main.gore[i].active = false;
            for (int i = 0; i < Main.maxCombatText; i++)
                Main.combatText[i].active = false;
            for (int i = 0; i < Main.maxItems; i++)
                Main.item[i].active = false;

            SetCalm(Silence);
            SetCombat(Silence);
            SetMusicMode(MusicStyle.Silent);
            regeneratingWorld = true;
            WorldGen.gen = true;
            ClearWorldTerRoguelike();
            Main.LocalPlayer.Center = new Vector2(Main.spawnTileX, Main.spawnTileY) * 16;

            if (!loop)
            {
                IEnumerable<Item> vanillaItems = [];
                for (int i = 0; i < 58; i++)
                    Main.LocalPlayer.inventory[i].type = Main.LocalPlayer.inventory[i].stack = 0;
                List<Item> startingItems = PlayerLoader.GetStartingItems(Main.LocalPlayer, vanillaItems);
                PlayerLoader.SetStartInventory(Main.LocalPlayer, startingItems);
                Main.LocalPlayer.trashItem = new(ItemID.None, 0);
                TerRoguelikeMenu.desiredPlayer = Main.ActivePlayerFileData;
            }


            for (int i = 0; i < RoomList.Count; i++)
                ResetRoomID(RoomList[i].ID);
            foreach (var floor in FloorID)
            {
                floor.Reset();
            }

            ThreadPool.QueueUserWorkItem(_ => TerRoguelikeWorldManagementSystem.RegenerateWorld());
        }
        public override void SetStaticDefaults()
        {
            
        }
        public override void PostSetupContent()
        {
            ItemManager.LoadStarterItems();
        }
        public override void Load()
        {
            calamityMod = null;
            japaneseTranslation = null;
            spanishTranslation = null;
            koreanTranslation = null;
            ModLoader.TryGetMod("CalamityMod", out calamityMod);
            ModLoader.TryGetMod("TerRoguelikeJapanese", out japaneseTranslation);
            ModLoader.TryGetMod("TerRoguelikeES", out spanishTranslation);
            ModLoader.TryGetMod("TerRoguelikeKR", out koreanTranslation);
            translationMods =
            [
                japaneseTranslation,
                spanishTranslation,
                koreanTranslation
            ];
        }
        public override void Unload()
        {
            translationMods = null;
            calamityMod = null;
            japaneseTranslation = null;
            spanishTranslation = null;
            koreanTranslation = null;
        }
    }
    public class HealingPulse
    {
        public HealingPulse(Vector2 position)
        {
            Position = position;
        }
        public Vector2 Position;
        public int Time = 30;
    }
    public class AttackPlanRocketBundle
    {
        public AttackPlanRocketBundle(Vector2 position, int count, int owner, int sourceRoom)
        {
            Position = position;
            Count = count;
            Owner = owner;
            Time = count * 12;
            SourceRoom = sourceRoom;
        }
        public Vector2 Position;
        public int Count;
        public int Owner;
        public int Time;
        public float Rotation = 0f;
        public int SourceRoom;
        public int StartupTime = 0;
    }
}
