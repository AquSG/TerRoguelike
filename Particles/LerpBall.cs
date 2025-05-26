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
    public class LerpBall : Particle
    {
        Vector2 startScale;
        float deceleration;
        int fadeOutTime;
        bool useLighting;
        Color startColor;
        Color endColor;
        int colorLerpTime;
        int maxTimeLeft;
        public LerpBall(Vector2 Position, Vector2 Velocity, int TimeLeft, Color StartColor, Color EndColor, int ColorLerpTime, Vector2 Scale, float Rotation = 0, float Deceleration = 0.96f, int fadeOutTimeLeftThreshold = 30, bool Additive = false)
        {
            if (Main.dedServ)
                return;

            texture = TexDict["Circle"];
            frame = new Rectangle(0, 0, texture.Width, texture.Height);
            additive = Additive;
            oldPosition = Position;
            position = Position;
            velocity = Velocity;
            color = startColor = StartColor;
            endColor = EndColor;
            rotation = Rotation;
            scale = startScale = Scale * 0.1f;
            spriteEffects = SpriteEffects.None;
            timeLeft = maxTimeLeft = TimeLeft;
            deceleration = Deceleration;
            fadeOutTime = fadeOutTimeLeftThreshold;
            colorLerpTime = ColorLerpTime;
        }
        public override void AI()
        {
            velocity *= deceleration;
            int time = (maxTimeLeft - timeLeft);
            color = Color.Lerp(startColor, endColor, MathHelper.Clamp(time / (float)colorLerpTime, 0, 1));
            if (timeLeft < fadeOutTime)
            {
                color *= timeLeft / (float)fadeOutTime;
            }
        }
    }
}
