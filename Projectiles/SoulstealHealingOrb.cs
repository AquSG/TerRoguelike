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

namespace TerRoguelike.Projectiles
{
    public class SoulstealHealingOrb : ModProjectile, ILocalizedModType
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 3;
            Projectile.timeLeft = 2400;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (player.dead || !player.active)
            {
                Projectile.Kill();
                return;
            }
            Projectile.localAI[0]++;

            if (Projectile.timeLeft == 2)
                Projectile.position = player.Center; //if it somehow fucking hasn't reached the player yet, just teleport it on top of them for the heal.

            Vector2 playerVector = player.Center - Projectile.Center;
            float playerDist = playerVector.Length();
            if (playerDist < 50f && Projectile.position.X < player.position.X + player.width && Projectile.position.X + Projectile.width > player.position.X && Projectile.position.Y < player.position.Y + player.height && Projectile.position.Y + Projectile.height > player.position.Y)
            {
                // heal the killer
                TerRoguelikePlayer modPlayer = player.GetModPlayer<TerRoguelikePlayer>();

                int healAmt = (int)(player.statLifeMax2 * modPlayer.soulstealCoating * 0.1f);
                modPlayer.ScaleableHeal(healAmt);
                SoundEngine.PlaySound(new SoundStyle("TerRoguelike/Sounds/OrbHeal", 5) { Volume = 0.12f }, Projectile.Center);
                Projectile.Kill();
            }
            Projectile.velocity = ((playerVector.SafeNormalize(Vector2.UnitY) * 6f) + (player.velocity / 4)) * MathHelper.Lerp(0f, 1f, Projectile.localAI[0] / 240f);
        }

        public override bool? CanDamage() => false;

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D lightTexture = ModContent.Request<Texture2D>(Texture).Value;
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float colorInterpolation = (float)Math.Cos(Projectile.timeLeft / 32f + Main.GlobalTimeWrappedHourly / 20f + i / (float)Projectile.oldPos.Length * MathHelper.Pi) * 0.5f + 0.5f;
                Color color = Color.Lerp(Color.LightSeaGreen, Color.LimeGreen, colorInterpolation) * 0.4f;
                color.A = 0;
                Vector2 drawPosition = Projectile.oldPos[i] - Main.screenPosition + new Vector2(8f, 8f);
                Color outerColor = color;
                Color innerColor = color * 0.5f;
                float intensity = 0.9f + 0.15f * (float)Math.Cos(Main.GlobalTimeWrappedHourly % 60f * MathHelper.TwoPi);
                intensity *= MathHelper.Lerp(0.15f, 1f, 1f - i / (float)Projectile.oldPos.Length);
                if (Projectile.timeLeft <= 60) //Shrinks to nothing when projectile is nearing death
                {
                    intensity *= Projectile.timeLeft / 60f;
                }
                // Become smaller the futher along the old positions we are.
                Vector2 outerScale = new Vector2(1f) * intensity;
                Vector2 innerScale = new Vector2(1f) * intensity * 0.7f;
                outerColor *= intensity;
                innerColor *= intensity;
                Main.EntitySpriteDraw(lightTexture, drawPosition, null, outerColor, 0f, lightTexture.Size() * 0.5f, outerScale * 0.25f, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(lightTexture, drawPosition, null, innerColor, 0f, lightTexture.Size() * 0.5f, innerScale * 0.25f, SpriteEffects.None, 0);
            }
            return false;
        }
    }
}
