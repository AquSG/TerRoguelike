using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TerRoguelike.Managers;
using TerRoguelike.Systems;
using TerRoguelike.TerPlayer;
using Microsoft.Xna.Framework.Graphics;
using static TerRoguelike.Managers.TextureManager;

namespace TerRoguelike.Projectiles
{
    public class MagicFist : ModProjectile, ILocalizedModType
    {
        public int direction;
        public ref float targetIndex => ref Projectile.ai[0];
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
        }
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.timeLeft = 36;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            direction = Main.rand.NextBool() ? -1 : 1;
        }

        public override void AI()
        {
            if (targetIndex != -1)
            {
                if (!Main.npc[(int)targetIndex].active || Main.npc[(int)targetIndex].life <= 0) //if target isn't going to work anymore, give up and punch early
                    targetIndex = -1;
            }
            if (Projectile.timeLeft > 6 && targetIndex != -1)
            {
                Projectile.Center = Main.npc[(int)targetIndex].Center + new Vector2(0, -96f); // float above npc before punching
            }
            else
            {
                if (Projectile.timeLeft > 6) //premature target deaths make the fist give up and punch early, doing nothing
                    Projectile.timeLeft = 6;

                if (targetIndex != -1)
                    Projectile.Center = Main.npc[(int)targetIndex].Center + new Vector2(0, (-16f * (Projectile.timeLeft - 1)) + Main.npc[(int)targetIndex].gfxOffY);
                else
                    Projectile.Center += new Vector2(0, 16f); //punch down
            }

            //fist curling anim
            if (Projectile.frame < Main.projFrames[Projectile.type] - 1)
                Projectile.frame = Projectile.frameCounter / 5;
            if (Projectile.frame >= Main.projFrames[Projectile.type])
                Projectile.frame = Main.projFrames[Projectile.type] - 1;

            Projectile.frameCounter++;
        }
        public override bool? CanDamage() => Projectile.timeLeft <= 6 ? (bool?)null : false; // punch starts at timeleft = 6
        public override bool? CanHitNPC(NPC target)
        {
            if (target.whoAmI != (int)targetIndex)
                return false;
            else if (Projectile.timeLeft <= 6) // can only hit the 1 designated target passed into ai[0], in the last 6 frames of lifetime
                return null;

            return false;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 20; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center + new Vector2(-20, -36), 40, 50, DustID.BlueTorch);
                dust.noLight = true;
                dust.noLightEmittence = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Texture2D glowTex = TexDict["LenaGlow"].Value;
            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            SpriteEffects spriteEffects = direction == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;
            float opacity = MathHelper.Clamp(MathHelper.Lerp(1f, 0f, (Projectile.timeLeft - 6) / 30f), 0f, 1f); // fade in
            Main.EntitySpriteDraw(glowTex, Projectile.Center - Main.screenPosition, null, Color.DarkCyan * 0.3f * opacity, MathHelper.PiOver2, glowTex.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, new Rectangle(0, frameHeight * Projectile.frame, texture.Width, frameHeight), Color.Cyan * opacity, MathHelper.PiOver2, new Vector2(texture.Width * 0.5f, (frameHeight * 0.5f)), Projectile.scale, spriteEffects);
            return false;
        }
    }
}
