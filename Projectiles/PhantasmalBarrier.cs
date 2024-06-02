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
using SteelSeries.GameSense;

namespace TerRoguelike.Projectiles
{
    public class PhantasmalBarrier : ModProjectile, ILocalizedModType
    {
        //almost everything in this is just visuals. the hitbox is active for 1/4 of a second after 30 frames pass, and is a big square
        public override string Texture => "TerRoguelike/Projectiles/InvisibleProj";

        public int maxTimeLeft;
        public Texture2D glowTex = null;
        public override void SetDefaults()
        {
            Projectile.width = 160;
            Projectile.height = 160;
            Projectile.timeLeft = maxTimeLeft = 360;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
        }

        public override void OnSpawn(IEntitySource source)
        {
            //SpawnSmokeParticles();
        }
        public override void AI()
        {
            var modProj = Projectile.ModProj();
            if (maxTimeLeft - Projectile.timeLeft >= 30 && Projectile.timeLeft > 120 && modProj != null && modProj.npcOwner >= 0 && Main.npc[modProj.npcOwner].ModNPC().sourceRoomListID >= 0 && RoomSystem.RoomList[Main.npc[modProj.npcOwner].ModNPC().sourceRoomListID].bossDead)
                Projectile.timeLeft = 120;
            else if (Projectile.timeLeft > 120 && Projectile.timeLeft < 130)
                Projectile.timeLeft = 130;


            Projectile.velocity *= 0.98f;
            if ((int)Projectile.localAI[0] % 26 == 0)
                SpawnSmokeParticles();
            Projectile.localAI[0]++;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            glowTex ??= TexDict["CircularGlow"];
            Color color = Color.Cyan;
            StartAdditiveSpritebatch();
            Main.EntitySpriteDraw(glowTex, Projectile.Center - Main.screenPosition, null, color * 0.3f, 0, glowTex.Size() * 0.5f, Main.rand.NextFloat(0.96f, 1f), SpriteEffects.None); // random scale to mix up the interference pattern
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
            Color color = Color.Lerp(Color.Cyan, Color.White, Main.rand.NextFloat());
            ParticleManager.AddParticle(new Smoke(
                    Projectile.Center + new Vector2(Main.rand.NextFloat(-25, 25), Main.rand.NextFloat(-25, 25)), Main.rand.NextVector2CircularEdge(2f, 2f) * Main.rand.NextFloat(0.6f, 0.86f), lifetime, color * 0.8f, new Vector2(0.5f) * scaleMulti,
                    Main.rand.Next(15), Main.rand.NextFloat(MathHelper.TwoPi), Main.rand.NextBool() ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0.98f),
                    ParticleManager.ParticleLayer.BehindTiles);
        }
    }
}
