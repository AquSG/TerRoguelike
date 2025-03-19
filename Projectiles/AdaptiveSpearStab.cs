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
using Terraria.GameContent.UI.ResourceSets;
using Terraria.DataStructures;
using TerRoguelike.Particles;

namespace TerRoguelike.Projectiles
{
    public class AdaptiveSpearStab : ModProjectile, ILocalizedModType
    {
        public override string Texture => "TerRoguelike/Projectiles/InvisibleProj";
        public TerRoguelikeGlobalProjectile modProj;
        TerRoguelikePlayer modPlayer;
        public Player player;
        public Vector2 stuckPosition = Vector2.Zero;
        public Texture2D squareTex, sparkTex, glowTex;
        public int animStopTime = 5;
        public override void SetStaticDefaults()
        {

        }
        public override void SetDefaults()
        {
            Projectile.width = 68;
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
            modProj = Projectile.ModProj();
            squareTex = TextureManager.TexDict["Square"];
            sparkTex = TextureManager.TexDict["ThinSpark"];
            glowTex = TextureManager.TexDict["CircularGlow"];
        }
        public override void OnSpawn(IEntitySource source)
        {
            //scale support
            Projectile.position = Projectile.Center + new Vector2(-34 * Projectile.scale, -34 * Projectile.scale);
            Projectile.width = (int)(68 * Projectile.scale);
            Projectile.height = (int)(68 * Projectile.scale);

            player ??= Main.player[Projectile.owner];
            modPlayer ??= player.ModPlayer();

            stuckPosition = player.position - Projectile.position;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.velocity = Vector2.Zero;
        }
        public override void AI()
        {
            player ??= Main.player[Projectile.owner];
            modPlayer ??= player.ModPlayer();

            if (stuckPosition == Vector2.Zero)
            {
                //keep this shit stuck to the player
                stuckPosition = player.position - Projectile.position;
            }
            Projectile.position = player.position - stuckPosition + (Vector2.UnitY * player.gfxOffY);
            Projectile.localAI[0] += 1 * player.GetAttackSpeed(DamageClass.Generic); // animation speed scales with attack speed
            if (Projectile.localAI[0] >= 20 && Projectile.timeLeft != 1000) // kill when done animating
            {
                Projectile.Kill();
            }

            if (Projectile.localAI[0] < 12)
            {
                float tipInterpolant = 1f - Math.Min(Projectile.localAI[0] / animStopTime, 1);
                Vector2 tipPos = Projectile.Center + Projectile.rotation.ToRotationVector2() * (-54 * tipInterpolant + 24);
                for (int i = -1; i <= 1; i += 2)
                {
                    if (!Main.rand.NextBool(5))
                        continue;
                    float particleRot = Projectile.rotation + MathHelper.Pi + 0.44f * i;
                    ParticleManager.AddParticle(new Square(tipPos + particleRot.ToRotationVector2() * Main.rand.NextFloat(30 * Projectile.scale), Vector2.Zero, 10, Color.Lerp(Color.Red, Color.Yellow, Main.rand.NextFloat()), new Vector2(Main.rand.NextFloat(0.5f, 1f)), Main.rand.NextFloat(MathHelper.TwoPi), 0.96f, 10, false), ParticleManager.ParticleLayer.AfterProjectiles);
                }
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Projectile.owner < 0)
                return null;

            for (int i = 0; i < 2; i++)
            {
                float interpolant = 1f - Math.Min(Projectile.localAI[0] / animStopTime, 1);
                Vector2 hitboxPos = Projectile.Center + Projectile.rotation.ToRotationVector2() * ((-54 * interpolant) - (i * 12));

                float radius = Projectile.height * 0.2f;

                if (targetHitbox.ClosestPointInRect(hitboxPos).Distance(hitboxPos) <= radius)
                    return true;
            }

            return false;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            int direction = Math.Sign((target.Center - Main.player[Projectile.owner].Center).X);
            if (direction == 0)
                direction = Main.rand.NextBool() ? 1 : -1;
            modifiers.HitDirectionOverride = direction;
        }

        public override bool? CanDamage() => Projectile.localAI[0] < animStopTime + 6 ? null : (Projectile.timeLeft == 1000 ? null : false);
        public override bool PreDraw(ref Color lightColor)
        {
            float tipInterpolant = 1f - Math.Min(Projectile.localAI[0] / animStopTime, 1);
            Vector2 tipPos = Projectile.Center + Projectile.rotation.ToRotationVector2() * (-54 * tipInterpolant + 24 + MathHelper.Lerp(-10, 0, Projectile.scale));
            Color sparkColor = Color.Red;
            sparkColor.A = 0;

            if (Projectile.localAI[0] > animStopTime - 3)
            {
                sparkColor *= 1 - ((Projectile.localAI[0] - animStopTime) / 13f);
            }
            for (int i = -1; i <= 1; i += 2)
            {
                Main.EntitySpriteDraw(sparkTex, tipPos - Main.screenPosition, null, sparkColor, Projectile.rotation + i * 0.44f, sparkTex.Size() * new Vector2(0.9f, 0.5f), 0.1f * Projectile.scale, SpriteEffects.None);
            }

            if (false)
            {
                for (int i = 0; i < 3; i++)
                {
                    float interpolant = 1f - Math.Min(Projectile.localAI[0] / animStopTime, 1);
                    Vector2 hitboxPos = Projectile.Center + Projectile.rotation.ToRotationVector2() * ((-54 * interpolant) - (i * 12));

                    float radius = Projectile.height * 0.2f;

                    for (int j = 0; j < 120; j++)
                    {
                        Main.EntitySpriteDraw(squareTex, hitboxPos + ((j / 120f * MathHelper.TwoPi).ToRotationVector2() * radius) - Main.screenPosition, null, Color.Red, 0, squareTex.Size() * 0.5f, 1f, SpriteEffects.None);
                    }
                }
            }

            return false;
        }
    }
}
