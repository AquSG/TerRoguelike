﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Animations;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using TerRoguelike.Managers;
using TerRoguelike.Particles;
using TerRoguelike.Projectiles;
using TerRoguelike.Systems;
using TerRoguelike.Utilities;
using static TerRoguelike.Managers.TextureManager;
using static TerRoguelike.Schematics.SchematicManager;
using static TerRoguelike.Systems.MusicSystem;
using static TerRoguelike.Systems.RoomSystem;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using Terraria.Graphics.Effects;
using Steamworks;

namespace TerRoguelike.NPCs.Enemy.Boss
{
    public class PharaohSpirit : BaseRoguelikeNPC
    {
        public Entity target;
        public Vector2 spawnPos;
        public bool ableToHit = true;
        public bool canBeHit = true;
        public override int modNPCID => ModContent.NPCType<PharaohSpirit>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Desert"] };
        public override int CombatStyle => -1;
        public int currentFrame;
        public bool CollisionPass = false;
        public float eyeGlowInterpolant = 0;

        Texture2D glowTex;
        public SoundStyle LocustSwarm = new SoundStyle("TerRoguelike/Sounds/LocustSwarm");
        public SlotId LocustSlot;
        public int soundMoveDirection = 0;

        public int deadTime = 0;
        public int cutsceneDuration = 120;
        public int deathCutsceneDuration = 120;

        public static Attack None = new Attack(0, 0, 180);
        public static Attack Sandnado = new Attack(1, 30, 120);
        public static Attack Locust = new Attack(2, 30, 360);
        public static Attack SandTurret = new Attack(3, 30, 180);
        public static Attack Tendril = new Attack(4, 30, 510);
        public static Attack Summon = new Attack(5, 40, 120);
        public float defaultMaxVelocity = 4;
        public float defaultAcceleration = 0.08f;
        public int sandnadoCooldown = 0;
        public int locustTelegraph = 60;
        public int locustFireRate = 7;
        public int locustCount = 1;
        public List<int> sandTurretFireTimes = new List<int>() { 0, 90 } ;
        public Vector2[] summonSpawnPositions = { new Vector2(-1), new Vector2(-1) };
        public int[] summonDesiredEnemies = { 0, 0 };
        public int summonTelegraph = 60;
        public int summonStartup = 15;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 13;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 30;
            NPC.height = 76;
            NPC.aiStyle = -1;
            NPC.damage = 30;
            NPC.lifeMax = 30000;
            NPC.HitSound = SoundID.NPCHit23;
            NPC.knockBackResist = 0f;
            modNPC.drawCenter = new Vector2(0, 8);
            modNPC.IgnoreRoomWallCollision = true;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            glowTex = TexDict["PharaohSpiritGlow"].Value;
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.Opacity = 0;
            NPC.immortal = true;
            NPC.dontTakeDamage = true;
            currentFrame = 0;
            NPC.localAI[0] = -(cutsceneDuration + 30);
            NPC.direction = -1;
            NPC.spriteDirection = -1;
            spawnPos = NPC.Center;
            NPC.ai[2] = None.Id;
            ableToHit = false;
        }
        public override void PostAI()
        {
            NPC.spriteDirection = NPC.direction;
            if (currentFrame > 3)
                eyeGlowInterpolant += 0.017f;
            else
                eyeGlowInterpolant -= 0.017f;
            eyeGlowInterpolant = MathHelper.Clamp(eyeGlowInterpolant, 0, 1);
            if (deadTime > 0)
            {
                if (SoundEngine.TryGetActiveSound(LocustSlot, out var sound) && sound.IsPlaying)
                {
                    sound.Volume -= 0.016f;
                }
            }
            else if (NPC.localAI[1] > 0)
            {
                if (SoundEngine.TryGetActiveSound(LocustSlot, out var sound) && sound.IsPlaying)
                {
                    if (NPC.localAI[1] < 150)
                    {
                        Vector2 pos = (Vector2)sound.Position;
                        if (soundMoveDirection == -1 ? (pos.X > spawnPos.X) : (pos.X < spawnPos.X))
                            sound.Position += new Vector2(4 * soundMoveDirection, 0);
                    }
                    else if (NPC.localAI[1] > 200)
                    {
                        sound.Position += new Vector2(3 * soundMoveDirection, 0);
                        sound.Volume -= 0.0025f;
                    }
                    NPC.localAI[1]++;
                }
            }
        }
        public override void AI()
        {
            if (deadTime > 0)
            {
                CheckDead();
                return;
            }
            if (modNPC.isRoomNPC && NPC.localAI[0] == -(cutsceneDuration + 30))
            {
                SetBossTrack(CrimsonVesselTheme);
            }

            ableToHit = NPC.localAI[0] >= 0;
            canBeHit = true;

            if (NPC.localAI[0] < 0)
            {
                target = modNPC.GetTarget(NPC);

                if (NPC.localAI[0] == -cutsceneDuration)
                {
                    CutsceneSystem.SetCutscene(spawnPos, cutsceneDuration, 30, 30, 2.5f);
                }
                NPC.localAI[0]++;
                
                if (NPC.localAI[0] == -30)
                {
                    NPC.Opacity = 1;
                    NPC.immortal = false;
                    NPC.dontTakeDamage = false;
                    NPC.ai[1] = 0;
                }
            }
            else
            {
                NPC.localAI[0]++;
                BossAI();
                NPC.rotation = NPC.rotation.AngleLerp(MathHelper.Clamp(NPC.velocity.X * 0.1f, -MathHelper.PiOver2 * 0.15f, MathHelper.PiOver2 * 0.15f), 0.25f);
            }
        }
        public void BossAI()
        {
            target = modNPC.GetTarget(NPC);

            NPC.ai[1]++;
            if (sandnadoCooldown > 0)
                sandnadoCooldown--;

            if (NPC.ai[0] == None.Id)
            {
                if (target != null)
                {
                    if (target.Center.X > NPC.Center.X)
                        NPC.direction = 1;
                    else
                        NPC.direction = -1;
                }
                else
                {
                    NPC.direction = Math.Sign(NPC.velocity.X);
                    if (NPC.direction == 0)
                        NPC.direction = -1;
                }
                    

                if (NPC.ai[1] >= None.Duration)
                {
                    ChooseAttack();
                }
                else
                {
                    NPC.velocity *= 0.98f;
                    if (target != null)
                    {
                        Vector2 targetPos = target.Center;
                        NPC.velocity += (targetPos - NPC.Center).SafeNormalize(Vector2.UnitY) * defaultAcceleration;
                    }
                    NPC.velocity.Y += (float)Math.Cos(NPC.localAI[0] / 20) * 0.04f;
                    if (NPC.velocity.Length() > defaultMaxVelocity)
                    {
                        NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * defaultMaxVelocity;
                    }
                }
            }

            if (NPC.ai[0] == Sandnado.Id)
            {
                NPC.velocity *= 0.9f;
                if (NPC.ai[1] == 30)
                {
                    Room room = modNPC.sourceRoomListID >= 0 ? RoomList[modNPC.sourceRoomListID] : null;
                    int nadoCount = 4;
                    for (int i = 0; i < nadoCount; i++)
                    {
                        float completion = (i + 1f) / (nadoCount + 1f);
                        Vector2 projPos = room != null ? room.RoomPosition16 + new Vector2(room.RoomDimensions16.X * completion, room.RoomDimensions16.Y * 0.5f) : spawnPos + new Vector2(-1000 + (2000 * completion), 0);
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), projPos, Vector2.Zero, ModContent.ProjectileType<Sandnado>(), NPC.damage, 0);
                    }
                }
                if (NPC.ai[1] >= Sandnado.Duration)
                {
                    sandnadoCooldown = 540;
                    currentFrame = 0;
                    NPC.localAI[0] = 0;
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    //NPC.ai[2] = Sandnado.Id;
                }
            }
            else if (NPC.ai[0] == Locust.Id)
            {
                if (NPC.ai[1] < locustTelegraph)
                {

                }
                else
                {
                    int time = (int)NPC.ai[1] - locustTelegraph;
                    if (time % locustFireRate == 0)
                    {
                        Room room = modNPC.isRoomNPC ? RoomList[modNPC.sourceRoomListID] : null;
                        Vector2 anchor = room != null ? room.RoomPosition16 + new Vector2(NPC.direction > 0 ? 0 : room.RoomDimensions16.X, room.RoomDimensions16.Y * 0.5f) : spawnPos + new Vector2(NPC.direction > 0 ? 1500 : -1500, 0);
                        if (NPC.ai[1] == locustTelegraph)
                        {
                            LocustSlot = SoundEngine.PlaySound(LocustSwarm with { Volume = 0.4f }, anchor);
                            NPC.localAI[1] = 1;
                            soundMoveDirection = NPC.direction;
                        }
                        Vector2 projVel = new Vector2(8 * NPC.direction, 0);
                        float height = room != null ? (room.RoomDimensions16.Y - 48) * 0.5f : 500; 
                        for (int i = 0; i < locustCount; i++)
                        {
                            Vector2 projPos = anchor + new Vector2(0, Main.rand.NextFloat(-height, height));
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), projPos, projVel, ModContent.ProjectileType<Locust>(), NPC.damage, 0);
                        }
                    }
                }
                if (NPC.ai[1] >= Locust.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Locust.Id;
                }
            }
            else if (NPC.ai[0] == SandTurret.Id)
            {
                if (target != null)
                {
                    if (target.Center.X > NPC.Center.X)
                        NPC.direction = 1;
                    else
                        NPC.direction = -1;
                }
                else
                {
                    NPC.direction = Math.Sign(NPC.velocity.X);
                    if (NPC.direction == 0)
                        NPC.direction = -1;
                }

                for (int i = 0; i < sandTurretFireTimes.Count; i++)
                {
                    int fireTime = sandTurretFireTimes[i];
                    if (NPC.ai[1] == fireTime)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_EtherianPortalSpawnEnemy with { Volume = 1f, Variants =  new ReadOnlySpan<int>(0) }, NPC.Top);
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<SandTurret>(), NPC.damage, 0);
                    }
                }
                if (NPC.ai[1] >= SandTurret.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = SandTurret.Id;
                }
            }
            else if (NPC.ai[0] == Tendril.Id)
            {
                NPC.velocity *= 0.9f;
                if (NPC.ai[1] == 15)
                {
                    Vector2 anchor = NPC.Center + new Vector2(NPC.direction * 16, 0);
                    for (int i = -2; i <= 2; i++)
                    {
                        Vector2 offset = new Vector2(30 * NPC.direction, 0);
                        offset = offset.RotatedBy(MathHelper.Pi * 0.15f * i);
                        offset.X *= 0.5f;
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), anchor + offset, offset.SafeNormalize(Vector2.UnitX * NPC.direction) * 3, ModContent.ProjectileType<DarkTendril>(), NPC.damage, 0);
                    }
                }
                if (NPC.ai[1] >= Tendril.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Tendril.Id;
                }
            }
            else if (NPC.ai[0] == Summon.Id)
            {
                NPC.velocity *= 0.9f;
                if (NPC.ai[1] == summonStartup)
                {
                    SoundEngine.PlaySound(SoundID.Item44 with { Volume = 0.6f }, NPC.Center);

                    for (int i = 0; i < summonSpawnPositions.Length; i++)
                    {
                        summonDesiredEnemies[i] = Main.rand.NextBool() ? ModContent.NPCType<DesertSpirit>() : ModContent.NPCType<Lamia>();
                        NPC dummyNPC = new NPC();
                        dummyNPC.type = summonDesiredEnemies[i];
                        dummyNPC.SetDefaults(summonDesiredEnemies[i]);
                        Rectangle dummyRect = new Rectangle(0, 0, dummyNPC.width, dummyNPC.height);

                        int direction = i == 0 ? -1 : 1;
                        float distanceBeside = 112f * direction;
                        Rectangle plannedRect = new Rectangle((int)(NPC.Center.X + distanceBeside - (dummyRect.Width * 0.5f)), (int)(NPC.Center.Y - (dummyRect.Height * 0.5f)), dummyRect.Width, dummyRect.Height);
                        if (modNPC.isRoomNPC)
                        {
                            plannedRect = RoomList[modNPC.sourceRoomListID].CheckRectWithWallCollision(plannedRect);
                        }
                        Vector2 position = plannedRect.Center.ToVector2();

                        Point tilePos = new Vector2(position.X, plannedRect.Bottom).ToTileCoordinates();

                        if (ParanoidTileRetrieval(tilePos.X, tilePos.Y).IsTileSolidGround(true))
                        {
                            bool found = false;
                            for (int y = 0; y < 25; y++)
                            {
                                for (int d = -1; d <= 1; d += 2)
                                {
                                    if (!ParanoidTileRetrieval(tilePos.X, tilePos.Y + (y * d)).IsTileSolidGround(true))
                                    {
                                        float offset = y * d * 16f;
                                        if (modNPC.isRoomNPC)
                                        {
                                            Rectangle rectCheck = new Rectangle(plannedRect.X, (int)(plannedRect.Y + offset), plannedRect.Width, plannedRect.Height);
                                            rectCheck = RoomList[modNPC.sourceRoomListID].CheckRectWithWallCollision(rectCheck);
                                            Vector2 posCheck = new Vector2(rectCheck.Center.X, rectCheck.Bottom);

                                            Point tilePosCheck = posCheck.ToTileCoordinates();
                                            if (!ParanoidTileRetrieval(tilePosCheck.X, tilePosCheck.Y).IsTileSolidGround(true))
                                            {
                                                found = true;
                                                position.Y += rectCheck.Y - plannedRect.Y;
                                            }
                                        }
                                        else
                                        {
                                            found = true;
                                            position.Y += offset;
                                        }
                                    }
                                }
                                if (found)
                                    break;
                            }
                            if (found)
                                summonSpawnPositions[i] = position;
                        }
                        else
                        {
                            summonSpawnPositions[i] = position;
                        }
                    }
                }
                if (NPC.ai[1] >= summonStartup && NPC.ai[1] < summonTelegraph + summonStartup)
                {
                    int time = 20;
                    Vector2 startPos = NPC.Center + new Vector2(32 * NPC.direction, -4);
                    float completion = (NPC.ai[1] - summonStartup) / summonTelegraph;
                    float curveMultiplier = 1f - (float)Math.Pow(Math.Abs(completion - 0.5f) * 2, 2);
                    for (int i = 0; i < summonSpawnPositions.Length; i++)
                    {
                        Vector2 endPos = summonSpawnPositions[i];
                        if (endPos == new Vector2(-1))
                            continue;
                        endPos += new Vector2(0, 16);

                        Vector2 particlePos = startPos + ((endPos - startPos) * completion) + (Vector2.UnitY * -32 * curveMultiplier);
                        particlePos += new Vector2(Main.rand.NextFloat(-10, 10), Main.rand.NextFloat(-10, 10));

                        ParticleManager.AddParticle(new Square(particlePos, Vector2.Zero, time, Color.HotPink, new Vector2((4f + Main.rand.NextFloat(-0.5f, 0.5f)) * 0.45f), 0, 0.96f, time, true));
                    }
                }
                else if (NPC.ai[1] == summonTelegraph + summonStartup)
                {
                    SoundEngine.PlaySound(SoundID.DD2_WitherBeastAuraPulse with { Volume = 1f }, NPC.Center);
                    SoundEngine.PlaySound(SoundID.DD2_SkeletonSummoned with { Volume = 1f }, NPC.Center);
                    for (int i = 0; i < summonSpawnPositions.Length; i++)
                    {
                        Vector2 pos = summonSpawnPositions[i];
                        if (pos == new Vector2(-1))
                            continue;

                        SpawnManager.SpawnEnemy(summonDesiredEnemies[i], pos, modNPC.sourceRoomListID, 60, 0.45f);

                        summonSpawnPositions[i] = new Vector2(-1);
                    }
                }
                if (NPC.ai[1] >= Summon.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Summon.Id;
                }
            }

            if (NPC.ai[0] != None.Id)
            {
                NPC.velocity *= 0.95f;
            }
        }

        public void ChooseAttack()
        {
            NPC.ai[1] = 0;
            int chosenAttack = 0;

            List<Attack> potentialAttacks = new List<Attack>() { Sandnado, Locust, SandTurret, Tendril, Summon };
            potentialAttacks.RemoveAll(x => x.Id == (int)NPC.ai[2]);
            if (sandnadoCooldown > 0)
                potentialAttacks.RemoveAll(x => x.Id == Sandnado.Id);

            int totalWeight = 0;
            for (int i = 0; i < potentialAttacks.Count; i++)
            {
                totalWeight += potentialAttacks[i].Weight;
            }
            int chosenRandom = Main.rand.Next(totalWeight);

            for (int i = potentialAttacks.Count - 1; i >= 0; i--)
            {
                Attack attack = potentialAttacks[i];
                chosenRandom -= attack.Weight;
                if (chosenRandom < 0)
                {
                    chosenAttack = attack.Id;
                    break;
                }
            }
            NPC.ai[0] = chosenAttack;
        }

        public override bool? CanFallThroughPlatforms()
        {
            return true;
        }
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            return ableToHit;
        }
        public override bool CanHitNPC(NPC target)
        {
            return ableToHit;
        }
        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            return canBeHit ? null : false;
        }

        public override bool CheckDead()
        {
            if (deadTime >= deathCutsceneDuration - 30)
            {
                return true;
            }

            NPC.ai[0] = None.Id;
            NPC.ai[1] = 1;

            modNPC.OverrideIgniteVisual = true;
            NPC.life = 1;
            NPC.immortal = true;
            NPC.dontTakeDamage = true;
            NPC.active = true;


            if (deadTime == 0)
            {
                NPC.rotation = 0;
                NPC.velocity *= 0;
                modNPC.ignitedStacks.Clear();
                if (modNPC.isRoomNPC)
                {
                    ActiveBossTheme.endFlag = true;
                    Room room = RoomList[modNPC.sourceRoomListID];
                    room.bossDead = true;
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        if (i == NPC.whoAmI)
                            continue;

                        NPC childNPC = Main.npc[i];
                        if (childNPC == null)
                            continue;
                        if (!childNPC.active)
                            continue;

                        TerRoguelikeGlobalNPC modChildNPC = childNPC.ModNPC();
                        if (modChildNPC == null)
                            continue;
                        if (modChildNPC.isRoomNPC && modChildNPC.sourceRoomListID == modNPC.sourceRoomListID)
                        {
                            childNPC.StrikeInstantKill();
                            childNPC.active = false;
                        }
                    }
                }
                CutsceneSystem.SetCutscene(NPC.Center, deathCutsceneDuration, 30, 30, 2.5f);
            }
            deadTime++;

            if (deadTime >= deathCutsceneDuration - 30)
            {
                NPC.immortal = false;
                NPC.dontTakeDamage = false;
                NPC.StrikeInstantKill();
            }

            return deadTime >= cutsceneDuration - 30;
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            Color color = new Color(222, 108, 48) * 0.7f;
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)NPC.lifeMax * 400; i++)
                {
                    Dust d = Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Sandstorm)];
                    d.color = color;
                    d.noGravity = true;
                    d.scale = 1.5f;
                    d.fadeIn = 0.7f;
                    d.velocity *= 3f;
                }
            }
            else if (deadTime > 0)
            {
                SoundEngine.PlaySound(SoundID.NPCDeath39, NPC.Center);
                for (int i = 0; i < 60; i++)
                {
                    Dust d = Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Sandstorm)];
                    d.color = color;
                    d.noGravity = true;
                    d.scale = 1.5f;
                    d.fadeIn = 0.7f;
                    d.velocity *= 3f;
                }
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 960, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 961, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 963, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 40f), NPC.velocity, 962, NPC.scale);
            }
        }
        public override void OnKill()
        {
            
        }
        public override void FindFrame(int frameHeight)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;

            if (deadTime > 0)
            {
                currentFrame = 10;
            }
            else if (NPC.localAI[0] < 0)
            {
                currentFrame = 0;
            }
            else if (NPC.ai[0] == None.Id)
            {
                currentFrame = (int)(NPC.localAI[0] * 0.1f) % 4;
            }
            else if (NPC.ai[0] == Sandnado.Id)
            {
                if (NPC.ai[1] < 30)
                {
                    currentFrame = (int)(NPC.ai[1] / 10) + 4;
                }
                else if (NPC.ai[1] >= 30 && NPC.ai[1] < 80)
                {
                    currentFrame = (int)((NPC.ai[1] / 10 + 2) % 4) + 5;
                }
                else if (NPC.ai[1] >= 80)
                {
                    currentFrame = (int)((NPC.ai[1] - 80) / 10) + 9;
                }
            }
            else if (NPC.ai[0] == Locust.Id)
            {
                if (NPC.ai[1] < 15 || Locust.Duration - NPC.ai[1] < 15)
                    currentFrame = 4;
                else
                    currentFrame = (int)((NPC.ai[1] / 10 - 2) % 4) + 5;
            }
            else if (NPC.ai[0] == SandTurret.Id)
            {
                for (int i = 0; i < sandTurretFireTimes.Count; i++)
                {
                    int fireTime = sandTurretFireTimes[i];
                    if (NPC.ai[1] < fireTime)
                        continue;

                    if (NPC.ai[1] < fireTime + 60)
                    {
                        currentFrame = NPC.ai[1] - fireTime < 15 ? 9 : 10;
                    }
                    else if (NPC.ai[1] < fireTime + 90)
                    {
                        currentFrame = NPC.ai[1] - fireTime < 75 ? 6 : (i == (sandTurretFireTimes.Count - 1) ? 4 : 12);
                    }
                }
            }
            else if (NPC.ai[0] == Tendril.Id)
            {
                if (NPC.ai[1] < 15 || Tendril.Duration - NPC.ai[1] < 15)
                    currentFrame = 4;
                else
                    currentFrame = (int)((NPC.ai[1] / 10 - 2) % 4) + 5;
            }
            else if (NPC.ai[0] == Summon.Id)
            {
                if (NPC.ai[1] < 30)
                {
                    currentFrame = (int)(NPC.ai[1] / 10) + 4;
                }
                else if (NPC.ai[1] >= 30 && NPC.ai[1] < 80)
                {
                    currentFrame = (int)((NPC.ai[1] / 10 + 2) % 4) + 5;
                }
                else if (NPC.ai[1] >= 80)
                {
                    currentFrame = (int)((NPC.ai[1] - 80) / 10) + 9;
                }
            }

            NPC.frame = new Rectangle(0, currentFrame * frameHeight, tex.Width, frameHeight);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;

            Vector2 drawPos = NPC.Center + modNPC.drawCenter - Main.screenPosition;
            Main.EntitySpriteDraw(tex, drawPos, NPC.frame, Color.Lerp(drawColor, Color.White, 0.25f), NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
            float finalEyeGlowInterpolant = eyeGlowInterpolant * (0.3f * ((float)Math.Cos(Main.GlobalTimeWrappedHourly * 16)) + 0.7f);
            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(glowTex, drawPos + (Vector2.UnitX * finalEyeGlowInterpolant * 1).RotatedBy(i * MathHelper.PiOver2), NPC.frame, Color.White * 0.5f, NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
            }
            
            return false;
        }
    }
}