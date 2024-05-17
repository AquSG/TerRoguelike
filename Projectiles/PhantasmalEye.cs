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
    public class PhantasmalEye : ModProjectile, ILocalizedModType
    {
        public int maxTimeLeft;
        public float timeOffset;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 1;
        }
        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 480;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.ModProj().killOnRoomClear = true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }
        public override void AI()
        {
            if (Projectile.timeLeft % 3 == 0)
            {
                Vector2 offset = Main.rand.NextVector2Circular(2, 2);
                ParticleManager.AddParticle(new Ball(
                    Projectile.Center + offset, offset, 
                    20, Color.Teal, new Vector2(0.25f), 0, 0.96f, 10));
            }
            if (Projectile.timeLeft % 6 == 0)
            {
                Color particleColor = Color.Lerp(Color.Teal, Color.White, 0.2f);
                ParticleManager.AddParticle(new Wriggler(
                    Projectile.Center - Projectile.rotation.ToRotationVector2() * 12 + (Vector2.UnitY * Main.rand.NextFloat(-4, 4)).RotatedBy(Projectile.rotation), Projectile.velocity * 0.5f,
                    26, particleColor, new Vector2(0.5f), Main.rand.Next(4), Projectile.rotation, 0.98f, 16, 
                    Main.rand.NextBool() ? SpriteEffects.None : SpriteEffects.FlipVertically));
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                Color outlineColor = Color.Lerp(Color.Teal, Color.Cyan, 0.4f);
                Vector2 offset = Main.rand.NextVector2Circular(2.5f, 2.5f);
                ParticleManager.AddParticle(new BallOutlined(
                    Projectile.Center - offset, offset,
                    21, outlineColor, Color.White * 0.75f, new Vector2(0.3f), 5, 0, 0.96f, 15));
            }
            
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 origin = tex.Size() * new Vector2(0.7f, 0.5f);

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None);
            return false;
        }
    }
}
