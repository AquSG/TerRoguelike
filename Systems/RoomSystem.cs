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
using Terraria.GameContent.Bestiary;
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

namespace TerRoguelike.Systems
{
    public class RoomSystem : ModSystem //This file handles pretty much everything relating to updating rooms
    {
        public static List<Room> RoomList; //List of all rooms currently in play in the world
        public static List<HealingPulse> healingPulses = new List<HealingPulse>();
        public static List<AttackPlanRocketBundle> attackPlanRocketBundles = new List<AttackPlanRocketBundle>();
        public static bool obtainedRoomListFromServer = false;
        public static bool debugDrawNotSpawnedEnemies = false;
        public static void NewRoom(Room room)
        {
            RoomList.Add(room);
        }
        public override void PostUpdateWorld()
        {
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
                if (escapeTime == 0)
                {
                    for (int i = 0; i < Main.maxPlayers; i++)
                    {
                        if (Main.player[i] == null)
                            continue;
                        if (!Main.player[i].active)
                            continue;
                        if (Main.player[i].dead)
                            continue;

                        if (Main.player[i].GetModPlayer<TerRoguelikePlayer>().escaped)
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

            if (RoomList.Count == 0)
                return;

            int loopCount = -1;
            foreach (Room room in RoomList)
            {
                loopCount++;
                if (room == null)
                    continue;

                room.myRoom = loopCount; //updates the room's 'myRoom' to refer to it's index on RoomList

                for (int i = 0; i < Main.maxPlayers; i++) //Player collision with rooms
                {
                    Player player;
                    if (Main.netMode == NetmodeID.SinglePlayer)
                        player = Main.player[Main.myPlayer];
                    else
                        player = Main.player[i];

                    TerRoguelikePlayer modPlayer = player.GetModPlayer<TerRoguelikePlayer>();
                    bool roomXcheck = player.Center.X - (player.width / 2f) > (room.RoomPosition.X + 1f) * 16f - 1f && player.Center.X + (player.width / 2f) < (room.RoomPosition.X - 1f + room.RoomDimensions.X) * 16f + 1f;
                    bool roomYcheck = player.Center.Y - (player.height / 2f) > (room.RoomPosition.Y + 1f) * 16f && player.Center.Y + (player.height / 2f) < (room.RoomPosition.Y - (15f / 16f) + room.RoomDimensions.Y) * 16f;
                    if (roomXcheck && roomYcheck)
                    {
                        modPlayer.currentRoom = -1; //Current room is -1 unless the player is inside an active room in RoomList
                        if (room.AssociatedFloor != -1)
                            modPlayer.currentFloor = FloorID[room.AssociatedFloor]; //If player is inside a room with a valid value for an associated floor, set it to that.
                        if (room.active)
                            modPlayer.currentRoom = room.myRoom;

                        if (modPlayer.currentFloor.ID == 10 && !lunarFloorInitialized)
                        {
                            InitializeLunarFloor();
                        }

                        if (escape && !room.IsStartRoom)
                        {
                            if (loopCount >= 2)
                            {
                                Room jumpstartRoom = RoomList[loopCount - 2];
                                if (!jumpstartRoom.initialized)
                                {
                                    jumpstartRoom.awake = true;
                                    jumpstartRoom.InitializeRoom();
                                }
                            }
                        }

                        room.awake = true;
                        if (room.CanDescend(player, modPlayer)) //New Floor Blue Wall Portal Teleport
                        {
                            room.Descend(player);
                            FloorTransitionEffects();
                        }
                        if (room.CanAscend(player, modPlayer))
                        {
                            room.Ascend(player);

                            FloorTransitionEffects();
                        }

                        if (room.closedTime == 1) // heal players on room clear so no waiting slog for natural life regen
                        {
                            player.statLife = player.statLifeMax2;
                        }
                    }

                    if (Main.netMode == NetmodeID.SinglePlayer) // don't loop through all players if in singleplayer lol
                        break;
                }

                room.Update();
            }
        }
        #region Initialize Lunar Floor
        public static void InitializeLunarFloor()
        {
            SetMusicMode(MusicStyle.AllCalm);
            SetCalm(FinalStage);
            CalmVolumeLevel = 0.4f;

            if (lunarFloorInitialized)
                return;
            lunarFloorInitialized = true;

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
                    chainList.Add(new Chain(chainStart, Main.npc[spawnedNpc].Center, 24, 120, spawnedNpc));
                    pillarCount++;
                    continue;
                }
                if (room.ID == RoomDict["LunarPillarRoomTopRight"])
                {
                    int spawnedNpc = NPC.NewNPC(NPC.GetSource_NaturalSpawn(), (int)((room.RoomPosition.X + (room.RoomDimensions.X * 0.5f)) * 16f), (int)((room.RoomPosition.Y + (room.RoomDimensions.Y * 0.5f)) * 16f) + 160, ModContent.NPCType<StardustPillar>());
                    Main.npc[spawnedNpc].ModNPC().isRoomNPC = true;
                    Main.npc[spawnedNpc].ModNPC().sourceRoomListID = i;
                    chainList.Add(new Chain(chainStart, Main.npc[spawnedNpc].Center, 24, 120, spawnedNpc));
                    pillarCount++;
                    continue;
                }
                if (room.ID == RoomDict["LunarPillarRoomBottomLeft"])
                {
                    int spawnedNpc = NPC.NewNPC(NPC.GetSource_NaturalSpawn(), (int)((room.RoomPosition.X + (room.RoomDimensions.X * 0.5f)) * 16f), (int)((room.RoomPosition.Y + (room.RoomDimensions.Y * 0.5f)) * 16f) + 160, ModContent.NPCType<NebulaPillar>());
                    Main.npc[spawnedNpc].ModNPC().isRoomNPC = true;
                    Main.npc[spawnedNpc].ModNPC().sourceRoomListID = i;
                    chainList.Add(new Chain(chainStart, Main.npc[spawnedNpc].Center, 24, 120, spawnedNpc));
                    pillarCount++;
                    continue;
                }
                if (room.ID == RoomDict["LunarPillarRoomBottomRight"])
                {
                    int spawnedNpc = NPC.NewNPC(NPC.GetSource_NaturalSpawn(), (int)((room.RoomPosition.X + (room.RoomDimensions.X * 0.5f)) * 16f), (int)((room.RoomPosition.Y + (room.RoomDimensions.Y * 0.5f)) * 16f) + 160, ModContent.NPCType<SolarPillar>());
                    Main.npc[spawnedNpc].ModNPC().isRoomNPC = true;
                    Main.npc[spawnedNpc].ModNPC().sourceRoomListID = i;
                    chainList.Add(new Chain(chainStart, Main.npc[spawnedNpc].Center, 24, 120, spawnedNpc));
                    pillarCount++;
                    continue;
                }
            }
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
        }
        #endregion

        /// <summary>
        /// Resets the given room ID to it's default values, aside from position and dimensions
        /// </summary>
        public static void ResetRoomID(int id)
        {
            Room room = RoomID[id];
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
            room.bossSpawnPos = Vector2.Zero;
            room.bossDead = false;
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

                Main.EntitySpriteDraw(TextureAssets.MagicPixel.Value, Vector2.Zero, null, Color.LightGray * worldTeleportOpacity, 0, Vector2.Zero, new Vector2(Main.screenWidth, Main.screenHeight * 0.0011f), SpriteEffects.None);

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

                if (room.wallActive)
                {
                    //Draw the pink borders indicating the bounds of the room
                    StartAdditiveSpritebatch(false);
                    for (float side = 0; side < 2; side++)
                    {
                        for (float i = 0; i < room.RoomDimensions.X; i++)
                        {
                            Vector2 targetBlock = room.RoomPosition + new Vector2(i, side * room.RoomDimensions.Y - side);

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
                        for (float i = 0; i < room.RoomDimensions.Y; i++)
                        {
                            Vector2 targetBlock = room.RoomPosition + new Vector2(side * room.RoomDimensions.X - side, i);
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
                player.respawnTimer = 60;

            DrawDeathScene();
            DrawPendingEnemies();
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
            if (Main.netMode == NetmodeID.SinglePlayer)
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

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            Color color = Color.HotPink;
            Vector3 colorHSL = Main.rgbToHsl(color);
            GameShaders.Misc["TerRoguelike:BasicTint"].UseOpacity(0.4f);
            GameShaders.Misc["TerRoguelike:BasicTint"].UseColor(Main.hslToRgb(1 - colorHSL.X, colorHSL.Y, colorHSL.Z));
            GameShaders.Misc["TerRoguelike:BasicTint"].Apply();
            for (int i = 0; i < SpawnManager.pendingEnemies.Count; i++)
            {
                PendingEnemy enemy = SpawnManager.pendingEnemies[i];
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
                        if (!player.active)
                            continue;

                        TerRoguelikePlayer modPlayer = player.GetModPlayer<TerRoguelikePlayer>();
                        modPlayer.ScaleableHeal((int)(player.statLifeMax2 / 2f));
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

        #region Chains
        public void UpdateChains()
        {
            if (chainList == null)
                return;
            if (chainList.Count == 0)
                return;

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
                        }
                        else
                            SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact with { Volume = 0.8f }, soundPos);
                    }
                    chain.TimeLeft--;
                }
                if (chain.AttachedNPC != -1)
                {
                    if (!Main.npc[chain.AttachedNPC].active)
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

                if (bundle.Time % 12 == 0 && bundle.Count > 0)
                {
                    Projectile.NewProjectile(Projectile.GetSource_None(), bundle.Position + new Vector2(0, -32).RotatedBy(bundle.Rotation), (-Vector2.UnitY * 2.2f).RotatedBy(bundle.Rotation), ModContent.ProjectileType<PlanRocket>(), 70, 1f, bundle.Owner, -1);
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
            ParticleManager.UpdateParticles();
            if (worldTeleportTime > 0)
            {
                worldTeleportTime++;
                if (worldTeleportTime > 60)
                    worldTeleportTime = 0;
            }

            if (itemBasins.Count > 0)
            {
                bool spawnParticles = (int)(Main.GlobalTimeWrappedHourly * 60) % 4 == 0;
                int basinTileType = ModContent.TileType<ItemBasin>();
                bool interact = PlayerInput.Triggers.JustPressed.MouseRight && !Main.mapFullscreen;
                Player player = Main.LocalPlayer;
                if (player == null || !player.active || player.ModPlayer() == null)
                    return;
                var modPlayer = player.ModPlayer();
                if (Main.mapFullscreen)
                    modPlayer.selectedBasin = null;

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
                        if (interact && basin.rect16.Contains(Main.MouseWorld.ToPoint()))
                        {
                            Vector2 checkPos = basin.position.ToWorldCoordinates(24, 16);
                            if (player.Center.Distance(checkPos) > 200)
                                continue;

                            modPlayer.selectedBasin = basin;
                            ItemBasinUI.gamepadSelectedOption = 0;
                            SoundEngine.PlaySound(SoundID.MenuOpen);
                        }
                    }
                }
            }
            if (ItemBasinUI.stickMoveCooldown > 0)
                ItemBasinUI.stickMoveCooldown--;
        }
        #endregion

        #region Networking
        public override void NetSend(BinaryWriter writer)
        {
            //Sorrowful attempt at any semblance of multiplayer compat
            writer.Write(TerRoguelikeWorld.IsTerRoguelikeWorld);
            List<byte> packageRoomLisIDs = new List<byte>();
            for (int i = 0; i < RoomList.Count; i++)
            {
                packageRoomLisIDs.Add((byte)RoomList[i].ID);
            }
            ReadOnlySpan<byte> sentRoomList = packageRoomLisIDs.ToArray();
            writer.Write(sentRoomList.Length);
            writer.Write(sentRoomList);
        }
        public override void NetReceive(BinaryReader reader)
        {
            if (obtainedRoomListFromServer)
                return;

            if (RoomList == null)
                RoomList = new List<Room>();

            TerRoguelikeWorld.IsTerRoguelikeWorld = reader.ReadBoolean();
            int roomListLength = reader.ReadInt32();
            byte[] recievedRoomIDs = reader.ReadBytes(roomListLength);
            for (int i = 0; i < recievedRoomIDs.Length; i++)
            {
                int roomID = (int)recievedRoomIDs[i];
                RoomList.Add(RoomID[roomID]);
                ResetRoomID(roomID);
            }
            obtainedRoomListFromServer = true;
        }
        #endregion
        public override void ClearWorld()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                obtainedRoomListFromServer = false;
            else
                obtainedRoomListFromServer = true;

            ZoomSystem.SetZoomAnimation(Main.GameZoomTarget, 2);

            chainList.Clear();
            ParticleManager.ActiveParticles.Clear();

            CutsceneSystem.cutsceneTimer = 0;
            CutsceneSystem.cutsceneDisableControl = false;
            CutsceneSystem.cutsceneActive = false;
            Main.screenPosition = Main.Camera.UnscaledPosition;
            TerRoguelikeWorld.IsDebugWorld = false;
            TerRoguelikeWorld.IsDeletableOnExit = false;
            TerRoguelikeWorld.IsTerRoguelikeWorld = false;
            TerRoguelikeWorld.lunarFloorInitialized = false;
            TerRoguelikeWorld.lunarBossSpawned = false;
            TerRoguelikeWorld.escape = false;
            itemBasins.Clear();
            worldTeleportTime = 0;
            sanctuaryTries = 0;
            sanctuaryCount = 0;
            quakeTime = 0;
            quakeCooldown = 0;
        }
        public override void SetStaticDefaults()
        {
            
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
    }
}
