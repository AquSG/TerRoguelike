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
using static TerRoguelike.Managers.TextureManager;
using Microsoft.Build.ObjectModelRemoting;

namespace TerRoguelike.Projectiles
{
    public class BloodOrb : ModProjectile, ILocalizedModType
    {
        public Texture2D glowTex;
        public override string Texture => "TerRoguelike/Projectiles/InvisibleProj";
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 60;
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
            Projectile.ai[2] = Projectile.velocity.Length();
            if (Projectile.ai[0] != 1)
                Projectile.velocity *= 0;
            else
                Projectile.velocity *= 0.25f;
                

            Projectile.ai[0] = -1;
        }

        public override void AI()
        {
            if (Projectile.timeLeft == 60)
                GetTarget();

            Projectile.velocity *= 0.965f;
            for (int j = 0; j < 3; j++)
            {
                for (int i = 0; i < Main.rand.Next(1, 3); i++)
                {
                    Vector2 position = Projectile.Center + (Vector2.UnitX * 8).RotatedBy(Main.rand.NextFloat(MathHelper.TwoPi));
                    Dust dust = Dust.NewDustPerfect(position, Projectile.ModProj().hostileTurnedAlly ? DustID.Clentaminator_Cyan : DustID.Crimson, (Projectile.Center - position) * 0.25f, 0, default, 1f);

                    dust.noGravity = true;
                    dust.noLightEmittence = true;
                    dust.noLight = true;
                }
                if (Main.rand.NextBool())
                {
                    Vector2 position = Projectile.Center + (Vector2.UnitX * 8).RotatedBy(Main.rand.NextFloat(MathHelper.TwoPi));
                    Dust dust = Dust.NewDustPerfect(position, Projectile.ModProj().hostileTurnedAlly ? DustID.CoralTorch : DustID.CrimsonTorch, (Projectile.Center - position) * 0.25f, 0, default, 1.5f);

                    dust.noGravity = true;
                    dust.noLightEmittence = true;
                    dust.noLight = true;
                }
            }
        }
        public override bool? CanDamage() => false;
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.localAI[0] = 1;
            return true;
        }
        public override void OnKill(int timeLeft)
        {
            if (Projectile.localAI[0] == 1)
                return;

            float direction = MathHelper.PiOver2;
            if (Projectile.ai[0] != -1)
            {
                if (Projectile.ai[1] == 0)
                {
                    direction = (Main.player[(int)Projectile.ai[0]].Center - Projectile.Center).ToRotation();
                }
                else if (Projectile.ai[1] == 1)
                {
                    direction = (Main.npc[(int)Projectile.ai[0]].Center - Projectile.Center).ToRotation();
                }
            }
            Vector2 velocity = (Vector2.UnitX * Projectile.ai[2]).RotatedBy(direction);

            if (!TerRoguelike.mpClient)
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<BloodClot>(), Projectile.damage, 0);

            for (int i = 0; i < 9; i++)
            {
                int d2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, Projectile.ModProj().hostileTurnedAlly ? DustID.Clentaminator_Cyan : DustID.Crimson, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), Projectile.alpha, Color.LimeGreen, 0.75f);
                Dust dust2 = Main.dust[d2];
                dust2.velocity *= 0.5f;
                dust2.noLightEmittence = true;
                dust2.noLight = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.ModProj().hostileTurnedAlly)
                return false;
            TerRoguelikeUtils.StartNonPremultipliedSpritebatch();
            Main.EntitySpriteDraw(glowTex, Projectile.Center - Main.screenPosition, null, Color.Black * 0.2f, Projectile.velocity.ToRotation(), glowTex.Size() * 0.5f, new Vector2(0.1f), SpriteEffects.None);
            TerRoguelikeUtils.StartVanillaSpritebatch();
            return false;
        }
        public void GetTarget()
        {
            var modProj = Projectile.ModProj();
            if (modProj != null && modProj.npcOwner >= 0)
            {
                NPC owner = Main.npc[modProj.npcOwner];
                if (owner.active)
                {
                    var modOwner = owner.ModNPC();
                    if (modOwner != null)
                    {
                        if (modOwner.targetNPC >= 0 && Main.npc[modOwner.targetNPC].active)
                        {
                            Projectile.ai[1] = 1;
                            Projectile.ai[0] = modOwner.targetNPC;
                            return;
                        }
                        if (modOwner.targetPlayer >= 0 && Main.player[modOwner.targetPlayer].active && !Main.player[modOwner.targetPlayer].dead)
                        {
                            Projectile.ai[1] = 0;
                            Projectile.ai[0] = modOwner.targetPlayer;
                            return;
                        }
                    }
                }
            }

            float closestTarget = 3200f;
            if (Projectile.hostile)
            {
                Projectile.ai[1] = 0;
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player player = Main.player[i];
                    if (player == null)
                        continue;
                    if (!player.active)
                        continue;
                    if (player.dead)
                        continue;

                    float distance = (Projectile.Center - player.Center).Length();
                    if (distance <= closestTarget)
                    {
                        closestTarget = distance;
                        Projectile.ai[0] = i;
                    }
                }
            }
            else
            {
                Projectile.ai[1] = 1;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc == null)
                        continue;
                    if (!npc.active)
                        continue;
                    if (npc.life <= 0)
                        continue;
                    if (npc.immortal)
                        continue;

                    float distance = (Projectile.Center - npc.Center).Length();
                    if (distance <= closestTarget)
                    {
                        closestTarget = distance;
                        Projectile.ai[0] = i;
                    }
                }
            }
        }
    }
}
