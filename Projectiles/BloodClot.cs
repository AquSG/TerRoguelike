﻿using System;
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
using static TerRoguelike.Managers.TextureManager;

namespace TerRoguelike.Projectiles
{
    public class BloodClot : ModProjectile, ILocalizedModType
    {
        public Texture2D glowTex;
        public override string Texture => "TerRoguelike/Projectiles/InvisibleProj";
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 300;
            Projectile.penetrate = 1;
            glowTex = TexDict["CircularGlow"];
            Projectile.hide = true;
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCs.Add(index);
        }
        public override void OnSpawn(IEntitySource source)
        {
            Point spawnTile = Projectile.Center.ToTileCoordinates();
            if (TerRoguelikeUtils.ParanoidTileRetrieval(spawnTile.X, spawnTile.Y).IsTileSolidGround(true))
            {
                Projectile.active = false;
                return;
            }
            SoundEngine.PlaySound(SoundID.NPCDeath9 with { Volume = 1f }, Projectile.Center);
        }
        public override void AI()
        {
            if (Main.rand.NextBool())
            {
                int d = Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, Projectile.ModProj().hostileTurnedAlly ? DustID.Clentaminator_Cyan : DustID.Crimson, 0, 0, Projectile.alpha, default(Color), 1.6f);
                Dust dust = Main.dust[d];
                dust.velocity *= 0.5f;
                dust.noGravity = true;
                dust.noLightEmittence = true;
                dust.noLight = true;

            }
            if (Main.rand.NextBool(5))
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, Projectile.ModProj().hostileTurnedAlly ? DustID.CoralTorch : DustID.CrimsonTorch, 0, 0, Projectile.alpha, Color.LimeGreen, 1.5f);
                Dust dust = Main.dust[d];
                dust.noGravity = true;
                dust.noLightEmittence = true;
                dust.noLight = true;
            }

            Vector2 velocity = -Projectile.velocity.SafeNormalize(Vector2.UnitY) * 1;
            velocity *= Main.rand.NextFloat(0.5f, 1f);
            velocity.Y -= 0.8f;
            if (Main.rand.NextBool(3))
                velocity *= 1.5f;
            Vector2 scale = new Vector2(0.25f, 0.4f) * 0.75f;
            int time = 30 + Main.rand.Next(20);
            ParticleManager.AddParticle(new Blood(Projectile.Center + Projectile.velocity, velocity, time, Color.Black * 0.65f, scale, velocity.ToRotation(), false));
            ParticleManager.AddParticle(new Blood(Projectile.Center + Projectile.velocity, velocity, time, Projectile.ModProj().hostileTurnedAlly ? Color.Cyan : Color.Red * 0.65f, scale, velocity.ToRotation(), true));
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, Projectile.ModProj().hostileTurnedAlly ? DustID.DungeonSpirit : DustID.Crimson, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), Projectile.alpha, default(Color), 1f);
                Dust dust = Main.dust[d];
                dust.noLightEmittence = true;
                dust.noLight = true;
                if (Main.rand.NextBool())
                {
                    d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, Projectile.ModProj().hostileTurnedAlly ? DustID.CoralTorch : DustID.CrimsonTorch, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), Projectile.alpha, Color.LimeGreen, 1.5f);
                    dust = Main.dust[d];
                    dust.noLightEmittence = true;
                    dust.noLight = true;
                }
            }
        }
        public override bool? CanDamage()
        {
            var modProj = Projectile.ModProj();
            if (modProj.npcOwner >= 0)
            {
                NPC npc = Main.npc[modProj.npcOwner];
                var modNPC = npc.ModNPC();
                if (modNPC.isRoomNPC)
                {
                    if (RoomSystem.RoomList[modNPC.sourceRoomListID].bossDead)
                        return false;
                }
            }

            return null;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.ModProj().hostileTurnedAlly)
                return false;
            TerRoguelikeUtils.StartNonPremultipliedSpritebatch();
            Main.EntitySpriteDraw(glowTex, Projectile.Center - Main.screenPosition, null, Color.Black * 0.2f, Projectile.velocity.ToRotation(), glowTex.Size() * 0.5f, new Vector2(0.1f, 0.05f), SpriteEffects.None);
            TerRoguelikeUtils.StartVanillaSpritebatch();
            return false;
        }
    }
}
