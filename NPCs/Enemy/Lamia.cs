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
using Terraria.GameContent.Animations;
using Terraria.Graphics.Shaders;
using TerRoguelike.Projectiles;

namespace TerRoguelike.NPCs.Enemy
{
    public class Lamia : BaseRoguelikeNPC
    {
        public List<int> offsetFrames = new List<int>() { 3, 4, 5, 10, 11, 12 };
        public override int modNPCID => ModContent.NPCType<Lamia>();
        public override List<int> associatedFloors => new List<int>() { 5 };
        public override int CombatStyle => 2;
        public int attackTelegraph = 30;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 10;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 18;
            NPC.height = 40;
            NPC.aiStyle = -1;
            NPC.damage = 36;
            NPC.lifeMax = 600;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.3f;
            modNPC.drawCenter = new Vector2(0, -7);
        }
        public override void AI()
        {
            NPC.frameCounter += NPC.velocity.Length() * 0.1d;
            modNPC.RogueFighterShooterAI(NPC, 2f, -7.9f, 320f, attackTelegraph, 60, 0f, ModContent.ProjectileType<DesertSpiritCurse>(), 2f, new Vector2(16 * NPC.direction, 0), NPC.damage, false, false);
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            NPC.ai[0] = 0;
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)NPC.lifeMax * 20.0; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 5, hit.HitDirection, -1f);
                }
            }
            else
            {
                for (int i = 0; i < 20; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 5, hit.HitDirection, -1f);
                }
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 876, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 877, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 877, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, 878, NPC.scale);
            }
        }
        public override void FindFrame(int frameHeight)
        {
            int currentFrame = NPC.ai[1] > 0 ? 9 : (NPC.velocity.Y == 0 ? (int)(NPC.frameCounter % (Main.npcFrameCount[modNPCID] - 2)) : 8);
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, TextureAssets.Npc[modNPCID].Value.Width, frameHeight);
        }
    }
}
