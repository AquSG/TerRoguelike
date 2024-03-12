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
using Terraria.DataStructures;
using Terraria.GameContent;

namespace TerRoguelike.Projectiles
{
    public class VineWall : ModProjectile, ILocalizedModType
    {
        Vector2 spawnPos;
        Vector2 startVel;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 1600;
        }
        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 900;
            Projectile.penetrate = -1;
            Projectile.hide = true;
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }
        public override void OnSpawn(IEntitySource source)
        {
            spawnPos = Projectile.Center;
            startVel = Projectile.velocity;
        }
        public override void AI()
        {
            Projectile.hostile = true;
            Projectile.friendly = false;
            if (Projectile.ai[1] <= 2)
                Projectile.velocity = startVel;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.timeLeft < 720)
            {
                Projectile.ai[1]++;
                if (Projectile.ai[1] > 1)
                {
                    Projectile.velocity = Vector2.Zero;
                    return false;
                }
            }
            Projectile.velocity = oldVelocity;
            return false;
        }
        public override void OnKill(int timeLeft)
        {
            bool soundPlayed = false;

            Texture2D tex = TextureAssets.Projectile[Type].Value;
            int length = (int)Projectile.ai[0] % 2 == 0 ? (int)Math.Abs(Projectile.Center.Y - spawnPos.Y) : (int)Math.Abs(Projectile.Center.X - spawnPos.X);
            Vector2 pos = spawnPos;
            float rot = Projectile.ai[0] * MathHelper.PiOver2;
            int endEase = length - 8;
            for (int i = 0; i < length; i += 4)
            {
                Vector2 posOffset = (Vector2.UnitY * (i + (tex.Height * 0.5f))).RotatedBy(rot);
                float interpolant = ((float)Math.Sin((float)i / tex.Height));
                float depthInterpolant = Math.Abs((float)Math.Sin((float)i / tex.Height * 0.5f));
                float depth = MathHelper.Lerp(0.7f, 1f, depthInterpolant);
                posOffset += (Vector2.UnitX * MathHelper.Lerp(-8.5f, 8.5f, interpolant)).RotatedBy(rot);
                Vector2 realPos = pos + posOffset;

                if (!soundPlayed && i > length * 0.5f)
                {
                    soundPlayed = true;
                    SoundEngine.PlaySound(SoundID.Dig with { Volume = 1f }, realPos);
                    SoundEngine.PlaySound(SoundID.Grass with { Volume = 1f }, realPos);
                }

                Dust.NewDustPerfect(realPos, DustID.Grass);
                if (i % 8 == 0)
                    Gore.NewGore(Projectile.GetSource_FromThis(), realPos, Vector2.Zero, GoreID.TreeLeaf_Normal);
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            int length = (int)Projectile.ai[0] % 2 == 0 ? (int)Math.Abs(Projectile.Center.Y - spawnPos.Y) : (int)Math.Abs(Projectile.Center.X - spawnPos.X);
            Vector2 pos = spawnPos;
            float rot = Projectile.ai[0] * MathHelper.PiOver2;
            int endEase = length - 8;
            for (int i = 0; i < length; i += 8)
            {
                Vector2 posOffset = (Vector2.UnitY * (i + (tex.Height * 0.5f))).RotatedBy(rot);
                Vector2 scale = new Vector2(1f);
                float interpolant = ((float)Math.Sin((float)i / tex.Height));
                float depthInterpolant = Math.Abs((float)Math.Sin((float)i / tex.Height * 0.5f));
                float depth = MathHelper.Lerp(0.7f, 1f, depthInterpolant);
                scale.X *= depth;
                posOffset += (Vector2.UnitX * MathHelper.Lerp(-8.5f, 8.5f, interpolant)).RotatedBy(rot);
                //if (i > endEase)
                //{
                    //scale.X *= 1f - ((i - endEase) / (length - (float)endEase));
                //}
                Vector2 realPos = pos + posOffset;
                if ((realPos - (targetHitbox.ClosestPointInRect(realPos))).Length() < tex.Height * 0.25f * scale.X)
                    return true;
            }

            return false;
        }
        public override bool PreDraw(ref Color lightColor)
{
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            int length = (int)Projectile.ai[0] % 2 == 0 ? (int)Math.Abs(Projectile.Center.Y - spawnPos.Y) : (int)Math.Abs(Projectile.Center.X - spawnPos.X);
            Vector2 pos = spawnPos;
            float rot = Projectile.ai[0] * MathHelper.PiOver2;
            int endEase = length - 8;
            for (int i = 0; i < length; i++)
            {
                Vector2 posOffset = (Vector2.UnitY * (i + (tex.Height * 0.5f))).RotatedBy(rot);
                int rectY = i % tex.Height;
                Rectangle rect = new Rectangle(0, rectY, tex.Width, 1);
                Vector2 scale = new Vector2(1f);
                float interpolant = ((float)Math.Sin((float)i / tex.Height));
                float depthInterpolant = Math.Abs((float)Math.Sin((float)i / tex.Height * 0.5f));
                float depth = MathHelper.Lerp(0.7f, 1f, depthInterpolant);
                float colorDepth = MathHelper.Lerp(0f, 1f, depthInterpolant);
                scale.X *= depth;
                Color color = Color.Lerp(Color.DarkGreen, Color.White, colorDepth);
                posOffset += (Vector2.UnitX * MathHelper.Lerp(-8.5f, 8.5f, interpolant)).RotatedBy(rot);
                if (i > endEase)
                {
                    scale.X *= 1f - ((i - endEase) / (length - (float)endEase));
                }
                Main.EntitySpriteDraw(tex, pos + posOffset - Main.screenPosition, rect, color, rot, tex.Size() * 0.5f, scale, SpriteEffects.None);
            }
            return false;
        }
    }
}
