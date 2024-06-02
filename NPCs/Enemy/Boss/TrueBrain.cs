using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Renderers;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using TerRoguelike.Managers;
using TerRoguelike.Particles;
using TerRoguelike.Projectiles;
using TerRoguelike.Systems;
using TerRoguelike.Utilities;
using TerRoguelike.World;
using static TerRoguelike.Managers.TextureManager;
using static TerRoguelike.Schematics.SchematicManager;
using static TerRoguelike.Systems.MusicSystem;
using static TerRoguelike.Systems.RoomSystem;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using static TerRoguelike.Systems.EnemyHealthBarSystem;
using Terraria.GameContent.Shaders;
using Terraria.Graphics.Effects;

namespace TerRoguelike.NPCs.Enemy.Boss
{
    public class TrueBrain : BaseRoguelikeNPC
    {
        public Entity target;
        public Vector2 spawnPos;
        public bool ableToHit = true;
        public bool canBeHit = true;
        public override int modNPCID => ModContent.NPCType<TrueBrain>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Lunar"] };
        public override int CombatStyle => -1;
        public int currentFrame = 0;
        public SlotId TeleportSlot;
        public SlotId ChargeSlot;
        public Texture2D eyeTex, innerEyeTex;
        public Vector2 eyeVector = Vector2.Zero;
        public Vector2 eyePosition { get { return new Vector2(0, -18) + modNPC.drawCenter; } }
        public Vector2 innerEyePosition { get { return new Vector2(0, -20) + modNPC.drawCenter; } }

        public int cutsceneDuration = 930;
        public int cutsceneLookingDownTime = 240;
        public int cutsceneLookOverTime = 270;
        public int cutsceneLookRoarTime = 280;
        public int cutsceneLookLeaveTime = 300;
        public Vector2 cutsceneTeleportPos1 = new Vector2(-1800, -120);
        public Vector2 cutsceneTeleportPos2 = new Vector2(1800, -120);
        public Vector2 cutsceneTeleportPos3 = new Vector2(-2000, -1000);
        public int cutsceneSideSweepTime = 120;
        public int cutsceneTopSweepTime = 260;
        public Vector2 cutsceneEyeVector = Vector2.Zero;

        public int deadTime = 0;
        public int deathCutsceneDuration = 120;

        public static int teleportTime = 40;
        public static int teleportMoveTimestamp = 20;
        public Vector2 teleportTargetPos = new Vector2(-1);
        public List<Vector2> phantomPositions = [];

        public int teleportAttackSetCooldown = 600;
        public int teleportAttackCooldown = 0;

        public static Attack None = new Attack(0, 0, 90);
        public static Attack TeleportBolt = new Attack(1, 30, 340);
        public static Attack ProjCharge = new Attack(2, 30, 280);
        public static Attack FakeCharge = new Attack(3, 30, 210);
        public static Attack CrossBeam = new Attack(4, 30, 180);
        public static Attack SpinBeam = new Attack(5, 30, 400);
        public static Attack Teleport = new Attack(6, 30, 115);
        public static Attack Summon = new Attack(7, 18, 150);
        public int TeleportBoltCycleTime = 90;
        public int TeleportBoltTelegraph = 20;
        public int TeleportBoltFireRate = 8;
        public int ProjChargeCycleTime = 100;
        public int ProjChargeTelegraph = 20;
        public int ProjChargeFireRate = 5;
        public int FakeChargeWindup = 90;
        public int FakeChargeTelegraph = 30;
        public int FakeChargeDashDuration = 60;
        public int FakeChargeFireRate = 3;
        public int CrossBeamCycleTime = 30;
        public int SpinBeamWindup = 110;
        public int SpinBeamFireDuration = 240;
        public int[] SpinBeamTrueEyeTimes = [100, 180];
        public int SummonRate = 12;
        public int SummonTime = 47;
        public int SummonWindup = 40;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 5;
            NPCID.Sets.MustAlwaysDraw[Type] = true;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 240;
            NPC.height = 150;
            NPC.aiStyle = -1;
            NPC.damage = 36;
            NPC.lifeMax = 60000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.knockBackResist = 0f;
            modNPC.IgnoreRoomWallCollision = true;
            modNPC.OverrideIgniteVisual = true;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            modNPC.AdaptiveArmorEnabled = true;
            modNPC.AdaptiveArmorAddRate = 50;
            modNPC.AdaptiveArmorDecayRate = 40;
            innerEyeTex = TexDict["MoonLordInnerEye"];
            eyeTex = TexDict["TrueBrainEye"];
            modNPC.drawCenter = new Vector2(0, 32);
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.immortal = true;
            NPC.dontTakeDamage = true;
            NPC.localAI[0] = -(cutsceneDuration + 30);
            NPC.direction = -1;
            NPC.spriteDirection = -1;
            spawnPos = NPC.Center;
            NPC.Center += new Vector2(800, 160);
            NPC.ai[2] = None.Id;
            ableToHit = false;
        }
        public override void PostAI()
        {
            if (NPC.localAI[0] >= -(cutsceneDuration + 30))
            {
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player player = Main.player[i];
                    if (!player.active)
                        continue;
                    var modPlayer = player.ModPlayer();
                    if (modPlayer == null)
                        continue;

                    modPlayer.moonLordVisualEffect = true;
                    modPlayer.moonLordSkyEffect = true;
                }
            }

            if (NPC.ai[3] > 0)
            {
                if (NPC.ai[3] == 5)
                {
                    TeleportSlot = SoundEngine.PlaySound(CrimsonVessel.TeleportSound with { Volume = 0.4f, MaxInstances = 2 }, NPC.Center);
                }
                else if (NPC.ai[3] == teleportMoveTimestamp)
                {
                    if (teleportTargetPos == new Vector2(-1))
                    {
                        Vector2 targetPos = target != null ? target.Center : spawnPos;
                        bool found = false;
                        for (int i = 0; i < 100; i++)
                        {
                            Vector2 wantedPos = targetPos + Main.rand.NextVector2CircularEdge(400, 340);
                            if (ParanoidTileRetrieval(wantedPos.ToTileCoordinates()).IsTileSolidGround(true))
                                continue;

                            if (modNPC.isRoomNPC)
                            {
                                if (!RoomList[modNPC.sourceRoomListID].GetRect().Contains(wantedPos.ToPoint()))
                                    continue;
                            }
                            if ((wantedPos - NPC.Center).Length() < 150f)
                                continue;

                            teleportTargetPos = wantedPos;
                            found = true;
                            break;
                        }
                        if (!found)
                        {
                            teleportTargetPos = targetPos + Main.rand.NextVector2CircularEdge(400, 340);
                        }
                    }

                    NPC.Center = teleportTargetPos;
                    if (SoundEngine.TryGetActiveSound(TeleportSlot, out var teleportSound) && teleportSound.IsPlaying)
                    {
                        teleportSound.Position = NPC.Center;
                        teleportSound.Update();
                    }
                }
                else if (NPC.ai[3] >= teleportTime)
                {
                    NPC.ai[3] = 0;
                    teleportTargetPos = new Vector2(-1);
                }
            }

            int cutsceneTime = (int)NPC.localAI[0] + cutsceneDuration;
            bool eyeCenter = NPC.ai[0] == SpinBeam.Id && NPC.ai[1] < SpinBeamWindup + SpinBeamFireDuration + 30;
            bool eyeCutscene = NPC.localAI[0] < -30 && (cutsceneTime < cutsceneLookingDownTime || (cutsceneTime > cutsceneLookLeaveTime && cutsceneTime < cutsceneLookLeaveTime + cutsceneSideSweepTime * 2 + cutsceneTopSweepTime));
            bool eyeVelocity = 
                (NPC.ai[0] == ProjCharge.Id && (int)NPC.ai[1] % ProjChargeCycleTime >= teleportMoveTimestamp + ProjChargeTelegraph) ||
                (NPC.ai[0] == FakeCharge.Id && NPC.ai[1] >= FakeChargeTelegraph + FakeChargeWindup);
            float rate = 0.15f;
            if (eyeCutscene)
            {
                float maxEyeOffset = 12;

                Vector2 targetVect = cutsceneEyeVector - (NPC.Center + innerEyePosition.RotatedBy(NPC.rotation));
                if (targetVect.Length() > maxEyeOffset)
                    targetVect = targetVect.SafeNormalize(Vector2.UnitY) * maxEyeOffset;
                eyeVector = Vector2.Lerp(eyeVector, targetVect, rate);
            }
            else if (eyeCenter)
            {
                eyeVector = Vector2.Lerp(eyeVector, Vector2.Zero, rate);
            }
            else
            {
                Vector2 targetPos = target != null ? target.Center : spawnPos;
                float maxEyeOffset = 12;

                Vector2 targetVect = eyeVelocity ? NPC.velocity :  targetPos - (NPC.Center + innerEyePosition.RotatedBy(NPC.rotation));
                if (targetVect.Length() > maxEyeOffset)
                    targetVect = targetVect.SafeNormalize(Vector2.UnitY) * maxEyeOffset;
                eyeVector = Vector2.Lerp(eyeVector, targetVect, rate);
            }

            if (SoundEngine.TryGetActiveSound(ChargeSlot, out var sound) && sound.IsPlaying)
            {
                sound.Position = NPC.Center;
                sound.Update();
            }
        }
        public override void AI()
        {
            if (NPC.ai[3] != 0)
            {
                NPC.ai[3]++;
            }

            NPC.rotation = 0f;
            NPC.frameCounter += 0.16d;
            if (deadTime > 0)
            {
                CheckDead();
                return;
            }

            ableToHit = NPC.localAI[0] >= 0 && NPC.ai[3] == 0 && deadTime == 0 && 
                !(NPC.ai[0] == FakeCharge.Id && NPC.ai[1] < FakeChargeWindup + FakeChargeTelegraph);

            if (NPC.localAI[0] < 0)
            {
                target = modNPC.GetTarget(NPC);

                if (NPC.localAI[0] == -cutsceneDuration)
                {
                    CutsceneSystem.SetCutscene(NPC.Center + eyePosition, cutsceneDuration, 30, 30, 1.1f);
                }
                NPC.localAI[0]++;

                float cameraLerp = 0.1f;
                Vector2 newCutscenePos = CutsceneSystem.cameraTargetCenter;
                int time = (int)NPC.localAI[0] + cutsceneDuration;
                int sweepingTime = time - cutsceneLookLeaveTime;
                if (time < cutsceneLookOverTime)
                {
                    NPC.velocity = Vector2.Zero;
                    if (time < cutsceneLookingDownTime)
                    {
                        cutsceneEyeVector = NPC.Center + Vector2.UnitY * 100;
                    }
                }
                else if (time < cutsceneLookLeaveTime)
                {
                    NPC.velocity = Vector2.Zero;
                    if (time == cutsceneLookRoarTime)
                    {

                    }
                    if (time == cutsceneLookLeaveTime - 20)
                    {
                        NPC.ai[3] = 1;
                        teleportTargetPos = spawnPos + cutsceneTeleportPos1;
                    }
                    if (NPC.ai[3] != 0)
                    {
                        newCutscenePos = teleportTargetPos;
                    }
                }
                else
                {
                    if (time < cutsceneLookLeaveTime + cutsceneSideSweepTime * 2 + cutsceneTopSweepTime)
                    {
                        if (sweepingTime == cutsceneSideSweepTime - 20)
                        {
                            NPC.ai[3] = 1;
                            teleportTargetPos = spawnPos + cutsceneTeleportPos2;
                        }
                        else if (sweepingTime == cutsceneSideSweepTime * 2 - 20)
                        {
                            NPC.ai[3] = 1;
                            teleportTargetPos = spawnPos + cutsceneTeleportPos3;
                        }
                        else if (sweepingTime == cutsceneSideSweepTime * 2 + cutsceneTopSweepTime - 20)
                        {
                            NPC.ai[3] = 1;
                            teleportTargetPos = spawnPos + new Vector2(0, -280);
                        }

                        if (sweepingTime < cutsceneSideSweepTime * 2)
                        {
                            NPC.velocity = -Vector2.UnitY * 10;
                        }
                        else
                        {
                            NPC.velocity = Vector2.UnitX * 15;
                            if (NPC.ai[3] == 0)
                                cameraLerp *= 1 + (sweepingTime - cutsceneSideSweepTime * 2) / (float)cutsceneTopSweepTime * 3f;
                        }
                        newCutscenePos = NPC.ai[3] != 0 && NPC.ai[3] <= teleportMoveTimestamp ? teleportTargetPos : NPC.Center;
                    }
                    else
                    {
                        if (time == cutsceneLookLeaveTime + cutsceneSideSweepTime * 2 + cutsceneTopSweepTime)
                        {
                            ZoomSystem.SetZoomAnimation(2f, 120);
                            SoundEngine.PlaySound(SoundID.NPCHit57 with { Volume = 0.5f, Pitch = 0.3f, PitchVariance = 0, SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest }, NPC.Center);
                        }
                        
                        newCutscenePos = NPC.ai[3] != 0 && NPC.ai[3] <= teleportMoveTimestamp ? teleportTargetPos : NPC.Center;
                        NPC.velocity = Vector2.Zero;
                    }
                }
                
                CutsceneSystem.cameraTargetCenter += (newCutscenePos - CutsceneSystem.cameraTargetCenter) * cameraLerp;

                Room room = modNPC.GetParentRoom();
                if (room != null)
                {
                    Vector2 soundPos = -Vector2.One;
                    if (time >= cutsceneLookLeaveTime && time < cutsceneLookLeaveTime + cutsceneSideSweepTime * 2 + cutsceneTopSweepTime)
                    {
                        Rectangle roomRect = room.GetRect();
                        roomRect.Inflate(room.WallInflateModifier.X * 16 + 48, room.WallInflateModifier.Y * 16 + 48);
                        int increment = 32;
                        if (sweepingTime < cutsceneSideSweepTime)
                        {
                            if (sweepingTime == 0)
                                ChargeSlot = SoundEngine.PlaySound(SoundID.Zombie102 with { Volume = 0.15f, Pitch = -0.5f, PitchVariance = 0, SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest }, NPC.Center);
                            Vector2 baseProjPos = new Vector2(roomRect.X, roomRect.Y);
                            Vector2 hitPos = TileCollidePositionInLine(baseProjPos, baseProjPos + Vector2.UnitY * roomRect.Height);
                            float length = baseProjPos.Distance(hitPos);

                            float completion = sweepingTime / (float)cutsceneSideSweepTime;
                            float oldCompletion = (sweepingTime - 1) / (float)cutsceneSideSweepTime;
                            int start = (int)((1 - completion) * length);
                            int end = (int)((1 - oldCompletion) * length);
                            for (int i = start; i < end; i++)
                            {
                                if (i % increment == 0)
                                {
                                    TrySpawnBorderProj(baseProjPos + Vector2.UnitY * i);
                                }
                            }
                        }
                        else if (sweepingTime < cutsceneSideSweepTime * 2)
                        {
                            int thisTime = sweepingTime - cutsceneSideSweepTime;
                            if (thisTime == 0)
                                ChargeSlot = SoundEngine.PlaySound(SoundID.Zombie102 with { Volume = 0.15f, Pitch = -0.36f, PitchVariance = 0, SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest }, NPC.Center);
                            Vector2 baseProjPos = new Vector2(roomRect.X + roomRect.Width, roomRect.Y);
                            Vector2 hitPos = TileCollidePositionInLine(baseProjPos, baseProjPos + Vector2.UnitY * roomRect.Height);
                            float length = baseProjPos.Distance(hitPos);

                            float completion = thisTime / (float)cutsceneSideSweepTime;
                            float oldCompletion = (thisTime - 1) / (float)cutsceneSideSweepTime;
                            int start = (int)((1 - completion) * length);
                            int end = (int)((1 - oldCompletion) * length);
                            for (int i = start; i < end; i++)
                            {
                                if (i % increment == 0)
                                {
                                    TrySpawnBorderProj(baseProjPos + Vector2.UnitY * i);
                                }
                            }
                        }
                        else
                        {
                            int thisTime = sweepingTime - (cutsceneSideSweepTime * 2);
                            if (thisTime == 0)
                                ChargeSlot = SoundEngine.PlaySound(SoundID.Zombie102 with { Volume = 0.15f, Pitch = -0.25f, PitchVariance = 0, SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest }, NPC.Center);
                            Vector2 baseProjPos = new Vector2(roomRect.X, roomRect.Y);
                            Vector2 hitPos = baseProjPos + Vector2.UnitX * roomRect.Width;
                            float length = baseProjPos.Distance(hitPos);

                            float completion = thisTime / (float)cutsceneTopSweepTime;
                            float oldCompletion = (thisTime - 1) / (float)cutsceneTopSweepTime;
                            int start = (int)((completion) * length);
                            int end = (int)((oldCompletion) * length);
                            if (end > 0)
                            {
                                for (int i = end; i < start; i++)
                                {
                                    if (i % increment == 0)
                                    {
                                        TrySpawnBorderProj(baseProjPos + Vector2.UnitX * i);
                                    }
                                }
                            }
                        }
                    }
                    NPC.localAI[2]--;
                    if (soundPos != -Vector2.One && NPC.localAI[2] <= 0)
                    {
                        NPC.localAI[2] = Main.rand.Next(3, 5);
                        SoundEngine.PlaySound(SoundID.Item88 with { Volume = 0.5f, Pitch = 0.05f, MaxInstances = 1,  SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest }, soundPos);
                        
                    }

                    void TrySpawnBorderProj(Vector2 pos)
                    {
                        if (!ParanoidTileRetrieval(pos.ToTileCoordinates()).IsTileSolidGround(true))
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), pos, Vector2.Zero, ModContent.ProjectileType<PhantasmalBarrier>(), NPC.damage, 0);
                            soundPos = pos;
                        }
                        cutsceneEyeVector = pos;
                    }
                }

                if (NPC.localAI[0] == -30)
                {
                    NPC.localAI[1] = 0;
                    NPC.immortal = false;
                    NPC.dontTakeDamage = false;
                    enemyHealthBar = new EnemyHealthBar([NPC.whoAmI], NPC.FullName);
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
            if (teleportAttackCooldown > 0)
                teleportAttackCooldown--;

            if (NPC.ai[0] == None.Id)
            {
                if (NPC.ai[1] >= None.Duration)
                {
                    ChooseAttack();
                }
                else
                {
                    DefaultMovement();
                    if (NPC.ai[1] == None.Duration - teleportMoveTimestamp + 1)
                    {
                        teleportTargetPos = new Vector2(-1);
                        NPC.ai[3] = 1;
                    }
                }
            }

            if (NPC.ai[0] == TeleportBolt.Id)
            {
                Vector2 targetPos = target != null ? target.Center : spawnPos;
                NPC.velocity = Vector2.Zero;
                int time = (int)NPC.ai[1] % TeleportBoltCycleTime;
                int attackStartTime = teleportMoveTimestamp + TeleportBoltTelegraph;
                List<Vector2> projSpawnRefPositions = [NPC.Center + innerEyePosition];
                if (phantomPositions.Count > 0)
                {
                    for (int i = 0; i < phantomPositions.Count; i++)
                    {
                        projSpawnRefPositions.Add(phantomPositions[i] + new Vector2(0, -20));
                    }
                }

                if (time < attackStartTime)
                {
                    if (time == teleportMoveTimestamp)
                    {
                        for (int i = 0; i < projSpawnRefPositions.Count; i++)
                        {
                            SoundEngine.PlaySound(SoundID.Item13 with { Volume = 1f / (projSpawnRefPositions.Count * 0.7f), Pitch = 0.2f, PitchVariance = 0, MaxInstances = 4 }, projSpawnRefPositions[i]);
                        }
                    }
                    if (time >= teleportMoveTimestamp && time % 3 == 0)
                    {
                        float maxLength = 12;
                        float range = MathHelper.PiOver4 * 1.5f;
                        for (int i = 0; i < projSpawnRefPositions.Count; i++)
                        {
                            Vector2 anchorPos = projSpawnRefPositions[i];
                            Vector2 vectToTarget = (targetPos - anchorPos);
                            if (vectToTarget.Length() > maxLength)
                                vectToTarget = vectToTarget.SafeNormalize(Vector2.UnitY) * maxLength;
                            Vector2 particleSpawnPos = anchorPos + vectToTarget * new Vector2(0.35f, 1f);

                            Vector2 offset = (Main.rand.NextFloat(-range, range) + vectToTarget.ToRotation()).ToRotationVector2() * Main.rand.NextFloat(32);

                            ParticleManager.AddParticle(new Ball(
                                particleSpawnPos + offset, -offset * 0.1f,
                                20, Color.Lerp(Color.Teal, Color.Cyan, Main.rand.NextFloat(0.25f, 0.75f)), new Vector2(0.25f), 0, 0.96f, 10));
                        }
                    }
                }
                else if (NPC.ai[3] == 0 && (time - attackStartTime) % TeleportBoltFireRate == 0)
                {
                    if ((time - attackStartTime) / TeleportBoltFireRate % 2 == 0)
                    {
                        float pitch = Main.rand.NextFloat(-0.05f, 0.05f);
                        for (int i = 0; i < projSpawnRefPositions.Count; i++)
                        {
                            SoundEngine.PlaySound(SoundID.Item125 with { Volume = 0.7f / (projSpawnRefPositions.Count * 0.66f), Pitch = pitch, PitchVariance = 0, MaxInstances = 8, SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest}, projSpawnRefPositions[i]);
                        }
                    }
                    float maxLength = 12;
                    for (int i = 0; i < projSpawnRefPositions.Count; i++)
                    {
                        Vector2 anchorPos = projSpawnRefPositions[i];
                        Vector2 vectToTarget = (targetPos - anchorPos);
                        if (vectToTarget.Length() > maxLength)
                            vectToTarget = vectToTarget.SafeNormalize(Vector2.UnitY) * maxLength;
                        Vector2 projSpawnPos = anchorPos + vectToTarget * new Vector2(0.35f, 1f);

                        Projectile.NewProjectile(NPC.GetSource_FromThis(), projSpawnPos, vectToTarget.SafeNormalize(Vector2.UnitY) * 15, ModContent.ProjectileType<PhantasmalBolt>(), NPC.damage, 0);
                    }
                }
                else if (time == TeleportBoltCycleTime - 20)
                {
                    teleportTargetPos = new Vector2(-1);
                    NPC.ai[3] = 1;
                }
                else if (time == TeleportBoltCycleTime - 1)
                {
                    phantomPositions.Add(NPC.Center + modNPC.drawCenter);
                }

                if (NPC.ai[1] == TeleportBolt.Duration)
                {
                    teleportTargetPos = new Vector2(-1);
                    NPC.ai[3] = 1;
                }
                if (NPC.ai[1] >= TeleportBolt.Duration + teleportMoveTimestamp - 1)
                {
                    phantomPositions.Clear();
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = TeleportBolt.Id;
                }
            }
            else if (NPC.ai[0] == ProjCharge.Id)
            {
                Vector2 targetPos = target != null ? target.Center : spawnPos;
                int time = (int)NPC.ai[1] % ProjChargeCycleTime;
                int chargeStartTime = teleportMoveTimestamp + ProjChargeTelegraph;
                Vector2 targetVect = targetPos - NPC.Center;

                if (time < chargeStartTime)
                {
                    if (time == 0)
                    {
                        ChargeSlot = SoundEngine.PlaySound(SoundID.NPCHit57 with { Volume = 0.4f, Pitch = 0.45f, PitchVariance = 0.2f, SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest }, NPC.Center);
                    }
                    float completion = time / (float)chargeStartTime;
                    NPC.velocity = -targetVect.SafeNormalize(Vector2.UnitY) * 10 * (1 - completion);
                }
                else
                {
                    if (time == chargeStartTime)
                    {
                        NPC.velocity = targetVect.SafeNormalize(Vector2.UnitY) * 22;
                        ChargeSlot = SoundEngine.PlaySound(SoundID.Zombie101 with { Volume = 0.3f, Pitch = -0.1f }, NPC.Center);
                    }
                    if (NPC.ai[3] == 0 && (time - chargeStartTime) % ProjChargeFireRate == 0)
                    {
                        SoundEngine.PlaySound(SoundID.Item12 with { Volume = 0.3f, MaxInstances = 24 }, NPC.Center);
                        if (!ParanoidTileRetrieval((NPC.Center + innerEyePosition).ToTileCoordinates()).IsTileSolidGround(true))
                        {
                            float direction = NPC.velocity.ToRotation();
                            for (int i = -1; i <= 1; i += 2)
                            {
                                float specificDirection = direction + i * MathHelper.PiOver2;
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + innerEyePosition, specificDirection.ToRotationVector2() * 8, ModContent.ProjectileType<PhantasmalEye>(), NPC.damage, 0);
                            }
                        }
                    }

                    int amount = NPC.ai[3] == 0 ? 3 : (NPC.ai[3] < 15 ? 1 : 0);
                    float velRot = NPC.velocity.ToRotation();
                    Vector2 baseParticlePos = NPC.Center + -NPC.velocity.SafeNormalize(Vector2.UnitY) * 75;
                    for (int i = 0; i < amount; i++)
                    {
                        Vector2 particlePos = baseParticlePos + (Vector2.UnitY * Main.rand.NextFloat(-75, 75)).RotatedBy(velRot);
                        Color particleColor = Color.Lerp(Color.Teal, Color.White, 0.2f);
                        ParticleManager.AddParticle(new Wriggler(
                            particlePos, NPC.velocity * -0.15f,
                            26, particleColor, new Vector2(0.5f), Main.rand.Next(4), velRot + Main.rand.NextFloat(-0.2f, 0.2f), 0.98f, 16,
                            Main.rand.NextBool() ? SpriteEffects.None : SpriteEffects.FlipVertically));

                        Vector2 offset = Main.rand.NextVector2Circular(2, 2);
                        ParticleManager.AddParticle(new Ball(
                            particlePos + offset, offset,
                            20, Color.Teal, new Vector2(0.25f), 0, 0.96f, 10));
                    }

                    if (time == ProjChargeCycleTime - 20)
                        NPC.ai[3] = 1;
                }

                if (NPC.ai[1] == ProjCharge.Duration)
                {
                    teleportTargetPos = new Vector2(-1);
                    NPC.ai[3] = 1;
                }
                if (NPC.ai[1] >= ProjCharge.Duration + teleportMoveTimestamp - 1)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = ProjCharge.Id;
                }
            }
            else if (NPC.ai[0] == FakeCharge.Id)
            {
                Vector2 targetPos = target != null ? target.Center : spawnPos;
                Vector2 targetVect = targetPos - NPC.Center;
                if (NPC.ai[1] < FakeChargeWindup)
                {
                    if (NPC.ai[1] == 0)
                    {
                        Vector2 anchor = spawnPos;
                        Vector2 oldSelectedPos = Vector2.Zero;
                        float startRot = Main.rand.NextFloat(MathHelper.TwoPi);
                        float stepRate = Main.rand.NextFloat(0.8f, 0.9f);
                        float currentRot = 0;
                        for (int i = 0; i < 23; i++)
                        {
                            for (int j = 0; j <= 100; j++)
                            {
                                bool force = j == 100;
                                currentRot += stepRate;
                                Vector2 offsetCalc = (currentRot + startRot).ToRotationVector2() * (currentRot * 60); // kinda spiral-y
                                Vector2 checkPos = offsetCalc + anchor;
                                if (oldSelectedPos.Distance(checkPos) > 360 || force)
                                {
                                    if (!ParanoidTileRetrieval(checkPos.ToTileCoordinates()).IsTileSolidGround(true) || force)
                                    {
                                        oldSelectedPos = checkPos;
                                        phantomPositions.Add(checkPos);
                                        break;
                                    }
                                }
                            }
                        }
                        List<Vector2> potentialPositions = [];
                        Vector2 chosenPos = Vector2.Zero;
                        for (int i = 0; i < phantomPositions.Count; i++)
                        {
                            Vector2 pos = phantomPositions[i];
                            if (pos.Distance(targetPos) < 500)
                                potentialPositions.Add(pos);
                        }
                        if (potentialPositions.Count == 0)
                        {
                            float distanceToBeat = 10000000;
                            for (int i = 0; i < phantomPositions.Count; i++)
                            {
                                Vector2 pos = phantomPositions[i];
                                float dist = pos.Distance(targetPos);
                                if (dist < distanceToBeat)
                                {
                                    distanceToBeat = dist;
                                    chosenPos = pos;
                                }
                            }
                            if (distanceToBeat == 10000000)
                            {
                                chosenPos = phantomPositions[0];
                            }
                        }
                        else
                        {
                            chosenPos = potentialPositions[Main.rand.Next(potentialPositions.Count)];
                        }
                        phantomPositions.Remove(chosenPos);
                        teleportTargetPos = chosenPos;
                    }
                    NPC.velocity = Vector2.Zero;
                }
                else if (NPC.ai[1] < FakeChargeWindup + FakeChargeTelegraph)
                {
                    if (NPC.ai[1] == FakeChargeWindup)
                    {
                        ChargeSlot = SoundEngine.PlaySound(SoundID.NPCHit57 with { Volume = 0.4f, Pitch = 0.45f, PitchVariance = 0.2f, SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest }, NPC.Center);
                    }
                    float completion = (NPC.ai[1] - (FakeChargeWindup)) / FakeChargeTelegraph;
                    NPC.velocity = -targetVect.SafeNormalize(Vector2.UnitY) * 10 * (1 - completion);
                }
                else if (NPC.ai[1] < FakeChargeWindup + FakeChargeTelegraph + FakeChargeDashDuration)
                {
                    int time = (int)NPC.ai[1] - (FakeChargeWindup + FakeChargeTelegraph);
                    if (time == 0)
                    {
                        NPC.velocity = targetVect.SafeNormalize(Vector2.UnitY) * 18;
                        ChargeSlot = SoundEngine.PlaySound(SoundID.Zombie101 with { Volume = 0.3f, Pitch = -0.1f }, NPC.Center);
                    }
                    if (time % FakeChargeFireRate == 0)
                    {
                        Vector2 projSpawnPos = NPC.Center + NPC.velocity + innerEyePosition;
                        int proj = Projectile.NewProjectile(NPC.GetSource_FromThis(), projSpawnPos, Vector2.Zero, ModContent.ProjectileType<PhantasmalSphere>(), NPC.damage, 0, -1, -1, 10, 120);
                        Main.projectile[proj].ai[2] -= 10;

                        SoundEngine.PlaySound(SoundID.DD2_DarkMageCastHeal with { Volume = 0.85f, MaxInstances = 100, Pitch = 1f }, projSpawnPos);
                    }

                    int amount = 3;
                    float velRot = NPC.velocity.ToRotation();
                    Vector2 baseParticlePos = NPC.Center + -NPC.velocity.SafeNormalize(Vector2.UnitY) * 75;
                    for (int i = 0; i < amount; i++)
                    {
                        Vector2 particlePos = baseParticlePos + (Vector2.UnitY * Main.rand.NextFloat(-75, 75)).RotatedBy(velRot);
                        Color particleColor = Color.Lerp(Color.Teal, Color.White, 0.2f);
                        ParticleManager.AddParticle(new Wriggler(
                            particlePos, NPC.velocity * -0.15f,
                            26, particleColor, new Vector2(0.5f), Main.rand.Next(4), velRot + Main.rand.NextFloat(-0.2f, 0.2f), 0.98f, 16,
                            Main.rand.NextBool() ? SpriteEffects.None : SpriteEffects.FlipVertically));

                        Vector2 offset = Main.rand.NextVector2Circular(2, 2);
                        ParticleManager.AddParticle(new Ball(
                            particlePos + offset, offset,
                            20, Color.Teal, new Vector2(0.25f), 0, 0.96f, 12));
                    }
                }
                else
                {
                    if (false && NPC.ai[1] == FakeChargeWindup + FakeChargeTelegraph + FakeChargeDashDuration + 30) // scrapped followup attack from clones
                    {
                        if (phantomPositions.Count > 0)
                        {
                            for (int i = 0; i < phantomPositions.Count; i++)
                            {
                                Vector2 projSpawnPos = phantomPositions[i] + new Vector2(0, -20);
                                if (projSpawnPos.Distance(targetPos) > 1000)
                                    continue;
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), projSpawnPos, (-Vector2.UnitY * 10).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)), ModContent.ProjectileType<PhantasmalSphere>(), NPC.damage, 0, -1, -1);
                            }
                        }
                    }
                    if (NPC.velocity.Length() > 1.6f)
                        NPC.velocity *= 0.97f;
                    NPC.velocity += targetVect.SafeNormalize(Vector2.UnitY) * 0.2f;
                }

                if (NPC.ai[1] == FakeCharge.Duration)
                {
                    teleportTargetPos = new Vector2(-1);
                    NPC.ai[3] = 1;
                }
                if (NPC.ai[1] >= FakeCharge.Duration + teleportMoveTimestamp - 1)
                {
                    phantomPositions.Clear();
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = FakeCharge.Id;
                }
            }
            else if (NPC.ai[0] == CrossBeam.Id)
            {
                DefaultMovement();
                Vector2 targetPos = target != null ? target.Center : spawnPos;
                if (NPC.ai[1] % CrossBeamCycleTime == 0)
                {
                    float radius = 370;
                    float rot = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 rotVect = rot.ToRotationVector2();
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), targetPos + rotVect * radius, -rotVect * radius * 1.5f, ModContent.ProjectileType<PhantasmalLaser>(), NPC.damage, 0); 

                    rot += Main.rand.NextFloat(0.7f, 1.3f);
                    rotVect = rot.ToRotationVector2();
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), targetPos + rotVect * radius, -rotVect * radius * 1.5f, ModContent.ProjectileType<PhantasmalLaser>(), NPC.damage, 0);
                }
                if (NPC.ai[1] == CrossBeam.Duration)
                {
                    teleportTargetPos = new Vector2(-1);
                    NPC.ai[3] = 1;
                }
                if (NPC.ai[1] >= CrossBeam.Duration + teleportMoveTimestamp - 1)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = CrossBeam.Id;
                }
            }
            else if (NPC.ai[0] == SpinBeam.Id)
            {
                NPC.velocity = Vector2.Zero;
                Vector2 targetPos = target != null ? target.Center : spawnPos;
                if (NPC.ai[1] < SpinBeamWindup)
                {
                    if (NPC.ai[1] == 0)
                    {
                        int teleportDir = targetPos.X > spawnPos.X ? -1 : 1;
                        teleportTargetPos = new Vector2(MathHelper.Clamp(targetPos.X - spawnPos.X, -1000, 1000) + Main.rand.NextFloat(280, 400) * teleportDir, Main.rand.NextFloat(200)) + spawnPos;

                        SoundEngine.PlaySound(WallOfFlesh.HellBeamCharge with { Volume = 0.9f, Pitch = 0.08f, PitchVariance = 0 }, teleportTargetPos);
                        SoundEngine.PlaySound(SoundID.Zombie95 with { Volume = 0.45f }, teleportTargetPos);
                    }

                    if (NPC.ai[1] > 6 && NPC.ai[1] < SpinBeamWindup - 24)
                    {
                        Color outlineColor = Color.Lerp(Color.Cyan, Color.Blue, 0.13f);
                        Color fillColor = Color.Lerp(outlineColor, Color.Teal, 0.2f);
                        Vector2 particleAnchor = NPC.Center + innerEyePosition;
                        Vector2 offset = Main.rand.NextVector2Circular(90, 90);
                        ParticleManager.AddParticle(new BallOutlined(
                            particleAnchor + offset, -offset * 0.05f,
                            60, outlineColor, fillColor, new Vector2(Main.rand.NextFloat(0.12f, 0.18f)), 4, 0, 0.97f, 50),
                            ParticleManager.ParticleLayer.AfterProjectiles);
                    }
                }
                else if (NPC.ai[1] < SpinBeamWindup + SpinBeamFireDuration)
                {
                    int fireTime = (int)NPC.ai[1] - SpinBeamWindup;
                    if (fireTime == 0)
                    {
                        SoundEngine.PlaySound(SoundID.Zombie104 with { Volume = 0.7f, Pitch = -0.4f, PitchVariance = 0 }, NPC.Center + innerEyePosition);

                        float startRot = Main.rand.NextFloat(MathHelper.TwoPi);
                        NPC.direction = targetPos.X > NPC.Center.X ? 1 : -1;
                        float rotPerCycle = MathHelper.TwoPi / 6f;
                        Vector2 projReferencePos = innerEyePosition;
                        for (int i = 0; i < 6; i++)
                        {
                            float rot = startRot + i * rotPerCycle;
                            Vector2 projSpawnPos = NPC.Center + innerEyePosition + rot.ToRotationVector2() * 4;
                            int proj = Projectile.NewProjectile(NPC.GetSource_FromThis(), projSpawnPos, Vector2.Zero, ModContent.ProjectileType<PhantasmalDeathBeam>(), NPC.damage, 0, -1, projReferencePos.X, projReferencePos.Y);
                            Main.projectile[proj].rotation = rot;
                        }
                    }
                    for (int i = 0; i < SpinBeamTrueEyeTimes.Length; i++)
                    {
                        int spawnTime = SpinBeamTrueEyeTimes[i];
                        if (fireTime == spawnTime)
                        {
                            float radius = 160;
                            float rot = (targetPos - NPC.Center - innerEyePosition).ToRotation();
                            rot += -NPC.direction * (Main.rand.NextFloat(-1.2f, 1.2f) + MathHelper.PiOver2);
                            Vector2 rotVect = rot.ToRotationVector2();
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), targetPos + rotVect * radius, -rotVect * 15, ModContent.ProjectileType<PhantasmalBoltShooter>(), NPC.damage, 0);
                        }
                    }
                }
                else
                {

                }
                if (NPC.ai[1] == SpinBeam.Duration)
                {
                    teleportTargetPos = new Vector2(-1);
                    NPC.ai[3] = 1;
                }
                if (NPC.ai[1] >= SpinBeam.Duration + teleportMoveTimestamp - 1)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = SpinBeam.Id;
                }
            }
            else if (NPC.ai[0] == Teleport.Id)
            {
                int time = (int)NPC.ai[1] - 25;
                DefaultMovement();
                NPC.velocity *= 0.5f;
                if (time % 45 == 0 && NPC.ai[1] < Teleport.Duration)
                {
                    teleportTargetPos = new Vector2(-1);
                    NPC.ai[3] = 1;
                }
                if (NPC.ai[1] == Teleport.Duration)
                {
                    teleportTargetPos = new Vector2(-1);
                    NPC.ai[3] = 1;
                }
                if (NPC.ai[1] >= Teleport.Duration + teleportMoveTimestamp - 1)
                {
                    teleportAttackCooldown = teleportAttackSetCooldown;
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                }
            }
            else if (NPC.ai[0] == Summon.Id)
            {
                Vector2 targetPos = target != null ? target.Center : spawnPos;
                DefaultMovement();
                NPC.velocity *= 0.3f;
                if (NPC.ai[1] < SummonWindup)
                {
                    if (NPC.ai[1] == 0)
                    {
                        int teleportDir = targetPos.X > spawnPos.X ? -1 : 1;
                        teleportTargetPos = new Vector2(MathHelper.Clamp(targetPos.X - spawnPos.X, -1000, 1000) + Main.rand.NextFloat(280, 400) * teleportDir, Main.rand.NextFloat(200)) + spawnPos;
                    }
                }
                else if (NPC.ai[1] < SummonWindup + SummonTime)
                {
                    int time = (int)NPC.ai[1] - SummonWindup;
                    if (time % SummonRate == 0)
                    {
                        Vector2 summonPos = NPC.Center + new Vector2(0, 80) + NPC.velocity * 4;
                        Vector2 summonVelocity = Vector2.UnitY.RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * 4;
                        NPC spawnedNPC = NPC.NewNPCDirect(NPC.GetSource_FromThis(), summonPos, ModContent.NPCType<TrueServant>(), 0, 0, -60);
                        spawnedNPC.velocity = summonVelocity;
                        spawnedNPC.rotation = summonVelocity.ToRotation() + MathHelper.PiOver2;
                        spawnedNPC.localAI[0] = 10;

                        SoundEngine.PlaySound(SoundID.DD2_WitherBeastAuraPulse with { Volume = 1f, MaxInstances = 4 }, summonPos);
                        SoundEngine.PlaySound(SoundID.DD2_SkeletonSummoned with { Volume = 1f, MaxInstances = 4 }, summonPos);
                    }
                }
                if (NPC.ai[1] == Summon.Duration)
                {
                    teleportTargetPos = new Vector2(-1);
                    NPC.ai[3] = 1;
                }
                if (NPC.ai[1] >= Summon.Duration + teleportMoveTimestamp - 1)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Summon.Id;
                }
            }

            void DefaultMovement()
            {
                Vector2 targetPos = target != null ? target.Center : spawnPos;

                NPC.velocity = (targetPos - NPC.Center).SafeNormalize(Vector2.UnitY) * 1f;
                float distance = targetPos.Distance(NPC.Center);
                if (distance < 120)
                {
                    NPC.velocity *= distance / 120;
                }
                else
                {
                    NPC.velocity *= (distance - 120) * 0.5f / 120f + 1;
                }
            }
        }
        public void ChooseAttack()
        {
            NPC.ai[1] = 0;
            int chosenAttack = 0;

            List<Attack> potentialAttacks = new List<Attack>() { TeleportBolt, ProjCharge, FakeCharge, CrossBeam, SpinBeam, Teleport, Summon };
            potentialAttacks.RemoveAll(x => x.Id == (int)NPC.ai[2]);
            if (teleportAttackCooldown > 0)
                potentialAttacks.RemoveAll(x => x.Id == Teleport.Id);

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
            ableToHit = false;
            canBeHit = false;

            if (deadTime == 0)
            {
                ExtraSoundSystem.ForceStopAllExtraSounds();
                enemyHealthBar.ForceEnd(0);
                NPC.velocity = Vector2.Zero;
                NPC.rotation = 0;
                modNPC.ignitedStacks.Clear();
                phantomPositions.Clear();

                if (modNPC.isRoomNPC)
                {
                    if (ActiveBossTheme != null)
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
                for (int i = 0; (double)i < hit.Damage * 0.01d; i++)
                {
                    int d = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Vortex, hit.HitDirection, -1f, 0, default, 0.9f);
                    Main.dust[d].noLight = true;
                    Main.dust[d].noLightEmittence = true;
                }
            }
        }
        public override void OnKill()
        {
            
        }
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            return ableToHit;
        }
        public override bool CanHitNPC(NPC target)
        {
            return ableToHit;
        }
        public override void ModifyHoverBoundingBox(ref Rectangle boundingBox)
        {
            boundingBox = NPC.Hitbox;
        }
        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            position = NPC.Center + new Vector2(0, 116);
            return !(NPC.ai[0] == FakeCharge.Id && NPC.ai[1] < FakeChargeWindup + 15);
        }
        public override void FindFrame(int frameHeight)
        {
            var tex = TextureAssets.Npc[Type].Value;

            currentFrame = (int)NPC.frameCounter % (Main.npcFrameCount[Type] - 1) + 1;
            if (!NPC.active)
                currentFrame = 0;
            NPC.frame = new Rectangle(0, frameHeight * currentFrame, tex.Width, frameHeight - 2);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            var tex = TextureAssets.Npc[Type].Value;
            Color npcColor = Color.White;
            Color phantomColor = Color.Lerp(Color.DarkBlue, Color.Cyan, 0.4f) * 0.65f;
            Vector2 scale = new Vector2(NPC.scale);
            Vector2 drawOff = -Main.screenPosition;
            if (NPC.ai[0] == FakeCharge.Id)
            {
                if (NPC.ai[1] < FakeChargeWindup + FakeChargeTelegraph)
                {
                    float interpolant = MathHelper.Clamp((NPC.ai[1] - FakeChargeWindup) / FakeChargeTelegraph, 0, 1);
                    npcColor = Color.Lerp(phantomColor, npcColor, interpolant);
                }
            }

            if (phantomPositions.Count > 0)
            {
                Vector2 targetPos = target != null ? target.Center : spawnPos;
                bool center = NPC.ai[0] == FakeCharge.Id;
                float teleportInterpolant = NPC.ai[3] / teleportTime;
                for (int i = 0; i < phantomPositions.Count; i++)
                {
                    Vector2 pos = phantomPositions[i];
                    var phantDraws = TerRoguelikeWorld.GetTrueBrainDrawList(pos, center ? Vector2.Zero : (targetPos - pos), scale, phantomColor, (int)NPC.frameCounter, teleportInterpolant);
                    for (int d = 0; d < phantDraws.Count; d++)
                    {
                        phantDraws[d].Draw(drawOff);
                    }
                }
            }

            
            if (NPC.ai[3] > 0)
            {
                float interpolant = NPC.ai[3] < teleportMoveTimestamp ? NPC.ai[3] / (teleportMoveTimestamp) : 1f - ((NPC.ai[3] - teleportMoveTimestamp) / (teleportTime - teleportMoveTimestamp));
                float horizInterpolant = MathHelper.Lerp(1f, 2f, 0.5f + (0.5f * -(float)Math.Cos(interpolant * MathHelper.TwoPi)));
                float verticInterpolant = MathHelper.Lerp(0.5f + (0.5f * (float)Math.Cos(interpolant * MathHelper.TwoPi)), 8f, interpolant * interpolant);
                scale.X *= horizInterpolant;
                scale.Y *= verticInterpolant;

                scale *= 1f - interpolant;
            }

            List<StoredDraw> draws = [];

            Vector2 bodyOff = modNPC.drawCenter.RotatedBy(NPC.rotation);
            Vector2 eyeoff = eyePosition.RotatedBy(NPC.rotation);
            Vector2 innerEyeOff = innerEyePosition.RotatedBy(NPC.rotation) + eyeVector * new Vector2(0.35f, 1f);
            draws.Add(new(tex, NPC.Center + bodyOff * scale, NPC.frame, npcColor, NPC.rotation, NPC.frame.Size() * 0.5f, scale, SpriteEffects.None));
            draws.Add(new(eyeTex, NPC.Center + eyeoff * scale, null, npcColor, NPC.rotation, eyeTex.Size() * 0.5f, scale, SpriteEffects.None));
            draws.Add(new(innerEyeTex, NPC.Center + innerEyeOff * scale, null, npcColor, 0, innerEyeTex.Size() * 0.5f, scale, SpriteEffects.None));

            if (modNPC.ignitedStacks.Count > 0)
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
                        draw.Draw(drawOff + Vector2.UnitX.RotatedBy(j * MathHelper.PiOver4 + draw.rotation) * 2);
                    }
                }
                StartVanillaSpritebatch();

            }
            
            for (int i = 0; i < draws.Count; i++)
            {
                draws[i].Draw(drawOff);
            }

            return false;
        }
    }
}
