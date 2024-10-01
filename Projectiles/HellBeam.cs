using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using static TerRoguelike.Managers.TextureManager;
using TerRoguelike.Managers;
using TerRoguelike.Particles;
using ReLogic.Utilities;
using Terraria.Audio;
using Terraria.ID;
using TerRoguelike.Systems;
using TerRoguelike.NPCs.Enemy.Boss;
using Terraria.GameContent;
using Humanizer;
using Terraria.Graphics.Shaders;
using Terraria.Graphics.Effects;
using System.Diagnostics;
using System.IO.Pipes;
using TerRoguelike.MainMenu;


namespace TerRoguelike.Projectiles
{
    public class HellBeam : ModProjectile, ILocalizedModType
    {
        public int maxTimeLeft;
        public List<Vector2> specialOldPos = [];
        public List<Vector2> specialOldVel = [];
        public List<bool> specialOldDead = [];
        public Texture2D waveTex;
        public Texture2D squareTex;
        public List<StoredDraw> draws = [];
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.timeLeft = maxTimeLeft = 1500;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.friendly = true;
            Projectile.hostile = false;
            waveTex = TexDict["HellBeamWave"];
            squareTex = TexDict["Square"];
            Projectile.hide = true;
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCs.Add(index);
        }
        public override void AI()
        {
            bool allow = true;
            var modProj = Projectile.ModProj();
            if (modProj.npcOwner >= 0)
            {
                NPC npc = Main.npc[modProj.npcOwner];
                var modNPC = npc.ModNPC();
                if (modNPC.isRoomNPC)
                {
                    if (RoomSystem.RoomList[modNPC.sourceRoomListID].bossDead)
                        allow = false;
                }
            }

            int maxSpecialPos = 240;
            int time = maxTimeLeft - Projectile.timeLeft;
            if (specialOldPos.Count < maxSpecialPos && allow)
            {
                Color outlineColor = Color.Lerp(Color.LightPink, Color.OrangeRed, 0.13f);
                Color fillColor = Color.Lerp(outlineColor, Color.DarkRed, 0.2f);
                for (int j = -6; j <= 6; j++)
                {
                    if (!Main.rand.NextBool(12) || j == 0)
                        continue;
                    Vector2 particleSpawnPos = Projectile.Center + Vector2.UnitX.RotatedBy(Math.Sign(j) * -0.3f + Projectile.rotation) * 0.4f + Projectile.rotation.ToRotationVector2() * 4;
                    Vector2 particleVel = (Projectile.rotation.ToRotationVector2()).RotatedBy(Math.Sign(j) * 0.5f + j * 0.12f + Main.rand.NextFloat(-0.02f, 0.02f)) * Main.rand.NextFloat(0.5f, 1f) * 4;
                    ParticleManager.AddParticle(new BallOutlined(
                        particleSpawnPos, particleVel,
                        60, outlineColor, fillColor, new Vector2(Main.rand.NextFloat(0.14f, 0.28f)), 4, 0, 0.97f, 50));
                }

                specialOldPos.Add(Projectile.Center);
                specialOldVel.Add(Projectile.rotation.ToRotationVector2() * 12f);
                specialOldDead.Add(false);
            }
            int deadCount = 0;
            int last = specialOldPos.Count - 1;
            bool ruin = TerRoguelikeMenu.RuinedMoonActive;
            for (int i = 0; i <= last; i++)
            {
                if (specialOldDead[i])
                {
                    deadCount++;
                }

                Vector2 pos = specialOldPos[i];
                Vector2 predictedPos = pos + specialOldVel[i];
                specialOldPos[i] = predictedPos;
                if (!ruin && !specialOldDead[i] && !CanHitInLine(pos, predictedPos))
                {
                    specialOldDead[i] = true;
                    Color outlineColor = Color.Lerp(Color.Salmon, Color.OrangeRed, 0.13f);
                    Color fillColor = Color.Lerp(outlineColor, Color.LightPink, 0.2f);
                    for (int j = -6; j <= 6; j++)
                    {
                        if (!Main.rand.NextBool(7) || j == 0)
                            continue;
                        Vector2 particleSpawnPos = predictedPos + specialOldVel[i].RotatedBy(Math.Sign(j) * -0.3f) * 0.4f;
                        Vector2 particleVel = -specialOldVel[i].SafeNormalize(Vector2.UnitY).RotatedBy(Math.Sign(j) * 0.5f + j * 0.12f + Main.rand.NextFloat(-0.02f, 0.02f)) * Main.rand.NextFloat(0.5f, 1f) * 4;
                        ParticleManager.AddParticle(new BallOutlined(
                            particleSpawnPos, particleVel,
                            60, outlineColor, fillColor, new Vector2(Main.rand.NextFloat(0.14f, 0.28f)), 4, 0, 0.97f, 50));
                    }
                }
                if (i % 2 == 0 && Projectile.timeLeft % 1 == 0 && !specialOldDead[i] && i > 0)
                {
                    Vector2 particlePos = specialOldPos[i];
                    float rot = (specialOldPos[i - 1] - specialOldPos[i]).ToRotation();
                    for (int j = -1; j <= 1; j += 2)
                    {
                        ParticleManager.AddParticle(new ThinSpark(
                            particlePos + (rot + MathHelper.PiOver2 * j).ToRotationVector2() * 24 - specialOldVel[i], specialOldVel[i],
                            10, Color.Red, new Vector2(0.1f, 0.4f) * 5, rot + (MathHelper.PiOver4 * 0.1f * j), true, false));
                    }

                }
            }
            if (!specialOldDead[last] && time < 300)
            {
                float completion = time < 60 ? MathHelper.Clamp(time / 4f, 0, 1) : (1f - MathHelper.Clamp((time - 240) / 60f, 0, 1));
                Point lPos = (Projectile.Center + Projectile.rotation.ToRotationVector2() * -20).ToTileCoordinates();
                Lighting.AddLight(lPos.X, lPos.Y, TorchID.Orange, completion);
            }

            if (deadCount >= specialOldPos.Count)
            {
                Projectile.Kill();
            }


            Texture2D tex = TextureAssets.Projectile[Type].Value;
            int frameWidth = tex.Width;
            float frameProgress = 0;
            if (specialOldPos.Count >= maxSpecialPos)
            {
                frameProgress = Projectile.Center.Distance(specialOldPos[specialOldPos.Count - 1]);
            }
            Rectangle frame;
            draws = [];

            for (int i = specialOldPos.Count - 1; i > 0; i--)
            {
                if (specialOldDead[i])
                    continue;

                Vector2 pos = specialOldPos[i];
                Vector2 frontPos = specialOldPos[i - 1];
                bool frontDead = i == 1 || specialOldDead[i - 1];
                bool backDead = i == specialOldPos.Count - 1 || specialOldDead[i + 1];
                float rot = (frontPos - pos).ToRotation();


                int giveUp = 0;
                while (pos != frontPos)
                {
                    Vector2 scale = new Vector2(1f);
                    float distance = pos.Distance(frontPos);
                    if (frontDead)
                    {
                        if (distance < 10)
                            scale.Y *= ((int)distance / 2 + 1) / 5f;
                    }
                    if (backDead)
                    {
                        scale.Y *= ((int)(12 - distance) / 2) / 5f;
                    }
                    int progress = 1;
                    if (!frontDead && !backDead) //This extends the frame drawn somewhat relative to distance to reduce the amount of draw calls
                    {
                        progress = (int)distance;
                        if (progress <= 1)
                            progress = 1;
                        else
                        {
                            int currentFramePos = (int)frameProgress % frameWidth;
                            if (currentFramePos + progress >= frameWidth)
                            {
                                progress = (frameWidth) - currentFramePos;
                                if (progress < 1)
                                    progress = 1;
                            }
                        }
                    }



                    frame = new Rectangle((int)frameProgress % frameWidth, 0, progress + 1, tex.Height);


                    draws.Add(new StoredDraw(tex, pos, frame, Color.White, rot, new Vector2(0, tex.Size().Y * 0.5f), scale, SpriteEffects.None));
                    frameProgress += progress;
                    Vector2 step = rot.ToRotationVector2() * progress;
                    if (distance <= 1f)
                    {
                        pos = frontPos;

                    }
                    else
                    {
                        pos += step;
                    }


                    giveUp++;
                    if (giveUp >= 400)
                    {
                        pos = frontPos;
                    }
                }

            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            int cap = draws.Count - 1;
            for (int i = 0; i <= cap; i += 1)
            {
                var draw = draws[i];
                if (draw.scale.Y < 1f || i >= cap - 4 || i < 4)
                    continue;
                Vector2 pos = draw.position;
                if (pos.Distance(draws[i + 1].position) > 24)
                    continue;
                if (draws[i + 4].scale.Y < 1f || draws[i - 4].scale.Y < 1f)
                    continue;
                int width = ((Rectangle)draw.frame).Width;
                i += 8 - Math.Clamp(width, 0, 8);
                if (targetHitbox.ClosestPointInRect(pos).Distance(pos) < 24)
                    return true;
            }
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.End();
            Effect maskEffect = Filters.Scene["TerRoguelike:MaskOverlay"].GetShader().Shader;
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, maskEffect, Main.GameViewMatrix.TransformationMatrix);
            Color tint = Color.Lerp(Color.LightSalmon, Color.White, 0.6f);
            maskEffect.Parameters["screenOffset"].SetValue(new Vector2(Main.GlobalTimeWrappedHourly * 10, (float)Math.Cos(Main.GlobalTimeWrappedHourly * 10) * 0.01f));
            maskEffect.Parameters["stretch"].SetValue(new Vector2(1, 1));
            maskEffect.Parameters["replacementTexture"].SetValue(waveTex);
            maskEffect.Parameters["tint"].SetValue(tint.ToVector4());
            for (int i = 0; i < draws.Count; i++)
            {
                var draw = draws[i];
                draw.Draw(-Main.screenPosition);
            }
            StartVanillaSpritebatch();
            if (false)
            {
                int cap = draws.Count - 1;
                for (int i = 0; i <= cap; i += 1)
                {
                    var draw = draws[i];
                    if (draw.scale.Y < 1f || i >= cap - 4 || i < 4)
                        continue;
                    Vector2 pos = draw.position;
                    if (pos.Distance(draws[i + 1].position) > 24)
                        continue;
                    if (draws[i + 4].scale.Y < 1f || draws[i - 4].scale.Y < 1f)
                        continue;
                    int width = ((Rectangle)draw.frame).Width;
                    i += 8 - Math.Clamp(width, 0, 8);
                    for (int j = 0; j < 60; j++)
                    {
                        Main.EntitySpriteDraw(squareTex, pos - Main.screenPosition + (j * MathHelper.TwoPi / 60f).ToRotationVector2() * 24, null, Color.Cyan, 0, squareTex.Size() * 0.5f, 1f, SpriteEffects.None);
                    }
                }
            }

            return false;
        }
    }
}
