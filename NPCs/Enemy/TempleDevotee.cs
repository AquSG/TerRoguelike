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
    public class TempleDevotee : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<TempleDevotee>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Temple"] };
        public override int CombatStyle => 2;

        public Vector2 bulletPos = Vector2.Zero;
        public float gunRot = 0;
        public SpriteEffects spriteEffects = SpriteEffects.None;
        public int attackTelegraph = 40;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 12;
            NPCID.Sets.MustAlwaysDraw[Type] = true;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 18;
            NPC.height = 40;
            NPC.aiStyle = -1;
            NPC.damage = 26;
            NPC.lifeMax = 600;
            NPC.HitSound = SoundID.NPCHit2;
            NPC.DeathSound = SoundID.NPCDeath2;
            NPC.knockBackResist = 0.4f;
            modNPC.drawCenter = new Vector2(0, -5);
        }
        public override void AI()
        {
            gunRot = NPC.spriteDirection == -1 ? 0f : MathHelper.Pi;

            if (modNPC.targetNPC != -1 && NPC.ai[0] >= 0)
            {
                NPC npc = Main.npc[modNPC.targetNPC];
                gunRot = (npc.Center - NPC.Center).ToRotation();
            }
            else if (modNPC.targetPlayer != -1 && NPC.ai[0] >= 0)
            {
                Player player = Main.player[modNPC.targetPlayer];
                gunRot = (player.Center - NPC.Center).ToRotation();
            }

            int attackCooldown = 30;
            NPC.frameCounter += NPC.velocity.Length() * 0.2d;
            bulletPos = new Vector2(24, 2 * NPC.direction).RotatedBy(gunRot);
            modNPC.RogueFighterShooterAI(NPC, 1.5f, -7.9f, 640f, attackTelegraph, attackCooldown, 0f, ModContent.ProjectileType<SeekerArrow>(), 7f, bulletPos, NPC.damage * 2, true, false, gunRot, 20);
            if (NPC.ai[1] == attackTelegraph)
            {
                SoundEngine.PlaySound(SoundID.Item5 with { Volume = 0.9f }, NPC.Center);
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            NPC.ai[0] = 0;
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)(NPC.lifeMax * 50); i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 5, hit.HitDirection, -1f);
                }
            }
            else
            {
                for (int i = 0; i < 30; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 5, 2.5f * (float)hit.HitDirection, -2.5f);
                }
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("TempleDevotee1").Type, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, Mod.Find<ModGore>("TempleDevotee2").Type, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, Mod.Find<ModGore>("TempleDevotee2").Type, NPC.scale);
            }
        }
        public override void FindFrame(int frameHeight)
        {
            int currentFrame = 1;
            if (NPC.ai[1] > 0)
            {
                float visualGunRot = gunRot;
                if (NPC.direction == -1)
                {
                    if (visualGunRot > 0)
                    {
                        visualGunRot = MathHelper.Pi - visualGunRot;
                    }
                    else
                    {
                        visualGunRot = -MathHelper.Pi - visualGunRot;
                    }
                }
                if (Math.Abs(visualGunRot) < MathHelper.PiOver4 * 0.333f)
                    currentFrame = Main.npcFrameCount[Type] - 3;
                else if (visualGunRot > 0)
                {
                    if (visualGunRot < MathHelper.PiOver4 + 0.4f)
                        currentFrame = Main.npcFrameCount[Type] - 4;
                    else
                        currentFrame = Main.npcFrameCount[Type] - 5;
                }
                else
                {
                    if (visualGunRot > -MathHelper.PiOver4 - 0.4f)
                        currentFrame = Main.npcFrameCount[Type] - 2;
                    else
                        currentFrame = Main.npcFrameCount[Type] - 1;
                }
            }
            else if (NPC.velocity.Y == 0)
            {
                currentFrame = (int)(NPC.frameCounter % (Main.npcFrameCount[Type] - 7)) + 2;
            }
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, NpcTexWidth(Type), frameHeight);
        }
    }
}
