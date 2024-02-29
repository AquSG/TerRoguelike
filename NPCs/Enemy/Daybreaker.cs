using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TerRoguelike;
using TerRoguelike.TerPlayer;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.GameContent;
using TerRoguelike.NPCs;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using TerRoguelike.Projectiles;
using static TerRoguelike.Schematics.SchematicManager;
using Terraria.Graphics.Shaders;
using Terraria.DataStructures;

namespace TerRoguelike.NPCs.Enemy
{
    public class Daybreaker : BaseRoguelikeNPC
    {
        public Texture2D lightTex = ModContent.Request<Texture2D>("TerRoguelike/NPCs/Enemy/DaybreakerGlow").Value;
        public Texture2D armTex = ModContent.Request<Texture2D>("TerRoguelike/NPCs/Enemy/DaybreakerArm").Value;
        public Texture2D wepTex = ModContent.Request<Texture2D>("TerRoguelike/Projectiles/Daybreak").Value;
        public override int modNPCID => ModContent.NPCType<Daybreaker>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Lunar"] };
        public override int CombatStyle => 2;
        public int attackTelegraph = 20;
        public int attackExtendTime = 24;
        public int attackCooldown = 120;
        int currentFrame;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 15;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 22;
            NPC.height = 47;
            NPC.aiStyle = -1;
            NPC.damage = 36;
            NPC.lifeMax = 800;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.2f;
            modNPC.drawCenter = new Vector2(0, -6);
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.ai[1] = -attackCooldown + 1;
        }
        public override void AI()
        {
            NPC.frameCounter += NPC.velocity.Length() * 0.08d;
            Vector2 weaponPos = Vector2.Zero;
            float weaponRot = 0;
            SpriteEffects spriteEffects = SpriteEffects.None;
            GetWeaponPositionAndRotation(ref weaponPos, ref weaponRot, ref spriteEffects);

            modNPC.RogueFighterShooterAI(NPC, 3f, -11.4f, 1000f, attackTelegraph, attackCooldown, 0f, ModContent.ProjectileType<Daybreak>(), 12f, weaponPos - NPC.Center, NPC.damage, true, false, weaponRot, attackExtendTime);
            if (NPC.ai[1] == attackTelegraph)
            {
                SoundEngine.PlaySound(SoundID.Item1 with { Volume = 0.6f }, NPC.Center);
            }
            if (NPC.ai[1] < -70 && (int)NPC.ai[1] % 2 == 0)
            {
                float scale = MathHelper.Lerp(1f, 0, (-NPC.ai[1] - 60) / 60);
                Dust d = Dust.NewDustPerfect(weaponPos, DustID.SolarFlare, null, 0, default, scale);
                d.noGravity = true;
                d.noLight = true;
                d.noLightEmittence = true;
            }
            else if (NPC.ai[1] == -65)
            {
                Vector2 pos = (weaponPos) + ((Vector2.UnitX * wepTex.Width * 0.5f) + (spriteEffects == SpriteEffects.FlipHorizontally ? Vector2.Zero : -Vector2.UnitX * wepTex.Width * 0.1f)).RotatedBy(weaponRot + MathHelper.Pi);
                for (int i = 0; i < wepTex.Width; i += 3)
                {
                    pos += (Vector2.UnitX * 3).RotatedBy(weaponRot);

                    Dust d = Dust.NewDustPerfect(pos, DustID.SolarFlare, null, 0, Color.White, 0.4f);
                    d.noGravity = true;
                    d.noLight = true;
                    d.noLightEmittence = true;
                }
                SoundEngine.PlaySound(SoundID.DD2_GhastlyGlaiveImpactGhost with { Volume = 0.55f }, NPC.Center);
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            NPC.ai[0] = 0;
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / 20.0; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.MarblePot, hit.HitDirection, -1f);
                    if (Main.rand.NextBool(4))
                    {
                        Dust d = Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, 6)];
                        d.noGravity = true;
                        d.scale = 1.5f;
                        d.fadeIn = 1f;
                        d.velocity *= 3f;
                    }
                }
            }
            else
            {
                for (int i = 0; i < 20; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.MarblePot, hit.HitDirection, -1f);
                    if (Main.rand.NextBool(3))
                    {
                        Dust d = Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, 6)];
                        d.noGravity = true;
                        d.scale = 1.5f;
                        d.fadeIn = 1f;
                        d.velocity *= 3f;
                    }
                }
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 844, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 845, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 847, NPC.scale);
            }
        }
        public override void FindFrame(int frameHeight)
        {
            int frameCount = Main.npcFrameCount[modNPCID];
            if (NPC.ai[1] > 0)
            {
                currentFrame = NPC.ai[1] < attackTelegraph ? frameCount - 5 : (int)(((NPC.ai[1] - attackTelegraph) * 4) / attackExtendTime) + frameCount - 4;
            }
            else
            {
                currentFrame = NPC.velocity.Y == 0 ? (int)(NPC.frameCounter % (frameCount - 7)) + 2 : 1;
            }
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, TextureAssets.Npc[modNPCID].Value.Width, frameHeight);
        }
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Main.EntitySpriteDraw(lightTex, NPC.Center - Main.screenPosition + modNPC.drawCenter + (Vector2.UnitY * NPC.gfxOffY), NPC.frame, Color.White, NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            if (NPC.ai[1] < attackTelegraph)
            {
                Vector2 weaponPos = Vector2.Zero;
                float weaponRot = 0;
                SpriteEffects spriteEffects = SpriteEffects.None;
                GetWeaponPositionAndRotation(ref weaponPos, ref weaponRot, ref spriteEffects);

                if (NPC.ai[1] >= -attackCooldown + 60)
                    Main.EntitySpriteDraw(wepTex, weaponPos - Main.screenPosition, null, Color.White, weaponRot, wepTex.Size() * 0.5f, NPC.scale, spriteEffects);

                if (NPC.ai[1] < 0 && NPC.ai[1] > -attackCooldown + 50)
                {
                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                    float opacity = NPC.ai[1] >= -attackCooldown + 60 ? MathHelper.Lerp(0, 1f, -NPC.ai[1] / 60) : MathHelper.Clamp(MathHelper.Lerp(1f, 0, (-NPC.ai[1] - 60) / ((attackCooldown - 110))), 0, 1f);
                    Color color = Color.White * opacity;
                    Vector3 colorHSL = Main.rgbToHsl(color);

                    GameShaders.Misc["TerRoguelike:BasicTint"].UseOpacity(1f);
                    GameShaders.Misc["TerRoguelike:BasicTint"].UseColor(Main.hslToRgb(1 - colorHSL.X, colorHSL.Y, colorHSL.Z));
                    GameShaders.Misc["TerRoguelike:BasicTint"].Apply();

                    Main.EntitySpriteDraw(wepTex, weaponPos - Main.screenPosition, null, Color.White, weaponRot, wepTex.Size() * 0.5f, NPC.scale, spriteEffects);

                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }

            Main.EntitySpriteDraw(armTex, NPC.Center - Main.screenPosition + modNPC.drawCenter + (Vector2.UnitY * NPC.gfxOffY), NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
        }
        public void GetWeaponPositionAndRotation(ref Vector2 position, ref float rotation, ref SpriteEffects spriteEffects)
        {
            Vector2 spearOffset = new Vector2(-10 * NPC.spriteDirection, 16);
            float spearRot = NPC.spriteDirection > 0 ? MathHelper.Pi * 0.125f : -MathHelper.Pi * 0.125f;
            spriteEffects = NPC.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            switch (currentFrame)
            {
                case 1:
                    spearOffset.X -= 2 * NPC.spriteDirection;
                    spearOffset.Y -= 16;
                    break;
                case 2:
                    spearOffset.X += 2 * NPC.spriteDirection;
                    spearOffset.Y -= 4;
                    break;
                case 3:
                    spearOffset.X += 2 * NPC.spriteDirection;
                    spearOffset.Y -= 4;
                    break;
                case 4:
                    spearOffset.Y -= 1;
                    break;
                case 5:
                    spearOffset.Y -= 1;
                    break;
                case 6:
                    spearOffset.Y -= 3;
                    break;
                case 7:
                    spearOffset.Y -= 3;
                    break;
                case 8:
                    spearOffset.Y -= 1;
                    break;
                case 9:
                    spearOffset.X += 2 * NPC.spriteDirection;
                    spearOffset.Y -= 1;
                    break;
                case 10:
                    spearOffset.X -= 2 * NPC.spriteDirection;
                    spearOffset.Y -= 28;
                    if (modNPC.targetNPC != -1 || modNPC.targetPlayer != -1)
                    {
                        Vector2 targetPos = Vector2.Zero;
                        if (modNPC.targetPlayer != -1)
                            targetPos = Main.player[modNPC.targetPlayer].Center;
                        else if (modNPC.targetNPC != -1)
                            targetPos = Main.npc[modNPC.targetNPC].Center;
                        spearRot = (targetPos - (NPC.Center + modNPC.drawCenter + (Vector2.UnitY * NPC.gfxOffY) + spearOffset)).ToRotation();
                        spriteEffects = SpriteEffects.None;
                    }
                    else
                        spearRot = 0;
                    break;
            }
            position = NPC.Center + spearOffset + modNPC.drawCenter + (Vector2.UnitY * NPC.gfxOffY);
            rotation = spearRot;
        }
    }
}
