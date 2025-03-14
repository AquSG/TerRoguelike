using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using TerRoguelike.Managers;
using TerRoguelike.Particles;
using TerRoguelike.Projectiles;
using TerRoguelike.Systems;
using static TerRoguelike.Managers.TextureManager;
using static TerRoguelike.Schematics.SchematicManager;
using static TerRoguelike.Systems.MusicSystem;
using static TerRoguelike.Systems.RoomSystem;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using static TerRoguelike.NPCs.Enemy.Boss.CrimsonVessel;
using System.IO;

namespace TerRoguelike.NPCs.Enemy.Boss
{
    public class CrimsonSeer : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<CrimsonSeer>();
        public static readonly SoundStyle SeerSpawn = new SoundStyle("TerRoguelike/Sounds/SeerSpawn");
        public override List<int> associatedFloors => new List<int>() { FloorDict["Crimson"] };
        Texture2D crossSparkTex;
        Texture2D lightningTex;
        List<LightningDraw> lightningPath = new List<LightningDraw>();
        public override int CombatStyle => -1;

        bool ableToHit = true;
        bool canBeHit = true;

        public override void SetStaticDefaults()
        {
            NPCID.Sets.NoMultiplayerSmoothingByType[Type] = true;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 24;
            NPC.height = 24;
            NPC.aiStyle = -1;
            NPC.damage = 35;
            NPC.lifeMax = 600;
            NPC.HitSound = SoundID.NPCHit9;
            NPC.DeathSound = SoundID.NPCDeath11;
            NPC.knockBackResist = 0f;
            modNPC.drawCenter = new Vector2(0, 0);
            modNPC.IgnoreRoomWallCollision = true;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            modNPC.OverrideIgniteVisual = true;
            NPC.hide = true;
            lightningTex = TexDict["Square"];
            crossSparkTex = TexDict["CrossSpark"];
        }
        public override void DrawBehind(int index)
        {
            Main.instance.DrawCacheNPCProjectiles.Add(index);
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.localAI[0] = Main.rand.NextFloat(-1, 1 + float.Epsilon);
            SoundEngine.PlaySound(SeerSpawn with { Volume = 0.25f, MaxInstances = 3 }, NPC.Center);

            NPC.ai[0] = -1;
            NPC.localAI[1] += Main.rand.Next(50);
            if (source is EntitySource_Parent parentSource)
            {
                if (parentSource.Entity is NPC)
                {
                    NPC.ai[0] = parentSource.Entity.whoAmI;
                    NPC npc = Main.npc[(int)NPC.ai[0]];
                    GenerateLightningPath();
                    if (!npc.active)
                    {
                        NPC.StrikeInstantKill();
                        return;
                    }
                        
                }
            }
            if (NPC.ai[0] == -1)
                NPC.StrikeInstantKill();
        }
        public void GenerateLightningPath()
        {
            float randRot = Main.rand.NextFloat(-MathHelper.Pi * 0.3f, MathHelper.Pi * 0.3f + float.Epsilon);
            lightningPath.Add(new LightningDraw(Vector2.Zero, randRot));
            for (int i = 1; i < 300; i++)
            {
                LightningDraw oldLightning = lightningPath[i - 1];
                float rot = oldLightning.rotation;
                if (i % 8 == 0)
                {
                    int sign = -Math.Sign(oldLightning.offset.Y);
                    if (sign == 0)
                        sign = Main.rand.NextBool() ? -1 : 1;
                    rot = sign * Main.rand.NextFloat(MathHelper.Pi * 0.02f, MathHelper.Pi * 0.3f + float.Epsilon);
                }
                Vector2 offset = oldLightning.offset + (rot.ToRotationVector2() * 2);
                lightningPath.Add(new LightningDraw(offset, rot));
            }
        }
        public override void AI()
        {
            if (modNPC.hostileTurnedAlly)
            {
                modNPC.IgnoreRoomWallCollision = false;
                NPC.position += Main.rand.NextVector2CircularEdge(2, 2);
                int attackTelegraph = 45;
                int attackCooldown = 15;
                modNPC.RogueFlyingShooterAI(NPC, 7f, 5.5f, 0.12f, 128f, 320f, attackTelegraph, attackCooldown, ModContent.ProjectileType<BloodClot>(), 8f, Vector2.Zero, NPC.damage, true);
                if (NPC.ai[2] == -attackCooldown)
                {
                    SoundEngine.PlaySound(SoundID.NPCHit13 with { Volume = 0.5f, Pitch = -0.6f }, NPC.Center);
                }
                return;
            }
            NPC.localAI[1] += Main.rand.NextFloat(0.9f, 1f);
            NPC.localAI[2]++;
            if (NPC.localAI[3] == 0)
            {
                for (int i = 0; i < 20; i++)
                {
                    float rot = (float)i / 20 * MathHelper.TwoPi;
                    rot += Main.rand.NextFloat(-0.1f, 0.1f);
                    Vector2 offset = rot.ToRotationVector2() * 6f;
                    ParticleManager.AddParticle(new ThinSpark(NPC.Center + offset, rot.ToRotationVector2() * 1.3f, 20, Color.LimeGreen, new Vector2(0.08f, 0.36f), rot, true));
                }
                for (int i = 0; i < 3; i++)
                {
                    float rot = Main.rand.NextFloat(MathHelper.TwoPi);
                    ParticleManager.AddParticle(new Square(NPC.Center + (rot.ToRotationVector2() * 12), rot.ToRotationVector2() * Main.rand.NextFloat(0.66f, 1f), Main.rand.Next(90, 131), Color.Green, new Vector2(0.5f), 0, 0.96f, 60));
                }
                NPC.localAI[3]++;
            }
            ableToHit = false;
            NPC parent = Main.npc[(int)NPC.ai[0]];
            if (parent.ai[0] == 3 && parent.ai[1] == 130)
                ableToHit = true;

            if (!parent.active)
                NPC.StrikeInstantKill();
        }
        public static void UpdateCrimsonSeer(NPC npc, int seerId, int seerCount, Vector2 orbitCenter)
        {
            NPC parent = Main.npc[(int)npc.ai[0]];

            bool healMode = parent.ai[0] == Heal.Id && parent.ai[1] > teleportTime && parent.ai[1] < Heal.Duration - seerSpawnTime;
            bool ballMode = parent.ai[0] == BouncyBall.Id && parent.ai[1] > teleportTime;
            bool bloodSpreadMode = parent.ai[0] == BloodSpread.Id && parent.ai[1] > teleportTime;

            if (healMode)
            {
                Vector2 direction = (parent.Center - npc.Center).SafeNormalize(Vector2.UnitY) * 1.2f;
                float magnitude = 0.85f + (0.15f * npc.localAI[0]);
                float healCompletion = (parent.ai[1] - teleportTime) / (Heal.Duration - seerSpawnTime - teleportTime);

                npc.Center += direction * magnitude * healCompletion;
            }
            else if (ballMode)
            {
                float interpolant = (parent.ai[1] - (teleportTime + 1)) / (ballLaunchTime - (teleportTime + teleportMoveTimestamp)) * 0.4f;
                float rotationOffset = npc.localAI[2] / 60 * MathHelper.TwoPi;
                
                Vector2 offset = seerId == 0 ? Vector2.Zero : ((((seerId - 1f) / (seerCount - 1)) * MathHelper.TwoPi) + rotationOffset).ToRotationVector2() * 28f;
                if (parent.ai[1] > ballLaunchTime)
                {
                    interpolant = ((parent.ai[1] - (BouncyBall.Duration - ballEndLag)) / (ballEndLag - 20)) * 0.4f;
                    npc.rotation += 0.01f;
                    float magnitude = (136 + (10 * (float)Math.Cos(npc.localAI[1] * 0.05f)));

                    offset = (npc.rotation.ToRotationVector2() * magnitude);
                }
                npc.Center += MathHelper.SmoothStep(0, 1, interpolant) * (orbitCenter + offset - npc.Center);
            }
            else if (bloodSpreadMode)
            {
                float interpolant = parent.ai[1] < (BloodSpread.Duration - bloodSpreadEndLag) ? (parent.ai[1] - teleportTime) / bloodSpreadStartup : 1f - ((parent.ai[1] - (BloodSpread.Duration - bloodSpreadEndLag)) / bloodSpreadEndLag);
                interpolant = MathHelper.Clamp(interpolant, 0, 1f);
                npc.rotation += 0.01f + (0.02f * interpolant);
                float extraMagnitude = 0;
                if (parent.ai[1] > teleportTime + bloodSpreadStartup && parent.ai[1] < BloodSpread.Duration - bloodSpreadEndLag)
                {
                    float start = parent.ai[1] - (teleportTime + bloodSpreadStartup);
                    float radian = start / (2 * bloodSpreadRate) * MathHelper.TwoPi;
                    radian %= MathHelper.TwoPi;
                    float periodCompletion = (int)start % (2 * bloodSpreadRate);
                    if (periodCompletion < 10)
                        radian *= 2;
                    else
                    {
                        radian /= 2f;
                        radian += MathHelper.Pi;
                    }
                        
                    extraMagnitude = ((-(float)Math.Cos(radian) * 0.5f) + 0.5f) * bloodSpreadSeerDistance;
                    if ((int)periodCompletion == 10)
                    {
                        for (int i = -1; i <= 1; i++)
                        {
                            float rot = npc.rotation + (i * MathHelper.PiOver4 * 0.33f);
                            Vector2 offset = rot.ToRotationVector2() * (npc.width * 0.5f);
                            Vector2 velocity = rot.ToRotationVector2() * 8f;
                            if (!TerRoguelike.mpClient)
                                Projectile.NewProjectile(parent.GetSource_FromThis(), npc.Center + offset, velocity, ModContent.ProjectileType<BloodClot>(), npc.damage, 0);
                        }
                    }
                }
                float magnitude = (136 + extraMagnitude + (10 * (float)Math.Cos(npc.localAI[1] * 0.05f)));
                npc.Center = orbitCenter + (npc.rotation.ToRotationVector2() * magnitude);
            }
            else
            {
                float interpolant = MathHelper.Clamp((npc.localAI[2]) / 60, 0, 1f);
                npc.rotation += 0.01f * interpolant;
                float magnitude = (136 + (10 * (float)Math.Cos(npc.localAI[1] * 0.05f)));

                npc.Center = orbitCenter + (npc.rotation.ToRotationVector2() * magnitude);
            }
        }
        public override bool CanHitNPC(NPC target)
        {
            return ableToHit;
        }
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            return ableToHit;
        }
        public override bool? CanBeHitByProjectile(Projectile projectile) => canBeHit ? null : false;
        public override bool? CanBeHitByItem(Player player, Item item) => canBeHit ? null : false;

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)NPC.lifeMax * 20.0; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 5, hit.HitDirection, -1f);
                }
            }
            else
            {
                for (int i = 0; i < 20; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 5, 2 * hit.HitDirection, -2f);
                }
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 402);

                for (int i = 0; i < 5; i++)
                {
                    Vector2 pos = NPC.Center + new Vector2(0, 16);
                    Vector2 velocity = new Vector2(0, -3.3f).RotatedBy(Main.rand.NextFloat(-MathHelper.PiOver4 * 0.7f, MathHelper.PiOver4 * 0.7f));
                    velocity *= Main.rand.NextFloat(0.5f, 1f);
                    velocity.X += hit.HitDirection;
                    if (Main.rand.NextBool(5))
                        velocity *= 1.5f;
                    Vector2 scale = new Vector2(0.25f, 0.4f) * 0.75f;
                    int time = 110 + Main.rand.Next(70);
                    ParticleManager.AddParticle(new Blood(pos, velocity, time, Color.Black * 0.65f, scale, velocity.ToRotation(), false));
                    ParticleManager.AddParticle(new Blood(pos, velocity, time, Color.Red * 0.65f, scale, velocity.ToRotation(), true));
                }
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;
            Vector2 drawPos = NPC.Center + modNPC.drawCenter;
            Color color = modNPC.ignitedStacks.Count > 0 ? Color.Lerp(Color.White, Color.OrangeRed, 0.4f) : Color.Lerp(Color.White, Lighting.GetColor(drawPos.ToTileCoordinates()), 0.6f);
            float rotation = 0;
            Vector2 scale = new Vector2(NPC.scale);
            if (NPC.ai[3] > 0 && !modNPC.hostileTurnedAlly)
            {
                float interpolant = NPC.ai[3] < teleportMoveTimestamp ? NPC.ai[3] / (teleportMoveTimestamp) : 1f - ((NPC.ai[3] - teleportMoveTimestamp) / (teleportTime - teleportMoveTimestamp));
                float verticInterpolant = MathHelper.Lerp(1f, 2f, 0.5f + (0.5f * -(float)Math.Cos(interpolant * MathHelper.TwoPi)));
                float horizInterpolant = MathHelper.Lerp(0.5f + (0.5f * (float)Math.Cos(interpolant * MathHelper.TwoPi)), 8f, interpolant * interpolant);
                scale.X *= horizInterpolant;
                scale.Y *= verticInterpolant;

                scale *= 1f - interpolant;
            }

            if (modNPC.ignitedStacks.Count > 0)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                Color fireColor = Color.Lerp(Color.Yellow, Color.OrangeRed, Main.rand.NextFloat(0.4f, 0.6f + float.Epsilon) + 0.2f + (0.2f * (float)Math.Cos((Main.GlobalTimeWrappedHourly * 20f)))) * 0.8f;
                Vector3 colorHSL = Main.rgbToHsl(fireColor);
                float outlineThickness = 1f;

                GameShaders.Misc["TerRoguelike:BasicTint"].UseOpacity(1f);
                GameShaders.Misc["TerRoguelike:BasicTint"].UseColor(Main.hslToRgb(1 - colorHSL.X, colorHSL.Y, colorHSL.Z));
                GameShaders.Misc["TerRoguelike:BasicTint"].Apply();

                for (float j = 0; j < 1; j += 0.125f)
                {
                    spriteBatch.Draw(tex, drawPos - Main.screenPosition + ((j * MathHelper.TwoPi + NPC.rotation).ToRotationVector2() * outlineThickness), NPC.frame, color, rotation, NPC.frame.Size() * 0.5f, scale, NPC.spriteDirection > 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
                }
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            
            Main.EntitySpriteDraw(tex, drawPos - Main.screenPosition, NPC.frame, color, rotation, NPC.frame.Size() * 0.5f, scale, NPC.spriteDirection > 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);

            if (modNPC.hostileTurnedAlly)
                return false;

            NPC parent = Main.npc[(int)NPC.ai[0]];
            if (NPC.localAI[2] < 60 || (parent.ai[0] == 2 && parent.ai[1] > teleportTime))
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                float interpolant = 1f - (NPC.localAI[2] / 60);
                if (NPC.localAI[2] >= 60)
                    interpolant = 1f;

                Vector3 colorHSL = Main.rgbToHsl(Color.Lerp(Color.Green, Color.Black, 1f - interpolant));


                GameShaders.Misc["TerRoguelike:BasicTint"].UseOpacity(1f);
                GameShaders.Misc["TerRoguelike:BasicTint"].UseColor(Main.hslToRgb(1 - colorHSL.X, colorHSL.Y, colorHSL.Z));
                GameShaders.Misc["TerRoguelike:BasicTint"].Apply();

                spriteBatch.Draw(tex, drawPos - Main.screenPosition, NPC.frame, color, rotation, NPC.frame.Size() * 0.5f, scale, NPC.spriteDirection > 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            if (parent.ai[0] == BloodSpread.Id && parent.ai[1] > teleportTime)
            {
                int time = (int)parent.ai[1] - teleportTime;
                float interpolant = time <= bloodSpreadStartup ? (float)time / bloodSpreadStartup : 1f - ((parent.ai[1] - (BloodSpread.Duration - bloodSpreadEndLag)) / bloodSpreadEndLag);
                interpolant = MathHelper.Clamp(interpolant, 0, 1);
                float rot = NPC.rotation - 0.6f;
                Vector2 pos = NPC.Center + (rot.ToRotationVector2() * NPC.width * 0.6f);
                StartAdditiveSpritebatch();

                Main.EntitySpriteDraw(crossSparkTex, pos - Main.screenPosition, null, Color.Red * 0.7f, rot, new Vector2(crossSparkTex.Width * 0.1f, crossSparkTex.Height * 0.5f), new Vector2(0.2f * interpolant, 0.2f + (0.1f * interpolant)), SpriteEffects.FlipHorizontally);

                StartVanillaSpritebatch();
            }

            if (parent.ai[0] == Heal.Id && (int)(Main.GlobalTimeWrappedHourly * 60) % 3 == 0)
            {
                lightningPath.Clear();
                GenerateLightningPath();
            }

            if (parent.ai[0] == Heal.Id && parent.ai[1] > teleportTime && parent.ai[1] < Heal.Duration - seerSpawnTime)
            {
                Vector2 parentVect = parent.Center - NPC.Center;
                float baseRot = parentVect.ToRotation();
                float length = (parentVect).Length();
                if (length > lightningPath.Count)
                    length = lightningPath.Count;

                int easing = 24;
                for (int i = 0; i < length; i++)
                {
                    LightningDraw lightning = lightningPath[i];
                    if (lightning.offset.X > length)
                        break;

                    Vector2 realOffset = lightning.offset;
                    if (i < easing)
                        realOffset.Y *= (float)i / easing;
                    else if (i >= length - easing)
                        realOffset.Y *= (i - (length - (easing - 1))) / easing;

                    Vector2 lightningPos = NPC.Center + realOffset.RotatedBy(baseRot);
                    Color lightningColor = Color.Lerp(Color.LimeGreen, Color.LightGreen, i / length);
                    Main.EntitySpriteDraw(lightningTex, lightningPos - Main.screenPosition, null, lightningColor * 0.5f, lightning.rotation + baseRot, lightningTex.Size() * 0.5f, 1f, SpriteEffects.None);

                }
            }

            return false;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(NPC.localAI[0]);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            NPC.localAI[0] = reader.ReadSingle();
        }
    }
    public class LightningDraw
    {
        public Vector2 offset;
        public float rotation;

        public LightningDraw(Vector2 Offset, float Rotation)
        {
            offset = Offset;
            rotation = Rotation;
        }
    }
}
