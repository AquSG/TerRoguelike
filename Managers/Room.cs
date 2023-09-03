using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerRoguelike.Managers;
using TerRoguelike.NPCs;
using Terraria.Chat;
using TerRoguelike.Systems;
using Microsoft.Xna.Framework.Graphics;

namespace TerRoguelike.Managers
{
    public class Room
    {
        public virtual string Key => null;
        public virtual string Filename => null;
        public virtual int ID => -1;
        public int myRoom;
        public bool initialized = false;
        public bool awake = false;
        public bool active = true;
        public Vector2 RoomDimensions;
        public int roomTime;
        public const int RoomSpawnCap = 200;
        public Vector2 RoomPosition;
        public Vector2[] NPCSpawnPosition = new Vector2[RoomSpawnCap];
        public int[] NPCToSpawn = new int[RoomSpawnCap];
        public int[] TimeUntilSpawn = new int[RoomSpawnCap];
        public int[] TelegraphDuration = new int[RoomSpawnCap];
        public float[] TelegraphSize = new float[RoomSpawnCap];
        public bool[] NotSpawned = new bool[RoomSpawnCap];
        public bool anyAlive = true;
        public int roomClearGraceTime = -1;
        public int lastTelegraphDuration;
        public bool wallActive = false;
        public virtual void AddRoomNPC(int arrayLocation, Vector2 npcSpawnPosition, int npcToSpawn, int timeUntilSpawn, int telegraphDuration, float telegraphSize = 0)
        {
            NPCSpawnPosition[arrayLocation] = npcSpawnPosition + (RoomPosition * 16f);
            NPCToSpawn[arrayLocation] = npcToSpawn;
            TimeUntilSpawn[arrayLocation] = timeUntilSpawn;
            TelegraphDuration[arrayLocation] = telegraphDuration;
            if (telegraphSize == 0)
                telegraphSize = 1f;
            TelegraphSize[arrayLocation] = telegraphSize;
            NotSpawned[arrayLocation] = true;
        }
        public virtual void Update()
        {
            if (!awake)
                return;

            if (!initialized)
                InitializeRoom();

            wallActive = true;
            WallUpdate();

            roomTime++;
            for (int i = 0; i < RoomSpawnCap; i++)
            {
                if (TimeUntilSpawn[i] - roomTime == 0)
                {
                    SpawnManager.SpawnEnemy(NPCToSpawn[i], NPCSpawnPosition[i], myRoom, TelegraphDuration[i], TelegraphSize[i]);
                    lastTelegraphDuration = TelegraphDuration[i];
                    NotSpawned[i] = false;
                } 
            }
            bool cancontinue = true;
            for (int i = 0; i < RoomSpawnCap; i++)
            {
                if (NotSpawned[i] == true)
                {
                    if (TimeUntilSpawn[i] - roomTime <= 0)
                    {
                        NotSpawned[i] = false;
                        continue;
                    } 
                    cancontinue = false;
                    break;
                }
            }

            if (cancontinue)
            {
                if (roomClearGraceTime == -1)
                {
                    roomClearGraceTime += lastTelegraphDuration + 60;
                }
                if (roomClearGraceTime > 0)
                    roomClearGraceTime--;

                anyAlive = false;
                for (int npc = 0; npc < Main.maxNPCs; npc++)
                {
                    if (Main.npc[npc] == null)
                        continue;
                    if (!Main.npc[npc].active)
                        continue;

                    if (!Main.npc[npc].GetGlobalNPC<TerRoguelikeGlobalNPC>().isRoomNPC)
                        continue;

                    if (Main.npc[npc].GetGlobalNPC<TerRoguelikeGlobalNPC>().sourceRoomListID == myRoom)
                    {
                        anyAlive = true;
                        break;
                    }
                }
            }
            if (!anyAlive && roomClearGraceTime == 0)
            {
                active = false;
                Item.NewItem(Item.GetSource_NaturalSpawn(), new Rectangle((int)RoomPosition.X * 16, (int)RoomPosition.Y * 16, (int)RoomDimensions.X * 16, (int)RoomDimensions.Y * 16), ItemID.Confetti);
                Main.NewText("roomListID " + myRoom.ToString() + " completed!");
                wallActive = false;
            }
        }
        public virtual void InitializeRoom()
        {
            initialized = true;
            for (int i = 0; i < NotSpawned.Length; i++)
            {
                NotSpawned[i] = false;
            }
        }
        public void WallUpdate()
        {
            for (int playerID = 0; playerID < Main.maxPlayers; playerID++)
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
            for(int npcID = 0; npcID < Main.maxNPCs; npcID++)
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

                bool boundLeft = (npc.position.X + npc.velocity.X) < (RoomPosition.X + 1f) * 16f;
                bool boundRight = (npc.position.X + (float)npc.width + npc.velocity.X) > (RoomPosition.X - 1f + RoomDimensions.X) * 16f;
                bool boundTop = (npc.position.Y + npc.velocity.Y) < (RoomPosition.Y + 1f) * 16f;
                bool boundBottom = (npc.position.Y + (float)npc.height + npc.velocity.Y) > (RoomPosition.Y - (1f) + RoomDimensions.Y) * 16f;
                if (boundLeft)
                {
                    npc.position.X = (RoomPosition.X + 1f) * 16f;
                    npc.velocity.X = 0;
                }
                if (boundRight)
                {
                    npc.position.X = ((RoomPosition.X - 1f + RoomDimensions.X) * 16f) - (float)npc.width;
                    npc.velocity.X = 0;
                }
                if (boundTop)
                {
                    npc.position.Y = (RoomPosition.Y + 1f) * 16f;
                    npc.velocity.Y = 0;
                }
                if (boundBottom)
                {
                    npc.position.Y = ((RoomPosition.Y - (1f) + RoomDimensions.Y) * 16f) - (float)npc.height;
                    npc.velocity.Y = 0;
                }
            }
        }
    }
}
