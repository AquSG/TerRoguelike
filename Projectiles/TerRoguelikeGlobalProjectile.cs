using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;

namespace TerRoguelike.Projectiles
{
    public class TerRoguelikeGlobalProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public bool originalHit = true;
        public bool critPreviously = false;
        public bool clingyGrenadePreviously = false;
        public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.DamageVariationScale *= 0;

            if (critPreviously)
                modifiers.SetCrit();
            else if (!originalHit)
            {
                modifiers.DisableCrit();
            }
        }
    }
}
