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
using TerRoguelike.UI;
using Terraria.UI.Chat;
using ReLogic.Graphics;
using Terraria.ID;
using Terraria.Graphics;
using Terraria.GameContent;

namespace TerRoguelike.UI
{
    public static class DeathUI
    {
        private static Texture2D baseUITex, mainMenuButtonTex, mainMenuButtonHoverTex;
        private static Vector2 mainMenuButtonOffset = new Vector2(0, 206);
        internal static void Load()
        {
            baseUITex = ModContent.Request<Texture2D>("TerRoguelike/UI/DeathUI", AssetRequestMode.ImmediateLoad).Value;
            mainMenuButtonTex = ModContent.Request<Texture2D>("TerRoguelike/UI/MenuButton", AssetRequestMode.ImmediateLoad).Value;
            mainMenuButtonHoverTex = ModContent.Request<Texture2D>("TerRoguelike/UI/MenuButtonHover", AssetRequestMode.ImmediateLoad).Value;
            Reset();
        }

        internal static void Unload()
        {
            Reset();
            baseUITex = mainMenuButtonTex = mainMenuButtonHoverTex = null;
        }

        internal static void Reset()
        {
        }

        public static void Draw(SpriteBatch spriteBatch, Player player)
        {
            TerRoguelikePlayer modPlayer = player.GetModPlayer<TerRoguelikePlayer>();

            Vector2 deathUIScreenPosRatio = new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.5f);
            // Convert the screen ratio position to an absolute position in pixels
            // Cast to integer to prevent blurriness which results from decimal pixel positions
            Vector2 DeathUIScreenPos = deathUIScreenPosRatio;
            DeathUIScreenPos.X = (int)(DeathUIScreenPos.X);
            DeathUIScreenPos.Y = (int)(DeathUIScreenPos.Y);

            float uiScale = Main.UIScale;
            Rectangle mouseHitbox = new Rectangle((int)Main.MouseScreen.X, (int)Main.MouseScreen.Y, 8, 8);

            Rectangle mainMenuBar = Utils.CenteredRectangle(DeathUIScreenPos + mainMenuButtonOffset, mainMenuButtonTex.Size() * uiScale);

            bool mainMenuHover = mouseHitbox.Intersects(mainMenuBar);

            MouseState ms = Mouse.GetState();
            if (ms.LeftButton == ButtonState.Pressed && mainMenuHover)
            {
                WorldGen.SaveAndQuit();
            }

            DrawDeathUI(spriteBatch, modPlayer, DeathUIScreenPos, player, mainMenuHover);   
        }

        #region Draw Death UI
        private static void DrawDeathUI(SpriteBatch spriteBatch, TerRoguelikePlayer modPlayer, Vector2 screenPos, Player player, bool mainMenuHover)
        {
            float opacity = MathHelper.Clamp(MathHelper.Lerp(0, 1f, (modPlayer.deadTime - 120) / 60f), 0, 1f);
            // Draw the border of the Barrier Bar first
            spriteBatch.Draw(baseUITex, screenPos, null, Color.White * 0.85f * opacity, 0f, baseUITex.Size() * 0.5f, 1f, SpriteEffects.None, 0);
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.DeathText.Value, "Killed by:", screenPos + new Vector2(130, -250), Color.MediumPurple * opacity, 0f, Vector2.Zero, new Vector2(0.9f));
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.DeathText.Value, "Items:", screenPos + new Vector2(-360, -250), Color.White * opacity, 0f, Vector2.Zero, new Vector2(0.9f));
            if (modPlayer.killerNPC != -1)
            {
                Texture2D enemyTex = TextureAssets.Npc[modPlayer.killerNPCType].Value;
                int frameHeight = enemyTex.Height / Main.npcFrameCount[modPlayer.killerNPCType];
                float horizontalScale = 180f / (float)enemyTex.Width;
                float verticalScale = 250f / (float)frameHeight;
                float scale;
                if (horizontalScale > verticalScale)
                    scale = verticalScale;
                else
                    scale = horizontalScale;
                spriteBatch.Draw(enemyTex, screenPos + new Vector2(240, -40), new Rectangle(0, frameHeight, enemyTex.Width, frameHeight), Color.White * opacity, 0f, new Vector2(enemyTex.Width * 0.5f, (frameHeight * 0.5f)), scale, SpriteEffects.None, 0);
            }
            else if (modPlayer.killerProj != -1)
            {

                Texture2D projTex = TextureAssets.Projectile[modPlayer.killerProjType].Value;
                int frameHeight = projTex.Height / Main.npcFrameCount[modPlayer.killerNPCType];
                float horizontalScale = 180f / (float)projTex.Width;
                float verticalScale = 250f / (float)frameHeight;
                float scale;
                if (horizontalScale > verticalScale)
                    scale = verticalScale;
                else
                    scale = horizontalScale;
                spriteBatch.Draw(projTex, screenPos + new Vector2(240, -40), new Rectangle(0, frameHeight, projTex.Width, frameHeight), Color.White * opacity, 0f, new Vector2(projTex.Width * 0.5f, (frameHeight * 0.5f)), scale, SpriteEffects.None, 0);
            }
            Texture2D finalMainMenuButtonTex = mainMenuHover ? mainMenuButtonHoverTex : mainMenuButtonTex;
            spriteBatch.Draw(finalMainMenuButtonTex, screenPos + mainMenuButtonOffset, null, Color.White * opacity, 0f, mainMenuButtonTex.Size() * 0.5f, 1f, SpriteEffects.None, 0);
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.DeathText.Value, "Main Menu", screenPos + (mainMenuHover ? new Vector2(-108, 182) : new Vector2(-95, 185)), (mainMenuHover ? Color.White : Color.LightGoldenrodYellow) * opacity, 0f, Vector2.Zero, new Vector2(mainMenuHover ? 1f : 0.9f));
        }
        #endregion
    }
}
