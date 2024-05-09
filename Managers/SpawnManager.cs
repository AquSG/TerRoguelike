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
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using TerRoguelike.World;
using TerRoguelike.Particles;
using static TerRoguelike.Systems.RoomSystem;
using static TerRoguelike.Utilities.TerRoguelikeUtils;

namespace TerRoguelike.Managers
{
    public class SpawnManager
    {
        //Custom classes being updated so that projectile/npc limitations don't get in the way of crucial gameplay elements
        public static List<PendingEnemy> pendingEnemies = new List<PendingEnemy>();
        public static List<PendingItem> pendingItems = new List<PendingItem>();
        public static void UpdateSpawnManager()
        {
            UpdatePendingEnemies();
            UpdatePendingItems();
        }

        public static void SpawnEnemy(int npcType, Vector2 position, int roomListID, int telegraphDuration, float telegraphSize = 0.45f)
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

                Texture2D dummyTex = enemy.dummyTex;
                int width = dummyTex.Width;
                int height = dummyTex.Height / Main.npcFrameCount[enemy.NPCType];
                float completion = enemy.TelegraphDuration / (float)enemy.MaxTelegraphDuration;
                int y = (int)(completion * height);
                Vector2 topLeftPos = enemy.Position + new Vector2(-width * 0.5f, -height * 0.5f);
                int time = enemy.TelegraphDuration < 15 ? 13 : 20;
                for (int i = 0; i < width; i += 100)
                {
                    int x = Main.rand.Next(width);
                    Vector2 pos = topLeftPos + new Vector2(x, y);
                    ParticleManager.AddParticle(new Square(pos, Vector2.Zero, time, Color.HotPink, new Vector2((4f + Main.rand.NextFloat(-0.5f, 0.5f)) * enemy.TelegraphSize), 0, 0.96f, time, true));
                }

                enemy.TelegraphDuration--;
                if (enemy.RoomListID != -1)
                {
                    if (RoomList[enemy.RoomListID].bossDead)
                    {
                        enemy.spent = true;
                        continue;
                    }
                }
                if (enemy.TelegraphDuration <= 0 && !enemy.spent)
                {
                    NPC dummyNpc = new NPC();
                    dummyNpc.type = enemy.NPCType;
                    dummyNpc.SetDefaults(dummyNpc.type);
                    SpawnNPCTerRoguelike(NPC.GetSource_NaturalSpawn(), new Vector2(enemy.Position.X, enemy.Position.Y + (dummyNpc.height / 2f)), enemy.NPCType, enemy.RoomListID);

                    enemy.spent = true;
                }
            }
            pendingEnemies.RemoveAll(enemy => enemy.spent);
        }
        public static NPC SpawnNPCTerRoguelike(IEntitySource source, Vector2 position,  int type, int roomListID = -1)
        {
            int spawnedNpc = NPC.NewNPC(source, (int)position.X, (int)position.Y, type);
            NPC npc = Main.npc[spawnedNpc];
            TerRoguelikeGlobalNPC modNpc = npc.ModNPC();
            if (roomListID > -1)
            {
                modNpc.isRoomNPC = true;
                modNpc.sourceRoomListID = roomListID;
            }
            return npc;
        }
        public static double healthScalingMultiplier => Math.Pow(1.2d, TerRoguelikeWorld.currentStage);
        public static double damageScalingMultiplier => Math.Pow(1.10d, TerRoguelikeWorld.currentStage);
        public static void ApplyNPCDifficultyScaling(NPC npc, TerRoguelikeGlobalNPC modNpc)
        {
            double healthMultiplier = healthScalingMultiplier;
            double damageMultiplier = damageScalingMultiplier;
            npc.lifeMax = (int)(modNpc.baseMaxHP * healthMultiplier);
            npc.life = npc.lifeMax;
            npc.damage = (int)(modNpc.baseDamage * damageMultiplier);
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
        public int MaxTelegraphDuration;
        public float TelegraphSize;
        public bool spent = false;
        public Texture2D dummyTex;
        public PendingEnemy(int npcType, Vector2 position, int roomListID, int telegraphDuration, float telegraphSize = 0.5f)
        {
            NPCType = npcType;
            Position = position;
            RoomListID = roomListID;
            TelegraphDuration = telegraphDuration;
            MaxTelegraphDuration = telegraphDuration;
            TelegraphSize = telegraphSize;
            dummyTex = TextureAssets.Npc[NPCType].Value;
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
