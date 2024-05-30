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
    public class FadingThinSpark : Particle
    {
        public Color startColor;
        public Vector2 startScale;
        public int maxTimeLeft;
        public FadingThinSpark(Vector2 Position, Vector2 Velocity, int TimeLeft, Color Color, Vector2 Scale, float Rotation = 0)
        {
            texture = TexDict["ThinSpark"];
            frame = new Rectangle(0, 0, texture.Width, texture.Height);
            additive = true;
            oldPosition = Position;
            position = Position;
            velocity = Velocity;
            color = Color;
            startColor = Color;
            rotation = Rotation;
            scale = Scale;
            startScale = Scale;
            spriteEffects = SpriteEffects.None;
            timeLeft = maxTimeLeft = TimeLeft;
        }
        public override void AI()
        {
            velocity *= 0.96f;
            float halfTime = maxTimeLeft * 0.5f;

            float multiplier;
            if (timeLeft > halfTime)
                multiplier = 1 - ((timeLeft - halfTime) / halfTime);
            else
                multiplier = timeLeft / halfTime;
            multiplier = -(float)Math.Pow((multiplier - 0.5f * 2), 2) + 1;

            color = startColor * multiplier;
            scale = startScale * multiplier;
        }
    }
}
