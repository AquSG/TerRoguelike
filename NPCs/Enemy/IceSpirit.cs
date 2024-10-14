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
using static TerRoguelike.Schematics.SchematicManager;

namespace TerRoguelike.NPCs.Enemy
{
    public class IceSpirit : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<IceSpirit>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Snow"] };
        public override int CombatStyle => 1;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 4;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 18;
            NPC.height = 40;
            NPC.aiStyle = -1;
            NPC.damage = 30;
            NPC.lifeMax = 600;
            NPC.HitSound = SoundID.NPCHit5;
            NPC.DeathSound = SoundID.NPCDeath7;
            NPC.knockBackResist = 0.6f;
            NPC.noGravity = true;
            modNPC.drawCenter = new Vector2(0, -3);
        }
        public override void AI()
        {
            NPC.frameCounter += 0.2d;
            modNPC.RogueFlyingShooterAI(NPC, 2f, 2f, 0.08f, 96f, 320f, 30, 60, ProjectileID.IceBolt, 4f, Vector2.Zero, NPC.damage, true);
            float direction = (NPC.velocity.X / 18f) * MathHelper.Pi;
            if (NPC.ai[2] == 124912491924)
            {
                if (modNPC.targetPlayer != -1)
                {
                    direction = (Main.player[modNPC.targetPlayer].Center - NPC.Center).ToRotation();
                }
                else if (modNPC.targetNPC != -1)
                {
                    direction = (Main.npc[modNPC.targetNPC].Center - NPC.Center).ToRotation();
                }
            }
            
            NPC.rotation = NPC.rotation.AngleTowards(direction, 0.1f);

        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)NPC.lifeMax * 50.0; i++)
                {
                    int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, 92, 0f, 0f, 0, default(Color), 1.5f);
                    Dust d = Main.dust[dust];
                    d.velocity *= 1.5f;
                    d.noGravity = true;
                }
            }
            else
            {
                for (int i = 0; i < 100; i++)
                {
                    int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, 92, 0f, 0f, 0, default(Color), 1.5f);
                    Dust d = Main.dust[dust];
                    d.scale = 1.5f;
                    d.velocity *= 2f;
                    d.noGravity = true;
                }
            }
        }
        public override void FindFrame(int frameHeight)
        {
            int currentFrame = (int)(NPC.frameCounter % Main.npcFrameCount[modNPCID]);
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, TextureAssets.Npc[modNPCID].Value.Width, frameHeight - 1);
        }
        public override bool? CanFallThroughPlatforms()
        {
            return true;
        }
        public override Color? GetAlpha(Color drawColor)
        {
            return Color.LightBlue;
        }
    }
}
