using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerRoguelike.NPCs;
using TerRoguelike.TerPlayer;
using TerRoguelike.World;
using static TerRoguelike.Managers.NPCManager;
using static TerRoguelike.Utilities.TerRoguelikeUtils;

namespace TerRoguelike.Projectiles
{
    public class TerRoguelikeGlobalProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public ProcChainBools procChainBools = new ProcChainBools();
        public int homingTarget = -1;
        public int extraBounces = 0;
        public int bounceCount = 0;
        public int homingCheckCooldown = 0;
        public int swingDirection = 0;
        public bool ultimateCollideOverride = false;
        public int npcOwner = -1;
        public int npcOwnerType = -1;
        public override bool PreAI(Projectile projectile)
        {
            extraBounces = 0; // set bounces in projectile ai.
            return true;
        }
        public override bool? Colliding(Projectile projectile, Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (ultimateCollideOverride)
            {
                ultimateCollideOverride = false;
                return true;
            }
            return null;
        }
        public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
        {
            Player player = Main.player[projectile.owner];
            TerRoguelikePlayer modPlayer = player.GetModPlayer<TerRoguelikePlayer>();

            if (TerRoguelikeWorld.IsTerRoguelikeWorld)
            {
                modifiers.DamageVariationScale *= 0;
            }

            
            //Crit inheritance and custom crit chance supported by proc luck
            if (procChainBools.critPreviously)
                modifiers.SetCrit();
            else if (!procChainBools.originalHit)
            {
                modifiers.DisableCrit();
            }
            else
            {
                float critChance = projectile.CritChance * 0.01f;
                if (ChanceRollWithLuck(critChance, modPlayer.procLuck))
                {
                    modifiers.SetCrit();
                }
                else
                {
                    modifiers.DisableCrit();
                }
            }
        }
        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (projectile.hostile || !projectile.friendly)
                return;

            TerRoguelikePlayer modPlayer = Main.player[projectile.owner].GetModPlayer<TerRoguelikePlayer>();
            if (modPlayer.volatileRocket > 0 && procChainBools.originalHit && projectile.type != ModContent.ProjectileType<Explosion>())
            {
                SpawnExplosion(projectile, modPlayer, target, crit: hit.Crit);
            }
        }
        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            if (source is EntitySource_Parent parentSource)
            {
                if (parentSource.Entity is NPC)
                {
                    NPC npc = Main.npc[parentSource.Entity.whoAmI];
                    npcOwner = npc.whoAmI;
                    npcOwnerType = npc.type;

                    TerRoguelikeGlobalNPC modNPC = npc.GetGlobalNPC<TerRoguelikeGlobalNPC>();
                    if (modNPC.hostileTurnedAlly || npc.friendly)
                    {
                        projectile.friendly = true;
                        projectile.hostile = false;
                    }
                    else
                    {
                        projectile.friendly = false;
                        projectile.hostile = true;
                        projectile.damage /= 2;
                    }
                }
                else if (parentSource.Entity is Projectile)
                {
                    Projectile parentProj = Main.projectile[parentSource.Entity.whoAmI];
                    TerRoguelikeGlobalProjectile parentModProj = parentProj.GetGlobalProjectile<TerRoguelikeGlobalProjectile>();
                    npcOwner = parentModProj.npcOwner;
                    npcOwnerType = parentModProj.npcOwnerType;
                    if (npcOwnerType != -1)
                    {
                        if (AllNPCs.Exists(x => x.modNPCID == npcOwnerType))
                        {
                            projectile.hostile = parentProj.hostile;
                            projectile.friendly = parentProj.friendly;
                        }
                    }
                }
            }
        }
        public override void OnKill(Projectile projectile, int timeLeft)
        {
            if (projectile.hostile || !projectile.friendly)
                return;

            TerRoguelikePlayer modPlayer = Main.player[projectile.owner].GetModPlayer<TerRoguelikePlayer>();
            if (modPlayer.volatileRocket > 0 && projectile.penetrate > 1 && procChainBools.originalHit && projectile.type != ModContent.ProjectileType<Explosion>())
            {
                SpawnExplosion(projectile, modPlayer, originalHit: true); //Explosions not spawned from hits are counted as original hits, to calculate crit themselves.
            }
        }
        public void SpawnExplosion(Projectile projectile, TerRoguelikePlayer modPlayer, NPC target = null, bool originalHit = false, bool crit = false)
        {
            Vector2 position = projectile.Center;
            if (target != null)
            {
                position = target.GetGlobalNPC<TerRoguelikeGlobalNPC>().SpecialProjectileCollisionRules ? projectile.Center + (Vector2.UnitX * (projectile.width > projectile.height ? projectile.height * 0.5f : projectile.height * 0.5f)).RotatedBy(projectile.rotation) : target.getRect().ClosestPointInRect(projectile.Center);
            }
            int spawnedProjectile = Projectile.NewProjectile(projectile.GetSource_FromThis(), position, Vector2.Zero, ModContent.ProjectileType<Explosion>(), (int)(projectile.damage * 0.6f), 0f, projectile.owner);
            Main.projectile[spawnedProjectile].scale = 1f * modPlayer.volatileRocket;
            TerRoguelikeGlobalProjectile modProj = Main.projectile[spawnedProjectile].GetGlobalProjectile<TerRoguelikeGlobalProjectile>();
            modProj.procChainBools = new ProcChainBools(projectile.GetGlobalProjectile<TerRoguelikeGlobalProjectile>().procChainBools);
            if (!originalHit)
                modProj.procChainBools.originalHit = false;
            if (crit)
                modProj.procChainBools.critPreviously = true;

            SoundEngine.PlaySound(SoundID.Item14 with { Volume = SoundID.Item41.Volume * 0.5f }, projectile.Center);
        }
        public void HomingAI(Projectile projectile, float homingStrength, bool idleSpin = false)
        {
            if (projectile.velocity == Vector2.Zero)
                return;
            
            if (homingTarget != -1)
            {
                if (!Main.npc[homingTarget].CanBeChasedBy() || Main.npc[homingTarget].life <= 0) //reset homing target if it's gone
                    homingTarget = -1;
            }

            if (homingTarget == -1 && idleSpin)
            {
                projectile.velocity = projectile.velocity.RotatedBy(homingStrength * MathHelper.TwoPi);
            }

            if (homingCheckCooldown > 0) //cooldown on homing checks as an attempt to stave off lag
            {
                homingCheckCooldown--;
                return;
            }

            if (homingTarget == -1)
            {
                //create a list of each npc's homing rating relative to the projectile's position and velocity direction to try and choose the best target.
                float prefferedDistance = 640f;
                List<float> npcHomingRating = new List<float>(new float[Main.maxNPCs]);
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.CanBeChasedBy(null, false) || npc.life <= 0)
                    {
                        npcHomingRating[i] = -10;
                        continue;
                    }
                    Vector2 distanceVect = npc.Center - projectile.Center;
                    float distance = distanceVect.Length();
                    npcHomingRating[i] += Vector2.Dot(Vector2.Normalize(projectile.velocity), Vector2.Normalize(distanceVect));
                    if (distance < prefferedDistance)
                    {
                        npcHomingRating[i] += 1f;
                    }
                    else
                    {
                        npcHomingRating[i] += 1f - (distance / 1000f);
                    }
                }
                homingCheckCooldown = 10;

                if (npcHomingRating.All(x => x == -10f))
                    return;

                homingTarget = npcHomingRating.FindIndex(x => x == npcHomingRating.Max());

                if (!Main.npc[homingTarget].CanBeChasedBy(null, false) || Main.npc[homingTarget].life <= 0)
                {
                    homingTarget = -1;
                    return;
                }
            }

            float maxChange = homingStrength * MathHelper.TwoPi;

            projectile.velocity = (Vector2.UnitX * projectile.velocity.Length()).RotatedBy(projectile.velocity.ToRotation().AngleTowards((Main.npc[homingTarget].Center - projectile.Center).ToRotation(), maxChange));
        }
    }
    public class ProcChainBools
    {
        //proc chain bools, usually used to make spawned projectiles inherit what their parent projectiles have done in the past. prevents infinite proc chains, and allows crit inheritance.
        public ProcChainBools() { }
        public ProcChainBools(ProcChainBools procChainBools)
        {
            originalHit = procChainBools.originalHit;
            critPreviously = procChainBools.critPreviously;
            clinglyGrenadePreviously = procChainBools.clinglyGrenadePreviously;
            lockOnMissilePreviously = procChainBools.lockOnMissilePreviously;
        }
        public bool originalHit = true;
        public bool critPreviously = false;
        public bool clinglyGrenadePreviously = false;
        public bool lockOnMissilePreviously = false;
        
    }
}
