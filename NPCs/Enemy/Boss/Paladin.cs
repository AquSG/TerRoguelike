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
using Terraria.DataStructures;
using static TerRoguelike.Managers.TextureManager;
using TerRoguelike.Systems;

namespace TerRoguelike.NPCs.Enemy.Boss
{
    public class Paladin : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<Paladin>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Base"] };
        public override int CombatStyle => -1;
        public int currentFrame;
        public Texture2D hammerTex;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 15;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 30;
            NPC.height = 47;
            NPC.aiStyle = -1;
            NPC.damage = 25;
            NPC.lifeMax = 600;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.NPCDeath6;
            NPC.knockBackResist = 0f;
            modNPC.drawCenter = new Vector2(0, -12);
            hammerTex = TexDict["PaladinHammer"];
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.immortal = true;
            NPC.dontTakeDamage = true;
            currentFrame = Main.npcFrameCount[Type] - 1;
            NPC.localAI[0] = -210;
            NPC.direction = -1;
            NPC.spriteDirection = -1;
            CutsceneSystem.SetCutscene(NPC.Center, 210, 30, 30, 2.5f);
        }
        public override void AI()
        {
            if (NPC.localAI[0] < 30)
            {
                if (NPC.localAI[0] == -150)
                {
                    SoundEngine.PlaySound(SoundID.DeerclopsRubbleAttack with { Volume = 1f, Pitch = 0.5f }, NPC.Center);
                    SoundEngine.PlaySound(SoundID.DeerclopsIceAttack with { Volume = 0.5f, Pitch = -0.2f }, NPC.Center);
                    for (int i = -15; i <= 15; i++)
                    {
                        Dust d = Dust.NewDustPerfect(NPC.Bottom + new Vector2(3 * i, 0), DustID.Smoke, null, 0, Color.LightGray, 1f);
                        d.velocity.Y -= 1;
                    }
                }
                NPC.localAI[0]++;
                if (NPC.localAI[0] == 30)
                {
                    NPC.immortal = false;
                    NPC.dontTakeDamage = false;
                }
            }
            else
            {
                BossAI();
            }
        }
        public void BossAI()
        {
            NPC.ai[3]++;
            if (NPC.ai[3] > 180)
            {
                modNPC.RogueFighterAI(NPC, 2f, -7f);
                NPC.frameCounter += 0.15d;
            }
        }
        public override void OnKill()
        {
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 10; i++)
                {
                    int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, 16, 0f, 0f, 0, default(Color), 1.5f);
                    Dust d = Main.dust[dust];
                    d.velocity *= 2f;
                    d.noGravity = true;
                }
                Vector2 pos = new Vector2(NPC.position.X, NPC.position.Y);
                Vector2 velocity = default(Vector2);
                int gore = Gore.NewGore(NPC.GetSource_Death(), pos, velocity, Main.rand.Next(11, 14), NPC.scale);
                Gore g = Main.gore[gore];
                g.velocity *= 0.5f;

                pos = new Vector2(NPC.position.X, NPC.position.Y + 20f);
                gore = Gore.NewGore(NPC.GetSource_Death(), pos, velocity, Main.rand.Next(11, 14), NPC.scale);
                g = Main.gore[gore];
                g.velocity *= 0.5f;

                pos = new Vector2(NPC.position.X, NPC.position.Y + 40f);
                gore = Gore.NewGore(NPC.GetSource_Death(), pos, velocity, Main.rand.Next(11, 14), NPC.scale);
                g = Main.gore[gore];
                g.velocity *= 0.5f;
            }
        }
        public override void FindFrame(int frameHeight)
        {
            int frameCount = Main.npcFrameCount[Type];
            bool swing = false;
            int swingTime = 0;
            if (NPC.localAI[0] < 0)
            {
                if (NPC.localAI[0] < -120)
                    currentFrame = frameCount - 1;
                else if (NPC.localAI[0] >= -50)
                    currentFrame = 0;
                else if (NPC.localAI[0] >= -70)
                    currentFrame = frameCount - 2;
                else if (NPC.localAI[0] >= -90)
                    currentFrame = frameCount - 3;
            }
            else
            {
                if (swing)
                {
                    currentFrame = swingTime < 0 ? frameCount - 6 : (swingTime < 10 ? frameCount - 5 : frameCount - 4);
                    NPC.frameCounter = 0;
                }
                else
                {
                    currentFrame = NPC.velocity.Y == 0 ? ((int)NPC.frameCounter % (frameCount - 8)) + 2 : 1;
                }
            }
            
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, TextureAssets.Npc[Type].Value.Width, frameHeight);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;
            int frameHeight = tex.Height / Main.npcFrameCount[Type];
            modNPC.drawCenter = new Vector2(0, -((frameHeight - NPC.height) * 0.5f) + 16);

            if (NPC.localAI[0] < -150)
            {
                modNPC.drawCenter.Y += MathHelper.Lerp(0, -1000, Math.Abs(NPC.localAI[0] + 150) / 60f);
            }
            Vector2 drawPos = NPC.Center + modNPC.drawCenter + (Vector2.UnitY * NPC.gfxOffY);
            Main.EntitySpriteDraw(tex, drawPos - Main.screenPosition, NPC.frame, Lighting.GetColor(drawPos.ToTileCoordinates()), NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            if (NPC.localAI[0] < -90)
            {
                Vector2 hammerPos = new Vector2(-8f, 0) + NPC.Bottom;
                Main.EntitySpriteDraw(hammerTex, hammerPos - Main.screenPosition, null, Lighting.GetColor(hammerPos.ToTileCoordinates()), MathHelper.Pi, hammerTex.Size() * 0.5f, 1f, SpriteEffects.None);
            }
            return false;
        }
    }
}
