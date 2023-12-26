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
    public class Antlion : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<Antlion>();
        public override List<int> associatedFloors => new List<int>() { 5 };
        public override Vector2 DrawCenterOffset => new Vector2(0, 0);
        public override int CombatStyle => 1;
        public Texture2D headTex = ModContent.Request<Texture2D>("TerRoguelike/NPCs/Enemy/AntlionHead").Value;
        public Texture2D texture;
        public override bool ignoreRoomWallCollision => true;
        public int attackTelegraph = 120;
        public int attackCooldown = 60;
        public int burrowDownTime = 60;
        public int burrowUpTime = 60;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 2;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 24;
            NPC.height = 24;
            NPC.aiStyle = -1;
            NPC.damage = 25;
            NPC.lifeMax = 800;
            NPC.HitSound = SoundID.NPCHit31;
            NPC.DeathSound = SoundID.NPCDeath34;
            NPC.knockBackResist = 0f;
            NPC.behindTiles = true;
            texture = ModContent.Request<Texture2D>(Texture).Value;
            NPC.noGravity = false;
        }
        public override void AI()
        {
            NPC.frameCounter += 0.2d;
            modNPC.RogueAntlionAI(NPC, MathHelper.PiOver2, 80f, 200f, burrowDownTime, burrowUpTime, 80f, 360, attackTelegraph, attackCooldown, ProjectileID.SandBallFalling, Vector2.Zero, 15, NPC.damage, MathHelper.Pi * 0.0675f, 7f, 12f);

            if (NPC.ai[0] >= 0 && (int)(NPC.ai[0] - attackTelegraph) % (attackTelegraph + attackCooldown) == 0)
            {
                SoundEngine.PlaySound(SoundID.NPCDeath13 with { Volume = 0.75f }, NPC.Center);
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)NPC.lifeMax * (double)100; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 250, hit.HitDirection, -1f);
                }
            }
            else
            {
                for (int i = 0; i < 50; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 250, 2 * hit.HitDirection, -2f);
                }
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 97);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 98);
            }
            
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            int headFrameHeight = (int)(headTex.Size().Y * 0.2d);
            int headFrame = (int)(NPC.frameCounter % 5d);
            int frameHeight = (int)(texture.Size().Y * 0.5d);
            float opacity = MathHelper.Clamp(MathHelper.Lerp(0, 1f, Math.Abs(NPC.ai[0] + burrowDownTime) / 60), 0f, 1f);
            Main.EntitySpriteDraw(headTex, NPC.Center - Main.screenPosition + (Vector2.UnitY * 8), new Rectangle(0, headFrameHeight * headFrame, headTex.Width, headFrameHeight), drawColor * opacity, NPC.rotation - MathHelper.PiOver2, new Vector2(headTex.Width * 0.5f, (headFrameHeight * 0.5f)) + (Vector2.UnitY * 8), NPC.scale, SpriteEffects.None);
            Main.EntitySpriteDraw(texture, NPC.Center - Main.screenPosition + (Vector2.UnitY * 10), new Rectangle(0, frameHeight, texture.Width, frameHeight), drawColor * opacity, 0f, new Vector2(headTex.Width * 0.5f, (frameHeight * 0.5f)), NPC.scale, SpriteEffects.None);
            return false;
        }
    }
}
