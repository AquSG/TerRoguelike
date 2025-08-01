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
using TerRoguelike.Utilities;
using Terraria.Audio;
using TerRoguelike.Schematics;
using System.Threading;
using static TerRoguelike.Systems.MusicSystem;

namespace TerRoguelike.UI
{
    public static class DebugUI
    {
        private static Texture2D ButtonTex, ButtonHoverTex;
        private static Vector2 PendingButtonOffset = new Vector2(0, 0);
        private static Vector2 ResetButtonOffset = new Vector2(0, 60);
        private static Vector2 BuildingButtonOffset = new Vector2(0, -60);
        private static Vector2 QuickBuildButtonOffset = new Vector2(0, -120);
        public static bool pendingEnemyHover = false;
        public static bool resetHover = false;
        public static bool buildingHover = false;
        public static bool quickBuildHover = false;
        public static bool allowBuilding = false;
        public static bool DebugUIActive = false;
        public static ButtonState oldButtonState = ButtonState.Released;
        internal static void Load()
        {
            ButtonTex = TexDict["MenuButton"];
            ButtonHoverTex = TexDict["MenuButtonHover"];
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
            Rectangle buildBar = Utils.CenteredRectangle(UIScreenPos + BuildingButtonOffset, ButtonTex.Size() * buttonScale);
            Rectangle quickBuildBar = Utils.CenteredRectangle(UIScreenPos + QuickBuildButtonOffset, ButtonTex.Size() * buttonScale);

            

            MouseState ms = Mouse.GetState();


            pendingEnemyHover = mouseHitbox.Intersects(pendingEnemyBar);
            resetHover = mouseHitbox.Intersects(resetBar);
            buildingHover = mouseHitbox.Intersects(buildBar);
            quickBuildHover = mouseHitbox.Intersects(quickBuildBar);

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
                if (RoomSystem.RoomList != null && RoomSystem.RoomList.Count > 0)
                {
                    TerRoguelikeWorld.lunarFloorInitialized = false;
                    TerRoguelikeWorld.lunarBossSpawned = false;
                    TerRoguelikeWorld.escape = false;
                    TerRoguelikeWorld.escaped = false;
                    Main.LocalPlayer.ModPlayer().escaped = false;
                    Main.LocalPlayer.ModPlayer().creditsViewTime = 0;
                    Main.LocalPlayer.ModPlayer().escapeFail = false;
                    CreditsUI.creditsList = null;
                    CreditsSystem.StopCredits();
                    CutsceneSystem.cutsceneDisableControl = false;
                    TerRoguelikeWorld.escapeTime = 0;
                    for (int i = 0; i < RoomSystem.RoomList.Count; i++)
                    {
                        Room room = RoomSystem.RoomList[i];
                        RoomSystem.ResetRoomID(room.ID);
                    }
                    TerRoguelikeWorld.lunarGambitSceneTime = 0;
                    TerRoguelikeWorld.lunarGambitSceneStartPos = Vector2.Zero;

                    foreach (var floor in SchematicManager.FloorID)
                    {
                        floor.Reset();
                    }

                    if (SoundEngine.TryGetActiveSound(TerRoguelikeWorld.PortalSlot, out var portalSound))
                    {
                        portalSound.Stop();
                    }
                }
            }
            else if (pressed && buildingHover)
            {
                allowBuilding = !allowBuilding;
            }
            else if (pressed && quickBuildHover)
            {
                ItemManager.PastRoomRewardCategories.Clear();
                var modPlayer = Main.LocalPlayer.ModPlayer();
                if (modPlayer != null)
                {
                    modPlayer.escaped = true;
                }
                TerRoguelikeWorld.currentStage = 6;
                TerRoguelikeWorld.currentLoop = 0;
                if (RoomSystem.RoomList != null && RoomSystem.RoomList.Count > 0)
                {
                    Room room = SchematicManager.RoomID[SchematicManager.RoomDict["SanctuaryRoom1"]];
                    Main.LocalPlayer.Center = room.RoomCenter16 + room.RoomPosition16;
                    for (int i = 0; i < 5; i++)
                    {
                        SchematicManager.FloorID[RoomManager.FloorIDsInPlay[i]].jstcProgress = Floor.JstcProgress.Jstc;
                    }
                }
                for (int i = 0; i < 45; i++)
                {
                    bool boss = (i + 1) % 8 == 0 || i == 44;
                    ItemManager.QuickSpawnRoomItem(Main.LocalPlayer.Center, boss);
                }
            }

            DrawDebugUI(spriteBatch, UIScreenPos, pendingEnemyHover, resetHover, buildingHover, quickBuildHover);   
        }

        #region Draw Debug UI
        private static void DrawDebugUI(SpriteBatch spriteBatch, Vector2 screenPos, bool mainMenuHover, bool restartHover, bool buildingHover, bool quickBuildHover)
        {

            float opacity = 0.5f;

            Vector2 buttonScale = new Vector2(1.2f, 0.5f);
            Vector2 textOrigin = new Vector2(ButtonTex.Size().X * 1.1f, ButtonTex.Size().Y * 0.4f);

            Texture2D finalMainMenuButtonTex = mainMenuHover ? ButtonHoverTex : ButtonTex;
            Texture2D finalRestartButtonTex = restartHover ? ButtonHoverTex : ButtonTex;
            Texture2D finalBuildingButtonTex = buildingHover ? ButtonHoverTex : ButtonTex;
            Texture2D finalQuickBuildButtonTex = quickBuildHover ? ButtonHoverTex : ButtonTex;
            spriteBatch.Draw(finalMainMenuButtonTex, screenPos + PendingButtonOffset, null, Color.White * opacity, 0f, ButtonTex.Size() * 0.5f, buttonScale, SpriteEffects.None, 0);
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.DeathText.Value, "Display Enemy Spawn Positions", screenPos + PendingButtonOffset, (mainMenuHover ? Color.White : Color.LightGoldenrodYellow) * opacity * 1.5f, 0f, textOrigin, new Vector2(mainMenuHover ? 1f : 0.9f) * 0.5f);
            spriteBatch.Draw(finalRestartButtonTex, screenPos + ResetButtonOffset, null, Color.White * opacity, 0f, ButtonTex.Size() * 0.5f, buttonScale, SpriteEffects.None, 0);
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.DeathText.Value, "Reset All Rooms In Play", screenPos + ResetButtonOffset, (restartHover ? Color.White : Color.LightGoldenrodYellow) * opacity * 1.5f, 0f, textOrigin, new Vector2(restartHover ? 1f : 0.9f) * 0.5f);
            spriteBatch.Draw(finalBuildingButtonTex, screenPos + BuildingButtonOffset, null, Color.White * opacity, 0f, ButtonTex.Size() * 0.5f, buttonScale, SpriteEffects.None, 0);
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.DeathText.Value, "Toggle Building", screenPos + BuildingButtonOffset, (buildingHover ? Color.White : Color.LightGoldenrodYellow) * opacity * 1.5f, 0f, textOrigin, new Vector2(buildingHover ? 1f : 0.9f) * 0.5f);
            spriteBatch.Draw(finalBuildingButtonTex, screenPos + QuickBuildButtonOffset, null, Color.White * opacity, 0f, ButtonTex.Size() * 0.5f, buttonScale, SpriteEffects.None, 0);
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.DeathText.Value, "Quick Item Build", screenPos + QuickBuildButtonOffset, (quickBuildHover ? Color.White : Color.LightGoldenrodYellow) * opacity * 1.5f, 0f, textOrigin, new Vector2(quickBuildHover ? 1f : 0.9f) * 0.5f);
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

    public class RegenerateCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "RegenWorld";
        public override void Action(CommandCaller caller, string input, string[] args)
        {
            RoomSystem.RegenerateWorld();
        }
    }

    public class UnstuckCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "unstuck";
        public override void Action(CommandCaller caller, string input, string[] args)
        {
            var modPlayer = Main.LocalPlayer.ModPlayer();
            if (!TerRoguelikeWorld.IsTerRoguelikeWorld || modPlayer == null || modPlayer.stuckTime == 0 || RoomSystem.RoomList == null || RoomSystem.RoomList.Count == 0 || modPlayer.lastKnownRoom == -1) return;

            Room room = RoomSystem.RoomList[modPlayer.lastKnownRoom];
            Main.LocalPlayer.Center = room.FindPlayerAirNearRoomCenter();
        }
    }
}
