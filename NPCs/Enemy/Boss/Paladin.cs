using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TerRoguelike.Projectiles;
using TerRoguelike.Systems;
using static TerRoguelike.Managers.TextureManager;
using static TerRoguelike.Schematics.SchematicManager;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using TerRoguelike.Particles;
using TerRoguelike.Managers;

namespace TerRoguelike.NPCs.Enemy.Boss
{
    public class Paladin : BaseRoguelikeNPC
    {
        Entity target;
        Vector2 spawnPos;
        public float acceleration = 0.05f;
        public float deceleration = 0.95f;
        public float xCap = 2f;
        public float chargeSpeed = 8f;
        public int chargeTelegraph = 60;
        public float jumpVelocity = -7.9f;
        public int slamTelegraph = 40;
        public int slamRise = 60;
        public int slamFall = 60;
        public bool ableToHit = true;
        public override int modNPCID => ModContent.NPCType<Paladin>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Base"] };
        public override int CombatStyle => -1;
        public int currentFrame;
        public Texture2D hammerTex;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 16;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 32;
            NPC.height = 56;
            NPC.aiStyle = -1;
            NPC.damage = 25;
            NPC.lifeMax = 20000;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.NPCDeath6;
            NPC.knockBackResist = 0f;
            modNPC.drawCenter = new Vector2(0, -12);
            hammerTex = TexDict["PaladinHammer"];
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.immortal = true;
            NPC.dontTakeDamage = true;
            currentFrame = Main.npcFrameCount[Type] - 1;
            NPC.localAI[0] = -270;
            NPC.direction = -1;
            NPC.spriteDirection = -1;
            spawnPos = NPC.Center;
            NPC.ai[2] = Slam.Id;
        }
        public override void AI()
        {
            ableToHit = !(NPC.ai[0] == Slam.Id && NPC.ai[1] >= slamTelegraph && NPC.ai[1] <= slamTelegraph + slamRise + slamFall);

            if (NPC.collideY)
                NPC.localAI[1] = 0;
            else
                NPC.localAI[1]++;

            if (NPC.localAI[0] < 0)
            {
                spawnPos = NPC.Center;
                if (NPC.localAI[0] == -210)
                {
                    CutsceneSystem.SetCutscene(NPC.Center, 210, 30, 30, 2.5f);
                    SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot with { Volume = 1f, Pitch = -1.3f }, NPC.Center);
                }
                if (NPC.localAI[0] == -150)
                {
                    SlamEffect();
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
        public Attack None = new Attack(0, 0, 300);
        public Attack Charge = new Attack(1, 30, 210);
        public Attack Throw = new Attack(2, 30, 240);
        public Attack Slam = new Attack(3, 20, 280);
        public Attack Summon = new Attack(4, 20, 150);
        public void BossAI()
        {
            target = modNPC.GetTarget(NPC, false, false);

            acceleration = 0.05f;
            deceleration = 0.88f;
            xCap = target == null ? 0 : 2f;
            chargeSpeed = 8f;
            chargeTelegraph = 60;
            jumpVelocity = -7.9f;

            NPC.ai[1]++;

            if (NPC.ai[0] == None.Id)
            {
                if (NPC.ai[1] >= None.Duration)
                {
                    ChooseAttack();
                }
                else
                {
                    if (target != null)
                    {
                        if (NPC.Center.X < target.Center.X)
                        {
                            NPC.direction = 1;
                            NPC.spriteDirection = 1;
                        }
                        else
                        {
                            NPC.direction = -1;
                            NPC.spriteDirection = -1;
                        }
                    }
                    if (NPC.velocity.X < -xCap || NPC.velocity.X > xCap)
                    {
                        if (NPC.velocity.Y == 0f)
                            NPC.velocity *= 0.8f;
                    }
                    else if (NPC.velocity.X < xCap && NPC.direction == 1)
                    {
                        NPC.velocity.X += acceleration;
                        if (NPC.velocity.X > xCap)
                            NPC.velocity.X = xCap;
                    }
                    else if (NPC.velocity.X > -xCap && NPC.direction == -1)
                    {
                        NPC.velocity.X -= acceleration;
                        if (NPC.velocity.X < -xCap)
                            NPC.velocity.X = -xCap;
                    }
                }
                NPC.frameCounter += 0.08d * Math.Abs(NPC.velocity.X);
            }

            if (NPC.ai[0] == Charge.Id)
            {
                if (NPC.ai[1] == 0)
                {
                    if (target != null)
                    {
                        if (NPC.Center.X < target.Center.X)
                        {
                            NPC.direction = 1;
                            NPC.spriteDirection = 1;
                        }
                        else
                        {
                            NPC.direction = -1;
                            NPC.spriteDirection = -1;
                        }
                    }
                }
                if (NPC.ai[1] < chargeTelegraph)
                {
                    NPC.velocity.X *= deceleration;
                    if (NPC.ai[1] % 20 == 0 || NPC.ai[1] % 20 == 5)
                    {
                        Vector2 particleVelocity = new Vector2(-3f * NPC.direction, -0.4f);
                        Vector2 particleScale = new Vector2(0.34f, 0.34f);
                        particleScale *= Main.rand.NextFloat(0.8f, 1f);
                        int timeLeft = Main.rand.Next(20, 26);

                        ParticleManager.AddParticle(new Spark(NPC.Center + new Vector2(NPC.width * 0.6f * -NPC.direction, 8), particleVelocity, timeLeft, Color.DarkGray, particleScale, particleVelocity.ToRotation()));
                        ParticleManager.AddParticle(new Spark(NPC.Center + new Vector2(NPC.width * 0.6f * -NPC.direction, 24), particleVelocity + new Vector2(0, -0.2f), timeLeft, Color.DarkGray, particleScale, particleVelocity.ToRotation()));

                        ParticleManager.AddParticle(new Spark(NPC.Center + new Vector2(NPC.width * 0.6f * -NPC.direction, -16), particleVelocity + new Vector2(0, 0.4f), timeLeft, Color.DarkGray, particleScale, particleVelocity.ToRotation()));
                    }
                }
                else if (NPC.ai[1] >= chargeTelegraph && NPC.ai[1] <= chargeTelegraph + 1)
                {
                    NPC.ai[1] = chargeTelegraph;
                    if (NPC.collideX)
                    {
                        SoundEngine.PlaySound(SoundID.DeerclopsRubbleAttack with { Volume = 0.5f, Pitch = 0.5f }, NPC.Center);
                        SoundEngine.PlaySound(SoundID.DeerclopsIceAttack with { Volume = 0.25f, Pitch = -0.2f }, NPC.Center);
                        SoundEngine.PlaySound(SoundID.Item70 with { Volume = 0.375f, Pitch = -0.2f }, NPC.Center);
                        for (int i = -15; i <= 15; i++)
                        {
                            Dust d = Dust.NewDustPerfect(NPC.Bottom + new Vector2(3 * i, 0), DustID.Smoke, null, 0, Color.LightGray, 1f);
                            d.velocity.Y -= 1;
                            d.velocity.X += -NPC.oldVelocity.X * 0.25f;
                        }
                        for (int i = -15; i <= 15; i++)
                        {
                            Dust d = Dust.NewDustPerfect(NPC.Bottom + new Vector2(0.75f * i, 0), DustID.Stone, new Vector2(Main.rand.NextFloat(0.05f, 0.15f) * i + (Math.Sign(i) * 1.5f), Main.rand.NextFloat(-4f, -2f)), 0, default, 1.2f);
                            d.velocity.X += -NPC.oldVelocity.X * 0.25f;
                        }
                        for (int i = -1; i <= 2; i++)
                        {
                            int goreid = Main.rand.NextFromCollection(new List<int>() { GoreID.Smoke1, GoreID.Smoke2, GoreID.Smoke3 });
                            Gore g = Gore.NewGorePerfect(NPC.GetSource_FromThis(), NPC.Center + new Vector2(16 * Math.Sign(NPC.oldVelocity.X), -16 - 16 * i), new Vector2(Main.rand.NextFloat(-1.2f, -0.7f) * Math.Sign(NPC.oldVelocity.X), Main.rand.NextFloat(-0.08f, 0.08f)), goreid);
                        }

                        NPC.ai[1] += 2;
                        NPC.velocity.X = -chargeSpeed * NPC.direction * 0.55f;
                        NPC.velocity.Y = -chargeSpeed * 0.4f;

                        for (int i = 0; i < 12; i++)
                        {
                            Point tilePos = Point.Zero;
                            int valid = -1;
                            for (int j = 0; j < 5; j++)
                            {
                                tilePos = (NPC.Center + new Vector2(Main.rand.NextFloat(-800, 60) * NPC.direction, -480)).ToTileCoordinates();
                                Tile tile = ParanoidTileRetrieval(tilePos.X, tilePos.Y);
                                if (!tile.IsTileSolidGround(true))
                                    continue;

                                for (int k = 1; k < 25; k++)
                                {
                                    Tile belowTile = ParanoidTileRetrieval(tilePos.X, tilePos.Y + k);
                                    if (!belowTile.IsTileSolidGround())
                                    {
                                        valid = k;
                                        break;
                                    }
                                }
                                if (valid != -1)
                                    break;
                            }
                            if (valid != -1)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), new Vector2(tilePos.X * 16f, (tilePos.Y + valid - 2) * 16f) + new Vector2(8f), new Vector2(0, 7), ModContent.ProjectileType<RockDebris>(), NPC.damage, 0f, -1, Main.rand.Next(-25, 1));
                            }
                        }
                    }
                    else
                    {
                        NPC.velocity.X = chargeSpeed * NPC.direction;
                        if ((int)NPC.localAI[0] % 9 == 0)
                        {
                            SoundEngine.PlaySound(SoundID.DeerclopsStep with { Volume = 0.35f, Pitch = -0.1f, MaxInstances = 8 }, NPC.Center);
                            SoundEngine.PlaySound(SoundID.NPCHit4 with { Volume = 0.05f, Pitch = -0.9f, PitchVariance = 0.08f, MaxInstances = 8 }, NPC.Center);
                        }
                        if ((int)NPC.localAI[0] % 3 == 0)
                        {
                            Vector2 particleVelocity = new Vector2(-3f * Math.Sign(NPC.velocity.X), -0.4f);
                            Vector2 particleScale = new Vector2(0.45f, 0.34f);
                            particleScale *= Main.rand.NextFloat(0.8f, 1f);
                            int timeLeft = Main.rand.Next(20, 26);

                            ParticleManager.AddParticle(new Spark(NPC.Center + new Vector2(NPC.width * 0.5f * -Math.Sign(NPC.velocity.X), 8), particleVelocity, timeLeft, Color.Goldenrod, particleScale, particleVelocity.ToRotation()));
                            ParticleManager.AddParticle(new Spark(NPC.Center + new Vector2(NPC.width * 0.5f * -Math.Sign(NPC.velocity.X), 24), particleVelocity + new Vector2(0, -0.2f), timeLeft, Color.Goldenrod, particleScale, particleVelocity.ToRotation()));

                            ParticleManager.AddParticle(new Spark(NPC.Center + new Vector2(NPC.width * 0.5f * -Math.Sign(NPC.velocity.X), -16), particleVelocity + new Vector2(0, 0.4f), timeLeft, Color.Goldenrod, particleScale, particleVelocity.ToRotation()));
                        }
                    }
                }
                if (NPC.ai[1] > chargeTelegraph + 1)
                {
                    NPC.velocity.X *= deceleration;
                }
                if (NPC.ai[1] >= Charge.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 120;
                    NPC.ai[2] = Charge.Id;
                }
                NPC.frameCounter += 0.05d * Math.Abs(NPC.velocity.X);
            }
            else if (NPC.ai[0] == Throw.Id)
            {
                if (target != null && NPC.localAI[3] <= 0)
                {
                    if (NPC.Center.X < target.Center.X)
                    {
                        NPC.direction = 1;
                        NPC.spriteDirection = 1;
                    }
                    else
                    {
                        NPC.direction = -1;
                        NPC.spriteDirection = -1;
                    }
                }

                int[] attackTimes = new int[] { 40, 120, 180, 220 };

                if (NPC.localAI[2] > 0)
                    NPC.localAI[2]--;
                if (NPC.localAI[3] > 0)
                    NPC.localAI[3]--;

                for (int i = 0; i < attackTimes.Length; i++)
                {
                    int attackTime = attackTimes[i];
                    if (NPC.ai[1] == attackTime - 20)
                    {
                        NPC.localAI[3] += 12;
                        for (int j = 0; j < 2; j++)
                        {
                            ParticleManager.AddParticle(new Spark(NPC.Center + new Vector2(12 * NPC.direction, -22), Vector2.Zero, 25, Color.Red, new Vector2(0.25f, 0.17f), MathHelper.PiOver2 * j, true, SpriteEffects.None, true, false));
                        }
                        SoundEngine.PlaySound(new SoundStyle("TerRoguelike/Sounds/Ding") with { Volume = 0.06f, MaxInstances = 3, Pitch = -0.5f, PitchVariance = 0.025f }, NPC.Center);
                        continue;
                    }
                    if (NPC.ai[1] == attackTime)
                    {
                        NPC.localAI[2] += 20;
                        SoundEngine.PlaySound(SoundID.Item1 with { Volume = 1f, MaxInstances = 3 }, NPC.Center);
                        Vector2 projPos = NPC.Center + new Vector2(0 * NPC.direction, -16);
                        Vector2 velocityDirection = target == null ? Vector2.UnitX * NPC.direction : (target.Center - projPos).SafeNormalize(Vector2.UnitX * NPC.direction);
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), projPos, velocityDirection * 8f, ModContent.ProjectileType<PaladinHammer>(), NPC.damage, 0f);
                    }
                }

                NPC.velocity.X *= deceleration;
                if (NPC.ai[1] >= Throw.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Throw.Id;
                }
            }
            else if (NPC.ai[0] == Slam.Id)
            {
                NPC.velocity.X *= deceleration;
                if (NPC.ai[1] >= slamTelegraph)
                    NPC.velocity.X = 0;
                
                if (NPC.ai[1] == 0)
                {
                    SoundEngine.PlaySound(SoundID.DeerclopsStep with { Volume = 0.35f, Pitch = -0.2f }, NPC.Center);
                    SoundEngine.PlaySound(SoundID.NPCHit4 with { Volume = 0.05f, Pitch = -0.9f, PitchVariance = 0.08f }, NPC.Center);
                    for (int i = -1; i <= 1; i += 2)
                    {
                        Vector2 particleVelocity = new Vector2(0, 0.4f);
                        Vector2 particleScale = new Vector2(0.3f, 0.38f);
                        int timeLeft = 30;
                        ParticleManager.AddParticle(new Spark(NPC.Bottom + new Vector2((NPC.width * 0.25f * i) + (8 * NPC.direction), 12), particleVelocity + NPC.velocity * 0.5f, timeLeft, Color.Yellow * 0.85f, particleScale, particleVelocity.ToRotation(), true, SpriteEffects.None, false, false));
                    }
                }
                if (NPC.ai[1] == slamTelegraph)
                {
                    NPC.immortal = true;
                    NPC.dontTakeDamage = true;
                    SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Volume = 0.72f, Pitch = -0.25f }, NPC.Center);
                    for (int i = -9; i <= 9; i++)
                    {
                        Dust d = Dust.NewDustPerfect(NPC.Bottom + new Vector2(3 * i, 0), DustID.Smoke, null, 0, Color.LightGray, 1f);
                        d.velocity.Y -= 1;
                    }
                    for (int i = -9; i <= 9; i++)
                    {
                        Dust d = Dust.NewDustPerfect(NPC.Bottom + new Vector2(0.75f * i, 0), DustID.Stone, new Vector2(Main.rand.NextFloat(0.05f, 0.15f) * i + (Math.Sign(i) * 1.5f), Main.rand.NextFloat(-4f, -2f)), 0, default, 1.2f);
                    }

                    for (int i = -2; i <= 2; i++)
                    {
                        if (i == 0)
                            continue;

                        int direction = Math.Sign(i);
                        Vector2 particleVelocity = new Vector2(5f * direction, Math.Abs(i) == 1 ? -0.4f : -1.6f);
                        Vector2 particleScale = new Vector2(0.7f);
                        particleScale *= Main.rand.NextFloat(0.8f, 1f);
                        int timeLeft = Main.rand.Next(20, 26);
                        ParticleManager.AddParticle(new Spark(NPC.Bottom + new Vector2(NPC.width * 0.2f * direction, Math.Abs(i) == 1 ? 0f : -4f), particleVelocity, timeLeft, Color.LightGray * 0.8f, particleScale, particleVelocity.ToRotation()));
                    }
                    
                }
                if (NPC.ai[1] == slamTelegraph + slamRise)
                {
                    NPC.Center = spawnPos;
                    NPC.direction = -1;
                    NPC.spriteDirection = -1;
                    SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot with { Volume = 1f, Pitch = -1.3f }, NPC.Center);
                }
                if (NPC.ai[1] == slamTelegraph + slamRise + 30)
                {
                    SoundEngine.PlaySound(SoundID.Tink with { Volume = 0.5f }, NPC.Center);
                }
                if (NPC.ai[1] == slamTelegraph + slamRise + slamFall)
                {
                    SlamEffect();
                    NPC.immortal = false;
                    NPC.dontTakeDamage = false;
                }
                if (NPC.ai[1] >= Slam.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 120;
                    NPC.ai[2] = Slam.Id;
                }
            }
            else if (NPC.ai[0] == Summon.Id)
            {
                NPC.velocity.X *= deceleration;
                if (NPC.ai[1] >= Summon.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Summon.Id;
                }
            }

            if (target != null)
            {
                if (NPC.ai[0] == None.Id && NPC.ai[1] < None.Duration - 60)
                {
                    if (NPC.velocity.Y == 0f && target.Bottom.Y < NPC.Top.Y && Math.Abs(NPC.Center.X - target.Center.X) < (float)(target.width * 3) && Collision.CanHit(NPC, target))
                    {
                        if (NPC.velocity.Y == 0f)
                        {
                            int padding = (int)(6 * (jumpVelocity / -7.9f));
                            if (target.Bottom.Y > NPC.Top.Y - (float)(padding * 16))
                            {
                                NPC.velocity.Y = jumpVelocity;
                                NPC.localAI[1] = 10;
                            }
                            else
                            {
                                int bottomtilepointx = (int)(NPC.Center.X / 16f);
                                int bottomtilepointY = (int)(NPC.Bottom.Y / 16f) - 1;
                                for (int i = bottomtilepointY; i > bottomtilepointY - padding; i--)
                                {
                                    if (Main.tile[bottomtilepointx, i].HasUnactuatedTile && TileID.Sets.Platforms[Main.tile[bottomtilepointx, i].TileType])
                                    {
                                        NPC.velocity.Y = jumpVelocity;
                                        NPC.localAI[1] = 10;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else if (NPC.velocity.Y == 0f && target.Top.Y > NPC.Bottom.Y && Math.Abs(NPC.Center.X - target.Center.X) < (float)(target.width * 3) && Collision.CanHit(NPC, target))
                    {
                        int fluff = 6;
                        int bottomtilepointx = (int)(NPC.Center.X / 16f);
                        int bottomtilepointY = (int)(NPC.Bottom.Y / 16f);
                        for (int i = bottomtilepointY; i > bottomtilepointY - fluff - 1; i--)
                        {
                            if (Main.tile[bottomtilepointx, i].HasUnactuatedTile && TileID.Sets.Platforms[Main.tile[bottomtilepointx, i].TileType])
                            {
                                NPC.position.Y += 1;
                                NPC.stairFall = true;
                                NPC.velocity.Y += 0.01f;
                                break;
                            }
                        }
                    }
                }
            }

            if (NPC.velocity.Y >= 0f)
            {
                int dir = 0;
                if (NPC.velocity.X < 0f)
                    dir = -1;
                if (NPC.velocity.X > 0f)
                    dir = 1;

                Vector2 futurePos = NPC.position;
                futurePos.X += NPC.velocity.X;
                int tileX = (int)((futurePos.X + (float)(NPC.width / 2) + (float)((NPC.width / 2 + 1) * dir)) / 16f);
                int tileY = (int)((futurePos.Y + (float)NPC.height - 1f) / 16f);
                if (WorldGen.InWorld(tileX, tileY, 4))
                {
                    if ((float)(tileX * 16) < futurePos.X + (float)NPC.width && (float)(tileX * 16 + 16) > futurePos.X && ((Main.tile[tileX, tileY].HasUnactuatedTile && !TopSlope(Main.tile[tileX, tileY]) && !TopSlope(Main.tile[tileX, tileY - 1]) && Main.tileSolid[Main.tile[tileX, tileY].TileType] && !Main.tileSolidTop[Main.tile[tileX, tileY].TileType]) || (Main.tile[tileX, tileY - 1].IsHalfBlock && Main.tile[tileX, tileY - 1].HasUnactuatedTile)) && (!Main.tile[tileX, tileY - 1].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX, tileY - 1].TileType] || Main.tileSolidTop[Main.tile[tileX, tileY - 1].TileType] || (Main.tile[tileX, tileY - 1].IsHalfBlock && (!Main.tile[tileX, tileY - 4].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX, tileY - 4].TileType] || Main.tileSolidTop[Main.tile[tileX, tileY - 4].TileType]))) && (!Main.tile[tileX, tileY - 2].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX, tileY - 2].TileType] || Main.tileSolidTop[Main.tile[tileX, tileY - 2].TileType]) && (!Main.tile[tileX, tileY - 3].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX, tileY - 3].TileType] || Main.tileSolidTop[Main.tile[tileX, tileY - 3].TileType]) && (!Main.tile[tileX - dir, tileY - 3].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX - dir, tileY - 3].TileType]))
                    {
                        float tilePosY = tileY * 16;
                        if (Main.tile[tileX, tileY].IsHalfBlock)
                            tilePosY += 8f;

                        if (Main.tile[tileX, tileY - 1].IsHalfBlock)
                            tilePosY -= 8f;

                        if (tilePosY < futurePos.Y + (float)NPC.height)
                        {
                            float difference = futurePos.Y + (float)NPC.height - tilePosY;
                            if (difference <= 16.1f)
                            {
                                NPC.gfxOffY += NPC.position.Y + (float)NPC.height - tilePosY;
                                NPC.position.Y = tilePosY - (float)NPC.height;
                            }

                            if (difference < 9f)
                                NPC.stepSpeed = 1f;
                            else
                                NPC.stepSpeed = 2f;
                        }

                    }
                }
            }
        }
        public void ChooseAttack()
        {
            NPC.ai[1] = 0;
            List<Attack> potentialAttacks = new List<Attack>() { Charge, Throw, Slam, Summon };
            potentialAttacks.RemoveAll(x => x.Id == (int)NPC.ai[2]);

            if (target != null)
            {
                if ((target.Center - NPC.Center).Length() > 450f)
                    potentialAttacks.RemoveAll(x => x.Id == Throw.Id);
            }
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
            if (target != null)
            {
                if (NPC.ai[0] == Charge.Id)
                {
                    if (NPC.ai[1] >= chargeTelegraph && NPC.ai[1] <= chargeTelegraph + 1)
                    {
                        if (NPC.direction == -1)
                        {
                            if ((NPC.Center.X > target.Center.X - 104f && NPC.Bottom.Y >= target.Bottom.Y))
                                return false;
                            else
                                return true;
                        }
                        else
                        {
                            if ((NPC.Center.X < target.Center.X + 104f && NPC.Bottom.Y >= target.Bottom.Y))
                                return false;
                            else
                                return true;
                        }
                    }
                }
            }
            return null;
        }
        public void SlamEffect()
        {
            SoundEngine.PlaySound(SoundID.DeerclopsRubbleAttack with { Volume = 0.75f, Pitch = 0.5f }, NPC.Center);
            SoundEngine.PlaySound(SoundID.DeerclopsIceAttack with { Volume = 0.375f, Pitch = -0.2f }, NPC.Center);
            SoundEngine.PlaySound(SoundID.Item70 with { Volume = 0.375f, Pitch = -0.2f }, NPC.Center);
            for (int i = -15; i <= 15; i++)
            {
                Dust d = Dust.NewDustPerfect(NPC.Bottom + new Vector2(3 * i, 0), DustID.Smoke, null, 0, Color.LightGray, 1f);
                d.velocity.Y -= 1;
            }
            for (int i = -15; i <= 15; i++)
            {
                Dust d = Dust.NewDustPerfect(NPC.Bottom + new Vector2(0.75f * i, 0), DustID.Stone, new Vector2(Main.rand.NextFloat(0.05f, 0.15f) * i + (Math.Sign(i) * 1.5f), Main.rand.NextFloat(-4f, -2f)), 0, default, 1.2f);
            }
            for (int i = -2; i <= 2; i++)
            {
                int goreid = Main.rand.NextFromCollection(new List<int>() { GoreID.Smoke1, GoreID.Smoke2, GoreID.Smoke3 });
                Gore.NewGorePerfect(NPC.GetSource_FromThis(), NPC.Bottom + new Vector2(16 * i + (24 * NPC.direction), -16), new Vector2(Main.rand.NextFloat(-0.08f, 0.35f) * i, Main.rand.NextFloat(-1.2f, -0.7f)), goreid);
            }
            for (int i = -1; i <= 1; i += 2)
            {
                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(0, 10), new Vector2(i * 6, 16), ModContent.ProjectileType<Shockwave>(), NPC.damage, 0f);
            }
            for (int i = 0; i < 8; i++)
            {
                Point tilePos = Point.Zero;
                int valid = -1;
                for (int j = 0; j < 5; j++)
                {
                    tilePos = (NPC.Center + new Vector2(Main.rand.NextFloat(-400, 400) * NPC.direction, -480)).ToTileCoordinates();
                    Tile tile = ParanoidTileRetrieval(tilePos.X, tilePos.Y);
                    if (!tile.IsTileSolidGround(true))
                        continue;

                    for (int k = 1; k < 25; k++)
                    {
                        Tile belowTile = ParanoidTileRetrieval(tilePos.X, tilePos.Y + k);
                        if (!belowTile.IsTileSolidGround())
                        {
                            valid = k;
                            break;
                        }
                    }
                    if (valid != -1)
                        break;
                }
                if (valid != -1)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), new Vector2(tilePos.X * 16f, (tilePos.Y + valid - 2) * 16f) + new Vector2(8f), new Vector2(0, 7), ModContent.ProjectileType<RockDebris>(), NPC.damage, 0f, -1, Main.rand.Next(-25, 1));
                }
            }
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
        public override void OnKill()
        {
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 10; i++)
                {
                    int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, 16, 0f, 0f, 0, default(Color), 1.5f);
                    Dust d = Main.dust[dust];
                    d.velocity *= 2f;
                    d.noGravity = true;
                }
                Vector2 pos = new Vector2(NPC.position.X, NPC.position.Y);
                Vector2 velocity = default(Vector2);
                int gore = Gore.NewGore(NPC.GetSource_Death(), pos, velocity, Main.rand.Next(11, 14), NPC.scale);
                Gore g = Main.gore[gore];
                g.velocity *= 0.5f;

                pos = new Vector2(NPC.position.X, NPC.position.Y + 20f);
                gore = Gore.NewGore(NPC.GetSource_Death(), pos, velocity, Main.rand.Next(11, 14), NPC.scale);
                g = Main.gore[gore];
                g.velocity *= 0.5f;

                pos = new Vector2(NPC.position.X, NPC.position.Y + 40f);
                gore = Gore.NewGore(NPC.GetSource_Death(), pos, velocity, Main.rand.Next(11, 14), NPC.scale);
                g = Main.gore[gore];
                g.velocity *= 0.5f;
            }
        }
        public override void FindFrame(int frameHeight)
        {
            int frameCount = Main.npcFrameCount[Type];

            if (NPC.localAI[0] < -30)
            {
                if (NPC.localAI[0] < -150)
                    currentFrame = frameCount - 1;
                else if (NPC.localAI[0] < -120)
                    currentFrame = frameCount - 2;
                else if (NPC.localAI[0] >= -50)
                    currentFrame = 0;
                else if (NPC.localAI[0] >= -70)
                    currentFrame = frameCount - 3;
                else if (NPC.localAI[0] >= -90)
                    currentFrame = frameCount - 4;
            }
            else
            {
                if (NPC.ai[0] == Throw.Id)
                {
                    if (NPC.localAI[2] > 0)
                    {
                        currentFrame = NPC.localAI[2] > 10 ?  frameCount - 6 : frameCount - 5;
                    }
                    else
                    {
                        currentFrame = frameCount - 7;
                    }

                    NPC.frameCounter = 0;
                }
                else if (NPC.ai[0] == Charge.Id)
                {
                    if (NPC.ai[1] < chargeTelegraph)
                    {
                        currentFrame = frameCount - 4; 
                    }
                    else if (NPC.ai[1] >= chargeTelegraph && NPC.ai[1] <= chargeTelegraph + 1)
                    {
                        currentFrame = ((int)NPC.frameCounter % (frameCount - 9)) + 2;
                    }
                    else
                    {
                        currentFrame = 1;
                    }
                }
                else if (NPC.ai[0] == Slam.Id)
                {
                    int backOnGround = slamTelegraph + slamRise + slamFall;
                    if (NPC.ai[1] < slamTelegraph)
                    {
                        currentFrame = frameCount - 4;
                    }
                    else if (NPC.ai[1] < slamTelegraph + slamRise)
                    {
                        currentFrame = 1;
                    }
                    else if (NPC.ai[1] < backOnGround)
                    {
                        currentFrame = frameCount - 1;
                    }
                    else if (NPC.ai[1] < backOnGround + 60)
                    {
                        currentFrame = frameCount - 2;
                    }
                    else if (NPC.ai[1] < backOnGround + 80)
                    {
                        currentFrame = frameCount - 4;
                    }
                    else if (NPC.ai[1] < backOnGround + 100)
                    {
                        currentFrame = frameCount - 3;
                    }
                    else if (NPC.ai[1] < backOnGround + 120)
                    {
                        currentFrame = 0;
                    }
                }
                else
                {
                    currentFrame = NPC.localAI[1] < 10 ? ((int)NPC.frameCounter % (frameCount - 9)) + 2 : 1;
                }
            }

            NPC.frame = new Rectangle(0, currentFrame * frameHeight, TextureAssets.Npc[Type].Value.Width, frameHeight);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;
            int frameHeight = tex.Height / Main.npcFrameCount[Type];
            modNPC.drawCenter = new Vector2(0, -((frameHeight - NPC.height) * 0.5f) + 16);

            if (NPC.localAI[0] < -150)
            {
                modNPC.drawCenter.Y += MathHelper.Lerp(0, -1000, Math.Abs(NPC.localAI[0] + 150) / 60f);
            }
            if (NPC.ai[0] == Slam.Id)
            {
                modNPC.drawCenter.Y += AddedYOffset();
            }
            Vector2 drawPos = NPC.Center + modNPC.drawCenter + (Vector2.UnitY * NPC.gfxOffY);
            Main.EntitySpriteDraw(tex, drawPos - Main.screenPosition, NPC.frame, Lighting.GetColor(drawPos.ToTileCoordinates()), NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            if (NPC.localAI[0] < -90 || (NPC.ai[0] == Slam.Id && NPC.ai[1] >= slamTelegraph + slamRise && NPC.ai[1] < slamTelegraph + slamRise + slamFall + 80))
            {
                Vector2 hammerPos = new Vector2(-8f, 0) + spawnPos + (NPC.Bottom - NPC.Center);
                float hammerRot = MathHelper.Pi;
                if (NPC.ai[0] == Slam.Id && NPC.ai[1] >= slamTelegraph + slamRise && NPC.ai[1] < slamTelegraph + slamRise + slamFall + 80)
                {
                    float completion = MathHelper.Clamp((NPC.ai[1] - slamTelegraph - slamRise) / 30f, 0, 1f);
                    hammerPos.Y += MathHelper.Lerp(-200, 0, completion);
                    hammerRot += MathHelper.Lerp(MathHelper.TwoPi * 2, 0, completion);
                }
                Main.EntitySpriteDraw(hammerTex, hammerPos - Main.screenPosition, null, Lighting.GetColor(hammerPos.ToTileCoordinates()), hammerRot, hammerTex.Size() * 0.5f, 1f, SpriteEffects.None);
            }
            return false;
        }
        public float AddedYOffset()
        {
            if (NPC.ai[1] >= slamTelegraph && NPC.ai[1] <= slamTelegraph + slamRise)
            {
                return MathHelper.Lerp(0, -500, (NPC.ai[1] - slamTelegraph) / 20f);
            }
            if (NPC.ai[1] > slamTelegraph + slamRise && NPC.ai[1] <= slamTelegraph + slamRise + slamFall)
            {
                return MathHelper.Lerp(-1000, 0, (NPC.ai[1] - slamTelegraph - slamRise) / 60f);
            }
            return 0;
        }
    }
}
