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

namespace TerRoguelike.NPCs.Enemy.Boss
{
    public class MoonLordHand : BaseRoguelikeNPC
    {
        public Entity target;
        public Vector2 spawnPos;
        public bool ableToHit = true;
        public bool canBeHit = true;
        public override int modNPCID => ModContent.NPCType<MoonLordHand>();
        public override string Texture => "TerRoguelike/NPCs/Enemy/Boss/MoonLordSideEye";
        public Texture2D trueEyeTex;
        public Texture2D innerEyeTex;
        public int currentFrame;
        public Vector2 trueEyeVector;
        public bool goreProc = false;
        public override List<int> associatedFloors => new List<int>() { FloorDict["Lunar"] };
        public override int CombatStyle => -1;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 46;
            NPC.height = 76;
            NPC.aiStyle = -1;
            NPC.damage = 36;
            NPC.lifeMax = 15000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.knockBackResist = 0f;
            modNPC.drawCenter = new Vector2(0, 0);
            modNPC.IgnoreRoomWallCollision = true;
            modNPC.OverrideIgniteVisual = true;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            trueEyeTex = TexDict["TrueEyeOfCthulhu"];
            innerEyeTex = TexDict["MoonLordInnerEye"];
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
            
        }
        public override void AI()
        {
            NPC parent = Main.npc[(int)NPC.ai[2]];
            if (!parent.active || parent.type != ModContent.NPCType<MoonLord>())
            {
                NPC.dontTakeDamage = false;
                NPC.immortal = false;
                NPC.StrikeInstantKill();
                NPC.active = false;
                return;
            }

            NPC.dontTakeDamage = false;
            NPC.immortal = false;
            canBeHit = true;
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
                    }
                    if (parent.ai[1] == phantSpinWindup) // only play the sound if it started naturally though because a similar sound plays when the eye pops out
                    {
                        SoundEngine.PlaySound(SoundID.Zombie101 with { Volume = 0.3f, MaxInstances = 2 }, NPC.Center + new Vector2(80 * NPC.direction, 0));
                    }
                    NPC.velocity = NPC.velocity.RotatedBy(0.013f * NPC.direction * spinSpeed);
                    NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;
                }
            }
            else
            {
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

            float rate = 0.2f;
            if (phantasmalSpin)
                rate = 0.05f;
            InnerEyePositionUpdate(ref trueEyeVector, NPC.Center);

            void InnerEyePositionUpdate(ref Vector2 eyeVector, Vector2 basePosition)
            {
                bool eyeCenter = phantasmalSpin || target == null;

                if (eyeCenter)
                {
                    eyeVector = Vector2.Lerp(eyeVector, Vector2.Zero, rate);
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
            return ableToHit;
        }
        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            return canBeHit ? null : false;
        }

        public override bool CheckDead()
        {
            if (NPC.ai[2] < 0)
                return true;
            ableToHit = false;
            NPC.frameCounter += 0.2d;
            if (NPC.localAI[3] == 0)
            {
                modNPC.ignitedStacks.Clear();
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
                for (int i = 0; (double)i < hit.Damage * 0.01d; i++)
                {
                    int d = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Vortex, hit.HitDirection, -1f, 0, default, 0.5f);
                    Main.dust[d].noLight = true;
                    Main.dust[d].noLightEmittence = true;
                }
            }
        }
        public override void OnKill()
        {
            
        }
        public override void ModifyHoverBoundingBox(ref Rectangle boundingBox)
        {
            boundingBox = NPC.Hitbox;
        }
        public override void FindFrame(int frameHeight)
        {
            currentFrame = (int)NPC.frameCounter % 4;
            frameHeight = trueEyeTex.Height / 4;
            NPC.frame = new Rectangle(0, frameHeight * currentFrame, trueEyeTex.Width, frameHeight - 2);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.life > 1)
                return false;

            Vector2 eyeOrigin = new Vector2(NPC.frame.Width * 0.5f);
            Main.EntitySpriteDraw(trueEyeTex, NPC.Center - Main.screenPosition, NPC.frame, Color.White, NPC.rotation, eyeOrigin, NPC.scale, SpriteEffects.None);
            Main.EntitySpriteDraw(innerEyeTex, NPC.Center + trueEyeVector - Main.screenPosition, null, Color.White, NPC.rotation, innerEyeTex.Size() * 0.5f, NPC.scale, SpriteEffects.None);


            return false;
        }
    }
}
