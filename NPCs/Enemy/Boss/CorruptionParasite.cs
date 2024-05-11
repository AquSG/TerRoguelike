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
    public class CorruptionParasite : BaseRoguelikeNPC
    {
        public Entity target;
        public Vector2 spawnPos;
        public bool ableToHit = true;
        public bool canBeHit = true;
        public override int modNPCID => ModContent.NPCType<CorruptionParasite>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Corrupt"] };
        public override int CombatStyle => -1;
        public int currentFrame;
        public Texture2D headTex;
        public Texture2D bodyTex;
        public Texture2D tailTex;
        public Texture2D eggTex;
        public Texture2D godRayTex;
        public bool CollisionPass = false;
        public Rectangle headFrame;

        public SlotId chargeSlot;
        List<GodRay> deathGodRays = new List<GodRay>();
        public int deadTime = 0;
        public int cutsceneDuration = 180;
        public int deathCutsceneDuration = 180;
        public float diggingEffect = 0;

        public static Attack None = new Attack(0, 0, 300);
        public static Attack Charge = new Attack(1, 30, 525);
        public static Attack WaveTunnel = new Attack(2, 30, 500);
        public static Attack Vomit = new Attack(3, 30, 150);
        public static Attack ProjCharge = new Attack(4, 30, 275);
        public static Attack Summon = new Attack(5, 20, 180);
        public float defaultMinVelocity = 2;
        public float defaultMaxVelocity = 5;
        public float hastyMaxVelocity = 7;
        public float defaultLookingAtThreshold = MathHelper.PiOver4 * 0.5f;
        public float defaultAcceleration = 0.04f;
        public float defaultDecelertaion = 0.02f;
        public float defaultMinRotation = 0.015f;
        public float defaultMaxRotation = 0.05f;
        public int defaultLookingAtBuffer = 0;
        public float segmentRotationInterpolant = 0.95f;
        public float setSegmentRotationInterpolant = 0.95f;
        public float chargeDesiredDist = 1300;
        public Vector2 chargeDesiredPos;
        public int chargeCount = 3;
        public int chargeTelegraph = 40;
        public int chargingDuration = 135;
        public float chargeSpeed = 20f;
        public Vector2 waveTunnelDesiredPos = Vector2.Zero;
        public float waveTunnelCornerPosOffset = 64;
        public float waveTunnelAmplitude = 264f;
        public int waveTunnelTelegraph = 40;
        public int waveTunnelTelegraphBurrowTime = 35;
        public int waveTunnelProjectileTelegraph = 88;
        public int projChargeTelegraph = 40;
        public int projChargingDuration = 135;
        public int projChargeShootTelegraph = 88;
        public float projChargeSpeed = 20f;
        public int summonCount = 3;


        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 1;
            NPCID.Sets.MustAlwaysDraw[modNPCID] = true;
            NPCID.Sets.TrailCacheLength[modNPCID] = 2;
            NPCID.Sets.TrailingMode[modNPCID] = 1;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 38;
            NPC.height = 38;
            NPC.aiStyle = -1;
            NPC.damage = 30;
            NPC.lifeMax = 30000;
            NPC.knockBackResist = 0f;
            modNPC.drawCenter = new Vector2(0, 0);
            modNPC.IgnoreRoomWallCollision = true;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            modNPC.OverrideIgniteVisual = true;
            modNPC.SpecialProjectileCollisionRules = true;
            NPC.behindTiles = true;
            headTex = TexDict["CorruptionParasiteHead"];
            bodyTex = TexDict["CorruptionParasiteBody"];
            tailTex = TexDict["CorruptionParasiteTail"];
            eggTex = TexDict["ParasiticEgg"];
            godRayTex = TexDict["GodRay"];
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
            NPC.ai[1] = -48;
            ableToHit = false;

            int segCount = 50;
            int segmentHeight = 38;
            NPC.position.Y += 320;
            NPC.velocity.Y = -5f;
            for (int i = 0; i < segCount; i++)
            {
                modNPC.Segments.Add(new WormSegment(NPC.Center + (Vector2.UnitY * segmentHeight * i), MathHelper.PiOver2 * 3f, i == 0 ? NPC.height : segmentHeight));
            }
        }
        public override void PostAI()
        {
            bool velocityToRotation = !(NPC.ai[0] == WaveTunnel.Id && NPC.ai[1] >= waveTunnelTelegraph);
            if (velocityToRotation)
                NPC.rotation = NPC.velocity.ToRotation();
            modNPC.UpdateWormSegments(NPC, segmentRotationInterpolant);

            if (NPC.localAI[0] >= 0 && deadTime <= 0)
            {
                if (diggingEffect > 0)
                    diggingEffect--;
                Point tile = new Point((int)(NPC.Center.X / 16f), (int)(NPC.Center.Y / 16f));
                if (ParanoidTileRetrieval(tile.X, tile.Y).IsTileSolidGround(true))
                {
                    if (diggingEffect <= 0)
                    {
                        SoundEngine.PlaySound(SoundID.WormDig with { Volume = 1f }, NPC.Center);
                        diggingEffect += 60 / (NPC.velocity == Vector2.Zero ? (NPC.position - NPC.oldPosition).Length() * 1.5f : NPC.velocity.Length());
                        Color lightColor = Lighting.GetColor(tile);
                        if (lightColor.R <= 30 && lightColor.G <= 30 && lightColor.B <= 30)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                Dust d = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.SpelunkerGlowstickSparkle);
                                Vector2 dVelocity = NPC.velocity == Vector2.Zero ? NPC.position - NPC.oldPosition : NPC.velocity;
                                d.velocity = dVelocity * 0.25f;
                            }
                        }
                    }
                }
            }
            if (SoundEngine.TryGetActiveSound(chargeSlot, out var sound) && sound.IsPlaying)
            {
                sound.Position = NPC.Center;
                sound.Update();
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

            if (NPC.localAI[0] < 0)
            {
                target = modNPC.GetTarget(NPC);

                if (NPC.localAI[0] == -cutsceneDuration)
                {
                    CutsceneSystem.SetCutscene(spawnPos, cutsceneDuration, 30, 30, 2.5f);
                }
                NPC.localAI[0]++;

                if (NPC.localAI[0] > -90 && NPC.localAI[0] < -30)
                {
                    float completion = 1f - (Math.Abs(NPC.localAI[0] + 60) / 30f);
                    NPC.velocity = (NPC.velocity.ToRotation() - (0.07f * completion)).ToRotationVector2() * NPC.velocity.Length();
                    if (NPC.localAI[0] == -50)
                    {
                        SoundEngine.PlaySound(SoundID.Zombie38 with { Volume = 0.26f, Pitch = -1f, PitchVariance = 0.11f, MaxInstances = 3 }, NPC.Center);
                        chargeSlot = SoundEngine.PlaySound(SoundID.DD2_BetsyScream with { Volume = 0.4f, Pitch = -0.6f, PitchVariance = 0f }, NPC.Center);
                    }
                }
                if (NPC.localAI[0] == -30)
                {
                    NPC.velocity = (NPC.velocity.ToRotation().AngleTowards(target != null ? (target.Center - NPC.Center).ToRotation() : NPC.velocity.ToRotation(), 0.02f)).ToRotationVector2() * NPC.velocity.Length();
                    NPC.hide = false;
                    NPC.immortal = false;
                    NPC.dontTakeDamage = false;
                    NPC.ai[1] = 0;
                    NPC.ai[3] = 0;
                }
                if (NPC.localAI[0] < -30)
                {
                    NPC.velocity *= 0.99f;
                    NPC.ai[1]++;
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

            if (NPC.ai[0] == Charge.Id)
            {
                int chargeProgress = (int)NPC.ai[1] % (chargeTelegraph + chargingDuration);
                if (chargeProgress == 0)
                {
                    ChooseChargeLocation();
                }
                if (chargeProgress <= 3)
                {
                    Vector2 targetPos = chargeDesiredPos;
                    bool close = (NPC.Center - targetPos).Length() <= 160;
                    float rotToTarget = (targetPos - NPC.Center).ToRotation();

                    float potentialRot = NPC.velocity.ToRotation().AngleTowards(rotToTarget, close ? 0.2f : defaultMaxRotation * (NPC.velocity.Length() / 10f));

                    if (close)
                    {
                        if (NPC.velocity.Length() > defaultMinVelocity * 0.5f)
                        {
                            NPC.velocity -= NPC.velocity.SafeNormalize(Vector2.UnitY) * defaultAcceleration * 6;
                            if (NPC.velocity.Length() < defaultMinVelocity * 0.5f)
                                NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * defaultMinVelocity * 0.5f;
                        }
                    }
                    else if (NPC.velocity.Length() < hastyMaxVelocity * 2)
                    {
                        NPC.velocity += NPC.velocity.SafeNormalize(Vector2.UnitY) * defaultAcceleration * 2;
                        if (NPC.velocity.Length() > hastyMaxVelocity * 2)
                            NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * hastyMaxVelocity * 2;
                    }

                    NPC.velocity = (Vector2.UnitX * NPC.velocity.Length()).RotatedBy(potentialRot);

                    if (chargeProgress == 3)
                    {
                        if ((NPC.Center - chargeDesiredPos).Length() > 70)
                        {
                            NPC.ai[1]--;
                            chargeProgress--;
                        }
                    }
                }
                if (chargeProgress >= 3)
                {
                    Vector2 targetPos = target == null ? spawnPos : target.Center;

                    if (chargeProgress == 3)
                    {
                        SoundEngine.PlaySound(SoundID.Zombie38 with { Volume = 0.26f, Pitch = -1f, PitchVariance = 0.11f, MaxInstances = 3 }, NPC.Center);
                        SoundEngine.PlaySound(SoundID.NPCDeath33 with { Volume = 0.35f, Pitch = -0.5f, PitchVariance = 0f, MaxInstances = 3 }, NPC.Center);
                    }

                    if (chargeProgress < chargeTelegraph)
                    {
                        float rotToTarget = (targetPos - NPC.Center).ToRotation();
                        float potentialRot = NPC.velocity.ToRotation().AngleTowards(rotToTarget, 0.3f);
                        NPC.velocity = (Vector2.UnitX * NPC.velocity.Length()).RotatedBy(potentialRot) * 0.95f;
                    }
                    else
                    {
                        float completion = 1f - ((float)(chargeProgress - chargeTelegraph) / (chargingDuration));
                        NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * chargeSpeed * (completion * 0.75f + 0.25f);
                    }
                }

                if (NPC.ai[1] >= Charge.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Charge.Id;
                }
            }
            else if (NPC.ai[0] == WaveTunnel.Id)
            {
                Room room = RoomList[modNPC.sourceRoomListID];
                if (NPC.ai[1] == 0)
                {
                    Vector2 targetPos = target == null ? NPC.Center : new Vector2(target.Center.X > NPC.Center.X ? room.RoomPosition16.X : room.RoomPosition16.X + room.RoomDimensions16.X, target.Center.Y);
                    float rotToRoom = (targetPos - (room.RoomPosition16 + room.RoomCenter16)).ToRotation();
                    int quadrant = (int)(rotToRoom / MathHelper.PiOver2);
                    if (rotToRoom < 0)
                        quadrant--;

                    switch (quadrant)
                    {
                        case -2:
                            waveTunnelDesiredPos = room.RoomPosition16 + room.PercentPosition(0, 0) + new Vector2(-waveTunnelCornerPosOffset, -1);
                            break;
                        case -1:
                            waveTunnelDesiredPos = room.RoomPosition16 + room.PercentPosition(1, 0) + new Vector2(waveTunnelCornerPosOffset, -1);
                            break;
                        case 0:
                            waveTunnelDesiredPos = room.RoomPosition16 + room.PercentPosition(1, 1) + new Vector2(waveTunnelCornerPosOffset, 1);
                            break;
                        case 1:
                            waveTunnelDesiredPos = room.RoomPosition16 + room.PercentPosition(0, 1) + new Vector2(-waveTunnelCornerPosOffset, 1);
                            break;
                        default:
                            waveTunnelDesiredPos = Vector2.Zero;
                            break;
                    }
                }
                if (NPC.ai[1] <= waveTunnelTelegraphBurrowTime)
                {
                    Vector2 targetPos = waveTunnelDesiredPos + new Vector2(0, Math.Sign(waveTunnelDesiredPos.Y - room.RoomPosition16.Y) * 240);
                    float rotToTarget = (targetPos - NPC.Center).ToRotation();
                    bool close = (NPC.Center - targetPos).Length() <= 160;

                    float potentialRot = NPC.velocity.ToRotation().AngleTowards(rotToTarget, close ? 0.2f : defaultMaxRotation * (NPC.velocity.Length() / 10f));

                    if (close)
                    {
                        if (NPC.velocity.Length() > defaultMinVelocity * 0.5f)
                        {
                            NPC.velocity -= NPC.velocity.SafeNormalize(Vector2.UnitY) * defaultAcceleration * 6;
                            if (NPC.velocity.Length() < defaultMinVelocity * 0.5f)
                                NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * defaultMinVelocity * 0.5f;
                        }
                    }
                    else if (NPC.velocity.Length() < hastyMaxVelocity * 2)
                    {
                        NPC.velocity += NPC.velocity.SafeNormalize(Vector2.UnitY) * defaultAcceleration * 2;
                        if (NPC.velocity.Length() > hastyMaxVelocity * 2)
                            NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * hastyMaxVelocity * 2;
                    }

                    NPC.velocity = (Vector2.UnitX * NPC.velocity.Length()).RotatedBy(potentialRot);

                    if (NPC.ai[1] == waveTunnelTelegraphBurrowTime)
                    {
                        if ((NPC.Center - targetPos).Length() > 35)
                        {
                            NPC.ai[1]--;
                        }
                    }
                }
                else if (NPC.ai[1] <= waveTunnelTelegraph)
                {
                    Vector2 targetPos = waveTunnelDesiredPos;
                    float rotToTarget = (targetPos - NPC.Center).ToRotation();
                    bool close = true;

                    float potentialRot = NPC.velocity.ToRotation().AngleTowards(rotToTarget, close ? 0.3f : defaultMaxRotation * (NPC.velocity.Length() / 10f));

                    if (NPC.velocity.Length() < hastyMaxVelocity * 2)
                    {
                        NPC.velocity += NPC.velocity.SafeNormalize(Vector2.UnitY) * defaultAcceleration * 2;
                        if (NPC.velocity.Length() > hastyMaxVelocity * 2)
                            NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * hastyMaxVelocity * 2;
                    }

                    NPC.velocity = (Vector2.UnitX * NPC.velocity.Length()).RotatedBy(potentialRot);

                    if (NPC.ai[1] == waveTunnelTelegraph)
                    {
                        if ((NPC.Center - waveTunnelDesiredPos).Length() > 24)
                        {
                            NPC.ai[1]--;
                        }
                    }
                }
                if (NPC.ai[1] >= waveTunnelTelegraph)
                {
                    segmentRotationInterpolant = 0.98f;
                    NPC.velocity = Vector2.Zero;
                    int waveTime = (int)NPC.ai[1] - waveTunnelTelegraph;
                    int waveDuration = WaveTunnel.Duration - waveTunnelTelegraph;
                    float waveCompletion = waveTime / (float)waveDuration;

                    int horizontalDirection = -Math.Sign((waveTunnelDesiredPos - (room.RoomPosition16 + room.RoomCenter16)).X);
                    int verticalDirection = -Math.Sign((waveTunnelDesiredPos - (room.RoomPosition16 + room.RoomCenter16)).Y);
                    float horizontalPosition = waveTunnelDesiredPos.X + (horizontalDirection * waveCompletion * (room.RoomDimensions16.X + (waveTunnelCornerPosOffset * 1)));
                    float sinCalc = (float)Math.Sin(MathHelper.Pi * 5 * waveCompletion);
                    float verticalPosition = waveTunnelDesiredPos.Y +  (verticalDirection * sinCalc * waveTunnelAmplitude);

                    NPC.Center = new Vector2(horizontalPosition, verticalPosition);
                    NPC.rotation = (NPC.position - NPC.oldPosition).ToRotation();

                    bool shoot = false;
                    if (NPC.ai[3] == -1 && sinCalc <= 0)
                        NPC.ai[3] = 0;
                    if (NPC.ai[3] == 0 && sinCalc > 0)
                        NPC.ai[3]++;
                    else if (NPC.ai[3] > 0)
                        NPC.ai[3]++;

                    if (NPC.ai[3] >= waveTunnelProjectileTelegraph)
                    {
                        shoot = true;
                        NPC.ai[3] = -1;
                    }
                    int shootingSegments = Math.Clamp((int)((modNPC.Segments.Count - 8) * waveCompletion), 0, modNPC.Segments.Count - 1);
                    if (shoot)
                    {
                        chargeSlot = SoundEngine.PlaySound(SoundID.DD2_WyvernHurt with { Volume = 0.3f, Pitch = -0.9f, PitchVariance = 0.05f }, NPC.Center);
                    }
                    ProjectileShootingLogic(0, shootingSegments, NPC.ai[3] > 0, shoot);
                }

                if (NPC.ai[1] >= WaveTunnel.Duration)
                {
                    segmentRotationInterpolant = setSegmentRotationInterpolant;
                    NPC.velocity = NPC.position - NPC.oldPosition;
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = WaveTunnel.Id;
                    NPC.ai[3] = 0;
                }
            }
            else if (NPC.ai[0] == Vomit.Id)
            {
                Vector2 targetPos = target == null ? NPC.Center - NPC.velocity : target.Center;
                float rotToTarget = (targetPos - NPC.Center).ToRotation();
                float angleBetween = Math.Abs(AngleSizeBetween(NPC.rotation, rotToTarget));
                if (angleBetween < defaultLookingAtThreshold * 2)
                    defaultLookingAtBuffer = 10;
                else if (angleBetween > MathHelper.PiOver2 && targetPos.Distance(NPC.Center) < 48f)
                    defaultLookingAtBuffer = 30;
                bool lookingAtTarget = defaultLookingAtBuffer > 0;
                float potentialRot = NPC.velocity.ToRotation().AngleTowards(rotToTarget, lookingAtTarget ? defaultMinRotation * 0.75f : defaultMaxRotation);

                Point tilePos = NPC.Center.ToTileCoordinates();
                if (ParanoidTileRetrieval(tilePos.X, tilePos.Y).IsTileSolidGround(true) || NPC.Center.Distance(targetPos) > 270 || (lookingAtTarget && target != null && !CanHitInLine(NPC.Center, target.Center)))
                {
                    if (NPC.ai[1] < Vomit.Duration - 31)
                        NPC.ai[1] -= 0.5f;

                    if (NPC.velocity.Length() < defaultMaxVelocity)
                        NPC.velocity += NPC.velocity.SafeNormalize(Vector2.UnitY) * defaultAcceleration * 0.5f;
                    if (NPC.velocity.Length() > defaultMaxVelocity)
                        NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * defaultMaxVelocity;
                }
                else
                {
                    if (NPC.velocity.Length() > defaultMinVelocity)
                    {
                        NPC.velocity -= NPC.velocity.SafeNormalize(Vector2.UnitY) * defaultDecelertaion * 2;
                        if (NPC.velocity.Length() < defaultMinVelocity)
                            NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * defaultMinVelocity;
                    }
                    if (NPC.velocity.Length() > defaultMaxVelocity)
                        NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * defaultMaxVelocity;
                }
                

                NPC.velocity = (Vector2.UnitX * NPC.velocity.Length()).RotatedBy(potentialRot);

                if (NPC.ai[1] > 60 && NPC.ai[1] < Vomit.Duration - 30)
                {
                    Vector2 offset = Main.rand.NextVector2CircularEdge(20, 20);
                    Vector2 velocity = -offset.SafeNormalize(Vector2.UnitY) * 2;
                    velocity *= Main.rand.NextFloat(0.5f, 1f);

                    Vector2 scale = new Vector2(0.1f, 0.3f);
                    int time = 30 + Main.rand.Next(5);
                    Color color = Color.Lerp(Color.Lerp(Color.Green, Color.Yellow, Main.rand.NextFloat(0.7f)), Color.Black, 0.36f);
                    offset += (NPC.rotation.ToRotationVector2() * 24);
                    ParticleManager.AddParticle(new Spark(NPC.Center + NPC.velocity + offset, velocity + NPC.velocity, time, Color.Black, scale, velocity.ToRotation(), false, SpriteEffects.None, true, false));
                    ParticleManager.AddParticle(new Spark(NPC.Center + NPC.velocity + offset, velocity + NPC.velocity, time, color, scale, velocity.ToRotation(), true, SpriteEffects.None, true, false));
                }
                if ((int)NPC.ai[1] == Vomit.Duration - 30)
                {
                    SoundEngine.PlaySound(SoundID.NPCDeath13 with { Volume = 0.8f, Pitch = -0.1f }, NPC.Center);
                    for (int i = -6; i <= 6; i++)
                    {
                        float randRot = Main.rand.NextFloat(-0.15f, 0.15f) + NPC.rotation;
                        randRot += 0.075f * i;
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + (NPC.rotation.ToRotationVector2() * 36), (randRot.ToRotationVector2() * Main.rand.NextFloat(4.8f, 6f)) + (NPC.velocity * 0.25f) - Vector2.UnitY, ModContent.ProjectileType<CorruptVomit>(), NPC.damage, 0);
                    }
                } 
                if (NPC.ai[1] >= Vomit.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = None.Duration - 120;
                    NPC.ai[2] = Vomit.Id;
                }
            }
            else if (NPC.ai[0] == ProjCharge.Id)
            {
                if (NPC.ai[1] == 0)
                {
                    ChooseChargeLocation();
                }
                if (NPC.ai[1] <= 3)
                {
                    Vector2 chargePos = chargeDesiredPos;
                    float rotToChargePos = (chargePos - NPC.Center).ToRotation();

                    float potentialRot = NPC.velocity.ToRotation().AngleTowards(rotToChargePos, defaultMaxRotation * (NPC.velocity.Length() / 10f));

                    if (NPC.velocity.Length() < hastyMaxVelocity * 2)
                    {
                        NPC.velocity += NPC.velocity.SafeNormalize(Vector2.UnitY) * defaultAcceleration * 2;
                        if (NPC.velocity.Length() > hastyMaxVelocity * 2)
                            NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * hastyMaxVelocity * 2;
                    }

                    NPC.velocity = (Vector2.UnitX * NPC.velocity.Length()).RotatedBy(potentialRot);

                    if (NPC.ai[1] == 3)
                    {
                        if ((NPC.Center - chargeDesiredPos).Length() > 70)
                        {
                            NPC.ai[1]--;
                        }
                    }
                }
                Vector2 targetPos = target == null ? spawnPos : target.Center;
                float rotToTarget = (targetPos - NPC.Center).ToRotation();
                if (NPC.ai[1] >= 3 && NPC.ai[1] < projChargeTelegraph + projChargingDuration)
                {

                    if (NPC.ai[1] == 3)
                    {
                        SoundEngine.PlaySound(SoundID.Zombie38 with { Volume = 0.26f, Pitch = -1f, PitchVariance = 0.11f, MaxInstances = 3 }, NPC.Center);
                        chargeSlot = SoundEngine.PlaySound(SoundID.DD2_BetsyScream with { Volume = 0.4f, Pitch = -0.6f, PitchVariance = 0f}, NPC.Center);
                    }

                    if (NPC.ai[1] < projChargeTelegraph)
                    {
                        float potentialVelToRot = NPC.velocity.ToRotation();

                        float potentialRot = potentialVelToRot.AngleTowards(rotToTarget, 0.3f);
                        NPC.velocity = (Vector2.UnitX * NPC.velocity.Length()).RotatedBy(potentialRot) * 0.95f;
                    }
                    else
                    {
                        float completion = 1f - ((float)(NPC.ai[1] - projChargeTelegraph) / (projChargingDuration));
                        NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * projChargeSpeed * (completion * 0.75f + 0.25f);
                    }
                }
                else if (NPC.ai[1] >= projChargeTelegraph + projChargingDuration)
                {
                    NPC.ai[3]++;
                    NPC.velocity = NPC.velocity.ToRotation().AngleTowards(rotToTarget, 0.04f * (NPC.ai[3] / projChargeShootTelegraph)).ToRotationVector2() * NPC.velocity.Length();
                    bool telegraph = NPC.ai[1] < projChargeTelegraph + projChargingDuration + projChargeShootTelegraph;
                    bool shoot = NPC.ai[1] == projChargeTelegraph + projChargingDuration + projChargeShootTelegraph;
                    ProjectileShootingLogic(0, modNPC.Segments.Count - 1, telegraph, shoot);
                }
                if (NPC.ai[1] >= ProjCharge.Duration)
{
                    NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * defaultMinVelocity;
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = ProjCharge.Id;
                    NPC.ai[3] = 0;
                }
            }
            else if (NPC.ai[0] == Summon.Id)
            {
                int summonTime = Summon.Duration / summonCount;
                if ((int)NPC.ai[1] % summonTime >= summonTime - 15)
                {
                    WormSegment tailSegment = modNPC.Segments[modNPC.Segments.Count - 1];
                    Vector2 tailPos = tailSegment.Position;
                    if (ParanoidTileRetrieval(tailPos.ToTileCoordinates()).IsTileSolidGround(true))
                    {
                        NPC.ai[1] = (((int)NPC.ai[1] / summonTime) * summonTime) + summonTime - 15;
                    }
                    else if ((int)NPC.ai[1] % summonTime == summonTime - 1)
                    {
                        SoundEngine.PlaySound(SoundID.Item17 with { Volume = 1f }, tailPos);
                        int whoAmI = NPC.NewNPC(NPC.GetSource_FromThis(), (int)tailPos.X, (int)tailPos.Y, ModContent.NPCType<ParasiticEgg>());
                        NPC npc = Main.npc[whoAmI];
                        npc.ModNPC().isRoomNPC = modNPC.isRoomNPC;
                        npc.ModNPC().sourceRoomListID = modNPC.sourceRoomListID;
                        npc.velocity = -tailSegment.Rotation.ToRotationVector2() * 7f;
                        npc.rotation = tailSegment.Rotation - MathHelper.PiOver2;
                        npc.Center = tailPos + (-tailSegment.Rotation.ToRotationVector2() * 34);
                    }
                }
                if (NPC.ai[1] >= Summon.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = None.Duration - 30;
                    NPC.ai[2] = Summon.Id;
                }
            }

            defaultLookingAtThreshold = 0.1f;
            bool defaultMovement = NPC.ai[0] == None.Id || NPC.ai[0] == Summon.Id;
            if (defaultLookingAtBuffer > 0)
                defaultLookingAtBuffer--;

            if (defaultMovement)
            {
                Vector2 targetPos = target == null ? NPC.Center - NPC.velocity : target.Center;
                float rotToTarget = (targetPos - NPC.Center).ToRotation();
                float angleBetween = Math.Abs(AngleSizeBetween(NPC.rotation, rotToTarget));
                if (angleBetween < defaultLookingAtThreshold)
                    defaultLookingAtBuffer = 10;
                else if (angleBetween > MathHelper.PiOver2 && targetPos.Distance(NPC.Center) < 48f)
                    defaultLookingAtBuffer = 30;
                bool lookingAtTarget = defaultLookingAtBuffer > 0;
                float potentialRot = NPC.velocity.ToRotation().AngleTowards(rotToTarget, lookingAtTarget ? defaultMinRotation : defaultMaxRotation);

                if (lookingAtTarget)
                    NPC.velocity += NPC.velocity.SafeNormalize(Vector2.UnitY) * defaultAcceleration;
                else if (NPC.velocity.Length() > defaultMinVelocity)
                {
                    NPC.velocity -= NPC.velocity.SafeNormalize(Vector2.UnitY) * defaultDecelertaion;
                    if (NPC.velocity.Length() < defaultMinVelocity)
                        NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * defaultMinVelocity;
                }
                if (NPC.velocity.Length() > defaultMaxVelocity)
                    NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * defaultMaxVelocity;

                NPC.velocity = (Vector2.UnitX * NPC.velocity.Length()).RotatedBy(potentialRot);
            }
        }

        public void ChooseAttack()
        {
            NPC.ai[1] = 0;
            int chosenAttack = 0;

            List<Attack> potentialAttacks = new List<Attack>() { Charge, WaveTunnel, Vomit, ProjCharge, Summon };
            potentialAttacks.RemoveAll(x => x.Id == (int)NPC.ai[2]);
            if (!modNPC.isRoomNPC)
                potentialAttacks.RemoveAll(x => x.Id == WaveTunnel.Id);

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
        public void ChooseChargeLocation()
        {
            if (target != null)
                chargeDesiredPos = target.Center;
            else
                chargeDesiredPos = spawnPos;

            float rot = (NPC.Center - chargeDesiredPos).ToRotation();

            rot += Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4 + float.Epsilon);
            chargeDesiredPos += rot.ToRotationVector2() * chargeDesiredDist;
        }
        public void ProjectileShootingLogic(int startSegment, int endSegment, bool telegraphActive, bool shoot)
        {
            for (int i = startSegment; i <= endSegment; i++)
            {
                WormSegment segment = modNPC.Segments[i];
                Point tilePos = segment.Position.ToTileCoordinates();
                if (ParanoidTileRetrieval(tilePos.X, tilePos.Y).IsTileSolidGround(true))
                    continue; 

                float opacity = NPC.ai[3] / waveTunnelProjectileTelegraph;
                if (telegraphActive)
                {
                    Color color = Color.Lerp(Color.LimeGreen, Color.Yellow, Main.rand.NextFloat(0.75f));
                    ParticleManager.AddParticle(new Square(segment.Position, Vector2.Zero, 4, color * opacity, new Vector2(2f, 0.8f), segment.Rotation, 0.96f, 4, false));
                    Vector2 randomVect = Main.rand.NextVector2CircularEdge(16f, 16f);
                    ParticleManager.AddParticle(new Square(segment.Position + randomVect, -randomVect * 0.07f, 10, color * ((0.25f * opacity) + 0.75f), new Vector2(0.65f), 0, 0.96f, 10, false));
                }
                if (shoot)
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), segment.Position, (target == null ? Vector2.UnitY : (target.Center - segment.Position)).SafeNormalize(Vector2.UnitY) * 8, ModContent.ProjectileType<CursedFlame>(), NPC.damage, 0);
            }
        }
        public override bool? CanFallThroughPlatforms()
        {
            return true;
        }
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            for (int i = 0; i < modNPC.Segments.Count; i++)
            {
                WormSegment segment = modNPC.Segments[i];
                float radius = i == 0 ? (NPC.height < NPC.width ? NPC.height / 2 : NPC.width / 2) : segment.Height / 2;
                if (segment.Position.Distance(target.getRect().ClosestPointInRect(segment.Position)) <= radius)
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
            for (int i = 0; i < modNPC.Segments.Count; i++)
            {
                WormSegment segment = modNPC.Segments[i];
                float radius = i == 0 ? (NPC.height < NPC.width ? NPC.height / 2 : NPC.width / 2) : segment.Height / 2;
                if (segment.Position.Distance(target.getRect().ClosestPointInRect(segment.Position)) <= radius)
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

            for (int i = 0; i < modNPC.Segments.Count; i++)
            {
                WormSegment segment = modNPC.Segments[i];
                bool pass = projectile.Colliding(projectile.getRect(), new Rectangle((int)(segment.Position.X - ((i == 0 ? NPC.width : segment.Height) / 2)), (int)(segment.Position.Y - ((i == 0 ? NPC.height : segment.Height) / 2)), NPC.width, NPC.height));
                if (pass)
                {
                    projectile.ModProj().ultimateCollideOverride = true;
                    modNPC.hitSegment = i;
                    return null;
                }
            }
            return false;
        }

        public override bool CheckDead()
        {
            segmentRotationInterpolant = 0.975f;
            if (deadTime >= deathCutsceneDuration - 30)
            {
                return true;
            }
            if (NPC.ai[0] == WaveTunnel.Id && NPC.ai[1] >= waveTunnelTelegraph)
            {
                NPC.velocity = NPC.position - NPC.oldPos[1];
            }

            NPC.ai[0] = None.Id;
            NPC.ai[1] = 1;

            modNPC.OverrideIgniteVisual = true;
            NPC.velocity *= 0.975f;
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

                    Rectangle roomRect = room.GetRect();
                    if (!roomRect.Contains(NPC.Center.ToPoint()))
                    {
                        float oobDistance = (roomRect.ClosestPointInRect(NPC.Center) - NPC.Center).Length();
                        float speed = MathHelper.Clamp(oobDistance * 0.05f, 5, 20);
                        NPC.velocity = (room.RoomCenter16 + room.RoomPosition16 - NPC.Center).SafeNormalize(Vector2.UnitY) * speed;
                    }
                }
                CutsceneSystem.SetCutscene(NPC.Center, deathCutsceneDuration, 30, 30, 2.5f);
            }
            deadTime++;

            if (target != null)
            {
                NPC.velocity = NPC.velocity.ToRotation().AngleLerp(((modNPC.isRoomNPC ? RoomList[modNPC.sourceRoomListID].RoomPosition16 + RoomList[modNPC.sourceRoomListID].RoomCenter16 : target.Center) - NPC.Center).ToRotation(), 0.0078f).ToRotationVector2() * NPC.velocity.Length();
            }

            if (deadTime < deathCutsceneDuration - 60)
            {
                for (int i = 0; i < modNPC.Segments.Count; i++)
                {
                    if (Main.rand.NextBool(40) || i == 0 && deadTime % 50 == 0)
                    {
                        deathGodRays.Add(new GodRay(Main.rand.NextFloat(MathHelper.TwoPi), deadTime, new Vector2(0.14f + Main.rand.NextFloat(-0.02f, 0.02f), 0.018f) * 1f, i));
                        SoundEngine.PlaySound(SoundID.NPCHit19 with { Volume = 0.4f, Pitch = -0.6f, PitchVariance = 0.05f, MaxInstances = 3, SoundLimitBehavior = SoundLimitBehavior.IgnoreNew }, modNPC.Segments[i].Position);
                        break;
                    }
                    i += Main.rand.Next(4);
                }
            }
            if (deadTime == deathCutsceneDuration - 90)
            {
                chargeSlot = SoundEngine.PlaySound(SoundID.DD2_WyvernHurt with { Volume = 0.6f, Pitch = -0.9f, PitchVariance = 0.05f }, NPC.Center);
            }
            

            if (deadTime % 5 == 0)
            {
                for (int i = 0; i < deathGodRays.Count; i++)
                {
                    GodRay ray = deathGodRays[i];
                    WormSegment segment = modNPC.Segments[ray.segment];
                    float rotation = ray.rotation;
                    Vector2 pos = modNPC.Segments[ray.segment].Position + (rotation.ToRotationVector2() * new Vector2(NPC.width * 0.55f, NPC.height * 0.7f));
                    Vector2 velocity = rotation.ToRotationVector2();
                    int xDir = Math.Sign(pos.X - modNPC.Segments[ray.segment].Position.X);
                    if (xDir == 0)
                        xDir = 1;
                    velocity.X += xDir * 0.6f;
                    velocity.Y -= 0.5f;
                    int time = 40 + Main.rand.Next(20);
                    Vector2 scale = new Vector2(0.25f, 0.4f) * 0.75f;
                    Color color = Color.Lerp(Color.Lerp(Color.Green, Color.Yellow, Main.rand.NextFloat(0.7f)), Color.Black, 0.48f);

                    Vector2 segVel = segment.Position - segment.OldPosition; 
                    ParticleManager.AddParticle(new Blood(pos, velocity + (segVel), time, Color.Black * 0.65f, scale, velocity.ToRotation(), false));
                    ParticleManager.AddParticle(new Blood(pos, velocity + (segVel), time, color * 0.65f, scale, velocity.ToRotation(), true));
                }
            }
            if (CutsceneSystem.cutsceneActive)
            {
                CutsceneSystem.cameraTargetCenter += (NPC.Center - CutsceneSystem.cameraTargetCenter) * 0.1f;
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
            WormSegment segment = modNPC.Segments[modNPC.hitSegment];
            SoundEngine.PlaySound(SoundID.NPCHit1, segment.Position);
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / 25.0; i++)
                {
                    Dust.NewDust(segment.Position + new Vector2(-segment.Height * 0.5f), (int)segment.Height, (int)segment.Height, 5, hit.HitDirection, -1f);
                }
            }
            else
            {
                
            }
        }
        public override void OnKill()
        {
            SoundEngine.PlaySound(SoundID.DD2_KoboldIgnite with { Volume = 0.5f, Pitch = -0.4f }, NPC.Center);
            SoundEngine.PlaySound(SoundID.NPCDeath12 with { Volume = 0.8f, Pitch = -0.5f }, NPC.Center);
            for (int s = 0; s < modNPC.Segments.Count; s++)
            {
                WormSegment segment = modNPC.Segments[s];
                for (int i = 0; i < 10; i++)
                {
                    Vector2 pos = segment.Position + new Vector2(0, 16);
                    int width = (int)(NPC.width * 0.25f);
                    pos.X += Main.rand.Next(-width, width);
                    Vector2 velocity = new Vector2(0, -4f).RotatedBy(Main.rand.NextFloat(-MathHelper.PiOver4 * 1.5f, MathHelper.PiOver4 * 1.5f));
                    velocity *= Main.rand.NextFloat(0.3f, 1f);
                    if (Main.rand.NextBool(5))
                        velocity *= 1.5f;
                    Vector2 scale = new Vector2(0.25f, 0.4f) * 0.86f;
                    int time = 110 + Main.rand.Next(70);
                    Color color = Color.Lerp(Color.Lerp(Color.Green, Color.Yellow, Main.rand.NextFloat(0.7f)), Color.Black, 0.48f);
                    ParticleManager.AddParticle(new Blood(pos, velocity, time, Color.Black * 0.65f, scale, velocity.ToRotation(), false));
                    ParticleManager.AddParticle(new Blood(pos, velocity, time, color * 0.65f, scale, velocity.ToRotation(), true));
                }
                if (s == 0)
                {
                    Gore.NewGore(NPC.GetSource_Death(), segment.Position + new Vector2(-segment.Height * 0.5f), Main.rand.NextVector2Circular(4, 4), 24);
                    Gore.NewGore(NPC.GetSource_Death(), segment.Position + new Vector2(-segment.Height * 0.5f), Main.rand.NextVector2Circular(4, 4), 25);
                }
                else if (s == modNPC.Segments.Count - 1)
                {
                    Gore.NewGore(NPC.GetSource_Death(), segment.Position + new Vector2(-segment.Height * 0.5f), Main.rand.NextVector2Circular(4, 4), 28);
                    Gore.NewGore(NPC.GetSource_Death(), segment.Position + new Vector2(-segment.Height * 0.5f), Main.rand.NextVector2Circular(4, 4), 29);
                }
                else
                {
                    Gore.NewGore(NPC.GetSource_Death(), segment.Position + new Vector2(-segment.Height * 0.5f), Main.rand.NextVector2Circular(4, 4), 26);
                    Gore.NewGore(NPC.GetSource_Death(), segment.Position + new Vector2(-segment.Height * 0.5f), Main.rand.NextVector2Circular(4, 4), 27);
                }
            }
        }
        public override void FindFrame(int frameHeight)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;
            currentFrame = 0;
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, tex.Width, frameHeight);

            int headFrameCounter = 0;

            int chargeProgress = (int)NPC.ai[1] % (chargeTelegraph + chargingDuration);
            bool open =
                (NPC.localAI[0] < 0 && NPC.localAI[0] >= -50) ||
                (deadTime > deathCutsceneDuration - 90) ||
                (NPC.ai[0] == Charge.Id && chargeProgress >= 3 && chargeProgress < chargeTelegraph + chargingDuration - 20) ||
                (NPC.ai[0] == ProjCharge.Id && NPC.ai[1] >= 3 && NPC.ai[1] < projChargeTelegraph + projChargingDuration - 20) ||
                (NPC.ai[0] == Vomit.Id && NPC.ai[1] >= Vomit.Duration - 30);

            if (open)
                headFrameCounter = 1;

            int headFrameHeight = headTex.Height / 2;
            headFrame = new Rectangle(0, headFrameHeight * headFrameCounter, headTex.Width, headFrameHeight);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (deadTime > 0)
            {
                if (deathGodRays.Count > 0)
                {
                    StartAdditiveSpritebatch();
                    for (int i = 0; i < deathGodRays.Count; i++)
                    {
                        GodRay ray = deathGodRays[i];
                        float rotation = ray.rotation;
                        Vector2 rayScale = ray.scale;
                        int time = ray.time;
                        float opacity = MathHelper.Clamp(MathHelper.Lerp(1f, 0.5f, (deadTime - time) / 60f), 0.5f, 1f) * 0.9f;
                        Main.EntitySpriteDraw(godRayTex, modNPC.Segments[ray.segment].Position - Main.screenPosition, null, Color.DarkGreen * opacity, rotation, new Vector2(0, godRayTex.Height * 0.5f), rayScale, SpriteEffects.None);
                    }
                    StartVanillaSpritebatch();
                }
            }

            if (NPC.ai[0] == Summon.Id)
            {
                int summonTime = Summon.Duration / summonCount;
                if ((int)NPC.ai[1] % summonTime < summonTime - 1)
                {
                    WormSegment segment = modNPC.Segments[modNPC.Segments.Count - 1];
                    float interpolant = MathHelper.SmoothStep(0, 1, Math.Clamp((NPC.ai[1] % summonTime) / 45f, 0, 1));
                    Vector2 drawPos = segment.Position;
                    Vector2 offset = -segment.Rotation.ToRotationVector2() * ((interpolant * 34));
                    Main.EntitySpriteDraw(eggTex, drawPos + offset - Main.screenPosition, null, Lighting.GetColor(drawPos.ToTileCoordinates()), segment.Rotation - MathHelper.PiOver2, eggTex.Size() * 0.5f, NPC.scale, SpriteEffects.None);
                }
            }

            if (modNPC.ignitedStacks.Count > 0 && deadTime <= 0)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                for (int i = modNPC.Segments.Count - 1; i >= 0; i--)
                {
                    WormSegment segment = modNPC.Segments[i];
                    Color blockColor = Lighting.GetColor(segment.Position.ToTileCoordinates());
                    if (blockColor.R <= 30 && blockColor.G <= 30 && blockColor.B <= 30)
                        continue;

                    Texture2D texture;
                    if (i == 0)
                        texture = headTex;
                    else if (i == modNPC.Segments.Count - 1)
                        texture = tailTex;
                    else
                        texture = bodyTex;

                    Color color = Color.Lerp(Color.Yellow, Color.OrangeRed, Main.rand.NextFloat(0.4f, 0.6f + float.Epsilon) + 0.2f + (0.2f * (float)Math.Cos((Main.GlobalTimeWrappedHourly * 20f)))) * 0.8f;
                    Vector3 colorHSL = Main.rgbToHsl(color);
                    float outlineThickness = 1f;
                    SpriteEffects spriteEffects = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

                    GameShaders.Misc["TerRoguelike:BasicTint"].UseOpacity(1f);
                    GameShaders.Misc["TerRoguelike:BasicTint"].UseColor(Main.hslToRgb(1 - colorHSL.X, colorHSL.Y, colorHSL.Z));
                    GameShaders.Misc["TerRoguelike:BasicTint"].Apply();

                    Vector2 position = segment.Position + (Vector2.UnitY * NPC.gfxOffY);
                    for (float j = 0; j < 1; j += 0.125f)
                    {
                        spriteBatch.Draw(texture, position + (j * MathHelper.TwoPi + segment.Rotation + MathHelper.PiOver2).ToRotationVector2() * outlineThickness - Main.screenPosition, i == 0 ? headFrame : null, color, segment.Rotation + MathHelper.PiOver2, texture.Size() * new Vector2(0.5f, i == 0 ? 0.25f : 0.5f), NPC.scale, spriteEffects, 0f);
                    }
                }
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            for (int i = modNPC.Segments.Count - 1; i >= 0; i--)
            {
                WormSegment segment = modNPC.Segments[i];
                Color blockColor = Lighting.GetColor(segment.Position.ToTileCoordinates());
                if (blockColor.R <= 30 && blockColor.G <= 30 && blockColor.B <= 30)
                    continue;

                Texture2D texture;
                if (i == 0)
                    texture = headTex;
                else if (i == modNPC.Segments.Count - 1)
                    texture = tailTex;
                else
                    texture = bodyTex;

                Color color = modNPC.ignitedStacks.Count > 0 ? Color.Lerp(Color.White, Color.OrangeRed, 0.4f) : blockColor;
                spriteBatch.Draw(texture, segment.Position - screenPos, i == 0 ? headFrame : null, color, segment.Rotation + MathHelper.PiOver2, texture.Size() * new Vector2(0.5f, i == 0 ? 0.25f : 0.5f), 1f, SpriteEffects.None, 0);
            }
            return false;
        }
    }
}
