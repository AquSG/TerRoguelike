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

namespace TerRoguelike.Player
{
    public class TerRoguelikePlayer : ModPlayer
    {
        #region Variables
        public int commonCombatItem;
        public int commonHealingItem;
        public int commonUtilityItem;
        public int uncommonCombatItem;
        public int uncommonHealingItem;
        public int uncommonUtilityItem;
        public int rareCombatItem;
        public int rareHealingItem;
        public int rareUtilityItem;
        #endregion
        public override void PreUpdate()
        {
            commonCombatItem = 0;
            commonHealingItem = 0;
            commonUtilityItem = 0;
            uncommonCombatItem = 0;
            uncommonHealingItem = 0;
            uncommonUtilityItem = 0;
            rareCombatItem = 0;
            rareHealingItem = 0;
            rareUtilityItem = 0;
        }
        public override void UpdateEquips()
        {
            Player.noFallDmg = true;

            if (commonCombatItem > 0)
            {
                float damageIncrease = commonCombatItem * 0.05f;
                Player.GetDamage(DamageClass.Generic) += damageIncrease;
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
    }
}
