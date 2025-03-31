using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
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
using TerRoguelike.Utilities;
using static TerRoguelike.Managers.TextureManager;
using static TerRoguelike.Schematics.SchematicManager;
using static TerRoguelike.Systems.MusicSystem;
using static TerRoguelike.Systems.RoomSystem;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using static TerRoguelike.Systems.ExtraSoundSystem;
using Terraria.GameContent.Animations;
using static TerRoguelike.Systems.EnemyHealthBarSystem;
using static TerRoguelike.MainMenu.TerRoguelikeMenu;
using TerRoguelike.World;
using System.IO;

namespace TerRoguelike.NPCs.Enemy.Boss
{
    public class QueenBee : BaseRoguelikeNPC
    {
        public Entity target;
        public Vector2 spawnPos;
        public bool ableToHit = true;
        public bool canBeHit = true;
        public override int modNPCID => ModContent.NPCType<QueenBee>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Jungle"] };
        public override int CombatStyle => -1;
        public int currentFrame;
        public bool CollisionPass = false;
        public List<ExtraHitbox> hitboxes = new List<ExtraHitbox>()
        {
            new ExtraHitbox(new Point(60, 60), new Vector2(0)),
            new ExtraHitbox(new Point(40, 40), new Vector2(0, -46)),
            new ExtraHitbox(new Point(50, 50), new Vector2(46, 0)),
        };
        public Texture2D squareTex;
        public SlotId BeeSwarmSlot;
        public SlotId ChargeSlot;

        public int deadTime = 0;
        public int cutsceneDuration = 180;
        public int deathCutsceneDuration = 240;

        public static Attack None = new Attack(0, 0, 75);
        public static Attack Shotgun = new Attack(1, 30, 224);
        public static Attack BeeSwarm = new Attack(2, 13, 360);
        public static Attack Charge = new Attack(3, 30, 230);
        public static Attack HoneyVomit = new Attack(4, 30, 80);
        public static Attack Summon = new Attack(5, 20, 110);
        public int shotgunFireRate = 24;
        public float shotgunRecoilInterpolant = 0;
        public Vector2 shotgunRecoilAnchorPos = Vector2.Zero;
        public int beeSwarmRotateDirection = 1;
        public int chargingDuration = 25;
        public int chargeWindup = 20;
        public int chargeEndLag = 20;
        public Vector2 summonPositionStartTelegraph = -Vector2.One;
        public Vector2 summonPosition = -Vector2.One;
        public List<int> summonTimes = new List<int> { 20, 60 };
        public bool attackInitialized = false;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 12;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            modNPC.TerRoguelikeBoss = true;
            NPC.width = 60;
            NPC.height = 60;
            NPC.aiStyle = -1;
            NPC.damage = 30;
            NPC.lifeMax = 25000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.knockBackResist = 0f;
            modNPC.drawCenter = new Vector2(0, -32);
            modNPC.IgnoreRoomWallCollision = true;
            modNPC.SpecialProjectileCollisionRules = true;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
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
            spawnPos = NPC.Center + new Vector2(48, 0);
            NPC.ai[2] = None.Id;
            ableToHit = false;
            NPC.Center += new Vector2(2546, 0);
        }
        public override void PostAI()
        {
            if (deadTime > 0)
            {
                if (SoundEngine.TryGetActiveSound(BeeSwarmSlot, out var sound) && sound.IsPlaying)
                {
                    sound.Volume -= 0.016f;
                }
            }
            else if (NPC.localAI[1] > 0)
            {
                if (SoundEngine.TryGetActiveSound(BeeSwarmSlot, out var sound) && sound.IsPlaying)
                {
                    sound.Position = NPC.Center;
                    if (NPC.localAI[1] > BeeSwarm.Duration - 180)
                    {
                        sound.Volume -= 0.0025f;
                    }
                    NPC.localAI[1]++;
                }
            }
            if (NPC.ai[0] == Charge.Id || NPC.localAI[0] < 0)
            {
                if (SoundEngine.TryGetActiveSound(ChargeSlot, out var sound) && sound.IsPlaying)
                {
                    sound.Position = NPC.Center;
                }
            }

            bool dashingFrames = NPC.ai[0] == Charge.Id;
            if (dashingFrames)
            {
                currentFrame = (int)NPC.frameCounter % 4 + 8;
            }
            else
            {
                currentFrame = (int)NPC.frameCounter % 8;
            }

            switch (currentFrame)
            {
                default:
                case var expression when (currentFrame < 8):
                    hitboxes[1].active = true;
                    hitboxes[2].active = false;
                    break;
                case var expression when (currentFrame >= 8):
                    hitboxes[1].active = false;
                    hitboxes[2].active = true;
                    break;
            }
            NPC.spriteDirection = NPC.direction;
            for (int i = 0; i < hitboxes.Count; i++)
            {
                var hitbox = hitboxes[i];
                hitbox.offset.X = Math.Abs(hitbox.offset.X) * -NPC.spriteDirection;
            }
        }
        public override void AI()
        {
            NPC.netSpam = 0;
            NPC.localAI[3] = 0;
            if (modNPC.isRoomNPC)
            {
                int checkType = ModContent.NPCType<Hornet>();
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active || npc.life <= 0 || npc.friendly || npc.ModNPC() == null)
                        continue;
                    if (npc.type == checkType && npc.ModNPC().sourceRoomListID == modNPC.sourceRoomListID)
                    {
                        NPC.localAI[3]++;
                    }
                }
            }

            NPC.frameCounter += 0.25d;

            if (deadTime > 0)
            {
                CheckDead();
                return;
            }
            if (modNPC.isRoomNPC && NPC.localAI[0] == -(cutsceneDuration + 30))
            {
                SetBossTrack(QueenBeeTheme);
            }

            ableToHit = NPC.localAI[0] >= 0 && deadTime == 0;
            canBeHit = true;

            if (NPC.localAI[0] < 0)
            {
                target = modNPC.GetTarget(NPC);

                if (NPC.localAI[0] == -cutsceneDuration)
                {
                    NPC.velocity.X = -27;
                    CutsceneSystem.SetCutscene(spawnPos, cutsceneDuration, 30, 30, 2.5f, CutsceneSystem.CutsceneSource.Boss);
                }
                NPC.localAI[0]++;

                if (NPC.localAI[0] > -120)
                {
                    NPC.velocity *= 0.972f;
                }
                if (NPC.localAI[0] == -90)
                    ChargeSlot = SoundEngine.PlaySound(SoundID.Item173 with { Volume = 1f }, NPC.Center);
                if (NPC.localAI[0] == -30)
                {
                    NPC.immortal = false;
                    NPC.dontTakeDamage = false;
                    NPC.ai[1] = 0;
                    if (!TerRoguelikeWorld.escape)
                        enemyHealthBar = new EnemyHealthBar([NPC.whoAmI], NPC.GivenOrTypeName);
                }
            }
            else
            {
                NPC.localAI[0]++;
                BossAI();
                if (NPC.ai[0] == None.Id)
                {
                    NPC.rotation = NPC.rotation.AngleLerp(0, 0.025f).AngleTowards(0, 0.015f);
                }
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
                attackInitialized = false;
                UpdateDirection();

                if (NPC.ai[1] >= None.Duration)
                {
                    Room room = modNPC.GetParentRoom();
                    bool centerCollide = NPC.Center.Distance(spawnPos) > 480 && (ParanoidTileRetrieval(NPC.Center.ToTileCoordinates()).IsTileSolidGround(true) || ParanoidTileRetrieval((NPC.Center + Vector2.UnitY * -17).ToTileCoordinates()).IsTileSolidGround(true));
                    bool roomCollide = room != null && !room.GetRect().Contains(NPC.getRect());
                    
                    if (roomCollide || centerCollide)
                    {
                        if (roomCollide)
                        {
                            NPC.velocity += (room.GetRect().ClosestPointInRect(NPC.Center) - NPC.Center).SafeNormalize(Vector2.UnitY) * 0.1f;
                        }
                        else if (centerCollide)
                        {
                            NPC.velocity += (spawnPos - NPC.Center).SafeNormalize(Vector2.UnitY) * 0.15f;
                        }
                        NPC.ai[1]--;
                    }
                }

                if (NPC.ai[1] >= None.Duration)
                {
                    ChooseAttack();
                }
                else
                {
                    if (hardMode && Math.Abs(NPC.rotation) < 0.1f && NPC.ai[1] < None.Duration - 2)
                    {
                        NPC.ai[1]++;
                    }
                        

                    if (target != null)
                    {
                        Vector2 targetPos = target.Center + new Vector2(0, -240);
                        targetPos = TileCollidePositionInLine(target.Center + new Vector2(0, -120), targetPos);
                        if (NPC.velocity.Length() < 10)
                            NPC.velocity += (targetPos - NPC.Center).SafeNormalize(Vector2.UnitY) * 0.15f;
                        if (NPC.velocity.Length() > 10)
                            NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * 10;
                    }
                }
            }

            if (NPC.ai[0] == Shotgun.Id)
            {
                NPC.velocity *= 0.97f;
                UpdateDirection();
                if (NPC.direction != NPC.oldDirection)
                {
                    NPC.rotation += MathHelper.PiOver2 * 0.4f * NPC.direction;
                }
                Vector2 targetPos = target == null ? spawnPos: target.Center;
                float buttRot = (MathHelper.PiOver2 * 0.8f * NPC.direction);
                float targetAngle = target == null ? -buttRot : (target.Center - NPC.Center).ToRotation() - buttRot;
                targetAngle -= (NPC.direction == -1 ? MathHelper.PiOver2 * 2f : 0);
                NPC.rotation = NPC.rotation.AngleLerp(targetAngle, 0.04f).AngleTowards(targetAngle, 0.04f);
                float fireDirection = NPC.rotation + buttRot + (NPC.direction == -1 ? MathHelper.PiOver2 * 2f : 0);
                Vector2 baseOffset = new Vector2(6 * NPC.direction, 16);
                int startupTime = shotgunFireRate * 2;

                if (NPC.ai[1] < startupTime)
                {
                    Vector2 potentialProjSpawnPos = baseOffset.RotatedBy(NPC.rotation) + NPC.Center;
                    if (ParanoidTileRetrieval(potentialProjSpawnPos.ToTileCoordinates()).IsTileSolidGround(true))
                    {
                        NPC.velocity += (targetPos - NPC.Center).SafeNormalize(Vector2.UnitY) * 0.1f;
                    }
                }

                if (NPC.ai[1] > 0 && NPC.ai[1] % shotgunFireRate == 0)
                {
                    shotgunRecoilAnchorPos = NPC.Center;
                    shotgunRecoilInterpolant = 1f;
                    if (NPC.ai[1] >= startupTime)
                    {
                        SoundEngine.PlaySound(SoundID.Item17 with { Volume = 1f, PitchVariance = 0.05f }, NPC.Center);
                        ChargeSlot = SoundEngine.PlaySound(SoundID.Item38 with { Volume = 0.2f, Pitch = 0.2f, PitchVariance = 0.05f }, NPC.Center);
                        bool ruin = RuinedMoonActive;
                        for (int i = 0; i < 12; i++)
                        {
                            Vector2 projSpawnPos = baseOffset + ((Vector2.UnitX * 16).RotatedBy(i * MathHelper.TwoPi / 12f) * new Vector2(1f, 0.6f));
                            projSpawnPos = projSpawnPos.RotatedBy(NPC.rotation);
                            if (!TerRoguelike.mpClient)
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + projSpawnPos, (fireDirection.ToRotationVector2() * Main.rand.NextFloat(7, 7.5f)).RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f)), ModContent.ProjectileType<Stinger>(), NPC.damage, 0, -1, 1);
                            if (ruin && Main.rand.NextBool(4))
                            {
                                if (!TerRoguelike.mpClient)
                                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + projSpawnPos, (fireDirection.ToRotationVector2() * Main.rand.NextFloat(7, 7.5f)).RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f)), ModContent.ProjectileType<HoneyGlob>(), NPC.damage, 0, -1, i / 4);
                            }
                        }
                    }
                    else
                        SoundEngine.PlaySound(SoundID.Item17 with { Volume = 0.65f, Pitch = -0.15f, PitchVariance = 0.05f }, NPC.Center);
                    Color outlineColor = Color.Goldenrod;
                    Color fillColor = Color.Lerp(outlineColor, Color.Black, 0.35f);
                    for (int i = 0; i < 12; i++)
                    {
                        Vector2 particleSpawnPos = baseOffset + ((Vector2.UnitX * 16 * Main.rand.NextFloat(0.8f, 1f)).RotatedBy(i * MathHelper.TwoPi / 12f) * new Vector2(1f, 0.6f));
                        particleSpawnPos = particleSpawnPos.RotatedBy(NPC.rotation);
                        Vector2 particleVel = (fireDirection.ToRotationVector2() * Main.rand.NextFloat(3, 5)).RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f));
                        ParticleManager.AddParticle(new BallOutlined(
                            NPC.Center + particleSpawnPos, particleVel,
                            60, outlineColor, fillColor, new Vector2(Main.rand.NextFloat(0.14f, 0.28f)), 4, 0, 0.96f, 50));
                    }

                    NPC.Center -= fireDirection.ToRotationVector2() * 12;
                }
                if (shotgunRecoilInterpolant > 0)
                {
                    NPC.Center += (shotgunRecoilAnchorPos - NPC.Center) * (1 - shotgunRecoilInterpolant) * 0.25f;
                    Vector2 offset = (targetPos - NPC.Center).SafeNormalize(Vector2.Zero) * 0.25f;
                    shotgunRecoilAnchorPos += offset;
                    NPC.Center += offset;

                    shotgunRecoilInterpolant -= 0.05f;
                }


                if (NPC.ai[1] >= Shotgun.Duration)
                {
                    shotgunRecoilInterpolant = 0;
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Shotgun.Id;
                }
            }
            else if (NPC.ai[0] == BeeSwarm.Id)
            {
                Room room = modNPC.isRoomNPC ? RoomList[modNPC.sourceRoomListID] : null;
                Vector2 anchor = room != null ? room.RoomPosition16 + room.RoomCenter16 : spawnPos;
                float rotToAnchor = (anchor - NPC.Center).ToRotation();
                if (NPC.ai[1] <= 2)
                {
                    NPC.direction = Math.Sign(NPC.velocity.X);
                    if (NPC.direction == 0)
                        NPC.direction = -1;

                    float orbitMagnitude = room != null ? room.RoomDimensions16.X * 0.25f : 400;
                    Vector2 wantedPos = -rotToAnchor.ToRotationVector2() * orbitMagnitude + anchor;
                    Vector2 targetPos = target != null ? target.Center : NPC.Center;
                    if (NPC.Center.Distance(wantedPos) < 48)
                    {
                        float angleBetween = AngleSizeBetween((NPC.Center - anchor).ToRotation(), (targetPos - anchor).ToRotation());
                        beeSwarmRotateDirection = -Math.Sign(angleBetween);
                        if (beeSwarmRotateDirection == 0)
                            beeSwarmRotateDirection = 1;
                    }
                    else
                    {
                        if (NPC.ai[1] == 2)
                            NPC.ai[1]--;

                        if (NPC.velocity.Length() < 10)
                            NPC.velocity += (wantedPos - NPC.Center).SafeNormalize(Vector2.UnitY) * 0.15f;
                        if (NPC.velocity.Length() > 10)
                            NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * 10;
                    }

                }
                if (NPC.ai[1] >= 2)
                {
                    if (anchor.X > NPC.Center.X)
                        NPC.direction = 1;
                    else
                        NPC.direction = -1;


                    if (NPC.ai[1] < 20)
                        NPC.velocity *= 0.96f;
                    else
                    {
                        NPC.velocity /= 0.98f;
                        if (NPC.ai[1] == 20)
                            NPC.velocity = Vector2.Zero;
                        if (NPC.ai[1] < 60)
                        {
                            NPC.velocity += (rotToAnchor - (MathHelper.PiOver2 * beeSwarmRotateDirection)).ToRotationVector2() * 0.15f;
                        }
                        else
                        {
                            if (NPC.ai[1] == 60)
                            {
                                BeeSwarmSlot = SoundEngine.PlaySound(PharaohSpirit.LocustSwarm with { Volume = 0.4f, PitchVariance = 0.03f }, anchor);
                                NPC.localAI[1] = 1;
                            }
                            NPC.velocity += rotToAnchor.ToRotationVector2() * 0.051f;
                            if (NPC.ai[1] % 4 == 0 && !TerRoguelike.mpClient)
                            {
                                Vector2 projSpawnPos = -rotToAnchor.ToRotationVector2() * 460 + NPC.Center + new Vector2(0, Main.rand.NextFloat(-380, -60) * beeSwarmRotateDirection).RotatedBy(rotToAnchor);
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), projSpawnPos, rotToAnchor.ToRotationVector2() * 9, ModContent.ProjectileType<Bee>(), NPC.damage, 0);
                            }
                        }
                    }
                }
                if (NPC.ai[1] > BeeSwarm.Duration - 45)
                {
                    NPC.velocity *= 0.98f;
                }

                if (NPC.ai[1] >= BeeSwarm.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = -60;
                    NPC.ai[2] = BeeSwarm.Id;
                }
            }
            else if (NPC.ai[0] == Charge.Id)
            {
                float chargeStartDist = 240;
                int chargeStartLag = 30;
                int time = ((int)NPC.ai[1] - chargeStartLag) % (chargeWindup + chargingDuration);
                Vector2 targetPos = target != null ? target.Center : spawnPos;
                Vector2 wantedPos = (NPC.Center - targetPos).SafeNormalize(Vector2.UnitY) * chargeStartDist;
                if (NPC.ai[1] < Charge.Duration - chargeEndLag)
                {
                    if (time < chargeWindup)
                    {
                        float wantedRot = (targetPos - NPC.Center).ToRotation();
                        if (NPC.direction == -1)
                            wantedRot += MathHelper.Pi;

                        if (NPC.Center.X > targetPos.X)
                            NPC.direction = -1;
                        else
                            NPC.direction = 1;
                        if (NPC.direction != NPC.oldDirection)
                        {
                            NPC.rotation += MathHelper.Pi;
                        }

                        NPC.rotation = NPC.rotation.AngleLerp(wantedRot, 0.032f).AngleTowards(wantedRot, 0.021f);
                    }
                    if (time <= 1)
                    {
                        float distance = NPC.Center.Distance(targetPos);
                        if (time == 1 && distance > chargeStartDist - 16 && distance < chargeStartDist + 16)
                        {
                            NPC.velocity *= 0.3f;
                            NPC.velocity += (NPC.Center - targetPos).SafeNormalize(Vector2.UnitY) * 10;
                        }
                        else
                        {
                            if (time == 1)
                                NPC.ai[1]--;

                            NPC.velocity *= 0.97f;
                            NPC.velocity += (wantedPos + targetPos - NPC.Center).SafeNormalize(Vector2.UnitY) * 0.7f;
                            if (NPC.velocity.Length() > 12)
                            {
                                NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * 13;
                            }
                        }
                    }
                    if (time >= 1)
                    {
                        if (time < chargeWindup)
                        {
                            NPC.velocity *= 0.98f;
                        }
                        else
                        {
                            if (time == chargeWindup)
                            {
                                NPC.velocity = (targetPos - NPC.Center).SafeNormalize(Vector2.UnitY) * 17;
                                ChargeSlot = SoundEngine.PlaySound(SoundID.Roar with { Volume = 0.65f, Pitch = 0.04f, SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest, MaxInstances = 4 }, NPC.Center);
                            }
                            NPC.rotation = NPC.velocity.ToRotation();
                            if (NPC.direction == -1)
                                NPC.rotation += MathHelper.Pi;
                            NPC.velocity /= 0.975f;
                        }
                    }
                }
                if (NPC.ai[1] >= Charge.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Charge.Id;
                }
            }
            else if (NPC.ai[0] == HoneyVomit.Id)
            {
                NPC.velocity *= 0.97f;
                UpdateDirection();

                Vector2 projSpawnPos = NPC.Center + new Vector2(16 * NPC.direction, -32).RotatedBy(NPC.rotation);
                if (ParanoidTileRetrieval(projSpawnPos.ToTileCoordinates()).IsTileSolidGround(true))
                {
                    NPC.velocity += (spawnPos - NPC.Center).SafeNormalize(Vector2.UnitY) * 0.2f;
                }

                Color outlineColor = Color.Goldenrod;
                Color fillColor = Color.Lerp(outlineColor, Color.Black, 0.35f);
                if (NPC.ai[1] < 40 && NPC.ai[1] % 20 == 0)
                {
                    SoundEngine.PlaySound(SoundID.Item17 with { Volume = 0.5f, PitchVariance = 0.05f }, NPC.Center);
                    for (int i = 0; i < 7; i++)
                    {
                        Vector2 particleVel = (Vector2.UnitY * Main.rand.NextFloat(1f, 1.7f)).RotatedBy(MathHelper.PiOver2 * 1.3f * -NPC.direction + Main.rand.NextFloat(-0.6f, 0.6f));
                        ParticleManager.AddParticle(new BallOutlined(
                            projSpawnPos, particleVel,
                            60, outlineColor, fillColor, new Vector2(Main.rand.NextFloat(0.14f, 0.28f)), 4, 0, 0.96f, 50));
                    }
                }

                else if (NPC.ai[1] == 40)
                {
                    SoundEngine.PlaySound(SoundID.NPCDeath13 with { Volume = 0.75f }, NPC.Center);
                    int spawnCount = RuinedMoonActive ? 50 : 7;
                    for (int i = 0; i < spawnCount; i++)
                    {
                        Vector2 projVel = (Vector2.UnitY * Main.rand.NextFloat(2f, 3.5f)).RotatedBy(MathHelper.PiOver2 * 1.3f * -NPC.direction + Main.rand.NextFloat(-0.3f, 0.3f));
                        if (!TerRoguelike.mpClient)
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), projSpawnPos, projVel, ModContent.ProjectileType<HoneyGlob>(), NPC.damage, 0, -1, i / 4);

                        Vector2 particleVel = (Vector2.UnitY * Main.rand.NextFloat(1f, 1.7f)).RotatedBy(MathHelper.PiOver2 * 1.3f * -NPC.direction + Main.rand.NextFloat(-0.6f, 0.6f));
                        ParticleManager.AddParticle(new BallOutlined(
                            projSpawnPos, particleVel + NPC.velocity,
                            60, outlineColor, fillColor, new Vector2(Main.rand.NextFloat(0.14f, 0.28f)), 4, 0, 0.96f, 50));
                    }

                }

                if (NPC.ai[1] >= HoneyVomit.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = HoneyVomit.Id;
                }
            }
            else if (NPC.ai[0] == Summon.Id)
            {
                NPC.velocity *= 0.97f;
                for (int i = 0; i < summonTimes.Count; i++)
                {
                    int summonTime = summonTimes[i];
                    if (NPC.ai[1] == summonTime)
                    {
                        UpdateDirection();

                        NPC dummyNPC = new NPC();
                        dummyNPC.type = ModContent.NPCType<Hornet>();
                        dummyNPC.SetDefaults(dummyNPC.type);
                        Rectangle dummyRect = new Rectangle(0, 0, dummyNPC.width, dummyNPC.height);

                        Vector2 spawnPosOffset = new Vector2(48 * NPC.direction, 90);
                        Rectangle plannedRect = new Rectangle((int)(NPC.Center.X - (dummyRect.Width * 0.5f)), (int)(NPC.Center.Y - (dummyRect.Height * 0.5f)), dummyRect.Width, dummyRect.Height);
                        plannedRect.X += (int)spawnPosOffset.X;
                        plannedRect.Y += (int)spawnPosOffset.Y;
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
                        summonPositionStartTelegraph = NPC.Center + new Vector2(8 * NPC.direction, 26).RotatedBy(NPC.rotation) + NPC.velocity * 5;
                        NPC.netUpdate = true;

                    }
                    if (NPC.ai[1] - summonTime >= 0 && NPC.ai[1] - summonTime <= 20)
                    {
                        int time = 20;
                        Vector2 startPos = summonPositionStartTelegraph;
                        float completion = (NPC.ai[1] - summonTime) / 20f;
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
                                SpawnManager.SpawnEnemy(ModContent.NPCType<Hornet>(), summonPosition, modNPC.sourceRoomListID, 60, 0.45f);
                            summonPosition = new Vector2(-1);
                        }
                    }
                }

                if (NPC.ai[1] >= Summon.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = -90;
                    NPC.ai[2] = Summon.Id;
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

            List<Attack> potentialAttacks = new List<Attack>() { Shotgun, BeeSwarm, Charge, HoneyVomit, Summon };
            potentialAttacks.RemoveAll(x => x.Id == (int)NPC.ai[2]);
            if (NPC.localAI[3] > 0)
            {
                potentialAttacks.RemoveAll(x => x.Id == Summon.Id);
            }

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
        public void UpdateDirection()
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
        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            return deadTime == 0 ? null : false;
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
                SoundEngine.PlaySound(SoundID.Item173 with { Volume = 0.8f, Pitch = 0.2f }, NPC.Center);
                ExtraSounds.Add(new ExtraSound(SoundEngine.PlaySound(PharaohSpirit.LocustSwarm with { Volume = 0.4f, MaxInstances = 2 }, NPC.Center), 1, deathCutsceneDuration, 360));
                NPC.velocity *= 0;
                NPC.rotation = 0;
                modNPC.ignitedStacks.Clear();
                modNPC.bleedingStacks.Clear();
                modNPC.ballAndChainSlow = 0;
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
                        if (modChildNPC.isRoomNPC && modChildNPC.sourceRoomListID == modNPC.sourceRoomListID && !TerRoguelike.mpClient)
                        {
                            childNPC.StrikeInstantKill();
                            childNPC.active = false;
                        }
                    }
                }
                CutsceneSystem.SetCutscene(NPC.Center, deathCutsceneDuration, 30, 30, 2.5f, CutsceneSystem.CutsceneSource.Boss);
            }
            deadTime++;

            if (deadTime < 60 && !TerRoguelike.mpClient)
            {
                Vector2 anchor = NPC.Center + modNPC.drawCenter + new Vector2(7 * NPC.direction, 10);
                for (int i = 0; i < 2; i++)
                {
                    Vector2 offset = Main.rand.NextVector2CircularEdge(600, 600) * Main.rand.NextFloat(0.7f, 1f);
                    Vector2 projectileTargettingPos = anchor + new Vector2(Main.rand.NextFloat(-32, 32), Main.rand.NextFloat(-40, 40));
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), anchor + offset, -offset.SafeNormalize(Vector2.UnitY) * 5, ModContent.ProjectileType<CutsceneBee>(), 0, 0, -1, projectileTargettingPos.X, projectileTargettingPos.Y, deadTime + Main.rand.Next(-90, 0));
                }
            }
            if (deadTime > 85 && deadTime % 2 == 0)
            {
                int rate = 3 - ((deadTime - 85) / 60);
                if (Main.rand.NextBool(rate < 1 ? 1 : rate))
                {
                    SoundEngine.PlaySound(SoundID.NPCHit1 with { Volume = 1f }, NPC.Center);
                    Vector2 pos = NPC.Center;
                    int width = (int)(NPC.width * 0.25f);
                    pos.X += Main.rand.Next(-width, width);
                    Vector2 velocity = new Vector2(0, -4.6f).RotatedBy(Main.rand.NextFloat(MathHelper.TwoPi));
                    velocity *= Main.rand.NextFloat(0.3f, 1f);
                    if (Main.rand.NextBool(5))
                        velocity *= 1.5f;
                    Vector2 scale = new Vector2(0.25f, 0.4f) * 0.7f;
                    int time = 130 + Main.rand.Next(70);
                    Color color = Color.Lerp(Color.Lerp(Color.Green, Color.Yellow, Main.rand.NextFloat(0.3f)), Color.Black, 0.48f);
                    ParticleManager.AddParticle(new Blood(pos, velocity, time, Color.Black * 0.3f, scale, velocity.ToRotation(), false));
                    ParticleManager.AddParticle(new Blood(pos, velocity, time, color * 0.8f, scale, velocity.ToRotation(), true));
                }
            }

            if (TerRoguelike.mpClient && deadTime >= deathCutsceneDuration - 31 && !TerRoguelikeWorld.escape)
            {
                NPC.immortal = false;
                NPC.dontTakeDamage = false;
            }
            if (deadTime >= deathCutsceneDuration - 30)
            {
                NPC.immortal = false;
                NPC.dontTakeDamage = false;
                if (!TerRoguelike.mpClient)
                    NPC.StrikeInstantKill();
            }

            if (deadTime == 1)
                NPC.netUpdate = true;

            return deadTime >= cutsceneDuration - 30;
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)NPC.lifeMax * 2000.0; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 18, hit.HitDirection, -1f, NPC.alpha, NPC.color, NPC.scale);
                }
            }
            else if (deadTime > 0)
            {
                SoundEngine.PlaySound(SoundID.NPCDeath66 with { Volume = 1f }, NPC.Center);
                SoundEngine.PlaySound(SoundID.NPCDeath1 with { Volume = 1f }, NPC.Center);
                SoundEngine.PlaySound(SoundID.NPCDeath12 with { Volume = 0.8f, Pitch = -0.5f }, NPC.Center);
                if (SoundEngine.TryGetActiveSound(BeeSwarmSlot, out var sound) && sound.IsPlaying)
                {
                    sound.Stop();
                }
                for (int i = 0; i < 50; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 18, hit.HitDirection, -2f, NPC.alpha, NPC.color, NPC.scale);
                }
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y - 35f), NPC.velocity, 303, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y - 45f), NPC.velocity, 304, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y), NPC.velocity, 305, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 306, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 10f), NPC.velocity, 307, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y - 10f), NPC.velocity, 308, NPC.scale);
                for (int i = 0; i < 60; i++)
                {
                    Vector2 pos = NPC.Center;
                    int width = (int)(NPC.width * 0.25f);
                    pos.X += Main.rand.Next(-width, width);
                    Vector2 velocity = new Vector2(0, -4.6f).RotatedBy(Main.rand.NextFloat(-MathHelper.PiOver4 * 1.5f, MathHelper.PiOver4 * 1.5f));
                    velocity *= Main.rand.NextFloat(0.3f, 1f);
                    if (Main.rand.NextBool(5))
                        velocity *= 1.5f;
                    Vector2 scale = new Vector2(0.25f, 0.4f) * 0.7f;
                    int time = 130 + Main.rand.Next(70);
                    Color color = Color.Lerp(Color.Lerp(Color.Green, Color.Yellow, Main.rand.NextFloat(0.3f)), Color.Black, 0.48f);
                    ParticleManager.AddParticle(new Blood(pos, velocity, time, Color.Black * 0.3f, scale, velocity.ToRotation(), false));
                    ParticleManager.AddParticle(new Blood(pos, velocity, time, color * 0.8f, scale, velocity.ToRotation(), true));
                }
            }
        }
        public override void OnKill()
        {

        }
        public override void FindFrame(int frameHeight)
        {
            if (Main.dedServ)
                return;

            Texture2D tex = TextureAssets.Npc[Type].Value;

            NPC.frame = new Rectangle(0, currentFrame * frameHeight, tex.Width, frameHeight);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;
            Color color = Color.Lerp(drawColor, Color.White, 0.2f);

            modNPC.drawCenter = new Vector2(-12 * NPC.spriteDirection, -32);
            modNPC.drawCenter = modNPC.drawCenter.RotatedBy(NPC.rotation);

            Main.EntitySpriteDraw(tex, NPC.Center - Main.screenPosition + modNPC.drawCenter, NPC.frame, color, NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);

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
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(NPC.localAI[0]);
            writer.WriteVector2(spawnPos);
            writer.Write(deadTime);
            writer.WriteVector2(summonPosition);
            writer.WriteVector2(summonPositionStartTelegraph);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            NPC.localAI[0] = reader.ReadSingle();
            spawnPos = reader.ReadVector2();
            int deadt = reader.ReadInt32();
            if (deadTime == 0 && deadt > 0)
            {
                CheckDead();
                deadTime = 1;
            }
            summonPosition = reader.ReadVector2();
            summonPositionStartTelegraph = reader.ReadVector2();
        }
    }
}
