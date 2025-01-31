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
using Terraria.Graphics.Renderers;
using Terraria.DataStructures;
using static TerRoguelike.Utilities.TerRoguelikeUtils;

namespace TerRoguelike.Projectiles
{
    public class NebulaLaser : ModProjectile, ILocalizedModType
    {
        public TerRoguelikeGlobalProjectile modProj;
        public override string Texture => "TerRoguelike/Projectiles/InvisibleProj";
        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.MaxUpdates = 30;
            Projectile.timeLeft = 5400;
            Projectile.penetrate = 1;
            modProj = Projectile.ModProj();
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.velocity /= Projectile.MaxUpdates;
            for (int i = 0; i < 6; i++)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, Projectile.ModProj().hostileTurnedAlly ? DustID.DungeonSpirit : DustID.ShadowbeamStaff, 0f, 0f, 100, Color.Purple, 1.2f);
                Dust dust = Main.dust[d];
                dust.velocity *= 2f;
                dust.noLight = true;
                dust.noLightEmittence = true;
            }
        }
        public override void AI()
        {  
            if (Main.rand.NextBool(3))
                return;

            int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, Projectile.ModProj().hostileTurnedAlly ? DustID.DungeonSpirit : DustID.ShadowbeamStaff, 0f, 0f, 100, Color.Purple, 1.2f);
            Dust dust = Main.dust[d];
            dust.velocity *= 0f;
            dust.noGravity = true;
            dust.noLight = true;
            dust.noLightEmittence = true;
        }
        
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            for (int i = 0; i < 12; i++)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, Projectile.ModProj().hostileTurnedAlly ? DustID.DungeonSpirit : DustID.ShadowbeamStaff, 0f, 0f, 100, Color.Purple, 1.2f);
                Dust dust = Main.dust[d];
                dust.velocity *= 3f;
                dust.noLight = true;
                dust.noLightEmittence = true;
            }
            SoundEngine.PlaySound(SoundID.NPCHit3 with { Volume = 0.7f }, Projectile.Center);
            return true;
        }
    }
}
