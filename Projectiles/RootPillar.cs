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
using Terraria.DataStructures;
using Terraria.GameContent;
using System.Linq;
using ReLogic.Utilities;
using Microsoft.Xna.Framework.Audio;
using static TerRoguelike.Managers.TextureManager;
using System.Diagnostics;
using System.IO;

namespace TerRoguelike.Projectiles
{
    public class RootPillar : ModProjectile, ILocalizedModType
    {
        Vector2 spawnPos;
        Vector2 startVel;
        int maxTimeLeft;
        SlotId RootSound;
        SoundStyle RootLoop = new SoundStyle("TerRoguelike/Sounds/RootLoop");
        SoundStyle RootImpact = new SoundStyle("TerRoguelike/Sounds/RootImpact", 12);
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 1600;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 660;
            Projectile.penetrate = -1;
            Projectile.hide = true;
            maxTimeLeft = Projectile.timeLeft;
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }
        public override void OnSpawn(IEntitySource source)
        {
            spawnPos = Projectile.Center;
            startVel = Projectile.velocity;
            RootSound = SoundEngine.PlaySound(RootLoop with { Volume = 0.3f, MaxInstances = 3, IsLooped = true }, Projectile.Center);
        }
        public override void AI()
        {
            if (SoundEngine.TryGetActiveSound(RootSound, out var rootSound))
            {
                rootSound.Position = Projectile.Center;
                if (Projectile.timeLeft <= maxTimeLeft - 30 && Projectile.ai[1] == 0)
                {
                    rootSound.Sound.Volume = 0.7f;
                }
                if (Projectile.ai[1] > 0 && Projectile.localAI[2] == 0)
                {
                    Projectile.localAI[2] = 1;
                    rootSound.Stop();
                }
                if (Projectile.localAI[2] == 2)
                {
                    rootSound.Volume = MathHelper.Clamp((Projectile.Center - spawnPos).Length() / 320f, 0, 1f);
                }
                rootSound.Update();
            }
            if (Projectile.localAI[2] == 1)
            {
                Projectile.localAI[2] = 2;
                RootSound = SoundEngine.PlaySound(RootLoop with { Volume = 0.3f, Pitch = -1f, MaxInstances = 3, IsLooped = true }, Projectile.Center);
            }
            
            float rot = Projectile.ai[0] * MathHelper.PiOver2;
            if (Projectile.ai[1] <= 0)
            {
                int shootTimeStamp = 30;
                int shootTime = maxTimeLeft - shootTimeStamp;
                if (Projectile.timeLeft > shootTime)
                {
                    float speed = MathHelper.SmoothStep(0.3f, 5f, (Projectile.timeLeft - shootTime) / (float)shootTimeStamp);
                    Projectile.velocity = new Vector2(0, speed).RotatedBy(rot);
                }
                else
                {
                    Projectile.velocity = startVel;
                }
            }
            else
            {
                int time = (int)Projectile.ai[2] - Projectile.timeLeft;
                int windbackWindow = 60;
                if (time < windbackWindow)
                {
                    float speed = MathHelper.SmoothStep(0.8f, -0.2f, time / (float)windbackWindow);
                    Projectile.ai[1] += (speed / 0.5f);
                    Projectile.velocity = new Vector2(0, speed).RotatedBy(rot);
                }
                else
                {
                    if (Projectile.ai[1] > 10)
                    {
                        Projectile.ai[1] -= 0.15f;
                    }
                    float speed = MathHelper.SmoothStep(0f, -8f, time * 2 / (Projectile.ai[2]));
                    Projectile.velocity = new Vector2(0, speed).RotatedBy(rot);
                }
            }
            if ((int)Projectile.ai[0] == 0)
            {
                if (Projectile.Center.Y < spawnPos.Y)
                    Projectile.Kill();
            }
            else if ((int)Projectile.ai[0] == 2)
            {
                if (Projectile.Center.Y > spawnPos.Y)
                    Projectile.Kill();
            }
            else if ((int)Projectile.ai[0] == 1)
            {
                if (Projectile.Center.X > spawnPos.X)
                    Projectile.Kill();
            }
            else if ((int)Projectile.ai[0] == -1)
            {
                if (Projectile.Center.X < spawnPos.X)
                    Projectile.Kill();
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.timeLeft < maxTimeLeft - 35)
            {
                Projectile.localAI[0] = Projectile.Center.X;
                Projectile.localAI[1] = Projectile.Center.Y;
                Projectile.ai[2] = Projectile.timeLeft;

                Projectile.ai[1] += 10;

                SoundEngine.PlaySound(RootImpact with { Volume = 0.5f, MaxInstances = 2, Pitch = -0.5f }, Projectile.Center);
                for (int i = 0; i < 15; i++)
                {
                    Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.WoodFurniture, 0, 0, 0, default, 1.6f);
                    d.velocity *= 1.5f;
                }
                if (Projectile.ai[1] > 0)
                {
                    Projectile.velocity = Vector2.Zero;
                    Projectile.tileCollide = false;
                    Projectile.netUpdate = true;
                    return false;
                }
            }
            Projectile.velocity = oldVelocity;
            return false;
        }
        public override bool? CanDamage() => Projectile.timeLeft < maxTimeLeft - 30;
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            int height = Main.dedServ ? 40 : TextureAssets.Projectile[Type].Height();

            int length = (int)Projectile.ai[0] % 2 == 0 ? (int)Math.Abs(Projectile.Center.Y - spawnPos.Y) : (int)Math.Abs(Projectile.Center.X - spawnPos.X);
            Vector2 pos = spawnPos;
            float rot = Projectile.ai[0] * MathHelper.PiOver2;
            int endEase = length - 40;

            for (int i = 0; i < length; i += 15)
            {
                Vector2 posOffset = (Vector2.UnitY * (i + (height * 0.5f))).RotatedBy(rot);
                Vector2 scale = new Vector2(1f);

                if (i > endEase)
                {
                    float endEaseInterpolant = ((i - endEase) / (length - (float)endEase));
                    scale.X = MathHelper.SmoothStep(scale.X, 0.1f, endEaseInterpolant);
                }

                Vector2 realPos = pos + posOffset;
                if ((realPos - targetHitbox.ClosestPointInRect(pos)).Length() < 13f * scale.X)
                {
                    return true;
                }
            }

            return false;
        }
        public override void OnKill(int timeLeft)
        {
            if (SoundEngine.TryGetActiveSound(RootSound, out var rootSound))
            {
                rootSound.Stop();
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            int length = (int)Projectile.ai[0] % 2 == 0 ? (int)Math.Abs(Projectile.Center.Y - spawnPos.Y) : (int)Math.Abs(Projectile.Center.X - spawnPos.X);
            Vector2 pos = spawnPos;
            float rot = Projectile.ai[0] * MathHelper.PiOver2;
            int endEase = length - 40;

            List<StoredRootDraw> drawList = new List<StoredRootDraw>();

            bool changeup = Projectile.ai[1] > 0;
            int quality = TerRoguelike.lowDetail ? 2 : 1;
            for (int i = 0; i < length; i += quality)
            {
                int rectY = ((length - quality - i) % tex.Height);
                Rectangle rect = new Rectangle(0, rectY, tex.Width, quality);
                List<StoredRootDraw> miniDrawList = new List<StoredRootDraw>();
                for (int r = 0; r < 4; r++)
                {
                    float periodOffset = (r * MathHelper.PiOver2) + (length * 0.006f);
                    Vector2 posOffset = (Vector2.UnitY * (i + 16 + (tex.Height * 0.5f))).RotatedBy(rot);
                    
                    Vector2 scale = new Vector2(0.8f, 1f);
                    float interpolant = (float)Math.Sin(((float)i / tex.Height) + periodOffset);
                    float depthInterpolant = 1f - Math.Abs((float)Math.Sin(((float)i * 0.5f / tex.Height) + (periodOffset * 0.5f)));

                    float depth = MathHelper.Lerp(0.6f, 1f, depthInterpolant);
                    float colorDepth = MathHelper.Lerp(0.3f, 1f, depthInterpolant);
                    scale.X *= depth;
                    Color color = Color.Lerp(Color.DarkSlateGray, Color.White, colorDepth);
                    Vector2 potentialOffset = (Vector2.UnitX * MathHelper.Lerp(0f, 10f, interpolant));

                    if (i < 16)
                    {
                        float startEaseInterpolant = i / 16f;
                        color *= MathHelper.Lerp(0f, 1f, startEaseInterpolant);
                    }
                    if (i > endEase)
                    {
                        float endEaseInterpolant = ((i - endEase) / (length - (float)endEase));
                        depth = MathHelper.SmoothStep(depth, 1f, endEaseInterpolant);
                        colorDepth = MathHelper.SmoothStep(colorDepth, 1f, endEaseInterpolant);
                        scale.X = MathHelper.SmoothStep(scale.X, 0.1f, endEaseInterpolant);
                        if (!changeup)
                        {
                            potentialOffset.X *= MathHelper.SmoothStep(1f, 0f, endEaseInterpolant);
                        }
                        else
                        {
                            float over = MathHelper.SmoothStep(0, 1f, Projectile.ai[1] / 55);
                            if (over > 1f)
                                over = 1f;
                            potentialOffset.X *= MathHelper.SmoothStep(1f, 3f * over, endEaseInterpolant);
                        }

                    }
                    posOffset += potentialOffset.RotatedBy(rot);

                    miniDrawList.Add(new StoredRootDraw(pos + posOffset - Main.screenPosition, rect, color, rot, scale, depthInterpolant));
                }
                miniDrawList.Sort((x, y) => x.depth.CompareTo(y.depth));
                drawList.AddRange(miniDrawList);
            }
            for (int i = 0; i < drawList.Count; i++)
            {
                StoredRootDraw d = drawList[i];
                Main.EntitySpriteDraw(tex, d.pos, d.rect, d.color, rot, tex.Size() * 0.5f, d.scale, SpriteEffects.None);
            }
            // draws where collision is happening for debugging
            /*
            Texture2D tempTex = TexDict["CircularGlow"];
            for (int i = 0; i < length; i += 15)
            {
                Vector2 posOffset = (Vector2.UnitY * (i + (tex.Height * 0.5f))).RotatedBy(rot);
                Vector2 scale = new Vector2(1f);

                if (i > endEase)
                {
                    float endEaseInterpolant = ((i - endEase) / (length - (float)endEase));
                    scale *= MathHelper.SmoothStep(1f, 0.1f, endEaseInterpolant);
                }

                Vector2 realPos = pos + posOffset;
                Main.EntitySpriteDraw(tempTex, realPos - Main.screenPosition, null, Color.White, 0f, tempTex.Size() * 0.5f, scale * 0.03f, SpriteEffects.None);
            }
            */
            return false;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.WriteVector2(spawnPos);
            writer.WriteVector2(startVel);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            spawnPos = reader.ReadVector2();
            startVel = reader.ReadVector2();
        }
    }
    public class StoredRootDraw
    {
        public Vector2 pos;
        public Rectangle rect;
        public Color color;
        public float rot;
        public Vector2 scale;
        public float depth;
        public StoredRootDraw(Vector2 position, Rectangle frame, Color Color, float rotation, Vector2 Scale, float Depth)
        {
            pos = position;
            rect = frame;
            color = Color;
            rot = rotation;
            scale = Scale;
            depth = Depth;
        }
    }
}
