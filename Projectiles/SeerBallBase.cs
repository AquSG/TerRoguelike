using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TerRoguelike.Managers;
using TerRoguelike.Systems;
using TerRoguelike.TerPlayer;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.Audio;

namespace TerRoguelike.Projectiles
{
    public class SeerBallBase : ModProjectile, ILocalizedModType
    {
        public override string Texture => "TerRoguelike/Projectiles/InvisibleProj";
        public float MaxScale = -1f;
        public Texture2D texture;
        public float startVelocity;
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.timeLeft = 2100;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
        }
        public override void OnSpawn(IEntitySource source)
        {
            if (Projectile.ai[1] == 0)
                Projectile.ai[1] = 1;
            startVelocity = Projectile.velocity.Length();

            //scale support
            MaxScale = Projectile.ai[1];
            Projectile.position = Projectile.Center + new Vector2(-(int)(Projectile.width * 0.5f) * MaxScale, -(int)(Projectile.height * 0.5f) * MaxScale);
            Projectile.width = (int)(Projectile.width * MaxScale);
            Projectile.height = (int)(Projectile.height * MaxScale);
        }
        public override void AI()
        {
            if (Projectile.ai[2] > 0)
            {
                Projectile.velocity = Vector2.Zero;
                Projectile.ai[2]--;
                if (Projectile.ai[2] <= 0)
                {
                    Projectile.tileCollide = true;
                    Projectile.velocity = Projectile.rotation.ToRotationVector2() * startVelocity;
                }
            }
            if (Projectile.localAI[0] > 0)
                Projectile.localAI[0]--;
        }
        public override bool? CanHitNPC(NPC target) => false;
        public override bool CanHitPlayer(Player target) => false;
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // If the projectile hits the left or right side of the tile, reverse the X velocity
            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
            {
                Projectile.velocity.X = -oldVelocity.X;
            }
            // If the projectile hits the top or bottom side of the tile, reverse the Y velocity
            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
            {
                Projectile.velocity.Y = -oldVelocity.Y;
            }
            SpawnProjectiles();
            if (Projectile.ai[0] >= 5)
                Projectile.active = false;
            return Projectile.ai[0] >= 5;
        }
        public void SpawnProjectiles()
        {
            if (Projectile.localAI[0] > 0)
                return;

            Projectile.ai[0]++;
            SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact with { Volume = 1f }, Projectile.Center);
            int projCount = 10;
            for (int i = 0; i < projCount; i++)
            {
                float rot = (float)i / projCount * MathHelper.TwoPi;
                Vector2 offset = rot.ToRotationVector2() * Projectile.width * 0.5f;
                Vector2 velocity = rot.ToRotationVector2() * 8f;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + offset, velocity, ModContent.ProjectileType<BloodOrb>(), Projectile.damage, 0f, -1, 1);
            }

            Projectile.localAI[0] = 12;
        }
    }
}
