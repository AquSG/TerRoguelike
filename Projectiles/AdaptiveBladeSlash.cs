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
using Terraria.GameContent;

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
                //scale support
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
                //keep this shit stuck to the player
                stuckPosition = player.position - Projectile.position;
            }
            Projectile.position = player.position - stuckPosition + (Vector2.UnitY * player.gfxOffY);
            Projectile.frame = (int)(Projectile.localAI[0] / 4);
            Projectile.localAI[0] += 1 * player.GetAttackSpeed(DamageClass.Generic); // animation speed scales with attack speed
            if (Projectile.frame > Main.projFrames[Projectile.type]) // kill when done animating
            {
                Projectile.Kill();
            }
        }
        //rotating rectangle hitbox collision
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => Projectile.RotatingHitboxCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.rotation.ToRotationVector2(), backCutoff: 0.3f);
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            int direction = Math.Sign((target.Center - Main.player[Projectile.owner].Center).X);
            if (direction == 0)
                direction = Main.rand.NextBool() ? 1 : -1;
            modifiers.HitDirectionOverride = direction;
        }

        //only hit if in the first 3 frames of animation
        public override bool? CanDamage() => Projectile.frame <= 3 ? (bool?)null : false;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            SpriteEffects spriteEffects = modProj.swingDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, new Rectangle(0, frameHeight * Projectile.frame, texture.Width, frameHeight), Color.Lerp(lightColor, Color.White, 0.5f), Projectile.rotation + (MathHelper.Pi * modProj.swingDirection / 12f), new Vector2(texture.Width / 2f, (frameHeight / 2f)), Projectile.scale, spriteEffects);
            return false;
        }
    }
}
