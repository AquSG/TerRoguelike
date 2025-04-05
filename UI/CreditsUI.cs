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
using Terraria.Localization;
using static TerRoguelike.UI.CreditsUI;
using TerRoguelike.Packets;

namespace TerRoguelike.UI
{
    public static class CreditsUI
    {
        private static Texture2D baseUITex, mainMenuButtonTex, mainMenuButtonHoverTex, moonTex;
        private static Vector2 mainMenuButtonOffset = new Vector2(-200, 206);
        private static Vector2 restartButtonOffset = new Vector2(200, 206);
        public static List<Item> itemsToDraw;
        public static bool mainMenuHover = false;
        public static bool restartHover = false;
        public static List<CreditsString> creditsList = null;
        internal static void Load()
        {
            baseUITex = TexDict["DeathUI"];
            mainMenuButtonTex = TexDict["MenuButton"];
            mainMenuButtonHoverTex = TexDict["MenuButtonHover"];
            moonTex = TexDict["UiMoon"];
            itemsToDraw = new List<Item>();
            Reset();
        }

        internal static void Unload()
        {
            Reset();
            baseUITex = mainMenuButtonTex = mainMenuButtonHoverTex = moonTex = null;
            itemsToDraw = null;
        }

        internal static void Reset()
        {
        }

        public static void Draw(SpriteBatch spriteBatch, Player player)
        {
            creditsList ??= GenerateCreditsList();

            TerRoguelikePlayer modPlayer = player.GetModPlayer<TerRoguelikePlayer>();

            Vector2 deathUIScreenPosRatio = new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.5f);
            // Convert the screen ratio position to an absolute position in pixels
            // Cast to integer to prevent blurriness which results from decimal pixel positions
            Vector2 DeathUIScreenPos = deathUIScreenPosRatio;
            DeathUIScreenPos.X = (int)(DeathUIScreenPos.X);
            DeathUIScreenPos.Y = (int)(DeathUIScreenPos.Y);

            Rectangle mouseHitbox = new Rectangle((int)Main.MouseScreen.X, (int)Main.MouseScreen.Y, 8, 8);

            Rectangle mainMenuBar = Utils.CenteredRectangle(DeathUIScreenPos + mainMenuButtonOffset, mainMenuButtonTex.Size());
            Rectangle restartBar = Utils.CenteredRectangle(DeathUIScreenPos + restartButtonOffset, mainMenuButtonTex.Size());

            

            MouseState ms = Mouse.GetState();
            GamePadState gs = GamePad.GetState(PlayerIndex.One);

            if (PlayerInput.UsingGamepad)
            {
                if (gs.ThumbSticks.Left.X < -0.4f || gs.DPad.Left == ButtonState.Pressed)
                {
                    mainMenuHover = true;
                    restartHover = false;
                }
                else if (gs.ThumbSticks.Left.X > 0.4f || gs.DPad.Right == ButtonState.Pressed)
                {
                    mainMenuHover = false;
                    restartHover = true;
                }
            }
            else
            {
                 mainMenuHover = mouseHitbox.Intersects(mainMenuBar);
                 restartHover = mouseHitbox.Intersects(restartBar);
            }

            bool pressed = PlayerInput.UsingGamepad ? gs.IsButtonDown(Buttons.A) || gs.IsButtonDown(Buttons.B) : ms.LeftButton == ButtonState.Pressed;
            if (pressed && mainMenuHover && modPlayer.creditsViewTime > 150)
            {
                ZoomSystem.SetZoomAnimation(Main.GameZoomTarget, 2);
                if (TerRoguelikeWorld.IsDeletableOnExit)
                {
                    TerRoguelikeMenu.wipeTempPlayer = true;
                    TerRoguelikeMenu.wipeTempWorld = true;
                }
                modPlayer.killerNPC = -1;
                modPlayer.killerProj = -1;
                SystemLoader.PreSaveAndQuit();
                WorldGen.SaveAndQuit();
            }
            else if (pressed && restartHover && modPlayer.creditsViewTime > 150)
            {
                ZoomSystem.SetZoomAnimation(Main.GameZoomTarget, 2);
                if (TerRoguelike.mpClient)
                {
                    StartRoomGenerationPacket.Send();
                }
                else
                {
                    bool stayinworld = true;
                    if (stayinworld)
                    {
                        RoomSystem.RegenerateWorld();
                        modPlayer.killerNPC = -1;
                        modPlayer.killerProj = -1;
                    }
                    else
                    {
                        if (TerRoguelikeWorld.IsDeletableOnExit)
                        {
                            TerRoguelikeMenu.wipeTempWorld = true;
                            TerRoguelikeMenu.prepareForRoguelikeGeneration = true;

                            IEnumerable<Item> vanillaItems = from item in player.inventory
                                                             where !item.IsAir
                                                             select item into x
                                                             select x.Clone();
                            List<Item> startingItems = PlayerLoader.GetStartingItems(player, vanillaItems);
                            PlayerLoader.SetStartInventory(player, startingItems);
                            player.trashItem = new(ItemID.None, 0);
                            TerRoguelikeMenu.desiredPlayer = Main.ActivePlayerFileData;
                        }
                        modPlayer.killerNPC = -1;
                        modPlayer.killerProj = -1;
                        WorldGen.SaveAndQuit();
                    }
                }
            }

            DrawCreditsUI(spriteBatch, modPlayer, DeathUIScreenPos, player, mainMenuHover, restartHover);   
        }

        #region Draw Credits UI
        private static void DrawCreditsUI(SpriteBatch spriteBatch, TerRoguelikePlayer modPlayer, Vector2 screenPos, Player player, bool mainMenuHover, bool restartHover)
        {
            float opacity = MathHelper.Clamp(MathHelper.Lerp(0, 1f, (modPlayer.creditsViewTime - 60) / 90f), 0, 1f);
            spriteBatch.Draw(baseUITex, screenPos, null, Color.White * 0.85f * opacity, 0f, baseUITex.Size() * 0.5f, 1f, SpriteEffects.None, 0);
            string itemsString = Language.GetOrRegister("Mods.TerRoguelike.DeathItems").Value;
            string victoryString = Language.GetOrRegister("Mods.TerRoguelike.CreditsVictory").Value;
            var font = FontAssets.DeathText.Value;

            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, itemsString, screenPos + new Vector2(-360, -250), Color.White * opacity, 0f, Vector2.Zero, new Vector2(0.9f));
            if (!modPlayer.escapeFail)
                ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, victoryString, screenPos + new Vector2(0, -340), Color.Lerp(Color.Yellow, Color.White, 0.5f) * opacity, 0f, font.MeasureString(victoryString) * 0.5f, new Vector2(1.4f));
            Vector2 creditsTextPos = screenPos + new Vector2(240, 80);
            float offsetPerCredit = 32;
            float maxOffset = (creditsList.Count) * offsetPerCredit;
            float baseOffset = -MathHelper.Clamp(modPlayer.creditsViewTime * 0.45f, 0, maxOffset);

            // Enforce a cutoff on everything. Thank you Calamity for letting me know how to do this
            Rectangle textCutoffRegion = new Rectangle((int)creditsTextPos.X, (int)creditsTextPos.Y, 1, 1);
            textCutoffRegion.Inflate(120, 150);
            textCutoffRegion.Y -= 124;
            var rasterizer = Main.Rasterizer;
            rasterizer.ScissorTestEnable = true;
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, rasterizer, null, Matrix.Identity);
            Rectangle cachedOldScissor = Main.spriteBatch.GraphicsDevice.ScissorRectangle;
            Main.spriteBatch.GraphicsDevice.ScissorRectangle = textCutoffRegion;

            for (int i = 0; i < creditsList.Count; i++)
            {
                var credit = creditsList[i];
                Vector2 creditoffset = new Vector2(0, offsetPerCredit * i + baseOffset);
                ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, credit.text, creditsTextPos + creditoffset, credit.color * opacity, 0f, font.MeasureString(credit.text) * 0.5f, credit.scale);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);
            rasterizer.ScissorTestEnable = false;
            Main.spriteBatch.GraphicsDevice.ScissorRectangle = cachedOldScissor;

            if (itemsToDraw.Count == 0)
            {
                for (int invItem = 0; invItem < 50; invItem++)
                {
                    Item item = player.inventory[invItem];
                    int rangedItemType = ItemManager.StarterRanged.FindIndex(x => x.id == item.type);
                    if (rangedItemType != -1)
                    {
                        itemsToDraw.Add(item);
                        continue;
                    }
                    int meleeItemType = ItemManager.StarterMelee.FindIndex(x => x.id == item.type);
                    if (meleeItemType != -1)
                    {
                        itemsToDraw.Add(item);
                        continue;
                    }
                    int rogueItemType = ItemManager.AllItems.FindIndex(x => x.modItemID == item.type);
                    if (rogueItemType != -1)
                    {
                        itemsToDraw.Add(item);
                        continue;
                    }
                }
            }

            if (itemsToDraw.Count > 0)
            {
                Vector2 itemDrawStartPos = new Vector2(-346, -180);
                Vector2 itemDisplayDimensions = new Vector2(48, 48);
                Vector2 itemDisplayPadding = new Vector2(1, 2);
                for (int i = 0; i < itemsToDraw.Count; i++)
                {
                    Item item = itemsToDraw[i];
                    Texture2D itemTex;
                    int xMultiplier = i % 10;
                    int yMultiplier = i / 10;
                    float scale;
                    Rectangle rect;
                    Main.GetItemDrawFrame(item.type, out itemTex, out rect);
                    if (itemTex.Width < itemTex.Height)
                    {
                        scale = 1f / (rect.Height / itemDisplayDimensions.Y);
                    }
                    else
                    {
                        scale = 1f / (itemTex.Width / itemDisplayDimensions.X);
                    }
                    if (scale > 1f)
                        scale = 1f;
                    Vector2 itemDrawPos = itemDrawStartPos + new Vector2((itemDisplayDimensions.X + itemDisplayPadding.X) * xMultiplier, (itemDisplayDimensions.Y + itemDisplayPadding.Y) * yMultiplier);
                    spriteBatch.Draw(itemTex, screenPos + itemDrawPos, rect, Color.White * opacity, 0f, rect.Size() * 0.5f, scale, SpriteEffects.None, 0);
                    if (item.stack > 1)
                    {
                        float textOffset = 8f - (6f * (item.stack.ToString().Count() - 1));
                        ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, item.stack.ToString(), screenPos + itemDrawPos + (Vector2.UnitX * textOffset), Color.White * opacity, 0f, Vector2.Zero, new Vector2(1f));
                    }
                }
            }

            string timeHeaderString = Language.GetOrRegister("Mods.TerRoguelike.CreditTimeHeader").Value;
            string runTime = modPlayer.playthroughTime.Elapsed.ToString();
            runTime = runTime.Substring(1, 11);
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, timeHeaderString, screenPos + new Vector2(130, -251), Color.GreenYellow * opacity, 0f, Vector2.Zero, new Vector2(0.5f));
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, runTime, screenPos + new Vector2(130, -231), Color.GreenYellow * opacity, 0f, Vector2.Zero, new Vector2(0.5f));

            string difficultyString = Language.GetOrRegister("Mods.TerRoguelike.MenuDifficulty").Value;
            Vector2 difficultyStringDimensions = font.MeasureString(difficultyString);
            Vector2 difficultyStringDrawPos = screenPos + new Vector2(-360, 100);
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, difficultyString, difficultyStringDrawPos, Color.Tomato * opacity, 0f, Vector2.Zero, new Vector2(0.6f));

            int moonFrameHeight = moonTex.Height / 5;
            Rectangle moonFrame = new Rectangle(0, moonFrameHeight * (int)TerRoguelikeMenu.difficulty, moonTex.Width, moonFrameHeight - 2);
            Vector2 moonDrawPos = difficultyStringDrawPos + new Vector2(12 + difficultyStringDimensions.X * 0.6f, -12);
            Main.EntitySpriteDraw(moonTex, moonDrawPos, moonFrame, Color.White * opacity, 0, Vector2.Zero, 1f, SpriteEffects.None);

            if (TerRoguelikeWorld.currentLoop > 0)
            {
                string loopString = Language.GetOrRegister("Mods.TerRoguelike.DeathLoop") + " " + TerRoguelikeWorld.currentLoop.ToString();
                ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, loopString, moonDrawPos + new Vector2(75, 12), Color.Lerp(Color.Cyan, Color.Blue, 0.15f) * opacity, 0, Vector2.Zero, new Vector2(0.6f));
            }

            string deathMainMenu = Language.GetOrRegister("Mods.TerRoguelike.DeathMainMenu").Value;
            string deathQuickRestart = Language.GetOrRegister("Mods.TerRoguelike.DeathQuickRestart").Value;
            Texture2D finalMainMenuButtonTex = mainMenuHover ? mainMenuButtonHoverTex : mainMenuButtonTex;
            Texture2D finalRestartButtonTex = restartHover ? mainMenuButtonHoverTex : mainMenuButtonTex;
            spriteBatch.Draw(finalMainMenuButtonTex, screenPos + mainMenuButtonOffset, null, Color.White * opacity, 0f, mainMenuButtonTex.Size() * 0.5f, 1f, SpriteEffects.None, 0);
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.DeathText.Value, deathMainMenu, screenPos + mainMenuButtonOffset, (mainMenuHover ? Color.White : Color.LightGoldenrodYellow) * opacity, 0f, mainMenuButtonTex.Size() * new Vector2(0.4f, 0.3f), new Vector2(mainMenuHover ? 1f : 0.9f));
            spriteBatch.Draw(finalRestartButtonTex, screenPos + restartButtonOffset, null, Color.White * opacity, 0f, mainMenuButtonTex.Size() * 0.5f, 1f, SpriteEffects.None, 0);
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.DeathText.Value, deathQuickRestart, screenPos + restartButtonOffset, (restartHover ? Color.White : Color.LightGoldenrodYellow) * opacity, 0f, mainMenuButtonTex.Size() * new Vector2(0.48f, 0.3f), new Vector2(restartHover ? 1f : 0.9f));
        }
        #endregion
        public static List<CreditsString> GenerateCreditsList()
        {
            creditsList = [];
            List<CreditsString> list = [];
            Vector2 headerScale = new Vector2(0.6f);
            Vector2 normalScale = new Vector2(0.45f);
            var font = FontAssets.DeathText.Value;

            for (int i = 0; i <= 9; i++)
            {
                string headerString;
                List<string> strings;
                Color headerColor;
                switch (i)
                {
                    default:
                    case 0:
                        headerString = Language.GetOrRegister("Mods.TerRoguelike.CreditDirector").Value;
                        headerColor = Color.LimeGreen;
                        strings = [
                        "AquaSG"
                        ];
                        break;
                    case 1:
                        headerString = Language.GetOrRegister("Mods.TerRoguelike.CreditProgramming").Value;
                        headerColor = Color.Red;
                        strings = [
                        "AquaSG"
                        ];
                        break;
                    case 2:
                        headerString = Language.GetOrRegister("Mods.TerRoguelike.CreditExternalCode").Value;
                        headerColor = Color.HotPink;
                        strings = [
                        "Calamity Mod",
                        "Starlight River"
                        ];
                        break;
                    case 3:
                        headerString = Language.GetOrRegister("Mods.TerRoguelike.CreditRoomBuilding").Value;
                        headerColor = Color.Lerp(Color.Brown, Color.White, 0.35f);
                        strings = [
                        "AquaSG",
                        "Sagittariod"
                        ];
                        break;
                    case 4:
                        headerString = Language.GetOrRegister("Mods.TerRoguelike.CreditSpriting").Value;
                        headerColor = Color.Cyan;
                        strings = [
                        "AquaSG",
                        "Potatostego",
                        "Sagittariod",
                        "Xyk"
                        ];
                        break;
                    case 5:
                        headerString = Language.GetOrRegister("Mods.TerRoguelike.CreditExternalAssets").Value;
                        headerColor = Color.Lerp(Color.Cyan, Color.Teal, 0.5f);
                        strings = [
                        "Terraria",
                        "Risk of Rain 2",
                        "Ultrakill",
                        "Undertale",
                        "Undertale Yellow",
                        "The Binding of Isaac",
                        "Armored Core VI",
                        "Minecraft",
                        "Pizza Tower",
                        "Pixabay",
                        "Adobe Stock"
                        ];
                        break;
                    case 6:
                        headerString = Language.GetOrRegister("Mods.TerRoguelike.CreditMusic").Value;
                        headerColor = Color.GreenYellow;
                        strings = [
                        "'Into the Fire'",
                        "Heaven Pierce Her",
                        "Ultrakill",
                        " ",
                        "'Xenomorph'",
                        "Karl Casey",
                        "White Bat Audio",
                        " ",
                        "'Dancer in the Darkness'",
                        "Heaven Pierce Her",
                        "Ultrakill",
                        " ",
                        "'Sword Flash'",
                        "Peritune",
                        " ",
                        "'Cold Winds'",
                        "Heaven Pierce Her",
                        "Ultrakill",
                        " ",
                        "'Pandemonium'",
                        "Heaven Pierce Her",
                        "Ultrakill",
                        " ",
                        "'Panic Betrayer'",
                        "Heaven Pierce Her",
                        "Ultrakill",
                        " ",
                        "'Deadly Force'",
                        "Karl Casey",
                        "White Bat Audio",
                        " ",
                        "'Deep Blue'",
                        "Heaven Pierce Her",
                        "Ultrakill",
                        " ",
                        "'Flap'",
                        "Peritune",
                        " ",
                        "'Sands of Tide'",
                        "Heaven Pierce Her",
                        "Ultrakill",
                        " ",
                        "'EpicBattle'",
                        "Peritune",
                        " ",
                        "'Danse Macabre'",
                        "Heaven Pierce Her",
                        "Ultrakill",
                        " ",
                        "'Collider'",
                        "Karl Casey",
                        "White Bat Audio",
                        " ",
                        "'Glory'",
                        "Heaven Pierce Her",
                        "Ultrakill",
                        " ",
                        "'Six Coffin Nails'",
                        "Kerosyn",
                        " ",
                        "'Tenebre Rosso Sangue'",
                        "KEYGEN CHURCH",
                        "Ultrakill",
                        " ",
                        "'Crisis'",
                        "Peritune",
                        " ",
                        "'Altars of Apostasy'",
                        "Heaven Pierce Her",
                        "Ultrakill",
                        " ",
                        "'EpicBattle J'",
                        "Peritune",
                        " ",
                        "'Sirens in Darkness'",
                        "The Cynic Project",
                        " ",
                        "'Unknown Cities'",
                        "Peritune",
                        " ",
                        "'Rapid4'",
                        "Peritune",
                        " ",
                        "'Steel Haze (Where Truths Meet)'",
                        "'Steel Haze (Rusted Pride)'",
                        "Shoi Miyazawa",
                        "Kota Hoshino",
                        "Armored Core VI",
                        " ",
                        "'Epic'",
                        "Peritune",
                        " ",
                        "'Dia Scriost'",
                        "Peritune",
                        " ",
                        "'Dark Moon'",
                        "Peritune",
                        " ",
                        "'Untouched Cache'",
                        "AquaSG",
                        "Hadalis",
                        ];
                        break;
                    case 7:
                        headerString = Language.GetOrRegister("Mods.TerRoguelike.CreditTesting").Value;
                        headerColor = Color.MediumPurple;
                        strings = [
                        "AquaSG",
                        "Potatostego",
                        "Sagittariod",
                        "Xyk",
                        "Eddie Spaghetti",
                        "Fabsol",
                        "Kirn",
                        "Memes",
                        "Spider Mod",
                        ];
                        break;
                    case 8:
                        headerString = Language.GetOrRegister("Mods.TerRoguelike.CreditSpecialThanks").Value;
                        headerColor = Color.Goldenrod;
                        strings = [
                        "Calamity Mod's Schematic System"
                        ];
                        break;
                    case 9:
                        headerString = Language.GetOrRegister("Mods.TerRoguelike.CreditFinal").Value;
                        headerColor = Color.Gray;
                        strings = [
                        " ",
                        " ",
                        " ",
                        " ",
                        " ",
                        " ",
                        " ",
                        Language.GetOrRegister("Mods.TerRoguelike.CreditThankYou").Value,
                        Language.GetOrRegister("Mods.TerRoguelike.CreditForPlaying").Value,
                        ];
                        break;
                }

                list.Add(new(headerString, headerScale, headerColor));
                foreach (string s in strings)
                {
                    Vector2 setScale = normalScale;
                    Vector2 stringDimensions = font.MeasureString(s);
                    float maxWidth = 460;
                    if (stringDimensions.X > maxWidth)
                    {
                        setScale *= (maxWidth / stringDimensions.X);
                    }
                    list.Add(new(s, setScale, Color.White));
                }
                list.Add(new(" ", headerScale, Color.White));
                list.Add(new(" ", headerScale, Color.White));
            }

            return list;
        }

        public class CreditsString
        {
            public string text;
            public Vector2 scale;
            public Color color;
            public CreditsString(string text, Vector2 scale, Color color)
            {
                this.text = text;
                this.scale = scale;
                this.color = color;
            }
        }
    }
}
