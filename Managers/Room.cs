using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerRoguelike.Managers;

namespace TerRoguelike.Managers
{
    public class Room
    {
        public virtual string Key => null;
        public virtual string Filename => null;
        public virtual int ID => -1;
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
        public virtual void AddRoomNPC(int arrayLocation, Vector2 npcSpawnPosition, int npcToSpawn, int timeUntilSpawn, int telegraphDuration, float telegraphSize = 0)
        {
            NPCSpawnPosition[arrayLocation] = npcSpawnPosition + (RoomPosition * 16f);
            NPCToSpawn[arrayLocation] = npcToSpawn;
            TimeUntilSpawn[arrayLocation] = timeUntilSpawn;
            TelegraphDuration[arrayLocation] = telegraphDuration;
            if (telegraphSize == 0)
                telegraphSize = 1f;
            TelegraphSize[arrayLocation] = telegraphSize;
        }
        public virtual void Update()
        {
            if (!awake)
                return;

            if (!initialized)
                InitializeRoom();

            roomTime++;
            for (int i = 0; i < RoomSpawnCap; i++)
            {
                if (TimeUntilSpawn[i] - roomTime == 0)
                    SpawnManager.SpawnEnemy(NPCToSpawn[i], NPCSpawnPosition[i], TelegraphDuration[i], TelegraphSize[i]);
            }
            if (roomTime > 7200)
                active = false;
        }
        public virtual void InitializeRoom()
        {
            initialized = true;
        }
    }
}
