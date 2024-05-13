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
using TerRoguelike.World;
using Terraria.GameContent;
using Terraria.UI.Chat;
using static TerRoguelike.Managers.TextureManager;
using Terraria.Localization;
using TerRoguelike.Utilities;
using Microsoft.Xna.Framework.Audio;
using Terraria.Audio;
using Terraria.ID;
using TerRoguelike.Managers;
using Terraria.GameInput;
using static TerRoguelike.Managers.ItemManager;
using System.Linq;
using TerRoguelike.Tiles;

namespace TerRoguelike.UI
{
    public static class ItemBasinUI
    {
        public static bool Active;
        public static Texture2D buttonTex, diceTex;
        internal static void Load()
        {
            buttonTex = TexDict["Square"];
            diceTex = TexDict["Random"];
        }

        internal static void Unload()
        {
            buttonTex = null;
        }

        public static void Draw()
        {
            Player player = Main.LocalPlayer;
            if (player == null || !player.active)
            {
                Close();
                return;
            }
            var modPlayer = player.ModPlayer();
            if (modPlayer == null || modPlayer.selectedBasin == null)
            {
                Close();
                return;
            }

            Open();

            var basin = modPlayer.selectedBasin;
            if (Main.tile[basin.position.X, basin.position.Y].TileType != ModContent.TileType<ItemBasin>())
            {
                Close();
                modPlayer.selectedBasin = null;
                return;
            }
            Vector2 anchorPos = basin.position.ToWorldCoordinates(24, -64);

            List<Item> invList = [];
            for (int invItem = 0; invItem < 51; invItem++)
            {
                var item = invItem switch
                {
                    50 => player.trashItem,
                    _ => player.inventory[invItem],
                };

                if (!item.active || item.stack <= 0 || item.type == 0)
                    continue;

                invList.Add(item);
            }
            for (int i = 0; i < basin.itemOptions.Count; i++)
            {
                if (!invList.Any(x => x.type == basin.itemOptions[i]))
                {
                    basin.itemOptions.RemoveAt(i);
                    i--;
                }
            }

            var basinItemList = basin.itemOptions;
            int effectiveCount = basinItemList.Count + 1;

            Vector2 buttonDimensions = new Vector2(48);
            Vector2 buttonDimensionsInflate = new Vector2(52);
            int maxButtonsWidth = 10;
            int horizButtonDisplay = (int)MathHelper.Clamp(effectiveCount, 1, maxButtonsWidth);
            int verticalButtonDisplay = (effectiveCount / maxButtonsWidth) + 1;

            bool mouseInteract = PlayerInput.Triggers.JustPressed.MouseLeft && player.inventory[58].type == 0;
            Vector2 drawStartPos = anchorPos + new Vector2(buttonDimensionsInflate.X * horizButtonDisplay * -0.5f, buttonDimensionsInflate.Y * -verticalButtonDisplay);
            Vector2 buttonScale = new Vector2(0.25f) * buttonDimensions;
            Vector2 buttonHighlightScale = new Vector2(0.25f) * buttonDimensionsInflate;
            int currentHighlight = -1;
            bool queueShrinkClose = false;
            bool queueClose = false;
            int priorityRemove = -1;

            for (int i = 0; i < effectiveCount; i++)
            {
                int itemType = i < effectiveCount - 1 ? basinItemList[i] : -1;

                int horizMulti = i % 10;
                int vertiMulti = i / 10;
                Point drawPos = (drawStartPos + (buttonDimensionsInflate * new Vector2(horizMulti, vertiMulti))).ToPoint();
                Rectangle interactRect = new Rectangle(drawPos.X, drawPos.Y, (int)buttonDimensionsInflate.X, (int)buttonDimensionsInflate.Y);
                if (interactRect.Contains(Main.MouseWorld.ToPoint()))
                {
                    Main.blockMouse = true;
                    currentHighlight = i;
                    Main.EntitySpriteDraw(buttonTex, (drawPos.ToVector2() - Main.screenPosition).ToPoint().ToVector2(), null, Color.Yellow, 0, Vector2.Zero, buttonHighlightScale, SpriteEffects.None);
                    if (mouseInteract)
                    {
                        int pulledItem = itemType;
                        if (pulledItem < 0)
                        {
                            bool found = false;
                            for (int T = 0; T < 300; T++) // try find an item to scoop up
                            {
                                int randInt = Main.rand.Next(51);
                                var potItem = randInt switch
                                {
                                    50 => player.trashItem,
                                    _ => player.inventory[randInt],
                                };

                                if (!potItem.active)
                                    continue;
                                int playerItemType = potItem.type;
                                if (playerItemType == 0 || playerItemType == basin.itemDisplay)
                                    continue;
                                
                                int rogueItemType = AllItems.FindIndex(x => x.modItemID == playerItemType);
                                if (rogueItemType != -1 && (ItemTier)AllItems[rogueItemType].itemTier == basin.tier)
                                {
                                    found = true;
                                    pulledItem = playerItemType;
                                    break;
                                }
                            }
                            if (!found) //somehow didn't randomly find an item to take. search manually through the player's items instead.
                            {
                                for (int invItem = 0; invItem < 51; invItem++)
                                {
                                    var potItem = invItem switch
                                    {
                                        50 => player.trashItem,
                                        _ => player.inventory[invItem],
                                    };

                                    if (!potItem.active)
                                        continue;

                                    int playerItemType = potItem.type;
                                    if (playerItemType == 0 || playerItemType == basin.itemDisplay)
                                        continue;

                                    int rogueItemType = AllItems.FindIndex(x => x.modItemID == playerItemType);
                                    if (rogueItemType != -1 && (ItemTier)AllItems[rogueItemType].itemTier == basin.tier)
                                    {
                                        pulledItem = playerItemType;
                                        found = true;
                                        break;
                                    }
                                }
                            }
                            if (!found)
                            {
                                queueClose = true;
                            }
                        }
                        for (int invItem = 0; invItem < 51; invItem++)
                        {
                            var potItem = invItem switch
                            {
                                50 => player.trashItem,
                                _ => player.inventory[invItem],
                            };

                            if (!potItem.active)
                                continue;
                            if (potItem.type == pulledItem)
                            {
                                if (potItem.stack == 1)
                                {
                                    priorityRemove = potItem.type;
                                }
                                potItem.stack--;
                                if (invItem == 50)
                                {
                                    player.inventory[player.selectedItem] = potItem;
                                }
                                int direction = player.Center.X > anchorPos.X ? 1 : -1;
                                SpawnManager.specialPendingItems.Add(new PendingItem(basin.itemDisplay, basin.position.ToWorldCoordinates(24, 0), basin.tier, 120, new Vector2(1.5f * direction, -2), 0.1f));
                                
                                SoundEngine.PlaySound(SoundID.MenuTick);
                                queueShrinkClose = true;
                                break;
                            }
                        }
                    }
                }
                drawPos += new Point(2, 2);
                Main.EntitySpriteDraw(buttonTex, (drawPos.ToVector2() - Main.screenPosition).ToPoint().ToVector2(), null, Color.DarkSlateBlue, 0, Vector2.Zero, buttonScale, SpriteEffects.None);
                if (itemType >= 0)
                {
                    DrawItem(drawPos.ToVector2() + buttonDimensions * 0.5f, itemType);
                }
                else
                {
                    Main.EntitySpriteDraw(diceTex, (drawPos.ToVector2() + buttonDimensions * 0.5f - Main.screenPosition).ToPoint().ToVector2(), null, Color.White, 0, diceTex.Size() * 0.5f, 1f, SpriteEffects.None);
                }
            }
            if (queueShrinkClose)
            {
                basin.ShrinkItemOptions(priorityRemove);
                Close();
                modPlayer.selectedBasin = null;
                return;
            }
            if (queueClose)
            {
                Close();
                modPlayer.selectedBasin = null;
                return;
            }
        }
        public static void Close()
        {
            if (!Active)
                return;

            SoundEngine.PlaySound(SoundID.MenuClose);
            Active = false;
        }
        public static void Open()
        {
            if (Active)
                return;

            Active = true;
        }
        public static void DrawItem(Vector2 position, int itemType)
        {
            Vector2 itemDisplayDimensions = new Vector2(36);
            Item item = new Item(itemType);
            Texture2D itemTex;
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

            Main.EntitySpriteDraw(itemTex, (position - Main.screenPosition).ToPoint().ToVector2(), rect, Color.White, 0f, rect.Size() * 0.5f, scale, SpriteEffects.None, 0);
        }
    }
}
