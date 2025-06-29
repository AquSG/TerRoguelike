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

namespace TerRoguelike.NPCs.Enemy.Boss.Mallet.MalletProjectiles
{
    public class SplitFeather : ModProjectile, ILocalizedModType
    {
        public int maxTimeLeft;
        public override string Texture => "TerRoguelike/Projectiles/InvisibleProj";
        public Texture2D FeatherSize3;
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = maxTimeLeft = 40;
            Projectile.penetrate = -1;
            Projectile.MaxUpdates = 2;
            Projectile.tileCollide = false;
            Projectile.netImportant = true; //makes projectile not able to be replaced if at the projectile cap 
            FeatherSize3 = TexDict["FeatherSize3"];
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }
        public override bool PreDraw(ref Color lightColor)
        {
            float opacity = Projectile.timeLeft / (float)maxTimeLeft;
            opacity = Math.Min(opacity * 2, 1);

            var featherFrame = FeatherSize3.Frame(1, 4, 0, Projectile.frame);
            Main.EntitySpriteDraw(FeatherSize3, Projectile.Center - Main.screenPosition, featherFrame, Color.White * opacity, Projectile.rotation, featherFrame.Size() * 0.5f, Projectile.scale, SpriteEffects.None);

            return false;
        }
        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            Mallet.InflictRetribution(target);
        }
        public override bool? CanDamage() => Projectile.timeLeft > 8 ? null : false;
    }
}
