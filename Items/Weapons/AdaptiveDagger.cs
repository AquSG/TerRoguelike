using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerRoguelike.Projectiles;
using TerRoguelike.Managers;
using TerRoguelike.Systems;
using TerRoguelike.TerPlayer;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using static TerRoguelike.Utilities.TerRoguelikeUtils;

namespace TerRoguelike.Items.Weapons
{
    public class AdaptiveDagger : ModItem, ILocalizedModType
    {
        public override void SetDefaults()
        {
            Item.damage = 100;
            Item.DamageType = DamageClass.Melee;
            Item.width = 38;
            Item.height = 38;
            Item.useTime = 26;
            Item.useAnimation = 26;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.channel = true;
            Item.knockBack = 5f;
            Item.rare = ItemRarityID.Cyan;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<AdaptiveDaggerHoldout>();
            Item.shootSpeed = 16f;
        }

        public override bool CanUseItem(Player player)
        {
            return !Main.projectile.Any(n => n.active && n.owner == player.whoAmI && n.type == ModContent.ProjectileType<AdaptiveDaggerHoldout>());
        }

        public override void UseItemFrame(Player player)
        {
            TerRoguelikePlayer modPlayer = player.ModPlayer();

            if (AimWorld().X > player.Center.X && modPlayer.swingAnimCompletion <= 0.00001f)
            {
                player.ChangeDir(1);
            }
            else if (AimWorld().X <= player.Center.X && modPlayer.swingAnimCompletion <= 0.00001f)
            {
                player.ChangeDir(-1);
            }
            modPlayer.lockDirection = true;

            //Calculate the dirction in which the players arms should be pointing at.
            var stretchAmt = Player.CompositeArmStretchAmount.Full;
            if (modPlayer.swingAnimCompletion <= 0 || modPlayer.playerToCursor == Vector2.Zero)
                modPlayer.playerToCursor = (AimWorld() - player.Center).SafeNormalize(Vector2.UnitX);
            float armPointingDirection = modPlayer.playerToCursor.ToRotation();
            if (modPlayer.swingAnimCompletion > 0)
            {
                modPlayer.swingAnimCompletion += 1f / (12f / player.GetAttackSpeed(DamageClass.Generic));
                if (modPlayer.swingAnimCompletion > 1f)
                    modPlayer.swingAnimCompletion = 1f;
                if (modPlayer.swingAnimCompletion >= 1f)
                {
                    modPlayer.swingAnimCompletion = 0;
                    modPlayer.playerToCursor = Vector2.Zero;
                    modPlayer.lockDirection = false;
                }
                float animCheck = Math.Abs(modPlayer.swingAnimCompletion - 0.5f);
                if (animCheck < 0.15f)
                    stretchAmt = Player.CompositeArmStretchAmount.Full;
                else if (animCheck < 0.25f)
                    stretchAmt = Player.CompositeArmStretchAmount.ThreeQuarters;
                else if (animCheck < 0.35f)
                    stretchAmt = Player.CompositeArmStretchAmount.Quarter;
                else
                    stretchAmt = Player.CompositeArmStretchAmount.None;
            }
            if (player.itemTime == 1)
            {
                modPlayer.swingAnimCompletion = 0;
                modPlayer.playerToCursor = Vector2.Zero;
                modPlayer.lockDirection = false;
            }
            player.SetCompositeArmFront(true, stretchAmt, armPointingDirection - MathHelper.PiOver2);
            CleanHoldStyle(player, player.compositeFrontArm.rotation + MathHelper.PiOver2 + MathHelper.PiOver4 * player.direction, player.GetFrontHandPosition(player.compositeFrontArm.stretch, player.compositeFrontArm.rotation).Floor(), new Vector2(32, 32), new Vector2(-12, 12));

            
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            TerRoguelikePlayer modPlayer = player.ModPlayer();

            if (AimWorld().X > player.Center.X && modPlayer.swingAnimCompletion <= 0)
            {
                player.ChangeDir(1);
            }
            else if (AimWorld().X <= player.Center.X && modPlayer.swingAnimCompletion <= 0)
            {
                player.ChangeDir(-1);
            }
        }
    }
}
