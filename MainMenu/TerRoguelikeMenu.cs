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
using TerRoguelike.Utilities;
using TerRoguelike.World;
using TerRoguelike.Systems;
using Terraria.Localization;

namespace TerRoguelike.MainMenu
{
    public static class TerRoguelikeMenu
    {
        public static bool prepareForRoguelikeGeneration = false;
        public static bool wipeTempPlayer = false;
        public static bool wipeTempWorld = false;
        public static PlayerFileData desiredPlayer = null;
        public static bool mouseHover = false;
        public static bool permitPlayerDeletion = false;
        public static Difficulty difficulty = Difficulty.FullMoon;
        public static ButtonState oldGamepadXState = ButtonState.Released;
        public static bool NewMoonActive => TerRoguelikeWorld.IsTerRoguelikeWorld && difficulty == Difficulty.NewMoon;
        public static bool FullMoonActive => TerRoguelikeWorld.IsTerRoguelikeWorld && difficulty == Difficulty.FullMoon;
        public static bool BloodMoonActive => TerRoguelikeWorld.IsTerRoguelikeWorld && difficulty == Difficulty.BloodMoon;

        public enum Difficulty
        {
            NewMoon = 0,
            FullMoon = 1,
            BloodMoon = 2,
        }

        public static string DisplayName => "TerRoguelike";
        public static void TerRoguelikeMenuInteractionLogic()
        {
            if (Main.menuMode == 0)
            {
                if (wipeTempWorld)
                {
                    bool fullyDelete = ModContent.GetInstance<TerRoguelikeConfig>().FullyDeletePlayerAndWorldFiles;
                    WorldFileData activeWorldFileData = Main.ActiveWorldFileData;
                    if (Main.netMode == NetmodeID.SinglePlayer)
                    {
                        bool isCloudSave = activeWorldFileData.IsCloudSave;
                        if (FileUtilities.Exists(Main.worldPathName, isCloudSave))
                        {
                            FileUtilities.Delete(Main.worldPathName, isCloudSave, fullyDelete);
                        }
                        if (FileUtilities.Exists(Main.worldPathName + ".bak", isCloudSave))
                        {
                            FileUtilities.Delete(Main.worldPathName + ".bak", isCloudSave, fullyDelete);
                        }
                        string moddedWorldPathName = Path.ChangeExtension(Main.worldPathName, ".twld");
                        if (FileUtilities.Exists(moddedWorldPathName, isCloudSave))
                        {
                            FileUtilities.Delete(moddedWorldPathName, isCloudSave, fullyDelete);
                        }
                        if (FileUtilities.Exists(moddedWorldPathName + ".bak", isCloudSave))
                        {
                            FileUtilities.Delete(moddedWorldPathName + ".bak", isCloudSave, fullyDelete);
                        }
                        Main.ActiveWorldFileData = new WorldFileData();
                    }
                    wipeTempWorld = false;
                    if (!wipeTempPlayer)
                    {
                        TerRoguelikeWorldManagementSystem.currentlyGeneratingTerRoguelikeWorld = true;
                        QuickCreateWorld();
                    }
                }
                if (wipeTempPlayer)
                {
                    if (Main.LocalPlayer.ModPlayer() != null && Main.LocalPlayer.ModPlayer().isDeletableOnExit)
                    {
                        bool fullyDelete = ModContent.GetInstance<TerRoguelikeConfig>().FullyDeletePlayerAndWorldFiles;
                        PlayerFileData activePlayerFileData = Main.ActivePlayerFileData;
                        if (!activePlayerFileData.ServerSideCharacter)
                        {
                            bool isCloudSave = activePlayerFileData.IsCloudSave;
                            if (FileUtilities.Exists(Main.playerPathName, isCloudSave))
                            {
                                FileUtilities.Delete(Main.playerPathName, isCloudSave, fullyDelete);
                            }
                            if (FileUtilities.Exists(Main.playerPathName + ".bak", isCloudSave))
                            {
                                FileUtilities.Delete(Main.playerPathName + ".bak", isCloudSave, fullyDelete);
                            }
                            string moddedPlayerPathName = Path.ChangeExtension(Main.playerPathName, ".tplr");
                            if (FileUtilities.Exists(moddedPlayerPathName, isCloudSave))
                            {
                                FileUtilities.Delete(moddedPlayerPathName, isCloudSave, fullyDelete);
                            }
                            if (FileUtilities.Exists(moddedPlayerPathName + ".bak", isCloudSave))
                            {
                                FileUtilities.Delete(moddedPlayerPathName + ".bak", isCloudSave, fullyDelete);
                            }
                            Main.ActivePlayerFileData = new PlayerFileData();
                        }
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

                if (PlayerInput.UsingGamepad ? GamePad.GetState(PlayerIndex.One).Buttons.X == ButtonState.Pressed : mouseHover && Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    if (PlayerInput.UsingGamepad)
                        oldGamepadXState = ButtonState.Pressed;

                    SoundEngine.PlaySound(SoundID.MenuOpen);
                    Main.PendingPlayer = new Player();
                    Main.menuMode = 888;
                    Main.MenuUI.SetState(new UICharacterCreation(Main.PendingPlayer));
                    prepareForRoguelikeGeneration = true;
                }
            }
            if (prepareForRoguelikeGeneration && Main.menuMode == 888 && !TerRoguelikeWorldManagementSystem.currentlyGeneratingTerRoguelikeWorld)
            {
                Vector2 centerPos = new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.72f);
                Vector2 buttonDimensionsInflate = new Vector2(52);
                int backgroundInflateAmt = 6;
                int buttonCount = 3;
                Vector2 totalDimensions = new Vector2(buttonDimensionsInflate.X * buttonCount, buttonDimensionsInflate.Y);
                Vector2 topLeft = centerPos - totalDimensions * 0.5f;
                Rectangle backgroundDrawRect = new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)totalDimensions.X, (int)totalDimensions.Y);
                backgroundDrawRect.Inflate(backgroundInflateAmt, backgroundInflateAmt);

                Vector2 drawStart = new Vector2(backgroundDrawRect.X, backgroundDrawRect.Y);

                if (PlayerInput.UsingGamepad)
                {
                    if (GamePad.GetState(PlayerIndex.One).Buttons.X == ButtonState.Pressed && oldGamepadXState == ButtonState.Released)
                    {
                        int newDifficulty = (int)difficulty + 1;
                        if (newDifficulty >= buttonCount)
                            newDifficulty = 0;
                        difficulty = (Difficulty)newDifficulty;
                        SoundEngine.PlaySound(SoundID.MenuTick);
                    }
                }
                else
                {
                    for (int i = 0; i < buttonCount; i++)
                    {
                        Vector2 myDrawPos = drawStart + new Vector2(buttonDimensionsInflate.X * i, 0);
                        Rectangle hoverRect = new Rectangle((int)myDrawPos.X, (int)myDrawPos.Y, (int)buttonDimensionsInflate.X, (int)buttonDimensionsInflate.Y);
                        bool hover = hoverRect.Contains(Main.MouseScreen.ToPoint());

                        if (hover && PlayerInput.Triggers.JustPressed.MouseLeft)
                        {
                            difficulty = (Difficulty)i;
                            SoundEngine.PlaySound(SoundID.MenuTick);
                        }
                    }
                }
            }

            if (Main.menuMode != 888 && Main.menuMode != 1 && Main.menuMode != 10 && Main.menuMode != 6)
                prepareForRoguelikeGeneration = false;
            if (Main.menuMode != 888 && Main.menuMode != 0)
            {
                TerRoguelikeWorldManagementSystem.currentlyGeneratingTerRoguelikeWorld = false;
            }
            else
            {
                if (prepareForRoguelikeGeneration)
                {
                    Main.PendingPlayer.difficulty = 0;
                    //ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.DeathText.Value, "prepare", Main.MouseScreen, Color.White, 0f, Vector2.Zero, new Vector2(0.8f));
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
            if (PlayerInput.UsingGamepad)
                oldGamepadXState = GamePad.GetState(PlayerIndex.One).Buttons.X;
        }
        public static void DrawTerRoguelikeMenu()
        {
            if (Main.menuMode == 0)
            {
                Vector2 position = new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.75f);

                var font = FontAssets.DeathText.Value;
                string playString = Language.GetOrRegister("Mods.TerRoguelike.MenuPlayTerRoguelike").Value;
                ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, playString, position, mouseHover ? Color.Cyan : Color.DarkCyan, 0f, font.MeasureString(playString) * new Vector2(0.5f, 0), new Vector2(0.8f));
                if (PlayerInput.UsingGamepadUI)
                {
                    Texture2D xButtonTex = TexDict["XButton"];
                    Main.spriteBatch.Draw(xButtonTex, position + new Vector2(-36, 8) - font.MeasureString(playString) * new Vector2(0.4f, 0), Color.White);
                }
                    
            }
            if (prepareForRoguelikeGeneration && Main.menuMode == 888 && !TerRoguelikeWorldManagementSystem.currentlyGeneratingTerRoguelikeWorld)
            {
                var font = FontAssets.DeathText.Value;

                Vector2 centerPos = new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.72f);
                Vector2 buttonDimensionsInflate = new Vector2(52);
                int backgroundInflateAmt = 6;
                int buttonCount = 3;
                Vector2 totalDimensions = new Vector2(buttonDimensionsInflate.X * buttonCount, buttonDimensionsInflate.Y);
                Vector2 topLeft = centerPos - totalDimensions * 0.5f;
                Rectangle backgroundDrawRect = new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)totalDimensions.X, (int)totalDimensions.Y);
                backgroundDrawRect.Inflate(backgroundInflateAmt, backgroundInflateAmt);

                int edgeWidth = 12; // do not touch unless the texture is changed.
                Rectangle cornerFrame = new Rectangle(0, 0, edgeWidth, edgeWidth);
                Rectangle edgeFrame = new Rectangle(edgeWidth, 0, 2, edgeWidth);
                Rectangle fillFrame = new Rectangle(edgeWidth, edgeWidth, 1, 1);

                var buttonTex = TexDict["BasinOptionBox"];
                var buttonHoverTex = TexDict["BasinOptionBoxHover"];
                var buttonBackgroundTex = TexDict["BasinOptionsBackground"];

                Vector2 drawStart = new Vector2(backgroundDrawRect.X, backgroundDrawRect.Y);
                Vector2 backgroundDrawStart = drawStart + new Vector2(-edgeWidth * 0.5f);
                Vector2 fillDrawPos = backgroundDrawStart + new Vector2(edgeWidth);
                Vector2 fillScale = new Vector2(backgroundDrawRect.Width - edgeWidth * 2, backgroundDrawRect.Height - edgeWidth * 2);
                Color backgroundColor = Color.Lerp(Color.DarkSlateBlue, Color.Blue, 0.6f);
                Main.EntitySpriteDraw(buttonBackgroundTex, (fillDrawPos).ToPoint().ToVector2(), fillFrame, backgroundColor, 0, Vector2.Zero, fillScale, SpriteEffects.None);

                for (int i = 0; i < 4; i++)
                {
                    Vector2 cornerDrawStart = backgroundDrawStart;
                    Vector2 edgeScale;
                    if (i == 1 || i == 2)
                    {
                        cornerDrawStart.X += backgroundDrawRect.Width;
                    }
                    if (i == 2 || i == 3)
                    {
                        cornerDrawStart.Y += backgroundDrawRect.Height;
                    }
                    if (i % 2 == 0)
                        edgeScale = new Vector2(fillScale.X * 0.5f, 1);
                    else
                        edgeScale = new Vector2(fillScale.Y * 0.5f, 1);

                    float rot = i * MathHelper.PiOver2;

                    Vector2 sideDrawStart = cornerDrawStart + new Vector2(edgeWidth, 0).RotatedBy(rot);

                    Main.EntitySpriteDraw(buttonBackgroundTex, (sideDrawStart).ToPoint().ToVector2(), edgeFrame, backgroundColor, rot, Vector2.Zero, edgeScale, SpriteEffects.None);
                    Main.EntitySpriteDraw(buttonBackgroundTex, (cornerDrawStart).ToPoint().ToVector2(), cornerFrame, backgroundColor, rot, Vector2.Zero, 1f, SpriteEffects.None);
                }

                var moonTex = TexDict["UiMoon"];
                int moonFrameHeight = moonTex.Height / buttonCount;
                for (int i = 0; i < buttonCount; i++)
                {
                    bool hover = i == (int)difficulty;
                    Vector2 myDrawPos = drawStart + new Vector2(buttonDimensionsInflate.X * i, 0);
                    Rectangle moonFrame = new Rectangle(0, moonFrameHeight * i, moonTex.Width, moonFrameHeight - 2);

                    if (hover)
                    {
                        Color highlightColor = Color.Lerp(Color.Yellow, Color.White, 0.4f);
                        Main.EntitySpriteDraw(buttonHoverTex, myDrawPos.ToPoint().ToVector2(), null, highlightColor, 0, Vector2.Zero, 1f, SpriteEffects.None);
                    }
                    Main.EntitySpriteDraw(buttonTex, myDrawPos.ToPoint().ToVector2(), null, Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None);

                    Main.EntitySpriteDraw(moonTex, (myDrawPos + buttonDimensionsInflate * 0.5f).ToPoint().ToVector2(), moonFrame, Color.White, 0, moonFrame.Size() * 0.5f, 0.66f, SpriteEffects.None);
                }

                string difficultyString = Language.GetOrRegister("Mods.TerRoguelike.MenuDifficulty").Value;
                ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, difficultyString, centerPos + new Vector2(-6, -56), Color.White, 0f, font.MeasureString(difficultyString) * 0.5f, new Vector2(0.6f));

                string difficultyName;
                string difficultyDescription1;
                string difficultyDescription2;
                switch (difficulty)
                {
                    default:
                    case Difficulty.BloodMoon:
                        difficultyName = Language.GetOrRegister("Mods.TerRoguelike.BloodMoonName").Value;
                        difficultyDescription1 = Language.GetOrRegister("Mods.TerRoguelike.BloodMoonDescription1").Value;
                        difficultyDescription2 = Language.GetOrRegister("Mods.TerRoguelike.BloodMoonDescription2").Value;
                        break;
                    case Difficulty.NewMoon:
                        difficultyName = Language.GetOrRegister("Mods.TerRoguelike.NewMoonName").Value;
                        difficultyDescription1 = Language.GetOrRegister("Mods.TerRoguelike.NewMoonDescription1").Value;
                        difficultyDescription2 = Language.GetOrRegister("Mods.TerRoguelike.NewMoonDescription2").Value;
                        break;
                    case Difficulty.FullMoon:
                        difficultyName = Language.GetOrRegister("Mods.TerRoguelike.FullMoonName").Value;
                        difficultyDescription1 = Language.GetOrRegister("Mods.TerRoguelike.FullMoonDescription").Value;
                        difficultyDescription2 = "";
                        break;
                }
                ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, difficultyName, centerPos + new Vector2(-6, 30), Color.White, 0f, font.MeasureString(difficultyName) * new Vector2(0.5f, 0), new Vector2(0.54f));
                ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, difficultyDescription1, centerPos + new Vector2(-6, 58), Color.White, 0f, font.MeasureString(difficultyDescription1) * new Vector2(0.5f, 0), new Vector2(0.4f));
                ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, difficultyDescription2, centerPos + new Vector2(-6, 83), Color.White, 0f, font.MeasureString(difficultyDescription2) * new Vector2(0.5f, 0), new Vector2(0.4f));

                if (PlayerInput.UsingGamepadUI)
                {
                    Texture2D xButtonTex = TexDict["XButton"];
                    Main.spriteBatch.Draw(xButtonTex, centerPos + new Vector2(-124, -18), Color.White);
                }
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
