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
using static TerRoguelike.Systems.EnemyHealthBarSystem;
using static TerRoguelike.MainMenu.TerRoguelikeMenu;
using TerRoguelike.World;
using System.IO;

namespace TerRoguelike.NPCs.Enemy.Boss
{
    public class Skeletron : BaseRoguelikeNPC
    {
        public Entity target;
        public Vector2 spawnPos;
        public bool ableToHit = true;
        public bool canBeHit = true;
        public override int modNPCID => ModContent.NPCType<Skeletron>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Dungeon"] };
        public override int CombatStyle => -1;
        public int currentFrame;
        public SlotId trackedSlot;
        public Texture2D eyeTex;
        public Texture2D squareTex;

        public int deadTime = 0;
        public int cutsceneDuration = 180;
        public int deathCutsceneDuration = 240;
        public int eyeParticleIntensity = 1;
        public static float spawnRotation = MathHelper.PiOver4 * 0.5f;

        public static Attack None = new Attack(0, 0, 100);
        public static Attack Charge = new Attack(1, 30, 270);
        public static Attack SoulBurst = new Attack(2, 30, 480);
        public static Attack BoneSpear = new Attack(3, 30, 120);
        public static Attack SoulTurret = new Attack(4, 30, 90);
        public static Attack TeleportDash = new Attack(5, 30, 269);
        public static Attack Summon = new Attack(6, 16, 239);
        public int chargeWindup = 60;
        public int chargeFireRate = 60;
        public int soulBurstWindup = 60;
        public int soulBurstShootingDuration = 240;
        public int soulBurstWindDown = 180;
        public float soulBurstProjRot = 0;
        public int boneSpearFireRate = 20;
        public int soulTurretWindup = 40;
        public int teleportDashWindup = 45;
        public int teleportDashingDuration = 45;
        public int summonWindup = 40;
        public int summonCooldown = 40;

        public override void SetStaticDefaults()
        {
            NPCID.Sets.NoMultiplayerSmoothingByType[Type] = true;
            Main.npcFrameCount[Type] = 2;
            NPCID.Sets.TrailCacheLength[Type] = 2;
            NPCID.Sets.TrailingMode[Type] = 3;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            modNPC.TerRoguelikeBoss = true;
            NPC.width = 80;
            NPC.height = 80;
            NPC.aiStyle = -1;
            NPC.damage = 32;
            NPC.lifeMax = 27000;
            NPC.HitSound = SoundID.NPCHit2;
            NPC.knockBackResist = 0f;
            modNPC.drawCenter = new Vector2(0, 0);
            modNPC.IgnoreRoomWallCollision = true;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            eyeTex = TexDict["SkeletronEye"];
            squareTex = TexDict["Square"];
            modNPC.IgniteCentered = true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.immortal = NPC.dontTakeDamage = !TerRoguelikeWorld.escape;
            currentFrame = 0;
            NPC.localAI[0] = -(cutsceneDuration + 30);
            NPC.direction = -1;
            NPC.spriteDirection = -1;
            NPC.Center += Vector2.UnitY * NPC.height * 0.5f;
            spawnPos = NPC.Center;
            NPC.rotation = spawnRotation;
            NPC.ai[2] = None.Id;
            ableToHit = false;
            soulBurstProjRot = Main.rand.NextFloat(MathHelper.TwoPi);
            eyeParticleIntensity = -1;

            
            NPC.Center = TileCollidePositionInLine(NPC.Center, NPC.Center + new Vector2(0, 1000));
            NPC.Center += new Vector2(0, -40);
        }
        public override void PostAI()
        {
            if (eyeParticleIntensity > 0)
            {
                var positions = EyePositions(NPC.Center + NPC.velocity, NPC.rotation);
                var oldPositions = EyePositions(NPC.oldPos[1] + new Vector2(NPC.width, NPC.height) * 0.5f, NPC.oldRot[1]);
                Color particleColor = Color.Lerp(Color.Cyan, Color.Blue, 0.15f);
                Vector2 baseScale = new Vector2(0.1f);
                if (eyeParticleIntensity > 1)
                    baseScale *= 1.35f;

                for (int i = 0; i < positions.Count; i++)
                {
                    Vector2 offset = oldPositions[i] - positions[i];

                    for (int j = 0; j < 8; j++)
                    {
                        int time = 6 + Main.rand.Next(8);
                        float completion = j / 8f;
                        bool switchup = Main.rand.NextBool(20);
                        Vector2 pos = positions[i] + offset * completion;
                        Vector2 velocity = switchup ? -Vector2.UnitY * 1.25f + NPC.velocity : Vector2.Zero;
                        Vector2 scale = baseScale;
                        if (switchup)
                            scale *= 0.75f;

                        ParticleManager.AddParticle(new Ball(
                            pos - Vector2.UnitY * 2, velocity + Main.rand.NextVector2Circular(0.25f, 0.25f),
                            time, particleColor, scale, 0, 0.96f, time, false));
                    }
                }
            }
            if (SoundEngine.TryGetActiveSound(trackedSlot, out var sound) && sound.IsPlaying)
            {
                sound.Position = NPC.Center;
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
                    CutsceneSystem.SetCutscene(NPC.Center, cutsceneDuration, 30, 30, 2.5f, CutsceneSystem.CutsceneSource.Boss);
                }
                NPC.localAI[0]++;

                CutsceneSystem.cameraTargetCenter += 0.05f * (NPC.Center - CutsceneSystem.cameraTargetCenter);
                int time = (int)NPC.localAI[0] + cutsceneDuration - 40;
                if (time > 60)
                {
                    NPC.velocity *= 0.98f;
                }
                else
                {
                    if (time >= 0)
                    {
                        NPC.rotation = NPC.rotation.AngleLerp(0, 0.01f).AngleTowards(0, 0.005f);
                        if (time == 30)
                        {
                            eyeParticleIntensity = 1;
                            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot with { Volume = 1f, Pitch = -0.5f }, NPC.Center);
                            EyeSummonParticleEffect(NPC.rotation.AngleTowards(0, 1.6f));
                        }
                        if (time == 25)
                        {
                            SoundEngine.PlaySound(SoundID.DD2_DarkMageSummonSkeleton with { Volume = 1, PitchVariance = 0, Variants = [0], Pitch = -0.15f }, NPC.Center);
                        }
                        NPC.velocity.Y -= 0.03f;
                    }
                }

                if (NPC.localAI[0] == -30)
                {
                    eyeParticleIntensity = 1;
                    NPC.immortal = false;
                    NPC.dontTakeDamage = false;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Summon.Id;
                    if (!TerRoguelikeWorld.escape)
                        enemyHealthBar = new EnemyHealthBar([NPC.whoAmI], NPC.GivenOrTypeName);
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
            bool hardMode = (int)difficulty >= (int)Difficulty.BloodMoon;

            target = modNPC.GetTarget(NPC);
            NPC.ai[1]++;
            NPC.velocity *= 0.98f;

            if (NPC.ai[0] == None.Id)
            {
                UpdateDirection();
                DefaultRotation();

                if (NPC.ai[1] >= None.Duration)
                {
                    Room room = modNPC.isRoomNPC ? RoomList[modNPC.sourceRoomListID] : null;
                    if (room != null)
                    {
                        if (!room.GetRect().Contains(NPC.Center.ToPoint()))
                        {
                            NPC.velocity += (room.GetRect().ClosestPointInRect(NPC.Center) - NPC.Center).SafeNormalize(Vector2.UnitY) * 0.1f;
                            NPC.ai[1]--;
                        }
                    }
                }

                if (NPC.ai[1] >= None.Duration)
                {
                    ChooseAttack();
                }
                else
                {
                    if (hardMode && NPC.ai[1] < None.Duration - 2)
                        NPC.ai[1]++;
                    DefaultMovement();
                }

            }

            if (NPC.ai[0] == Charge.Id)
            {
                Vector2 targetPos = target != null ? target.Center : NPC.velocity.ToRotation().AngleTowards((spawnPos - NPC.Center).ToRotation(), 0.03f).ToRotationVector2() * 180 + NPC.Center;
                float magnitude = MathHelper.Clamp(targetPos.Distance(NPC.Center) * 0.01f, 3f, 8f);
                NPC.velocity = (targetPos - NPC.Center).SafeNormalize(Vector2.UnitY) * magnitude;
                NPC.rotation += 0.2f * NPC.direction;

                if (NPC.ai[1] < chargeWindup)
                {
                    float startupCompletion = NPC.ai[1] / chargeWindup;
                    NPC.velocity *= (float)Math.Pow(startupCompletion, 4);
                    if (NPC.ai[1] == 0)
                    {
                        SoundEngine.PlaySound(SoundID.Roar with { Volume = 0.9f, SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest }, NPC.Center);
                    }
                }
                else
                {
                    int time = (int)NPC.ai[1] - chargeWindup;
                    if (time % chargeFireRate == 0)
                    {
                        SoundEngine.PlaySound(SoundID.NPCHit36 with { Volume = 0.3f, MaxInstances = 2 }, NPC.Center);
                        float fireDirection = (-NPC.velocity).ToRotation();
                        for (float i = -3; i <= 3; i += 2)
                        {
                            Vector2 projVelDir = (fireDirection + 0.5f * i).ToRotationVector2();
                            if (!TerRoguelike.mpClient)
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + projVelDir * 28, projVelDir * 12, ModContent.ProjectileType<SeekingSoulBlast>(), NPC.damage, 0, -1, targetPos.X, targetPos.Y, NPC.velocity.ToRotation());
                        }
                    }
                }
                if (NPC.Center.Distance(NPC.Center + NPC.velocity) >= NPC.Center.Distance(targetPos))
                {
                    NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY * NPC.Center.Distance(targetPos));
                }
                
                if (NPC.ai[1] >= Charge.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Charge.Id;
                }
            }
            else if (NPC.ai[0] == SoulBurst.Id)
            {
                Vector2 targetPos = target != null ? target.Center : spawnPos;
                Vector2 targetPosOffset = (NPC.Center - targetPos).SafeNormalize(Vector2.UnitY) * 160;
                if (targetPosOffset.Y > 0)
                    targetPosOffset = targetPosOffset.ToRotation().AngleTowards(-MathHelper.PiOver2, 0.8f).ToRotationVector2() * targetPosOffset.Length();
                targetPos = TileCollidePositionInLine(targetPos, targetPos + targetPosOffset);

                if (NPC.ai[1] < soulBurstWindup)
                {
                    UpdateDirection();
                    DefaultRotation();
                    
                    if (NPC.localAI[2] % 20 == 0)
                    {
                        trackedSlot = SoundEngine.PlaySound(SoundID.Item13 with { Volume = 0.8f, Pitch = 0.12f, PitchVariance = 0 }, NPC.Center);
                    }
                    NPC.localAI[2]++;

                    bool collide = false;
                    Vector2 baseCheckPos = NPC.position;
                    Vector2 offsetPerLoop = new Vector2(NPC.width * 0.5f, NPC.height * 0.5f);
                    Vector2 collidingVector = Vector2.Zero;
                    for (int x = 0; x < 3; x++)
                    {
                        for (int y = 0; y < 3; y++)
                        {
                            Vector2 checkPos = baseCheckPos + offsetPerLoop * new Vector2(x, y);
                            if (TileCollisionAtThisPosition(checkPos))
                            {
                                collide = true;
                                collidingVector += new Vector2(-1 + x, -1 + y);
                            }
                        }
                    }
                    float targetAngle = (targetPos - NPC.Center).ToRotation();
                    if (collide && (NPC.Center.Distance(targetPos) < 32 || NPC.ai[1] == soulBurstWindup - 1))
                    {
                        if (target != null)
                            targetPos = target.Center;

                        if (collidingVector == Vector2.Zero)
                            collidingVector = (NPC.Center - (TileCollisionAtThisPosition(targetPos) ? spawnPos : targetPos));
                        targetAngle = (-collidingVector).ToRotation();
                    }
                    NPC.velocity += targetAngle.ToRotationVector2() * 0.15f;
                    float speedCap = 10;
                    if (NPC.velocity.Length() > speedCap)
                        NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * speedCap;

                    if (NPC.ai[1] == soulBurstWindup - 1 && TileCollisionAtThisPosition(NPC.Center))
                        NPC.ai[1]--;

                    Vector2 pos = NPC.Center + new Vector2(0, 32).RotatedBy(NPC.rotation);
                    for (int i = 0; i < 1; i++)
                    {
                        Vector2 offset = Main.rand.NextVector2CircularEdge(19, 19) * (1f - (float)Math.Pow(Main.rand.NextFloat(), 4));
                        offset.Y *= Math.Abs(offset.X) / 19f;
                        offset = offset.RotatedBy(NPC.rotation);

                        Color color = Color.Lerp(Color.Cyan, Color.White, Main.rand.NextFloat(0.6f));
                        ParticleManager.AddParticle(new Ball(
                            pos + offset, -offset.SafeNormalize(Vector2.UnitY) * 1.3f + NPC.velocity * 1.2f,
                            15, color, new Vector2(0.2f) * Main.rand.NextFloat(0.7f, 1f), 0, 0.96f, 15));
                    }
                }
                else if (NPC.ai[1] < SoulBurst.Duration - soulBurstWindDown)
                {
                    if (SoundEngine.TryGetActiveSound(trackedSlot, out var sound) && sound.IsPlaying)
                        sound.Volume -= 0.03f;

                    eyeParticleIntensity = 2;
                    currentFrame = 1;
                    NPC.velocity *= 0.92f;
                    NPC.rotation = NPC.rotation.AngleTowards(0, 0.02f);
                    if (NPC.ai[1] % 6 == 0)
                    {
                        float randRot = Main.rand.NextFloat(-0.25f, 0.25f);
                        NPC.rotation = randRot + Math.Sign(randRot) * 0.05f;
                        SoundEngine.PlaySound(SoundID.Item103 with { Volume = 0.42f, MaxInstances = 7, Pitch = 0.14f, SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest }, NPC.Center);
                        NPC.direction = Math.Sign(NPC.rotation);
                    }

                    if (NPC.ai[1] % 2 == 0 && !TerRoguelike.mpClient)
                    {
                        int count = RuinedMoonActive ? 4 : 1;
                        for (int i = 0; i < count; i++)
                        {
                            Vector2 projSpawnPos = NPC.Center + new Vector2(0, 32).RotatedBy(NPC.rotation);
                            Vector2 projVel = soulBurstProjRot.ToRotationVector2() * 8;
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), projSpawnPos, projVel, ModContent.ProjectileType<SoulBlast>(), NPC.damage, 0);
                            soulBurstProjRot += (MathHelper.PiOver2 + Main.rand.NextFloat(-0.7f, 0.7f)) * NPC.direction;
                        }
                    }

                    bool collide = false;
                    Vector2 baseCheckPos = NPC.position;
                    Vector2 offsetPerLoop = new Vector2(NPC.width * 0.5f, NPC.height * 0.5f);
                    Vector2 collidingVector = Vector2.Zero;
                    for (int x = 0; x < 3; x++)
                    {
                        for (int y = 0; y < 3; y++)
                        {
                            if (x == 1 && y == 1)
                                continue;

                            Vector2 checkPos = baseCheckPos + offsetPerLoop * new Vector2(x, y);
                            if (TileCollisionAtThisPosition(checkPos))
                            {
                                collide = true;
                                collidingVector += new Vector2(-1 + x, -1 + y);
                            }
                        }
                    }
                    if (collide)
                    {
                        NPC.Center += -collidingVector.SafeNormalize(Vector2.UnitY);
                    }
                }
                else
                {   
                    if (NPC.ai[1] == SoulBurst.Duration - soulBurstWindDown)
                    {
                        eyeParticleIntensity = -1;
                        SoundEngine.PlaySound(SoundID.NPCHit36 with { Volume = 0.1f, MaxInstances = 2, Pitch = -1f }, NPC.Center);
                    }

                    currentFrame = 0;
                    NPC.noTileCollide = false;
                    NPC.noGravity = false;
                    NPC.GravityMultiplier *= 0.9f;
                    NPC.rotation += NPC.velocity.Y * 0.01f * NPC.direction;
                    if (NPC.localAI[1] == 0 && NPC.ai[1] > SoulBurst.Duration - soulBurstWindDown && NPC.collideY)
                    {
                        NPC.localAI[1] = 1;
                        SoundEngine.PlaySound(SoundID.DD2_SkeletonHurt with { Volume = 1f }, NPC.Center);
                    }
                    if (NPC.ai[1] >= SoulBurst.Duration - 30)
                    {
                        if (NPC.ai[1] == SoulBurst.Duration - 30)
                        {
                            eyeParticleIntensity = 1;
                            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot with { Volume = 1f, Pitch = -0.5f }, NPC.Center);
                            EyeSummonParticleEffect(NPC.rotation.AngleTowards(0, 1.6f));
                        }
                        NPC.rotation = NPC.rotation.AngleLerp(0, 0.08f).AngleTowards(0, 0.01f);
                    }
                }
                if (NPC.ai[1] >= SoulBurst.Duration)
                {
                    currentFrame = 0;
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = SoulBurst.Id;
                    NPC.noTileCollide = true;
                    NPC.noGravity = true;
                    NPC.localAI[1] = 0;
                    NPC.localAI[2] = 0;
                    eyeParticleIntensity = 1;
                }
            }
            else if (NPC.ai[0] == BoneSpear.Id)
            {
                DefaultMovement();
                DefaultRotation();

                if (NPC.ai[1] % boneSpearFireRate == 0)
                {
                    Room room = modNPC.isRoomNPC ? RoomList[modNPC.sourceRoomListID] : null;
                    Vector2 targetPos = target != null ? target.Center : spawnPos;
                    Vector2 basePos = targetPos;
                    float checkLength = 300;
                    for (int i = 0; i < 50; i++)
                    {
                        float randRot = Main.rand.NextFloat(MathHelper.TwoPi);
                        Vector2 rotVect = randRot.ToRotationVector2();
                        Vector2 checkStart = basePos + new Vector2(0, Main.rand.NextFloat(-16, 16)).RotatedBy(randRot);
                        Vector2 checkEnd = checkStart + rotVect * checkLength;
                        Vector2 projSpawnPos = TileCollidePositionInLine(checkStart, checkEnd);
                        if (projSpawnPos == checkEnd)
                            continue;
                        if (room != null && !room.GetRect().Contains(projSpawnPos.ToPoint()))
                            continue;
                        if (projSpawnPos.Distance(checkStart) < 64)
                            continue;
                        bool cont = false;
                        for (int j = -1; j <= 1; j += 2) // stops the bottom part of bone from potentially peeking out of the ground at harsh angles
                        {
                            if (!TileCollisionAtThisPosition(projSpawnPos + new Vector2(8, 5 * j).RotatedBy(randRot)))
                            {
                                cont = true;
                                break;
                            }
                        }
                        if (cont)
                            continue;

                        projSpawnPos -= rotVect;
                        if (!TerRoguelike.mpClient)
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), projSpawnPos, -rotVect, ModContent.ProjectileType<BoneSpear>(), NPC.damage, 0);
                        break;
                    }
                }

                if (NPC.ai[1] >= BoneSpear.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = -30;
                    NPC.ai[2] = BoneSpear.Id;
                }
            }
            else if (NPC.ai[0] == SoulTurret.Id)
            {
                DefaultRotation();
                NPC.velocity *= 0.98f;
                
                if (NPC.ai[1] < soulTurretWindup)
                {
                    currentFrame = 1;
                    if (NPC.ai[1] == soulTurretWindup - 7)
                        SoundEngine.PlaySound(SoundID.DD2_DarkMageSummonSkeleton with { Volume = 1, PitchVariance = 0, Variants = [0] }, NPC.Center);
                }
                else
                {
                    int time = (int)NPC.ai[1] - soulTurretWindup;

                    if (time == 0)
                    {
                        Vector2 spawningDimensions;
                        Vector2 start;
                        if (modNPC.isRoomNPC)
                        {
                            Room room = RoomList[modNPC.sourceRoomListID];
                            spawningDimensions = room.RoomDimensions16 * new Vector2(0.83f, 0.8f);
                            start = room.RoomPosition16 + room.RoomCenter16;
                        }
                        else
                        {
                            spawningDimensions = new Vector2(800, 400);
                            start = spawnPos;
                        }

                        int halfProjCount = 7;
                        for (int d = -1; d <= 1; d += 2)
                        {
                            for (int i = 0; i < halfProjCount; i++)
                            {
                                float completion = i / ((float)halfProjCount - 1);
                                float amplitude = (Math.Abs(MathHelper.Lerp(0.0025f, 0.9975f, completion) - 0.5f) * -2) + 1;
                                amplitude = (float)Math.Pow(amplitude, 0.5f);
                                float xOff = (spawningDimensions.X * completion) - spawningDimensions.X * 0.5f;
                                float yOff = (spawningDimensions.Y * amplitude * 0.4f + spawningDimensions.Y * 0.1f) * d;
                                Vector2 spawnPos = start + new Vector2(xOff, yOff);
                                Vector2 off = (NPC.Center + new Vector2(0, 32).RotatedBy(NPC.rotation)) - spawnPos;
                                if (!TerRoguelike.mpClient)
                                    Projectile.NewProjectile(NPC.GetSource_FromThis(), spawnPos, Vector2.Zero, ModContent.ProjectileType<SoulTurret>(), NPC.damage, 0, -1, off.X, off.Y);
                            }
                        }

                        Vector2 pos = NPC.Center + new Vector2(0, 32).RotatedBy(NPC.rotation);
                        for (int i = 0; i < 40; i++)
                        {
                            Vector2 offset = Main.rand.NextVector2CircularEdge(32, 19) * (1f - (float)Math.Pow(Main.rand.NextFloat(), 4));
                            offset.Y *= Math.Abs(offset.X) / 32f;
                            offset.Y += Main.rand.NextFloat(-2, 2);
                            offset = offset.RotatedBy(NPC.rotation);

                            if (Main.rand.NextBool(5))
                                offset *= 1.5f;

                            Color color = Color.Lerp(Color.Cyan, Color.White, Main.rand.NextFloat(0.8f));
                            ParticleManager.AddParticle(new Ball(
                                pos + offset * 0.25f, offset * 0.09f + NPC.velocity * 1.2f,
                                30, color, new Vector2(0.25f) * Main.rand.NextFloat(0.7f, 1f), 0, 0.98f, 25));
                        }
                    }
                    if (time > 40)
                    {
                        currentFrame = 0;
                    }
                }

                if (NPC.ai[1] >= SoulTurret.Duration)
                {
                    currentFrame = 0;
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = -240;
                    NPC.ai[2] = SoulTurret.Id;
                }
            }
            else if (NPC.ai[0] == TeleportDash.Id)
            {
                float chargeStartDist = 240;
                int time = ((int)NPC.ai[1]) % (teleportDashWindup + teleportDashingDuration);
                Vector2 targetPos = target != null ? target.Center : spawnPos;
                Vector2 wantedPos = targetPos + (NPC.Center - targetPos).SafeNormalize(Vector2.UnitY) * chargeStartDist;

                if (time < teleportDashWindup)
                {
                    if (time == 0)
                    {
                        trackedSlot = default;
                        eyeParticleIntensity = 0;
                        float randRot = Main.rand.NextFloat(-MathHelper.PiOver2 * 0.75f, MathHelper.PiOver2 * 0.75f);
                        randRot += Math.Sign(randRot) * MathHelper.PiOver2 * 0.25f;
                        Vector2 teleportPos = ((wantedPos - targetPos).ToRotation() + randRot).ToRotationVector2() * chargeStartDist + targetPos;
                        Vector2 teleportVect = NPC.Center - teleportPos;
                        if (TerRoguelike.mpClient)
                            NPC.Center = teleportPos;
                        NPC.netUpdate = true;
                        if (target != null)
                            NPC.velocity = target.velocity * 0.4f;
                        UpdateDirection();

                        SoundEngine.PlaySound(SoundID.Zombie54 with { Volume = 0.8f, Pitch = -0.27f, PitchVariance = 0.1f }, NPC.Center);
                        SoundEngine.PlaySound(SoundID.Item8 with { Volume = 0.8f, Pitch = -0.27f, PitchVariance = 0.1f }, NPC.Center);

                        float length = teleportVect.Length() * 0.5f;
                        float teleportRot = teleportVect.ToRotation();
                        for (int i = -2; i <= 2; i++)
                        {
                            for (int j = (int)teleportVect.Length(); j >= teleportVect.Length() - length; j -= 15)
                            {
                                Vector2 particlePos = NPC.Center + new Vector2(j, 10 * i).RotatedBy(teleportRot);
                                ParticleManager.AddParticle(new ThinSpark(
                                    particlePos, -teleportRot.ToRotationVector2() * length * 0.185f,
                                    20, Color.Cyan * 0.5f, new Vector2(0.25f, 0.4f), teleportRot, true, false));
                            }
                        }
                    }
                    else
                    {
                        if (time == 2)
                            eyeParticleIntensity = 1;
                        float speedCap = 10;
                        NPC.velocity += (wantedPos - NPC.Center).SafeNormalize(Vector2.UnitY) * 0.15f;
                        if (NPC.velocity.Length() > 10)
                            NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * speedCap;
                        if (NPC.Center.Distance(wantedPos) <= NPC.velocity.Length())
                            NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * NPC.Center.Distance(wantedPos);
                    }
                }
                else
                {
                    if (time == teleportDashWindup)
                    {
                        trackedSlot = SoundEngine.PlaySound(SoundID.Roar with { Volume = 0.9f, SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest }, NPC.Center);
                        NPC.velocity = (targetPos - NPC.Center).SafeNormalize(Vector2.UnitY) * 14;
                        Vector2 basePos = NPC.Center;
                        for (int i = 0; i < 50; i++)
                        {
                            int particleTime = Main.rand.Next(26, 31);
                            Vector2 offset = Main.rand.NextVector2CircularEdge(9, 9) * Main.rand.NextFloat(0.4f, 1f);
                            ParticleManager.AddParticle(new Square(
                                basePos + offset, offset * 0.6f + NPC.velocity * Main.rand.NextFloat(), 
                                particleTime, Color.Cyan, new Vector2(1f), offset.ToRotation(), Main.rand.NextFloat(0.94f, 0.96f), particleTime, true));
                        }
                    }
                    NPC.velocity /= 0.98f;
                }

                NPC.rotation += 0.2f * NPC.direction;
                if (NPC.ai[1] >= TeleportDash.Duration)
                {
                    eyeParticleIntensity = 1;
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = TeleportDash.Id;
                }
            }
            else if (NPC.ai[0] == Summon.Id)
            {
                DefaultRotation();
                NPC.velocity *= 0.93f;
                int time = (int)NPC.ai[1] % (summonWindup + summonCooldown);
                Vector2 pos = NPC.Center + new Vector2(0, 32).RotatedBy(NPC.rotation);

                if (time < summonWindup)
                {
                    currentFrame = 0;
                    for (int i = 0; i < 1; i++)
                    {
                        Vector2 offset = Main.rand.NextVector2CircularEdge(25, 19) * (1f - (float)Math.Pow(Main.rand.NextFloat(), 4));
                        offset.Y *= Math.Abs(offset.X) / 32f;
                        offset = offset.RotatedBy(NPC.rotation);

                        Color color = Color.HotPink;
                        ParticleManager.AddParticle(new Square(
                            pos + offset, -offset.SafeNormalize(Vector2.UnitY) * 1.3f + NPC.velocity * 1.2f,
                            20, color, new Vector2(1.5f) * Main.rand.NextFloat(0.7f, 1f), 0, 0.96f, 20));
                    }
                }
                else
                {
                    if (time == summonWindup)
                    {
                        currentFrame = 1;
                        SoundEngine.PlaySound(SoundID.DD2_WitherBeastAuraPulse with { Volume = 1f, MaxInstances = 2, Variants = [0] }, NPC.Center);
                        SoundEngine.PlaySound(SoundID.DD2_SkeletonSummoned with { Volume = 1f, MaxInstances = 2 }, NPC.Center);

                        SpawnManager.SpawnEnemy(ModContent.NPCType<DungeonSpirit>(), NPC.Center + new Vector2(0, 32).RotatedBy(NPC.rotation) + NPC.velocity, modNPC.sourceRoomListID, 40, 0.45f);
                    }
                }

                if (NPC.ai[1] >= Summon.Duration)
                {
                    currentFrame = 0;
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = -180;
                    NPC.ai[2] = Summon.Id;
                }
            }

            void DefaultMovement()
            {
                if (target != null)
                {
                    Vector2 targetPos = target.Center + new Vector2(0, -240 + (float)Math.Cos(NPC.localAI[0] * 0.03f) * 8);
                    targetPos = TileCollidePositionInLine(target.Center + new Vector2(0, -104), targetPos);
                    if (NPC.velocity.Length() < 10)
                        NPC.velocity += (targetPos - NPC.Center).SafeNormalize(Vector2.UnitY) * 0.15f;
                    if (NPC.velocity.Length() > 10)
                        NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * 10;
                }
                else
                {
                    Vector2 targetPos = spawnPos + new Vector2(0, 80);
                    if (NPC.velocity.Length() < 10)
                        NPC.velocity += (targetPos - NPC.Center).SafeNormalize(Vector2.UnitY) * 0.15f;
                    if (NPC.velocity.Length() > 10)
                        NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * 10;
                }
            }
            void DefaultRotation()
            {
                float rotBound = MathHelper.PiOver2 * 0.6f;
                NPC.rotation = NPC.rotation.AngleTowards(MathHelper.Clamp(NPC.velocity.X * 0.1f, -rotBound, rotBound), 0.2f);
            }
            void UpdateDirection()
            {
                if (target != null)
                {
                    if (target.Center.X > NPC.Center.X)
                        NPC.direction = 1;
                    else
                        NPC.direction = -1;
                }
                else
                {
                    NPC.direction = Math.Sign(NPC.velocity.X);
                    if (NPC.direction == 0)
                        NPC.direction = -1;
                }
            }
        }
        public void ChooseAttack()
        {
            if (TerRoguelike.mpClient)
                return;
            NPC.netUpdate = true;

            NPC.ai[1] = 0;
            int chosenAttack = 0;

            List<Attack> potentialAttacks = new List<Attack>() { Charge, SoulBurst, BoneSpear, SoulTurret, TeleportDash, Summon };
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

            NPC.ai[0] = chosenAttack;
        }
        public override bool? CanFallThroughPlatforms()
        {
            return true;
        }
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            Vector2 circlePos = NPC.Center + new Vector2(0, -10).RotatedBy(NPC.rotation);
            float radius = NPC.width * 0.4f;
            if (target.getRect().ClosestPointInRect(circlePos).Distance(circlePos) < radius)
                return ableToHit;
            return false;
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
            eyeParticleIntensity = 1;
            NPC.noTileCollide = false;
            ableToHit = false;

            if (deadTime == 0)
            {
                enemyHealthBar.ForceEnd(0);
                NPC.velocity = Vector2.Zero;
                NPC.rotation = 0;
                modNPC.ignitedStacks.Clear();
                modNPC.bleedingStacks.Clear();
                modNPC.ballAndChainSlow = 0;
                currentFrame = 0;
                NPC.localAI[1] = 0;
                NPC.localAI[2] = 0;
                NPC.noGravity = true;

                Vector2 startingPos = NPC.Center;
                if (CollidingVector(NPC.position, new Vector2(NPC.width, NPC.height)) != null)
                {
                    startingPos = spawnPos;
                    if (target != null && modNPC.targetPlayer >= 0)
                    {
                        if (true)
                        {
                            startingPos = Main.player[modNPC.targetPlayer].ModPlayer().FindAirToPlayer(NPC.Center);
                        }
                    }

                    Vector2 teleportVect = NPC.Center - startingPos;
                    NPC.Center = startingPos;
                    for (int i = 0; i < 50; i++)
                    {
                        var collidingVect = CollidingVector(NPC.position, new Vector2(NPC.width, NPC.height));
                        if (collidingVect != null)
                            NPC.Center -= (Vector2)collidingVect;
                    }

                    float length = teleportVect.Length() * 0.5f;
                    float teleportRot = teleportVect.ToRotation();
                    for (int i = -2; i <= 2; i++)
                    {
                        for (int j = (int)teleportVect.Length(); j >= teleportVect.Length() - length; j -= 15)
                        {
                            Vector2 particlePos = NPC.Center + new Vector2(j, 10 * i).RotatedBy(teleportRot);
                            ParticleManager.AddParticle(new ThinSpark(
                                particlePos, -teleportRot.ToRotationVector2() * length * 0.185f,
                                20, Color.Cyan * 0.5f, new Vector2(0.25f, 0.4f), teleportRot, true, false));
                        }
                    }
                }

                

                if (modNPC.isRoomNPC)
                {
                    if (ActiveBossTheme != null)
                        ActiveBossTheme.endFlag = true;
                    Room room = RoomList[modNPC.sourceRoomListID];
                    room.bossDead = true;
                    ClearChildren();
                }
                CutsceneSystem.SetCutscene(NPC.Center, deathCutsceneDuration, 30, 30, 2.5f, CutsceneSystem.CutsceneSource.Boss);
            }
            if (deadTime <= 2)
                eyeParticleIntensity = 0;

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
                    if (modChildNPC.isRoomNPC && modChildNPC.sourceRoomListID == modNPC.sourceRoomListID && !TerRoguelike.mpClient)
                    {
                        childNPC.StrikeInstantKill();
                        childNPC.active = false;
                    }
                }
            }
            deadTime++;

            CutsceneSystem.cameraTargetCenter += 0.075f * (NPC.Center - CutsceneSystem.cameraTargetCenter);
            if (deadTime < 100)
            {
                NPC.velocity.Y -= 0.02f;
                currentFrame = 1;
                NPC.rotation = NPC.rotation.AngleTowards(0, 0.02f);
                if (deadTime % 6 == 0)
                {
                    float randRot = Main.rand.NextFloat(-0.25f, 0.25f);
                    NPC.rotation = randRot + Math.Sign(randRot) * 0.05f;
                    NPC.direction = Math.Sign(NPC.rotation);
                }
            }
            else
            {
                if (deadTime == 100)
                {
                    EyeSummonParticleEffect(NPC.rotation);
                    SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot with { Volume = 1f, Pitch = -0.75f }, NPC.Center);
                }
                eyeParticleIntensity = -1;
                NPC.noGravity = false;
                NPC.GravityMultiplier *= 1.1f;
                NPC.rotation += NPC.velocity.Y * 0.01f * NPC.direction;
                if (NPC.collideY && NPC.oldVelocity.Y > 0)
                {
                    deadTime = deathCutsceneDuration - 30;
                    SoundEngine.PlaySound(SoundID.DD2_SkeletonHurt with { Volume = 1f, Variants = [2] }, NPC.Center);
                }
            }

            if (deadTime >= deathCutsceneDuration - 30)
            {
                NPC.immortal = false;
                NPC.dontTakeDamage = false;
                NPC.StrikeInstantKill();
                if (RuinedMoonActive)
                {
                    for (int i = 0; i < 40; i++)
                    {
                        SpawnManager.SpawnNPCTerRoguelike(NPC.GetSource_NaturalSpawn(), NPC.Center + Main.rand.NextVector2Circular(80, 80), ModContent.NPCType<DungeonSpirit>(), modNPC.sourceRoomListID);
                    }
                }
                
            }

            return deadTime >= cutsceneDuration - 30;
        }
        
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (Main.dedServ)
                return;

            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)NPC.lifeMax * 2000.0; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 26, hit.HitDirection, -1f);
                }
            }
            else if (deadTime > 0)
            {
                SoundEngine.PlaySound(SoundID.NPCDeath2 with { Volume = 1f }, NPC.Center);

                for (int i = 0; i < 150; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 26, Main.rand.NextFloat(-2.5f, 2.5f), -2.5f);
                }

                //Custom gores because for some reason when using vanilla ones, the very first time the gores were spawned, they were offset by a lot.
                Vector2 goreOff = new Vector2(0, -18).RotatedBy(NPC.rotation);
                int g = Gore.NewGore(NPC.GetSource_Death(), NPC.position, -Vector2.UnitY * 0.75f + new Vector2(0, -24) * 0.07f, Mod.Find<ModGore>("Skeletron1").Type);
                Gore gore = Main.gore[g];
                gore.position = NPC.Center + goreOff - new Vector2(gore.Width, gore.Height) * 0.5f;
                gore.rotation = NPC.rotation;

                Vector2 goreOff2 = new Vector2(0, 32).RotatedBy(NPC.rotation);
                int g2 = Gore.NewGore(NPC.GetSource_Death(), NPC.position, -Vector2.UnitY * 0.75f + new Vector2(0, 24).RotatedBy(NPC.rotation) * 0.07f, Mod.Find<ModGore>("Skeletron2").Type);
                Gore gore2 = Main.gore[g2];
                gore2.position = NPC.Center + goreOff2 - new Vector2(gore2.Width, gore2.Height) * 0.5f;
                gore2.rotation = NPC.rotation;
            }
        }
        public List<Vector2> EyePositions(Vector2 center, float rotation)
        {
            List<Vector2> positions = [];
            for (int i = -1; i <= 1; i += 2)
            {
                Vector2 offset = (new Vector2(18 * i, -12) * NPC.scale).RotatedBy(rotation);
                positions.Add(center + offset);
            }
            return positions;
        }
        public void EyeSummonParticleEffect(float potentialRot)
        {
            var positions = EyePositions(NPC.Center, NPC.rotation);
            var potentialPositions = EyePositions(NPC.Center, potentialRot);
            for (int i = 0; i < positions.Count; i++)
            {
                Vector2 pos = positions[i];
                Vector2 potPos = potentialPositions[i];
                Vector2 extraVel = (potPos - pos) * 0.07f;
                for (int j = 0; j < 10; j++)
                {
                    Color color = Color.Lerp(Color.Cyan, Color.White, Main.rand.NextFloat(0.75f));
                    ParticleManager.AddParticle(new Ball(
                        pos, Main.rand.NextVector2CircularEdge(0.6f, 0.8f) * Main.rand.NextFloat(0.5f, 1.3f) + extraVel,
                        15, color, new Vector2(0.25f) * Main.rand.NextFloat(0.7f, 1f), 0, 0.96f, 15));
                }
            }
        }
        public override void FindFrame(int frameHeight)
        {
            if (Main.dedServ)
                return;

            Texture2D tex = TextureAssets.Npc[Type].Value;

            NPC.frame = new Rectangle(0, currentFrame * frameHeight, tex.Width, frameHeight - 2);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;
            Color color = Color.Lerp(drawColor, Color.White, 0.2f);
            float opacity = 1;

            bool ghostly = NPC.ai[0] == TeleportDash.Id && ((int)NPC.ai[1]) % (teleportDashWindup + teleportDashingDuration) < teleportDashWindup;
            if (ghostly)
            {
                int dashTime = ((int)NPC.ai[1]) % (teleportDashWindup + teleportDashingDuration);
                if (dashTime < 10)
                {
                    opacity *= (dashTime + 5) / 15f;
                }
                color = Color.Cyan * 0.6f;
            }
                
            
            Main.EntitySpriteDraw(tex, NPC.Center + modNPC.drawCenter - Main.screenPosition, NPC.frame, color * opacity, NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);

            if (ghostly)
            {
                StartAdditiveSpritebatch();
                Main.EntitySpriteDraw(tex, NPC.Center + modNPC.drawCenter - Main.screenPosition, NPC.frame, Color.Cyan * 0.4f * opacity, NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
                StartVanillaSpritebatch();
            }

            bool drawEyes = eyeParticleIntensity >= 0;
            if (drawEyes)
            {
                var positions = EyePositions(NPC.Center, NPC.rotation);
                float eyeScale = eyeParticleIntensity <= 1 ? 1 : 1.15f;
                for (int i = 0; i < positions.Count; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        float scaleoff = Main.rand.NextFloat(0.2f);
                        Vector2 scale = new Vector2(1 - scaleoff, 1 + scaleoff);
                        Main.EntitySpriteDraw(eyeTex, positions[i] - Main.screenPosition + Main.rand.NextVector2CircularEdge(1.5f, 2f) * NPC.scale * eyeScale + (-Vector2.UnitY * scaleoff * eyeTex.Height * NPC.scale * 0.8f), null, Color.White * 0.4f, 0, eyeTex.Size() * 0.5f, scale * NPC.scale * eyeScale, SpriteEffects.None);
                    }
                    Main.EntitySpriteDraw(eyeTex, positions[i] - Main.screenPosition, null, Color.White, 0, eyeTex.Size() * 0.5f, NPC.scale * eyeScale, SpriteEffects.None);

                }
            }

            if (false)
            {
                Vector2 circlePos = NPC.Center + new Vector2(0, -10).RotatedBy(NPC.rotation);
                float radius = NPC.width * 0.4f;
                for (int i = 0; i < 100; i++)
                {
                    float completion = i / 100f;
                    float rot = completion * MathHelper.TwoPi;
                    Main.EntitySpriteDraw(squareTex, circlePos + rot.ToRotationVector2() * radius - Main.screenPosition, null, Color.Red, 0, squareTex.Size() * 0.5f, 1f, SpriteEffects.None);
                }
            }
            return false;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(NPC.localAI[0]);
            writer.WriteVector2(spawnPos);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            NPC.localAI[0] = reader.ReadSingle();
            spawnPos = reader.ReadVector2();
        }
    }
}
