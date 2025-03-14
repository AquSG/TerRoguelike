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
    public class BigGore : Particle
    {
        Vector2 startScale;
        float acceleration;
        int fadeOutTime;
        float yVelCap;
        Color startColor;
        int maxTimeLeft;
        int direction;
        float rotAmount;

        public BigGore(Texture2D tex, Vector2 Position, Vector2 Velocity, int TimeLeft, Color Color, Vector2 Scale, float Acceleration, float yCap, float Rotation, int RotDirection, SpriteEffects SpriteEffects, float RotAmount = 0.075f)
        {
            if (Main.dedServ)
                return;

            texture = tex;
            frame = new Rectangle(0, 0, texture.Width, texture.Height);
            additive = false;
            position = oldPosition = Position;
            velocity = Velocity;
            color = startColor = Color;
            rotation = Rotation;
            scale = Scale;
            spriteEffects = SpriteEffects;
            timeLeft = TimeLeft;
            yVelCap = yCap;
            acceleration = Acceleration;
            direction = RotDirection;
            rotAmount = RotAmount;
        }
        public override void AI()
        {
            velocity.Y += acceleration;
            if (Math.Abs(velocity.Y) > Math.Abs(yVelCap))
                velocity.Y = Math.Sign(velocity.Y) * yVelCap;

            int time = maxTimeLeft - timeLeft;
            if (timeLeft < 60)
            {
                color = startColor * (timeLeft / 60f);
            }
            rotation += rotAmount * (Math.Abs(velocity.Y) / Math.Abs(yVelCap)) * direction;
        }
    }
}
