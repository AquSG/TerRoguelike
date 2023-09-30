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
using TerRoguelike.Utilities;

namespace TerRoguelike.Projectiles
{
    public class AdaptiveBladeSlash : ModProjectile, ILocalizedModType
    {
        public TerRoguelikeGlobalProjectile modProj;
        TerRoguelikePlayer modPlayer;
        public Player player;
        public Vector2 stuckPosition = Vector2.Zero;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 5;
        }
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 68;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.timeLeft = 1000;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            modProj = Projectile.GetGlobalProjectile<TerRoguelikeGlobalProjectile>();
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0)
            {
                Projectile.position = Projectile.Center + new Vector2(-34 * Projectile.scale, -34 * Projectile.scale);
                Projectile.width = (int)(68 * Projectile.scale);
                Projectile.height = (int)(68 * Projectile.scale);
            }

            if (player == null)
                player = Main.player[Projectile.owner];
            if (modPlayer == null)
                modPlayer = player.GetModPlayer<TerRoguelikePlayer>();

            if (stuckPosition == Vector2.Zero)
            {
                stuckPosition = player.position - Projectile.position;
            }
            Projectile.position = player.position - stuckPosition;
            Projectile.frame = (int)(Projectile.localAI[0] / 4);
            Projectile.localAI[0] += 1 * player.GetAttackSpeed(DamageClass.Generic);
            if (Projectile.frame > Main.projFrames[Projectile.type])
            {
                Projectile.Kill();
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => Projectile.RotatingHitboxCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.rotation.ToRotationVector2(), backCutoff: 0.3f);
        public override bool? CanDamage() => Projectile.frame <= 3 ? (bool?)null : false;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            SpriteEffects spriteEffects = modProj.swingDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, new Rectangle(0, frameHeight * Projectile.frame, texture.Width, frameHeight), Color.Lerp(lightColor, Color.White, 0.5f), Projectile.rotation + (MathHelper.Pi * modProj.swingDirection / 12f), new Vector2(texture.Width / 2f, (frameHeight / 2f)), Projectile.scale, spriteEffects);
            return false;
        }
    }
}
