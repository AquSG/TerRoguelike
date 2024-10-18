using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using static TerRoguelike.Managers.TextureManager;
using TerRoguelike.Managers;
using TerRoguelike.Particles;
using TerRoguelike.MainMenu;

namespace TerRoguelike.Projectiles
{
    public class ResidualBurden : ModProjectile, ILocalizedModType
    {
        //almost everything in this is just visuals. the hitbox is active for 1/4 of a second after 30 frames pass, and is a big square
        public override string Texture => "TerRoguelike/Projectiles/InvisibleProj";
        public List<Vector2> rockPosition = new List<Vector2>();
        public List<float> rockRotation = new List<float>();
        public List<int> rockFrame = new List<int>();
        public List<int> rockDirection = new List<int>();
        public Texture2D rockTex;
        public int maxTimeLeft;
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 12;
            Projectile.timeLeft = maxTimeLeft = 420;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            rockTex = TexDict["RockDebris"];
            Projectile.hide = true;
            Projectile.manualDirectionChange = true;
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            Projectile.frame = Main.rand.Next(3);
            Projectile.direction = Main.rand.NextBool() ? -1 : 1;
        }
        public override void AI()
        {
            Projectile.localAI[0]++;
            if (Projectile.timeLeft % 3 == 0)
            {
                Vector2 scale = new Vector2(0.12f);
                if (Projectile.timeLeft < 45)
                {
                    scale *= Projectile.timeLeft / 45f;
                }
                if (Projectile.localAI[0] < 10)
                {
                    scale *= Projectile.localAI[0] / 10f;
                }

                ParticleManager.AddParticle(new Glow(
                Projectile.Center, Projectile.velocity, 5, Color.Teal * 0.4f, scale * Projectile.scale, 0, 0.96f, 3, true),
                ParticleManager.ParticleLayer.BehindTiles);
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CircularHitboxCollision(Projectile.Center, targetHitbox, Projectile.width * 0.4f * Projectile.scale);
        public override bool? CanDamage() => Projectile.timeLeft > 45 ? null : false;
        public override bool PreDraw(ref Color lightColor)
        {
            int frameHeight = rockTex.Height / 3;
            float scale = Projectile.scale;
            if (Projectile.timeLeft < 60)
            {
                scale *= Projectile.timeLeft / 60f;
            }
            if (Projectile.localAI[0] < 10)
            {
                scale *= Projectile.localAI[0] / 10f;
            }
            Color color = Color.Lerp(Color.Teal, Color.Green, 0.34f);
            color.A = 50;

            Rectangle frame = new Rectangle(0, frameHeight * Projectile.frame, rockTex.Width, frameHeight - 2);
            Main.EntitySpriteDraw(rockTex, Projectile.Center - Main.screenPosition, frame, color, Main.GlobalTimeWrappedHourly * Projectile.direction + Projectile.rotation, frame.Size() * 0.5f, scale, Projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);

            return false;
        }
    }
}
