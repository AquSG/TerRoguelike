﻿using System;
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
using Terraria.GameInput;

namespace TerRoguelike.Items.Weapons
{
    public class AdaptiveSpaceGun : ModItem, ILocalizedModType
    {
        public override void SetDefaults()
        {
            Item.damage = 110;
            Item.DamageType = DamageClass.Ranged;
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
            Item.shoot = ModContent.ProjectileType<AdaptiveSpaceGunHoldout>();
            Item.shootSpeed = 16f;
        }

        public override bool CanUseItem(Player player)
        {
            return !Main.projectile.Any(n => n.active && n.owner == player.whoAmI && n.type == ModContent.ProjectileType<AdaptiveSpaceGunHoldout>());
        }

        public override void UseItemFrame(Player player)
        {
            var modPlayer = player.ModPlayer();
            //Calculate the dirction in which the players arms should be pointing at.
            Vector2 playerToCursor = (modPlayer.mouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            float armPointingDirection = (playerToCursor.ToRotation());

            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armPointingDirection - MathHelper.PiOver2);
            Vector2 anchor = player.GetFrontHandPosition(player.compositeFrontArm.stretch, player.compositeFrontArm.rotation).Floor();
            CleanHoldStyle(player, player.compositeFrontArm.rotation + MathHelper.PiOver2, anchor, new Vector2(36, 24), new Vector2(-12, -4));

            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armPointingDirection - MathHelper.PiOver2 + 0.3f * player.direction);
            //player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, (anchor - player.ModPlayer().GetPositionRelativeToFrontHand(0)).ToRotation() - MathHelper.PiOver2 + 0.2f * player.direction);

            if (modPlayer.mouseWorld.X > player.Center.X)
            {
                player.ChangeDir(1);
            }
            else
            {
                player.ChangeDir(-1);
            }
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            var modPlayer = player.ModPlayer();
            if (modPlayer.mouseWorld.X > player.Center.X)
            {
                player.ChangeDir(1);
            }
            else
            {
                player.ChangeDir(-1);
            }
        }
    }
}
