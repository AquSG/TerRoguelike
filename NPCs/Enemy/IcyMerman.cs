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
using Terraria.GameContent.Animations;
using Terraria.Graphics.Shaders;
using static TerRoguelike.Schematics.SchematicManager;
using static TerRoguelike.Managers.TextureManager;

namespace TerRoguelike.NPCs.Enemy
{
    public class IcyMerman : BaseRoguelikeNPC
    {
        public Texture2D headTex;
        public int currentFrame = 0;
        public List<int> offsetFrames = new List<int>() { 3, 4, 5, 10, 11, 12 };
        public override int modNPCID => ModContent.NPCType<IcyMerman>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Snow"] };
        public override int CombatStyle => 2;


        public Vector2 headPosition = Vector2.Zero;
        public int headYOffset = 0;
        public float headRotation = 0;
        public SpriteEffects spriteEffects = SpriteEffects.None;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 16;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 18;
            NPC.height = 40;
            NPC.aiStyle = -1;
            NPC.damage = 36;
            NPC.lifeMax = 600;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.5f;
            modNPC.drawCenter = new Vector2(0, -6);
            headTex = TexDict["IcyMermanHead"].Value;
        }
        public override void AI()
        {
            NPC.frameCounter += NPC.velocity.Length() * 0.25d;
            modNPC.RogueFighterShooterAI(NPC, 2f, -7.9f, 320f, 30, 30, 0.3f, ProjectileID.IceBolt, 5f, new Vector2(0, -14), NPC.damage, true);
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            NPC.ai[0] = 0;
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / 20.0; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 137, hit.HitDirection, -1f);
                }
            }
            else
            {
                for (int i = 0; i < 35; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 137, 2 * hit.HitDirection, -2f);
                }
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 273, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 274, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 274, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, 275, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, 275, NPC.scale);
            }   
        }
        public override void FindFrame(int frameHeight)
        {
            currentFrame = NPC.velocity.Y == 0 ? (int)(NPC.frameCounter % (Main.npcFrameCount[modNPCID] - 2)) + 2 : 1;
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, TextureAssets.Npc[modNPCID].Value.Width, frameHeight);
        }
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            spriteBatch.Draw(headTex, headPosition, null, drawColor, headRotation, headTex.Size() * 0.5f, NPC.scale, spriteEffects, 0f);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            headRotation = NPC.spriteDirection == -1 ? 0f : MathHelper.Pi;

            if (modNPC.targetNPC != -1 && NPC.ai[0] >= 0)
            {
                NPC npc = Main.npc[modNPC.targetNPC];
                headRotation = (NPC.Center - npc.Center).ToRotation();
            }
            else if (modNPC.targetPlayer != -1 && NPC.ai[0] >= 0)
            {
                Player player = Main.player[modNPC.targetPlayer];
                headRotation = (NPC.Center - player.Center).ToRotation();
            }

            headYOffset = 0;
            if (offsetFrames.Contains(currentFrame))
                headYOffset = -2;

            headPosition = NPC.Center + new Vector2(0, -14 + headYOffset + NPC.gfxOffY) - Main.screenPosition + new Vector2(2, 0).RotatedBy(headRotation);
            spriteEffects = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipVertically;

            if (modNPC.ignitedStacks.Any())
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                
                Color color = Color.Lerp(Color.Yellow, Color.OrangeRed, Main.rand.NextFloat(0.4f, 0.6f + float.Epsilon) + 0.2f + (0.2f * (float)Math.Cos((Main.GlobalTimeWrappedHourly * 20f)))) * 0.8f;
                Vector3 colorHSL = Main.rgbToHsl(color);
                float outlineThickness = 1f;

                GameShaders.Misc["TerRoguelike:BasicTint"].UseOpacity(1f);
                GameShaders.Misc["TerRoguelike:BasicTint"].UseColor(Main.hslToRgb(1 - colorHSL.X, colorHSL.Y, colorHSL.Z));
                GameShaders.Misc["TerRoguelike:BasicTint"].Apply();

                Vector2 position = headPosition + (Vector2.UnitY * (NPC.gfxOffY));
                for (float j = 0; j < 1; j += 0.125f)
                {
                    spriteBatch.Draw(headTex, position + (j * MathHelper.TwoPi + headRotation + MathHelper.PiOver2).ToRotationVector2() * outlineThickness, null, color, headRotation, headTex.Size() * 0.5f, NPC.scale, spriteEffects, 0f);
                }
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            return true;
        }
    }
}
