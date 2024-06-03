using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using TerRoguelike.Managers;
using TerRoguelike.TerPlayer;
using TerRoguelike.World;
using Terraria.ID;
using TerRoguelike.Systems;
using TerRoguelike.Projectiles;
using Terraria.ModLoader.IO;
using Microsoft.Xna.Framework;
using TerRoguelike.NPCs;
using static TerRoguelike.Schematics.SchematicManager;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.Audio;
using System.IO;
using Terraria.UI;
using TerRoguelike.MainMenu;
using TerRoguelike.NPCs.Enemy.Pillar;
using static TerRoguelike.World.TerRoguelikeWorld;
using static TerRoguelike.Systems.MusicSystem;
using Terraria.DataStructures;
using static TerRoguelike.Managers.TextureManager;
using static TerRoguelike.Utilities.TerRoguelikeUtils;

namespace TerRoguelike.Particles
{
    public class OrbitingGore : Particle
    {
        Vector2 startScale;
        int fadeOutTime;
        int maxTimeLeft;
        int direction;
        float rotAmount;
        float deceleration;
        float orbitRate;
        Vector2 orbitAnchor;
        float outerLength;

        public OrbitingGore(Texture2D tex, Vector2 Position, Vector2 Velocity, float Decelertation, float OrbitRate, Vector2 OrbitAnchor, Rectangle Frame, int TimeLeft, Color Color, Vector2 Scale, float Rotation, int RotDirection, int FadeOutTime = 180, float RotAmount = 0.075f)
        {
            texture = tex;
            frame = Frame;
            additive = false;
            position = oldPosition = Position;
            velocity = Velocity;
            color = Color;
            rotation = Rotation;
            scale = startScale = Scale;
            spriteEffects = SpriteEffects.None;
            timeLeft = TimeLeft;
            direction = RotDirection;
            rotAmount = RotAmount;
            fadeOutTime = FadeOutTime;
            deceleration = Decelertation;
            orbitRate = OrbitRate;
            orbitAnchor = OrbitAnchor;
            outerLength = 1;
        }
        public override void AI()
        {
            velocity *= deceleration;
            int time = maxTimeLeft - timeLeft;
            float fadeOutInterpolant = 1f;
            float effectiveOrbitRate = orbitRate;
            effectiveOrbitRate *= MathHelper.Clamp(MathHelper.Lerp(1f, 0.5f, time / (float)fadeOutTime), 0.5f, 1f);
            if (timeLeft < fadeOutTime)
            {
                fadeOutInterpolant = (timeLeft / (float)fadeOutTime);
                fadeOutInterpolant = (float)Math.Pow(fadeOutInterpolant, 2f);
                scale = startScale * fadeOutInterpolant;
            }
            rotation += rotAmount * direction;

            if (fadeOutInterpolant == 1)
                outerLength = position.Distance(orbitAnchor);

            position = ((position - orbitAnchor).SafeNormalize(Vector2.UnitY) * outerLength).RotatedBy(effectiveOrbitRate) * (float)Math.Pow(fadeOutInterpolant, 1.25f) + orbitAnchor;
        }
    }
}
