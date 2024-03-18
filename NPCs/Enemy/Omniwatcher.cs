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

namespace TerRoguelike.NPCs.Enemy
{
    public class Omniwatcher : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<Omniwatcher>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Lunar"] };
        public override int CombatStyle => 1;
        public Texture2D lightTex;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 6;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 40;
            NPC.height = 40;
            NPC.aiStyle = -1;
            NPC.damage = 35;
            NPC.lifeMax = 800;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            modNPC.drawCenter = new Vector2(0, -2);
            NPC.noGravity = true;
            NPC.noTileCollide = false;
            lightTex = TexDict["OmniwatcherGlow"].Value;
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.velocity = Main.rand.NextVector2CircularEdge(0.4f, 0.4f);
        }
        public override void AI()
        {
            int attackTelegraph = 30;
            int attackCooldown = 90;
            NPC.frameCounter += 0.25d;
            modNPC.RogueTeleportingShooterAI(NPC, 160f, 320f, 240, attackTelegraph, attackCooldown, ModContent.ProjectileType<NebulaLaser>(), 12f, new Vector2(16 * NPC.spriteDirection, -8), NPC.damage, true);
            if ((int)(NPC.ai[0] - attackTelegraph) % (attackTelegraph + attackCooldown) == 0)
            {
                SoundEngine.PlaySound(SoundID.Item12 with { Volume = 1f }, NPC.Center);
            }
            else if (NPC.ai[0] == 0)
            {
                SoundEngine.PlaySound(SoundID.Item8 with { Volume = 1f }, NPC.Center);
                for (int i = 0; i < 50; i++)
                {
                    int d = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.ShadowbeamStaff, 0f, 0f, 100, default, 1.8f);
                    Dust dust = Main.dust[d];
                    dust.velocity *= 3f;
                    dust.noGravity = true;
                    dust.noLight = true;
                    dust.noLightEmittence = true;
                }
                NPC.velocity = Main.rand.NextVector2CircularEdge(0.4f, 0.4f);
            }
            if (NPC.collideX)
            {
                NPC.velocity.X = -NPC.oldVelocity.X;
            }
            if (NPC.collideY)
            {
                NPC.velocity.Y = -NPC.oldVelocity.Y;
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)20.0; i++)
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
                Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity * 0.8f, 782);
                Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity * 0.8f, 783);
                Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity * 0.9f, 784);
            }
        }
        public override void FindFrame(int frameHeight)
        {
            int currentFrame = (int)(NPC.frameCounter % Main.npcFrameCount[modNPCID]);
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, TextureAssets.Npc[modNPCID].Value.Width, frameHeight);
        }
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Main.EntitySpriteDraw(lightTex, NPC.Center - Main.screenPosition + modNPC.drawCenter + (Vector2.UnitY * NPC.gfxOffY), NPC.frame, Color.White, NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
        }
        public override bool? CanFallThroughPlatforms()
        {
            return true;
        }
        public override bool CanHitNPC(NPC target) => false;
        public override bool CanHitPlayer(Player target, ref int cooldownSlot) => false;
    }
}
