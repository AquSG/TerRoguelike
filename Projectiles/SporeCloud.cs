﻿using System;
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
using Terraria.DataStructures;
using static TerRoguelike.Managers.TextureManager;

namespace TerRoguelike.Projectiles
{
    public class SporeCloud : ModProjectile, ILocalizedModType
    {
        public Texture2D glowTex;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 5;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 240;
            Projectile.direction = Main.rand.NextBool() ? -1 : 1;
            Projectile.penetrate = -1;
            glowTex = TexDict["CircularGlow"];
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.frame = Main.rand.Next(30);
        }
        public override void AI()
        {
            Projectile.frameCounter++;
            Projectile.frame = (Projectile.frameCounter % 30) / (Main.projFrames[Type] + 1);
            Projectile.rotation += MathHelper.PiOver2 * 0.02f * Projectile.velocity.Length() * Projectile.direction;
            Projectile.velocity *= 0.98f;
            if (Projectile.timeLeft > 60 && Main.rand.NextBool(5))
            {
                int d = Dust.NewDust(Projectile.position - new Vector2(12), Projectile.width + 24, Projectile.height + 24, Projectile.ModProj().hostileTurnedAlly ? DustID.CoralTorch : DustID.GreenTorch, 0, 0, 0, default, 2f);
                Dust dust = Main.dust[d];
                dust.noGravity = true;
                dust.noLight = true;
                dust.noLightEmittence = true;
            }

            if (Projectile.timeLeft == 45 && Projectile.ModProj().hostileTurnedAlly)
            {
                for (int i = 0; i < 8; i++)
                {
                    Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.DungeonSpirit, Scale: 1.4f);
                    d.noGravity = true;
                }
                Projectile.Kill();
            }
        }
        public override bool? CanDamage() => Projectile.timeLeft > 45 ? null : false;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D sporeTex = TextureAssets.Projectile[Type].Value;
            int frameHeight = sporeTex.Height / Main.projFrames[Type];

            float opacity = MathHelper.Clamp(MathHelper.Lerp(0, 1f, Projectile.timeLeft / 60f), 0, 1f);
            float scale = 0.2f;

            Projectile.ModProj().EliteSpritebatch(true, BlendState.Additive);

            Main.EntitySpriteDraw(glowTex, Projectile.Center - Main.screenPosition, null ,Color.Green * opacity * 0.8f, 0f, glowTex.Size() * 0.5f, 0.45f * scale * Projectile.scale, SpriteEffects.None, 0);

            Projectile.ModProj().EliteSpritebatch();

            Main.EntitySpriteDraw(sporeTex, Projectile.Center - Main.screenPosition, new Rectangle(0, frameHeight * Projectile.frame, sporeTex.Width, frameHeight), Color.White * opacity, Projectile.rotation, new Vector2(sporeTex.Width * 0.5f, frameHeight * 0.5f), Projectile.scale, SpriteEffects.None, 0);
            
            return false;
        }
    }
}
