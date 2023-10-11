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
        public bool activatedThrillOfTheHunt = false;
        public bool activatedClusterBombSatchel = false;
        public bool activatedSteamEngine = false;
        public bool activatedItemPotentiometer = false;

        public List<IgnitedStack> ignitedStacks = new List<IgnitedStack>();
        public int ignitedHitCooldown = 0;
        public List<BleedingStack> bleedingStacks = new List<BleedingStack>();
        public int bleedingHitCooldown = 0;
        public int ballAndChainSlow = 0;
        #endregion
        public override bool InstancePerEntity => true;

        public override bool PreAI(NPC npc)
        {
            if (ballAndChainSlow > 0)
            {
                npc.velocity /= 0.7f;
                ballAndChainSlow--;
            }
            return true;
        }
        public override void PostAI(NPC npc)
        {
            if (ignitedStacks != null && ignitedStacks.Any())
            {
                if (ignitedHitCooldown <= 0)
                {
                    int hitDamage = 0;
                    int targetDamage = (int)(npc.lifeMax * 0.01f);
                    int damageCap = 50;
                    int owner = -1;
                    if (targetDamage > damageCap)
                        targetDamage = damageCap;
                    else if (targetDamage < 1)
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
            }
            if (ignitedHitCooldown > 0)
                ignitedHitCooldown--;

            if (bleedingStacks != null && bleedingStacks.Any())
            {
                if (bleedingHitCooldown <= 0)
                {
                    int hitDamage = 0;
                    int targetDamage = 40;
                    int owner = -1;

                    for (int i = 0; i < bleedingStacks.Count; i++)
                    {
                        if (bleedingStacks[i].DamageToDeal < targetDamage)
                        {
                            hitDamage += ignitedStacks[i].DamageToDeal;
                            bleedingStacks[i].DamageToDeal = 0;
                        }
                        else
                        {
                            hitDamage += targetDamage;
                            bleedingStacks[i].DamageToDeal -= targetDamage;
                        }
                        if (i == bleedingStacks.Count - 1)
                        {
                            owner = bleedingStacks[i].Owner;
                        }
                    }
                    BleedingHit(hitDamage, npc, owner);
                    bleedingStacks.RemoveAll(x => x.DamageToDeal <= 0);
                }
            }
            if (bleedingHitCooldown > 0)
                bleedingHitCooldown--;

            if (ballAndChainSlow > 0)
            {
                npc.velocity *= 0.7f;
                ballAndChainSlow--;
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
        public void BleedingHit(int hitDamage, NPC npc, int owner)
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
            CombatText.NewText(npc.getRect(), Color.MediumVioletRed, hitDamage);
            bleedingHitCooldown = 20;
        }
        public void AddBleedingStackWithRefresh(BleedingStack stack)
        {
            if (bleedingStacks.Any())
            {
                for (int i = 0; i < bleedingStacks.Count; i++)
                {
                    if (bleedingStacks[i].DamageToDeal < stack.DamageToDeal)
                        bleedingStacks[i].DamageToDeal = stack.DamageToDeal;
                }
            }
            else
                bleedingHitCooldown += 20;

            bleedingStacks.Add(stack);
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

                Vector2 position = GetDrawCenter(npc);
                for (float i = 0; i < 1; i += 0.125f)
                {
                    spriteBatch.Draw(texture, position + (i * MathHelper.TwoPi + npc.rotation).ToRotationVector2() * outlineThickness - Main.screenPosition, npc.frame, color, npc.rotation, vector, npc.scale, spriteEffects, 0f);
                }

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            }

            if (bleedingStacks != null && bleedingStacks.Any())
            {

                DrawRotatlingBloodParticles(false, npc);
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
            if (ballAndChainSlow > 0)
            {
                drawColor = drawColor.MultiplyRGB(Color.LightGray);
                Dust.NewDust(npc.BottomLeft + new Vector2(0, -4f), npc.width, 1, DustID.t_Slime, newColor: Color.Gray, Scale: 0.5f);
            }
        }
        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (bleedingStacks != null && bleedingStacks.Any())
            {
                DrawRotatlingBloodParticles(true, npc);
                
            }
        }
        public void DrawRotatlingBloodParticles(bool inFront, NPC npc)
        {
            Texture2D texture = ModContent.Request<Texture2D>("TerRoguelike/Projectiles/AdaptiveGunBullet").Value;
            Color color = Color.Red * 0.8f;
            Vector2 position = GetDrawCenter(npc);

            for (int i = 0; i < bleedingStacks.Count; i++)
            {
                Vector2 specificPosition = position;
                float rotation = MathHelper.Lerp(0, MathHelper.TwoPi, Main.GlobalTimeWrappedHourly * 1.5f);
                float rotationCompletionOffset = MathHelper.TwoPi / bleedingStacks.Count * i;
                rotation += rotationCompletionOffset;
                specificPosition += new Vector2(0, 16).RotatedBy(rotation);
                specificPosition += (specificPosition - position) * new Vector2((npc.frame.Width * npc.scale) / 32f, -0.5f) ;
                if (specificPosition.Y >= position.Y && inFront)
                    Main.EntitySpriteDraw(texture, specificPosition - Main.screenPosition, null, color, 0f, texture.Size() * 0.5f, 1f, SpriteEffects.None);
                else if (!inFront)
                    Main.EntitySpriteDraw(texture, specificPosition - Main.screenPosition, null, color, 0f, texture.Size() * 0.5f, 1f, SpriteEffects.None);
            }
        }
        public Vector2 GetDrawCenter(NPC npc)
        {
            Vector2 position = npc.Center + new Vector2(0, npc.gfxOffY);
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
            return position;
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
    public class BleedingStack
    {
        public BleedingStack(int damageToDeal, int owner)
        {
            DamageToDeal = damageToDeal;
            Owner = owner;
        }
        public int DamageToDeal = 0;
        public int Owner = -1;
    }
}
