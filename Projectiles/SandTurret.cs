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
using Terraria.Graphics.Effects;
using Terraria.DataStructures;
using static TerRoguelike.Managers.TextureManager;
using static TerRoguelike.Systems.RoomSystem;

namespace TerRoguelike.Projectiles
{
    public class SandTurret : ModProjectile, ILocalizedModType
    {
        Entity target = null;
        public int maxTimeLeft;
        public float timeOffset;
        public Texture2D noiseTex;
        public Texture2D glowTex;
        public Texture2D crossGlowTex;
        public int direction = 1;
        public override void SetStaticDefaults()
        {

        }
        public override void SetDefaults()
        {
            Projectile.width = 54;
            Projectile.height = 54;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = maxTimeLeft = 300;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            noiseTex = TexDict["Crust"].Value;
            glowTex = TexDict["CircularGlow"].Value;
            crossGlowTex = TexDict["CrossSpark"].Value;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.damage = 1;
            Projectile.hostile = true;
            Projectile.friendly = false;
        }
        public override void AI()
        {
            int time = maxTimeLeft - Projectile.timeLeft;
            var modProj = Projectile.ModProj();
            target = modProj.GetTarget(Projectile);
            Projectile.rotation -= 0.05f;
            Projectile.frameCounter++;
            Projectile.velocity *= 0.97f;

            if (Projectile.localAI[0] > 0)
                Projectile.localAI[0]--;

            if (time <= 60)
            {
                if (modProj.npcOwner >= 0)
                {
                    NPC npc = Main.npc[modProj.npcOwner];
                    if (npc.active && npc.life > 0)
                    {
                        Projectile.Center = npc.Top + new Vector2(0, -48).RotatedBy(npc.rotation);
                    }
                }
                if ((time) == 60)
                {
                    Projectile.velocity = (target != null ? (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY) : Main.rand.NextVector2CircularEdge(0.5f, 0.5f));
                    Projectile.velocity *= target == null ? 16f : (target.Center - Projectile.Center).Length() * 0.025f;
                }
            }
            else
            {
                if (Projectile.timeLeft > 60)
                {
                    if (time >= 90)
                        {
                    {
                        if (Projectile.timeLeft % 8 == 0)
                            SoundEngine.PlaySound(SoundID.Item91 with { Volume = 0.2f, Pitch = 0.5f, PitchVariance = 0.08f }, Projectile.Center);
                            SoundEngine.PlaySound(SoundID.Dig with { Volume = 0.4f }, Projectile.Center);
                            Projectile.localAI[0] = 5;
                            Vector2 direction = target == null ? Vector2.UnitY : (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + direction * 24, direction * 6, ModContent.ProjectileType<Locust>(), Projectile.damage, 0);
                        }
                    }

                    if (modProj.npcOwner >= 0)
                    {
                        NPC npc = Main.npc[modProj.npcOwner];
                        var modNPC = npc.ModNPC();
                        if (modNPC.isRoomNPC)
                        {
                            if (RoomList[modNPC.sourceRoomListID].bossDead)
                                Projectile.timeLeft = 60;
                        }
                    }
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            int time = maxTimeLeft - Projectile.timeLeft;

            float scaleInterpolant = Projectile.timeLeft < 60 ? Projectile.timeLeft / 60f : (maxTimeLeft - Projectile.timeLeft) / 30f;
            scaleInterpolant = MathHelper.SmoothStep(0, 1, MathHelper.Clamp(scaleInterpolant, 0, 1));

            Texture2D tex = TextureAssets.Projectile[Type].Value;
            float scale = 0.974f + (float)Math.Cos(Projectile.frameCounter / 10f) * 0.026f;

            Main.spriteBatch.End();
            Effect maskEffect = Filters.Scene["TerRoguelike:MaskOverlay"].GetShader().Shader;
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, maskEffect, Main.GameViewMatrix.TransformationMatrix);

            Vector2 screenOff = new Vector2(-Projectile.rotation / MathHelper.TwoPi, Projectile.frameCounter / 60f);
            Color tint = Color.Goldenrod;
            if ((maxTimeLeft - Projectile.timeLeft) < 60)
                tint *= (maxTimeLeft - Projectile.timeLeft) / 60f;

            maskEffect.Parameters["screenOffset"].SetValue(screenOff);
            maskEffect.Parameters["stretch"].SetValue(new Vector2(0.5f));
            maskEffect.Parameters["replacementTexture"].SetValue(noiseTex);
            maskEffect.Parameters["tint"].SetValue(tint.ToVector4());

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, tex.Size() * 0.5f, Projectile.scale * 0.05f * scale * scaleInterpolant, SpriteEffects.None);

            TerRoguelikeUtils.StartAdditiveSpritebatch();
            Main.EntitySpriteDraw(glowTex, Projectile.Center - Main.screenPosition, null, tint, Projectile.rotation, glowTex.Size() * 0.5f, Projectile.scale * 0.3f * scale * scaleInterpolant, SpriteEffects.None);

            if (time > 45)
            {
                float crossOpacity = time < 120 ? (time - 45) / 45f : (Projectile.timeLeft - 20) / 40f;
                crossOpacity = MathHelper.Clamp(crossOpacity, 0, 1);
                float direction = target == null ? MathHelper.PiOver2 : (target.Center - Projectile.Center).ToRotation();
                Color crossColor = Color.Lerp(tint * 0.92f, Color.White, Projectile.localAI[0] / 5) * 0.97f;
                Main.EntitySpriteDraw(crossGlowTex, Projectile.Center - Main.screenPosition + direction.ToRotationVector2() * 38 * scale * scaleInterpolant * Projectile.scale, null, crossColor * crossOpacity, direction, new Vector2(0, crossGlowTex.Size().Y * 0.5f), Projectile.scale * 0.3f * scale * scaleInterpolant * new Vector2(1f, 2f), SpriteEffects.FlipHorizontally);
            }
            

            TerRoguelikeUtils.StartVanillaSpritebatch();
            return false;
        }
    }
}
