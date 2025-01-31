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
using Microsoft.Build.Construction;

namespace TerRoguelike.Projectiles
{
    public class SeekingStarCell : ModProjectile, ILocalizedModType
    {
        public float turnMultiplier = 1f;
        public Texture2D lightTex;
        public int maxTimeLeft;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 2;
            Main.projFrames[Type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 480;
            Projectile.penetrate = 1;
            Projectile.ignoreWater = true;
            lightTex = TexDict["SeekingStarCellGlow"];
            maxTimeLeft = Projectile.timeLeft;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.ai[2] = -1;
            Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            Projectile.direction = Main.rand.NextBool() ? -1 : 1;
        }
        public override void AI()
        {
            if (Projectile.timeLeft == maxTimeLeft)
                GetTarget();

            Projectile.frameCounter++;
            Projectile.frame = ((int)Projectile.frameCounter / 10) % Main.projFrames[Type];
            if (!Main.rand.NextBool(4))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Clentaminator_Cyan, 0, 0, 0, default, 0.65f);
                d.noGravity = true;
                d.noLight = true;
                d.noLightEmittence = true;
            }

            Projectile.rotation += MathHelper.Clamp(Projectile.velocity.Length(), 0, 6) * 0.05f * Projectile.direction;
            if (Projectile.timeLeft > maxTimeLeft - 90)
            {
                Projectile.velocity *= 0.972f;
                return;
            }

            float newRot = Projectile.velocity.ToRotation();
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
                if (Projectile.timeLeft >= maxTimeLeft - 91)
                {
                    newRot = direction;
                }
                else if (Math.Abs(AngleSizeBetween(Projectile.velocity.ToRotation(), direction)) < MathHelper.PiOver2)
                {
                    turnMultiplier = Projectile.velocity.Length() / 2.2f;
                    newRot = Projectile.velocity.ToRotation().AngleTowards(direction, 0.006f * turnMultiplier);
                }
            }
            float speed = MathHelper.Clamp(MathHelper.Lerp(0f, 10f, (((maxTimeLeft - 90) - Projectile.timeLeft) * 2.7f) / (maxTimeLeft - 90)), 0f, 10f);
            Projectile.velocity = (Vector2.UnitX * speed).RotatedBy(newRot);
        }
        public override bool? CanDamage() => Projectile.timeLeft < maxTimeLeft - 90 ? null : false;
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 15; i++)
            {
                Vector2 offset = (Main.rand.Next(1, 4) * Vector2.UnitX.RotatedBy(Main.rand.NextFloat(MathHelper.TwoPi)));
                Dust d = Dust.NewDustPerfect(Projectile.Center + offset, DustID.Clentaminator_Cyan, offset.SafeNormalize(Vector2.UnitX) + (Projectile.oldVelocity * 0.5f), Projectile.alpha, default, 1f);
                d.noGravity = true;
                d.noLight = true;
                d.noLightEmittence = true;
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.timeLeft > maxTimeLeft - 90)
            {
                // If the projectile hits the left or right side of the tile, reverse the X velocity
                if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
                {
                    Projectile.velocity.X = -oldVelocity.X;
                }
                // If the projectile hits the top or bottom side of the tile, reverse the Y velocity
                if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
                {
                    Projectile.velocity.Y = -oldVelocity.Y;
                }

                return false;
            }
                
            SoundEngine.PlaySound(SoundID.Item88 with { Volume = 0.5f, Pitch = 0.125f, PitchVariance = 0.04f }, Projectile.Center);
            return true;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 offset = new Vector2(Projectile.width * 0.5f, Projectile.height * 0.5f);
            int frameHeight = lightTex.Height / Main.projFrames[Type];
            Rectangle frame = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);
            if (Projectile.ModProj().hostileTurnedAlly)
                StartVanillaSpritebatch();
            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Vector2 pos = Projectile.oldPos[i] + offset;
                float opacity = (1f - ((float)i / Projectile.oldPos.Length)) * 0.6f;
                Main.EntitySpriteDraw(lightTex, pos - Main.screenPosition, frame, Color.White * opacity, Projectile.oldRot[i], frame.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            }
            Projectile.ModProj().EliteSpritebatch();
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, frame, Projectile.GetAlpha(lightColor), Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            Main.EntitySpriteDraw(lightTex, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            return false;
        }

        public void GetTarget()
        {
            var modProj = Projectile.ModProj();
            if (modProj != null && modProj.npcOwner >= 0)
            {
                NPC owner = Main.npc[modProj.npcOwner];
                if (owner.active && (owner.friendly == Projectile.friendly))
                {
                    var modOwner = owner.ModNPC();
                    
                    if (modOwner != null)
                    {
                        if (modOwner.targetNPC >= 0 && Main.npc[modOwner.targetNPC].active)
                        {
                            Projectile.ai[1] = 1;
                            Projectile.ai[2] = modOwner.targetNPC;
                            return;
                        }
                        if (modOwner.targetPlayer >= 0 && Main.player[modOwner.targetPlayer].active && !Main.player[modOwner.targetPlayer].dead)
                        {
                            Projectile.ai[1] = 0;
                            Projectile.ai[2] = modOwner.targetPlayer;
                            return;
                        }
                    }
                }
            }

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
                        closestTarget = distance;
                        Projectile.ai[2] = i;
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
                         closestTarget = distance;
                         Projectile.ai[2] = i;
                    }
                }
            }
        }
    }
}
