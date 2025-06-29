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
using TerRoguelike.Particles;
using static TerRoguelike.Managers.TextureManager;
using Steamworks;
using ReLogic.Utilities;

namespace TerRoguelike.NPCs.Enemy.Boss.Mallet.MalletProjectiles
{
    public class SpeedingFeather : ModProjectile, ILocalizedModType
    {
        public int maxTimeLeft;
        public override string Texture => "TerRoguelike/Projectiles/InvisibleProj";
        public Texture2D FeatherSize2;
        public int oldPosCut = 4;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 48;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = maxTimeLeft = 360;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.netImportant = true; //makes projectile not able to be replaced if at the projectile cap 
            FeatherSize2 = TexDict["FeatherSize2"];
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindProjectiles.Add(index);
            Projectile.hide = true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }
        public override void AI()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 6 * oldPosCut;
            Projectile.frameCounter++;
            Projectile.frameCounter %= oldPosCut;
            Projectile.rotation = Projectile.velocity.ToRotation();
        }
        public override bool PreDraw(ref Color lightColor)
        {
            var featherFrame = FeatherSize2.Frame(1, 4, 0, Projectile.frame);
            var origin = featherFrame.Size() * 0.5f;

            Vector2 halfwidthheight = new Vector2(Projectile.width * 0.5f, Projectile.height * 0.5f);
            for (int i = Projectile.oldPos.Length - 1; i > 0; i--)
            {
                if (i % oldPosCut != Projectile.frameCounter)
                    continue;

                float completion = (float)i / ProjectileID.Sets.TrailCacheLength[Type];
                float opacity = MathHelper.Lerp(0.5f, 0, completion);
                Main.EntitySpriteDraw(FeatherSize2, Projectile.oldPos[i] + halfwidthheight - Main.screenPosition, featherFrame, Color.White * opacity, Projectile.oldRot[i], origin, Projectile.scale, SpriteEffects.None);
            }
            Main.EntitySpriteDraw(FeatherSize2, Projectile.Center - Main.screenPosition, featherFrame, Color.White, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None);

            return false;
        }
        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            Mallet.InflictRetribution(target);
        }
    }
}
