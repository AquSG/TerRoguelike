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
using static TerRoguelike.Managers.TextureManager;
using Steamworks;
using ReLogic.Utilities;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using Terraria.GameContent.UI.States;
using Terraria.ModLoader.IO;
using System.Diagnostics;
using System.Timers;

namespace TerRoguelike.NPCs.Enemy.Boss.Mallet.MalletProjectiles
{
    public class Junk : ModProjectile, ILocalizedModType
    {
        public int maxTimeLeft;
        public static readonly int junkTypeCount = 14;
        public static int junkSoundCooldown = 0;
        public enum JunkType
        {
            Ball,
            Can,
            Glass,
            Globe,
            Mallet,
            Mug,
            Nail,
            Plank,
            Radio,
            Saw,
            Screw,
            Screwdriver,
            Shoe,
            Thingy
        }
        public override string Texture => "TerRoguelike/Projectiles/InvisibleProj";
        public static Texture2D[] junkTex = new Texture2D[junkTypeCount];
        public JunkType myJunkType;
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = maxTimeLeft = 600;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.manualDirectionChange = true;
            Projectile.netImportant = true; //makes projectile not able to be replaced if at the projectile cap 
        }
        public override void OnSpawn(IEntitySource source)
        {
            myJunkType = (JunkType)Main.rand.Next(junkTypeCount);
            Projectile.direction = Main.rand.NextBool() ? -1 : 1;
            if (Projectile.ai[0] == 0)
                Projectile.ai[0] = 0.2f;
            Projectile.ai[2] = Main.rand.NextBool() ? -2 : 2;
            Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }
        public override void AI()
        {
            int time = maxTimeLeft - Projectile.timeLeft;

            if (Projectile.tileCollide && Projectile.timeLeft <= 120)
            {
                if (myJunkType == JunkType.Ball)
                    Projectile.timeLeft = 90;
                Projectile.tileCollide = false;
                Projectile.netUpdate = true;
            }
            if (Projectile.timeLeft < 90)
            {
                Projectile.Opacity = Projectile.timeLeft / 90f;
            }
            else if (time < 30)
                Projectile.Opacity = time / 30f;
            else
                Projectile.Opacity = 1;

            float maxSpeed = 32;
            if (Projectile.velocity.Y < maxSpeed)
                Projectile.velocity.Y += Projectile.ai[0];
            else if (Projectile.velocity.Y > maxSpeed)
                Projectile.velocity.Y = maxSpeed;

            if (Projectile.ai[1] == 1)
            {
                if (myJunkType == JunkType.Ball)
                {
                    Projectile.rotation += 0.03f * Projectile.velocity.X;
                }
                else
                    Projectile.rotation += 0.02f * Projectile.ai[2];
            }
            else
            {
                Projectile.rotation += (0.01f + Projectile.ai[0] * 0.02f) * Math.Sign(Projectile.ai[2]);
            }

        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.velocity.Y = -oldVelocity.Y * 0.4f;
            if (Projectile.ai[1] == 1 && myJunkType == JunkType.Ball)
            {
                if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
                {
                    Projectile.tileCollide = false;
                    Projectile.velocity = oldVelocity;
                    Projectile.timeLeft = 90;
                }
            }
            if (Projectile.ai[1] == 0 && Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
            {
                Projectile.netUpdate = true;
                Projectile.ai[0] = 0.3f;
                Projectile.ai[1] = 1;
                Projectile.ai[2] = (Main.rand.NextBool() ? -1 : 1) * Main.rand.NextFloat(0.3f, 1f);
                if (myJunkType == JunkType.Ball)
                {
                    if (oldVelocity.Y < 16)
                        oldVelocity.Y = 16;
                    Projectile.velocity.X = oldVelocity.Y * 0.2f;
                    if (Main.rand.NextBool())
                        Projectile.velocity.X *= -1;
                    Projectile.timeLeft = 300;

                    int checkDir = Math.Sign(Projectile.velocity.X);
                    Vector2 start = Projectile.Bottom - Vector2.UnitY;
                    start.X += Projectile.width * 0.5f * checkDir;
                    for (int i = 0; i < 10; i++)
                    {
                        Vector2 checkPos = start + Vector2.UnitX * i * checkDir * 16;
                        var tile = ParanoidTileRetrieval(checkPos.ToTileCoordinates());
                        if (tile == null)
                            break;
                        if (tile.IsTileSolidGround(true))
                        {
                            Projectile.velocity.X *= -1;
                            break;
                        }
                    }
                }
                else
                {
                    Projectile.tileCollide = false;
                    Projectile.timeLeft = 120;
                    Projectile.velocity.X = Projectile.ai[2];
                }

                var variant = myJunkType switch
                {
                    JunkType.Ball or JunkType.Can or JunkType.Screwdriver or JunkType.Shoe => 1,
                    JunkType.Mallet or JunkType.Plank or JunkType.Radio or JunkType.Saw => 3,
                    _ => 2,
                };

                if (junkSoundCooldown == 0)
                {
                    SoundEngine.PlaySound(Mallet.TrashHit with { Volume = 0.16f, Variants = [variant], MaxInstances = 100 }, Vector2.Lerp(Projectile.Center, Main.LocalPlayer.Center, 0.5f));
                    junkSoundCooldown = 3;
                }
            }
                
            return false;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (projHitbox.Center().DistanceSQ(targetHitbox.Center()) > 40000)
                return false;

            var hitboxes = GetHitboxes();

            foreach(var hitbox in hitboxes)
            {
                if (targetHitbox.RotatedRectanglesIntersect(0, targetHitbox.Center(), hitbox.rect, hitbox.rot, hitbox.origin))
                    return true;
            }

            return false;
        }
        public List<RotatableRectangle> GetHitboxes()
        {
            switch (myJunkType)
            {
                default:
                case JunkType.Ball:
                    return [
                        new(Projectile.Center, new Vector2(20, 40), Projectile.rotation, Projectile.Center),
                        new(Projectile.Center, new Vector2(40, 20), Projectile.rotation, Projectile.Center),
                        new(Projectile.Center, new Vector2(32, 32), Projectile.rotation, Projectile.Center),
                        ];
                case JunkType.Can:
                    return [
                        new(Projectile.Center, new Vector2(30, 34), Projectile.rotation, Projectile.Center),
                        ];
                case JunkType.Glass:
                    return [
                        new(Projectile.Center + new Vector2(3 * Projectile.direction, -2), new Vector2(20, 16), Projectile.rotation - 0.1f * Projectile.direction, Projectile.Center),
                        new(Projectile.Center + new Vector2(-12 * Projectile.direction, -2), new Vector2(10, 6), Projectile.rotation - MathHelper.PiOver4 * Projectile.direction, Projectile.Center),
                        ];
                case JunkType.Globe:
                    return [
                        new(Projectile.Center + new Vector2(0, 2), new Vector2(34, 40), Projectile.rotation, Projectile.Center),
                        new(Projectile.Center + new Vector2(0, -2), new Vector2(44, 20), Projectile.rotation, Projectile.Center),
                        ];
                case JunkType.Mallet:
                    return [
                        new(Projectile.Center, new Vector2(6, 56), Projectile.rotation, Projectile.Center),
                        new(Projectile.Center + new Vector2(0, -15), new Vector2(40, 24), Projectile.rotation, Projectile.Center),
                        ];
                case JunkType.Mug:
                    return [
                        new(Projectile.Center + new Vector2(-3 * Projectile.direction, 7), new Vector2(22, 28), Projectile.rotation, Projectile.Center),
                        new(Projectile.Center + new Vector2(2 * Projectile.direction, 3), new Vector2(34, 20), Projectile.rotation, Projectile.Center),
                        new(Projectile.Center + new Vector2(-19 * Projectile.direction, 0), new Vector2(24, 4), Projectile.rotation + MathHelper.PiOver4 * Projectile.direction, Projectile.Center),
                        ];
                case JunkType.Nail:
                    return [
                        new(Projectile.Center, new Vector2(6, 32), Projectile.rotation, Projectile.Center),
                        new(Projectile.Center + new Vector2(0, -13), new Vector2(26, 8), Projectile.rotation, Projectile.Center),
                        ];
                case JunkType.Plank:
                    return [
                        new(Projectile.Center, new Vector2(82, 30), Projectile.rotation, Projectile.Center),
                        ];
                case JunkType.Radio:
                    return [
                        new(Projectile.Center, new Vector2(74, 20), Projectile.rotation, Projectile.Center),
                        ];
                case JunkType.Saw:
                    return [
                        new(Projectile.Center + new Vector2(0, -7), new Vector2(72, 10), Projectile.rotation, Projectile.Center),
                        new(Projectile.Center + new Vector2(-26 * Projectile.direction, 0), new Vector2(20, 26), Projectile.rotation, Projectile.Center),
                        new(Projectile.Center + new Vector2(10 * Projectile.direction, 0), new Vector2(50, 10), Projectile.rotation - 0.2f * Projectile.direction, Projectile.Center),
                        ];
                case JunkType.Screw:
                    return [
                        new(Projectile.Center, new Vector2(10, 24), Projectile.rotation, Projectile.Center),
                        new(Projectile.Center + new Vector2(0, -8), new Vector2(18, 10), Projectile.rotation, Projectile.Center),
                        ];
                case JunkType.Screwdriver:
                    return [
                        new(Projectile.Center, new Vector2(2, 58), Projectile.rotation, Projectile.Center),
                        new(Projectile.Center + new Vector2(0, -15), new Vector2(6, 30), Projectile.rotation, Projectile.Center),
                        ];
                case JunkType.Shoe:
                    return [
                        new(Projectile.Center + new Vector2(8 * Projectile.direction, 0), new Vector2(30, 35), Projectile.rotation, Projectile.Center),
                        new(Projectile.Center + new Vector2(-4 * Projectile.direction, 12), new Vector2(38, 16), Projectile.rotation, Projectile.Center),
                        ];
                case JunkType.Thingy:
                    return [
                        new(Projectile.Center, new Vector2(22, 20), Projectile.rotation + 0.6f * Projectile.direction, Projectile.Center),
                        ];
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            var tex = junkTex[(int)myJunkType];
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition + Vector2.UnitY * Projectile.gfxOffY, null, Color.White * Projectile.Opacity, Projectile.rotation, tex.Size() * 0.5f, Projectile.scale, Projectile.direction > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);

            bool drawHitboxes = false;
            if (drawHitboxes && CanDamage() != false)
            {
                var squareTex = TexDict["Square"];
                var hitboxes = GetHitboxes();

                for (int i = 0; i < hitboxes.Count; i++)
                {
                    var hitbox = hitboxes[i];
                    var corners = GetRotatedCorners(hitbox.rect, hitbox.rot, hitbox.origin);
                    int length = corners.Length;
                    for (int j = 0; j < length; j++)
                    {
                        Vector2 cornerA = corners[j];
                        Vector2 cornerB = corners[(j + 1) % length];
                        int squareCount = (int)cornerA.Distance(cornerB);
                        if (squareCount <= 1)
                            continue;
                        for (int k = 0; k < squareCount; k++)
                        {
                            Vector2 squarePos = Vector2.Lerp(cornerA, cornerB, (float)k / (squareCount - 1));
                            Main.EntitySpriteDraw(squareTex, squarePos - Main.screenPosition, null, Color.Red, hitbox.rot, squareTex.Size() * 0.5f, 0.5f, SpriteEffects.None);
                        }
                    }
                }
            }
            
            return false;
        }
        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            Mallet.InflictRetribution(target);
        }
        public override bool? CanDamage() => Projectile.Opacity > 0.8f ? null : false;
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write((byte)myJunkType);
            writer.Write(Projectile.direction);
            writer.Write(Projectile.tileCollide);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            myJunkType = (JunkType)reader.ReadByte();
            Projectile.direction = reader.ReadInt32();
            Projectile.tileCollide = reader.ReadBoolean();
        }
    }
}
