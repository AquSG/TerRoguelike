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

namespace TerRoguelike.Items.Weapons
{
    public class AdaptiveRifle : ModItem, ILocalizedModType
    {
        public override void SetDefaults()
        {
            Item.damage = 250;
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
            Item.shoot = ModContent.ProjectileType<AdaptiveRifleHoldout>();
            Item.shootSpeed = 16f;
        }

        public override bool CanUseItem(Player player)
        {
            return !Main.projectile.Any(n => n.active && n.owner == player.whoAmI && n.type == ModContent.ProjectileType<AdaptiveRifleHoldout>());
        }

        public override void UseItemFrame(Player player)
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
            modPlayer.lockDirection = true;

            //Calculate the dirction in which the players arms should be pointing at.
            if (modPlayer.swingAnimCompletion <= 0 || modPlayer.playerToCursor == Vector2.Zero)
                modPlayer.playerToCursor = (AimWorld() - player.Center).SafeNormalize(Vector2.UnitX);
            float armPointingDirection = (modPlayer.playerToCursor.ToRotation());
            if (modPlayer.swingAnimCompletion > 0)
            {
                modPlayer.swingAnimCompletion += 1f / (20f / player.GetAttackSpeed(DamageClass.Generic));
                if (modPlayer.swingAnimCompletion > 1f)
                    modPlayer.swingAnimCompletion = 1f;
                if (modPlayer.swingAnimCompletion >= 1f)
                {
                    modPlayer.swingAnimCompletion = 0;
                    modPlayer.playerToCursor = Vector2.Zero;
                    modPlayer.lockDirection = false;
                }
                    
            }
            if (player.itemTime == 1)
            {
                modPlayer.swingAnimCompletion = 0;
                modPlayer.playerToCursor = Vector2.Zero;
                modPlayer.lockDirection = false;
            }

            float itemRot = armPointingDirection;
            Vector2 handWantedPos = player.MountedCenter + new Vector2(10 * player.direction, 5).RotatedBy(itemRot + (player.direction == -1 ? MathHelper.Pi : 0));
            Vector2 frontArmAnchor = modPlayer.GetPositionRelativeToFrontHand(0);
            float frontWantedRot = (handWantedPos - frontArmAnchor).ToRotation();
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, frontWantedRot - MathHelper.PiOver2);
            player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, armPointingDirection - MathHelper.PiOver2 + 0.3f * player.direction);
            CleanHoldStyle(player, itemRot, player.MountedCenter, new Vector2(62, 22), new Vector2(-14, -4));
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