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
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.DataStructures;

namespace TerRoguelike.Projectiles
{
    public class Bee : ModProjectile, ILocalizedModType
    {
        public int maxTimeLeft;
        public float timeOffset;
        Entity target = null;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
        }
        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = maxTimeLeft = 300;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hide = true;
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            timeOffset = Main.rand.NextFloat(MathHelper.TwoPi);
        }
        public override void AI()
        {
            var modProj = Projectile.ModProj();
            target = modProj.GetTarget(Projectile);

            Projectile.frameCounter++;
            Projectile.frame = Projectile.frameCounter / 3 % Main.projFrames[Type];
            Projectile.rotation = Projectile.velocity.ToRotation();

            float direction = target != null ? (target.Center - Projectile.Center).ToRotation() : Projectile.rotation;

            if (Math.Abs(TerRoguelikeUtils.AngleSizeBetween(Projectile.velocity.ToRotation(), direction)) < MathHelper.PiOver2)
            {
                float newRot = Projectile.velocity.ToRotation().AngleTowards(direction, 0.004f);
                Projectile.velocity = (Vector2.UnitX * Projectile.velocity.Length()).RotatedBy(newRot);
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
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            int frameHeight = tex.Height / Main.projFrames[Type];
            Rectangle frame = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);

            bool pass = false;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(-tex.Width * 0.5f, -frameHeight * 0.5f);
                if (i == 1 || i == 2)
                    offset.X *= -1;
                if (i >= 3)
                    offset.Y *= -1;

                if (!TerRoguelikeUtils.TileCollisionAtThisPosition(Projectile.Center + offset.RotatedBy(Projectile.rotation)))
                {
                    pass = true;
                    break;
                }
            }

            if (!pass)
                return false;

            float scaleMultiplier = MathHelper.Clamp((maxTimeLeft - Projectile.timeLeft) / 10f, 0, 1);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            Vector3 colorHSL = Main.rgbToHsl(Color.Lerp(Color.Goldenrod, Color.White, (float)Math.Cos(Main.GlobalTimeWrappedHourly * 20 + timeOffset) * 0.15f + 0.3f));

            GameShaders.Misc["TerRoguelike:BasicTint"].UseOpacity(1f);
            GameShaders.Misc["TerRoguelike:BasicTint"].UseColor(Main.hslToRgb(1 - colorHSL.X, colorHSL.Y, colorHSL.Z));
            GameShaders.Misc["TerRoguelike:BasicTint"].Apply();
            for (int i = 0; i < 8; i++)
            {
                Vector2 offset = (Vector2.UnitX * 1).RotatedBy(Projectile.rotation + (i * MathHelper.PiOver4));
                Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition + offset, frame, Color.White, Projectile.rotation, frame.Size() * 0.5f, Projectile.scale * scaleMultiplier, Math.Sign(Projectile.velocity.X) == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None);
            }
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, frame.Size() * 0.5f, Projectile.scale * scaleMultiplier, Math.Sign(Projectile.velocity.X) == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None);
            return false;
        }
    }
}
