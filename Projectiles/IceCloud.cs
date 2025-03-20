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
using TerRoguelike.Systems;

namespace TerRoguelike.Projectiles
{
    public class IceCloud : ModProjectile, ILocalizedModType
    {
        //almost everything in this is just visuals. the hitbox is active for 1/4 of a second after 30 frames pass, and is a big square
        public override string Texture => "TerRoguelike/Projectiles/InvisibleProj";
        public float randomSmokeRotation = -100f; // purely visual
        public float MaxScale = -1f;
        public Texture2D smokeTex;
        public int maxTimeLeft = 360;
        public int direction;
        public override void SetDefaults()
        {
            Projectile.width = 100;
            Projectile.height = 100;
            Projectile.timeLeft = maxTimeLeft;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            smokeTex = TexDict["Smoke"];
        }

        public override void OnSpawn(IEntitySource source)
        {
            //scale support
            MaxScale = Projectile.scale * 1f;
            Projectile.position = Projectile.Center + new Vector2(-50 * MaxScale, -50 * MaxScale);
            Projectile.width = (int)(Projectile.width * MaxScale);
            Projectile.height = (int)(Projectile.height * MaxScale);

            randomSmokeRotation = Main.rand.NextFloatDirection();

            direction = Math.Sign(Projectile.velocity.X);
            Projectile.timeLeft = maxTimeLeft = (int)Projectile.ai[0];
            Projectile.rotation = Projectile.velocity.ToRotation() - (MathHelper.PiOver2 * direction * 0.16f);
            SpawnSmokeParticles();
            Projectile.localAI[0] = 1;
        }
        public override void AI()
        {
            if (Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = 1;
                MaxScale = Projectile.scale * 1f;
            }
            var modProj = Projectile.ModProj();
            if (maxTimeLeft - Projectile.timeLeft >= 30 && Projectile.timeLeft > 120 && modProj != null && modProj.npcOwner >= 0 && Main.npc[modProj.npcOwner].ModNPC().sourceRoomListID >= 0 && RoomSystem.RoomList[Main.npc[modProj.npcOwner].ModNPC().sourceRoomListID].bossDead)
                Projectile.timeLeft = 120;


            Projectile.velocity *= 0.98f;
            if (Projectile.timeLeft >= 90)
            {
                if (Projectile.timeLeft % 20 == 0)
                    SpawnSmokeParticles();
                if (Projectile.timeLeft % 10 == 0 && Main.rand.NextBool(6))
                {
                    Vector2 spawnPos = Projectile.BottomLeft + new Vector2(Main.rand.Next(Projectile.width), 0);
                    if (!ParanoidTileRetrieval(spawnPos.ToTileCoordinates()).IsTileSolidGround(true))
                    {
                        ParticleManager.AddParticle(new Snow(
                        spawnPos, Vector2.UnitY * Main.rand.NextFloat(2, 3),
                        600, Color.Cyan * 0.55f, new Vector2(Main.rand.NextFloat(0.03f, 0.04f)), Main.rand.NextFloat(MathHelper.TwoPi), 0.96f, 0.04f, 180, 0));
                    }
                }
            }

        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CircularHitboxCollision(Projectile.Center, targetHitbox, Projectile.width / 2);
        public override bool? CanDamage() => Projectile.timeLeft <= maxTimeLeft - 20 && Projectile.timeLeft >= 50 ? (bool?)null : false;
        public override bool PreDraw(ref Color lightColor)
        {
            StartAdditiveSpritebatch();

            Vector2 smokeOffset = Vector2.UnitY * -16;
            Color smokeColor = Color.LightCyan * 0.86f;

            float smokeOpacity = 1f;
            if (Projectile.timeLeft > maxTimeLeft - 30)
            {
                smokeOpacity = MathHelper.Lerp(0f, 1f, MathHelper.Clamp((maxTimeLeft - Projectile.timeLeft) / 30f, 0, 1f));
            }
            else if (Projectile.timeLeft < 60)
            {
                smokeOpacity = MathHelper.Lerp(0f, 1f, MathHelper.Clamp(Projectile.timeLeft / 60f, 0, 1f));
            }
            smokeOpacity = (float)Math.Sqrt(smokeOpacity);
            Main.EntitySpriteDraw(smokeTex, Projectile.Center - Main.screenPosition + smokeOffset, null, smokeColor * smokeOpacity * 0.6f, Projectile.rotation, smokeTex.Size() * 0.5f, MaxScale * 0.45f + (smokeOpacity * 0.5f), direction < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
            Main.EntitySpriteDraw(smokeTex, Projectile.Center - Main.screenPosition + smokeOffset + new Vector2(0, 16), null, smokeColor * smokeOpacity * 0.75f, randomSmokeRotation + Projectile.rotation, smokeTex.Size() * 0.5f, (MaxScale * 0.5f) + (smokeOpacity * 0.5f),  direction < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);

            StartVanillaSpritebatch();

            return false;
        }
        public void SpawnSmokeParticles()
        {
            int lifetime = 120;
            float scaleMulti = 1f;
            if (Projectile.timeLeft < 120)
            {
                lifetime -= 120 - Projectile.timeLeft;
                scaleMulti *= 1f - ((120 - Projectile.timeLeft) / 90f);
            }
            ParticleManager.AddParticle(new Smoke(
                    Projectile.Center, Main.rand.NextVector2CircularEdge(2f, 2f) * Main.rand.NextFloat(0.6f, 0.86f), lifetime, Color.Cyan * 0.8f, new Vector2(0.5f) * scaleMulti,
                    Main.rand.Next(15), Main.rand.NextFloat(MathHelper.TwoPi), Main.rand.NextBool() ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0.98f));
        }
    }
}
