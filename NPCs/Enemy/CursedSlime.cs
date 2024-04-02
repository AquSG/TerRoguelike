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
using Microsoft.Xna.Framework.Graphics.PackedVector;
using TerRoguelike.Projectiles;
using Terraria.DataStructures;
using TerRoguelike.Managers;
using TerRoguelike.Particles;

namespace TerRoguelike.NPCs.Enemy
{
    public class CursedSlime : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<CursedSlime>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Corrupt"] };
        public override int CombatStyle => 1;
        public int attackCooldown = 190;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 2;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 28;
            NPC.height = 18;
            NPC.aiStyle = -1;
            NPC.damage = 28;
            NPC.lifeMax = 600;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            modNPC.drawCenter = new Vector2(0, 0);
            NPC.alpha = 60;
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.ai[1] = -attackCooldown + 1;
        }
        public override void AI()
        {
            if (Main.rand.NextBool(5))
            {
                int d = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.CursedTorch, 0, 0, NPC.alpha, Color.LimeGreen, 1.5f);
                Dust dust = Main.dust[d];
                dust.noGravity = true;
                dust.noLightEmittence = true;
                dust.noLight = true;
            }

            int attackTelegraph = 60;
            modNPC.RogueCrawlerShooterAI(NPC, 1.2f, 0.04f, 90, 320f, attackTelegraph, 31, 30, attackCooldown, ModContent.ProjectileType<CursedFlame>(), 9f, NPC.damage);
            if (NPC.ai[0] == 0)
            {
                NPC.frameCounter += 0.1d;
            }
            else
            {
                NPC.frameCounter += 0.025d;
            }

            if (NPC.ai[1] > 0 && NPC.ai[1] < attackTelegraph)
            {
                Vector2 offset = (Main.rand.Next(15, 21) * Vector2.UnitX.RotatedBy(Main.rand.NextFloat(MathHelper.TwoPi)));
                Dust d = Dust.NewDustPerfect(NPC.Center + offset + NPC.velocity, DustID.CursedTorch, -offset.SafeNormalize(Vector2.UnitX) + NPC.velocity, NPC.alpha, default(Color), 1.5f);
                d.noGravity = true;
                d.noLight = true;
                d.noLightEmittence = true;
                ParticleManager.AddParticle(new Square(NPC.Center + offset + NPC.velocity, -offset.SafeNormalize(Vector2.UnitX) + NPC.velocity, 20, Color.Lerp(Color.LimeGreen, Color.Yellow, Main.rand.NextFloat(0.75f)), new Vector2(Main.rand.NextFloat(0.9f, 1f)), Main.rand.NextFloat(MathHelper.TwoPi), 0.96f, 20, false));
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)NPC.lifeMax * 50.0; i++)
                {
                    int d = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.CursedTorch, hit.HitDirection, -1f, NPC.alpha, Color.LimeGreen);
                    Dust dust = Main.dust[d];
                    dust.noLightEmittence = true;
                    dust.noLight = true;
                }
            }
            else
            {
                for (int i = 0; i < 50; i++)
                {
                    int d = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.CursedTorch, 2 * hit.HitDirection, -2f, NPC.alpha, Color.LimeGreen);
                    Dust dust = Main.dust[d];
                    dust.noLightEmittence = true;
                    dust.noLight = true;
                }
            }
        }
        public override void FindFrame(int frameHeight)
        {
            int currentFrame = (int)(NPC.frameCounter % (Main.npcFrameCount[modNPCID]));
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, TextureAssets.Npc[modNPCID].Value.Width, frameHeight);
        }
        public override Color? GetAlpha(Color drawColor)
        {
            return Color.White;
        }
    }
}
