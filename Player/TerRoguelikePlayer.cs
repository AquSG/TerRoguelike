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
using TerRoguelike.NPCs;

namespace TerRoguelike.Player
{
    public class TerRoguelikePlayer : ModPlayer
    {
        #region Variables
        public int commonCombatItem;
        public int clingyGrenade;
        public int criticalSights;
        public int regenerationCrystal;
        public int soulstealCoating;
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
            regenerationCrystal = 0;
            soulstealCoating = 0;
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
            if (regenerationCrystal > 0)
            {
                int regenIncrease = regenerationCrystal * 4;
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

            if (evilEye > 0)
            {
                Player.GetCritChance(DamageClass.Generic) += 5;
                if (evilEyeStacks.Any())
                {
                    for (int i = 0; i < evilEyeStacks.Count; i++)
                    {
                        evilEyeStacks[i]--;
                    }

                    evilEyeStacks.RemoveAll(time => time <= 0);

                    Player.GetAttackSpeed(DamageClass.Generic) += MathHelper.Clamp(evilEyeStacks.Count(), 1, 4 + evilEye) * 0.1f * (float)evilEye;
                }
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
            TerRoguelikeGlobalProjectile modProj = proj.GetGlobalProjectile<TerRoguelikeGlobalProjectile>();

            if (target.life <= 0)
                OnKillEffects(proj, target, hit, damageDone);

            if (clingyGrenade > 0 && !modProj.procChainBools.clinglyGrenadePreviously)
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
                    TerRoguelikeGlobalProjectile spawnedModProj = Main.projectile[spawnedProjectile].GetGlobalProjectile<TerRoguelikeGlobalProjectile>();

                    spawnedModProj.procChainBools = modProj.procChainBools;
                    spawnedModProj.procChainBools.originalHit = false;
                    spawnedModProj.procChainBools.clinglyGrenadePreviously = true;
                    if (hit.Crit)
                        spawnedModProj.procChainBools.critPreviously = true;
                }
            }
            if (evilEye > 0 && hit.Crit)
            {
                if (evilEyeStacks == null)
                    evilEyeStacks = new List<int>();

                evilEyeStacks.Add(180);
            }
        }
        public void OnKillEffects(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (soulstealCoating > 0 && !target.GetGlobalNPC<TerRoguelikeGlobalNPC>().activatedSoulstealCoating)
            {
                int healingAmt = (int)(Main.player[proj.owner].statLifeMax2 * soulstealCoating * 0.1f);
                Projectile.NewProjectile(Projectile.GetSource_None(), target.Center, Vector2.Zero, ModContent.ProjectileType<SoulstealHealingOrb>(), 0, 0f, Player.whoAmI, healingAmt);
                target.GetGlobalNPC<TerRoguelikeGlobalNPC>().activatedSoulstealCoating = true;
            }
        }
    }
}
