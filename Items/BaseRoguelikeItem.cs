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
        public virtual int itemTier => 0;
        public virtual int animationTicksPerFrame => 0;
        public virtual int animationFrameCount => 0;
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
        }
        public virtual void ItemEffects(Player player)
        {

        }
        public override void UpdateInventory(Player player)
        {
            ItemEffects(player);
        }
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            if (animationTicksPerFrame > 0 && animationFrameCount > 0)
            {
                Rectangle drawRect = Main.itemAnimations[Item.type].GetFrame(texture);
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
