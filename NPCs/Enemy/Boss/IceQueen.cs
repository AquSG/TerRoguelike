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

        public int deadTime = 0;
        public int cutsceneDuration = 120;
        public int deathCutsceneDuration = 120;

        public static Attack None = new Attack(0, 0, 180);
        public static Attack IceWave = new Attack(1, 40, 400);
        public static Attack Snowflake = new Attack(2, 40, 300);
        public static Attack Spin = new Attack(3, 30, 420);
        public static Attack IceRain = new Attack(4, 40, 308);
        public static Attack IceFog = new Attack(5, 40, 300);
        public static Attack Summon = new Attack(6, 30, 300);
        public float defaultMaxSpeed = 16f;
        public float defaultAcceleration = 0.2f;
        public float defaultDeceleration = 0.95f;
        public int iceWavePassTime = 120;
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
            NPC.DeathSound = SoundID.NPCDeath5;
            NPC.knockBackResist = 0f;
            modNPC.drawCenter = new Vector2(0, 0);
            modNPC.IgnoreRoomWallCollision = true;
            modNPC.SpecialProjectileCollisionRules = true;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            glowTex = TexDict["IceQueenGlow"].Value;
            squareTex = TexDict["Square"].Value;
        }
        public override void OnSpawn(IEntitySource source)
        {
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
                SetBossTrack(CorruptionParasiteTheme);
            }

            ableToHit = NPC.localAI[0] >= 0;
            canBeHit = true;

            NPC.frameCounter += 0.13d;
            if (NPC.localAI[0] < 0)
            {
                target = modNPC.GetTarget(NPC, false, false);

                if (NPC.localAI[0] == -cutsceneDuration)
                {
                    CutsceneSystem.SetCutscene(spawnPos, cutsceneDuration, 30, 30, 2.5f);
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
                bool defaultRotation = NPC.ai[0] != Spin.Id;
                if (defaultRotation)
                    NPC.rotation = NPC.rotation.AngleLerp(MathHelper.Clamp(NPC.velocity.X * 0.05f, -MathHelper.PiOver2 * 0.33f, MathHelper.PiOver2 * 0.33f), 0.25f);
            }
        }
        public void BossAI()
        {
            target = modNPC.GetTarget(NPC, false, false);

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
                if (NPC.ai[1] >= IceFog.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = IceFog.Id;
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

            bool defaultMovement = NPC.ai[0] == None.Id || NPC.ai[0] == IceRain.Id;
            bool sineWaveVelocity = NPC.ai[0] != Spin.Id;
            if (defaultMovement)
            {
                if (target != null)
                {
                    float distanceAbove = -250f;
                    Vector2 targetPos = target.Center + new Vector2(0, distanceAbove);
                    bool LoS = CanHitInLine(new Vector2(targetPos.X, NPC.Bottom.Y), target.Center);
                    if (!LoS)
                        targetPos = target.Center + new Vector2(0, distanceAbove * 0.4f);

                    float targetRadius = LoS ? 42f : 32f;
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

            //List<Attack> potentialAttacks = new List<Attack>() { IceWave, Snowflake, Spin, IceRain, IceFog, Summon };
            List<Attack> potentialAttacks = new List<Attack>() { IceWave, Snowflake, Spin, IceRain };
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
                currentFrame = 0;
                NPC.velocity *= 0;
                NPC.rotation = 0;
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
            if (NPC.life > 0)
            {

            }
            else
            {
                
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

            Main.EntitySpriteDraw(tex, NPC.Center - Main.screenPosition, NPC.frame, color, NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
            int trailLength = (int)MathHelper.Clamp(NPC.velocity.Length() * 0.4f, 0, 3);
            for (int i = trailLength; i >= 0; i--)
            {
                Vector2 offset = (NPC.oldPosition - NPC.position).SafeNormalize(Vector2.UnitY) * 2 * i;
                Main.EntitySpriteDraw(glowTex, NPC.Center + offset - Main.screenPosition, NPC.frame, i == 0 ? Color.White : (Color.White * 0.4f), NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
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
