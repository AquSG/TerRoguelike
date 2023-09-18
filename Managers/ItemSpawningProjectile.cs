using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerRoguelike.NPCs;
using TerRoguelike.Systems;
using TerRoguelike.Managers;
using Terraria.Chat;

namespace TerRoguelike.Managers
{
    public class ItemSpawningProjectile : ModProjectile, ILocalizedModType
    {
        public override string Texture => "TerRoguelike/Projectiles/InvisibleProj";
        public ref float telegraphDuration =>  ref Projectile.ai[0];
        public ref float effectScale => ref Projectile.ai[1];
        public ref float item => ref Projectile.ai[2];
        public int dustID = -1;
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
            if (dustID == -1)
            {
                if (Projectile.knockBack == 0)
                {
                    dustID = DustID.Firework_Blue;
                }
                else if (Projectile.knockBack == 1)
                {
                    dustID = DustID.Firework_Green;
                }
                else
                {
                    dustID = DustID.Firework_Red;
                }
            }

            Dust.NewDustDirect(Projectile.position - new Vector2(15f*effectScale, 15f*effectScale), (int)(30*effectScale), (int)(30*effectScale), dustID, Scale: 0.5f);
            telegraphDuration--;
            if (telegraphDuration == 0)
            {
                effectScale *= 2f;
                int spawnedItem = Item.NewItem(Item.GetSource_NaturalSpawn(), new Rectangle((int)Projectile.position.X, (int)Projectile.position.Y, Projectile.width, Projectile.height), (int)item);
                for (int i = 0; i < 15; i++)
                {
                    Dust dust = Dust.NewDustDirect(Projectile.position - new Vector2(15f * effectScale, 15f * effectScale), (int)(30 * effectScale), (int)(30 * effectScale), dustID, Scale: 0.75f);
                    dust.noGravity = true;
                }
                Projectile.Kill();
            }
        }

        public override bool? CanDamage() => false;
    }
}
