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

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];

                if (player == null || !player.active)
                    return;

                Vector2 playerVector = player.Center - Projectile.Center;
                float playerDist = playerVector.Length();
                if (playerDist < 50f && Projectile.position.X < player.position.X + player.width && Projectile.position.X + Projectile.width > player.position.X && Projectile.position.Y < player.position.Y + player.height && Projectile.position.Y + Projectile.height > player.position.Y)
                {
                    TerRoguelikePlayer modPlayer = player.GetModPlayer<TerRoguelikePlayer>();
                    int healAmt = ownerModPlayer.benignFungus;
                    modPlayer.ScaleableHeal(healAmt);
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
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Color color = Color.White * 0.5f;
            if (Projectile.timeLeft < 15)
                color *= Projectile.timeLeft / 15f;
            Main.EntitySpriteDraw(texture, drawPosition, new Rectangle(0, frameHeight * Projectile.frame, texture.Width, frameHeight), color, 0f, new Vector2(texture.Width / 2f, (frameHeight / 2f)), 1f, spriteEffects, 0);
            return false;
        }
    }
}
