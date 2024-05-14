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
using static TerRoguelike.Systems.EnemyHealthBarSystem;
using ReLogic.Graphics;
using Terraria.DataStructures;
using TerRoguelike.Utilities;
using Terraria.Graphics.Effects;

namespace TerRoguelike.UI
{
    public static class EnemyHealthbarUI
    {
        //"ripper bars" lifted from the Calamity Mod and largely gutted to only the things needed for drawing the barrier bar
        private static Texture2D pixelTex, glowTex;
        internal static void Load()
        {
            pixelTex = TexDict["Square"];
            glowTex = TexDict["CircularGlow"];
        }

        internal static void Unload()
        {
            pixelTex = glowTex = null;
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            var healthBar = enemyHealthBar;
            if (enemyHealthBar.Opacity == 0)
                return;

            Vector2 pixlelScale = new Vector2(0.25f);
            Vector2 barDimensions = new Vector2(500, 24);
            Vector2 barCenter = new Vector2(Main.screenWidth * 0.5f, 50);
            bool barOverlappingHotbar = (barCenter.X - barDimensions.X * 0.5f * Main.UIScale) < 428 / Main.UIScale + 20;
            if (barOverlappingHotbar)
                barCenter.Y += 47 * Main.UIScale * 0.5f + 12;
            Vector2 barDrawStart = barCenter + barDimensions * -0.5f;

            Vector2 MainBarScale = barDimensions * new Vector2(healthBar.MainBar, 1);
            Vector2 ExtraBarScale = barDimensions * new Vector2(healthBar.ExtraBar, 1);
            Vector2 opacityToScaleMultiplier = new Vector2(1 - (float)Math.Pow(1 - MathHelper.Clamp(MathHelper.Lerp(0, 1, (healthBar.Opacity - 0.33f) * 1.5f), 0, 1), 3f), 1);

            Color underlayColor = new Color(0.3f, 0.3f, 0.3f);
            Vector2 underlayBarInflate = new Vector2(60, 3);

            Main.spriteBatch.End();
            Effect fadeEffect = Filters.Scene["TerRoguelike:SideFade"].GetShader().Shader;
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, fadeEffect, Main.UIScaleMatrix);

            Color tint = underlayColor * (float)Math.Pow(healthBar.Opacity, 4) * 0.66f;
            fadeEffect.Parameters["tint"].SetValue(tint.ToVector4());
            fadeEffect.Parameters["fadeTint"].SetValue(Color.Transparent.ToVector4());
            fadeEffect.Parameters["fadeCutoff"].SetValue(0.1f);

            Main.EntitySpriteDraw(pixelTex, (barDrawStart - underlayBarInflate).ToPoint().ToVector2(), null, Color.White, 0, Vector2.Zero, (barDimensions + underlayBarInflate * 2) * pixlelScale, SpriteEffects.None);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);

            Color extraBarColor = new Color(1f, 0.8f, 0.8f);
            Main.EntitySpriteDraw(pixelTex, barDrawStart.ToPoint().ToVector2(), null, extraBarColor * (float)Math.Pow(healthBar.Opacity, 4), 0, Vector2.Zero, ExtraBarScale * pixlelScale * opacityToScaleMultiplier, SpriteEffects.None);

            Color mainBarColor = new Color(1f, 0, 0);
            Main.EntitySpriteDraw(pixelTex, barDrawStart.ToPoint().ToVector2(), null, mainBarColor * healthBar.Opacity, 0, Vector2.Zero, MainBarScale * pixlelScale * opacityToScaleMultiplier, SpriteEffects.None);

            DynamicSpriteFont healthFont = FontAssets.MouseText.Value;

            Color healthDisplayColor = Color.White * healthBar.Opacity;
            Vector2 healthDisplayScale = new Vector2(1);

            string currentHealth = healthBar.CurrentHealth.ToString();
            Vector2 currentHealthDimensions = healthFont.MeasureString(currentHealth) * new Vector2(1, 0.75f);

            string maxHealth = healthBar.MaxHealth.ToString();
            Vector2 maxHealthDimensions = healthFont.MeasureString(maxHealth) * new Vector2(1, 0.75f);

            Vector2 slashDimensions = healthFont.MeasureString("/") * new Vector2(1, 0.75f);
            for (int i = 0; i < 2; i++)
            {
                Color color = Color.Black * healthBar.Opacity * 0.5f;
                Vector2 anchor = barCenter + new Vector2(2.3f);
                if (i != 0)
                {
                    color = healthDisplayColor;
                    anchor = barCenter;
                }
                ChatManager.DrawColorCodedString(spriteBatch, healthFont, "/", anchor + new Vector2(4, 0), color, 0, slashDimensions * 0.5f, healthDisplayScale);

                ChatManager.DrawColorCodedString(spriteBatch, healthFont, currentHealth, anchor + new Vector2(-6, 0), color, 0, currentHealthDimensions * new Vector2(1, 0.5f), healthDisplayScale);
                ChatManager.DrawColorCodedString(spriteBatch, healthFont, maxHealth, anchor + new Vector2(14, 0), color, 0, maxHealthDimensions * new Vector2(0, 0.5f), healthDisplayScale);
            }
            
            DynamicSpriteFont nameFont = FontAssets.DeathText.Value;
            Vector2 enemyNameScale = new Vector2(0.5f);
            Color enemyNameColor = healthDisplayColor * healthBar.Opacity;
            Vector2 enemyNameDimensions = nameFont.MeasureString(healthBar.Name) * new Vector2(1, 0.75f);
            enemyNameDimensions.X -= nameFont.MeasureString(" ").X;
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, nameFont, healthBar.Name, barCenter + new Vector2(0, -barDimensions.Y - 8), enemyNameColor, 0, enemyNameDimensions * 0.5f, enemyNameScale);
        }
    }
}
