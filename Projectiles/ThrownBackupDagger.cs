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
            modProj = Projectile.GetGlobalProjectile<TerRoguelikeGlobalProjectile>();
        }

        public override bool? CanDamage() => ableToHit ? (bool?)null : false;
        public override bool? CanHitNPC(NPC target)
        {
            if (Projectile.penetrate == 1)
                return false;

            return (bool?)null;
        }

        public override void AI()
        {
            if (Projectile.localAI[1] == 0)
            {
                originalDirection = Projectile.velocity.SafeNormalize(-Vector2.UnitY);
                SoundEngine.PlaySound(SoundID.Item39 with { Volume = 0.5f }, Projectile.Center);
            }
            if (Projectile.timeLeft == 59)
            {
                
            }
            Projectile.localAI[1]++;

            if (modPlayer == null)
                modPlayer = Main.player[Projectile.owner].GetModPlayer<TerRoguelikePlayer>();

            if (modProj.homingTarget == -1 && Projectile.ai[0] != -1)
                modProj.homingTarget = (int)Projectile.ai[0];

            modProj.HomingAI(Projectile, 0.001128f * 4f, false);

            if (Projectile.timeLeft <= 60)
            {
                ableToHit = false;
                Projectile.velocity = Vector2.Zero;
                return;
            }
                
            Projectile.rotation = Projectile.velocity != Vector2.Zero ? Projectile.velocity.ToRotation() - (MathHelper.PiOver4 * 3f) : Projectile.oldRot[1];
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.velocity = Vector2.Zero;
            if (Projectile.timeLeft > 60)
                Projectile.timeLeft = 60;

            Projectile.penetrate = 1;
            if (modPlayer.volatileRocket > 0)
                modProj.SpawnExplosion(Projectile, modPlayer, originalHit: true);
            Projectile.rotation = Projectile.oldRot[1];
            SoundEngine.PlaySound(SoundID.Dig with { Volume = 0.5f }, Projectile.Center);
            return false;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D lightTexture = ModContent.Request<Texture2D>(Texture).Value;
            for (int j = 1; j < Projectile.oldPos.Length + 1; j++)
            {
                int i = j % Projectile.oldPos.Length;

                if (Projectile.timeLeft <= 60 && i + Projectile.timeLeft <= 60)
                    continue;

                Color color;
                if (i == 0)
                    color = Color.White * 0.8f;
                else
                    color = Color.Lerp(Color.Red, Color.DarkRed, (float)i / (Projectile.oldPos.Length / 2)) * 0.65f;

                Vector2 drawPosition = Projectile.oldPos[i] + new Vector2(Projectile.width * 0.5f, Projectile.height * 0.5f) - Main.screenPosition;
                
                // Become smaller the futher along the old positions we are.
                Vector2 scale = new Vector2(1f) * MathHelper.Lerp(0.25f, 1f, 1f - i / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(lightTexture, drawPosition, null, color, Projectile.oldRot[i], lightTexture.Size() * 0.5f, scale, SpriteEffects.None, 0);
            }
            return false;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Projectile.timeLeft = 60;
            ableToHit = false;
            Projectile.velocity = Vector2.Zero;
            SoundEngine.PlaySound(SoundID.NPCHit18 with { Volume = 0.3f }, Projectile.Center);
        }
    }
}
