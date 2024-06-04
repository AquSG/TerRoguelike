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

namespace TerRoguelike.UI
{
    public static class CreditsUI
    {
        private static Texture2D baseUITex, mainMenuButtonTex, mainMenuButtonHoverTex;
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
            itemsToDraw = new List<Item>();
            Reset();
        }

        internal static void Unload()
        {
            Reset();
            baseUITex = mainMenuButtonTex = mainMenuButtonHoverTex = null;
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

            bool pressed = PlayerInput.UsingGamepad ? gs.IsButtonDown(Buttons.A) : ms.LeftButton == ButtonState.Pressed;
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
                if (TerRoguelikeWorld.IsDeletableOnExit)
                {
                    IEnumerable<Item> vanillaItems = from item in player.inventory
                                                     where !item.IsAir
                                                     select item into x
                                                     select x.Clone();
                    List<Item> startingItems = PlayerLoader.GetStartingItems(player, vanillaItems);
                    PlayerLoader.SetStartInventory(player, startingItems);
                    TerRoguelikeMenu.desiredPlayer = Main.ActivePlayerFileData;
                    TerRoguelikeMenu.wipeTempWorld = true;
                    TerRoguelikeMenu.prepareForRoguelikeGeneration = true;
                }
                modPlayer.killerNPC = -1;
                modPlayer.killerProj = -1;
                WorldGen.SaveAndQuit();
            }

            DrawCreditsUI(spriteBatch, modPlayer, DeathUIScreenPos, player, mainMenuHover, restartHover);   
        }

        #region Draw Credits UI
        private static void DrawCreditsUI(SpriteBatch spriteBatch, TerRoguelikePlayer modPlayer, Vector2 screenPos, Player player, bool mainMenuHover, bool restartHover)
        {
            float opacity = MathHelper.Clamp(MathHelper.Lerp(0, 1f, (modPlayer.creditsViewTime - 20) / 80f), 0, 1f);
            spriteBatch.Draw(baseUITex, screenPos, null, Color.White * 0.85f * opacity, 0f, baseUITex.Size() * 0.5f, 1f, SpriteEffects.None, 0);
            string itemsString = Language.GetOrRegister("Mods.TerRoguelike.DeathItems").Value;
            string victoryString = Language.GetOrRegister("Mods.TerRoguelike.CreditsVictory").Value;
            var font = FontAssets.DeathText.Value;

            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, itemsString, screenPos + new Vector2(-360, -250), Color.White * opacity, 0f, Vector2.Zero, new Vector2(0.9f));
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, victoryString, screenPos + new Vector2(0, -340), Color.Lerp(Color.Yellow, Color.White, 0.5f) * opacity, 0f, font.MeasureString(victoryString) * 0.5f, new Vector2(1.4f));
            Vector2 creditsTextPos = screenPos + new Vector2(240, 80);
            float offsetPerCredit = 32;
            float maxOffset = (creditsList.Count) * offsetPerCredit;
            float baseOffset = -MathHelper.Clamp(modPlayer.creditsViewTime * 0.31f, 0, maxOffset);

            // Enforce a cutoff on everything. Thank you Calamity for letting me know how to do this
            Rectangle textCutoffRegion = new Rectangle((int)creditsTextPos.X, (int)creditsTextPos.Y, 1, 1);
            textCutoffRegion.Inflate(120, 150);
            textCutoffRegion.Y -= 124;
            var rasterizer = Main.Rasterizer;
            rasterizer.ScissorTestEnable = true;
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, rasterizer, null, Matrix.Identity);
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

            if (itemsToDraw.Count == 0)
            {
                for (int invItem = 0; invItem < 50; invItem++)
                {
                    Item item = player.inventory[invItem];
                    int rogueItemType = ItemManager.AllItems.FindIndex(x => x.modItemID == item.type);
                    if (rogueItemType != -1)
                    {
                        itemsToDraw.Add(item);
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
                        "AquaSG"
                        ];
                        break;
                    case 4:
                        headerString = Language.GetOrRegister("Mods.TerRoguelike.CreditSpriting").Value;
                        headerColor = Color.Cyan;
                        strings = [
                        "AquaSG",
                        "Potatostego",
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
                        "Pixbay",
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
                        "'You're Gonna Need a Bigger Ukelele'",
                        "Chris Christodoulou",
                        "Risk of Rain 2",
                        " ",
                        "'Dancer in the Darkness'",
                        "Heaven Pierce Her",
                        "Ultrakill",
                        " ",
                        "'Surface Tension'",
                        "Chris Christodoulou",
                        "Risk of Rain Returns",
                        " ",
                        "'Cold Winds'",
                        "Heaven Pierce Her",
                        "Ultrakill",
                        " ",
                        "'Hemolysis'",
                        "Chris Christodoulou",
                        "Deadbolt",
                        " ",
                        "'Panic Betrayer'",
                        "Heaven Pierce Her",
                        "Ultrakill",
                        " ",
                        "'The Great Beyond'",
                        "Chris Christodoulou",
                        "Deadbolt",
                        " ",
                        "'Deep Blue'",
                        "Heaven Pierce Her",
                        "Ultrakill",
                        " ",
                        "'Arctic Oscillation'",
                        "Chris Christodoulou",
                        "Risk of Rain Returns",
                        " ",
                        "'Sands of Tide'",
                        "Heaven Pierce Her",
                        "Ultrakill",
                        " ",
                        "'Having Fallen, it was Blood'",
                        "Chris Christodoulou",
                        "RoR2: Survivors of the Void",
                        " ",
                        "'Danse Macabre'",
                        "Heaven Pierce Her",
                        "Ultrakill",
                        " ",
                        "'Hailstorm'",
                        "Chris Christodoulou",
                        "Risk of Rain Returns",
                        " ",
                        "'Glory'",
                        "Heaven Pierce Her",
                        "Ultrakill",
                        " ",
                        "'Double F***king Rainbow'",
                        "Chris Christodoulou",
                        "Risk of Rain Returns",
                        " ",
                        "'Tenebre Rosso Sangue'",
                        "KEYGEN CHURCH",
                        "Ultrakill",
                        " ",
                        "'Dies Irae'",
                        "Chris Christodoulou",
                        "Deadbolt",
                        " ",
                        "'Altars of Apostasy'",
                        "Heaven Pierce Her",
                        "Ultrakill",
                        " ",
                        "'Double F***king Rainbow Returns'",
                        "Chris Christodoulou",
                        "Risk of Rain Returns",
                        " ",
                        "'Parjanya'",
                        "Chris Christodoulou",
                        "Risk of Rain 2",
                        " ",
                        "'Coalescence'",
                        "Chris Christodoulou",
                        "Risk of Rain Returns",
                        " ",
                        "'Precipitation'",
                        "Chris Christodoulou",
                        "Risk of Rain Returns",
                        " ",
                        "'Steel Haze (Where Truths Meet)'",
                        "'Steel Haze (Rusted Pride)'",
                        "Shoi Miyazawa",
                        "Kota Hoshino",
                        "Armored Core VI",
                        " ",
                        "'Surface Tension Returns'",
                        "Damjan Mravunac",
                        "Risk of Rain Returns",
                        " ",
                        "'The Proverbial Dust Biters'",
                        "Chris Christodoulou",
                        "Deadbolt",
                        ];
                        break;
                    case 7:
                        headerString = Language.GetOrRegister("Mods.TerRoguelike.CreditTesting").Value;
                        headerColor = Color.MediumPurple;
                        strings = [
                        "AquaSG"
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
