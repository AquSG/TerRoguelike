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
using TerRoguelike.NPCs.Enemy.Boss;

namespace TerRoguelike.Projectiles
{
    public class Iceflake : ModProjectile, ILocalizedModType
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 3;
            ProjectileID.Sets.TrailCacheLength[Type] = 20;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 240 * 4;
            Projectile.direction = Main.rand.NextBool() ? -1 : 1;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 3;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.frame = Main.rand.Next(0, Main.projFrames[Type]);
            if (source is EntitySource_Parent parentSource)
            {
                if (parentSource.Entity is NPC)
                {
                    if (Main.npc[parentSource.Entity.whoAmI].type == ModContent.NPCType<IceQueen>())
                        Projectile.localAI[0] = 1;
                }
                else if (parentSource.Entity is Projectile)
                {
                    if (Main.projectile[parentSource.Entity.whoAmI].type == ModContent.ProjectileType<IceBomb>())
                    {
                        Projectile.localAI[0] = 1;
                        if (TerRoguelikeUtils.ParanoidTileRetrieval(Projectile.Center.ToTileCoordinates()).IsTileSolidGround(true))
                        {
                            Projectile.localAI[1] = 1;
                            Projectile.Kill();
                        }
                    }
                }
            }
        }

        public override void AI()
        {
            if (Projectile.numUpdates != 0)
                return;

            if ((Projectile.timeLeft / Projectile.MaxUpdates) % 2 == 0 && Main.rand.NextBool())
            {
                int offset = 10;
                int d = Dust.NewDust(Projectile.position - Projectile.velocity * 2f - new Vector2((float)offset, (float)offset), Projectile.width + offset * 2, Projectile.height + offset * 2, 92, Projectile.velocity.X * 0.4f, Projectile.velocity.Y * 0.4f, 50, default(Color), 0.45f);
                Dust dust = Main.dust[d];
                dust.velocity *= 0.5f;
                dust.velocity += Projectile.velocity * 0.25f;
                dust.noLight = true;
                dust.noLightEmittence = true;
            }
        }
        public override void OnKill(int timeLeft)
        {
            if (Projectile.localAI[1] == 0)
                SoundEngine.PlaySound(SoundID.Item27 with { Volume = Projectile.localAI[0] == 1 ? 0.5f : 0.9f, MaxInstances = Projectile.localAI[0] == 1 ? 10 : 1}, Projectile.Center);

            for (int i = 0; i < 16; i++)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 92, 0f, 0f, 0, default(Color), 1.5f);
                Dust d = Main.dust[dust];
                d.velocity *= 2f;
                d.noGravity = true;
                d.noLight = true;
                d.noLightEmittence = true;
            }
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            int frameHeight = tex.Height / 3; ;
            Rectangle frame = new Rectangle(0, Projectile.frame * frameHeight, tex.Width, frameHeight);
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float ratio = 1 - (i / (float)Projectile.oldPos.Length);
                float opacity = 1f;
                if (i != 0)
                {
                    opacity *= 0.5f * ratio;
                }
                float scale = 1f;
                if (i != 0)
                {
                    scale = 0.85f * ratio;
                }
                Main.EntitySpriteDraw(tex, Projectile.oldPos[i] - Main.screenPosition + (new Vector2(Projectile.width, Projectile.height) * 0.5f), frame, Color.White * opacity, Projectile.oldRot[i], new Vector2(tex.Width, frameHeight) * 0.5f, Projectile.scale * scale, SpriteEffects.None);
            }
            return false;
        }
    }
}
