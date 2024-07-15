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
using static TerRoguelike.Managers.ItemManager;

namespace TerRoguelike.MainMenu
{
    public static class TerRoguelikeMenu
    {
        public static int rangedSelection = 0;
        public static int meleeSelection = 0;
        public static int uiControllerCycle = 0;
        public static bool prepareForRoguelikeGeneration = false;
        public static bool wipeTempPlayer = false;
        public static bool wipeTempWorld = false;
        public static PlayerFileData desiredPlayer = null;
        public static bool mouseHover = false;
        public static bool permitPlayerDeletion = false;
        public static Difficulty difficulty = Difficulty.FullMoon;
        public static ButtonState oldGamepadXState = ButtonState.Released;
        public static ButtonState oldGamepadLeftStickState = ButtonState.Released;
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
                if (PlayerInput.UsingGamepad && GamePad.GetState(PlayerIndex.One).Buttons.LeftStick == ButtonState.Pressed && oldGamepadLeftStickState == ButtonState.Released)
                {
                    oldGamepadLeftStickState = ButtonState.Pressed;
                    uiControllerCycle++;
                    if (uiControllerCycle > 2)
                        uiControllerCycle = 0;
                }
                DifficultyInteraction();
                WeaponInteraction();

                void DifficultyInteraction()
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

                    if (PlayerInput.UsingGamepad && uiControllerCycle == 0)
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

                void WeaponInteraction()
                {
                    if (PlayerInput.UsingGamepad)
                    {
                        if (GamePad.GetState(PlayerIndex.One).Buttons.X == ButtonState.Pressed && oldGamepadXState == ButtonState.Released)
                        {
                            if (uiControllerCycle == 1)
                            {
                                rangedSelection++;
                                if (rangedSelection >= StarterRanged.Count)
                                    rangedSelection = 0;
                                SoundEngine.PlaySound(SoundID.MenuTick);
                            }
                            else if (uiControllerCycle == 2)
                            {
                                meleeSelection++;
                                if (meleeSelection >= StarterMelee.Count)
                                    meleeSelection = 0;
                                SoundEngine.PlaySound(SoundID.MenuTick);
                            }
                        }
                    }
                    else
                    {
                        for (int s = -1; s <= 1; s += 2)
                        {
                            Vector2 centerPos = new Vector2(Main.screenWidth * 0.5f + 295 * s, Main.screenHeight * 0.47f);
                            Vector2 buttonDimensionsInflate = new Vector2(52);
                            int backgroundInflateAmt = 6;

                            List<StarterItem> itemList = s switch
                            {
                                1 => StarterMelee,
                                _ => StarterRanged,
                            };

                            int buttonCount = itemList.Count;
                            Vector2 totalDimensions = new Vector2(buttonDimensionsInflate.X, buttonDimensionsInflate.Y * buttonCount);
                            Vector2 topLeft = centerPos - totalDimensions * 0.5f;
                            Rectangle backgroundDrawRect = new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)totalDimensions.X, (int)totalDimensions.Y);
                            backgroundDrawRect.Inflate(backgroundInflateAmt, backgroundInflateAmt);

                            Vector2 drawStart = new Vector2(backgroundDrawRect.X, backgroundDrawRect.Y);

                            Vector2 itemDisplayDimensions = new Vector2(36);
                            for (int i = 0; i < buttonCount; i++)
                            {
                                Vector2 myDrawPos = drawStart + new Vector2(0, buttonDimensionsInflate.Y * i);

                                Rectangle hoverRect = new Rectangle((int)myDrawPos.X, (int)myDrawPos.Y, (int)buttonDimensionsInflate.X, (int)buttonDimensionsInflate.Y);
                                bool hover = hoverRect.Contains(Main.MouseScreen.ToPoint());

                                if (hover && PlayerInput.Triggers.JustPressed.MouseLeft)
                                {
                                    switch (s)
                                    {
                                        default:
                                        case -1:
                                            rangedSelection = i;
                                            break;
                                        case 1:
                                            meleeSelection = i;
                                            break;
                                    }
                                    SoundEngine.PlaySound(SoundID.MenuTick);
                                }
                            }
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
            {
                oldGamepadXState = GamePad.GetState(PlayerIndex.One).Buttons.X;
                oldGamepadLeftStickState = GamePad.GetState(PlayerIndex.One).Buttons.LeftStick;
            }
                
        }
        public static void DrawTerRoguelikeMenu()
        {
            if (Main.menuMode == 0)
            {
                Vector2 position = new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.75f);
                if (Main.screenHeight < 800)
                {
                    position.Y += 60;
                }

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

                DifficultyDrawing();
                StarterWeaponDrawing();

                void DifficultyDrawing()
                {
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

                    if (PlayerInput.UsingGamepadUI && uiControllerCycle == 0)
                    {
                        Texture2D xButtonTex = TexDict["XButton"];
                        var lClickTex = TexDict["LStickButton"];
                        Main.spriteBatch.Draw(xButtonTex, centerPos + new Vector2(-114, -6), null, Color.White, 0, xButtonTex.Size() * 0.5f, 1f, SpriteEffects.None, 0);
                        Main.spriteBatch.Draw(lClickTex, centerPos + new Vector2(-144, -6), null, Color.White, 0, lClickTex.Size() * 0.5f, 0.6f, SpriteEffects.None, 0);
                    }
                }

                void StarterWeaponDrawing()
                {
                    for (int s = -1; s <= 1; s += 2)
                    {
                        Vector2 centerPos = new Vector2(Main.screenWidth * 0.5f + 295 * s, Main.screenHeight * 0.47f);
                        Vector2 buttonDimensionsInflate = new Vector2(52);
                        int backgroundInflateAmt = 6;

                        List<StarterItem> itemList = s switch
                        {
                            1 => StarterMelee,
                            _ => StarterRanged,
                        };

                        int buttonCount = itemList.Count;
                        Vector2 totalDimensions = new Vector2(buttonDimensionsInflate.X, buttonDimensionsInflate.Y * buttonCount);
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

                        Vector2 itemDisplayDimensions = new Vector2(36);
                        for (int i = 0; i < buttonCount; i++)
                        {
                            var currentItem = itemList[i];
                            var itemTex = TextureAssets.Item[currentItem.id].Value;
                            bool hover = s switch
                            {
                                1 => i == meleeSelection,
                                _ => i == rangedSelection,
                            };
                            Vector2 myDrawPos = drawStart + new Vector2(0, buttonDimensionsInflate.Y * i);

                            if (hover)
                            {
                                Color highlightColor = Color.Lerp(Color.Yellow, Color.White, 0.4f);
                                Main.EntitySpriteDraw(buttonHoverTex, myDrawPos.ToPoint().ToVector2(), null, highlightColor, 0, Vector2.Zero, 1f, SpriteEffects.None);
                            }
                            Main.EntitySpriteDraw(buttonTex, myDrawPos.ToPoint().ToVector2(), null, Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None);

                            float itemScale;
                            if (itemTex.Width < itemTex.Height)
                            {
                                itemScale = 1f / (itemTex.Height / itemDisplayDimensions.Y);
                            }
                            else
                            {
                                itemScale = 1f / (itemTex.Width / itemDisplayDimensions.X);
                            }
                            if (itemScale > 1f)
                                itemScale = 1f;

                            Main.EntitySpriteDraw(itemTex, (myDrawPos + buttonDimensionsInflate * 0.5f).ToPoint().ToVector2(), null, Color.White, 0, itemTex.Size() * 0.5f, itemScale, SpriteEffects.None);
                        }


                        if (PlayerInput.UsingGamepad && ((uiControllerCycle == 1 && s == -1) || (uiControllerCycle == 2 && s == 1)))
                        {
                            Texture2D xButtonTex = TexDict["XButton"];
                            var lClickTex = TexDict["LStickButton"];
                            Main.spriteBatch.Draw(xButtonTex, drawStart + buttonDimensionsInflate.X * 0.5f * Vector2.UnitX + 40 * s * Vector2.UnitX - 30 * Vector2.UnitY, null, Color.White, 0, xButtonTex.Size() * 0.5f, 1f, SpriteEffects.None, 0);
                            Main.spriteBatch.Draw(lClickTex, drawStart + buttonDimensionsInflate.X * 0.5f * Vector2.UnitX + 70 * s * Vector2.UnitX - 30 * Vector2.UnitY, null, Color.White, 0, lClickTex.Size() * 0.5f, 0.6f, SpriteEffects.None, 0);
                        }
                        int selection = s switch
                        {
                            1 => meleeSelection,
                            _ => rangedSelection,
                        };
                        var dummyItem = new Item(itemList[selection].id);
                        string itemName = dummyItem.Name;
                        Vector2 itemNameDimensions = font.MeasureString(itemName);
                        ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, itemName, drawStart + buttonDimensionsInflate.X * 0.5f * Vector2.UnitX + 40 * s * Vector2.UnitX, Color.White, 0, itemNameDimensions * new Vector2(0.5f + 0.5f * -s, 0), new Vector2(0.5f));

                        List<string> itemDesc = [];
                        var itemTip = dummyItem.ToolTip;
                        for (int i = 2; i < itemTip.Lines; i++)
                        {
                            itemDesc.Add(itemTip.GetLine(i));
                        }

                        float yPerLine = font.MeasureString("ypjiILkPMN").Y * 0.3f;
                        float biggestDimension = 0;
                        for (int i = 0; i < itemDesc.Count; i++)
                        {
                            biggestDimension = Math.Max(font.MeasureString(itemDesc[i]).X, biggestDimension);
                        }
                        for (int i = 0; i < itemDesc.Count; i++)
                        {
                            Vector2 itemDescDimensions = font.MeasureString(itemDesc[i]);
                            ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, itemDesc[i], drawStart + buttonDimensionsInflate.X * 0.5f * Vector2.UnitX + 40 * s * Vector2.UnitX + Vector2.UnitY * (itemNameDimensions.Y * 0.5f + yPerLine * i), Color.White, 0, itemDescDimensions * new Vector2(0.5f + 0.5f * -s, 0), new Vector2(0.3f));
                        }
                        
                    }
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
