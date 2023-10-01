using System;
using System.Collections.Generic;
using System.Text;
using TerRoguelike.TerPlayer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace TerRoguelike.UI
{
    public static class BarrierUI
    {
        // These values were handpicked on a 1080p screen by Ozzatron. Please disregard the bizarre precision.
        private const float MouseDragEpsilon = 0.05f; // 0.05%

        private static Vector2? barrierDragOffset = null;


        private static Texture2D barrierBarTex, barrierBorderTex;
        internal static void Load()
        {
            barrierBarTex = ModContent.Request<Texture2D>("TerRoguelike/UI/BarrierBar", AssetRequestMode.ImmediateLoad).Value;
            barrierBorderTex = ModContent.Request<Texture2D>("TerRoguelike/UI/BarrierBarBorder", AssetRequestMode.ImmediateLoad).Value;

            Reset();
        }

        internal static void Unload()
        {
            Reset();
            barrierBarTex = barrierBorderTex = null;
        }

        internal static void Reset()
        {
            barrierDragOffset = null;
        }

        public static void Draw(SpriteBatch spriteBatch, Player player)
        {

            Vector2 barrierScreenRatioPos = new Vector2(((float)Main.screenWidth - (Main.UIScale * 361f)), (Main.UIScale * 32f));
            // Convert the screen ratio position to an absolute position in pixels
            // Cast to integer to prevent blurriness which results from decimal pixel positions
            Vector2 barrierScreenPos = barrierScreenRatioPos;
            barrierScreenPos.X = (int)(barrierScreenPos.X);
            barrierScreenPos.Y = (int)(barrierScreenPos.Y);

            // Grab the ModPlayer object and draw if applicable. If not applicable, save positions to config.
            TerRoguelikePlayer modPlayer = player.GetModPlayer<TerRoguelikePlayer>();

            DrawBarriereBar(spriteBatch, modPlayer, barrierScreenPos, player);
            
            float uiScale = Main.UIScale;
            Rectangle mouseHitbox = new Rectangle((int)Main.MouseScreen.X, (int)Main.MouseScreen.Y, 8, 8);

            Rectangle barrierBar = Utils.CenteredRectangle(barrierScreenPos, barrierBorderTex.Size() * uiScale);

            bool barrierHover = mouseHitbox.Intersects(barrierBar);

            MouseState ms = Mouse.GetState();
            Vector2 mousePos = Main.MouseScreen;

            if (barrierHover)
            {

                // Add hover text if the mouse is over Barrier bar
                Main.instance.MouseText($"Barrier " + "(" + ((int)modPlayer.barrierHealth).ToString() + "/" + player.statLifeMax2.ToString() + ")");

            }
        }

        #region Draw Barrier Bar
        private static void DrawBarriereBar(SpriteBatch spriteBatch, TerRoguelikePlayer modPlayer, Vector2 screenPos, Player player)
        {
            float uiScale = Main.UIScale;
            
            float barrierRatio = modPlayer.barrierHealth / (float)player.statLifeMax2;

            // Draw the border of the Barrier Bar first
            spriteBatch.Draw(barrierBorderTex, screenPos, null, Color.White, 0f, barrierBorderTex.Size() * 0.5f, uiScale, SpriteEffects.None, 0);

            // The amount of the bar to draw depends on the player's current Barrier
            // offset calculates the deadspace that is the border and not the bar. Bar is 24 pixels tall
            int barWidth = barrierBarTex.Width;
            float offset = (barrierBorderTex.Width - barrierBarTex.Width) * 0.5f;
            Rectangle cropRect = new Rectangle(0, 0, (int)(barWidth * barrierRatio), barrierBarTex.Height);
            spriteBatch.Draw(barrierBarTex, screenPos + new Vector2(offset * uiScale, 0), cropRect, Color.White, 0f, barrierBorderTex.Size() * 0.5f, uiScale, SpriteEffects.None, 0);
        }
        #endregion
    }
}
