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
using Terraria.ModLoader.IO;
using Terraria.GameContent;
using static TerRoguelike.Managers.TextureManager;

namespace TerRoguelike.Projectiles
{
    public class RollingBoulder : ModProjectile, ILocalizedModType
    {
        public Texture2D lightTex;
        public int maxTimeLeft;
        public override string Texture => "TerRoguelike/Projectiles/TempleBoulder";
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = maxTimeLeft = 1200;
            Projectile.penetrate = -1;
            lightTex = TexDict["TempleBoulderGlow"];
            Projectile.ModProj().killOnRoomClear = true;
            Projectile.hide = true;
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }
        public override void OnSpawn(IEntitySource source)
        {

        }
        public override void AI()
        {
            Projectile.rotation += MathHelper.Clamp(Projectile.velocity.X * 0.08f, -0.3f, 0.3f);
            Projectile.velocity.Y += 0.24f;

            for (int i = 0; i < 3; i++)
            {
                Vector2 checkPos = Projectile.BottomLeft + new Vector2(Projectile.width * 0.5f * i, 0);
                Point bottomTilePos = checkPos.ToTileCoordinates();
                Vector2 checkVel = Projectile.velocity;
                if (checkVel.Y == 0)
                    checkVel.Y = 0.001f;
                Point extraBottomTimePos = (checkPos + Vector2.UnitY * checkVel.Y).ToTileCoordinates();
                if (bottomTilePos.Y != extraBottomTimePos.Y && Projectile.velocity.Y >= 0)
                {
                    Point futureTile = (checkPos + checkVel).ToTileCoordinates();
                    Tile tile = TerRoguelikeUtils.ParanoidTileRetrieval(futureTile);
                    if (TileID.Sets.Platforms[tile.TileType] && tile.IsTileSolidGround() && tile.BlockType == BlockType.Solid)
                    {
                        Vector2 oldVel = Projectile.velocity;
                        Projectile.velocity.Y = 0;
                        Projectile.position.Y = futureTile.ToWorldCoordinates(0, 0).Y - Projectile.height;
                        OnTileCollide(oldVel);
                    }
                }
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (oldVelocity.Y > 2.25f)
            {
                SoundEngine.PlaySound(SoundID.Dig with { Volume = 0.2f, Pitch = -0.8f, MaxInstances = 10 }, Projectile.Center);
            }
            // If the projectile hits the left or right side of the tile, kill
            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
            {
                return true;
            }
            // If the projectile hits the top or bottom side of the tile, reverse the Y velocity
            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
            {
                Projectile.velocity.Y = -oldVelocity.Y * 0.4f;
                if (oldVelocity.Y > 3)
                    Projectile.velocity.X = oldVelocity.X * 0.9f;
            }
            return false;
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item70 with { Volume = 0.3f, MaxInstances = 3 }, Projectile.Center);
            for (int i = 0; i < 30; i++)
            {
                int dustId = Main.rand.NextBool() ? DustID.t_Lihzahrd : DustID.OrangeTorch;
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dustId, Projectile.velocity.X, Projectile.velocity.Y);
                Main.dust[d].noLightEmittence = true;
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float radius = 14;
            if (targetHitbox.ClosestPointInRect(Projectile.Center).Distance(Projectile.Center) <= radius)
                return true;
            return false;
        }
        public override bool? CanDamage() => Projectile.timeLeft > 60 ? null : false;
        public override Color? GetAlpha(Color lightColor)
        {
            return lightColor * MathHelper.Clamp(Projectile.timeLeft / 60f, 0, 1f);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            int time = maxTimeLeft - Projectile.timeLeft;
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            float opacity = MathHelper.Clamp(1f - (time / 90f), 0.5f, 1f);

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(Lighting.GetColor(Projectile.Center.ToTileCoordinates())), Projectile.rotation, tex.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            Main.EntitySpriteDraw(lightTex, Projectile.Center - Main.screenPosition, null, Color.White * opacity, Projectile.rotation, tex.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            return false;
        }
    }
}
