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
using Terraria.Audio;
using TerRoguelike.Projectiles;

namespace TerRoguelike.NPCs.Enemy
{
    public class UndeadPrisoner : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<UndeadPrisoner>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Base"] };
        public override int CombatStyle => 2;
        public int attackTelegraph = 30;
        public int attackCooldown = 150;
        public int extendedAttackSlowdown = 18;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 19;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 18;
            NPC.height = 40;
            NPC.aiStyle = -1;
            NPC.damage = 30;
            NPC.lifeMax = 600;
            NPC.HitSound = SoundID.NPCHit2;
            NPC.DeathSound = SoundID.NPCDeath2;
            NPC.knockBackResist = 0.5f;
            modNPC.drawCenter = new Vector2(0, -4);
        }
        public override void AI()
        {
            NPC.frameCounter += NPC.velocity.Length() * 0.25d;
            modNPC.RogueFighterShooterAI(NPC, 1.4f, -7.9f, 240f, attackTelegraph, attackCooldown, 0f, ModContent.ProjectileType<Bone>(), 8f, Vector2.Zero, NPC.damage, true, false, null, extendedAttackSlowdown);
            if (NPC.ai[1] == attackTelegraph)
            {
                SoundEngine.PlaySound(SoundID.Item1 with { Volume = 0.9f }, NPC.Center);
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
            int currentFrame = NPC.ai[1] > 0 ? (int)((NPC.ai[1] - 1) / ((attackTelegraph + extendedAttackSlowdown) / 4)) + Main.npcFrameCount[modNPCID] - 5 : (NPC.velocity.Y == 0 ? (int)(NPC.frameCounter % (Main.npcFrameCount[modNPCID] - 5)) + 1 : 0);
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, TextureAssets.Npc[modNPCID].Value.Width, frameHeight);
        }
    }
}
