﻿using System;
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
using static TerRoguelike.Utilities.TerRoguelikeUtils;

namespace TerRoguelike.NPCs.Enemy
{
    public class StoneDrone : BaseRoguelikeNPC
    {
        public Texture2D lightTex;
        public override int modNPCID => ModContent.NPCType<StoneDrone>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Base"] };
        public override int CombatStyle => 1;
        public int attackTelegraph = 60;
        public int attackCooldown = 60;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 22;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 20;
            NPC.height = 32;
            NPC.aiStyle = -1;
            NPC.damage = 24;
            NPC.lifeMax = 600;
            NPC.HitSound = SoundID.NPCHit7;
            NPC.DeathSound = SoundID.NPCDeath6;
            NPC.knockBackResist = 0.8f;
            modNPC.drawCenter = new Vector2(0, -10);
            NPC.noGravity = true;
            lightTex = TexDict["StoneDroneGlow"];
        }
        public override void AI()
        {
            NPC.frameCounter += 0.25d;
            NPC.rotation = MathHelper.PiOver2 * NPC.velocity.Length() * 0.02f * NPC.direction;
            modNPC.RogueFlyingShooterAI(NPC, 2f, 2f, 0.05f, 120f, 200f, attackTelegraph, attackCooldown, ModContent.ProjectileType<Rock>(), 8f, Vector2.Zero, NPC.damage, true);
            if (NPC.ai[2] == -attackCooldown)
            {
                SoundEngine.PlaySound(SoundID.Item51 with { Volume = 1f }, NPC.Center);
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)(NPC.lifeMax * 50); i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Stone, hit.HitDirection, -1f);
                }
            }
            else
            {
                for (int i = 0; i < 30; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Stone, 2.5f * (float)hit.HitDirection, -2.5f);
                }
            }
        }
        public override void FindFrame(int frameHeight)
        {
            NPC.gfxOffY = 10;
            int currentFrame;
            if (NPC.ai[2] > 0)
            {
                currentFrame = NPC.ai[2] < attackTelegraph - 11 ? (Main.npcFrameCount[Type] - 9) + (((int)(NPC.ai[2]) * 9) / (attackTelegraph - 11)) : (Main.npcFrameCount[Type] - 8) - ((int)(NPC.ai[2] - (attackTelegraph - 11)) / 6);
            }
            else if (NPC.ai[2] < -attackCooldown + 6)
            {
                NPC.frameCounter = 1;
                currentFrame = (Main.npcFrameCount[Type] - 10);
            }
            else
            {
                currentFrame = (int)(NPC.frameCounter % (Main.npcFrameCount[Type] - 10));
            }
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, NpcTexWidth(Type), frameHeight);
        }
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (modNPC.ignitedStacks.Count == 0)
                Main.EntitySpriteDraw(lightTex, NPC.Center - Main.screenPosition, NPC.frame, Color.White, NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
        }
    }
}
