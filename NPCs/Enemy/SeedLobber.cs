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
using static TerRoguelike.Utilities.TerRoguelikeUtils;

namespace TerRoguelike.NPCs.Enemy
{
    public class SeedLobber : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<SeedLobber>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Forest"] };
        public override int CombatStyle => 2;
        public int attackTelegraph = 60;
        public int attackExtend = 60;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 10;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 18;
            NPC.height = 40;
            NPC.aiStyle = -1;
            NPC.damage = 24;
            NPC.lifeMax = 600;
            NPC.HitSound = SoundID.NPCHit7;
            NPC.DeathSound = SoundID.DD2_KoboldDeath;
            NPC.knockBackResist = 0.2f;
            modNPC.drawCenter = new Vector2(0, -8);
        }
        public override void AI()
        {
            modNPC.RogueFighterShooterAI(NPC, 2f, -8.1f, 400f, attackTelegraph, 170, 0f, ModContent.ProjectileType<GreenPetal>(), 8f, Vector2.Zero, NPC.damage, true, false, -MathHelper.PiOver2, attackExtend);
            if (NPC.ai[1] <= 0)
                NPC.frameCounter += NPC.velocity.Length() * 0.1d;
            else
                NPC.frameCounter = (int)NPC.ai[1] % 3 == 0 ? Main.rand.Next(3) : NPC.frameCounter;
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            NPC.ai[0] = 0;
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)NPC.lifeMax * 50.0; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.WoodFurniture, hit.HitDirection, -1f, 0, default(Color), 1.1f);
                }
            }
            else
            {
                for (int i = 0; i < 60; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.WoodFurniture, 2 * hit.HitDirection, -2f, 0, default(Color), 1.1f);
                }
            }
        }
        public override void FindFrame(int frameHeight)
        {
            int frameTotal = Main.npcFrameCount[Type];
            int currentFrame;
            if (NPC.ai[1] > attackTelegraph)
            {
                currentFrame = frameTotal - 1;
            }
            else if (NPC.ai[1] > 0)
            {
                if (NPC.frameCounter >= 3)
                    NPC.frameCounter = 0;
                currentFrame = frameTotal - 4 + (int)NPC.frameCounter;
            }
            else if (NPC.velocity.Y != 0)
            {
                currentFrame = 1;
            }
            else
            {
                currentFrame = (int)(NPC.frameCounter % (frameTotal - 6)) + 2;
            }
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, NpcTexWidth(Type), frameHeight);
        }
    }
}
