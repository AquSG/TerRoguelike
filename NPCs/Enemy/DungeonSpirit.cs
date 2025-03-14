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
using TerRoguelike.Projectiles;
using TerRoguelike.NPCs;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using static TerRoguelike.Schematics.SchematicManager;
using static TerRoguelike.Utilities.TerRoguelikeUtils;

namespace TerRoguelike.NPCs.Enemy
{
    public class DungeonSpirit : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<DungeonSpirit>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Dungeon"] };
        public override int CombatStyle => 0;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 3;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 20;
            NPC.height = 20;
            NPC.aiStyle = -1;
            NPC.damage = 30;
            NPC.lifeMax = 400;
            NPC.DeathSound = SoundID.NPCDeath39 with { Volume = 0.5f };
            NPC.knockBackResist = 1f;
            modNPC.drawCenter = new Vector2(0, -3);
            NPC.noGravity = true;
            NPC.noTileCollide = true;
        }
        public override void AI()
        {
            NPC.frameCounter += 0.05d;

            modNPC.RogueDungeonSpiritAI(NPC, 0.2f, 6f);
            float targetRot = MathHelper.PiOver2;
            if (modNPC.targetNPC != -1)
            {
                targetRot = (Main.npc[modNPC.targetNPC].Center - NPC.Center).ToRotation();
            }
            else if (modNPC.targetPlayer != -1)
            {
                targetRot = (Main.player[modNPC.targetPlayer].Center - NPC.Center).ToRotation();
            }

            
            NPC.rotation = (NPC.rotation + MathHelper.PiOver2).AngleTowards(targetRot, 0.06f) - MathHelper.PiOver2;

            for (int i = 0; i < 1; i++)
            {
                Dust d = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.DungeonSpirit, NPC.velocity.X * 0.5f, NPC.velocity.Y * 0.5f);
                d.noGravity = true;
            }
        }
        public override Color? GetAlpha(Color drawColor)
        {
            return Color.White;
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.soundDelay <= 0 && NPC.life > 0)
            {
                SoundEngine.PlaySound(SoundID.NPCHit36 with { Volume = 0.3f }, NPC.Center);
                NPC.soundDelay = 15;
            }
            
            if (NPC.life <= 0)
            {
                SoundEngine.PlaySound(SoundID.NPCHit36 with { Volume = 0.3f }, NPC.Center);
                for (int i = 0; i < 50; i++)
                {
                    int d = Dust.NewDust(NPC.position, NPC.width, NPC.height, 180, NPC.velocity.X * 0.5f, NPC.velocity.Y * 0.5f);
                    Dust dust = Main.dust[d];
                    dust.velocity *= 2f;
                    dust.noGravity = true;
                    dust.scale = 1.4f;
                }
            }
            
        }
            
        public override void FindFrame(int frameHeight)
        {
            int currentFrame = (int)(NPC.frameCounter % Main.npcFrameCount[Type]);
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, NpcTexWidth(Type), frameHeight);
        }
    }
}
