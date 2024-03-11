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

namespace TerRoguelike.Systems
{
    public class RoomSystem : ModSystem //This file handles pretty much everything relating to updating rooms
    {
        public static List<Room> RoomList; //List of all rooms currently in play in the world
        public static List<HealingPulse> healingPulses = new List<HealingPulse>();
        public static List<AttackPlanRocketBundle> attackPlanRocketBundles = new List<AttackPlanRocketBundle>();
        public static bool obtainedRoomListFromServer = false;
        public static Vector2 DrawBehindTilesOffset = new Vector2(190, 150);
        public static void NewRoom(Room room)
        {
            RoomList.Add(room);
        }
        public override void PostUpdateWorld()
        {
            if (escapeTime > 0 && escape)
            {
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
                    escape = false;
                }
            }
            SpawnManager.UpdateSpawnManager(); //Run all logic for all pending items and enemies being telegraphed
            UpdateHealingPulse(); //Used for uncommon healing item based on room time
            UpdateAttackPlanRocketBundles(); //Used for the attack plan item that handles future attack plan bundles
            UpdateChains();

            if (RoomList == null)
                return;

            if (!RoomList.Any())
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

                        if (escape)
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
                        bool descendTeleportCheck = room.closedTime > 180 && room.IsBossRoom && player.position.X + player.width >= ((room.RoomPosition.X + room.RoomDimensions.X) * 16f) - 22f && !player.dead && !escape;
                        bool ascendTeleportCheck = room.IsStartRoom && player.position.X <= (room.RoomPosition.X * 16f) + 22f && !player.dead && escape;
                        if (descendTeleportCheck) //New Floor Blue Wall Portal Teleport
                        {
                            int nextStage = modPlayer.currentFloor.Stage + 1;
                            if (nextStage >= RoomManager.FloorIDsInPlay.Count) // if FloorIDsInPlay overflows, send back to the start
                            {
                                nextStage = 0;
                                currentStage = 0;
                            }
                            else
                            {
                                if (nextStage > currentStage)
                                    currentStage = nextStage;
                            }

                            var nextFloor = FloorID[RoomManager.FloorIDsInPlay[nextStage]];
                            var targetRoom = RoomID[nextFloor.StartRoomID];
                            player.Center = (targetRoom.RoomPosition + (targetRoom.RoomDimensions / 2f)) * 16f;
                            modPlayer.currentFloor = nextFloor;

                            if (nextFloor.Name != "Lunar")
                            {
                                SetCalm(nextFloor.Soundtrack.CalmTrack);
                                SetCombat(nextFloor.Soundtrack.CombatTrack);
                                SetMusicMode(MusicStyle.Dynamic);
                                CombatVolumeCache = 0;
                                CalmVolumeCache = 0;
                            }
                            
                            NewFloorEffects(targetRoom, modPlayer);
                        }

                        if (ascendTeleportCheck)
                        {
                            int nextStage = modPlayer.currentFloor.Stage - 1;
                            if (nextStage < 0) // if FloorIDsInPlay underflows, send back to the start
                            {
                                nextStage = 0;
                                escape = false;
                            }

                            var nextFloor = FloorID[RoomManager.FloorIDsInPlay[nextStage]];
                            Room potentialRoom = null;
                            for (int j = 0; j < nextFloor.BossRoomIDs.Count; j++)
                            {
                                potentialRoom = RoomList.Find(x => x.ID == nextFloor.BossRoomIDs[j]);
                                if (potentialRoom != null)
                                    break;
                            }
                            var targetRoom = potentialRoom;
                            if (escape)
                            {
                                player.Center = (targetRoom.RoomPosition + (targetRoom.RoomDimensions / 2f)) * 16f;
                                player.BottomRight = modPlayer.FindAirToPlayer((targetRoom.RoomPosition + targetRoom.RoomDimensions) * 16f);
                                modPlayer.currentFloor = nextFloor;
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

                                    if (modNPC.sourceRoomListID > targetRoom.myRoom)
                                        npc.active = false;
                                }
                            }
                            else
                            {
                                player.Center = new Vector2(Main.maxTilesX * 8f, 3000f);
                                player.GetModPlayer<TerRoguelikePlayer>().escaped = true;
                                for (int L = 0; L < RoomList.Count; L++)
                                {
                                    ResetRoomID(RoomList[L].ID);
                                }
                                SetMusicMode(MusicStyle.Silent);

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

                                    if (modNPC.sourceRoomListID > targetRoom.myRoom)
                                        npc.active = false;
                                }
                            }

                            NewFloorEffects(targetRoom, modPlayer);
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
            SetCalm(FinalStage with { Volume = 0.4f });

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
            tag["isTerRoguelikeWorld"] = isTerRoguelikeWorld;
            tag["isDeletableOnExit"] = isDeletableOnExit;

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
            MusicSystem.Initialized = false;
            TerRoguelikeWorld.lunarFloorInitialized = false;
            TerRoguelikeWorld.lunarBossSpawned = false;
            TerRoguelikeWorld.escape = false;
            TerRoguelikeMenu.prepareForRoguelikeGeneration = false;
            var isTerRoguelikeWorld = tag.GetBool("isTerRoguelikeWorld");
            var isDeletableOnExit = tag.GetBool("isDeletableOnExit");
            TerRoguelikeWorld.IsTerRoguelikeWorld = isTerRoguelikeWorld;
            TerRoguelikeWorld.IsDeletableOnExit = isDeletableOnExit;
            TerRoguelikeWorld.currentStage = 0;
            if (!TerRoguelikeWorld.IsTerRoguelikeWorld)
            {
                RoomList = new List<Room>();
                return;
            }

            if (SpawnManager.pendingEnemies != null)
                SpawnManager.pendingEnemies.Clear();
            else
                SpawnManager.pendingEnemies = new List<PendingEnemy>();

            if (SpawnManager.pendingItems != null)
                SpawnManager.pendingItems.Clear();
            else
                SpawnManager.pendingItems = new List<PendingItem>();

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
            RoomManager.FloorIDsInPlay = new List<int>();
            foreach (int floorID in floorIDsInPlay)
            {
                RoomManager.FloorIDsInPlay.Add(floorID);
            }
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
        public static void PostDrawWalls()
        {
            DrawChains();
        }
        public override void PostDrawTiles()
        {
            Player player = Main.player[Main.myPlayer];
            if (IsTerRoguelikeWorld && player.dead)
                player.respawnTimer = 60;

            DrawDeathScene();
            DrawPendingEnemies();
            DrawHealingPulse();
            ParticleManager.DrawParticles();

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

            Texture2D lightTexture = TexDict["TemporaryBlock"];
            foreach (Room room in RoomList)
            {
                if (room == null)
                    continue;
                
                if (lunarFloorInitialized && (!lunarBossSpawned || (!room.awake && room.closedTime <= 0)))
                {
                    if (room.ID == RoomDict["LunarBossRoom1"])
                    {
                        StartAlphaBlendSpritebatch(false);
                        Vector2 position = (room.RoomPosition + (room.RoomDimensions * 0.5f)) * 16f;
                        Texture2D moonLordTex = TexDict["StillMoonLord"];
                        Main.EntitySpriteDraw(moonLordTex, (room.RoomPosition + (room.RoomDimensions * 0.5f)) * 16f - Main.screenPosition, null, Color.White * (0.5f + (MathHelper.Lerp(0, 0.125f, 0.5f + ((float)Math.Cos(Main.GlobalTimeWrappedHourly * 2f) * 0.5f)))), 0f, moonLordTex.Size() * 0.5f, 1f, SpriteEffects.None);
                        Main.spriteBatch.End();
                    }
                }

                if (!room.StartCondition())
                    continue;

                if (room.IsStartRoom && escape)
                {
                    bool canDraw = false;
                    for (int i = 0; i < Main.maxPlayers; i++)
                    {
                        if (Vector2.Distance(Main.player[i].Center, (room.RoomPosition + new Vector2(0, 0)) * 16f) < 2500f)
                        {
                            canDraw = true;
                            break;
                        }
                    }
                    if (!canDraw)
                        continue;

                    //Draw the blue wall portal
                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                    for (float i = 0; i < room.RoomDimensions.Y; i++)
                    {
                        Vector2 targetBlock = room.RoomPosition + new Vector2(1, i);
                        int tileType = Main.tile[targetBlock.ToPoint()].TileType;
                        if (TileID.Sets.BlockMergesWithMergeAllBlock[tileType])
                        {
                            if (Main.tile[targetBlock.ToPoint()].HasTile)
                                continue;
                        }

                        Color color = Color.Cyan;

                        color.A = 255;

                        Vector2 drawPosition = targetBlock * 16f - Main.screenPosition + (Vector2.UnitX * 16f);
                        float rotation = -MathHelper.PiOver2;

                        Main.EntitySpriteDraw(lightTexture, drawPosition, null, color, rotation, lightTexture.Size(), 1f, SpriteEffects.None);

                        float scale = 0.75f;

                        if (Main.rand.NextBool(8))
                            Dust.NewDustDirect((targetBlock * 16f) + new Vector2(2f, 0), 2, 16, 206, Scale: scale);
                    }
                    Main.spriteBatch.End();
                }

                if (room.closedTime > 60)
                {
                    if (room.IsBossRoom && !escape)
                    {
                        bool canDraw = false;
                        for (int i = 0; i < Main.maxPlayers; i++)
                        {
                            if (Vector2.Distance(Main.player[i].Center, (room.RoomPosition + new Vector2(room.RoomDimensions.X, 0)) * 16f) < 2500f)
                            {
                                canDraw = true;
                                break;
                            }
                        }
                        if (!canDraw)
                            continue;

                        //Draw the blue wall portal
                        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                        for (float i = 0; i < room.RoomDimensions.Y; i++)
                        {
                            Vector2 targetBlock = room.RoomPosition + new Vector2(room.RoomDimensions.X - 2, i);
                            int tileType = Main.tile[targetBlock.ToPoint()].TileType;
                            if (TileID.Sets.BlockMergesWithMergeAllBlock[tileType])
                            {
                                if (Main.tile[targetBlock.ToPoint()].HasTile)
                                    continue;
                            }

                            Color color = Color.Cyan;

                            color.A = (byte)MathHelper.Clamp(MathHelper.Lerp(0, 255, (room.closedTime - 120) / 60f), 0, 255);

                            Vector2 drawPosition = targetBlock * 16f - Main.screenPosition - new Vector2(0, -16f);
                            float rotation = MathHelper.PiOver2;

                            Main.EntitySpriteDraw(lightTexture, drawPosition, null, color, rotation, lightTexture.Size(), 1f, SpriteEffects.None);

                            float scale = MathHelper.Clamp(MathHelper.Lerp(0.85f, 0.75f, (room.closedTime - 120f) / 60f), 0.75f, 0.85f);

                            if (Main.rand.NextBool((int)MathHelper.Clamp(MathHelper.Lerp(30f, 8f, (room.closedTime - 60f) / 120f), 8f, 20f)))
                                Dust.NewDustDirect((targetBlock * 16f) + new Vector2(10f, 0), 2, 16, 206, Scale: scale);
                        }
                        Main.spriteBatch.End();
                    }
                    continue;
                }

                if (room.wallActive)
                {
                    //Draw the pink borders indicating the bounds of the room
                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                    for (float side = 0; side < 2; side++)
                    {
                        for (float i = 0; i < room.RoomDimensions.X; i++)
                        {
                            Vector2 targetBlock = room.RoomPosition + new Vector2(i, side * room.RoomDimensions.Y - side);

                            int tileType = Main.tile[targetBlock.ToPoint()].TileType;
                            if (TileID.Sets.BlockMergesWithMergeAllBlock[tileType])
                            {
                                if (Main.tile[targetBlock.ToPoint()].HasTile)
                                    continue;
                            }

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
                            int tileType = Main.tile[targetBlock.ToPoint()].TileType;
                            if (TileID.Sets.BlockMergesWithMergeAllBlock[tileType])
                            {
                                if (Main.tile[targetBlock.ToPoint()].HasTile)
                                    continue;
                            }

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
                    Main.spriteBatch.End();
                }
            }
        }
        #region New Floor Effects
        public void NewFloorEffects(Room targetRoom, TerRoguelikePlayer modPlayer)
        {
            modPlayer.soulOfLenaUses = 0;
            modPlayer.lenaVisualPosition = Vector2.Zero;
            modPlayer.droneBuddyVisualPosition = Vector2.Zero;
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
            if (!SpawnManager.pendingEnemies.Any())
                return;

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            for (int i = 0; i < SpawnManager.pendingEnemies.Count; i++)
            {
                PendingEnemy enemy = SpawnManager.pendingEnemies[i];
                Texture2D texture = enemy.dummyTex;
                int frameCount = Main.npcFrameCount[enemy.NPCType];
                int height = (int)(texture.Height / frameCount);
                Color color = Color.HotPink * (0.9f);
                float completion = enemy.TelegraphDuration / (float)enemy.MaxTelegraphDuration;
                int cutoff = (int)(completion * height);
                Main.EntitySpriteDraw(texture, enemy.Position + new Vector2(0, cutoff) - Main.screenPosition, new Rectangle(0, cutoff, texture.Width, height - cutoff), color, 0f, new Vector2(texture.Width / 2f, texture.Height / frameCount / 2f), 1f, SpriteEffects.None);
            }

            Main.spriteBatch.End();
        }
        #endregion

        #region Healing Pulses
        public void UpdateHealingPulse()
        {
            if (healingPulses == null)
                healingPulses = new List<HealingPulse>();

            if (!healingPulses.Any())
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
            if (!healingPulses.Any())
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
            if (!chainList.Any())
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

            if (!chainList.Any())
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
                    Main.EntitySpriteDraw(j % 2 == 0 ? chain2Tex : chain1Tex, visualStart + position - Main.screenPosition + DrawBehindTilesOffset, null, Color.White, rotation + MathHelper.PiOver2, j % 2 == 0 ? chain2Tex.Size() * 0.5f : chain1Tex.Size() * 0.5f, 1f, SpriteEffects.None);
                }
            }
        }

        public override void PostUpdateEverything()
        {
            ParticleManager.UpdateParticles();
        }
        #endregion

        #region Attack Plan Rocket Bundles
        public void UpdateAttackPlanRocketBundles()
        {
            if (attackPlanRocketBundles == null)
                attackPlanRocketBundles = new List<AttackPlanRocketBundle>();
            if (!attackPlanRocketBundles.Any())
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
                    Projectile.NewProjectile(Projectile.GetSource_None(), bundle.Position + new Vector2(0, -32).RotatedBy(bundle.Rotation), (-Vector2.UnitY * 2.2f).RotatedBy(bundle.Rotation), ModContent.ProjectileType<PlanRocket>(), 100, 1f, bundle.Owner, -1);
                    bundle.Count--;
                    bundle.Rotation += MathHelper.PiOver4;
                }
                bundle.Time--;
            }
            attackPlanRocketBundles.RemoveAll(x => x.Time < 0);
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
