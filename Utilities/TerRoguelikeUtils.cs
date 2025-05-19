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
using TerRoguelike.Projectiles;
using TerRoguelike.Items;
using Terraria.DataStructures;
using Terraria.UI;
using TerRoguelike.Systems;
using Terraria.ModLoader.Core;
using System.IO;
using Terraria.GameContent;
using System.Runtime.CompilerServices;

namespace TerRoguelike.Utilities
{
    public static partial class TerRoguelikeUtils
    {
        public static TerRoguelikePlayer ModPlayer(this Player player)
        {
            try
            {
                return (TerRoguelikePlayer)player.ModPlayers[ModContent.GetInstance<TerRoguelikePlayer>().Index] ?? null;
            }
            catch (Exception e)
            {
                TerRoguelike.Instance.Logger.Error(e);
                return null;
            }
        }
        public static TerRoguelikeGlobalProjectile ModProj(this Projectile projectile) => TerRoguelikeGlobalProjectile.TryGetGlobal(projectile.type, projectile.EntityGlobals, out TerRoguelikeGlobalProjectile result) ? result : null;
        public static TerRoguelikeGlobalNPC ModNPC(this NPC npc) => TerRoguelikeGlobalNPC.TryGetGlobal(npc.type, npc.EntityGlobals, out TerRoguelikeGlobalNPC result) ? result : null;
        public static TerRoguelikeGlobalItem ModItem(this Item item) => TerRoguelikeGlobalItem.TryGetGlobal(item.type, item.EntityGlobals, out TerRoguelikeGlobalItem result) ? result : null;

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

            player.itemLocation = finalPosition + new Vector2(spriteSize.X * 0.5f, 0);
        }

        /// <summary>
        /// Determines if a tile is solid ground based on whether it's active and not actuated or if the tile is solid in any way, including just the top.
        /// </summary>
        /// <param name="tile">The tile to check.</param>
        public static bool IsTileSolidGround(this Tile tile, bool IgnorePlatforms = false) => tile != null && tile.HasUnactuatedTile && (Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType]) && (!IgnorePlatforms || !Main.tileSolidTop[tile.TileType]); //function lifted from the Calamity Mod

        public static Tile ParanoidTileRetrieval(int x, int y)
        {
            //function lifted from the Calamity Mod
            if (!WorldGen.InWorld(x, y))
                return new Tile();

            return Main.tile[x, y];
        }
        public static Tile ParanoidTileRetrieval(Point coords)
        {
            return ParanoidTileRetrieval(coords.X, coords.Y);
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

        public static bool TopSlope(Tile tile)
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

                TerRoguelikeGlobalNPC modNPC = npc.ModNPC();

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

        public static int ClosestPlayer(Vector2 origin, float maxDistance)
        {
            int furthest = -1;
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (!player.active || player.dead)
                    continue;

                float distance = (origin - player.getRect().ClosestPointInRect(origin)).Length();
                if (distance < maxDistance)
                {
                    maxDistance = distance;
                    furthest = i;
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
        public static float AngleSizeBetween(float angle1, float angle2)
        {

            float rad1 = angle1 % MathHelper.TwoPi;
            float rad2 = angle2 % MathHelper.TwoPi;

            float angle = rad2 - rad1;

            if (Math.Abs(angle) > MathHelper.Pi)
                angle -= Math.Sign(angle) * MathHelper.TwoPi;

            return MathHelper.WrapAngle(angle);
        }
        public static bool? CircularHitboxCollision(Vector2 circleCenter, Rectangle targetHitbox, float radius)
        {
            if ((circleCenter - targetHitbox.ClosestPointInRect(circleCenter)).Length() <= radius)
                return null;

            return false;
        }
        public static Vector2 Abs(this Vector2 vector) => new(Math.Abs(vector.X), Math.Abs(vector.Y));
        /// <summary>
        /// Checks for tile collisions in a line from the start to end point
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="lengthCap">The max amount of units that will be checked before returning false</param>
        /// <returns>True if there is a tile in the way of the line</returns> 
        public static bool CanHitInLine(Vector2 start, Vector2 end, float lengthCap = 2000f)
        {

            float length = (start - end).Length();
            Vector2 unitVect = (end - start).SafeNormalize(Vector2.UnitY);

            if (length < 1f)
                return true;
            if (length > lengthCap)
                return false;

            Vector2 currentPos = start;
            Point lastAirPos = new Point(-1, -1);
            for (int i = 0; i < (int)length; i++)
            {
                currentPos += unitVect;

                Point tilePos = currentPos.ToTileCoordinates();

                if (tilePos == lastAirPos)
                    continue;

                if (!WorldGen.InWorld(tilePos.X, tilePos.Y))
                    continue;

                Tile tile = Main.tile[tilePos.X, tilePos.Y];
                if (!tile.IsTileSolidGround(true))
                {
                    lastAirPos = tilePos;
                    continue;
                }

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
        /// <summary>
        /// Checks for tile collisions in a line from the start to end point
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="lengthCap">The max amount of units that will be checked before returning the end vector</param>
        /// <returns>The position in the world where the line collided with a tile. Returns the end vector if nothing was in the way.</returns> 
        public static Vector2 TileCollidePositionInLine(Vector2 start, Vector2 end, float lengthCap = 2000f)
        {
            float length = (start - end).Length();
            Vector2 unitVect = (end - start).SafeNormalize(Vector2.UnitY);

            if (length < 1f)
            {
                Point endWorldPos = end.ToTileCoordinates();
                return ParanoidTileRetrieval(endWorldPos.X, endWorldPos.Y).IsTileSolidGround(true) ? start : end;
            }
            else if (length > lengthCap)
            {
                length = lengthCap;
            }

            Vector2 currentPos = start;
            Point lastAirPos = new Point(-1, -1);
            for (int i = 0; i < (int)length; i++)
            {
                currentPos += unitVect;

                Point tilePos = currentPos.ToTileCoordinates();

                if (tilePos == lastAirPos)
                    continue;

                if (!WorldGen.InWorld(tilePos.X, tilePos.Y))
                    continue;

                Tile tile = Main.tile[tilePos.X, tilePos.Y];
                if (!tile.IsTileSolidGround(true))
                {
                    lastAirPos = tilePos;
                    continue;
                }

                if (tile.Slope == SlopeType.Solid && !tile.IsHalfBlock)
                    return currentPos;

                Vector2 tileWorldPos = new Vector2(tilePos.X * 16, tilePos.Y * 16);
                Vector2 currentPosInTile = currentPos - tileWorldPos;
                if (tile.IsHalfBlock)
                {
                    if (currentPosInTile.Y >= 8f)
                        return currentPos;
                }
                else if (tile.Slope == SlopeType.SlopeDownLeft)
                {
                    if (currentPosInTile.X <= currentPosInTile.Y)
                        return currentPos;
                }
                else if (tile.Slope == SlopeType.SlopeDownRight)
                {
                    if ((16 - currentPosInTile.X) <= currentPosInTile.Y)
                        return currentPos;
                }
                else if (tile.Slope == SlopeType.SlopeUpLeft)
                {
                    if (currentPosInTile.X <= (16 - currentPosInTile.Y))
                        return currentPos;
                }
                else if (tile.Slope == SlopeType.SlopeUpRight)
                {
                    if (currentPosInTile.X >= currentPosInTile.Y)
                        return currentPos;
                }
            }

            return end;
        }
        /// <summary>
        /// Checks for tile collision at the position
        /// </summary>
        /// <returns>Whether or not a collision would happen at the position</returns> 
        public static bool TileCollisionAtThisPosition(Vector2 position)
        {
            Point tilePos = position.ToTileCoordinates();

            if (!WorldGen.InWorld(tilePos.X, tilePos.Y))
                return false;

            Tile tile = Main.tile[tilePos.X, tilePos.Y];
            if (!tile.IsTileSolidGround(true))
                return false;

            if (tile.Slope == SlopeType.Solid && !tile.IsHalfBlock)
                return true;

            Vector2 tileWorldPos = new Vector2(tilePos.X * 16, tilePos.Y * 16);
            Vector2 currentPosInTile = position - tileWorldPos;
            if (tile.IsHalfBlock)
            {
                if (currentPosInTile.Y >= 8f)
                    return true;
            }
            else if (tile.Slope == SlopeType.SlopeDownLeft)
            {
                if (currentPosInTile.X <= currentPosInTile.Y)
                    return true;
            }
            else if (tile.Slope == SlopeType.SlopeDownRight)
            {
                if ((16 - currentPosInTile.X) <= currentPosInTile.Y)
                    return true;
            }
            else if (tile.Slope == SlopeType.SlopeUpLeft)
            {
                if (currentPosInTile.X <= (16 - currentPosInTile.Y))
                    return true;
            }
            else if (tile.Slope == SlopeType.SlopeUpRight)
            {
                if (currentPosInTile.X >= currentPosInTile.Y)
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Finds the normal vector to the average colliding position on all the points in the box.
        /// </summary>
        /// <param name="topLeftPosition"></param>
        /// <param name="dimensions"></param>
        /// <param name="precision"></param>
        /// <returns>null if no collision, otherwise a normal vector. The normal vector can be 0.</returns>
        public static Vector2? CollidingVector(Vector2 topLeftPosition, Vector2 dimensions, int precision = 3)
        {
            bool collide = false;
            Vector2 baseCheckPos = topLeftPosition;
            Vector2 offsetPerLoop = new Vector2(dimensions.X * 0.5f, dimensions.X * 0.5f);
            Vector2 collidingVector = Vector2.Zero;
            int vectOffset = precision / 2;

            for (int x = 0; x < precision; x++)
            {
                for (int y = 0; y < precision; y++)
                {
                    Vector2 checkPos = baseCheckPos + offsetPerLoop * new Vector2(x, y);
                    if (TileCollisionAtThisPosition(checkPos))
                    {
                        collide = true;
                        collidingVector += new Vector2(-vectOffset + x, -vectOffset + y);
                    }
                }
            }
            if (!collide)
                return null;
            else
                return collidingVector.SafeNormalize(Vector2.Zero);
        }
        public static float FalseSunLightCollisionCheck(Vector2 start, float rot, float length, int step)
        {
            Vector2 unitVect = rot.ToRotationVector2();

            if (length < 1f)
            {
                Point endWorldPos = (start + unitVect * length).ToTileCoordinates();
                return ParanoidTileRetrieval(endWorldPos.X, endWorldPos.Y).IsTileSolidGround(true) ? 0 : length;
            }

            for (int r = 0; r < 4; r++)
            {
                float checkRot = r * MathHelper.PiOver2 - MathHelper.PiOver4;
                if (Math.Abs(AngleSizeBetween(rot, checkRot)) < 0.05f)
                {
                    step = 1;
                    break;
                }
            }

            Vector2 currentPos = start;
            Point lastAirPos = new Point(-1, -1);
            for (int i = 0; i < (int)length; i += step)
            {
                currentPos += unitVect * step;

                Point tilePos = currentPos.ToTileCoordinates();

                if (tilePos == lastAirPos)
                    continue;

                if (!WorldGen.InWorld(tilePos.X, tilePos.Y))
                    continue;

                Tile tile = Main.tile[tilePos.X, tilePos.Y];
                if (!tile.IsTileSolidGround(true))
                {
                    lastAirPos = tilePos;
                    continue;
                }

                if (tile.Slope == SlopeType.Solid && !tile.IsHalfBlock)
                {
                    if (EndFalseSunCheck())
                        return i;
                    else
                        continue;
                }

                Vector2 tileWorldPos = new Vector2(tilePos.X * 16, tilePos.Y * 16);
                Vector2 currentPosInTile = currentPos - tileWorldPos;
                if (tile.IsHalfBlock)
                {
                    if (currentPosInTile.Y >= 8f)
                    {
                        if (EndFalseSunCheck())
                            return i;
                        else
                            continue;
                    }
                }
                else if (tile.Slope == SlopeType.SlopeDownLeft)
                {
                    if (currentPosInTile.X <= currentPosInTile.Y)
                    {
                        if (EndFalseSunCheck())
                            return i;
                        else
                            continue;
                    }
                }
                else if (tile.Slope == SlopeType.SlopeDownRight)
                {
                    if ((16 - currentPosInTile.X) <= currentPosInTile.Y)
                    {
                        if (EndFalseSunCheck())
                            return i;
                        else
                            continue;
                    }
                }
                else if (tile.Slope == SlopeType.SlopeUpLeft)
                {
                    if (currentPosInTile.X <= (16 - currentPosInTile.Y))
                    {
                        if (EndFalseSunCheck())
                            return i;
                        else
                            continue;
                    }
                }
                else if (tile.Slope == SlopeType.SlopeUpRight)
                {
                    if (currentPosInTile.X >= currentPosInTile.Y)
                    {
                        if (EndFalseSunCheck())
                            return i;
                        else
                            continue;
                    }
                }

                bool EndFalseSunCheck()
                {
                    if (step != 1)
                    {
                        i -= step;
                        currentPos -= unitVect * step;
                        step = 1;
                        return false;
                    }
                    return true;
                }
            }

            return length;
        }
        public static void StartAdditiveSpritebatch(bool end = true)
        {
            if (end)
                Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

        }
        public static void StartAlphaBlendSpritebatch(bool end = true)
        {
            if (end)
                Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
        public static void StartNonPremultipliedSpritebatch(bool end = true)
        {
            if (end)
                Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
        public static void StartVanillaSpritebatch(bool end = true)
        {
            if (end)
                Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
        public static string GetAllTooltipLines(ItemTooltip tooltip)
        {
            string final = "";
            for (int i = 0; i < tooltip.Lines; i++)
            {
                final += tooltip.GetLine(i);
                if (i != tooltip.Lines - 1)
                    final += "\n";
            }
            return final;
        }
        public static void IterateEveryModsTypes<T>(bool includeBaseType = false, Action<Type> action = null)
        {
            if (action is null)
                return;

            Type baseType = typeof(T);
            var types = ModLoader.Mods.SelectMany(mod => AssemblyManager.GetLoadableTypes(mod.Code));
            foreach (var type in types)
            {
                if (type.IsSubclassOf(baseType) && !type.IsAbstract && (!includeBaseType && type != baseType))
                {
                    action.Invoke(type);
                }
            }
        }
        public static int NpcTexWidth(int Type)
        {
            if (Main.dedServ)
                return 1;

            return TextureAssets.Npc[Type].Width();
        }

        public static Point GetSection(Vector2 position)
        {
            var tilePos = position.ToTileCoordinates();
            return new Point(Netplay.GetSectionX(tilePos.X), Netplay.GetSectionY(tilePos.Y));
        }
        public static Vector2 MouseWorldAfterZoom => ((Main.MouseWorld - Main.Camera.Center) / ZoomSystem.zoomOverride) + Main.Camera.Center;

        public class RotatableRectangle
        {
            public Rectangle rect;
            public float rot;
            public Vector2 origin;
            public RotatableRectangle(Rectangle rect, float rot, Vector2 origin)
            {
                this.rect = rect;
                this.rot = rot;
                this.origin = origin;
            }
            public RotatableRectangle(RotatableRectangle rotrect)
            {
                rect = rotrect.rect;
                rot = rotrect.rot;
                origin = rotrect.origin;
            }
            public RotatableRectangle()
            {
                rect = new Rectangle();
                origin = Vector2.Zero;
            }
            public RotatableRectangle(Vector2 position, Vector2 dimensions, float rot, Vector2 origin)
            {
                float halfWidth = dimensions.X * 0.5f;
                float halfHeight = dimensions.Y * 0.5f;
                rect = new Rectangle((int)(position.X - halfWidth), (int)(position.Y - halfHeight), (int)dimensions.X, (int)dimensions.Y);
                this.rot = rot;
                this.origin = origin;
            }
        }
        /// <summary>
        /// Whether or not 2 rotated rectangles intersect with eachother. Origin is based on world position, so if you are rotating a rectangle based on a projectile, you'd probably want to use Projectile.Center as the origin. rect.Center() is effectively no origin.
        /// </summary>
        /// <param name="rectA"></param>
        /// <param name="rotA"></param>
        /// <param name="originA"></param>
        /// <param name="rectB"></param>
        /// <param name="rotB"></param>
        /// <param name="originB"></param>
        /// <returns>True if the rotated rectangles intersect. false if otherwise.</returns>
        public static bool RotatedRectanglesIntersect(this Rectangle rectA, float rotA, Vector2 originA, Rectangle rectB, float rotB, Vector2 originB)
        {
            Vector2[] cornersA = GetRotatedCorners(rectA, rotA, originA);
            Vector2[] cornersB = GetRotatedCorners(rectB, rotB, originB);

            return !PolygonsSeparatedCheck(cornersA, cornersB);
        }
        public static Vector2[] GetRotatedCorners(Rectangle rect, float rotation, Vector2 origin)
        {
            float cos = MathF.Cos(rotation);
            float sin = MathF.Sin(rotation);
            float halfWidth = rect.Width * 0.5f;
            float halfHeight = rect.Height * 0.5f;
            Vector2 center = rect.Center();
            Vector2 offset = center - origin;

            Vector2[] corners =
            [
                new Vector2(-halfWidth, -halfHeight) + offset,
                new Vector2(halfWidth, -halfHeight) + offset,
                new Vector2(halfWidth, halfHeight) + offset,
                new Vector2(-halfWidth, halfHeight) + offset,
            ];
            for (int i = 0; i < 4; i++)
            {
                float rotatedX = corners[i].X * cos - corners[i].Y * sin;
                float rotatedY = corners[i].X * sin + corners[i].Y * cos;
                corners[i] = center + new Vector2(rotatedX, rotatedY) - offset;
            }

            return corners;
        }
        /// <summary>
        /// Checks for intersection based on the separating axis theorem
        /// </summary>
        /// <param name="polygonA"></param>
        /// <param name="polygonB"></param>
        /// <returns>True if polygons are considered separated, false if they overlap</returns>
        public static bool PolygonsSeparatedCheck(Vector2[] polygonA, Vector2[] polygonB)
        {
            int totalEdges = polygonA.Length + polygonB.Length;

            for (int i = 0; i < totalEdges; i++)
            {
                Vector2[] polygon = i < polygonA.Length ? polygonA : polygonB;
                int index = i % polygon.Length;

                Vector2 edge = polygon[(index + 1) % polygon.Length] - polygon[index];
                Vector2 axis = Vector2.Normalize(new Vector2(-edge.Y, edge.X));

                (float minA, float maxA) = ProjectPolygon(polygonA, axis);
                (float minB, float maxB) = ProjectPolygon(polygonB, axis);

                if (maxA < minB || maxB < minA)
                    return true;
            }

            return false;
        }
        public static (float min, float max) ProjectPolygon(Vector2[] polygon, Vector2 axis)
        {
            float min = Vector2.Dot(polygon[0], axis);
            float max = min;

            for (int i = 1; i < polygon.Length; i++)
            {
                float projection = Vector2.Dot(polygon[i], axis);
                if (projection < min)
                    min = projection;
                else if (projection > max)
                    max = projection;
            }

            return (min, max);
        }
    }
}
