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
using Terraria.GameContent;

namespace TerRoguelike.Projectiles
{
    public class SlimeGlob : ModProjectile, ILocalizedModType
    {
        public SpriteEffects spriteEffects = SpriteEffects.FlipVertically;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
        }
        public override void SetDefaults()
        {
            Projectile.width = 9;
            Projectile.height = 9;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 1200;
        }

        public override void AI()
        {
            Projectile.ai[0]++;
            TerRoguelikePlayer ownerModPlayer = Main.player[Projectile.owner].GetModPlayer<TerRoguelikePlayer>();
            if (Projectile.frame != 3)
            {
                Projectile.frame = (int)Projectile.ai[0] / 7 % 3;
                Projectile.rotation = Projectile.velocity.ToRotation();
                if (Projectile.velocity.Y < 8)
                    Projectile.velocity.Y += 0.085f;
                if (Projectile.velocity.Y > 8)
                    Projectile.velocity.Y = 8;

                return;
            }

            if (Projectile.ai[0] % 30 != 0)
                return;

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];

                if (player == null || !player.active) // herobrine touched my bungus
                    continue;

                Vector2 playerVector = player.Center - Projectile.Center;
                float playerDist = playerVector.Length();
                if (playerDist < 32f)
                {
                    TerRoguelikePlayer modPlayer = player.GetModPlayer<TerRoguelikePlayer>();
                    int healAmt = (int)(ownerModPlayer.nutritiousSlime * player.statLifeMax2 * 0.02f); // heal for how much slime the original spawner player had
                    modPlayer.ScaleableHeal(healAmt); // however, scales based off of the touchee's healing effectiveness
                }
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            
            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
            {
                Projectile.velocity.X = -oldVelocity.X * 0.5f;
            }

            if (oldVelocity.Y >= 0 * 5 && Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
            {
                Projectile.frame = 3;
                Projectile.velocity = Vector2.Zero;
                Projectile.rotation = 0f;
                Projectile.Bottom = new Vector2(Projectile.Bottom.X, (int)((Projectile.oldPosition.Y + 1f) / 16f) * 16f + 16f);
                SoundEngine.PlaySound(SoundID.NPCDeath21 with { Volume = 0.4f }, Projectile.Center);
            }
            
            return false;
        }
        public override bool? CanDamage() => false;

        public override bool PreDraw(ref Color lightColor)
        {
            if (spriteEffects == SpriteEffects.FlipVertically)
            {
                spriteEffects = Main.rand.NextBool() ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            }
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Color color = Color.White * 0.5f;
            float opacity = MathHelper.Clamp(MathHelper.Lerp(0f, 1f, Projectile.timeLeft / 60f), 0, 1f);

            Main.EntitySpriteDraw(texture, drawPosition - Vector2.UnitY, new Rectangle(0, frameHeight * Projectile.frame, texture.Width, frameHeight), color * opacity, Projectile.rotation, new Vector2(texture.Width / 2f, (frameHeight / 2f)), 1f, Projectile.frame == 3 ? spriteEffects : SpriteEffects.None, 0);
            return false;
        }
    }
}
