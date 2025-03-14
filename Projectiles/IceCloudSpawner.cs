using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using static TerRoguelike.Managers.TextureManager;
using TerRoguelike.Managers;
using TerRoguelike.Particles;
using ReLogic.Utilities;
using Terraria.Audio;
using Terraria.ID;
using TerRoguelike.Systems;

namespace TerRoguelike.Projectiles
{
    public class IceCloudSpawner : ModProjectile, ILocalizedModType
    {
        public override string Texture => "TerRoguelike/Projectiles/InvisibleProj";
        public int maxTimeLeft;
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.timeLeft = maxTimeLeft = 570;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
        }
        public override void OnSpawn(IEntitySource source)
        {
            
        }
        public override void AI()
        {
            if (!TerRoguelike.mpClient && (int)Projectile.velocity.Length() > 0 && Projectile.timeLeft > 90 && Projectile.timeLeft % (int)(Projectile.velocity.Length() * 0.5f) == 0 && !ParanoidTileRetrieval(Projectile.Center.ToTileCoordinates()).IsTileSolidGround(true))
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity * 0.04f, ModContent.ProjectileType<IceCloud>(), Projectile.damage, 0, -1, Projectile.timeLeft + ((maxTimeLeft - Projectile.timeLeft) * 0.75f));
            }
        }
        public override bool? CanDamage() => false;
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.velocity = oldVelocity;
            return Projectile.timeLeft < 440;
        }
    }
}
