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
using rail;

namespace TerRoguelike.Projectiles
{
    public class AdaptiveDaggerStab : ModProjectile, ILocalizedModType
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
            if (Projectile.localAI[0] >= 12 && Projectile.timeLeft != 1000) // kill when done animating
            {
                Projectile.Kill();
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Projectile.owner < 0)
                return null;

            for (int i = 0; i < 4; i++)
            {
                Vector2 hitboxPos = Projectile.Center + Projectile.rotation.ToRotationVector2() * (-8 + i * 9 + MathHelper.Lerp(-10, 0, Projectile.scale));

                float radius = Projectile.height * 0.1f;

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

        public override bool? CanDamage() => Projectile.localAI[0] < animStopTime + 5 ? (bool?)null : (Projectile.timeLeft == 1000 ? null : false);
        public override bool PreDraw(ref Color lightColor)
        {
            float tipInterpolant = 1f - Math.Min(Projectile.localAI[0] / animStopTime, 1);
            Vector2 tipPos = Projectile.Center + Projectile.rotation.ToRotationVector2() * (-16 * tipInterpolant + 28 + MathHelper.Lerp(-16, 0, Projectile.scale));
            Color sparkColor = Color.White;
            sparkColor.A = 0;

            if (Projectile.localAI[0] > animStopTime)
            {
                sparkColor *= 1 - ((Projectile.localAI[0] - animStopTime) / 7f);
            }
            for (int i = -1; i <= 1; i += 2)
            {
                Main.EntitySpriteDraw(sparkTex, tipPos - Main.screenPosition, null, sparkColor, Projectile.rotation + i * 0.44f, sparkTex.Size() * new Vector2(0.9f, 0.5f), 0.05f * Projectile.scale, SpriteEffects.None);
            }

            if (false)
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector2 hitboxPos = Projectile.Center + Projectile.rotation.ToRotationVector2() * (-8 + i * 9 + MathHelper.Lerp(-10, 0, Projectile.scale));

                    float radius = Projectile.height * 0.1f;

                    for (int j = 0; j < 120; j++)
                    {
                        Main.EntitySpriteDraw(squareTex, hitboxPos + ((j / 120f * MathHelper.TwoPi).ToRotationVector2() * radius) - Main.screenPosition, null, Color.Red, 0, squareTex.Size() * 0.5f, 0.5f, SpriteEffects.None);
                    }
                }
            }

            return false;
        }
    }
}
