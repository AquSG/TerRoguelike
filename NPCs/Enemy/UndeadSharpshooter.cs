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
using TerRoguelike.Projectiles;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using static TerRoguelike.Schematics.SchematicManager;
using static TerRoguelike.Managers.TextureManager;

namespace TerRoguelike.NPCs.Enemy
{
    public class UndeadSharpshooter : BaseRoguelikeNPC
    {
        public Texture2D gunTex;
        public Texture2D telegraphTex;
        public int currentFrame = 0;
        public override int modNPCID => ModContent.NPCType<UndeadSharpshooter>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Dungeon"] };
        public override int CombatStyle => 2;

        public Vector2 bulletPos = Vector2.Zero;
        public float gunRot = 0;
        public SpriteEffects spriteEffects = SpriteEffects.None;
        public int attackTelegraph = 180;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 17;
            NPCID.Sets.MustAlwaysDraw[Type] = true;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 18;
            NPC.height = 40;
            NPC.aiStyle = -1;
            NPC.damage = 25;
            NPC.lifeMax = 600;
            NPC.HitSound = SoundID.NPCHit2;
            NPC.DeathSound = SoundID.NPCDeath2;
            NPC.knockBackResist = 0.2f;
            modNPC.drawCenter = new Vector2(0, -6);
            gunTex = TexDict["UndeadSharpshooterGun"];
            telegraphTex = TexDict["LineGradient"];
        }
        public override void AI()
        {
            gunRot = NPC.spriteDirection == -1 ? 0f : MathHelper.Pi;

            if (modNPC.targetNPC != -1 && NPC.ai[0] >= 0)
            {
                NPC npc = Main.npc[modNPC.targetNPC];
                gunRot = (NPC.Center - npc.Center).ToRotation();
            }
            else if (modNPC.targetPlayer != -1 && NPC.ai[0] >= 0)
            {
                Player player = Main.player[modNPC.targetPlayer];
                gunRot = (NPC.Center - player.Center).ToRotation();
            }

            int attackCooldown = 120;
            NPC.frameCounter += NPC.velocity.Length() * 0.2d;
            bulletPos = new Vector2(-36, 2 * NPC.direction).RotatedBy(gunRot);
            modNPC.RogueFighterShooterAI(NPC, 1.5f, -7.9f, 720f, attackTelegraph, attackCooldown, 0f, ModContent.ProjectileType<SniperBullet>(), 2f, bulletPos, NPC.damage * 3, true, false, gunRot + MathHelper.Pi);
            if (NPC.ai[1] > 0 && NPC.ai[1] < attackTelegraph)
            {
                modNPC.DiscourageTargetting();
            }
            if (NPC.ai[1] == -attackCooldown)
            {
                SoundEngine.PlaySound(SoundID.Item40 with { Volume = 1f }, NPC.Center);
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            NPC.ai[0] = 0;
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)NPC.lifeMax * 50.0; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 26, hit.HitDirection, -1f);
                }
            }
            else
            {
                for (int i = 0; i < 20; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 26, 2.5f * (float)hit.HitDirection, -2.5f);
                }
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 42, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 43, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 43, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, 44, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, 44, NPC.scale);
            }
        }
        public override void FindFrame(int frameHeight)
        {
            currentFrame = NPC.velocity.Y == 0 ? (NPC.ai[1] > 0 ? 16 : (int)(NPC.frameCounter % (Main.npcFrameCount[Type] - 3)) + 2) : 1;
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, NpcTexWidth(Type), frameHeight);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.ai[1] > 1)
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                if ((NPC.ai[1] < attackTelegraph - 30 || NPC.ai[1] % 6 < 3) && !modNPC.hostileTurnedAlly)
                {
                    Vector2 anchorPos = bulletPos + NPC.Center;
                    Color startColor = Color.Red * 0.9f;
                    Vector2 origin = telegraphTex.Size() * 0.5f;
                    float rotation = gunRot + MathHelper.Pi;
                    Vector2 offsetPerLoop = 2 * Vector2.UnitX;
                    Vector2 endPoint = TileCollidePositionInLine(anchorPos, anchorPos + (Vector2.UnitX * 2000).RotatedBy(rotation), 2001f);
                    float length = (anchorPos - endPoint).Length();
                    int maxLoops = (int)(length * 0.5f);
                    for (int i = 0; i < maxLoops; i++)
                    {
                        Vector2 pos = anchorPos + (i * offsetPerLoop).RotatedBy(rotation);

                        float opacity = 1f;
                        if (i < 8)
                        {
                            opacity = MathHelper.Lerp(0.6f, 1f, i / 8f);
                        }
                        else if (i >= 900)
                        {
                            opacity = MathHelper.Lerp(1f, 0f, (i - 900) / 100f);
                        }
                        if (NPC.ai[1] < 120)
                        {
                            opacity *= MathHelper.Lerp(0f, 1f, NPC.ai[1] / 120);
                        }

                        Main.EntitySpriteDraw(telegraphTex, pos - Main.screenPosition, null, startColor * opacity, gunRot, origin, 1f, SpriteEffects.None, 0);
                    }
                
                }

                if (modNPC.ignitedStacks.Count > 0 && NPC.ai[1] > 0)
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
                        spriteBatch.Draw(gunTex, NPC.Center - Main.screenPosition + (j * MathHelper.TwoPi + gunRot).ToRotationVector2() * outlineThickness, null, drawColor, gunRot, new Vector2(gunTex.Size().X * 0.75f, gunTex.Size().Y * (spriteEffects == SpriteEffects.FlipVertically ? 0.35f : 0.65f)), NPC.scale, spriteEffects, 0f);
                    }
                }
                modNPC.EliteEffectSpritebatch(NPC, new());
            }
            
            return true;
        }
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            modNPC.EliteEffectSpritebatch(NPC, new(1, 1, gunTex.Size(), gunTex.Frame()));
            spriteEffects = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipVertically;
            if (currentFrame == 16)
                spriteBatch.Draw(gunTex, NPC.Center - Main.screenPosition, null, drawColor, gunRot, new Vector2(gunTex.Size().X * 0.75f, gunTex.Size().Y * (spriteEffects == SpriteEffects.FlipVertically ? 0.35f : 0.65f)), NPC.scale, spriteEffects, 0f);
        }
    }
}
