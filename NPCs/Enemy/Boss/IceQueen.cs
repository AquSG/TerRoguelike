using Microsoft.CodeAnalysis;
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
using static TerRoguelike.Systems.EnemyHealthBarSystem;

namespace TerRoguelike.NPCs.Enemy.Boss
{
    public class IceQueen : BaseRoguelikeNPC
    {
        public Entity target;
        public Vector2 spawnPos;
        public bool ableToHit = true;
        public bool canBeHit = true;
        public override int modNPCID => ModContent.NPCType<IceQueen>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Snow"] };
        public override int CombatStyle => -1;
        public int currentFrame;
        public bool CollisionPass = false;
        public List<ExtraHitbox> hitboxes = new List<ExtraHitbox>()
        {
            new ExtraHitbox(new Point(80, 80), new Vector2(0)),
            new ExtraHitbox(new Point(40, 40), new Vector2(-55, -12)),
            new ExtraHitbox(new Point(40, 40), new Vector2(55, -12)),
            new ExtraHitbox(new Point(34, 34), new Vector2(-90, -20)),
            new ExtraHitbox(new Point(34, 34), new Vector2(90, -20)),
            new ExtraHitbox(new Point(45, 45), new Vector2(0, 59)),
            new ExtraHitbox(new Point(40, 40), new Vector2(0, -84)),
            new ExtraHitbox(new Point(26, 26), new Vector2(0, -54)),
        };
        Texture2D glowTex;
        Texture2D squareTex;
        public SlotId IceWindSlot;

        public int deadTime = 0;
        public int cutsceneDuration = 180;
        public int deathCutsceneDuration = 180;

        public static Attack None = new Attack(0, 0, 180);
        public static Attack IceWave = new Attack(1, 40, 400);
        public static Attack Snowflake = new Attack(2, 40, 300);
        public static Attack Spin = new Attack(3, 30, 420);
        public static Attack IceRain = new Attack(4, 40, 308);
        public static Attack IceFog = new Attack(5, 40, 105);
        public static Attack Summon = new Attack(6, 30, 110);
        public float defaultMaxSpeed = 16f;
        public float defaultAcceleration = 0.2f;
        public float defaultDeceleration = 0.95f;
        public float distanceAbove = -250f;
        public int iceWavePassTime = 90;
        public int iceWaveTimePerShot = 30;
        public int iceWaveShotTelegraph = 15;
        public int iceWaveStartupTime = 30;
        public int snowflakeShotTelegraph = 20;
        public List<int> snowflakeShootTimes = new List<int> { 50, -70, 110, -130, 190, 250 }; //sign indicates direction. last 2 are both directions. absolute value is the time shot relating to NPC.ai[1]
        public int snowflakeCount = 14;
        public float snowflakeMaxHorizontalVelocity = 38f;
        public int spinWindUp = 60;
        public int spinWindDown = 60;
        public int spinFireRate = 8;
        public int spinSuperProjCooldown = 35;
        public int iceRainFireRate = 14;
        public int iceRainTelegraph = 42;
        public int iceFogTelegraph = 45;
        public Vector2 summonPositionStartTelegraph = -Vector2.One;
        public Vector2 summonPosition = -Vector2.One;
        public List<int> summonTimes = new List<int> { 20, 60 };

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 6;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 140;
            NPC.height = 140;
            NPC.aiStyle = -1;
            NPC.damage = 30;
            NPC.lifeMax = 30000;
            NPC.HitSound = SoundID.NPCHit7;
            NPC.knockBackResist = 0f;
            modNPC.drawCenter = new Vector2(0, 0);
            modNPC.IgnoreRoomWallCollision = true;
            modNPC.SpecialProjectileCollisionRules = true;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            glowTex = TexDict["IceQueenGlow"];
            squareTex = TexDict["Square"];
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
            switch (currentFrame)
            {
                default:
                case 0:
                    hitboxes[3].active = true;
                    hitboxes[4].active = true;
                    break;
                case 1:
                    hitboxes[3].active = false;
                    hitboxes[4].active = true;
                    break;
                case 2:
                    hitboxes[3].active = true;
                    hitboxes[4].active = false;
                    break;
                case 3:
                case 4:
                case 5:
                    hitboxes[3].active = false;
                    hitboxes[4].active = false;
                    break;
            }
            if (NPC.localAI[0] >= 0)
            {
                if (SoundEngine.TryGetActiveSound(IceWindSlot, out var sound) && sound.IsPlaying)
                {
                    sound.Volume -= deadTime > 0 ? 0.025f : 0.0015f;
                }
            }
            else
            {
                if (SoundEngine.TryGetActiveSound(IceWindSlot, out var sound) && sound.IsPlaying)
                {
                    sound.Volume += NPC.localAI[0] < -100 ? 2f : -1.5f;
                    if (NPC.localAI[0] == -1)
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
                SetBossTrack(IceQueenTheme);
            }

            ableToHit = NPC.localAI[0] >= 0 && deadTime == 0;
            canBeHit = true;

            if (NPC.localAI[0] < 0)
            {
                target = modNPC.GetTarget(NPC);

                if (NPC.localAI[0] == -cutsceneDuration)
                {
                    IceWindSlot = SoundEngine.PlaySound(SoundID.DD2_BookStaffTwisterLoop with { Volume = 0.008f, PitchVariance = 0.05f }, NPC.Center);
                    if (SoundEngine.TryGetActiveSound(IceWindSlot, out var sound) && sound.IsPlaying)
                    {
                        sound.Volume = 0;
                    }
                    CutsceneSystem.SetCutscene(spawnPos, cutsceneDuration, 30, 30, 2.5f);
                }
                NPC.localAI[0]++;

                if (NPC.localAI[0] < -30)
                {
                    if (NPC.localAI[0] < -90)
                    {
                        Vector2 offset = NPC.localAI[0] % 2 == 0 ? new Vector2(Main.rand.NextFloat(-32, 32), Main.rand.NextFloat(-120, 120)) : new Vector2(Main.rand.NextFloat(-120, 120), Main.rand.NextFloat(-32, 0));
                        ParticleManager.AddParticle(new Smoke(
                            NPC.Center + offset, Main.rand.NextVector2CircularEdge(0.8f, 0.8f) * Main.rand.NextFloat(0.6f, 0.86f), 120, Color.LightCyan * 0.6f, new Vector2(0.4f),
                            Main.rand.Next(15), Main.rand.NextFloat(MathHelper.TwoPi), Main.rand.NextBool() ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0.98f));
                        if (NPC.localAI[0] > -cutsceneDuration + 40 && NPC.localAI[0] % 3 == 0)
                        {
                            ParticleManager.AddParticle(new Snow(
                            NPC.Center + offset, Main.rand.NextVector2CircularEdge(1, 1f) * Main.rand.NextFloat(2, 3),
                            300, Color.White * 0.7f, new Vector2(Main.rand.NextFloat(0.02f, 0.035f)), Main.rand.NextFloat(MathHelper.TwoPi), 0.96f, 0.04f, 180, 0));
                        }
                    }
                    
                    if (NPC.localAI[0] > -90)
                    {
                        NPC.Opacity = (NPC.localAI[0] + 90) / 60f;
                    }
                }
                
                else if (NPC.localAI[0] == -30)
                {
                    NPC.Opacity = 1;
                    NPC.immortal = false;
                    NPC.dontTakeDamage = false;
                    NPC.ai[1] = 0;
                    enemyHealthBar = new EnemyHealthBar([NPC.whoAmI], NPC.FullName);
                }
            }
            else
            {
                NPC.localAI[0]++;
                BossAI();
                bool defaultRotation = NPC.ai[0] != Spin.Id;
                if (defaultRotation)
                    NPC.rotation = NPC.rotation.AngleLerp(MathHelper.Clamp(NPC.velocity.X * 0.05f, -MathHelper.PiOver2 * 0.33f, MathHelper.PiOver2 * 0.33f), 0.25f);
            }
        }
        public void BossAI()
        {
            target = modNPC.GetTarget(NPC);

            NPC.ai[1]++;
            NPC.velocity *= 0.985f;

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
                    NPC.direction = 1;

                if (NPC.ai[1] >= None.Duration)
                {
                    ChooseAttack();
                }
                else
                {

                }
            }

            if (NPC.ai[0] == IceWave.Id)
            {
                NPC.localAI[1]++;
                bool outsideRoom = false;
                if (modNPC.isRoomNPC)
                {
                    if (Math.Abs(NPC.ai[3]) > iceWaveShotTelegraph)
                    {
                        Room room = RoomList[modNPC.sourceRoomListID];
                        Vector2 checkPos = new Vector2(NPC.Center.X, room.RoomPosition16.Y + room.RoomCenter16.Y);
                        if (!room.GetRect().Contains((int)checkPos.X, (int)checkPos.Y))
                            outsideRoom = true;
                    }
                }
                if (outsideRoom && Math.Abs(NPC.ai[3]) < iceWavePassTime - iceWaveShotTelegraph)
                    NPC.ai[3] = (iceWavePassTime - iceWaveShotTelegraph) * Math.Sign(NPC.ai[3]);

                if (Math.Abs(NPC.ai[3]) >= iceWavePassTime)
                {
                    NPC.direction *= -1;
                    NPC.ai[3] = 0;
                    currentFrame = 0;
                    NPC.localAI[1] = 1;
                }
                Vector2 targetPos = target == null ? spawnPos : target.Center;
                if (NPC.ai[3] == 0)
                    NPC.ai[3] += NPC.direction;
                else if (Math.Sign(targetPos.X - NPC.Center.X) == -Math.Sign(NPC.ai[3]))
                {
                    NPC.ai[3] += Math.Sign(NPC.ai[3]);
                }
                if (NPC.ai[1] >= IceWave.Duration)
                    NPC.ai[1]--;

                Vector2 wantedPos = targetPos + new Vector2(0, -282);
                wantedPos.X = NPC.Center.X + Math.Sign(NPC.ai[3]) * 250;

                if (NPC.velocity.Length() < defaultMaxSpeed * 0.5f)
                    NPC.velocity += (wantedPos - NPC.Center).SafeNormalize(Vector2.UnitY) * defaultAcceleration;
                if (NPC.velocity.Length() > defaultMaxSpeed * 0.5f)
                    NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * defaultMaxSpeed * 0.5f;

                
                if (NPC.localAI[1] >= iceWaveStartupTime && (Math.Abs(NPC.ai[3]) <= iceWavePassTime - iceWaveShotTelegraph || currentFrame != 0))
                {
                    int progress = ((int)NPC.localAI[1] - iceWaveStartupTime) % iceWaveTimePerShot;
                    if (progress < iceWaveShotTelegraph)
                        currentFrame = Math.Sign(NPC.ai[3]) == -1 ? 1 : 2;
                    else
                        currentFrame = 0;
                    if (progress == iceWaveShotTelegraph)
                    {
                        Vector2 projSpawnPos = NPC.Center + new Vector2(Math.Sign(NPC.ai[3]) * 60, 0).RotatedBy(NPC.rotation);
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), projSpawnPos, (targetPos - projSpawnPos).SafeNormalize(Vector2.UnitY) * 9, ModContent.ProjectileType<IceWave>(), NPC.damage, 0);
                        if (NPC.ai[1] == IceWave.Duration - 1 && Math.Abs(NPC.ai[3]) > 1 && Math.Abs(NPC.ai[3]) <= iceWaveShotTelegraph * 2)
                            NPC.ai[1]++;
                    }
                }
                else
                    currentFrame = 0;

                if (NPC.ai[1] >= IceWave.Duration)
                {
                    NPC.localAI[1] = 0;
                    NPC.localAI[2] = 0;
                    currentFrame = 0;
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = IceWave.Id;
                    NPC.ai[3] = 0;
                }
            }
            else if (NPC.ai[0] == Snowflake.Id)
            {
                Vector2 wantedPos = spawnPos + new Vector2(0, -640);
                if (modNPC.isRoomNPC)
                {
                    Room room = RoomList[modNPC.sourceRoomListID];
                    wantedPos = room.RoomPosition16 + new Vector2(room.RoomDimensions16.X * 0.5f, 160);
                }
                if (NPC.ai[1] < 50)
                {
                    NPC.velocity *= 0.96f;
                    float targetRadius = 48f;
                    if (NPC.ai[1] == 1 && NPC.Center.Distance(wantedPos) > targetRadius)
                        NPC.ai[1]--;

                    if (NPC.ai[1] <= 0)
                    {
                        if (NPC.velocity.Length() < defaultMaxSpeed)
                            NPC.velocity += (wantedPos - NPC.Center).SafeNormalize(Vector2.UnitY) * defaultAcceleration * 2.6f;
                    }
                    else
                    {
                        NPC.velocity *= 0.92f;
                        NPC.Center += (wantedPos - NPC.Center) * 0.075f;
                    }
                        

                    if (NPC.velocity.Length() > defaultMaxSpeed)
                        NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * defaultMaxSpeed;

                    if (target != null)
                    {
                        if (target.Center.X > NPC.Center.X)
                            NPC.direction = 1;
                        else
                            NPC.direction = -1;
                    }
                    else
                        NPC.direction = 1;

                    if (NPC.ai[1] == 30)
                        currentFrame = 3;
                }
                else
                {
                    if (NPC.ai[1] == 50)
                    {
                        NPC.localAI[0] = 30;
                        NPC.velocity = Vector2.Zero;
                    }

                    int bothDirShootStart = snowflakeShootTimes.Count - 2;
                    for (int i = 0; i < snowflakeShootTimes.Count; i++)
                    {
                        int shootTime = Math.Abs(snowflakeShootTimes[i]);
                        if (NPC.ai[1] == shootTime - 6)
                            SoundEngine.PlaySound(SoundID.DD2_GhastlyGlaiveImpactGhost with { Volume = 0.6f, Pitch = 0.5f, PitchVariance = 0.1f, MaxInstances = 4 }, NPC.Center);
                        if (NPC.ai[1] == shootTime)
                        {
                            SoundEngine.PlaySound(SoundID.NPCDeath7 with { Volume = 0.6f, Pitch = 0.3f, PitchVariance = 0.2f, MaxInstances = 4 }, NPC.Center);
                            int direction = i >= bothDirShootStart ? 0 : Math.Sign(snowflakeShootTimes[i]) * NPC.direction;
                            int start = direction == 0 ? -1 : direction;
                            for (int d = start; d <= (direction == 0 ? 1 : start); d += 2)
                            {
                                float extraVelocity = direction == 0 ? i % 2 : (i % 4 >= 2 ? 1 : 0);
                                extraVelocity *= d * 1.4f;
                                int skip = 0;
                                if (i == 1)
                                    skip = 1;
                                else if (i == bothDirShootStart && d == -1)
                                    skip = 1;
                                for (int j = skip; j < snowflakeCount; j++)
                                {
                                    Vector2 projVel = new Vector2((snowflakeMaxHorizontalVelocity * d * (j / (float)snowflakeCount)) + extraVelocity, Main.rand.NextFloat(-0.14f, 0));
                                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(0, 0), projVel, ModContent.ProjectileType<RoyalSnowflake>(), NPC.damage, 0);

                                    if (j % 5 == 0)
                                    {
                                        ParticleManager.AddParticle(new Snow(
                                        NPC.Center + new Vector2(d * 40, -16), new Vector2(d * Main.rand.NextFloat(2, 3), -Main.rand.NextFloat(2)),
                                        120, Color.White * 0.66f, new Vector2(Main.rand.NextFloat(0.03f, 0.04f)), Main.rand.NextFloat(MathHelper.TwoPi), 0.96f, 0.04f, 60, 0));
                                    }
                                }
                            }
                            switch (direction)
                            {
                                default:
                                case 0:
                                    currentFrame = 0;
                                    break;
                                case -1:
                                    currentFrame = 2;
                                    break;
                                case 1:
                                    currentFrame = 1;
                                    break;
                            }
                        }
                        else if (NPC.ai[1] == shootTime + snowflakeShotTelegraph)
                            currentFrame = 3;
                    }
                }

                if (NPC.ai[1] >= Snowflake.Duration)
                {
                    currentFrame = 0;
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Snowflake.Id;
                }
            }
            else if (NPC.ai[0] == Spin.Id)
            {
                NPC.velocity *= 0.95f;
                currentFrame = 5;
                float interpolant = 1f;
                bool windUp = NPC.ai[1] < spinWindUp;
                bool windDown = NPC.ai[1] > Spin.Duration - spinWindDown;
                if (windUp)
                {
                    if (modNPC.isRoomNPC)
                    {
                        Room room = RoomList[modNPC.sourceRoomListID];
                        if (!room.GetRect().Contains(NPC.getRect()))
                            NPC.velocity += (spawnPos - NPC.Center).SafeNormalize(Vector2.UnitY) * 0.4f;
                    }
                    interpolant = NPC.ai[1] / spinWindUp;
                }
                else if (windDown)
                {
                    interpolant = (Spin.Duration - NPC.ai[1]) / spinWindDown;
                    currentFrame = 3;
                }
                NPC.rotation += 0.2f * NPC.direction * interpolant;
                if (windDown)
                    NPC.rotation = NPC.rotation.AngleLerp(0, MathHelper.Lerp(0.17f, 0, interpolant));

                if (!windUp && !windDown)
                {
                    NPC.ai[3]++;
                    int progress = (int)NPC.ai[1] - spinWindUp;
                    spinFireRate = 2;
                    if (progress % spinFireRate == 0)
                    {
                        int projType = ModContent.ProjectileType<Iceflake>();
                        float speed = 2f;
                        if (NPC.ai[3] >= spinSuperProjCooldown && Main.rand.NextBool())
                        {
                            projType = ModContent.ProjectileType<IceBomb>();
                            NPC.ai[3] = Main.rand.Next(3);
                            speed = 8f;
                        }

                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + (Vector2.UnitY * 90).RotatedBy(NPC.rotation), Vector2.UnitY.RotatedBy(NPC.rotation) * speed, projType, NPC.damage, 0);
                    }

                }


                if (NPC.ai[1] >= Spin.Duration)
                {
                    NPC.ai[0] = 30;
                    currentFrame = 0;
                    NPC.rotation = 0;
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Spin.Id;
                    NPC.ai[3] = 0;
                }
            }
            else if (NPC.ai[0] == IceRain.Id)
            {
                currentFrame = NPC.ai[1] % (iceRainFireRate * 2) < iceRainFireRate ? 3 : 4;
                if (NPC.ai[1] < iceRainTelegraph)
                {

                }
                else
                {
                    int progress = (int)NPC.ai[1] - iceRainTelegraph;
                    if (progress % iceRainFireRate == 0)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + (Vector2.UnitY * 90).RotatedBy(NPC.rotation), Vector2.UnitY.RotatedBy(Main.rand.NextFloat(-0.1f, 0.1f)) * 2, ModContent.ProjectileType<Iceflake>(), NPC.damage, 0);
                    }
                }
                
                if (NPC.ai[1] >= IceRain.Duration)
                {
                    currentFrame = 0;
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = None.Duration - 60;
                    NPC.ai[2] = IceRain.Id;
                }
            }
            else if (NPC.ai[0] == IceFog.Id)
            {
                Vector2 wantedPos = target == null ? spawnPos : target.Center + new Vector2(0, distanceAbove - 25);
                if (wantedPos.Y > spawnPos.Y)
                    wantedPos.Y = spawnPos.Y;

                bool inRoom = true;
                if (modNPC.isRoomNPC)
                {
                    Room room = RoomList[modNPC.sourceRoomListID];
                    Rectangle roomRect = room.GetRect();
                    roomRect.Inflate(-100, -100);
                    if (!roomRect.Contains(wantedPos.ToPoint()))
                    {
                        wantedPos = roomRect.ClosestPointInRect(wantedPos);
                    }
                    inRoom = roomRect.Contains(NPC.Center.ToPoint());
                }

                float targetDist = 64f;
                if (NPC.ai[1] <= 1 && (Math.Abs(NPC.Center.X - wantedPos.X) > targetDist || NPC.Center.Y > spawnPos.Y || !inRoom))
                {
                    NPC.velocity *= 0.98f;
                    if (NPC.ai[1] == 1)
                        NPC.ai[1]--;
                    if (NPC.velocity.Length() < defaultMaxSpeed)
                        NPC.velocity += (wantedPos - NPC.Center).SafeNormalize(Vector2.UnitY) * defaultAcceleration * 1.6f;
                }
                else
                    NPC.velocity *= 0.95f;

                if (NPC.velocity.Length() > defaultMaxSpeed)
                    NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * defaultMaxSpeed;

                if (NPC.ai[1] >= 1)
                {
                    if (NPC.ai[1] < iceFogTelegraph)
                    {
                        currentFrame = 3;
                        if (NPC.ai[1] < iceFogTelegraph - 10)
                        {
                            Vector2 offset = Main.rand.NextVector2CircularEdge(120, 120) * Main.rand.NextFloat(0.7f, 1f);
                            ParticleManager.AddParticle(new Smoke(
                                NPC.Center + offset, -offset.SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(5f, 7f) + NPC.velocity, 20, Color.LightCyan * 0.8f, new Vector2(0.05f),
                                Main.rand.Next(15), Main.rand.NextFloat(MathHelper.TwoPi), Main.rand.NextBool() ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0.96f, 10, 10));
                        }
                    }
                    else if (NPC.ai[1] == iceFogTelegraph)
                    {
                        SoundEngine.PlaySound(SoundID.Item43 with { Volume = 0.7f, Pitch = 0.2f }, NPC.Center);
                        SoundEngine.PlaySound(SoundID.Item30 with { Volume = 0.5f }, NPC.Center);
                        IceWindSlot = SoundEngine.PlaySound(SoundID.DD2_BookStaffTwisterLoop with { Volume = 0.9f, PitchVariance = 0.05f }, NPC.Center);
                        currentFrame = 0;
                        for (int d = -1; d <= 1; d += 2)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(4 * d, -16), (MathHelper.PiOver2 + (-d * MathHelper.PiOver2)).AngleTowards(MathHelper.PiOver2, MathHelper.PiOver2 * 0.25f).ToRotationVector2() * 10, ModContent.ProjectileType<IceCloudSpawner>(), NPC.damage, 0);
                        }
                    }
                }
                if (NPC.ai[1] >= IceFog.Duration)
                {
                    currentFrame = 0;
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = None.Duration - 60;
                    NPC.ai[2] = IceFog.Id;
                }
            }
            else if (NPC.ai[0] == Summon.Id)
            {
                NPC.velocity *= 0.97f;
                for (int i = 0; i < summonTimes.Count; i++)
                {
                    int summonTime = summonTimes[i];
                    if (summonTime - NPC.ai[1] == 20)
                    {
                        currentFrame = 3;
                    }
                    else if (NPC.ai[1] == summonTime)
                    {
                        currentFrame = 4;
                        NPC dummyNPC = new NPC();
                        dummyNPC.type = ModContent.NPCType<Frostbiter>();
                        dummyNPC.SetDefaults(dummyNPC.type);
                        Rectangle dummyRect = new Rectangle(0, 0, dummyNPC.width, dummyNPC.height);

                        float distanceBelow = 220f;
                        Rectangle plannedRect = new Rectangle((int)(NPC.Center.X - (dummyRect.Width * 0.5f)), (int)(NPC.Center.Y + distanceBelow - (dummyRect.Height * 0.5f)), dummyRect.Width, dummyRect.Height);
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
                                summonPosition = position;
                        }
                        else
                        {
                            summonPosition = position;
                        }
                        summonPositionStartTelegraph = NPC.Center + new Vector2(0, 80).RotatedBy(NPC.rotation);

                    }
                    if (NPC.ai[1] - summonTime >= 0 && NPC.ai[1] - summonTime <= 20)
                    {
                        int time = 20;
                        Vector2 startPos = summonPositionStartTelegraph;
                        float completion = (NPC.ai[1] - summonTime) / 20f;
                        float curveMultiplier = 1f - (float)Math.Pow(Math.Abs(completion - 0.5f) * 2, 2);
                        Vector2 endPos = summonPosition;
                        if (endPos != new Vector2(-1))
                        {
                            endPos += new Vector2(0, 16);

                            Vector2 particlePos = startPos + ((endPos - startPos) * completion);
                            particlePos += new Vector2(Main.rand.NextFloat(-10, 10), Main.rand.NextFloat(-10, 10));

                            ParticleManager.AddParticle(new Square(particlePos, Vector2.Zero, time, Color.HotPink, new Vector2((4f + Main.rand.NextFloat(-0.5f, 0.5f)) * 0.45f), 0, 0.96f, time, true));
                        }
                        
                        if (NPC.ai[1] - summonTime == 20)
                        {
                            SoundEngine.PlaySound(SoundID.DD2_WitherBeastAuraPulse with { Volume = 1f, MaxInstances = 2 }, NPC.Center);
                            SoundEngine.PlaySound(SoundID.DD2_SkeletonSummoned with { Volume = 1f, MaxInstances = 2 }, NPC.Center);

                            if (summonPosition != new Vector2(-1))
                                SpawnManager.SpawnEnemy(ModContent.NPCType<Frostbiter>(), summonPosition, modNPC.sourceRoomListID, 60, 0.45f);
                            summonPosition = new Vector2(-1);
                        }
                    }
                }
                if (NPC.ai[1] == summonTimes[summonTimes.Count - 1] + 20)
                    currentFrame = 3;

                if (NPC.ai[1] >= Summon.Duration)
                {
                    currentFrame = 0;
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = -60;
                    NPC.ai[2] = Summon.Id;
                }
            }

            bool defaultMovement = NPC.ai[0] == None.Id || NPC.ai[0] == IceRain.Id;
            bool sineWaveVelocity = NPC.ai[0] != Spin.Id;
            if (defaultMovement)
            {
                if (target != null)
                {
                    Vector2 targetPos = target.Center + new Vector2(0, distanceAbove);
                    bool LoS = CanHitInLine(new Vector2(targetPos.X, NPC.Bottom.Y), target.Center);
                    if (!LoS)
                        targetPos = target.Center + new Vector2(0, distanceAbove * 0.4f);

                    float targetRadius = LoS ? ( NPC.ai[0] == None.Id ? 64f : 42f) : 32f;
                    if (NPC.Center.Distance(targetPos) > targetRadius)
                    {
                        if (NPC.velocity.Length() < defaultMaxSpeed)
                        {
                            NPC.velocity += (targetPos - NPC.Center).SafeNormalize(Vector2.UnitY) * defaultAcceleration;
                        }
                    }
                    else
                    {
                        NPC.velocity *= 0.98f;
                    }

                    if (NPC.velocity.Length() > defaultMaxSpeed)
                    {
                        NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * defaultMaxSpeed;
                    }
                }
            }
            if (sineWaveVelocity)
            {
                NPC.velocity.Y += (float)Math.Cos(NPC.localAI[0] * 0.05f) * 0.033f;
            }
        }

        public void ChooseAttack()
        {
            NPC.ai[1] = 0;
            int chosenAttack = 0;

            List<Attack> potentialAttacks = new List<Attack>() { IceWave, Snowflake, Spin, IceRain, IceFog, Summon };
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
                enemyHealthBar.ForceEnd(0);
                currentFrame = 0;
                NPC.velocity *= 0;
                NPC.rotation = 0;
                modNPC.ignitedStacks.Clear();
                if (modNPC.isRoomNPC)
                {
                    if (ActiveBossTheme != null)
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

            NPC.Opacity = 0.8f + ((float)Math.Cos(deadTime / 16f) * 0.2f);
            if (deadTime % 20 == 0 || deadTime % 36 == 0 || (deadTime > 70 && deadTime % 12 == 0))
            {
                SoundEngine.PlaySound(SoundID.NPCHit11 with { Volume = 0.6f, MaxInstances = 10 }, NPC.Center);
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact with { Volume = 0.35f, MaxInstances = 10 }, NPC.Center);
                float rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                for (int i = 0; i < 20; i++)
                {
                    ParticleManager.AddParticle(new Square(NPC.Center + (rotation.ToRotationVector2() * Main.rand.NextFloat(4, 20)),
                        (rotation + Main.rand.NextFloat(-0.3f, 0.3f)).ToRotationVector2() * Main.rand.NextFloat(4, 5), 30, Color.Lerp(Color.White, Color.LightCyan, Main.rand.NextFloat()) * 1f, new Vector2(1f), rotation
                        ));
                }
            }
            if (deadTime % 6 == 0)
            {
                int timeleft = (int)MathHelper.Clamp(cutsceneDuration - deadTime, 0, 120);
                ParticleManager.AddParticle(new Square(NPC.Center + new Vector2(Main.rand.NextFloat(-100, 100), -32), -Vector2.UnitY * Main.rand.NextFloat(0.7f, 1f), timeleft, Color.Cyan, new Vector2(0.5f), 0, 0.98f, 60));
                ParticleManager.AddParticle(new Smoke(
                            NPC.Center, Main.rand.NextVector2CircularEdge(1.8f, 1.8f) * Main.rand.NextFloat(0.6f, 0.86f), 120, Color.White * 0.4f * (timeleft / 120f), new Vector2(0.4f),
                            Main.rand.Next(15), Main.rand.NextFloat(MathHelper.TwoPi), Main.rand.NextBool() ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0.98f));

                Vector2 snowPos = NPC.Center + new Vector2(Main.rand.NextFloat(-100, 100), -28);
                if (!ParanoidTileRetrieval(snowPos.ToTileCoordinates()).IsTileSolidGround(true))
                    ParticleManager.AddParticle(new Snow(snowPos, Vector2.UnitY * Main.rand.NextFloat(1f), 300, Color.White * 0.6f, new Vector2(Main.rand.NextFloat(0.018f, 0.024f)), 0, 0.96f, 0.05f, 30, 0, true));
            }
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
            if (NPC.life <= 0 && deadTime > 0)
            {
                SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Volume = 0.7f, Pitch = 1f }, NPC.Center);
                SoundEngine.PlaySound(SoundID.Item48 with { Volume = 0.7f, Pitch = 0 }, NPC.Center);
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastDeath with { Volume = 1f, Pitch = 0 }, NPC.Center);

                Vector2 goreOffset = new Vector2(-28);
                Gore.NewGore(NPC.GetSource_Death(), NPC.Top + goreOffset, Vector2.Zero, 513, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), NPC.Left + goreOffset, -Vector2.UnitX, 514, NPC.scale);
                int g = Gore.NewGore(NPC.GetSource_Death(), NPC.Right + goreOffset + new Vector2(-32, 0), Vector2.UnitX, 514, NPC.scale);
                Main.gore[g].rotation += MathHelper.Pi;
                Gore.NewGore(NPC.GetSource_Death(), NPC.Bottom + goreOffset + new Vector2(0, -24), Vector2.Zero, 515, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), NPC.Center + goreOffset, -Vector2.UnitY, 516, NPC.scale);

                for (int j = 0; j < 5; j++)
                {
                    float rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                    for (int i = 0; i < 18; i++)
                    {
                        ParticleManager.AddParticle(new Square(NPC.Center + (rotation.ToRotationVector2() * Main.rand.NextFloat(5)),
                            (rotation + Main.rand.NextFloat(-0.24f, 0.24f)).ToRotationVector2() * Main.rand.NextFloat(2, 4), 60, Color.Lerp(Color.White, Color.Cyan, Main.rand.NextFloat()) * 1f, new Vector2(0.6f), rotation, 0.96f, 60
                            ));
                    }
                }
                if (!ParanoidTileRetrieval(NPC.Center.ToTileCoordinates()).IsTileSolidGround(true))
                {
                    for (int i = 0; i < 80; i++)
                    {
                        float rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                        Vector2 vel = rotation.ToRotationVector2() * -Main.rand.NextFloat(0.6f, 1.5f);
                        vel.Y -= 1f;
                        if (Main.rand.NextBool(10))
                            vel.Y *= 2.5f;
                        ParticleManager.AddParticle(new Snow(
                            NPC.Center, vel,
                            600, Color.Lerp(Color.White, Color.LightCyan, Main.rand.NextFloat()) * 0.66f, new Vector2(Main.rand.NextFloat(0.02f, 0.027f)), Main.rand.NextFloat(MathHelper.TwoPi), 0.96f, 0.04f, 180, 0));
                    }
                }
            }
        }
        public override void OnKill()
        {
            
        }
        public override void FindFrame(int frameHeight)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;

            NPC.frame = new Rectangle(0, currentFrame * frameHeight, tex.Width, frameHeight);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;
            Color color = Color.Lerp(drawColor, Color.White, 0.2f);

            Main.EntitySpriteDraw(tex, NPC.Center - Main.screenPosition, NPC.frame, color * NPC.Opacity, NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
            int trailLength = (int)MathHelper.Clamp(NPC.velocity.Length() * 0.4f, 0, 2);
            for (int i = trailLength; i >= 0; i--)
            {
                Vector2 offset = (NPC.oldPosition - NPC.position).SafeNormalize(Vector2.UnitY) * 2 * i;
                Main.EntitySpriteDraw(glowTex, NPC.Center + offset - Main.screenPosition, NPC.frame, i == 0 ? Color.White * NPC.Opacity : (Color.White * 0.4f * NPC.Opacity), NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
            }

            bool drawHitboxes = false;
            if (drawHitboxes)
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
            return false;
        }
    }
    public class ExtraHitbox
    {
        public Point dimensions;
        public Vector2 offset;
        public bool active;
        public ExtraHitbox(Point Dimensions, Vector2 Offset, bool Active = true)
        {
            dimensions = Dimensions;
            offset = Offset;
            active = Active;
        }
        public Rectangle GetHitbox(Vector2 origin, float rotation)
        {
            Point hitboxPos = (offset.RotatedBy(rotation) + origin).ToPoint() - new Point(dimensions.X / 2, dimensions.Y / 2);
            return new Rectangle(hitboxPos.X, hitboxPos.Y, dimensions.X, dimensions.Y);
        }
    }
}
