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
using System.Reflection;

namespace TerRoguelike.Projectiles
{
    public class AdaptiveBladeHoldout : ModProjectile, ILocalizedModType
    {
        public override string Texture => "TerRoguelike/Projectiles/InvisibleProj";

        public ref float Charge => ref Projectile.ai[0];

        public Terraria.Player Owner => Main.player[Projectile.owner];
        public TerRoguelikePlayer modPlayer;

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
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
                modPlayer = Owner.GetModPlayer<TerRoguelikePlayer>();

            if (Charge >= 60)
            {
                Charge = 60;
                
            }

            if (Owner.channel)
            {
                Projectile.timeLeft = 2;
                Owner.itemTime = (int)(20 / Owner.GetAttackSpeed(DamageClass.Generic)) + 1;
                if (Owner.itemTime < 2)
                    Owner.itemTime = 2;
                Owner.itemAnimation = (int)(20 / Owner.GetAttackSpeed(DamageClass.Generic)) + 1;
                if (Owner.itemAnimation < 2)
                    Owner.itemAnimation = 2;
                Owner.heldProj = Projectile.whoAmI;
            }
            else
                ReleaseSword();

            float pointingRotation = (Main.MouseWorld - Owner.MountedCenter).ToRotation();
            Projectile.Center = Owner.MountedCenter + pointingRotation.ToRotationVector2() * 16f;

            if (Charge < 60)
            {
                Charge += 1f * Owner.GetAttackSpeed(DamageClass.Generic);
                if (Charge >= 60)
                    SoundEngine.PlaySound(new SoundStyle("TerRoguelike/Sounds/Ding") with { Volume = 0.12f }, Owner.Center);
            }
                
        }

        public void ReleaseSword()
        {
            modPlayer.swingAnimCompletion += 0.00001f;
            int shotsToFire = Owner.GetModPlayer<TerRoguelikePlayer>().shotsToFire;
            int damage = Charge == 60 ? (int)(Projectile.damage * 4f) : (int)(Projectile.damage * (1 + (Charge / 60f * 3f)));
            SoundEngine.PlaySound(SoundID.Item1 with { Volume = SoundID.Item41.Volume * 1f });
            for (int i = 0; i < shotsToFire; i++)
            {
                float mainAngle;
                float spread = 6f;
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
                int spawnedProjectile = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Owner.MountedCenter + (direction * 16f), Vector2.Zero, ModContent.ProjectileType<AdaptiveBladeSlash>(), damage, 1f, Owner.whoAmI);
                Main.projectile[spawnedProjectile].rotation = direction.ToRotation();
                Main.projectile[spawnedProjectile].GetGlobalProjectile<TerRoguelikeGlobalProjectile>().swingDirection = Owner.direction;
            }
        }
    }
}
