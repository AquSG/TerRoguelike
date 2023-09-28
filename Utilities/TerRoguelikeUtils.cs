using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerRoguelike.Utilities
{
    public static partial class TerRoguelikeUtils
    {
        /// <summary>
        /// Properly sets the player's held item rotation and position by doing the annoying math for you, since vanilla decided to be wholly inconsistent about it!
        /// This all assumes the player is facing right. All the flip stuff is automatically handled in here
        /// </summary>
        /// <param name="player">The player for which we set the hold style</param>
        /// <param name="desiredRotation">The desired rotation of the item</param>
        /// <param name="desiredPosition">The desired position of the item</param>
        /// <param name="spriteSize">The size of the item sprite (used in calculations)</param>
        /// <param name="rotationOriginFromCenter">The offset from the center of the sprite of the rotation origin</param>
        /// <param name="noSandstorm">Should the swirly effect from the sandstorm jump be disabled</param>
        /// <param name="flipAngle">Should the angle get flipped with the player, or should it be rotated by 180 degrees</param>
        /// <param name="stepDisplace">Should the item get displaced with the player's height during the walk anim? </param>
        public static void CleanHoldStyle(Terraria.Player player, float desiredRotation, Vector2 desiredPosition, Vector2 spriteSize, Vector2? rotationOriginFromCenter = null, bool noSandstorm = false, bool flipAngle = false, bool stepDisplace = true)
        {
            if (noSandstorm)
                player.sandStorm = false;

            //Since Vector2.Zero isn't a compile-time constant, we can't use it directly as the default parameter
            if (rotationOriginFromCenter == null)
                rotationOriginFromCenter = Vector2.Zero;

            Vector2 origin = rotationOriginFromCenter.Value;
            //Flip the origin's X position, since the sprite will be flipped if the player faces left.
            origin.X *= player.direction;
            //Additionally, flip the origin's Y position in case the player is in reverse gravity.
            origin.Y *= player.gravDir;

            player.itemRotation = desiredRotation;

            if (flipAngle)
                player.itemRotation *= player.direction;
            else if (player.direction < 0)
                player.itemRotation += MathHelper.Pi;

            //This can anchors the item to rotate around the middle left of its sprite
            //Vector2 consistentLeftAnchor = (player.itemRotation).ToRotationVector2() * -10f * player.direction;

            //This anchors the item to rotate around the center of its sprite.
            Vector2 consistentCenterAnchor = player.itemRotation.ToRotationVector2() * (spriteSize.X / -2f - 10f) * player.direction;

            //This shifts the item so it rotates around the set origin instead
            Vector2 consistentAnchor = consistentCenterAnchor - origin.RotatedBy(player.itemRotation);

            //The sprite needs to be offset by half its sprite size.
            Vector2 offsetAgain = spriteSize * -0.5f;

            Vector2 finalPosition = desiredPosition + offsetAgain + consistentAnchor;

            //Account for the players extra height when stepping
            if (stepDisplace)
            {
                int frame = player.bodyFrame.Y / player.bodyFrame.Height;
                if ((frame > 6 && frame < 10) || (frame > 13 && frame < 17))
                {
                    finalPosition -= Vector2.UnitY * 2f;
                }
            }

            player.itemLocation = finalPosition;
        }

        /// <summary>
        /// Determines if a tile is solid ground based on whether it's active and not actuated or if the tile is solid in any way, including just the top.
        /// </summary>
        /// <param name="tile">The tile to check.</param>
        public static bool IsTileSolidGround(this Tile tile) => tile != null && tile.HasUnactuatedTile && (Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType]);

        public static Tile ParanoidTileRetrieval(int x, int y)
        {
            if (!WorldGen.InWorld(x, y))
                return new Tile();

            return Main.tile[x, y];
        }
    }
}
