using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using static TerRoguelike.Managers.TextureManager;

namespace TerRoguelike.Projectiles
{
    public class PhantasmalResidue : ModProjectile, ILocalizedModType
    {
        //almost everything in this is just visuals. the hitbox is active for 1/4 of a second after 30 frames pass, and is a big square
        public override string Texture => "TerRoguelike/Projectiles/InvisibleProj";
        public List<Vector2> rockPositions = new List<Vector2>();
        public List<float> rockRotations = new List<float>();
        public List<int> rockFrames = new List<int>();
        public List<int> rockDirections = new List<int>();
        public Texture2D rockTex;
        public int maxTimeLeft;
        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.timeLeft = maxTimeLeft = 540;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            rockTex = TexDict["RockDebris"];
            Projectile.hide = true;
            Projectile.ModProj().killOnRoomClear = true;
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCs.Add(index);
        }
        public override void OnSpawn(IEntitySource source)
        {
            float rotOff = Main.rand.NextFloat(MathHelper.TwoPi);

            for (int i = 0; i < 8; i++)
            {
                Vector2 center = Vector2.Zero;
                if (i != 0)
                    center += (i / 7f * MathHelper.TwoPi + rotOff).ToRotationVector2() * Projectile.width * 0.25f;

                center += Main.rand.NextVector2Circular(Projectile.width * 0.1f, Projectile.width * 0.1f);

                rockPositions.Add(center);
                rockRotations.Add(Main.rand.NextFloat(MathHelper.TwoPi));
                rockFrames.Add(Main.rand.Next(3));
                rockDirections.Add(Main.rand.NextBool() ? -1 : 1);
            }
        }
        public override void AI()
        {

        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CircularHitboxCollision(Projectile.Center, targetHitbox, Projectile.width * 0.4f);
        public override bool? CanDamage() => Projectile.timeLeft > 45 ? null : false;
        public override bool PreDraw(ref Color lightColor)
        {
            int frameHeight = rockTex.Height / 3;
            float scale = 1f;
            if (Projectile.timeLeft < 60)
            {
                scale *= Projectile.timeLeft / 60f;
            }
            Color color = Color.Lerp(Color.Teal, Color.Green, 0.34f);
            color.A = 50;
            for (int i = 0; i < rockPositions.Count; i++)
            {
                int frameCount = rockFrames[i];
                Vector2 pos = rockPositions[i];
                float rot = rockRotations[i];
                Rectangle frame = new Rectangle(0, frameHeight * frameCount, rockTex.Width, frameHeight - 2);
                int dir = rockDirections[i];
                float rotOff = Main.GlobalTimeWrappedHourly * 1f * dir;
                Main.EntitySpriteDraw(rockTex, pos + Projectile.Center - Main.screenPosition, frame, color, rot + rotOff, frame.Size() * 0.5f, scale, dir == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
            }

            return false;
        }
    }
}
