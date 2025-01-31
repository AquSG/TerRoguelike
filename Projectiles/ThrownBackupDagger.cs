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
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using System.Security.Cryptography.X509Certificates;

namespace TerRoguelike.Projectiles
{
    public class ThrownBackupDagger : ModProjectile, ILocalizedModType
    {
        public bool ableToHit = true;
        public TerRoguelikeGlobalProjectile modProj;
        TerRoguelikePlayer modPlayer;
        public int setTimeLeft = 6300;
        public Vector2 originalDirection;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 60;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.extraUpdates = 7;
            Projectile.timeLeft = setTimeLeft;
            Projectile.penetrate = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            modProj = Projectile.ModProj();
        }

        public override bool? CanDamage() => ableToHit ? (bool?)null : false;
        public override bool? CanHitNPC(NPC target)
        {
            //Lasts past the first hit for visuals.
            if (Projectile.penetrate == 1)
                return false;

            return (bool?)null;
        }

        public override void AI()
        {
            if (Projectile.localAI[1] == 0)
            {
                originalDirection = Projectile.velocity.SafeNormalize(-Vector2.UnitY);
                SoundEngine.PlaySound(SoundID.Item39 with { Volume = 1f }, Projectile.Center);
            }
            Projectile.localAI[1]++;

            if (modPlayer == null)
                modPlayer = Main.player[Projectile.owner].GetModPlayer<TerRoguelikePlayer>();

            if (modProj.homingTarget == -1 && Projectile.ai[0] != -1)
                modProj.homingTarget = (int)Projectile.ai[0];

            float homingStrength = 4f;
            homingStrength *= MathHelper.Lerp(1f, 2.2f, (setTimeLeft - Projectile.timeLeft) / (float)setTimeLeft);
            modProj.HomingAI(Projectile, 0.001128f * homingStrength, false);

            if (Projectile.timeLeft <= 60)
            {
                ableToHit = false;
                Projectile.velocity = Vector2.Zero;
                return;
            }
                
            Projectile.rotation = Projectile.velocity != Vector2.Zero ? Projectile.velocity.ToRotation() : Projectile.oldRot[1];
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            bool collideX = Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon;
            bool collideY = Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon;

            void Stop()
            {
                Projectile.velocity = Vector2.Zero;
                if (Projectile.timeLeft > 60)
                    Projectile.timeLeft = 60;

                Projectile.penetrate = 1;
                if (modPlayer.volatileRocket > 0)
                    modProj.SpawnExplosion(Projectile, modPlayer, originalHit: true);
                Projectile.rotation = Projectile.oldRot[1];
                SoundEngine.PlaySound(SoundID.Dig with { Volume = 0.75f }, Projectile.Center);
            }
            if ((collideX && collideY) || Projectile.timeLeft <= 60 || Projectile.penetrate == 1)
            {
                Stop();
                return false;
            }

            if (collideX)
            {
                Projectile.velocity = Vector2.UnitY * Math.Sign(oldVelocity.Y) * oldVelocity.Length();
            }
            else if (collideY)
            {
                Projectile.velocity = Vector2.UnitX * Math.Sign(oldVelocity.X) * oldVelocity.Length();
            }
            if (Projectile.velocity == Vector2.Zero)
            {
                Stop();
                return false;
            }
            
            return false;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D lightTexture = TextureAssets.Projectile[Type].Value;
            for (int j = 1; j < Projectile.oldPos.Length + 1; j++)
            {
                int i = j % Projectile.oldPos.Length;

                if (Projectile.timeLeft <= 60 && i + Projectile.timeLeft <= 60)
                    continue;

                Color color;
                if (i == 0)
                    color = Color.White * 0.8f;
                else
                    color = Color.Lerp(Color.Red, Color.DarkRed, (float)i / (Projectile.oldPos.Length)) * 0.65f;

                Vector2 drawPosition = Projectile.oldPos[i] + new Vector2(Projectile.width * 0.5f, Projectile.height * 0.5f) - Main.screenPosition;
                
                // Become smaller the futher along the old positions we are.
                Vector2 scale = i == 0 ? new Vector2(1f) : new Vector2(1f) * MathHelper.Lerp(0.25f, 0.75f, 1f - i / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(lightTexture, drawPosition, null, color, Projectile.oldRot[i], lightTexture.Size() * 0.5f, scale, SpriteEffects.None, 0);
            }
            return false;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Projectile.timeLeft = 60;
            ableToHit = false;
            Projectile.velocity = Vector2.Zero;
            SoundEngine.PlaySound(SoundID.NPCHit18 with { Volume = 0.6f }, Projectile.Center);
        }
    }
}
