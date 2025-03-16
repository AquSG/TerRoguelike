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

namespace TerRoguelike.Projectiles
{
    public class AdaptiveBladeSlash : ModProjectile, ILocalizedModType
    {
        public TerRoguelikeGlobalProjectile modProj;
        TerRoguelikePlayer modPlayer;
        public Player player;
        public Vector2 stuckPosition = Vector2.Zero;
        public Texture2D squareTex;
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
            squareTex = TextureManager.TexDict["Square"];
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
        }
        public override void AI()
        {
            player ??= Main.player[Projectile.owner];
            modPlayer ??= player.ModPlayer();

            if (stuckPosition == Vector2.Zero)
            {
                //keep this shit stuck to the player
                stuckPosition = player.position - Projectile.position;
            }
            Projectile.position = player.position - stuckPosition + (Vector2.UnitY * player.gfxOffY);
            Projectile.frame = (int)(Projectile.localAI[0] / 4);
            Projectile.localAI[0] += 1 * player.GetAttackSpeed(DamageClass.Generic); // animation speed scales with attack speed
            if (Projectile.frame > Main.projFrames[Projectile.type]) // kill when done animating
            {
                Projectile.Kill();
            }
        }
        //rotating rectangle hitbox collision
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Projectile.owner < 0)
                return null;
            Point dimensions = new Point((int)(Projectile.height * 0.25f), (int)(Projectile.height * 0.25f));

            for (int i = -1; i <= 1; i++)
            {
                int pullIn = -Math.Abs(i);
                float radius = Projectile.height * (0.36f + (0.066f * pullIn)) * 1.2f;
                Vector2 offset = Vector2.Zero;
                offset.Y += Projectile.height * 0.25f * i;
                offset.X += radius * (0.2f + 0.1f * pullIn);

                Vector2 pos = Projectile.Center + offset.RotatedBy(Projectile.rotation);
                if (targetHitbox.ClosestPointInRect(pos).Distance(pos) <= radius)
                    return true;
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

        //only hit if in the first 3 frames of animation
        public override bool? CanDamage() => Projectile.frame <= 3 ? (bool?)null : false;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            SpriteEffects spriteEffects = modProj.swingDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, new Rectangle(0, frameHeight * Projectile.frame, texture.Width, frameHeight), Color.Lerp(lightColor, Color.White, 0.5f), Projectile.rotation + (MathHelper.Pi * modProj.swingDirection / 12f), new Vector2(texture.Width / 2f, (frameHeight / 2f)), Projectile.scale, spriteEffects);

            if (false)
            {
                for (int i = -1; i <= 1; i++)
                {
                    int pullIn = -Math.Abs(i);
                    float radius = Projectile.height * (0.36f + (0.066f * pullIn)) * 1.2f;
                    Vector2 offset = Vector2.Zero;
                    offset.Y += Projectile.height * 0.25f * i;
                    offset.X += radius * (0.2f + 0.1f * pullIn);

                    Vector2 pos = Projectile.Center + offset.RotatedBy(Projectile.rotation);
                    for (int j = 0; j < 120; j++)
                    {
                        Main.EntitySpriteDraw(squareTex, pos + ((j / 120f * MathHelper.TwoPi).ToRotationVector2() * radius) - Main.screenPosition, null, Color.Red, 0, squareTex.Size() * 0.5f, 1f, SpriteEffects.None);
                    }
                }
            }
            return false;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(modProj.swingDirection);
            writer.WriteVector2(stuckPosition);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            modProj.swingDirection = reader.ReadInt32();
            stuckPosition = reader.ReadVector2();
        }
    }
}
