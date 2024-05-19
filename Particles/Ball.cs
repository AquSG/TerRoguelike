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

namespace TerRoguelike.Particles
{
    public class Ball : Particle
    {
        Vector2 startScale;
        float deceleration;
        int fadeOutTime;
        bool useLighting;
        Color startColor;
        public Ball(Vector2 Position, Vector2 Velocity, int TimeLeft, Color Color, Vector2 Scale, float Rotation = 0, float Deceleration = 0.96f, int fadeOutTimeLeftThreshold = 30, bool Additive = false, bool UseLighting = false)
        {
            texture = TexDict["Circle"];
            frame = new Rectangle(0, 0, texture.Width, texture.Height);
            additive = Additive;
            oldPosition = Position;
            position = Position;
            velocity = Velocity;
            color = startColor = Color;
            rotation = Rotation;
            scale = startScale = Scale * 0.1f;
            spriteEffects = SpriteEffects.None;
            timeLeft = TimeLeft;
            deceleration = Deceleration;
            fadeOutTime = fadeOutTimeLeftThreshold;
            useLighting = UseLighting;
        }
        public override void AI()
        {
            velocity *= deceleration;
            if (timeLeft < fadeOutTime)
            {
                scale = startScale * (timeLeft / (float)fadeOutTime);
            }
            if (useLighting)
            {
                color = startColor.MultiplyRGBA(Lighting.GetColor(position.ToTileCoordinates()));
            }
        }
    }
}
