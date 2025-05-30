﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Default;
using TerRoguelike.MainMenu;
using TerRoguelike.Managers;
using TerRoguelike.NPCs.Enemy.Boss;
using TerRoguelike.Projectiles;
using TerRoguelike.Schematics;
using TerRoguelike.Systems;
using TerRoguelike.TerPlayer;
using TerRoguelike.World;
using Terraria.Graphics.Effects;
using static TerRoguelike.Managers.TextureManager;
using static TerRoguelike.Systems.RoomSystem;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using Terraria.Audio;
using Terraria.ModLoader.IO;
using System.IO;
using Terraria.GameContent.UI.States;
using TerRoguelike.Packets;
using Steamworks;
using Terraria.Localization;

namespace TerRoguelike.NPCs
{
    public class TerRoguelikeGlobalNPC : GlobalNPC
    {
        #region Variables
        public static bool anyPuppets
        {
            get
            {
                foreach (NPC npc in Main.ActiveNPCs)
                {
                    if (!npc.friendly)
                        continue;
                    var modNPC = npc.ModNPC();
                    if (!modNPC.hostileTurnedAlly || !modNPC.activatedPuppeteersHand)
                        continue;
                    return true;
                }
                return false;
            }
        }

        public bool TerRoguelikeBoss = false;
        public bool isRoomNPC = false;
        public int sourceRoomListID = -1;
        public bool eliteNamed = false;
        public bool hostileTurnedAlly = false;
        public int puppetOwner = -1;
        public bool IgnoreRoomWallCollision = false;
        public Vector2 RoomWallCollisionShrink = Vector2.Zero;
        public bool SpecialProjectileCollisionRules = false; // Makes certain things happen on the attacking projectile rather than the closest point in the NPC rect to the projectile
        public Rectangle? specialAllSeeingEyeHoverBox = null;
        public bool ignoreForRoomClearing = false;
        public int baseMaxHP = 0;
        public int baseDamage = 0;
        public float diminishingDR = 0;
        public List<WormSegment> Segments = [];
        public List<Vector2> ExtraIgniteTargetPoints = [];
        public int hitSegment = 0;
        public bool scalingApplied = false;
        public int packetCooldown = 0;
        public float effectiveDamageTakenMulti 
        { 
            get 
            {
                float finalDR = diminishingDR + (AdaptiveArmorEnabled ? AdaptiveArmor : 0);
                return finalDR == 0 ? 1f : (finalDR > 0 ? (100f / (100f + finalDR)) : 2 - (100f / (100f - finalDR))); 
            } 
        }
        public int overheadArrowTime = 0;
        public bool AdaptiveArmorEnabled = false;
        public float AdaptiveArmorCap = 400;
        public float AdaptiveArmorAddRate = 20;
        public float AdaptiveArmorDecayRate = 60;
        public float AdaptiveArmor = 0;
        public int currentUpdate = 1;
        public int maxUpdates = 1;
        public bool drawAfterEverything = false;
        public bool drawBeforeWalls = false;
        public bool drawingBeforeWallsCurrently = false;
        public int puppetLifetime = 0;

        //On kill bools to not let an npc somehow proc it more than once on death.
        public bool activatedHotPepper = false;
        public bool activatedSoulstealCoating = false;
        public bool activatedAmberBead = false;

        public bool activatedThrillOfTheHunt = false;
        public bool activatedClusterBombSatchel = false;
        public bool activatedDisposableTurret= false;

        public bool activatedSteamEngine = false;
        public bool activatedNutritiousSlime = false;
        public bool activatedItemPotentiometer = false;
        public bool activatedPuppeteersHand = false;

        public bool activatedJstc = false;
        public bool shouldHaveDied = false;

        //debuffs
        public List<IgnitedStack> ignitedStacks = [];
        public int ignitedHitCooldown = 0;
        public List<BleedingStack> bleedingStacks = [];
        public int bleedingHitCooldown = 0;
        public int ballAndChainSlow = 0;
        public bool ballAndChainSlowApplied = false;
        public Vector2 drawCenter = new Vector2(-1000);
        public int whoAmI;
        public int targetPlayer = -1;
        public int targetNPC = -1;
        public int targetCooldown = 0;
        public int[] friendlyFireHitCooldown = new int[Main.maxNPCs];
        public bool OverrideIgniteVisual = false;
        public bool IgniteCentered = false;
        public int sluggedTime = 0;
        public bool sluggedSlowApplied;

        //elites
        public EliteVars eliteVars = new();
        public bool sluggedEliteSlowApplied = false;
        public class EliteVars
        {
            public EliteVars(EliteVars flags)
            {
                tainted = flags.tainted;
                slugged = flags.slugged;
                burdened = flags.burdened;
                
            }
            public EliteVars()
            {
                tainted = false;
                slugged = false;
                burdened = false;
            }
            public bool tainted; // kb immunity, extra update, lesser adaptive armor
            public bool slugged; // kb immunity, slower, inflicts slowness, more damage, innate armor, adaptive armor
            public bool burdened; // kb immunity, spreads residual burden, adaptive armor
        }
        #endregion

        #region Base AIs
        public void RogueFighterAI(NPC npc, float xCap, float jumpVelocity, float acceleration = 0.07f)
        {
            Entity target = GetTarget(npc);

            if (target == null && npc.direction == 0)
            {
                npc.direction = 1;
                npc.spriteDirection = 1;
            }

            bool LoSBoredomCheck = target == null ? false : (Math.Abs(npc.Center.X - target.Center.X) < 96 && !CanHitInLine(npc.Center, target.Center));
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
                if (!LoSBoredomCheck)
                {
                    npc.direction *= -1;
                    npc.spriteDirection *= -1;
                }
                else
                {
                    npc.direction = Main.rand.NextBool() ? -1 : 1;
                    npc.spriteDirection = npc.direction;
                }
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

            if (npc.collideX || LoSBoredomCheck)
            {
                npc.ai[0]++;
                if (npc.collideX && npc.collideY && npc.oldVelocity.Y >= 0)
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
                        int padding = (int)(6 * (jumpVelocity / -7.9f));
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
                    for (int i = bottomtilepointY; i < bottomtilepointY + fluff; i++)
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
        public void RogueFighterShooterAI(NPC npc, float xCap, float jumpVelocity, float attackDistance, int attackTelegraph, int attackCooldown, float speedMultiWhenShooting, int projType, float projSpeed, Vector2 projOffset, int projDamage, bool LoSRequired, bool canJumpShoot = true, float? projVelocityDirectionOverride = null, int extendedAttackSlowdownTime = 0, int projectileCount = 1, float projMaxSpread = 0, float maxVelocityDeviation = 0, int jumpDetectionPadding = 6, bool canShootFall = true)
        {
            Entity target = GetTarget(npc);

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
                if (npc.ai[1] == 0 && npc.ai[0] >= 0 && (canJumpShoot || npc.velocity.Y == 0) && (npc.Center - target.Center).Length() <= attackDistance && (!LoSRequired || CanHitInLine(npc.Center + projOffset, target.Center)))
                {
                    npc.ai[1]++;
                }

                if (npc.ai[1] == attackTelegraph)
                {
                    Vector2 projSpawnPos = npc.Center + projOffset;
                    Vector2 velocityDirection = ((projVelocityDirectionOverride == null ? (target.Center - projSpawnPos).SafeNormalize(Vector2.UnitY) : Vector2.UnitX.RotatedBy((double)projVelocityDirectionOverride)));
                    for (int i = 0; i < projectileCount; i++)
                    {
                        Vector2 velocity = velocityDirection * (projSpeed + (maxVelocityDeviation == 0 ? 0 : Main.rand.NextFloat(-maxVelocityDeviation, maxVelocityDeviation + float.Epsilon)));
                        if (!TerRoguelike.mpClient)
                            Projectile.NewProjectile(npc.GetSource_FromThis(), projSpawnPos, projMaxSpread == 0 ? velocity : velocity.RotatedBy(Main.rand.NextFloat(-projMaxSpread, projMaxSpread + float.Epsilon)), projType, projDamage, 0f);
                    }
                }

                if (npc.ai[1] >= attackTelegraph + extendedAttackSlowdownTime)
                {
                    npc.ai[1] = -attackCooldown;
                }
            }

            bool LoSBoredomCheck = target == null ? false : (LoSRequired && Math.Abs(npc.Center.X - target.Center.X) < 96 && !CanHitInLine(npc.Center + projOffset, target.Center));
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

                if (!LoSBoredomCheck)
                {
                    npc.direction *= -1;
                    npc.spriteDirection *= -1;
                }
                else
                {
                    npc.direction = Main.rand.NextBool() ? -1 : 1;
                    npc.spriteDirection = npc.direction;
                }
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

            if (npc.collideX || LoSBoredomCheck)
            {
                npc.ai[0]++;
                if (npc.collideY && npc.oldVelocity.Y >= 0 && npc.collideX)
                    npc.velocity.Y = jumpVelocity;
            }
            else if (npc.ai[0] > 0)
                npc.ai[0] = 0f;

            if (target != null)
            {
                if (npc.velocity.Y == 0f && target.Bottom.Y < npc.Top.Y && Math.Abs(npc.Center.X - target.Center.X) < (float)(target.width * 3) && Collision.CanHit(npc, target))
                {

                    if (npc.velocity.Y == 0f && (canJumpShoot || npc.ai[1] <= 0))
                    {
                        int padding = jumpDetectionPadding;
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
                else if (npc.velocity.Y == 0f && target.Top.Y > npc.Bottom.Y && Math.Abs(npc.Center.X - target.Center.X) < (float)(target.width * 3) && Collision.CanHit(npc, target) && (canShootFall || npc.ai[1] <= 0))
                {
                    int fluff = 6;
                    int bottomtilepointx = (int)(npc.Center.X / 16f);
                    int bottomtilepointY = (int)(npc.Bottom.Y / 16f);
                    for (int i = bottomtilepointY; i < bottomtilepointY + fluff; i++)
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
            Entity target = GetTarget(npc);

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
                if (!TerRoguelike.mpClient)
                    Projectile.NewProjectile(npc.GetSource_FromThis(), projPos, projVel, projType, projDamage, 0f, -1, target != null ? target.whoAmI : -1, targetPlayer != -1 ? 1 : (targetNPC != -1 ? 2 : 0));
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
                            int npcBlockHeight = npc.height / 16 + (npc.height % 16 == 0 ? 0 : 1);

                            Point targetBlock = new Point((int)(teleportPos.X / 16f), (int)(teleportPos.Y / 16f));


                            if (Main.tile[targetBlock].IsTileSolidGround() || (!respectGravity && Main.tile[targetBlock + new Point(0, npcBlockHeight)].IsTileSolidGround()))
                            {
                                for (int y = 0; y < 25; y++)
                                {
                                    if (!Main.tile[targetBlock - new Point(0, y)].IsTileSolidGround())
                                    {
                                        if (!Main.tile[targetBlock - new Point(0, y + 1)].IsTileSolidGround())
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
                                            targetBlock += new Point(0, y);
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
                                if (room.wallActive && potentialTeleportRect != room.CheckRectWithWallCollision(potentialTeleportRect))
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
                if (!TerRoguelike.mpClient)
                {
                    npc.Bottom = teleportPos;
                    npc.netUpdate = true;
                }
            }
        }
        public void RogueTurretAI(NPC npc, int attackTelegraph, int attackCooldown, float attackDist, int projType, int projDamage, float projVelocity, Vector2 projOffset, bool LoSRequired, float? directionOverride = null, float? attackCone = null, int attackDuration = 0, int attackTimeBetween = 0)
        {
            Entity target = GetTarget(npc);

            if (target != null)
            {
                if (npc.ai[0] == 0)
                {
                    if ((!LoSRequired || CanHitInLine(npc.Center + projOffset, target.Center) && (npc.Center - target.Center).Length() <= attackDist && (attackCone == null || Math.Abs(AngleSizeBetween((target.Center - npc.Center).ToRotation(), (float)directionOverride)) <= attackCone * 0.5f)))
                    {
                        npc.ai[0]++;
                    }
                }
                else
                {
                    npc.ai[0]++;
                    if (attackDuration > 0 ? npc.ai[0] >= attackTelegraph && (npc.ai[0] - attackTelegraph) % attackTimeBetween == 0 : npc.ai[0] == attackTelegraph)
                    {
                        float direction = directionOverride == null ? (target.Center - (npc.Center + projOffset)).ToRotation() : (float)directionOverride;
                        if (!TerRoguelike.mpClient)
                            Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center + projOffset, Vector2.UnitX.RotatedBy(direction) * projVelocity, projType, projDamage, 0f);
                        if (attackDuration <= 0 || (attackDuration > 0 && npc.ai[0] >= attackTelegraph + attackDuration))
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
            Entity target = GetTarget(npc);

            npc.stairFall = true;
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
                    if (!TerRoguelike.mpClient)
                        Projectile.NewProjectile(npc.GetSource_FromThis(), projPos, projVel, projType, projDamage, 0f);
                }

            }

            if (npc.ai[0] < 0)
            {
                npc.noGravity = true;
                npc.noTileCollide = true;
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
            else
            {
                npc.noTileCollide = false;
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

                        burrowPos.Y = (int)(burrowPos.Y / 16) * 16f;
                        Point block = burrowPos.ToTileCoordinates();

                        bool validPos = false;
                        int validYoffset = 0;
                        int checkDirection = ParanoidTileRetrieval(block).IsTileSolidGround(true) ? -1 : 1;

                        for (int j = 0; j < 50; j++)
                        {
                            if (checkDirection == 1)
                            {
                                if (ParanoidTileRetrieval(block + new Point(0, j)).IsTileSolidGround(true))
                                {
                                    if (roomCondition)
                                    {
                                        Vector2 potentialBurrowPos = burrowPos + (Vector2.UnitY * 16 * j);
                                        Rectangle potentialBurrowRect = new Rectangle((int)potentialBurrowPos.X, (int)potentialBurrowPos.Y, 1, 1);
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
                                if (!ParanoidTileRetrieval(block + new Point(0, -j)).IsTileSolidGround(true))
                                {
                                    if (roomCondition)
                                    {
                                        Vector2 potentialBurrowPos = burrowPos + (Vector2.UnitY * -16 * j);
                                        Rectangle potentialBurrowRect = new Rectangle((int)potentialBurrowPos.X, (int)potentialBurrowPos.Y, 1, 1);
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
                            burrowPos = new Vector2(block.X, block.Y + validYoffset) * 16f;
                            break;
                        }

                        if (i == 49)
                            burrowPos = npc.Center;
                    }
                }
                if (!TerRoguelike.mpClient)
                {
                    npc.ai[2] = burrowPos.X;
                    npc.ai[3] = burrowPos.Y;
                    npc.netUpdate = true;
                }
            }
        }
        public void RogueFlyingShooterAI(NPC npc, float xCap, float yCap, float acceleration, float minAttackDist, float maxAttackDist, int attackTelegraph, int attackCooldown, int projType, float projSpeed, Vector2 projOffset, int projDamage, bool LoSRequired, float deceleration = 0.93f, int attackSuperCooldown = 0, int attacksToSuperCooldown = 0, bool ignorePlatforms = false)
        {
            Entity target = GetTarget(npc);

            npc.ai[3]++;
            npc.stairFall = true;
            if (npc.collideY && !ignorePlatforms)
            {
                int fluff = 1;
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

                if (!LoSRequired || CanHitInLine(npc.Center + projOffset, target.Center))
                    LoSCheck = true;

                if (npc.collideX)
                {
                    if (LoSCheck && !CanHitInLine(npc.Bottom, target.Bottom))
                        npc.velocity.Y += acceleration * -1;
                }

                if (npc.ai[2] == 0 && LoSCheck && distanceCheck)
                {
                    npc.ai[2]++;
                }
                else if (npc.ai[2] >= attackTelegraph)
                {
                    if (attacksToSuperCooldown > 0)
                        npc.ai[1]++;

                    npc.ai[2] = npc.ai[1] >= attacksToSuperCooldown && attacksToSuperCooldown > 0 ? -attackSuperCooldown : -attackCooldown;
                    if (attacksToSuperCooldown > 0 && npc.ai[1] >= attacksToSuperCooldown)
                        npc.ai[1] = 0;

                    if (!TerRoguelike.mpClient)
                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center + projOffset, (target.Center - npc.Center).SafeNormalize(Vector2.UnitX * npc.direction) * projSpeed, projType, projDamage, 0f);
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

                    if (Math.Abs(npc.velocity.X) > xCap)
                    {
                        npc.velocity.X *= 0.98f;
                    }
                    if (Math.Abs(npc.velocity.Y) > yCap)
                    {
                        npc.velocity.Y *= 0.98f;
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
            Entity target = GetTarget(npc);

            if (npc.ai[1] < attackCooldown && npc.ai[1] >= 0 && npc.ai[1] != attackCooldown - attackTelegraph)
                npc.ai[1]++;
            else if (npc.ai[1] == attackCooldown - attackTelegraph && npc.collideY)
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

            if (npc.ai[1] >= attackCooldown)
            {
                npc.ai[1] = -jumpTime - dashTime;
            }

            bool dashing = false;
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
                dashing = true;
                if (npc.ai[1] >= -dashTime && npc.ai[1] < 0)
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
                    if (npc.ai[1] < 0)
                        npc.stairFall = true;
                    if (npc.collideX)
                    {
                        npc.direction *= -1;
                        npc.spriteDirection *= -1;
                        npc.velocity.X = npc.oldVelocity.X * -0.8f;
                    }
                    if (npc.collideY)
                    {
                        if (npc.ai[1] < -attackTelegraph)
                            npc.velocity.Y = npc.oldVelocity.Y * -0.75f;
                        else if (npc.ai[1] < 0)
                        {
                            npc.velocity.Y = npc.oldVelocity.Y * -0.2f;
                            npc.velocity.X *= 0.5f;
                        }
                        npc.velocity.X *= 0.9f;
                    }
                    npc.ai[1]++;
                }
            }


            if (npc.collideX)
            {
                npc.ai[0]++;
                if (!dashing && npc.collideY && npc.oldVelocity.Y >= 0 && npc.collideX)
                    npc.velocity.Y = -4.4f;
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
                    for (int i = bottomtilepointY; i < bottomtilepointY + fluff; i++)
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
            Entity target = GetTarget(npc);

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
                    npc.direction = -Math.Sign(npc.velocity.X);
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
                    for (int i = bottomtilepointY; i < bottomtilepointY + fluff; i++)
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
            Entity target = GetTarget(npc);

            Vector2 targetPos = npc.Center + Vector2.UnitX.RotatedBy(-npc.rotation) * 240;
            if (target != null)
            {
                targetPos = target.Center;
            }

            float targetAngle = (targetPos - npc.Center).ToRotation();
            bool slowTurn = npc.ai[0] == 0;
            float newAngle = npc.rotation.AngleTowards(targetAngle, slowTurn ? turnRadians : turnRadians * 0.3f);
            float angleChange = AngleSizeBetween(npc.rotation, newAngle);
            if ((targetPos - npc.Center).Length() > slowTurnDist)
            {
                npc.ai[0] = 0;
            }
            if (Math.Abs(angleChange) <= turnRadians * 0.29f || (targetPos - npc.Center).Length() < slowTurnDist * 0.75f)
                npc.ai[0] = 1;

            float velMultiplier = slowTurn ? Math.Abs(Vector2.Dot((npc.rotation + MathHelper.PiOver2).ToRotationVector2(), targetAngle.ToRotationVector2())) : 0;
            npc.rotation = newAngle;
            npc.velocity = Vector2.UnitX.RotatedBy(npc.rotation) * npc.velocity.Length();
            Vector2 wantedVelocity = slowTurn ? npc.rotation.ToRotationVector2() * (maxVelocity * ((((1f - velMultiplier * 0.85f)) + 0.15f))) : npc.rotation.ToRotationVector2() * maxVelocity;
            npc.velocity = Vector2.Lerp(npc.velocity, wantedVelocity, 0.07f);
            npc.direction = npc.velocity.X > 0 ? 1 : -1;
        }
        public void RogueGiantBatAI(NPC npc, float distanceAbove, float acceleration, float maxVelocity, int attackTelegraph, int attackCooldown, float attackDistance, int projType, Vector2 projVelocity, int projDamage)
        {
            Entity target = GetTarget(npc);

            npc.stairFall = true;

            npc.ai[1]++;

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

            float magnitude = 0.5f;

            if (target != null)
            {
                Vector2 targetPos = target.Center + new Vector2(0, -distanceAbove - 1);
                targetPos = TileCollidePositionInLine(target.Center, targetPos);
                targetPos += Vector2.UnitY;
                bool canSeeTarget = CanHitInLine(target.Center, npc.Center);
                bool canSeeTargetPos = CanHitInLine(npc.Top, targetPos);
                if (!canSeeTargetPos || !canSeeTarget)
                {
                    magnitude = 1;
                    npc.velocity += Vector2.UnitY * acceleration * 0.25f * (float)Math.Cos(npc.ai[1] / 600f * MathHelper.TwoPi);
                }

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
                    if (canSeeTarget && canSeeTargetPos)
                        npc.velocity += (targetPos - npc.Center).SafeNormalize(Vector2.UnitY) * acceleration;
                    else if (canSeeTarget)
                        npc.velocity += (target.Center - npc.Center).SafeNormalize(Vector2.UnitY) * acceleration;
                    else
                        npc.velocity += new Vector2(1 * acceleration * (npc.velocity.X == 0 ? (Main.rand.NextBool() ? -1 : 1) : Math.Sign(npc.velocity.X)), 0);
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
                    if (!TerRoguelike.mpClient)
                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, projVelocity, projType, projDamage, 0f);
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
                    if (!TerRoguelike.mpClient)
                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, projVelocity, projType, projDamage, 0f);
                }

                npc.velocity.X += (npc.velocity.X == 0 ? (Main.rand.NextBool() ? 1 : -1) : Math.Sign(npc.velocity.X)) * acceleration;
                npc.velocity.Y += Main.rand.NextFloat(-acceleration / 2f, acceleration / 2f);

                if (npc.velocity.Length() > maxVelocity)
                {
                    npc.velocity = npc.velocity.SafeNormalize(Vector2.UnitX) * maxVelocity;
                }
            }

            npc.velocity += Vector2.UnitY * acceleration * magnitude * (float)Math.Cos(npc.ai[1] / 20f * MathHelper.TwoPi);

            if (npc.collideX && Math.Abs(npc.velocity.X) > 0.5f)
            {
                npc.velocity.X *= -0.75f;
            }
            if (npc.collideY)
            {
                npc.velocity.Y *= -0.25f;
            }
        }
        public void RogueFrostbiterAI(NPC npc, float distanceBeside, int dashTime, float dashVelocity, float passiveAccel, float passiveMaxVelocity, int attackTelegraph, int waitTimeAfterAttack, float attackDistance, int projType, float projVelocity, int projDamage, int projCount, bool LoSRequired = false, bool dontRelocateForProjectiles = false, float attackActivationRadius = 64f)
        {
            //ai0 is for attack timers.
            //ai1 is for attack state.
            //ai2 and ai3 store a vector, unless dontRelocateForProjectiles is true

            Entity target = GetTarget(npc);

            bool LosCheck = true;
            if (target != null)
                LosCheck = !LoSRequired || CanHitInLine(npc.Center, target.Center);

            if (dontRelocateForProjectiles)
                npc.ai[2] += 0.2f;

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
                    Vector2 nextTarget = dontRelocateForProjectiles ? npc.Center : Main.rand.NextVector2CircularEdge(attackDistance, attackDistance);
                    if (!dontRelocateForProjectiles)
                    {
                        nextTarget.X = Math.Abs(nextTarget.X) * -npc.direction;
                        nextTarget += target == null ? npc.Center : target.Center;
                        npc.ai[2] = nextTarget.X;
                        npc.ai[3] = nextTarget.Y;
                        npc.netUpdate = true;
                    }
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

            if (npc.collideX)
                npc.velocity.X = -npc.oldVelocity.X * 0.4f;
            if (npc.collideY)
                npc.velocity.Y = -npc.oldVelocity.Y * 0.4f;
            if (target != null && LosCheck)
            {
                Vector2 targetPos = npc.ai[1] == 0 ? new Vector2(distanceBeside * npc.direction, 0) + target.Center : (dontRelocateForProjectiles ? npc.Center : new Vector2(npc.ai[2], npc.ai[3]));

                float greaterDist = attackDistance > distanceBeside ? attackDistance : distanceBeside;
                if ((npc.Center - targetPos).Length() < attackActivationRadius || (npc.collideX || npc.collideY))
                {
                    if (npc.ai[0] == 0)
                        npc.ai[0]++;
                }
                else if (npc.ai[0] == 0)
                {
                    npc.velocity *= 0.99f;
                    npc.velocity += (targetPos - npc.Center).SafeNormalize(Vector2.UnitY) * ((npc.Center - targetPos).Length() < attackActivationRadius ? passiveAccel : passiveAccel * 2f);
                    if (dontRelocateForProjectiles)
                        npc.velocity.Y += (float)Math.Cos((double)npc.ai[2] * MathHelper.TwoPi) * 6 * passiveAccel;
                }
                if (npc.velocity.Length() > passiveMaxVelocity && npc.ai[0] < attackTelegraph)
                    npc.velocity = npc.velocity.SafeNormalize(Vector2.UnitY) * passiveMaxVelocity;
            }
            else if (!LosCheck)
            {
                float x = npc.velocity.X == 0 ? -0.5f : Math.Sign(npc.velocity.X) * 0.5f;
                float y = (float)Math.Cos((double)npc.ai[2] * 0.66f) * 2;
                npc.velocity += new Vector2(x, y).SafeNormalize(Vector2.UnitY) * passiveAccel;
                if (npc.velocity.Length() > passiveMaxVelocity && npc.ai[0] < attackTelegraph)
                    npc.velocity = npc.velocity.SafeNormalize(Vector2.UnitY) * passiveMaxVelocity;
            }

            if (npc.ai[0] >= attackTelegraph)
            {
                if (npc.ai[1] == 0 && npc.ai[0] == attackTelegraph)
                {
                    npc.velocity = target != null ? (target.Center - npc.Center).SafeNormalize(-Vector2.UnitX * npc.direction) * dashVelocity : -Vector2.UnitX * npc.direction * dashVelocity;
                }
                else if (npc.ai[1] == 1 && !TerRoguelike.mpClient)
                {
                    float anglePerProj = MathHelper.TwoPi / projCount;
                    if (projType != ProjectileID.None)
                    {
                        for (int i = 0; i < projCount; i++)
                        {
                            int proj = Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, (-Vector2.UnitY * projVelocity).RotatedBy(anglePerProj * i), projType, projDamage, 0f);
                        }
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
            Entity target = GetTarget(npc);

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
            Entity target = GetTarget(npc);

            if (npc.ai[2] != 2)
            {
                if (npc.ai[1] < attackCooldown)
                {
                    npc.ai[1]++;
                }
                else if (npc.ai[1] == attackCooldown && target != null)
                {
                    if (CanHitInLine(npc.Center, target.Center) && (npc.Center - target.Center).Length() <= attackDist)
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
                    bool LoSBoredomCheck = target == null ? false : (Math.Abs(npc.Center.X - target.Center.X) < 96 && !CanHitInLine(npc.Center, target.Center));
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
                        if (!LoSBoredomCheck)
                        {
                            npc.direction *= -1;
                            npc.spriteDirection *= -1;
                        }
                        else
                        {
                            npc.direction = Main.rand.NextBool() ? -1 : 1;
                            npc.spriteDirection = npc.direction;
                        }
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

                    if (npc.collideX || LoSBoredomCheck)
                    {
                        npc.ai[0]++;
                        if (npc.collideX && npc.collideY && npc.oldVelocity.Y >= 0)
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
                                int padding = (int)(6 * (jumpVelocity / -7.9f));
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
                            for (int i = bottomtilepointY; i < bottomtilepointY + fluff; i++)
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
                    if (Math.Abs(AngleSizeBetween((ball.Center - npc.Center).ToRotation(), targetAngle - (MathHelper.PiOver2 * npc.ai[2]))) < MathHelper.Pi * 0.0625f)
                    {
                        if (!TerRoguelike.mpClient)
                        {
                            npc.localAI[3] = Projectile.NewProjectile(npc.GetSource_FromThis(), ball.Center, (Vector2.UnitX * launchVelocity).RotatedBy(targetAngle + (MathHelper.PiOver4 * npc.ai[2] * 0.4f)), ModContent.ProjectileType<SpikedBall>(), damage, 0f, -1, ball.Rotation);
                            Main.projectile[(int)npc.localAI[3]].direction = (int)npc.ai[2];
                            npc.netUpdate = true;
                        }
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
                    if ((Main.projectile[(int)npc.localAI[3]].Center - npc.Center).Length() <= 12f || !Main.projectile[(int)npc.localAI[3]].active && !TerRoguelike.mpClient)
                    {
                        npc.ai[2] = 0;
                        npc.ai[1] = 0;
                    }
                }
                else
                {
                    npc.ai[1]++;
                    if (npc.ai[1] >= attackWindUpTime + attackCooldown && !Main.projectile[(int)npc.localAI[3]].active && !TerRoguelike.mpClient)
                    {
                        npc.ai[1] = 0;
                        npc.ai[2] = 0;
                    }
                }
            }
        }
        public void RogueAssasinAI(NPC npc, float xCap, float jumpVelocity, float acceleration, int teleportTelegraph, int teleportCooldown, int teleportExhaustTime, float minTeleportDist, float maxTeleportDist)
        {
            Entity target = GetTarget(npc);

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
                                    if (room.wallActive && potentialTeleportRect != room.CheckRectWithWallCollision(potentialTeleportRect))
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
                        if (!TerRoguelike.mpClient)
                        {
                            npc.Bottom = teleportPos;
                            npc.netUpdate = true;
                        }
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
                            int padding = (int)(6 * (jumpVelocity / -7.9f));
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
                        for (int i = bottomtilepointY; i < bottomtilepointY + fluff; i++)
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
            Entity target = GetTarget(npc);
            if (targetCooldown > 30 && target != null && !CanHitInLine(npc.Center, target.Center))
                targetCooldown = 30;
                
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
                    if (CanHitInLine(npc.Center, target.Center) && (npc.Center - target.Center).Length() <= homeRadius * 6)
                    {
                        npc.ai[1] = 2;
                        npc.ai[0] = 1;
                    }
                }
                if (npc.ai[1] == 2)
                {
                    if (!CanHitInLine(npc.Center, target.Center))
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
                npc.ai[0]++;
                if (npc.ai[0] >= boredomTime)
                {
                    npc.ai[1] = 1;
                }
            }

            if (npc.ai[1] != 2 || (target == null && npc.ai[1] == 2))
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

                    if (npc.collideX && ((npc.oldVelocity.X < 0 && (target.Center.X - npc.Center.X) < 0) || (npc.oldVelocity.X > 0 && (target.Center.X - npc.Center.X) > 0)))
                    {
                        npc.velocity.X = 1f * Math.Sign(npc.oldVelocity.X) * -speedCap;
                    }
                    if (npc.collideY && ((npc.oldVelocity.Y < 0 && (target.Center.Y - npc.Center.Y) < 0) || (npc.oldVelocity.Y > 0 && (target.Center.Y - npc.Center.Y) > 0)))
                    {
                        npc.velocity.Y = 1f * Math.Sign(npc.oldVelocity.Y) * -speedCap;
                    }
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
        public void RogueCrawlerAI(NPC npc, float speedCap, float acceleration, int waitTime)
        {
            Entity target = GetTarget(npc);
            if (npc.ai[2] < 0)
                npc.ai[2]++;

            if (npc.ai[0] <= 0)
            {
                if (npc.direction == 0)
                {
                    npc.direction = Main.rand.NextBool() ? -1 : 1;
                }

                if (Math.Abs(npc.velocity.X) < speedCap)
                    npc.velocity.X += acceleration * npc.direction;
                if (Math.Abs(npc.velocity.X) > speedCap)
                    npc.velocity.X = speedCap * npc.direction;

                Point targetBlock = (npc.Bottom + Vector2.UnitY).ToTileCoordinates();

                if (npc.collideX && npc.ai[2] >= 0)
                {
                    npc.ai[0]++;
                }
                else if (npc.velocity.Y == 0 && !ParanoidTileRetrieval(targetBlock).IsTileSolidGround() && !ParanoidTileRetrieval(targetBlock + new Point(0, 1)).IsTileSolidGround() && npc.ai[2] >= 0)
                {
                    npc.ai[0]++;
                }
                else if (npc.velocity.Y >= 0f)
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
            else
            {
                npc.ai[0]++;
                npc.velocity.X *= 0.8f;
                if (npc.ai[0] >= waitTime)
                {
                    npc.ai[2] = -60;
                    npc.ai[0] = 0;
                    npc.direction *= -1;
                }
            }
        }
        public void RogueCrawlerShooterAI(NPC npc, float speedCap, float acceleration, int waitTime, float attackDist, int attackTelegraph, int attackDuration, int attackTimeBetween, int attackCooldown, int projType, float projSpeed, int projDamage, float speedMultiWhenAttacking = 1f, float? projVelocityDirectionOverride = null)
        {
            Entity target = GetTarget(npc);

            if (npc.ai[1] != 0)
                npc.ai[1]++;
            if (npc.ai[2] < 0)
                npc.ai[2]++;

            if (npc.ai[0] <= 0)
            {
                if (npc.direction == 0)
                {
                    npc.direction = Main.rand.NextBool() ? -1 : 1;
                }

                float speedMulti = npc.ai[1] > 0 ? speedMultiWhenAttacking : 1f;

                if (Math.Abs(npc.velocity.X) < speedCap * speedMulti)
                    npc.velocity.X += acceleration * npc.direction;
                if (Math.Abs(npc.velocity.X) > speedCap * speedMulti)
                    npc.velocity.X = speedCap * npc.direction * speedMulti;

                Point targetBlock = (npc.Bottom + Vector2.UnitY + Vector2.UnitX * Math.Sign(npc.velocity.X)).ToTileCoordinates();

                if (npc.collideX && npc.ai[2] >= 0)
                {
                    npc.ai[0]++;
                }
                else if (npc.collideY && !Main.tile[targetBlock.X, targetBlock.Y].IsTileSolidGround() && !ParanoidTileRetrieval(targetBlock + new Point(0, 1)).IsTileSolidGround() && npc.ai[2] >= 0)
                {
                    npc.ai[0]++;
                }
                else if (npc.velocity.Y >= 0f)
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
            else
            {
                npc.ai[0]++;
                npc.velocity.X *= 0.8f;
                if (npc.ai[0] >= 60)
                {
                    npc.ai[2] = -waitTime;
                    npc.ai[0] = 0;
                    npc.direction *= -1;
                }
            }

            if (target != null)
            {
                if (npc.ai[1] == 0 && (npc.Center - target.Center).Length() <= attackDist && CanHitInLine(npc.Center, target.Center))
                {
                    npc.ai[1]++;
                }

                if (npc.ai[1] >= attackTelegraph)
                {
                    if (((int)npc.ai[1] - attackTelegraph) % attackTimeBetween == 0)
                    {
                        Vector2 direction = projVelocityDirectionOverride == null ? (target.Center - npc.Center).SafeNormalize(-Vector2.UnitY) : Vector2.UnitX.RotatedBy((float)projVelocityDirectionOverride);
                        if (!TerRoguelike.mpClient)
                            Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, direction * projSpeed, projType, projDamage, 0);
                    }

                    if (npc.ai[1] >= attackTelegraph + attackDuration)
                        npc.ai[1] = -attackCooldown;
                }
            }
            else if (npc.ai[1] > 0)
                npc.ai[1] = 0;
        }
        public void RogueTumbletwigAI(NPC npc, float speedCap, float acceleration, float attackDist, int projType, float projSpeed, int projDamage, int attackTelegraph, int attackDuration, int attackShootCooldown, int attackCooldown, float attackSpread)
        {
            // I FUCKING HAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAATE SLOOOOOOOOOOOOOOOOOOOOOOOOOOOPES FUCK YOU RED FUUUUUUUUCK YOUUUUUUUUU I SWEAR TO FUCK WHY ARE SLOPES SO FUCKING JANK. I'LL JUST NEVER FUCKING USE THEM IN MY BUILDS. (edit- slopes should work now :] )
            npc.netSpam = 0;

            Entity target = GetTarget(npc);
            //npc ai 3: 0 = right, 1 = down, 2 = left, 3 = up

            if (npc.direction == 0)
            {
                npc.direction = Main.rand.NextBool() ? -1 : 1; // 1 counterclockwise, -1 clockwise
                npc.ai[3] = 1;
            }
            if (npc.ai[2] > 0)
                npc.ai[2]--;

            if (npc.collideX || npc.collideY)
            {
                npc.noGravity = true;
            }

            if (!npc.noGravity)
            {
                npc.ai[0] = 0;
                return;
            }

            bool attacking = false;

            if (target != null)
            {
                if (npc.ai[0] > 0)
                    npc.ai[0]++;
                else if (CanHitInLine(npc.Center, target.Center) && (npc.Center - target.Center).Length() <= attackDist)
                {
                    npc.ai[0]++;
                }

                if ((npc.ai[0] - attackTelegraph) % attackShootCooldown == 0 && npc.ai[0] <= attackTelegraph + attackDuration && npc.ai[0] >= attackTelegraph)
                {
                    if (!TerRoguelike.mpClient)
                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, (target.Center - npc.Center).SafeNormalize(Vector2.UnitY).RotatedBy(Main.rand.NextFloat(-attackSpread, attackSpread + float.Epsilon)) * projSpeed, projType, projDamage, 0f);
                }
                if (npc.ai[0] > 0 && npc.ai[0] < attackTelegraph + attackDuration)
                    attacking = true;

                if (npc.ai[0] == attackTelegraph + attackDuration)
                {
                    npc.direction = Main.rand.NextBool() ? -1 : 1;
                    npc.netUpdate = true;
                }
                else if (npc.ai[0] >= attackTelegraph + attackDuration + attackCooldown)
                    npc.ai[0] = 0;
            }
            else
            {
                npc.ai[0] = 0;
            }

            if (npc.ai[3] == 1) // down collide
            {
                bool antiStuck = npc.oldVelocity.X != 0 && npc.velocity.X == 0;
                if (!attacking)
                    npc.velocity.X += acceleration * npc.direction;
                npc.velocity.Y += 16f;
                if (npc.collideX || antiStuck)
                {
                    npc.ai[3] = npc.direction == 1 ? 0 : 2;
                    npc.velocity.Y = -Math.Abs(npc.oldVelocity.X);
                    npc.ai[2] = 6;
                    npc.netUpdate = true;
                }
                else if (!npc.collideY && npc.ai[2] == 0)
                {
                    npc.ai[3] = npc.direction == 1 ? 2 : 0;
                    npc.velocity.Y = Math.Abs(npc.oldVelocity.X);
                    npc.velocity.X *= -1;
                    npc.ai[2] = 6;
                    npc.netUpdate = true;
                }
            }
            else if (npc.ai[3] == 0) // right collide
            {
                npc.velocity.X += 16f;
                if (!attacking)
                    npc.velocity.Y += -acceleration * npc.direction;
                if (npc.collideY)
                {
                    npc.ai[3] = npc.direction == 1 ? 3 : 1;
                    npc.velocity.X = -Math.Abs(npc.oldVelocity.Y);
                    npc.velocity.Y = npc.direction == 1 ? -16 : 16;
                    npc.ai[2] = 6;
                    npc.netUpdate = true;
                }
                else if (!npc.collideX && npc.ai[2] == 0)
                {
                    npc.ai[3] = npc.direction == 1 ? 1 : 3;
                    npc.velocity.X = Math.Abs(npc.oldVelocity.Y);
                    npc.velocity.Y *= -1;
                    npc.ai[2] = 6;
                    npc.netUpdate = true;
                }
            }
            else if (npc.ai[3] == 3) // up collide
            {
                bool antiStuck = npc.oldVelocity.X != 0 && npc.velocity.X == 0;
                if (!attacking)
                    npc.velocity.X += -acceleration * npc.direction;
                npc.velocity.Y += -16f;
                if (npc.collideX || antiStuck)
                {
                    npc.ai[3] = npc.direction == 1 ? 2 : 0;
                    npc.velocity.Y = Math.Abs(npc.oldVelocity.X);
                    npc.ai[2] = 6;
                    npc.netUpdate = true;
                }
                else if (!npc.collideY && npc.ai[2] == 0)
                {
                    npc.ai[3] = npc.direction == 1 ? 0 : 2;
                    npc.velocity.Y = -Math.Abs(npc.oldVelocity.X);
                    npc.velocity.X *= -1;
                    npc.ai[2] = 6;
                    npc.netUpdate = true;
                }
            }
            else if (npc.ai[3] == 2)// left collide
            {
                npc.velocity.X += -16f;
                if (!attacking)
                    npc.velocity.Y += acceleration * npc.direction;
                if (npc.collideY)
                {
                    npc.ai[3] = npc.direction == 1 ? 1 : 3;
                    npc.velocity.X = Math.Abs(npc.oldVelocity.Y);
                    npc.velocity.Y = npc.direction == 1 ? 16 : -16;
                    npc.ai[2] = 6;
                    npc.netUpdate = true;
                }
                else if (!npc.collideX && npc.ai[2] == 0)
                {
                    npc.ai[3] = npc.direction == 1 ? 3 : 1;
                    npc.velocity.X = -Math.Abs(npc.oldVelocity.Y);
                    npc.velocity.Y *= -1;
                    npc.ai[2] = 6;
                    npc.netUpdate = true;
                }
            }

            if (Math.Abs(npc.velocity.X) > speedCap)
                npc.velocity.X = speedCap * Math.Sign(npc.velocity.X);
            if (Math.Abs(npc.velocity.Y) > speedCap)
                npc.velocity.Y = speedCap * Math.Sign(npc.velocity.Y);

            if (attacking)
            {
                npc.velocity *= 0.97f;
            }
        }
        public void RogueRockGolemAI(NPC npc, float xCap, float jumpVelocity, float meleeDistance, int meleeDuration, int meleeCooldown, float attackDistance, int attackTelegraph, int attackCooldown, float speedMultiWhenAttacking, int projType, float projSpeed, Vector2 projOffset, int projDamage, bool LoSRequired, bool canJumpShoot = false, int extendedAttackSlowdownTime = 0)
        {
            Entity target = GetTarget(npc);

            float realXCap = (npc.ai[1] > 0 || npc.ai[2] > 0) ? xCap * speedMultiWhenAttacking : xCap;

            if (npc.ai[1] != 0)
                npc.ai[1]++;
            if (npc.ai[2] != 0)
                npc.ai[2]++;

            if (target == null)
            {
                if (npc.ai[1] > 0)
                    npc.ai[1] = 0;
                if (npc.ai[2] >= meleeDuration)
                    npc.ai[2] = -meleeCooldown;
            }
            else
            {
                if (npc.ai[2] <= 0)
                {
                    if (npc.ai[1] == 0 && npc.ai[0] >= 0 && (canJumpShoot || npc.velocity.Y == 0) && (npc.Center - target.Center).Length() <= attackDistance && (!LoSRequired || CanHitInLine(npc.Center + projOffset, target.Center)))
                    {
                        npc.ai[1]++;
                    }

                    if (npc.ai[1] == attackTelegraph)
                    {
                        Vector2 projSpawnPos = npc.Center + projOffset;
                        Vector2 velocityDirection = (target.Center - projSpawnPos).SafeNormalize(Vector2.UnitY);
                        Vector2 velocity = velocityDirection * projSpeed;
                        if (!TerRoguelike.mpClient)
                            Projectile.NewProjectile(npc.GetSource_FromThis(), projSpawnPos, velocity, projType, projDamage, 0f);
                    }

                    if (npc.ai[1] >= attackTelegraph + extendedAttackSlowdownTime)
                    {
                        npc.ai[1] = -attackCooldown;
                    }
                }
                if (npc.ai[1] <= 0)
                {
                    if (npc.ai[2] == 0 && (target.Center - npc.Center).Length() <= meleeDistance && (canJumpShoot || npc.velocity.Y == 0))
                    {
                        npc.ai[2]++;
                    }
                    if (npc.ai[2] >= meleeDuration)
                    {
                        npc.ai[2] = -meleeCooldown;
                    }
                }
            }

            bool LoSBoredomCheck = target == null ? false : (LoSRequired && Math.Abs(npc.Center.X - target.Center.X) < 96 && !CanHitInLine(npc.Center + projOffset, target.Center));
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
                if (!LoSBoredomCheck)
                {
                    npc.direction *= -1;
                    npc.spriteDirection *= -1;
                }
                else
                {
                    npc.direction = Main.rand.NextBool() ? -1 : 1;
                    npc.spriteDirection = npc.direction;
                }

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

            if (npc.collideX || LoSBoredomCheck)
            {
                npc.ai[0]++;
                if (npc.collideX && npc.collideY && npc.oldVelocity.Y >= 0)
                    npc.velocity.Y = jumpVelocity;
            }
            else if (npc.ai[0] > 0)
                npc.ai[0] = 0f;

            if (target != null)
            {
                if (npc.velocity.Y == 0f && target.Bottom.Y < npc.Top.Y && Math.Abs(npc.Center.X - target.Center.X) < (float)(target.width * 3) && Collision.CanHit(npc, target))
                {

                    if (npc.velocity.Y == 0f && (canJumpShoot || ((npc.ai[1] <= 0) && (npc.ai[2] <= 0))))
                    {
                        int padding = (int)(6 * (jumpVelocity / -7.9f));
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
                    for (int i = bottomtilepointY; i < bottomtilepointY + fluff; i++)
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
        public void RogueCorruptorAI(NPC npc, float speedCap, float acceleration, int attackTelegraph, int attackCooldown, int projType, float projSpeed, int projDamage)
        {
            Entity target = GetTarget(npc);

            if (npc.ai[3] > 0)
                npc.ai[3] -= npc.collideX || npc.collideY ? 10 : 1;
            else if (npc.ai[3] < 0)
                npc.ai[3] += npc.collideX || npc.collideY ? 10 : 1;
            if (npc.ai[0] != 0)
                npc.ai[0]++;

            if (npc.collideX)
            {
                npc.velocity.X = -npc.oldVelocity.X * 0.5f;
            }
            if (npc.collideY)
            {
                npc.velocity.Y = -npc.oldVelocity.Y * 0.5f;
            }

            if (target != null)
            {
                Vector2 velocityDirection = (target.Center - npc.Center);
                float angle = velocityDirection.ToRotation();
                bool LoS = CanHitInLine(npc.Center, target.Center);
                if (!LoS)
                {
                    if (npc.ai[3] == 0)
                    {
                        npc.ai[3] = 300 * (Main.rand.NextBool() ? -1 : 1);
                    }
                    int direction = npc.ai[3] > 0 ? 1 : -1;
                    angle += MathHelper.Pi * Main.rand.NextFloat(0.3f, 0.45f) * direction;
                }
                else
                {
                    npc.ai[3] = 0;
                }
                npc.velocity += (Vector2.UnitX * acceleration).RotatedBy(angle);

                if (npc.ai[0] == 0 && LoS)
                {
                    npc.ai[0]++;
                }
                if (npc.ai[0] >= attackTelegraph)
                {
                    if (!TerRoguelike.mpClient)
                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, velocityDirection.SafeNormalize(Vector2.UnitX) * projSpeed, projType, projDamage, 0);
                    npc.ai[0] = -attackCooldown;
                }
            }
            else
            {
                if (npc.ai[0] > 0)
                    npc.ai[0] = 0;

                npc.velocity += (Vector2.UnitX * acceleration).RotatedBy(npc.velocity.ToRotation());
            }
            if (npc.velocity.Length() > speedCap)
            {
                npc.velocity = npc.velocity.SafeNormalize(Vector2.UnitX) * speedCap;
            }
        }
        public void RogueClingerAI(NPC npc, float speedCap, float acceleration, Vector2 anchorPos, float maxDist, int attackTelegraph, int attackCooldown, int projType, float projSpeed, int projDamage)
        {
            Entity target = GetTarget(npc);

            if (npc.ai[0] != 0)
                npc.ai[0]++;

            Vector2 targetPos = target == null ? (npc.Center - anchorPos).SafeNormalize(-Vector2.UnitY) * maxDist + anchorPos : ((target.Center - anchorPos).Length() > maxDist ? (target.Center - anchorPos).SafeNormalize(-Vector2.UnitY) * maxDist + anchorPos : target.Center);

            if (target != null)
            {
                if (npc.ai[0] == 0 && CanHitInLine(npc.Center, target.Center))
                {
                    npc.ai[0]++;
                }

                if (npc.ai[0] >= attackTelegraph)
                {
                    if (!TerRoguelike.mpClient)
                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, (target.Center - npc.Center).SafeNormalize(-Vector2.UnitY) * projSpeed, projType, projDamage, 0f);
                    npc.ai[0] = -attackCooldown;
                }
            }
            else if (npc.ai[0] > 0)
                npc.ai[0] = 0;

            Vector2 directionVector = targetPos - npc.Center;
            npc.velocity *= 0.9925f;
            npc.velocity += (npc.Center - anchorPos).Length() > maxDist ? (anchorPos - npc.Center).SafeNormalize(-Vector2.UnitY) * acceleration : (directionVector).SafeNormalize(-Vector2.UnitY) * acceleration;
            if (npc.velocity.Length() > speedCap)
                npc.velocity = npc.velocity.SafeNormalize(-Vector2.UnitY) * speedCap;
        }
        public void RogueEvilToolAI(NPC npc, float dashSpeed, int attackTelegraph, int attackDuration, int attackTimeBetween, int attackCooldown, int projType, float projSpeed, int projDamage)
        {
            Entity target = GetTarget(npc);
            if (npc.ai[0] != attackTelegraph)
                npc.ai[0]++;

            if (npc.direction == 0)
            {
                npc.direction = Main.rand.NextBool() ? -1 : 1;
                npc.spriteDirection = npc.direction;
            }
            if (target != null)
            {
                if (npc.ai[0] == attackTelegraph)
                {
                    npc.velocity = (target.Center - npc.Center).SafeNormalize(Vector2.UnitY) * dashSpeed;

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

                    npc.ai[0]++;
                }
            }
            else if (npc.ai[0] == attackTelegraph)
            {
                npc.velocity = (npc.rotation - MathHelper.PiOver4 * 3).ToRotationVector2() * dashSpeed;
                npc.ai[0]++;
                npc.netUpdate = true;
            }

            if (npc.ai[0] <= attackTelegraph)
            {
                npc.velocity *= 0.95f;
            }
            else if (npc.ai[0] <= attackTelegraph + attackDuration)
            {
                if (((int)npc.ai[0] - attackTelegraph) % attackTimeBetween == 0)
                {
                    if (!TerRoguelike.mpClient)
                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, target == null ? Vector2.UnitY * projSpeed : (target.Center - npc.Center).SafeNormalize(Vector2.UnitY) * projSpeed, projType, projDamage, 0);
                }
            }
            else
            {
                npc.ai[0] = -attackCooldown;
            }
        }
        public void RogueFlierAI(NPC npc, float xCap, float yCap, float acceleration, bool LoSRequired, bool ignorePlatforms = false)
        {
            Entity target = GetTarget(npc);

            npc.ai[3]++;
            npc.stairFall = true;
            if (npc.collideY && !ignorePlatforms)
            {
                int fluff = 1;
                int bottomtilepointx = (int)(npc.Center.X / 16f);
                int bottomtilepointY = (int)(npc.Bottom.Y / 16f);
                int floor = bottomtilepointY - fluff;
                for (int i = bottomtilepointY; i > floor - 1; i--)
                {
                    Tile tile = Main.tile[bottomtilepointx, i];
                    if (tile.HasUnactuatedTile && TileID.Sets.Platforms[tile.TileType] && tile.Slope == SlopeType.Solid)
                    {
                        npc.position.Y += 1;
                        npc.velocity.Y += 0.01f;
                        break;
                    }
                    if (i == floor)
                    {
                        npc.velocity.Y = -npc.oldVelocity.Y * 0.15f;
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


            bool LoSCheck = false;

            if (target != null)
            {
                if (!LoSRequired || CanHitInLine(npc.Center, target.Center))
                    LoSCheck = true;
            }

            if (npc.collideX)
            {
                if (target != null)
                {
                    if (LoSCheck && !CanHitInLine(npc.Bottom, target.Bottom))
                        npc.velocity.Y += acceleration * -1;
                }
                npc.velocity.X = -npc.oldVelocity.X * 0.15f;
            }

            if (LoSCheck)
            {
                Vector2 desiredPos = target.Center;
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
                if (Math.Abs(npc.velocity.X) > xCap * 1.5f)
                {
                    npc.velocity.X *= 0.98f;
                }
                if (Math.Abs(npc.velocity.Y) > yCap * 1.5f)
                {
                    npc.velocity.Y *= 0.98f;
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
        public void RogueDemonAI(NPC npc, float xCap, float yCap, float acceleration, bool LoSRequired, float attackDist, int attackTelegraph, int attackDuration, int attackTimeBetween, int attackCooldown, int projType, float projSpeed, int projDamage, bool ignorePlatforms = false)
        {
            Entity target = GetTarget(npc);

            if (npc.ai[1] != 0)
                npc.ai[1]++;

            npc.ai[3]++;
            npc.stairFall = true;
            if (npc.collideY && !ignorePlatforms)
            {
                int fluff = 1;
                int bottomtilepointx = (int)(npc.Center.X / 16f);
                int bottomtilepointY = (int)(npc.Bottom.Y / 16f);
                int floor = bottomtilepointY - fluff;
                for (int i = bottomtilepointY; i > floor - 1; i--)
                {
                    if (Main.tile[bottomtilepointx, i].HasUnactuatedTile && TileID.Sets.Platforms[Main.tile[bottomtilepointx, i].TileType])
                    {
                        npc.position.Y += 1;
                        npc.velocity.Y += 0.01f;
                        break;
                    }
                    if (i == floor)
                    {
                        npc.velocity.Y = -npc.oldVelocity.Y * 0.15f;
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


            bool LoSCheck = false;

            if (target != null)
            {
                if (!LoSRequired || CanHitInLine(npc.Center, target.Center))
                    LoSCheck = true;
            }
            if (npc.collideX)
            {
                if (target != null)
                {
                    if (LoSCheck && !CanHitInLine(npc.Bottom, target.Bottom))
                        npc.velocity.Y += acceleration * -1;
                }
                npc.velocity.X = -npc.oldVelocity.X * 0.15f;
            }

            if (LoSCheck || (target != null && npc.ai[1] > 0))
            {
                if (npc.ai[1] == 0)
                {
                    if ((target.Center - npc.Center).Length() <= attackDist)
                        npc.ai[1]++;
                }

                Vector2 desiredPos = target.Center;
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

                if (npc.ai[1] >= attackTelegraph)
                {
                    if (((int)npc.ai[1] - attackTelegraph) % attackTimeBetween == 0)
                    {
                        if (!TerRoguelike.mpClient)
                            Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, (target.Center - npc.Center).SafeNormalize(Vector2.UnitY) * projSpeed, projType, projDamage, 0);
                    }
                    if (npc.ai[1] >= attackTelegraph + attackDuration)
                        npc.ai[1] = -attackCooldown;
                }
            }
            else
            {
                if (npc.ai[1] > 0 && target == null)
                    npc.ai[1] = 0;

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
        public void RogueStormDiverAI(NPC npc, float xCap, float jumpVelocity, float attackDistance, int attackTelegraph, int attackCooldown, int extendedAttackSlowdownTime, int jetpackTelegraph, int jetpackDuration, int jetpackCooldown, float jetpackDistBeside, float jetpackSpeed, float speedMultiWhenShooting, float maxAimAngle, int projType, float projSpeed, Vector2 projOffset, int projDamage, bool LoSRequired, bool canJumpShoot = false, int projectileCount = 1, float projMaxSpread = 0, float maxVelocityDeviation = 0, int jumpDetectionPadding = 6)
        {
            Entity target = GetTarget(npc);

            float realXCap = npc.ai[1] > 0 || npc.ai[2] < -jetpackDuration ? xCap * speedMultiWhenShooting : xCap;

            if (npc.ai[1] != 0)
                npc.ai[1]++;

            if (npc.ai[2] < jetpackCooldown)
                npc.ai[2]++;

            bool LosCheck = target == null ? false : (!LoSRequired || CanHitInLine(npc.Center + projOffset, target.Center));
            if (target == null)
            {
                if (npc.ai[1] > 0)
                    npc.ai[1] = 0;
                if (npc.ai[2] >= jetpackCooldown)
                {
                    npc.ai[2] = -(jetpackDuration + jetpackTelegraph);
                }
            }
            else
            {
                bool attackDistanceCheck = (npc.Center - target.Center).Length() <= attackDistance;
                if (npc.ai[2] >= 0 && npc.ai[1] == 0 && npc.ai[0] >= 0 && (canJumpShoot || npc.velocity.Y == 0) && attackDistanceCheck && LosCheck && Math.Abs(AngleSizeBetween(npc.direction > 0 ? 0f : MathHelper.Pi, (target.Center - npc.Center + projOffset).ToRotation())) <= maxAimAngle)
                {
                    npc.ai[1]++;
                }

                if (npc.ai[1] == attackTelegraph)
                {
                    Vector2 projSpawnPos = npc.Center + projOffset;
                    Vector2 velocityDirection = (target.Center - projSpawnPos).SafeNormalize(Vector2.UnitY);
                    float angleBetween = AngleSizeBetween(npc.direction > 0 ? 0f : MathHelper.Pi, velocityDirection.ToRotation());
                    if (Math.Abs(angleBetween) > maxAimAngle)
                    {
                        velocityDirection = Vector2.UnitX.RotatedBy((Math.Sign(angleBetween) * maxAimAngle) + (npc.direction > 0 ? 0f : MathHelper.Pi));
                    }

                    for (int i = 0; i < projectileCount; i++)
                    {
                        Vector2 velocity = velocityDirection * (projSpeed + (maxVelocityDeviation == 0 ? 0 : Main.rand.NextFloat(-maxVelocityDeviation, maxVelocityDeviation + float.Epsilon)));
                        if (!TerRoguelike.mpClient)
                            Projectile.NewProjectile(npc.GetSource_FromThis(), projSpawnPos, projMaxSpread == 0 ? velocity : velocity.RotatedBy(Main.rand.NextFloat(-projMaxSpread, projMaxSpread + float.Epsilon)), projType, projDamage, 0f);
                    }
                }

                if (npc.ai[1] >= attackTelegraph + extendedAttackSlowdownTime)
                {
                    npc.ai[1] = -attackCooldown;
                }

                if (npc.ai[1] <= 0 && npc.ai[2] >= jetpackCooldown && npc.ai[0] >= 0 && LosCheck)
                {
                    if (target.Bottom.Y < npc.Top.Y - 96f || !attackDistanceCheck)
                        npc.ai[2] = -(jetpackDuration + jetpackTelegraph);
                }
            }

            if (npc.ai[2] < 0 && npc.ai[2] >= -jetpackDuration)
            {
                npc.noGravity = true;
                Vector2 targetPos = target == null ? npc.Center : target.Center + (Vector2.UnitX * jetpackDistBeside * Math.Sign(npc.Center.X - target.Center.X));
                npc.velocity = -Vector2.UnitY * jetpackSpeed;
                float rotation = MathHelper.Clamp(MathHelper.Lerp(0, MathHelper.Pi * 0.27f, (targetPos.X - npc.Center.X) / 240f), -MathHelper.Pi * 0.27f, MathHelper.Pi * 0.27f + float.Epsilon);
                npc.velocity = npc.velocity.RotatedBy(rotation);
            }
            else
                npc.noGravity = false;

            bool LoSBoredomCheck = target == null ? false : (LoSRequired && Math.Abs(npc.Center.X - target.Center.X) < 96 && !LosCheck);
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
            else if (npc.ai[0] > 60 && npc.ai[1] <= 0 && npc.ai[2] >= 0)
            {
                npc.ai[0] = -240;

                if (!LoSBoredomCheck)
                {
                    npc.direction *= -1;
                    npc.spriteDirection *= -1;
                }
                else
                {
                    npc.direction = Main.rand.NextBool() ? -1 : 1;
                    npc.spriteDirection = npc.direction;
                }
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

            if ((npc.collideX || LoSBoredomCheck) && npc.ai[2] >= 0)
            {
                npc.ai[0]++;
                if (npc.collideY && npc.oldVelocity.Y >= 0 && npc.collideX)
                    npc.velocity.Y = jumpVelocity;
            }
            else if (npc.ai[0] > 0)
                npc.ai[0] = 0f;

            if (target != null)
            {
                if (npc.velocity.Y == 0f && target.Bottom.Y < npc.Top.Y && Math.Abs(npc.Center.X - target.Center.X) < (float)(target.width * 3) && Collision.CanHit(npc, target))
                {
                    if ((canJumpShoot || npc.ai[1] <= 0) && npc.ai[2] >= 0)
                    {
                        int padding = jumpDetectionPadding;
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
                else if (npc.velocity.Y == 0f && target.Top.Y > npc.Bottom.Y && Math.Abs(npc.Center.X - target.Center.X) < 148f && Collision.CanHit(npc, target))
                {
                    int fluff = 6;
                    int bottomtilepointx = (int)(npc.Center.X / 16f);
                    int bottomtilepointY = (int)(npc.Bottom.Y / 16f);
                    for (int i = bottomtilepointY; i < bottomtilepointY + fluff; i++)
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
        public void UpdateWormSegments(NPC npc, float segmentRotationInterpolant = 0.95f)
        {
            for (int i = 0; i < Segments.Count; i++)
            {
                WormSegment segment = Segments[i];
                segment.OldPosition = segment.Position;
                segment.OldRotation = segment.Rotation;
                if (i == 0)
                {
                    segment.Position = npc.Center;
                    segment.Position += npc.velocity;
                    segment.Rotation = npc.rotation;
                    continue;
                }

                WormSegment oldSeg = Segments[i - 1];

                segment.Position = oldSeg.Position - (Vector2.UnitX * (i == 1 ? segment.Height : oldSeg.Height)).RotatedBy(oldSeg.Rotation.AngleLerp((oldSeg.Position - segment.Position).ToRotation(), segmentRotationInterpolant));

                Vector2 difference = oldSeg.Position - segment.Position;

                segment.Rotation = (difference).ToRotation();
            }
        }

        #endregion

        public override bool InstancePerEntity => true;
        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter writer)
        {
            writer.Write(eliteVars.tainted);
            writer.Write(eliteVars.slugged);
            writer.Write(eliteVars.burdened);
            writer.Write(hostileTurnedAlly);
            writer.Write(isRoomNPC);
            writer.Write(sourceRoomListID);
            writer.Write(targetNPC);
            writer.Write(targetPlayer);
            writer.Write(npc.friendly);

            bool sendZero = npc.ai[0] == 0 || npc.ai[1] == 0 || npc.ai[2] == 0 || npc.ai[3] == 0;
            writer.Write(sendZero);
            if (sendZero)
            {
                writer.Write(npc.ai[0]);
                writer.Write(npc.ai[1]);
                writer.Write(npc.ai[2]);
                writer.Write(npc.ai[3]);
            }
            writer.Write(npc.direction);

            int segCount = Segments.Count;
            writer.Write(segCount);
            for (int i = 0; i < segCount; i++)
            {
                var seg = Segments[i];
                writer.WriteVector2(seg.Position);
                writer.WriteVector2(seg.OldPosition);
                writer.Write(seg.Rotation);
                writer.Write(seg.OldRotation);
                writer.Write(seg.Height);
            }
            if (segCount > 0)
                writer.Write(npc.rotation);

            writer.Write(npc.immortal);
            writer.Write(npc.dontTakeDamage);
            writer.Write(puppetOwner);
            writer.Write(puppetLifetime);
            writer.Write(ballAndChainSlow);
            writer.Write(sluggedTime);
            writer.Write(npc.gfxOffY);
            writer.Write(AdaptiveArmorEnabled);
            writer.Write(AdaptiveArmorAddRate);
            writer.Write(AdaptiveArmorDecayRate);
            writer.Write(AdaptiveArmorCap);
        }
        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader reader)
        {
            eliteVars.tainted = reader.ReadBoolean();
            eliteVars.slugged = reader.ReadBoolean();
            eliteVars.burdened = reader.ReadBoolean();
            hostileTurnedAlly = reader.ReadBoolean();
            isRoomNPC = reader.ReadBoolean();
            sourceRoomListID = reader.ReadInt32();
            targetNPC = reader.ReadInt32();
            targetPlayer = reader.ReadInt32();
            npc.friendly = reader.ReadBoolean();

            bool recieveZero = reader.ReadBoolean();
            if (recieveZero)
            {
                npc.ai[0] = reader.ReadSingle();
                npc.ai[1] = reader.ReadSingle();
                npc.ai[2] = reader.ReadSingle();
                npc.ai[3] = reader.ReadSingle();
            }
            npc.direction = reader.ReadInt32();

            Segments.Clear();
            int segCount = reader.ReadInt32();
            for (int i = 0; i < segCount; i++)
            {
                Vector2 pos = reader.ReadVector2();
                Vector2 oldpos = reader.ReadVector2();
                float rot = reader.ReadSingle();
                float oldrot = reader.ReadSingle();
                float height = reader.ReadSingle();

                Segments.Add(new(pos, rot, height));
                Segments[i].OldPosition = oldpos;
                Segments[i].OldRotation = oldrot;
            }
            if (segCount > 0)
                npc.rotation = reader.ReadSingle();

            npc.immortal = reader.ReadBoolean();
            npc.dontTakeDamage = reader.ReadBoolean();
            puppetOwner = reader.ReadInt32();
            puppetLifetime = reader.ReadInt32();
            ballAndChainSlow = reader.ReadInt32();
            sluggedTime = reader.ReadInt32();
            npc.gfxOffY = reader.ReadSingle();
            AdaptiveArmorEnabled = reader.ReadBoolean();
            AdaptiveArmorAddRate = reader.ReadSingle();
            AdaptiveArmorDecayRate = reader.ReadSingle();
            AdaptiveArmorCap = reader.ReadSingle();
        }
        public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
        {
            if (TerRoguelikeWorld.IsTerRoguelikeWorld)
            {
                spawnRate = int.MaxValue;
                maxSpawns = 0;
            }
        }
        public override void SetDefaults(NPC entity)
        {
            if (calamityMod is not null)
            {
                if (entity.type == calamityMod.Find<ModNPC>("Yharon").Type)
                    OverrideIgniteVisual = true;
            }
        }

        public override void OnSpawn(NPC npc, IEntitySource source)
        {
            whoAmI = npc.whoAmI;
            baseMaxHP = npc.lifeMax;
            baseDamage = npc.damage;

            if (!TerRoguelikeWorld.IsTerRoguelikeWorld)
                return;

            npc.netUpdate = true;

            if (npc.type == NPCID.OldMan || npc.type == NPCID.Guide)
                npc.active = false;
            if (source is EntitySource_TileBreak breakSource)
            {
                npc.active = false;
            }
            if (source is EntitySource_Parent parentSource)
            {
                if (parentSource.Entity is NPC)
                {
                    NPC parent = Main.npc[parentSource.Entity.whoAmI];
                    var modparent = parent.ModNPC();
                    if (modparent != null)
                    {
                        if (modparent.isRoomNPC)
                        {
                            isRoomNPC = true;
                            sourceRoomListID = modparent.sourceRoomListID;
                        }
                        if (modparent.hostileTurnedAlly && modparent.puppetOwner >= 0)
                        {
                            var parentowner = Main.player[modparent.puppetOwner];
                            var modparentowner = parentowner.ModPlayer();
                            modparentowner?.MakeNPCPuppet(npc, npc.ModNPC());
                        }
                        targetPlayer = modparent.targetPlayer;
                        targetNPC = modparent.targetNPC;
                    }
                }
            }

            if (TerRoguelikeBoss && TerRoguelikeMenu.RuinedMoonActive)
            {
                AdaptiveArmorEnabled = true;
            }

            SpawnManager.ApplyNPCDifficultyScaling(npc, this);

            if (TerRoguelikeWorld.escape && TerRoguelikeBoss)
            {
                EnemyHealthBarSystem.enemyHealthBar = new([npc.whoAmI], npc.GivenOrTypeName);
            }
        }
        public override bool PreAI(NPC npc)
        {
            if (packetCooldown > 0)
                packetCooldown--;
            if (currentUpdate == 1 && Segments != null && Segments.Count > 0)
            {
                npc.netSpam = 0;
                if (packetCooldown <= 0)
                {
                    npc.netUpdate = true;
                    packetCooldown = 5;
                }
            }
            if (TerRoguelikeWorld.IsTerRoguelikeWorld && !scalingApplied)
            {
                baseMaxHP = npc.lifeMax;
                baseDamage = npc.damage;
                SpawnManager.ApplyNPCDifficultyScaling(npc, this);
            }

            diminishingDR = 0;

            maxUpdates = 1;
            if (eliteVars.tainted)
            {
                AdaptiveArmorEnabled = true;
                AdaptiveArmorDecayRate = 280;
                AdaptiveArmorCap = 200;
                maxUpdates = 2;
                npc.knockBackResist = 0;
            }
            if (eliteVars.slugged)
            {
                AdaptiveArmorEnabled = true;
                AdaptiveArmorDecayRate = 280;
                diminishingDR += 20;
                if (sluggedEliteSlowApplied)
                {
                    npc.velocity /= 0.85f;
                    sluggedEliteSlowApplied = false;
                }
                npc.knockBackResist = 0;
            }
            if (eliteVars.burdened)
            {
                AdaptiveArmorEnabled = true;
                AdaptiveArmorDecayRate = 280;
                npc.knockBackResist = 0;
            }
            
            if (hostileTurnedAlly)
            {
                npc.friendlyRegen = -1000000;
            }
            if (targetCooldown > 0 && currentUpdate == 1)
            {
                targetCooldown--;
            }

            if (ballAndChainSlowApplied) // grant slowed velocity back as an attempt to make the ai run normal as if it was going full speed
            {
                npc.velocity /= 0.85f;
                ballAndChainSlowApplied = false;
            }
            if (sluggedSlowApplied)
            {
                npc.velocity /= 0.7f;
                sluggedSlowApplied = false;
            }
            return true;
        }
        public override void PostAI(NPC npc)
        {
            if (ballAndChainSlow > 0) // slow down
            {
                npc.velocity *= 0.85f;
                ballAndChainSlowApplied = true;
                if (currentUpdate == 1)
                {
                    ballAndChainSlow--;
                }
            }

            if (eliteVars.slugged)
            {
                npc.velocity *= 0.85f;
                sluggedEliteSlowApplied = true;
            }
            else if (sluggedTime > 0)
            {
                npc.velocity *= 0.7f;
                sluggedSlowApplied = true;
            }
            if (eliteVars.burdened && !TerRoguelike.mpClient)
            {
                bool pass = true;
                int projType = ModContent.ProjectileType<ResidualBurden>();
                Vector2 spawnPos = npc.Center;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    var proj = Main.projectile[i];
                    if (!proj.active || proj.type != projType || proj.timeLeft < 45) continue;

                    var projRect = proj.getRect();
                    projRect.Inflate(6, 6);
                    if (projRect.Contains(spawnPos.ToPoint()))
                    {
                        pass = false;
                        break;
                    }
                }
                if (pass)
                    Projectile.NewProjectileDirect(npc.GetSource_FromThis(), spawnPos, Vector2.Zero, projType, npc.damage, 0);
            }

            if (hostileTurnedAlly)
            {
                if (currentUpdate == 1)
                {
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        if (friendlyFireHitCooldown[i] > 0)
                            friendlyFireHitCooldown[i]--;
                    }
                }
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC target = Main.npc[i];
                    if (friendlyFireHitCooldown[i] > 0 || !target.active || !target.ModNPC().CanBeChased(false, false) || i == npc.whoAmI)
                        continue;

                    int cdslot = 0;
                    float multi = 1;
                    float multienemy = 1;
                    Rectangle hitbox = npc.Hitbox;
                    Rectangle enemyHitbox = target.Hitbox;
                    NPCLoader.CanHitNPC(target, npc);
                    NPCLoader.ModifyCollisionData(target, hitbox, ref cdslot, ref multienemy, ref enemyHitbox);
                    if (NPCLoader.CanHitNPC(npc, target) && NPCLoader.ModifyCollisionData(npc, enemyHitbox, ref cdslot, ref multi, ref hitbox) &&
                        hitbox.Intersects(enemyHitbox))
                    {
                        var modTarget = target.ModNPC();
                        int hitDamage = (int)(npc.damage * multi * (modTarget != null ? modTarget.effectiveDamageTakenMulti : 1));
                        if (hitDamage < 1)
                            hitDamage = 1;
                        var modifiers = new NPC.HitModifiers();
                        NPCLoader.ModifyHitNPC(npc, target, ref modifiers);

                        NPC.HitInfo info = new NPC.HitInfo();
                        info.HideCombatText = true;
                        info.Damage = hitDamage;
                        info.InstantKill = false;
                        info.HitDirection = 1;
                        info.Knockback = 0f;
                        info.Crit = false;

                        target.StrikeNPC(info);
                        NetMessage.SendStrikeNPC(target, info);
                        NPCLoader.OnHitNPC(npc, target, info);
                        CombatText.NewText(target.getRect(), Color.Orange, hitDamage);
                        if (target.life <= 0 || modTarget.shouldHaveDied)
                        {
                            if (puppetOwner >= 0)
                            {
                                if (puppetOwner == Main.myPlayer)
                                {
                                    var modOwner = Main.player[puppetOwner].ModPlayer();
                                    modOwner?.OnKillEffects(target);
                                }
                                else if (Main.dedServ)
                                {
                                    ActivateOnKillPacket.Send(npc.whoAmI, npc.type, npc.Center, puppetOwner);
                                }
                                
                            }
                        }

                        friendlyFireHitCooldown[i] += 30;
                    }
                }
            }

            if (currentUpdate == 1)
            {
                if (!eliteNamed)
                    GiveEliteName(npc.whoAmI);

                if (hostileTurnedAlly)
                {
                    puppetLifetime--;
                    if (puppetLifetime <= 0)
                        npc.StrikeInstantKill();
                }

                if (sluggedTime > 0)
                    sluggedTime--;

                if (ignitedStacks != null && ignitedStacks.Count > 0) // ignite debuff logic
                {
                    if (ignitedHitCooldown <= 0)
                    {
                        int hitDamage = 0;
                        int displayedDamage = 0;
                        int targetDamage = (int)(npc.lifeMax * 0.01f);

                        for (int i = 0; i < ignitedStacks.Count; i++)
                        {
                            int thisOwner = ignitedStacks[i].Owner;

                            var igniteStack = ignitedStacks[i];
                            int myDamageCap = igniteStack.DamageCapPerTick;
                            int myTargetDamage = targetDamage;
                            if (myTargetDamage > myDamageCap)
                                myTargetDamage = myDamageCap;
                            else if (myTargetDamage < 1)
                                myTargetDamage = 1;

                            if (ignitedStacks[i].DamageToDeal < myTargetDamage)
                            {
                                if (thisOwner == Main.myPlayer)
                                    hitDamage += ignitedStacks[i].DamageToDeal;
                                displayedDamage += ignitedStacks[i].DamageToDeal;
                                ignitedStacks[i].DamageToDeal = 0;
                            }
                            else
                            {
                                if (thisOwner == Main.myPlayer)
                                    hitDamage += myTargetDamage;
                                displayedDamage += myTargetDamage;
                                ignitedStacks[i].DamageToDeal -= myTargetDamage;
                            }
                        }
                        IgniteHit(hitDamage, displayedDamage, npc, Main.myPlayer);
                        ignitedStacks.RemoveAll(x => x.DamageToDeal <= 0);
                    }
                }
                if (ignitedHitCooldown > 0)
                    ignitedHitCooldown--;

                if (bleedingStacks != null && bleedingStacks.Count > 0) // bleeding debuff logic
                {
                    if (bleedingHitCooldown <= 0)
                    {
                        int hitDamage = 0;
                        int displayedDamage = 0;
                        int targetDamage = 40;
                        int owner = -1;

                        for (int i = 0; i < bleedingStacks.Count; i++)
                        {
                            int thisOwner = bleedingStacks[i].Owner;
                            if (bleedingStacks[i].DamageToDeal < targetDamage)
                            {
                                if (thisOwner == Main.myPlayer)
                                    hitDamage += bleedingStacks[i].DamageToDeal;
                                displayedDamage += bleedingStacks[i].DamageToDeal;
                                bleedingStacks[i].DamageToDeal = 0;
                            }
                            else
                            {
                                if (thisOwner == Main.myPlayer)
                                    hitDamage += targetDamage;
                                displayedDamage += targetDamage;
                                bleedingStacks[i].DamageToDeal -= targetDamage;
                            }
                            owner = thisOwner;
                        }
                        BleedingHit(hitDamage, displayedDamage, npc, owner);
                        bleedingStacks.RemoveAll(x => x.DamageToDeal <= 0);
                    }
                }
                if (bleedingHitCooldown > 0)
                    bleedingHitCooldown--;

                if (AdaptiveArmor > 0)
                {
                    if (AdaptiveArmor > AdaptiveArmorCap)
                        AdaptiveArmor = AdaptiveArmorCap;

                    AdaptiveArmor -= AdaptiveArmorDecayRate / 60f;
                    if (AdaptiveArmor < 0)
                        AdaptiveArmor = 0;
                }

                if (isRoomNPC && sourceRoomListID >= 0)
                {
                    Room room = RoomList[sourceRoomListID];
                    if (!room.IsBossRoom && !TerRoguelikeWorld.escape && (room.roomTime - room.waveClearGraceTime >= 480 || overheadArrowTime != 0))
                    {
                        overheadArrowTime++;
                    }
                }
            }
        }
        public override bool CheckDead(NPC npc)
        {
            if (TerRoguelikeWorld.escape)
            {
                if (!activatedJstc && isRoomNPC && sourceRoomListID >= 0)
                {
                    Floor targetFloor = SchematicManager.FloorID[RoomList[sourceRoomListID].AssociatedFloor];
                    if (targetFloor.jstcProgress == Floor.JstcProgress.Start)
                        targetFloor.jstc++;
                    activatedJstc = true;
                }
            }
            return true;
        }
        public void CleanseDebuffs()
        {
            ignitedStacks.Clear();
            bleedingStacks.Clear();
            ballAndChainSlow = 0;
            sluggedTime = 0;
        }
        public void IgniteHit(int hitDamage, int displayDamage, NPC npc, int owner)
        {
            ignitedHitCooldown += 10; // hits 6 times a second

            if (npc.immortal || npc.dontTakeDamage || CutsceneSystem.cutsceneDisableControl)
                return;

            int origDamage = hitDamage;
            TerRoguelikePlayer modPlayer = Main.player[owner].ModPlayer();

            hitDamage = (int)(hitDamage * modPlayer.GetBonusDamageMulti(npc, npc.Center) * effectiveDamageTakenMulti);
            displayDamage = (int)(displayDamage * modPlayer.GetBonusDamageMulti(npc, npc.Center) * effectiveDamageTakenMulti);
            if (hitDamage < 1)
                hitDamage = 1;
            if (displayDamage < 1)
                displayDamage = 1;

            NPC.HitInfo info = new NPC.HitInfo();
            info.HideCombatText = true;
            info.Damage = hitDamage;
            info.InstantKill = false;
            info.HitDirection = Main.rand.NextBool() ? -1 : 1;
            info.Knockback = 0f;
            info.Crit = false;

            if (origDamage > 0)
            {
                int preHitHP = npc.life;
                npc.StrikeNPC(info);
                NetMessage.SendStrikeNPC(npc, info, owner);

                if (preHitHP - displayDamage <= 0 || shouldHaveDied)
                {
                    modPlayer.OnKillEffects(npc);
                }
            }
            if (npc.Center.Distance(Main.Camera.Center) < 1600)
                CombatText.NewText(npc.getRect(), Color.DarkKhaki, displayDamage);
        }
        public void BleedingHit(int hitDamage, int displayDamage, NPC npc, int owner)
        {
            bleedingHitCooldown = 20; // hits 3 times a second

            if (npc.immortal || npc.dontTakeDamage || owner < 0 || owner >= Main.maxPlayers)
                return;

            int origDamage = hitDamage;
            TerRoguelikePlayer modPlayer = Main.player[owner].ModPlayer();
            if (modPlayer == null)
                return;

            hitDamage = (int)(hitDamage * modPlayer.GetBonusDamageMulti(npc, npc.Center) * effectiveDamageTakenMulti);
            displayDamage = (int)(hitDamage * modPlayer.GetBonusDamageMulti(npc, npc.Center) * effectiveDamageTakenMulti);
            if (hitDamage < 1)
                hitDamage = 1;
            if (displayDamage < 1)
                displayDamage = 1;

            NPC.HitInfo info = new NPC.HitInfo();
            info.HideCombatText = true;
            info.Damage = hitDamage;
            info.InstantKill = false;
            info.HitDirection = Main.rand.NextBool() ? -1 : 1;
            info.Knockback = 0f;
            info.Crit = false;

            if (origDamage > 0)
            {
                int prehitHP = npc.life;
                npc.StrikeNPC(info);
                NetMessage.SendStrikeNPC(npc, info, owner);
                if (prehitHP - displayDamage <= 0 || shouldHaveDied)
                {
                    modPlayer.OnKillEffects(npc);
                }
            }
            
            CombatText.NewText(npc.getRect(), Color.MediumVioletRed, displayDamage);
        }
        public void AddBleedingStackWithRefresh(BleedingStack stack, int target, bool noSend = false)
        {
            if (bleedingStacks.Count > 0)
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
            if (!noSend)
                ApplyBleedPacket.Send(stack, target, -1, Main.myPlayer);
        }
        public void AddIgniteStack(IgnitedStack stack, int target, bool noSend = false)
        {
            ignitedStacks.Add(stack);
            if (!noSend)
                ApplyIgnitePacket.Send(stack, target);
        }
        public override bool PreKill(NPC npc)
        {
            if (hostileTurnedAlly)
            {
                SoundEngine.PlaySound(SoundID.NPCHit36 with { Volume = 0.1f }, npc.Center);
                SoundEngine.PlaySound(SoundID.NPCDeath6 with { Volume = 1f }, npc.Center);
                for (int i = 0; i < 50; i++)
                {
                    int d = Dust.NewDust(npc.position, npc.width, npc.height, 180, npc.velocity.X * 0.5f, npc.velocity.Y * 0.5f);
                    Dust dust = Main.dust[d];
                    dust.velocity *= 2f;
                    dust.noGravity = true;
                    dust.scale = 1.4f;
                }
                return false;
            }

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
            if (Segments.Count > 0)
                modifiers.HideCombatText();

            if (TerRoguelikeWorld.IsTerRoguelikeWorld)
            {
                modifiers.DefenseEffectiveness *= 0f;
            }

            modifiers.SourceDamage *= effectiveDamageTakenMulti;
        }
        public override void HitEffect(NPC npc, NPC.HitInfo hit)
        {
            overheadArrowTime = -480;
            if (AdaptiveArmorEnabled)
            {
                AdaptiveArmor += AdaptiveArmorAddRate * 100 * (hit.Damage / (float)npc.lifeMax);
                if (AdaptiveArmor > AdaptiveArmorCap)
                    AdaptiveArmor = AdaptiveArmorCap;
            }
            if (npc.life <= 0 && !activatedJstc && isRoomNPC && sourceRoomListID >= 0)
            {
                Floor targetFloor = SchematicManager.FloorID[RoomList[sourceRoomListID].AssociatedFloor];
                if (targetFloor.jstcProgress == Floor.JstcProgress.Start)
                    targetFloor.jstc++;
                activatedJstc = true;
            }
        }
        public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            if (Segments.Count > 0)
            {
                Vector2 position = Segments[hitSegment].Position;
                int segHeight = (int)Segments[hitSegment].Height;
                Rectangle segRect = new Rectangle((int)position.X - (segHeight / 2), (int)position.Y - (segHeight / 2), segHeight, segHeight);
                CombatText.NewText(segRect, hit.Crit ? CombatText.DamagedHostileCrit : CombatText.DamagedHostile, hit.Damage, hit.Crit);
            }
        }
        public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
        {
            if (eliteVars.slugged)
            {
                target.ModPlayer().sluggedAttempt = true;
            }
        }
        public override void OnHitNPC(NPC npc, NPC target, NPC.HitInfo hit)
        {
            var modTarget = target.ModNPC();
            if (modTarget != null)
            {
                if (target.friendly && modTarget.Segments.Count > 0)
                {
                    Vector2 position = modTarget.Segments[hitSegment].Position;
                    int segHeight = (int)modTarget.Segments[hitSegment].Height;
                    Rectangle segRect = new Rectangle((int)position.X - (segHeight / 2), (int)position.Y - (segHeight / 2), segHeight, segHeight);
                    CombatText.NewText(segRect, hit.Crit ? CombatText.DamagedFriendlyCrit : CombatText.DamagedFriendly, hit.Damage, hit.Crit);
                }
            }
        }
        public override bool ModifyCollisionData(NPC npc, Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox)
        {
            if (Segments.Count > 0)
            {
                for (int i = 0; i < Segments.Count; i++)
                {
                    WormSegment segment = Segments[i];
                    float radius = i == 0 ? (npc.height < npc.width ? npc.height / 2 : npc.width / 2) : segment.Height / 2;
                    if (segment.Position.Distance(victimHitbox.ClosestPointInRect(segment.Position)) <= radius)
                    {
                        Point closestPoint = victimHitbox.ClosestPointInRect(segment.Position).ToPoint();
                        npcHitbox = new Rectangle(closestPoint.X - 1, closestPoint.Y - 1, 3, 3);
                        hitSegment = i;
                        break;
                    }
                }
            }
            return true;
        }
        public override void ModifyHoverBoundingBox(NPC npc, ref Rectangle boundingBox)
        {
            if (Segments.Count > 0)
            {
                for (int i = 0; i < Segments.Count; i++)
                {
                    WormSegment segment = Segments[i];
                    //bool pass = new Rectangle((int)(segment.Position.X - ((i == 0 ? npc.width : segment.Height) / 2)), (int)(segment.Position.Y - ((i == 0 ? npc.height : segment.Height) / 2)), npc.width, npc.height).Contains(Main.MouseWorld.ToPoint());
                    bool pass = Main.MouseWorld.Distance(segment.Position) < segment.Height;
                    if (pass)
                    {
                        boundingBox = new Rectangle(0, 0, Main.maxTilesX * 16, Main.maxTilesY * 16);
                        break;
                    }
                }
            }
        }
        public class EliteEffectHelperVars
        {
            public Vector2? texSize;
            public Rectangle? frame;
            public int vertFrameCount;
            public int horizFrameCount;
            public EliteEffectHelperVars(int VertFrameCount = -1, int HorizFrameCount = 1, Vector2? TexSize = null, Rectangle? Frame = null)
            {
                vertFrameCount = VertFrameCount;
                horizFrameCount = HorizFrameCount;
                texSize = TexSize;
                frame = Frame;
            }
        }
        public static bool EliteSpritebatch = false;
        public static void GhostSpritebatch(bool end = true, BlendState blendState = null)
        {
            if (blendState == null)
                blendState = BlendState.AlphaBlend;

            if (end)
                Main.spriteBatch.End();

            Effect ghostEffect = Filters.Scene["TerRoguelike:GrayscaleRecolor"].GetShader().Shader;
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, blendState, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, ghostEffect, Main.GameViewMatrix.TransformationMatrix);

            Color recolor = Color.Cyan;
            recolor.A = 0;
            ghostEffect.Parameters["recolor"].SetValue(recolor.ToVector4());
            ghostEffect.Parameters["intensity"].SetValue(0.6f);
        }
        public bool EliteEffectSpritebatch(NPC npc, EliteEffectHelperVars vars, bool end = true)
        {
            if (npc.IsABestiaryIconDummy)
                return false;

            var sb = Main.spriteBatch;
            if (end)
                sb.End();
            if (hostileTurnedAlly && npc.friendly)
            {
                GhostSpritebatch(false);
            }
            else if (eliteVars.tainted)
            {
                Effect taintedEffect = Filters.Scene["TerRoguelike:DualContrast"].GetShader().Shader;
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, taintedEffect, Main.GameViewMatrix.TransformationMatrix);

                taintedEffect.Parameters["lightTint"].SetValue(Color.Black.ToVector4());
                taintedEffect.Parameters["darkTint"].SetValue(Color.Yellow.ToVector4());
                taintedEffect.Parameters["contrastThreshold"].SetValue(0.3f);
            }
            else if (eliteVars.slugged)
            {
                float time = Main.GlobalTimeWrappedHourly + npc.whoAmI;
                Vector2 texSize = vars.texSize == null ? TextureAssets.Npc[npc.type].Size() : (Vector2)vars.texSize;
                Rectangle frame = vars.frame == null ? npc.frame : (Rectangle)vars.frame;
                if (vars.vertFrameCount < 0)
                    vars.vertFrameCount = Main.npcFrameCount[npc.type];

                Effect maskEffect = Filters.Scene["TerRoguelike:MaskOverlay"].GetShader().Shader;
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, maskEffect, Main.GameViewMatrix.TransformationMatrix);

                Vector2 screenOff = new Vector2((float)Math.Cos(time * MathHelper.PiOver4) / texSize.X * 40, time / texSize.Y * 40);
                Vector2 frameTopLeft = new Vector2(frame.X, frame.Y);
                screenOff -= frameTopLeft / texSize;
                Color tint = Color.Purple;

                maskEffect.Parameters["screenOffset"].SetValue(screenOff);
                maskEffect.Parameters["stretch"].SetValue(texSize / 300);
                maskEffect.Parameters["replacementTexture"].SetValue(TexDict["Streaks"]);
                maskEffect.Parameters["tint"].SetValue(tint.ToVector4());
            }
            else if (eliteVars.burdened)
            {
                float time = Main.GlobalTimeWrappedHourly + npc.whoAmI;
                Vector2 texSize = vars.texSize == null ? TextureAssets.Npc[npc.type].Size() : (Vector2)vars.texSize;
                Rectangle frame = vars.frame == null ? npc.frame : (Rectangle)vars.frame;
                if (vars.vertFrameCount < 0)
                    vars.vertFrameCount = Main.npcFrameCount[npc.type];

                Effect maskEffect = Filters.Scene["TerRoguelike:MaskOverlay"].GetShader().Shader;
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, maskEffect, Main.GameViewMatrix.TransformationMatrix);

                Vector2 screenOff = new Vector2((float)Math.Cos(time * MathHelper.PiOver4) / texSize.X * 40, time / texSize.Y * 70);
                Vector2 frameTopLeft = new Vector2(frame.X, frame.Y);
                screenOff -= frameTopLeft / texSize;
                Color tint = Color.Lerp(Color.Teal, Color.Cyan, 0.3f);

                maskEffect.Parameters["screenOffset"].SetValue(screenOff);
                maskEffect.Parameters["stretch"].SetValue(texSize / 400);
                maskEffect.Parameters["replacementTexture"].SetValue(TexDict["Crust"]);
                maskEffect.Parameters["tint"].SetValue(tint.ToVector4());
            }
            else
            {
                StartVanillaSpritebatch(false);
                return false;
            }
            return true;
        }
        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (!eliteNamed)
                GiveEliteName(npc.whoAmI);

            if (ignitedStacks != null && ignitedStacks.Count > 0 && !OverrideIgniteVisual)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);


                Texture2D texture = TextureAssets.Npc[npc.type].Value;
                Color color = Color.Lerp(Color.Yellow, Color.OrangeRed, Main.rand.NextFloat(0.4f, 0.6f + float.Epsilon) + 0.2f + (0.2f * (float)Math.Cos((Main.GlobalTimeWrappedHourly * 20f)))) * 0.8f;
                Vector3 colorHSL = Main.rgbToHsl(color);
                float outlineThickness = 1f;
                Vector2 vector = IgniteCentered ? new Vector2(npc.frame.Width / 2f, npc.frame.Height / 2f) : new Vector2(npc.frame.Width / 2f, texture.Height / (float)Main.npcFrameCount[npc.type] * 0.5f);
                SpriteEffects spriteEffects = npc.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

                GameShaders.Misc["TerRoguelike:BasicTint"].UseOpacity(1f);
                GameShaders.Misc["TerRoguelike:BasicTint"].UseColor(Main.hslToRgb(1 - colorHSL.X, colorHSL.Y, colorHSL.Z));
                GameShaders.Misc["TerRoguelike:BasicTint"].Apply();

                Vector2 position = GetDrawCenter(npc) + (Vector2.UnitY * npc.gfxOffY);
                Vector2 halfSize = vector;
                for (float i = 0; i < 1; i += 0.125f)
                {
                    if (!IgniteCentered)
                        spriteBatch.Draw(texture, npc.Bottom + (i * MathHelper.TwoPi + npc.rotation).ToRotationVector2() * outlineThickness - screenPos + new Vector2((float)(-texture.Width) * npc.scale / 2f + halfSize.X * npc.scale, (float)(-texture.Height) * npc.scale / (float)Main.npcFrameCount[npc.type] + 4f + halfSize.Y * npc.scale + Main.NPCAddHeight(npc) + npc.gfxOffY), (Rectangle?)npc.frame, Color.White, npc.rotation, halfSize, npc.scale, spriteEffects, 0f);
                    else
                        spriteBatch.Draw(texture, position + (i * MathHelper.TwoPi + npc.rotation).ToRotationVector2() * outlineThickness - Main.screenPosition, npc.frame, color, npc.rotation, vector, npc.scale, spriteEffects, 0f);
                }

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            }

            if (bleedingStacks != null && bleedingStacks.Count > 0)
            {
                DrawRotatlingBloodParticles(false, npc);
            }

            EliteEffectSpritebatch(npc, new());

            return true;
        }
        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (ignitedStacks != null && ignitedStacks.Count > 0)
            {
                drawColor = Color.Lerp(Color.White, Color.OrangeRed, 0.4f);
                for (int i = 0; i < Math.Min(ignitedStacks.Count, 10); i++)
                {
                    if (Main.rand.NextBool(5))
                    {
                        int d = Dust.NewDust(npc.position + npc.ModNPC().drawCenter, npc.width, npc.height, DustID.Torch);
                        if (Segments.Count > 0)
                            Main.dust[d].noLight = true;
                    }
                        
                }
            }
            if (ballAndChainSlow > 0)
            {
                drawColor = drawColor.MultiplyRGB(Color.LightGray);
                //Dust.NewDust(npc.BottomLeft + new Vector2(0, -4f), npc.width, 1, DustID.t_Slime, newColor: Color.Gray, Scale: 0.5f);
            }
        }
        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            StartVanillaSpritebatch();
            if (bleedingStacks != null && bleedingStacks.Count > 0)
            {
                DrawRotatlingBloodParticles(true, npc);
            }
        }
        /// <summary>
        /// Drawns top half behind npcs and bottom half in front of npcs.
        /// </summary>
        public void DrawRotatlingBloodParticles(bool inFront, NPC npc)
        {
            Texture2D texture = TexDict["AdaptiveGunBullet"];
            Color color = Color.Red * 1f;
            Vector2 position = GetDrawCenter(npc) + (Vector2.UnitY * npc.gfxOffY);

            int count = Math.Min(bleedingStacks.Count, 100);
            for (int i = 0; i < count; i++)
            {
                Vector2 specificPosition = position;
                float rotation = MathHelper.Lerp(0, MathHelper.TwoPi, Main.GlobalTimeWrappedHourly * 1.5f);
                float rotationCompletionOffset = MathHelper.TwoPi / count * i;
                rotation += rotationCompletionOffset;
                specificPosition += new Vector2(0, 16).RotatedBy(rotation);
                specificPosition += (specificPosition - position) * new Vector2(((npc.width + npc.frame.Width) * 0.5f * npc.scale) / 32f, -0.5f);
                if (specificPosition.Y >= position.Y && inFront)
                    Main.EntitySpriteDraw(texture, specificPosition - Main.screenPosition, null, color, 0f, texture.Size() * 0.5f, 1f, SpriteEffects.None);
                else if (specificPosition.Y < position.Y && !inFront)
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
                case NPCAIStyleID.CritterWorm:
                    if (npc.type == NPCID.Grubby)
                        position += new Vector2(0, -3);
                    else
                        position += new Vector2(0, 0);
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
        public Entity GetTarget(NPC npc)
        {
            if (targetPlayer != -1)
            {
                if (!Main.player[targetPlayer].active || Main.player[targetPlayer].dead || npc.friendly)
                {
                    targetPlayer = -1;
                    targetCooldown = 0;
                    npc.netUpdate = true;
                }
            }
            if (targetNPC != -1)
            {
                if (hostileTurnedAlly ? (!Main.npc[targetNPC].ModNPC().CanBeChased(false, false) || !npc.friendly) : (!Main.npc[targetNPC].ModNPC().CanBeChased(false, true)))
                {
                    targetNPC = -1;
                    targetCooldown = 0;
                    npc.netUpdate = true;
                }
            }

            if (TerRoguelike.mpClient)
            {
                if (targetPlayer >= 0)
                    return Main.player[targetPlayer];
                if (targetNPC >= 0)
                    return Main.npc[targetNPC];
                return null;
            }

            if (npc.friendly)
            {
                if (targetNPC == -1 || targetPlayer != -1 || targetCooldown <= 0)
                {
                    targetNPC = ClosestNPC(npc.Center, 50000f, false);
                    targetPlayer = -1;
                    targetCooldown = 300;
                    npc.netUpdate = true;
                }
            }
            else if (targetCooldown <= 0)
            {
                targetPlayer = npc.FindClosestPlayer();
                if (targetNPC == -1 && !TerRoguelikeBoss)
                    targetNPC = ClosestNPC(npc.Center, 50000f, true);
                if (Main.player[targetPlayer].dead)
                    targetPlayer = -1;
                if (targetPlayer != -1 && targetNPC != -1)
                {
                    Player p = Main.player[targetPlayer];
                    NPC n = Main.npc[targetNPC];
                    if (npc.Center.DistanceSQ(n.Center) < npc.Center.DistanceSQ(p.Center))
                        targetPlayer = -1;
                    else
                        targetNPC = -1;
                }
                targetCooldown = 300;
                npc.netUpdate = true;
            }
            return targetPlayer != -1 ? Main.player[targetPlayer] : (targetNPC != -1 ? Main.npc[targetNPC] : null);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputVect"></param>
        /// <returns>The position of the closest segment to the input vector. Returns inputVect if you call this when there's no segment in the Segments list </returns>
        public Vector2 ClosestSegment(Vector2 inputVect)
        {
            if (Segments.Count <= 0)
                return inputVect;

            int closest = 0;
            float closestLength = (inputVect - Segments[0].Position).Length();

            for (int i = 1; i < Segments.Count; i++)
            {
                WormSegment segment = Segments[i];
                float distance = (segment.Position - inputVect).Length();
                if (distance < closestLength)
                {
                    closestLength = distance;
                    closest = i;
                }
            }

            Vector2 closestVect = Segments[closest].Position;
            closestVect += (inputVect - closestVect).SafeNormalize(Vector2.UnitY) * Segments[closest].Height * 0.5f;
            return closestVect;
        }
        public Vector2 ClosestPosition(Vector2 fallback, Vector2 origin, NPC npc)
        {
            if (Segments.Count > 0)
            {
                return ClosestSegment(origin);
            }
            if (ExtraIgniteTargetPoints.Count > 0)
            {
                int closest = 0;
                float closestLength = (origin - (ExtraIgniteTargetPoints[0] + npc.Center)).Length();

                for (int i = 1; i < ExtraIgniteTargetPoints.Count; i++)
                {
                    var position = ExtraIgniteTargetPoints[i] + npc.Center;
                    float distance = (position - origin).Length();
                    if (distance < closestLength)
                    {
                        closestLength = distance;
                        closest = i;
                    }
                }
                return ExtraIgniteTargetPoints[closest] + npc.Center;
            }
            return fallback;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="npc"></param>
        /// <param name="point"></param>
        /// <returns>Whether or not the input point is inside any of the calculated segment rectangles</returns>
        public bool IsPointInsideSegment(NPC npc, Point point)
        {
            bool pass = false;
            for (int i = 0; i < Segments.Count; i++)
            {
                WormSegment segment = Segments[i];
                Rectangle segRect = new Rectangle((int)(segment.Position.X - ((i == 0 ? npc.width : segment.Height) / 2)), (int)(segment.Position.Y - ((i == 0 ? npc.height : segment.Height) / 2)), npc.width, npc.height);
                if (segRect.Contains(point))
                {
                    pass = true;
                    break;
                }
            }
            return pass;
        }
        /// <summary>
        /// Tries to get the room that this modNPC spawned from
        /// </summary>
        /// <returns>null if the npc is not from a room, otherwise returns the room</returns>
        public Room GetParentRoom()
        {
            if (!isRoomNPC || sourceRoomListID < 0)
                return null;
            return RoomList[sourceRoomListID];
        }

        public void DiscourageTargetting()
        {
            if (currentUpdate == 1)
            {
                targetCooldown++;
            }
        }

        public void GiveEliteName(int who)
        {
            NPC npc = Main.npc[who];
            string giveName = "";
            if (eliteVars.tainted)
            {
                giveName += Language.GetOrRegister("Mods.TerRoguelike.EliteTainted").Value;
            }
            if (eliteVars.slugged)
            {
                giveName += Language.GetOrRegister("Mods.TerRoguelike.EliteSlugged").Value; ;
            }
            if (eliteVars.burdened)
            {
                giveName += Language.GetOrRegister("Mods.TerRoguelike.EliteBurdened").Value; ;
            }
            if (giveName.Length > 0)
            {
                npc.GivenName = giveName + npc.GivenName + npc.TypeName;
            }
            eliteNamed = true;
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
        public IgnitedStack(int damageToDeal, int owner, int damageCapPerTick = 50)
        {
            Owner = owner;
            if (owner == Main.myPlayer)
                damageToDeal *= Main.player[owner].ModPlayer().forgottenBioWeapon + 1;
            DamageToDeal = damageToDeal;
            DamageCapPerTick = damageCapPerTick;
        }
        public int DamageToDeal = 0;
        public int Owner = -1;
        public int DamageCapPerTick;
    }
    public class BleedingStack
    {
        public BleedingStack(int damageToDeal, int owner)
        {
            Owner = owner;
            if (owner == Main.myPlayer)
                damageToDeal *= Main.player[owner].ModPlayer().forgottenBioWeapon + 1;
            DamageToDeal = damageToDeal;
        }
        public int DamageToDeal = 0;
        public int Owner = -1;
    }
}
