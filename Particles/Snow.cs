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
    public class Snow : Particle
    {
        Vector2 startScale;
        float deceleration;
        int fadeOutTime;
        float Gravity;
        float XCap;
        float YCap;
        bool tileCollide;
        int maxTimeleft;
        int floatTime;
        float periodOffset;
        public Snow(Vector2 Position, Vector2 Velocity, int TimeLeft, Color Color, Vector2 Scale, float Rotation = 0, float Deceleration = 0.96f, float gravity = 0.04f, int fadeOutTimeLeftThreshold = 30, int airHangTime = 30, bool TileCollide = true, bool Additive = false)
        {
            texture = TexDict["Snowflake"].Value;
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
            XCap = 6f;
            YCap = 8f;
            tileCollide = TileCollide;
            floatTime = airHangTime;
            periodOffset = Main.rand.NextFloat(MathHelper.TwoPi);
        }
        public override void AI()
        {
            velocity.X *= deceleration;
            if (Math.Abs(velocity.X) > XCap)
            {
                velocity.X = Math.Sign(velocity.X) * XCap;
            }
            if (velocity.Y > YCap)
            {
                velocity.Y = YCap;
            }
            else if (velocity.Y < YCap && timeLeft < maxTimeleft - floatTime)
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
                    velocity.X *= 0.7f;
                    position = potentialPos;
                }
            }
            if (velocity.Y != 0)
            {
                velocity.X += (float)Math.Sin(periodOffset + (timeLeft / 120f)) * 0.1f;
            }
            rotation += velocity.X * 0.01f;
        }
    }
}
