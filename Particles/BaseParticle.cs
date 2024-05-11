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
    public class Particle
    {
        public Texture2D texture;
        public Vector2 position;
        public Vector2 oldPosition;
        public float rotation;
        public Vector2 velocity;
        public SpriteEffects spriteEffects;
        public Color color;
        public Vector2 scale;
        public bool additive;
        public Rectangle frame;
        public int timeLeft;
        public virtual void AI()
        {

        }
        public void Update()
        {
            AI();
            position += velocity;
            timeLeft--;
        }
        public virtual bool PreDraw()
        {
            return true;
        }
        public void Draw()
        {
            if (!PreDraw())
                return;

            Main.EntitySpriteDraw(texture, position - Main.screenPosition, frame, color, rotation, frame.Size() * 0.5f, scale, spriteEffects);
        }
        public Vector2 CheckFutureTileCollision()
        {
            Vector2 end = position + velocity;
            Vector2 potentialPos = TileCollidePositionInLine(position, end);
            return potentialPos;
        }
    }
}
