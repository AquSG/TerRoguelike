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

namespace TerRoguelike.Projectiles
{
    public class SniperBullet : ModProjectile, ILocalizedModType
    {
        public override string Texture => "TerRoguelike/Projectiles/AdaptiveGunBullet";
        public bool ableToHit = true;
        public TerRoguelikeGlobalProjectile modProj;
        public int setTimeLeft = 3000;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 60;
        }
        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.extraUpdates = 29;
            Projectile.timeLeft = setTimeLeft;
            Projectile.penetrate = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            modProj = Projectile.GetGlobalProjectile<TerRoguelikeGlobalProjectile>();
        }

        public override bool? CanDamage() => ableToHit ? (bool?)null : false;
        public override bool? CanHitNPC(NPC target)
        {
            // used for not immediately cutting off afterimages when the projectile would in normal circumstances be killed.
            // allows the afterimages to visually catch up so that the bullet always visually looks like it reached a point.
            if (Projectile.penetrate == 1) 
                return false;

            return (bool?)null;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0)
            {
                //scale support
                Projectile.position = Projectile.Center + new Vector2(-2 * Projectile.scale, -2 * Projectile.scale);
                Projectile.width = (int)(4 * Projectile.scale);
                Projectile.height = (int)(4 * Projectile.scale);
                Projectile.localAI[0]++;
            }

            if (Projectile.timeLeft <= 60)
            {
                ableToHit = false;
                Projectile.velocity = Vector2.Zero;
                return;
            }
                
            Projectile.rotation = Projectile.velocity.ToRotation();
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D lightTexture = ModContent.Request<Texture2D>(Texture).Value;
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.timeLeft <= 60 && i + Projectile.timeLeft < 45)
                    continue;

                Color color = Color.Lerp(Color.Red, Color.Pink, (float)i / (Projectile.oldPos.Length));
                Vector2 drawPosition = Projectile.oldPos[i] + (lightTexture.Size() * 0.5f * Projectile.scale) - Main.screenPosition;
                
                // Become smaller the futher along the old positions we are.
                Vector2 scale = new Vector2(1f) * MathHelper.Lerp(0.25f, 1f, 1f - i / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(lightTexture, drawPosition, null, color, Projectile.oldRot[i], lightTexture.Size() * 0.5f, scale * Projectile.scale, SpriteEffects.None, 0);
            }
            return false;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Projectile.timeLeft = 60;
            ableToHit = false;
            Projectile.velocity = Vector2.Zero;
        }
        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            Projectile.timeLeft = 60;
            ableToHit = false;
            Projectile.velocity = Vector2.Zero;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.timeLeft > 60)
            {
                Projectile.timeLeft = 60;
                SoundEngine.PlaySound(SoundID.NPCHit3 with { Volume = 0.15f }, Projectile.Center);
            }
                
            ableToHit = false;
            Projectile.velocity = Vector2.Zero;
            return false;
        }
    }
}
