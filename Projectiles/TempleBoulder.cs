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
using Terraria.ModLoader.IO;
using Terraria.GameContent;
using static TerRoguelike.Managers.TextureManager;

namespace TerRoguelike.Projectiles
{
    public class TempleBoulder : ModProjectile, ILocalizedModType
    {
        public Texture2D lightTex;
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 240;
            Projectile.penetrate = -1;
            lightTex = TexDict["TempleBoulderGlow"];
        }
        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.Item69 with { Volume = 0.9f }, Projectile.Center);
        }
        public override void AI()
        {
            Projectile.rotation += MathHelper.Clamp(Projectile.velocity.X * 0.08f, -0.3f, 0.3f);
            if (Projectile.timeLeft < 230)
                Projectile.velocity.Y += 0.24f;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {

            if ((Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon && Math.Abs(oldVelocity.X) > 3f) || (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon && Math.Abs(oldVelocity.Y) > 3f))
                SoundEngine.PlaySound(SoundID.Item70 with { Volume = 0.4f }, Projectile.Center);

            // If the projectile hits the left or right side of the tile, reverse the X velocity
            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
            {
                Projectile.velocity.X = -oldVelocity.X * 0.8f;
            }
            // If the projectile hits the top or bottom side of the tile, reverse the Y velocity
            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
            {
                Projectile.velocity.Y = -oldVelocity.Y * 0.6f;
                Projectile.velocity.X *= 0.9f;
            }
            return false;
        }
        public override bool? CanDamage() => Projectile.timeLeft > 60 ? null : false;
        public override Color? GetAlpha(Color lightColor)
        {
            return lightColor * MathHelper.Clamp(Projectile.timeLeft / 60f, 0, 1f);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            float opacity = MathHelper.Clamp((Projectile.timeLeft - 90) / 90f, 0, 1f);

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, tex.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            Main.EntitySpriteDraw(lightTex, Projectile.Center - Main.screenPosition, null, Color.White * opacity, Projectile.rotation, tex.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            return false;
        }
    }
}
