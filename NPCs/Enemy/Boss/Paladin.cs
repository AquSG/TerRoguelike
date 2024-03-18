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
    public class Paladin : BaseRoguelikeNPC
    {
        List<GodRay> deathGodRays = new List<GodRay>();
        public Entity target;
        public Vector2 spawnPos;
        public Vector2[] SummonSpawnPositions = new Vector2[] { new Vector2(-1), new Vector2(-1) };
        public float acceleration = 0.05f;
        public float deceleration = 0.95f;
        public float xCap = 2f;
        public float chargeSpeed = 8f;
        public int chargeTelegraph = 60;
        public float jumpVelocity = -7.9f;
        public int slamTelegraph = 40;
        public int slamRise = 60;
        public int slamFall = 60;
        public int summonTelegraph = 60;
        public int desiredEnemy;
        public bool ableToHit = true;
        public static readonly SoundStyle HammerRaise = new SoundStyle("TerRoguelike/Sounds/HammerRaise", 4);
        public static readonly SoundStyle HammerLand = new SoundStyle("TerRoguelike/Sounds/HammerLand", 5);
        public override int modNPCID => ModContent.NPCType<Paladin>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Base"] };
        public override int CombatStyle => -1;
        public int currentFrame;
        public Texture2D hammerTex;
        public Texture2D godRayTex;
        public int deadTime = 0;

        public Attack None = new Attack(0, 0, 300);
        public Attack Charge = new Attack(1, 30, 210);
        public Attack Throw = new Attack(2, 40, 240);
        public Attack Slam = new Attack(3, 20, 280);
        public Attack Summon = new Attack(4, 20, 150);

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
            NPC.damage = 40;
            NPC.lifeMax = 20000;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.NPCDeath6;
            NPC.knockBackResist = 0f;
            modNPC.drawCenter = new Vector2(0, -12);
            hammerTex = TexDict["PaladinHammer"].Value;
            godRayTex = TexDict["GodRay"].Value;
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
            if (deadTime > 0)
            {
                CheckDead();
                return;
            }
            if (modNPC.isRoomNPC && NPC.localAI[0] == -270)
            {
                SetBossTrack(PaladinTheme);
            }

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
        public void BossAI()
        {
            target = modNPC.GetTarget(NPC, false, false);

            acceleration = 0.05f;
            deceleration = 0.88f;
            xCap = target == null ? 0 : 2f;
            chargeSpeed = 8f;
            chargeTelegraph = 60;
            jumpVelocity = -7.9f;
            desiredEnemy = ModContent.NPCType<UndeadGuard>();

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
                            SoundEngine.PlaySound(SoundID.DeerclopsStep with { Volume = 0.7f, Pitch = -0.1f, MaxInstances = 8 }, NPC.Center);
                            SoundEngine.PlaySound(SoundID.NPCHit4 with { Volume = 0.10f, Pitch = -0.9f, PitchVariance = 0.08f, MaxInstances = 8 }, NPC.Center);
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
                        SoundEngine.PlaySound(new SoundStyle("TerRoguelike/Sounds/Ding") with { Volume = 0.12f, MaxInstances = 3, Pitch = -0.5f, PitchVariance = 0.025f }, NPC.Center);
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
                    SoundEngine.PlaySound(SoundID.DeerclopsStep with { Volume = 0.62f, Pitch = -0.2f }, NPC.Center);
                    SoundEngine.PlaySound(SoundID.NPCHit4 with { Volume = 0.08f, Pitch = -0.9f, PitchVariance = 0.08f }, NPC.Center);
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
                    SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Volume = 1f, Pitch = -0.25f }, NPC.Center);
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
                    SoundEngine.PlaySound(HammerLand with { Volume = 0.38f }, NPC.Center);
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
                if (NPC.ai[1] == 0)
                {
                    SoundEngine.PlaySound(HammerRaise with { Volume = 0.6f }, NPC.Center);

                    NPC dummyNPC = new NPC();
                    dummyNPC.type = desiredEnemy;
                    dummyNPC.SetDefaults(desiredEnemy);
                    Rectangle dummyRect = new Rectangle(0, 0, dummyNPC.width, dummyNPC.height);
                    for (int i = 0; i < SummonSpawnPositions.Length; i++)
                    {
                        int direction = i == 0 ? -1 : 1;
                        float distanceBeside = 112f * direction;
                        Rectangle plannedRect = new Rectangle((int)(NPC.Center.X + distanceBeside - (dummyRect.Width * 0.5f)), (int)(NPC.Center.Y - (dummyRect.Height * 0.5f)), dummyRect.Width, dummyRect.Height);
                        if (modNPC.isRoomNPC)
                        {
                            plannedRect = RoomList[modNPC.sourceRoomListID].CheckRectWithWallCollision(plannedRect);
                        }
                        Vector2 position = plannedRect.Center.ToVector2();

                        Point tilePos = new Vector2(position.X, plannedRect.Bottom).ToTileCoordinates();

                        if (ParanoidTileRetrieval(tilePos.X, tilePos.Y).IsTileSolidGround(true))
                        {
                            bool found = false;
                            for (int y = 0; y < 25; y++)
                            {
                                for (int d = -1; d <= 1; d += 2)
                                {
                                    if (!ParanoidTileRetrieval(tilePos.X, tilePos.Y + (y * d)).IsTileSolidGround(true))
                                    {
                                        float offset = y * d * 16f;
                                        if (modNPC.isRoomNPC)
                                        {
                                            Rectangle rectCheck = new Rectangle(plannedRect.X, (int)(plannedRect.Y + offset), plannedRect.Width, plannedRect.Height);
                                            rectCheck = RoomList[modNPC.sourceRoomListID].CheckRectWithWallCollision(rectCheck);
                                            Vector2 posCheck = new Vector2(rectCheck.Center.X, rectCheck.Bottom);

                                            Point tilePosCheck = posCheck.ToTileCoordinates();
                                            if (!ParanoidTileRetrieval(tilePosCheck.X, tilePosCheck.Y).IsTileSolidGround(true))
                                            {
                                                found = true;
                                                position.Y += rectCheck.Y - plannedRect.Y;
                                            }
                                        }
                                        else
                                        {
                                            found = true;
                                            position.Y += offset;
                                        }
                                    }
                                }
                                if (found)
                                    break;
                            }
                            if (found)
                                SummonSpawnPositions[i] = position;
                        }
                        else
                        {
                            SummonSpawnPositions[i] = position;
                        }
                    }
                }
                if (NPC.ai[1] < summonTelegraph)
                {
                    int time = 20;
                    Vector2 startPos = NPC.Top + new Vector2(-12 * NPC.direction, -8);
                    float completion = NPC.ai[1] / summonTelegraph;
                    float curveMultiplier = 1f - (float)Math.Pow(Math.Abs(completion - 0.5f) * 2, 2);
                    for (int i = 0; i < SummonSpawnPositions.Length; i++)
                    {
                        Vector2 endPos = SummonSpawnPositions[i];
                        if (endPos == new Vector2(-1))
                            continue;
                        endPos += new Vector2(0, 16);

                        Vector2 particlePos = startPos + ((endPos - startPos) * completion) + (Vector2.UnitY * -32 * curveMultiplier);
                        particlePos += new Vector2(Main.rand.NextFloat(-10, 10), Main.rand.NextFloat(-10, 10));

                        ParticleManager.AddParticle(new Square(particlePos, Vector2.Zero, time, Color.HotPink, new Vector2((4f + Main.rand.NextFloat(-0.5f, 0.5f)) * 0.45f), 0, 0.96f, time, true));
                    }
                }
                else if (NPC.ai[1] == summonTelegraph)
                {
                    SoundEngine.PlaySound(SoundID.DD2_WitherBeastAuraPulse with { Volume = 1f }, NPC.Center);
                    SoundEngine.PlaySound(SoundID.DD2_SkeletonSummoned with { Volume = 1f }, NPC.Center);
                    for (int i = 0; i < SummonSpawnPositions.Length; i++)
                    {
                        Vector2 pos = SummonSpawnPositions[i];
                        if (pos == new Vector2(-1))
                            continue;

                        SpawnManager.SpawnEnemy(desiredEnemy, pos, modNPC.sourceRoomListID, 60, 0.45f);

                        SummonSpawnPositions[i] = new Vector2(-1);
                    }
                }

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
        public override bool CheckDead()
        {
            if (deadTime >= 150)
            {
                return true;
            }
            NPC.ai[0] = None.Id;
            NPC.ai[1] = 1;

            modNPC.OverrideIgniteVisual = true;
            NPC.velocity.X = 0;
            NPC.life = 1;
            NPC.immortal = true;
            NPC.dontTakeDamage = true;
            NPC.active = true;
            if (deadTime == 0)
            {
                CutsceneSystem.SetCutscene(NPC.Center, 210, 30, 30, 2.5f);
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
            }
            if (deadTime % 20 == 0 || (deadTime > 90 && deadTime % 14 == 0))
            {
                if (deadTime != 0)
                    deathGodRays.Add(new GodRay(Main.rand.NextFloat(MathHelper.TwoPi), deadTime, new Vector2(0.16f + Main.rand.NextFloat(-0.02f, 0.02f), 0.025f)));
                SoundEngine.PlaySound(HammerLand with { Volume = 0.15f, MaxInstances = 10, Pitch = 0.67f, PitchVariance = 0.06f }, NPC.Center); ;
            }
            deadTime++;

            if (deadTime >= 150)
            {
                NPC.immortal = false;
                NPC.dontTakeDamage = false;
                NPC.StrikeInstantKill();
            }
                
            return deadTime >= 150;
        }
        public override void OnKill()
        {
            SoundEngine.PlaySound(SoundID.NPCDeath12 with { Volume = 0.8f, Pitch = -0.5f }, NPC.Center);
            SoundEngine.PlaySound(HammerLand with { Volume = 0.5f, MaxInstances = 10 }, NPC.Center);
            Gore.NewGore(NPC.GetSource_Death(), NPC.Center + new Vector2(14 * NPC.direction, 0), (Vector2.UnitX * NPC.direction * 3), Mod.Find<ModGore>("Paladin1").Type);
            Gore.NewGore(NPC.GetSource_Death(), NPC.Center + new Vector2(0, -14), (-Vector2.UnitY * 3), Mod.Find<ModGore>("Paladin2").Type);
            Gore.NewGore(NPC.GetSource_Death(), NPC.Center + new Vector2(-14 * NPC.direction, 14), (-Vector2.UnitX * NPC.direction * 3), Mod.Find<ModGore>("Paladin3").Type);
            Gore.NewGore(NPC.GetSource_Death(), NPC.Center + new Vector2(10 * NPC.direction, 14), (Vector2.UnitX * NPC.direction * 3), Mod.Find<ModGore>("Paladin4").Type);
            Gore.NewGore(NPC.GetSource_Death(), NPC.Center + new Vector2(-18 * NPC.direction, -8), (-Vector2.UnitX * NPC.direction * 3), Mod.Find<ModGore>("Paladin5").Type);
            Gore.NewGore(NPC.GetSource_Death(), NPC.Center + new Vector2(1 * NPC.direction, 0), (-Vector2.UnitY * 3), Mod.Find<ModGore>("Paladin6").Type);
            Gore.NewGore(NPC.GetSource_Death(), NPC.Center + new Vector2(-18 * NPC.direction, 0), (-Vector2.UnitX * NPC.direction * 3) + (-Vector2.UnitY * 3), Mod.Find<ModGore>("Paladin7").Type);
            for (int i = 0; i < 60; i++)
            {
                Vector2 pos = NPC.Center + new Vector2(0, 16);
                Vector2 velocity = new Vector2(0, -4f).RotatedBy(Main.rand.NextFloat(-MathHelper.PiOver4 * 1.5f, MathHelper.PiOver4 * 1.5f));
                velocity *= Main.rand.NextFloat(0.3f, 1f);
                if (Main.rand.NextBool(5))
                    velocity *= 1.5f;
                Vector2 scale = new Vector2(0.25f, 0.4f);
                int time = 110 + Main.rand.Next(70);
                ParticleManager.AddParticle(new Blood(pos, velocity, time, Color.Black * 0.65f, scale, velocity.ToRotation(), false));
                ParticleManager.AddParticle(new Blood(pos, velocity, time, Color.Red* 0.65f, scale, velocity.ToRotation(), true));
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
            else if (deadTime > 0)
            {
                currentFrame = 1;
            }
            else
            {
                if (NPC.ai[0] == Throw.Id)
                {
                    if (NPC.localAI[2] > 0)
                    {
                        currentFrame = NPC.localAI[2] > 10 ? frameCount - 6 : frameCount - 5;
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
                else if (NPC.ai[0] == Summon.Id)
                {
                    if (NPC.ai[1] <= summonTelegraph + 75)
                    {
                        currentFrame = frameCount - 7;
                    }
                    else
                    {
                        currentFrame = 0;
                        NPC.frameCounter = 0;
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

            if (deadTime > 0)
            {
                modNPC.drawCenter.X += Main.rand.NextFloat(-2f, 2f) * ((deadTime + 50) / 200f);
                if (deathGodRays.Any())
                {
                    StartAdditiveSpritebatch();
                    for (int i = 0; i < deathGodRays.Count; i++)
                    {
                        int direction = i == 0 ? -1 : 1;
                        GodRay ray = deathGodRays[i];
                        float rotation = ray.rotation;
                        Vector2 scale = ray.scale;
                        int time = ray.time;
                        rotation += (deadTime - time) * 0.0033f * direction * ((i + 5) / 5);
                        float opacity = MathHelper.Clamp(MathHelper.Lerp(1f, 0.5f, (deadTime - time) / 60f), 0.5f, 1f);
                        Main.EntitySpriteDraw(godRayTex, NPC.Center - Main.screenPosition, null, Color.White * opacity, rotation, new Vector2(0, godRayTex.Height * 0.5f), scale, SpriteEffects.None);
                    }
                    StartVanillaSpritebatch();
                }
            }

            if (NPC.ai[0] == Summon.Id)
            {
                if (NPC.ai[1] <= summonTelegraph + 75)
                {
                    Vector2 hammerRaisePos = NPC.Top + new Vector2(-12 * NPC.direction, -8);
                    Main.EntitySpriteDraw(hammerTex, hammerRaisePos - Main.screenPosition, null, drawColor, -MathHelper.PiOver4, hammerTex.Size() * 0.5f, NPC.scale, SpriteEffects.None);
                }
            }

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
            if (NPC.localAI[0] < -90 || (NPC.ai[0] == Slam.Id && NPC.ai[1] >= slamTelegraph + slamRise - 60 && NPC.ai[1] < slamTelegraph + slamRise + slamFall + 80))
            {
                Vector2 hammerPos = new Vector2(-8f, 0) + spawnPos + (NPC.Bottom - NPC.Center);
                float hammerRot = MathHelper.Pi;
                if (NPC.ai[0] == Slam.Id && NPC.ai[1] >= slamTelegraph + slamRise - 60 && NPC.ai[1] < slamTelegraph + slamRise + slamFall + 80)
                {
                    float completion = MathHelper.Clamp((NPC.ai[1] - slamTelegraph - slamRise) / 30f, -5, 1f);
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
    public class GodRay
    {
        public float rotation;
        public int time;
        public Vector2 scale;
        public GodRay(float Rotation, int Time, Vector2 Scale)
        {
            rotation = Rotation;
            time = Time;
            scale = Scale;
        }
    }
}
