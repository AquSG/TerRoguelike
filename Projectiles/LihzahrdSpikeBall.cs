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

namespace TerRoguelike.Projectiles
{
    public class LihzahrdSpikeBall : ModProjectile, ILocalizedModType
    {
        public int direction;
        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 420;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.hide = true;
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }
        public override void OnSpawn(IEntitySource source)
        {
            direction = Main.rand.NextBool() ? 1 : -1;
            Projectile.ai[1] = Projectile.ai[0];
        }
        public override void AI()
        {
            if (Projectile.ai[1] == Projectile.ai[0])
                Projectile.localAI[0] = Projectile.velocity.Y;

            if (Projectile.timeLeft < 360)
                Projectile.tileCollide = true;

            Projectile.velocity.Y += 0.15f;
            Projectile.rotation += MathHelper.Clamp((Math.Abs(Projectile.localAI[0])) * direction * 0.02f, -0.2f, 0.2f);
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.ai[0]--;
            
            if (Projectile.ai[0] <= -1)
            {
                SoundEngine.PlaySound(SoundID.Dig with { Volume = 0.35f, Pitch = 0.1f }, Projectile.Center);
                return true;
            }

            Projectile.timeLeft = 420;
            // If the projectile hits the left or right side of the tile, reverse the X velocity
            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
            {
                Projectile.velocity.X = -oldVelocity.X * 0.8f;
            }
            // If the projectile hits the top or bottom side of the tile, reverse the Y velocity
            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
            {
                Projectile.velocity.Y = -oldVelocity.Y * Main.rand.NextFloat(0.78f, 0.85f);
            }
            Projectile.localAI[0] = Projectile.velocity.Y;
            return false;
        }
        public override Color? GetAlpha(Color lightColor) => Color.Lerp(lightColor, Color.White, 0f);
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 5; i++)
            {
                int dustType = Main.rand.NextBool() ? DustID.t_Lihzahrd : DustID.OrangeTorch;
                Vector2 randVel = Main.rand.NextVector2CircularEdge(1, 1) * Main.rand.NextFloat(2f, 3f);
                int d = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, dustType, randVel.X, randVel.Y, 0, default(Color), 1.5f);
                Dust dust = Main.dust[d];
                dust.noGravity = true;
                dust.noLight = true;
                dust.noLightEmittence = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            var tex = TextureAssets.Projectile[Type].Value;
            TerRoguelikeUtils.StartAdditiveSpritebatch();
            Main.EntitySpriteDraw(tex, Projectile.Center + (Projectile.oldPosition - Projectile.position) * 0.5f - Main.screenPosition, null, Color.Yellow, Projectile.rotation, tex.Size() * 0.5f, Projectile.scale * 1.5f, SpriteEffects.None);
            TerRoguelikeUtils.StartVanillaSpritebatch();
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, (Color)GetAlpha(Lighting.GetColor(Projectile.Center.ToTileCoordinates())), Projectile.rotation, tex.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            return false;
        }
    }
}