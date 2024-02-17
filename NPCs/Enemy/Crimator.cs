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

namespace TerRoguelike.NPCs.Enemy
{
    public class Crimator : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<Crimator>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Crimson"] };
        public override int CombatStyle => 2;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 2;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 30;
            NPC.height = 30;
            NPC.aiStyle = -1;
            NPC.damage = 30;
            NPC.lifeMax = 600;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.5f;
            modNPC.drawCenter = new Vector2(0, -2);
            NPC.noGravity = true;
        }
        public override void AI()
        {
            modNPC.RogueCorruptorAI(NPC, 4.5f, 0.035f, 30, 135, ModContent.ProjectileType<BloodClot>(), 8f, NPC.damage);

            NPC.frameCounter += 0.1d;
            float direction = NPC.velocity.ToRotation();
            if (modNPC.targetNPC != -1)
            {
                direction = (Main.npc[modNPC.targetNPC].Center - NPC.Center).ToRotation();
            }
            else if (modNPC.targetPlayer != -1)
            {
                direction = (Main.player[modNPC.targetPlayer].Center - NPC.Center).ToRotation();
            }
            NPC.rotation = NPC.rotation.AngleLerp(direction, 0.1f);
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)NPC.lifeMax * 100.0; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 5, hit.HitDirection, -1f, NPC.alpha);
                }
            }
            else
            {
                for (int i = 0; i < 50; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 5, 2 * hit.HitDirection, -2f, NPC.alpha);
                }
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 223);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 224);
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
            int currentFrame = (int)(NPC.frameCounter % Main.npcFrameCount[modNPCID]);
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, TextureAssets.Npc[modNPCID].Value.Width, frameHeight);
        }
    }
}
