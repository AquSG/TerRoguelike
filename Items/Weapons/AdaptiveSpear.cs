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
using Terraria.GameContent;

namespace TerRoguelike.Items.Weapons
{
    public class AdaptiveSpear : ModItem, ILocalizedModType
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
            Item.shoot = ModContent.ProjectileType<AdaptiveSpearHoldout>();
            Item.shootSpeed = 16f;
        }

        public override bool CanUseItem(Player player)
        {
            return !Main.projectile.Any(n => n.active && n.owner == player.whoAmI && n.type == ModContent.ProjectileType<AdaptiveSpearHoldout>());
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
            float armPointingDirection = modPlayer.playerToCursor.ToRotation() + (MathHelper.PiOver2 * player.direction);
            float itemPotentialRot = modPlayer.playerToCursor.ToRotation();
            Vector2 itemPotentialPos = player.MountedCenter + armPointingDirection.ToRotationVector2() * 8;
            

            if (modPlayer.swingAnimCompletion > 0)
            {
                modPlayer.swingAnimCompletion += 1f / (20f / player.GetAttackSpeed(DamageClass.Generic));
                if (modPlayer.swingAnimCompletion > 1f)
                    modPlayer.swingAnimCompletion = 1f;
                //armPointingDirection += MathHelper.Lerp(0f, MathHelper.TwoPi * 9f / 16f, modPlayer.swingAnimCompletion) * player.direction;
                float armShoveForwardInterpolant = 0;
                if (modPlayer.swingAnimCompletion < 0.5f)
                {
                    armShoveForwardInterpolant = Math.Min(MathHelper.Lerp(0, 1, modPlayer.swingAnimCompletion * 4), 1);
                }
                else
                {
                    armShoveForwardInterpolant = MathHelper.Lerp(1, 0, (modPlayer.swingAnimCompletion - 0.5f) * 2);
                }
                itemPotentialPos += itemPotentialRot.ToRotationVector2() * MathHelper.Lerp(0, 40, armShoveForwardInterpolant);
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

            Vector2 handWantedPos = itemPotentialPos - itemPotentialRot.ToRotationVector2() * 14;
            Vector2 frontArmAnchor = player.GetFrontHandPosition(Player.CompositeArmStretchAmount.None, 0);
            Vector2 backArmAnchor = player.GetBackHandPosition(Player.CompositeArmStretchAmount.None, 0);
            float frontArmWantedRot = (handWantedPos - frontArmAnchor).ToRotation();
            float backArmWantedRot = (handWantedPos - backArmAnchor).ToRotation() + MathHelper.Pi * 0.15f * player.direction;
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, frontArmWantedRot - MathHelper.PiOver2);
            player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, backArmWantedRot - MathHelper.PiOver2);
            CleanHoldStyle(player, itemPotentialRot, itemPotentialPos, new Vector2(100, 30), new Vector2(0, 0));
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
