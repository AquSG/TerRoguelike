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

namespace TerRoguelike.Projectiles
{
    public class AdaptiveGunHoldout : ModProjectile, ILocalizedModType
    {
        //This manages whatever happens when you hold down with adaptive gun
        public override string Texture => "TerRoguelike/Projectiles/InvisibleProj";

        public ref float Charge => ref Projectile.ai[0];

        public Player Owner => Main.player[Projectile.owner];
        public TerRoguelikePlayer modPlayer;


        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
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
            {
                modPlayer = Owner.GetModPlayer<TerRoguelikePlayer>();
            }

            if (Owner.channel) //Keep the player's hands full relative to attack speed
            {
                Projectile.timeLeft = 2;
                Owner.itemTime = (int)(20 / Owner.GetAttackSpeed(DamageClass.Generic));
                if (Owner.itemTime < 2)
                    Owner.itemTime = 2;
                Owner.itemAnimation = (int)(20 / Owner.GetAttackSpeed(DamageClass.Generic));
                if (Owner.itemAnimation < 2)
                    Owner.itemAnimation = 2;
                Owner.heldProj = Projectile.whoAmI;
            }

            float pointingRotation = (Main.MouseWorld - Owner.MountedCenter).ToRotation();
            Projectile.Center = Owner.MountedCenter + pointingRotation.ToRotationVector2() * 40f;

            
            if (Charge > 0f) // attack when charge is full. scales great with attack speed
            {
                ShootBullet();
            }

            Charge += 1f * Owner.GetAttackSpeed(DamageClass.Generic); //increase charge relative to attack speed
        }

        public void ShootBullet()
        {
            float distance = Collision.CanHit(Owner.MountedCenter, 1, 1, Projectile.Center, 1, 1) ? 30f : 5f;
            int shotsToFire = Owner.GetModPlayer<TerRoguelikePlayer>().shotsToFire; //multishot support
            SoundEngine.PlaySound(SoundID.Item41 with { Volume = SoundID.Item41.Volume * 0.6f });
            for (int i = 0; i < shotsToFire; i++)
            {
                float mainAngle;
                float spread = 64f;
                if (shotsToFire == 1)
                {
                    mainAngle = (Projectile.Center - Owner.MountedCenter).ToRotation();
                }
                else if (shotsToFire % 2 == 0)
                {
                    mainAngle = (Projectile.Center - Owner.MountedCenter).ToRotation() - ((float)((shotsToFire - 1) * 2) * MathHelper.Pi/(spread * 4f)) + ((float)i * MathHelper.Pi/spread);
                }
                else
                {
                    mainAngle = (Projectile.Center - Owner.MountedCenter).ToRotation() - ((float)((shotsToFire - 1) / 2) * MathHelper.Pi/spread) + ((float)i * MathHelper.Pi / spread);
                }

                
                Vector2 direction = (mainAngle).ToRotationVector2();
                int spawnedProjectile = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Owner.MountedCenter + (direction * distance), direction * 1.5f, ModContent.ProjectileType<AdaptiveGunBullet>(), Projectile.damage, 1f, Owner.whoAmI);
                Main.projectile[spawnedProjectile].scale = modPlayer.scaleMultiplier;
            }
            Charge -= 20f;
            if (Charge > 0f) // if the player has enough attack speed to shoot more than once a frame, allow it.
                ShootBullet();

        }
    }
}
