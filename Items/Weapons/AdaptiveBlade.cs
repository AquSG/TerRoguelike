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
using TerRoguelike.Utilities;
using static Humanizer.In;

namespace TerRoguelike.Items.Weapons
{
    public class AdaptiveBlade : ModItem, ILocalizedModType
    {
        public override void SetDefaults()
        {
            Item.damage = 100;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 62;
            Item.height = 32;
            Item.useTime = 26;
            Item.useAnimation = 26;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.channel = true;
            Item.knockBack = 5f;
            Item.rare = ItemRarityID.Cyan;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<AdaptiveBladeHoldout>();
            Item.shootSpeed = 16f;
        }

        public override bool CanUseItem(Player player)
        {
            return !Main.projectile.Any(n => n.active && n.owner == player.whoAmI && n.type == ModContent.ProjectileType<AdaptiveBladeHoldout>());
        }

        public override void UseItemFrame(Player player)
        {
            TerRoguelikePlayer modPlayer = player.GetModPlayer<TerRoguelikePlayer>();
            //Calculate the dirction in which the players arms should be pointing at.
            if (modPlayer.swingAnimCompletion <= 0 || modPlayer.playerToCursor == Vector2.Zero)
                modPlayer.playerToCursor = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            float armPointingDirection = (modPlayer.playerToCursor.ToRotation() - (MathHelper.Pi * player.direction / 3f));
            if (modPlayer.swingAnimCompletion > 0)
            {
                modPlayer.swingAnimCompletion += 1f / (20f / player.GetAttackSpeed(DamageClass.Generic));
                if (modPlayer.swingAnimCompletion > 1f)
                    modPlayer.swingAnimCompletion = 1f;
                armPointingDirection += MathHelper.Lerp(0f, MathHelper.TwoPi * 9f / 16f, modPlayer.swingAnimCompletion) * player.direction;
                if (modPlayer.swingAnimCompletion >= 1f)
                {
                    modPlayer.swingAnimCompletion = 0;
                    modPlayer.playerToCursor = Vector2.Zero;
                }
                    
            }
            if (player.itemTime == 1)
            {
                modPlayer.swingAnimCompletion = 0;
                modPlayer.playerToCursor = Vector2.Zero;
            }
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armPointingDirection - MathHelper.PiOver2);
            TerRoguelikeUtils.CleanHoldStyle(player, player.compositeFrontArm.rotation + MathHelper.PiOver2, player.GetFrontHandPosition(player.compositeFrontArm.stretch, player.compositeFrontArm.rotation).Floor(), new Vector2(38, 38), new Vector2(-14, 14));
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            TerRoguelikePlayer modPlayer = player.GetModPlayer<TerRoguelikePlayer>();

            if (Main.MouseWorld.X > player.Center.X && modPlayer.swingAnimCompletion <= 0)
            {
                player.ChangeDir(1);
            }
            else if (Main.MouseWorld.X <= player.Center.X && modPlayer.swingAnimCompletion <= 0)
            {
                player.ChangeDir(-1);
            }
            
        }
    }
}
