using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using TerRoguelike.NPCs;
using TerRoguelike.Projectiles;
using TerRoguelike.Systems;
using TerRoguelike.TerPlayer;
using TerRoguelike.World;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using static TerRoguelike.Systems.MusicSystem;

namespace TerRoguelike.Managers
{
    public class Room
    {
        // base room class used by all rooms
        public virtual string Key => null; //schematic key
        public virtual string Filename => null; //schematic filename
        public int ID = -1; //ID in RoomID list
        public virtual int AssociatedFloor => -1; // what floor this room is associated with
        public virtual bool CanExitRight => false; // if room is capable of exiting right
        public virtual bool CanExitDown => false; // if room is capable of exiting down
        public virtual bool CanExitUp => false; // if room is capable of exiting up
        public virtual bool IsBossRoom => false; //if room is the end to a floor
        public virtual bool IsStartRoom => false; // if room is the start of a floor
        public virtual bool IsPillarRoom => false; // if room uses the pillar clearing mechanic (lunar floor)
        public virtual bool IsRoomVariant => false; // if room uses the schematic of another room
        public virtual bool HasTransition => false; // if room is preceeded by a transition room
        public virtual int TransitionDirection => -1; // -1 if not a transition room. 0: right, 1: Down, 2: Up
        public int myRoom; // index in RoomList
        public bool initialized = false; // whether initialize has run yet
        public bool escapeInitialized = false; // whether initialize has been run yet in the escape sequence
        public bool awake = false; // whether a player has ever stepped into this room
        public bool active = true; // whether the room has been completed or not
        public bool haltSpawns = false; // if true, prevents any more enemies from spawning in that room
        public Vector2 RoomDimensions; // dimensions of the room
        public int roomTime; // time the room has been active
        public int closedTime; // time the room has been completed
        public int waveStartTime;
        public int waveCount;
        public int currentWave;
        public int waveClearGraceTime;
        public const int RoomSpawnCap = 200;
        public Vector2 RoomPosition; //position of the room
        public Vector2 RoomPosition16
        {
            get { return RoomPosition * 16f; }
        }
        public Vector2 RoomCenter16
        {
            get { return RoomDimensions * 8f; }
        }
        public Vector2 RoomDimensions16
        {
            get { return RoomDimensions * 16f; }
        }
        public Vector2 PercentPosition(float x, float y) => new Vector2(RoomDimensions.X * 16f * x, RoomDimensions.Y * 16f * y);

        //potential NPC variables
        public Vector2[] NPCSpawnPosition = new Vector2[RoomSpawnCap];
        public int[] NPCToSpawn = new int[RoomSpawnCap];
        public int[] TimeUntilSpawn = new int[RoomSpawnCap];
        public int[] TelegraphDuration = new int[RoomSpawnCap];
        public float[] TelegraphSize = new float[RoomSpawnCap];
        public bool[] NotSpawned = new bool[RoomSpawnCap];
        public int[] AssociatedWave = new int[RoomSpawnCap];

        public bool anyAlive = true; // whether any associated npcs are alive
        public int roomClearGraceTime = -1; // time gap of 1 seconds after the last enemy has spawned where a room cannot be considered cleared to prevent any accidents happening
        public int lastTelegraphDuration; // used for roomClearGraceTime
        public bool wallActive = false; // whether the barriers of the room are active
        public virtual void AddRoomNPC(Vector2 npcSpawnPosition, int npcToSpawn, int timeUntilSpawn, int telegraphDuration, float telegraphSize = 0, int wave = 0)
        {
            for (int i = 0; i < RoomSpawnCap; i++)
            {
                if (!NotSpawned[i])
                {
                    NPCSpawnPosition[i] = npcSpawnPosition + (RoomPosition * 16f);
                    NPCToSpawn[i] = npcToSpawn;
                    TimeUntilSpawn[i] = timeUntilSpawn;
                    TelegraphDuration[i] = telegraphDuration;
                    if (telegraphSize == 0)
                        telegraphSize = 1f;
                    TelegraphSize[i] = telegraphSize;
                    NotSpawned[i] = true;
                    AssociatedWave[i] = wave;
                    if (wave > waveCount)
                        waveCount = wave;
                    break;
                }
            }
            
        }
        public virtual void Update()
        {
            if (!StartCondition()) // not been touched yet? return
                return;

            if (!initialized) // initialize the room
                InitializeRoom();

            if (closedTime <= 60 && (TerRoguelikeWorld.escapeTime > TerRoguelikeWorld.escapeTimeSet - 180 || !TerRoguelikeWorld.escape)) //wall is visually active up to 1 second after room clear
                wallActive = true;

            if (!active) // room done, closed time increments
            {
                closedTime++;
                return;
            }
            WallUpdate(); // update wall logic
            PlayerItemsUpdate(); // update items from all players

            roomTime++; //time room is active

            if (!haltSpawns)
            {
                for (int i = 0; i < RoomSpawnCap; i++)
                {
                    if (AssociatedWave[i] > currentWave || !NotSpawned[i])
                        continue;

                    if (TimeUntilSpawn[i] - roomTime + waveStartTime <= 0) //spawn pending enemy that has reached it's time
                    {
                        SpawnManager.SpawnEnemy(NPCToSpawn[i], NPCSpawnPosition[i], myRoom, TelegraphDuration[i], TelegraphSize[i]);
                        lastTelegraphDuration = TelegraphDuration[i];
                        waveClearGraceTime = roomTime;
                        NotSpawned[i] = false;
                    }
                }
            }
            
            // if there is still an enemy yet to be spawned, do not continue with room clear logic
            bool cancontinue = roomTime - waveClearGraceTime > 60;
            bool encourageNextWave = false;
            if (!haltSpawns)
            {
                if (currentWave < waveCount && roomTime - waveClearGraceTime > lastTelegraphDuration + 60)
                {
                    encourageNextWave = true;

                    for (int j = 0; j < RoomSpawnCap; j++)
                    {
                        if (NotSpawned[j] == true && AssociatedWave[j] <= currentWave)
                        {
                            encourageNextWave = false;
                            break;
                        }
                    }
                    if (encourageNextWave)
                    {
                        for (int i = 0; i < Main.maxNPCs; i++)
                        {
                            NPC npc = Main.npc[i];
                            if (npc == null)
                                continue;
                            if (!npc.active)
                                continue;

                            TerRoguelikeGlobalNPC modNPC = npc.GetGlobalNPC<TerRoguelikeGlobalNPC>();

                            if (!modNPC.isRoomNPC)
                                continue;

                            if (modNPC.sourceRoomListID == myRoom && !modNPC.hostileTurnedAlly)
                            {
                                encourageNextWave = false;
                                break;
                            }
                        }
                    }
                }
                for (int i = 0; i < RoomSpawnCap; i++)
                {
                    if (NotSpawned[i] == true || currentWave < waveCount)
                    {
                        cancontinue = false;
                        break;
                    }
                }
            }

            if (encourageNextWave)
            {
                currentWave++;
                waveStartTime = roomTime;
                waveClearGraceTime = roomTime;
            }
            if (cancontinue)
            {
                // start checking if any npcs in the world are active and associated with this room
                if (roomClearGraceTime == -1)
                {
                    roomClearGraceTime += lastTelegraphDuration + 60;
                }
                if (roomClearGraceTime > 0)
                    roomClearGraceTime--;

                anyAlive = false;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc == null)
                        continue;
                    if (!npc.active)
                        continue;

                    TerRoguelikeGlobalNPC modNPC = npc.GetGlobalNPC<TerRoguelikeGlobalNPC>();

                    if (!modNPC.isRoomNPC)
                        continue;

                    if (modNPC.sourceRoomListID == myRoom && !modNPC.hostileTurnedAlly)
                    {
                        anyAlive = true;
                        break;
                    }
                }
            }
            if (ClearCondition()) // all associated enemies are gone. room cleared.
            {
                active = false;
                RoomClearReward();
            }
        }
        public virtual void InitializeRoom()
        {
            initialized = true;
            //sanity check for all npc slots
            for (int i = 0; i < NotSpawned.Length; i++)
            {
                NotSpawned[i] = false;
            }
        }
        public void WallUpdate()
        {
            if (!wallActive)
                return;

            for (int playerID = 0; playerID < Main.maxPlayers; playerID++) // keep players in the fucking room
            {
                var player = Main.player[playerID];
                bool boundLeft = (player.position.X + player.velocity.X) < (RoomPosition.X + 1f) * 16f;
                bool boundRight = (player.position.X + (float)player.width + player.velocity.X) > (RoomPosition.X - 1f + RoomDimensions.X) * 16f;
                bool boundTop = (player.position.Y + player.velocity.Y) < (RoomPosition.Y + 1f) * 16f;
                bool boundBottom = (player.position.Y + (float)player.height + player.velocity.Y) > (RoomPosition.Y - (1f) + RoomDimensions.Y) * 16f;
                if (boundLeft)
                {
                    player.position.X = (RoomPosition.X + 1f) * 16f;
                    player.velocity.X = 0;
                }
                if (boundRight)
                {
                    player.position.X = ((RoomPosition.X - 1f + RoomDimensions.X) * 16f) - (float)player.width;
                    player.velocity.X = 0;
                }
                if (boundTop)
                {
                    player.position.Y = (RoomPosition.Y + 1f) * 16f;
                    player.velocity.Y = 0;
                }
                if (boundBottom)
                {
                    player.position.Y = ((RoomPosition.Y - (1f) + RoomDimensions.Y) * 16f) - (float)player.height;
                    player.velocity.Y = 0;
                }

            }
            for (int npcID = 0; npcID < Main.maxNPCs; npcID++) // keep npcs in the fucking room
            {
                var npc = Main.npc[npcID];
                if (npc == null)
                    continue;
                if (!npc.active)
                    continue;
                if (!npc.GetGlobalNPC<TerRoguelikeGlobalNPC>().isRoomNPC)
                    continue;
                if (npc.GetGlobalNPC<TerRoguelikeGlobalNPC>().sourceRoomListID != myRoom)
                    continue;
                if (npc.GetGlobalNPC<TerRoguelikeGlobalNPC>().IgnoreRoomWallCollision)
                    continue;

                bool boundLeft = (npc.position.X + npc.velocity.X) < (RoomPosition.X + 1f) * 16f;
                bool boundRight = (npc.position.X + (float)npc.width + npc.velocity.X) > (RoomPosition.X - 1f + RoomDimensions.X) * 16f;
                bool boundTop = (npc.position.Y + npc.velocity.Y) < (RoomPosition.Y + 1f) * 16f;
                bool boundBottom = (npc.position.Y + (float)npc.height + npc.velocity.Y) > (RoomPosition.Y - (1f) + RoomDimensions.Y) * 16f;
                if (boundLeft)
                {
                    npc.position.X = (RoomPosition.X + 1f) * 16f;
                    npc.velocity.X = 0;
                    npc.collideX = true;
                }
                if (boundRight)
                {
                    npc.position.X = ((RoomPosition.X - 1f + RoomDimensions.X) * 16f) - (float)npc.width;
                    npc.velocity.X = 0;
                    npc.collideX = true;
                }
                if (boundTop)
                {
                    npc.position.Y = (RoomPosition.Y + 1f) * 16f;
                    npc.velocity.Y = 0;
                    npc.collideY = true;
                }
                if (boundBottom)
                {
                    npc.position.Y = ((RoomPosition.Y - (1f) + RoomDimensions.Y) * 16f) - (float)npc.height;
                    npc.velocity.Y = 0;
                    npc.collideY = true;
                }
            }
        }
        public Rectangle CheckRectWithWallCollision(Rectangle rect)
        {
            bool boundLeft = (rect.X) < (RoomPosition.X + 1f) * 16f;
            bool boundRight = (rect.X + rect.Width) > (RoomPosition.X - 1f + RoomDimensions.X) * 16f;
            bool boundTop = (rect.Y) < (RoomPosition.Y + 1f) * 16f;
            bool boundBottom = (rect.Y + rect.Height) > (RoomPosition.Y - (1f) + RoomDimensions.Y) * 16f;
            if (boundLeft)
            {
                rect.X = (int)((RoomPosition.X + 1f) * 16f);
            }
            if (boundRight)
            {
                rect.X = (int)((RoomPosition.X - 1f + RoomDimensions.X) * 16f) - rect.Width;
            }
            if (boundTop)
            {
                rect.Y = (int)((RoomPosition.Y + 1f) * 16f);
            }
            if (boundBottom)
            {
                rect.Y = (int)((RoomPosition.Y - (1f) + RoomDimensions.Y) * 16f) - rect.Height;
            }

            return rect;
        }
        public virtual void RoomClearReward()
        {
            if (TerRoguelikeWorld.escape)
                return;

            if (Main.player[Main.myPlayer].GetModPlayer<TerRoguelikePlayer>().currentFloor.Stage == 4 && IsBossRoom)
                SetMusicMode(MusicStyle.Silent);

            ClearRockets();

            // reward. boss rooms give higher tiers.
            int chance = Main.rand.Next(1, 101);
            int itemType;
            int itemTier;

            if (IsBossRoom)
            {
                if (chance <= 80)
                {
                    itemType = ItemManager.GiveUncommon(false);
                    itemTier = 1;
                }
                else
                {
                    itemType = ItemManager.GiveRare(false);
                    itemTier = 2;
                }
                SpawnManager.SpawnItem(itemType, FindAirNearRoomCenter(), itemTier, 75, 0.5f);
                return;
            }

            if (ItemManager.RoomRewardCooldown > 0)
            {
                ItemManager.RoomRewardCooldown--;
                return;
            }
                
            if (chance <= 80)
            {
                itemType = ItemManager.GiveCommon();
                itemTier = 0;
            }
            else if (chance <= 98)
            {
                itemType = ItemManager.GiveUncommon();
                itemTier = 1;
            }
            else
            {
                itemType = ItemManager.GiveRare();
                itemTier = 2;
            }
            SpawnManager.SpawnItem(itemType, FindAirNearRoomCenter(), itemTier, 75, 0.5f);
        }
        public void PlayerItemsUpdate()
        {
            if (TerRoguelikeWorld.escape)
                return;

            int totalAutomaticDefibrillator = 0;
            Vector2 roomCenter = new Vector2(RoomPosition.X + (RoomDimensions.X * 0.5f), RoomPosition.Y + (RoomDimensions.Y * 0.5f)) * 16f;

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (player == null)
                    continue;
                if (!player.active)
                    continue;

                TerRoguelikePlayer modPlayer = player.GetModPlayer<TerRoguelikePlayer>();
                totalAutomaticDefibrillator += modPlayer.automaticDefibrillator;
            }

            if (totalAutomaticDefibrillator > 0)
            {
                int healTime = (int)(1500 * (4 / (float)(totalAutomaticDefibrillator + 4)));
                if (healTime <= 0)
                    healTime = 1;
                if (roomTime % healTime == 0 && roomTime > 0)
                {
                    RoomSystem.healingPulses.Add(new HealingPulse(roomCenter));
                }
            }

            if (roomTime == 30)
            {
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player player = Main.player[i];
                    if (player == null)
                        continue;
                    if (!player.active)
                        continue;

                    TerRoguelikePlayer modPlayer = player.GetModPlayer<TerRoguelikePlayer>();
                    if (modPlayer.attackPlan <= 0)
                        continue;

                    int rocketCount = 4 + (4 * modPlayer.attackPlan);
                    RoomSystem.attackPlanRocketBundles.Add(new AttackPlanRocketBundle(roomCenter, rocketCount, player.whoAmI, myRoom));
                }
            }
        }
        public void ClearRockets()
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.type == ModContent.ProjectileType<PlanRocket>() || proj.type == ModContent.ProjectileType<Missile>())
                {
                    proj.timeLeft = 60;
                }
            }
        }
        public virtual bool ClearCondition()
        {
            return !anyAlive && roomClearGraceTime == 0;
        }
        public virtual bool StartCondition()
        {
            return awake;
        }
        public Vector2 FindAirNearRoomCenter()
        {
            Point centerTile = new Point((int)(RoomPosition.X + (RoomDimensions.X * 0.5f)), (int)(RoomPosition.Y + (RoomDimensions.Y * 0.5f)));
            if (!ParanoidTileRetrieval(centerTile.X, centerTile.Y).IsTileSolidGround(true))
                return RoomPosition16 + RoomCenter16;

            for (int i = 0; i < 200; i++)
            {
                int direction = i % 4 > 1 ? (i % 2 == 0 ? 2 : -2) : (i % 2 == 0 ? 1 : -1);
                if (Math.Abs(direction) == 2)
                {
                    direction = Math.Sign(direction);
                    if (ParanoidTileRetrieval(centerTile.X, centerTile.Y + ((i / 4) * direction)).IsTileSolidGround(true))
                        continue;

                    return new Vector2(centerTile.X, centerTile.Y + ((i / 4) * direction)) * 16f + new Vector2(8, 8);
                }
                else
                {
                    if (ParanoidTileRetrieval(centerTile.X + ((i / 4) * direction), centerTile.Y).IsTileSolidGround(true))
                        continue;

                    return new Vector2(centerTile.X + ((i / 4) * direction), centerTile.Y) * 16f + new Vector2(8, 8);
                }
            }

            return RoomPosition16 + RoomCenter16;
        }
    }
}
