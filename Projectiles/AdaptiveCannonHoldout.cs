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
using TerRoguelike.Particles;

namespace TerRoguelike.Projectiles
{
    public class AdaptiveCannonHoldout : ModProjectile, ILocalizedModType
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
                modPlayer = Owner.ModPlayer();
            }

            if (Owner.channel) //Keep the player's hands full relative to attack speed
            {
                Projectile.timeLeft = 2;
                Owner.itemTime = (int)(60 / Owner.GetAttackSpeed(DamageClass.Generic));
                if (Owner.itemTime < 2)
                    Owner.itemTime = 2;
                Owner.itemAnimation = (int)(60 / Owner.GetAttackSpeed(DamageClass.Generic));
                if (Owner.itemAnimation < 2)
                    Owner.itemAnimation = 2;
                Owner.heldProj = Projectile.whoAmI;
            }

            float pointingRotation = (AimWorld() - Owner.MountedCenter).ToRotation();
            Projectile.Center = Owner.MountedCenter + pointingRotation.ToRotationVector2() * 50f;


            if (Charge > 0f) // attack when charge is full. scales great with attack speed
            {
                ShootBullet();
                float shootingRot = (Projectile.Center - Owner.MountedCenter).ToRotation();
            }

            Charge += 1f * Owner.GetAttackSpeed(DamageClass.Generic); //increase charge relative to attack speed
        }

        public void ShootBullet()
        {
            float distance = Collision.CanHit(Owner.MountedCenter, 1, 1, Projectile.Center, 1, 1) ? 40f : 5f;
            int shotsToFire = Owner.ModPlayer().shotsToFire; //multishot support
            SoundEngine.PlaySound(AdaptiveCannonBlast.CannonLaunch with { Volume = 0.3f, PitchVariance = 0.3f }, Projectile.Center);
            
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
                if (modPlayer.shotgunComponent > 0)
                {
                    mainAngle += Main.rand.NextFloat(-MathHelper.Pi * 0.01f, MathHelper.Pi * 0.01f + float.Epsilon);
                }
                if (modPlayer.minigunComponent > 0)
                {
                    mainAngle += Main.rand.NextFloat(-MathHelper.Pi * 0.025f, MathHelper.Pi * 0.025f + float.Epsilon);
                }
                
                Vector2 direction = (mainAngle).ToRotationVector2();
                int spawnedProjectile = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Owner.MountedCenter + (direction * distance) + (Vector2.UnitY * Owner.gfxOffY), direction * 6, ModContent.ProjectileType<AdaptiveCannonBlast>(), Projectile.damage, 1f, Owner.whoAmI, modPlayer.scaleMultiplier);
            }
            Charge -= 60f;
            if (Charge > 0f) // if the player has enough attack speed to shoot more than once a frame, allow it.
                ShootBullet();

        }
    }
}
