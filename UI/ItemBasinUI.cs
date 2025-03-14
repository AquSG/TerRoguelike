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
using Terraria.Graphics;
using TerRoguelike.Systems;
using System.Diagnostics;
using TerRoguelike.Packets;

namespace TerRoguelike.UI
{
    public static class ItemBasinUI
    {
        public static bool Active;
        public static int gamepadSelectedOption = 0;
        public static Texture2D buttonTex, buttonHoverTex, buttonBackgroundTex, diceTex, noCircleTex;
        public static int stickMoveCooldown = 0;
        internal static void Load()
        {
            buttonTex = TexDict["BasinOptionBox"];
            buttonHoverTex = TexDict["BasinOptionBoxHover"];
            buttonBackgroundTex = TexDict["BasinOptionsBackground"];
            diceTex = TexDict["Random"];
            noCircleTex = TexDict["NoCircle"];
        }

        internal static void Unload()
        {
            buttonTex = buttonHoverTex = buttonBackgroundTex = diceTex = noCircleTex = null;
        }

        public static void Draw()
        {
            Player player = Main.LocalPlayer;
            if (player == null || !player.active || player.dead)
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

            List<Item> allowedItemList = [];
            for (int invItem = 0; invItem < 52; invItem++)
            {
                var item = invItem switch
                {
                    51 => player.HeldItem,
                    50 => player.trashItem,
                    _ => player.inventory[invItem],
                };

                if (!item.active || item.stack <= 0 || item.type == 0 || item.type == basin.itemDisplay)
                    continue;

                var modItem = item.ModItem();
                if (modItem == null || modItem.RogueItemTier != (int)basin.tier || allowedItemList.Any(x => x.type == item.type))
                    continue;

                allowedItemList.Add(item);
            }
            for (int i = 0; i < basin.itemOptions.Count; i++)
            {
                if (!allowedItemList.Any(x => x.type == basin.itemOptions[i]))
                {
                    basin.itemOptions.RemoveAt(i);
                    i--;
                }
            }
            bool noCircle = allowedItemList.Count == 0;

            bool gamepad = PlayerInput.UsingGamepad;

            var basinItemList = basin.itemOptions;
            int effectiveCount = basinItemList.Count + 1;

            Vector2 buttonDimensions = new Vector2(48);
            Vector2 buttonDimensionsInflate = new Vector2(52);
            int maxButtonsWidth = 10;
            int horizButtonDisplay = (int)MathHelper.Clamp(effectiveCount, 1, maxButtonsWidth);
            int verticalButtonDisplay = ((effectiveCount - 1) / maxButtonsWidth);
            if (verticalButtonDisplay < 0)
                verticalButtonDisplay = 0;
            verticalButtonDisplay++;


            bool mouseInteract = (PlayerInput.Triggers.JustPressed.MouseLeft || (PlayerInput.UsingGamepad && PlayerInput.Triggers.JustPressed.QuickMount)) && player.inventory[58].type == 0;
            Vector2 drawStartPos = anchorPos + new Vector2(buttonDimensionsInflate.X * horizButtonDisplay * -0.5f, buttonDimensionsInflate.Y * -verticalButtonDisplay);
            int currentHighlight = -1;
            bool queueShrinkClose = false;
            bool queueClose = false;
            int priorityRemove = -1;

            // draw the backdrop to the buttons
            Rectangle backgroundDrawRect = new Rectangle((int)drawStartPos.X, (int)drawStartPos.Y, (int)(horizButtonDisplay * buttonDimensionsInflate.X), (int)(verticalButtonDisplay * buttonDimensionsInflate.Y));
            int inflateAmt = 6;
            int edgeWidth = 12; // do not touch unless the texture is changed.
            backgroundDrawRect.Inflate(inflateAmt, inflateAmt);
            Rectangle cornerFrame = new Rectangle(0, 0, edgeWidth, edgeWidth);
            Rectangle edgeFrame = new Rectangle(edgeWidth, 0, 2, edgeWidth);
            Rectangle fillFrame = new Rectangle(edgeWidth, edgeWidth, 1, 1);

            Vector2 drawStart = new Vector2(backgroundDrawRect.X, backgroundDrawRect.Y);
            Vector2 fillDrawPos = drawStart + new Vector2(edgeWidth);
            Vector2 fillScale = new Vector2(backgroundDrawRect.Width - edgeWidth * 2, backgroundDrawRect.Height - edgeWidth * 2);
            Color backgroundColor = Color.Lerp(Color.DarkSlateBlue, Color.Blue, 0.6f);
            Main.EntitySpriteDraw(buttonBackgroundTex, (fillDrawPos - Main.screenPosition).ToPoint().ToVector2(), fillFrame, backgroundColor, 0, Vector2.Zero, fillScale, SpriteEffects.None);

            for (int i = 0; i < 4; i++)
            {
                Vector2 cornerDrawStart = drawStart;
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

                Main.EntitySpriteDraw(buttonBackgroundTex, (sideDrawStart - Main.screenPosition).ToPoint().ToVector2(), edgeFrame, backgroundColor, rot, Vector2.Zero, edgeScale, SpriteEffects.None);
                Main.EntitySpriteDraw(buttonBackgroundTex, (cornerDrawStart - Main.screenPosition).ToPoint().ToVector2(), cornerFrame, backgroundColor, rot, Vector2.Zero, 1f, SpriteEffects.None);
            }

            for (int i = 0; i < effectiveCount; i++)
            {
                int itemType = i < effectiveCount - 1 ? basinItemList[i] : -1;

                int horizMulti = i % 10;
                int vertiMulti = i / 10;
                Point drawPos = (drawStartPos + (buttonDimensionsInflate * new Vector2(horizMulti, vertiMulti))).ToPoint();
                Rectangle interactRect = new Rectangle(drawPos.X, drawPos.Y, (int)buttonDimensionsInflate.X, (int)buttonDimensionsInflate.Y);
                if (!gamepad ? interactRect.Contains(Main.MouseWorld.ToPoint()) : i == gamepadSelectedOption)
                {
                    Main.blockMouse = true;
                    currentHighlight = i;
                    Color highlightColor = Color.Lerp(Color.Yellow, Color.White, 0.4f);
                    Main.EntitySpriteDraw(buttonHoverTex, (drawPos.ToVector2() - Main.screenPosition).ToPoint().ToVector2(), null, highlightColor, 0, Vector2.Zero, 1f, SpriteEffects.None);
                    if (mouseInteract)
                    {
                        if (noCircle)
                        {
                            queueClose = true;
                            break;
                        }
                        int pulledItem = itemType;
                        if (pulledItem < 0)
                        {
                            pulledItem = allowedItemList[Main.rand.Next(allowedItemList.Count)].type;
                        }
                        for (int invItem = 0; invItem < 51; invItem++)
                        {
                            var potItem = invItem switch
                            {
                                50 => player.trashItem,
                                _ => player.inventory[invItem],
                            };

                            if (!potItem.active || potItem.stack <= 0)
                                continue;
                            if (potItem.type == pulledItem)
                            {
                                if (potItem.stack == 1)
                                {
                                    priorityRemove = potItem.type;
                                }
                                potItem.stack--;

                                int direction = player.Center.X > anchorPos.X ? 1 : -1;
                                var itemSend = new PendingItem(basin.itemDisplay, basin.position.ToWorldCoordinates(24, 0), basin.tier, 75, new Vector2(1.5f * direction * Main.rand.NextFloat(0.75f, 1.06f), -2), 0.1f, player.Top, pulledItem);
                                SpawnManager.specialPendingItems.Add(itemSend);
                                SpecialPendingItemPacket.Send(itemSend);
                                
                                SoundEngine.PlaySound(SoundID.MenuTick);
                                queueShrinkClose = true;
                                break;
                            }
                        }
                    }
                }
                Main.EntitySpriteDraw(buttonTex, (drawPos.ToVector2() - Main.screenPosition).ToPoint().ToVector2(), null, Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None);
                if (itemType >= 0)
                {
                    DrawItem(drawPos.ToVector2() + buttonDimensionsInflate * 0.5f, itemType);
                }
                else
                {
                    Main.EntitySpriteDraw(noCircle ? noCircleTex : diceTex, (drawPos.ToVector2() + buttonDimensionsInflate * 0.5f - Main.screenPosition).ToPoint().ToVector2(), null, Color.White, 0, noCircle ? noCircleTex.Size() * 0.5f : diceTex.Size() * 0.5f, 1f, SpriteEffects.None);
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
            if (currentHighlight >= 0)
            {
                if (gamepad)
                {
                    var triggers = PlayerInput.Triggers.JustPressed;
                    bool pass = true;
                    int horizPos = currentHighlight % horizButtonDisplay;
                    int vertiPos = currentHighlight / 10;
                    int lastRowLength = effectiveCount % horizButtonDisplay;
                    bool onLastRow = (currentHighlight / horizButtonDisplay) == (effectiveCount / horizButtonDisplay);
                    int checkLength = onLastRow ? lastRowLength : horizButtonDisplay;

                    Vector2 rightStick = PlayerInput.GamepadThumbstickRight;
                    int stickdir = -1;
                    if (stickMoveCooldown <= 1 && rightStick.Length() > 0.12f)
                    {
                        stickMoveCooldown = stickMoveCooldown == 0 ? 15 : (int)(6 / rightStick.Length());
                        float stickRot = rightStick.ToRotation();
                        if (Math.Abs(stickRot) < MathHelper.PiOver4)
                        {
                            stickdir = 2;
                        }
                        else if (stickRot > 0 && stickRot < MathHelper.PiOver4 * 3)
                        {
                            stickdir = 3;
                        }
                        else if (stickRot < 0 && stickRot > MathHelper.PiOver4 * -3)
                        {
                            stickdir = 1;
                        }
                        else
                        {
                            stickdir = 4;
                        }
                    }
                    if (triggers.DpadMouseSnap4 || triggers.DpadRadial4 || stickdir == 4)
                    {
                        if (currentHighlight % checkLength == 0)
                        {
                            currentHighlight += checkLength;
                        }
                        currentHighlight--;
                    }
                    else if (triggers.DpadMouseSnap2 || triggers.DpadRadial2 || stickdir == 2)
                    {
                        if (currentHighlight % checkLength == checkLength - 1)
                        {
                            currentHighlight -= checkLength;
                        }
                        currentHighlight++;
                    }
                    else if (triggers.DpadMouseSnap1 || triggers.DpadRadial1 || stickdir == 1)
                    {
                        currentHighlight -= 10;
                        if (currentHighlight < 0)
                        {
                            currentHighlight += 10 * verticalButtonDisplay;
                            if (currentHighlight >= effectiveCount)
                                currentHighlight -= 10;
                        }
                    }
                    else if (triggers.DpadMouseSnap3 || triggers.DpadRadial3 || stickdir == 3)
                    {
                        currentHighlight += 10;
                        if (currentHighlight >= effectiveCount)
                        {
                            currentHighlight -= 10 * verticalButtonDisplay;
                            if (currentHighlight < 0)
                                currentHighlight += 10;
                        }
                    }
                    else
                        pass = false;
                    if (pass)
                    {
                        gamepadSelectedOption = currentHighlight;
                        
                    }
                }
            }
        }
        public static void Close()
        {
            if (!Active)
                return;

            gamepadSelectedOption = 0;
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
