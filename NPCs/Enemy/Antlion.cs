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
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using static TerRoguelike.Schematics.SchematicManager;
using static TerRoguelike.Managers.TextureManager;

namespace TerRoguelike.NPCs.Enemy
{
    public class Antlion : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<Antlion>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Desert"] };
        public override int CombatStyle => 2;
        public Texture2D headTex;
        public Texture2D texture;
        public int attackTelegraph = 120;
        public int attackCooldown = 60;
        public int burrowDownTime = 60;
        public int burrowUpTime = 60;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 2;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 24;
            NPC.height = 24;
            NPC.aiStyle = -1;
            NPC.damage = 25;
            NPC.lifeMax = 800;
            NPC.HitSound = SoundID.NPCHit31;
            NPC.DeathSound = SoundID.NPCDeath34;
            NPC.knockBackResist = 0f;
            NPC.behindTiles = true;
            NPC.noGravity = false;
            modNPC.IgnoreRoomWallCollision = true;
            modNPC.drawCenter = new Vector2(0, 0);
            headTex = TexDict["AntlionHead"];
            modNPC.OverrideIgniteVisual = true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.rotation = MathHelper.PiOver2;
            
        }
        public override void AI()
        {
            NPC.frameCounter += 0.2d;
            modNPC.RogueAntlionAI(NPC, MathHelper.PiOver2, 80f, 200f, burrowDownTime, burrowUpTime, 48f, 360, attackTelegraph, attackCooldown, ModContent.ProjectileType<SandBlast>(), Vector2.Zero, 15, NPC.damage, MathHelper.Pi * 0.0675f, 9f, 15f);

            if (NPC.ai[0] >= 0 && (int)(NPC.ai[0] - attackTelegraph) % (attackTelegraph + attackCooldown) == 0)
            {
                SoundEngine.PlaySound(SoundID.NPCDeath13 with { Volume = 0.75f }, NPC.Center);
            }
            else if (NPC.ai[0] < 0 && NPC.ai[0] % 2 == 0)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Sand, 0, 0, 0, default, 0.8f);
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)NPC.lifeMax * (double)100; i++)
                {
                    Dust.NewDust(new Vector2(NPC.position.X, NPC.Center.Y), NPC.width, NPC.height / 2, 250, hit.HitDirection, -1f);
                }
            }
            else
            {
                for (int i = 0; i < 50; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 250, 2 * hit.HitDirection, -2f);
                }
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 97);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 98);
            }
            
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            texture = TextureAssets.Npc[Type].Value;
            int headFrameHeight = (int)(headTex.Size().Y * 0.2d);
            int headFrame = (int)(NPC.frameCounter % 5d);
            int frameHeight = (int)(texture.Size().Y * 0.5d);
            float opacity = MathHelper.Clamp(MathHelper.Lerp(0, 1f, Math.Abs(NPC.ai[0] + burrowDownTime) / 20), 0f, 1f);

            if (modNPC.ignitedStacks.Count > 0)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                Color color = Color.Lerp(Color.Yellow, Color.OrangeRed, Main.rand.NextFloat(0.4f, 0.6f + float.Epsilon) + 0.2f + (0.2f * (float)Math.Cos((Main.GlobalTimeWrappedHourly * 20f)))) * 0.8f;
                Vector3 colorHSL = Main.rgbToHsl(color);
                float outlineThickness = 1f;

                GameShaders.Misc["TerRoguelike:BasicTint"].UseOpacity(1f);
                GameShaders.Misc["TerRoguelike:BasicTint"].UseColor(Main.hslToRgb(1 - colorHSL.X, colorHSL.Y, colorHSL.Z));
                GameShaders.Misc["TerRoguelike:BasicTint"].Apply();

                for (float j = 0; j < 1; j += 0.125f)
                {
                    spriteBatch.Draw(headTex, NPC.Center - Main.screenPosition + (Vector2.UnitY * 8) + (j * MathHelper.TwoPi + NPC.rotation - MathHelper.PiOver2).ToRotationVector2() * outlineThickness, new Rectangle(0, headFrameHeight * headFrame, headTex.Width, headFrameHeight), color * opacity, NPC.rotation - MathHelper.PiOver2, new Vector2(headTex.Width * 0.5f, (headFrameHeight * 0.5f)) + (Vector2.UnitY * 8), NPC.scale, SpriteEffects.None, 0f);
                    spriteBatch.Draw(texture, NPC.Center - Main.screenPosition + (Vector2.UnitY * 10) + (j * MathHelper.TwoPi).ToRotationVector2() * outlineThickness, new Rectangle(0, frameHeight, texture.Width, frameHeight), color * opacity, 0f, new Vector2(headTex.Width * 0.5f, (frameHeight * 0.5f)), NPC.scale, SpriteEffects.None, 0f);
                }
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            Main.EntitySpriteDraw(headTex, NPC.Center - Main.screenPosition + (Vector2.UnitY * 8), new Rectangle(0, headFrameHeight * headFrame, headTex.Width, headFrameHeight), drawColor * opacity, NPC.rotation - MathHelper.PiOver2, new Vector2(headTex.Width * 0.5f, (headFrameHeight * 0.5f)) + (Vector2.UnitY * 8), NPC.scale, SpriteEffects.None);
            Main.EntitySpriteDraw(texture, NPC.Center - Main.screenPosition + (Vector2.UnitY * 10), new Rectangle(0, frameHeight, texture.Width, frameHeight), drawColor * opacity, 0f, new Vector2(headTex.Width * 0.5f, (frameHeight * 0.5f)), NPC.scale, SpriteEffects.None);
            return false;
        }
    }
}
