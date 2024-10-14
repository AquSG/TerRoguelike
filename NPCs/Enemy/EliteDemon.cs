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
    public class EliteDemon : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<EliteDemon>();
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
            NPC.damage = 35;
            NPC.lifeMax = 600;
            NPC.HitSound = SoundID.NPCHit21;
            NPC.DeathSound = SoundID.NPCDeath24;
            NPC.knockBackResist = 0.8f;
            modNPC.drawCenter = new Vector2(0, -22);
            NPC.lavaImmune = true;
            NPC.noGravity = true;
        }
        public override void AI()
        {
            NPC.frameCounter += 0.2d;
            NPC.rotation = MathHelper.Pi * NPC.velocity.X * 0.02f;
            modNPC.RogueDemonAI(NPC, 4.2f, 4.2f, 0.053f, true, 480f, 60, 100, 15, 240, ProjectileID.UnholyTridentFriendly, 5f, NPC.damage);
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)NPC.lifeMax * 100.0; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 5, hit.HitDirection, -1f);
                }
                return;
            }
            for (int i = 0; i < 50; i++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, 5, 2 * hit.HitDirection, -2f);
            }
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 184);
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 185);
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 185);
        }
        public override void FindFrame(int frameHeight)
        {
            NPC.gfxOffY = 14;
            int currentFrame = (int)(NPC.frameCounter % Main.npcFrameCount[modNPCID]);
            NPC.frame = new Rectangle(0, currentFrame * frameHeight + 1, TextureAssets.Npc[modNPCID].Value.Width, frameHeight - 1);
        }
    }
}
