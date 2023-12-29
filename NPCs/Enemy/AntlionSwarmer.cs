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
using TerRoguelike.Projectiles;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;

namespace TerRoguelike.NPCs.Enemy
{
    public class AntlionSwarmer : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<AntlionSwarmer>();
        public override List<int> associatedFloors => new List<int>() { 5 };
        public override int CombatStyle => 1;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 9;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 40;
            NPC.height = 31;
            NPC.aiStyle = -1;
            NPC.damage = 36;
            NPC.lifeMax = 600;
            NPC.HitSound = SoundID.NPCHit32;
            NPC.DeathSound = SoundID.NPCDeath35;
            NPC.knockBackResist = 0.6f;
            modNPC.drawCenter = new Vector2(0, -9);
            NPC.noGravity = true;
        }
        public override void AI()
        {
            int attackTelegraph = 60;
            int attackCooldown = 30;
            NPC.frameCounter += 0.25d;
            NPC.rotation = MathHelper.PiOver2 * NPC.velocity.Length() * 0.02f * NPC.direction;
            modNPC.RogueAntlionSwarmerAI(NPC, 7f, 4f, 0.09f, 128f, 240f, attackTelegraph, attackCooldown, ModContent.ProjectileType<SandBlast>(), 15f, new Vector2(18 * NPC.direction, -6).RotatedBy(NPC.rotation), NPC.damage, true);
            if (NPC.ai[2] == -attackCooldown)
            {
                SoundEngine.PlaySound(SoundID.Item63 with { Volume = 0.9f }, NPC.Center);
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
			if (NPC.life > 0)
			{
				for (int i = 0; (double)i < hit.Damage / (double)NPC.lifeMax * (double)100f; i++)
				{
					Dust.NewDust(NPC.position, NPC.width, NPC.height, 250, hit.HitDirection, -1f);
				}
			}
            else
            {
                for (int i = 0; (float)i < 50f; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 250, 2 * hit.HitDirection, -2f);
                }
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 815);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 816);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 817);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 818);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 818);
            }
			
		}
        public override void FindFrame(int frameHeight)
        {
            int currentFrame = (int)(NPC.frameCounter % Main.npcFrameCount[modNPCID]);
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, TextureAssets.Npc[modNPCID].Value.Width, frameHeight);
        }
    }
}
