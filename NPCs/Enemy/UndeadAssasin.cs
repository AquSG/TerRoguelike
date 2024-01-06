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
using Terraria.GameContent.Animations;
using Terraria.Graphics.Shaders;
using TerRoguelike.Projectiles;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using static TerRoguelike.Schematics.SchematicManager;

namespace TerRoguelike.NPCs.Enemy
{
    public class UndeadAssasin : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<UndeadAssasin>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Dungeon"] };
        public override int CombatStyle => 0;
        public int attackTelegraph = 30;
        public int attackCooldown = 270;
        public int attackExhaust = 60;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 18;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 18;
            NPC.height = 40;
            NPC.aiStyle = -1;
            NPC.damage = 35;
            NPC.lifeMax = 600;
            NPC.HitSound = SoundID.NPCHit2;
            NPC.DeathSound = SoundID.NPCDeath2;
            NPC.knockBackResist = 0.5f;
            modNPC.drawCenter = new Vector2(0, -4);
        }
        public override void AI()
        {
            NPC.frameCounter += NPC.velocity.Length() * 0.2d;

            modNPC.RogueAssasinAI(NPC, 3f, -7.9f, 0.07f, attackTelegraph, attackCooldown, attackExhaust, 128, 212f);

            if (NPC.ai[1] == attackTelegraph + attackCooldown + 1)
            {
                SoundEngine.PlaySound(SoundID.DD2_DarkMageAttack with { Volume = 0.7f }, NPC.Center);
                for (int i = 0; i < 50; i++)
                {
                    int d = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, DustID.Bone, 0f, 0f, 100, default(Color), 2f);
                    Dust dust = Main.dust[d];
                    dust.noGravity = true;
                    dust.velocity *= 0.75f;
                }
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            NPC.ai[0] = 0;
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)NPC.lifeMax * 50.0; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 26, hit.HitDirection, -1f);
                }
            }
            else
            {
                for (int i = 0; i < 20; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 26, 2.5f * (float)hit.HitDirection, -2.5f);
                }
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 42, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 43, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 43, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, 44, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, 44, NPC.scale);
            }
        }
        public override void FindFrame(int frameHeight)
        {
            int currentFrame = NPC.ai[1] > attackCooldown ? (NPC.ai[1] > attackTelegraph + attackCooldown ? 16 : 17) : (NPC.velocity.Y != 0 ? 1 : (int)(NPC.frameCounter % (Main.npcFrameCount[modNPCID] - 4)) + 2);
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, TextureAssets.Npc[modNPCID].Value.Width, frameHeight);
        }
    }
}
