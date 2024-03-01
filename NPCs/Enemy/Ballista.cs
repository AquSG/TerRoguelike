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
using static TerRoguelike.Schematics.SchematicManager;
using TerRoguelike.Projectiles;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using static TerRoguelike.Managers.TextureManager;

namespace TerRoguelike.NPCs.Enemy
{
    public class Ballista : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<Ballista>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Base"] };
        public override int CombatStyle => 1;
        public int attackTelegraph = 10;
        public int attackCooldown = 170;
        public Texture2D texture;
        public Texture2D baseTex;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 7;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 32;
            NPC.height = 40;
            NPC.aiStyle = -1;
            NPC.damage = 30;
            NPC.lifeMax = 800;
            NPC.HitSound = SoundID.NPCHit42;
            NPC.DeathSound = SoundID.NPCDeath37;
            NPC.knockBackResist = 0f;
            modNPC.drawCenter = new Vector2(0, 0);
            modNPC.OverrideIgniteVisual = true;
            baseTex = TexDict["BallistaBase"];
        }
        public override void AI()
        {
            modNPC.RogueTurretAI(NPC, attackTelegraph, attackCooldown, 320f, ModContent.ProjectileType<BallistaShot>(), NPC.damage, 16f, Vector2.UnitY * -20, true, NPC.rotation, MathHelper.PiOver4);
            if (NPC.ai[0] == 1)
            {
                SoundEngine.PlaySound(SoundID.DD2_BallistaTowerShot with { Volume = 1f }, NPC.Center);
            }
            else if (NPC.ai[0] == -attackCooldown + 60)
            {
                SoundEngine.PlaySound(new SoundStyle("TerRoguelike/Sounds/creaking") with { Volume = 1f }, NPC.Center);
            }

            if (modNPC.targetNPC >= 0)
            {
                NPC.rotation = NPC.rotation.AngleTowards((Main.npc[modNPC.targetNPC].Center - NPC.Center).ToRotation(), 0.04f); 
            }
            else if (modNPC.targetPlayer >= 0)
            {
                NPC.rotation = NPC.rotation.AngleTowards((Main.player[modNPC.targetPlayer].Center - NPC.Center).ToRotation(), 0.04f);
            }
            else
            {
                NPC.rotation = NPC.rotation.AngleTowards(0f, 0.04f);
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)NPC.lifeMax * 50.0; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.WoodFurniture, hit.HitDirection, -1f);
                }
            }
            else
            {
                for (int i = 0; i < 20; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.WoodFurniture, 2.5f * (float)hit.HitDirection, -2.5f);
                }
                
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            texture = TextureAssets.Npc[Type].Value;
            int currentFrame = NPC.ai[0] >= -150 ? 1 : (NPC.ai[0] > 0 ? (int)(NPC.ai[0] / 5) + 2 : (int)((NPC.ai[0] + attackCooldown) / 5) + 3);
            int frameHeight = texture.Height / Main.npcFrameCount[modNPCID];
            SpriteEffects effects = Math.Abs(NPC.rotation) < MathHelper.PiOver2 ? SpriteEffects.None : SpriteEffects.FlipVertically;
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

                for (float j = 0; j < 1; j += 0.125f)
                {
                    spriteBatch.Draw(baseTex, NPC.Bottom + (-Vector2.UnitY * baseTex.Size().Y * 0.5f) - Main.screenPosition + (j * MathHelper.TwoPi).ToRotationVector2() * outlineThickness, null, drawColor, 0f, baseTex.Size() * 0.5f, NPC.scale, SpriteEffects.None, 0);
                    spriteBatch.Draw(texture, NPC.Center - Main.screenPosition + (-Vector2.UnitY * baseTex.Size().Y * 0.5f) + (j * MathHelper.TwoPi + NPC.rotation).ToRotationVector2() * outlineThickness, new Rectangle(0, frameHeight * currentFrame, texture.Width, frameHeight), drawColor, NPC.rotation, new Vector2(texture.Width * 0.5f, (frameHeight * 0.5f)), NPC.scale, effects, 0);
                }
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            Main.EntitySpriteDraw(baseTex, NPC.Bottom + (-Vector2.UnitY * baseTex.Size().Y * 0.5f) - Main.screenPosition, null, drawColor, 0f, baseTex.Size() * 0.5f, NPC.scale, SpriteEffects.None);
            Main.EntitySpriteDraw(texture, NPC.Center - Main.screenPosition + (-Vector2.UnitY * baseTex.Size().Y * 0.5f), new Rectangle(0, frameHeight * currentFrame, texture.Width, frameHeight), drawColor, NPC.rotation, new Vector2(texture.Width * 0.5f, (frameHeight * 0.5f)), NPC.scale, effects);

            return false;
        }
    }
}
