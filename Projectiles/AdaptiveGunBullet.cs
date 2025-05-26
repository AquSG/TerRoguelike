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
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using Terraria.DataStructures;

namespace TerRoguelike.Projectiles
{
    public class AdaptiveGunBullet : ModProjectile, ILocalizedModType
    {
        public bool ableToHit = true;
        public TerRoguelikeGlobalProjectile modProj;
        TerRoguelikePlayer modPlayer;
        public int setTimeLeft = 400;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 9;
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
            Projectile.MaxUpdates = 4;
            Projectile.timeLeft = setTimeLeft;
            Projectile.penetrate = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            modProj = Projectile.ModProj();
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
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.scale = Projectile.ai[0];
            //scale support
            Projectile.position = Projectile.Center + new Vector2(-2 * Projectile.scale, -2 * Projectile.scale);
            Projectile.width = (int)(4 * Projectile.scale);
            Projectile.height = (int)(4 * Projectile.scale);

            modPlayer = Main.player[Projectile.owner].ModPlayer();
        }
        public override void AI()
        {
            modPlayer ??= Main.player[Projectile.owner].ModPlayer();
            if (modPlayer.heatSeekingChip > 0)
                modProj.HomingAI(Projectile, (float)Math.Log(modPlayer.heatSeekingChip + 1, 1.2d) / (833f * Projectile.MaxUpdates));

            if (modPlayer.bouncyBall > 0 || modPlayer.trash > 0)
                modProj.extraBounces += modPlayer.bouncyBall + modPlayer.trash;

            if (Projectile.timeLeft <= 2 * Projectile.MaxUpdates)
            {
                ableToHit = false;
                Projectile.velocity = Vector2.Zero;
                return;
            }
                
            Projectile.rotation = Projectile.velocity.ToRotation();
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D lightTexture = TextureAssets.Projectile[Type].Value;
            int deathTime = 2 * Projectile.MaxUpdates;
            Vector2 innateOffset = (lightTexture.Size() * 0.5f * Projectile.scale) - Main.screenPosition;
            for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero)
                    continue;
                if (i > 0)
                {
                    if (Projectile.oldPos[i + 1] == Vector2.Zero)
                        continue;
                }
                for (int j = 0; j < 8; j++)
                {
                    int progress = (i * 8) + j;
                    /*
                    if (Projectile.timeLeft <= deathTime)
                    {
                        if (Projectile.timeLeft > progress + 5)
                            continue;
                    }
                    */
                    float jCompletion = j / 8f;
                    float completion = (float)(progress) / 64;
                    Color color = Color.Lerp(Color.Yellow, Color.White, (completion * 2));
                    Vector2 drawPosition = Projectile.oldPos[i] + innateOffset;
                    Vector2 offset = (Projectile.oldPos[i + 1] - Projectile.oldPos[i]) * jCompletion;

                    Vector2 scale = new Vector2(1.2f) * MathHelper.Lerp(0.25f, 1f, 1f - completion);
                    Main.EntitySpriteDraw(lightTexture, drawPosition + offset, null, color, Projectile.oldRot[i], lightTexture.Size() * 0.5f, scale * Projectile.scale, SpriteEffects.None, 0);
                }
            }
            if (modPlayer != null)
            {
                if (modPlayer.volatileRocket > 0 && Projectile.velocity != Vector2.Zero)
                {
                    Texture2D rocketTexture = TexDict["VolatileRocket"];
                    Vector2 drawPosition = Projectile.Center - Main.screenPosition;
                    Main.EntitySpriteDraw(rocketTexture, drawPosition, null, Color.White, Projectile.velocity.ToRotation(), rocketTexture.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
                }
            }
            return false;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            //currently, no piercing is available for this.
            Projectile.timeLeft = 2 * Projectile.MaxUpdates;
            ableToHit = false;
            Projectile.velocity = Vector2.Zero;
            Projectile.penetrate = 1;
            Projectile.netUpdate = true;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            modProj.bounceCount++;
            if (modProj.bounceCount >= 1 + modProj.extraBounces)
            {
                if (oldVelocity != Vector2.Zero)
                    Projectile.Center = TileCollidePositionInLine(Projectile.Center, Projectile.Center + oldVelocity.SafeNormalize(Vector2.UnitY) * 16);
                if (Projectile.timeLeft > 2 * Projectile.MaxUpdates)
                    Projectile.timeLeft = 2 * Projectile.MaxUpdates;
                ableToHit = false;
                Projectile.velocity = Vector2.Zero;

            }
            else
            {
                // If the projectile hits the left or right side of the tile, reverse the X velocity
                if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
                {
                    Projectile.velocity.X = -oldVelocity.X;
                    Projectile.timeLeft = setTimeLeft;
                }
                // If the projectile hits the top or bottom side of the tile, reverse the Y velocity
                if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
                {
                    Projectile.velocity.Y = -oldVelocity.Y;
                    Projectile.timeLeft = setTimeLeft;
                }
            }
            return false;
        }
    }
}
