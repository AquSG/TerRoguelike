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
    public class Ash : Particle
    {
        Vector2 startScale;
        float deceleration;
        int fadeOutTime;
        float Gravity;
        float YCap;
        bool tileCollide;
        int maxTimeleft;
        int floatTime;
        public Ash(Vector2 Position, Vector2 Velocity, int TimeLeft, Color Color, Vector2 Scale, float Rotation = 0, float Deceleration = 0.96f, float gravity = 0.04f, int fadeOutTimeLeftThreshold = 30, int airHangTime = 30, bool Additive = false)
        {
            texture = TexDict["Square"];
            frame = new Rectangle(0, 0, texture.Width, texture.Height);
            additive = Additive;
            oldPosition = Position;
            position = Position;
            velocity = Velocity;
            color = Color;
            rotation = Rotation;
            scale = Scale;
            startScale = Scale;
            spriteEffects = SpriteEffects.None;
            timeLeft = TimeLeft;
            maxTimeleft = TimeLeft;
            deceleration = Deceleration;
            fadeOutTime = fadeOutTimeLeftThreshold;
            Gravity = gravity;
            YCap = 5f;
            tileCollide = true;
            floatTime = airHangTime;
        }
        public override void AI()
        {
            velocity.X *= deceleration;
            if (Math.Abs(velocity.Y) > YCap)
            {
                velocity.Y = Math.Sign(velocity.Y) * YCap;
            }
            else if (Math.Abs(velocity.Y) < YCap && timeLeft < maxTimeleft - floatTime)
            {
                velocity.Y += Gravity;
            }

            if (timeLeft < fadeOutTime)
            {
                scale = startScale * (timeLeft / (float)fadeOutTime);
            }

            if (tileCollide)
            {
                Vector2 potentialPos = CheckFutureTileCollision();
                if (potentialPos != position + velocity)
                {
                    velocity.Y = 0;
                    position = potentialPos;
                }
            }
        }
    }
}
