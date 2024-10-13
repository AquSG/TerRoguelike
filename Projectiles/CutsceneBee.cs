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
    public class CutsceneBee : ModProjectile, ILocalizedModType
    {
        public int maxTimeLeft;
        public float timeOffset;
        public override string Texture => "TerRoguelike/Projectiles/Bee";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
        }
        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = maxTimeLeft = 600;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hide = true;
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            timeOffset = Main.rand.NextFloat(MathHelper.TwoPi);
            Projectile.damage = 0;
        }
        public override void AI()
        {
            Projectile.frameCounter++;
            Projectile.frame = Projectile.frameCounter / 3 % Main.projFrames[Type];
            if (Projectile.ai[2] < 240)
            {
                Vector2 targetPos = new Vector2(Projectile.ai[0], Projectile.ai[1]);
                Projectile.rotation = Projectile.velocity.ToRotation().AngleTowards((targetPos - Projectile.Center).ToRotation(), 0.3f);
                Projectile.velocity = Projectile.rotation.ToRotationVector2() * Projectile.velocity.Length();
            }
            else
            {
                Projectile.velocity = Projectile.velocity.RotatedBy(Math.Cos(Projectile.timeLeft * 0.1f) * 0.05f * Main.rand.NextFloat());
                Projectile.rotation = Projectile.velocity.ToRotation();
            }

            Projectile.ai[2]++;
        }
        public override bool? CanDamage() => false;
        public override bool PreDraw(ref Color lightColor)
        {
            lightColor = Lighting.GetColor(Projectile.Center.ToTileCoordinates());
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            int frameHeight = tex.Height / Main.projFrames[Type];
            Rectangle frame = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);

            float scaleMultiplier = Projectile.timeLeft > 60 ? MathHelper.Clamp((maxTimeLeft - Projectile.timeLeft) / 10f, 0, 1) : MathHelper.Clamp(Projectile.timeLeft / 60f, 0, 1);

            float opacity = Math.Min(Projectile.frameCounter / 15f, 1f);
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, frame, Color.Lerp(lightColor, Color.White, 0.2f) * opacity, Projectile.rotation, frame.Size() * 0.5f, Projectile.scale * scaleMultiplier, Math.Sign(Projectile.velocity.X) == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None);
            return false;
        }
    }
}
