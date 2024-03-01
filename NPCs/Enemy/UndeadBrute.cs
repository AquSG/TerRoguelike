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

namespace TerRoguelike.NPCs.Enemy
{
    public class UndeadBrute : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<UndeadBrute>();
        public Texture2D ballTex = ModContent.Request<Texture2D>("TerRoguelike/Projectiles/SpikedBall").Value;
        public Texture2D chainTex = ModContent.Request<Texture2D>("TerRoguelike/Projectiles/SpikedBallChain").Value;
        public Texture2D armTex = ModContent.Request<Texture2D>("TerRoguelike/NPCs/Enemy/UndeadBruteArm").Value;
        public override List<int> associatedFloors => new List<int>() { FloorDict["Dungeon"] };
        public override int CombatStyle => 2;
        public BallAndChain ball;
        public int attackWindup = 90;
        public int attackExhaust = 120;
        public int attackCooldown = 120;
        public bool CollisionPass;
        Vector2 rotationCenter;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 16;
            NPCID.Sets.MustAlwaysDraw[modNPCID] = true;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 18;
            NPC.height = 40;
            NPC.aiStyle = -1;
            NPC.damage = 40;
            NPC.lifeMax = 900;
            NPC.HitSound = SoundID.NPCHit2;
            NPC.DeathSound = SoundID.NPCDeath2;
            NPC.knockBackResist = 0.2f;
            modNPC.drawCenter = new Vector2(0, -4);
            ball = new BallAndChain(NPC.Center - (Vector2.UnitX * 32), 20, 20, 0);
        }
        public override void AI()
        {
            NPC.frameCounter += NPC.velocity.Length() * 0.2d;

            modNPC.RogueBallAndChainThrowerAI(NPC, 2f, -7.9f, 0.07f, attackWindup, attackExhaust, attackCooldown, ref ball, 8f, 48f, NPC.damage, 160f);

            if (NPC.ai[1] > attackCooldown && NPC.ai[1] <= attackWindup + attackCooldown + 1 && ((NPC.ai[1] - attackCooldown) % 0.66f < 0.02f) || NPC.ai[1] == attackWindup + attackCooldown + 1)
            {
                SoundEngine.PlaySound(new SoundStyle("TerRoguelike/Sounds/chainHit") with { Volume = 0.8f }, NPC.Center);
                SoundEngine.PlaySound(SoundID.Item1 with { Volume = 0.7f, Pitch = -0.8f}, NPC.Center);
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
            int currentFrame = NPC.ai[1] > attackCooldown ? 15 : (NPC.velocity.Y != 0 ? 0 : (int)(NPC.frameCounter % (Main.npcFrameCount[modNPCID] - 2) + 1));
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, TextureAssets.Npc[modNPCID].Value.Width, frameHeight);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Vector2 chainVect = ball.Center - NPC.Center;
            if (NPC.ai[1] > attackCooldown && NPC.ai[1] <= attackCooldown + attackWindup)
            {
                int chainLength = (int)(chainVect.Length() / 10);
                float posOffset = chainVect.Length() % 10f;
                float rot = chainVect.ToRotation();
                for (int i = 0; i < chainLength; i++)
                {
                    Vector2 pos = ((Vector2.UnitX * 10 * i) + (Vector2.UnitX * posOffset)).RotatedBy(rot) + NPC.Center;
                    Color color = Lighting.GetColor(new Point((int)(pos.X / 16), (int)(pos.Y / 16)));
                    spriteBatch.Draw(chainTex, pos - Main.screenPosition, null, color, rot + MathHelper.PiOver2, new Vector2(chainTex.Size().X * 0.5f, 0), 1f, SpriteEffects.None, 0);
                }

                spriteBatch.Draw(ballTex, ball.Center - Main.screenPosition, null, Lighting.GetColor(new Point((int)(ball.Center.X / 16), (int)(ball.Center.Y / 16))), ball.Rotation, ballTex.Size() * 0.5f, 1f, SpriteEffects.None, 0);
            }
            if (NPC.ai[1] > attackCooldown)
            {
                Vector2 armPos = NPC.Center + (new Vector2(6 * NPC.spriteDirection, -2) * NPC.scale);
                float armRot = NPC.ai[1] <= attackCooldown + attackWindup ? chainVect.ToRotation() : (Main.projectile[(int)NPC.ai[3]].Center - NPC.Center).ToRotation();
                SpriteEffects spriteEffects = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                float realArmRot = armRot - MathHelper.PiOver4 * 0.9f * NPC.spriteDirection + (NPC.spriteDirection == -1 ? MathHelper.Pi : 0);

                if (modNPC.ignitedStacks.Any() && NPC.ai[1] > 0)
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
                        spriteBatch.Draw(armTex, armPos - Main.screenPosition + (j * MathHelper.TwoPi + realArmRot).ToRotationVector2() * outlineThickness, null, drawColor, realArmRot, new Vector2(armTex.Size().X * 0.5f, armTex.Size().Y * 0.3f), NPC.scale, spriteEffects, 0f);
                    }
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
                spriteBatch.Draw(armTex, armPos - Main.screenPosition, null, drawColor, realArmRot, new Vector2(armTex.Size().X * 0.5f, armTex.Size().Y * 0.3f), NPC.scale, spriteEffects, 0);
            }

            return true;
        }
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            if (NPC.getRect().Intersects(target.getRect()))
            {
                CollisionPass = true;
                return true;
            }    
            if (NPC.ai[1] > attackCooldown && NPC.ai[1] <= attackCooldown + attackWindup && target.getRect().Intersects(new Rectangle((int)ball.Position.X, (int)ball.Position.Y, ball.Width, ball.Height)))
            {
                CollisionPass = true;
                return true;
            }
                
            CollisionPass = false;
            return false;
        }
        public override bool CanHitNPC(NPC target)
        {
            if (NPC.getRect().Intersects(target.getRect()))
            {
                CollisionPass = true;
                return true;
            }
            if (NPC.ai[1] > attackCooldown && NPC.ai[1] <= attackCooldown + attackWindup && target.getRect().Intersects(new Rectangle((int)ball.Position.X, (int)ball.Position.Y, ball.Width, ball.Height)))
            {
                CollisionPass = true;
                return true;
            }

            CollisionPass = false;
            return false;
        }
        public override bool ModifyCollisionData(Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox)
        {
            if (CollisionPass)
            {
                npcHitbox = new Rectangle(0, 0, Main.maxTilesX * 16, Main.maxTilesY * 16);
            }
            return CollisionPass;
        }
    }
}
