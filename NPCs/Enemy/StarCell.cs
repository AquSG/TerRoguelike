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
using TerRoguelike.Projectiles;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using static TerRoguelike.Schematics.SchematicManager;
using static TerRoguelike.Managers.TextureManager;
using Terraria.Graphics.Shaders;

namespace TerRoguelike.NPCs.Enemy
{
    public class StarCell : BaseRoguelikeNPC
    {
        public Texture2D lightTex;
        public override int modNPCID => ModContent.NPCType<StarCell>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Lunar"] };
        public override int CombatStyle => 0;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 4;
            NPCID.Sets.TrailCacheLength[modNPCID] = 10;
            NPCID.Sets.TrailingMode[modNPCID] = 3;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 36;
            NPC.height = 36;
            NPC.aiStyle = -1;
            NPC.damage = 34;
            NPC.lifeMax = 600;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.6f;
            modNPC.drawCenter = new Vector2(0, 0);
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            lightTex = TexDict["StarCellGlow"];
            modNPC.OverrideIgniteVisual = true;
        }
        public override void AI()
        {
            NPC.frameCounter += 0.18d;
            modNPC.RogueFlierAI(NPC, 6f, 6f, 0.12f, true, true);
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < 5.0 + hit.Damage / 10.0; i++)
                {
                    int width = NPC.width / 4;
                    int dust = Dust.NewDust(NPC.Center - Vector2.One * (float)width, width * 2, width * 2, DustID.Clentaminator_Cyan);
                    Dust d = Main.dust[dust];
                    Vector2 offset = Vector2.Normalize(d.position - NPC.Center);
                    d.position = NPC.Center + offset * (float)width * NPC.scale - new Vector2(4f);
                    if (i < 30)
                    {
                        d.velocity = offset * ((Vector2)(d.velocity)).Length() * 2f;
                    }
                    else
                    {
                        d.velocity = 2f * offset * (float)Main.rand.Next(45, 91) / 10f;
                    }
                    d.noGravity = true;
                    d.scale = 0.7f + Main.rand.NextFloat();
                    d.noLight = true;
                    d.noLightEmittence = true;
                }
            }
            else
            {
                SoundEngine.PlaySound(SoundID.NPCDeath7 with { Volume = 0.5f }, NPC.Center);
                for (int p = 0; p < 4; p++)
                {
                    int proj = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Main.rand.NextVector2CircularEdge(5f, 5f) * Main.rand.NextFloat(0.5f, 1f), ModContent.ProjectileType<SeekingStarCell>(), NPC.damage, 0);
                }
                for (int i = 0; i < 60; i++)
                {
                    int width = NPC.width / 4;
                    int dust = Dust.NewDust(NPC.Center - Vector2.One * (float)width, width * 2, width * 2, DustID.Clentaminator_Cyan);
                    Dust d = Main.dust[dust];
                    Vector2 offset = Vector2.Normalize(d.position - NPC.Center);
                    d.position = NPC.Center + offset * (float)width * NPC.scale - new Vector2(4f);
                    if (i < 30)
                    {
                        d.velocity = offset * ((Vector2)(d.velocity)).Length() * 0.4f;
                    }
                    else
                    {
                        d.velocity = 2f * offset * (float)Main.rand.Next(45, 91) * 0.1f;
                    }
                    d.noGravity = true;
                    d.scale = 0.7f;
                    d.noLight = true;
                    d.noLightEmittence = true;
                }
            }

        }
        public override void FindFrame(int frameHeight)
        {
            int currentFrame = (int)(NPC.frameCounter % Main.npcFrameCount[modNPCID]);
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, TextureAssets.Npc[modNPCID].Value.Width, frameHeight);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;
            Vector2 offset = new Vector2(NPC.width * 0.5f, NPC.height * 0.5f);

            for (int i = 1; i < NPC.oldPos.Length; i++)
            {
                float opacity = MathHelper.Lerp(0.18f, 0, (float)i / NPC.oldPos.Length);
                Main.EntitySpriteDraw(lightTex, NPC.oldPos[i] + offset - Main.screenPosition + (Vector2.UnitY * NPC.gfxOffY) + modNPC.drawCenter, NPC.frame, Color.White * opacity, NPC.oldRot[i], NPC.frame.Size() * 0.5f, NPC.scale, SpriteEffects.None);
            }

            if (modNPC.ignitedStacks.Any())
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                Color color = Color.Lerp(Color.Yellow, Color.OrangeRed, Main.rand.NextFloat(0.4f, 0.6f + float.Epsilon) + 0.2f + (0.2f * (float)Math.Cos((Main.GlobalTimeWrappedHourly * 20f)))) * 0.8f;
                Vector3 colorHSL = Main.rgbToHsl(color);
                float outlineThickness = 2f;

                GameShaders.Misc["TerRoguelike:BasicTint"].UseOpacity(1f);
                GameShaders.Misc["TerRoguelike:BasicTint"].UseColor(Main.hslToRgb(1 - colorHSL.X, colorHSL.Y, colorHSL.Z));
                GameShaders.Misc["TerRoguelike:BasicTint"].Apply();

                for (float j = 0; j < 1; j += 0.125f)
                {
                    spriteBatch.Draw(tex, NPC.Center - Main.screenPosition + ((j * MathHelper.TwoPi + NPC.rotation).ToRotationVector2() * outlineThickness) + (Vector2.UnitY * NPC.gfxOffY) + modNPC.drawCenter, NPC.frame, drawColor, NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, SpriteEffects.None, 0f);
                }
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            Main.EntitySpriteDraw(tex, NPC.Center - Main.screenPosition + (Vector2.UnitY * NPC.gfxOffY) + modNPC.drawCenter, NPC.frame, drawColor, NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, SpriteEffects.None);
            Main.EntitySpriteDraw(lightTex, NPC.Center - Main.screenPosition + (Vector2.UnitY * NPC.gfxOffY) + modNPC.drawCenter, NPC.frame, Color.White, NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, SpriteEffects.None);
            return false;
        }
        public override bool? CanFallThroughPlatforms()
        {
            return true;
        }
    }
}
