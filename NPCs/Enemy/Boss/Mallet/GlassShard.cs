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
using TerRoguelike.NPCs.Enemy.Boss.Mallet;
using ReLogic.Content;

namespace TerRoguelike.Particles
{
    public class GlassShard : Particle
    {
        Vector2 startScale;
        float acceleration;
        int verticalFrameCount = 3;
        int currentFrame;
        int frameWidth;
        int frameHeight;
        int fadeOutTime;
        float yVelCap;
        Color startColor;
        bool useLighting;
        int maxTimeLeft;
        float rotationIncrement;

        public GlassShard(Vector2 Position, Vector2 Velocity, int TimeLeft, Color Color, Vector2 Scale, int StartFrame, float Rotation, float RotationIncrement, SpriteEffects SpriteEffects, float Acceleration, float yCap, int fadeOutTimeLeftThreshold = 60)
        {
            if (Main.dedServ)
                return;

            texture = TexDict["GlassShard"];
            frameWidth = texture.Width;
            frameHeight = texture.Height / verticalFrameCount;
            currentFrame = StartFrame;
            FindFrame();
            additive = false;
            oldPosition = Position;
            position = Position;
            velocity = Velocity;
            color = startColor = Color;
            rotation = Rotation;
            scale = startScale = Scale;
            spriteEffects = SpriteEffects;
            timeLeft = maxTimeLeft = TimeLeft;
            acceleration = Acceleration;
            fadeOutTime = fadeOutTimeLeftThreshold;
            yVelCap = yCap;
            rotationIncrement = RotationIncrement;
        }
        public override void AI()
        {
            velocity.Y += acceleration;
            if (Math.Abs(velocity.Y) > Math.Abs(yVelCap))
                velocity.Y = Math.Sign(velocity.Y) * yVelCap;

            int time = maxTimeLeft - timeLeft;
            if (timeLeft < fadeOutTime)
            {
                float interpolant = (timeLeft / (float)fadeOutTime);
                color = startColor * interpolant;
            }
            rotation += rotationIncrement;
        }
        public void FindFrame()
        {
            int vertiFrame = currentFrame % verticalFrameCount;
            frame = new Rectangle(0, vertiFrame * frameHeight, frameWidth, frameHeight);
        }
    }
}
