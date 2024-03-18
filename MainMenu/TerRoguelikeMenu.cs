using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using TerRoguelike;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria.GameContent;
using Terraria.UI.Chat;
using Terraria.Audio;
using Terraria.ID;
using Terraria.GameContent.UI.States;
using Terraria.IO;
using System.IO;
using Terraria.Utilities;
using Terraria.UI.Gamepad;
using Terraria.GameInput;
using TerRoguelike.Managers;
using static TerRoguelike.Managers.TextureManager;

namespace TerRoguelike.MainMenu
{
    public static class TerRoguelikeMenu
    {
        public static bool prepareForRoguelikeGeneration = false;
        public static bool wipeTempPlayer = false;
        public static bool wipeTempWorld = false;
        public static PlayerFileData desiredPlayer = null;
        public static bool mouseHover = false;

        public static string DisplayName => "TerRoguelike";
        public static void DrawTerRoguelikeMenu()
        {
            if (Main.menuMode == 0)
            {
                if (wipeTempWorld)
                {
                    WorldFileData activeWorldFileData = Main.ActiveWorldFileData;
                    if (Main.netMode == NetmodeID.SinglePlayer)
                    {
                        bool isCloudSave = activeWorldFileData.IsCloudSave;
                        if (FileUtilities.Exists(Main.worldPathName, isCloudSave))
                        {
                            FileUtilities.Delete(Main.worldPathName, isCloudSave, true);
                        }
                        if (FileUtilities.Exists(Main.worldPathName + ".bak", isCloudSave))
                        {
                            FileUtilities.Delete(Main.worldPathName + ".bak", isCloudSave, true);
                        }
                        string moddedWorldPathName = Path.ChangeExtension(Main.worldPathName, ".twld");
                        if (FileUtilities.Exists(moddedWorldPathName, isCloudSave))
                        {
                            FileUtilities.Delete(moddedWorldPathName, isCloudSave, true);
                        }
                        if (FileUtilities.Exists(moddedWorldPathName + ".bak", isCloudSave))
                        {
                            FileUtilities.Delete(moddedWorldPathName + ".bak", isCloudSave, true);
                        }
                        Main.ActiveWorldFileData = new WorldFileData();
                    }
                    wipeTempWorld = false;
                    if (!wipeTempPlayer)
                    {
                        QuickCreateWorld();
                    }
                }
                if (wipeTempPlayer)
                {
                    PlayerFileData activePlayerFileData = Main.ActivePlayerFileData;
                    if (!activePlayerFileData.ServerSideCharacter)
                    {
                        bool isCloudSave = activePlayerFileData.IsCloudSave;
                        if (FileUtilities.Exists(Main.playerPathName, isCloudSave))
                        {
                            FileUtilities.Delete(Main.playerPathName, isCloudSave, true);
                        }
                        if (FileUtilities.Exists(Main.playerPathName + ".bak", isCloudSave))
                        {
                            FileUtilities.Delete(Main.playerPathName + ".bak", isCloudSave, true);
                        }
                        string moddedPlayerPathName = Path.ChangeExtension(Main.playerPathName, ".tplr");
                        if (FileUtilities.Exists(moddedPlayerPathName, isCloudSave))
                        {
                            FileUtilities.Delete(moddedPlayerPathName, isCloudSave, true);
                        }
                        if (FileUtilities.Exists(moddedPlayerPathName + ".bak", isCloudSave))
                        {
                            FileUtilities.Delete(moddedPlayerPathName + ".bak", isCloudSave, true);
                        }
                        Main.ActivePlayerFileData = new PlayerFileData();
                    }
                    wipeTempPlayer = false;
                }

                if (!wipeTempPlayer)
                    desiredPlayer = null;

                Vector2 position = new Vector2(Main.screenWidth / 2 - 150, Main.screenHeight * 0.75f);
                if (new Rectangle((int)position.X, (int)position.Y, 290, 40).Contains((int)Main.MouseScreen.X, (int)Main.MouseScreen.Y))
                {
                    if (!mouseHover)
                        SoundEngine.PlaySound(SoundID.MenuTick);

                    mouseHover = true;
                }
                else
                    mouseHover = false;

                Texture2D xButtonTex = TexDict["XButton"].Value;
                ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.DeathText.Value, "Play TerRoguelike", position, mouseHover ? Color.Cyan : Color.DarkCyan, 0f, Vector2.Zero, new Vector2(0.8f));
                if (PlayerInput.UsingGamepadUI)
                    Main.spriteBatch.Draw(xButtonTex, position + new Vector2(-36, 8), Color.White);

                if (PlayerInput.UsingGamepad ? GamePad.GetState(PlayerIndex.One).Buttons.X == ButtonState.Pressed : mouseHover && Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    SoundEngine.PlaySound(SoundID.MenuOpen);
                    Main.PendingPlayer = new Player();
                    Main.menuMode = 888;
                    Main.MenuUI.SetState(new UICharacterCreation(Main.PendingPlayer));
                    prepareForRoguelikeGeneration = true;
                }
            }
            if (Main.menuMode != 888 && Main.menuMode != 1 && Main.menuMode != 10 && Main.menuMode != 6)
                prepareForRoguelikeGeneration = false;
            else
            {
                if (prepareForRoguelikeGeneration)
                {
                    Main.PendingPlayer.difficulty = 0;
                }
            }
            if (Main.menuMode == 1 && prepareForRoguelikeGeneration)
            {
                QuickCreateWorld();
            }
            if (Main.menuMode == 6 && prepareForRoguelikeGeneration)
            {
                Main.menuMode = 10;
                WorldGen.playWorld();
            }
        }
        public static void QuickCreateWorld()
        {
            if (desiredPlayer != null)
                desiredPlayer.SetAsActive();

            Main.maxTilesX = 6400;
            Main.maxTilesY = 1800;
            Main.GameMode = 0;
            WorldGen.WorldGenParam_Evil = 0;
            Main.ActiveWorldFileData = WorldFile.CreateMetadata(Main.worldName = "The Dungeon", false, Main.GameMode);
            Main.ActiveWorldFileData.SetSeedToRandom();
            Main.menuMode = 10;
            WorldGen.CreateNewWorld();
        }
    }
}
