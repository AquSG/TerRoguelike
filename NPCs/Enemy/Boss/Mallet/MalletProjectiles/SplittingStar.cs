using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerRoguelike.Managers;
using TerRoguelike.Systems;
using TerRoguelike.TerPlayer;
using Microsoft.Xna.Framework.Graphics;
using TerRoguelike.Items.Common;
using TerRoguelike.Utilities;
using Terraria.DataStructures;
using TerRoguelike.Particles;
using static TerRoguelike.Managers.TextureManager;
using Steamworks;
using ReLogic.Utilities;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using Terraria.GameContent.UI.States;
using Terraria.ModLoader.IO;
using System.Diagnostics;
using System.Timers;
using System.Security.Policy;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using TerRoguelike.Projectiles;
using System.Runtime;

namespace TerRoguelike.NPCs.Enemy.Boss.Mallet.MalletProjectiles
{
    public class SplittingStar : BaseRoguelikeNPC, ILocalizedModType
    {
        public override string Texture => "TerRoguelike/NPCs/Enemy/Boss/Mallet/MalletProjectiles/StarBig";
        public bool CollisionPass = false;
        public bool ableToHit => deadTime == 0 && NPC.Opacity >= 0.8f;
        public int deadTime = 0;
        public bool deathInitialized = false;
        public StarType myStarType => (StarType)NPC.ai[0];
        public bool syncStart = false;
        public enum StarType
        {
            Big,
            Medium,
            Small
        }
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 2;
            NPCID.Sets.NoMultiplayerSmoothingByType[Type] = true;
        }
        public override void SetDefaults()
        {
            NPC.width = NPC.height = 170;
            NPC.friendly = false;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            NPC.lifeMax = 1500;
            NPC.knockBackResist = 0;
            modNPC.IgnoreRoomWallCollision = true;
            NPC.damage = 36;
            modNPC.OverrideIgniteVisual = true;
        }
        public override void DrawBehind(int index)
        {
            
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.Center = NPC.Bottom;
            if (NPC.ai[0] > 2)
            {
                NPC.active = false;
                return;
            }
            if (myStarType == StarType.Medium)
            {
                Vector2 center = NPC.Center;
                NPC.lifeMax /= 2;
                NPC.width = NPC.height = 110;
                syncStart = true;
                NPC.Center = center;
            }
            else if (myStarType == StarType.Small)
            {
                Vector2 center = NPC.Center;
                NPC.lifeMax /= 4;
                NPC.width = NPC.height = 60;
                syncStart = true;
                NPC.Center = center;
            }
            else
            {
                NPC.Opacity = 0;
            }
            NPC.velocity = NPC.ai[1].ToRotationVector2() * NPC.ai[2];
            FindFrame(182);
        }
        public override void AI()
        {
            NPC.frameCounter += 0.1d;
            float oldai = NPC.localAI[0];
            NPC.localAI[0] += NPC.velocity.Length() * 0.04f;
            if (myStarType == StarType.Big)
            {
                NPC.Opacity = MathHelper.Clamp((float)NPC.frameCounter / 4, 0, 1);
            }

            NPC.localAI[2]++;
            if (deadTime > 0 || NPC.localAI[2] > 720)
            {
                CheckDead();
                if (deadTime > 0)
                    return;
            }
            NPC.immortal = NPC.dontTakeDamage = false;

            if ((int)oldai != (int)NPC.localAI[0])
            {
                float particleScale = 2f;
                if (myStarType == StarType.Medium)
                    particleScale *= 0.7f;
                else if (myStarType == StarType.Small)
                    particleScale *= 0.5f;
                Vector2 particlePos = NPC.Center + Main.rand.NextVector2Circular(NPC.width * 0.2f, NPC.height * 0.2f);
                Vector2 particleVel = Main.rand.NextVector2Circular(3, 3) + NPC.velocity * 0.3f;
                Color particleColor = modNPC.hostileTurnedAlly ? Color.Cyan : Color.Yellow;
                for (int j = 0; j < 2; j++)
                {
                    for (int i = 0; i < 1; i++)
                        ParticleManager.AddParticle(new ThinSpark(particlePos, particleVel, 40, particleColor, new Vector2(0.05f, 0.035f) * particleScale, MathHelper.PiOver2 * j, true, false), ParticleManager.ParticleLayer.BehindTiles);
                }
            }
            if (NPC.localAI[1] > 0)
            {
                NPC.localAI[1] -= 0.2f;
                if (NPC.localAI[1] < 0)
                    NPC.localAI[1] = 0;
            }

            var target = modNPC.GetTarget(NPC);

            Vector2 targetPos = target == null ? NPC.Center + NPC.velocity : target.Center;
            NPC.velocity *= 0.98f;
            NPC.velocity += (targetPos - NPC.Center).SafeNormalize(Vector2.UnitY) * NPC.ai[2] * 0.025f;
            if (NPC.velocity.Length() > NPC.ai[2])
                NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * NPC.ai[2];
            NPC.velocity = NPC.velocity.ToRotation().AngleTowards((targetPos - NPC.Center).ToRotation(), 0.02f).ToRotationVector2() * NPC.velocity.Length();
        }
        public override bool CheckDead()
        {
            if (modNPC.hostileTurnedAlly && modNPC.shouldHaveDied)
            {
                NPC.immortal = false;
                NPC.dontTakeDamage = false;
                NPC.velocity = NPC.oldVelocity;
                NPC.netUpdate = true;
                deadTime = 0;
                modNPC.shouldHaveDied = false;
                deathInitialized = false;
                return false;
            }

            NPC.immortal = true;
            NPC.dontTakeDamage = true;
            modNPC.CleanseDebuffs();
            modNPC.OverrideIgniteVisual = true;
            if (NPC.life <= 0)
            {
                NPC.active = true;
                NPC.life = 1;
                modNPC.shouldHaveDied = !modNPC.hostileTurnedAlly;
                if (!TerRoguelike.mpClient && myStarType != StarType.Small)
                {
                    var target = modNPC.GetTarget(NPC);
                    float shootRot = target == null ? NPC.velocity.ToRotation() : (target.Center - NPC.Center).ToRotation();
                    for (int i = -1; i <= 1; i += 2)
                    {
                        NPC.NewNPCDirect(NPC.GetSource_FromThis(), NPC.Center, Type, 0, NPC.ai[0] + 1, shootRot + MathHelper.PiOver2 * i, NPC.ai[2] * 1.2f);
                    }
                }
            }

            if (!deathInitialized && !CutsceneSystem.cutsceneActive)
            {
                
                SoundEngine.PlaySound(Mallet.MeteorBoom with { Volume = 0.6f, MaxInstances = 3, Pitch = myStarType == StarType.Small ? 0.4f : 0 }, NPC.Center);
                NPC.netUpdate = true;
                deathInitialized = true;
            }

            NPC.velocity = Vector2.Zero;
            deadTime++;
            if (deadTime >= 30)
            {
                NPC.active = false;
            }
            return deadTime >= 30;
        }
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            if (deadTime == 0)
            {
                var targetBox = target.Hitbox;
                float radius = NPC.width * 0.4f;
                float squish = myStarType == StarType.Big ? 7 : 5; // target distance is multiplied to effectively simulate oval shape
                for (int i = 0; i < 2; i++)
                {
                    var closestPoint = targetBox.ClosestPointInRect(NPC.Center);
                    var squishVect = (squish - 1) * (closestPoint - NPC.Center);
                    if (i == 0)
                        closestPoint.X += squishVect.X;
                    else
                        closestPoint.Y += squishVect.Y;

                    if (closestPoint.Distance(NPC.Center) < radius)
                    {
                        CollisionPass = ableToHit;
                        return CollisionPass;
                    }
                }
            }
            

            CollisionPass = false;
            return false;
        }
        public override bool CanHitNPC(NPC target)
        {
            if (deadTime == 0)
            {
                var targetBox = target.Hitbox;
                float radius = NPC.width * 0.4f;
                float squish = myStarType == StarType.Big ? 7 : 5; // target distance is multiplied to effectively simulate oval shape
                for (int i = 0; i < 2; i++)
                {
                    var closestPoint = targetBox.ClosestPointInRect(NPC.Center);
                    var squishVect = (squish - 1) * (closestPoint - NPC.Center);
                    if (i == 0)
                        closestPoint.X += squishVect.X;
                    else
                        closestPoint.Y += squishVect.Y;

                    if (closestPoint.Distance(NPC.Center) < radius)
                    {
                        CollisionPass = ableToHit;
                        return CollisionPass;
                    }
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
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0 && deadTime <= 1 && NPC.soundDelay <= 0 && !CutsceneSystem.cutsceneActive)
            {
                NPC.soundDelay = Main.rand.Next(3, 7);
                if (!modNPC.hostileTurnedAlly)
                    SoundEngine.PlaySound(Mallet.BitHurt with { Volume = 0.5f, PitchVariance = 0.2f }, NPC.Center);
                NPC.localAI[1] = 1;
            }
        }
        public override void FindFrame(int frameHeight)
        {
            if (Main.dedServ)
                return;
            Texture2D tex;
            switch (myStarType)
            {
                default:
                case StarType.Big:
                    tex = TexDict["StarBig"];
                    modNPC.drawCenter = new Vector2(-2, 0);
                    break;
                case StarType.Medium:
                    tex = TexDict["StarMedium"];
                    modNPC.drawCenter = new Vector2(3, 0);
                    break;
                case StarType.Small:
                    tex = TexDict["StarSmall"];
                    modNPC.drawCenter = new Vector2(0);
                    break;
            }
            NPC.frame = tex.Frame(1, 2, 0, (int)NPC.frameCounter % 2);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (deadTime == 0)
            {
                Texture2D tex = myStarType switch
                {
                    StarType.Medium => TexDict["StarMedium"],
                    StarType.Small => TexDict["StarSmall"],
                    _ => TexDict["StarBig"],
                };

                if (NPC.localAI[1] > 0)
                {
                    StartAlphaBlendSpritebatch();
                    Vector3 colorHSL = Main.rgbToHsl(Color.White * NPC.Opacity);

                    GameShaders.Misc["TerRoguelike:BasicTint"].UseOpacity(1f);
                    GameShaders.Misc["TerRoguelike:BasicTint"].UseColor(Main.hslToRgb(1 - colorHSL.X, colorHSL.Y, colorHSL.Z));
                    GameShaders.Misc["TerRoguelike:BasicTint"].Apply();
                }
                Main.EntitySpriteDraw(tex, NPC.Center + modNPC.drawCenter - screenPos, NPC.frame, Color.White * NPC.Opacity, NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.direction > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
                if (NPC.localAI[1] > 0)
                    StartVanillaSpritebatch();
            }
            else
            {
                int frameCount = deadTime / 5;
                int maxFrame = myStarType == StarType.Small ? 3 : 4;
                if (frameCount < maxFrame)
                {
                    var tex = myStarType == StarType.Small ? TexDict["StarExplosionSmall"] : TexDict["StarExplosionMedium"];
                    var frame = tex.Frame(1, maxFrame, 0, frameCount);
                    Main.EntitySpriteDraw(tex, NPC.Center + modNPC.drawCenter - screenPos, frame, Color.White, 0, frame.Size() * 0.5f, NPC.scale, NPC.direction > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
                }
            }
            

            bool drawHitboxes = false;
            if (drawHitboxes)
            {
                var squaretex = TexDict["Square"];
                var squareorigin = squaretex.Size() * 0.5f;

                if (deadTime == 0)
                {
                    float radius = NPC.width * 0.4f;
                    float squish = myStarType == StarType.Big ? 7 : 5; // target distance is multiplied to effectively simulate oval shape
                    for (int j = 0; j < 2; j++)
                    {
                        for (int i = 0; i < 100; i++)
                        {
                            float completion = i / 100f;
                            float rot = completion * MathHelper.TwoPi;
                            Vector2 rotVect = rot.ToRotationVector2() * radius;
                            if (j == 0)
                                rotVect.X /= squish;
                            else
                                rotVect.Y /= squish;
                            Main.EntitySpriteDraw(squaretex, NPC.Center + rotVect - Main.screenPosition, null, Color.Red, 0, squareorigin, 1f, SpriteEffects.None);
                        }
                    }
                }
            }

            return false;
        }
        public override bool? CanFallThroughPlatforms() => true;
        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            Mallet.InflictRetribution(target);
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(deadTime);
            writer.Write(syncStart);
            writer.Write(deathInitialized);
            if (syncStart)
            {
                syncStart = false;
                writer.Write(NPC.lifeMax);
                writer.Write(NPC.width);
                writer.Write(NPC.height);
            }
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            deadTime = reader.ReadInt32();
            bool startsync = reader.ReadBoolean();
            deathInitialized = reader.ReadBoolean();
            if (startsync)
            {
                NPC.lifeMax = NPC.life = reader.ReadInt32();
                NPC.width = reader.ReadInt32();
                NPC.height = reader.ReadInt32();
            }
        }
    }
}
