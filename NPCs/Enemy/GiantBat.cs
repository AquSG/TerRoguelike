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
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using static TerRoguelike.Schematics.SchematicManager;
using TerRoguelike.Projectiles;
using static TerRoguelike.Utilities.TerRoguelikeUtils;

namespace TerRoguelike.NPCs.Enemy
{
    public class GiantBat : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<GiantBat>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Snow"] };
        public override int CombatStyle => 1;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 12;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 26;
            NPC.height = 24;
            NPC.aiStyle = -1;
            NPC.damage = 25;
            NPC.lifeMax = 600;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.5f;
            NPC.noGravity = true;
            modNPC.drawCenter = new Vector2(0, -3);
        }
        public override void AI()
        {
            int attackCooldown = 30;
            NPC.frameCounter += 0.2d;
            modNPC.RogueGiantBatAI(NPC, 128f, 0.3f, 4.8f, 30, attackCooldown, 32f, ModContent.ProjectileType<Iceflake>(), new Vector2(0, 2.5f), NPC.damage);
            NPC.rotation = (NPC.velocity.X / 18f) * MathHelper.PiOver2;
            if (NPC.ai[0] == -attackCooldown)
            {
                SoundEngine.PlaySound(SoundID.Item28 with { Volume = 0.9f }, NPC.Center);
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
            {
                if (Main.netMode != NetmodeID.Server)
                {
                    Gore.NewGore(NPC.GetSource_Death(), NPC.Center + (Vector2.UnitX * -12f), NPC.velocity, Mod.Find<ModGore>("GiantBat1").Type, NPC.scale);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity, Mod.Find<ModGore>("GiantBat2").Type, NPC.scale);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.Center + (Vector2.UnitX * 12f), NPC.velocity, Mod.Find<ModGore>("GiantBat3").Type, NPC.scale);
                }
            }
        }
        public override void FindFrame(int frameHeight)
        {
            int currentFrame = (int)(NPC.frameCounter % (Main.npcFrameCount[Type] / 3)) + (NPC.ai[0] > 0 ? 4 : (NPC.ai[0] < -19 ? 8 : 0));
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, NpcTexWidth(Type), frameHeight - 1);
        }
        public override bool? CanFallThroughPlatforms()
        {
            return true;
        }
    }
}
