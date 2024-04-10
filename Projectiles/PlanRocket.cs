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
using TerRoguelike.Particles;

namespace TerRoguelike.Projectiles
{
    public class PlanRocket : ModProjectile, ILocalizedModType
    {
        //This is basically the same as missile but changed for different visuals and allowing wiggling in all directions.
        //also gets killed on room clear
        public override string Texture => "TerRoguelike/Projectiles/AdaptiveGunBullet";
        public float rotationOffset = 0;
        public bool ableToHit = true;
        public TerRoguelikeGlobalProjectile modProj;
        TerRoguelikePlayer modPlayer;
        public int setTimeLeft = 6300;
        public Vector2 originalDirection;
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
            modProj = Projectile.ModProj();
        }

        public override bool? CanDamage() => ableToHit && Projectile.localAI[1] >= 300 ? (bool?)null : false;
        public override bool? CanHitNPC(NPC target)
        {
            if (Projectile.penetrate == 1)
                return false;

            return (bool?)null;
        }

        public override void AI()
        {
            if (Projectile.localAI[1] == 0)
            {
                originalDirection = Projectile.velocity.SafeNormalize(-Vector2.UnitY);
                SoundEngine.PlaySound(new SoundStyle("TerRoguelike/Sounds/MissileLaunch", 4) with { Volume = 0.15f }, Projectile.Center);
                for (int i = 0; i < 5; i++)
                {
                    ParticleManager.AddParticle(new Square(Projectile.Center + (Projectile.velocity * 4), -Projectile.velocity.RotatedBy(Main.rand.NextFloat(-0.4f, 0.4f)) * Main.rand.NextFloat(2, 3), 20, Color.HotPink, new Vector2(0.8f), Projectile.velocity.ToRotation(), 0.9f, 20, true));
                }
            }
            if (Projectile.timeLeft == 59)
            {
                SoundEngine.PlaySound(new SoundStyle("TerRoguelike/Sounds/MissileHit", 4) with { Volume = 0.45f }, Projectile.Center);
                for (int i = 0; i < 10; i++)
                {
                    Dust dust = Dust.NewDustDirect(Projectile.Center + new Vector2(-16, -16), 32, 32, DustID.PinkTorch);
                    dust.noLight = true;
                    dust.noLightEmittence = true;
                }
            }
            Projectile.localAI[1]++;

            if (modPlayer == null)
                modPlayer = Main.player[Projectile.owner].GetModPlayer<TerRoguelikePlayer>();

            if (modProj.homingTarget == -1 && Projectile.ai[0] != -1)
                modProj.homingTarget = (int)Projectile.ai[0];

            if (Projectile.localAI[1] > 300)
                modProj.HomingAI(Projectile, 0.001128f * 2.9f, true);
            else
            {
                rotationOffset += Main.rand.Next(-1, 2) * (MathHelper.Pi / 72f);
                Projectile.velocity = (Projectile.velocity.Length() * originalDirection).RotatedBy(rotationOffset);
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
            Texture2D lightTexture = TextureAssets.Projectile[Type].Value; ;
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.timeLeft <= 60 && i + Projectile.timeLeft <= 60)
                    continue;

                Color color = Color.Lerp(Color.DeepPink, Color.White, (float)i / (Projectile.oldPos.Length / 2));
                Vector2 drawPosition = Projectile.oldPos[i] + (lightTexture.Size() * 0.5f) - Main.screenPosition;
                
                // Become smaller the futher along the old positions we are.
                Vector2 scale = new Vector2(1.3f) * MathHelper.Lerp(0.25f, 1f, 1f - i / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(lightTexture, drawPosition, null, color, Projectile.oldRot[i], lightTexture.Size() * 0.5f, scale, SpriteEffects.None, 0);
            }
            if (Projectile.velocity != Vector2.Zero)
            {
                Texture2D rocketTexture = TexDict["PlanRocket"].Value;
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
