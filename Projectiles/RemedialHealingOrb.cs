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
using Terraria.GameContent;

namespace TerRoguelike.Projectiles
{
    public class RemedialHealingOrb : ModProjectile, ILocalizedModType
    {
        public override string Texture => "TerRoguelike/Projectiles/SoulstealHealingOrb";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
        }
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.MaxUpdates = 4;
            Projectile.timeLeft = 300 * Projectile.MaxUpdates;
        }

        public override void AI()
        {
            Projectile.velocity *= 0.97f;
            TerRoguelikePlayer ownerModPlayer = Main.player[Projectile.owner].GetModPlayer<TerRoguelikePlayer>();
            Projectile.ai[0]++;
            if (Projectile.numUpdates != -1 || Projectile.ai[0] < 30 * Projectile.MaxUpdates || Projectile.timeLeft < 30 * Projectile.MaxUpdates)
                return;

            Rectangle projRect = Projectile.getRect();
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];

                if (player == null || !player.active || player.dead) // herobrine touched my bungus
                    continue;

                if (player.getRect().Intersects(projRect))
                {
                    TerRoguelikePlayer modPlayer = player.GetModPlayer<TerRoguelikePlayer>();
                    int healAmt = ownerModPlayer.remedialTapeworm * 1; // heal for how much tapeworm the original spawner player had
                    modPlayer.ScaleableHeal(healAmt); // however, scales based off of the touchee's healing effectiveness
                    Projectile.Kill();
                }
            }
        }

        public override bool? CanDamage() => false;

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D lightTexture = TextureAssets.Projectile[Type].Value;
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float colorInterpolation = (float)Math.Cos(Projectile.timeLeft / 32f + Main.GlobalTimeWrappedHourly / 20f + i / (float)Projectile.oldPos.Length * MathHelper.Pi) * 0.5f + 0.5f;
                Color color = Color.Lerp(Color.LightSeaGreen, Color.LimeGreen, colorInterpolation) * (i <= 1 ? 1f - (i * 0.05f) : 0.7f);
                color.A = 0;
                Vector2 drawPosition = Projectile.oldPos[i] - Main.screenPosition + new Vector2(Projectile.width, Projectile.height) * 0.5f;
                Color outerColor = color;
                Color innerColor = color * 0.5f;
                float intensity = 0.8f + 0.15f * (float)Math.Cos(Projectile.timeLeft * MathHelper.TwoPi / Projectile.MaxUpdates * 0.04f);
                intensity *= i <= 1 ? 1f : MathHelper.Lerp(0.15f, 0.6f, 1f - i / (float)Projectile.oldPos.Length);
                if (Projectile.timeLeft <= 60 * Projectile.MaxUpdates) //Shrinks to nothing when projectile is nearing death
                {
                    intensity *= Projectile.timeLeft / (60f * Projectile.MaxUpdates);
                }
                if (Projectile.ai[0] < 30 * Projectile.MaxUpdates)
                {
                    intensity *= MathHelper.Lerp(0.5f, 1f, Projectile.ai[0] / (30 * Projectile.MaxUpdates));
                }
                // Become smaller the futher along the old positions we are.
                Vector2 outerScale = new Vector2(1f) * intensity;
                Vector2 innerScale = new Vector2(1f) * intensity * 0.7f;
                outerColor *= intensity;
                innerColor *= intensity;
                Main.EntitySpriteDraw(lightTexture, drawPosition, null, outerColor, 0f, lightTexture.Size() * 0.5f, outerScale * 0.15f, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(lightTexture, drawPosition, null, innerColor, 0f, lightTexture.Size() * 0.5f, innerScale * 0.15f, SpriteEffects.None, 0);
            }
            return false;
        }
    }
}
