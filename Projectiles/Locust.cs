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
    public class Locust : ModProjectile, ILocalizedModType
    {
        public int maxTimeLeft;
        public float timeOffset;
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
            Projectile.timeLeft = 230;
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
            Projectile.rotation = Main.rand.NextFloat(0.3f, 0.38f) * Math.Sign(Projectile.velocity.X);
            timeOffset = Main.rand.NextFloat(MathHelper.TwoPi);
        }
        public override void AI()
        {
            Projectile.frameCounter++;
            Projectile.frame = Projectile.frameCounter / 3 % Main.projFrames[Type];
            Tile tile = TerRoguelikeUtils.ParanoidTileRetrieval(Projectile.Center.ToTileCoordinates());
            if (tile.IsTileSolidGround() && TileID.Sets.Platforms[tile.TileType])
                Projectile.Center += new Vector2(0, 0.4f);
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
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            Vector3 colorHSL = Main.rgbToHsl(Color.Lerp(Color.Purple, Color.White, (float)Math.Cos(Main.GlobalTimeWrappedHourly * 20 + timeOffset) * 0.15f + 0.3f));

            GameShaders.Misc["TerRoguelike:BasicTint"].UseOpacity(1f);
            GameShaders.Misc["TerRoguelike:BasicTint"].UseColor(Main.hslToRgb(1 - colorHSL.X, colorHSL.Y, colorHSL.Z));
            GameShaders.Misc["TerRoguelike:BasicTint"].Apply();
            for (int i = 0; i < 8; i++)
            {
                Vector2 offset = (Vector2.UnitX * 1).RotatedBy(Projectile.rotation + (i * MathHelper.PiOver4));
                Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition + offset, frame, Color.White, Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, Projectile.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            }
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, Projectile.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            return false;
        }
    }
}
