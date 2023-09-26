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
        public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
        {
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

                Main.NewText(Main.npc[homingTarget].FullName);
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
        public bool originalHit = true;
        public bool critPreviously = false;
        public bool clinglyGrenadePreviously = false;
    }
}
