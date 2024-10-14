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
using static TerRoguelike.Managers.ItemManager;
using static TerRoguelike.NPCs.TerRoguelikeGlobalNPC;
using Microsoft.Xna.Framework.Audio;
using Terraria.Audio;
using System.Threading;
using TerRoguelike.Items.Rare;
using TerRoguelike.MainMenu;

namespace TerRoguelike.Managers
{
    public class SpawnManager
    {
        //Custom classes being updated so that projectile/npc limitations don't get in the way of crucial gameplay elements
        public static List<PendingEnemy> pendingEnemies = [];
        public static List<PendingItem> pendingItems = [];
        public static List<PendingItem> specialPendingItems = [];

        public static List<int> trashList = [ItemID.OldShoe, ItemID.FishingSeaweed, ItemID.TinCan];
        public static void UpdateSpawnManager()
        {
            UpdatePendingEnemies();
            UpdatePendingItems();
        }

        public static void SpawnEnemy(int npcType, Vector2 position, int roomListID, int telegraphDuration, float telegraphSize = 0.45f, EliteVars eliteVariables = null)
        {
            eliteVariables ??= new EliteVars();
            pendingEnemies.Add(new PendingEnemy(npcType, position, roomListID, telegraphDuration, telegraphSize, eliteVariables));
        }
        public static void UpdatePendingEnemies()
        {
            if (pendingEnemies.Count == 0)
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
                    ParticleManager.AddParticle(new Square(pos, Vector2.Zero, time, GetEliteColor(enemy.eliteVars), new Vector2((4f + Main.rand.NextFloat(-0.5f, 0.5f)) * enemy.TelegraphSize), 0, 0.96f, time, true));
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
                    NPC spawnedNPC = SpawnNPCTerRoguelike(NPC.GetSource_NaturalSpawn(), new Vector2(enemy.Position.X, enemy.Position.Y + (dummyNpc.height / 2f)), enemy.NPCType, enemy.RoomListID, enemy.eliteVars);

                    enemy.spent = true;
                }
            }
            pendingEnemies.RemoveAll(enemy => enemy.spent);
        }
        public static Color GetEliteColor(EliteVars eliteVars)
        {
            if (eliteVars.tainted)
                return Color.Yellow;
            if (eliteVars.slugged)
                return Color.Purple;
            if (eliteVars.burdened)
                return Color.Lerp(Color.Teal, Color.Cyan, 0.3f);

            return Color.HotPink;
        }
        public static NPC SpawnNPCTerRoguelike(IEntitySource source, Vector2 position,  int type, int roomListID = -1, EliteVars eliteVariables = null)
        {
            int spawnedNpc = NPC.NewNPC(source, (int)position.X, (int)position.Y, type);
            NPC npc = Main.npc[spawnedNpc];
            TerRoguelikeGlobalNPC modNpc = npc.ModNPC();

            eliteVariables ??= new EliteVars();
            modNpc.eliteVars = eliteVariables;
            string giveName = "";
            if (modNpc.eliteVars.tainted)
            {
                giveName += "Tainted ";
                npc.damage = (int)(npc.damage * 1.5f);
            }
            if (modNpc.eliteVars.slugged)
            {
                giveName += "Slugged ";

                npc.damage = (int)(npc.damage * 2f);
            }
            if (modNpc.eliteVars.burdened)
            {
                giveName += "Burdened ";
                npc.damage = (int)(npc.damage * 1.5f);
            }
            if (giveName.Length > 0)
            {
                npc.GivenName = giveName + npc.GivenName + npc.TypeName;
            }
            if (npc.damage < 0)
                npc.damage = int.MaxValue;

            if (roomListID > -1)
            {
                modNpc.isRoomNPC = true;
                modNpc.sourceRoomListID = roomListID;
            }
            return npc;
        }
        public static double healthScalingMultiplier => Math.Pow(1.2d, TerRoguelikeWorld.currentStageForScaling);
        public static double damageScalingMultiplier => Math.Pow(1.10d, TerRoguelikeWorld.currentStageForScaling);
        public static void ApplyNPCDifficultyScaling(NPC npc, TerRoguelikeGlobalNPC modNpc)
        {
            double healthMultiplier = healthScalingMultiplier;
            if (modNpc.TerRoguelikeBoss && TerRoguelikeMenu.SunnyDayActive)
                healthMultiplier *= 0.8d;
            double damageMultiplier = damageScalingMultiplier;
            npc.lifeMax = (int)(modNpc.baseMaxHP * healthMultiplier);
            npc.damage = (int)(modNpc.baseDamage * damageMultiplier);

            if (npc.lifeMax < 1)
                npc.lifeMax = int.MaxValue;
            if (npc.damage < 0)
                npc.damage = int.MaxValue;

            npc.life = npc.lifeMax;
        }
        public static void SpawnItem(int itemType, Vector2 position, int itemTier, int telegraphDuration, float telegraphSize = 0.5f)
        {
            if (TerRoguelikeMenu.RuinedMoonActive && Main.rand.NextFloat() < 0.15f)
                itemType = trashList[Main.rand.Next(trashList.Count)];

            pendingItems.Add(new PendingItem(itemType, position, itemTier, telegraphDuration, telegraphSize));
            SoundEngine.PlaySound(ItemSpawn with { Volume = 0.12f, Variants = [itemTier], MaxInstances = 10 }, position);
        }
        public static void UpdatePendingItems()
        {
            UpdateSpecialPendingItems();

            if (pendingItems.Count == 0)
                return;

            int loopcount = -1;
            foreach (PendingItem item in pendingItems)
            {
                loopcount++;

                if (item.particleTier == -1)
                {
                    if (item.ItemTier == 0)
                    {
                        item.particleTier = DustID.Firework_Blue;
                    }
                    else if (item.ItemTier == 1)
                    {
                        item.particleTier = DustID.Firework_Green;
                    }
                    else
                    {
                        item.particleTier = DustID.Firework_Red;
                    }
                }
                
                Dust.NewDustDirect(item.Position - new Vector2(15f * item.TelegraphSize, 15f * item.TelegraphSize), (int)(30 * item.TelegraphSize), (int)(30 * item.TelegraphSize), item.particleTier, Scale: 0.5f);
                item.TelegraphDuration--;
                if (item.TelegraphDuration == 0)
                {
                    item.TelegraphSize *= 2f;
                    int spawnedItem = Item.NewItem(Item.GetSource_NaturalSpawn(), new Rectangle((int)item.Position.X, (int)item.Position.Y, 1, 1), item.ItemType);
                    SoundEngine.PlaySound(ItemLand with { Volume = 0.2f, Variants = [item.ItemTier], MaxInstances = 10 }, item.Position);
                    for (int i = 0; i < 15; i++)
                    {
                        Dust dust = Dust.NewDustDirect(item.Position - new Vector2(15f * item.TelegraphSize, 15f * item.TelegraphSize), (int)(30 * item.TelegraphSize), (int)(30 * item.TelegraphSize), item.particleTier, Scale: 0.75f);
                        dust.noGravity = true;
                    }
                    item.spent = true;
                }
            }
            pendingItems.RemoveAll(item => item.spent);
        }
        public static void UpdateSpecialPendingItems()
        {
            if (specialPendingItems.Count <= 0)
                return;

            int loopcount = -1;
            foreach (PendingItem item in specialPendingItems)
            {
                loopcount++;

                var color = (ItemTier)item.ItemTier switch
                {
                    ItemTier.Uncommon => new Color(0.2f, 1f, 0.2f),
                    ItemTier.Rare => new Color(1f, 0.2f, 0.2f),
                    _ => new Color(0.1f, 0.5f, 1f),
                };
                item.TelegraphDuration--;

                if (item.TelegraphDuration > 120)
                {
                    if (item.TelegraphDuration == 150)
                    {
                        int particleCount = 8 + (3 * item.ItemTier);
                        for (int i = 0; i < particleCount; i++)
                        {
                            float randRot = Main.rand.NextFloat(MathHelper.TwoPi);
                            randRot = randRot.AngleLerp(-MathHelper.PiOver2, 0.56f);
                            Vector2 offset = randRot.ToRotationVector2() * (float)Math.Pow(Main.rand.NextFloat(0.5f, 0.9f), 3) * 4;
                            SpawnParticle(item.Position + offset, randRot, offset.Length() * 0.5f, color, Main.rand.Next(20, 61), new Vector2(Main.rand.NextFloat(0.7f, 0.9f) * 0.2f));
                        }
                        if (item.Sound)
                            SoundEngine.PlaySound(SoundID.Item176 with { Volume = 1f, MaxInstances = 3 }, item.Position);
                    }
                }
                else // item has been dunked in the soup. proceed.
                {
                    if (item.TelegraphDuration == 120)
                    {
                        if (item.Sound)
                        {
                            SoundEngine.PlaySound(ItemSpawn with { Volume = 0.2f, Variants = [item.ItemTier], MaxInstances = 10 }, item.Position);
                        }
                    }
                    item.Velocity.Y += item.Gravity;
                    if (item.Velocity.Y > 6)
                        item.Velocity.Y = 6;
                    Vector2 futurePos = item.Position + item.Velocity;
                    Vector2 futurePosColliding = TileCollidePositionInLine(item.Position, futurePos);
                    if (futurePos != futurePosColliding)
                    {
                        item.TelegraphDuration = 0;
                    }
                    ParticleManager.AddParticle(new Square(
                        item.Position + (futurePosColliding - item.Position) * 0.5f, Vector2.Zero, 30, color, new Vector2(3f, 1f), item.Velocity.ToRotation(), 0.96f, 30, false));

                    item.Position = futurePosColliding;
                    ParticleManager.AddParticle(new BallOutlined(
                        item.Position, Vector2.Zero, 2, color, Color.White * 0.75f, new Vector2(0.12f), 6, 0, 0.96f, 0));

                    if (item.TelegraphDuration == 0)
                    {
                        int particleCount = 15 + (5 * item.ItemTier);
                        for (int i = 0; i < particleCount; i++)
                        {
                            float randRot = Main.rand.NextFloat(MathHelper.TwoPi);
                            Vector2 offset = randRot.ToRotationVector2() * (float)Math.Pow(Main.rand.NextFloat(0.5f, 0.9f), 3) * 5;
                            SpawnParticle(item.Position + offset, offset.ToRotation(), offset.Length() * 0.5f, color, Main.rand.Next(20, 61), new Vector2(Main.rand.NextFloat(0.7f, 0.9f) * 0.2f));
                        }
                        item.Position.Y -= 10;
                        int spawnedItem = Item.NewItem(Item.GetSource_NaturalSpawn(), new Rectangle((int)item.Position.X, (int)item.Position.Y, 1, 1), item.ItemType);
                        if (item.Sound)
                            SoundEngine.PlaySound(ItemLand with { Volume = 0.2f, Variants = [item.ItemTier], MaxInstances = 10 }, item.Position);
                        Main.item[spawnedItem].noGrabDelay = 24;
                        item.spent = true;
                    }
                }
            }

            void SpawnParticle(Vector2 position, float rotation, float speed, Color color, int time, Vector2 scale)
            {
                ParticleManager.AddParticle(new BallOutlined(
                    position, rotation.ToRotationVector2() * speed, time, color, Color.White * 0.5f, scale, 6, rotation, 0.96f, time));
            }

            specialPendingItems.RemoveAll(item => item.spent);
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
        public EliteVars eliteVars;
        public PendingEnemy(int npcType, Vector2 position, int roomListID, int telegraphDuration, float telegraphSize = 0.5f, EliteVars EliteVars = null)
        {
            NPCType = npcType;
            Position = position;
            RoomListID = roomListID;
            TelegraphDuration = telegraphDuration;
            MaxTelegraphDuration = telegraphDuration;
            TelegraphSize = telegraphSize;
            dummyTex = TextureAssets.Npc[NPCType].Value;
            EliteVars ??= new EliteVars();
            eliteVars = EliteVars;
        }
    }
    public class PendingItem
    {
        public int ItemType;
        public Vector2 Position;
        public int ItemTier;
        public int TelegraphDuration;
        public float TelegraphSize;
        public int particleTier = -1;
        public Vector2 Velocity;
        public float Gravity;
        public bool spent = false;
        public bool Sound;
        public int setTelegraphDuration;
        public Vector2 displayInterpolationStartPos;
        public int itemSacrificeType;
        public PendingItem(int itemType, Vector2 position, int itemTier, int telegraphDuration, float telegraphSize = 0.5f)
        {
            ItemType = itemType;
            Position = position;
            ItemTier = itemTier;
            TelegraphDuration = setTelegraphDuration = telegraphDuration;
            TelegraphSize = telegraphSize;
            Velocity = Vector2.Zero;
            Gravity = 0;
            Sound = false;
            displayInterpolationStartPos = Vector2.Zero;
            itemSacrificeType = 0;
        }
        public PendingItem(int itemType, Vector2 position, ItemTier itemTier, int telegraphDuration, Vector2 velocity, float gravity, Vector2 start, int ItemSacrificeType, bool sound = true)
        {
            ItemType = itemType;
            Position = position;
            ItemTier = (int)itemTier;
            TelegraphDuration = setTelegraphDuration = 120 + telegraphDuration;
            Velocity = velocity;
            Gravity = gravity;
            TelegraphSize = 1;
            Sound = sound;
            displayInterpolationStartPos = Main.LocalPlayer.Top;
            if (Sound)
            {
                SoundEngine.PlaySound(SoundID.Item1 with { Volume = 1f, Pitch = -0.24f }, displayInterpolationStartPos);
            }
            itemSacrificeType = ItemSacrificeType;
        }
        public void DrawPreDunkInSoup()
        {
            int time = (setTelegraphDuration - TelegraphDuration);
            int endTime = setTelegraphDuration - 150;


            if (TelegraphDuration > 150)
            {
                float completion = (time + 1) / (float)endTime;
                completion = 1f - (float)Math.Pow(1f - completion, 0.66f);
                Vector2 sacrificeDrawPos = displayInterpolationStartPos + ((Position - displayInterpolationStartPos) * completion);
                sacrificeDrawPos.Y -= (1f - (float)Math.Pow(Math.Abs(completion - 0.5f) * -2, 2)) * 32;
                Vector2 itemDisplayDimensions = new Vector2(36);
                Texture2D itemTex;
                float scale;
                Rectangle rect;
                Main.GetItemDrawFrame(itemSacrificeType, out itemTex, out rect);
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
                if (time < 12)
                {
                    scale *= MathHelper.SmoothStep(0, 1, MathHelper.Clamp(time / 9f, 0, 1));
                }
                else if (completion > 0.8f)
                {
                    scale *= 1f - ((completion - 0.8f) / 0.4f);
                }

                Main.EntitySpriteDraw(itemTex, (sacrificeDrawPos - Main.screenPosition), rect, Color.White * 0.9f, 0f, rect.Size() * 0.5f, scale, SpriteEffects.None, 0);
            }
        }
    }
}
