using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;
using TerRoguelike.World;
using Microsoft.Xna.Framework;
using TerRoguelike.Items.Rare;
using TerRoguelike.Player;
using Terraria.Audio;
using Terraria.ID;
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
        public override bool PreAI(Projectile projectile)
        {
            extraBounces = 0;
            return true;
        }
        public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
        {
            Terraria.Player player = Main.player[projectile.owner];
            TerRoguelikePlayer modPlayer = player.GetModPlayer<TerRoguelikePlayer>();

            if (TerRoguelikeWorld.IsTerRoguelikeWorld)
            {
                modifiers.DamageVariationScale *= 0;
            }

            
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
            TerRoguelikePlayer modPlayer = Main.player[projectile.owner].GetModPlayer<TerRoguelikePlayer>();
            if (modPlayer.volatileRocket > 0 && procChainBools.originalHit && projectile.type != ModContent.ProjectileType<Explosion>())
            {
                SpawnExplosion(projectile, modPlayer, crit: hit.Crit);
            }
        }
        public override void Kill(Projectile projectile, int timeLeft)
        {
            TerRoguelikePlayer modPlayer = Main.player[projectile.owner].GetModPlayer<TerRoguelikePlayer>();
            if (modPlayer.volatileRocket > 0 && projectile.penetrate > 1 && procChainBools.originalHit && projectile.type != ModContent.ProjectileType<Explosion>())
            {
                SpawnExplosion(projectile, modPlayer, true);
            }
        }
        public void SpawnExplosion(Projectile projectile, TerRoguelikePlayer modPlayer, bool originalHit = false, bool crit = false)
        {
            int spawnedProjectile = Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.Center, Vector2.Zero, ModContent.ProjectileType<Explosion>(), (int)(projectile.damage * 0.6f), 0f, projectile.owner);
            Main.projectile[spawnedProjectile].scale = 1f * modPlayer.volatileRocket;
            TerRoguelikeGlobalProjectile modProj = Main.projectile[spawnedProjectile].GetGlobalProjectile<TerRoguelikeGlobalProjectile>();
            modProj.procChainBools = new ProcChainBools(projectile.GetGlobalProjectile<TerRoguelikeGlobalProjectile>().procChainBools);
            if (!originalHit)
                modProj.procChainBools.originalHit = false;
            if (crit)
                modProj.procChainBools.critPreviously = true;

            SoundEngine.PlaySound(SoundID.Item14 with { Volume = SoundID.Item41.Volume * 0.5f }, projectile.Center);
        }
        public void HomingAI(Projectile projectile, float homingStrength)
        {
            if (homingCheckCooldown > 0)
            {
                homingCheckCooldown--;
                return;
            }

            if (projectile.velocity == Vector2.Zero)
                return;

            int projIndex = projectile.whoAmI;
            

            if (homingTarget != -1)
            {
                if (!Main.npc[homingTarget].active)
                    homingTarget = -1;
            }
            if (homingTarget == -1)
            {
                float prefferedDistance = 160f;
                List<float> npcHomingRating = new List<float>(new float[Main.maxNPCs]);
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.CanBeChasedBy(null, false))
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
                        npcHomingRating[i] += 1f - (distance / 1000);
                    }
                }
                homingCheckCooldown = 10;

                if (npcHomingRating.All(x => x == -10f))
                    return;

                homingTarget = npcHomingRating.FindIndex(x => x == npcHomingRating.Max());

                if (!Main.npc[homingTarget].CanBeChasedBy(null, false))
                {
                    homingTarget = -1;
                    return;
                }
            }

            Vector2 realDistanceVect = Main.npc[homingTarget].Center - projectile.Center;
            float targetAngle = Math.Abs(projectile.velocity.ToRotation() - realDistanceVect.ToRotation());
            float setAngle = homingStrength * MathHelper.TwoPi;

            if (setAngle > targetAngle)
                setAngle = targetAngle;


            if (Vector2.Dot(Vector2.Normalize(projectile.velocity).RotatedBy(MathHelper.PiOver2), Vector2.Normalize(realDistanceVect)) < 0)
                setAngle *= -1;

            setAngle += projectile.velocity.ToRotation();

            Vector2 setVelocity = setAngle.ToRotationVector2() * projectile.velocity.Length();


            Main.projectile[projIndex].velocity = setVelocity;
        }
    }
    public class ProcChainBools
    {
        public ProcChainBools() { }
        public ProcChainBools(ProcChainBools procChainBools)
        {
            originalHit = procChainBools.originalHit;
            critPreviously = procChainBools.critPreviously;
            clinglyGrenadePreviously = procChainBools.clinglyGrenadePreviously;
        }
        public bool originalHit = true;
        public bool critPreviously = false;
        public bool clinglyGrenadePreviously = false;
        
    }
}
