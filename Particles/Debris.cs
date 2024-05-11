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
    public class Debris : Particle
    {
        Vector2 startScale;
        float acceleration;
        int verticalFrameCount = 3;
        int currentFrame;
        int frameWidth;
        int frameHeight;
        int fadeOutTime;
        float yVelCap;

        public Debris(Vector2 Position, Vector2 Velocity, int TimeLeft, Color Color, Vector2 Scale, int StartFrame, float Rotation, SpriteEffects SpriteEffects, float Acceleration, float yCap, int fadeOutTimeLeftThreshold = 60)
        {
            texture = TexDict["RockDebris"].Value;
            frameWidth = texture.Width;
            frameHeight = texture.Height / verticalFrameCount;
            currentFrame = StartFrame;
            FindFrame();
            additive = true;
            oldPosition = Position;
            position = Position;
            velocity = Velocity;
            color = startColor = Color;
            rotation = Rotation;
            scale = startScale = Scale;
            spriteEffects = SpriteEffects;
            timeLeft = TimeLeft;
            acceleration = Acceleration;
            fadeOutTime = fadeOutTimeLeftThreshold;
            yVelCap = yCap;
        }
        public override void AI()
        {
            velocity.Y += acceleration;
            if (Math.Abs(velocity.Y) > Math.Abs(yVelCap))
                velocity.Y = Math.Sign(velocity.Y) * yVelCap;
            if (timeLeft < fadeOutTime)
            {
                float interpolant = (timeLeft / (float)fadeOutTime);
                scale = startScale * interpolant;
            }
            rotation += 0.075f * (Math.Abs(velocity.Y) / Math.Abs(yVelCap)) * (spriteEffects == SpriteEffects.None ? 1 : -1);
        }
        public void FindFrame()
        {
            int vertiFrame = currentFrame % verticalFrameCount;
            frame = new Rectangle(0, vertiFrame * frameHeight, frameWidth, frameHeight - 2);
        }
    }
}
