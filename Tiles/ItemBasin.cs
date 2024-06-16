using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.ObjectData;
using static TerRoguelike.Managers.ItemManager;
using static TerRoguelike.Managers.TextureManager;
using Microsoft.Xna.Framework.Graphics;
using TerRoguelike.World;
using Terraria.GameContent;
using TerRoguelike.Managers;
using Terraria.Localization;
using TerRoguelike.Projectiles;
using ReLogic.Content;
using Microsoft.Build.Tasks;
using Terraria.GameContent.ObjectInteractions;
using Terraria.Audio;
using TerRoguelike.UI;
using static TerRoguelike.World.TerRoguelikeWorld;
using TerRoguelike.Utilities;

namespace TerRoguelike.Tiles
{
    public class ItemBasin : ModTile
    {
        int currentFrame = 0;

        public static Texture2D glowTex = null;
        public static Texture2D highlightTex = null;
        public override void SetStaticDefaults()
        {
            Main.tileLighted[Type] = true;
            Main.tileFrameImportant[Type] = true;
            Main.tileLavaDeath[Type] = false;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.addTile(Type);
            AnimationFrameHeight = 38;
            AddMapEntry(new Color(100, 100, 100), Language.GetOrRegister("Mods.TerRoguelike.ItemBasin.DisplayName"));
        }
        public override void PostSetDefaults()
        {
            
        }
        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;
        public override bool RightClick(int i, int j)
        {
            var modPlayer = Main.LocalPlayer.ModPlayer();
            if (modPlayer == null)
                return false;

            for (int b = 0; b < itemBasins.Count; b++)
            {
                var basin = itemBasins[b];

                if (basin.nearby > 0)
                {
                    if (basin.rect.Contains(new Point(i, j)))
                    {
                        if (modPlayer.selectedBasin == basin)
                        {
                            modPlayer.selectedBasin = null;
                        }
                        else
                        {
                            modPlayer.selectedBasin = basin;
                            ItemBasinUI.gamepadSelectedOption = 0;
                            SoundEngine.PlaySound(SoundID.MenuOpen);
                        }
                    }
                }
            }

            return true;
        }
        public override void NearbyEffects(int i, int j, bool closer)
        {
            Point tilePos = new Point(i, j);
            for (int b = 0; b < TerRoguelikeWorld.itemBasins.Count; b++)
            {
                var basin = TerRoguelikeWorld.itemBasins[b];
                if (basin.rect.Contains(tilePos))
                {
                    basin.nearby = 60;
                    
                    if (basin.itemDisplay == 0)
                    {
                        basin.itemDisplay = ChooseItemUnbiased((int)basin.tier);
                    }
                    break;
                }
            }
        }
        public override void AnimateTile(ref int frame, ref int frameCounter)
        {
            frameCounter++;
            if (frameCounter >= 8)
            {
                frame = (frame + 1) % 4;
                currentFrame = frame;
                frameCounter = 0;
            }
        }
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            glowTex ??= TexDict["ItemBasinGlow"];
            highlightTex ??= TexDict["ItemBasin_Highlight"];

            Point tilePos = new Point(i, j);
            ItemTier tier = new();

            for (int b = 0; b < TerRoguelikeWorld.itemBasins.Count; b++)
            {
                var basin = TerRoguelikeWorld.itemBasins[b];
                if (basin.rect.Contains(tilePos))
                {
                    tier = basin.tier;
                    if (basin.nearby > 0 && tilePos == basin.position)
                    {
                        DrawItemDisplay(basin);
                    }
                    break;
                }
            }

            Tile tile = Main.tile[i, j];
            int xPos = tile.TileFrameX;
            int yPos = tile.TileFrameY;
            var color = tier switch
            {
                ItemTier.Uncommon => new Color(0.4f, 1f, 0.4f),
                ItemTier.Rare => new Color(1f, 0.4f, 0.4f),
                _ => new Color(0.2f, 0.55f, 1f),
            };
            color *= 0.8f;
            color.A = 100;

            Vector2 offset = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
            Vector2 drawPos = new Vector2(i, j) * 16 - Main.screenPosition + offset;

            Rectangle? thisTileFrame = new Rectangle?(new Rectangle(xPos, yPos + currentFrame * AnimationFrameHeight, 16, 16));
            Main.spriteBatch.Draw(glowTex, drawPos, thisTileFrame, color, 0, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);

            int selectionTier = 0;
            if (Main.InSmartCursorHighlightArea(i, j, out var actuallySelected))
            {
                selectionTier = actuallySelected ? 2 : 1;
            }
            if (selectionTier > 0)
            {
                Color baseTileColor = Lighting.GetColor(i, j);
                int averageBrightness = (baseTileColor.R + baseTileColor.G + baseTileColor.B) / 3;
                Color selectionColor = Colors.GetSelectionGlowColor(selectionTier == 2, averageBrightness);

                Main.spriteBatch.Draw(highlightTex, drawPos, thisTileFrame, selectionColor, 0, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
            }
        }
        public override void PlaceInWorld(int i, int j, Item item)
        {
            TerRoguelikeWorld.itemBasins.Add(new ItemBasinEntity(new Point(i - 1, j - 1), (ItemTier)(TerRoguelikeWorld.itemBasins.Count % 3)));
        }

        public void DrawItemDisplay(ItemBasinEntity basin)
        {
            if (basin.itemDisplay == 0)
                return;

            Vector2 drawPos = (basin.position.ToVector2() + new Vector2(1, 0)).ToWorldCoordinates(8, 0) + new Vector2(0, -32);
            float period = (Main.GlobalTimeWrappedHourly + basin.position.X + basin.position.Y);
            drawPos.Y += (float)Math.Cos(period * 0.5f) * 4;

            Vector2 itemDisplayDimensions = new Vector2(48, 48);
            Item item = new Item(basin.itemDisplay);
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

            float opacity = 0.7f + (float)Math.Cos(period * 2) * 0.15f;
            Color color = Color.White * opacity;
            Main.EntitySpriteDraw(itemTex, drawPos - Main.screenPosition + new Vector2(Main.offScreenRange), rect, color, 0f, rect.Size() * 0.5f, scale, SpriteEffects.None, 0);
        }
    }
}
