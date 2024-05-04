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
using static TerRoguelike.Managers.TextureManager;
using static TerRoguelike.Schematics.SchematicManager;
using static TerRoguelike.Systems.MusicSystem;
using static TerRoguelike.Systems.RoomSystem;
using static TerRoguelike.Utilities.TerRoguelikeUtils;

namespace TerRoguelike.NPCs.Enemy.Boss
{
    public class WallOfFlesh : BaseRoguelikeNPC
    {
        public Entity target;
        public Vector2 spawnPos;
        public bool ableToHit = true;
        public bool canBeHit = true;
        public override int modNPCID => ModContent.NPCType<WallOfFlesh>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Hell"] };
        public override int CombatStyle => -1;
        public int currentFrame;
        public bool CollisionPass = false;
        public List<ExtraHitbox> hitboxes = new List<ExtraHitbox>()
        {
            new ExtraHitbox(new Point(128, 1104), new Vector2(32, 0)),
            new ExtraHitbox(new Point(80, 80), new Vector2(-32, -28)),
            new ExtraHitbox(new Point(80, 80), new Vector2(-32, -331.2f)),
            new ExtraHitbox(new Point(80, 80), new Vector2(-32, 303.6f)),
        };
        public static readonly SoundStyle HellBeamSound = new SoundStyle("TerRoguelike/Sounds/HellBeam");
        public static readonly SoundStyle HellBeamCharge = new SoundStyle("TerRoguelike/Sounds/DeathrayCharge");
        public Texture2D squareTex;
        public Texture2D bodyTex;
        public Texture2D eyeTex;
        public Texture2D mouthTex;
        public Texture2D godRayTex;
        public SlotId DeathraySlot;
        public float topEyeRotation = MathHelper.Pi;
        public float bottomEyeRotation = MathHelper.Pi;
        public float mouthRotation = MathHelper.Pi;
        public bool positionsinitialized = false;
        public double mouthFrameCounter = 0;

        public int deadTime = 0;
        public int cutsceneDuration = 240;
        public int deathCutsceneDuration = 180;
        List<GodRay> deathGodRays = [];

        public static Attack None = new Attack(0, 0, 100);
        public static Attack Laser = new Attack(1, 30, 180);
        public static Attack Deathray = new Attack(2, 40, 330);
        public static Attack BouncyBall = new Attack(3, 30, 180);
        public static Attack Bloodball = new Attack(4, 30, 60);
        public static Attack Summon = new Attack(5, 50, 180);
        public int laserStartup = 60;
        public int laserFireRate = 8;
        public int laserVomitStartTime = 120;
        public int laserVomitStartup = 40;
        public int laserVomitFireRate = 21;
        public int deathrayTrackedProjId1 = -1;
        public int deathrayTrackedProjId2 = -1;
        public int bouncyballFireRate = 60;
        public int bouncyballWindup = 40;
        public int bloodBallFireRate = 11;
        public int summonCap = 10;

        public override void SetStaticDefaults()
        {
            SoundEngine.PlaySound(HellBeamSound with { Volume = 0 });
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 80;
            NPC.height = 80;
            NPC.aiStyle = -1;
            NPC.damage = 35;
            NPC.lifeMax = 32000;
            NPC.knockBackResist = 0f;
            modNPC.drawCenter = new Vector2(0, -32);
            modNPC.IgnoreRoomWallCollision = true;
            modNPC.SpecialProjectileCollisionRules = true;
            modNPC.OverrideIgniteVisual = true;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            NPC.behindTiles = true;
            squareTex = TexDict["Square"].Value;
            bodyTex = TexDict["WallOfFleshBody"].Value;
            eyeTex = TexDict["WallOfFleshEye"].Value;
            mouthTex = TexDict["WallOfFleshMouth"].Value;
            godRayTex = TexDict["GodRay"].Value;
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
        }
        public override void PostAI()
        {
            NPC.localAI[1]++;
            if (target != null)
            {
                bool hellbeam = NPC.ai[0] == Deathray.Id && NPC.ai[1] >= 90;
                float angleLerpSpeed = 0.12f;
                float angleBound = MathHelper.PiOver2 * 0.999f;


                float angleToTarget = (target.Center - (NPC.Center + hitboxes[2].offset)).ToRotation();
                angleToTarget = MathHelper.Clamp(AngleSizeBetween(MathHelper.Pi, angleToTarget), -angleBound, angleBound) + MathHelper.Pi;
                AngleCalculation(ref topEyeRotation);

                angleToTarget = (target.Center - (NPC.Center + hitboxes[3].offset)).ToRotation();
                angleToTarget = MathHelper.Clamp(AngleSizeBetween(MathHelper.Pi, angleToTarget), -angleBound, angleBound) + MathHelper.Pi;
                AngleCalculation(ref bottomEyeRotation);

                angleToTarget = (target.Center - (NPC.Center + hitboxes[1].offset)).ToRotation();
                angleToTarget = MathHelper.Clamp(AngleSizeBetween(MathHelper.Pi, angleToTarget), -angleBound, angleBound) + MathHelper.Pi;
                AngleCalculation(ref mouthRotation);

                void AngleCalculation(ref float setAngle)
                {
                    if (hellbeam)
                    {
                        float potentialAngle = setAngle.AngleLerp(angleToTarget, 0.07f);
                        float clamp = 0.01f;
                        float addAngle = MathHelper.Clamp(AngleSizeBetween(setAngle, potentialAngle), -clamp, clamp);
                        setAngle += addAngle;
                    }
                    else
                    {
                        setAngle = setAngle.AngleLerp(angleToTarget, angleLerpSpeed);
                    }
                }
            }
            if (SoundEngine.TryGetActiveSound(DeathraySlot, out var sound) && sound.IsPlaying)
            {
                Vector2 basePos = (NPC.Center + hitboxes[1].offset);
                if (NPC.ai[0] != Deathray.Id)
                {
                    sound.Position += (basePos - (Vector2)sound.Position).SafeNormalize(Vector2.UnitY) * 1.2f;
                    if (sound.Pitch > -0.15f)
                        sound.Pitch -= 0.008f;
                    else
                        sound.Pitch -= 0.003f;
                    sound.Volume -= 0.008f;
                    if (sound.Volume <= 0)
                        sound.Stop();
                }
                else
                {
                    float time = (NPC.ai[1] - 90);
                    sound.Position = basePos + (Main.LocalPlayer.Center - basePos) * 0.75f * MathHelper.Clamp(time / 90, 0, 1);
                    if (time < 120)
                    {
                        sound.Pitch += 0.45f / 120;
                    }
                    else
                    {
                        sound.Pitch += 0.12f / 120;
                    }
                }
            }
            NPC.ai[3] = 0;
        }
        public override void AI()
        {
            if (!positionsinitialized)
            {
                positionsinitialized = true;
                if (modNPC.isRoomNPC)
                {
                    Room room = RoomList[modNPC.sourceRoomListID];
                    hitboxes[0].dimensions.Y = (int)room.RoomDimensions16.Y;
                    hitboxes[1].offset.Y = -28;
                    hitboxes[2].offset.Y = hitboxes[0].dimensions.Y * -0.3f;
                    hitboxes[3].offset.Y = hitboxes[0].dimensions.Y * 0.275f;
                }
                Vector2 basePos = hitboxes[0].offset + hitboxes[0].dimensions.ToVector2() * -0.5f;
                for (int i = 0; i < hitboxes[0].dimensions.Y; i += 32)
                {
                    modNPC.ExtraIgniteTargetPoints.Add(basePos + i * Vector2.UnitY);
                }
                for (int i = 1; i < hitboxes.Count; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        modNPC.ExtraIgniteTargetPoints.Add(hitboxes[i].offset + hitboxes[i].dimensions.ToVector2() * new Vector2(-0.5f, -0.5f * j));
                    }
                }
            }
            NPC.frameCounter += 0.1d;
            mouthFrameCounter += 0.1d;

            if (deadTime > 0)
            {
                CheckDead();
                return;
            }
            if (modNPC.isRoomNPC && NPC.localAI[0] == -(cutsceneDuration + 30))
            {
                SetBossTrack(QueenBeeTheme);
            }

            ableToHit = NPC.localAI[0] >= 0;
            canBeHit = true;

            if (NPC.localAI[0] < 0)
            {
                target = modNPC.GetTarget(NPC);

                if (NPC.localAI[0] == -cutsceneDuration)
                {
                    Vector2 soundPos = NPC.Center + hitboxes[1].offset;
                    SoundEngine.PlaySound(SoundID.DD2_WitherBeastAuraPulse with { Volume = 1f, Pitch = -1f, MaxInstances = 2 }, soundPos);
                    SoundEngine.PlaySound(SoundID.DD2_SkeletonSummoned with { Volume = 1f, Pitch = -1f, MaxInstances = 2 }, soundPos);
                    CutsceneSystem.SetCutscene(NPC.Center + hitboxes[1].offset.X * Vector2.UnitX, cutsceneDuration, 30, 30, 1.25f);
                }
                NPC.localAI[0]++;

                if (NPC.localAI[0] == -cutsceneDuration + 60)
                {
                    Vector2 soundPos = NPC.Center + hitboxes[1].offset;
                    SoundEngine.PlaySound(SoundID.DD2_WitherBeastAuraPulse with { Volume = 0.5f, Pitch = -1f, MaxInstances = 2 }, soundPos);
                    SoundEngine.PlaySound(SoundID.DD2_SkeletonSummoned with { Volume = 0.5f, Pitch = -1f, MaxInstances =2 }, soundPos);
                }
                if (NPC.localAI[0] < -30)
                {
                    Vector2 basePos = NPC.Center;
                    basePos = TileCollidePositionInLine(basePos, basePos + new Vector2(500, 0));
                    basePos -= Vector2.UnitX * 2;
                    Vector2 topPos = TileCollidePositionInLine(basePos, basePos + new Vector2(0, -1000));
                    Vector2 bottomPos = TileCollidePositionInLine(basePos, basePos + new Vector2(0, 1000));
                    float range = topPos.Distance(bottomPos);
                    int amount = 30;
                    if (NPC.localAI[0] > -90)
                    {
                        amount = (-(int)NPC.localAI[0] - 30) / 2;
                    }
                    for (int i = 0; i < amount; i++)
                    {
                        Vector2 randPos = topPos + new Vector2(0, Main.rand.NextFloat(range));
                        Color color = Color.Lerp(Color.Red, Color.DarkRed, Main.rand.NextFloat());
                        Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                        vel -= Vector2.UnitX * 0.75f;
                        ParticleManager.AddParticle(new Ball(
                            randPos, vel,
                            20, color, new Vector2(0.25f) * Main.rand.NextFloat(0.7f, 1f), 0, 0.96f, 15));
                    }
                }
                if (NPC.localAI[0] >= -60)
                {
                    if (NPC.localAI[0] == -60)
                    {
                        NPC.ai[1] = -1;
                        NPC.ai[0] = Summon.Id;
                    }
                    else if (NPC.localAI[0] == -30)
                    {
                        NPC.immortal = false;
                        NPC.dontTakeDamage = false;  
                    }
                    BossAI();
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
                if (NPC.ai[1] < laserStartup)
                {
                    if (NPC.ai[1] == 0 || NPC.ai[1] == 24)
                    {
                        SoundEngine.PlaySound(SoundID.Item13 with { Volume = 0.7f, Pitch = 0 }, NPC.Center + hitboxes[1].offset);
                    }
                    for (int i = 0; i <= 1; i++)
                    {
                        Vector2 pos = NPC.Center + (i == 0 ? hitboxes[2].offset : hitboxes[3].offset);
                        float rot = i == 0 ? topEyeRotation : bottomEyeRotation;
                        pos += rot.ToRotationVector2() * 44;

                        Vector2 offset = Main.rand.NextVector2CircularEdge(16, 24).RotatedBy(rot);
                        ParticleManager.AddParticle(new Square(
                            pos + offset, -offset.SafeNormalize(Vector2.UnitY) * 1f,
                            20, Color.MediumPurple, new Vector2(1), offset.ToRotation(), 0.96f, 10, true));
                    }
                }
                else
                {
                    int time = (int)NPC.ai[1] - laserStartup;
                    if (time % laserFireRate == 0)
                    {
                        for (int i = 0; i <= 1; i++)
                        {
                            Vector2 pos = NPC.Center + (i == 0 ? hitboxes[2].offset : hitboxes[3].offset);
                            float rot = i == 0 ? topEyeRotation : bottomEyeRotation;
                            pos += rot.ToRotationVector2() * 44;
                            SoundEngine.PlaySound(SoundID.Item33 with { Volume = 0.3f, Pitch = 0.5f, PitchVariance = 0.04f, MaxInstances = 2 }, pos);

                            Projectile.NewProjectile(NPC.GetSource_FromThis(), pos, rot.ToRotationVector2() * 12, ModContent.ProjectileType<DemonLaser>(), NPC.damage, 0);
                        }

                    }
                    int vomitTime = (int)NPC.ai[1] - laserVomitStartTime;
                    if (vomitTime > -laserVomitStartup && vomitTime < 0)
                    {
                        mouthFrameCounter = 1;
                        Vector2 pos = NPC.Center + hitboxes[1].offset;
                        float rot = mouthRotation;
                        pos += rot.ToRotationVector2() * 36;

                        Vector2 offset = Main.rand.NextVector2CircularEdge(16, 24).RotatedBy(rot);
                        ParticleManager.AddParticle(new Square(
                            pos + offset, -offset.SafeNormalize(Vector2.UnitY) * 1f,
                            20, Color.OrangeRed, new Vector2(1), offset.ToRotation(), 0.96f, 10, true));
                    }
                    else if (vomitTime >= 0 && vomitTime % laserVomitFireRate == 0)
                    {
                        mouthFrameCounter = 0;
                        Vector2 pos = NPC.Center + hitboxes[1].offset;
                        float rot = mouthRotation;
                        pos += rot.ToRotationVector2() * 36;
                        Vector2 extraSpeed = target != null ? (target.Center - pos) * 0.005f : Vector2.Zero;

                        SoundEngine.PlaySound(SoundID.NPCDeath13 with { Volume = 0.8f, Pitch = -0.1f }, NPC.Center);
                        for (int i = -6; i <= 6; i++)
                        {
                            float randRot = Main.rand.NextFloat(-0.15f, 0.15f) + rot;
                            randRot += 0.075f * i;
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), pos, (randRot.ToRotationVector2() * Main.rand.NextFloat(4.8f, 6f)) - Vector2.UnitY + (extraSpeed.ToRotation().AngleLerp(-MathHelper.PiOver2, 0.2f).ToRotationVector2() * extraSpeed.Length()), ModContent.ProjectileType<FleshVomit>(), NPC.damage, 0);
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
            else if (NPC.ai[0] == Deathray.Id)
            {
                if (NPC.ai[1] == 0)
                {
                    SoundEngine.PlaySound(HellBeamCharge with { Volume = 0.45f, Pitch = 0.32f }, NPC.Center + hitboxes[1].offset);
                }
                if (NPC.ai[1] < 90)
                {
                    Color outlineColor = Color.Lerp(Color.LightPink, Color.OrangeRed, 0.13f);
                    Color fillColor = Color.Lerp(outlineColor, Color.DarkRed, 0.2f);
                    for (int j = -6; j <= 6; j++)
                    {
                        if (!Main.rand.NextBool(12) || j == 0)
                            continue;
                        Vector2 offset = Main.rand.NextVector2CircularEdge(16, 24);
                        Vector2 particleSpawnPos = NPC.Center + hitboxes[2].offset + topEyeRotation.ToRotationVector2() * 44 + offset.RotatedBy(topEyeRotation);
                        Vector2 particleVel = -offset.RotatedBy(topEyeRotation).SafeNormalize(Vector2.UnitX) * 0.8f;
                        ParticleManager.AddParticle(new BallOutlined(
                            particleSpawnPos, particleVel,
                            20, outlineColor, fillColor, new Vector2(Main.rand.NextFloat(0.14f, 0.28f)), 4, 0, 0.99f, 10));

                        offset = Main.rand.NextVector2CircularEdge(16, 24);
                        particleSpawnPos = NPC.Center + hitboxes[3].offset + bottomEyeRotation.ToRotationVector2() * 44 + offset.RotatedBy(bottomEyeRotation);
                        particleVel = -offset.RotatedBy(bottomEyeRotation).SafeNormalize(Vector2.UnitX) * 0.8f;
                        ParticleManager.AddParticle(new BallOutlined(
                            particleSpawnPos, particleVel,
                            20, outlineColor, fillColor, new Vector2(Main.rand.NextFloat(0.14f, 0.28f)), 4, 0, 0.99f, 10));
                    }
                }
                if (NPC.ai[1] == 90)
                {
                    DeathraySlot = SoundEngine.PlaySound(HellBeamSound with { Volume = 1f }, NPC.Center + hitboxes[1].offset);
                    deathrayTrackedProjId1 = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + hitboxes[2].offset + topEyeRotation.ToRotationVector2() * 42, Vector2.Zero, ModContent.ProjectileType<HellBeam>(), NPC.damage, 0);
                    Main.projectile[deathrayTrackedProjId1].rotation = topEyeRotation;
                    deathrayTrackedProjId2 = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + hitboxes[3].offset + bottomEyeRotation.ToRotationVector2() * 42, Vector2.Zero, ModContent.ProjectileType<HellBeam>(), NPC.damage, 0);
                    Main.projectile[deathrayTrackedProjId2].rotation = bottomEyeRotation;
                }
                if (deathrayTrackedProjId1 >= 0)
                {
                    var proj = Main.projectile[deathrayTrackedProjId1];
                    if (proj.type != ModContent.ProjectileType<HellBeam>())
                    {
                        deathrayTrackedProjId1 = -1;
                    }
                    else
                    {
                        proj.Center = NPC.Center + hitboxes[2].offset + topEyeRotation.ToRotationVector2() * 42;
                        proj.rotation = topEyeRotation;
                    }
                }
                if (deathrayTrackedProjId2 >= 0)
                {
                    var proj = Main.projectile[deathrayTrackedProjId2];
                    if (proj.type != ModContent.ProjectileType<HellBeam>())
                    {
                        deathrayTrackedProjId2 = -1;
                    }
                    else
                    {
                        proj.Center = NPC.Center + hitboxes[3].offset + bottomEyeRotation.ToRotationVector2() * 42;
                        proj.rotation = bottomEyeRotation;
                    }
                }
                if (NPC.ai[1] >= Deathray.Duration)
                {
                    deathrayTrackedProjId1 = -1;
                    deathrayTrackedProjId2 = -1;
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Deathray.Id;
                }
            }
            else if (NPC.ai[0] == BouncyBall.Id)
            {
                int time = ((int)NPC.ai[1] % bouncyballFireRate);
                if (NPC.ai[1] > 0 && time == 0)
                {
                    mouthFrameCounter = 0;
                    Vector2 pos = NPC.Center + hitboxes[1].offset;
                    float rot = mouthRotation;
                    pos += rot.ToRotationVector2() * 12;

                    SoundEngine.PlaySound(SoundID.NPCDeath13 with { Volume = 0.8f, Pitch = -0.1f }, NPC.Center);
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), pos, rot.ToRotationVector2() * 6, ModContent.ProjectileType<FleshBall>(), NPC.damage, 0);
                }
                if (time >= 30)
                {
                    mouthFrameCounter = 1;
                    Vector2 pos = NPC.Center + hitboxes[1].offset;
                    float rot = mouthRotation;
                    pos += rot.ToRotationVector2() * 24;

                    Vector2 offset = Main.rand.NextVector2CircularEdge(16, 24).RotatedBy(rot);
                    ParticleManager.AddParticle(new Square(
                        pos + offset, -offset.SafeNormalize(Vector2.UnitY) * 1f,
                        20, Color.OrangeRed, new Vector2(1), offset.ToRotation(), 0.96f, 10, true));
                }
                if (NPC.ai[1] >= BouncyBall.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = -180;
                    NPC.ai[2] = BouncyBall.Id;
                }
            }
            else if (NPC.ai[0] == Bloodball.Id)
            {
                if (NPC.ai[1] % bloodBallFireRate == 0)
                {
                    int number = (int)NPC.ai[1] / bloodBallFireRate;
                    Vector2 startPos;
                    if (number % 2 == 0)
                    {
                        startPos = NPC.Center + hitboxes[2].offset + topEyeRotation.ToRotationVector2() * 52;
                    }
                    else
                    {
                        startPos = NPC.Center + hitboxes[3].offset + bottomEyeRotation.ToRotationVector2() * 52;
                    }
                    startPos += Vector2.UnitY * -4;
                    Vector2 pos = target != null ? target.Center : spawnPos + hitboxes[1].offset + new Vector2(-950, 0);
                    Vector2 offset = Vector2.Zero;
                    Vector2 extra = target != null ? target.velocity * 5 : Vector2.Zero;
                    for (int i = 0; i < 70; i++)
                    {
                        offset = Main.rand.NextVector2CircularEdge(320, 320) * Main.rand.NextFloat(0.8f, 1f) + extra;
                        Vector2 potentialPos = pos + offset;
                        if (potentialPos.X < NPC.position.X - 80 && !ParanoidTileRetrieval((potentialPos).ToTileCoordinates()).IsTileSolidGround(true))
                            break;
                    }
                    pos += offset;
                    SoundEngine.PlaySound(SoundID.Item73 with { Volume = 0.5f, Pitch = -0.5f, PitchVariance = 0.05f, MaxInstances = 6 }, pos);
                    SoundEngine.PlaySound(SoundID.NPCHit13 with { Volume = 0.3f, Pitch = -0.95f, PitchVariance = 0.05f, MaxInstances = 6 }, pos);
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), pos, Vector2.Zero, ModContent.ProjectileType<VolatileFlesh>(), NPC.damage, 0, -1, startPos.X - pos.X, startPos.Y - pos.Y);
                }
                if (NPC.ai[1] >= Bloodball.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = -120;
                    NPC.ai[2] = Bloodball.Id;
                }
            }
            else if (NPC.ai[0] == Summon.Id)
            {
                if (NPC.ai[1] == 0)
                {
                    SoundEngine.PlaySound(SoundID.NPCDeath10 with { Volume = 0.5f }, NPC.Center + hitboxes[1].offset);
                }
                if (NPC.ai[1] > 60)
                {
                    if (NPC.ai[1] == 61)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_WitherBeastAuraPulse with { Volume = 1f, Pitch = -0.66f }, NPC.Center);
                        SoundEngine.PlaySound(SoundID.DD2_SkeletonSummoned with { Volume = 1f, Pitch = -0.66f }, NPC.Center);
                    }
                    if (NPC.ai[1] % 10 == 1 && NPC.ai[3] < summonCap)
                    {
                        
                        Vector2 pos = NPC.Center + hitboxes[0].offset + new Vector2(-hitboxes[0].dimensions.X * 0.5f, hitboxes[0].dimensions.Y * Main.rand.NextFloat(-0.4f, 0.4f));
                        NPC.NewNPCDirect(NPC.GetSource_FromThis(), pos + new Vector2(32, 12), ModContent.NPCType<HungryAnchored>(), NPC.whoAmI);
                        NPC.ai[3]++;
                    }
                }
                else
                {
                    mouthFrameCounter = 0;
                }
                if (NPC.ai[1] < 90 && NPC.ai[1] % 2 == 0)
                {
                    Vector2 pos = NPC.Center + hitboxes[0].offset + new Vector2(-hitboxes[0].dimensions.X * 0.5f, hitboxes[0].dimensions.Y * Main.rand.NextFloat(-0.24f, 0.24f));
                    Vector2 velocity = new Vector2(-1f, -1f) * Main.rand.NextFloat(1f, 1.7f);

                    if (Main.rand.NextBool(3))
                        velocity *= 1.5f;
                    Vector2 scale = new Vector2(0.25f, 0.4f) * 0.75f;
                    int time = 50 + Main.rand.Next(20);
                    Color color = Color.Lerp(Color.Red * 0.65f, Color.Purple, Main.rand.NextFloat());
                    ParticleManager.AddParticle(new Blood(pos, velocity, time, Color.Black * 0.65f, scale, velocity.ToRotation(), false));
                    ParticleManager.AddParticle(new Blood(pos, velocity, time, color, scale, velocity.ToRotation(), true));
                }
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
            int chosenAttack = 0;

            List<Attack> potentialAttacks = new List<Attack>() { Laser, Deathray, BouncyBall, Bloodball, Summon };
            potentialAttacks.RemoveAll(x => x.Id == (int)NPC.ai[2]);
            if (NPC.ai[3] > 3)
                potentialAttacks.RemoveAll(x => x.Id == Summon.Id);


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
            NPC.ai[0] = chosenAttack;
        }
        public override bool? CanFallThroughPlatforms()
        {
            return true;
        }
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            Rectangle targetBox = target.Hitbox;
            for (int i = 0; i < hitboxes.Count; i++)
            {
                if (!hitboxes[i].active)
                    continue;

                bool pass = targetBox.Intersects(hitboxes[i].GetHitbox(NPC.Center, NPC.rotation));
                if (pass)
                {
                    CollisionPass = ableToHit;
                    return ableToHit;
                }
            }
            CollisionPass = false;
            return false;
        }
        public override bool CanHitNPC(NPC target)
        {
            Rectangle targetBox = target.Hitbox;
            for (int i = 0; i < hitboxes.Count; i++)
            {
                if (!hitboxes[i].active)
                    continue;

                bool pass = targetBox.Intersects(hitboxes[i].GetHitbox(NPC.Center, NPC.rotation));
                if (pass)
                {
                    CollisionPass = ableToHit;
                    return ableToHit;
                }
            }
            CollisionPass = false;
            return false;
        }
        public override bool ModifyCollisionData(Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox)
        {
            if (CollisionPass)
            {
                npcHitbox = new Rectangle(0, 0, Main.maxTilesX * 16, Main.maxTilesY * 16);
            }
            return CollisionPass;
        }
        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            if ((projectile.hostile && !NPC.friendly) || (projectile.friendly && NPC.friendly))
                return false;

            for (int i = 0; i < hitboxes.Count; i++)
            {
                if (!hitboxes[i].active)
                    continue;

                bool pass = projectile.Colliding(projectile.getRect(), hitboxes[i].GetHitbox(NPC.Center, NPC.rotation));
                if (pass)
                {
                    projectile.ModProj().ultimateCollideOverride = true;
                    return canBeHit ? null : false;
                }
            }

            return false;
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
                modNPC.ignitedStacks.Clear();
                if (modNPC.isRoomNPC)
                {
                    ActiveBossTheme.endFlag = true;
                    Room room = RoomList[modNPC.sourceRoomListID];
                    room.bossDead = true;
                    ClearChildren();
                }
                CutsceneSystem.SetCutscene(NPC.Center + hitboxes[1].offset.X * Vector2.UnitX, deathCutsceneDuration, 30, 30, 1.25f);
            }
            else if (deadTime == 1 && modNPC.isRoomNPC)
                ClearChildren();

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

            if (deadTime % 30 == 0 || deadTime % 52 == 0 || (deadTime > 70 && deadTime % 13 == 0))
            {
                SoundEngine.PlaySound(SoundID.NPCHit8 with { Volume = 0.2f, MaxInstances = 4 }, NPC.Center + hitboxes[1].offset);
                DeathBloodParticles();

                Rectangle bodyRect = hitboxes[0].GetHitbox(NPC.Center, 0);
                bodyRect.Inflate(0, -100);
                Vector2 randPos = new Vector2(bodyRect.X, bodyRect.Y + Main.rand.NextFloat(bodyRect.Height));
                deathGodRays.Add(new GodRay(Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4) + MathHelper.Pi, deadTime, new Vector2(0.16f + Main.rand.NextFloat(-0.02f, 0.02f), 0.018f) * 1.6f, 0, randPos));
                SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact with { Volume = 0.35f, Pitch = -0.2f,MaxInstances = 3 }, NPC.Center);
                SoundEngine.PlaySound(SoundID.NPCHit19 with { Volume = 0.28f, Pitch = -0.6f, PitchVariance = 0.05f,MaxInstances = 3 }, NPC.Center);
            }

            if (deadTime % 4 == 0)
            {
                for (int i = 0; i < deathGodRays.Count; i++)
                {
                    GodRay ray = deathGodRays[i];
                    float rotation = ray.rotation;
                    Vector2 pos = ray.position;
                    Vector2 velocity = rotation.ToRotationVector2() * 2.5f;
                    int xDir = Math.Sign(pos.X - NPC.Center.X);
                    if (xDir == 0)
                        xDir = 1;
                    velocity.X += xDir * 0.6f;
                    velocity.Y -= 1f;
                    int time = 40 + Main.rand.Next(20);
                    Vector2 scale = new Vector2(0.25f, 0.4f) * 0.75f;

                    ParticleManager.AddParticle(new Blood(pos, velocity, time, Color.Black * 0.65f, scale, velocity.ToRotation(), false));
                    ParticleManager.AddParticle(new Blood(pos, velocity, time, Color.Red * 0.65f, scale, velocity.ToRotation(), true));
                }
            }

            if (deadTime >= deathCutsceneDuration - 30)
            {
                NPC.immortal = false;
                NPC.dontTakeDamage = false;
                NPC.StrikeInstantKill();
            }

            return deadTime >= cutsceneDuration - 30;
        }
        public void DeathBloodParticles(bool dead = false)
        {
            int time = dead ? 40 : 20;
            int fadeout = dead ? 35 : 15;
            float multi = dead ? 0.4f : 1f;
            float decel = dead ? 0.98f : 0.96f;

            Rectangle particleRect = hitboxes[0].GetHitbox(NPC.Center, 0);
            particleRect.Inflate(0, -48);
            for (int j = 0; j < 3; j++)
            {
                Vector2 randPos = Main.rand.NextVector2FromRectangle(particleRect);
                for (int i = 0; i < 16; i++)
                {
                    Color color = Color.Lerp(Color.Red, Color.DarkRed, Main.rand.NextFloat());
                    Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                    vel.Y += 0.5f;
                    vel *= multi;
                    ParticleManager.AddParticle(new Ball(
                        randPos, vel,
                        time, color, new Vector2(0.25f) * Main.rand.NextFloat(0.7f, 1f), 0, decel, fadeout));
                }
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                SoundEngine.PlaySound(SoundID.NPCHit8 with { Volume = 0.4f, MaxInstances = 4 }, NPC.Center + hitboxes[1].offset);
            }
        }
        public override void OnKill()
        {
            if (SoundEngine.TryGetActiveSound(DeathraySlot, out var sound) && sound.IsPlaying)
            {
                sound.Stop();
            }
            SoundEngine.PlaySound(SoundID.NPCDeath10 with { Volume = 0.5f }, NPC.Center + hitboxes[1].offset);
            SoundEngine.PlaySound(SoundID.NPCDeath12 with { Volume = 0.8f, Pitch = -0.5f }, NPC.Center);
            SoundEngine.PlaySound(SoundID.DD2_KoboldIgnite with { Volume = 0.8f, Pitch = -0.4f }, NPC.Center);


            //the following was copied from the vanilla WoF gore code, and I tried cleaning it up a lot. still shows a bit
            for (int e = 1; e <= 3; e++)
            {
                Rectangle rect = hitboxes[e].GetHitbox(NPC.Center, 0);
                Vector2 pos = new Vector2(rect.X, rect.Y);
                Dust.NewDust(pos, rect.Width, rect.Height, 5, -2, -1f);
                if (e > 1)
                {
                    Gore.NewGore(NPC.GetSource_Death(), pos, NPC.velocity, 137, NPC.scale);
                    Gore.NewGore(NPC.GetSource_Death(), new Vector2(pos.X, pos.Y + (rect.Height / 2)), NPC.velocity, 139, NPC.scale);
                    Gore.NewGore(NPC.GetSource_Death(), new Vector2(pos.X + (rect.Width / 2), pos.Y), NPC.velocity, 139, NPC.scale);
                    Gore.NewGore(NPC.GetSource_Death(), new Vector2(pos.X + (rect.Width / 2), pos.Y + (rect.Height / 2)), NPC.velocity, 137, NPC.scale);
                }
                else
                {
                    Gore.NewGore(NPC.GetSource_Death(), new Vector2(pos.X, pos.Y), NPC.velocity, 137, NPC.scale);
                    Gore.NewGore(NPC.GetSource_Death(), new Vector2(pos.X, pos.Y + (rect.Height / 2)), NPC.velocity, 138, NPC.scale);
                    Gore.NewGore(NPC.GetSource_Death(), new Vector2(pos.X + (rect.Width / 2), pos.Y), NPC.velocity, 138, NPC.scale);
                    Gore.NewGore(NPC.GetSource_Death(), new Vector2(pos.X + (rect.Width / 2), pos.Y + (rect.Height / 2)), NPC.velocity, 137, NPC.scale);
                }
            }

            Rectangle bodyRect = hitboxes[0].GetHitbox(NPC.Center, 0);
            Vector2 topLeft = new Vector2(bodyRect.X, bodyRect.Y);
            for (int x = 0; x < bodyRect.Width - 32; x += 46)
            {
                for (int y = 0; y < bodyRect.Height; y += 52)
                {
                    Vector2 pos = topLeft + new Vector2(x, y);
                    if (ParanoidTileRetrieval(pos.ToTileCoordinates()).IsTileSolidGround(true))
                        continue;

                    if (Main.rand.NextBool(3))
                        DeathBloodParticles(true);

                    for (int i = 0; i < 5; i++)
                    {
                        Dust.NewDust(pos, 32, 32, 5, Main.rand.NextFloat(-0.6f, 0.6f), Main.rand.NextFloat(-0.6f, 0.6f));

                        if (Main.rand.NextBool())
                            continue;

                        Vector2 velocity = new Vector2(-1f, -1f) * Main.rand.NextFloat(1f, 1.7f);
                        velocity.X *= Main.rand.NextFloat(0.5f, 3f);

                        if (Main.rand.NextBool(3))
                            velocity *= 1.5f;
                        Vector2 scale = new Vector2(0.25f, 0.4f) * 0.65f;
                        int time = 110 + Main.rand.Next(70);
                        Color color = Color.Lerp(Color.Red * 0.65f, Color.Purple, Main.rand.NextFloat(0.75f));
                        Vector2 randParticlePos = Main.rand.NextVector2FromRectangle(new Rectangle((int)pos.X, (int)pos.Y, 32, 32));
                        ParticleManager.AddParticle(new Blood(randParticlePos, velocity, time, Color.Black * 0.65f, scale, velocity.ToRotation(), false));
                        ParticleManager.AddParticle(new Blood(randParticlePos, velocity, time, color, scale, velocity.ToRotation(), true));
                    }
                    Vector2 goreVel = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), Main.rand.NextFloat(-0.6f, 0.2f));
                    goreVel += Vector2.UnitX * -1.5f;
                    if (goreVel.X < 0)
                        goreVel.X *= 3.5f;
                    Gore.NewGore(NPC.GetSource_Death(), pos, goreVel, Main.rand.Next(140, 143));
                }
            }
        }
        public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
        {
            modifiers.HideCombatText();
        }
        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            Rectangle rect = hitboxes[0].GetHitbox(NPC.Center, 0);
            rect.Inflate(-16, -100);
            rect.X -= 16;
            rect.Y += (int)(rect.Height * 0.25f);
            CombatText.NewText(rect, hit.Crit ? CombatText.DamagedHostileCrit : CombatText.DamagedHostile, hit.Damage, hit.Crit);
        }
        public override void ModifyHoverBoundingBox(ref Rectangle boundingBox)
        {
            for (int i = 0; i < hitboxes.Count; i++)
            {
                Rectangle hitbox = hitboxes[i].GetHitbox(NPC.Center, 0);
                hitbox.Inflate(15, 16);
                bool pass = hitbox.Contains(Main.MouseWorld.ToPoint());
                if (pass)
                {
                    boundingBox = new Rectangle(0, 0, Main.maxTilesX * 16, Main.maxTilesY * 16);
                    return;
                }
            }
            boundingBox = new Rectangle(0, 0, 0, 0);
        }
        public override void FindFrame(int frameHeight)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;

            NPC.frame = new Rectangle(0, currentFrame * frameHeight, tex.Width, frameHeight);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (deadTime > 0)
            {
                if (deathGodRays.Any())
                {
                    StartAdditiveSpritebatch();
                    for (int i = 0; i < deathGodRays.Count; i++)
                    {
                        GodRay ray = deathGodRays[i];
                        float rotation = ray.rotation;
                        Vector2 rayScale = ray.scale;
                        int time = ray.time;
                        float opacity = MathHelper.Clamp(MathHelper.Lerp(1f, 0.5f, (deadTime - time) / 60f), 0.5f, 1f);
                        Main.EntitySpriteDraw(godRayTex, ray.position - Main.screenPosition, null, Color.Red * opacity, rotation, new Vector2(0, godRayTex.Height * 0.5f), rayScale, SpriteEffects.None);
                    }
                    StartVanillaSpritebatch();
                }
            }

            Vector2 bodyDrawStart = NPC.Center + hitboxes[0].offset + new Vector2(-16, hitboxes[0].dimensions.Y * -0.5f) + Vector2.UnitY * 16 + Vector2.UnitX * -8;

            int eyeFrameCount = 2;
            int eyeFrameHeight = eyeTex.Height / eyeFrameCount;
            int currentEyeFrame = (int)(NPC.frameCounter % eyeFrameCount);
            Rectangle eyeFrame = new Rectangle(0, currentEyeFrame * eyeFrameHeight, eyeTex.Width, eyeFrameHeight - 2);

            int mouthFrameCount = 2;
            int mouthFrameHeight = mouthTex.Height / mouthFrameCount;
            int currentMouthFrame = (int)(mouthFrameCounter % eyeFrameCount);
            Rectangle mouthFrame = new Rectangle(0, currentMouthFrame * mouthFrameHeight, mouthTex.Width, mouthFrameHeight - 2);

            SpriteEffects spriteEffects = NPC.spriteDirection > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Color bodyDrawColor;
            float offsetMagnitude = (float)Math.Sin(NPC.localAI[1] * 0.03f);
            int bodyFrameYOff = ((int)(NPC.frameCounter * 2) % 3 * (bodyTex.Height / 3)) + (int)(-10 * offsetMagnitude);

            List<StoredDraw> draws = [];

            for (int i = 16; i < hitboxes[0].dimensions.Y - 32; i += 4)
            {
                Vector2 bodyDrawPos = bodyDrawStart + Vector2.UnitY * i;
                int frameYPos = (i + bodyFrameYOff) % bodyTex.Height;
                Rectangle frame = new Rectangle(0, frameYPos, bodyTex.Width - 16, 4);
                bodyDrawColor = Lighting.GetColor(bodyDrawPos.ToTileCoordinates());
                draws.Add(new StoredDraw(bodyTex, bodyDrawPos, frame, bodyDrawColor, 0, new Vector2(frame.Size().X * 0.5f, 0), 1f, spriteEffects));
            }
            Vector2 drawPos = hitboxes[2].offset + NPC.Center - Vector2.UnitY * 4;
            draws.Add(new StoredDraw(eyeTex, drawPos, eyeFrame, Lighting.GetColor(drawPos.ToTileCoordinates()), topEyeRotation, eyeFrame.Size() * new Vector2(0.6f, 0.5f), 1f, SpriteEffects.FlipHorizontally));
            drawPos = hitboxes[3].offset + NPC.Center - Vector2.UnitY * 4;
            draws.Add(new StoredDraw(eyeTex, drawPos, eyeFrame, Lighting.GetColor(drawPos.ToTileCoordinates()), bottomEyeRotation, eyeFrame.Size() * new Vector2(0.6f, 0.5f), 1f, SpriteEffects.FlipHorizontally));
            drawPos = hitboxes[1].offset + NPC.Center;
            draws.Add(new StoredDraw(mouthTex, drawPos, mouthFrame, Lighting.GetColor(drawPos.ToTileCoordinates()), mouthRotation, mouthFrame.Size() * new Vector2(0.6f, 0.5f), 1f, SpriteEffects.FlipHorizontally));

            if (modNPC.ignitedStacks.Any())
            {
                StartAlphaBlendSpritebatch();

                Color color = Color.Lerp(Color.Yellow, Color.OrangeRed, Main.rand.NextFloat(0.4f, 0.6f + float.Epsilon) + 0.2f + (0.2f * (float)Math.Cos((Main.GlobalTimeWrappedHourly * 20f)))) * 0.8f;
                Vector3 colorHSL = Main.rgbToHsl(color);
                GameShaders.Misc["TerRoguelike:BasicTint"].UseOpacity(1f);
                GameShaders.Misc["TerRoguelike:BasicTint"].UseColor(Main.hslToRgb(1 - colorHSL.X, colorHSL.Y, colorHSL.Z));
                GameShaders.Misc["TerRoguelike:BasicTint"].Apply();

                for (int i = 0; i < draws.Count; i++)
                {
                    var draw = draws[i];
                    for (int j = 0; j < 8; j++)
                    {
                        draw.Draw(-Main.screenPosition + Vector2.UnitX.RotatedBy(j * MathHelper.PiOver4 + draw.rotation) * 2);
                    }
                }

                StartVanillaSpritebatch();
            }
            Vector2 drawOff = -Main.screenPosition;
            if (NPC.localAI[0] < -30)
            {
                float completion = ((-NPC.localAI[0] - 30f) / cutsceneDuration);
                completion = MathHelper.SmoothStep(0, 1, completion);
                drawOff += new Vector2(160 * completion, 0);
            }
            for (int i = 0; i < draws.Count; i++)
            {
                var draw = draws[i];
                draw.Draw(drawOff);
            }
            if (NPC.localAI[0] < 0)
            {
                Main.EntitySpriteDraw(squareTex, NPC.Center + hitboxes[1].offset + new Vector2(284, 0) - Main.screenPosition, null, Color.Black, 0, squareTex.Size() * 0.5f, hitboxes[0].dimensions.ToVector2() * new Vector2(0.6f, 0.25f), SpriteEffects.None);
            }

            if (false)
            {
                for (int i = 0; i < hitboxes.Count; i++)
                {
                    if (!hitboxes[i].active)
                        continue;

                    Rectangle hitbox = hitboxes[i].GetHitbox(NPC.Center, NPC.rotation);
                    for (int d = 0; d <= 1; d++)
                    {
                        for (int x = 0; x < hitbox.Width; x++)
                        {
                            Main.EntitySpriteDraw(squareTex, hitbox.Location.ToVector2() + new Vector2(x, hitbox.Height * d) - Main.screenPosition, null, Color.Red, 0, squareTex.Size() * 0.5f, 0.5f, SpriteEffects.None);
                        }
                        for (int y = 0; y < hitbox.Height; y++)
                        {
                            Main.EntitySpriteDraw(squareTex, hitbox.Location.ToVector2() + new Vector2(hitbox.Width * d, y) - Main.screenPosition, null, Color.Red, 0, squareTex.Size() * 0.5f, 0.5f, SpriteEffects.None);
                        }
                    }
                }
            }
            if (false)
            {
                for (int i = 0; i < modNPC.ExtraIgniteTargetPoints.Count; i++)
                {
                    Main.EntitySpriteDraw(squareTex, modNPC.ExtraIgniteTargetPoints[i] + NPC.Center - Main.screenPosition, null, Color.Red, 0, squareTex.Size() * 0.5f, 0.5f, SpriteEffects.None);
                }
            }
            return false;
        }
    }
    public class StoredDraw
    {
        public Texture2D texture;
        public Vector2 position;
        public Rectangle? frame;
        public Color color;
        public float rotation;
        public Vector2 origin;
        public Vector2 scale;
        public SpriteEffects spriteEffects;
        public StoredDraw(Texture2D texture, Vector2 position, Rectangle? frame, Color color, float rotation, Vector2 origin, float scale, SpriteEffects spriteEffects)
        {
            Create(texture, position, frame, color, rotation, origin, new Vector2(scale), spriteEffects);
        }
        public StoredDraw(Texture2D texture, Vector2 position, Rectangle? frame, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects spriteEffects)
        {
            Create(texture, position, frame, color, rotation, origin, scale, spriteEffects);
        }
        void Create(Texture2D texture, Vector2 position, Rectangle? frame, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects spriteEffects)
        {
            this.texture = texture;
            this.position = position;
            this.frame = frame;
            this.color = color;
            this.rotation = rotation;
            this.origin = origin;
            this.scale = scale;
            this.spriteEffects = spriteEffects;
        }
        public void Draw()
        {
            Main.EntitySpriteDraw(texture, position, frame, color, rotation, origin, scale, spriteEffects);
        }
        public void Draw(Vector2 offset)
        {
            Main.EntitySpriteDraw(texture, position + offset, frame, color, rotation, origin, scale, spriteEffects);
        }
    }
}
