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

namespace TerRoguelike.NPCs.Enemy
{
    public class Corite : BaseRoguelikeNPC
    {
        public Texture2D lightTex = ModContent.Request<Texture2D>("TerRoguelike/NPCs/Enemy/CoriteGlow").Value;
        public override int modNPCID => ModContent.NPCType<Corite>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Lunar"] };
        public override int CombatStyle => 0;
        public int attackTelegraph = 30;
        public int dashTime = 40;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 8;
            NPCID.Sets.TrailCacheLength[modNPCID] = 10;
            NPCID.Sets.TrailingMode[modNPCID] = 3;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 38;
            NPC.height = 38;
            NPC.aiStyle = -1;
            NPC.damage = 36;
            NPC.lifeMax = 800;
            NPC.HitSound = SoundID.NPCHit3;
            NPC.DeathSound = SoundID.NPCDeath3;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            modNPC.drawCenter = new Vector2(0, -2);
        }
        public override void AI()
        {
            int attackCooldown = 5;
            NPC.frameCounter += 0.2d;
            modNPC.RogueFrostbiterAI(NPC, 240, dashTime, 10f, 0.2f, 7f, attackTelegraph, attackCooldown, 180f, ProjectileID.None, 1f, NPC.damage, 1, false, true, 100f);
            NPC.collideX = false;
            NPC.collideY = false;
            NPC.spriteDirection = Math.Sign(NPC.velocity.X);

            NPC.rotation = NPC.rotation.AngleTowards((NPC.velocity.X / 18f) * MathHelper.PiOver2, 0.1f);

            if (NPC.ai[0] > 0 && NPC.ai[0] < attackTelegraph - 5 && NPC.ai[1] == 0)
            {
                Vector2 offset = Main.rand.NextVector2CircularEdge(36f, 36f);
                Dust d = Dust.NewDustPerfect(NPC.Center + offset, DustID.SolarFlare, -offset * 0.06f + NPC.velocity, 0, default, 1f);
                d.noGravity = true;
                d.noLight = true;
                d.noLightEmittence = true;
            }
            else if (NPC.ai[0] == attackTelegraph && NPC.ai[1] == 0)
                SoundEngine.PlaySound(SoundID.DD2_WyvernDiveDown with { Volume = 1f, Pitch = -0.25f }, NPC.Center);
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)NPC.lifeMax * 20.0; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.MarblePot, hit.HitDirection, -1f);
                    if (Main.rand.NextBool())
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
                    Dust d = Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, 6)];
                    d.noGravity = true;
                    d.scale = 1.5f;
                    d.fadeIn = 1f;
                    d.velocity *= 3f;
                }
                Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity * 0.8f, 841);
                Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity * 0.8f, 842);
                Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity * 0.8f, 842);
                Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity * 0.9f, 843);
                Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity * 0.9f, 843);
            }
        }
        public override void FindFrame(int frameHeight)
        {
            int frameCount = Main.npcFrameCount[modNPCID];
            int currentFrame = (int)(NPC.frameCounter % (frameCount - 3));
            if (NPC.ai[1] == 0 && NPC.ai[0] >= attackTelegraph)
            {
                currentFrame = (int)MathHelper.Clamp(NPC.ai[0] < attackTelegraph + 20 ? ((NPC.ai[0] - attackTelegraph) * 6 / dashTime) + 4 : -((NPC.ai[0] - attackTelegraph - 20) * 6 / dashTime) + frameCount, frameCount - 4, frameCount - 1);
            }
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, TextureAssets.Npc[modNPCID].Value.Width, frameHeight);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Vector2 offset = new Vector2(NPC.width * 0.5f, NPC.height * 0.5f);
            Texture2D tex = TextureAssets.Npc[Type].Value;
            for (int i = 0; i < NPC.oldPos.Length; i++)
            {
                Vector2 drawPos = NPC.oldPos[i] + offset;
                Main.EntitySpriteDraw(tex, drawPos - Main.screenPosition + modNPC.drawCenter, NPC.frame, Color.Orange * MathHelper.Lerp(0.7f, 0, (float)i / NPC.oldPos.Length), NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale * 1.2f, NPC.spriteDirection > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            }
            return true;
        }
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Main.EntitySpriteDraw(lightTex, NPC.Center - Main.screenPosition + modNPC.drawCenter, NPC.frame, Color.White, NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
        }
        public override bool? CanFallThroughPlatforms()
        {
            return true;
        }
    }
}
