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
using rail;
using System.IO;

namespace TerRoguelike.Items.Weapons
{
    public class AdaptiveSaber : ModItem, ILocalizedModType
    {
        public override void SetDefaults()
        {
            Item.damage = 70;
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
            Item.shoot = ModContent.ProjectileType<AdaptiveSaberHoldout>();
            Item.shootSpeed = 16f;
        }

        public override bool CanUseItem(Player player)
        {
            return !Main.projectile.Any(n => n.active && n.owner == player.whoAmI && n.type == ModContent.ProjectileType<AdaptiveSaberHoldout>());
        }

        public override void UseItemFrame(Player player)
        {
            TerRoguelikePlayer modPlayer = player.ModPlayer();

            if (modPlayer.swingAnimCompletion == -0.00001f)
            {
                modPlayer.verticalSwingDirection = -1;
                modPlayer.swingAnimCompletion = 0.00001f;
            }
            else if (modPlayer.verticalSwingDirection == -1 && modPlayer.swingAnimCompletion == 0.00001f)
            {
                modPlayer.verticalSwingDirection = 1;
            }

            if (!modPlayer.changedDir)
            {
                if (modPlayer.mouseWorld.X > player.Center.X && modPlayer.swingAnimCompletion <= 0)
                {
                    player.ChangeDir(1);
                }
                else if (modPlayer.mouseWorld.X <= player.Center.X && modPlayer.swingAnimCompletion <= 0)
                {
                    player.ChangeDir(-1);
                }
            }
            modPlayer.lockDirection = true;


            //Calculate the dirction in which the players arms should be pointing at.
            if (modPlayer.swingAnimCompletion <= 0 || modPlayer.playerToCursor == Vector2.Zero)
                modPlayer.playerToCursor = (modPlayer.mouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            float armPointingDirection = (modPlayer.playerToCursor.ToRotation() - (MathHelper.Pi * player.direction / 3f));
            if (modPlayer.swingAnimCompletion > 0)
            {
                modPlayer.swingAnimCompletion += 1f / (20f / player.GetAttackSpeed(DamageClass.Generic));
                if (modPlayer.swingAnimCompletion > 1f)
                    modPlayer.swingAnimCompletion = 1f;
                float anim = modPlayer.swingAnimCompletion;
                anim = MathHelper.SmoothStep(0, 1, anim);
                armPointingDirection += MathHelper.Lerp(0f, MathHelper.TwoPi * 9f / 16f, modPlayer.verticalSwingDirection == 1 ? anim : 1 - anim) * player.direction;
                if (modPlayer.swingAnimCompletion >= 1f && !player.channel)
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
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armPointingDirection - MathHelper.PiOver2);
            //CleanHoldStyle(player, player.compositeFrontArm.rotation + MathHelper.PiOver2, player.GetFrontHandPosition(player.compositeFrontArm.stretch, player.compositeFrontArm.rotation).Floor(), new Vector2(56, 56), new Vector2(-14, 14));
            player.itemLocation = new Vector2(Main.maxTilesX * 16, Main.maxTilesY * 16); // begone;
        }
        public static float GetArmRotation(float baseRot, float anim, int upDownDir, int direction)
        {
            float armPointingDirection = baseRot - (MathHelper.Pi * direction / 3f);
            anim = MathHelper.SmoothStep(0, 1, anim);
            armPointingDirection += MathHelper.Lerp(0f, MathHelper.TwoPi * 9f / 16f, upDownDir == 1 ? anim : 1 - anim) * direction;
            return armPointingDirection - MathHelper.PiOver2;
        }
        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            TerRoguelikePlayer modPlayer = player.ModPlayer();
            if (modPlayer.changedDir)
                return;

            if (modPlayer.mouseWorld.X > player.Center.X && modPlayer.swingAnimCompletion <= 0)
            {
                player.ChangeDir(1);
            }
            else if (modPlayer.mouseWorld.X <= player.Center.X && modPlayer.swingAnimCompletion <= 0)
            {
                player.ChangeDir(-1);
            }
        }
    }
}
