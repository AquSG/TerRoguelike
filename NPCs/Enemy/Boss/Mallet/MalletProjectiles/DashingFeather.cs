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
using System.IO;

namespace TerRoguelike.NPCs.Enemy.Boss.Mallet.MalletProjectiles
{
    public class DashingFeather : ModProjectile, ILocalizedModType
    {
        public int maxTimeLeft;
        public int targetPlayer = -1;
        public int targetNPC = -1;
        public Vector2 spawnPos = Vector2.Zero;
        public Vector2 targetCenter
        {
            get
            {
                return new Vector2(Projectile.localAI[0], Projectile.localAI[1]);
            }
            set
            {
                Projectile.localAI[0] = value.X;
                Projectile.localAI[1] = value.Y;
            }
        }
        public Vector2 offsetVector
        {
            get
            {
                return new Vector2(Projectile.ai[0], Projectile.ai[1]);
            }
            set
            {
                Projectile.ai[0] = value.X;
                Projectile.ai[1] = value.Y;
            }
        }
        Entity target
        {
            get
            {
                if (targetPlayer >= 0)
                {
                    return Main.player[targetPlayer];
                }
                else if (targetNPC >= 0)
                {
                    return Main.npc[targetNPC];
                }
                else
                    return null;
            }
        }
        public Vector2 reticlePos = Vector2.Zero;
        public Vector2 reticleOffset = Vector2.Zero;
        public override string Texture => "TerRoguelike/Projectiles/InvisibleProj";
        public Texture2D FeatherSize1, Square, Line;
        public int setupTime = 15;
        public int launchTime = 63;
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = maxTimeLeft = 360;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.netImportant = true; //makes projectile not able to be replaced if at the projectile cap 
            FeatherSize1 = TexDict["FeatherSize1"];
            Square = TexDict["Square"];
            Line = TexDict["LerpLineGradient"];
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            if (maxTimeLeft - Projectile.timeLeft < setupTime)
            {
                behindNPCs.Add(index);
                Projectile.hide = true;
            }
            else
                Projectile.hide = false;
        }
        public override void OnSpawn(IEntitySource source)
        {
            if (source is EntitySource_Parent parentSource)
            {
                if (parentSource.Entity is NPC)
                {
                    NPC parent = Main.npc[parentSource.Entity.whoAmI];
                    var modparent = parent.ModNPC();
                    if (modparent != null)
                    {
                        targetNPC = modparent.targetNPC;
                        targetPlayer = modparent.targetPlayer;
                        spawnPos = Projectile.Center;
                        reticleOffset = Projectile.velocity;
                        Projectile.velocity = Vector2.Zero;
                        if (target == null)
                        {
                            Projectile.active = false;
                            return;
                        }
                        targetCenter = target.Center;
                        return;
                    }
                }
            }
            Projectile.active = false;
        }
        public override bool PreAI()
        {
            if (target != null)
            {
                if (targetPlayer >= 0)
                {
                    var player = Main.player[targetPlayer];
                    if (!player.active || player.dead)
                    {
                        targetPlayer = -1;
                        Projectile.netUpdate = true;
                    }
                }
                else if (targetNPC >= 0)
                {
                    var npc = Main.npc[targetNPC];
                    if (!npc.active)
                    {
                        targetNPC = -1;
                        Projectile.netUpdate = true;
                    }
                }
            }
            if (target != null)
                targetCenter = target.Center;
            return true;
        }
        public override void AI()
        {
            Projectile.netSpam = 0;
            Vector2 targetPos;

            int time = maxTimeLeft - Projectile.timeLeft;
            if (time <= setupTime)
            {
                reticlePos = targetCenter + reticleOffset;

                targetPos = targetCenter + offsetVector;


                float completion = time / (float)setupTime;
                float direction = targetPos.Y > targetCenter.Y ? (targetPos.X < targetCenter.X ? MathHelper.Pi : 0) : -MathHelper.PiOver2;

                Projectile.Center = Vector2.Lerp(spawnPos, targetPos, MathHelper.SmoothStep(0, 1, completion));
                float paraMulti = -(float)Math.Pow((MathHelper.SmoothStep(0, 1, completion) - 0.5f) * 2, 2) + 1;
                Projectile.Center += (Vector2.UnitX * 64 * paraMulti).RotatedBy(direction);
                Projectile.rotation = (Projectile.position - Projectile.oldPosition).ToRotation().AngleLerp((reticlePos - targetPos).ToRotation(), MathHelper.SmoothStep(0, 1, completion));
            }
            else
            {
                targetPos = reticlePos + offsetVector;

                if (time == setupTime + 1)
                {
                    Projectile.rotation = (reticlePos - Projectile.Center).ToRotation();
                    //Projectile.netUpdate = true;
                }
                if (time < launchTime)
                    Projectile.velocity += Projectile.rotation.ToRotationVector2() * -0.06f;
                else
                    Projectile.velocity = Projectile.rotation.ToRotationVector2() * 20;

                int prepTime = time - setupTime - 1;
                if (time < launchTime && prepTime % 12 == 0)
                {
                    SoundEngine.PlaySound(Mallet.Warning with { Volume = 0.5f }, Projectile.Center);
                }
                if (time == launchTime)
                {
                    Projectile.MaxUpdates = 2;
                    SoundEngine.PlaySound(Mallet.TalonSwipe with { Volume = 0.7f }, Projectile.Center);
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            int time = maxTimeLeft - Projectile.timeLeft;

            if (time >= setupTime && time < launchTime)
            {
                int prepTime = time - setupTime - 1;
                Color warnColor;
                if (time / 12 % 2 == 0)
                    warnColor = Color.Yellow;
                else
                    warnColor = Color.Red;
                warnColor.A = (byte)(warnColor.A * 0.55f);

                Vector2 squarePos = Projectile.Center - Main.screenPosition;
                Vector2 squareScale = new Vector2(300, 1 * Projectile.scale);

                TerRoguelikeUtils.StartNonPremultipliedSpritebatch();
                Main.EntitySpriteDraw(Square, squarePos, null, warnColor, Projectile.rotation, Square.Size() * new Vector2(0, 0.5f), squareScale, SpriteEffects.None);
                Main.EntitySpriteDraw(Line, squarePos + Projectile.rotation.ToRotationVector2() * squareScale.X * Square.Width, null, warnColor, Projectile.rotation, Line.Size() * new Vector2(0, 0.5f), Projectile.scale, SpriteEffects.None);
                TerRoguelikeUtils.StartVanillaSpritebatch();
            }

            float opacity = 1f;
            if (Projectile.timeLeft < 40)
                opacity *= Projectile.timeLeft / 40f;
            var featherFrame = FeatherSize1.Frame(1, 4, 0, Projectile.frame);
            Main.EntitySpriteDraw(FeatherSize1, Projectile.Center - Main.screenPosition, featherFrame, Color.White * opacity, Projectile.rotation, featherFrame.Size() * 0.5f, Projectile.scale, SpriteEffects.None);

            return false;
        }
        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            Mallet.InflictRetribution(target);
        }
        public override bool? CanDamage() => maxTimeLeft - Projectile.timeLeft < setupTime || Projectile.timeLeft < 20 ? false : null;
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(targetPlayer);
            writer.Write(targetNPC);
            writer.WriteVector2(spawnPos);
            writer.WriteVector2(targetCenter);
            writer.WriteVector2(reticlePos);
            writer.WriteVector2(reticleOffset);
            writer.Write(Projectile.rotation);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            targetPlayer = reader.ReadInt32();
            targetNPC = reader.ReadInt32();
            spawnPos = reader.ReadVector2();
            targetCenter = reader.ReadVector2();
            reticlePos = reader.ReadVector2();
            reticleOffset = reader.ReadVector2();
            Projectile.rotation = reader.ReadSingle();
        }
    }
}
