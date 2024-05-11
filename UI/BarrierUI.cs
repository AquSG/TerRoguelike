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
using TerRoguelike.World;
using Terraria.GameContent;
using Terraria.UI.Chat;
using static TerRoguelike.Managers.TextureManager;
using Terraria.Localization;

namespace TerRoguelike.UI
{
    public static class BarrierUI
    {
        //"ripper bars" lifted from the Calamity Mod and largely gutted to only the things needed for drawing the barrier bar
        private static Texture2D barrierBarTex, barrierBorderTex;
        internal static void Load()
        {
            barrierBarTex = TexDict["BarrierBar"];
            barrierBorderTex = TexDict["BarrierBarBorder"];

            Reset();
        }

        internal static void Unload()
        {
            Reset();
            barrierBarTex = barrierBorderTex = null;
        }

        internal static void Reset()
        {
        }

        public static void Draw(SpriteBatch spriteBatch, Player player)
        {
            if (TerRoguelikeWorld.escape)
            {
                int time = TerRoguelikeWorld.escapeTime;
                int maxTime = TerRoguelikeWorld.escapeTimeSet;
                int minutes = time / 3600;
                int seconds = (time - (minutes * 3600)) / 60;
                int miliseconds = (int)((time - (minutes * 3600) - (seconds * 60)) / 60f * 1000); 
                string secondsString = seconds.ToString().Length == 1 ? "0" + seconds.ToString() : seconds.ToString();
                string milisecondsString = miliseconds.ToString().Length == 1 ? "00" + miliseconds.ToString() : (miliseconds.ToString().Length == 2 ? "0" + miliseconds.ToString() : miliseconds.ToString());
                string timer = minutes.ToString() + ":" + secondsString + "." + milisecondsString;
                ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.DeathText.Value, timer, new Vector2((Main.screenWidth / 2) - 80, MathHelper.Clamp(MathHelper.Lerp(-60, 0, (maxTime - time) / 270f), -5000, 0)), Color.White, 0f, Vector2.Zero, new Vector2(1f));
            }


            TerRoguelikePlayer modPlayer = player.GetModPlayer<TerRoguelikePlayer>();
            if (player.dead || modPlayer.deathEffectTimer > 0)
                return;

            Vector2 barrierScreenRatioPos = new Vector2(((float)Main.screenWidth - (Main.UIScale * 361f)), (Main.UIScale * 32f));
            // Convert the screen ratio position to an absolute position in pixels
            // Cast to integer to prevent blurriness which results from decimal pixel positions
            Vector2 barrierScreenPos = barrierScreenRatioPos;
            barrierScreenPos.X = (int)(barrierScreenPos.X);
            barrierScreenPos.Y = (int)(barrierScreenPos.Y);

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
                string barrierName = Language.GetOrRegister("Mods.TerRoguelike.BarrierName").Value;
                Main.instance.MouseText(barrierName + $" " + "(" + ((int)modPlayer.barrierHealth).ToString() + "/" + player.statLifeMax2.ToString() + ")");

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
