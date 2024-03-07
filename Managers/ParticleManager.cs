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

namespace TerRoguelike.Managers
{
    public class ParticleManager
    {
        public static List<Particle> ActiveParticles = new List<Particle>();
        public static void AddParticle(Particle particle)
        {
            ActiveParticles.Add(particle);
        }
        public static void UpdateParticles()
        {
            if (ActiveParticles == null)
                return;
            if (!ActiveParticles.Any())
                return;

            for (int i = 0; i < ActiveParticles.Count; i++)
            {
                Particle particle = ActiveParticles[i];
                particle.Update();
            }
            ActiveParticles.RemoveAll(x => x.timeLeft <= 0);
        }
        public static void DrawParticles()
        {
            if (ActiveParticles == null)
                return;
            if (!ActiveParticles.Any())
                return;

            StartAlphaBlendSpritebatch(false);
            for (int i = 0; i < ActiveParticles.Count; i++)
            {
                Particle particle = ActiveParticles[i];
                if (!particle.additive)
                    particle.Draw();
            }
            StartAdditiveSpritebatch();
            for (int i = 0; i < ActiveParticles.Count; i++)
            {
                Particle particle = ActiveParticles[i];
                if (particle.additive)
                    particle.Draw();
            }
            Main.spriteBatch.End();
        }
    }
}
