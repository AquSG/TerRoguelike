using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Terraria.ID;
using TerRoguelike.NPCs;

namespace TerRoguelike.Managers
{
    public class SpawnManager
    {
        public static List<PendingEnemy> pendingEnemies = new List<PendingEnemy>();
        public static List<PendingItem> pendingItems = new List<PendingItem>();
        public static void UpdateSpawnManager()
        {
            UpdatePendingEnemies();
            UpdatePendingItems();
        }

        public static void SpawnEnemy(int npcType, Vector2 position, int roomListID, int telegraphDuration, float telegraphSize = 1f)
        {
            pendingEnemies.Add(new PendingEnemy(npcType, position, roomListID, telegraphDuration, telegraphSize));
        }
        public static void UpdatePendingEnemies()
        {
            if (!pendingEnemies.Any())
                return;

            int loopcount = -1;
            foreach (PendingEnemy enemy in pendingEnemies)
            {
                loopcount++;
                Dust.NewDustDirect(enemy.Position - new Vector2(15f * enemy.TelegraphSize, 15f * enemy.TelegraphSize), (int)(30 * enemy.TelegraphSize), (int)(30 * enemy.TelegraphSize), DustID.CrystalPulse, Scale: 0.5f);
                enemy.TelegraphDuration--;
                if (enemy.TelegraphDuration <= 0)
                {
                    enemy.TelegraphSize *= 2f;
                    int spawnedNpc = NPC.NewNPC(NPC.GetSource_NaturalSpawn(), (int)enemy.Position.X, (int)enemy.Position.Y, enemy.NPCType);
                    Main.npc[spawnedNpc].GetGlobalNPC<TerRoguelikeGlobalNPC>().isRoomNPC = true;
                    Main.npc[spawnedNpc].GetGlobalNPC<TerRoguelikeGlobalNPC>().sourceRoomListID = enemy.RoomListID;
                    for (int i = 0; i < 15; i++)
                    {
                        Dust dust = Dust.NewDustDirect(enemy.Position - new Vector2(15f * enemy.TelegraphSize, 15f * enemy.TelegraphSize), (int)(30 * enemy.TelegraphSize), (int)(30 * enemy.TelegraphSize), DustID.CrystalPulse, Scale: 1f);
                        dust.noGravity = true;
                    }
                    enemy.spent = true;
                }
            }
            pendingEnemies.RemoveAll(enemy => enemy.spent);
        }
        public static void SpawnItem(int itemType, Vector2 position, int itemTier, int telegraphDuration, float telegraphSize = 0.5f)
        {
            pendingItems.Add(new PendingItem(itemType, position, itemTier, telegraphDuration, telegraphSize));
        }
        public static void UpdatePendingItems()
        {
            if (!pendingItems.Any())
                return;

            int loopcount = -1;
            foreach (PendingItem item in pendingItems)
            {
                loopcount++;

                if (item.dustID == -1)
                {
                    if (item.ItemTier == 0)
                    {
                        item.dustID = DustID.Firework_Blue;
                    }
                    else if (item.ItemTier == 1)
                    {
                        item.dustID = DustID.Firework_Green;
                    }
                    else
                    {
                        item.dustID = DustID.Firework_Red;
                    }
                }
                
                Dust.NewDustDirect(item.Position - new Vector2(15f * item.TelegraphSize, 15f * item.TelegraphSize), (int)(30 * item.TelegraphSize), (int)(30 * item.TelegraphSize), item.dustID, Scale: 0.5f);
                item.TelegraphDuration--;
                if (item.TelegraphDuration == 0)
                {
                    item.TelegraphSize *= 2f;
                    int spawnedItem = Item.NewItem(Item.GetSource_NaturalSpawn(), new Rectangle((int)item.Position.X, (int)item.Position.Y, 1, 1), item.ItemType);
                    for (int i = 0; i < 15; i++)
                    {
                        Dust dust = Dust.NewDustDirect(item.Position - new Vector2(15f * item.TelegraphSize, 15f * item.TelegraphSize), (int)(30 * item.TelegraphSize), (int)(30 * item.TelegraphSize), item.dustID, Scale: 0.75f);
                        dust.noGravity = true;
                    }
                    item.spent = true;
                }
            }
            pendingItems.RemoveAll(item => item.spent);
        }
    }

    public class PendingEnemy
    {
        public int NPCType;
        public Vector2 Position;
        public int RoomListID;
        public int TelegraphDuration;
        public float TelegraphSize;
        public bool spent = false;
        public PendingEnemy(int npcType, Vector2 position, int roomListID, int telegraphDuration, float telegraphSize = 0.5f)
        {
            NPCType = npcType;
            Position = position;
            RoomListID = roomListID;
            TelegraphDuration = telegraphDuration;
            TelegraphSize = telegraphSize;
        }
    }
    public class PendingItem
    {
        public int ItemType;
        public Vector2 Position;
        public int ItemTier;
        public int TelegraphDuration;
        public float TelegraphSize;
        public int dustID = -1;
        public bool spent = false;
        public PendingItem(int itemType, Vector2 position, int itemTier, int telegraphDuration, float telegraphSize = 0.5f)
        {
            ItemType = itemType;
            Position = position;
            ItemTier = itemTier;
            TelegraphDuration = telegraphDuration;
            TelegraphSize = telegraphSize;
        }
    }
}
