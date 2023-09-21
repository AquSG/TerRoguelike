using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using TerRoguelike.Managers;
using TerRoguelike.Systems;
using TerRoguelike.Rooms;
using Microsoft.Xna.Framework;
using TerRoguelike.Projectiles;

namespace TerRoguelike.Player
{
    public class TerRoguelikePlayer : ModPlayer
    {
        #region Variables
        public int commonCombatItem;
        public int clingyGrenade;
        public int criticalSights;
        public int commonHealingItem;
        public int commonUtilityItem;
        public int uncommonCombatItem;
        public int evilEye;
        public int uncommonHealingItem;
        public int uncommonUtilityItem;
        public int rareCombatItem;
        public int rareHealingItem;
        public int rareUtilityItem;
        public List<int> evilEyeStacks = new List<int>();
        #endregion
        public override void PreUpdate()
        {
            commonCombatItem = 0;
            clingyGrenade = 0;
            criticalSights = 0;
            commonHealingItem = 0;
            commonUtilityItem = 0;
            uncommonCombatItem = 0;
            evilEye = 0;
            uncommonHealingItem = 0;
            uncommonUtilityItem = 0;
            rareCombatItem = 0;
            rareHealingItem = 0;
            rareUtilityItem = 0;
        }
        public override void UpdateEquips()
        {
            Player.noFallDmg = true;
            Player.GetCritChance(DamageClass.Generic) -= 3f;

            if (commonCombatItem > 0)
            {
                float damageIncrease = commonCombatItem * 0.05f;
                //Player.GetDamage(DamageClass.Generic) += damageIncrease;
                Player.GetAttackSpeed(DamageClass.Generic) += damageIncrease;
            }
            if (criticalSights > 0)
            {
                float critIncrease = criticalSights * 10f;
                Player.GetCritChance(DamageClass.Generic) += critIncrease;
            }
            if (commonHealingItem > 0)
            {
                int regenIncrease = commonHealingItem;
                Player.lifeRegen += regenIncrease;
            }
            if (commonUtilityItem > 0)
            {
                float speedIncrease = commonUtilityItem * 0.05f;
                Player.moveSpeed += speedIncrease;
            }
            if (uncommonCombatItem > 0)
            {
                float damageIncrease = uncommonCombatItem * 0.15f;
                Player.GetDamage(DamageClass.Generic) += damageIncrease;
            }

            if (evilEye > 0 && evilEyeStacks.Any())
            {
                for (int i = 0; i < evilEyeStacks.Count; i++)
                {
                    evilEyeStacks[i]--;
                }

                evilEyeStacks.RemoveAll(time => time <= 0);

                Player.GetAttackSpeed(DamageClass.Generic) += MathHelper.Clamp(evilEyeStacks.Count(), 1, 4 + evilEye) * 0.1f * (float)evilEye;
            }
            else if (evilEyeStacks.Any())
                evilEyeStacks.Clear();

            if (uncommonHealingItem > 0)
            {
                int regenIncrease = uncommonHealingItem * 3;
                Player.lifeRegen += regenIncrease;
            }
            if (uncommonUtilityItem > 0)
            {
                float speedIncrease = uncommonUtilityItem * 0.15f;
                Player.moveSpeed += speedIncrease;
            }
            if (rareCombatItem > 0)
            {
                float damageIncrease = rareCombatItem * 0.60f;
                Player.GetDamage(DamageClass.Generic) += damageIncrease;
            }
            if (rareHealingItem > 0)
            {
                int regenIncrease = rareHealingItem * 12;
                Player.lifeRegen += regenIncrease;
            }
            if (rareUtilityItem > 0)
            {
                float speedIncrease = rareUtilityItem * 0.60f;
                Player.moveSpeed += speedIncrease;
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (clingyGrenade > 0 && !proj.GetGlobalProjectile<TerRoguelikeGlobalProjectile>().clingyGrenadePreviously)
            {
                int chance;
                chance = clingyGrenade * 5;
                if (chance > Main.rand.Next(1, 101))
                {
                    float radius;
                    if (target.width < target.height)
                        radius = (float)target.width;
                    else
                        radius = (float)target.height;

                    radius *= 0.4f;

                    Vector2 direction = (proj.Center - target.Center).SafeNormalize(Vector2.UnitY);
                    Vector2 spawnPosition = (direction * radius) + target.Center;
                    int damage = (int)(hit.Damage * 1.5f);
                    if (hit.Crit)
                        damage /= 2;

                    int spawnedProjectile = Projectile.NewProjectile(Projectile.GetSource_None(), spawnPosition, Vector2.Zero, ModContent.ProjectileType<ClingyGrenade>(), damage, 0f, proj.owner, target.whoAmI);
                    if (hit.Crit)
                        Main.projectile[spawnedProjectile].GetGlobalProjectile<TerRoguelikeGlobalProjectile>().critPreviously = true;
                    Main.projectile[spawnedProjectile].GetGlobalProjectile<TerRoguelikeGlobalProjectile>().clingyGrenadePreviously = true;
                    Main.projectile[spawnedProjectile].GetGlobalProjectile<TerRoguelikeGlobalProjectile>().originalHit = false;

                }
            }
            if (evilEye > 0 && hit.Crit)
            {
                if (evilEyeStacks == null)
                    evilEyeStacks = new List<int>();

                evilEyeStacks.Add(180);
            }
        }
    }
}
