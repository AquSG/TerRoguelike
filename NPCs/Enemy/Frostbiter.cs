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
    public class Frostbiter : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<Frostbiter>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Snow"] };
        public override int CombatStyle => 2;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 6;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 54;
            NPC.height = 54;
            NPC.aiStyle = -1;
            NPC.damage = 36;
            NPC.lifeMax = 600;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath6;
            NPC.knockBackResist = 0.5f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            modNPC.drawCenter = new Vector2(0, 2);
        }
        public override void AI()
        {
            int attackCooldown = 90;
            int attackTelegraph = 60;
            int dashTime = 50;
            NPC.frameCounter += 0.2d;
            modNPC.RogueFrostbiterAI(NPC, 240, dashTime, 8f, 0.2f, 7f, attackTelegraph, attackCooldown, 180f, ModContent.ProjectileType<Snowflake>(), 5f, NPC.damage, 8);
            NPC.collideX = false;
            NPC.collideY = false;
            if (NPC.ai[0] >= attackTelegraph && NPC.ai[1] == 0)
            {
                NPC.rotation += 0.25f * -NPC.direction;
            }
            else
                NPC.rotation = (NPC.velocity.X / 18f) * MathHelper.PiOver2;

            if (NPC.ai[0] == -attackCooldown && NPC.ai[1] == 0)
            {
                SoundEngine.PlaySound(SoundID.Item28 with { Volume = 0.5f }, NPC.Center);
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)NPC.lifeMax * 100.0; i++)
                {
                    int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, 67, hit.HitDirection, -1f);
                    Main.dust[dust].noGravity = true;
                }
            }
            else
            {
                for (int i = 0; i < 60; i++)
                {
                    int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.SnowflakeIce, 2 * hit.HitDirection, -2f);
                    Main.dust[dust].noGravity = false;
                }
            }
        }
        public override Color? GetAlpha(Color drawColor)
        {
            return Color.White;
        }
        public override void FindFrame(int frameHeight)
        {
            int currentFrame = (int)(NPC.frameCounter % Main.npcFrameCount[modNPCID]);
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, TextureAssets.Npc[modNPCID].Value.Width, frameHeight);
        }
    }
}
