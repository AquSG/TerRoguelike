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
using TerRoguelike.Items.Common;
using TerRoguelike.Utilities;
using Terraria.DataStructures;
using TerRoguelike.Particles;
using static TerRoguelike.Managers.TextureManager;
using Steamworks;
using ReLogic.Utilities;
using static TerRoguelike.Utilities.TerRoguelikeUtils;

namespace TerRoguelike.NPCs.Enemy.Boss.Mallet.MalletProjectiles
{
    public class YellowWing : ModProjectile, ILocalizedModType
    {
        public int maxTimeLeft;
        public float startVel = 0;
        public override string Texture => "TerRoguelike/Projectiles/InvisibleProj";
        public Texture2D wingTex;
        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = maxTimeLeft = 300;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.manualDirectionChange = true;
            Projectile.netImportant = true; //makes projectile not able to be replaced if at the projectile cap 
            wingTex = TexDict["YellowWing"];
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.direction = Projectile.ai[0] < 0 ? -1 : 1;
            Projectile.ai[0] = 0;
            startVel = Projectile.velocity.Length();
            Projectile.velocity = Vector2.Zero;
        }
        public override void AI()
        {
            int time = maxTimeLeft - Projectile.timeLeft;

            Projectile.frame = Math.Min(3, Projectile.frameCounter / 6);
            Projectile.frameCounter++;
            if (Projectile.frame == 2)
                Projectile.frameCounter++;
            
            int movetime = 12;
            if (time > movetime)
            {
                float interpolant = MathHelper.Clamp(MathHelper.Lerp(0, 1, (time - movetime) / 30f), 0, 1);
                Projectile.velocity = Vector2.UnitY * startVel * interpolant;
            }
            else if (time == movetime)
            {
                SoundEngine.PlaySound(Mallet.TalonSwipe with { Volume = 0.7f }, Projectile.Center);

                int projCount = (int)Projectile.ai[1];
                float startAngle = MathHelper.PiOver2 * 0.08f;
                float spreadAngle = MathHelper.PiOver2 * 0.4f;
                float radius = 300;
                Vector2 basePos = Projectile.Center + new Vector2(0 * Projectile.direction, -radius + 48);
                for (int i = 0; i < projCount; i++)
                {
                    float completion = (float)i / Math.Max(projCount - 1, 1);
                    float thisAngle = (startAngle + spreadAngle * completion) * Projectile.direction + MathHelper.PiOver2;
                    Vector2 projPos = basePos + thisAngle.ToRotationVector2() * radius;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), projPos, thisAngle.ToRotationVector2() * startVel * 1.2f, ModContent.ProjectileType<SpeedingFeather>(), Projectile.damage, 0);
                }
            }

        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2[] cirlceArray = [
                new Vector2(40 * -Projectile.direction, 32),
                new Vector2(64 * -Projectile.direction, 16),
                new Vector2(96 * -Projectile.direction, 0),
                new Vector2(134 * -Projectile.direction, -20),
                new Vector2(170 * -Projectile.direction, -34),
                new Vector2(230 * -Projectile.direction, -34)
                ];
            for (int i = 0; i < cirlceArray.Length; i++)
            {
                Vector2 checkPos = Projectile.Center + cirlceArray[i];
                float radius = i == cirlceArray.Length - 1 ? 24 : 40;

                if (targetHitbox.ClosestPointInRect(checkPos).Distance(checkPos) < radius)
                    return true;
            }
            return false;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            var wingFrame = wingTex.Frame(1, 4, 0, Projectile.frame, 0, 0);

            StartNonPremultipliedSpritebatch();
            Main.EntitySpriteDraw(wingTex, drawPos, wingFrame, Color.White, Projectile.rotation, wingFrame.Size() * new Vector2(Projectile.direction < 0 ? 0 : 1, 0.5f), Projectile.scale, Projectile.direction > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            StartVanillaSpritebatch();

            bool drawHitboxes = false;
            if (drawHitboxes && CanDamage() != false)
            {
                var squareTex = TexDict["Square"];

                Vector2[] cirlceArray = [
                    new Vector2(40 * -Projectile.direction, 32),
                    new Vector2(64 * -Projectile.direction, 16),
                    new Vector2(96 * -Projectile.direction, 0),
                    new Vector2(134 * -Projectile.direction, -20),
                    new Vector2(170 * -Projectile.direction, -34),
                    new Vector2(230 * -Projectile.direction, -34)
                    ];
                for (int i = 0; i < cirlceArray.Length; i++)
                {
                    Vector2 checkPos = Projectile.Center + cirlceArray[i];
                    float radius = i == 0 || i == cirlceArray.Length - 1 ? 24 : 36;

                    for (int j = 0; j < 120; j++)
                    {
                        Main.EntitySpriteDraw(squareTex, checkPos - Main.screenPosition + (j * MathHelper.TwoPi / 120f).ToRotationVector2() * radius, null, Color.Red, 0, squareTex.Size() * 0.5f, 1f, SpriteEffects.None);
                    }
                }
            }
            
            return false;
        }
        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            Mallet.InflictRetribution(target);
        }
        public override bool? CanDamage() => Projectile.frame >= 2 ? null : false;
    }
}
