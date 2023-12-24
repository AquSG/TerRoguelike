using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerRoguelike.Managers;
using TerRoguelike.Systems;
using TerRoguelike.TerPlayer;
using Microsoft.Xna.Framework.Graphics;

namespace TerRoguelike.Items
{
    public abstract class BaseRoguelikeItem : ModItem, ILocalizedModType
    {
        public virtual bool CombatItem => false;
        public virtual bool HealingItem => false;
        public virtual bool UtilityItem => false;
        public virtual float ItemDropWeight => 1f; //the item's weight in it's respective lists. this is normally used for items in multiple categories to give them the same overall drop chance as other items.
        public virtual int itemTier => 0; // item tiers : 0 - common, 1 - uncommon, 2 - rare
        public virtual int animationTicksPerFrame => 0; // used if the item sprite has animation
        public virtual int animationFrameCount => 0;// used if the item sprite has animation
        public virtual int modItemID => -1; // the ModContent.ItemType<> of this item, used for collecting it's default information on the fly
        public Texture2D texture;
        public override void SetStaticDefaults()
        {
            if (animationTicksPerFrame > 0 && animationFrameCount > 0)
            {
                Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(animationTicksPerFrame, animationFrameCount));
                ItemID.Sets.AnimatesAsSoul[Type] = true;
            }
        }
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            texture = ModContent.Request<Texture2D>(Texture).Value;
        }
        /// <summary>
        /// Item Effects happen here. updated in UpdateInventory(Player player)
        /// </summary>
        public virtual void ItemEffects(Player player)
        {

        }
        public override void UpdateInventory(Player player)
        {
            ItemEffects(player);
        }
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            // Generalized drawing for all TerRoguelike Items. they all have the same 20x20 hitbox but have different offsets to account for their size.
            if (animationTicksPerFrame > 0 && animationFrameCount > 0)
            {
                Rectangle drawRect = Main.itemAnimations[Type].GetFrame(texture);
                Vector2 origin = new Vector2(texture.Width / 2f, drawRect.Height * 0.5f - 2f);
                spriteBatch.Draw(texture, new Vector2(Item.Center.X, Item.Bottom.Y - (drawRect.Height * 0.5f)) - Main.screenPosition, drawRect, Color.White, rotation, origin, 1f, SpriteEffects.None, 0f);
            }
            else
            {
                Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f - 2f);
                spriteBatch.Draw(texture, new Vector2(Item.Center.X, Item.Bottom.Y - (texture.Size().Y * 0.5f)) - Main.screenPosition, null, Color.White, rotation, origin, 1f, SpriteEffects.None, 0f);
            }
            return false;
        }
    }
}
