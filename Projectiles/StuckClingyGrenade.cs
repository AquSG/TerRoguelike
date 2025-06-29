using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerRoguelike.Managers;
using TerRoguelike.Systems;
using TerRoguelike.TerPlayer;
using Microsoft.Xna.Framework.Graphics;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using System.Linq;
using Terraria.DataStructures;
using TerRoguelike.ILEditing;

namespace TerRoguelike.Projectiles
{
    public class StuckClingyGrenade : ModProjectile, ILocalizedModType
    {
        public int target = -1;
        public int stuckNPC = -1;
        public Vector2 stuckPosition = Vector2.Zero;
        public int stuckSegment = -1;
        public float origStuckRot = 0;
        public static int soundCooldown = 0;
        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 28;
            Projectile.timeLeft = 120;
            Projectile.rotation = Main.rand.NextFloatDirection();
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.scale = 0.85f;
            Projectile.spriteDirection = Main.rand.NextBool() ? -1 : 1;
        }
        public override void OnSpawn(IEntitySource source)
        {
            stuckNPC = (int)Projectile.ai[0];
            var stuckModNPC = Main.npc[stuckNPC].ModNPC();
            bool segments = stuckModNPC.Segments.Count > 0;
            stuckSegment = segments ? stuckModNPC.hitSegment : -1;
            stuckPosition = Projectile.Center - (segments ? stuckModNPC.Segments[stuckSegment].Position : Main.npc[stuckNPC].Center);

            float potentialRot = (Projectile.Center - (segments ? stuckModNPC.Segments[stuckSegment].Position : Main.npc[stuckNPC].Center)).ToRotation();
            origStuckRot = potentialRot - (segments ? stuckModNPC.Segments[stuckSegment].Rotation : Main.npc[stuckNPC].rotation);
        }
        public override void AI()
        {
            if (ILEdits.dualContrastTileShader)
            {
                Projectile.light = 0.75f;
            }

            float fallSpeedCap = 25f;
            float downwardsAccel = 0.3f;
            Projectile.gfxOffY = 0;

            if (stuckNPC != -1)
            {
                NPC npc = Main.npc[stuckNPC];
                bool destick = false;

                if (npc.ModNPC != null)
                {
                    if (npc.ModNPC.CanBeHitByProjectile(Projectile) == false)
                    {
                        if (npc.ModNPC().Segments.Count == 0)
                        {
                            Rectangle npcRect = npc.Hitbox;
                            MultipliableFloat f = new MultipliableFloat();
                            int immunitySlot = 0;
                            npc.ModNPC.ModifyCollisionData(Projectile.getRect(), ref immunitySlot, ref f, ref npcRect);
                            if (!Projectile.getRect().Intersects(npcRect))
                                destick = true;
                        }
                        else
                            destick = true;
                    }
                }
                else if (!npc.Hitbox.Intersects(Projectile.getRect()))
                    destick = true;

                if (!npc.active || npc.life <= 0 || npc.immortal || npc.dontTakeDamage || destick)
                {
                    stuckNPC = -1;
                    stuckPosition = Vector2.Zero;
                    stuckSegment = -1;
                }
            }

            
            if (stuckNPC == -1)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && npc.life > 0 && !npc.dontTakeDamage && !npc.immortal)
                    {
                        if (npc.ModNPC != null)
                        {
                            if (npc.ModNPC.CanBeHitByProjectile(Projectile) != false)
                            {
                                if (npc.ModNPC().Segments.Count == 0)
                                {
                                    Rectangle npcRect = npc.Hitbox;
                                    MultipliableFloat f = new MultipliableFloat();
                                    int immunitySlot = 0;
                                    npc.ModNPC.ModifyCollisionData(Projectile.getRect(), ref immunitySlot, ref f, ref npcRect);
                                    if (!Projectile.getRect().Intersects(npcRect))
                                        continue;
                                }
                                Projectile.ModProj().ultimateCollideOverride = false;
                                stuckNPC = i;
                                bool anySegments = npc.ModNPC().Segments.Count > 0;
                                if (anySegments)
                                {
                                    stuckSegment = npc.ModNPC().hitSegment;
                                }
                                Vector2 pos = anySegments ? npc.ModNPC().Segments[stuckSegment].Position : npc.Center;
                                stuckPosition = Projectile.Center - pos;
                                origStuckRot = stuckPosition.ToRotation() - (anySegments ? npc.ModNPC().Segments[stuckSegment].Rotation : Main.npc[stuckNPC].rotation);
                                break;
                            }
                        }
                        else if (npc.Hitbox.Intersects(Projectile.getRect()))
                        {
                            stuckNPC = i;
                            stuckPosition = Projectile.Center - npc.Center;
                        }
                    }
                }
            }
            
            if (stuckNPC != -1)
            {
                NPC npc = Main.npc[stuckNPC];
                bool anySegments = npc.ModNPC().Segments.Count > 0;
                Vector2 pos = anySegments ? npc.ModNPC().Segments[stuckSegment].Position : npc.Center;
                stuckPosition = (Vector2.UnitX * stuckPosition.Length()).RotatedBy((anySegments ? npc.ModNPC().Segments[stuckSegment].Rotation : npc.rotation) + origStuckRot);
                Projectile.Center = pos + stuckPosition;
                Projectile.gfxOffY = npc.gfxOffY;
                return;
            }

            if (Projectile.velocity.Y < fallSpeedCap)
                Projectile.velocity.Y += downwardsAccel;
            if (Projectile.velocity.Y > fallSpeedCap)
                Projectile.velocity.Y = fallSpeedCap;

        }

        public override bool PreKill(int timeLeft)
        {
            int spawnedProjectile = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<Explosion>(), Projectile.damage, 0f, Projectile.owner);
            Main.projectile[spawnedProjectile].scale = 1f;
            Main.projectile[spawnedProjectile].ModProj().procChainBools = Projectile.ModProj().procChainBools;

            if (soundCooldown <= 60)
            {
                SoundEngine.PlaySound(SoundID.Item110, Projectile.Center);
                soundCooldown += 10;
            }
            return true;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (stuckNPC == -1)
                Projectile.velocity = Vector2.Zero;

            return false;
        }
        public override bool? CanDamage() => false;
    }
}
