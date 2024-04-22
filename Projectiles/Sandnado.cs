using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TerRoguelike.NPCs.Enemy.Boss;
using TerRoguelike.Systems;
using TerRoguelike.Utilities;
using static TerRoguelike.Managers.TextureManager;

namespace TerRoguelike.Projectiles
{
    public class Sandnado : ModProjectile, ILocalizedModType
    {
        public Texture2D lightTex;
        public Texture2D glowTex;
        public int maxTimeLeft;
        public Vector2 parentPos = Vector2.Zero;
        public float rotationSpeed = 0.05f;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 1600;
        }
        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 800;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.timeLeft = maxTimeLeft = 600;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            lightTex = TexDict["SandnadoLight"].Value;
            glowTex = TexDict["CircularGlow"].Value;
            Projectile.hide = true;
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCs.Add(index);
        }
        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.Item60 with { Volume = 0.35f, MaxInstances = 10 }, Projectile.Center);
            Projectile.rotation += Main.rand.NextFloat(MathHelper.TwoPi);
            rotationSpeed = Main.rand.NextFloat(0.047f, 0.053f);

            if (source is EntitySource_Parent parentSource)
            {
                if (parentSource.Entity is NPC)
                {
                    if (Main.npc[parentSource.Entity.whoAmI].type == ModContent.NPCType<PharaohSpirit>())
                        parentPos = Main.npc[parentSource.Entity.whoAmI].Center + new Vector2(Main.npc[parentSource.Entity.whoAmI].direction * 28, -3);
                }
            }
        }
        public override void AI()
        {
            var modProj = Projectile.ModProj();
            if (maxTimeLeft - Projectile.timeLeft >= 120 && Projectile.timeLeft > 60 && modProj != null && modProj.npcOwner >= 0 && Main.npc[modProj.npcOwner].ModNPC().sourceRoomListID >= 0 && RoomSystem.RoomList[Main.npc[modProj.npcOwner].ModNPC().sourceRoomListID].bossDead)
                Projectile.timeLeft = 60;

            Projectile.rotation += rotationSpeed;
            if (maxTimeLeft - Projectile.timeLeft == 60)
            {
                SoundEngine.PlaySound(SoundID.Item82 with { Volume = 0.35f, MaxInstances = 10 }, Projectile.Center);
            }

            if (maxTimeLeft - Projectile.timeLeft < 60 && parentPos != Vector2.Zero)
            {
                float completion = (maxTimeLeft - Projectile.timeLeft) / 60f;
                Vector2 dustPos = parentPos + new Vector2((Projectile.Center.X - parentPos.X) * completion, ((Projectile.Center.Y - parentPos.Y) * completion) + (float)Math.Pow(Math.Abs(completion - 0.5f) * 2, 2) * 100 - 100);
                Dust d = Dust.NewDustPerfect(dustPos + Main.rand.NextVector2Circular(12, 12), DustID.Sandnado, Vector2.Zero, 0, default, 1f);
                d.noLight = true;
            }
            if (Projectile.timeLeft < 50)
                return;

            //This is taken from the vanilla sandnado dust code. Really liked the dust effect they had going but it's like, super specific. So I tried my best to make it suit the big sandnado.
            Vector2 anchor = Projectile.Center;
            Vector2 dimensions = new Vector2((int)(Projectile.width * 2.5f), (int)(Projectile.height * 1.1f));
            float randFloat = Main.rand.NextFloat();
            Vector2 dustOffset = new Vector2(MathHelper.Lerp(0.1f, 1f, Main.rand.NextFloat()), MathHelper.Lerp(-0.5f, 0.9f, randFloat));
            dustOffset.X *= MathHelper.Lerp(2.2f, 0.6f, randFloat);
            dustOffset.X *= -1f;
            Vector2 dustMagnet = new Vector2(2f, 10f);
            Vector2 dustSetPos = anchor + dimensions * dustOffset * 0.5f + dustMagnet;
            Dust dust = Main.dust[Dust.NewDust(dustSetPos, 0, 0, DustID.Sandnado)];
            dust.position = dustSetPos;
            dust.customData = anchor + dustMagnet;
            dust.fadeIn = 1f;
            dust.scale = 0.3f;
            dust.noLight = true;
            if (dustOffset.X > -1.2f)
            {
                dust.velocity.X = 1f + Main.rand.NextFloat();
            }
            dust.velocity.Y = Main.rand.NextFloat() * -0.5f - 3f;
        }
        public override bool? CanDamage() => (maxTimeLeft - Projectile.timeLeft > 90 && Projectile.timeLeft > 50) ? null : false;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;

            Color baseColor = Color.Lerp(Color.Yellow, Color.PaleGoldenrod, 0.6f);
            float fadeIn = 60;
            float lightFadeIn = 30;

            int time = maxTimeLeft - Projectile.timeLeft;
            float nadoOpacity = time < fadeIn * 2 ? (time - fadeIn) / fadeIn : Projectile.timeLeft / fadeIn;
            nadoOpacity = MathHelper.Clamp(nadoOpacity, 0, 1);
            float lightOpacity = time < lightFadeIn ? time / lightFadeIn : 1f - ((time - lightFadeIn - (25)) / (fadeIn * 2));
            lightOpacity = MathHelper.SmoothStep(0, 1, MathHelper.Clamp(lightOpacity, 0, 1));
            Color nadoColor = Color.Lerp(baseColor, Color.Orange, 0.2f) * 0.6f * nadoOpacity;

            Vector2 start = Projectile.Top;
            float maxYOff = Projectile.height;
            int dir = 1;
            for (int i = Projectile.height - 1; i >= 0; i -= 4)
            {
                dir *= -1;
                float inverseCompletion = ((float)i / Projectile.height);
                float completion = 1f - inverseCompletion;
                float extraFadeIn = inverseCompletion < 0.5f ? MathHelper.Clamp(inverseCompletion * 4, 0, 1) : MathHelper.Clamp(completion * 7, 0, 1);
                Vector2 pos = start + new Vector2(0, maxYOff * inverseCompletion);
                float rotation = (Projectile.rotation + ((i + (i * completion * 0.5f)) * 0.01f)) * dir;
                Vector2 scale = MathHelper.Lerp(1f, 3f, completion) * Vector2.One;

                Main.EntitySpriteDraw(tex, pos - Main.screenPosition, null, nadoColor * extraFadeIn, rotation, tex.Size() * 0.5f, Projectile.scale * scale, SpriteEffects.None);
            }
            TerRoguelikeUtils.StartAdditiveSpritebatch();
            Main.EntitySpriteDraw(glowTex, Projectile.Center - Main.screenPosition, null, baseColor * lightOpacity * 0.6f, 0, glowTex.Size() * 0.5f, Projectile.scale * new Vector2(1f, 5f) * 0.16f, SpriteEffects.None);
            TerRoguelikeUtils.StartVanillaSpritebatch();

            Main.EntitySpriteDraw(lightTex, Projectile.Center - Main.screenPosition, null, baseColor * lightOpacity * 0.85f, 0, lightTex.Size() * 0.5f, Projectile.scale * new Vector2(2f, 6f), SpriteEffects.None);
            return false;
        }
    }
}
