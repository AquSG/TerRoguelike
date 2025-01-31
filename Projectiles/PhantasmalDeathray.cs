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
using TerRoguelike.Items.Common;
using TerRoguelike.Utilities;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;

namespace TerRoguelike.Projectiles
{
    public class PhantasmalDeathray : ModProjectile, ILocalizedModType
    {
        // this projectile exists for PlayerDeathReason. when the deathray should hit (calculatd in Moon Lord CanHit) then this projectile is spawned on the player for 1 frame to hit them and be used as a potential damage source.
        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 1;
            Projectile.penetrate = -1;
            Projectile.hide = true;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 2;
        }
    }
}
