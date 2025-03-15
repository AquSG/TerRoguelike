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
using static TerRoguelike.Schematics.SchematicManager;
using TerRoguelike.Projectiles;
using Terraria.DataStructures;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using Terraria.Audio;

namespace TerRoguelike.NPCs.Enemy
{
    public class WrathfulRoot : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<WrathfulRoot>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Forest"] };
        public override int CombatStyle => 1;
        public int attackCooldown = 80;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1;
            NPCID.Sets.NoMultiplayerSmoothingByType[Type] = true;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 14;
            NPC.height = 40;
            NPC.aiStyle = -1;
            NPC.damage = 30;
            NPC.lifeMax = 600;
            NPC.HitSound = SoundID.NPCHit7;
            NPC.DeathSound = SoundID.NPCDeath6;
            NPC.knockBackResist = 0f;
            modNPC.drawCenter = new Vector2(0, 2);
            NPC.noGravity = true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.ai[0] = -attackCooldown + 1;
            Point spawnTile = new Point((int)(NPC.Center.X / 16f), (int)(NPC.Center.Y / 16f));
            if (!ParanoidTileRetrieval(spawnTile.X, spawnTile.Y).IsTileSolidGround())
            {
                for (int i = 0; i < 50; i++)
                {
                    int direction = i % 2 == 0 ? -1 : 1;
                    if (!ParanoidTileRetrieval(spawnTile.X + ((i / 2) * direction), spawnTile.Y).IsTileSolidGround())
                        continue;

                    NPC.direction = -direction;
                    NPC.spriteDirection = -direction;
                    NPC.Center = new Vector2((spawnTile.X + ((i / 2) * direction)) * 16f + (NPC.direction == 1 ? 16 : 0) + (NPC.width * 0.5f * NPC.direction), NPC.Center.Y);
                    break;
                }
            }
            else
            {
                NPC.direction = -1;
                NPC.spriteDirection = -1;
            }
        }
        public override void AI()
        {
            NPC.velocity.X += 0.5f * -NPC.direction;
            modNPC.RogueTurretAI(NPC, 10, attackCooldown, 300f, ModContent.ProjectileType<WoodSliver>(), NPC.damage, 12f, Vector2.UnitY * 7, true, NPC.direction == 1 ? 0f : MathHelper.Pi, MathHelper.PiOver4);
            if (NPC.ai[0] == -attackCooldown)
            {
                SoundEngine.PlaySound(SoundID.Item17 with { Volume = 1f }, NPC.Center);
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage/ (double)NPC.lifeMax * 25.0; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.WoodFurniture, hit.HitDirection, -1f, 0, default(Color), 1.1f);
                }
            }
            else
            {
                for (int i = 0; i < 30; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.WoodFurniture, 2 * hit.HitDirection, -2f, 0, default(Color), 1.1f);
                }
            }
        }
        public override void FindFrame(int frameHeight)
        {
            int currentFrame = 0;
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, NpcTexWidth(Type), frameHeight);
        }
    }
}
