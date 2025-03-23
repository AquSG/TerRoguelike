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
using static Terraria.GameContent.PlayerEyeHelper;
using static TerRoguelike.Managers.TextureManager;
using static TerRoguelike.Schematics.SchematicManager;
using static TerRoguelike.Systems.MusicSystem;
using static TerRoguelike.Systems.RoomSystem;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using static TerRoguelike.Systems.EnemyHealthBarSystem;
using static TerRoguelike.NPCs.Enemy.Boss.MoonLord;
using System.IO;

namespace TerRoguelike.NPCs.Enemy.Boss
{
    public class MoonLordHand : BaseRoguelikeNPC
    {
        public Entity target;
        public Vector2 spawnPos;
        public bool ableToHit = false;
        public bool canBeHit = true;
        public override int modNPCID => ModContent.NPCType<MoonLordHand>();
        public override string Texture => "TerRoguelike/NPCs/Enemy/Boss/MoonLordSideEye";
        public Texture2D trueEyeTex;
        public Texture2D innerEyeTex;
        public int currentFrame;
        public Vector2 trueEyeVector;
        public bool goreProc = false;
        public SlotId trackedSlot;
        public override List<int> associatedFloors => new List<int>() { FloorDict["Lunar"] };
        public override int CombatStyle => -1;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1;
            NPCID.Sets.MustAlwaysDraw[Type] = true;
            NPCID.Sets.NoMultiplayerSmoothingByType[Type] = true;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            modNPC.TerRoguelikeBoss = true;
            NPC.width = 46;
            NPC.height = 76;
            NPC.aiStyle = -1;
            NPC.damage = 36;
            NPC.lifeMax = 10000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.knockBackResist = 0f;
            modNPC.drawCenter = new Vector2(0, 0);
            modNPC.IgnoreRoomWallCollision = true;
            modNPC.OverrideIgniteVisual = true;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            trueEyeTex = TexDict["TrueEyeOfCthulhu"];
            innerEyeTex = TexDict["MoonLordInnerEye"];
            modNPC.AdaptiveArmorEnabled = true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.ai[2] = -1;
            if (source is EntitySource_Parent parentSource)
            {
                if (parentSource.Entity is NPC)
                {
                    NPC.ai[2] = parentSource.Entity.whoAmI;
                    NPC npc = Main.npc[(int)NPC.ai[2]];
                    if (!npc.active || npc.type != ModContent.NPCType<MoonLord>())
                    {
                        NPC.ai[2] = -1;
                        NPC.StrikeInstantKill();
                        NPC.active = false;
                        return;
                    }

                }
            }

            if (NPC.ai[2] == -1)
            {
                NPC.StrikeInstantKill();
                NPC.active = false;
            }

            NPC.immortal = true;
            NPC.dontTakeDamage = true;
            spawnPos = NPC.Center;
            ableToHit = false;
            trueEyeVector = Vector2.Zero;
        }
        public override void PostAI()
        {
            if (SoundEngine.TryGetActiveSound(trackedSlot, out var sound) && sound.IsPlaying)
            {
                sound.Position = NPC.Center;
            }
        }
        public override void AI()
        {
            NPC.netSpam = 0;
            NPC parent = Main.npc[(int)NPC.ai[2]];
            if (!parent.active || parent.type != ModContent.NPCType<MoonLord>())
            {
                NPC.dontTakeDamage = false;
                NPC.immortal = false;
                NPC.StrikeInstantKill();
                NPC.active = false;
                return;
            }

            NPC.dontTakeDamage = parent.localAI[0] <= -30;
            NPC.immortal = parent.localAI[0] <= -30;
            canBeHit = parent.localAI[3] == 1;
            if (NPC.life <= 1)
            {
                if (CheckDead())
                    return;

                TrueEyeAI(parent);
            }
        }
        public void TrueEyeAI(NPC parent)
        {
            var modNpc = NPC.ModNPC();
            var modParent = parent.ModNPC();

            modNpc.targetPlayer = modParent.targetPlayer;
            modNpc.targetNPC = modParent.targetNPC;

            if (modNpc.targetPlayer >= 0)
            {
                target = Main.player[modNpc.targetPlayer];
            }
            else if (modNpc.targetNPC >= 0)
            {
                target = Main.npc[modNpc.targetNPC];
            }
            else
                target = null;

            float rotCap = MathHelper.PiOver2 * 0.66f;
            bool phantasmalSpin = parent.ai[0] == PhantSpin.Id;
            bool phantasmalSphere = parent.ai[0] == PhantSphere.Id;
            bool deathray = parent.ai[0] == Deathray.Id && parent.ai[1] > deathrayWindup - 30;
            bool deathrayComing = parent.ai[0] == Deathray.Id && parent.ai[1] < deathrayWindup - 20;
            bool tentacleCharge = parent.ai[0] == TentacleCharge.Id && parent.ai[1] >= tentacleWindup && TentacleCharge.Duration - parent.ai[1] > 150;

            Vector2 targetPos = target != null ? target.Center : spawnPos;

            if (phantasmalSpin)
            {
                if (parent.ai[1] < phantSpinWindup)
                {
                    NPC.velocity *= 0.95f;
                    NPC.rotation = MathHelper.Clamp(NPC.velocity.X * 0.1f, -rotCap, rotCap);
                }
                else
                {

                    float spinSpeed = 10;
                    bool initiateSpin = parent.ai[1] == phantSpinWindup || NPC.velocity.Length() < 1f; // starts the spin either in sync with moon lord or if an eye pops out mid attack, which is the 1 case where this guy shouldn't have high velocity mid-attack
                    if (initiateSpin)
                    {
                        NPC.velocity = new Vector2(0, -spinSpeed);
                        NPC.netUpdate = true;
                    }
                    if (parent.ai[1] == phantSpinWindup) // only play the sound if it started naturally though because a similar sound plays when the eye pops out
                    {
                        SoundEngine.PlaySound(SoundID.Zombie101 with { Volume = 0.3f, MaxInstances = 2 }, NPC.Center + new Vector2(80 * NPC.direction, 0));
                        NPC.netUpdate = true;
                    }
                    NPC.velocity = NPC.velocity.RotatedBy(0.013f * NPC.direction * spinSpeed);
                    NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;
                }
            }
            else if (phantasmalSphere)
            {
                float quickMoveVel = 0.5f;
                Vector2 attackStartPos = new Vector2(NPC.localAI[0], NPC.localAI[1]);
                if (parent.ai[1] == 0 || attackStartPos.X == 0)
                {
                    if (!TerRoguelike.mpClient)
                    {
                        Vector2 randVect = Main.rand.NextVector2CircularEdge(quickMoveVel, quickMoveVel);
                        randVect.Y = Math.Abs(randVect.Y);
                        NPC.velocity = randVect;
                        attackStartPos = NPC.Center;
                        NPC.localAI[0] = attackStartPos.X;
                        NPC.localAI[1] = attackStartPos.Y;
                        NPC.netUpdate = true;
                    }
                }
                else if (parent.ai[1] % 30 < 10)
                {
                    if (parent.ai[1] % 30 == 0 && !TerRoguelike.mpClient)
                    {
                        NPC.velocity *= 0.35f;
                        float randRot = Main.rand.NextFloat(-1.2f, 1.2f);
                        NPC.velocity += ((attackStartPos - NPC.Center).SafeNormalize(Vector2.UnitY) * quickMoveVel * 3).RotatedBy(randRot + Math.Sign(randRot) * 0.2f);
                        NPC.netUpdate = true;
                    }
                    else
                    {
                        NPC.velocity += NPC.velocity.SafeNormalize(Vector2.UnitY) * quickMoveVel;
                    }
                }
                NPC.velocity *= 0.97f;
                NPC.rotation = NPC.rotation.AngleTowards(NPC.rotation = MathHelper.Clamp(NPC.velocity.X * 0.1f, -rotCap, rotCap), 0.1f);
                    
            }
            else if (tentacleCharge)
            {
                int tentacleChargeTimer = (int)parent.ai[1] - tentacleWindup;
                int chargeTimer = tentacleChargeTimer % 120;
                Vector2 targetVect = targetPos - NPC.Center;
                float targetVectLength = targetVect.Length();
                float rotToTarget = (targetVect).ToRotation();
                if (chargeTimer < 60)
                {
                    if (chargeTimer == 26)
                    {
                        SoundEngine.PlaySound(SoundID.Zombie102 with { Volume = 0.1f, Pitch = -0.5f, MaxInstances = 10, SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest }, NPC.Center);
                    }
                    NPC.localAI[0] = 1; // this is here to try and catch if the eye was popped out when it would be mid charge.
                    NPC.velocity *= 0.97f;
                    float minTargetDist = 300;
                    float maxTargetDist = 460;
                    if (targetVectLength > minTargetDist && targetVectLength < maxTargetDist)
                    {
                        NPC.velocity *= 0.98f;
                    }
                    else
                    {
                        Vector2 wantedPos = targetPos + (-rotToTarget.ToRotationVector2() * ((minTargetDist + maxTargetDist) * 0.5f));
                        NPC.velocity += (wantedPos - NPC.Center).SafeNormalize(Vector2.UnitY) * 0.5f;
                    }
                    NPC.rotation = NPC.rotation.AngleLerp(rotToTarget + MathHelper.PiOver2, 0.05f);
                }
                else
                {
                    if (chargeTimer < 90 && NPC.localAI[0] == 1)
                    {
                        if (chargeTimer == 60)
                        {
                            NPC.velocity = rotToTarget.ToRotationVector2() * 16;
                            NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;
                            trackedSlot = SoundEngine.PlaySound(SoundID.Zombie101 with { Volume = 0.15f, MaxInstances = 2, Pitch = -0.1f }, NPC.Center + new Vector2(80 * NPC.direction, 0));
                            NPC.netUpdate = true;
                        }
                        Color particleColor = Color.Lerp(Color.Teal, Color.White, 0.2f);
                        float effectiveRot = NPC.rotation - MathHelper.PiOver2;
                        ParticleManager.AddParticle(new Wriggler(
                            NPC.Center - effectiveRot.ToRotationVector2() * 20 + (Vector2.UnitY * Main.rand.NextFloat(-20, 20)).RotatedBy(effectiveRot), NPC.velocity * 0.66f,
                            26, particleColor, new Vector2(0.5f), Main.rand.Next(4), effectiveRot, 0.98f, 16,
                            Main.rand.NextBool() ? SpriteEffects.None : SpriteEffects.FlipVertically));
                    }
                    else
                    {
                        NPC.localAI[0] = 0;
                        NPC.velocity *= 0.96f;
                        if (TentacleCharge.Duration - parent.ai[1] < 180)
                        {
                            NPC.rotation = NPC.rotation.AngleLerp(0, 0.05f);
                        }
                        else
                        {
                            NPC.rotation = NPC.rotation.AngleLerp(rotToTarget + MathHelper.PiOver2, 0.05f);
                        }
                        
                    }
                }
                
            }
            else if (deathray)
            {
                if ((int)parent.ai[1] % 10 == 0)
                    NPC.netUpdate = true;

                NPC.velocity *= 0.98f;
                if (parent.ai[1] >= deathrayWindup)
                {
                    NPC.velocity += -trueEyeVector * 0.0014f;
                }
                if (NPC.rotation > rotCap)
                {
                    NPC.rotation = NPC.rotation.AngleTowards(0, 0.1f);
                }
                else
                    NPC.rotation = MathHelper.Clamp(NPC.velocity.X * 0.1f, -rotCap, rotCap);
            }
            else
            {
                if ((int)parent.ai[1] % 20 <= 1)
                    NPC.netUpdate = true;
                Vector2 wantedPos = targetPos + new Vector2(130 * NPC.direction, -230) + trueEyeVector * -7;
                float wantedRadius = 90;
                if (NPC.Center.Distance(wantedPos) <= wantedRadius)
                {
                    NPC.velocity *= Main.rand.NextFloat(0.96f, 0.97f);
                    float velocityLength = NPC.velocity.Length();
                    if (velocityLength > 1)
                    {
                        NPC.velocity = (NPC.velocity.ToRotation().AngleTowards((wantedPos - NPC.Center).ToRotation(), 0.02f)).ToRotationVector2() * velocityLength;
                    }
                }
                else
                {
                    float maxSpeed = 12;
                    NPC.velocity *= 0.98f;
                    NPC.velocity += (wantedPos - NPC.Center).SafeNormalize(Vector2.UnitY) * 0.25f * Main.rand.NextFloat(0.9f, 1f);
                    if (NPC.velocity.Length() > maxSpeed)
                        NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * maxSpeed;
                }
                if (NPC.rotation > rotCap)
                {
                    NPC.rotation = NPC.rotation.AngleTowards(0, 0.1f);
                }
                else
                    NPC.rotation = MathHelper.Clamp(NPC.velocity.X * 0.1f, -rotCap, rotCap);
            }
            if (deathrayComing)
            {
                if (NPC.ai[1] % 2 == 0)
                {
                    Vector2 offset = Main.rand.NextVector2Circular(16, 16);
                    offset += trueEyeVector.SafeNormalize(Vector2.UnitY) * 16;
                    ParticleManager.AddParticle(new Ball(
                        NPC.Center + trueEyeVector.SafeNormalize(Vector2.UnitY) * 3 + offset + trueEyeVector, -offset * 0.1f + NPC.velocity,
                        20, Color.Lerp(Color.White, Color.Cyan, 0.75f), new Vector2(0.25f), 0, 0.96f, 10));
                }
            }

            float rate = 0.2f;
            if (phantasmalSpin || phantasmalSphere || deathray)
                rate = 0.05f;
            InnerEyePositionUpdate(ref trueEyeVector, NPC.Center);

            void InnerEyePositionUpdate(ref Vector2 eyeVector, Vector2 basePosition)
            {
                bool eyeCenter = phantasmalSpin || phantasmalSphere;

                if (eyeCenter)
                {
                    eyeVector = Vector2.Lerp(eyeVector, Vector2.Zero, rate);
                }
                else if (deathray)
                {
                    if (parent.ai[1] >= deathrayWindup)
                        rate = 0.5f;
                    Vector2 deathrayConvergePos = new Vector2(parent.localAI[1], parent.localAI[2]);
                    Vector2 deathrayVect = deathrayConvergePos - basePosition;
                    float maxEyeOffset = 15;
                    if (deathrayVect.Length() > maxEyeOffset)
                        deathrayVect = deathrayVect.SafeNormalize(Vector2.UnitY) * maxEyeOffset;
                    eyeVector = Vector2.Lerp(eyeVector, deathrayVect, rate);
                }
                else
                {
                    Vector2 targetPos = target != null ? target.Center : spawnPos + new Vector2(0, -80);
                    float maxEyeOffset = 15;

                    Vector2 targetVect = targetPos - basePosition;
                    if (targetVect.Length() > maxEyeOffset)
                        targetVect = targetVect.SafeNormalize(Vector2.UnitY) * maxEyeOffset;
                    eyeVector = Vector2.Lerp(eyeVector, targetVect, rate);
                }
            }
        }
       
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            return ableToHit;
        }
        public override bool CanHitNPC(NPC target)
        {
            return canBeHit;
        }
        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            return canBeHit ? null : false;
        }

        public override bool CheckDead()
        {
            if (NPC.ai[2] < 0 || NPC.ai[0] == 1)
            {
                NPC.immortal = false;
                NPC.dontTakeDamage = false;
                NPC.StrikeInstantKill();
                return true;
            }
            ableToHit = false;
            NPC.frameCounter += 0.2d;
            if (NPC.localAI[3] == 0)
            {
                modNPC.ignitedStacks.Clear();
                modNPC.bleedingStacks.Clear();
                modNPC.ballAndChainSlow = 0;
                NPC.height = NPC.width;
            }
            NPC.localAI[3]++;
            NPC parent = Main.npc[(int)NPC.ai[2]];
            if (parent.active)
            {
                NPC.active = true;
                NPC.life = 1;
                NPC.immortal = true;
                NPC.dontTakeDamage = true;
                return false;
            }
            NPC.StrikeInstantKill();
            return true;
        }
        
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 1)
            {
                if (!CheckDead())
                {
                    if (!goreProc)
                    {
                        NPC parent = Main.npc[(int)NPC.ai[2]];
                        SoundEngine.PlaySound(SoundID.NPCDeath1 with { Volume = 1f }, parent.Center + new Vector2(0, -300));
                        SoundEngine.PlaySound(SoundID.NPCHit57 with { Volume = 0.5f }, parent.Center + new Vector2(0, -300));
                        SoundEngine.PlaySound(SoundID.NPCDeath62 with { Volume = 0.8f }, NPC.Center);
                        int[] goreIds = [GoreID.MoonLordHeart1, GoreID.MoonLordHeart2, GoreID.MoonLordHeart3, GoreID.MoonLordHeart4];
                        for (int i = 0; i < 8; i++)
                        {
                            int goreId = goreIds[(i / 2) % 4];
                            Gore.NewGore(NPC.GetSource_Death(), Main.rand.NextVector2Circular(NPC.width * 0.3f, NPC.height * 0.3f) + NPC.position + new Vector2(NPC.width * 0.25f, NPC.height * 0.5f), Main.rand.NextVector2CircularEdge(3, 3) + Vector2.UnitY * 0.5f, goreId, NPC.scale * 0.5f);
                        }
                        goreProc = true;
                    }
                }
            }
            else
            {
                for (int i = 0; (double)i < hit.Damage / (double)NPC.lifeMax * 1000.0; i++)
                {
                    int d = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Vortex, hit.HitDirection, -1f, 0, default, 0.5f);
                    Main.dust[d].noLight = true;
                    Main.dust[d].noLightEmittence = true;
                }
            }
        }
        public override void OnKill()
        {
            for (int i = 0; i < 16; i++)
            {
                Color outlineColor = Color.Lerp(Color.Teal, Color.Cyan, 0.4f);
                Vector2 offset = Main.rand.NextVector2Circular(2.5f, 2.5f);
                ParticleManager.AddParticle(new BallOutlined(
                    NPC.Center - offset + Main.rand.NextVector2CircularEdge(16, 16), offset,
                    21, outlineColor, Color.White * 0.75f, new Vector2(0.3f), 5, 0, 0.96f, 15));
            }
        }
        public override void ModifyHoverBoundingBox(ref Rectangle boundingBox)
        {
            boundingBox = NPC.Hitbox;
        }
        public override void FindFrame(int frameHeight)
        {
            if (Main.dedServ)
                return;

            currentFrame = (int)NPC.frameCounter % 4;
            frameHeight = trueEyeTex.Height / 4;
            NPC.frame = new Rectangle(0, frameHeight * currentFrame, trueEyeTex.Width, frameHeight - 2);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.life <= 1)
            {
                Vector2 eyeOrigin = new Vector2(NPC.frame.Width * 0.5f);
                Main.EntitySpriteDraw(trueEyeTex, NPC.Center - Main.screenPosition, NPC.frame, Color.White, NPC.rotation, eyeOrigin, NPC.scale, SpriteEffects.None);
                Main.EntitySpriteDraw(innerEyeTex, NPC.Center + trueEyeVector - Main.screenPosition, null, Color.White, NPC.rotation, innerEyeTex.Size() * 0.5f, NPC.scale, SpriteEffects.None);
            }

            DrawDeathrayForNPC(NPC, Main.npc[(int)NPC.ai[2]]);
            return false;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.WriteVector2(spawnPos);
            writer.Write(NPC.localAI[0]);
            writer.Write(NPC.localAI[1]);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            spawnPos = reader.ReadVector2();
            NPC.localAI[0] = reader.ReadSingle();
            NPC.localAI[1] = reader.ReadSingle();
        }
    }
}
