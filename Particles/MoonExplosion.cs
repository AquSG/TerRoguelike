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
    public class MoonExplosion : Particle
    {
        int maxTimeLeft;
        int verticalFrameCount = 7;
        int currentFrame;
        int frameWidth;
        int frameHeight;

        public MoonExplosion(Vector2 Position, int TimeLeft, Color Color, Vector2 Scale, float Rotation = 0)
        {
            if (Main.dedServ)
                return;

            texture = TexDict["MoonDeadExplosion"];
            frameWidth = texture.Width;
            frameHeight = texture.Height / verticalFrameCount;
            currentFrame = 0;
            FindFrame();
            additive = false;
            oldPosition = Position;
            position = Position;
            velocity = Vector2.Zero;
            color = Color;
            rotation = Rotation;
            scale = Scale;
            spriteEffects = SpriteEffects.None;
            timeLeft = maxTimeLeft = TimeLeft;
        }
        public override void AI()
        {
            currentFrame = (int)((maxTimeLeft - timeLeft) / (float)maxTimeLeft * verticalFrameCount);
            FindFrame();
        }
        public void FindFrame()
        {
            int vertiFrame = currentFrame % verticalFrameCount;
            frame = new Rectangle(0, vertiFrame * frameHeight, frameWidth, frameHeight - 2);
        }
    }
}
