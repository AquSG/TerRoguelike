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
using System.Reflection;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using Terraria.Graphics.Effects;

namespace TerRoguelike.Projectiles
{
    public class AdaptiveRifleHoldout : ModProjectile, ILocalizedModType
    {
        //This manages whatever happens when you hold down with adaptive blade
        public override string Texture => "TerRoguelike/Projectiles/InvisibleProj";

        public float wantedCharge = 40;
        public ref float Charge => ref Projectile.ai[0];

        public bool autoRelease = false;
        public Player Owner => Main.player[Projectile.owner];
        public TerRoguelikePlayer modPlayer;
        public float ChargeSpread
        {
            get
            {
                float spread = (float)Math.Pow(1 - (Charge / wantedCharge), 2) * 0.75f;
                if (modPlayer != null)
                {
                    if (modPlayer.minigunComponent > 0)
                        spread += MathHelper.Pi * 0.0125f;
                    spread += (modPlayer.shotsToFire - 1) * (MathHelper.PiOver4 / 64f);
                }
                return Math.Min(spread, MathHelper.Pi);
            }
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.netImportant = true; //makes projectile not able to be replaced if at the projectile cap 
        }

        public override bool? CanDamage()
        {
            return false;
        }

        public override bool ShouldUpdatePosition()
        {
            return false;
        }

        public override void AI()
        {
            if (modPlayer == null)
                modPlayer = Owner.ModPlayer();

            if (Charge >= wantedCharge && !autoRelease) //cap chargetime if no autorelease
            {
                Charge = wantedCharge;
            }

            float pointingRotation = (AimWorld() - Owner.MountedCenter).ToRotation();
            Projectile.rotation = pointingRotation;
            Projectile.Center = Owner.MountedCenter + pointingRotation.ToRotationVector2() * 35f;

            if (Owner.channel) //Keep the player's hands full relative to attack speed
            {
                Projectile.timeLeft = 2;
                Owner.itemTime = (int)(20 / Owner.GetAttackSpeed(DamageClass.Generic)) + 2;
                if (Owner.itemTime < 2)
                    Owner.itemTime = 2;
                Owner.itemAnimation = (int)(20 / Owner.GetAttackSpeed(DamageClass.Generic)) + 2;
                if (Owner.itemAnimation < 2)
                    Owner.itemAnimation = 2;
                Owner.heldProj = Projectile.whoAmI;
            }
            else // player released m1. shoot.
            {
                ShootBullet();
                return;
            }
            if (autoRelease && Charge >= wantedCharge) // autorelease shoot
            {
                ShootBullet();
            }
                
            
            if (Charge < wantedCharge || autoRelease) // charge. autorelease allows overflowing of charge amount, leading to more than 1 shot a frame
            {
                float chargeAmt = 1f * Owner.GetAttackSpeed(DamageClass.Generic);
                if (chargeAmt >= 2f) // autorelease at 3x attack speed
                    autoRelease = true;
                else
                    autoRelease = false;

                Charge += chargeAmt;
            }
        }

        public void ShootBullet()
        {
            if ((Charge <= wantedCharge || (Owner.channel && autoRelease)) && modPlayer.swingAnimCompletion == 0)
                modPlayer.swingAnimCompletion += 0.00001f; // start the shoot anim

            float distance = Collision.CanHit(Owner.MountedCenter, 1, 1, Projectile.Center, 1, 1) ? 35f : 5f;
            int shotsToFire = Owner.ModPlayer().shotsToFire; //multishot support

            SoundEngine.PlaySound(SoundID.Item40 with { Volume = SoundID.Item40.Volume * 0.6f }, Owner.Center);

            float chargeSpread = ChargeSpread;
            for (int i = 0; i < shotsToFire; i++)
            {
                float baseAngle = (Projectile.Center - Owner.MountedCenter).ToRotation() + Main.rand.NextFloat(-chargeSpread, chargeSpread);

                Vector2 direction = baseAngle.ToRotationVector2();
                int spawnedProjectile = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Owner.MountedCenter + ((Projectile.Center - Owner.MountedCenter).SafeNormalize(Vector2.UnitY) * distance) + (Vector2.UnitY * Owner.gfxOffY), direction * 11.25f, ModContent.ProjectileType<AdaptiveGunBullet>(), Projectile.damage, 1f, Owner.whoAmI, modPlayer.scaleMultiplier);
            }
            /*
            float baseAngle = (Projectile.Center - Owner.MountedCenter).ToRotation() + Main.rand.NextFloat(-chargeSpread, chargeSpread);
            for (int i = 0; i < shotsToFire; i++)
            {
                float baseAngle = (Projectile.Center - Owner.MountedCenter).ToRotation() + Main.rand.NextFloat(-chargeSpread, chargeSpread);
                float mainAngle;
                float spread = 64f;
                if (shotsToFire == 1)
                {
                    mainAngle = baseAngle;
                }
                else if (shotsToFire % 2 == 0)
                {
                    mainAngle = baseAngle - ((float)((shotsToFire - 1) * 2) * MathHelper.Pi / (spread * 4f)) + ((float)i * MathHelper.Pi / spread);
                }
                else
                {
                    mainAngle = baseAngle - ((float)((shotsToFire - 1) / 2) * MathHelper.Pi / spread) + ((float)i * MathHelper.Pi / spread);
                }
                if (modPlayer.shotgunComponent > 0)
                {
                    mainAngle += Main.rand.NextFloat(-MathHelper.Pi * 0.01f, MathHelper.Pi * 0.01f + float.Epsilon);
                }
                if (modPlayer.minigunComponent > 0)
                {
                    mainAngle += Main.rand.NextFloat(-MathHelper.Pi * 0.025f, MathHelper.Pi * 0.025f + float.Epsilon);
                }

                Vector2 direction = (mainAngle).ToRotationVector2();
                int spawnedProjectile = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Owner.MountedCenter + ((Projectile.Center - Owner.MountedCenter).SafeNormalize(Vector2.UnitY) * distance) + (Vector2.UnitY * Owner.gfxOffY), direction * 11.25f, ModContent.ProjectileType<AdaptiveGunBullet>(), Projectile.damage, 1f, Owner.whoAmI, modPlayer.scaleMultiplier);
            }
            */

            Charge -= wantedCharge;
            if (Charge > wantedCharge) // support for swinging more than once a frame if one has that much attack speed
            {
                ShootBullet();
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            float baseRot = Projectile.rotation;
            float opacity = MathHelper.Lerp(0.3f, 1, Charge / wantedCharge);

            if (ChargeSpread < MathHelper.Pi)
            {
                Main.spriteBatch.End();
                Effect fadeEffect = Filters.Scene["TerRoguelike:ConeFade"].GetShader().Shader;
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, fadeEffect, Main.GameViewMatrix.TransformationMatrix);

                fadeEffect.Parameters["tint"].SetValue((Color.White * 0.7f * opacity).ToVector4());
                fadeEffect.Parameters["fadeTint"].SetValue(Color.Transparent.ToVector4());
                fadeEffect.Parameters["fadeCutoff"].SetValue(0.6f);
                fadeEffect.Parameters["halfCone"].SetValue(ChargeSpread);
                fadeEffect.Parameters["coneFadeStrength"].SetValue(0.99990f);

                Texture2D tex = TextureManager.TexDict["Square"];
                Main.EntitySpriteDraw(tex, Projectile.Center + Owner.gfxOffY * Vector2.UnitY - Main.screenPosition, null, Color.White, baseRot, tex.Size() * 0.5f, 300, SpriteEffects.None);

                StartAdditiveSpritebatch();

                var lineTex = TextureManager.TexDict["LerpLineGradient"];
                float spread = ChargeSpread;
                for (int i = -1; i <= 1; i += 2)
                {
                    Main.EntitySpriteDraw(lineTex, Projectile.Center + Owner.gfxOffY * Vector2.UnitY - Main.screenPosition, null, Color.White * 0.50f * opacity, baseRot + spread * i, lineTex.Size() * new Vector2(0, 0.5f), new Vector2(0.7f, 0.35f), SpriteEffects.None);
                }
            }

            StartAlphaBlendSpritebatch();
            return false;
        }
    }
}
