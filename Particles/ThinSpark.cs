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
    public class ThinSpark : Particle
    {
        public Color startColor;
        public Vector2 startScale;
        public bool noGravity;
        public bool velocityToRotaion;
        public ThinSpark(Vector2 Position, Vector2 Velocity, int TimeLeft, Color Color, Vector2 Scale, float Rotation = 0, bool NoGravity = false, bool VelocityToRotation = true)
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
            timeLeft = TimeLeft;
            noGravity = NoGravity;
            velocityToRotaion = VelocityToRotation;
        }
        public override void AI()
        {
            velocity *= 0.96f;
            if (!noGravity)
                velocity.Y += 0.04f;
            if (velocityToRotaion)
                rotation = velocity.ToRotation();
            if (timeLeft < 30)
            {
                color = startColor * (timeLeft / 30f);
                scale = startScale * (timeLeft / 30f);
            }
        }
    }
}
