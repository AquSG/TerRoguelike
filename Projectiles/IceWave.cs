using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TerRoguelike.Managers;
using TerRoguelike.Particles;
using TerRoguelike.Utilities;

namespace TerRoguelike.Projectiles
{
    public class IceWave : ModProjectile, ILocalizedModType
    {
        public Vector2 startVelocity;
        public int maxTimeLeft;
        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 48;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = maxTimeLeft = 600;
            Projectile.penetrate = -1;
        }
        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.Item8 with { Volume = 0.9f }, Projectile.Center);
            Projectile.rotation = Projectile.velocity.ToRotation();
            startVelocity = Projectile.velocity;

            for (int i = 0; i < 6; i++)
            {
                ParticleManager.AddParticle(new Snow(
                        Projectile.Center, Main.rand.NextVector2CircularEdge(3, 3) * Main.rand.NextFloat(0.66f, 1f),
                        60, Color.White * 0.66f, new Vector2(Main.rand.NextFloat(0.035f, 0.045f)), Main.rand.NextFloat(MathHelper.TwoPi), 0.96f, 0.04f, 60, 0, false));
            }
        }
        public override void AI()
        {
            Projectile.velocity = startVelocity * MathHelper.Clamp(MathHelper.Lerp(0.5f, 1f, (maxTimeLeft - Projectile.timeLeft) / 50f), 0.5f, 1f);
            Projectile.localAI[1]++;
            if (Projectile.localAI[1] == 5)
                SoundEngine.PlaySound(SoundID.Item30 with { Volume = 0.4f }, Projectile.Center);

            Projectile.localAI[0]++;
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (Projectile.localAI[0] > 20)
            {
                Projectile.localAI[0] = Main.rand.Next(8);
                Vector2 spawnPos = Projectile.Center + (Vector2.UnitY * Main.rand.Next(-32, 33)).RotatedBy(Projectile.rotation);
                if (!TerRoguelikeUtils.ParanoidTileRetrieval(spawnPos.ToTileCoordinates()).IsTileSolidGround(true))
                {
                    ParticleManager.AddParticle(new Snow(
                        spawnPos, Projectile.rotation.ToRotationVector2() * -Main.rand.NextFloat(2, 3),
                        600, Color.White * 0.66f, new Vector2(Main.rand.NextFloat(0.03f, 0.04f)), Main.rand.NextFloat(MathHelper.TwoPi), 0.96f, 0.04f, 180, 0));
                }

            }
            if (Projectile.timeLeft % 1 == 0)
            {
                for (int i = -1; i <= 1; i += 2)
                {
                    float rotation = Projectile.rotation + MathHelper.Pi + (i * MathHelper.Pi * 0.01f);
                    ParticleManager.AddParticle(new ThinSpark(
                        Projectile.Center + (Vector2.UnitY * 24 * i).RotatedBy(rotation),
                        (rotation.ToRotationVector2() * 13f) + Projectile.velocity, 
                        20, Color.Cyan * 0.85f, new Vector2(0.1f, 0.1f), rotation, true, false));

                    rotation = Projectile.rotation + MathHelper.Pi + (i * MathHelper.Pi * 0.036f);
                    ParticleManager.AddParticle(new ThinSpark(
                        Projectile.Center + (Vector2.UnitY * 40 * i).RotatedBy(rotation),
                        (rotation.ToRotationVector2() * 13f) + Projectile.velocity,
                        20, Color.Cyan * 0.62f, new Vector2(0.1f, 0.1f), rotation, true, false));
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, texture.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            return false;
        }
        public override Color? GetAlpha(Color lightColor) => Color.Lerp(Color.White, lightColor, 0.15f);
    }
}
