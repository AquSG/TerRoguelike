using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerRoguelike.Rooms;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using TerRoguelike.Schematics;
using TerRoguelike.Managers;
using TerRoguelike.Systems;
using TerRoguelike.Items;
using TerRoguelike.Items.Common;
using TerRoguelike.Items.Uncommon;
using TerRoguelike.Items.Rare;
using Terraria.ModLoader.Core;
using TerRoguelike.NPCs.Enemy;
using TerRoguelike.NPCs.Enemy.Pillar;
using TerRoguelike.NPCs;
using static TerRoguelike.Schematics.SchematicManager;
using Microsoft.Xna.Framework.Graphics;
using TerRoguelike.Particles;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using ReLogic.Threading;
using System.Diagnostics;
using rail;

namespace TerRoguelike.Managers
{
    public class ParticleManager
    {
        public static List<Particle> ActiveParticles = new List<Particle>();
        public static List<Particle> ActiveParticlesBehindTiles = new List<Particle>();
        public static List<Particle> ActiveParticlesAfterProjectiles = new List<Particle>();
        public enum ParticleLayer
        {
            Default = 0,
            BehindTiles = 1,
            AfterProjectiles = 2
        }
        public static void AddParticle(Particle particle, ParticleLayer layer = ParticleLayer.Default)
        {
            if (layer == ParticleLayer.Default)
                ActiveParticles.Add(particle);
            else if (layer == ParticleLayer.BehindTiles)
                ActiveParticlesBehindTiles.Add(particle);
            else if (layer == ParticleLayer.AfterProjectiles)
                ActiveParticlesAfterProjectiles.Add(particle);
        }
        public static void UpdateParticles()
        {
            UpdateParticles_Default();
            UpdateParticles_BehindTiles();
            UpdateParticles_AfterProjectiles();
        }
        public static void UpdateParticles_Default()
        {
            if (ActiveParticles == null)
                return;
            if (ActiveParticles.Count == 0)
                return;

            FastParallel.For(0, ActiveParticles.Count, delegate (int start, int end, object context)
            {
                for (int i = start; i < end; i++)
                {
                    Particle particle = ActiveParticles[i];
                    particle.Update();
                }
            });
            ActiveParticles.RemoveAll(x => x.timeLeft <= 0);
        }
        public static void UpdateParticles_BehindTiles()
        {
            if (ActiveParticlesBehindTiles == null)
                return;
            if (ActiveParticlesBehindTiles.Count == 0)
                return;

            FastParallel.For(0, ActiveParticlesBehindTiles.Count, delegate (int start, int end, object context)
            {
                for (int i = start; i < end; i++)
                {
                    Particle particle = ActiveParticlesBehindTiles[i];
                    particle.Update();
                }
            });
            ActiveParticlesBehindTiles.RemoveAll(x => x.timeLeft <= 0);
        }
        public static void UpdateParticles_AfterProjectiles()
        {
            if (ActiveParticlesAfterProjectiles == null)
                return;
            if (ActiveParticlesAfterProjectiles.Count == 0)
                return;

            FastParallel.For(0, ActiveParticlesAfterProjectiles.Count, delegate (int start, int end, object context)
            {
                for (int i = start; i < end; i++)
                {
                    Particle particle = ActiveParticlesAfterProjectiles[i];
                    particle.Update();
                }
            });
            ActiveParticlesAfterProjectiles.RemoveAll(x => x.timeLeft <= 0);
        }
        public static void DrawParticles_Default()
        {
            if (ActiveParticles == null)
                return;
            if (ActiveParticles.Count == 0)
                return;
            Point ScreenPos = Main.Camera.ScaledPosition.ToPoint();
            Point ScreenDimensions = (new Vector2(Main.screenWidth, Main.screenHeight) / ZoomSystem.ScaleVector * 1.1f).ToPoint();
            Rectangle ScreenRect = new Rectangle(ScreenPos.X, ScreenPos.Y, ScreenDimensions.X, ScreenDimensions.Y);
            StartAlphaBlendSpritebatch(false);
            for (int i = 0; i < ActiveParticles.Count; i++)
            {
                Particle particle = ActiveParticles[i];
                if (particle.additive)
                    continue;
                if (!DrawScreenCheckWithFluff(particle.position, (int)((particle.frame.Width > particle.frame.Height ? particle.frame.Width : particle.frame.Height) * (particle.scale.X > particle.scale.Y ? particle.scale.X : particle.scale.Y)), ScreenRect))
                    continue;

                particle.Draw();
            }
            StartAdditiveSpritebatch();
            for (int i = 0; i < ActiveParticles.Count; i++)
            {
                Particle particle = ActiveParticles[i];
                if (!particle.additive)
                    continue;
                if (!DrawScreenCheckWithFluff(particle.position, (int)((particle.frame.Width > particle.frame.Height ? particle.frame.Width : particle.frame.Height) * (particle.scale.X > particle.scale.Y ? particle.scale.X : particle.scale.Y)), ScreenRect))
                    continue;

                particle.Draw();
            }
            Main.spriteBatch.End();
        }
        public static void DrawParticles_BehindTiles()
        {
            if (ActiveParticlesBehindTiles == null)
                return;
            if (ActiveParticlesBehindTiles.Count == 0)
                return;
            Point ScreenPos = Main.Camera.ScaledPosition.ToPoint();
            Point ScreenDimensions = (new Vector2(Main.screenWidth, Main.screenHeight) / ZoomSystem.ScaleVector * 1.1f).ToPoint();
            Rectangle ScreenRect = new Rectangle(ScreenPos.X, ScreenPos.Y, ScreenDimensions.X, ScreenDimensions.Y);
            StartAlphaBlendSpritebatch();
            for (int i = 0; i < ActiveParticlesBehindTiles.Count; i++)
            {
                Particle particle = ActiveParticlesBehindTiles[i];
                if (particle.additive)
                    continue;
                if (!DrawScreenCheckWithFluff(particle.position, (int)((particle.frame.Width > particle.frame.Height ? particle.frame.Width : particle.frame.Height) * (particle.scale.X > particle.scale.Y ? particle.scale.X : particle.scale.Y)), ScreenRect))
                    continue;

                particle.Draw();
            }
            StartAdditiveSpritebatch();
            for (int i = 0; i < ActiveParticlesBehindTiles.Count; i++)
            {
                Particle particle = ActiveParticlesBehindTiles[i];
                if (!particle.additive)
                    continue;
                if (!DrawScreenCheckWithFluff(particle.position, (int)((particle.frame.Width > particle.frame.Height ? particle.frame.Width : particle.frame.Height) * (particle.scale.X > particle.scale.Y ? particle.scale.X : particle.scale.Y)), ScreenRect))
                    continue;

                particle.Draw();
            }
            StartVanillaSpritebatch();
        }
        public static void DrawParticles_AfterProjectiles()
        {
            if (ActiveParticlesAfterProjectiles == null)
                return;
            if (ActiveParticlesAfterProjectiles.Count == 0)
                return;
            Point ScreenPos = Main.Camera.ScaledPosition.ToPoint();
            Point ScreenDimensions = (new Vector2(Main.screenWidth, Main.screenHeight) / ZoomSystem.ScaleVector * 1.1f).ToPoint();
            Rectangle ScreenRect = new Rectangle(ScreenPos.X, ScreenPos.Y, ScreenDimensions.X, ScreenDimensions.Y);
            StartAlphaBlendSpritebatch(false);
            for (int i = 0; i < ActiveParticlesAfterProjectiles.Count; i++)
            {
                Particle particle = ActiveParticlesAfterProjectiles[i];
                if (particle.additive)
                    continue;
                if (!DrawScreenCheckWithFluff(particle.position, (int)((particle.frame.Width > particle.frame.Height ? particle.frame.Width : particle.frame.Height) * (particle.scale.X > particle.scale.Y ? particle.scale.X : particle.scale.Y)), ScreenRect))
                    continue;

                particle.Draw();
            }
            StartAdditiveSpritebatch();
            for (int i = 0; i < ActiveParticlesAfterProjectiles.Count; i++)
            {
                Particle particle = ActiveParticlesAfterProjectiles[i];
                if (!particle.additive)
                    continue;
                if (!DrawScreenCheckWithFluff(particle.position, (int)((particle.frame.Width > particle.frame.Height ? particle.frame.Width : particle.frame.Height) * (particle.scale.X > particle.scale.Y ? particle.scale.X : particle.scale.Y)), ScreenRect))
                    continue;

                particle.Draw();
            }
            Main.spriteBatch.End();
        }
        public static bool DrawScreenCheckWithFluff(Vector2 position, int fluff = 100, Rectangle? screenRect = null)
        {
            Rectangle checkRect = new Rectangle((int)position.X, (int)position.Y, 1, 1);
            checkRect.Inflate(fluff, fluff);
            Rectangle ScreenRect;
            if (screenRect == null)
            {
                Point ScreenPos = (Main.Camera.ScaledPosition - Main.screenPosition).ToPoint();
                Point ScreenDimensions = (new Vector2(Main.screenWidth, Main.screenHeight) / ZoomSystem.ScaleVector * 1.1f).ToPoint();
                ScreenRect = new Rectangle(ScreenPos.X, ScreenPos.Y, ScreenDimensions.X, ScreenDimensions.Y);
            }
            else
            {
                ScreenRect = (Rectangle)screenRect;
            }
            return ScreenRect.Intersects(checkRect);
        }
    }
}
