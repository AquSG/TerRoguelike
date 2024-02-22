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
using static TerRoguelike.Schematics.SchematicManager;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using TerRoguelike.Projectiles;
using Terraria.Audio;

namespace TerRoguelike.NPCs.Enemy
{
    public class FireImp : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<FireImp>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Hell"] };
        public override int CombatStyle => 1;
        int attackTelegraph = 60;
        int attackCooldown = 90;
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
            NPC.damage = 35;
            NPC.lifeMax = 600;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.4f;
            NPC.lavaImmune = true;
            modNPC.drawCenter = new Vector2(0, -4);
            NPC.alpha = 60;
        }
        public override void AI()
        {
            modNPC.RogueTeleportingShooterAI(NPC, 96f, 240f, 315, attackTelegraph, attackCooldown, ModContent.ProjectileType<BouncingFire>(), 4f, new Vector2(12 * NPC.direction, -12), NPC.damage, true, true);
            NPC.frameCounter += 0.25d;

            NPC.velocity.X *= 0.8f;

            if (NPC.ai[0] == 0)
            {
                SoundEngine.PlaySound(SoundID.Item8 with { Volume = 0.5f }, NPC.Center);
                for (int i = 0; i < 50; i++)
                {
                    int d = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Torch, 0f, 0f, 100, default(Color), 2.5f);
                    Dust dust = Main.dust[d];
                    dust.velocity *= 3f;
                    dust.noGravity = true;
                    dust.noLight = true;
                    dust.noLightEmittence = true;
                }
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)NPC.lifeMax * 50; i++)
                {
                    int d = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, 6, NPC.velocity.X, NPC.velocity.Y, 100, default(Color), 2.5f);
                    Main.dust[d].noGravity = true;
                }
            }
            else
            {
                for (int i = 0; i < 50; i++)
                {
                    int d = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, 6, NPC.velocity.X, NPC.velocity.Y, 100, default(Color), 2.5f);
                    Dust dust = Main.dust[d];
                    dust.noGravity = true;
                    dust.velocity *= 2f;
                }
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 45);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 46);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 46);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, 47);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, 47);
            }
            
        }
        public override void FindFrame(int frameHeight)
        {
            int attackAnimTimer = (int)NPC.ai[0] % (attackTelegraph + attackCooldown);
            int currentFrame = attackAnimTimer >= attackTelegraph ? (int)(NPC.frameCounter % (Main.npcFrameCount[modNPCID] - 6)) : ((attackAnimTimer * 6 / attackTelegraph) % (Main.npcFrameCount[modNPCID] - 4)) + 4;
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, TextureAssets.Npc[modNPCID].Value.Width, frameHeight);
        }
        public override Color? GetAlpha(Color drawColor)
        {
            return Color.White;
        }
    }
}
