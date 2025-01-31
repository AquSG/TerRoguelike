using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using static TerRoguelike.Managers.TextureManager;

namespace TerRoguelike.Projectiles
{
    public class NebulaSphere : ModProjectile, ILocalizedModType
    {
        public float turnMultiplier = 1f;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 5;
            ProjectileID.Sets.TrailingMode[Type] = 2;
            Main.projFrames[Type] = 4;
        }
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 480;
            Projectile.penetrate = 1;
            Projectile.ignoreWater = true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.ai[2] = -1;
            Projectile.rotation = Projectile.velocity.ToRotation();
            turnMultiplier = Projectile.velocity.Length();
            for (int i = 0; i < 10; i++)
            {
                Vector2 offset = (Main.rand.Next(3, 6) * Vector2.UnitX.RotatedBy(Main.rand.NextFloat(MathHelper.TwoPi)));
                Dust d = Dust.NewDustPerfect(Projectile.Center + offset, DustID.ShadowbeamStaff, offset.SafeNormalize(Vector2.UnitX) * 2, Projectile.alpha, Color.Purple, 1.2f);
                d.noGravity = true;
            }
        }
        public override void AI()
        {
            Projectile.frameCounter++;
            Projectile.frame = ((int)Projectile.frameCounter / 6) % Main.projFrames[Type];

            if (Projectile.ai[2] == -1)
                GetTarget();

            if (Projectile.ai[2] != -1)
            {
                float direction = 0;
                if (Projectile.ai[1] == 0)
                {
                    direction = (Main.player[(int)Projectile.ai[2]].Center - Projectile.Center).ToRotation();
                }
                else if (Projectile.ai[1] == 1)
                {
                    direction = (Main.npc[(int)Projectile.ai[2]].Center - Projectile.Center).ToRotation();
                }
                if (Math.Abs(AngleSizeBetween(Projectile.velocity.ToRotation(), direction)) < MathHelper.PiOver2)
                {
                    float newRot = Projectile.velocity.ToRotation().AngleTowards(direction, 0.0013f * turnMultiplier);
                    Projectile.velocity = (Vector2.UnitX * Projectile.velocity.Length()).RotatedBy(newRot); 
                }
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.ShadowbeamStaff, 0, 0, 0, Color.Purple, 1.2f);
            d.noGravity = true;
            d.noLight = true;
            d.noLightEmittence = true;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            SoundEngine.PlaySound(SoundID.NPCDeath3 with { Volume = 1f }, Projectile.Center);
            return true;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 15; i++)
            {
                Vector2 offset = (Main.rand.Next(3, 6) * Vector2.UnitX.RotatedBy(Main.rand.NextFloat(MathHelper.TwoPi)));
                Dust d = Dust.NewDustPerfect(Projectile.Center + offset, DustID.ShadowbeamStaff, offset.SafeNormalize(Vector2.UnitX) + (Projectile.oldVelocity * 0.6f), Projectile.alpha, Color.Purple, 1.5f);
                d.noGravity = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 offset = new Vector2(Projectile.width * 0.5f, Projectile.height * 0.5f);
            int frameHeight = tex.Height / Main.projFrames[Type];
            Rectangle frame = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);
            if (Projectile.ModProj().hostileTurnedAlly)
                StartVanillaSpritebatch();
            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Vector2 pos = Projectile.oldPos[i] + offset;
                float opacity = (1f - ((float)i / Projectile.oldPos.Length)) * 0.6f;
                Main.EntitySpriteDraw(tex, pos - Main.screenPosition, frame, Color.White * opacity, Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            }
            Projectile.ModProj().EliteSpritebatch();
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            return false;
        }

        public void GetTarget()
        {
            float closestTarget = 3200f;
            if (Projectile.hostile)
            {
                Projectile.ai[1] = 0;
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player player = Main.player[i];
                    if (player == null)
                        continue;
                    if (!player.active)
                        continue;
                    if (player.dead)
                        continue;

                    float distance = (Projectile.Center - player.Center).Length();
                    if (distance <= closestTarget)
                    {
                        if (Math.Abs(AngleSizeBetween(Projectile.rotation, (player.Center - Projectile.Center).ToRotation())) < MathHelper.PiOver2)
                        {
                            closestTarget = distance;
                            Projectile.ai[2] = i;
                        }
                    }
                }
            }
            else
            {
                Projectile.ai[1] = 1;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc == null)
                        continue;
                    if (!npc.active)
                        continue;
                    if (npc.life <= 0)
                        continue;
                    if (npc.immortal)
                        continue;

                    float distance = (Projectile.Center - npc.Center).Length();
                    if (distance <= closestTarget)
                    {
                        if (Math.Abs(AngleSizeBetween(Projectile.rotation, (npc.Center - Projectile.Center).ToRotation())) < MathHelper.PiOver2)
                        {
                            closestTarget = distance;
                            Projectile.ai[2] = i;
                        }
                    }
                }
            }
        }
    }
}
