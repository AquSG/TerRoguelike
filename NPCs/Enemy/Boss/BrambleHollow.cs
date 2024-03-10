using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TerRoguelike.Managers;
using TerRoguelike.Particles;
using TerRoguelike.Projectiles;
using TerRoguelike.Systems;
using static TerRoguelike.Managers.TextureManager;
using static TerRoguelike.Schematics.SchematicManager;
using static TerRoguelike.Systems.MusicSystem;
using static TerRoguelike.Systems.RoomSystem;
using static TerRoguelike.Utilities.TerRoguelikeUtils;

namespace TerRoguelike.NPCs.Enemy.Boss
{
    public class BrambleHollow : BaseRoguelikeNPC
    {
        List<GodRay> deathGodRays = new List<GodRay>();
        public Entity target;
        public Vector2 spawnPos;
        public bool ableToHit = true;
        public override int modNPCID => ModContent.NPCType<BrambleHollow>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Forest"] };
        public override int CombatStyle => -1;
        public int currentFrame;
        public Texture2D lightTex;
        public int deadTime = 0;
        public int cutsceneDuration = 180;
        public int deathCutsceneDuration = 150;

        public Attack None = new Attack(0, 0, 180);
        public Attack Burrow = new Attack(1, 15, 180);
        public Attack VineWall = new Attack(2, 30, 240);
        public Attack RootLift = new Attack(3, 40, 280);
        public Attack SeedBarrage = new Attack(4, 40, 150);
        public Attack Summon = new Attack(5, 20, 150);

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 10;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 128;
            NPC.height = 128;
            NPC.aiStyle = -1;
            NPC.damage = 40;
            NPC.lifeMax = 20000;
            NPC.HitSound = SoundID.NPCHit7;
            NPC.DeathSound = SoundID.NPCDeath5;
            NPC.knockBackResist = 0f;
            modNPC.drawCenter = new Vector2(0, 0);
            lightTex = TexDict["BrambleHollowGlow"];
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.immortal = true;
            NPC.dontTakeDamage = true;
            currentFrame = Main.npcFrameCount[Type] - 1;
            NPC.localAI[0] = -(cutsceneDuration + 30);
            NPC.direction = -1;
            NPC.spriteDirection = -1;
            spawnPos = NPC.Center;
            NPC.ai[2] = Burrow.Id;
        }
        public override void AI()
        {
            if (deadTime > 0)
            {
                CheckDead();
                return;
            }
            if (modNPC.isRoomNPC && NPC.localAI[0] == -270)
            {
                SetBossTrack(PaladinTheme);
            }

            ableToHit = !(NPC.ai[0] == Burrow.Id);

            if (NPC.localAI[0] < 0)
            {
                spawnPos = NPC.Center;
                if (NPC.localAI[0] == -cutsceneDuration)
                {
                    CutsceneSystem.SetCutscene(NPC.Center, cutsceneDuration, 30, 30, 2.5f);
                }
                NPC.localAI[0]++;
                if (NPC.localAI[0] == -30)
                {
                    NPC.immortal = false;
                    NPC.dontTakeDamage = false;
                }
            }
            else
            {
                NPC.localAI[0]++;
                BossAI();
            }
        }
        public void BossAI()
        {
            target = modNPC.GetTarget(NPC, false, false);

            NPC.ai[1]++;
            NPC.frameCounter += 0.18d;
            if (NPC.ai[0] == None.Id)
            {
                if (NPC.ai[1] >= None.Duration)
                {
                    ChooseAttack();
                }
                else
                {

                }
            }

            if (NPC.ai[0] == Burrow.Id)
            {
                if (NPC.ai[1] >= Burrow.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = None.Duration - 30;
                    NPC.ai[2] = Burrow.Id;
                }
            }
            else if (NPC.ai[0] == VineWall.Id)
            {
                if (NPC.ai[1] >= VineWall.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = VineWall.Id;
                }
            }
            else if (NPC.ai[0] == RootLift.Id)
            {
                if (NPC.ai[1] >= RootLift.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = RootLift.Id;
                }
            }
            else if (NPC.ai[0] == SeedBarrage.Id)
            {
                if (NPC.ai[1] >= SeedBarrage.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = SeedBarrage.Id;
                }
            }
            else if (NPC.ai[0] == Summon.Id)
            {
                if (NPC.ai[1] >= Summon.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Summon.Id;
                }
            }
        }
        public void ChooseAttack()
        {
            NPC.ai[1] = 0;
            List<Attack> potentialAttacks = new List<Attack>() { Burrow, VineWall, RootLift, SeedBarrage, Summon };
            potentialAttacks.RemoveAll(x => x.Id == (int)NPC.ai[2]);

            int totalWeight = 0;
            for (int i = 0; i < potentialAttacks.Count; i++)
            {
                totalWeight += potentialAttacks[i].Weight;
            }
            int chosenRandom = Main.rand.Next(totalWeight);
            int chosenAttack = 0;
            for (int i = potentialAttacks.Count - 1; i >= 0; i--)
            {
                Attack attack = potentialAttacks[i];
                chosenRandom -= attack.Weight;
                if (chosenRandom < 0)
                {
                    chosenAttack = attack.Id;
                    break;
                }
            }
            NPC.ai[0] = chosenAttack;
        }
        public override bool? CanFallThroughPlatforms()
        {
            return true;
        }
        public override bool CanHitNPC(NPC target)
        {
            return ableToHit;
        }
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            return ableToHit;
        }
        public override bool? CanBeHitByItem(Player player, Item item)
        {
            return ableToHit ? null : false;
        }
        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            return ableToHit ? null : false;
        }
        public override bool CheckDead()
        {
            if (deadTime >= deathCutsceneDuration)
            {
                return true;
            }
            NPC.ai[0] = None.Id;
            NPC.ai[1] = 1;

            modNPC.OverrideIgniteVisual = true;
            NPC.velocity *= 0;
            NPC.life = 1;
            NPC.immortal = true;
            NPC.dontTakeDamage = true;
            NPC.active = true;
            if (deadTime == 0)
            {
                CutsceneSystem.SetCutscene(NPC.Center, deathCutsceneDuration + 30, 30, 30, 2.5f);
                if (modNPC.isRoomNPC)
                {
                    ActiveBossTheme.endFlag = true;
                    RoomList[modNPC.sourceRoomListID].bossDead = true;
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        if (i == NPC.whoAmI)
                            continue;

                        NPC childNPC = Main.npc[i];
                        if (childNPC == null)
                            continue;
                        if (!childNPC.active)
                            continue;

                        TerRoguelikeGlobalNPC modChildNPC = childNPC.GetGlobalNPC<TerRoguelikeGlobalNPC>();
                        if (modChildNPC == null)
                            continue;
                        if (modChildNPC.isRoomNPC && modChildNPC.sourceRoomListID == modNPC.sourceRoomListID)
                        {
                            childNPC.StrikeInstantKill();
                            childNPC.active = false;
                        }
                    }
                }
            }
            deadTime++;

            if (deadTime >= deathCutsceneDuration)
            {
                NPC.immortal = false;
                NPC.dontTakeDamage = false;
                NPC.StrikeInstantKill();
            }
                
            return deadTime >= cutsceneDuration;
        }
        public override void OnKill()
        {
            SoundEngine.PlaySound(SoundID.NPCDeath12 with { Volume = 0.8f, Pitch = -0.5f }, NPC.Center);
        }
        public override void FindFrame(int frameHeight)
        {
            int frameCount = Main.npcFrameCount[Type];

            currentFrame = (int)NPC.frameCounter % (frameCount - 5);

            NPC.frame = new Rectangle(0, currentFrame * frameHeight, TextureAssets.Npc[Type].Value.Width, frameHeight);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;
            int frameHeight = tex.Height / Main.npcFrameCount[Type];
            modNPC.drawCenter = NPC.Bottom - NPC.Center + new Vector2(0, (-frameHeight * 0.5f) + 2);
            Vector2 drawPos = NPC.Center + modNPC.drawCenter;
            float glowOpacity = 0.8f + (float)(Math.Cos(Main.GlobalTimeWrappedHourly) * 0.2f);
            Main.EntitySpriteDraw(tex, drawPos - Main.screenPosition, NPC.frame, drawColor, NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection > 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            Main.EntitySpriteDraw(lightTex, drawPos - Main.screenPosition, NPC.frame, Color.White * glowOpacity, NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection > 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            return false;
        }
    }
}
