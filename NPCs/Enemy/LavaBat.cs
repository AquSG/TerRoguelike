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
    public class LavaBat : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<LavaBat>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Hell"] };
        public override int CombatStyle => 0;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 4;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 22;
            NPC.height = 22;
            NPC.aiStyle = -1;
            NPC.damage = 30;
            NPC.lifeMax = 500;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath4;
            NPC.knockBackResist = 0.8f;
            modNPC.drawCenter = new Vector2(0, -9);
            NPC.noGravity = true;
        }
        public override void AI()
        {
            NPC.frameCounter += 0.18d;
            NPC.rotation = MathHelper.Pi * NPC.velocity.Length() * 0.02f * NPC.direction;
            modNPC.RogueFlierAI(NPC, 4f, 4f, 0.1f, true);
            if (Main.rand.NextBool())
            {
                int d = Dust.NewDust(NPC.position, NPC.width, NPC.height, 6, 0, 0, NPC.alpha, default(Color), 1.2f);
                Main.dust[d].noGravity = true;
            }
                
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)NPC.lifeMax * 50.0; i++)
                {
                    int d = Dust.NewDust(NPC.position, NPC.width, NPC.height, 6, hit.HitDirection * 2, -1f, NPC.alpha, default(Color), 1.5f);
                    if (!Main.rand.NextBool(8))
                    {
                        Main.dust[d].noGravity = true;
                    }
                }
            }
            else
            {
                for (int i = 0; i < 40; i++)
                {
                    int d = Dust.NewDust(NPC.position, NPC.width, NPC.height, 6, hit.HitDirection * 2, -1f, NPC.alpha, default(Color), 1.5f);
                    if (!Main.rand.NextBool(8))
                    {
                        Main.dust[d].noGravity = true;
                    }
                }
            }
            
        }
        public override void FindFrame(int frameHeight)
        {
            NPC.gfxOffY = 4;
            int currentFrame = (int)(NPC.frameCounter % Main.npcFrameCount[modNPCID]);
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, TextureAssets.Npc[modNPCID].Value.Width, frameHeight);
        }
        public override Color? GetAlpha(Color drawColor)
        {
            return Color.White;
        }
    }
}
