using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerRoguelike.Managers
{
    public class EnemySpawningProjectile : ModProjectile, ILocalizedModType
    {
        public override string Texture => "TerRoguelike/Projectiles/InvisibleProj";
        public ref float telegraphDuration =>  ref Projectile.ai[0];
        public ref float effectScale => ref Projectile.ai[1];
        public ref float npc => ref Projectile.ai[2];
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 1;
            Projectile.friendly = true;
            Projectile.damage = 0;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 3000;
        }
        public override void AI()
        {
            Dust.NewDustDirect(Projectile.position - new Vector2(15f*effectScale, 15f*effectScale), (int)(30*effectScale), (int)(30*effectScale), DustID.CrystalPulse, Scale: 0.5f);
            telegraphDuration--;
            if (telegraphDuration == 0)
            {
                effectScale *= 2f;
                NPC.NewNPC(Projectile.GetSource_FromThis(), (int)Projectile.position.X, (int)Projectile.position.Y, (int)npc);
                for (int i = 0; i < 15; i++)
                {
                    Dust dust = Dust.NewDustDirect(Projectile.position - new Vector2(15f * effectScale, 15f * effectScale), (int)(30 * effectScale), (int)(30 * effectScale), DustID.CrystalPulse, Scale: 1f);
                    dust.noGravity = true;
                }
                Projectile.Kill();
            }
        }
    }
}
