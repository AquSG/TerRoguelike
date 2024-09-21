using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TerRoguelike.TerPlayer;
using Terraria.GameContent;
using TerRoguelike.Managers;
using TerRoguelike.Particles;

namespace TerRoguelike.Items
{
    public class LunarGambit : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.rare = ItemRarityID.Cyan;
            Item.maxStack = Item.CommonMaxStack;
        }
        public override void UpdateInventory(Player player)
        {
            player.GetModPlayer<TerRoguelikePlayer>().lunarGambit += Item.stack;
        }
        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            DrawLunarGambit(position, scale);
            return false;
        }
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            DrawLunarGambit(Item.Center - Main.screenPosition + Vector2.UnitY * (float)Math.Sin(Item.timeSinceItemSpawned / 60f * MathHelper.PiOver2) * 3, scale, rotation);
            return false;
        }
        public void DrawLunarGambit(Vector2 position, float scale, float rotation = 0)
        {
            var tex = TextureAssets.Item[ModContent.ItemType<LunarGambit>()].Value;
            Vector2 origin = tex.Size() * 0.5f;
            Color glowColor = Color.Cyan * ((float)Math.Cos(Main.GlobalTimeWrappedHourly * MathHelper.PiOver4) * 0.1f + 0.4f);
            glowColor.A = 25;

            float rotOff = Main.GlobalTimeWrappedHourly * MathHelper.PiOver4;
            for (int i = 0; i < 6; i++)
            {
                float completion = i / 6f * MathHelper.TwoPi;
                float thisRot = completion + rotOff + rotation;
                float magnitude = (float)Math.Cos(completion + rotOff * 1.5f + i * 2.122456) * 0.28f + 0.64f;
                Main.EntitySpriteDraw(tex, position + thisRot.ToRotationVector2() * 8 * magnitude * scale, null, glowColor, rotation, origin, scale, SpriteEffects.None);
            }

            Main.EntitySpriteDraw(tex, position, null, Color.White * 0.9f, rotation, origin, scale, SpriteEffects.None);
        }
        public override void Update(ref float gravity, ref float maxFallSpeed)
        {
            gravity = 0.045f;
            maxFallSpeed = 0;
            Vector2 itemPos = Item.Center + Vector2.UnitY * (float)Math.Sin(Item.timeSinceItemSpawned / 60f * MathHelper.PiOver2) * 3;
            if (Main.rand.NextBool(5))
            {
                ParticleManager.AddParticle(new Ball(
                    itemPos, 
                    Main.rand.NextVector2CircularEdge(3, 3) * Main.rand.NextFloat(0.5f, 1f) + Item.velocity,
                    30, Color.Lerp(Color.White, Color.Cyan, Main.rand.NextFloat(0.3f, 1f)) * 1,
                    new Vector2(Main.rand.NextFloat(0.5f, 1f) * 0.12f), 0, 0.96f, 30, true, false), ParticleManager.ParticleLayer.Default);
            }
            
        }
    }
}
