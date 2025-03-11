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
        public static bool pendingEnemyHover = false;
        public static bool resetHover = false;
        public static bool buildingHover = false;
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

            

            MouseState ms = Mouse.GetState();


            pendingEnemyHover = mouseHitbox.Intersects(pendingEnemyBar);
            resetHover = mouseHitbox.Intersects(resetBar);
            buildingHover = mouseHitbox.Intersects(buildBar);

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

            DrawDebugUI(spriteBatch, UIScreenPos, pendingEnemyHover, resetHover, buildingHover);   
        }

        #region Draw Debug UI
        private static void DrawDebugUI(SpriteBatch spriteBatch, Vector2 screenPos, bool mainMenuHover, bool restartHover, bool buildingHover)
        {

            float opacity = 0.5f;

            Vector2 buttonScale = new Vector2(1.2f, 0.5f);
            Vector2 textOrigin = new Vector2(ButtonTex.Size().X * 1.1f, ButtonTex.Size().Y * 0.4f);

            Texture2D finalMainMenuButtonTex = mainMenuHover ? ButtonHoverTex : ButtonTex;
            Texture2D finalRestartButtonTex = restartHover ? ButtonHoverTex : ButtonTex;
            Texture2D finalBuildingButtonTex = buildingHover ? ButtonHoverTex : ButtonTex;
            spriteBatch.Draw(finalMainMenuButtonTex, screenPos + PendingButtonOffset, null, Color.White * opacity, 0f, ButtonTex.Size() * 0.5f, buttonScale, SpriteEffects.None, 0);
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.DeathText.Value, "Display Enemy Spawn Positions", screenPos + PendingButtonOffset, (mainMenuHover ? Color.White : Color.LightGoldenrodYellow) * opacity * 1.5f, 0f, textOrigin, new Vector2(mainMenuHover ? 1f : 0.9f) * 0.5f);
            spriteBatch.Draw(finalRestartButtonTex, screenPos + ResetButtonOffset, null, Color.White * opacity, 0f, ButtonTex.Size() * 0.5f, buttonScale, SpriteEffects.None, 0);
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.DeathText.Value, "Reset All Rooms In Play", screenPos + ResetButtonOffset, (restartHover ? Color.White : Color.LightGoldenrodYellow) * opacity * 1.5f, 0f, textOrigin, new Vector2(restartHover ? 1f : 0.9f) * 0.5f);
            spriteBatch.Draw(finalBuildingButtonTex, screenPos + BuildingButtonOffset, null, Color.White * opacity, 0f, ButtonTex.Size() * 0.5f, buttonScale, SpriteEffects.None, 0);
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.DeathText.Value, "Toggle Building", screenPos + BuildingButtonOffset, (restartHover ? Color.White : Color.LightGoldenrodYellow) * opacity * 1.5f, 0f, textOrigin, new Vector2(buildingHover ? 1f : 0.9f) * 0.5f);
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
            if (!TerRoguelikeWorld.IsTerRoguelikeWorld)
                return;

            TerRoguelikeMenu.prepareForRoguelikeGeneration = true;
            for (int i = 0; i < Main.maxNPCs; i++)
                Main.npc[i].active = false;
            for (int i = 0; i < Main.maxProjectiles; i++)
                Main.projectile[i].active = false;
            for (int i = 0; i < Main.maxDust; i++)
                Main.dust[i].active = false;
            for (int i = 0; i < Main.maxGore; i++)
                Main.gore[i].active = false;
            for (int i = 0; i < Main.maxCombatText; i++)
                Main.combatText[i].active = false;

            SetCalm(Silence);
            SetCombat(Silence);
            SetMusicMode(MusicStyle.Silent);
            RoomSystem.regeneratingWorld = true;
            WorldGen.gen = true;
            RoomSystem.ClearWorldTerRoguelike();
            Main.LocalPlayer.Center = new Vector2(Main.spawnTileX, Main.spawnTileY) * 16;

            bool loop = false;
            if (!loop)
            {
                IEnumerable<Item> vanillaItems = [];
                for (int i = 0; i < 58; i++)
                    Main.LocalPlayer.inventory[i].type = Main.LocalPlayer.inventory[i].stack = 0;
                List<Item> startingItems = PlayerLoader.GetStartingItems(Main.LocalPlayer, vanillaItems);
                PlayerLoader.SetStartInventory(Main.LocalPlayer, startingItems);
                Main.LocalPlayer.trashItem = new(ItemID.None, 0);
                TerRoguelikeMenu.desiredPlayer = Main.ActivePlayerFileData;
            }


            for (int i = 0; i < RoomSystem.RoomList.Count; i++)
                RoomSystem.ResetRoomID(RoomSystem.RoomList[i].ID);
            foreach (var floor in SchematicManager.FloorID)
            {
                floor.Reset();
            }

            ThreadPool.QueueUserWorkItem(_ => TerRoguelikeWorldManagementSystem.RegenerateWorld());
        }
    }
}
