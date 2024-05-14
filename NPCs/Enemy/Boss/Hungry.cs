using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using TerRoguelike.Managers;
using TerRoguelike.Particles;
using TerRoguelike.Projectiles;
using TerRoguelike.Systems;
using static TerRoguelike.Managers.TextureManager;
using static TerRoguelike.Schematics.SchematicManager;
using static TerRoguelike.Systems.MusicSystem;
using static TerRoguelike.Systems.RoomSystem;
using static TerRoguelike.Utilities.TerRoguelikeUtils;

namespace TerRoguelike.NPCs.Enemy.Boss
{
    public class Hungry : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<Hungry>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Hell"] };
        public override int CombatStyle => -1;
        public int currentFrame = 0;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 6;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 30;
            NPC.height = 30;
            NPC.aiStyle = -1;
            NPC.damage = 35;
            NPC.lifeMax = 360;
            NPC.HitSound = SoundID.NPCHit9;
            NPC.DeathSound = SoundID.NPCDeath12;
            NPC.knockBackResist = 0.6f;
            modNPC.drawCenter = new Vector2(0, -6);
            modNPC.IgnoreRoomWallCollision = true;
            NPC.noTileCollide = false;
            NPC.noGravity = true;
            NPC.hide = true;
            NPC.spriteDirection = 1;
            NPC.gfxOffY = 5;
        }
        public override void DrawBehind(int index)
        {
            Main.instance.DrawCacheNPCProjectiles.Add(index);
        }
        public override void OnSpawn(IEntitySource source)
        {
            if (NPC.ai[0] >= 0)
            {
                NPC npc = Main.npc[(int)NPC.ai[0]];
                if (!npc.active || npc.type != ModContent.NPCType<WallOfFlesh>())
                {
                    NPC.ai[0] = -1;
                    NPC.StrikeInstantKill();
                    return;
                }
            }
            if (NPC.ai[0] == -1)
                NPC.StrikeInstantKill();
        }
        
        public override void AI()
        {
            Entity target = modNPC.GetTarget(NPC);

            if (target != null)
            {
                if (CanHitInLine(NPC.Center, target.Center))
                    NPC.ai[1] = 0;
                else
                    NPC.ai[1]++;

                if (NPC.ai[1] < 30)
                    NPC.velocity += (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX) * 0.2f;
                else
                    NPC.velocity += (NPC.velocity.RotatedBy(Main.rand.NextFloat(-0.1f, 0.1f))).SafeNormalize(Vector2.UnitX) * 0.2f;

                if (NPC.velocity.Length() > 6)
                    NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * 6;

                NPC.velocity *= 0.98f;

            }

            NPC.rotation = NPC.rotation.AngleTowards(NPC.velocity.ToRotation(), 0.1f);
            NPC.frameCounter += 0.2d;
            if (NPC.collideX || NPC.collideY)
            {
                if (NPC.collideX)
                {
                    NPC.velocity.X = -NPC.oldVelocity.X * 0.5f;
                }
                if (NPC.collideY)
                {
                    NPC.velocity.Y = -NPC.oldVelocity.Y * 0.5f;
                }
                NPC.velocity *= 0.8f;
            }

            NPC parent = Main.npc[(int)NPC.ai[0]];
            if (!parent.active || parent.type != ModContent.NPCType<WallOfFlesh>())
                NPC.StrikeInstantKill();
            else
                parent.ai[3]++;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int i = 0; i < 5; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 5, hit.HitDirection, -1f);
                }
            }
            else
            {
                for (int i = 0; i < 20; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 5, hit.HitDirection, -1f);
                }
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 132, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 133, NPC.scale);
            }
        }
        public override void OnKill()
        {

        }
        public override void FindFrame(int frameHeight)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;
            currentFrame = (int)NPC.frameCounter % Main.npcFrameCount[Type];

            NPC.frame = new Rectangle(0, currentFrame * frameHeight, tex.Width, frameHeight);
        }
        public override bool? CanFallThroughPlatforms() => true;
    }
}
