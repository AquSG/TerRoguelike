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
using System.IO;

namespace TerRoguelike.Projectiles
{
    public class RockDebris : ModProjectile, ILocalizedModType
    {
        public Vector2 startVelocity;
        public int dir = 0;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 3;

        }
        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 600;
            Projectile.penetrate = 1;
            Projectile.hide = true;
            Projectile.tileCollide = false;
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }
        public override void OnSpawn(IEntitySource source)
        {
            startVelocity = Projectile.velocity;
            Projectile.frame = Main.rand.Next(Main.projFrames[Type]);
            Projectile.velocity = new Vector2(0, 2f);
            dir = Main.rand.NextBool() ? 1 : -1;
        }
        public override void AI()
        {
            Projectile.ai[0]++;
            Projectile.rotation += Projectile.velocity.Y * dir * 0.04f;
            if (Projectile.ai[0] < 60)
            {
                Projectile.velocity.Y *= 0.93f;
            }
            else
            {
                if (Projectile.velocity.Y < startVelocity.Y)
                {
                    Projectile.velocity.Y += 0.1f;
                }
                else if (Projectile.velocity.Y > startVelocity.Y)
                {
                    Projectile.velocity.Y = startVelocity.Y;
                }
                
                if (Projectile.ai[0] > 70)
                {
                    Projectile.tileCollide = true;
                }
            }
        }
    
        public override Color? GetAlpha(Color lightColor)
        {
            Color color = new Color(170, 170, 170);
            if (Projectile.timeLeft > 580)
                color *= 1f - ((Projectile.timeLeft - 580) / 20f);
            return color;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            for (int i = 0; i < 8; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Stone, 0, 0, 0, default, 1.5f);
            }
            SoundEngine.PlaySound(SoundID.Item70 with { Volume = 0.4f, MaxInstances = 3 }, Projectile.Center);
            return true;
        }
        public override bool? CanDamage() => Projectile.ai[0] > 60 ? null : false;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            int frameHeight = tex.Height / Main.projFrames[Type];
            Rectangle frame = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, frame, Projectile.GetAlpha(lightColor), Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            return false;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.WriteVector2(startVelocity);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            startVelocity = reader.ReadVector2();
        }
    }
}
