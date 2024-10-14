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
using Microsoft.Xna.Framework.Graphics.PackedVector;
using static TerRoguelike.Schematics.SchematicManager;

namespace TerRoguelike.NPCs.Enemy
{
    public class Demon : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<Demon>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Hell"] };
        public override int CombatStyle => 2;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 5;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 28;
            NPC.height = 48;
            NPC.aiStyle = -1;
            NPC.damage = 30;
            NPC.lifeMax = 600;
            NPC.HitSound = SoundID.NPCHit21;
            NPC.DeathSound = SoundID.NPCDeath24;
            NPC.knockBackResist = 0.8f;
            modNPC.drawCenter = new Vector2(0, -3);
            NPC.lavaImmune = true;
            NPC.noGravity = true;
        }
        public override void AI()
        {
            int attackTelegraph = 60;
            int attackTimeBetween = 18;
            NPC.frameCounter += 0.2d;
            NPC.rotation = MathHelper.Pi * NPC.velocity.X * 0.02f;
            modNPC.RogueDemonAI(NPC, 4f, 4f, 0.05f, true, 480f, attackTelegraph, 120, attackTimeBetween, 240, ProjectileID.DemonScythe, 0.1f, NPC.damage);
            if (NPC.ai[1] >= attackTelegraph)
            {
                if (((int)NPC.ai[1] - attackTelegraph) % attackTimeBetween == 0)
                {
                    SoundEngine.PlaySound(SoundID.Item8 with { Volume = 1f }, NPC.Center);
                }
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)NPC.lifeMax * 50; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 5, hit.HitDirection, -1f);
                }
            }
            else
            {
                for (int i = 0; i < 50; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 5, 2 * hit.HitDirection, -2f);
                }
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 93);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 94);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 94);
            }
            
        }
        public override void FindFrame(int frameHeight)
        {
            int currentFrame = (int)(NPC.frameCounter % Main.npcFrameCount[modNPCID]);
            NPC.frame = new Rectangle(0, currentFrame * frameHeight + 1, TextureAssets.Npc[modNPCID].Value.Width, frameHeight - 1);
        }
    }
}
