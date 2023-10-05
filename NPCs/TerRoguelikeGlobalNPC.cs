using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using TerRoguelike;
using TerRoguelike.World;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.Graphics;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;

namespace TerRoguelike.NPCs
{
    public class TerRoguelikeGlobalNPC : GlobalNPC
    {
        #region Variables
        public bool isRoomNPC = false;
        public int sourceRoomListID = -1;

        public bool activatedSoulstealCoating = false;
        public bool activatedAmberBead = false;
        public bool activatedItemPotentiometer = false;

        public bool ignited = false;
        #endregion
        public override bool InstancePerEntity => true;
        public override bool PreKill(NPC npc)
        {
            if (!isRoomNPC)
                return true;

            var AllLoadedItemIDs = new int[ItemLoader.ItemCount];
            for (int i = 0; i < ItemLoader.ItemCount; i++)
            {
                AllLoadedItemIDs[i] = i;
            }
            foreach (int itemID in AllLoadedItemIDs)
            {
                NPCLoader.blockLoot.Add(itemID);
            }
            npc.value = 0;
            
            return true;
        }
        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            if (TerRoguelikeWorld.IsTerRoguelikeWorld)
            {
                modifiers.DefenseEffectiveness *= 0f;
            }
        }
        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (ignited)
            {
                /*
                Texture2D texture = TextureAssets.Npc[npc.type].Value;
                SpriteEffects spriteEffects = npc.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                for (int i = 0; i < 4; i++)
                {
                    drawColor = Color.Lerp(Color.Yellow, Color.Orange, Main.rand.NextFloat(0.3f + float.Epsilon) + 0.35f + (0.35f * (float)Math.Cos((Main.GlobalTimeWrappedHourly * 8f % 60f)))) * 0.8f;
                    Vector2 position =  npc.Center + (Vector2.UnitY * 4).RotatedBy(MathHelper.PiOver2 * i) - Main.screenPosition;
                    Main.EntitySpriteDraw(texture, position, npc.frame, drawColor, npc.rotation, npc.frame.Size() * 0.5f, npc.scale, spriteEffects);
                }
                */

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                Texture2D texture = TextureAssets.Npc[npc.type].Value;
                Color color = Color.Lerp(Color.Yellow, Color.Orange, Main.rand.NextFloat(0.3f + float.Epsilon) + 0.35f + (0.35f * (float)Math.Cos((Main.GlobalTimeWrappedHourly * 8f % 60f)))) * 0.8f;
                Vector3 colorHSL = Main.rgbToHsl(color);
                float outlineThickness = 1f;
                Vector2 vector = new Vector2(npc.frame.Width / 2f, npc.frame.Height / 2f);
                SpriteEffects spriteEffects = npc.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

                GameShaders.Misc["TerRoguelike:BasicTint"].UseOpacity(1f);
                GameShaders.Misc["TerRoguelike:BasicTint"].UseColor(Main.hslToRgb(1 - colorHSL.X, colorHSL.Y, colorHSL.Z));
                GameShaders.Misc["TerRoguelike:BasicTint"].Apply();
                Vector2 offset = new Vector2(0f, (-npc.frame.Height / 2f + 4) * npc.scale);
                Vector2 offset2 = new Vector2((npc.frame.Width / 4f + 4) * npc.scale, (npc.frame.Height / 4f) * npc.scale + 4);

                for (float i = 0; i < 1; i += 0.125f)
{
                    spriteBatch.Draw(texture, npc.Bottom + offset + (i * MathHelper.TwoPi + npc.rotation).ToRotationVector2() * outlineThickness - Main.screenPosition, npc.frame, color, npc.rotation, vector, npc.scale, spriteEffects, 0f);
                }

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            }

            return true;
        }
    }
}
