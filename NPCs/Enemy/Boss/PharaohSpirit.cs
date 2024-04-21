using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Animations;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using TerRoguelike.Managers;
using TerRoguelike.Particles;
using TerRoguelike.Projectiles;
using TerRoguelike.Systems;
using TerRoguelike.Utilities;
using static TerRoguelike.Managers.TextureManager;
using static TerRoguelike.Schematics.SchematicManager;
using static TerRoguelike.Systems.MusicSystem;
using static TerRoguelike.Systems.RoomSystem;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using Terraria.Graphics.Effects;

namespace TerRoguelike.NPCs.Enemy.Boss
{
    public class PharaohSpirit : BaseRoguelikeNPC
    {
        public Entity target;
        public Vector2 spawnPos;
        public bool ableToHit = true;
        public bool canBeHit = true;
        public override int modNPCID => ModContent.NPCType<PharaohSpirit>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Desert"] };
        public override int CombatStyle => -1;
        public int currentFrame;
        public bool CollisionPass = false;
        public float eyeGlowInterpolant = 0;

        Texture2D glowTex;

        public int deadTime = 0;
        public int cutsceneDuration = 120;
        public int deathCutsceneDuration = 120;

        public static Attack None = new Attack(0, 0, 180);
        public static Attack Sandnado = new Attack(1, 30, 120);
        public static Attack Locust = new Attack(2, 30, 180);
        public static Attack SandTurret = new Attack(3, 30, 180);
        public static Attack Tendril = new Attack(4, 30, 180);
        public static Attack Summon = new Attack(5, 40, 180);
        public float defaultMaxVelocity = 4;
        public float defaultAcceleration = 0.08f;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 13;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 30;
            NPC.height = 76;
            NPC.aiStyle = -1;
            NPC.damage = 30;
            NPC.lifeMax = 30000;
            NPC.HitSound = SoundID.NPCHit23;
            NPC.knockBackResist = 0f;
            modNPC.drawCenter = new Vector2(0, 0);
            modNPC.IgnoreRoomWallCollision = true;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            glowTex = TexDict["PharaohSpiritGlow"].Value;
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.Opacity = 0;
            NPC.immortal = true;
            NPC.dontTakeDamage = true;
            currentFrame = 0;
            NPC.localAI[0] = -(cutsceneDuration + 30);
            NPC.direction = -1;
            NPC.spriteDirection = -1;
            spawnPos = NPC.Center;
            NPC.ai[2] = None.Id;
            ableToHit = false;
        }
        public override void PostAI()
        {
            NPC.spriteDirection = NPC.direction;
            if (currentFrame > 3)
                eyeGlowInterpolant += 0.017f;
            else
                eyeGlowInterpolant -= 0.017f;
            eyeGlowInterpolant = MathHelper.Clamp(eyeGlowInterpolant, 0, 1);
        }
        public override void AI()
        {
            if (deadTime > 0)
            {
                CheckDead();
                return;
            }
            if (modNPC.isRoomNPC && NPC.localAI[0] == -(cutsceneDuration + 30))
            {
                SetBossTrack(IceQueenTheme);
            }

            ableToHit = NPC.localAI[0] >= 0;
            canBeHit = true;

            if (NPC.localAI[0] < 0)
            {
                target = modNPC.GetTarget(NPC);

                if (NPC.localAI[0] == -cutsceneDuration)
                {
                    CutsceneSystem.SetCutscene(spawnPos, cutsceneDuration, 30, 30, 2.5f);
                }
                NPC.localAI[0]++;
                
                if (NPC.localAI[0] == -30)
                {
                    NPC.Opacity = 1;
                    NPC.immortal = false;
                    NPC.dontTakeDamage = false;
                    NPC.ai[1] = 0;
                }
            }
            else
            {
                NPC.localAI[0]++;
                BossAI();
                NPC.rotation = NPC.rotation.AngleLerp(MathHelper.Clamp(NPC.velocity.X * 0.1f, -MathHelper.PiOver2 * 0.15f, MathHelper.PiOver2 * 0.15f), 0.25f);
            }
        }
        public void BossAI()
        {
            target = modNPC.GetTarget(NPC);

            NPC.ai[1]++;

            if (NPC.ai[0] == None.Id)
            {
                if (target != null)
                {
                    if (target.Center.X > NPC.Center.X)
                        NPC.direction = 1;
                    else
                        NPC.direction = -1;
                }
                else
                    NPC.direction = Math.Sign(NPC.velocity.X);

                if (NPC.ai[1] >= None.Duration)
                {
                    ChooseAttack();
                }
                else
                {
                    NPC.velocity *= 0.98f;
                    if (target != null)
                    {
                        Vector2 targetPos = target.Center;
                        NPC.velocity += (targetPos - NPC.Center).SafeNormalize(Vector2.UnitY) * defaultAcceleration;
                    }
                    NPC.velocity.Y += (float)Math.Cos(NPC.localAI[0] / 20) * 0.04f;
                    if (NPC.velocity.Length() > defaultMaxVelocity)
                    {
                        NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * defaultMaxVelocity;
                    }
                }
            }
            if (NPC.ai[0] == Sandnado.Id)
            {
                if (NPC.ai[1] == 30)
                {
                    Room room = modNPC.sourceRoomListID >= 0 ? RoomList[modNPC.sourceRoomListID] : null;
                    int nadoCount = 4;
                    for (int i = 0; i < nadoCount; i++)
                    {
                        float completion = (i + 1f) / (nadoCount + 1f);
                        Vector2 projPos = room != null ? room.RoomPosition16 + new Vector2(room.RoomDimensions16.X * completion, room.RoomDimensions16.Y * 0.5f) : spawnPos + new Vector2(-1000 + (2000 * completion), 0);
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), projPos, Vector2.Zero, ModContent.ProjectileType<Sandnado>(), NPC.damage, 0);
                    }
                }
                if (NPC.ai[1] >= Sandnado.Duration)
                {
                    currentFrame = 0;
                    NPC.localAI[0] = 0;
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Sandnado.Id;
                }
            }
            else if (NPC.ai[0] == Locust.Id)
            {
                if (NPC.ai[1] >= Locust.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Locust.Id;
                }
            }
            else if (NPC.ai[0] == SandTurret.Id)
            {
                if (NPC.ai[1] >= SandTurret.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = SandTurret.Id;
                }
            }
            else if (NPC.ai[0] == Tendril.Id)
            {
                if (NPC.ai[1] >= Tendril.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Tendril.Id;
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

            if (NPC.ai[0] != None.Id)
            {
                NPC.velocity *= 0.95f;
            }
        }

        public void ChooseAttack()
        {
            NPC.ai[1] = 0;
            int chosenAttack = 0;

            List<Attack> potentialAttacks = new List<Attack>() { Sandnado, Locust, SandTurret, Tendril, Summon };
            potentialAttacks.RemoveAll(x => x.Id == (int)NPC.ai[2]);

            int totalWeight = 0;
            for (int i = 0; i < potentialAttacks.Count; i++)
            {
                totalWeight += potentialAttacks[i].Weight;
            }
            int chosenRandom = Main.rand.Next(totalWeight);

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
            chosenAttack = Sandnado.Id;
            NPC.ai[0] = chosenAttack;
        }

        public override bool? CanFallThroughPlatforms()
        {
            return true;
        }
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            return ableToHit;
        }
        public override bool CanHitNPC(NPC target)
        {
            return ableToHit;
        }
        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            return canBeHit ? null : false;
        }

        public override bool CheckDead()
        {
            if (deadTime >= deathCutsceneDuration - 30)
            {
                return true;
            }

            NPC.ai[0] = None.Id;
            NPC.ai[1] = 1;

            modNPC.OverrideIgniteVisual = true;
            NPC.life = 1;
            NPC.immortal = true;
            NPC.dontTakeDamage = true;
            NPC.active = true;


            if (deadTime == 0)
            {
                NPC.rotation = 0;
                NPC.velocity *= 0;
                modNPC.ignitedStacks.Clear();
                if (modNPC.isRoomNPC)
                {
                    ActiveBossTheme.endFlag = true;
                    Room room = RoomList[modNPC.sourceRoomListID];
                    room.bossDead = true;
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        if (i == NPC.whoAmI)
                            continue;

                        NPC childNPC = Main.npc[i];
                        if (childNPC == null)
                            continue;
                        if (!childNPC.active)
                            continue;

                        TerRoguelikeGlobalNPC modChildNPC = childNPC.ModNPC();
                        if (modChildNPC == null)
                            continue;
                        if (modChildNPC.isRoomNPC && modChildNPC.sourceRoomListID == modNPC.sourceRoomListID)
                        {
                            childNPC.StrikeInstantKill();
                            childNPC.active = false;
                        }
                    }
                }
                CutsceneSystem.SetCutscene(NPC.Center, deathCutsceneDuration, 30, 30, 2.5f);
            }
            deadTime++;

            if (deadTime >= deathCutsceneDuration - 30)
            {
                NPC.immortal = false;
                NPC.dontTakeDamage = false;
                NPC.StrikeInstantKill();
            }

            return deadTime >= cutsceneDuration - 30;
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            Color color = new Color(222, 108, 48) * 0.7f;
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)NPC.lifeMax * 400; i++)
                {
                    Dust d = Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Sandstorm)];
                    d.color = color;
                    d.noGravity = true;
                    d.scale = 1.5f;
                    d.fadeIn = 0.7f;
                    d.velocity *= 3f;
                }
            }
            else if (deadTime > 0)
            {
                SoundEngine.PlaySound(SoundID.NPCDeath39, NPC.Center);
                for (int i = 0; i < 60; i++)
                {
                    Dust d = Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Sandstorm)];
                    d.color = color;
                    d.noGravity = true;
                    d.scale = 1.5f;
                    d.fadeIn = 0.7f;
                    d.velocity *= 3f;
                }
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 960, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 961, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 963, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 40f), NPC.velocity, 962, NPC.scale);
            }
        }
        public override void OnKill()
        {
            
        }
        public override void FindFrame(int frameHeight)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;

            if (deadTime > 0)
            {
                currentFrame = 10;
            }
            else if (NPC.localAI[0] < 0)
            {
                currentFrame = 0;
            }
            else if (NPC.ai[0] == None.Id)
            {
                currentFrame = (int)(NPC.localAI[0] * 0.1f) % 4;
            }
            else if (NPC.ai[0] == Sandnado.Id)
            {
                if (NPC.ai[1] < 30)
                {
                    currentFrame = (int)(NPC.ai[1] / 10) + 4;
                }
                else if (NPC.ai[1] >= 30 && NPC.ai[1] < 80)
                {
                    currentFrame = (int)((NPC.ai[1] / 10 + 2) % 4) + 5;
                }
                else if (NPC.ai[1] >= 80)
                {
                    currentFrame = (int)((NPC.ai[1] - 80) / 10) + 9;
                }
            }
            
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, tex.Width, frameHeight);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;

            Vector2 drawPos = NPC.Center + modNPC.drawCenter - Main.screenPosition;
            Main.EntitySpriteDraw(tex, drawPos, NPC.frame, Color.Lerp(drawColor, Color.White, 0.25f), NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
            float finalEyeGlowInterpolant = eyeGlowInterpolant * (0.3f * ((float)Math.Cos(Main.GlobalTimeWrappedHourly * 16)) + 0.7f);
            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(glowTex, drawPos + (Vector2.UnitX * finalEyeGlowInterpolant * 1).RotatedBy(i * MathHelper.PiOver2), NPC.frame, Color.White * 0.5f, NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
            }
            

            return false;
        }
    }
}
