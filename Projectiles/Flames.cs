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
using Terraria.GameContent;

namespace TerRoguelike.Projectiles
{
    public class Flames : ModProjectile, ILocalizedModType
    {
        public int maxTimeLeft;
        public int fadeOutThreshold = 60;
        public int fadeOutTime = 12;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 7;
        }
        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = maxTimeLeft = 300;
            Projectile.penetrate = 4;
            Projectile.extraUpdates = 2;
            Projectile.alpha = 255;
            Projectile.ignoreWater = true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            
        }
        public override void AI()
        {
            //Vanilla flamethrower code, ported

            Projectile.localAI[0] += 1f;
            if (Projectile.localAI[0] > 30 && Projectile.ai[0] < fadeOutThreshold)
                Projectile.localAI[0] -= Projectile.ai[1];

            int totalTime = fadeOutThreshold + fadeOutTime;
            if (Projectile.localAI[0] >= totalTime)
            {
                Projectile.Kill();
            }
            if (Projectile.localAI[0] >= fadeOutThreshold)
            {
                Projectile.velocity *= 0.95f;
            }
            int dustSwitchThreshold = 50;
            if (Projectile.localAI[0] < dustSwitchThreshold && Main.rand.NextFloat() < 0.25f)
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(60f, 60f) * Utils.Remap(Projectile.localAI[0], 0f, 72f, 0.5f, 1f), 4, 4, 6, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 100);
                if (Main.rand.NextBool(4))
                {
                    dust.noGravity = true;
                    dust.scale *= 3f;
                    dust.velocity.X *= 2f;
                    dust.velocity.Y *= 2f;
                }
                else
                {
                    dust.scale *= 1.5f;
                }
                dust.scale *= 1.5f;
                dust.velocity *= 1.2f;
                dust.velocity += Projectile.velocity * 1f * Utils.Remap(Projectile.localAI[0], 0f, fadeOutThreshold * 0.75f, 1f, 0.1f) * Utils.Remap(Projectile.localAI[0], 0f, fadeOutThreshold * 0.1f, 0.1f, 1f);
                dust.customData = 1;
            }
            if (Projectile.localAI[0] >= dustSwitchThreshold && Main.rand.NextFloat() < 0.5f)
            {
                Vector2 offset = (Projectile.velocity).SafeNormalize(Vector2.Zero).RotatedByRandom(0.19634954631328583) * 7f;
                Dust dust = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(50f, 50f) - offset * 2f, 4, 4, 31, 0f, 0f, 150, new Color(80, 80, 80));
                dust.noGravity = true;
                dust.velocity = offset;
                dust.scale *= 1.1f + Main.rand.NextFloat() * 0.2f;
                dust.customData = -0.3f - 0.15f * Main.rand.NextFloat();
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.velocity = oldVelocity * 0.95f;
            Projectile.position -= Projectile.velocity;
            return false;
        }
        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            int inflateBy = (int)Utils.Remap(Projectile.localAI[0], 0f, 72f, 10f, 40f);
            hitbox.Inflate(inflateBy, inflateBy);
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (!projHitbox.Intersects(targetHitbox))
            {
                return false;
            }
            return Collision.CanHit(Projectile.Center, 0, 0, targetHitbox.Center.ToVector2(), 0, 0);
        }
        public override bool? CanDamage() => Projectile.timeLeft >= 18 ? null : false;
        public override bool PreDraw(ref Color lightColor)
        {
            var tex = TextureAssets.Projectile[Type].Value;

            // vanilla flamethrower proj drawing, ported
            float fromMax = fadeOutThreshold + fadeOutTime;
            Color color1 = new Color(255, 80, 20, 200);
            Color color2 = new Color(255, 255, 20, 70);
            Color color3 = Color.Lerp(new Color(255, 80, 20, 100), color2, 0.25f);
            Color color4 = new Color(80, 80, 80, 100);
            float opacity1 = 0.35f;
            float opacity2 = 0.7f;
            float opacity3 = 0.85f;
            float opacity4 = ((Projectile.localAI[0] > fadeOutThreshold - 10f) ? 0.175f : 0.2f);
            float opacity5 = Utils.Remap(Projectile.localAI[0], fadeOutThreshold, fromMax, 1f, 0f);
            float threshold1 = Math.Min(Projectile.localAI[0], 20f);
            float lerp1 = Utils.Remap(Projectile.localAI[0], 0f, fromMax, 0f, 1f);
            float lerp2 = Utils.Remap(lerp1, 0.2f, 0.5f, 0.25f, 1f);
            Rectangle frame = tex.Frame(1, Main.projFrames[Type], 0, 3);
            for (int i = 0; i < 2; i++)
            {
                for (float j = 1f; j >= 0f; j -= opacity4)
                {
                    Color chosenColor = ((lerp1 < 0.1f) ? Color.Lerp(Color.Transparent, color1, Utils.GetLerpValue(0f, 0.1f, lerp1, clamped: true)) : ((lerp1 < 0.2f) ? Color.Lerp(color1, color2, Utils.GetLerpValue(0.1f, 0.2f, lerp1, clamped: true)) : ((lerp1 < opacity1) ? color2 : ((lerp1 < opacity2) ? Color.Lerp(color2, color3, Utils.GetLerpValue(opacity1, opacity2, lerp1, clamped: true)) : ((lerp1 < opacity3) ? Color.Lerp(color3, color4, Utils.GetLerpValue(opacity2, opacity3, lerp1, clamped: true)) : ((!(lerp1 < 1f)) ? Color.Transparent : Color.Lerp(color4, Color.Transparent, Utils.GetLerpValue(opacity3, 1f, lerp1, clamped: true))))))));
                    float colorInterpolant = (1f - j) * Utils.Remap(lerp1, 0f, 0.2f, 0f, 1f);
                    Vector2 vector = Projectile.Center - Main.screenPosition + Projectile.velocity * (0f - threshold1) * j;
                    Color preFinalColor = chosenColor * colorInterpolant;
                    Color finalColor = preFinalColor;
                    finalColor.G = (byte)(finalColor.G / 2);
                    finalColor.B = (byte)(finalColor.B / 2);
                    finalColor.A = (byte)Math.Min(preFinalColor.A + 80f * colorInterpolant, 255f);
                    Utils.Remap(Projectile.localAI[0], 20f, fromMax, 0f, 1f);
                    
                    float rotationMagnitude = 1f / opacity4 * (j + 1f);
                    float rotation1 = Projectile.rotation + j * ((float)Math.PI / 2f) + Main.GlobalTimeWrappedHourly * rotationMagnitude * 2f;
                    float rotation2 = Projectile.rotation - j * ((float)Math.PI / 2f) - Main.GlobalTimeWrappedHourly * rotationMagnitude * 2f;
                    switch (i)
                    {
                        case 0:
                            Main.EntitySpriteDraw(tex, vector + Projectile.velocity * (0f - threshold1) * opacity4 * 0.5f, frame, finalColor * opacity5 * 0.25f, rotation1 + (float)Math.PI / 4f, frame.Size() / 2f, lerp2, SpriteEffects.None);
                            Main.EntitySpriteDraw(tex, vector, frame, finalColor * opacity5, rotation2, frame.Size() * 0.5f, lerp2, SpriteEffects.None);
                            break;
                        case 1:
                            Main.EntitySpriteDraw(tex, vector + Projectile.velocity * (0f - threshold1) * opacity4 * 0.2f, frame, preFinalColor * opacity5 * 0.25f, rotation1 + (float)Math.PI / 2f, frame.Size() / 2f, lerp2 * 0.75f, SpriteEffects.None);
                            Main.EntitySpriteDraw(tex, vector, frame, preFinalColor * opacity5, rotation2 + (float)Math.PI / 2f, frame.Size() * 0.5f, lerp2 * 0.75f, SpriteEffects.None);
                            break;
                    }
                }
            }

            return false;
        }
    }
}
