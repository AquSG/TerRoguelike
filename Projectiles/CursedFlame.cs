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
using Terraria.DataStructures;
using TerRoguelike.Particles;
using TerRoguelike.NPCs.Enemy.Boss;

namespace TerRoguelike.Projectiles
{
    public class CursedFlame : ModProjectile, ILocalizedModType
    {
        public override string Texture => "TerRoguelike/Projectiles/InvisibleProj";
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 600;
            Projectile.penetrate = 1;
        }
        public override void OnSpawn(IEntitySource source)
        {
            SoundStyle? fireSound = null;
            if (source is EntitySource_Parent parentSource && parentSource.Entity is NPC)
            {
                NPC npc = Main.npc[parentSource.Entity.whoAmI];
                if (npc.type == ModContent.NPCType<CorruptionParasite>())
                {
                    fireSound = SoundID.Item20 with { Volume = 0.25f, MaxInstances = 8 };
                }
            }
            if (fireSound == null)
                fireSound = SoundID.Item20 with { Volume = 1f };

            SoundEngine.PlaySound(fireSound, Projectile.Center);
            for (int i = 0; i < 4; i++)
            {
                float randRot = Main.rand.NextFloat(-0.2f, 0.2f);
                if (randRot > 0)
                    randRot += 0.2f;
                else
                    randRot -= 0.2f;

                ParticleManager.AddParticle(new Square(Projectile.Center, Projectile.velocity.RotatedBy(randRot) * 0.5f, 20, Color.Lerp(Color.LimeGreen, Color.Yellow, Main.rand.NextFloat(0.75f)), new Vector2(Main.rand.NextFloat(1f, 1.15f)), Main.rand.NextFloat(MathHelper.TwoPi), 0.97f, 20, false));
            }
        }
        public override void AI()
        {
            Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(Projectile.width * 0.55f, Projectile.height * 0.55f);
            Vector2 vel = Main.rand.NextVector2Circular(0.5f, 0.5f);
            ParticleManager.AddParticle(new Square(pos, vel, 20, Color.Lerp(Color.LimeGreen, Color.Yellow, Main.rand.NextFloat(0.75f)), new Vector2(Main.rand.NextFloat(0.9f, 1f)), vel.ToRotation(), 0.96f, 10, false));
            if (Main.rand.NextBool())
                ParticleManager.AddParticle(new Square(Projectile.Center + Main.rand.NextVector2Circular(5, 5), Projectile.velocity * 0.5f, 8, Color.LimeGreen, new Vector2(0.75f), Projectile.velocity.ToRotation(), 0.96f, 4, false));
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                Vector2 pos = Main.rand.NextVector2FromRectangle(Projectile.getRect());
                Vector2 vel = Main.rand.NextVector2CircularEdge(1.5f, 1.5f);
                vel *= Main.rand.NextFloat(0.9f, 1f);
                vel.Y += 1.3f;
                ParticleManager.AddParticle(new Square(pos, vel, 40, Color.Lerp(Color.LimeGreen, Color.Yellow, Main.rand.NextFloat(0.75f)), new Vector2(Main.rand.NextFloat(0.5f, 0.7f)), vel.ToRotation(), 0.96f, 20, false));
            }
        }
    }
}
