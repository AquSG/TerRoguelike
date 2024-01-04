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
using TerRoguelike.Projectiles;
using TerRoguelike.NPCs;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;

namespace TerRoguelike.NPCs.Enemy
{
    public class Diabolist : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<Diabolist>();
        public override List<int> associatedFloors => new List<int>() { 8 };
        public override int CombatStyle => 1;
        public int attackTelegraph = 60;
        public int attackCooldown = 120;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 2;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 18;
            NPC.height = 40;
            NPC.aiStyle = -1;
            NPC.damage = 30;
            NPC.lifeMax = 800;
            NPC.HitSound = SoundID.NPCHit2;
            NPC.DeathSound = SoundID.NPCDeath2;
            NPC.knockBackResist = 0.5f;
            modNPC.drawCenter = new Vector2(0, -4);
        }
        public override void AI()
        {
            modNPC.RogueTeleportingShooterAI(NPC, 96f, 240f, 360, attackTelegraph, attackCooldown, ModContent.ProjectileType<FireBlast>(), 6f, new Vector2(0, -20), NPC.damage, true, true);

            NPC.velocity.X *= 0.8f;

            if (NPC.ai[0] == 0)
            {
                SoundEngine.PlaySound(SoundID.Item8 with { Volume = 0.5f }, NPC.Center);
                for (int i = 0; i < 50; i++)
                {
                    int d = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, 174, 0f, 0f, 100, default(Color), 1.2f);
                    Dust dust = Main.dust[d];
                    dust.noGravity = true;
                    dust.velocity *= 2f;
                }
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
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
            
        public override Color? GetAlpha(Color drawColor)
        {
            //return Color.Lerp(drawColor, Color.White, 0.5f);
            return drawColor;
        }
        public override void FindFrame(int frameHeight)
        {
            int currentFrame = NPC.ai[0] % (attackCooldown + attackTelegraph) <= attackTelegraph ? 1 : 0;
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, TextureAssets.Npc[modNPCID].Value.Width, frameHeight);
        }
    }
}
