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
using TerRoguelike.Utilities;
using Terraria.GameContent;
using Terraria.GameContent.UI.ResourceSets;
using Terraria.DataStructures;
using System.IO;
using static TerRoguelike.Projectiles.AdaptiveSaberHoldout;
using static TerRoguelike.Managers.TextureManager;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using TerRoguelike.Particles;
using XPT.Core.Audio.MP3Sharp.Decoding.Decoders.LayerIII;
using TerRoguelike.Items.Weapons;

namespace TerRoguelike.Projectiles
{
    public class AdaptiveSaberSlash : ModProjectile, ILocalizedModType
    {
        public Player Owner => Main.player[Projectile.owner];
        public TerRoguelikeGlobalProjectile modProj;
        TerRoguelikePlayer modPlayer;
        public Player player;
        public Vector2 stuckPosition = Vector2.Zero;
        public Texture2D squareTex;
        public bool firstUpdate = true;
        public float myanim = 0;
        public float effectiveRot = 0;
        public Vector2 effectivePos = Vector2.Zero;
        public Vector2 oldEffectivePos = Vector2.Zero;
        public float anchorRot = 0;
        public SwordColor swordLevel
        {
            get { return (SwordColor)Projectile.ai[0]; }
            set { Projectile.ai[0] = (int)value; }
        }

        public ref float rainbowProg => ref Projectile.ai[1];
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 5;
        }
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 68;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.timeLeft = 1000;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            modProj = Projectile.ModProj();
            squareTex = TexDict["Square"];
            Projectile.manualDirectionChange = true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            //scale support
            Projectile.position = Projectile.Center + new Vector2(-34 * Projectile.scale, -34 * Projectile.scale);
            Projectile.width = (int)(68 * Projectile.scale);
            Projectile.height = (int)(68 * Projectile.scale);

            player ??= Main.player[Projectile.owner];
            modPlayer ??= player.ModPlayer();

            stuckPosition = player.position - Projectile.position;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.velocity = Vector2.Zero;
            anchorRot = modPlayer.playerToCursor.ToRotation();
        }
        public override void AI()
        {
            float effectiveAnim = Math.Abs(modPlayer.swingAnimCompletion);
            if (myanim > effectiveAnim || myanim == effectiveAnim || effectiveAnim >= 1)
            {
                if (myanim == 0.00001f)
                {
                    myanim = 0.5f;
                }
                else if (myanim < 1 && Owner.GetAttackSpeed(DamageClass.Generic) >= 4)
                {
                    myanim = 1;
                }
                else
                {
                    Projectile.Kill();
                    return;
                }
            }
            else
            {
                if (myanim == 0.00001f && effectiveAnim > 0.5f)
                    myanim = 0.5f;
                else
                    myanim = effectiveAnim;
            }
               

            int ownerdir = Owner.direction;
            Owner.direction = Projectile.direction;
            oldEffectivePos = effectivePos;
            Projectile.localAI[2] = effectiveRot;
            effectiveRot = AdaptiveSaber.GetArmRotation(anchorRot, myanim, modProj.swingDirection, Projectile.direction) + MathHelper.PiOver2 * Projectile.direction + (Projectile.rotation - anchorRot);

            effectivePos = Owner.GetFrontHandPosition(Owner.compositeFrontArm.stretch, Owner.compositeFrontArm.rotation).Floor() + new Vector2(28 * Projectile.direction, -28).RotatedBy(effectiveRot) * (Projectile.scale - (Projectile.scale - 1) * 0.5f) + Vector2.UnitY * Owner.gfxOffY;
            int particleCount = (int)(effectivePos.Distance(oldEffectivePos) * 0.5f);
            Owner.direction = ownerdir;
            

            if (firstUpdate)
            {
                particleCount = 1;
                oldEffectivePos = effectivePos;
                Projectile.localAI[2] = effectiveRot;
                Projectile.netUpdate = true;
                firstUpdate = false;
            }
            else
            {
                if (swordLevel == SwordColor.Rainbow)
                    rainbowProg += 0.0154936875f;
            }
            player ??= Main.player[Projectile.owner];
            modPlayer ??= player.ModPlayer();


            for (int i = 0; i < particleCount; i++)
            {
                float completion = i / (float)particleCount;
                Vector2 thisPos = Vector2.Lerp(oldEffectivePos, effectivePos, completion);
                float thisRot = (Projectile.localAI[2] - MathHelper.PiOver4 * Projectile.direction).AngleLerp(effectiveRot - MathHelper.PiOver4 * Projectile.direction, completion);
                ParticleManager.AddParticle(new Beam(thisPos, Vector2.Zero, 5, GetSwordColor(swordLevel, rainbowProg), new Vector2(0.1f * Projectile.scale), thisRot, 0, 5, true));
            }

            if (stuckPosition == Vector2.Zero)
            {
                //keep this shit stuck to the player
                stuckPosition = player.position - Projectile.position;
            }
            Projectile.position = player.position - stuckPosition + (Vector2.UnitY * player.gfxOffY);
            Projectile.frame = (int)(Projectile.localAI[0] / 4);
            Projectile.localAI[0] += 1 * player.GetAttackSpeed(DamageClass.Generic); // animation speed scales with attack speed
        }
        //rotating rectangle hitbox collision
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Projectile.owner < 0)
                return null;


            if (Owner.GetAttackSpeed(DamageClass.Generic) < 4)
            {
                for (int j = -2; j <= 1; j++)
                {
                    int amt = j == -2 ? 12 : 16;
                    float radius = 8 * Projectile.scale;
                    Vector2 pos = effectivePos + (effectiveRot - MathHelper.PiOver4 * Projectile.direction).ToRotationVector2() * amt * Projectile.direction * j * Projectile.scale;
                    if (targetHitbox.ClosestPointInRect(pos).Distance(pos) <= radius)
                        return true;
                }
            }
            else
            {
                for (int i = -1; i <= 1; i++)
                {
                    int pullIn = -Math.Abs(i);
                    float radius = Projectile.height * (0.36f + (0.066f * pullIn)) * 1.4f * Projectile.scale;
                    Vector2 offset = Vector2.Zero;
                    offset.Y += Projectile.height * 0.4f * i * Projectile.scale;
                    offset.X += radius * (0.2f + 0.1f * pullIn);

                    Vector2 pos = Projectile.Center + offset.RotatedBy(Projectile.rotation);
                    if (targetHitbox.ClosestPointInRect(pos).Distance(pos) <= radius)
                        return true;
                }
            }
            
            return false;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            int direction = Math.Sign((target.Center - Main.player[Projectile.owner].Center).X);
            if (direction == 0)
                direction = Main.rand.NextBool() ? 1 : -1;
            modifiers.HitDirectionOverride = direction;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.localAI[1] == 1)
                return;
            int checktype = ModContent.ProjectileType<AdaptiveSaberHoldout>();
            foreach (Projectile proj in Main.ActiveProjectiles)
            {
                if (proj.owner == Projectile.owner && proj.type == checktype)
                {
                    Projectile.localAI[1] = 1;
                    if (proj.ai[1] < (int)SwordColor.Rainbow)
                    {
                        proj.ai[1]++;
                        proj.netUpdate = true;
                    }
                    break;
                }
            }
        }

        //only hit if in the first 3 frames of animation
        public override bool? CanDamage() => null;
        public override bool PreDraw(ref Color lightColor)
        {
            StartAdditiveSpritebatch();
            var tex = TexDict["Beam"];
            var frame = tex.Frame();
            Vector2 origin = frame.Size() * 0.5f;

            Color color = GetSwordColor(swordLevel, rainbowProg);
            float rotation = effectiveRot - MathHelper.PiOver4 * Projectile.direction;

            Vector2 basePos = effectivePos;
            Main.EntitySpriteDraw(tex, basePos - Main.screenPosition, frame, color, rotation, origin, 0.1f * Projectile.scale, SpriteEffects.None);
            StartVanillaSpritebatch();
            if (false)
            {
                for (int j = -2; j <= 1; j++)
                {
                    int amt = j == -2 ? 12 : 16;
                    float radius = 8 * Projectile.scale;
                    Vector2 pos = effectivePos + (effectiveRot - MathHelper.PiOver4 * Owner.direction).ToRotationVector2() * amt * Owner.direction * j * Projectile.scale;
                    for (int i = 0; i < 100; i++)
                    {
                        float completion = i / 100f;
                        Main.EntitySpriteDraw(squareTex, pos - Main.screenPosition + radius * Vector2.UnitX.RotatedBy(completion * MathHelper.TwoPi), null, Color.Red, 0, squareTex.Size() * 0.5f, 1f, SpriteEffects.None);
                    }
                }
            }
            return false;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(modProj.swingDirection);
            writer.WriteVector2(stuckPosition);
            writer.Write(anchorRot);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            modProj.swingDirection = reader.ReadInt32();
            stuckPosition = reader.ReadVector2();
            anchorRot = reader.ReadSingle();
        }
    }
}
