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
    public class FlowInvader : BaseRoguelikeNPC
    {
        public Texture2D lightTex;
        public override int modNPCID => ModContent.NPCType<FlowInvader>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Lunar"] };
        public override int CombatStyle => 1;
        public int attackTelegraph = 30;
        public int attackCooldown = 30;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 5;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 40;
            NPC.height = 60;
            NPC.aiStyle = -1;
            NPC.damage = 35;
            NPC.lifeMax = 1200;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            modNPC.drawCenter = new Vector2(0, -3);
            lightTex = TexDict["FlowInvaderGlow"];
            NPC.noGravity = true;
        }
        public override void AI()
        {
            modNPC.RogueFlyingShooterAI(NPC, 4f, 4f, 0.13f, 120f, 400f, attackTelegraph, attackCooldown, ModContent.ProjectileType<FlowSpawn>(), 12f, Vector2.Zero, NPC.damage, true);
            NPC.frameCounter += 0.18d;
            NPC.rotation = NPC.velocity.X * 0.06f;

            if (NPC.ai[2] == -attackCooldown)
            {
                SoundEngine.PlaySound(SoundID.NPCDeath7 with { Volume = 0.6f }, NPC.Center);
                for (int i = 0; i < 10; i++)
                {
                    Vector2 offset = Main.rand.NextVector2CircularEdge(8f, 8f);
                    Dust d = Dust.NewDustPerfect(NPC.Center + (offset * 0.5f), DustID.Clentaminator_Cyan, null, 0, default, 1f);
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
                for (int i = 0; (double)i < hit.Damage / 10.0; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 17, hit.HitDirection, -1f, 0, Color.Transparent, 0.75f);
                    if (Main.rand.NextBool())
                    {
                        Dust d = Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Clentaminator_Cyan)];
                        d.noGravity = true;
                    }
                }
            }
            else
            {
                for (int i = 0; i < 20; i++)
                {
                    if (!Main.rand.NextBool())
                    {
                        Dust.NewDust(NPC.position, NPC.width, NPC.height, 17, hit.HitDirection, -1f);
                    }
                    Dust d = Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Clentaminator_Cyan)];
                    d.noGravity = true;
                    d.velocity *= 3f;
                }
                Gore.NewGore(NPC.GetSource_Death(), NPC.Top, NPC.velocity * 0.8f, 778);
                Gore.NewGore(NPC.GetSource_Death(), NPC.Top, NPC.velocity * 0.8f, 779);
                Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity * 0.9f, 780);
                Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity * 0.9f, 781);
                Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity * 0.9f, 780);
                Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity * 0.9f, 781);
            }
        }
        public override void FindFrame(int frameHeight)
        {
            int currentFrame = (int)(NPC.frameCounter % (Main.npcFrameCount[modNPCID]));
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, TextureAssets.Npc[modNPCID].Value.Width, frameHeight);
        }
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Main.EntitySpriteDraw(lightTex, NPC.Center - Main.screenPosition + modNPC.drawCenter + (Vector2.UnitY * NPC.gfxOffY), NPC.frame, Color.White, NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
        }
        public override bool? CanFallThroughPlatforms()
        {
            return true;
        }
    }
}
