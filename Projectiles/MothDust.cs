using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using static TerRoguelike.Managers.TextureManager;

namespace TerRoguelike.Projectiles
{
    public class MothDust : ModProjectile, ILocalizedModType
    {
        //almost everything in this is just visuals. the hitbox is active for 1/4 of a second after 30 frames pass, and is a big square
        public override string Texture => "TerRoguelike/Projectiles/InvisibleProj";
        public List<Vector2> starPositions = new List<Vector2>(); // purely visual 
        public List<Vector2> starVelocities = new List<Vector2>(); // purely visual
        public List<Color> starColors = new List<Color>(); // purely visual
        public List<float> starRotations = new List<float>(); // purely visual
        public List<float> starScales = new List<float>(); // purely visual
        public List<int> starLifetimeOffsets = new List<int>(); // purely visual
        public float randomSmokeRotation = -100f; // purely visual
        public float MaxScale = -1f;
        public Texture2D smokeTex;
        public Texture2D starTex;
        public int maxTimeLeft = 240;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 5;
        }
        public override void SetDefaults()
        {
            Projectile.width = 100;
            Projectile.height = 100;
            Projectile.timeLeft = maxTimeLeft;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            smokeTex = TexDict["Smoke"].Value;
            starTex = TexDict["CrossGlow"].Value;
        }

        public override void OnSpawn(IEntitySource source)
        {
            //scale support
            MaxScale = Projectile.scale * 1f;
            Projectile.position = Projectile.Center + new Vector2(-50 * MaxScale, -50 * MaxScale);
            Projectile.width = (int)(Projectile.width * MaxScale);
            Projectile.height = (int)(Projectile.height * MaxScale);
            for (int i = 0; i < (int)(10 * MaxScale); i++)
            {
                Vector2 starVector = Main.rand.NextVector2Circular(2f, 2f) * MaxScale;
                starPositions.Add(starVector);
                starVelocities.Add(starVector);
                starRotations.Add(Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi + float.Epsilon));
                starLifetimeOffsets.Add(Main.rand.Next(50));
                starColors.Add(Color.Lerp(Color.HotPink, Color.Blue, Main.rand.NextFloat(1f + float.Epsilon)));
                starScales.Add(Main.rand.NextFloat(0.8f, 1f + float.Epsilon));
            }

            randomSmokeRotation = Main.rand.NextFloatDirection();
        }
        public override void AI()
        {
            Projectile.velocity *= 0.992f;

            for (int i = 0; i < starPositions.Count; i++)
            {
                if (starRotations[i] > 0)
                    starRotations[i] += (starVelocities[i].Length() + Projectile.velocity.Length()) * 0.02f;
                else
                    starRotations[i] -= (starVelocities[i].Length() + Projectile.velocity.Length()) * 0.02f;

                starPositions[i] += starVelocities[i];
                starVelocities[i] *= 0.975f;
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CircularHitboxCollision(Projectile.Center, targetHitbox, Projectile.width / 3);
        public override bool? CanDamage() => Projectile.timeLeft <= maxTimeLeft - 20 && Projectile.timeLeft >= 50 ? (bool?)null : false;
        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            for (int i = 0; i < starPositions.Count; i++)
            {
                float opacity = MathHelper.Lerp(0, 1f, MathHelper.Clamp((Projectile.timeLeft - starLifetimeOffsets[i] - 50) / 30f, 0f, 1f));
                Main.EntitySpriteDraw(starTex, starPositions[i] + Projectile.Center - Main.screenPosition, null, starColors[i] * opacity, starRotations[i], starTex.Size() * 0.5f, 1f * starScales[i], SpriteEffects.None);
            }
            Vector2 smokeOffset = Vector2.UnitY * -16;
            Color smokeColor = Color.HotPink;

            float smokeOpacity = 1f;
            if (Projectile.timeLeft > maxTimeLeft - 30)
            {
                smokeOpacity = MathHelper.Lerp(0f, 1f, MathHelper.Clamp((maxTimeLeft - Projectile.timeLeft) / 30f, 0, 1f));
            }
            else if (Projectile.timeLeft < 60)
            {
                smokeOpacity = MathHelper.Lerp(0f, 1f, MathHelper.Clamp(Projectile.timeLeft / 60f, 0, 1f));
            }
            smokeOpacity = (float)Math.Sqrt(smokeOpacity);
            Main.EntitySpriteDraw(smokeTex, Projectile.Center - Main.screenPosition + smokeOffset, null, smokeColor * smokeOpacity * 0.6f, 0, smokeTex.Size() * 0.5f, MaxScale * 0.45f + (smokeOpacity * 0.5f), SpriteEffects.None);
            Main.EntitySpriteDraw(smokeTex, Projectile.Center - Main.screenPosition + smokeOffset + new Vector2(0, 16), null, smokeColor * smokeOpacity * 0.75f, randomSmokeRotation, smokeTex.Size() * 0.5f, (MaxScale * 0.5f) + (smokeOpacity * 0.5f), SpriteEffects.None);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
    }
}
