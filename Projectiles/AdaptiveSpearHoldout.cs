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

namespace TerRoguelike.Projectiles
{
    public class AdaptiveSpearHoldout : ModProjectile, ILocalizedModType
    {
        //This manages whatever happens when you hold down with adaptive spear
        public override string Texture => "TerRoguelike/Projectiles/InvisibleProj";

        public ref float Charge => ref Projectile.ai[0];

        public bool autoRelease = false;
        public Player Owner => Main.player[Projectile.owner];
        public TerRoguelikePlayer modPlayer;

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

            if (Charge >= 80f && !autoRelease) //cap chargetime if no autorelease
            {
                Charge = 80f;
                
            }

            float pointingRotation = (modPlayer.mouseWorld - Owner.MountedCenter).ToRotation();
            Projectile.Center = Owner.MountedCenter + pointingRotation.ToRotationVector2() * 16f;

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
            else // player released m1. swing it.
            {
                ReleaseSword();
                return;
            }
            if (autoRelease && Charge >= 80f) // autorelease swing sword
            {
                ReleaseSword();
            }
                
            
            if (Charge < 80 || autoRelease) // charge. autorelease allows overflowing of charge amount, leading to more than 1 swing a frame
            {
                float chargeAmt = 1f * Owner.GetAttackSpeed(DamageClass.Generic);
                if (chargeAmt >= 3f) // autorelease at 3x attack speed
                    autoRelease = true;
                else
                    autoRelease = false;

                Charge += chargeAmt;
                if (Charge >= 80f)
                {
                    SoundEngine.PlaySound(new SoundStyle("TerRoguelike/Sounds/Ding") with { Volume = 0.1f }, Owner.Center);
                    modPlayer.bladeFlashTime = 15;
                }
            }
        }

        public void ReleaseSword()
        {
            if ((Charge <= 80f || (Owner.channel && autoRelease)) && modPlayer.swingAnimCompletion == 0)
                modPlayer.swingAnimCompletion += 0.00001f; // start the swing anim

            int shotsToFire = Owner.ModPlayer().shotsToFire; //multishot support
            float damageBoost = Charge >= 80f ? 5f : (1 + (Charge / 80f * 3f));
            int damage = (int)(Projectile.damage * damageBoost);
            SoundEngine.PlaySound(SoundID.Item1 with { Volume = SoundID.Item41.Volume * 1f }, Owner.Center);

            float armPointingDirection = modPlayer.playerToCursor.ToRotation() + (MathHelper.PiOver2 * Owner.direction);
            Vector2 projSpawnPos = Owner.MountedCenter + armPointingDirection.ToRotationVector2() * 8 + modPlayer.playerToCursor * 22;
            for (int i = 0; i < shotsToFire; i++)
            {
                float mainAngle;
                float spread = 25f;
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

                if (Projectile.owner == Main.myPlayer)
                {
                    Vector2 direction = (mainAngle).ToRotationVector2();
                    int spawnedProjectile = Projectile.NewProjectile(Projectile.GetSource_FromThis(), projSpawnPos + (direction * 48f), direction, ModContent.ProjectileType<AdaptiveSpearStab>(), damage, 1f, Owner.whoAmI);
                    Projectile spawnedProj = Main.projectile[spawnedProjectile];
                    spawnedProj.scale = modPlayer.scaleMultiplier;
                    spawnedProj.ModProj().swingDirection = Owner.direction;
                    spawnedProj.ModProj().notedBoostedDamage = damageBoost;
                }
            }
            Charge -= 80f;
            if (Charge > 80f) // support for swinging more than once a frame if one has that much attack speed
            {
                ReleaseSword();
            }
        }
    }
}
