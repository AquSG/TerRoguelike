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
using TerRoguelike.UI;
using TerRoguelike.Tiles;
using Terraria.GameInput;
using TerRoguelike.Particles;
using Terraria.Graphics.Effects;

namespace TerRoguelike.Skies
{
    public class MoonLordSkyClone : CustomSky
    {
        public bool isActive = false;
        public float intensity;
        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth >= 0 && minDepth < 0)
            {
                spriteBatch.Draw(TextureAssets.BlackTile.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black * intensity);
            }
        }
        public override void Update(GameTime gameTime)
        {
            if (isActive)
            {
                intensity += 0.01f;
            }
            else
            {
                intensity -= 0.01f;
            }
            intensity = MathHelper.Clamp(intensity, 0, 1);
        }
        public override float GetCloudAlpha()
        {
            return 1f - intensity;
        }
        public override Color OnTileColor(Color inColor)
        {
            return Color.Lerp(new Color(0.5f, 0.8f, 1, 1), inColor, 1 - intensity);
        }
        public override bool IsActive()
        {
            return isActive || intensity > 0.001f;
        }
        public override void Activate(Vector2 position, params object[] args)
        {
            isActive = true;
            intensity = 1;
        }
        public override void Deactivate(params object[] args)
        {
            isActive = false;
        }
        public override void Reset()
        {
            isActive = false;
            intensity = 0;
        }
    }
    
}
