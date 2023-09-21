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
    public class AdaptiveGunHoldout : ModProjectile, ILocalizedModType
    {
        public override string Texture => "TerRoguelike/Projectiles/InvisibleProj";

        public ref float Charge => ref Projectile.ai[0];

        public Terraria.Player Owner => Main.player[Projectile.owner];



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
            if (Owner.channel)
            {
                Projectile.timeLeft = 2;
                Owner.itemTime = (int)(20 / Owner.GetAttackSpeed(DamageClass.Generic));
                Owner.itemAnimation = (int)(20 / Owner.GetAttackSpeed(DamageClass.Generic));
                Owner.heldProj = Projectile.whoAmI;
            }

            float pointingRotation = (Main.MouseWorld - Owner.MountedCenter).ToRotation();
            Projectile.Center = Owner.MountedCenter + pointingRotation.ToRotationVector2() * 40f;

            
            if (Charge > 0f)
            {
                SoundEngine.PlaySound(SoundID.Item41 with { Volume = SoundID.Item41.Volume * 0.6f });
                float mainAngle = (Projectile.Center - Owner.MountedCenter).ToRotation();
                Vector2 direction = (mainAngle).ToRotationVector2();
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Owner.MountedCenter + (direction * 30f), direction * 1.5f, ModContent.ProjectileType<AdaptiveGunBullet>(), Projectile.damage, 1f, Owner.whoAmI);
                Charge = -20f;
            }

            Charge += 1f * Owner.GetAttackSpeed(DamageClass.Generic);
        }
    }
}
