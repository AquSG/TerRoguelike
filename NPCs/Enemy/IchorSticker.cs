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
    public class IchorSticker : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<IchorSticker>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Crimson"] };
        public override int CombatStyle => 1;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 4;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 28;
            NPC.height = 56;
            NPC.aiStyle = -1;
            NPC.damage = 35;
            NPC.lifeMax = 1000;
            NPC.HitSound = SoundID.NPCHit13;
            NPC.DeathSound = SoundID.NPCDeath19;
            NPC.knockBackResist = 0.6f;
            modNPC.drawCenter = new Vector2(0, -11);
            NPC.noGravity = true;
        }
        public override void AI()
        {
            int attackTelegraph = 45;
            int attackCooldown = 75;
            NPC.frameCounter += 0.15d;
            NPC.rotation = MathHelper.PiOver2 * Math.Abs(NPC.velocity.X) * 0.10f * (NPC.velocity.X == 0 ? 0 : Math.Sign(NPC.velocity.X));
            modNPC.RogueFlyingShooterAI(NPC, 1.5f, 1.5f, 0.04f, 160f, 480f, attackTelegraph, attackCooldown, ModContent.ProjectileType<IchorBlob>(), 8f, new Vector2(0, 10).RotatedBy(NPC.rotation), NPC.damage, true, 0.98f);
            if (NPC.ai[2] == -attackCooldown)
            {
                SoundEngine.PlaySound(SoundID.Item17 with { Volume = 1f }, NPC.Center);
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < NPC.damage / (double)NPC.lifeMax * 100.0; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 5, hit.HitDirection, -1f, NPC.alpha);
                }
                return;
            }
            else
            {
                for (int i = 0; i < 50; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 5, 2 * hit.HitDirection, -2f, NPC.alpha);
                }
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 403);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 404);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 405);
            }
            

        }
        public override void FindFrame(int frameHeight)
        {
            int currentFrame = (int)(NPC.frameCounter % Main.npcFrameCount[modNPCID]);
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, TextureAssets.Npc[modNPCID].Value.Width, frameHeight);
        }
    }
}
