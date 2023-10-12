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

namespace TerRoguelike.Projectiles
{
    public class Missile : ModProjectile, ILocalizedModType
    {
        public override string Texture => "TerRoguelike/Projectiles/AdaptiveGunBullet";
        public float rotationOffset = 0;
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
            Projectile.DamageType = DamageClass.Generic;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = false;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.extraUpdates = 7;
            Projectile.timeLeft = setTimeLeft;
            Projectile.penetrate = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            modProj = Projectile.GetGlobalProjectile<TerRoguelikeGlobalProjectile>();
        }

        public override bool? CanDamage() => ableToHit ? (bool?)null : false;
        public override bool? CanHitNPC(NPC target)
        {
            //same situation as adaptive gun bullet. hits once but lives on for visuals.
            if (Projectile.penetrate == 1)
                return false;

            return (bool?)null;
        }

        public override void AI()
        {
            if (Projectile.localAI[1] == 0)
            {
                SoundEngine.PlaySound(new SoundStyle("TerRoguelike/Sounds/MissileLaunch", 4) with { Volume = 0.07f }, Projectile.Center);
            }
            Projectile.localAI[1]++;

            if (Projectile.timeLeft == 59)
            {
                SoundEngine.PlaySound(new SoundStyle("TerRoguelike/Sounds/MissileHit", 4) with { Volume = 0.3f }, Projectile.Center);
                for (int i = 0; i < 10; i++)
                {
                    Dust dust = Dust.NewDustDirect(Projectile.Center + new Vector2(-16, -16), 32, 32, DustID.YellowTorch);
                    dust.noLight = true;
                    dust.noLightEmittence = true;
                }
            }

            if (modPlayer == null)
                modPlayer = Main.player[Projectile.owner].GetModPlayer<TerRoguelikePlayer>();

            if (modProj.homingTarget == -1)
                modProj.homingTarget = (int)Projectile.ai[0];

            if (Projectile.localAI[1] > 90)
                modProj.HomingAI(Projectile, 0.001128f * 2.9f, true);
            else
            {
                // wiggle at the start
                rotationOffset += Main.rand.Next(-1, 2) * (MathHelper.Pi / 72f);
                Projectile.velocity = (Projectile.velocity.Length() * -Vector2.UnitY).RotatedBy(rotationOffset);
            }

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
                if (Projectile.timeLeft <= 60 && i + Projectile.timeLeft <= 60)
                    continue;

                Color color = Color.Lerp(Color.Yellow, Color.White, (float)i / (Projectile.oldPos.Length / 2));
                Vector2 drawPosition = Projectile.oldPos[i] + (lightTexture.Size() * 0.5f) - Main.screenPosition;
                
                // Become smaller the futher along the old positions we are.
                Vector2 scale = new Vector2(2.2f) * MathHelper.Lerp(0.25f, 1f, 1f - i / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(lightTexture, drawPosition, null, color, Projectile.oldRot[i], lightTexture.Size() * 0.5f, scale, SpriteEffects.None, 0);
            }
            if (Projectile.velocity != Vector2.Zero)
            {
                Texture2D rocketTexture = ModContent.Request<Texture2D>("TerRoguelike/Projectiles/Missile").Value;
                Vector2 drawPosition = Projectile.Center - Main.screenPosition;
                Main.EntitySpriteDraw(rocketTexture, drawPosition, null, Color.White, Projectile.velocity.ToRotation(), rocketTexture.Size() * 0.5f, 1f, SpriteEffects.None);
            }
            return false;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Projectile.timeLeft = 60;
            ableToHit = false;
            Projectile.velocity = Vector2.Zero;
        }
    }
}
