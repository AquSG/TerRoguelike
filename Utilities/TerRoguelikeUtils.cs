using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerRoguelike.TerPlayer;
using TerRoguelike.NPCs;
using Terraria.GameInput;

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
        public static void CleanHoldStyle(Player player, float desiredRotation, Vector2 desiredPosition, Vector2 spriteSize, Vector2? rotationOriginFromCenter = null, bool noSandstorm = false, bool flipAngle = false, bool stepDisplace = true)
        {
            //function lifted from the Calamity Mod
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
        public static bool IsTileSolidGround(this Tile tile, bool IgnorePlatforms = false) => tile != null && tile.HasUnactuatedTile && (Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType]) && (!IgnorePlatforms || !TileID.Sets.Platforms[tile.TileType]); //function lifted from the Calamity Mod

        public static Tile ParanoidTileRetrieval(int x, int y)
        {
            //function lifted from the Calamity Mod
            if (!WorldGen.InWorld(x, y))
                return new Tile();

            return Main.tile[x, y];
        }
        /// <summary>
        /// Performs collision based a rotating hitbox for an entity by treating the hitbox as a line. By default uses the velocity of the entity as a direction. This can be overriden.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="targetTopLeft">The top left coordinates of the target to check.</param>
        /// <param name="targetHitboxDimensions">The hitbox size of the target to check.</param>
        /// <param name="directionOverride">An optional direction override</param>
        public static bool RotatingHitboxCollision(this Entity entity, Vector2 targetTopLeft, Vector2 targetHitboxDimensions, Vector2? directionOverride = null, float scale = 1f, float backCutoff = 1f)
        {
            //function lifted from the Calamity Mod. return statement edited due to an oversight where if the entity hitbox was 100% encased within the target hitbox, it would only hit if the outlines barely overlapped.

            Vector2 lineDirection = directionOverride ?? entity.velocity;

            // Ensure that the line direction is a unit vector.
            lineDirection = lineDirection.SafeNormalize(Vector2.UnitY);
            Vector2 start = entity.Center - lineDirection * entity.height * 0.5f * scale * backCutoff;
            Vector2 end = entity.Center + lineDirection * entity.height * 0.5f * scale;

            float _ = 0f;

            return Collision.CheckAABBvLineCollision(targetTopLeft, targetHitboxDimensions, start, end, entity.width * scale, ref _) || new Rectangle((int)targetTopLeft.X, (int)targetTopLeft.Y, (int)targetHitboxDimensions.X, (int)targetHitboxDimensions.Y).Contains(entity.Center.ToPoint());
        }
        /// <summary>
        /// Returns whether a proc should occur or not with the given chance.
        /// </summary>
        /// <param name="chance">The chance of the proc occurring, 0f is 0%, 1f is 100%</param>
        /// <param name="procLuck">The amount of times the chance is checked again upon failure</param>
        public static bool ChanceRollWithLuck(float chance, int procLuck)
        {
            for (int i = 0; i < procLuck + 1; i++)
            {
                if (Main.rand.NextFloat(1f + float.Epsilon) < chance)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Returns a color lerp that supports multiple colors.
        /// </summary>
        /// <param name="increment">The 0-1 incremental value used when interpolating.</param>
        /// <param name="colors">The various colors to interpolate across.</param>
        /// <returns></returns>
        public static Color MulticolorLerp(float increment, params Color[] colors)
        {
            //function lifted from the Calamity Mod

            increment %= 0.999f;
            int currentColorIndex = (int)(increment * colors.Length);
            Color currentColor = colors[currentColorIndex];
            Color nextColor = colors[(currentColorIndex + 1) % colors.Length];
            return Color.Lerp(currentColor, nextColor, increment * colors.Length % 1f);
        }

        public static bool TopSlope (Tile tile)
        {
            byte b = (byte)tile.Slope;
            if (b != 1)
            {
                return b == 2;
            }
            return true;
        }
        /// <summary>
        /// Returns the closest npc with the given conditions.
        /// chaseFriendly == null: neutral. both friendly and hostile can pass
        /// chaseFriendly == true: only passes if friendly
        /// chaseFriendly == false: only passes if not friendly
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="maxDistance"></param>
        /// <returns>The index of the npc. -1 if not found</returns>
        public static int ClosestNPC(Vector2 origin, float maxDistance, bool? chaseFriendly)
        {
            int furthest = -1;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active)
                    continue;

                TerRoguelikeGlobalNPC modNPC = npc.GetGlobalNPC<TerRoguelikeGlobalNPC>();
                
                if (modNPC.CanBeChased(false, chaseFriendly))
                {
                    float distance = (origin - npc.getRect().ClosestPointInRect(origin)).Length();
                    if (distance < maxDistance)
                    {
                        maxDistance = distance;
                        furthest = i;
                    }
                }
            }
            return furthest;
        }

        /// <summary>
        /// Basically Main.mouseWorld but takes into account if you are on controller and locked on to an NPC
        /// </summary>
        /// <returns></returns>
        public static Vector2 AimWorld()
        {
            return LockOnHelper.Enabled ? LockOnHelper.AimedTarget.Center : Main.MouseWorld;
        }
        /// <summary>
        /// Measures the radians between 2 values. angle1 is the primary angle, for directional purposes.
        /// </summary>
        /// <returns>The radians present between the 2 angles, positive or negative based on the closest direction</returns>
        public static float RadianSizeBetween(float angle1, float angle2)
        {
            float rad1 = angle1 % MathHelper.TwoPi;
            float rad2 = angle2 % MathHelper.TwoPi;

            float angle = rad2 - rad1;

            if (Math.Abs(angle) > MathHelper.Pi)
                angle = (Math.Sign(angle) * MathHelper.TwoPi) - angle;

            return angle;
        }
        public static bool? CircularHitboxCollision(Vector2 circleCenter, Rectangle targetHitbox, float radius)
        {
            if ((circleCenter - targetHitbox.ClosestPointInRect(circleCenter)).Length() <= radius)
                return null;

            return false;
        }
        public static bool CanHitInLine(Vector2 start, Vector2 end, float lengthCap = 2000f)
        {
            float length = (start - end).Length();
            Vector2 unitVect = (end - start).SafeNormalize(Vector2.UnitY);

            if (length < 1f)
                return true;
            if (length > lengthCap)
                return false;

            Vector2 currentPos = start;
            for (int i = 0; i < (int)length; i++)
            {
                currentPos += unitVect;

                Point tilePos = new Point((int)(currentPos.X / 16), (int)(currentPos.Y / 16));

                if (!WorldGen.InWorld(tilePos.X, tilePos.Y))
                    continue;

                Tile tile = Main.tile[tilePos.X, tilePos.Y];
                if (!tile.IsTileSolidGround(true))
                    continue;

                if (tile.Slope == SlopeType.Solid && !tile.IsHalfBlock)
                    return false;

                Vector2 tileWorldPos = new Vector2(tilePos.X * 16, tilePos.Y * 16);
                Vector2 currentPosInTile = currentPos - tileWorldPos;
                if (tile.IsHalfBlock)
                {
                    if (currentPosInTile.Y >= 8f)
                        return false;
                }
                else if (tile.Slope == SlopeType.SlopeDownLeft)
                {
                    if (currentPosInTile.X <= currentPosInTile.Y)
                        return false;
                }
                else if (tile.Slope == SlopeType.SlopeDownRight)
                {
                    if ((16 - currentPosInTile.X) <= currentPosInTile.Y)
                        return false;
                }
                else if (tile.Slope == SlopeType.SlopeUpLeft)
                {
                    if (currentPosInTile.X <= (16 - currentPosInTile.Y))
                        return false;
                }
                else if (tile.Slope == SlopeType.SlopeUpRight)
                {
                    if (currentPosInTile.X >= currentPosInTile.Y)
                        return false;
                }
            }

            return true;
        }
    }
}
