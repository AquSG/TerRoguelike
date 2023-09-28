using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TerRoguelike.Managers;
using TerRoguelike.Systems;
using TerRoguelike.Player;
using Microsoft.Xna.Framework.Graphics;

namespace TerRoguelike.Projectiles
{
    public class Explosion : ModProjectile, ILocalizedModType
    {
        public override string Texture => "TerRoguelike/Projectiles/Explosion";
        public float MaxScale = -1f;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 5;
        }
        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.timeLeft = 20;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
        }

        public override void AI()
        {
            if (MaxScale == -1f)
            {
                MaxScale = Projectile.scale;
                Projectile.position = Projectile.Center + new Vector2(-25 * MaxScale, -25 * MaxScale);
                Projectile.width = (int)(50 * MaxScale);
                Projectile.height = (int)(50 * MaxScale);
            }

            if (Projectile.localAI[0] != 1)
            {
                for (int i = 0; i < 15; i++)
                {
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.SolarFlare, Scale: 0.85f);
                }
                Projectile.localAI[0] = 1;
            }

            Projectile.scale = (0.5f + (0.5f * Projectile.timeLeft / 20f)) * MaxScale;
            Projectile.frame = Projectile.frameCounter / 4 % Main.projFrames[Projectile.type];
            Projectile.frameCounter++;
        }
        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.SolarFlare, Scale: 0.75f);
            }
        }
        public override bool? CanDamage() => Projectile.timeLeft == 20 ? (bool?)null : false;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, new Rectangle(0, frameHeight * Projectile.frame, texture.Width, frameHeight), Color.White, 0f, new Vector2(texture.Width / 2f, (frameHeight / 2f)), Projectile.scale, SpriteEffects.None);
            return false;
        }
    }
}
