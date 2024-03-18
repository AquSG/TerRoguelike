﻿using System;
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
using TerRoguelike.Managers;
using System.Linq;
using TerRoguelike.Items;
using TerRoguelike.MainMenu;
using TerRoguelike.World;
using TerRoguelike.Systems;
using Terraria.GameInput;
using static TerRoguelike.Managers.TextureManager;

namespace TerRoguelike.UI
{
    public static class DebugUI
    {
        private static Texture2D ButtonTex, ButtonHoverTex;
        private static Vector2 PendingButtonOffset = new Vector2(-200, 206);
        private static Vector2 ResetButtonOffset = new Vector2(200, 206);
        public static bool pendingEnemyHover = false;
        public static bool resetHover = false;
        public static bool DebugUIActive = false;
        public static ButtonState oldButtonState = ButtonState.Released;
        internal static void Load()
        {
            ButtonTex = TexDict["MenuButton"].Value;
            ButtonHoverTex = TexDict["MenuButtonHover"].Value;
            Reset();
        }

        internal static void Unload()
        {
            Reset();
            ButtonTex = ButtonHoverTex = null;
        }

        internal static void Reset()
        {
        }

        public static void Draw(SpriteBatch spriteBatch, Player player)
        {
            if (!DebugUIActive)
                return;

            Vector2 UIScreenPosRatio = new Vector2(Main.screenWidth * 0.9f, Main.screenHeight * 0.85f);
            // Convert the screen ratio position to an absolute position in pixels
            // Cast to integer to prevent blurriness which results from decimal pixel positions
            Vector2 UIScreenPos = UIScreenPosRatio;
            UIScreenPos.X = (int)(UIScreenPos.X);
            UIScreenPos.Y = (int)(UIScreenPos.Y);

            Rectangle mouseHitbox = new Rectangle((int)Main.MouseScreen.X, (int)Main.MouseScreen.Y, 8, 8);

            Vector2 buttonScale = new Vector2(1.2f, 0.5f);

            Rectangle pendingEnemyBar = Utils.CenteredRectangle(UIScreenPos + PendingButtonOffset, ButtonTex.Size() * buttonScale);
            Rectangle resetBar = Utils.CenteredRectangle(UIScreenPos + ResetButtonOffset, ButtonTex.Size() * buttonScale);

            

            MouseState ms = Mouse.GetState();


            pendingEnemyHover = mouseHitbox.Intersects(pendingEnemyBar);
            resetHover = mouseHitbox.Intersects(resetBar);

            bool pressed = ms.LeftButton == ButtonState.Released && oldButtonState == ButtonState.Pressed;

            oldButtonState = ms.LeftButton;

            if (pressed && pendingEnemyHover)
            {
                if (!RoomSystem.debugDrawNotSpawnedEnemies)
                    RoomSystem.debugDrawNotSpawnedEnemies = true;
                else
                    RoomSystem.debugDrawNotSpawnedEnemies = false;
            }
            else if (pressed && resetHover)
            {
                if (RoomSystem.RoomList != null && RoomSystem.RoomList.Any())
                {
                    for (int i = 0; i < RoomSystem.RoomList.Count; i++)
                    {
                        Room room = RoomSystem.RoomList[i];
                        RoomSystem.ResetRoomID(room.ID);
                    }
                }
            }

            DrawDebugUI(spriteBatch, UIScreenPos, pendingEnemyHover, resetHover);   
        }

        #region Draw Debug UI
        private static void DrawDebugUI(SpriteBatch spriteBatch, Vector2 screenPos, bool mainMenuHover, bool restartHover)
        {

            float opacity = 0.5f;

            Vector2 buttonScale = new Vector2(1.2f, 0.5f);
            PendingButtonOffset = new Vector2(0, 0);
            ResetButtonOffset = new Vector2(0, 60);
            Vector2 textOrigin = new Vector2(ButtonTex.Size().X * 1.1f, ButtonTex.Size().Y * 0.4f);

            Texture2D finalMainMenuButtonTex = mainMenuHover ? ButtonHoverTex : ButtonTex;
            Texture2D finalRestartButtonTex = restartHover ? ButtonHoverTex : ButtonTex;
            spriteBatch.Draw(finalMainMenuButtonTex, screenPos + PendingButtonOffset, null, Color.White * opacity, 0f, ButtonTex.Size() * 0.5f, buttonScale, SpriteEffects.None, 0);
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.DeathText.Value, "Display Enemy Spawn Positions", screenPos + PendingButtonOffset, (mainMenuHover ? Color.White : Color.LightGoldenrodYellow) * opacity * 1.5f, 0f, textOrigin, new Vector2(mainMenuHover ? 1f : 0.9f) * 0.5f);
            spriteBatch.Draw(finalRestartButtonTex, screenPos + ResetButtonOffset, null, Color.White * opacity, 0f, ButtonTex.Size() * 0.5f, buttonScale, SpriteEffects.None, 0);
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.DeathText.Value, "Reset All Rooms In Play", screenPos + ResetButtonOffset, (restartHover ? Color.White : Color.LightGoldenrodYellow) * opacity * 1.5f, 0f, textOrigin, new Vector2(restartHover ? 1f : 0.9f) * 0.5f);
        }
        #endregion
    }
    public class DebugCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "TerRoguelikeDebugTools";
        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (!DebugUI.DebugUIActive)
            {
                DebugUI.DebugUIActive = true;
                DebugUI.oldButtonState = ButtonState.Released;
            }
            else
                DebugUI.DebugUIActive = false;
        }
    }
}