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
    public class BallOutlined : Particle
    {
        Vector2 startScale;
        float deceleration;
        int fadeOutTime;
        float outlineWidth;
        Color outlineColor;
        public BallOutlined(Vector2 Position, Vector2 Velocity, int TimeLeft, Color OutlineColor, Color FillColor, Vector2 Scale, float OutlineWidth, float Rotation = 0, float Deceleration = 0.96f, int fadeOutTimeLeftThreshold = 30)
        {
            texture = TexDict["DarkTendril"].Value;
            frame = new Rectangle(0, 0, texture.Width, texture.Height);
            additive = false;
            oldPosition = Position;
            position = Position;
            velocity = Velocity;
            color = FillColor;
            outlineColor = OutlineColor;
            rotation = Rotation;
            scale = Scale;
            startScale = Scale;
            spriteEffects = SpriteEffects.None;
            timeLeft = TimeLeft;
            deceleration = Deceleration;
            fadeOutTime = fadeOutTimeLeftThreshold;
            outlineWidth = OutlineWidth;
        }
        public override void AI()
        {
            velocity *= deceleration;
            if (timeLeft < fadeOutTime)
            {
                scale = startScale * (timeLeft / (float)fadeOutTime);
            }
        }
        public override bool PreDraw()
        {
            Vector2 basePos = position - Main.screenPosition;
            Vector2 baseOffset = Vector2.UnitX * outlineWidth * (float)Math.Sqrt(scale.Length());
            Vector2 origin = frame.Size() * 0.5f;
            for (int i = 0; i < 8; i++)
            {
                Main.EntitySpriteDraw(texture, basePos + baseOffset.RotatedBy(i * MathHelper.PiOver4 + rotation), frame, outlineColor, rotation, origin, scale, spriteEffects);
            }
            return true;
        }
    }
}
