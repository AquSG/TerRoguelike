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
using static TerRoguelike.Managers.TextureManager;
using TerRoguelike.Projectiles;
using Terraria.Audio;

namespace TerRoguelike.NPCs.Enemy
{
    public class StarSpewer : BaseRoguelikeNPC
    {
        public Texture2D lightTex;
        public override int modNPCID => ModContent.NPCType<StarSpewer>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Lunar"] };
        public override int CombatStyle => 1;
        public int attackTelegraph = 20;
        public int attackExtend = 20;
        public int attackCooldown = 90;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 11;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 28;
            NPC.height = 32;
            NPC.aiStyle = -1;
            NPC.damage = 35;
            NPC.lifeMax = 800;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            modNPC.drawCenter = new Vector2(0, 1);
            lightTex = TexDict["StarSpewerGlow"];
            NPC.lavaImmune = true;
        }
        public override void AI()
        {
            modNPC.RogueCrawlerShooterAI(NPC, 1.3f, 0.04f, 20, 500f, attackTelegraph, attackExtend, attackExtend + 1, attackCooldown, ModContent.ProjectileType<SeekingStarCell>(), 5f, NPC.damage, 0f, -MathHelper.PiOver2);
            NPC.frameCounter += Math.Abs(NPC.velocity.X) * 0.1d;

            if (NPC.ai[0] == 0)
            {
                NPC.frameCounter += 0.1d;
            }
            if (NPC.ai[1] == attackTelegraph)
            {
                SoundEngine.PlaySound(SoundID.Item114 with { Volume = 1f, Pitch = 0f }, NPC.Center);
                for (int i = 0; i < 10; i++)
                {
                    Vector2 offset = Main.rand.NextVector2CircularEdge(8f, 8f);
                    Dust d = Dust.NewDustPerfect(NPC.Center + (offset * 0.5f), DustID.Clentaminator_Cyan, offset * 0.15f + (-Vector2.UnitY), 0, default, 1f);
                    d.noGravity = true;
                    d.noLight = true;
                    d.noLightEmittence = true;
                }
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)NPC.lifeMax * 50.0; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 17, hit.HitDirection, -1f);
                    if (Main.rand.NextBool(4))
                    {
                        Dust d = Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, 229)];
                        d.noGravity = true;
                    }
                }
            }
            else
            {
                for (int i = 0; i < 20; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 17, hit.HitDirection, -1f);
                    Dust d = Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Clentaminator_Cyan)];
                    d.noGravity = true;
                    d.velocity *= 3f;
                }
                Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity * 0.8f, 775);
                Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity * 0.8f, 776);
                Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity * 0.9f, 777);
            }
        }
        public override void FindFrame(int frameHeight)
        {
            int frameCount = Main.npcFrameCount[modNPCID];
            int currentFrame = (int)(NPC.frameCounter % (frameCount - 3)) + 1;
            if (NPC.ai[1] > 0 && NPC.ai[1] < attackTelegraph + attackExtend)
            {
                int selector = ((int)NPC.ai[1] / 8) % 5;
                if (selector == 2)
                    currentFrame = frameCount - 1;
                else if (selector % 2 == 1)
                    currentFrame = frameCount - 2;
                else
                    currentFrame = 0;
                NPC.frameCounter = 0;
            }
            else if (Math.Abs(NPC.velocity.X) < 0.5f)
            {
                currentFrame = 0;
                NPC.frameCounter = 0;
            }

            NPC.frame = new Rectangle(0, currentFrame * frameHeight, TextureAssets.Npc[modNPCID].Value.Width, frameHeight);
        }
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Main.EntitySpriteDraw(lightTex, NPC.Center - Main.screenPosition + modNPC.drawCenter + (Vector2.UnitY * NPC.gfxOffY), NPC.frame, Color.White, NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
        }
    }
}
