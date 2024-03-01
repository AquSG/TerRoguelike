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
using Terraria.Audio;

namespace TerRoguelike.NPCs.Enemy
{
    public class IchorSlime : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<IchorSlime>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Crimson"] };
        public override int CombatStyle => 1;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 3;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 32;
            NPC.height = 20;
            NPC.aiStyle = -1;
            NPC.damage = 35;
            NPC.lifeMax = 600;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            modNPC.drawCenter = new Vector2(0, 0);
            NPC.alpha = 60;
        }
        public override void AI()
        {
            if (Main.rand.NextBool(5))
            {
                int d = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.IchorTorch, 0, 0, NPC.alpha, Color.Yellow, 1.5f);
                Dust dust = Main.dust[d];
                dust.noGravity = true;
                dust.noLightEmittence = true;
                dust.noLight = true;
            }
            int attackTelegraph = 60;
            int attackTimeBetween = 15;
            modNPC.RogueCrawlerShooterAI(NPC, 1.4f, 0.04f, 70, 320f, attackTelegraph, 44, attackTimeBetween, 240, ModContent.ProjectileType<IchorBlob>(), 6.5f, NPC.damage);
            if (NPC.ai[0] == 0)
            {
                NPC.frameCounter += 0.1d;
            }
            else
            {
                NPC.frameCounter += 0.06d;
            }

            if (NPC.ai[1] > 0 && NPC.ai[1] < attackTelegraph)
            {
                Vector2 offset = (Main.rand.Next(14, 19) * Vector2.UnitX.RotatedBy(Main.rand.NextFloat(MathHelper.TwoPi)));
                Dust d = Dust.NewDustPerfect(NPC.Center + offset + NPC.velocity, DustID.IchorTorch, -offset.SafeNormalize(Vector2.UnitX) + NPC.velocity, NPC.alpha, Color.Yellow, 1.5f);
                d.noGravity = true;
                d.noLight = true;
                d.noLightEmittence = true;
            }

            if (NPC.ai[1] >= attackTelegraph && ((int)NPC.ai[1] - attackTelegraph) % attackTimeBetween == 0)
            {
                SoundEngine.PlaySound(SoundID.Item17 with { Volume = 1f }, NPC.Center);
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)NPC.lifeMax * 50.0; i++)
{
                    int d = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.IchorTorch, hit.HitDirection, -1f, NPC.alpha, Color.Yellow);
                    Dust dust = Main.dust[d];
                    dust.noLightEmittence = true;
                    dust.noLight = true;
                }
            }
            else
            {
                for (int i = 0; i < 50; i++)
{
                    int d = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.IchorTorch, 2 * hit.HitDirection, -2f, NPC.alpha, Color.Yellow);
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
