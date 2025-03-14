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
    public class Wriggler : Particle
    {
        Color startColor;
        int startFrame;
        Vector2 startScale;
        int maxTimeLeft;
        float deceleration;
        int fadeOutTime;
        int verticalFrameCount = 4;
        int currentFrame;
        int frameWidth;
        int frameHeight;

        public Wriggler(Vector2 Position, Vector2 Velocity, int TimeLeft, Color Color, Vector2 Scale, int StartFrame = 0, float Rotation = 0, float Deceleration = 0.96f, int fadeOutTimeLeftThreshold = 15, SpriteEffects SpriteEffects = SpriteEffects.None)
        {
            if (Main.dedServ)
                return;

            texture = TexDict["Wriggler"];
            frameWidth = texture.Width;
            frameHeight = texture.Height / verticalFrameCount;
            currentFrame = startFrame = StartFrame;
            FindFrame();
            additive = true;
            oldPosition = Position;
            position = Position;
            velocity = Velocity;
            color = startColor = Color;
            rotation = Rotation;
            scale = startScale = Scale;
            spriteEffects = SpriteEffects;
            timeLeft = maxTimeLeft = TimeLeft;
            deceleration = Deceleration;
            fadeOutTime = fadeOutTimeLeftThreshold;
        }
        public override void AI()
        {
            velocity *= deceleration;
            if (timeLeft < fadeOutTime)
            {
                float interpolant = (timeLeft / (float)fadeOutTime);
                color = startColor * interpolant;
                scale = startScale + (startScale * (float)Math.Sqrt((1f - interpolant)) * 0.35f);
            }
            currentFrame = startFrame + (int)((maxTimeLeft - timeLeft) * 0.1f);
            FindFrame();
        }
        public void FindFrame()
        {
            int vertiFrame = currentFrame % verticalFrameCount;
            frame = new Rectangle(0, vertiFrame * frameHeight, frameWidth, frameHeight - 2);
        }
    }
}
