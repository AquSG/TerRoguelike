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

                var anchorRoom = RoomID[FloorID[FloorDict["Surface"]].StartRoomID];
                Vector2 moonAnchor = anchorRoom.RoomPosition16 + anchorRoom.RoomDimensions16 * new Vector2(0.5f, 0.25f);
                Vector2 paralaxOff = (Main.Camera.Center - moonAnchor) * 0.6f;
                Vector2 drawPos = moonAnchor + paralaxOff - Main.screenPosition;

                Main.spriteBatch.End();
                Color glowColor = Color.Lerp(Color.White, Color.Cyan, 0.6f) * intensity;
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                Texture2D glowTex = TexDict["CircularGlow"];
                Main.EntitySpriteDraw(glowTex, drawPos, null, glowColor, 0, glowTex.Size() * 0.5f, 1.8f, SpriteEffects.None);
                Main.spriteBatch.End();
                Effect Pixelation = Filters.Scene["TerRoguelike:Pixelation"].GetShader().Shader;
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, Pixelation, Main.GameViewMatrix.TransformationMatrix);

                Color tint = Color.Lerp(Color.Lerp(Color.White, Color.Cyan, 0.5f), Color.Black, 0.3f) * intensity;
                Texture2D moonTex = TexDict["Moon"];
                Pixelation.Parameters["tint"].SetValue(tint.ToVector4());
                Pixelation.Parameters["dimensions"].SetValue(moonTex.Size());
                Pixelation.Parameters["offRot"].SetValue(Main.GlobalTimeWrappedHourly * 0.02f);
                Pixelation.Parameters["pixelation"].SetValue(8);

                Main.EntitySpriteDraw(moonTex, drawPos, null, Color.White, 0, moonTex.Size() * 0.5f, 0.5f, SpriteEffects.None);

                StartVanillaSpritebatch();
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
