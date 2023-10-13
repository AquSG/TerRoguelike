using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using TerRoguelike.World;

namespace TerRoguelike.ILEditing
{
    public class ILEdits : ModSystem
    {
        public override void OnModLoad()
        {
            On_Main.DamageVar_float_int_float += AdjustDamageVariance;
        }
        private int AdjustDamageVariance(On_Main.orig_DamageVar_float_int_float orig, float dmg, int percent, float luck)
        {
            // Change the default damage variance from +-15% to +-5%.
            // If other mods decide to change the scale, they can override this. We're solely killing the default value.
            if (percent == Main.DefaultDamageVariationPercent && TerRoguelikeWorld.IsTerRoguelikeWorld)
                percent = 0;
            // Remove the ability for luck to affect damage variance by setting it to 0 always.
            return orig(dmg, percent, 0f);
        }
    }
}
