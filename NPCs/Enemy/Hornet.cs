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
    public class Hornet : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<Hornet>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Jungle"] };
        public override int CombatStyle => 1;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 3;
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
            NPC.knockBackResist = 0.8f;
            modNPC.drawCenter = new Vector2(0, 0);
            NPC.noGravity = true;
        }
        public override void AI()
        {
            int attackTelegraph = 45;
            int attackCooldown = 15;
            NPC.frameCounter += 0.25d;
            NPC.rotation = MathHelper.PiOver2 * NPC.velocity.Length() * 0.02f * NPC.direction;
            modNPC.RogueFlyingShooterAI(NPC, 7f, 5.5f, 0.12f, 128f, 320f, attackTelegraph, attackCooldown, ModContent.ProjectileType<Stinger>(), 8f, new Vector2(12 * NPC.direction, 14).RotatedBy(NPC.rotation), NPC.damage, true);
            if (NPC.ai[2] == -attackCooldown)
            {
                SoundEngine.PlaySound(SoundID.Item17 with { Volume = 1f }, NPC.Center);
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)NPC.lifeMax * 100.0; i++)
{
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 18, hit.HitDirection, -1f, NPC.alpha, NPC.color, NPC.scale);
                }
            }
            else
            {
                for (int i = 0; i < 50; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 18, hit.HitDirection, -2f, NPC.alpha, NPC.color, NPC.scale);
                }
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 70, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 71, NPC.scale);
            }
        }
        public override void FindFrame(int frameHeight)
        {
            int currentFrame = (int)(NPC.frameCounter % Main.npcFrameCount[modNPCID]);
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, TextureAssets.Npc[modNPCID].Value.Width, frameHeight - 1);
        }
    }
}
