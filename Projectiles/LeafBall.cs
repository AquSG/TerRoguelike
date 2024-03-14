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
    public class LeafBall : ModProjectile, ILocalizedModType
    {
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 300;
            Projectile.penetrate = 1;
        }
        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.DD2_GoblinBomberThrow with { Volume = 1f }, Projectile.Center);
            Projectile.localAI[0] = Math.Sign(Projectile.velocity.X);
            if (Projectile.localAI[0] == 0)
                Projectile.localAI[0] = 1;
            for (int i = 0; i < 8; i++)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Grass, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), Projectile.alpha, default(Color), 1f);
                Dust dust = Main.dust[d];
                dust.noLightEmittence = true;
                dust.noLight = true;
            }
        }
        public override void AI()
        {
            if ((int)Projectile.ai[0] % 2 == 0)
                Projectile.velocity.X *= 0.984f;
            else
                Projectile.velocity.Y *= 0.984f;
            
            Projectile.rotation += 0.025f * Projectile.localAI[0] * Projectile.velocity.Length();
            if (Projectile.timeLeft % 10 == 0 && Main.rand.NextBool())
            {
                Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(0, 0.5f), GoreID.TreeLeaf_Normal, 1f);
            }
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Grass with { Volume = 1f, Pitch = -0.5f, PitchVariance = 0.1f }, Projectile.Center);
            for (int i = 0; i < 12; i++)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Grass, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), Projectile.alpha, default(Color), 1f);
                Dust dust = Main.dust[d];
                dust.noLightEmittence = true;
                dust.noLight = true;
            }
            for (int i = 0; i < 4; i++)
            {
                Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(0, 0.5f) + (Projectile.velocity * 0.5f), GoreID.TreeLeaf_Normal, 1f);
            }
            float rot = Projectile.ai[0] * MathHelper.PiOver2;
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + new Vector2(0, -22).RotatedBy(rot), new Vector2(0, 2).RotatedBy(rot), ModContent.ProjectileType<VineWall>(), Projectile.damage, 0f, -1, Projectile.ai[0]);
        }
        public override bool PreDraw(ref Color lightColor)
{
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, tex.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            return false;
        }
    }
}
