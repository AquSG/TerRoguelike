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
using static TerRoguelike.Managers.TextureManager;
using Terraria.DataStructures;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using TerRoguelike.Packets;

namespace TerRoguelike.NPCs.Enemy
{
    public class BrainSuckler : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<BrainSuckler>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Lunar"] };
        public override int CombatStyle => 0;
        public Texture2D lightTex;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 6;
            NPCID.Sets.NoMultiplayerSmoothingByType[Type] = true;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 44;
            NPC.height = 44;
            NPC.aiStyle = -1;
            NPC.damage = 15;
            NPC.lifeMax = 1200;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.8f;
            modNPC.drawCenter = new Vector2(0, 5);
            NPC.noGravity = true;
            lightTex = TexDict["BrainSucklerGlow"];
            NPC.lavaImmune = true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.ai[1] = -1;
            NPC.ai[2] = -1;
            NPC.ai[0] = -1;
        }
        public override void AI()
        {
            float xCap = 4f;

            NPC.frameCounter += 0.25d;
            if (NPC.ai[2] < -1)
            {
                NPC.rotation = NPC.rotation.AngleLerp(0.3f * Math.Sign(NPC.velocity.X) * (Math.Abs(NPC.velocity.X) / 4f), 0.1f);
                NPC.ai[2] = (int)NPC.ai[2] + 1;
                NPC.velocity *= 0.9f;
            }
            else if (NPC.ai[2] == -1)
            {
                NPC.rotation = NPC.rotation.AngleLerp(0.3f * Math.Sign(NPC.velocity.X) * (Math.Abs(NPC.velocity.X) / 4f), 0.1f);
                modNPC.RogueFlierAI(NPC, xCap, 4f, 0.17f, true);
            }
            else
            {
                NPC.rotation = NPC.rotation.AngleLerp(0, 0.1f);
                if (NPC.ai[0] == 0)
                {
                    Player p = Main.player[(int)NPC.ai[1]];
                    if (p.dead)
                    {
                        NPC.ai[1] = -1;
                        NPC.ai[2] = -2;
                    }
                    else
                    {
                        p.GetModPlayer<TerRoguelikePlayer>().brainSucked = true;
                        NPC.Center = p.Top + (Vector2.UnitY * p.gfxOffY);
                    }
                }
                else if (NPC.ai[0] == 1)
                {
                    NPC n = Main.npc[(int)NPC.ai[1]];
                    if (n.life <= 0 || n.immortal || n.dontTakeDamage)
                    {
                        NPC.ai[1] = -1;
                        NPC.ai[2] = -2;
                    }
                    else
                    {
                        NPC.Center = n.Top + (Vector2.UnitY * n.gfxOffY);
                    }
                }
            }
        }
        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            StickToTarget(target.whoAmI, true);
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            StickToTarget(target.whoAmI, false);
        }
        public void StickToTarget(int target, bool player)
        {
            if (!player && Main.npc[target].type == ModContent.NPCType<BrainSuckler>())
                return;

            NPC.velocity *= 0;
            if (NPC.ai[2] < -1)
                return;

            if (NPC.ai[1] == -1)
                NPC.ai[1] = target;
            if (NPC.ai[2] == -1)
            {
                NPC.ai[2] = NPC.life - (int)(NPC.lifeMax * 0.1f);
                if (NPC.ai[2] < 0)
                    NPC.ai[2] = 0;
            }
            NPC.ai[0] = player ? 0 : 1;
            NpcStickPacket.Send(NPC);
            NPC.netUpdate = true;
        }
        public override bool CanHitPlayer(Player target, ref int cooldownSlot) => NPC.ai[2] >= -1;
        public override bool CanHitNPC(NPC target) => NPC.ai[2] >= -1;
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.ai[2] >= 0 && NPC.life < NPC.ai[2])
            {
                NPC.ai[1] = -1;
                NPC.ai[2] = -90;
                NPC.ai[0] = -1;
            }

            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)NPC.lifeMax * 50.0; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 249, hit.HitDirection, -1f);
                    if (Main.rand.NextBool())
                    {
                        Dust d = Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, 242)];
                        d.noGravity = true;
                        d.scale = 1.5f;
                        d.fadeIn = 1f;
                        d.velocity *= 3f;
                    }
                }
            }
            else
            {
                for (int i = 0; i < 10; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 249, hit.HitDirection, -1f);
                    Dust d = Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, 242)];
                    d.noGravity = true;
                    d.scale = 1.5f;
                    d.fadeIn = 1f;
                    d.velocity *= 3f;
                }
                Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity * 0.8f, 785);
                Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity * 0.8f, 786);
                Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity * 0.9f, 787);
            }
        }
        public override void FindFrame(int frameHeight)
        {
            NPC.gfxOffY = -4;
            int currentFrame = (int)(NPC.frameCounter % Main.npcFrameCount[Type]);
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, NpcTexWidth(Type), frameHeight);
        }
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Main.EntitySpriteDraw(lightTex, NPC.Center - Main.screenPosition + modNPC.drawCenter + (Vector2.UnitY * NPC.gfxOffY), NPC.frame, Color.White, NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
        }
    }
}
