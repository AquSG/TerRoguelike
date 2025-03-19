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
using Terraria.Graphics.Renderers;
using Terraria.GameContent;
using static TerRoguelike.Managers.TextureManager;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using Terraria.DataStructures;
using TerRoguelike.Particles;
using Terraria.Graphics.Shaders;
using System.IO;

namespace TerRoguelike.Projectiles
{
    public class AdaptiveCannonBlast : ModProjectile, ILocalizedModType
    {
        public bool ableToHit = true;
        public TerRoguelikeGlobalProjectile modProj;
        TerRoguelikePlayer modPlayer;
        public int setTimeLeft = 780;
        public int explodeTimeLeft = 20;
        public static readonly SoundStyle CannonLaunch = new("TerRoguelike/Sounds/CannonLaunch");
        public static readonly SoundStyle CannonBoom = new("TerRoguelike/Sounds/CannonBoom");
        Texture2D squareTex;

        public override void SetStaticDefaults()
        {

        }
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.timeLeft = setTimeLeft;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            modProj = Projectile.ModProj();
            squareTex = TexDict["Square"];
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation += Main.rand.NextFloat(MathHelper.TwoPi);

            Projectile.scale = Projectile.ai[0];
            if (false)
            {
                //scale support
                Projectile.position = Projectile.Center + new Vector2(-12 * Projectile.scale, -12 * Projectile.scale);
                Projectile.width = (int)(24 * Projectile.scale);
                Projectile.height = (int)(24 * Projectile.scale);
            }

            modPlayer = Main.player[Projectile.owner].ModPlayer();
            if (CollidingVector(Projectile.position, new Vector2(Projectile.width, Projectile.height)) != null)
            {
                Projectile.ai[1] = 1;
                Projectile.timeLeft = explodeTimeLeft;
                Projectile.velocity = Vector2.Zero;
                Projectile.tileCollide = false;
            }
        }
        public override void AI()
        {
            Projectile.scale = Projectile.ai[0];
            modPlayer = Main.player[Projectile.owner].ModPlayer();

            if (Projectile.ai[1] == 0 && modPlayer.heatSeekingChip > 0)
                modProj.HomingAI(Projectile, (float)Math.Log(modPlayer.heatSeekingChip + 1, 1.2d) / (5600f));

            if (modPlayer.bouncyBall > 0 || modPlayer.trash > 0)
                modProj.extraBounces += modPlayer.bouncyBall + modPlayer.trash;

            Projectile.rotation += 0.2f * Projectile.direction;

            if (Projectile.timeLeft == 1 && Projectile.ai[1] == 0)
            {
                Projectile.timeLeft = explodeTimeLeft;
                Projectile.ai[1] = 1;
                Projectile.velocity = Vector2.Zero;
                Projectile.tileCollide = false;
            }

            if (Projectile.ai[1] == 0)  
            {
                float sparkRot = Projectile.rotation + MathHelper.PiOver2 * Projectile.direction;
                for (int i = 0; i < 3; i++)
                {
                    ParticleManager.AddParticle(new ThinSpark(Projectile.Center + sparkRot.ToRotationVector2() * 16 * Projectile.scale, (sparkRot + Main.rand.NextFloat(-0.5f, 0.5f)).ToRotationVector2() * 2 + Projectile.velocity, 20, Color.Lerp(Color.Red, Color.Yellow, Main.rand.NextFloat()), new Vector2(0.03f) * Projectile.scale, sparkRot - 0.4f * Projectile.direction, false, false));
                }
            }
            else
            {
                if (Projectile.timeLeft == explodeTimeLeft - 1)
                {
                    SoundEngine.PlaySound(CannonBoom with { Volume = 0.34f, PitchVariance = 0.25f, Pitch = 0.1f }, Projectile.Center);

                    for (int i = 0; i < 32; i++)
                    {
                        float completion = i / 32f;
                        float rot = MathHelper.TwoPi * completion + Main.rand.NextFloat(-0.04f, 0.04f);
                        Color color = Color.Lerp(Color.Red, Color.Yellow, Main.rand.NextFloat(0.5f));
                        ParticleManager.AddParticle(new ThinSpark(
                            Projectile.Center + rot.ToRotationVector2() * 20, rot.ToRotationVector2() * 6.6f * Main.rand.NextFloat(0.3f, 1f) * Projectile.scale,
                            34, color * 0.9f, new Vector2(0.13f, 0.27f) * Main.rand.NextFloat(0.7f, 1f) * 0.6f, rot, true, false));
                        if (i % 3 == 0)
                        {
                            ParticleManager.AddParticle(new Snow(Projectile.Center, rot.ToRotationVector2() * 5 * Main.rand.NextFloat(0.3f, 1f), 60, color, new Vector2(0.04f)));
                        }
                    }

                    int baseCount = (int)(30 * Projectile.scale);
                    for (int i = 0; i < baseCount; i++)
                    {
                        float completion = i / (float)baseCount;
                        float particleRot = completion * MathHelper.TwoPi + Main.rand.NextFloat(-0.8f, 0.8f);
                        ParticleManager.AddParticle(new Square(Projectile.Center + particleRot.ToRotationVector2() * 104 * Projectile.scale, Projectile.scale * (Main.rand.NextVector2Circular(2, 2) + particleRot.ToRotationVector2() * -1), 
                            18, Color.Lerp(Color.Red, Color.Yellow, Main.rand.NextFloat()) * 0.5f, new Vector2(Main.rand.NextFloat(0.6f, 1f)), particleRot, 0.95f, 15, false));
                    }
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.ai[1] == 0)
            {
                Texture2D bombTex = TextureAssets.Projectile[Type].Value;
                Main.EntitySpriteDraw(bombTex, Projectile.Center - Main.screenPosition, null, Lighting.GetColor(Projectile.Center.ToTileCoordinates()), Projectile.rotation, bombTex.Size() * 0.5f, Projectile.scale, Projectile.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.FlipVertically);
            }
            else
            {
                
            }

            if (false)
            {
                float radius = 12;
                if (Projectile.ai[1] != 0)
                    radius = 104;
                radius *= Projectile.scale;
                Vector2 hitboxPos = Projectile.Center;

                for (int j = 0; j < 120; j++)
                {
                    Main.EntitySpriteDraw(squareTex, hitboxPos + ((j / 120f * MathHelper.TwoPi).ToRotationVector2() * radius) - Main.screenPosition, null, Color.Red, 0, squareTex.Size() * 0.5f, 1f, SpriteEffects.None);
                }
            }
            return false;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Projectile.owner < 0)
                return null;

            float radius = 12;
            if (Projectile.ai[1] != 0)
            {
                if (Projectile.timeLeft < explodeTimeLeft - 6)
                    return false;
                radius = 104;
            }
            radius *= Projectile.scale;
            Vector2 hitboxPos = Projectile.Center;

            if (targetHitbox.ClosestPointInRect(hitboxPos).Distance(hitboxPos) <= radius)
                return true;

            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.ai[1] == 0)
            {
                Projectile.timeLeft = explodeTimeLeft;
                Projectile.ai[1] = 1;
                Projectile.velocity = Vector2.Zero;
                Projectile.tileCollide = false;
                Projectile.netUpdate = true;
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            modProj.bounceCount++;
            if (Projectile.ai[1] != 0)
                return false;

            if (modProj.bounceCount >= 1 + modProj.extraBounces)
            {
                modProj.bounceCount--;
                Projectile.velocity = Vector2.Zero;
                Projectile.ai[1] = 1;
                Projectile.timeLeft = explodeTimeLeft;
                Projectile.tileCollide = false;
            }
            else
            {
                // If the projectile hits the left or right side of the tile, reverse the X velocity
                if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
                {
                    Projectile.velocity.X = -oldVelocity.X;
                    Projectile.timeLeft = setTimeLeft;
                }
                // If the projectile hits the top or bottom side of the tile, reverse the Y velocity
                if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
                {
                    Projectile.velocity.Y = -oldVelocity.Y;
                    Projectile.timeLeft = setTimeLeft;
                }
            }
            return false;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Projectile.penetrate);
            writer.Write(Projectile.tileCollide);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.penetrate = reader.ReadInt32();
            Projectile.tileCollide = reader.ReadBoolean();
        }
    }
}
