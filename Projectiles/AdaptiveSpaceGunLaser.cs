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
using TerRoguelike.Utilities;
using static TerRoguelike.Projectiles.TileLineSegment;
using System.IO;
using Terraria.GameContent.UI.States;

namespace TerRoguelike.Projectiles
{
    public class AdaptiveSpaceGunLaser : ModProjectile, ILocalizedModType
    {
        public bool ableToHit = true;
        public TerRoguelikeGlobalProjectile modProj;
        TerRoguelikePlayer modPlayer;
        public int setTimeLeft = 480;
        public List<Vector2> oldPos = [];
        public List<float> oldRot = [];
        public List<bool> oldBounce = [];
        public int capPos = 100000;
        public bool firstReceive = true;
        public Vector2 startVel = Vector2.Zero;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;
        }
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = false;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.timeLeft = setTimeLeft;
            Projectile.penetrate = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            modProj = Projectile.ModProj();
        }

        public override bool? CanDamage() => ableToHit ? (bool?)null : false;
        public override bool? CanHitNPC(NPC target)
        {
            // used for not immediately cutting off afterimages when the projectile would in normal circumstances be killed.
            if (Projectile.penetrate == 1) 
                return false;

            return null;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.scale = Projectile.ai[0];
            //scale support
            Vector2 center = Projectile.Center;
            Projectile.width = (int)(8 * Projectile.scale);
            Projectile.height = (int)(8 * Projectile.scale);
            Projectile.Center = center;


            modPlayer = Main.player[Projectile.owner].ModPlayer();
            Projectile.rotation = Projectile.velocity.ToRotation();
            startVel = Projectile.velocity;
            AI();
        }
        public override void AI()
        {
            Projectile.scale = Projectile.ai[0];
            modPlayer ??= Main.player[Projectile.owner].ModPlayer();
            if (modPlayer.heatSeekingChip > 0)
                modProj.HomingAI(Projectile, (float)Math.Log(modPlayer.heatSeekingChip + 1, 1.2d) / (4000 * Projectile.MaxUpdates));

            if (modPlayer.bouncyBall > 0 || modPlayer.trash > 0)
                modProj.extraBounces = modPlayer.bouncyBall + modPlayer.trash;

            if (Projectile.timeLeft <= 20 || Projectile.localAI[0] == 1 || (Projectile.owner != Main.myPlayer && oldPos.Count > capPos))
            {
                Projectile.MaxUpdates = 1;
                Projectile.localAI[0] = 1;
                ableToHit = false;
                Projectile.velocity = Vector2.Zero;
                if (Projectile.timeLeft > 20)
                    Projectile.timeLeft = 20;
                return;
            }

            oldPos.Add(Projectile.Center);
            oldRot.Add(Projectile.rotation);
            oldBounce.Add(false);

            Projectile.rotation = Projectile.velocity.ToRotation();
            Vector2 premovepos = Projectile.Center;
            Vector2 end = Projectile.Center + Projectile.velocity;
            Projectile.Center = TileCollidePositionInLine(Projectile.Center, end);
            if (Projectile.position.X <= Main.leftWorld || Projectile.position.X + (float)Projectile.width >= Main.rightWorld || Projectile.position.Y <= Main.topWorld || Projectile.position.Y + (float)Projectile.height >= Main.bottomWorld)
            {
                Projectile.Center = end = premovepos;
                modProj.bounceCount = int.MaxValue;
            }
            if (Projectile.Center != end)
            {
                modProj.bounceCount++;
                if (modProj.bounceCount < 1 + modProj.extraBounces)
                {
                    oldBounce[^1] = true;
                    Projectile.Center = TileCollidePositionInLine(Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.UnitY) * 2, end, 2000, 0.1f);
                    Point tilePos = Projectile.Center.ToTileCoordinates();
                    Tile tile = ParanoidTileRetrieval(tilePos);
                    Vector2 hitVect = Projectile.velocity.SafeNormalize(Vector2.Zero);
                    Vector2 relativePosBeforeTile = premovepos - (tilePos.ToVector2() * 16);

                    List<TileLineSegment> lines = [];
                    switch (tile.Slope)
                    {
                        default:
                        case SlopeType.Solid:
                            if (tile.IsHalfBlock)
                            {
                                lines.Add(new(LineType.HalfRight));
                                lines.Add(new(LineType.HalfLeft));
                                lines.Add(new(LineType.HalfTop));
                                lines.Add(new(LineType.Bottom));
                                break;
                            }
                            lines.Add(new(LineType.Right));
                            lines.Add(new(LineType.Left));
                            lines.Add(new(LineType.Top));
                            lines.Add(new(LineType.Bottom));
                            break;
                        case SlopeType.SlopeDownLeft:
                            lines.Add(new(LineType.Bottom));
                            lines.Add(new(LineType.Left));
                            lines.Add(new(LineType.BottomLeft));
                            break;
                        case SlopeType.SlopeDownRight:
                            lines.Add(new(LineType.Bottom));
                            lines.Add(new(LineType.Right));
                            lines.Add(new(LineType.BottomRight));
                            break;
                        case SlopeType.SlopeUpLeft:
                            lines.Add(new(LineType.Top));
                            lines.Add(new(LineType.Left));
                            lines.Add(new(LineType.TopLeft));
                            break;
                        case SlopeType.SlopeUpRight:
                            lines.Add(new(LineType.Top));
                            lines.Add(new(LineType.Right));
                            lines.Add(new(LineType.TopRight));
                            break;
                    }
                    if (hitVect.X > 0)
                    {
                        lines.RemoveAll(x => x.type == LineType.Right || x.type == LineType.HalfRight);
                    }
                    else if (hitVect.X < 0)
                    {
                        lines.RemoveAll(x => x.type == LineType.Left || x.type == LineType.HalfLeft);
                    }
                    if (hitVect.Y > 0)
                    {
                        lines.RemoveAll(x => x.type == LineType.Bottom);
                    }
                    else if (hitVect.Y < 0)
                    {
                        lines.RemoveAll(x => x.type == LineType.Top || x.type == LineType.HalfTop);
                    }
                    List<Vector2?> intersections = [];
                    foreach (TileLineSegment line in lines)
                    {
                        var segment = line.lineSegment;
                        intersections.Add(TilePointOfIntersection(relativePosBeforeTile, relativePosBeforeTile + hitVect, segment.Start, segment.End));
                    }
                    int chosen = -1;
                    float distance = -1;
                    for (int i = 0; i < intersections.Count; i++)
                    {
                        Vector2? position = intersections[i];
                        if (position == null)
                            continue;
                        float thisDistance = premovepos.Distance((Vector2)position);
                        if (chosen == -1 || thisDistance < distance)
                        {
                            chosen = i;
                            distance = thisDistance;
                        }
                    }

                    if (chosen != -1)
                    {
                        float flipAngle = lines[chosen].flipOverRotation;
                        float newAngle = flipAngle + AngleSizeBetween(hitVect.ToRotation() + MathHelper.Pi, flipAngle);
                        Projectile.velocity = Projectile.velocity.Length() * newAngle.ToRotationVector2();
                        Projectile.rotation = Projectile.velocity.ToRotation();
                        //Projectile.timeLeft = setTimeLeft; // sorry guys game crashes, just too good :^)
                    }
                }
                else
                {
                    Projectile.localAI[0] = 1;
                    if (Projectile.timeLeft > 20)
                        Projectile.timeLeft = 20;
                    ableToHit = false;
                    oldRot.Add(Projectile.velocity.ToRotation());
                    Projectile.velocity = Vector2.Zero;
                    oldPos.Add(Projectile.Center);
                    oldBounce.Add(false);
                    Projectile.penetrate = 1;
                }
            }
            if (Projectile.localAI[0] == 0 && Projectile.owner == Main.myPlayer)
                Projectile.Damage();
            Projectile.timeLeft--;
            AI();
        }
        public override void PostAI()
        {

        }
        public override bool PreDraw(ref Color lightColor)
        {
            StartAdditiveSpritebatch();
            var tex = TextureAssets.Projectile[Type].Value;
            var circleTex = TexDict["Circle"];
            Vector2 origin = tex.Size() * 0.5f;
            float scaleMulti = (Math.Min(20, Projectile.timeLeft) / 20f);
            Vector2 scale = new Vector2(0.3f * scaleMulti);
            Color color = Color.LimeGreen;
            color.A = (byte)(color.A * 0.5f);
            Point ScreenPos = Main.Camera.ScaledPosition.ToPoint();
            Point ScreenDimensions = (new Vector2(Main.screenWidth, Main.screenHeight) / ZoomSystem.ScaleVector * 1.1f).ToPoint();
            Rectangle ScreenRect = new Rectangle(ScreenPos.X, ScreenPos.Y, ScreenDimensions.X, ScreenDimensions.Y);
            int increment = TerRoguelike.lowDetail ? 2 : 1;
            scale.X *= increment;
            for (int i = 0; i < oldPos.Count - 1; i++)
            {
                if (!ParticleManager.DrawScreenCheckWithFluff(oldPos[i], 100, ScreenRect))
                    continue;
                bool final = i == oldPos.Count - 1;
                int count = final ? 1 : (int)((oldPos[i] - oldPos[i + 1]).Length() * 0.7111f);
                if (count > 1)
                    count /= increment;
                for (int j = 0; j < count; j++)
                {
                    float completion = j / (float)count;
                    Vector2 drawPos = oldPos[i];
                    float drawRot = oldRot[i];
                    if (!final)
                    {
                        drawPos = Vector2.Lerp(drawPos, oldPos[i + 1], completion);
                        if (!oldBounce[i])
                            drawRot = drawRot.AngleLerp(oldRot[i + 1], completion);
                    }
                    float yMulti = Projectile.scale;
                    if (i == 0)
                    {
                        yMulti *= completion;
                    }
                    else if (i == oldPos.Count - 2)
                    {
                        yMulti *= 1 - completion;
                    }
                    
                    Main.EntitySpriteDraw(tex, drawPos - Main.screenPosition, null, color, drawRot, origin, new Vector2(scale.X, scale.Y * yMulti), SpriteEffects.None);
                }
            }
            StartVanillaSpritebatch();
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            //currently, no piercing is available for this.
            Projectile.timeLeft = 20;
            ableToHit = false;
            Projectile.position += Projectile.velocity;
            Projectile.rotation = Projectile.velocity.ToRotation();
            oldRot.Add(Projectile.rotation);
            oldPos.Add(Projectile.Center);
            Projectile.velocity = Vector2.Zero;
            Projectile.localAI[0] = 1;
            Projectile.netUpdate = true;
            Projectile.MaxUpdates = 1;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {

            return false;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(oldPos.Count);
            if (oldPos.Count > 0)
            {
                writer.WriteVector2(oldPos[0]);
                writer.Write(oldRot[0]);
                writer.WriteVector2(startVel);
            }
            else
            {
                writer.WriteVector2(Projectile.Center);
                writer.Write(Projectile.rotation);
                writer.WriteVector2(startVel);
            }
                
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            capPos = reader.ReadInt32();
            Vector2 pos = reader.ReadVector2();
            float rot = reader.ReadSingle();
            Vector2 vel = reader.ReadVector2();
            if (firstReceive)
            {
                firstReceive = false;
                Projectile.Center = pos;
                Projectile.rotation = rot;
                Projectile.velocity = startVel = vel;
                Projectile.timeLeft = setTimeLeft;
                Projectile.penetrate = 2;
                AI();
            }
        }
    }
    public class TileLineSegment
    {
        public enum LineType
        {
            Right,
            Bottom,
            Left,
            Top,
            BottomLeft,
            BottomRight,
            TopLeft,
            TopRight,
            HalfLeft,
            HalfRight,
            HalfTop,
        }
        public LineType type;
        public LineSegment lineSegment
        {
            get
            {
                return type switch
                {
                    LineType.Right => new(new(16, 0), new(16, 16)),
                    LineType.Bottom => new(new(0, 16), new(16, 16)),
                    LineType.Left => new(new(0, 0), new(0, 16)),
                    LineType.Top => new(new(0, 0), new(16, 0)),
                    LineType.BottomLeft => new(new(0, 0), new(16, 16)),
                    LineType.BottomRight => new(new(0, 16), new(16, 0)),
                    LineType.TopLeft => new(new(0, 16), new(16, 0)),
                    LineType.TopRight => new(new(0, 0), new(16, 16)),
                    LineType.HalfLeft => new(new(0, 8), new(0, 16)),
                    LineType.HalfRight => new(new(16, 8), new(16, 16)),
                    _ => new(new(0, 8), new(16, 8)), //half top
                };
            }
        }
        public float flipOverRotation
        {
            get
            {
                return type switch
                {
                    LineType.Right => 0,
                    LineType.Bottom => MathHelper.PiOver2,
                    LineType.Left => MathHelper.Pi,
                    LineType.Top => -MathHelper.PiOver2,
                    LineType.BottomLeft => -MathHelper.PiOver4,
                    LineType.BottomRight => -MathHelper.PiOver4 * 3,
                    LineType.TopLeft => MathHelper.PiOver4,
                    LineType.TopRight => MathHelper.PiOver4 * 3,
                    LineType.HalfLeft => MathHelper.Pi,
                    LineType.HalfRight => 0,
                    _ => -MathHelper.PiOver2, //half top
                };
            }
        }

        public TileLineSegment(LineType Type)
        {
            type = Type;
        }
    }
}
