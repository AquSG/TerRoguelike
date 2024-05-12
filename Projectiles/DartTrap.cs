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
using static TerRoguelike.Managers.TextureManager;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using TerRoguelike.Particles;

namespace TerRoguelike.Projectiles
{
    public class DartTrap : ModProjectile, ILocalizedModType
    {
        public int maxTimeLeft;
        public int shootTime = 30;
        public int rotateInTime = 15;
        public int rotateOutTime = 15;
        public override void SetStaticDefaults()
        {

        }
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.MaxUpdates = 1;
            Projectile.timeLeft = maxTimeLeft = 45;
            Projectile.penetrate = -1;
            Projectile.hide = true;
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = MathHelper.PiOver2;
            Projectile.ai[0] = Projectile.velocity.ToRotation();
            Projectile.ai[1] = Projectile.velocity.Length();
            Projectile.velocity = Vector2.Zero;
        }
        public override void AI()
        {
            int time = maxTimeLeft - Projectile.timeLeft;
            if (time < rotateInTime)
            {
                Projectile.rotation = Projectile.rotation.AngleTowards(Projectile.ai[0], Math.Abs(AngleSizeBetween(MathHelper.PiOver2, Projectile.ai[0])) / rotateInTime);
            }
            else if (time >= shootTime)
            {
                Projectile.rotation = Projectile.rotation.AngleTowards(-MathHelper.PiOver2, Math.Abs(AngleSizeBetween(Projectile.ai[0], -MathHelper.PiOver2)) / rotateOutTime);
                if (time == shootTime)
                {
                    Projectile.rotation = Projectile.ai[0];
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + Projectile.ai[0].ToRotationVector2() * 16, Projectile.rotation.ToRotationVector2() * Projectile.ai[1], ModContent.ProjectileType<Dart>(), Projectile.damage, 0);

                    SoundEngine.PlaySound(SoundID.Item108 with { Volume = 0.5f, Pitch = 0.1f, PitchVariance = 0, MaxInstances = 100, SoundLimitBehavior = SoundLimitBehavior.IgnoreNew }, Projectile.Center + Projectile.ai[0].ToRotationVector2() * -390);

                    ParticleManager.AddParticle(new ThinSpark(
                    Projectile.Center + Projectile.ai[0].ToRotationVector2() * 26, Vector2.Zero, 30, Color.LightGoldenrodYellow * 0.6f, new Vector2(0.1f, 0.13f) * 1.05f, MathHelper.PiOver2, true, false));

                    ParticleManager.AddParticle(new ThinSpark(
                        Projectile.Center + Projectile.ai[0].ToRotationVector2() * 40, Vector2.Zero, 30, Color.LightGoldenrodYellow * 0.6f, new Vector2(0.2f, 0.13f) * 1.05f, 0, true, false));
                }
            }
            else
            {
                ParticleManager.AddParticle(new ThinSpark(
                    Projectile.Center + Projectile.ai[0].ToRotationVector2() * 26, Vector2.Zero, 30, Color.Goldenrod * 0.2f, new Vector2(0.1f, 0.13f), MathHelper.PiOver2, true, false));

                ParticleManager.AddParticle(new ThinSpark(
                    Projectile.Center + Projectile.ai[0].ToRotationVector2() * 40, Vector2.Zero, 30, Color.Goldenrod * 0.2f, new Vector2(0.2f, 0.13f), 0, true, false));
            }
        }
        public override bool? CanDamage() => false;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            SpriteEffects spriteEffects = Math.Abs(MathHelper.WrapAngle(Projectile.rotation)) < MathHelper.PiOver2 ? SpriteEffects.None : SpriteEffects.FlipVertically;
            Color color = Lighting.GetColor((Projectile.Center + Projectile.ai[0].ToRotationVector2() * 24).ToTileCoordinates());

            Vector2 scale = Vector2.One;
            int time = maxTimeLeft - Projectile.timeLeft;
            float interpolant = 1;
            if (time < 5)
            {
                interpolant *= time / 5f;
            }
            else if (Projectile.timeLeft < 5)
            {
                interpolant *= Projectile.timeLeft / 5f;
            }
            scale.Y *= interpolant;

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, color, Projectile.rotation, new Vector2(0, tex.Height * 0.5f), scale, spriteEffects);

            return false;
        }
    }
}