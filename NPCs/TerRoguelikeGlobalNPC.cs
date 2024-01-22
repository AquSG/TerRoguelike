using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using TerRoguelike;
using TerRoguelike.World;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.Graphics;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using TerRoguelike.TerPlayer;
using TerRoguelike.Managers;
using System.IO;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using Terraria.DataStructures;
using TerRoguelike.Projectiles;
using static TerRoguelike.Systems.RoomSystem;
using System.Threading.Tasks.Dataflow;

namespace TerRoguelike.NPCs
{
    public class TerRoguelikeGlobalNPC : GlobalNPC
    {
        #region Variables
        public bool isRoomNPC = false;
        public int sourceRoomListID = -1;
        public bool hostileTurnedAlly = false;
        public bool IgnoreRoomWallCollision = false;

        //On kill bools to not let an npc somehow proc it more than once on death.
        public bool activatedHotPepper = false;
        public bool activatedSoulstealCoating = false;
        public bool activatedAmberBead = false;
        public bool activatedThrillOfTheHunt = false;
        public bool activatedClusterBombSatchel = false;
        public bool activatedSteamEngine = false;
        public bool activatedNutritiousSlime = false;
        public bool activatedItemPotentiometer = false;

        //debuffs
        public List<IgnitedStack> ignitedStacks = new List<IgnitedStack>();
        public int ignitedHitCooldown = 0;
        public List<BleedingStack> bleedingStacks = new List<BleedingStack>();
        public int bleedingHitCooldown = 0;
        public int ballAndChainSlow = 0;
        public Vector2 drawCenter = new Vector2(-1000);
        public int whoAmI;
        public int targetPlayer = -1;
        public int targetNPC = -1;
        public int friendlyFireHitCooldown = 0;
        public bool OverrideIgniteVisual = false;
        #endregion

        #region Base AIs
        public void RogueFighterAI(NPC npc, float xCap, float jumpVelocity, float acceleration = 0.07f)
        {
            Entity target = GetTarget(npc, false, false);

            if (target == null && npc.direction == 0)
            {
                npc.direction = 1;
                npc.spriteDirection = 1;
            }
            if (npc.ai[0] == 0 && target != null)
            {
                if (npc.Center.X < target.Center.X)
                {
                    npc.direction = 1;
                    npc.spriteDirection = 1;
                }
                else
                {
                    npc.direction = -1;
                    npc.spriteDirection = -1;
                }
            }
            else if (npc.ai[0] > 60)
            {
                npc.ai[0] = -240;
                npc.direction *= -1;
                npc.spriteDirection *= -1;
            }
            if (npc.ai[0] < 0)
                npc.ai[0]++;

            if (npc.velocity.X < -xCap || npc.velocity.X > xCap)
            {
                if (npc.velocity.Y == 0f)
                    npc.velocity *= 0.8f;
            }
            else if (npc.velocity.X < xCap && npc.direction == 1)
            {
                npc.velocity.X += acceleration;
                if (npc.velocity.X > xCap)
                    npc.velocity.X = xCap;
            }
            else if (npc.velocity.X > -xCap && npc.direction == -1)
            {
                npc.velocity.X -= acceleration;
                if (npc.velocity.X < -xCap)
                    npc.velocity.X = -xCap;
            }

            if (npc.collideX)
            {
                npc.ai[0]++;
                if (npc.collideY && npc.oldVelocity.Y >= 0)
                    npc.velocity.Y = jumpVelocity;
            }
            else if (npc.ai[0] > 0)
                npc.ai[0] = 0f;

            if (target != null)
            {
                if (npc.velocity.Y == 0f && target.Bottom.Y < npc.Top.Y && Math.Abs(npc.Center.X - target.Center.X) < (float)(target.width * 3) && Collision.CanHit(npc, target))
                {

                    if (npc.velocity.Y == 0f)
                    {
                        int padding = 6;
                        if (target.Bottom.Y > npc.Top.Y - (float)(padding * 16))
                        {
                            npc.velocity.Y = jumpVelocity;
                        }
                        else
                        {
                            int bottomtilepointx = (int)(npc.Center.X / 16f);
                            int bottomtilepointY = (int)(npc.Bottom.Y / 16f) - 1;
                            for (int i = bottomtilepointY; i > bottomtilepointY - padding; i--)
                            {
                                if (Main.tile[bottomtilepointx, i].HasUnactuatedTile && TileID.Sets.Platforms[Main.tile[bottomtilepointx, i].TileType])
                                {
                                    npc.velocity.Y = jumpVelocity;
                                    break;
                                }
                            }
                        }
                    }
                }
                else if (npc.velocity.Y == 0f && target.Top.Y > npc.Bottom.Y && Math.Abs(npc.Center.X - target.Center.X) < (float)(target.width * 3) && Collision.CanHit(npc, target))
                {
                    int fluff = 6;
                    int bottomtilepointx = (int)(npc.Center.X / 16f);
                    int bottomtilepointY = (int)(npc.Bottom.Y / 16f);
                    for (int i = bottomtilepointY; i > bottomtilepointY - fluff - 1; i--)
                    {
                        if (Main.tile[bottomtilepointx, i].HasUnactuatedTile && TileID.Sets.Platforms[Main.tile[bottomtilepointx, i].TileType])
                        {
                            npc.position.Y += 1;
                            npc.stairFall = true;
                            npc.velocity.Y += 0.01f;
                            break;
                        }
                    }
                }
            }

            if (npc.velocity.Y >= 0f)
            {
                int dir = 0;
                if (npc.velocity.X < 0f)
                    dir = -1;
                if (npc.velocity.X > 0f)
                    dir = 1;

                Vector2 futurePos = npc.position;
                futurePos.X += npc.velocity.X;
                int tileX = (int)((futurePos.X + (float)(npc.width / 2) + (float)((npc.width / 2 + 1) * dir)) / 16f);
                int tileY = (int)((futurePos.Y + (float)npc.height - 1f) / 16f);
                if (WorldGen.InWorld(tileX, tileY, 4))
                {
                    if ((float)(tileX * 16) < futurePos.X + (float)npc.width && (float)(tileX * 16 + 16) > futurePos.X && ((Main.tile[tileX, tileY].HasUnactuatedTile && !TopSlope(Main.tile[tileX, tileY]) && !TopSlope(Main.tile[tileX, tileY - 1]) && Main.tileSolid[Main.tile[tileX, tileY].TileType] && !Main.tileSolidTop[Main.tile[tileX, tileY].TileType]) || (Main.tile[tileX, tileY - 1].IsHalfBlock && Main.tile[tileX, tileY - 1].HasUnactuatedTile)) && (!Main.tile[tileX, tileY - 1].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX, tileY - 1].TileType] || Main.tileSolidTop[Main.tile[tileX, tileY - 1].TileType] || (Main.tile[tileX, tileY - 1].IsHalfBlock && (!Main.tile[tileX, tileY - 4].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX, tileY - 4].TileType] || Main.tileSolidTop[Main.tile[tileX, tileY - 4].TileType]))) && (!Main.tile[tileX, tileY - 2].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX, tileY - 2].TileType] || Main.tileSolidTop[Main.tile[tileX, tileY - 2].TileType]) && (!Main.tile[tileX, tileY - 3].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX, tileY - 3].TileType] || Main.tileSolidTop[Main.tile[tileX, tileY - 3].TileType]) && (!Main.tile[tileX - dir, tileY - 3].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX - dir, tileY - 3].TileType]))
                    {
                        float tilePosY = tileY * 16;
                        if (Main.tile[tileX, tileY].IsHalfBlock)
                            tilePosY += 8f;

                        if (Main.tile[tileX, tileY - 1].IsHalfBlock)
                            tilePosY -= 8f;

                        if (tilePosY < futurePos.Y + (float)npc.height)
                        {
                            float difference = futurePos.Y + (float)npc.height - tilePosY;
                            if (difference <= 16.1f)
                            {
                                npc.gfxOffY += npc.position.Y + (float)npc.height - tilePosY;
                                npc.position.Y = tilePosY - (float)npc.height;
                            }

                            if (difference < 9f)
                                npc.stepSpeed = 1f;
                            else
                                npc.stepSpeed = 2f;
                        }

                    }
                }
            }
        }
        public void RogueFighterShooterAI(NPC npc, float xCap, float jumpVelocity, float attackDistance, int attackTelegraph, int attackCooldown, float speedMultiWhenShooting, int projType, float projSpeed, Vector2 projOffset, int projDamage, bool LoSRequired, bool canJumpShoot = true, float? projVelocityDirectionOverride = null)
        {
            Entity target = GetTarget(npc, false, false);

            float realXCap = npc.ai[1] > 0 ? xCap * speedMultiWhenShooting : xCap;

            if (npc.ai[1] != 0)
                npc.ai[1]++;

            if (target == null)
            {
                if (npc.ai[1] > 0)
                    npc.ai[1] = 0;
            }
            else
            {
                if (npc.ai[1] == 0 && npc.ai[0] >= 0 && (canJumpShoot || npc.velocity.Y == 0) && (npc.Center - target.Center).Length() <= attackDistance && (!LoSRequired || Collision.CanHit(npc.Center + projOffset, 1, 1, target.Center, 1, 1)))
                {
                    npc.ai[1]++;
                }

                if (npc.ai[1] >= attackTelegraph)
                {
                    Vector2 projSpawnPos = npc.Center + projOffset;
                    int proj = Projectile.NewProjectile(npc.GetSource_FromThis(), projSpawnPos, (projVelocityDirectionOverride == null ? (target.Center - projSpawnPos).SafeNormalize(Vector2.UnitY) : Vector2.UnitX.RotatedBy((double)projVelocityDirectionOverride)) * projSpeed, projType, projDamage, 0f);
                    SetUpNPCProj(npc, proj);
                    npc.ai[1] = -attackCooldown;
                }
            }

            if (target == null && npc.direction == 0)
            {
                npc.direction = 1;
                npc.spriteDirection = 1;
            }
            if (npc.ai[0] == 0 && target != null)
            {
                if (npc.Center.X < target.Center.X)
                {
                    npc.direction = 1;
                    npc.spriteDirection = 1;
                }
                else
                {
                    npc.direction = -1;
                    npc.spriteDirection = -1;
                }
            }
            else if (npc.ai[0] > 60 && npc.ai[1] <= 0)
            {
                npc.ai[0] = -240;
                npc.direction *= -1;
                npc.spriteDirection *= -1;
            }
            if (npc.ai[0] < 0)
                npc.ai[0]++;

            if (npc.velocity.X < -realXCap || npc.velocity.X > realXCap)
            {
                if (npc.velocity.Y == 0f)
                    npc.velocity *= 0.8f;
            }
            else if (npc.velocity.X < realXCap && npc.direction == 1)
            {
                npc.velocity.X += 0.07f;
                if (npc.velocity.X > realXCap)
                    npc.velocity.X = realXCap;
            }
            else if (npc.velocity.X > -realXCap && npc.direction == -1)
            {
                npc.velocity.X -= 0.07f;
                if (npc.velocity.X < -realXCap)
                    npc.velocity.X = -realXCap;
            }

            if (npc.collideX)
            {
                npc.ai[0]++;
                if (npc.collideY && npc.oldVelocity.Y >= 0)
                    npc.velocity.Y = jumpVelocity;
            }
            else if (npc.ai[0] > 0)
                npc.ai[0] = 0f;

            if (target != null)
            {
                if (npc.velocity.Y == 0f && target.Bottom.Y < npc.Top.Y && Math.Abs(npc.Center.X - target.Center.X) < (float)(target.width * 3) && Collision.CanHit(npc, target))
                {

                    if (npc.velocity.Y == 0f && (canJumpShoot || npc.ai[1] == -1 || npc.ai[1] == 0))
                    {
                        int padding = 6;
                        if (target.Bottom.Y > npc.Top.Y - (float)(padding * 16))
                        {
                            npc.velocity.Y = jumpVelocity;
                        }
                        else
                        {
                            int bottomtilepointx = (int)(npc.Center.X / 16f);
                            int bottomtilepointY = (int)(npc.Bottom.Y / 16f) - 1;
                            for (int i = bottomtilepointY; i > bottomtilepointY - padding; i--)
                            {
                                if (Main.tile[bottomtilepointx, i].HasUnactuatedTile && TileID.Sets.Platforms[Main.tile[bottomtilepointx, i].TileType])
                                {
                                    npc.velocity.Y = jumpVelocity;
                                    break;
                                }
                            }
                        }
                    }
                }
                else if (npc.velocity.Y == 0f && target.Top.Y > npc.Bottom.Y && Math.Abs(npc.Center.X - target.Center.X) < (float)(target.width * 3) && Collision.CanHit(npc, target))
                {
                    int fluff = 6;
                    int bottomtilepointx = (int)(npc.Center.X / 16f);
                    int bottomtilepointY = (int)(npc.Bottom.Y / 16f);
                    for (int i = bottomtilepointY; i > bottomtilepointY - fluff - 1; i--)
                    {
                        if (Main.tile[bottomtilepointx, i].HasUnactuatedTile && TileID.Sets.Platforms[Main.tile[bottomtilepointx, i].TileType])
                        {
                            npc.position.Y += 1;
                            npc.stairFall = true;
                            npc.velocity.Y += 0.01f;
                            break;
                        }
                    }
                }
            }

            if (npc.velocity.Y >= 0f)
            {
                int dir = 0;
                if (npc.velocity.X < 0f)
                    dir = -1;
                if (npc.velocity.X > 0f)
                    dir = 1;

                Vector2 futurePos = npc.position;
                futurePos.X += npc.velocity.X;
                int tileX = (int)((futurePos.X + (float)(npc.width / 2) + (float)((npc.width / 2 + 1) * dir)) / 16f);
                int tileY = (int)((futurePos.Y + (float)npc.height - 1f) / 16f);
                if (WorldGen.InWorld(tileX, tileY, 4))
                {
                    if ((float)(tileX * 16) < futurePos.X + (float)npc.width && (float)(tileX * 16 + 16) > futurePos.X && ((Main.tile[tileX, tileY].HasUnactuatedTile && !TopSlope(Main.tile[tileX, tileY]) && !TopSlope(Main.tile[tileX, tileY - 1]) && Main.tileSolid[Main.tile[tileX, tileY].TileType] && !Main.tileSolidTop[Main.tile[tileX, tileY].TileType]) || (Main.tile[tileX, tileY - 1].IsHalfBlock && Main.tile[tileX, tileY - 1].HasUnactuatedTile)) && (!Main.tile[tileX, tileY - 1].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX, tileY - 1].TileType] || Main.tileSolidTop[Main.tile[tileX, tileY - 1].TileType] || (Main.tile[tileX, tileY - 1].IsHalfBlock && (!Main.tile[tileX, tileY - 4].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX, tileY - 4].TileType] || Main.tileSolidTop[Main.tile[tileX, tileY - 4].TileType]))) && (!Main.tile[tileX, tileY - 2].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX, tileY - 2].TileType] || Main.tileSolidTop[Main.tile[tileX, tileY - 2].TileType]) && (!Main.tile[tileX, tileY - 3].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX, tileY - 3].TileType] || Main.tileSolidTop[Main.tile[tileX, tileY - 3].TileType]) && (!Main.tile[tileX - dir, tileY - 3].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX - dir, tileY - 3].TileType]))
                    {
                        float tilePosY = tileY * 16;
                        if (Main.tile[tileX, tileY].IsHalfBlock)
                            tilePosY += 8f;

                        if (Main.tile[tileX, tileY - 1].IsHalfBlock)
                            tilePosY -= 8f;

                        if (tilePosY < futurePos.Y + (float)npc.height)
                        {
                            float difference = futurePos.Y + (float)npc.height - tilePosY;
                            if (difference <= 16.1f)
                            {
                                npc.gfxOffY += npc.position.Y + (float)npc.height - tilePosY;
                                npc.position.Y = tilePosY - (float)npc.height;
                            }

                            if (difference < 9f)
                                npc.stepSpeed = 1f;
                            else
                                npc.stepSpeed = 2f;
                        }

                    }
                }
            }
        }
        public void RogueTeleportingShooterAI(NPC npc, float minTeleportDist, float maxTeleportDist, int teleportCooldown, int attackTelegraph, int attackCooldown, int projType, float projSpeed, Vector2 projOffset, int projDamage, bool findAir = false, bool respectGravity = false)
        {
            Entity target = GetTarget(npc, false, false);

            if (target == null && npc.direction == 0)
            {
                npc.direction = 1;
                npc.spriteDirection = 1;
            }
            if (target != null)
            {
                if (npc.Center.X < target.Center.X)
                {
                    npc.direction = 1;
                    npc.spriteDirection = 1;
                }
                else
                {
                    npc.direction = -1;
                    npc.spriteDirection = -1;
                }
            }

            npc.ai[0]++;

            if ((int)(npc.ai[0] - attackTelegraph) % (attackTelegraph + attackCooldown) == 0)
            {
                Vector2 projPos = npc.Center + projOffset;
                Vector2 projVel = target == null ? Vector2.UnitX * npc.direction * projSpeed : (target.Center - projPos).SafeNormalize(Vector2.UnitX * npc.direction) * projSpeed;
                int proj = Projectile.NewProjectile(npc.GetSource_FromThis(), projPos, projVel, projType, projDamage, 0f, -1, target != null ? target.whoAmI : -1, targetPlayer != -1 ? 1 : (targetNPC != -1 ? 2 : 0));
                SetUpNPCProj(npc, proj);
            }

            if (npc.ai[0] >= teleportCooldown)
            {
                npc.ai[0] = 0;
                Vector2 teleportPos = npc.Bottom;
                if (target != null)
                {
                    int checks = 1;
                    if (findAir)
                        checks = 12;

                    for (int i = 0; i < checks; i++)
                    {
                        teleportPos = (Main.rand.NextFloat(MathHelper.TwoPi).ToRotationVector2() * Main.rand.NextFloat(minTeleportDist, maxTeleportDist + float.Epsilon)) + target.Center;
                        if (isRoomNPC && sourceRoomListID >= 0)
                        {
                            Room room = RoomList[sourceRoomListID];
                            if (room.wallActive)
                            {
                                Rectangle npcRect = npc.getRect();
                                npcRect.X += (int)(teleportPos - npc.Bottom).X;
                                npcRect.Y += (int)(teleportPos - npc.Bottom).Y;

                                Rectangle newRect = room.CheckRectWithWallCollision(npcRect);
                                teleportPos = new Vector2(newRect.X + (newRect.Width * 0.5f), newRect.Y + (newRect.Height * 0.5f));
                                if ((teleportPos - target.Center).Length() < minTeleportDist)
                                {
                                    teleportPos = ((room.RoomCenter16 + room.RoomPosition16 - target.Center).SafeNormalize(Vector2.UnitX) * minTeleportDist) + target.Center;
                                }
                            }
                        }


                        if (findAir)
                        {
                            bool pass = false;
                            int npcBlockHeight = (int)(npc.height / 16f) + npc.height % 16 == 0 ? 0 : 1;
                            Point targetBlock = new Point((int)(teleportPos.X / 16f), (int)(teleportPos.Y / 16f));


                            if (Main.tile[targetBlock].IsTileSolidGround() || (!respectGravity && Main.tile[targetBlock + new Point(0, npcBlockHeight)].IsTileSolidGround()))
                            {
                                for (int y = 0; y < 25; y++)
                                {
                                    if (!Main.tile[targetBlock - new Point(0, y)].IsTileSolidGround())
                                    {
                                        if (!Main.tile[targetBlock - new Point(0, y + npcBlockHeight)].IsTileSolidGround())
                                        {
                                            targetBlock -= new Point(0, y - 1);
                                            pass = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            else if (respectGravity)
                            {
                                for (int y = 0; y < 25; y++)
                                {
                                    if (Main.tile[targetBlock + new Point(0, y)].IsTileSolidGround())
                                    {
                                        if (!Main.tile[targetBlock + new Point(0, y - npcBlockHeight)].IsTileSolidGround())
                                        {
                                            targetBlock += new Point(0, y + npcBlockHeight - 1);
                                            pass = true;
                                            break;
                                        }
                                    }
                                }
                            }

                            if (isRoomNPC && sourceRoomListID >= 0)
                            {
                                Room room = RoomList[sourceRoomListID];
                                Rectangle potentialTeleportRect = new Rectangle((int)((targetBlock.X * 16f) + 8) - (npc.width / 2), (int)(targetBlock.Y * 16f) - npc.height, npc.width, npc.height);
                                if (potentialTeleportRect != room.CheckRectWithWallCollision(potentialTeleportRect))
                                {
                                    continue;
                                }
                            }

                            if (pass)
                            {
                                Vector2 potentialTeleportPos = new Vector2(targetBlock.X * 16f + 8, targetBlock.Y * 16f);
                                if ((potentialTeleportPos - target.Center).Length() < minTeleportDist)
                                    continue;

                                teleportPos = potentialTeleportPos;
                                break;
                            }
                            else if (i == checks - 1)
                                teleportPos = npc.Bottom;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                npc.Bottom = teleportPos;
            }
        }
        public void RogueTurretAI(NPC npc, int attackTelegraph, int attackCooldown, float attackDist, int projType, int projDamage, float projVelocity, Vector2 projOffset, bool LoSRequired, float? directionOverride = null, float? attackCone = null)
        {
            Entity target = GetTarget(npc, false, false);

            if (target != null)
            {
                if (npc.ai[0] == 0)
                {
                    if ((!LoSRequired || Collision.CanHit(target.Center, 1, 1, npc.Center, 1, 1)) && (npc.Center - target.Center).Length() <= attackDist && (attackCone == null || Math.Abs(RadianSizeBetween((target.Center - npc.Center).ToRotation(), (float)directionOverride)) <= attackCone * 0.5f))
                    {
                        npc.ai[0]++;
                    }
                }
                else
                {
                    npc.ai[0]++;
                    if (npc.ai[0] == attackTelegraph)
                    {
                        float direction = directionOverride == null ? (target.Center - (npc.Center + projOffset)).ToRotation() : (float)directionOverride;
                        int proj = Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center + projOffset, Vector2.UnitX.RotatedBy(direction) * projVelocity, projType, projDamage, 0f);
                        SetUpNPCProj(npc, proj);
                        npc.ai[0] = -attackCooldown;
                    }
                }
            }
            else if (npc.ai[0] > 0)
                npc.ai[0] = 0;
            else if (npc.ai[0] < 0)
                npc.ai[0]++;
        }
        public void RogueAntlionAI(NPC npc, float attackCone, float minBurrowDist, float maxBurrowDist, int burrowDownTime, int burrowUpTime, float burrowDepth, int burrowCooldown, int attackTelegraph, int attackCooldown, int projType, Vector2 projOffset, int projCount, int projDamage, float projSpread, float minProjVelocity, float maxProjVelocity)
        {
            Entity target = GetTarget(npc, false, false);

            npc.ai[0]++;

            if (target != null)
            {
                npc.rotation = MathHelper.Clamp(npc.rotation.AngleTowards((npc.Center - target.Center).ToRotation(), 0.02f), MathHelper.PiOver2 - (attackCone * 0.5f), MathHelper.PiOver2 + (attackCone * 0.5f));
            }
            else
            {
                npc.rotation = npc.rotation.AngleTowards(0f, 0.02f);
            }

            if (npc.ai[0] >= 0 && (int)(npc.ai[0] - attackTelegraph) % (attackTelegraph + attackCooldown) == 0)
            {
                Vector2 projPos = npc.Center + projOffset;
                for (int i = 0; i < projCount; i++)
                {
                    float projSpeed = Main.rand.NextFloat(minProjVelocity, maxProjVelocity + float.Epsilon);
                    Vector2 projVel = (npc.rotation + MathHelper.Pi + Main.rand.NextFloat(-projSpread * 0.5f, projSpread * 0.5f + float.Epsilon)).ToRotationVector2() * projSpeed;
                    int proj = Projectile.NewProjectile(npc.GetSource_FromThis(), projPos, projVel, projType, projDamage, 0f);
                    SetUpNPCProj(npc, proj);
                }

            }

            if (npc.ai[0] < 0)
            {
                npc.noGravity = true;
                if (npc.ai[0] == -burrowUpTime)
                {
                    npc.rotation = MathHelper.PiOver2;
                    npc.Center = new Vector2(npc.ai[2], npc.ai[3]) + (Vector2.UnitY * burrowDepth);
                }

                if (npc.ai[0] < -burrowUpTime)
                {
                    npc.Center += Vector2.UnitY * (burrowDepth / burrowDownTime);
                }
                else
                {
                    npc.Center -= Vector2.UnitY * (burrowDepth / burrowUpTime);
                }

            }

            if (npc.ai[0] >= burrowCooldown)
            {
                bool roomCondition = false;

                npc.ai[0] = -(burrowDownTime + burrowUpTime);
                Vector2 burrowPos = npc.Center;
                if (target != null)
                {
                    if (isRoomNPC && sourceRoomListID >= 0)
                    {
                        Room room = RoomList[sourceRoomListID];
                        if (room.wallActive)
                        {
                            roomCondition = true;
                        }
                    }

                    for (int i = 0; i < 50; i++)
                    {
                        burrowPos = (Main.rand.NextBool() ? -Vector2.UnitX : Vector2.UnitX) * Main.rand.NextFloat(minBurrowDist, maxBurrowDist + float.Epsilon) + target.Center;

                        Rectangle npcRect = npc.getRect();
                        npcRect.X += (int)(burrowPos - npc.Center).X;
                        npcRect.Y += (int)(burrowPos - npc.Center).Y;

                        Room room = null;
                        if (roomCondition)
                        {
                            room = RoomList[sourceRoomListID];
                        }



                        Rectangle newRect = npcRect;
                        if (roomCondition)
                        {
                            newRect = room.CheckRectWithWallCollision(npcRect);
                        }
                        burrowPos = new Vector2(newRect.X + (newRect.Width * 0.5f), newRect.Y + (newRect.Height * 0.5f));
                        if (roomCondition && (burrowPos - target.Center).Length() < minBurrowDist)
                        {
                            burrowPos = ((Vector2.UnitX * (room.RoomCenter16.X + room.RoomPosition16.X - target.Center.X)).SafeNormalize(Vector2.UnitX) * minBurrowDist) + target.Center;
                        }

                        int blockX = (int)(burrowPos.X / 16f);
                        int blockY = (int)(burrowPos.Y / 16f);

                        bool validPos = false;
                        int validYoffset = 0;
                        int checkDirection = TileID.Sets.BlockMergesWithMergeAllBlock[Main.tile[blockX, blockY].TileType] && Main.tile[blockX, blockY].HasTile ? -1 : 1;

                        for (int j = 0; j < 25; j++)
                        {
                            if (checkDirection == 1)
                            {
                                if (TileID.Sets.BlockMergesWithMergeAllBlock[Main.tile[blockX, blockY + j].TileType] && Main.tile[blockX, blockY + j].HasTile)
                                {
                                    if (roomCondition)
                                    {
                                        Vector2 potentialBurrowPos = ((burrowPos + (Vector2.UnitY * 16 * j)) - npc.Center) + npc.position;
                                        Rectangle potentialBurrowRect = new Rectangle((int)potentialBurrowPos.X, (int)potentialBurrowPos.Y, npc.width, npc.height);
                                        if (potentialBurrowRect != room.CheckRectWithWallCollision(potentialBurrowRect))
                                            continue;
                                    }
                                    validPos = true;
                                    validYoffset = j - 1;
                                    break;
                                }
                            }
                            else
                            {
                                if (!(TileID.Sets.BlockMergesWithMergeAllBlock[Main.tile[blockX, blockY - j].TileType] && Main.tile[blockX, blockY - j].HasTile))
                                {
                                    if (roomCondition)
                                    {
                                        Vector2 potentialBurrowPos = ((burrowPos + (Vector2.UnitY * -16 * j)) - npc.Center) + npc.position;
                                        Rectangle potentialBurrowRect = new Rectangle((int)potentialBurrowPos.X, (int)potentialBurrowPos.Y, npc.width, npc.height);
                                        if (potentialBurrowRect != room.CheckRectWithWallCollision(potentialBurrowRect))
                                            continue;
                                    }
                                    validPos = true;
                                    validYoffset = -j;
                                    break;
                                }
                            }
                        }
                        if (validPos)
                        {
                            burrowPos = new Vector2(blockX, blockY + validYoffset) * 16f;
                            break;
                        }

                        if (i == 49)
                            burrowPos = npc.Center;

                    }
                }
                npc.ai[2] = burrowPos.X;
                npc.ai[3] = burrowPos.Y;
            }
        }
        public void RogueFlyingShooterAI(NPC npc, float xCap, float yCap, float acceleration, float minAttackDist, float maxAttackDist, int attackTelegraph, int attackCooldown, int projType, float projSpeed, Vector2 projOffset, int projDamage, bool LoSRequired, float deceleration = 0.93f)
        {
            Entity target = GetTarget(npc, false, false);

            npc.ai[3]++;
            npc.stairFall = true;
            if (npc.collideY)
            {
                int fluff = 6;
                int bottomtilepointx = (int)(npc.Center.X / 16f);
                int bottomtilepointY = (int)(npc.Bottom.Y / 16f);
                for (int i = bottomtilepointY; i > bottomtilepointY - fluff - 1; i--)
                {
                    if (Main.tile[bottomtilepointx, i].HasUnactuatedTile && TileID.Sets.Platforms[Main.tile[bottomtilepointx, i].TileType])
                    {
                        npc.position.Y += 1;
                        npc.velocity.Y += 0.01f;
                        break;
                    }
                }
            }

            if (npc.direction == 0)
            {
                if (target == null)
                {
                    npc.direction = 1;
                    npc.spriteDirection = 1;
                }

                else
                {
                    if (npc.Center.X >= target.Center.X)
                    {
                        npc.direction = -1;
                        npc.spriteDirection = -1;
                    }
                    else
                    {
                        npc.direction = 1;
                        npc.spriteDirection = 1;
                    }
                }
            }

            if (npc.ai[2] != 0)
                npc.ai[2]++;

            bool distanceCheck = false;
            bool LoSCheck = false;

            if (target != null)
            {
                float dist = (target.Center - npc.Center).Length();

                if (dist >= minAttackDist && dist <= maxAttackDist)
                    distanceCheck = true;

                if (!LoSRequired || Collision.CanHit(npc.Center + projOffset, 1, 1, target.Center, 1, 1))
                    LoSCheck = true;

                if (npc.ai[2] == 0 && LoSCheck && distanceCheck)
                {
                    npc.ai[2]++;
                }
                else if (npc.ai[2] >= attackTelegraph)
                {
                    npc.ai[2] = -attackCooldown;
                    int proj = Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center + projOffset, (target.Center - npc.Center).SafeNormalize(Vector2.UnitX * npc.direction) * projSpeed, projType, projDamage, 0f);
                    SetUpNPCProj(npc, proj);
                }
            }

            if (npc.ai[2] > 0)
            {
                npc.velocity *= deceleration;
                if (target != null)
                {
                    if (npc.Center.X >= target.Center.X)
                    {
                        npc.direction = -1;
                        npc.spriteDirection = -1;
                    }
                    else
                    {
                        npc.direction = 1;
                        npc.spriteDirection = 1;
                    }
                }
                else
                    npc.ai[2] = 0;
            }
            else
            {
                if (distanceCheck && LoSCheck)
                {
                    npc.velocity *= deceleration;
                }
                else if (LoSCheck && !distanceCheck)
                {
                    float desiredDist = (minAttackDist + maxAttackDist) * 0.5f;
                    Vector2 desiredPos = (npc.Center - target.Center).SafeNormalize(Vector2.UnitX) * desiredDist + target.Center;
                    npc.velocity += (desiredPos - npc.Center).SafeNormalize(Vector2.UnitY) * acceleration;

                    if (npc.Center.X >= target.Center.X)
                    {
                        npc.direction = -1;
                        npc.spriteDirection = -1;
                    }
                    else
                    {
                        npc.direction = 1;
                        npc.spriteDirection = 1;
                    }
                }
                else
                {
                    if (Math.Abs(npc.velocity.X) < xCap)
                    {
                        npc.velocity.X += acceleration * npc.direction;
                    }
                    if (Math.Abs(npc.velocity.X) > xCap)
                    {
                        npc.velocity.X *= 0.98f;
                    }
                    if (Math.Abs(npc.velocity.Y) < yCap)
                    {
                        npc.velocity.Y += (target == null ? 1f : 0.75f) * acceleration * (float)Math.Cos((npc.ai[3] / 60f) * MathHelper.Pi) + (target == null ? 0 : (target.Center.Y >= npc.Center.Y ? 1 : -1) * acceleration * 0.25f);
                    }
                    if (Math.Abs(npc.velocity.Y) > yCap)
                    {
                        npc.velocity.Y *= 0.98f;
                    }

                    if (npc.collideX)
                        npc.ai[0]++;
                    else
                        npc.ai[0] = 0;

                    if (npc.ai[0] >= 90)
                    {
                        npc.ai[0] = 0;
                        npc.direction *= -1;
                        npc.spriteDirection *= -1;
                    }
                }
            }
        }
        public void RogueTortoiseAI(NPC npc, float xCap, float jumpVelocity, int jumpTime, int dashTime, float dashVelocity, int attackCooldown, int attackTelegraph)
        {
            Entity target = GetTarget(npc, false, false);

            if (npc.ai[1] < attackCooldown && npc.ai[1] >= 0)
                npc.ai[1]++;

            if (target == null && npc.direction == 0)
            {
                npc.direction = 1;
                npc.spriteDirection = 1;
            }
            if (npc.ai[0] == 0 && target != null && npc.ai[1] >= 0)
            {
                if (npc.Center.X < target.Center.X)
                {
                    npc.direction = 1;
                    npc.spriteDirection = 1;
                }
                else
                {
                    npc.direction = -1;
                    npc.spriteDirection = -1;
                }
            }
            else if (npc.ai[0] > 60 && npc.ai[1] >= 0)
            {
                npc.ai[0] = -240;
                npc.direction *= -1;
                npc.spriteDirection *= -1;
            }
            if (npc.ai[0] < 0)
                npc.ai[0]++;

            if (npc.ai[0] >= 0 && npc.ai[1] >= attackCooldown && npc.collideY)
            {
                npc.ai[1] = -jumpTime - dashTime;
            }

            if (npc.ai[1] >= 0 && npc.ai[1] < attackCooldown - attackTelegraph)
            {
                if (npc.velocity.X < -xCap || npc.velocity.X > xCap)
                {
                    if (npc.velocity.Y == 0f)
                        npc.velocity *= 0.8f;
                }
                else if (npc.velocity.X < xCap && npc.direction == 1)
                {
                    npc.velocity.X += 0.07f;
                    if (npc.velocity.X > xCap)
                        npc.velocity.X = xCap;
                }
                else if (npc.velocity.X > -xCap && npc.direction == -1)
                {
                    npc.velocity.X -= 0.07f;
                    if (npc.velocity.X < -xCap)
                        npc.velocity.X = -xCap;
                }
            }
            else
            {
                if (npc.ai[1] >= -dashTime)
                {
                    npc.stairFall = true;
                }

                if (npc.ai[1] < -dashTime)
                {
                    npc.ai[1]++;
                    npc.velocity.Y = jumpVelocity;
                }
                else if (npc.collideY && npc.oldVelocity.Y > 0)
                {
                    if (npc.ai[1] == -dashTime)
                    {
                        npc.ai[1]++;
                        npc.velocity.X = dashVelocity * npc.direction;

                    }
                }
                if (npc.ai[1] > -dashTime)
                {
                    npc.stairFall = true;
                    if (npc.collideX)
                    {
                        npc.direction *= -1;
                        npc.spriteDirection *= -1;
                        npc.velocity.X = npc.oldVelocity.X * -0.8f;
                    }
                    if (npc.collideY)
                    {
                        npc.velocity.Y = npc.oldVelocity.Y * -0.75f;
                        npc.velocity.X *= 0.9f;
                    }
                    npc.ai[1]++;
                }
            }


            if (npc.collideX)
            {
                npc.ai[0]++;
            }
            else if (npc.ai[0] > 0)
                npc.ai[0] = 0f;

            if (target != null)
            {
                if (npc.velocity.Y == 0f && target.Top.Y > npc.Bottom.Y && Math.Abs(npc.Center.X - target.Center.X) < (float)(target.width * 3) && Collision.CanHit(npc, target))
                {
                    int fluff = 6;
                    int bottomtilepointx = (int)(npc.Center.X / 16f);
                    int bottomtilepointY = (int)(npc.Bottom.Y / 16f);
                    for (int i = bottomtilepointY; i > bottomtilepointY - fluff - 1; i--)
                    {
                        if (Main.tile[bottomtilepointx, i].HasUnactuatedTile && TileID.Sets.Platforms[Main.tile[bottomtilepointx, i].TileType])
                        {
                            npc.position.Y += 1;
                            npc.stairFall = true;
                            npc.velocity.Y += 0.01f;
                            break;
                        }
                    }
                }
            }

            if (npc.velocity.Y >= 0f)
            {
                int dir = 0;
                if (npc.velocity.X < 0f)
                    dir = -1;
                if (npc.velocity.X > 0f)
                    dir = 1;

                Vector2 futurePos = npc.position;
                futurePos.X += npc.velocity.X;
                int tileX = (int)((futurePos.X + (float)(npc.width / 2) + (float)((npc.width / 2 + 1) * dir)) / 16f);
                int tileY = (int)((futurePos.Y + (float)npc.height - 1f) / 16f);
                if (WorldGen.InWorld(tileX, tileY, 4))
                {
                    if ((float)(tileX * 16) < futurePos.X + (float)npc.width && (float)(tileX * 16 + 16) > futurePos.X && ((Main.tile[tileX, tileY].HasUnactuatedTile && !TopSlope(Main.tile[tileX, tileY]) && !TopSlope(Main.tile[tileX, tileY - 1]) && Main.tileSolid[Main.tile[tileX, tileY].TileType] && !Main.tileSolidTop[Main.tile[tileX, tileY].TileType]) || (Main.tile[tileX, tileY - 1].IsHalfBlock && Main.tile[tileX, tileY - 1].HasUnactuatedTile)) && (!Main.tile[tileX, tileY - 1].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX, tileY - 1].TileType] || Main.tileSolidTop[Main.tile[tileX, tileY - 1].TileType] || (Main.tile[tileX, tileY - 1].IsHalfBlock && (!Main.tile[tileX, tileY - 4].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX, tileY - 4].TileType] || Main.tileSolidTop[Main.tile[tileX, tileY - 4].TileType]))) && (!Main.tile[tileX, tileY - 2].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX, tileY - 2].TileType] || Main.tileSolidTop[Main.tile[tileX, tileY - 2].TileType]) && (!Main.tile[tileX, tileY - 3].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX, tileY - 3].TileType] || Main.tileSolidTop[Main.tile[tileX, tileY - 3].TileType]) && (!Main.tile[tileX - dir, tileY - 3].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX - dir, tileY - 3].TileType]))
                    {
                        float tilePosY = tileY * 16;
                        if (Main.tile[tileX, tileY].IsHalfBlock)
                            tilePosY += 8f;

                        if (Main.tile[tileX, tileY - 1].IsHalfBlock)
                            tilePosY -= 8f;

                        if (tilePosY < futurePos.Y + (float)npc.height)
                        {
                            float difference = futurePos.Y + (float)npc.height - tilePosY;
                            if (difference <= 16.1f)
                            {
                                npc.gfxOffY += npc.position.Y + (float)npc.height - tilePosY;
                                npc.position.Y = tilePosY - (float)npc.height;
                            }

                            if (difference < 9f)
                                npc.stepSpeed = 1f;
                            else
                                npc.stepSpeed = 2f;
                        }

                    }
                }
            }
            if (npc.ai[1] > attackCooldown - attackTelegraph)
            {
                npc.velocity.X *= 0.8f;
            }
        }
        public void RogueSpookrowAI(NPC npc, float xCap, float jumpVelocity)
        {
            Entity target = GetTarget(npc, false, false);

            int slopeCheck1 = (int)Main.tile[(int)(npc.BottomLeft.X / 16f), (int)(npc.BottomLeft.Y / 16f)].Slope;
            int slopeCheck2 = (int)Main.tile[(int)(npc.BottomRight.X / 16f), (int)(npc.BottomRight.Y / 16f)].Slope;
            if (npc.velocity.Y >= 0 && (slopeCheck1 == 1 || slopeCheck1 == 2 || slopeCheck2 == 1 || slopeCheck2 == 2))
            {
                npc.collideY = true;
            }


            if (npc.collideY)
            {
                npc.velocity.X *= 0.6f;
                if (npc.ai[0] > 60 && npc.oldVelocity.Y >= 0)
                    npc.ai[0] = 30;
            }


            npc.velocity.X *= 0.98f;
            if (npc.collideX)
                npc.velocity.X = npc.oldVelocity.X * -0.5f;

            if (target != null)
            {
                if (npc.Center.X < target.Center.X)
                    npc.direction = 1;
                else
                    npc.direction = -1;
            }
            else
            {
                if (npc.velocity.X != 0)
                {
                    npc.direction = -(int)(npc.velocity.X / Math.Abs(npc.velocity.X));
                }
            }


            if (npc.ai[0] == 0)
            {
                npc.spriteDirection = npc.direction;
            }
            if (npc.ai[0] >= 30 && npc.ai[0] < 60)
                npc.spriteDirection = npc.direction;
            if (npc.ai[0] != 60)
            {
                npc.ai[0]++;
                if (!npc.collideY && npc.ai[0] < 60)
                    npc.ai[0]--;
            }
            if (npc.ai[0] == 60 && npc.collideY)
            {
                npc.velocity.X = xCap * npc.direction;
                npc.velocity.Y = jumpVelocity;
                npc.ai[0]++;
            }



            if (target != null)
            {
                if (npc.velocity.Y == 0f && target.Top.Y > npc.Bottom.Y && Math.Abs(npc.Center.X - target.Center.X) < (float)(target.width * 4) && Collision.CanHit(npc, target))
                {
                    int fluff = 6;
                    int bottomtilepointx = (int)(npc.Center.X / 16f);
                    int bottomtilepointY = (int)(npc.Bottom.Y / 16f);
                    for (int i = bottomtilepointY; i > bottomtilepointY - fluff - 1; i--)
                    {
                        if (Main.tile[bottomtilepointx, i].HasUnactuatedTile && TileID.Sets.Platforms[Main.tile[bottomtilepointx, i].TileType])
                        {
                            npc.position.Y += 1;
                            npc.stairFall = true;
                            npc.velocity.Y += 0.01f;
                            break;
                        }
                    }
                }
            }

            if (npc.velocity.Y >= 0f)
            {
                int dir = 0;
                if (npc.velocity.X < 0f)
                    dir = -1;
                if (npc.velocity.X > 0f)
                    dir = 1;

                Vector2 futurePos = npc.position;
                futurePos.X += npc.velocity.X;
                int tileX = (int)((futurePos.X + (float)(npc.width / 2) + (float)((npc.width / 2 + 1) * dir)) / 16f);
                int tileY = (int)((futurePos.Y + (float)npc.height - 1f) / 16f);
                if (WorldGen.InWorld(tileX, tileY, 4))
                {
                    if ((float)(tileX * 16) < futurePos.X + (float)npc.width && (float)(tileX * 16 + 16) > futurePos.X && ((Main.tile[tileX, tileY].HasUnactuatedTile && !TopSlope(Main.tile[tileX, tileY]) && !TopSlope(Main.tile[tileX, tileY - 1]) && Main.tileSolid[Main.tile[tileX, tileY].TileType] && !Main.tileSolidTop[Main.tile[tileX, tileY].TileType]) || (Main.tile[tileX, tileY - 1].IsHalfBlock && Main.tile[tileX, tileY - 1].HasUnactuatedTile)) && (!Main.tile[tileX, tileY - 1].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX, tileY - 1].TileType] || Main.tileSolidTop[Main.tile[tileX, tileY - 1].TileType] || (Main.tile[tileX, tileY - 1].IsHalfBlock && (!Main.tile[tileX, tileY - 4].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX, tileY - 4].TileType] || Main.tileSolidTop[Main.tile[tileX, tileY - 4].TileType]))) && (!Main.tile[tileX, tileY - 2].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX, tileY - 2].TileType] || Main.tileSolidTop[Main.tile[tileX, tileY - 2].TileType]) && (!Main.tile[tileX, tileY - 3].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX, tileY - 3].TileType] || Main.tileSolidTop[Main.tile[tileX, tileY - 3].TileType]) && (!Main.tile[tileX - dir, tileY - 3].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX - dir, tileY - 3].TileType]))
                    {
                        float tilePosY = tileY * 16;

                        if (Main.tile[tileX, tileY].IsHalfBlock)
                            tilePosY += 8f;

                        if (Main.tile[tileX, tileY - 1].IsHalfBlock)
                            tilePosY -= 8f;

                        if (tilePosY < futurePos.Y + (float)npc.height)
                        {
                            float difference = futurePos.Y + (float)npc.height - tilePosY;
                            if (difference <= 16.1f)
                            {
                                npc.gfxOffY += npc.position.Y + (float)npc.height - tilePosY;
                                npc.position.Y = tilePosY - (float)npc.height;
                            }

                            if (difference < 9f)
                                npc.stepSpeed = 1f;
                            else
                                npc.stepSpeed = 2f;
                        }

                    }
                }
            }
        }
        public void RogueWormAI(NPC npc, float maxVelocity, float turnRadians, float slowTurnDist)
        {
            Entity target = GetTarget(npc, false, false);

            Vector2 targetPos = npc.Center + Vector2.UnitX.RotatedBy(-npc.rotation);
            if (target != null)
            {
                targetPos = target.Center;
            }

            float targetAngle = (targetPos - npc.Center).ToRotation();
            bool slowTurn = npc.ai[0] == 0;
            float newAngle = npc.rotation.AngleTowards(targetAngle, slowTurn ? turnRadians : turnRadians * 0.4f);
            float angleChange = (float)Math.Atan2(Math.Sin(newAngle - npc.rotation), Math.Cos(newAngle - npc.rotation));
            if ((targetPos - npc.Center).Length() > slowTurnDist)
            {
                npc.ai[0] = 0;
            }
            if (Math.Abs(angleChange) <= turnRadians * 0.3f || (targetPos - npc.Center).Length() < slowTurnDist * 0.75f)
                npc.ai[0] = 1;

            float velMultiplier = slowTurn ? Math.Abs(Vector2.Dot((npc.rotation + MathHelper.PiOver2).ToRotationVector2(), targetAngle.ToRotationVector2())) : 0;
            npc.rotation = newAngle;
            Vector2 wantedVelocity = slowTurn ? npc.rotation.ToRotationVector2() * (maxVelocity * ((((1f - velMultiplier * 0.85f)) + 0.15f))) : npc.rotation.ToRotationVector2() * maxVelocity;
            npc.velocity = Vector2.Lerp(npc.velocity, wantedVelocity, 0.75f);
        }
        public void RogueGiantBatAI(NPC npc, float distanceAbove, float acceleration, float maxVelocity, int attackTelegraph, int attackCooldown, float attackDistance, int projType, Vector2 projVelocity, int projDamage)
        {
            Entity target = GetTarget(npc, false, false);

            npc.stairFall = true;

            if (npc.collideY)
            {
                int fluff = 6;
                int bottomtilepointx = (int)(npc.Center.X / 16f);
                int bottomtilepointY = (int)(npc.Bottom.Y / 16f);
                for (int i = bottomtilepointY; i > bottomtilepointY - fluff - 1; i--)
                {
                    if (Main.tile[bottomtilepointx, i].HasUnactuatedTile && TileID.Sets.Platforms[Main.tile[bottomtilepointx, i].TileType])
                    {
                        npc.position.Y += 1;
                        npc.stairFall = true;
                        npc.velocity.Y += 0.01f;
                        break;
                    }
                }
            }

            if (target != null)
            {
                Vector2 targetPos = target.Center + new Vector2(0, -distanceAbove);
                if ((npc.Center - targetPos).Length() < attackDistance)
                {
                    npc.velocity *= 0.95f;
                    if (npc.ai[0] == 0)
                    {
                        npc.ai[0]++;
                    }
                }
                else
                {
                    npc.velocity += (targetPos - npc.Center).SafeNormalize(Vector2.UnitY) * acceleration;
                }
                if (npc.velocity.Length() > maxVelocity)
                {
                    npc.velocity = npc.velocity.SafeNormalize(Vector2.UnitY) * maxVelocity;
                }

                if (npc.ai[0] != 0)
                {
                    npc.ai[0]++;
                }

                if (npc.ai[0] >= attackTelegraph)
                {
                    npc.ai[0] = -attackCooldown;
                    int proj = Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, projVelocity, projType, projDamage, 0f);
                    SetUpNPCProj(npc, proj);
                }
            }
            else
            {
                if (npc.ai[0] != 0)
                {
                    npc.ai[0]++;
                }

                if (npc.ai[0] >= attackTelegraph)
                {
                    npc.ai[0] = -attackCooldown;
                    int proj = Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, projVelocity, projType, projDamage, 0f);
                    SetUpNPCProj(npc, proj);
                }

                npc.velocity.X += (npc.velocity.X == 0 ? 1 : npc.velocity.X / Math.Abs(npc.velocity.X)) * acceleration;
                npc.velocity.Y += Main.rand.NextFloat(-acceleration / 2f, acceleration / 2f);

                if (npc.velocity.Length() > maxVelocity)
                {
                    npc.velocity = npc.velocity.SafeNormalize(Vector2.UnitX) * maxVelocity;
                }
            }

            if (npc.collideX)
            {
                npc.velocity.X *= -0.75f;
            }
            if (npc.collideY)
            {
                npc.velocity.Y *= -0.25f;
            }
        }
        public void RogueFrostbiterAI(NPC npc, float distanceBeside, int dashTime, float dashVelocity, float passiveAccel, float passiveMaxVelocity, int attackTelegraph, int waitTimeAfterAttack, float attackDistance, int projType, float projVelocity, int projDamage, int projCount)
        {
            //ai0 is for attack timers.
            //ai1 is for attack state.
            //ai2 and ai3 store a vector.

            Entity target = GetTarget(npc, false, false);

            if (npc.ai[0] == 0)
            {
                if (npc.ai[1] == 0)
                {
                    if (target != null)
                    {
                        if (npc.Center.X >= target.Center.X)
                            npc.direction = 1;
                        else
                            npc.direction = -1;
                    }
                    else
                    {
                        if (npc.direction == 0)
                            npc.direction = -1;
                        else
                            npc.direction *= -1;
                    }

                }
                else if (npc.direction != 0)
                {
                    Vector2 nextTarget = Main.rand.NextVector2CircularEdge(attackDistance, attackDistance);
                    nextTarget.X = Math.Abs(nextTarget.X) * -npc.direction;
                    nextTarget += target == null ? npc.Center : target.Center;
                    npc.ai[2] = nextTarget.X;
                    npc.ai[3] = nextTarget.Y;
                    if (target != null)
                        npc.direction = 0;
                }
            }

            if (npc.ai[0] != 0 || target == null)
            {
                npc.ai[0]++;
                if (npc.ai[0] < attackTelegraph)
                    npc.velocity *= 0.93f;
            }

            if (target != null)
            {
                Vector2 targetPos = npc.ai[1] == 0 ? new Vector2(distanceBeside * npc.direction, 0) + target.Center : new Vector2(npc.ai[2], npc.ai[3]);

                float greaterDist = attackDistance > distanceBeside ? attackDistance : distanceBeside;
                if ((npc.Center - targetPos).Length() < 64f || ((npc.collideX || npc.collideY) && (npc.Center - target.Center).Length() < greaterDist))
                {
                    if (npc.ai[0] == 0)
                        npc.ai[0]++;
                }
                else if (npc.ai[0] == 0)
                {
                    npc.velocity += (targetPos - npc.Center).SafeNormalize(Vector2.UnitY) * ((npc.Center - targetPos).Length() < 64f ? passiveAccel : passiveAccel * 2f);
                }
                if (npc.velocity.Length() > passiveMaxVelocity && npc.ai[0] < attackTelegraph)
                    npc.velocity = npc.velocity.SafeNormalize(Vector2.UnitY) * passiveMaxVelocity;
            }

            if (npc.ai[0] >= attackTelegraph)
            {
                if (npc.ai[1] == 0 && npc.ai[0] == attackTelegraph)
                {
                    npc.velocity = target != null ? (target.Center - npc.Center).SafeNormalize(-Vector2.UnitX * npc.direction) * dashVelocity : -Vector2.UnitX * npc.direction * dashVelocity;
                }
                else if (npc.ai[1] == 1)
                {
                    float anglePerProj = MathHelper.TwoPi / projCount;
                    for (int i = 0; i < projCount; i++)
                    {
                        int proj = Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, (-Vector2.UnitY * projVelocity).RotatedBy(anglePerProj * i), projType, projDamage, 0f);
                        SetUpNPCProj(npc, proj);
                    }
                }

                if (npc.ai[1] == 1 || npc.ai[0] >= attackTelegraph + dashTime)
                {
                    npc.ai[0] = -waitTimeAfterAttack;
                    if (npc.ai[1] == 0)
                        npc.ai[1] = 1;
                    else
                        npc.ai[1] = 0;
                }
            }
        }
        public void RogueDungeonSpiritAI(NPC npc, float acceleration, float speedCap)
        {
            Entity target = GetTarget(npc, false, false);

            if (target != null)
            {
                npc.velocity += (target.Center - npc.Center).SafeNormalize(Vector2.UnitX) * acceleration;
                if (npc.velocity.Length() > speedCap || (npc.Center - target.Center).Length() < 64f)
                {
                    npc.velocity *= 0.98f;
                }
            }
            else
            {
                npc.velocity *= 0.98f;
            }
        }
        public void RogueBallAndChainThrowerAI(NPC npc, float xCap, float jumpVelocity, float acceleration, int attackWindUpTime, int attackExhaustTime, int attackCooldown, ref BallAndChain ball, float launchVelocity, float ballTetherDist, int damage, float attackDist)
        {
            Entity target = GetTarget(npc, false, false);

            if (npc.ai[2] != 2)
            {
                if (npc.ai[1] < attackCooldown)
                {
                    npc.ai[1]++;
                }
                else if (npc.ai[1] == attackCooldown && target != null)
                {
                    if (Collision.CanHit(npc.Center, 1, 1, target.Center, 1, 1) && (npc.Center - target.Center).Length() <= attackDist)
                    {
                        ball.Position = (Vector2.UnitX * 24 * npc.direction) + new Vector2(-ball.Width * 0.5f, -ball.Height * 0.5f) + npc.Center;
                        npc.ai[2] = npc.direction;
                        npc.ai[1]++;
                    }
                }

                if (npc.ai[1] > attackCooldown)
                {
                    float distance = MathHelper.Clamp(MathHelper.Lerp(24f, 48f, (npc.ai[1] - attackCooldown) / attackWindUpTime), 24f, 48f);

                    ball.Rotation += 0.08f * npc.ai[2];
                    ball.Position = ((ball.Center - npc.Center).SafeNormalize(Vector2.UnitX).ToRotation() + (npc.ai[2] * MathHelper.Pi / 32f)).ToRotationVector2() * distance + new Vector2(-ball.Width * 0.5f, -ball.Height * 0.5f) + npc.Center;
                }
                if (npc.ai[1] > attackCooldown && npc.ai[1] < attackCooldown + attackWindUpTime)
                {
                    npc.velocity.X *= 0.9f;
                    npc.ai[1]++;
                }
                else if (npc.ai[2] == 0)
                {
                    if (target == null && npc.direction == 0)
                    {
                        npc.direction = 1;
                        npc.spriteDirection = 1;
                    }
                    if (npc.ai[0] == 0 && target != null)
                    {
                        if (npc.Center.X < target.Center.X)
                        {
                            npc.direction = 1;
                            npc.spriteDirection = 1;
                        }
                        else
                        {
                            npc.direction = -1;
                            npc.spriteDirection = -1;
                        }
                    }
                    else if (npc.ai[0] > 60)
                    {
                        npc.ai[0] = -240;
                        npc.direction *= -1;
                        npc.spriteDirection *= -1;
                    }
                    if (npc.ai[0] < 0)
                        npc.ai[0]++;

                    if (npc.velocity.X < -xCap || npc.velocity.X > xCap)
                    {
                        if (npc.velocity.Y == 0f)
                            npc.velocity *= 0.8f;
                    }
                    else if (npc.velocity.X < xCap && npc.direction == 1)
                    {
                        npc.velocity.X += acceleration;
                        if (npc.velocity.X > xCap)
                            npc.velocity.X = xCap;
                    }
                    else if (npc.velocity.X > -xCap && npc.direction == -1)
                    {
                        npc.velocity.X -= acceleration;
                        if (npc.velocity.X < -xCap)
                            npc.velocity.X = -xCap;
                    }

                    if (npc.collideX)
                    {
                        npc.ai[0]++;
                        if (npc.collideY && npc.oldVelocity.Y >= 0)
                            npc.velocity.Y = jumpVelocity;
                    }
                    else if (npc.ai[0] > 0)
                        npc.ai[0] = 0f;

                    if (target != null)
                    {
                        if (npc.velocity.Y == 0f && target.Bottom.Y < npc.Top.Y && Math.Abs(npc.Center.X - target.Center.X) < (float)(target.width * 3) && Collision.CanHit(npc, target))
                        {

                            if (npc.velocity.Y == 0f)
                            {
                                int padding = 6;
                                if (target.Bottom.Y > npc.Top.Y - (float)(padding * 16))
                                {
                                    npc.velocity.Y = jumpVelocity;
                                }
                                else
                                {
                                    int bottomtilepointx = (int)(npc.Center.X / 16f);
                                    int bottomtilepointY = (int)(npc.Bottom.Y / 16f) - 1;
                                    for (int i = bottomtilepointY; i > bottomtilepointY - padding; i--)
                                    {
                                        if (Main.tile[bottomtilepointx, i].HasUnactuatedTile && TileID.Sets.Platforms[Main.tile[bottomtilepointx, i].TileType])
                                        {
                                            npc.velocity.Y = jumpVelocity;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else if (npc.velocity.Y == 0f && target.Top.Y > npc.Bottom.Y && Math.Abs(npc.Center.X - target.Center.X) < (float)(target.width * 3) && Collision.CanHit(npc, target))
                        {
                            int fluff = 6;
                            int bottomtilepointx = (int)(npc.Center.X / 16f);
                            int bottomtilepointY = (int)(npc.Bottom.Y / 16f);
                            for (int i = bottomtilepointY; i > bottomtilepointY - fluff - 1; i--)
                            {
                                if (Main.tile[bottomtilepointx, i].HasUnactuatedTile && TileID.Sets.Platforms[Main.tile[bottomtilepointx, i].TileType])
                                {
                                    npc.position.Y += 1;
                                    npc.stairFall = true;
                                    npc.velocity.Y += 0.01f;
                                    break;
                                }
                            }
                        }
                    }

                    if (npc.velocity.Y >= 0f)
                    {
                        int dir = 0;
                        if (npc.velocity.X < 0f)
                            dir = -1;
                        if (npc.velocity.X > 0f)
                            dir = 1;

                        Vector2 futurePos = npc.position;
                        futurePos.X += npc.velocity.X;
                        int tileX = (int)((futurePos.X + (float)(npc.width / 2) + (float)((npc.width / 2 + 1) * dir)) / 16f);
                        int tileY = (int)((futurePos.Y + (float)npc.height - 1f) / 16f);
                        if (WorldGen.InWorld(tileX, tileY, 4))
                        {
                            if ((float)(tileX * 16) < futurePos.X + (float)npc.width && (float)(tileX * 16 + 16) > futurePos.X && ((Main.tile[tileX, tileY].HasUnactuatedTile && !TopSlope(Main.tile[tileX, tileY]) && !TopSlope(Main.tile[tileX, tileY - 1]) && Main.tileSolid[Main.tile[tileX, tileY].TileType] && !Main.tileSolidTop[Main.tile[tileX, tileY].TileType]) || (Main.tile[tileX, tileY - 1].IsHalfBlock && Main.tile[tileX, tileY - 1].HasUnactuatedTile)) && (!Main.tile[tileX, tileY - 1].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX, tileY - 1].TileType] || Main.tileSolidTop[Main.tile[tileX, tileY - 1].TileType] || (Main.tile[tileX, tileY - 1].IsHalfBlock && (!Main.tile[tileX, tileY - 4].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX, tileY - 4].TileType] || Main.tileSolidTop[Main.tile[tileX, tileY - 4].TileType]))) && (!Main.tile[tileX, tileY - 2].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX, tileY - 2].TileType] || Main.tileSolidTop[Main.tile[tileX, tileY - 2].TileType]) && (!Main.tile[tileX, tileY - 3].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX, tileY - 3].TileType] || Main.tileSolidTop[Main.tile[tileX, tileY - 3].TileType]) && (!Main.tile[tileX - dir, tileY - 3].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX - dir, tileY - 3].TileType]))
                            {
                                float tilePosY = tileY * 16;
                                if (Main.tile[tileX, tileY].IsHalfBlock)
                                    tilePosY += 8f;

                                if (Main.tile[tileX, tileY - 1].IsHalfBlock)
                                    tilePosY -= 8f;

                                if (tilePosY < futurePos.Y + (float)npc.height)
                                {
                                    float difference = futurePos.Y + (float)npc.height - tilePosY;
                                    if (difference <= 16.1f)
                                    {
                                        npc.gfxOffY += npc.position.Y + (float)npc.height - tilePosY;
                                        npc.position.Y = tilePosY - (float)npc.height;
                                    }

                                    if (difference < 9f)
                                        npc.stepSpeed = 1f;
                                    else
                                        npc.stepSpeed = 2f;
                                }

                            }
                        }
                    }
                }

                if (npc.ai[1] == attackWindUpTime + attackCooldown)
                {
                    npc.velocity.X *= 0.9f;
                    float targetAngle = target != null ? (target.Center - npc.Center).ToRotation() : (npc.ai[2] * -MathHelper.PiOver2) + MathHelper.PiOver2;
                    if (Math.Abs(RadianSizeBetween((ball.Center - npc.Center).ToRotation(), targetAngle - (MathHelper.PiOver2 * npc.ai[2]))) <  MathHelper.Pi * 0.0625f)
                    {
                        npc.ai[3] = Projectile.NewProjectile(npc.GetSource_FromThis(), ball.Center, (Vector2.UnitX * launchVelocity).RotatedBy(targetAngle + (MathHelper.PiOver4 * npc.ai[2] * 0.4f)), ModContent.ProjectileType<SpikedBall>(), damage, 0f, -1, ball.Rotation);
                        SetUpNPCProj(npc, (int)npc.ai[3]);
                        Main.projectile[(int)npc.ai[3]].direction = (int)npc.ai[2];
                        npc.ai[2] = 2;
                        if (target != null)
                        {
                            if (npc.Center.X > target.Center.X)
                            {
                                npc.direction = -1;
                                npc.spriteDirection = -1;
                            }
                            else
                            {
                                npc.direction = 1;
                                npc.spriteDirection = 1;
                            }
                        }
                    }
                }
            }
            else
            {
                npc.velocity.X *= 0.9f;
                if (npc.ai[1] >= attackWindUpTime + attackExhaustTime + attackCooldown)
                {
                    if ((Main.projectile[(int)npc.ai[3]].Center - npc.Center).Length() <= 12f || !Main.projectile[(int)npc.ai[3]].active)
                    {
                        npc.ai[2] = 0;
                        npc.ai[1] = 0;
                    }
                }
                else
                {
                    npc.ai[1]++;
                }
            }
        }
        public void RogueAssasinAI(NPC npc, float xCap, float jumpVelocity, float acceleration, int teleportTelegraph, int teleportCooldown, int teleportExhaustTime, float minTeleportDist, float maxTeleportDist)
        {
            Entity target = GetTarget(npc, false, false);

            bool teleporting = false;
            if (npc.ai[1] < teleportCooldown)
            {
                npc.ai[1]++;
            }
            if (npc.ai[1] == teleportCooldown && npc.velocity.Y == 0)
                npc.ai[1]++;

            if (target != null)
            {
                if (npc.ai[1] > teleportCooldown)
                {
                    npc.ai[0] = 0;
                    teleporting = true;
                    if (npc.ai[1] != teleportTelegraph + teleportCooldown)
                    {
                        npc.velocity.X *= 0.9f;
                    }
                    else
                    {
                        Vector2 teleportPos = npc.Bottom;
                        if (target != null)
                        {
                            for (int i = 0; i < 12; i++)
                            {
                                teleportPos = (Main.rand.NextFloat(MathHelper.TwoPi).ToRotationVector2() * Main.rand.NextFloat(minTeleportDist, maxTeleportDist + float.Epsilon)) + target.Center;
                                if (isRoomNPC && sourceRoomListID >= 0)
                                {
                                    Room room = RoomList[sourceRoomListID];
                                    if (room.wallActive)
                                    {
                                        Rectangle npcRect = npc.getRect();
                                        npcRect.X += (int)(teleportPos - npc.Bottom).X;
                                        npcRect.Y += (int)(teleportPos - npc.Bottom).Y;

                                        Rectangle newRect = room.CheckRectWithWallCollision(npcRect);
                                        teleportPos = new Vector2(newRect.X + (newRect.Width * 0.5f), newRect.Y + (newRect.Height * 0.5f));
                                        if ((teleportPos - target.Center).Length() < minTeleportDist)
                                        {
                                            teleportPos = ((room.RoomCenter16 + room.RoomPosition16 - target.Center).SafeNormalize(Vector2.UnitX) * minTeleportDist) + target.Center;
                                        }
                                    }
                                }



                                    bool pass = false;
                                    int npcBlockHeight = (int)(npc.height / 16f) + npc.height % 16 == 0 ? 0 : 1;
                                    Point targetBlock = new Point((int)(teleportPos.X / 16f), (int)(teleportPos.Y / 16f));


                                    if (Main.tile[targetBlock].IsTileSolidGround())
                                    {
                                        for (int y = 0; y < 25; y++)
                                        {
                                            if (!Main.tile[targetBlock - new Point(0, y)].IsTileSolidGround())
                                            {
                                                if (!Main.tile[targetBlock - new Point(0, y + npcBlockHeight)].IsTileSolidGround())
                                                {
                                                    targetBlock -= new Point(0, y - 1);
                                                    pass = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        for (int y = 0; y < 25; y++)
                                        {
                                            if (Main.tile[targetBlock + new Point(0, y)].IsTileSolidGround())
                                            {
                                                if (!Main.tile[targetBlock + new Point(0, y - npcBlockHeight)].IsTileSolidGround())
                                                {
                                                    targetBlock += new Point(0, y + npcBlockHeight - 1);
                                                    pass = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }

                                    if (isRoomNPC && sourceRoomListID >= 0)
                                    {
                                        Room room = RoomList[sourceRoomListID];
                                        Rectangle potentialTeleportRect = new Rectangle((int)((targetBlock.X * 16f) + 8) - (npc.width / 2), (int)(targetBlock.Y * 16f) - npc.height, npc.width, npc.height);
                                        if (potentialTeleportRect != room.CheckRectWithWallCollision(potentialTeleportRect))
                                        {
                                            continue;
                                        }
                                    }

                                    if (pass)
                                    {
                                        Vector2 potentialTeleportPos = new Vector2(targetBlock.X * 16f + 8, targetBlock.Y * 16f);
                                        if ((potentialTeleportPos - target.Center).Length() < minTeleportDist)
                                            continue;

                                        teleportPos = potentialTeleportPos;
                                        break;
                                    }
                                    else if (i == 11)
                                        teleportPos = npc.Bottom;
                            }
                        }
                        npc.Bottom = teleportPos;
                    }

                    npc.ai[1]++;
                }
            }
            else
                npc.ai[1] = 0;

            if (npc.ai[1] > teleportCooldown + teleportTelegraph + teleportExhaustTime)
                npc.ai[1] = 0;

            if (target == null && npc.direction == 0)
            {
                npc.direction = 1;
                npc.spriteDirection = 1;
            }
            if (npc.ai[0] == 0 && target != null)
            {
                if (npc.Center.X < target.Center.X)
                {
                    npc.direction = 1;
                    npc.spriteDirection = 1;
                }
                else
                {
                    npc.direction = -1;
                    npc.spriteDirection = -1;
                }
            }
            else if (npc.ai[0] > 60 && !teleporting)
            {
                npc.ai[0] = -240;
                npc.direction *= -1;
                npc.spriteDirection *= -1;
            }
            if (npc.ai[0] < 0)
                npc.ai[0]++;

            if (!teleporting)
            {
                if (npc.velocity.X < -xCap || npc.velocity.X > xCap)
                {
                    if (npc.velocity.Y == 0f)
                        npc.velocity *= 0.8f;
                }
                else if (npc.velocity.X < xCap && npc.direction == 1)
                {
                    npc.velocity.X += acceleration;
                    if (npc.velocity.X > xCap)
                        npc.velocity.X = xCap;
                }
                else if (npc.velocity.X > -xCap && npc.direction == -1)
                {
                    npc.velocity.X -= acceleration;
                    if (npc.velocity.X < -xCap)
                        npc.velocity.X = -xCap;
                }

                if (npc.collideX)
                {
                    npc.ai[0]++;
                    if (npc.collideY && npc.oldVelocity.Y >= 0)
                        npc.velocity.Y = jumpVelocity;
                }
                else if (npc.ai[0] > 0)
                    npc.ai[0] = 0f;

                if (target != null)
                {
                    if (npc.velocity.Y == 0f && target.Bottom.Y < npc.Top.Y && Math.Abs(npc.Center.X - target.Center.X) < (float)(target.width * 3) && Collision.CanHit(npc, target))
                    {

                        if (npc.velocity.Y == 0f)
                        {
                            int padding = 6;
                            if (target.Bottom.Y > npc.Top.Y - (float)(padding * 16))
                            {
                                npc.velocity.Y = jumpVelocity;
                            }
                            else
                            {
                                int bottomtilepointx = (int)(npc.Center.X / 16f);
                                int bottomtilepointY = (int)(npc.Bottom.Y / 16f) - 1;
                                for (int i = bottomtilepointY; i > bottomtilepointY - padding; i--)
                                {
                                    if (Main.tile[bottomtilepointx, i].HasUnactuatedTile && TileID.Sets.Platforms[Main.tile[bottomtilepointx, i].TileType])
                                    {
                                        npc.velocity.Y = jumpVelocity;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else if (npc.velocity.Y == 0f && target.Top.Y > npc.Bottom.Y && Math.Abs(npc.Center.X - target.Center.X) < (float)(target.width * 3) && Collision.CanHit(npc, target))
                    {
                        int fluff = 6;
                        int bottomtilepointx = (int)(npc.Center.X / 16f);
                        int bottomtilepointY = (int)(npc.Bottom.Y / 16f);
                        for (int i = bottomtilepointY; i > bottomtilepointY - fluff - 1; i--)
                        {
                            if (Main.tile[bottomtilepointx, i].HasUnactuatedTile && TileID.Sets.Platforms[Main.tile[bottomtilepointx, i].TileType])
                            {
                                npc.position.Y += 1;
                                npc.stairFall = true;
                                npc.velocity.Y += 0.01f;
                                break;
                            }
                        }
                    }
                }


                if (npc.velocity.Y >= 0f)
                {
                    int dir = 0;
                    if (npc.velocity.X < 0f)
                        dir = -1;
                    if (npc.velocity.X > 0f)
                        dir = 1;

                    Vector2 futurePos = npc.position;
                    futurePos.X += npc.velocity.X;
                    int tileX = (int)((futurePos.X + (float)(npc.width / 2) + (float)((npc.width / 2 + 1) * dir)) / 16f);
                    int tileY = (int)((futurePos.Y + (float)npc.height - 1f) / 16f);
                    if (WorldGen.InWorld(tileX, tileY, 4))
                    {
                        if ((float)(tileX * 16) < futurePos.X + (float)npc.width && (float)(tileX * 16 + 16) > futurePos.X && ((Main.tile[tileX, tileY].HasUnactuatedTile && !TopSlope(Main.tile[tileX, tileY]) && !TopSlope(Main.tile[tileX, tileY - 1]) && Main.tileSolid[Main.tile[tileX, tileY].TileType] && !Main.tileSolidTop[Main.tile[tileX, tileY].TileType]) || (Main.tile[tileX, tileY - 1].IsHalfBlock && Main.tile[tileX, tileY - 1].HasUnactuatedTile)) && (!Main.tile[tileX, tileY - 1].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX, tileY - 1].TileType] || Main.tileSolidTop[Main.tile[tileX, tileY - 1].TileType] || (Main.tile[tileX, tileY - 1].IsHalfBlock && (!Main.tile[tileX, tileY - 4].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX, tileY - 4].TileType] || Main.tileSolidTop[Main.tile[tileX, tileY - 4].TileType]))) && (!Main.tile[tileX, tileY - 2].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX, tileY - 2].TileType] || Main.tileSolidTop[Main.tile[tileX, tileY - 2].TileType]) && (!Main.tile[tileX, tileY - 3].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX, tileY - 3].TileType] || Main.tileSolidTop[Main.tile[tileX, tileY - 3].TileType]) && (!Main.tile[tileX - dir, tileY - 3].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX - dir, tileY - 3].TileType]))
                        {
                            float tilePosY = tileY * 16;
                            if (Main.tile[tileX, tileY].IsHalfBlock)
                                tilePosY += 8f;

                            if (Main.tile[tileX, tileY - 1].IsHalfBlock)
                                tilePosY -= 8f;

                            if (tilePosY < futurePos.Y + (float)npc.height)
                            {
                                float difference = futurePos.Y + (float)npc.height - tilePosY;
                                if (difference <= 16.1f)
                                {
                                    npc.gfxOffY += npc.position.Y + (float)npc.height - tilePosY;
                                    npc.position.Y = tilePosY - (float)npc.height;
                                }

                                if (difference < 9f)
                                    npc.stepSpeed = 1f;
                                else
                                    npc.stepSpeed = 2f;
                            }

                        }
                    }
                }
            }
        }
        public void RogueSpiderAI(NPC npc, float speedCap, float acceleration, int passiveRoamCooldown, int passiveRoamTime, int boredomTime, float homeRadius)
        {
            Entity target = GetTarget(npc, false, false);
            npc.stairFall = true;

            Vector2 homePos = new Vector2(npc.ai[2], npc.ai[3]);
            if (homePos == Vector2.Zero)
            {
                homePos = npc.Center;
                npc.ai[2] = npc.Center.X;
                npc.ai[3] = npc.Center.Y;
            }

            if (target != null)
            {
                if (npc.ai[1] != 2)
                {
                    if (Collision.CanHit(target.Center, 1, 1, npc.Center, 1, 1) && (npc.Center - target.Center).Length() <= homeRadius * 6)
                    {
                        npc.ai[1] = 2;
                        npc.ai[0] = 1;
                    }
                }
                if (npc.ai[1] == 2)
                {
                    if (!Collision.CanHit(target.Center, 1, 1, npc.Center, 1, 1))
                    {
                        npc.ai[0]++;
                        if (npc.ai[0] >= boredomTime)
                        {
                            npc.ai[1] = 1;
                        }
                    }
                    else
                    {
                        npc.ai[0] = 1;
                    }
                }
            }
            else
            {
                npc.ai[0] = 0;
            }

            if (npc.ai[1] != 2)
            {
                if ((npc.Center - homePos).Length() <= homeRadius)
                {
                    npc.ai[1] = 0;
                }
                else
                {
                    npc.ai[1] = 1;
                }
            }
            
            if (npc.ai[1] == 2)
            {
                if (target != null)
                {
                    npc.velocity += (target.Center - npc.Center).SafeNormalize(Vector2.UnitX) * acceleration;
                }

                if (npc.collideX && ((npc.oldVelocity.X < 0 && (target.Center.X - npc.Center.X) < 0) || (npc.oldVelocity.X > 0 && (target.Center.X - npc.Center.X) > 0)))
                {
                    npc.velocity.X = 1f * (npc.oldVelocity.X / Math.Abs(npc.oldVelocity.X)) * -speedCap;
                }
                if (npc.collideY && ((npc.oldVelocity.Y < 0 && (target.Center.Y - npc.Center.Y) < 0) || (npc.oldVelocity.Y > 0 && (target.Center.Y - npc.Center.Y) > 0)))
                {
                    npc.velocity.Y = 1f * (npc.oldVelocity.Y / Math.Abs(npc.oldVelocity.Y)) * -speedCap;
                }
            }
            else if (npc.ai[1] == 1)
            {
                if ((npc.Center - homePos).Length() > homeRadius * 4)
                {
                    npc.ai[2] = npc.Center.X;
                    npc.ai[3] = npc.Center.Y;
                    homePos = npc.Center;
                    npc.ai[1] = 0;
                }
                else
                {
                    npc.velocity += (homePos - npc.Center).SafeNormalize(Vector2.UnitX) * acceleration;
                    npc.ai[0] -= 0.5f;
                    if (npc.ai[0] < 0)
                    {
                        npc.ai[2] = npc.Center.X;
                        npc.ai[3] = npc.Center.Y;
                        homePos = npc.Center;
                        npc.ai[1] = 0;
                    }
                    else
                    {
                        if (npc.collideX && ((npc.velocity.X < 0 && (homePos.X - npc.Center.X) < 0) || (npc.velocity.X > 0 && (homePos.X - npc.Center.X) > 0)))
                        {
                            npc.velocity.X = 1f * (npc.velocity.X / -Math.Abs(npc.velocity.X)) * speedCap;
                        }
                        if (npc.collideY && ((npc.velocity.Y < 0 && (homePos.Y - npc.Center.Y) < 0) || (npc.velocity.Y > 0 && (homePos.Y - npc.Center.Y) > 0)))
                        {
                            npc.velocity.Y = 1f * (npc.velocity.Y / -Math.Abs(npc.velocity.Y)) * speedCap;
                        }

                    }
                }
            }
            else if (npc.ai[1] == 0)
            {
                if (npc.ai[0] >= 0)
                {
                    npc.ai[0] = -passiveRoamCooldown - passiveRoamTime;
                }

                if (npc.ai[0] >= -passiveRoamTime)
                {
                    float direction = npc.velocity.ToRotation();
                    if (npc.ai[0] == -passiveRoamTime)
                    {
                        direction = Main.rand.NextFloat(MathHelper.TwoPi);
                    }

                    npc.velocity += (Vector2.UnitX * acceleration).RotatedBy(direction);
                }
                else
                {
                    npc.velocity *= (1 - acceleration);
                }
                npc.ai[0]++;
            }

            if (npc.velocity.Length() > speedCap)
            {
                npc.velocity = npc.velocity.SafeNormalize(Vector2.UnitX) * speedCap;
            }
        }

        public void UpdateWormSegments(ref List<WormSegment> segments, NPC npc)
        {
            for (int i = 0; i < segments.Count; i++)
            {
                WormSegment segment = segments[i];
                segment.OldPosition = segment.Position;
                segment.OldRotation = segment.Rotation;
                if (i == 0)
                {
                    segment.Position += npc.velocity;
                    segment.Rotation = npc.rotation;
                    continue;
                }

                WormSegment oldSeg = segments[i - 1];

                segment.Position = oldSeg.Position - (Vector2.UnitX * oldSeg.Height).RotatedBy(oldSeg.Rotation.AngleLerp((oldSeg.Position - segment.Position).ToRotation(), 0.95f));

                Vector2 difference = oldSeg.Position - segment.Position;

                segment.Rotation = (difference).ToRotation();
            }
        }

        #endregion

        public override bool InstancePerEntity => true;
        public override void OnSpawn(NPC npc, IEntitySource source)
        {
            whoAmI = npc.whoAmI;
        }
        public override bool PreAI(NPC npc)
        {
            if (ballAndChainSlow > 0) // grant slowed velocity back as an attempt to make the ai run normall as if it was going full speed
            {
                npc.velocity /= 0.7f;
                ballAndChainSlow--;
            }
            return true;
        }
        public override void PostAI(NPC npc)
        {
            if (ignitedStacks != null && ignitedStacks.Any()) // ignite debuff logic
            {
                if (ignitedHitCooldown <= 0)
                {
                    int hitDamage = 0;
                    int targetDamage = (int)(npc.lifeMax * 0.01f);
                    int damageCap = 50;
                    int owner = -1;
                    if (targetDamage > damageCap)
                        targetDamage = damageCap;
                    else if (targetDamage < 1)
                        targetDamage = 1;

                    for (int i = 0; i < ignitedStacks.Count; i++)
                    {
                        if (ignitedStacks[i].DamageToDeal < targetDamage)
                        {
                            hitDamage += ignitedStacks[i].DamageToDeal;
                            ignitedStacks[i].DamageToDeal = 0;
                        }
                        else
                        {
                            hitDamage += targetDamage;
                            ignitedStacks[i].DamageToDeal -= targetDamage;
                        }
                        if (i == ignitedStacks.Count - 1)
                        {
                            owner = ignitedStacks[i].Owner;
                        }
                    }
                    IgniteHit(hitDamage, npc, owner);
                    ignitedStacks.RemoveAll(x => x.DamageToDeal <= 0);
                }
            }
            if (ignitedHitCooldown > 0)
                ignitedHitCooldown--;

            if (bleedingStacks != null && bleedingStacks.Any()) // bleeding debuff logic
            {
                if (bleedingHitCooldown <= 0)
                {
                    int hitDamage = 0;
                    int targetDamage = 40;
                    int owner = -1;

                    for (int i = 0; i < bleedingStacks.Count; i++)
                    {
                        if (bleedingStacks[i].DamageToDeal < targetDamage)
                        {
                            hitDamage += ignitedStacks[i].DamageToDeal;
                            bleedingStacks[i].DamageToDeal = 0;
                        }
                        else
                        {
                            hitDamage += targetDamage;
                            bleedingStacks[i].DamageToDeal -= targetDamage;
                        }
                        if (i == bleedingStacks.Count - 1)
                        {
                            owner = bleedingStacks[i].Owner;
                        }
                    }
                    BleedingHit(hitDamage, npc, owner);
                    bleedingStacks.RemoveAll(x => x.DamageToDeal <= 0);
                }
            }
            if (bleedingHitCooldown > 0)
                bleedingHitCooldown--;

            if (ballAndChainSlow > 0) // slow down
            {
                npc.velocity *= 0.7f;
                ballAndChainSlow--;
            }

            if (hostileTurnedAlly)
            {
                if (friendlyFireHitCooldown == 0)
                {
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC target = Main.npc[i];
                        if (!target.active)
                            continue;
                        if (!target.GetGlobalNPC<TerRoguelikeGlobalNPC>().CanBeChased(false, false))
                            continue;

                        if (npc.getRect().Intersects(target.getRect()))
                        {
                            NPC.HitInfo info = new NPC.HitInfo();
                            info.HideCombatText = true;
                            info.Damage = npc.damage;
                            info.InstantKill = false;
                            info.HitDirection = 1;
                            info.Knockback = 0f;
                            info.Crit = false;

                            target.StrikeNPC(info);
                            NetMessage.SendStrikeNPC(target, info);
                            CombatText.NewText(target.getRect(), Color.Orange, npc.damage);
                            friendlyFireHitCooldown += 20;
                        }
                    }
                }
                else if (friendlyFireHitCooldown > 0)
                    friendlyFireHitCooldown--;
                
            }
        }
        public void IgniteHit(int hitDamage, NPC npc, int owner)
        {
            TerRoguelikePlayer modPlayer = Main.player[owner].GetModPlayer<TerRoguelikePlayer>();

            hitDamage = (int)(hitDamage * modPlayer.GetBonusDamageMulti(npc, npc.Center));

            if (npc.life - hitDamage <= 0)
            {
                modPlayer.OnKillEffects(npc);
            }
            NPC.HitInfo info = new NPC.HitInfo();
            info.HideCombatText = true;
            info.Damage = hitDamage;
            info.InstantKill = false;
            info.HitDirection = Main.rand.NextBool() ? -1 : 1;
            info.Knockback = 0f;
            info.Crit = false;

            npc.StrikeNPC(info);
            NetMessage.SendStrikeNPC(npc, info);
            CombatText.NewText(npc.getRect(), Color.DarkKhaki, hitDamage);
            ignitedHitCooldown += 10; // hits 6 times a second
        }
        public void BleedingHit(int hitDamage, NPC npc, int owner)
        {
            TerRoguelikePlayer modPlayer = Main.player[owner].GetModPlayer<TerRoguelikePlayer>();

            hitDamage = (int)(hitDamage * modPlayer.GetBonusDamageMulti(npc, npc.Center));

            if (npc.life - hitDamage <= 0)
            {
                modPlayer.OnKillEffects(npc);
            }
            NPC.HitInfo info = new NPC.HitInfo();
            info.HideCombatText = true;
            info.Damage = hitDamage;
            info.InstantKill = false;
            info.HitDirection = Main.rand.NextBool() ? -1 : 1;
            info.Knockback = 0f;
            info.Crit = false;

            npc.StrikeNPC(info);
            NetMessage.SendStrikeNPC(npc, info);
            CombatText.NewText(npc.getRect(), Color.MediumVioletRed, hitDamage);
            bleedingHitCooldown = 20; // hits 3 times a second
        }
        public void AddBleedingStackWithRefresh(BleedingStack stack)
        {
            if (bleedingStacks.Any())
            {
                for (int i = 0; i < bleedingStacks.Count; i++)
                {
                    if (bleedingStacks[i].DamageToDeal < stack.DamageToDeal)
                        bleedingStacks[i].DamageToDeal = stack.DamageToDeal;
                }
            }
            else
                bleedingHitCooldown += 20;

            bleedingStacks.Add(stack);
        }
        public override bool PreKill(NPC npc)
        {
            if (!isRoomNPC)
                return true;

            //stop all room npcs from dropping shit at all 

            var AllLoadedItemIDs = new int[ItemLoader.ItemCount];
            for (int i = 0; i < ItemLoader.ItemCount; i++)
            {
                AllLoadedItemIDs[i] = i;
            }
            foreach (int itemID in AllLoadedItemIDs)
            {
                NPCLoader.blockLoot.Add(itemID);
            }
            npc.value = 0;
            
            return true;
        }
        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            if (TerRoguelikeWorld.IsTerRoguelikeWorld)
            {
                modifiers.DefenseEffectiveness *= 0f;
            }
        }
        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (ignitedStacks != null && ignitedStacks.Any() && !OverrideIgniteVisual)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);


                Texture2D texture = TextureAssets.Npc[npc.type].Value;
                Color color = Color.Lerp(Color.Yellow, Color.OrangeRed, Main.rand.NextFloat(0.4f, 0.6f + float.Epsilon) + 0.2f + (0.2f * (float)Math.Cos((Main.GlobalTimeWrappedHourly * 20f)))) * 0.8f;
                Vector3 colorHSL = Main.rgbToHsl(color);
                float outlineThickness = 1f;
                Vector2 vector = new Vector2(npc.frame.Width / 2f, npc.frame.Height / 2f);
                SpriteEffects spriteEffects = npc.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

                GameShaders.Misc["TerRoguelike:BasicTint"].UseOpacity(1f);
                GameShaders.Misc["TerRoguelike:BasicTint"].UseColor(Main.hslToRgb(1 - colorHSL.X, colorHSL.Y, colorHSL.Z));
                GameShaders.Misc["TerRoguelike:BasicTint"].Apply();

                Vector2 position = GetDrawCenter(npc) + (Vector2.UnitY * npc.gfxOffY);
                for (float i = 0; i < 1; i += 0.125f)
                {
                    spriteBatch.Draw(texture, position + (i * MathHelper.TwoPi + npc.rotation).ToRotationVector2() * outlineThickness - Main.screenPosition, npc.frame, color, npc.rotation, vector, npc.scale, spriteEffects, 0f);
                }

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            }

            if (bleedingStacks != null && bleedingStacks.Any())
            {
                DrawRotatlingBloodParticles(false, npc);
            }

            return true;
        }
        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (ignitedStacks != null && ignitedStacks.Any())
            {
                drawColor = Color.Lerp(Color.White, Color.OrangeRed, 0.4f);
                for (int i = 0; i < ignitedStacks.Count; i++)
                {
                    if (Main.rand.NextBool(5))
                        Dust.NewDust(npc.position, npc.width, npc.height, DustID.Torch);
                }
            }
            if (ballAndChainSlow > 0)
            {
                drawColor = drawColor.MultiplyRGB(Color.LightGray);
                Dust.NewDust(npc.BottomLeft + new Vector2(0, -4f), npc.width, 1, DustID.t_Slime, newColor: Color.Gray, Scale: 0.5f);
            }
        }
        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (bleedingStacks != null && bleedingStacks.Any())
            {
                DrawRotatlingBloodParticles(true, npc);
                
            }
        }
        /// <summary>
        /// Drawns top half behind npcs and bottom half in front of npcs.
        /// </summary>
        public void DrawRotatlingBloodParticles(bool inFront, NPC npc)
        {
            Texture2D texture = ModContent.Request<Texture2D>("TerRoguelike/Projectiles/AdaptiveGunBullet").Value;
            Color color = Color.Red * 0.8f;
            Vector2 position = GetDrawCenter(npc) + (Vector2.UnitY * npc.gfxOffY);

            for (int i = 0; i < bleedingStacks.Count; i++)
            {
                Vector2 specificPosition = position;
                float rotation = MathHelper.Lerp(0, MathHelper.TwoPi, Main.GlobalTimeWrappedHourly * 1.5f);
                float rotationCompletionOffset = MathHelper.TwoPi / bleedingStacks.Count * i;
                rotation += rotationCompletionOffset;
                specificPosition += new Vector2(0, 16).RotatedBy(rotation);
                specificPosition += (specificPosition - position) * new Vector2((npc.frame.Width * npc.scale) / 32f, -0.5f) ;
                if (specificPosition.Y >= position.Y && inFront)
                    Main.EntitySpriteDraw(texture, specificPosition - Main.screenPosition, null, color, 0f, texture.Size() * 0.5f, 1f, SpriteEffects.None);
                else if (!inFront)
                    Main.EntitySpriteDraw(texture, specificPosition - Main.screenPosition, null, color, 0f, texture.Size() * 0.5f, 1f, SpriteEffects.None);
            }
        }
        /// <summary>
        /// A sorrowful attempt at getting the sprite center of every npc in the game for proper visuals
        /// </summary>
        public Vector2 GetDrawCenter(NPC npc)
        {
            if (drawCenter != new Vector2(-1000))
                return drawCenter + npc.Center;

            Vector2 position = npc.Center;
            switch (npc.aiStyle)
            {
                case NPCAIStyleID.Fighter:
                    position = npc.VisualPosition + new Vector2(npc.width / 2, npc.height) + new Vector2(0f, (-npc.frame.Height / 2f + 4) * npc.scale);
                    break;
                case NPCAIStyleID.Slime:
                    position = npc.VisualPosition + new Vector2(npc.width / 2, npc.height) + new Vector2(0f, (-npc.frame.Height / 2f + 4) * npc.scale);
                    break;
                case NPCAIStyleID.KingSlime:
                    position = npc.VisualPosition + new Vector2(npc.width / 2, npc.height) + new Vector2(0f, (-npc.frame.Height / 2f + 4) * npc.scale);
                    break;
                case NPCAIStyleID.TeslaTurret:
                    position += new Vector2(-2, 2);
                    break;
                case NPCAIStyleID.Flying:
                    position += new Vector2(0, 5);
                    break;
                case NPCAIStyleID.HoveringFighter:
                    position += new Vector2(0, 5);
                    break;
                case NPCAIStyleID.EnchantedSword:
                    if (npc.type == NPCID.CursedHammer)
                        position += new Vector2(0, 2);
                    if (npc.type == NPCID.CrimsonAxe)
                        position += new Vector2(0, -9);
                    break;
            }
            return position;
        }
        /// <summary>
        /// Whether this npc can be chased with the given conditions.
        /// chaseFriendly == null: neutral. both friendly and hostile can pass
        /// chaseFriendly == true: only passes if friendly
        /// chaseFriendly == false: only passes if not friendly
        /// </summary>
        /// <param name="ignoreDontTakeDamage"></param>
        /// <param name="chaseFriendly"></param>
        /// <returns>true if passing, else false</returns>
        public bool CanBeChased(bool ignoreDontTakeDamage = false, bool? chaseFriendly = false)
        {
            NPC npc = Main.npc[whoAmI];
            bool allianceCheck = chaseFriendly == null ? true : (chaseFriendly == true ? npc.friendly : !npc.friendly);
            if (npc.active && npc.chaseable && npc.lifeMax > 5 && (!npc.dontTakeDamage || ignoreDontTakeDamage) && allianceCheck)
            {
                return !npc.immortal;
            }
            return false;
        }
        /// <summary>
        /// Basic finding target behaviour. if friendly, targets hostile npcs. if not friendly, targets players.
        /// </summary>
        /// <param name="npc"></param>
        /// <param name="resetDir"></param>
        /// <param name="resetSpriteDir"></param>
        /// <returns></returns>
        public Entity GetTarget(NPC npc, bool resetDir = false, bool resetSpriteDir = false)
        {
            if (targetPlayer != -1)
            {
                if (!Main.player[targetPlayer].active || Main.player[targetPlayer].dead || npc.friendly)
                {
                    targetPlayer = -1;
                }
            }
            if (targetNPC != -1)
            {
                if (!Main.npc[targetNPC].GetGlobalNPC<TerRoguelikeGlobalNPC>().CanBeChased(false, false) || !npc.friendly)
                {
                    
                    targetNPC = -1;
                }
            }

            if (npc.friendly)
            {
                if (targetNPC == -1 || targetPlayer != -1)
                {
                    targetNPC = ClosestNPC(npc.Center, 3200f, false);
                    targetPlayer = -1;
                    if (resetDir)
                        npc.direction = 1;
                    if (resetSpriteDir)
                        npc.spriteDirection = 1;
                }
            }
            else
            {
                if (targetPlayer == -1 || targetNPC != -1)
                {
                    targetPlayer = npc.FindClosestPlayer();
                    if (Main.player[targetPlayer].dead)
                        targetPlayer = -1;

                    targetNPC = -1;
                    if (resetDir)
                        npc.direction = 1;
                    if (resetSpriteDir)
                        npc.spriteDirection = 1;
                }
            }
            return targetPlayer != -1 ? Main.player[targetPlayer] : (targetNPC != -1 ? Main.npc[targetNPC] : null);
        }

        public void SetUpNPCProj(NPC npc, int proj)
        {
            Main.projectile[proj].GetGlobalProjectile<TerRoguelikeGlobalProjectile>().npcOwner = npc.whoAmI;
            Main.projectile[proj].GetGlobalProjectile<TerRoguelikeGlobalProjectile>().npcOwnerType = npc.type;
            if (hostileTurnedAlly || npc.friendly)
            {
                Main.projectile[proj].friendly = true;
                Main.projectile[proj].hostile = false;
            }
            else
            {
                Main.projectile[proj].friendly = false;
                Main.projectile[proj].hostile = true;
                Main.projectile[proj].damage /= 2;
            }
        }
    }

    public class BallAndChain
    {
        public BallAndChain(Vector2 position, int width, int height, float rotation)
        {
            Position = position;
            Width = width;
            Height = height;
            Rotation = rotation;
        }

        public Vector2 Position;
        public int Width;
        public int Height;
        public float Rotation;
        public Vector2 Center { get { return Position + new Vector2(Width * 0.5f, Height * 0.5f); } }
    }
    public class WormSegment
    {
        public WormSegment(Vector2 position, float rotation = 0f, float height = 1)
        {
            Position = position;
            OldPosition = position;
            Rotation = rotation;
            OldRotation = rotation;
            Height = height;
        }
        public Vector2 Position = Vector2.Zero;
        public Vector2 OldPosition = Vector2.Zero;
        public float Rotation = 0;
        public float OldRotation = 0;
        public float Height = 1f;
    }

    public class IgnitedStack
    {
        public IgnitedStack(int damageToDeal, int owner)
        {
            Owner = owner;
            damageToDeal *= Main.player[owner].GetModPlayer<TerRoguelikePlayer>().forgottenBioWeapon + 1;
            DamageToDeal = damageToDeal;
        }
        public int DamageToDeal = 0;
        public int Owner = -1;
    }
    public class BleedingStack
    {
        public BleedingStack(int damageToDeal, int owner)
        {
            Owner = owner;
            damageToDeal *= Main.player[owner].GetModPlayer<TerRoguelikePlayer>().forgottenBioWeapon + 1;
            DamageToDeal = damageToDeal;
        }
        public int DamageToDeal = 0;
        public int Owner = -1;
    }
}
