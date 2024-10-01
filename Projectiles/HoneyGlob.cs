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
using TerRoguelike.Particles;
using TerRoguelike.MainMenu;

namespace TerRoguelike.Projectiles
{
    public class HoneyGlob : ModProjectile, ILocalizedModType
    {
        public SpriteEffects spriteEffects = SpriteEffects.FlipVertically;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
        }
        public override void SetDefaults()
        {
            Projectile.width = 9;
            Projectile.height = 9;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.timeLeft = TerRoguelikeMenu.RuinedMoonActive ? 4000 : 800;
            Projectile.hide = true;
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCs.Add(index);
        }
        public override void AI()
        {
            Projectile.localAI[0]++;
            if (Projectile.frame != 3)
            {
                Projectile.frame = (int)Projectile.localAI[0] / 7 % 3;
                Projectile.rotation = Projectile.velocity.ToRotation();
                if (Projectile.velocity.Y < 8)
                    Projectile.velocity.Y += 0.085f;
                if (Projectile.velocity.Y > 8)
                    Projectile.velocity.Y = 8;

                Point bottomTilePos = Projectile.Bottom.ToTileCoordinates();
                Point extraBottomTimePos = (Projectile.Bottom + Vector2.UnitY * Projectile.velocity.Y).ToTileCoordinates();
                if (bottomTilePos.Y != extraBottomTimePos.Y && Projectile.velocity.Y > 1)
                {
                    Point futureTile = (Projectile.Bottom + Projectile.velocity).ToTileCoordinates();
                    Tile tile = TerRoguelikeUtils.ParanoidTileRetrieval(futureTile);
                    if (TileID.Sets.Platforms[tile.TileType] && tile.IsTileSolidGround() && tile.BlockType == BlockType.Solid)
                    {
                        if (Projectile.ai[0] > 0)
                        {
                            Projectile.ai[0]--;
                        }
                        else
                        {
                            Projectile.frame = 3;
                            Projectile.velocity = Vector2.Zero;
                            Projectile.rotation = 0f;
                            Projectile.Bottom = new Vector2(Projectile.Bottom.X, extraBottomTimePos.ToWorldCoordinates(0, 0).Y);
                            SoundEngine.PlaySound(SoundID.NPCDeath21 with { Volume = 0.4f }, Projectile.Center);
                        }
                    }
                }
            }
            var modProj = Projectile.ModProj();
            if (Projectile.timeLeft > 60)
            {
                if (Projectile.timeLeft % 10 == 0)
                {
                    float scaleMulti = Projectile.frame == 3 ? 1f : 0.5f;
                    Vector2 projSpawnPos = Projectile.Center + Main.rand.NextVector2Circular(4, 2) + Vector2.UnitY * 4;

                    Color outlineColor = Color.Goldenrod;
                    Color fillColor = Color.Lerp(outlineColor, Color.Black, 0.35f);
                    Vector2 particleVel = Main.rand.NextVector2Circular(1.5f, 0.66f);
                    ParticleManager.AddParticle(new BallOutlined(
                        projSpawnPos, particleVel,
                        60, outlineColor, fillColor, new Vector2(Main.rand.NextFloat(0.1f, 0.14f) * scaleMulti), 4, 0, 0.96f, 50));
                }
                if (Projectile.frame == 3 && modProj != null && modProj.npcOwner >= 0 && Main.npc[modProj.npcOwner].ModNPC().sourceRoomListID >= 0 && RoomSystem.RoomList[Main.npc[modProj.npcOwner].ModNPC().sourceRoomListID].bossDead)
                    Projectile.timeLeft = 60;
            }
            
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
            {
                Projectile.velocity.X = -oldVelocity.X * 0.5f;
            }

            if (oldVelocity.Y >= 0 * 5 && Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
            {
                Projectile.frame = 3;
                Projectile.velocity = Vector2.Zero;
                Projectile.rotation = 0f;
                Projectile.Bottom = new Vector2(Projectile.Bottom.X, (Projectile.Bottom + Vector2.UnitY * 16).ToTileCoordinates().ToWorldCoordinates(0, 0).Y);
                SoundEngine.PlaySound(SoundID.NPCDeath21 with { Volume = 0.4f }, Projectile.Center);
            }
            
            return false;
        }
        public override bool? CanDamage() => Projectile.timeLeft > 60 ? null : false;
        public override bool PreDraw(ref Color lightColor)
        {
            if (spriteEffects == SpriteEffects.FlipVertically)
            {
                spriteEffects = Main.rand.NextBool() ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            }
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Color color = Color.Yellow;
            float opacity = MathHelper.Clamp(MathHelper.Lerp(0f, 1f, Projectile.timeLeft / 60f), 0, 1f);

            if (Projectile.frame == 3)
                Main.EntitySpriteDraw(texture, drawPosition - Vector2.UnitY * 3, new Rectangle(0, frameHeight * Projectile.frame, texture.Width, frameHeight - 1), Color.DarkOrange * opacity, Projectile.rotation, new Vector2(texture.Width * 0.5f, (frameHeight * 0.5f)), 1f, Projectile.frame == 3 ? spriteEffects : SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, drawPosition - Vector2.UnitY, new Rectangle(0, frameHeight * Projectile.frame, texture.Width, frameHeight - 1), color * opacity, Projectile.rotation, new Vector2(texture.Width * 0.5f, (frameHeight * 0.5f)), 1f, Projectile.frame == 3 ? spriteEffects : SpriteEffects.None, 0);
            return false;
        }
    }
}
