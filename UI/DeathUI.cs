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

namespace TerRoguelike.UI
{
    public static class DeathUI
    {
        private static Texture2D baseUITex, mainMenuButtonTex, mainMenuButtonHoverTex, questionMarkTex, moonTex;
        private static Vector2 mainMenuButtonOffset = new Vector2(-200, 206);
        private static Vector2 restartButtonOffset = new Vector2(200, 206);
        public static List<Item> itemsToDraw;
        public static bool mainMenuHover = false;
        public static bool restartHover = false;
        internal static void Load()
        {
            baseUITex = TexDict["DeathUI"];
            mainMenuButtonTex = TexDict["MenuButton"];
            mainMenuButtonHoverTex = TexDict["MenuButtonHover"];
            questionMarkTex = TexDict["QuestionMark"];
            moonTex = TexDict["UiMoon"];
            itemsToDraw = new List<Item>();
            Reset();
        }

        internal static void Unload()
        {
            Reset();
            baseUITex = mainMenuButtonTex = mainMenuButtonHoverTex = questionMarkTex = moonTex = null;
            itemsToDraw = null;
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

            DrawDeathUI(spriteBatch, modPlayer, DeathUIScreenPos, player, mainMenuHover, restartHover);

            bool pressed = PlayerInput.UsingGamepad ? gs.IsButtonDown(Buttons.A) : ms.LeftButton == ButtonState.Pressed;
            if (pressed && mainMenuHover && modPlayer.deadTime > 150)
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
                modPlayer.deadTime = 0;
            }
            else if (pressed && restartHover && modPlayer.deadTime > 150)
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
                    player.trashItem = new(ItemID.None, 0);
                    TerRoguelikeMenu.desiredPlayer = Main.ActivePlayerFileData;
                    TerRoguelikeMenu.wipeTempWorld = true;
                    TerRoguelikeMenu.prepareForRoguelikeGeneration = true;
                }
                modPlayer.killerNPC = -1;
                modPlayer.killerProj = -1;
                WorldGen.SaveAndQuit();
                modPlayer.deadTime = 0;
            }
        }

        #region Draw Death UI
        private static void DrawDeathUI(SpriteBatch spriteBatch, TerRoguelikePlayer modPlayer, Vector2 screenPos, Player player, bool mainMenuHover, bool restartHover)
        {
            float opacity = MathHelper.Clamp(MathHelper.Lerp(0, 1f, (modPlayer.deadTime - 120) / 60f), 0, 1f);
            spriteBatch.Draw(baseUITex, screenPos, null, Color.White * 0.85f * opacity, 0f, baseUITex.Size() * 0.5f, 1f, SpriteEffects.None, 0);
            string deathKilledBy = Language.GetOrRegister("Mods.TerRoguelike.DeathKilledBy").Value;
            string deathItems = Language.GetOrRegister("Mods.TerRoguelike.DeathItems").Value;
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.DeathText.Value, deathKilledBy, screenPos + new Vector2(130, -250), Color.MediumPurple * opacity, 0f, Vector2.Zero, new Vector2(0.9f));
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.DeathText.Value, deathItems, screenPos + new Vector2(-360, -250), Color.White * opacity, 0f, Vector2.Zero, new Vector2(0.9f));
            if (modPlayer.killerNPC != -1)
            {
                Texture2D enemyTex = TextureAssets.Npc[modPlayer.killerNPCType].Value;
                int frameHeight = enemyTex.Height / Main.npcFrameCount[modPlayer.killerNPCType];
                NPC dummyNPC = new NPC();
                dummyNPC.type = modPlayer.killerNPCType;
                dummyNPC.SetDefaults(dummyNPC.type);
                dummyNPC.active = false; // basically you can add a check for active in findframe if you want it to be a super specific frame instead of just the default from no upating of the npc
                dummyNPC.FindFrame();
                Rectangle frame = dummyNPC.frame;
                //Rectangle frame = new Rectangle(0, 0, enemyTex.Width, frameHeight);
                float horizontalScale = 180f / (float)frame.Width;
                float verticalScale = 250f / (float)frame.Height;
                float scale;
                if (horizontalScale > verticalScale)
                    scale = verticalScale;
                else
                    scale = horizontalScale;
                if (scale > 4f)
                    scale = 4f;
                spriteBatch.Draw(enemyTex, screenPos + new Vector2(240, -40), frame, Color.White * opacity, 0f, frame.Size() * 0.5f, scale, SpriteEffects.None, 0);
            }
            else if (modPlayer.killerProj != -1)
            {
                Texture2D projTex = TextureAssets.Projectile[modPlayer.killerProjType].Value;
                int frameHeight = projTex.Height / Main.projFrames[modPlayer.killerProjType];
                float horizontalScale = 180f / (float)projTex.Width;
                float verticalScale = 250f / (float)frameHeight;
                float scale;
                if (horizontalScale > verticalScale)
                    scale = verticalScale;
                else
                    scale = horizontalScale;
                if (scale > 4f)
                    scale = 4f;
                spriteBatch.Draw(projTex, screenPos + new Vector2(240, -40), new Rectangle(0, 0, projTex.Width, frameHeight), Color.White * opacity, 0f, new Vector2(projTex.Width * 0.5f, (frameHeight * 0.5f)), scale, SpriteEffects.None, 0);
            }
            else
            {
                Main.EntitySpriteDraw(questionMarkTex, screenPos + new Vector2(240, -40), null, Color.White * opacity, 0, questionMarkTex.Size() * 0.5f, 1f, SpriteEffects.None);
            }

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

            var deathFont = FontAssets.DeathText.Value;
            string difficultyString = Language.GetOrRegister("Mods.TerRoguelike.MenuDifficulty").Value;
            Vector2 difficultyStringDimensions = deathFont.MeasureString(difficultyString);
            Vector2 difficultyStringDrawPos = screenPos + new Vector2(-360, 100);
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, deathFont, difficultyString, difficultyStringDrawPos, Color.Tomato * opacity, 0f, Vector2.Zero, new Vector2(0.6f));

            int moonFrameHeight = moonTex.Height / 3;
            Rectangle moonFrame = new Rectangle(0, moonFrameHeight * (int)TerRoguelikeMenu.difficulty, moonTex.Width, moonFrameHeight - 2);
            Main.EntitySpriteDraw(moonTex, difficultyStringDrawPos + new Vector2(12 + difficultyStringDimensions.X * 0.6f, -12), moonFrame, Color.White * opacity, 0, Vector2.Zero, 1f, SpriteEffects.None);

            string deathMainMenu = Language.GetOrRegister("Mods.TerRoguelike.DeathMainMenu").Value;
            string deathQuickRestart = Language.GetOrRegister("Mods.TerRoguelike.DeathQuickRestart").Value;
            Texture2D finalMainMenuButtonTex = mainMenuHover ? mainMenuButtonHoverTex : mainMenuButtonTex;
            Texture2D finalRestartButtonTex = restartHover ? mainMenuButtonHoverTex : mainMenuButtonTex;
            spriteBatch.Draw(finalMainMenuButtonTex, screenPos + mainMenuButtonOffset, null, Color.White * opacity, 0f, mainMenuButtonTex.Size() * 0.5f, 1f, SpriteEffects.None, 0);
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, deathFont, deathMainMenu, screenPos + mainMenuButtonOffset, (mainMenuHover ? Color.White : Color.LightGoldenrodYellow) * opacity, 0f, mainMenuButtonTex.Size() * new Vector2(0.4f, 0.3f), new Vector2(mainMenuHover ? 1f : 0.9f));
            spriteBatch.Draw(finalRestartButtonTex, screenPos + restartButtonOffset, null, Color.White * opacity, 0f, mainMenuButtonTex.Size() * 0.5f, 1f, SpriteEffects.None, 0);
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, deathFont, deathQuickRestart, screenPos + restartButtonOffset, (restartHover ? Color.White : Color.LightGoldenrodYellow) * opacity, 0f, mainMenuButtonTex.Size() * new Vector2(0.48f, 0.3f), new Vector2(restartHover ? 1f : 0.9f));
        }
        #endregion
    }
}
