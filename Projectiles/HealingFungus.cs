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
    public class HealingFungus : ModProjectile, ILocalizedModType
    {
        public SpriteEffects spriteEffects = SpriteEffects.FlipVertically;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 6;
        }
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 1200;
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;
            TerRoguelikePlayer ownerModPlayer = Main.player[Projectile.owner].GetModPlayer<TerRoguelikePlayer>();
            Projectile.ai[0]++;
            if (Projectile.ai[0] < 25)
                return;

            var projRect = Projectile.getRect();
            foreach (Player player in Main.ActivePlayers)
            {
                if (player.dead)
                    continue;

                if (player.getRect().Intersects(projRect))
                {
                    TerRoguelikePlayer modPlayer = player.GetModPlayer<TerRoguelikePlayer>();
                    int healAmt = ownerModPlayer.benignFungus; // heal for how much bungus the original spawner player had
                    modPlayer.ScaleableHeal(healAmt); // however, scales based off of the touchee's healing effectiveness
                    Projectile.Kill();
                }
            }   
        }

        public override bool? CanDamage() => false;

        public override bool PreDraw(ref Color lightColor)
        {
            if (spriteEffects == SpriteEffects.FlipVertically)
            {
                spriteEffects = Main.rand.NextBool() ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                Projectile.frame = Main.rand.Next(Main.projFrames[Projectile.type]);
            }
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Color color = Color.White * 0.5f;
            if (Projectile.timeLeft < 15)
                color *= Projectile.timeLeft / 15f;
            Main.EntitySpriteDraw(texture, drawPosition, new Rectangle(0, frameHeight * Projectile.frame, texture.Width, frameHeight), color, 0f, new Vector2(texture.Width * 0.5f, (frameHeight * 0.5f)), 1f, spriteEffects, 0);
            return false;
        }
    }
}
