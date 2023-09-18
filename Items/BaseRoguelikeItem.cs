using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerRoguelike.Managers;
using TerRoguelike.Systems;
using TerRoguelike.Player;
using Microsoft.Xna.Framework.Graphics;

namespace TerRoguelike.Items
{
    public abstract class BaseRoguelikeItem : ModItem, ILocalizedModType
    {
        public virtual bool CombatItem => false;
        public virtual bool HealingItem => false;
        public virtual bool UtilityItem => false;
        public virtual int itemTier => 0;
        public virtual void ItemEffects(Terraria.Player player)
        {

        }
        public override void UpdateInventory(Terraria.Player player)
        {
            ItemEffects(player);
        }
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f - 2f);
            spriteBatch.Draw(texture, Item.Center - Main.screenPosition, null, Color.White, rotation, origin, 1f, SpriteEffects.None, 0f);
            return false;
        }
    }
}
