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
using static TerRoguelike.Utilities.TerRoguelikeUtils;

namespace TerRoguelike.NPCs.Enemy
{
    public class AlienHornet : BaseRoguelikeNPC
    {
        public Texture2D lightTex;
        public override int modNPCID => ModContent.NPCType<AlienHornet>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Lunar"] };
        public override int CombatStyle => 1;
        public int attackTelegraph = 20;
        public int attackCooldown = 90;
        int currentFrame;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 3;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 36;
            NPC.height = 36;
            NPC.aiStyle = -1;
            NPC.damage = 36;
            NPC.lifeMax = 800;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.4f;
            NPC.noGravity = true;
            modNPC.drawCenter = new Vector2(0, 0);
            lightTex = TexDict["AlienHornetGlow"];
            NPC.lavaImmune = true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.ai[1] = -attackCooldown + 1;
        }
        public override void AI()
        {
            NPC.frameCounter += 0.25d;
            NPC.rotation = MathHelper.PiOver2 * NPC.velocity.Length() * 0.02f * NPC.direction;
            NPC.velocity *= 0.995f;
            modNPC.RogueFlyingShooterAI(NPC, 7f, 5.5f, 0.12f, 96f, 216f, attackTelegraph, attackCooldown, ModContent.ProjectileType<VortexLightning>(), 10f, new Vector2(14 * NPC.direction, 16).RotatedBy(NPC.rotation), NPC.damage, true);
            if (NPC.ai[2] == -attackCooldown)
            {
                SoundEngine.PlaySound(SoundID.DD2_LightningBugZap with { Volume = 1f }, NPC.Center);
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int num47 = 0; (double)num47 < hit.Damage / (double)NPC.lifeMax * 100.0; num47++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 241, hit.HitDirection, -1f);
                    if (Main.rand.NextBool())
                    {
                        Dust d = Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, 229)];
                        d.noGravity = true;
                        d.scale = 1.5f;
                        d.fadeIn = 1f;
                        d.velocity *= 3f;
                    }
                }
            }
            else
            {
                for (int i = 0; i < 20; i++)
                {
                    if (i % 2 == 0)
                        Dust.NewDust(NPC.position, NPC.width, NPC.height, 241, hit.HitDirection, -1f);

                    int dustID = Utils.SelectRandom<int>(Main.rand, 229, 229, 240);
                    Dust d = Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, dustID)];
                    d.noGravity = true;
                    d.scale = 1.25f + Main.rand.NextFloat();
                    d.fadeIn = 0.25f;
                    d.velocity *= 3f;
                    d.noLight = true;
                }
                Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity * 0.8f, 802);
                Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity * 0.9f, 803);
                Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity * 0.9f, 804);
                Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity * 0.8f, 805);
            }
        }
        public override void FindFrame(int frameHeight)
        {
            currentFrame = ((int)NPC.frameCounter % (1 + Main.npcFrameCount[Type]));
            if (currentFrame == 3)
                currentFrame = 1;
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, NpcTexWidth(Type), frameHeight);
        }
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Main.EntitySpriteDraw(lightTex, NPC.Center - Main.screenPosition + modNPC.drawCenter + (Vector2.UnitY * NPC.gfxOffY), NPC.frame, Color.White, NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
        }
    }
}
