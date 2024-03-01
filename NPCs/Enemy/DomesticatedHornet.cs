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
    public class DomesticatedHornet : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<DomesticatedHornet>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Temple"] };
        public override int CombatStyle => 1;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 2;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 34;
            NPC.height = 32;
            NPC.aiStyle = -1;
            NPC.damage = 24;
            NPC.lifeMax = 600;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.7f;
            modNPC.drawCenter = new Vector2(0, -4);
            NPC.noGravity = true;
        }
        public override void AI()
        {
            int attackTelegraph = 9;
            int attackCooldown = 9;
            int attackSuperCooldown = 120;
            NPC.frameCounter += 0.3d;
            NPC.rotation = MathHelper.PiOver2 * NPC.velocity.Length() * 0.02f * NPC.direction;
            modNPC.RogueFlyingShooterAI(NPC, 7f, 5.5f, 0.12f, 128f, 380f, attackTelegraph, attackCooldown, ModContent.ProjectileType<Stinger>(), 8f, new Vector2(-10 * NPC.direction, 12).RotatedBy(NPC.rotation), NPC.damage, true, 0.93f, attackSuperCooldown, 7);
            if (NPC.ai[2] == attackTelegraph -1)
            {
                SoundEngine.PlaySound(SoundID.Item17 with { Volume = 1f }, NPC.Center);
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int num840 = 0; (double)num840 < hit.Damage / (double)NPC.lifeMax * 60.0; num840++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 18, hit.HitDirection, -1f, NPC.alpha, NPC.color, NPC.scale);
                }
            }
            else
            {
                for (int num841 = 0; num841 < 50; num841++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 18, hit.HitDirection, -2f, NPC.alpha, NPC.color, NPC.scale);
                }
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 229, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 230, NPC.scale);
            }
            
        }
        public override void FindFrame(int frameHeight)
        {
            int currentFrame = (int)(NPC.frameCounter % Main.npcFrameCount[modNPCID]);
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, TextureAssets.Npc[modNPCID].Value.Width, frameHeight);
        }
    }
}
