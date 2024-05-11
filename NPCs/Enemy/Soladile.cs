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
using Terraria.Graphics.Shaders;
using Terraria.DataStructures;
using static TerRoguelike.Managers.TextureManager;

namespace TerRoguelike.NPCs.Enemy
{
    public class Soladile : BaseRoguelikeNPC
    {
        public Texture2D lightTex;
        public override int modNPCID => ModContent.NPCType<Soladile>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Lunar"] };
        public override int CombatStyle => 2;
        public int attackTelegraph = 20;
        public int attackExtendTime = 20;
        public int attackCooldown = 160;
        int currentFrame;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 10;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 60;
            NPC.height = 40;
            NPC.aiStyle = -1;
            NPC.damage = 30;
            NPC.lifeMax = 1200;
            NPC.HitSound = SoundID.NPCHit21;
            NPC.DeathSound = SoundID.NPCDeath5;
            NPC.knockBackResist = 0.2f;
            modNPC.drawCenter = new Vector2(0, -4);
            lightTex = TexDict["SoladileGlow"];
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.ai[1] = -attackCooldown + 1;
        }
        public override void AI()
        {
            NPC.frameCounter += Math.Abs(NPC.velocity.X) * 0.08d;

            modNPC.RogueFighterShooterAI(NPC, 2.4f, -7.9f, 1000f, attackTelegraph, attackCooldown, 0f, ModContent.ProjectileType<SolarFlare>(), 4.5f, new Vector2(32 * NPC.direction, 0), NPC.damage, true, false, null, attackExtendTime, 5, MathHelper.Pi * 0.125f, 1.1f);
            if (NPC.ai[1] == attackTelegraph)
            {
                SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot with { Volume = 1f }, NPC.Center);
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            NPC.ai[0] = 0;
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)20.0; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.MarblePot, hit.HitDirection, -1f);
                    if (Main.rand.NextBool(4))
                    {
                        Dust d = Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, 6)];
                        d.noGravity = true;
                        d.scale = 1.5f;
                        d.fadeIn = 1f;
                        d.velocity *= 3f;
                    }
                }
            }
            else
            {
                for (int i = 0; i < 40; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.MarblePot, hit.HitDirection, -1f);
                    Dust d = Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, 6)];
                    d.noGravity = true;
                    d.scale = 1.5f;
                    d.fadeIn = 1f;
                    d.velocity *= 3f;
                }
                Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity * 0.8f, 831);
                Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity * 0.8f, 832);
                Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity * 0.9f, 833);
                Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity * 0.9f, 834);
            }
        }
        public override void FindFrame(int frameHeight)
        {
            int frameCount = Main.npcFrameCount[modNPCID];
            if (NPC.ai[1] > 0)
            {
                NPC.frameCounter = 0;
                currentFrame = (NPC.ai[1] < attackTelegraph && (int)NPC.ai[1] % 10 < 5) || NPC.ai[1] > attackTelegraph + 15 ? frameCount - 2 : frameCount - 1;
            }
            else
            {
                currentFrame = NPC.velocity.Y == 0 ? (int)(NPC.frameCounter % (frameCount - 4)) + 2 : 1;
            }
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, TextureAssets.Npc[modNPCID].Value.Width, frameHeight);
        }
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Main.EntitySpriteDraw(lightTex, NPC.Center - Main.screenPosition + modNPC.drawCenter + (Vector2.UnitY * NPC.gfxOffY), NPC.frame, Color.White, NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
        }
    }
}
