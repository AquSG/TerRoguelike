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
using TerRoguelike.Items.Common;
using TerRoguelike.Utilities;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.DataStructures;
using TerRoguelike.Particles;

namespace TerRoguelike.Projectiles
{
    public class PhantasmalBolt : ModProjectile, ILocalizedModType
    {
        public int maxTimeLeft;
        public float timeOffset;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 5;
        }
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 480;
            Projectile.penetrate = -1;
            Projectile.ignoreWater = true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.MaxUpdates = 2;
            Projectile.velocity /= Projectile.MaxUpdates;
            maxTimeLeft = Projectile.timeLeft;
            Projectile.tileCollide = false;
            Projectile.rotation = Projectile.velocity.ToRotation();
            for (int i = 0; i < 8; i++)
            {
                int time = i * 2;
                if (Projectile.timeLeft % (1 * Projectile.MaxUpdates) == 0)
                {
                    Vector2 offset = Main.rand.NextVector2Circular(2, 2);
                    ParticleManager.AddParticle(new Ball(
                        Projectile.Center + offset, offset + Projectile.velocity * Projectile.MaxUpdates,
                        time, Color.Cyan, new Vector2(0.15f), 0, 0.96f, 15));
                }
            }
        }
        public override void AI()
        {
            if (Projectile.numUpdates != 0)
                return;

            Projectile.frameCounter++;
            Projectile.frame = Projectile.frameCounter / 5 % Main.projFrames[Type];

            if (Projectile.timeLeft % (3 * Projectile.MaxUpdates) == 0)
            {
                Vector2 offset = Main.rand.NextVector2Circular(1.6f, 1.6f);
                ParticleManager.AddParticle(new Ball(
                    Projectile.Center + offset, offset + Projectile.velocity * Projectile.MaxUpdates,
                    30, Color.Cyan, new Vector2(0.1f), 0, 0.96f, 15));
            }
        }
        public override void OnKill(int timeLeft)
        {

        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            
            int frameHeight = tex.Height / Main.projFrames[Type];
            Rectangle frame = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);
            Vector2 origin = frame.Size() * new Vector2(0.65f, 0.5f);

            int length = Math.Min((maxTimeLeft - Projectile.timeLeft) / Projectile.MaxUpdates + 1, 10);
            for (int i = 0; i < length; i++)
            {
                float completion = 1 - (i / (float)length);
                Vector2 scale = new Vector2(1, completion) * Projectile.scale;
                Main.EntitySpriteDraw(tex, Projectile.Center - Projectile.rotation.ToRotationVector2() * i * 20 - Main.screenPosition, frame, Color.White * completion, Projectile.rotation, origin, scale, SpriteEffects.None);
            }
            return false;
        }
    }
}
