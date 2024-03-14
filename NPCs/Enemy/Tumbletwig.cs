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
    public class Tumbletwig : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<Tumbletwig>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Forest"] };
        public override int CombatStyle => 1;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 1;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 30;
            NPC.height = 30;
            NPC.aiStyle = -1;
            NPC.damage = 35;
            NPC.lifeMax = 400;
            NPC.HitSound = SoundID.NPCHit11;
            NPC.DeathSound = SoundID.NPCDeath15;
            NPC.knockBackResist = 0f;
            modNPC.drawCenter = new Vector2(0, -2);
            NPC.noGravity = false;
            NPC.noTileCollide = false;
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.direction = Main.rand.NextBool() ? 1 : -1;
            NPC.ai[3] = 1;
            for (int i = 0; i < 4; i++)
            {
                float rot = MathHelper.PiOver2 * i;
                Point checkPos = (NPC.Center + new Vector2((NPC.width * 0.5f) + 1, 0).RotatedBy(rot)).ToTileCoordinates();
                if (ParanoidTileRetrieval(checkPos.X, checkPos.Y).IsTileSolidGround(true))
                {
                    NPC.ai[3] = i;
                    NPC.ai[2] = 0;
                    NPC.noGravity = true;
                    NPC.velocity += new Vector2(16f, 0).RotatedBy(rot);
                    if (i % 2 == 0)
                        NPC.collideX = true;
                    else
                        NPC.collideY = true;
                    break;
                }
            }
        }
        public override void AI()
        {
            if (NPC.noGravity)
                NPC.rotation += NPC.velocity.Length() * NPC.direction * 0.04f;

            int attackTelegraph = 60;
            int attackShootCooldown = 8;
            int attackDuration = 120;
            modNPC.RogueTumbletwigAI(NPC, 2.5f, 0.02f, 400f, ModContent.ProjectileType<WoodSliver>(), 10f, NPC.damage, attackTelegraph, attackDuration, attackShootCooldown, 240, MathHelper.PiOver4 * 0.2f);
            if ((NPC.ai[0] - attackTelegraph) % attackShootCooldown == 0 && NPC.ai[0] <= attackTelegraph + attackDuration && NPC.ai[0] >= attackTelegraph)
            {
                SoundEngine.PlaySound(SoundID.Item39 with { Volume = 1f, MaxInstances = 8 }, NPC.Center);
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage/ (double)NPC.lifeMax * 30.0; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.WoodFurniture, hit.HitDirection, -1f, 0, default(Color), 1.4f);
                }
            }
            else
            {
                for (int i = 0; i < 60; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.WoodFurniture, 2 * hit.HitDirection, -2f, 0, default(Color), 1.4f);
                }
            }
        }
        public override void FindFrame(int frameHeight)
        {
            int currentFrame = 0;
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, TextureAssets.Npc[modNPCID].Value.Width, frameHeight);
        }
        public override bool? CanFallThroughPlatforms()
        {
            return true;
        }

    }
}
