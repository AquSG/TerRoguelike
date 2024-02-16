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

namespace TerRoguelike.NPCs.Enemy
{
    public class RockGolem : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<RockGolem>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Forest"] };
        public override int CombatStyle => 2;
        public int attackTelegraph = 20;
        public int attackExtend = 20;
        public int meleeDuration = 30;
        public int armRaiseTime = 0;
        public bool CollisionPass;
        public int meleeHitboxExtension = 20;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 21;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 36;
            NPC.height = 48;
            NPC.aiStyle = -1;
            NPC.damage = 24;
            NPC.lifeMax = 1200;
            NPC.HitSound = SoundID.NPCHit41;
            NPC.DeathSound = SoundID.NPCDeath43;
            NPC.knockBackResist = 0.1f;
            modNPC.drawCenter = new Vector2(0, -9);
        }
        public override void AI()
        {
            int attackCooldown = 90;
            NPC.frameCounter += NPC.velocity.Length() * 0.1d;
            modNPC.RogueRockGolemAI(NPC, 1f, -6f, 48f, meleeDuration, 60, 180f, attackTelegraph, attackCooldown, 0f, ProjectileID.RockGolemRock, 8f, -Vector2.UnitY * 16, NPC.damage, true, false, attackExtend);

            if (NPC.velocity.Y != 0 && !NPC.collideY)
                armRaiseTime++;
            else
                armRaiseTime = 0;
            
            if (NPC.ai[1] == attackTelegraph)
            {
                SoundEngine.PlaySound(SoundID.Item51 with { Volume = 0.6f }, NPC.Center);
            }
            if (NPC.ai[2] == 17)
            {
                SoundEngine.PlaySound(SoundID.Item1 with { Volume = 0.9f }, NPC.Center);
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            NPC.ai[0] = 0;
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)(NPC.lifeMax * 50); i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 1, hit.HitDirection, -1f);
                }
            }
            else
            {
                for (int i = 0; i < 30; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 1, 2.5f * (float)hit.HitDirection, -2.5f);
                }
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y), NPC.velocity, 1210, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 10f), NPC.velocity, 1211, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 1212, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 40f), NPC.velocity, 1213, NPC.scale);
            }
        }
        public override void FindFrame(int frameHeight)
        {
            int frameTotal = Main.npcFrameCount[modNPCID];
            int currentFrame;
            if (NPC.ai[1] > 0)
            {
                NPC.frameCounter = 0;
                currentFrame = frameTotal - 6 + ((int)NPC.ai[1] * 6 / (attackTelegraph + attackExtend));
            }
            else if (NPC.ai[2] > 0)
            {
                NPC.frameCounter = 0;
                currentFrame = frameTotal - 12 + ((int)NPC.ai[2] * 6 / meleeDuration);
            }
            else if (NPC.velocity.Y != 0)
            {
                currentFrame = frameTotal - 13 + (Math.Clamp(armRaiseTime, 0, 21) * 3 / 21);
            }
            else
            {
                currentFrame = (int)(NPC.frameCounter % (frameTotal - 13)) + 1;
            }
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, TextureAssets.Npc[modNPCID].Value.Width, frameHeight);
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            if (NPC.getRect().Intersects(target.getRect()))
            {
                CollisionPass = true;
                return true;
            }
            if (NPC.ai[2] > 20)
            {
                Rectangle extendedHitbox = NPC.getRect();
                if (NPC.direction == 1)
                {
                    extendedHitbox.Width += meleeHitboxExtension;
                }
                else
                {
                    extendedHitbox.X -= meleeHitboxExtension;
                    extendedHitbox.Width += meleeHitboxExtension;
                }
                if (extendedHitbox.Intersects(target.getRect()))
                {
                    CollisionPass = true;
                    return true;
                }
            }

            CollisionPass = false;
            return false;
        }
        public override bool CanHitNPC(NPC target)
        {
            if (NPC.getRect().Intersects(target.getRect()))
            {
                CollisionPass = true;
                return true;
            }
            if (NPC.ai[2] > 20)
            {
                Rectangle extendedHitbox = NPC.getRect();
                if (NPC.direction == 1)
                {
                    extendedHitbox.Width += meleeHitboxExtension;
                }
                else
                {
                    extendedHitbox.X -= meleeHitboxExtension;
                    extendedHitbox.Width += meleeHitboxExtension;
                }
                if (extendedHitbox.Intersects(target.getRect()))
                {
                    CollisionPass = true;
                    return true;
                }
            }

            CollisionPass = false;
            return false;
        }
        public override bool ModifyCollisionData(Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox)
        {
            if (CollisionPass)
            {
                npcHitbox = new Rectangle(0, 0, Main.maxTilesX * 16, Main.maxTilesY * 16);
            }
            return CollisionPass;
        }
    }
}
