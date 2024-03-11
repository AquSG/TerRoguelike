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
using Terraria.GameContent;
using static TerRoguelike.Managers.TextureManager;

namespace TerRoguelike.Projectiles
{
    public class SpikedBall : ModProjectile, ILocalizedModType
    {
        public Texture2D chainTex;
        public TerRoguelikeGlobalProjectile modProj;
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 10000;
            Projectile.penetrate = -1;
            modProj = Projectile.ModProj();
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.hide = true;
            chainTex = TexDict["SpikedBallChain"];
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCs.Add(index);
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Projectile.ai[0];
        }
        public override void AI()
        {
            if (modProj.npcOwner >= 0)
            {
                if (!Main.npc[modProj.npcOwner].active || Main.npc[modProj.npcOwner].life <= 0)
                {
                    Projectile.Kill();
                }
            }

            Projectile.ai[1]++;
            if (Projectile.ai[1] < 90)
            {
                Projectile.rotation += Projectile.velocity.Length() * 0.03f * Projectile.direction;
                Projectile.velocity.X *= 0.98f;
                Projectile.velocity.Y += 0.2f;
            }
            else
            {
                Projectile.rotation += Projectile.velocity.Length() * -0.03f * Projectile.direction;
                if (Projectile.ai[1] > 270 && Projectile.velocity.Length() < 0.2f)
                    Projectile.tileCollide = false;
                else if (Projectile.tileCollide)
                {
                    Projectile.velocity.Y += 0.2f;
                }

                Projectile.ignoreWater = true;
                if (modProj.npcOwner >= 0)
                {
                    Vector2 targetPos = Main.npc[modProj.npcOwner].Center;
                    if (Projectile.velocity.Length() > 2f)
                        Projectile.velocity *= 0.98f;
                    else
                        Projectile.velocity += (targetPos - Projectile.Center).SafeNormalize(Vector2.UnitX) * 0.6f;

                    if ((targetPos - Projectile.Center).Length() <= 12f)
                    {
                        Projectile.Kill();
                        SoundEngine.PlaySound(new SoundStyle("TerRoguelike/Sounds/chainHit") with { Volume = 1f }, Projectile.Center);
                    }
                }
                else
                {
                    if (Projectile.timeLeft > 30)
                        Projectile.timeLeft = 30;
                }
            }

            if (Projectile.ai[1] == 90)
            {
                SoundEngine.PlaySound(new SoundStyle("TerRoguelike/Sounds/chainLoop") with { Volume = 1f }, Projectile.Center);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (modProj.npcOwner < 0)
                return true;

            NPC owner = Main.npc[modProj.npcOwner];

            Vector2 chainVect = Projectile.Center - owner.Center;
            int chainLength = (int)(chainVect.Length() / 10);
            float posOffset = chainVect.Length() % 10f;
            float rot = chainVect.ToRotation();
            for (int i = 0; i < chainLength; i++)
            {
                Vector2 pos = (Vector2.UnitX * 10 * i + (Vector2.UnitX * posOffset)).RotatedBy(rot) + owner.Center;
                Color color = Lighting.GetColor(new Point((int)(pos.X / 16), (int)(pos.Y / 16)));
                Main.spriteBatch.Draw(chainTex, pos - Main.screenPosition, null, color, rot + MathHelper.PiOver2, new Vector2(chainTex.Size().X * 0.5f, 0), 1f, SpriteEffects.None, 0);
            }

            Main.spriteBatch.Draw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, null, Lighting.GetColor(new Point((int)(Projectile.Center.X / 16), (int)(Projectile.Center.Y / 16))), Projectile.rotation, TextureAssets.Projectile[Type].Value.Size() * 0.5f, 1f, SpriteEffects.None, 0);
            return false;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.ai[1] < 90)
            {
                // If the projectile hits the left or right side of the tile, reverse the X velocity
                if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
                {
                    Projectile.velocity.X = -oldVelocity.X * 0.5f;
                }
                // If the projectile hits the top or bottom side of the tile, reverse the Y velocity
                if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
                {
                    Projectile.velocity.Y = -oldVelocity.Y * 0.1f;
                    Projectile.velocity.X *= 0.985f;
                }
            }

            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.ai[0] >= 1)
                return;

            Projectile.ai[0] = 1;
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (Projectile.ai[0] >= 1)
                return;

            Projectile.ai[0] = 1;
        }
    }
}
