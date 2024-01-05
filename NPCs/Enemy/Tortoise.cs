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
    public class Tortoise : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<Tortoise>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Jungle"] };
        public override int CombatStyle => 0;
        public int attackCooldown = 360;
        public int dashTime = 220;
        public int attackTelegraph = 60;
        public float dashVelocity = 9f;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 8;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 40;
            NPC.height = 40;
            NPC.aiStyle = -1;
            NPC.damage = 45;
            NPC.lifeMax = 1200;
            NPC.HitSound = SoundID.NPCHit24;
            NPC.DeathSound = SoundID.NPCDeath27;
            NPC.knockBackResist = 0f;
            modNPC.drawCenter = new Vector2(0, 0);
        }
        public override void AI()
        {
            if (NPC.ai[1] >= 0 && NPC.ai[1] < attackCooldown - attackTelegraph)
            {
                NPC.frameCounter += NPC.velocity.Length() * 0.06d;
            }
            else if (NPC.ai[1] > -attackTelegraph)
            {
                if (NPC.frameCounter >= 0)
                    NPC.frameCounter = -attackTelegraph + 1;

                NPC.frameCounter++;
            }
            else
            {
                NPC.frameCounter = 0;
            }

            if (NPC.ai[1] < -attackTelegraph)
            {
                if (NPC.ai[1] <= -dashTime)
                    NPC.rotation += MathHelper.Pi / 25f * NPC.direction;
                else
                {
                    NPC.rotation += NPC.velocity.Length() * MathHelper.Pi / 80f * NPC.direction;
                }
            }
            else
            {
                NPC.rotation = NPC.rotation.AngleTowards(0f, MathHelper.Pi / 30f);
            } 

            modNPC.RogueTortoiseAI(NPC, 1.8f, -9f, 10, dashTime, dashVelocity, attackCooldown, attackTelegraph);

            if (NPC.ai[1] < 0 && NPC.ai[1] > -dashTime && (NPC.collideX || (NPC.collideY && NPC.oldVelocity.Y > 3f)))
            {
                SoundEngine.PlaySound(SoundID.Item70 with { Volume = 0.25f * Math.Abs(NPC.velocity.X / dashVelocity) }, NPC.Center);
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            NPC.ai[0] = 0;
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
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 177);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 178);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 179);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 179);
            }
        }
        public override void FindFrame(int frameHeight)
        {
            int currentFrame = 0;
            if (NPC.ai[1] >= 0 && NPC.ai[1] <= attackCooldown - attackTelegraph + 1)
            {
                currentFrame = (int)(NPC.frameCounter % (Main.npcFrameCount[modNPCID] - 3));
            }
            else if (NPC.ai[1] > attackCooldown - attackTelegraph)
            {
                currentFrame = (int)((NPC.frameCounter / attackTelegraph) * 3) + 7;
            }
            else if (NPC.ai[1] > -attackTelegraph + 1)
            {
                currentFrame = (int)((-NPC.frameCounter / attackTelegraph) * 3) + 5;
            }
            else
            {
                currentFrame = Main.npcFrameCount[modNPCID] - 1;
            }
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, TextureAssets.Npc[modNPCID].Value.Width, frameHeight);
        }
    }
}
