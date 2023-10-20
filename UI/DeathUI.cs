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

namespace TerRoguelike.UI
{
    public static class DeathUI
    {
        private static Texture2D baseUITex, mainMenuButtonTex, mainMenuButtonHoverTex;
        private static Vector2 mainMenuButtonOffset = new Vector2(-200, 206);
        private static Vector2 restartButtonOffset = new Vector2(200, 206);
        public static List<Item> itemsToDraw;
        internal static void Load()
        {
            baseUITex = ModContent.Request<Texture2D>("TerRoguelike/UI/DeathUI", AssetRequestMode.ImmediateLoad).Value;
            mainMenuButtonTex = ModContent.Request<Texture2D>("TerRoguelike/UI/MenuButton", AssetRequestMode.ImmediateLoad).Value;
            mainMenuButtonHoverTex = ModContent.Request<Texture2D>("TerRoguelike/UI/MenuButtonHover", AssetRequestMode.ImmediateLoad).Value;
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

            bool mainMenuHover = mouseHitbox.Intersects(mainMenuBar);
            bool restartHover = mouseHitbox.Intersects(restartBar);

            MouseState ms = Mouse.GetState();
            if (ms.LeftButton == ButtonState.Pressed && mainMenuHover && modPlayer.deadTime > 150)
            {
                if (TerRoguelikeWorld.IsDeletableOnExit)
                {
                    TerRoguelikeMenu.wipeTempPlayer = true;
                    TerRoguelikeMenu.wipeTempWorld = true;
                }
                WorldGen.SaveAndQuit();
            }
            else if (ms.LeftButton == ButtonState.Pressed && restartHover && modPlayer.deadTime > 150)
            {
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
                }
                WorldGen.SaveAndQuit();
            }

            DrawDeathUI(spriteBatch, modPlayer, DeathUIScreenPos, player, mainMenuHover, restartHover);   
        }

        #region Draw Death UI
        private static void DrawDeathUI(SpriteBatch spriteBatch, TerRoguelikePlayer modPlayer, Vector2 screenPos, Player player, bool mainMenuHover, bool restartHover)
        {
            //if (!modPlayer.inWorld)
                //return;

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
                if (scale > 4f)
                    scale = 4f;
                spriteBatch.Draw(enemyTex, screenPos + new Vector2(240, -40), new Rectangle(0, frameHeight, enemyTex.Width, frameHeight), Color.White * opacity, 0f, new Vector2(enemyTex.Width * 0.5f, (frameHeight * 0.5f)), scale, SpriteEffects.None, 0);
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

            if (!itemsToDraw.Any())
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

            if (itemsToDraw.Any())
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
            
            Texture2D finalMainMenuButtonTex = mainMenuHover ? mainMenuButtonHoverTex : mainMenuButtonTex;
            Texture2D finalRestartButtonTex = restartHover ? mainMenuButtonHoverTex : mainMenuButtonTex;
            spriteBatch.Draw(finalMainMenuButtonTex, screenPos + mainMenuButtonOffset, null, Color.White * opacity, 0f, mainMenuButtonTex.Size() * 0.5f, 1f, SpriteEffects.None, 0);
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.DeathText.Value, "Main Menu", screenPos + (mainMenuHover ? new Vector2(-312, 182) : new Vector2(-300, 185)), (mainMenuHover ? Color.White : Color.LightGoldenrodYellow) * opacity, 0f, Vector2.Zero, new Vector2(mainMenuHover ? 1f : 0.9f));
            spriteBatch.Draw(finalRestartButtonTex, screenPos + restartButtonOffset, null, Color.White * opacity, 0f, mainMenuButtonTex.Size() * 0.5f, 1f, SpriteEffects.None, 0);
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.DeathText.Value, "Quick Restart", screenPos + (restartHover ? new Vector2(78 - 12, 182) : new Vector2(78, 185)), (restartHover ? Color.White : Color.LightGoldenrodYellow) * opacity, 0f, Vector2.Zero, new Vector2(restartHover ? 1f : 0.9f));
        }
        #endregion
    }
}
