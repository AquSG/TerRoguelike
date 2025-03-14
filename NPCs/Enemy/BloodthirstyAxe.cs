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
using Terraria.DataStructures;
using static TerRoguelike.Utilities.TerRoguelikeUtils;

namespace TerRoguelike.NPCs.Enemy
{
    public class BloodthirstyAxe : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<BloodthirstyAxe>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Crimson"] };
        public override int CombatStyle => 2;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 5;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 30;
            NPC.height = 30;
            NPC.aiStyle = -1;
            NPC.damage = 27;
            NPC.lifeMax = 600;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.NPCDeath6;
            NPC.knockBackResist = 0f;
            modNPC.drawCenter = new Vector2(0, -14f);
            NPC.noGravity = true;
            NPC.noTileCollide = true;
        }
        public override void AI()
        {
            int attackTelegraph = 120;
            modNPC.RogueEvilToolAI(NPC, 8f, attackTelegraph, 60, 12, 120, ModContent.ProjectileType<BloodOrb>(), 6f, NPC.damage);

            NPC.frameCounter += 0.1d;
            if (NPC.ai[0] >= 0 && NPC.ai[0] < attackTelegraph)
            {
                NPC.rotation += (((NPC.ai[0] * 0.66f) / attackTelegraph) + 0.33f) * 0.2f * NPC.direction;
                NPC.rotation += MathHelper.PiOver4 * NPC.spriteDirection - MathHelper.PiOver2;
            }
            else if (NPC.ai[0] == attackTelegraph + 1)
            {
                NPC.rotation = NPC.velocity.ToRotation();
            }
            if (!(NPC.ai[0] > attackTelegraph + 1 || NPC.ai[0] < 0))
            {
                NPC.rotation -= MathHelper.PiOver4 * NPC.spriteDirection - MathHelper.PiOver2;
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)NPC.lifeMax * 50.0; i++)
                {
                    int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.CrimsonTorch, 0f, 0f, 0, default(Color), 1.5f);
                    Dust d = Main.dust[dust];
                    //d.noGravity = true;
                    d.noLight = true;
                    d.noLightEmittence = true;
                    d.velocity *= 2;
                }
            }
            else
            {
                for (int i = 0; i < 20; i++)
                {
                    int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, 31, 0f, 0f, 0, default(Color), 1.5f);
                    Dust d = Main.dust[dust];
                    d.velocity *= 2f;
                    d.noGravity = true;
                }
                for (int j = 0; j < 3; j++)
                {
                    int gore = Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + (float)(NPC.height / 2) - 10f), new Vector2((float)Main.rand.Next(-2, 3), (float)Main.rand.Next(-2, 3)), 61, NPC.scale);
                    Main.gore[gore].velocity *= 0.5f;
                }
            }
            
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }
        public override bool? CanFallThroughPlatforms()
        {
            return true;
        }
        public override void FindFrame(int frameHeight)
        {
            NPC.gfxOffY = 16;
            int currentFrame = (int)(NPC.frameCounter % Main.npcFrameCount[Type]);
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, NpcTexWidth(Type), frameHeight - 1);
        }
        public override Color? GetAlpha(Color drawColor)
        {
            return Color.Lerp(Color.Pink, drawColor, 0.5f);
        }
    }
}
