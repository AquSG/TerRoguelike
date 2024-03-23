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
using TerRoguelike.Projectiles;
using static TerRoguelike.Schematics.SchematicManager;

namespace TerRoguelike.NPCs.Enemy
{
    public class OvergrownBat : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<OvergrownBat>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Jungle"] };
        public override int CombatStyle => 2;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 4;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 38;
            NPC.height = 34;
            NPC.aiStyle = -1;
            NPC.damage = 32;
            NPC.lifeMax = 800;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath4;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            modNPC.drawCenter = new Vector2(0, -2);
        }
        public override void AI()
        {
            int attackCooldown = 30;
            int attackTelegraph = 30;
            int dashTime = 40;
            NPC.frameCounter += 0.2d;
            modNPC.RogueFrostbiterAI(NPC, 240, dashTime, 8f, 0.15f, 7f, attackTelegraph, attackCooldown, 180f, ModContent.ProjectileType<SporeCloud>(), 5f, NPC.damage, 11, true, true);

            NPC.rotation = NPC.rotation.AngleTowards((NPC.velocity.X / 18f) * MathHelper.PiOver2, 0.1f);

            Vector2 target = Vector2.Zero;
            if (modNPC.targetPlayer != -1)
            {
                target = Main.player[modNPC.targetPlayer].Center;
            }
            else if (modNPC.targetNPC != -1)
            {
                target = Main.npc[modNPC.targetNPC].Center;
            }
            if (target != Vector2.Zero && NPC.ai[0] < attackTelegraph)
            {
                if (Collision.CanHit(NPC.Center, 1, 1, target, 1, 1))
                {
                    if (target.X > NPC.Center.X)
                    {
                        NPC.spriteDirection = 1;
                    }
                    else
                    {
                        NPC.spriteDirection = -1;
                    }
                }
                else
                {
                    NPC.spriteDirection = Math.Sign(NPC.velocity.X);
                }
            }
            if (NPC.ai[0] == -attackCooldown && NPC.ai[1] == 0)
            {
                SoundEngine.PlaySound(SoundID.Item76 with { Volume = 1f }, NPC.Center);
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)NPC.lifeMax * 30.0; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 5, hit.HitDirection, -1f);
                }
            }
            else
            {
                for (int i = 0; i < 15; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 5, 2 * hit.HitDirection, -2f);
                }
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 175);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 176);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 176);
            }
            
        }
        public override void FindFrame(int frameHeight)
        {
            int currentFrame = (int)(NPC.frameCounter % Main.npcFrameCount[modNPCID]);
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, TextureAssets.Npc[modNPCID].Value.Width, frameHeight);
        }
        public override bool? CanFallThroughPlatforms()
        {
            return true;
        }
    }
}
