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
using TerRoguelike.TerPlayer;

namespace TerRoguelike.NPCs
{
    public class TerRoguelikeGlobalNPC : GlobalNPC
    {
        #region Variables
        public bool isRoomNPC = false;
        public int sourceRoomListID = -1;

        public bool activatedHotPepper = false;
        public bool activatedSoulstealCoating = false;
        public bool activatedAmberBead = false;
        public bool activatedItemPotentiometer = false;

        public List<IgnitedStack> ignitedStacks = new List<IgnitedStack>();
        public int ignitedHitCooldown = 0;
        #endregion
        public override bool InstancePerEntity => true;

        public override void PostAI(NPC npc)
        {
            if (ignitedStacks != null && ignitedStacks.Any())
            {
                if (ignitedHitCooldown <= 0)
                {
                    int hitDamage = 0;
                    int targetDamage = (int)(npc.lifeMax * 0.01f);
                    int damageCap = 5;
                    int owner = -1;
                    if (targetDamage > damageCap)
                        targetDamage = damageCap;
                    else if (targetDamage <= 0)
                        targetDamage = 1;

                    for (int i = 0; i < ignitedStacks.Count; i++)
                    {
                        if (ignitedStacks[i].DamageToDeal < targetDamage)
                        {
                            hitDamage += ignitedStacks[i].DamageToDeal;
                            ignitedStacks[i].DamageToDeal = 0;
                        }
                        else
                        {
                            hitDamage += targetDamage;
                            ignitedStacks[i].DamageToDeal -= targetDamage;
                        }
                        if (i == ignitedStacks.Count - 1)
                        {
                            owner = ignitedStacks[i].Owner;
                        }
                    }
                    IgniteHit(hitDamage, npc, owner);
                    ignitedStacks.RemoveAll(x => x.DamageToDeal <= 0);
                }
                else
                {
                    ignitedHitCooldown--;
                }
            }
        }
        public void IgniteHit(int hitDamage, NPC npc, int owner)
        {
            TerRoguelikePlayer modPlayer = Main.player[owner].GetModPlayer<TerRoguelikePlayer>();

            hitDamage = (int)(hitDamage * modPlayer.GetBonusDamageMulti(npc, npc.Center));

            if (npc.life - hitDamage <= 0)
            {
                modPlayer.OnKillEffects(npc);
            }
            NPC.HitInfo info = new NPC.HitInfo();
            info.HideCombatText = true;
            info.Damage = hitDamage;
            info.InstantKill = false;
            info.HitDirection = 1;
            info.Knockback = 0f;
            info.Crit = false;

            npc.StrikeNPC(info);
            NetMessage.SendStrikeNPC(npc, info);
            CombatText.NewText(npc.getRect(), Color.DarkKhaki, hitDamage);
            ignitedHitCooldown += 10;
        }
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
            if (ignitedStacks != null && ignitedStacks.Any())
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
                Color color = Color.Lerp(Color.Yellow, Color.OrangeRed, Main.rand.NextFloat(0.4f, 0.6f + float.Epsilon) + 0.2f + (0.2f * (float)Math.Cos((Main.GlobalTimeWrappedHourly * 20f)))) * 0.8f;
                Vector3 colorHSL = Main.rgbToHsl(color);
                float outlineThickness = 1f;
                Vector2 vector = new Vector2(npc.frame.Width / 2f, npc.frame.Height / 2f);
                SpriteEffects spriteEffects = npc.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

                GameShaders.Misc["TerRoguelike:BasicTint"].UseOpacity(1f);
                GameShaders.Misc["TerRoguelike:BasicTint"].UseColor(Main.hslToRgb(1 - colorHSL.X, colorHSL.Y, colorHSL.Z));
                GameShaders.Misc["TerRoguelike:BasicTint"].Apply();

                Vector2 position = npc.Center;
                switch (npc.aiStyle)
                {
                    case NPCAIStyleID.Fighter:
                        position = npc.VisualPosition + new Vector2(npc.width / 2, npc.height) + new Vector2(0f, (-npc.frame.Height / 2f + 4) * npc.scale);
                        break;
                    case NPCAIStyleID.Slime:
                        position = npc.VisualPosition + new Vector2(npc.width / 2, npc.height) + new Vector2(0f, (-npc.frame.Height / 2f + 4) * npc.scale);
                        break;
                    case NPCAIStyleID.KingSlime:
                        position = npc.VisualPosition + new Vector2(npc.width / 2, npc.height) + new Vector2(0f, (-npc.frame.Height / 2f + 4) * npc.scale);
                        break;
                    case NPCAIStyleID.TeslaTurret:
                        position += new Vector2(-2, 2);
                        break;
                    case NPCAIStyleID.Flying:
                        position += new Vector2(0, 5);
                        break;
                    case NPCAIStyleID.HoveringFighter:
                        position += new Vector2(0, 5);
                        break;
                    case NPCAIStyleID.EnchantedSword:
                        if (npc.type == NPCID.CursedHammer)
                            position += new Vector2(0, 2);
                        if (npc.type == NPCID.CrimsonAxe)
                            position += new Vector2(0, -9);
                        break;
                }

                for (float i = 0; i < 1; i += 0.125f)
                {
                    spriteBatch.Draw(texture, position + new Vector2(0, npc.gfxOffY) + (i * MathHelper.TwoPi + npc.rotation).ToRotationVector2() * outlineThickness - Main.screenPosition, npc.frame, color, npc.rotation, vector, npc.scale, spriteEffects, 0f);
                }

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            }

            return true;
        }
        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (ignitedStacks != null && ignitedStacks.Any())
            {
                drawColor = Color.Lerp(Color.White, Color.OrangeRed, 0.4f);
                for (int i = 0; i < ignitedStacks.Count; i++)
                {
                    if (Main.rand.NextBool(5))
                        Dust.NewDust(npc.position, npc.width, npc.height, DustID.Torch);
                }
            }
        }
    }

    public class IgnitedStack
    {
        public IgnitedStack(int damageToDeal, int owner)
        {
            DamageToDeal = damageToDeal;
            Owner = owner;
        }
        public int DamageToDeal = 0;
        public int Owner = -1;
    }
}
