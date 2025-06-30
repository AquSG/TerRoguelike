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
    public class SplittingFeather : ModProjectile, ILocalizedModType
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
        public float reticleRot = 0;
        public override string Texture => "TerRoguelike/Projectiles/InvisibleProj";
        public Texture2D FeatherSize1, CircleInner, CircleOuter, CircleSpread, FeatherExplosion;
        public SlotId FeatherSlot;
        public int setupTime = 15;
        public int launchTime = 35;
        public int hitTime = 50;
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = maxTimeLeft = 120;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.netImportant = true; //makes projectile not able to be replaced if at the projectile cap 
            FeatherSize1 = TexDict["FeatherSize1"];
            CircleInner = TexDict["CircleInner"];
            CircleOuter = TexDict["CircleOuter"];
            CircleSpread = TexDict["CircleSpread"];
            FeatherExplosion = TexDict["FeatherExplosion"];
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
            var type = (ReticleType)(int)Projectile.ai[2];

            int time = maxTimeLeft - Projectile.timeLeft;
            if (time <= setupTime)
            {
                reticlePos = targetCenter + reticleOffset;
                reticleRot = (targetCenter + (target != null ? target.velocity : Vector2.Zero) - reticlePos).ToRotation();

                targetPos = reticlePos + offsetVector;


                float completion = time / (float)setupTime;
                float direction = targetPos.Y > targetCenter.Y ? (targetPos.X < targetCenter.X ? MathHelper.Pi : 0) : -MathHelper.PiOver2;

                Projectile.Center = Vector2.Lerp(spawnPos, targetPos, MathHelper.SmoothStep(0, 1, completion));
                float paraMulti = -(float)Math.Pow((MathHelper.SmoothStep(0, 1, completion) - 0.5f) * 2, 2) + 1;
                Projectile.Center += (Vector2.UnitX * 64 * paraMulti).RotatedBy(direction);
                Projectile.rotation = (Projectile.position - Projectile.oldPosition).ToRotation().AngleLerp((reticlePos - targetPos).ToRotation(), MathHelper.SmoothStep(0, 1, completion));

                if (time == 0)
                {
                    FeatherSlot = SoundEngine.PlaySound(Mallet.Feather with { Variants = [1], Volume = 0.6f, MaxInstances = 10 }, targetPos);
                }
            }
            else
            {
                //if (time == setupTime + 1)
                    //Projectile.netUpdate = true;

                targetPos = reticlePos + offsetVector;
                if (time < launchTime)
                {
                    Projectile.frameCounter++;
                }
                else if (time <= hitTime)
                {
                    //if (time == launchTime)
                        //Projectile.netUpdate = true;

                    Projectile.frameCounter = 0;
                    int movingTime = time - launchTime;
                    float movingCompletion = movingTime / (float)(hitTime - launchTime);
                    Projectile.Center = Vector2.Lerp(targetPos, reticlePos, movingCompletion);
                    if (time == hitTime)
                    {
                        SoundEngine.PlaySound(Mallet.FeatherBoom with { Variants = [2], Volume = 0.4f, MaxInstances = 10 }, Projectile.Center);

                        if (!TerRoguelike.mpClient)
                        {
                            if (type == ReticleType.Spread)
                            {
                                int count = 7;
                                float angleIncr = MathHelper.PiOver2 / 9;
                                for (int i = 0; i < count; i++)
                                {
                                    float rot = reticleRot;
                                    if (i % 2 == 0)
                                        rot += (i / 2) * angleIncr;
                                    else
                                        rot -= ((i + 1) / 2) * angleIncr;

                                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + rot.ToRotationVector2() * 20, rot.ToRotationVector2() * 12, ModContent.ProjectileType<SplitFeather>(), Projectile.damage, 0);
                                }
                            }
                            else if (type == ReticleType.Circle)
                            {
                                int count = 11;
                                float angleIncr = MathHelper.TwoPi / count;
                                for (int i = 0; i < count; i++)
                                {
                                    float rot = reticleRot + angleIncr * i;

                                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + rot.ToRotationVector2() * 20, rot.ToRotationVector2() * 12, ModContent.ProjectileType<SplitFeather>(), Projectile.damage, 0);
                                }
                            }
                        }
                    }
                }
                
                Projectile.rotation = (reticlePos - Projectile.Center).ToRotation();
            }
            Projectile.frame = Projectile.frameCounter / 4 % 4;

            reticleRot = reticleRot.AngleTowards((targetCenter - reticlePos).ToRotation(), type == ReticleType.Circle ? 10 : 0.025f);

            if (time < launchTime && SoundEngine.TryGetActiveSound(FeatherSlot, out var sound) && sound.IsPlaying)
            {
                sound.Position = Projectile.Center;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            int time = maxTimeLeft - Projectile.timeLeft;

            if (time >= setupTime)
            {
                int reticleTime = time - setupTime;
                int reticleFrameCounter = (int)(((float)reticleTime / (hitTime - setupTime)) * 8);

                if (reticleFrameCounter < 8)
                {
                    var type = (ReticleType)(int)Projectile.ai[2];
                    var reticleFrame = CircleInner.Frame(1, 10, 0, reticleFrameCounter);
                    var reticleDrawPos = reticlePos - Main.screenPosition;
                    var reticleOrigin = reticleFrame.Size() * 0.5f;

                    Main.EntitySpriteDraw(CircleInner, reticleDrawPos, reticleFrame, Color.White, 0, reticleOrigin, Projectile.scale, SpriteEffects.None);

                    if (type == ReticleType.Spread)
                        Main.EntitySpriteDraw(CircleSpread, reticleDrawPos, reticleFrame, Color.White, reticleRot, reticleOrigin, Projectile.scale, SpriteEffects.None);
                    else if (type == ReticleType.Circle)
                        Main.EntitySpriteDraw(CircleOuter, reticleDrawPos, reticleFrame, Color.White, 0, reticleOrigin, Projectile.scale, SpriteEffects.None);
                }
            }
            if (time < hitTime)
            {
                var featherFrame = FeatherSize1.Frame(1, 4, 0, Projectile.frame);
                Main.EntitySpriteDraw(FeatherSize1, Projectile.Center - Main.screenPosition, featherFrame, Color.White, Projectile.rotation, featherFrame.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            }
            else
            {
                int explosionFrameCounter = (time - hitTime) / 4;
                if (explosionFrameCounter < 6)
                {
                    var explosionFrame = FeatherExplosion.Frame(1, 6, 0, explosionFrameCounter);
                    Main.EntitySpriteDraw(FeatherExplosion, Projectile.Center - Main.screenPosition, explosionFrame, Color.White, 0, explosionFrame.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
                }
                
            }

            return false;
        }
        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            Mallet.InflictRetribution(target);
        }
        public override bool? CanDamage() => maxTimeLeft - Projectile.timeLeft < setupTime || maxTimeLeft - Projectile.timeLeft > hitTime ? false : null;
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(targetPlayer);
            writer.Write(targetNPC);
            writer.WriteVector2(spawnPos);
            writer.WriteVector2(targetCenter);
            writer.WriteVector2(reticlePos);
            writer.WriteVector2(reticleOffset);
            writer.Write(Projectile.rotation);
            writer.Write(reticleRot);
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
            reticleRot = reader.ReadSingle();
        }
        public enum ReticleType
        {
            Spread = 0,
            Circle = 1,
        }
    }
}
