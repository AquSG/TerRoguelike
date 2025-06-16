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
using Terraria.GameContent;
using static TerRoguelike.Managers.TextureManager;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using Terraria.DataStructures;
using TerRoguelike.Particles;
using Terraria.Graphics.Shaders;
using System.IO;
using static TerRoguelike.Projectiles.AdaptiveSaberHoldout;
using ReLogic.Utilities;

namespace TerRoguelike.Projectiles
{
    public class AdaptiveSaberThrow : ModProjectile, ILocalizedModType
    {
        public override string Texture => "TerRoguelike/Projectiles/InvisibleProj";
        public bool ableToHit = true;
        public TerRoguelikeGlobalProjectile modProj;
        TerRoguelikePlayer modPlayer;
        public int setTimeLeft = 600;
        Texture2D squareTex;
        public float oldRot = 0;
        public SlotId windslot;
        public SwordColor swordLevel
        {
            get { return (SwordColor)Projectile.ai[1]; }
            set { Projectile.ai[1] = (int)value; }
        }
        public float rainbowProg = 0;

        public override void SetStaticDefaults()
        {

        }
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.timeLeft = setTimeLeft;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            modProj = Projectile.ModProj();
            squareTex = TexDict["Square"];
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation += Main.rand.NextFloat(MathHelper.TwoPi);

            Projectile.scale = Projectile.ai[0];

            modPlayer = Main.player[Projectile.owner].ModPlayer();
            rainbowProg = Projectile.ai[2];
        }
        public override void AI()
        {
            if (swordLevel == SwordColor.Rainbow)
                rainbowProg += 0.0154936875f;

            Projectile.scale = Projectile.ai[0];
            modPlayer = Main.player[Projectile.owner].ModPlayer();

            if (Projectile.localAI[1] == 0 && modPlayer.heatSeekingChip > 0)
                modProj.HomingAI(Projectile, (float)Math.Log(modPlayer.heatSeekingChip + 1, 1.2d) / (2600f));

            if (modPlayer.bouncyBall > 0 || modPlayer.trash > 0)
                modProj.extraBounces += modPlayer.bouncyBall + modPlayer.trash;

            oldRot = Projectile.rotation;
            Projectile.rotation += 0.5f * Projectile.direction;

            int particleCount = (int)(Projectile.position.Distance(Projectile.oldPosition) * 1f);
            if (Projectile.localAI[0] == 0)
            {
                particleCount = 1;
                Projectile.localAI[0] = 1;
                oldRot = Projectile.rotation;
            }
            if (Projectile.timeLeft % 10 == 0)
            {
                windslot = SoundEngine.PlaySound(SoundID.Item7 with { Volume = 1f, MaxInstances = 10 });
            }
            if (SoundEngine.TryGetActiveSound(windslot, out var sound))
            {
                sound.Position = Projectile.Center;
            }
            Vector2 newPos = Projectile.Center + Projectile.velocity;
            for (int i = 0; i < particleCount; i++)
            {
                float completion = i / (float)particleCount;
                Vector2 thisPos = Vector2.Lerp(Projectile.Center, newPos, completion);
                float thisRot = (oldRot - MathHelper.PiOver4 + MathHelper.PiOver4 * 2 * Projectile.direction).AngleLerp(Projectile.rotation - MathHelper.PiOver4 + MathHelper.PiOver4 * 2 * Projectile.direction, completion);
                ParticleManager.AddParticle(new Beam(thisPos + thisRot.ToRotationVector2() * 5 * Projectile.scale, Vector2.Zero, 5, GetSwordColor(swordLevel, rainbowProg), new Vector2(0.1f * Projectile.scale), thisRot, 0, 5, true));
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.localAI[1] == 0)
            {
                Color swordColor = GetSwordColor(swordLevel, rainbowProg);
                StartAdditiveSpritebatch();
                var beam = TexDict["Beam"];
                float beamRot = Projectile.rotation - MathHelper.PiOver4 + MathHelper.PiOver4 * 2 * Projectile.direction;
                Main.EntitySpriteDraw(beam, Projectile.Center + beamRot.ToRotationVector2() - Main.screenPosition, null, swordColor, beamRot, beam.Size() * 0.5f, 0.1f * Projectile.scale, SpriteEffects.None);
                StartVanillaSpritebatch();

                
                Texture2D tex = TextureAssets.Projectile[ModContent.ProjectileType<AdaptiveSaberHoldout>()].Value;
                var frame = tex.Frame(1, 2, 0, 0, 0, -2);
                Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, frame, Lighting.GetColor(Projectile.Center.ToTileCoordinates()), Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, Projectile.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.FlipVertically);
                frame = tex.Frame(1, 2, 0, 1, 0, -2);
                Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, frame, swordColor, Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, Projectile.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.FlipVertically);
            }

            if (false)
            {
                float radius = 24;
                radius *= Projectile.scale;
                Vector2 hitboxPos = Projectile.Center;

                for (int j = 0; j < 120; j++)
                {
                    Main.EntitySpriteDraw(squareTex, hitboxPos + ((j / 120f * MathHelper.TwoPi).ToRotationVector2() * radius) - Main.screenPosition, null, Color.Red, 0, squareTex.Size() * 0.5f, 1f, SpriteEffects.None);
                }
            }
            return false;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Projectile.owner < 0)
                return null;

            float radius = 24;
            radius *= Projectile.scale;
            Vector2 hitboxPos = Projectile.Center;

            if (targetHitbox.ClosestPointInRect(hitboxPos).Distance(hitboxPos) <= radius)
                return true;

            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.localAI[1] == 0)
            {
                Projectile.localAI[1] = 1;
                Projectile.velocity = Vector2.Zero;
                Projectile.tileCollide = false;
                Projectile.netUpdate = true;
                Projectile.timeLeft = 2;
                SoundEngine.PlaySound(SoundID.Item52 with { Volume = 0.13f, Pitch = -0.5f }, Projectile.Center);
                SoundEngine.PlaySound(SoundID.Dig with { Volume = 0.6f, Pitch = 0f }, Projectile.Center);
                for (int i = 0; i < 10; i++)
                {
                    ParticleManager.AddParticle(new Square(
                        Projectile.Center,
                        Main.rand.NextVector2CircularEdge(3, 3) * Main.rand.NextFloat(0.3f, 1f) * Projectile.scale,
                        20, GetSwordColor(swordLevel, rainbowProg + i * 0.1f), new Vector2(Main.rand.NextBool() ? 2 : 5) * Projectile.scale, Main.rand.NextFloat(MathHelper.Pi), 0.96f));
                }
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            modProj.bounceCount++;
            if (Projectile.localAI[1] != 0)
                return false;

            if (modProj.bounceCount >= 1 + modProj.extraBounces)
            {
                modProj.bounceCount--;
                Projectile.velocity = Vector2.Zero;
                Projectile.localAI[1] = 1;
                Projectile.tileCollide = false;
                Projectile.timeLeft = 2;
                SoundEngine.PlaySound(SoundID.Item178 with { Volume = 0.2f, Pitch = -0.5f }, Projectile.Center);
                SoundEngine.PlaySound(SoundID.Dig with { Volume = 0.6f, Pitch = 0f }, Projectile.Center);
                for (int i = 0; i < 20; i++)
                {
                    ParticleManager.AddParticle(new Square(
                        Projectile.Center, 
                        Main.rand.NextVector2CircularEdge(3, 3) * Main.rand.NextFloat(0.3f, 1f) * Projectile.scale, 
                        20, GetSwordColor(swordLevel, rainbowProg + i * 0.1f), new Vector2(Main.rand.NextBool() ? 2 : 5) * Projectile.scale, Main.rand.NextFloat(MathHelper.Pi), 0.96f));
                }
            }
            else
            {
                // If the projectile hits the left or right side of the tile, reverse the X velocity
                if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
                {
                    Projectile.velocity.X = -oldVelocity.X;
                    Projectile.timeLeft = setTimeLeft;
                }
                // If the projectile hits the top or bottom side of the tile, reverse the Y velocity
                if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
                {
                    Projectile.velocity.Y = -oldVelocity.Y;
                    Projectile.timeLeft = setTimeLeft;
                }
            }
            return false;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Projectile.penetrate);
            writer.Write(Projectile.tileCollide);
            writer.Write(Projectile.timeLeft);
            writer.Write(Projectile.localAI[1]);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.penetrate = reader.ReadInt32();
            Projectile.tileCollide = reader.ReadBoolean();
            Projectile.timeLeft = reader.ReadInt32();
            Projectile.localAI[1] = reader.ReadSingle();
        }
    }
}
