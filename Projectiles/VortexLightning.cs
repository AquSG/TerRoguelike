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
using Terraria.GameContent;
using static TerRoguelike.Managers.TextureManager;
using Terraria.DataStructures;
using static TerRoguelike.Utilities.TerRoguelikeUtils;

namespace TerRoguelike.Projectiles
{
    public class VortexLightning : ModProjectile, ILocalizedModType
    {
        public bool ableToHit = true;
        public TerRoguelikeGlobalProjectile modProj;
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
            Projectile.MaxUpdates = 5;
            Projectile.timeLeft = 200 * Projectile.MaxUpdates;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            modProj = Projectile.ModProj();
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.velocity /= Projectile.MaxUpdates;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.ai[0] = Projectile.rotation;
            Projectile.ai[2] = Projectile.ai[0] + Main.rand.NextFloat(-MathHelper.Pi * 0.3f, MathHelper.Pi * 0.3f + float.Epsilon);
            Projectile.velocity = (Vector2.UnitX * Projectile.velocity.Length()).RotatedBy(Projectile.ai[2]);
        }
        public override bool? CanDamage() => ableToHit ? (bool?)null : false;
        public override bool? CanHitNPC(NPC target)
        {
            // used for not immediately cutting off afterimages when the projectile would in normal circumstances be killed.
            // allows the afterimages to visually catch up so that the bullet always visually looks like it reached a point.
            if (!ableToHit) 
                return false;

            return (bool?)null;
        }

        public override void AI()
        {
            if (Projectile.timeLeft <= 60)
            {
                ableToHit = false;
                Projectile.velocity = Vector2.Zero;
                if (Projectile.timeLeft % (2 * Projectile.MaxUpdates) == 0)
                {
                    Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, 0, 0, 0, default, 0.8f);
                    d.velocity.Y -= 1f;
                    d.noLight = true;
                    d.noLightEmittence = true;
                    d.noLight = true;
                }
                return;
            }
            if (Projectile.timeLeft % (4 * Projectile.MaxUpdates) == 0)
            {
                int sign = Math.Sign(Projectile.ai[0] - Projectile.ai[2]);
                if (sign == 0)
                    sign = 1;
                Projectile.ai[2] = Projectile.ai[0] + (sign * Main.rand.NextFloat(MathHelper.Pi * 0.02f, MathHelper.Pi * 0.3f + float.Epsilon));
                Projectile.velocity = (Vector2.UnitX * Projectile.velocity.Length()).RotatedBy(Projectile.ai[2]);
            }
                
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (Projectile.timeLeft % Projectile.MaxUpdates == 0)
            {
                Dust d =Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, 0, 0, 0, default, 0.5f);
                d.velocity *= 0.5f;
                d.noGravity = true;
                d.noLight = true;
                d.noLightEmittence = true;
                d.noLight = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D lightTexture = TextureAssets.Projectile[Type].Value;
            Vector2 offset = new Vector2(Projectile.width * 0.5f, Projectile.height * 0.5f);
            for (int i = Projectile.oldPos.Length - 1; i >= 0; i--)
            {
                if (Projectile.timeLeft <= 60 && i + Projectile.timeLeft < 45)
                    continue;

                Color color = Color.Lerp(Color.White, Color.White, (float)i / (Projectile.oldPos.Length / 2));
                Vector2 drawPosition = Projectile.oldPos[i] + offset - Main.screenPosition;
                
                Vector2 scale = new Vector2(0.33f) * MathHelper.Lerp(0.33f, 1f, 1f - i / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(lightTexture, drawPosition, null, color, Projectile.oldRot[i], lightTexture.Size() * 0.5f, scale * Projectile.scale, SpriteEffects.None, 0);
            }

            return false;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.timeLeft > (200 * Projectile.MaxUpdates) - (2 * Projectile.MaxUpdates))
                return false;
            if (Projectile.timeLeft > 60)
                Projectile.timeLeft = 60;
            ableToHit = false;
            Projectile.velocity = Vector2.Zero;
            return false;
        }
    }
}
