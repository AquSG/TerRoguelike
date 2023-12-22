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

namespace TerRoguelike.NPCs.Enemy
{
    public class IceTortoise : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<IceTortoise>();
        public override List<int> associatedFloors => new List<int>() { 4 };
        public override Vector2 DrawCenterOffset => new Vector2(0, 0);
        public override int CombatStyle => 0;
        public int attackCooldown = 300;
        public int dashTime = 180;
        public int attackTelegraph = 60;
        public float dashVelocity = 8f;
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
            NPC.damage = 40;
            NPC.lifeMax = 1200;
            NPC.HitSound = SoundID.NPCHit24;
            NPC.DeathSound = SoundID.NPCDeath27;
            NPC.knockBackResist = 0f;
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

            modNPC.RogueTortoiseAI(NPC, 1.4f, -7.9f, 10, dashTime, dashVelocity, attackCooldown, attackTelegraph);

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
                for (int i = 0; (double)i < hit.Damage / (double)NPC.lifeMax * 50.0; i++)
                {
                    int num321 = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, 67, NPC.velocity.X, NPC.velocity.Y, 90, default(Color), 1.5f);
                    Main.dust[num321].noGravity = true;
                    Dust dust122 = Main.dust[num321];
                    Dust dust190 = dust122;
                    dust190.velocity *= 0.2f;
                }
            }
            else
            {
                for (int i = 0; i < 50; i++)
                {
                    int num324 = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, 67, NPC.velocity.X, NPC.velocity.Y, 90, default(Color), 1.5f);
                    Main.dust[num324].noGravity = true;
                    Dust dust123 = Main.dust[num324];
                    Dust dust190 = dust123;
                    dust190.velocity *= 0.2f;
                }
                Gore.NewGore(NPC.GetSource_FromThis(), NPC.position, NPC.velocity, 180, NPC.scale);
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
