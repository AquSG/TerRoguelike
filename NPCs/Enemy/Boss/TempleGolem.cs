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
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using TerRoguelike.Managers;
using TerRoguelike.Particles;
using TerRoguelike.Projectiles;
using TerRoguelike.Systems;
using TerRoguelike.Utilities;
using TerRoguelike.World;
using static Terraria.GameContent.PlayerEyeHelper;
using static TerRoguelike.Managers.TextureManager;
using static TerRoguelike.Schematics.SchematicManager;
using static TerRoguelike.Systems.MusicSystem;
using static TerRoguelike.Systems.RoomSystem;
using static TerRoguelike.Utilities.TerRoguelikeUtils;

namespace TerRoguelike.NPCs.Enemy.Boss
{
    public class TempleGolem : BaseRoguelikeNPC
    {
        public Entity target;
        public Vector2 spawnPos;
        public bool ableToHit = true;
        public bool canBeHit = true;
        public override int modNPCID => ModContent.NPCType<TempleGolem>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Temple"] };
        public override int CombatStyle => -1;
        public int currentFrame;
        public SlotId RumbleSlot;
        public Texture2D eyeTex;
        public Texture2D lightTex;
        public List<Vector2> eyePositions = [];

        public int deadTime = 0;
        public int cutsceneDuration = 120;
        public int deathCutsceneDuration = 120;

        public static Attack None = new Attack(0, 0, 30);
        public static Attack Laser = new Attack(1, 30, 180);
        public static Attack SpikeBall = new Attack(2, 30, 180);
        public static Attack Flame = new Attack(3, 30, 180);
        public static Attack DartTrap = new Attack(4, 30, 180);
        public static Attack Boulder = new Attack(5, 30, 180);
        public static Attack Summon = new Attack(6, 16, 180);
        public int laserWindup = 60;
        public int laserFireRate = 7;
        public int spikeBallWindup = 60;
        public int spikeBallFireRate = 4;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 2;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 70;
            NPC.height = 70;
            NPC.aiStyle = -1;
            NPC.damage = 34;
            NPC.lifeMax = 31000;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.knockBackResist = 0f;
            modNPC.drawCenter = new Vector2(0, 0);
            modNPC.IgnoreRoomWallCollision = true;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            eyeTex = TexDict["TempleGolemEyes"];
            lightTex = TexDict["TempleGolemGlow"];
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.immortal = true;
            NPC.dontTakeDamage = true;
            currentFrame = 0;
            NPC.localAI[0] = -(cutsceneDuration + 30);
            NPC.direction = -1;
            NPC.spriteDirection = -1;
            NPC.Center += Vector2.UnitY * NPC.height * 0.5f;
            spawnPos = NPC.Center;
            NPC.ai[2] = None.Id;
            ableToHit = false;

            NPC.Center = TileCollidePositionInLine(NPC.Center, NPC.Center + new Vector2(0, -1000));
            NPC.Center += new Vector2(0, 45);

            for (int i = -1; i <= 1; i += 2)
            {
                eyePositions.Add(new Vector2(-15 * i - 1, -3) + NPC.Center + modNPC.drawCenter);
            }
        }
        public override void PostAI()
        {
            if (SoundEngine.TryGetActiveSound(RumbleSlot, out var sound) && sound.IsPlaying)
            {
                if (NPC.ai[0] != SpikeBall.Id || (NPC.ai[0] == SpikeBall.Id && NPC.ai[1] >= spikeBallWindup))
                {
                    sound.Volume *= 0.99f;
                    sound.Volume -= 0.001f;
                    if (sound.Volume <= 0)
                        sound.Stop();
                }
            }
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
                SetBossTrack(SkeletronTheme);
            }

            ableToHit = NPC.localAI[0] >= 0 && deadTime == 0;
            canBeHit = true;

            if (NPC.localAI[0] < 0)
            {
                target = modNPC.GetTarget(NPC);

                if (NPC.localAI[0] == -cutsceneDuration)
                {
                    CutsceneSystem.SetCutscene(NPC.Center, cutsceneDuration, 30, 30, 2.5f);
                }
                NPC.localAI[0]++;

                if (NPC.localAI[0] == -30)
                {
                    NPC.immortal = false;
                    NPC.dontTakeDamage = false;
                    NPC.ai[1] = 0;
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
            target = modNPC.GetTarget(NPC);
            NPC.ai[1]++;
            NPC.velocity *= 0.98f;

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

            if (NPC.ai[0] == Laser.Id)
            {
                Vector2 targetPos = target != null ? target.Center : spawnPos;
                Color particleColor = Color.Lerp(Color.Goldenrod, Color.White, 0.3f);
                if (NPC.ai[1] < laserWindup)
                {
                    if (NPC.ai[1] == 0)
                    {
                        SoundEngine.PlaySound(SoundID.Item15 with { Volume = 1f, Pitch = -0.5f, PitchVariance = 0 }, NPC.Center);
                    }
                    if (NPC.ai[1] % 30 == 0)
                    {
                        SoundEngine.PlaySound(SoundID.Item13 with { Volume = 0.8f, Pitch = -0.05f, PitchVariance = 0 }, NPC.Center);
                    }
                    for (int i = 0; i < eyePositions.Count; i++)
                    {
                        if (Main.rand.NextBool())
                        {
                            Vector2 offset = Main.rand.NextVector2CircularEdge(24f, 24f);
                            ParticleManager.AddParticle(new Square(
                                eyePositions[i] + offset, -offset * 0.1f, 30, particleColor, new Vector2(Main.rand.NextFloat(0.5f, 0.75f)), offset.ToRotation(), 0.96f, 30, true));
                        }
                    }
                }
                else
                {
                    int time = (int)NPC.ai[1] - laserWindup;
                    if (time % laserFireRate == 0)
                    {
                        SoundEngine.PlaySound(SoundID.Item91 with { Volume = 0.8f, MaxInstances = 2 }, NPC.Center);
                        for (int i = 0; i < eyePositions.Count; i++)
                        {
                            Vector2 pos = eyePositions[i];
                            float rotToTarget = (targetPos - pos).ToRotation();
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), pos, rotToTarget.ToRotationVector2() * 10, ModContent.ProjectileType<LihzahrdLaser>(), NPC.damage, 0, -1, 1);

                            for (int j = 0; j < 5; j++)
                            {
                                Vector2 offset = Main.rand.NextVector2CircularEdge(24f, 24f);
                                ParticleManager.AddParticle(new Square(
                                    eyePositions[i] + offset, -offset * 0.1f, 30, particleColor, new Vector2(Main.rand.NextFloat(0.75f, 1f)), offset.ToRotation(), 0.96f, 30, true));
                            }
                        }
                    }
                }
                
                if (NPC.ai[1] >= Laser.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Laser.Id;
                }
            }
            else if (NPC.ai[0] == SpikeBall.Id)
            {
                Vector2 leftBound = TileCollidePositionInLine(NPC.Center + new Vector2(0, 160), NPC.Center + new Vector2(-1000, 160)) + Vector2.UnitX * 8;
                Vector2 rightBound = TileCollidePositionInLine(leftBound, leftBound + new Vector2(2000, 0)) - Vector2.UnitX * 8;
                float boundWidth = leftBound.Distance(rightBound);
                float thirdWidth = boundWidth * 0.3333f;
                if (NPC.ai[1] < spikeBallWindup)
                {
                    if (NPC.ai[1] == 0)
                    {
                        NPC.ai[3] = Main.rand.Next(3);
                        RumbleSlot = SoundEngine.PlaySound(TerRoguelikeWorld.EarthTremor with { Volume = 1f }, new Vector2(leftBound.X + (thirdWidth * NPC.ai[3]) + thirdWidth * 0.5f, NPC.position.Y));
                    }
                }
                else
                {
                    if (NPC.ai[1] % spikeBallFireRate == 0)
                    {
                        Vector2 projSpawnPos = new Vector2(leftBound.X + (thirdWidth * NPC.ai[3]) + Main.rand.NextFloat(thirdWidth), NPC.Center.Y);
                        projSpawnPos = TileCollidePositionInLine(projSpawnPos, projSpawnPos + new Vector2(0, -240)) - Vector2.UnitY * 16;
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), projSpawnPos, Vector2.Zero, ModContent.ProjectileType<LihzahrdSpikeBall>(), NPC.damage, 0, -1, 7);
                    }
                    
                }
                if (NPC.localAI[1] >= 0)
                {
                    NPC.localAI[1] = -(2 + ((int)NPC.ai[1] / spikeBallWindup) * 2);
                    Vector2 particlePos = new Vector2(leftBound.X + (thirdWidth * NPC.ai[3]) + Main.rand.NextFloat(thirdWidth), NPC.Center.Y);
                    particlePos = TileCollidePositionInLine(particlePos, particlePos + new Vector2(0, -240)) - Vector2.UnitY * 16;
                    ParticleManager.AddParticle(new Debris(
                        particlePos, Vector2.UnitY * Main.rand.NextFloat(0.75f, 1.25f),
                        80, Color.DarkOrange * 0.875f, new Vector2(0.5f), Main.rand.Next(3), Main.rand.NextFloat(MathHelper.TwoPi),
                        Main.rand.NextBool() ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0.1f, 7f, 60),
                        ParticleManager.ParticleLayer.BehindTiles);
                }
                else
                    NPC.localAI[1]++;

                if (NPC.ai[1] >= SpikeBall.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = SpikeBall.Id;
                    NPC.ai[3] = 0;
                    NPC.localAI[1] = 0;
                }
            }
            else if (NPC.ai[0] == Flame.Id)
            {
                if (NPC.ai[1] >= Flame.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Flame.Id;
                }
            }
            else if (NPC.ai[0] == DartTrap.Id)
            {
                if (NPC.ai[1] >= DartTrap.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = DartTrap.Id;
                }
            }
            else if (NPC.ai[0] == Boulder.Id)
            {
                if (NPC.ai[1] >= Boulder.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Boulder.Id;
                }
            }
            else if (NPC.ai[0] == Summon.Id)
            {
                if (NPC.ai[1] >= Summon.Duration)
                {
                    currentFrame = 0;
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Summon.Id;
                }
            }
        }
        public void ChooseAttack()
        {
            NPC.ai[1] = 0;
            int chosenAttack = 0;

            //List<Attack> potentialAttacks = new List<Attack>() { Laser, SpikeBall, Flame, DartTrap, Boulder, Summon };
            List<Attack> potentialAttacks = new List<Attack>() { Laser, SpikeBall };
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
            chosenAttack = SpikeBall.Id;
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
            NPC.ai[3] = 0;
            NPC.localAI[1] = 0;

            modNPC.OverrideIgniteVisual = true;
            NPC.life = 1;
            NPC.immortal = true;
            NPC.dontTakeDamage = true;
            NPC.active = true;
            ableToHit = false;
            currentFrame = 0;

            if (deadTime == 0)
            {
                NPC.velocity = Vector2.Zero;
                NPC.rotation = 0;
                modNPC.ignitedStacks.Clear();
                

                if (modNPC.isRoomNPC)
                {
                    ActiveBossTheme.endFlag = true;
                    Room room = RoomList[modNPC.sourceRoomListID];
                    room.bossDead = true;
                    ClearChildren();
                }
                CutsceneSystem.SetCutscene(NPC.Center, deathCutsceneDuration, 30, 30, 2.5f);
            }

            void ClearChildren()
            {
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
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage * 0.04d; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.t_Lihzahrd, hit.HitDirection, -1f);
                }
            }
        }
        public override void OnKill()
        {
            SoundEngine.PlaySound(SoundID.NPCDeath14 with { Volume = 1f }, NPC.Center);

            Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X + Main.rand.Next(NPC.width), NPC.position.Y + Main.rand.Next(NPC.height)), NPC.velocity, 368, NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X + Main.rand.Next(NPC.width), NPC.position.Y + Main.rand.Next(NPC.height)), NPC.velocity, 370, NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X + Main.rand.Next(NPC.width), NPC.position.Y + Main.rand.Next(NPC.height)), NPC.velocity, 368, NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X + Main.rand.Next(NPC.width), NPC.position.Y + Main.rand.Next(NPC.height)), NPC.velocity, 370, NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 365, NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 363, NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 362, NPC.scale);
        }
        public override void FindFrame(int frameHeight)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;

            NPC.frame = new Rectangle(0, currentFrame * frameHeight, tex.Width, frameHeight - 2);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;
            Color color = Color.Lerp(drawColor, Color.White, 0.2f);
            Vector2 origin = NPC.frame.Size() * 0.5f;

            Main.EntitySpriteDraw(tex, NPC.Center + modNPC.drawCenter - Main.screenPosition, NPC.frame, color, NPC.rotation, origin, NPC.scale, NPC.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);

            Vector2 eyePos = NPC.Center + modNPC.drawCenter;
            for (int j = 0; j < 5; j++)
            {
                Main.EntitySpriteDraw(eyeTex, eyePos - Main.screenPosition + Main.rand.NextVector2CircularEdge(2f, 2f) * NPC.scale, null, Color.White * 0.4f, 0, origin, NPC.scale, SpriteEffects.None);
            }

            Main.EntitySpriteDraw(eyeTex, eyePos - Main.screenPosition, null, Color.White, NPC.rotation, origin, NPC.scale, NPC.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
            Main.EntitySpriteDraw(lightTex, NPC.Center + modNPC.drawCenter - Main.screenPosition, null, Color.White, NPC.rotation, origin, NPC.scale, NPC.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);

            return false;
        }
    }
}
