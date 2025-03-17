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
    public class AdaptiveDaggerHoldout : ModProjectile, ILocalizedModType
    {
        //This manages whatever happens when you hold down with adaptive dagger
        public override string Texture => "TerRoguelike/Projectiles/InvisibleProj";

        public ref float Charge => ref Projectile.ai[0];

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

            float pointingRotation = (modPlayer.mouseWorld - Owner.MountedCenter).ToRotation();
            Projectile.Center = Owner.MountedCenter + pointingRotation.ToRotationVector2() * 1f;

            if (Owner.channel) //Keep the player's hands full relative to attack speed
            {
                Projectile.timeLeft = 2;
                Owner.heldProj = Projectile.whoAmI;
                if (Owner.itemTime < 2)
                    Owner.itemTime = 2;
                if (Owner.itemAnimation < 2)
                    Owner.itemAnimation = 2;
            }
            else
            {

            }
            if (Charge >= 0f) // autorelease swing sword
            {
                ReleaseSword();
            }


            float chargeAmt = 1f * Owner.GetAttackSpeed(DamageClass.Generic);

            Charge += chargeAmt;
        }

        public void ReleaseSword()
        {
            if (modPlayer.mouseWorld.X > Owner.Center.X)
            {
                Owner.ChangeDir(1);
            }
            else if (modPlayer.mouseWorld.X <= Owner.Center.X)
            {
                Owner.ChangeDir(-1);
            }

            if (modPlayer.swingAnimCompletion <= 0 || modPlayer.playerToCursor == Vector2.Zero)
                modPlayer.playerToCursor = (modPlayer.mouseWorld - Owner.Center).SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f));
            float armPointingDirection = modPlayer.playerToCursor.ToRotation();
            Owner.SetCompositeArmFront(true, Owner.compositeFrontArm.stretch, armPointingDirection - MathHelper.PiOver2);

            if (Owner.channel) //Keep the player's hands full relative to attack speed
            {
                Owner.itemTime = (int)(12 / Owner.GetAttackSpeed(DamageClass.Generic)) + 1;
                if (Owner.itemTime < 2)
                    Owner.itemTime = 2;
                Owner.itemAnimation = (int)(12 / Owner.GetAttackSpeed(DamageClass.Generic)) + 1;
                if (Owner.itemAnimation < 2)
                    Owner.itemAnimation = 2;
            }
            else
            {

            }

            if (Owner.channel)
                modPlayer.swingAnimCompletion = 0.00001f; // start the swing anim

            int shotsToFire = Owner.ModPlayer().shotsToFire; //multishot support
            float damageBoost = 1;
            int damage = (int)(Projectile.damage * damageBoost);
            SoundEngine.PlaySound(SoundID.Item1 with { Volume = SoundID.Item41.Volume * 1f }, Owner.Center);
            for (int i = 0; i < shotsToFire; i++)
            {
                float baseRot = Owner.compositeFrontArm.rotation + MathHelper.PiOver2;
                float mainAngle;
                float spread = 40f;
                if (shotsToFire == 1)
                {
                    mainAngle = baseRot;
                }
                else if (shotsToFire % 2 == 0)
                {
                    mainAngle = baseRot - ((float)((shotsToFire - 1) * 2) * MathHelper.Pi/(spread * 4f)) + ((float)i * MathHelper.Pi/spread);
                }
                else
                {
                    mainAngle = baseRot - ((float)((shotsToFire - 1) / 2) * MathHelper.Pi/spread) + ((float)i * MathHelper.Pi / spread);
                }

                
                Vector2 direction = (mainAngle).ToRotationVector2();
                int spawnedProjectile = Projectile.NewProjectile(Projectile.GetSource_FromThis(), modPlayer.GetPositionRelativeToFrontHand(10) + (direction * 16f), Vector2.Zero, ModContent.ProjectileType<AdaptiveDaggerStab>(), damage, 1f, Owner.whoAmI);
                Projectile spawnedProj = Main.projectile[spawnedProjectile];
                spawnedProj.rotation = direction.ToRotation();
                spawnedProj.scale = modPlayer.scaleMultiplier;
                spawnedProj.ModProj().swingDirection = Owner.direction;
                spawnedProj.ModProj().notedBoostedDamage = damageBoost;
            }
            Charge -= 12f;
            if (Charge > 0f) // support for swinging more than once a frame if one has that much attack speed
            {
                ReleaseSword();
            }
        }
    }
}
