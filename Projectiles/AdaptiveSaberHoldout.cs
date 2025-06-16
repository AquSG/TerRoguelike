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
using TerRoguelike.Packets;
using Terraria.GameContent;

namespace TerRoguelike.Projectiles
{
    public class AdaptiveSaberHoldout : ModProjectile, ILocalizedModType
    {
        public ref float Charge => ref Projectile.ai[0];

        public Player Owner => Main.player[Projectile.owner];
        public TerRoguelikePlayer modPlayer;
        public int swingDirection = -1;
        public bool released = false;
        public SwordColor swordLevel
        {
            get { return (SwordColor)Projectile.ai[1]; }
            set { Projectile.ai[1] = (int)value; }
        }
        public enum SwordColor
        {
            Purple,
            Blue,
            Green,
            Yellow,
            Orange,
            Red,
            Rainbow,
        }
        public float rainbowProg = 0;
        public bool thrown = false;

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
            Projectile.netSpam = 0;
            if (Projectile.localAI[1] != Projectile.ai[1] && Projectile.owner == Main.myPlayer)
            {
                if (swordLevel == SwordColor.Rainbow)
                {
                    SoundEngine.PlaySound(new SoundStyle("TerRoguelike/Sounds/Ding") with { Volume = 0.1f, Pitch = -0.32f }, Owner.Center);
                }
                
                    SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact with { Volume = 0.5f + Projectile.ai[1] * 0.05f, Pitch = Projectile.ai[1] * 0.04f }, Projectile.Center);
            }
            Projectile.localAI[1] = Projectile.ai[1];
            if (swordLevel == SwordColor.Rainbow)
                rainbowProg += 0.0154936875f;
            if (modPlayer == null)
                modPlayer = Owner.ModPlayer();

            float pointingRotation = (modPlayer.mouseWorld - Owner.MountedCenter).ToRotation();
            Projectile.Center = Owner.MountedCenter + pointingRotation.ToRotationVector2() * 16f;

            if (Owner.channel && !released) //Keep the player's hands full relative to attack speed
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
                released = true;
                if (Owner.itemAnimation == 1 || Owner.itemTime == 1)
                {
                    ThrowSword();
                }
                else
                {
                    Projectile.timeLeft = 2;
                }
                return;
            }
            if (Charge >= 0f) // autorelease swing sword
            {
                ReleaseSword();
            }
                
            
            if (Charge < 0) // charge. autorelease allows overflowing of charge amount, leading to more than 1 swing a frame
            {
                float chargeAmt = 1f * Owner.GetAttackSpeed(DamageClass.Generic);

                Charge += chargeAmt;
            }
        }

        public void ReleaseSword()
        {
            swingDirection *= -1;
            modPlayer.swingAnimCompletion = 0.00001f * swingDirection; // start the swing anim
            TerPlayerPacket.cooldown -= 8;
            Owner.itemTime = (int)(20 / Owner.GetAttackSpeed(DamageClass.Generic)) + 1;
            if (Owner.itemTime < 2)
                Owner.itemTime = 2;
            Owner.itemAnimation = (int)(20 / Owner.GetAttackSpeed(DamageClass.Generic)) + 1;
            if (Owner.itemAnimation < 2)
                Owner.itemAnimation = 2;
            if (modPlayer.mouseWorld.X > Owner.Center.X)
            {
                Owner.ChangeDir(1);
            }
            else if (modPlayer.mouseWorld.X <= Owner.Center.X)
            {
                Owner.ChangeDir(-1);
            }
            modPlayer.playerToCursor = (modPlayer.mouseWorld - Owner.Center).SafeNormalize(Vector2.UnitX);
            float anim = modPlayer.swingAnimCompletion;
            anim = MathHelper.SmoothStep(0, 1, anim);
            float armPointingDirection = (modPlayer.playerToCursor.ToRotation() - (MathHelper.Pi * Owner.direction / 3f));
            armPointingDirection += MathHelper.Lerp(0f, MathHelper.TwoPi * 9f / 16f, swingDirection == 1 ? anim : 1 - anim) * Owner.direction;
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armPointingDirection - MathHelper.PiOver2);

            int shotsToFire = Owner.ModPlayer().shotsToFire; //multishot support
            float damageBoost = 1f + Math.Min((int)swordLevel, 5) * 0.05f;
            if (swordLevel == SwordColor.Rainbow)
                damageBoost += 0.12f;

            int damage = (int)(Projectile.damage * damageBoost);
            SoundEngine.PlaySound(SoundID.Item15 with { Volume = 1f, MaxInstances = 6, SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest }, Owner.Center);
            for (int i = 0; i < shotsToFire; i++)
            {
                float mainAngle;
                float spread = 10f;
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
                    int spawnedProjectile = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Owner.MountedCenter + (direction * 16f), direction, ModContent.ProjectileType<AdaptiveSaberSlash>(), damage, 1f, Owner.whoAmI, (int)swordLevel, rainbowProg);
                    Projectile spawnedProj = Main.projectile[spawnedProjectile];
                    spawnedProj.scale = modPlayer.scaleMultiplier;
                    spawnedProj.ModProj().swingDirection = Owner.direction;
                    spawnedProj.ModProj().notedBoostedDamage = damageBoost;
                }
            }
            Charge -= 20f;
            if (Charge > 0f) // support for swinging more than once a frame if one has that much attack speed
            {
                ReleaseSword();
            }
        }
        public void ThrowSword()
        {
            thrown = true;
            modPlayer.swingAnimCompletion = 0.00001f; // start the swing anim
            TerPlayerPacket.cooldown -= 8;
            Owner.itemTime = (int)(20 / Owner.GetAttackSpeed(DamageClass.Generic)) + 1;
            if (Owner.itemTime < 2)
                Owner.itemTime = 2;
            Owner.itemAnimation = (int)(20 / Owner.GetAttackSpeed(DamageClass.Generic)) + 1;
            if (Owner.itemAnimation < 2)
                Owner.itemAnimation = 2;
            if (modPlayer.mouseWorld.X > Owner.Center.X)
            {
                Owner.ChangeDir(1);
            }
            else if (modPlayer.mouseWorld.X <= Owner.Center.X)
            {
                Owner.ChangeDir(-1);
            }

            int shotsToFire = Owner.ModPlayer().shotsToFire; //multishot support
            float damageBoost = 1f + Math.Min((int)swordLevel, 5) * 0.75f;
            if (swordLevel == SwordColor.Rainbow)
                damageBoost = 9;

            int damage = (int)(Projectile.damage * damageBoost);
            SoundEngine.PlaySound(SoundID.Item1 with { Volume = SoundID.Item41.Volume * 1f, Pitch = -0.5f }, Owner.Center);
            for (int i = 0; i < shotsToFire; i++)
            {
                float mainAngle;
                float spread = 20f;
                if (shotsToFire == 1)
                {
                    mainAngle = (Projectile.Center - Owner.MountedCenter).ToRotation();
                }
                else if (shotsToFire % 2 == 0)
                {
                    mainAngle = (Projectile.Center - Owner.MountedCenter).ToRotation() - ((float)((shotsToFire - 1) * 2) * MathHelper.Pi / (spread * 4f)) + ((float)i * MathHelper.Pi / spread);
                }
                else
                {
                    mainAngle = (Projectile.Center - Owner.MountedCenter).ToRotation() - ((float)((shotsToFire - 1) / 2) * MathHelper.Pi / spread) + ((float)i * MathHelper.Pi / spread);
                }


                if (Projectile.owner == Main.myPlayer)
                {
                    Vector2 direction = (mainAngle).ToRotationVector2();
                    int spawnedProjectile = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Owner.MountedCenter + (direction * 16f), direction * 8, ModContent.ProjectileType<AdaptiveSaberThrow>(), damage, 1f, Owner.whoAmI, modPlayer.scaleMultiplier, (int)swordLevel, rainbowProg);
                    Projectile spawnedProj = Main.projectile[spawnedProjectile];
                    spawnedProj.ModProj().notedBoostedDamage = damageBoost;
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (thrown)
                return false;
            var tex = TextureAssets.Projectile[Type].Value;
            var frame = tex.Frame(1, 2, 0, 0, 0, -2);
            Vector2 origin = frame.Size() * 0.5f;

            Color light = Lighting.GetColor(Projectile.Center.ToTileCoordinates());
            float rotation = Owner.compositeFrontArm.rotation + MathHelper.PiOver2;
            if (Owner.direction < 0)
                rotation += MathHelper.Pi;

            Vector2 basePos = Owner.GetFrontHandPosition(Owner.compositeFrontArm.stretch, Owner.compositeFrontArm.rotation).Floor() + new Vector2(frame.Width * 0.5f * Owner.direction, -frame.Height * 0.5f).RotatedBy(rotation);
            basePos.Y += Owner.gfxOffY;
            Main.EntitySpriteDraw(tex, basePos - Main.screenPosition, frame, light, rotation, origin, 1f, Owner.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);


            frame = tex.Frame(1, 2, 0, 1, 0, -2);
            Color color = GetSwordColor(swordLevel, rainbowProg);
            Main.EntitySpriteDraw(tex, basePos - Main.screenPosition, frame, color, rotation, origin, 1f, Owner.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);

            return false;
        }
        public static Color GetSwordColor(SwordColor color, float rainbowProg = 0)
        {
            Color thisColor = color switch
            {
                SwordColor.Purple => Color.Purple,
                SwordColor.Blue => Color.Blue,
                SwordColor.Green => Color.Green,
                SwordColor.Yellow => Color.Yellow,
                SwordColor.Orange => Color.OrangeRed,
                SwordColor.Red => Color.Red,
                _ => RainbowColor(rainbowProg),
            };
            thisColor.R = (byte)Math.Min(255, thisColor.R + 50);
            thisColor.G = (byte)Math.Min(255, thisColor.G + 50);
            thisColor.B = (byte)Math.Min(255, thisColor.B + 50);
            return thisColor;
        }
        public static Color RainbowColor(float progress)
        {
            float third = 1 / 3f;
            float r = 0;
            float g = 0;
            float b = 0;

            float colorPoints = 1;
            progress %= 1;
            if (progress < third)
            {
                r += colorPoints - (1 - (progress / third));
                g += colorPoints - r;
            }
            else if (progress < third * 2)
            {
                g += colorPoints - (1 - ((progress - third) / third));
                b += colorPoints - g;
            }
            else
            {
                b += colorPoints - (1 - ((progress - (third * 2)) / third));
                r += colorPoints - b;
            }

            return new Color(r, g, b);
        }
    }
}
