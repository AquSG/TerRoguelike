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

namespace TerRoguelike.NPCs.Enemy.Boss.Mallet.MalletProjectiles
{
    public class Meteor : BaseRoguelikeNPC, ILocalizedModType
    {
        public override string Texture => "TerRoguelike/NPCs/Enemy/Boss/Mallet/MalletProjectiles/MeteorBig";
        public bool BigMeteor => NPC.ai[0] == 0;
        public bool CollisionPass = false;
        public bool ableToHit => NPC.ai[1] == 0 && (NPC.Opacity >= 0.8f || deadTime > 0);
        public int deadTime = 0;
        public bool deathInitialized = false;
        public override void SetDefaults()
        {
            NPC.width = NPC.height = 90;
            NPC.friendly = false;
            NPC.noTileCollide = false;
            NPC.noGravity = true;
            NPC.lifeMax = 750;
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
            NPC.direction = (int)NPC.ai[1];
            if (NPC.direction == 0)
                NPC.direction = NPC.spriteDirection = Main.rand.NextBool() ? -1 : 1;
            NPC.ai[1] = 0;
            if (!BigMeteor)
            {
                Vector2 center = NPC.Center;
                NPC.width = NPC.height = 44;
                NPC.Center = center;
                NPC.velocity = new Vector2(4 * NPC.direction, -10);
                NPC.noTileCollide = true;
            }
            else
            {
                NPC.velocity = new Vector2(NPC.ai[2], NPC.ai[3]);
            }
            NPC.ai[2] = 0;
            NPC.ai[3] = 0;
            NPC.localAI[0] += 0.001f;
            NPC.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }
        public override void AI()
        {
            if (deadTime == 0)
                NPC.oldVelocity = NPC.velocity;
            if (NPC.collideX || NPC.collideY)
            {
                CheckDead();
                return;
            }
            if (NPC.localAI[0] == 0)
                NPC.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            float oldai = NPC.localAI[0];
            NPC.localAI[0] += NPC.velocity.Length() * 0.04f;
            if (BigMeteor)
                NPC.Opacity = MathHelper.Clamp(NPC.localAI[0] * 0.05f, 0, 1);
            if ((int)oldai != (int)NPC.localAI[0])
            {
                Color colorA = modNPC.hostileTurnedAlly ? Color.Cyan * 0.6f : Color.Yellow;
                Color colorB = modNPC.hostileTurnedAlly ? Color.Cyan * 0.6f : Color.Red;
                ParticleManager.AddParticle(new LerpBall(NPC.Center, Main.rand.NextVector2Circular(1.5f, 1.5f), 30, colorA * NPC.Opacity, colorB * NPC.Opacity, 30, new Vector2(1), 0, 0.96f, 15, false), ParticleManager.ParticleLayer.BehindTiles);
            }
            if (deadTime > 0)
            {
                CheckDead();
                if (deadTime > 0)
                    return;
            }
            NPC.immortal = NPC.dontTakeDamage = false;

            NPC.rotation += (BigMeteor ? 0.2f : 0.2f) * NPC.direction;
            if (!BigMeteor)
            {
                NPC.velocity.Y += 0.4f;
                if (NPC.velocity.Y > 20)
                    NPC.velocity.Y = 20;
                NPC.noTileCollide = NPC.localAI[0] < 10;
            }
            else
            {
                NPC.velocity.Y += 0.05f;
                NPC.velocity.X *= 0.994f;
            }
        }
        public override bool CheckDead()
        {
            if (modNPC.hostileTurnedAlly && modNPC.shouldHaveDied)
            {
                NPC.immortal = false;
                NPC.dontTakeDamage = false;
                if (NPC.oldVelocity != Vector2.Zero)
                    NPC.velocity = NPC.oldVelocity;
                NPC.netUpdate = true;
                deadTime = 0;
                modNPC.shouldHaveDied = false;
                NPC.ai[1] = 0;
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
                NPC.ai[1] = 1;
                modNPC.shouldHaveDied = !modNPC.hostileTurnedAlly;
                if (!TerRoguelike.mpClient && BigMeteor)
                {
                    for (int i = -1; i <= 1; i += 2)
                    {
                        NPC.NewNPCDirect(NPC.GetSource_FromThis(), NPC.Center, Type, 0, 1, i);
                    }
                }
            }

            if (!deathInitialized)
            {
                if (!CutsceneSystem.cutsceneActive)
                {
                    SoundEngine.PlaySound(Mallet.MeteorBreak with { Volume = 0.4f, MaxInstances = 3, Pitch = -0.1f, PitchVariance = 0.1f }, NPC.Center);
                    if (NPC.ai[1] == 0)
                    {
                        SoundEngine.PlaySound(Mallet.MeteorBoom with { Volume = 0.4f, MaxInstances = 3 }, NPC.Center);
                    }
                }
                Color colorA = modNPC.hostileTurnedAlly ? Color.Cyan : Color.Yellow;
                Color colorB = modNPC.hostileTurnedAlly ? Color.Cyan : Color.Red;
                for (int i = 0; i < 10; i++)
                {
                    int time = Main.rand.Next(15, 30);
                    Vector2 scale = new Vector2(Main.rand.NextFloat(0.25f, 0.5f));
                    if (BigMeteor)
                        scale *= 2;
                    ParticleManager.AddParticle(new LerpBall(NPC.Center, (i / 10f * MathHelper.TwoPi + Main.rand.NextFloat(-0.1f, 0.1f)).ToRotationVector2() * Main.rand.NextFloat(1f, 4f), time, colorA * NPC.Opacity, colorB * NPC.Opacity, time, scale, 0, 0.96f, 15, false), ParticleManager.ParticleLayer.BehindTiles);
                }
                NPC.netUpdate = true;
                deathInitialized = true;
            }

            NPC.velocity = Vector2.Zero;
            deadTime++;
            if (deadTime >= 60)
            {
                NPC.active = false;
            }
            return deadTime >= 60;
        }
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            Rectangle targetBox = target.Hitbox;

            if (deadTime == 0)
            {
                float radius = BigMeteor ? 46 : 22;
                if (targetBox.ClosestPointInRect(NPC.Center).Distance(NPC.Center) < radius)
                {
                    CollisionPass = ableToHit;
                    return CollisionPass;
                }
            }
            else if (NPC.ai[1] == 0)
            {
                float pulseCompletion = (deadTime / 60f);
                float opacity = Math.Min(1, (1 - pulseCompletion) * 3);
                if (opacity > 0.8f)
                {
                    float multiplier = BigMeteor ? 0.75f : 0.5f;
                    float pulseScale = pulseCompletion * multiplier;
                    float drawScale = 2000;
                    float pixelation = 0.0125f;
                    if (!BigMeteor)
                        pixelation *= 0.66f;
                    float radius = drawScale * pulseScale * 0.5f;
                    float radiusInner = radius - (drawScale * pixelation * 0.5f);

                    float thickness = radius - radiusInner;

                    float miniCircleRadius = thickness * 0.5f;
                    Vector2 circlePos = NPC.Center + (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * (radiusInner + miniCircleRadius);
                    if (targetBox.ClosestPointInRect(circlePos).Distance(circlePos) < miniCircleRadius) // project a mini circle at where the target would intersect the pulse wave at that rotation
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
            Rectangle targetBox = target.Hitbox;

            if (deadTime == 0)
            {
                float radius = BigMeteor ? 46 : 22;
                if (targetBox.ClosestPointInRect(NPC.Center).Distance(NPC.Center) < radius)
                {
                    CollisionPass = ableToHit;
                    return CollisionPass;
                }
            }
            else if (NPC.ai[1] == 0)
            {
                float pulseCompletion = (deadTime / 60f);
                float opacity = Math.Min(1, (1 - pulseCompletion) * 3);
                if (opacity > 0.8f)
                {
                    float multiplier = BigMeteor ? 0.75f : 0.5f;
                    float pulseScale = pulseCompletion * multiplier;
                    float drawScale = 2000;
                    float pixelation = 0.0125f;
                    if (!BigMeteor)
                        pixelation *= 0.66f;
                    float radius = drawScale * pulseScale * 0.5f;
                    float radiusInner = radius - (drawScale * pixelation * 0.5f);

                    float thickness = radius - radiusInner;

                    float miniCircleRadius = thickness * 0.5f;
                    Vector2 circlePos = NPC.Center + (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * (radiusInner + miniCircleRadius);
                    if (targetBox.ClosestPointInRect(circlePos).Distance(circlePos) < miniCircleRadius) // project a mini circle at where the target would intersect the pulse wave at that rotation
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
                SoundEngine.PlaySound(Mallet.BitHurt with { Volume = 0.5f, PitchVariance = 0.2f }, NPC.Center);
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (deadTime == 0)
            {
                var tex = BigMeteor ? TexDict["MeteorBig"] : TexDict["MeteorSmall"];

                Main.EntitySpriteDraw(tex, NPC.Center - screenPos, null, Color.White * NPC.Opacity, NPC.rotation, tex.Size() * 0.5f, NPC.scale, NPC.direction > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
            }
            else
            {
                int frameCount = deadTime / 5;
                if (frameCount < 4)
                {
                    var tex = TexDict["StarExplosionMedium"];
                    var frame = tex.Frame(1, 4, 0, frameCount);
                    Main.EntitySpriteDraw(tex, NPC.Center - screenPos, frame, Color.White * NPC.Opacity, 0, frame.Size() * 0.5f, NPC.scale, NPC.direction > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
                }
                if (NPC.ai[1] == 0)
                {
                    var pulseTex = TexDict["InvisibleProj"];
                    float pulseCompletion = (deadTime / 60f);

                    StartAlphaBlendSpritebatch();

                    float multiplier = BigMeteor ? 0.75f : 0.5f;
                    float pulseScale = pulseCompletion * multiplier;
                    float opacity = Math.Min(1, (1 - pulseCompletion) * 3);
                    float drawScale = 2000;
                    float finalMultiplier = 1f;
                    float pixelation = 0.0125f;
                    if (!BigMeteor)
                        pixelation *= 0.66f;
                    Color color = modNPC.hostileTurnedAlly ? Color.Cyan : Color.Lerp(Color.White, Color.Yellow, pulseCompletion * 0.7f);
                    if (modNPC.hostileTurnedAlly)
                        opacity *= 0.5f;

                    GameShaders.Misc["TerRoguelike:CircularPulse"].UseOpacity(opacity);
                    GameShaders.Misc["TerRoguelike:CircularPulse"].UseImage0(TextureAssets.Projectile[ModContent.ProjectileType<SandTurret>()]);
                    GameShaders.Misc["TerRoguelike:CircularPulse"].UseColor(color);
                    GameShaders.Misc["TerRoguelike:CircularPulse"].UseShaderSpecificData(new(pulseScale, 2 * finalMultiplier, pixelation / finalMultiplier, 0));

                    GameShaders.Misc["TerRoguelike:CircularPulse"].Apply();

                    Main.EntitySpriteDraw(pulseTex, NPC.Center - Main.screenPosition, null, Color.White, 0, pulseTex.Size() * 0.5f, drawScale * finalMultiplier, 0, 0);

                    StartVanillaSpritebatch();
                }
            }
            

            bool drawHitboxes = false;
            if (drawHitboxes)
            {
                var squaretex = TexDict["Square"];
                var squareorigin = squaretex.Size() * 0.5f;

                if (deadTime == 0)
                {
                    float radius = BigMeteor ? 46 : 22;
                    for (int i = 0; i < 100; i++)
                    {
                        float completion = i / 100f;
                        float rot = completion * MathHelper.TwoPi;
                        Main.EntitySpriteDraw(squaretex, NPC.Center + rot.ToRotationVector2() * radius - Main.screenPosition, null, Color.Red, 0, squareorigin, 1f, SpriteEffects.None);
                    }
                }
                else if (NPC.ai[1] == 0)
                {
                    float pulseCompletion = (deadTime / 60f);
                    float opacity = Math.Min(1, (1 - pulseCompletion) * 3);
                    if (opacity > 0.8f)
                    {
                        float multiplier = BigMeteor ? 0.75f : 0.5f;
                        float pulseScale = pulseCompletion * multiplier;
                        float drawScale = 2000;
                        float pixelation = 0.0125f;
                        if (!BigMeteor)
                            pixelation *= 0.66f;
                        float radius = drawScale * pulseScale * 0.5f;
                        float radiusInner = radius - (drawScale * pixelation * 0.5f);

                        float thickness = radius - radiusInner;

                        float miniCircleRadius = thickness * 0.5f;
                        Vector2 circlePos = NPC.Center + (Main.LocalPlayer.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * (radiusInner + miniCircleRadius);

                        for (int i = 0; i < 800; i++)
                        {
                            float completion = i / 100f;
                            float rot = completion * MathHelper.TwoPi;
                            Main.EntitySpriteDraw(squaretex, NPC.Center + rot.ToRotationVector2() * radius - Main.screenPosition, null, Color.Red, 0, squareorigin, 1f, SpriteEffects.None);

                            Main.EntitySpriteDraw(squaretex, NPC.Center + rot.ToRotationVector2() * radiusInner - Main.screenPosition, null, Color.Red, 0, squareorigin, 1f, SpriteEffects.None);

                            Main.EntitySpriteDraw(squaretex, circlePos + rot.ToRotationVector2() * miniCircleRadius - Main.screenPosition, null, Color.Red, 0, squareorigin, 1f, SpriteEffects.None);
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
            writer.Write(NPC.width);
            writer.Write(NPC.height);
            writer.Write(NPC.direction);
            writer.Write(NPC.noTileCollide);
            writer.Write(deadTime);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            NPC.width = reader.ReadInt32();
            NPC.height = reader.ReadInt32();
            NPC.direction = NPC.spriteDirection = reader.ReadInt32();
            NPC.noTileCollide = reader.ReadBoolean();
            deadTime = reader.ReadInt32();
        }
    }
}
