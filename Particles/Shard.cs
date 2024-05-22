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
    public class Shard : Particle
    {
        Color startColor;
        int startFrame;
        Vector2 startScale;
        float deceleration;
        int fadeOutTime;
        int verticalFrameCount = 4;
        int frameWidth;
        int frameHeight;
        float gravity;

        public Shard(Vector2 Position, Vector2 Velocity, int TimeLeft, Color Color, Vector2 Scale, int StartFrame = 0, float Rotation = 0, float Deceleration = 0.96f, int fadeOutTimeLeftThreshold = 15, float Gravity = 0.1f, SpriteEffects SpriteEffects = SpriteEffects.None, bool Additive = true)
        {
            texture = TexDict["Shard"];
            frameWidth = texture.Width;
            frameHeight = texture.Height / verticalFrameCount;
            startFrame = StartFrame;
            FindFrame();
            additive = Additive;
            oldPosition = Position;
            position = Position;
            velocity = Velocity;
            color = startColor = Color;
            rotation = Rotation;
            scale = startScale = Scale;
            spriteEffects = SpriteEffects;
            timeLeft = TimeLeft;
            deceleration = Deceleration;
            fadeOutTime = fadeOutTimeLeftThreshold;
            gravity = Gravity;
        }
        public override void AI()
        {
            velocity *= deceleration;
            velocity.Y += gravity;
            rotation += velocity.X * 0.05f;
            if (timeLeft < fadeOutTime)
            {
                float interpolant = (timeLeft / (float)fadeOutTime);
                color = startColor * interpolant;
                scale = startScale + (startScale * (float)Math.Sqrt((1f - interpolant)) * 0.35f);
            }
            FindFrame();
        }
        public void FindFrame()
        {
            int vertiFrame = startFrame % verticalFrameCount;
            frame = new Rectangle(0, vertiFrame * frameHeight, frameWidth, frameHeight);
        }
    }
}
