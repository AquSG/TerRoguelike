using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerRoguelike.Managers;
using TerRoguelike.Systems;
using TerRoguelike.Player;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Renderers;

namespace TerRoguelike.Projectiles
{
    public class AdaptiveGunBullet : ModProjectile, ILocalizedModType
    {
        public bool ableToHit = true;
        public TerRoguelikeGlobalProjectile modProj;
        TerRoguelikePlayer modPlayer;
        public int setTimeLeft = 3000;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 60;
        }
        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.extraUpdates = 29;
            Projectile.timeLeft = setTimeLeft;
            Projectile.penetrate = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            modProj = Projectile.GetGlobalProjectile<TerRoguelikeGlobalProjectile>();
        }

        public override bool? CanDamage() => ableToHit ? (bool?)null : false;
        public override bool? CanHitNPC(NPC target)
        {
            if (Projectile.penetrate == 1)
                return false;

            return (bool?)null;
        }

        public override void AI()
        {
            if (modPlayer == null)
                modPlayer = Main.player[Projectile.owner].GetModPlayer<TerRoguelikePlayer>();

            if (modPlayer.heatSeekingChip > 0)
                modProj.HomingAI(Projectile, (float)Math.Log(modPlayer.heatSeekingChip + 1, 1.2d) / 25000f);

            if (modPlayer.bouncyBall > 0)
                modProj.extraBounces += modPlayer.bouncyBall;

            if (Projectile.timeLeft <= 60)
            {
                ableToHit = false;
                Projectile.velocity = Vector2.Zero;
                return;
            }
                
            Projectile.rotation = Projectile.velocity.ToRotation();
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D lightTexture = ModContent.Request<Texture2D>(Texture).Value;
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Color color = Color.Lerp(Color.Yellow, Color.White, (float)i / (Projectile.oldPos.Length / 2));
                Vector2 drawPosition = Projectile.oldPos[i] + (lightTexture.Size() * 0.5f) - Main.screenPosition;
                
                // Become smaller the futher along the old positions we are.
                Vector2 scale = new Vector2(1f) * MathHelper.Lerp(0.25f, 1f, 1f - i / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(lightTexture, drawPosition, null, color, Projectile.oldRot[i], lightTexture.Size() * 0.5f, scale, SpriteEffects.None, 0);
            }
            if (modPlayer != null)
            {
                if (modPlayer.volatileRocket > 0 && Projectile.velocity != Vector2.Zero)
                {
                    Texture2D rocketTexture = ModContent.Request<Texture2D>("TerRoguelike/Projectiles/VolatileRocket").Value;
                    Vector2 drawPosition = Projectile.Center - Main.screenPosition;
                    Main.EntitySpriteDraw(rocketTexture, drawPosition, null, Color.White, Projectile.velocity.ToRotation(), rocketTexture.Size() * 0.5f, 1f, SpriteEffects.None);
                }
            }
            return false;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Projectile.timeLeft = 60;
            ableToHit = false;
            Projectile.velocity = Vector2.Zero;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            modProj.bounceCount++;
            if (modProj.bounceCount >= 1 + modProj.extraBounces)
            {
                if (Projectile.timeLeft > 60)
                    Projectile.timeLeft = 60;
                ableToHit = false;
                Projectile.velocity = Vector2.Zero;

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
    }
}
