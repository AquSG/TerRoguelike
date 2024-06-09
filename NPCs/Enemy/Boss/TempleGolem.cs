using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Renderers;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using TerRoguelike.Managers;
using TerRoguelike.Particles;
using TerRoguelike.Projectiles;
using TerRoguelike.Systems;
using TerRoguelike.Utilities;
using TerRoguelike.World;
using static Terraria.GameContent.PlayerEyeHelper;
using static TerRoguelike.Managers.TextureManager;
using static TerRoguelike.Schematics.SchematicManager;
using static TerRoguelike.Systems.MusicSystem;
using static TerRoguelike.Systems.RoomSystem;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using static TerRoguelike.Systems.EnemyHealthBarSystem;
using static TerRoguelike.MainMenu.TerRoguelikeMenu;

namespace TerRoguelike.NPCs.Enemy.Boss
{
    public class TempleGolem : BaseRoguelikeNPC
    {
        public Entity target;
        public Vector2 spawnPos;
        public bool ableToHit = true;
        public bool canBeHit = true;
        public override int modNPCID => ModContent.NPCType<TempleGolem>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Temple"] };
        public override int CombatStyle => -1;
        public int currentFrame;
        public SlotId RumbleSlot;
        public Texture2D eyeTex;
        public Texture2D lightTex;
        public Texture2D godRayTex;
        public static readonly SoundStyle DingSound = new SoundStyle("TerRoguelike/Sounds/Ding");
        public static readonly SoundStyle GolemAwaken = new SoundStyle("TerRoguelike/Sounds/GolemAwaken");
        public List<Vector2> eyePositions = [];

        public int deadTime = 0;
        public int cutsceneDuration = 160;
        public int deathCutsceneDuration = 180;
        List<GodRay> deathGodRays = [];

        public static Attack None = new Attack(0, 0, 30);
        public static Attack Laser = new Attack(1, 30, 180);
        public static Attack SpikeBall = new Attack(2, 30, 180);
        public static Attack Flame = new Attack(3, 35, 180);
        public static Attack DartTrap = new Attack(4, 30, 180);
        public static Attack Boulder = new Attack(5, 25, 180);
        public static Attack Summon = new Attack(6, 18, 180);
        public int laserWindup = 60;
        public int laserFireRate = 7;
        public int spikeBallWindup = 60;
        public int spikeBallFireRate = 4;
        public int flameWindup = 60;
        public int flameFireRate = 4;
        public int dartTrapWindup = 30;
        public int dartTrapFireRate = 15;
        public int boulderWindup = 60;
        public int summonWindup = 80;
        public int summonChooseAttackCooldown = 600;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 2;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 70;
            NPC.height = 70;
            NPC.aiStyle = -1;
            NPC.damage = 30;
            NPC.lifeMax = 32000;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.knockBackResist = 0f;
            modNPC.drawCenter = new Vector2(0, 0);
            modNPC.IgnoreRoomWallCollision = true;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            eyeTex = TexDict["TempleGolemEyes"];
            lightTex = TexDict["TempleGolemGlow"];
            godRayTex = TexDict["GodRay"];
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.immortal = true;
            NPC.dontTakeDamage = true;
            currentFrame = 0;
            NPC.localAI[0] = -(cutsceneDuration + 30);
            NPC.direction = -1;
            NPC.spriteDirection = -1;
            NPC.Center += Vector2.UnitY * NPC.height * 0.5f;
            spawnPos = NPC.Center;
            NPC.ai[2] = None.Id;
            ableToHit = false;
            NPC.localAI[2] = Main.rand.Next(0, 3);

            NPC.Center = TileCollidePositionInLine(NPC.Center, NPC.Center + new Vector2(0, -1000));
            NPC.Center += new Vector2(0, 45);

            for (int i = -1; i <= 1; i += 2)
            {
                eyePositions.Add(new Vector2(-15 * i - 1, -3) + NPC.Center + modNPC.drawCenter);
            }
        }
        public override void PostAI()
        {
            if (SoundEngine.TryGetActiveSound(RumbleSlot, out var sound) && sound.IsPlaying)
            {
                if (NPC.ai[0] != SpikeBall.Id || (NPC.ai[0] == SpikeBall.Id && NPC.ai[1] >= spikeBallWindup))
                {
                    sound.Volume *= 0.99f;
                    sound.Volume -= 0.001f;
                    if (sound.Volume <= 0)
                        sound.Stop();
                }
            }
        }
        public override void AI()
        {
            NPC.localAI[3] = 0;
            if (summonChooseAttackCooldown > 0)
                summonChooseAttackCooldown--;
            if (modNPC.isRoomNPC)
            {
                int checkType = ModContent.NPCType<LihzahrdSentry>();
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active || npc.life <= 0 || npc.friendly || npc.ModNPC() == null)
                        continue;
                    if (npc.type == checkType && npc.ModNPC().sourceRoomListID == modNPC.sourceRoomListID)
                    {
                        NPC.localAI[3]++;
                        summonChooseAttackCooldown = 600;
                    }
                }
            }
            if (deadTime > 0)
            {
                CheckDead();
                return;
            }
            if (modNPC.isRoomNPC && NPC.localAI[0] == -(cutsceneDuration + 30))
            {
                SetBossTrack(TempleGolemTheme);
            }

            ableToHit = NPC.localAI[0] >= 0 && deadTime == 0;
            canBeHit = true;

            if (NPC.localAI[0] < 0)
            {
                target = modNPC.GetTarget(NPC);

                if (NPC.localAI[0] == -cutsceneDuration)
                {
                    CutsceneSystem.SetCutscene(NPC.Center, cutsceneDuration, 30, 30, 2.5f);
                }
                NPC.localAI[0]++;

                if (NPC.localAI[0] == -130)
                {
                    SoundEngine.PlaySound(GolemAwaken with { Volume = 0.5f, Pitch = -0.4f }, NPC.Center);
                }
                if (NPC.localAI[0] == -85)
                {
                    SoundEngine.PlaySound(DingSound with { Volume = 0.07f, Pitch = -1f, MaxInstances = 2 }, NPC.Center);
                    SoundEngine.PlaySound(DingSound with { Volume = 0.07f, Pitch = -0.8f, MaxInstances = 2 }, NPC.Center);
                    for (int i = 0; i < eyePositions.Count; i++)
                    {
                        Vector2 particlePos = eyePositions[i];
                        for (int p = 0; p < 2; p++)
                        {
                            ParticleManager.AddParticle(new ThinSpark(
                                particlePos, Vector2.Zero, 40, Color.OrangeRed, new Vector2(0.1f, 0.24f), MathHelper.PiOver2 * p, true, false));
                        }
                    }
                }

                if (NPC.localAI[0] == -30)
                {
                    NPC.immortal = false;
                    NPC.dontTakeDamage = false;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Summon.Id;
                    enemyHealthBar = new EnemyHealthBar([NPC.whoAmI], NPC.FullName);
                }
            }
            else
            {
                NPC.localAI[0]++;
                BossAI();
            }
        }
        public void BossAI()
        {
            bool hardMode = difficulty == Difficulty.BloodMoon;

            target = modNPC.GetTarget(NPC);
            NPC.ai[1]++;
            NPC.velocity *= 0.98f;

            if (NPC.ai[0] == None.Id)
            {
                if (NPC.ai[1] >= None.Duration)
                {
                    ChooseAttack();
                }
                else
                {
                    if (hardMode)
                        NPC.ai[1]++;
                }
            }

            if (NPC.ai[0] == Laser.Id)
            {
                Vector2 targetPos = target != null ? target.Center : spawnPos;
                Color particleColor = Color.Lerp(Color.Goldenrod, Color.White, 0.3f);
                if (NPC.ai[1] < laserWindup)
                {
                    if (NPC.ai[1] == 0)
                    {
                        SoundEngine.PlaySound(SoundID.Item15 with { Volume = 1f, Pitch = -0.5f, PitchVariance = 0 }, NPC.Center);
                    }
                    if (NPC.ai[1] % 30 == 0)
                    {
                        SoundEngine.PlaySound(SoundID.Item13 with { Volume = 0.8f, Pitch = -0.05f, PitchVariance = 0 }, NPC.Center);
                    }
                    for (int i = 0; i < eyePositions.Count; i++)
                    {
                        if (Main.rand.NextBool())
                        {
                            Vector2 offset = Main.rand.NextVector2CircularEdge(24f, 24f);
                            ParticleManager.AddParticle(new Square(
                                eyePositions[i] + offset, -offset * 0.1f, 30, particleColor, new Vector2(Main.rand.NextFloat(0.5f, 0.75f)), offset.ToRotation(), 0.96f, 30, true));
                        }
                    }
                }
                else
                {
                    int time = (int)NPC.ai[1] - laserWindup;
                    if (time % laserFireRate == 0)
                    {
                        SoundEngine.PlaySound(SoundID.Item91 with { Volume = 0.8f, MaxInstances = 2 }, NPC.Center);
                        for (int i = 0; i < eyePositions.Count; i++)
                        {
                            Vector2 pos = eyePositions[i];
                            float rotToTarget = (targetPos - pos).ToRotation();
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), pos, rotToTarget.ToRotationVector2() * 10, ModContent.ProjectileType<LihzahrdLaser>(), NPC.damage, 0, -1, 1);

                            for (int j = 0; j < 5; j++)
                            {
                                Vector2 offset = Main.rand.NextVector2CircularEdge(24f, 24f);
                                ParticleManager.AddParticle(new Square(
                                    eyePositions[i] + offset, -offset * 0.1f, 30, particleColor, new Vector2(Main.rand.NextFloat(0.75f, 1f)), offset.ToRotation(), 0.96f, 30, true));
                            }
                        }
                    }
                }
                
                if (NPC.ai[1] >= Laser.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = -20;
                    NPC.ai[2] = Laser.Id;
                }
            }
            else if (NPC.ai[0] == SpikeBall.Id)
            {
                Vector2 leftBound = TileCollidePositionInLine(NPC.Center + new Vector2(0, 160), NPC.Center + new Vector2(-1000, 160)) + Vector2.UnitX * 8;
                Vector2 rightBound = TileCollidePositionInLine(leftBound, leftBound + new Vector2(2000, 0)) - Vector2.UnitX * 8;
                float boundWidth = leftBound.Distance(rightBound);
                float thirdWidth = boundWidth * 0.3333f;
                if (NPC.ai[1] < spikeBallWindup)
                {
                    if (NPC.ai[1] == 0)
                    {
                        List<int> potentialThirds = [0, 1, 2];
                        potentialThirds.RemoveAll(x => x == (int)NPC.localAI[2]);
                        NPC.localAI[2] = potentialThirds[Main.rand.Next(potentialThirds.Count)];
                        RumbleSlot = SoundEngine.PlaySound(TerRoguelikeWorld.EarthTremor with { Volume = 1f }, new Vector2(leftBound.X + (thirdWidth * NPC.localAI[2]) + thirdWidth * 0.5f, NPC.position.Y));
                    }
                }
                else
                {
                    if (NPC.ai[1] % spikeBallFireRate == 0)
                    {
                        Vector2 projSpawnPos = new Vector2(leftBound.X + (thirdWidth * NPC.localAI[2]) + Main.rand.NextFloat(thirdWidth), NPC.Center.Y);
                        projSpawnPos = TileCollidePositionInLine(projSpawnPos, projSpawnPos + new Vector2(0, -240)) - Vector2.UnitY * 16;
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), projSpawnPos, new Vector2(0, -1), ModContent.ProjectileType<LihzahrdSpikeBall>(), NPC.damage, 0, -1, 7);
                    }
                    
                }
                if (NPC.localAI[1] >= 0)
                {
                    NPC.localAI[1] = -(2 + ((int)NPC.ai[1] / spikeBallWindup) * 2);
                    Vector2 particlePos = new Vector2(leftBound.X + (thirdWidth * NPC.localAI[2]) + Main.rand.NextFloat(thirdWidth), NPC.Center.Y);
                    particlePos = TileCollidePositionInLine(particlePos, particlePos + new Vector2(0, -240)) - Vector2.UnitY * 16;
                    ParticleManager.AddParticle(new Debris(
                        particlePos, Vector2.UnitY * Main.rand.NextFloat(0.75f, 1.25f),
                        80, Color.DarkOrange * 0.875f, new Vector2(0.5f), Main.rand.Next(3), Main.rand.NextFloat(MathHelper.TwoPi),
                        Main.rand.NextBool() ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0.1f, 7f, 60),
                        ParticleManager.ParticleLayer.BehindTiles);
                }
                else
                    NPC.localAI[1]++;

                if (NPC.ai[1] >= SpikeBall.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = -15;
                    NPC.ai[2] = SpikeBall.Id;
                    NPC.ai[3] = 0;
                    NPC.localAI[1] = 0;
                }
            }
            else if (NPC.ai[0] == Flame.Id)
            {
                Vector2 targetPos = target != null ? target.Center : spawnPos;
                Vector2 projSpawnPos = NPC.Center + new Vector2(0, 34);
                float rotToTarget = (targetPos - projSpawnPos).ToRotation();
                currentFrame = 1;
                if (NPC.ai[1] < flameWindup)
                {
                    if (NPC.ai[1] == 0)
                    {
                        NPC.ai[3] = MathHelper.PiOver2;
                        int soundCount = 2; // play more so it's a higher volume lol
                        for (int i = 0; i < soundCount; i++)
                        {
                            SoundEngine.PlaySound(SoundID.DD2_BetsySummon with { Volume = 1f, Variants = [2], MaxInstances = soundCount, PitchVariance = 0, Pitch = -0.4f }, projSpawnPos);
                        }
                    }
                    if (NPC.ai[1] < flameWindup - 5)
                    {
                        if (target != null && NPC.ai[1] < flameWindup - 10)
                            NPC.ai[3] = rotToTarget;
                        Color particleColor = Color.Lerp(Color.Red, Color.Yellow, Main.rand.NextFloat());
                        Vector2 offset = Main.rand.NextVector2CircularEdge(30f, 20f);
                        offset.Y *= (Math.Abs(offset.X) / 45) + 0.33f;
                        if (Main.rand.NextBool(5))
                            offset.Y *= Main.rand.NextFloat(1.2f, 1.7f);
                        ParticleManager.AddParticle(new Square(
                            projSpawnPos + offset, -offset * 0.1f, 30, particleColor, new Vector2(Main.rand.NextFloat(0.8f, 1.3f)), offset.ToRotation(), 0.96f, 30, true));
                    }
                        
                }
                else
                {
                    int time = (int)NPC.ai[1] - flameWindup;
                    if (time % flameFireRate == 0)
                    {
                        float slowdownCone = MathHelper.PiOver4 * 0.55f;
                        float angleBetween = MathHelper.Clamp(Math.Abs(AngleSizeBetween(NPC.ai[3], rotToTarget)), 0, slowdownCone);
                        if (target != null)
                            NPC.ai[3] = NPC.ai[3].AngleTowards(rotToTarget, 0.075f * (angleBetween / slowdownCone));
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), projSpawnPos, NPC.ai[3].ToRotationVector2() * 7, ModContent.ProjectileType<Flames>(), NPC.damage, 0, -1, 0, 0.24f);
                    }
                    if (time % 10 == 0)
                    {
                        SoundEngine.PlaySound(SoundID.Item34 with { Volume = 1f, MaxInstances = 2 }, projSpawnPos);
                        if (time == 0)
                            SoundEngine.PlaySound(SoundID.DD2_BetsyFlameBreath with { Volume = 1 }, projSpawnPos);
                        else
                            SoundEngine.PlaySound(SoundID.DD2_BetsyFlameBreath with { Volume = 0.5f, MaxInstances = 10, PitchVariance = 0.1f }, projSpawnPos);
                        
                    }

                    Color particleColor = Color.Lerp(Color.Red, Color.Yellow, Main.rand.NextFloat());
                    Vector2 offset = Main.rand.NextVector2CircularEdge(30f, 20f);
                    offset.Y *= (Math.Abs(offset.X) / 45) + 0.33f;
                    float rotForParticles = NPC.ai[3].AngleTowards(rotToTarget, 0.15f);
                    ParticleManager.AddParticle(new Square(
                        projSpawnPos, offset.RotatedBy(rotForParticles + MathHelper.PiOver2) * 0.1f + rotForParticles.ToRotationVector2() * 1.6f, 30, particleColor, new Vector2(Main.rand.NextFloat(1, 1.5f)), offset.ToRotation(), 0.96f, 30, true));
                }
                if (NPC.ai[1] >= Flame.Duration)
                {
                    currentFrame = 0;
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = -20;
                    NPC.ai[2] = Flame.Id;
                    NPC.ai[3] = 0;
                }
            }
            else if (NPC.ai[0] == DartTrap.Id)
            {
                Vector2 leftBound = TileCollidePositionInLine(spawnPos, spawnPos + new Vector2(-1000, 0)) + Vector2.UnitX * 1;
                Vector2 rightBound = TileCollidePositionInLine(leftBound, leftBound + new Vector2(2000, 0)) - Vector2.UnitX * 1;
                leftBound = TileCollidePositionInLine(leftBound, leftBound + new Vector2(0, -1000)) + Vector2.UnitY * 8;
                rightBound = TileCollidePositionInLine(rightBound, rightBound + new Vector2(0, -1000)) + Vector2.UnitY * 8;
                float leftBoundHeight = TileCollidePositionInLine(leftBound, leftBound + new Vector2(0, 2000)).Distance(leftBound) - 8;
                float rightBoundHeight = TileCollidePositionInLine(rightBound, rightBound + new Vector2(0, 2000)).Distance(rightBound) - 8;
                if (NPC.ai[1] < dartTrapWindup)
                {
                    currentFrame = 1;
                    if (NPC.ai[1] == 0)
                    {
                        SoundEngine.PlaySound(SoundID.Item15 with { Volume = 1f, PitchVariance = 0, Pitch = -1 }, NPC.Center);
                        SoundEngine.PlaySound(SoundID.Item44 with { Volume = 0.4f, PitchVariance = 0, Pitch = -0.8f }, NPC.Center);
                    }
                }
                else
                {
                    int time = (int)NPC.ai[1] - dartTrapWindup;
                    if (time > 25)
                    {
                        currentFrame = 0;
                    }
                    if (time % dartTrapFireRate == 0)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            Vector2 projSpawnPos;
                            float dir;
                            switch (i)
                            {
                                default:
                                case 0:
                                    projSpawnPos = leftBound + new Vector2(0, Main.rand.NextFloat(leftBoundHeight));
                                    dir = 0;
                                    break;
                                case 1:
                                    projSpawnPos = rightBound + new Vector2(0, Main.rand.NextFloat(rightBoundHeight));
                                    dir = MathHelper.Pi;
                                    break;
                            }
                            projSpawnPos = projSpawnPos.ToTileCoordinates().ToWorldCoordinates();
                            projSpawnPos = TileCollidePositionInLine(projSpawnPos, projSpawnPos + dir.ToRotationVector2() * -160);
                            Room room = modNPC.GetParentRoom();
                            if (room != null)
                            {
                                Rectangle roomRect = room.GetRect();
                                roomRect.Inflate(-16, -16);
                                if (!roomRect.Contains(projSpawnPos.ToPoint()))
                                {
                                    projSpawnPos = roomRect.ClosestPointInRect(projSpawnPos);
                                }
                            }
                            projSpawnPos += dir.ToRotationVector2() * -4;
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), projSpawnPos, dir.ToRotationVector2() * 8, ModContent.ProjectileType<DartTrap>(), NPC.damage, 0);
                        }
                    }
                }
                if (NPC.ai[1] >= DartTrap.Duration)
                {
                    currentFrame = 0;
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = -45;
                    NPC.ai[2] = DartTrap.Id;
                }
            }
            else if (NPC.ai[0] == Boulder.Id)
            {
                Vector2 leftBound = TileCollidePositionInLine(spawnPos, spawnPos + new Vector2(-1000, 0)) + Vector2.UnitX * 1;
                Vector2 rightBound = TileCollidePositionInLine(leftBound, leftBound + new Vector2(2000, 0)) - Vector2.UnitX * 1;
                leftBound = TileCollidePositionInLine(leftBound, leftBound + new Vector2(0, -1000)) + Vector2.UnitY * 1;
                rightBound = TileCollidePositionInLine(rightBound, rightBound + new Vector2(0, -1000)) + Vector2.UnitY * 1;
                Room room = modNPC.GetParentRoom();
                int height = room != null ? (int)room.RoomDimensions.Y : 60;
                bool spawnProj = NPC.ai[1] == boulderWindup;
                bool spawnParticle = NPC.ai[1] < boulderWindup;
                bool spawnDebris = NPC.ai[1] % 2 == 0;

                if (NPC.ai[1] == 0)
                {
                    RumbleSlot = SoundEngine.PlaySound(TerRoguelikeWorld.EarthPound with { Volume = 0.67f }, spawnPos);
                }

                for (int i = 0; i < 2; i++)
                {
                    Vector2 basePos;
                    int dir;
                    switch (i)
                    {
                        default:
                        case 0:
                            basePos = leftBound;
                            dir = -1;
                            break;
                        case 1:
                            basePos = rightBound;
                            dir = 1;
                            break;
                    }
                    for (int y = 0; y < height; y++)
                    {
                        Vector2 checkPos = basePos + new Vector2(0, y * 16);
                        Tile tile = ParanoidTileRetrieval(checkPos.ToTileCoordinates());
                        if (tile.HasTile && TileID.Sets.Platforms[tile.TileType] && tile.IsTileSolidGround() && tile.BlockType == BlockType.Solid)
                        {
                            Vector2 spawnPos = checkPos.ToTileCoordinates().ToWorldCoordinates(8 + 8 * dir, 0);
                            if (spawnProj)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), spawnPos + new Vector2(20 * dir, -24), new Vector2(-dir * Main.rand.NextFloat(1.9f, 2f), 0), ModContent.ProjectileType<RollingBoulder>(), NPC.damage, 0);
                            }
                            if (spawnParticle)
                            {
                                for (int p = 0; p < 2; p++)
                                {
                                    ParticleManager.AddParticle(new ThinSpark(
                                        spawnPos + new Vector2(dir * 4, -8 - 16 * p), Vector2.Zero,
                                        60, Color.Goldenrod * 0.15f, new Vector2(0.075f, 0.1f), MathHelper.PiOver2, true, false));
                                }
                                if (spawnDebris && Main.rand.NextBool())
                                {
                                    Vector2 particleVel = Main.rand.NextVector2Circular(0.5f, 0.5f) + new Vector2(2 * -dir, -0.5f);
                                    if (Main.rand.NextBool(3))
                                        particleVel.X *= 1.7f;
                                    ParticleManager.AddParticle(new Debris(
                                    spawnPos + new Vector2(16 * dir, Main.rand.NextFloat(-32, 0)), particleVel,
                                    40, Color.DarkOrange * 0.875f, new Vector2(0.5f), Main.rand.Next(3), Main.rand.NextFloat(MathHelper.TwoPi),
                                        Main.rand.NextBool() ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0.05f, 7f, 30),
                                        ParticleManager.ParticleLayer.BehindTiles);
                                }
                            }
                        }
                    }
                }

                if (NPC.ai[1] >= Boulder.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Boulder.Id;
                }
            }
            else if (NPC.ai[0] == Summon.Id)
            {
                Vector2 targetPos = target != null ? target.Center : spawnPos;
                Vector2 startPos = NPC.Center + new Vector2(0, 34);
                Vector2[] spawnPositions = [NPC.Center + new Vector2(-400, 0), NPC.Center + new Vector2(400, 0)];

                currentFrame = 1;
                if (NPC.ai[1] == 0)
                {
                    SoundEngine.PlaySound(SoundID.DD2_GhastlyGlaiveImpactGhost with { Volume = 1f, Pitch = -0.6f }, startPos);
                }
                if (NPC.ai[1] < 15)
                {
                    Vector2 offset = Main.rand.NextVector2Circular(30, 20);

                    ParticleManager.AddParticle(new Square(startPos + offset, -offset * 0.1f, 20, Color.HotPink, new Vector2((4f + Main.rand.NextFloat(-0.5f, 0.5f)) * 0.35f), 0, 0.96f, 20, true));
                }
                if (NPC.ai[1] < summonWindup && NPC.ai[1] >= 20)
                {
                    for (int i = 0; i < spawnPositions.Length; i++)
                    {
                        var summonPosition = spawnPositions[i];

                        int time = 20;
                        float completion = (NPC.ai[1] - 20) / 60f;
                        Vector2 endPos = summonPosition;
                        endPos.Y += 48;

                        Vector2 particlePos = startPos + ((startPos - endPos) * completion);
                        particlePos += new Vector2(Main.rand.NextFloat(-10, 10), Main.rand.NextFloat(-10, 10));

                        ParticleManager.AddParticle(new Square(particlePos, Vector2.Zero, time, Color.HotPink, new Vector2((4f + Main.rand.NextFloat(-0.5f, 0.5f)) * 0.45f), 0, 0.96f, time, true));
                        
                    }
                }
                else
                {
                    if (NPC.ai[1] == summonWindup)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_WitherBeastAuraPulse with { Volume = 1f, MaxInstances = 2 }, NPC.Center);
                        SoundEngine.PlaySound(SoundID.DD2_SkeletonSummoned with { Volume = 1f, MaxInstances = 2 }, NPC.Center);

                        for (int i = 0; i < spawnPositions.Length; i++)
                        {
                            var summonPosition = spawnPositions[i];
                            SpawnManager.SpawnEnemy(ModContent.NPCType<LihzahrdSentry>(), summonPosition, modNPC.sourceRoomListID, 60, 0.45f);
                        }
                    }
                    if (NPC.ai[1] > summonWindup)
                    {
                        currentFrame = 0;
                    }
                }
                if (NPC.ai[1] >= Summon.Duration)
                {
                    currentFrame = 0;
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = -200;
                    NPC.ai[2] = Summon.Id;
                }
            }
        }
        public void ChooseAttack()
        {
            NPC.ai[1] = 0;
            int chosenAttack = 0;

            List<Attack> potentialAttacks = new List<Attack>() { Laser, SpikeBall, Flame, DartTrap, Boulder, Summon };
            potentialAttacks.RemoveAll(x => x.Id == (int)NPC.ai[2]);
            if (target != null && NPC.Center.Distance(target.Center) > 540)
                potentialAttacks.RemoveAll(x => x.Id == Flame.Id);
            if (NPC.localAI[3] > 0 || summonChooseAttackCooldown > 0)
                potentialAttacks.RemoveAll(x => x.Id == Summon.Id);

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
            NPC.ai[3] = 0;
            NPC.localAI[1] = 0;

            modNPC.OverrideIgniteVisual = true;
            NPC.life = 1;
            NPC.immortal = true;
            NPC.dontTakeDamage = true;
            NPC.active = true;
            ableToHit = false;
            currentFrame = 0;
            NPC.noTileCollide = false;
            NPC.noGravity = false;
            NPC.GravityMultiplier *= MathHelper.Clamp((float)Math.Pow(deadTime / 60f, 1.4f), 0, 1);
            NPC.rotation -= (NPC.velocity.Y * 0.012f);
            if (NPC.velocity.Y > 2 && NPC.collideY)
            {
                SoundEngine.PlaySound(SoundID.Item70 with { Volume = 0.8f, MaxInstances = 3, Pitch = -0.6f }, NPC.Center);
            }

            if (deadTime == 0)
            {
                enemyHealthBar.ForceEnd(0);
                NPC.velocity = Vector2.Zero;
                NPC.rotation = 0;
                modNPC.ignitedStacks.Clear();
                SoundEngine.PlaySound(SoundID.NPCDeath14 with { Volume = 1f, Pitch = -0.1f }, NPC.Center);

                if (modNPC.isRoomNPC)
                {
                    if (ActiveBossTheme != null)
                        ActiveBossTheme.endFlag = true;
                    Room room = RoomList[modNPC.sourceRoomListID];
                    room.bossDead = true;
                    ClearChildren();
                }
                CutsceneSystem.SetCutscene(NPC.Center, deathCutsceneDuration, 30, 30, 2.5f);
            }

            void ClearChildren()
            {
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
            if (deadTime % 20 == 0 || (deadTime > 90 && deadTime % 14 == 0))
            {
                if (deadTime != 0)
                    deathGodRays.Add(new GodRay(Main.rand.NextFloat(MathHelper.TwoPi), deadTime, new Vector2(0.16f + Main.rand.NextFloat(-0.02f, 0.02f), 0.025f)));
                SoundEngine.PlaySound(Paladin.HammerLand with { Volume = 0.15f, MaxInstances = 10, Pitch = 0.67f, PitchVariance = 0.06f }, NPC.Center);
                SoundEngine.PlaySound(SoundID.NPCHit4 with { Volume = 0.22f }, NPC.Center);
            }
            deadTime++;

            CutsceneSystem.cameraTargetCenter += (NPC.Center - CutsceneSystem.cameraTargetCenter) * 0.05f;

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
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage * 0.04d; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.t_Lihzahrd, hit.HitDirection, -1f);
                }
            }
        }
        public override void OnKill()
        {
            SoundEngine.PlaySound(SoundID.NPCDeath14 with { Volume = 1f }, NPC.Center);
            SoundEngine.PlaySound(SoundID.DD2_OgreGroundPound with { Volume = 0.8f, Pitch = -0.5f }, NPC.Center);

            Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X + Main.rand.Next(NPC.width), NPC.position.Y + Main.rand.Next(NPC.height)), NPC.velocity, 368, NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X + Main.rand.Next(NPC.width), NPC.position.Y + Main.rand.Next(NPC.height)), NPC.velocity, 370, NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X + Main.rand.Next(NPC.width), NPC.position.Y + Main.rand.Next(NPC.height)), NPC.velocity, 368, NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X + Main.rand.Next(NPC.width), NPC.position.Y + Main.rand.Next(NPC.height)), NPC.velocity, 370, NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 365, NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 363, NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 362, NPC.scale);

            for (int i = 0; i < 8; i++)
            {
                float rot = i * MathHelper.TwoPi * 0.125f + Main.rand.NextFloat(-0.5f, 0.5f);
                ParticleManager.AddParticle(new ThinSpark(
                    NPC.Center + rot.ToRotationVector2() * 15, rot.ToRotationVector2() * 3, 43, Color.Goldenrod, new Vector2(0.075f, 0.075f), rot, true, false));
            }
            for (int i = 0; i < 25; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(30, 30);
                ParticleManager.AddParticle(new Square(
                    NPC.Center, offset * 0.2f, 40, Color.Goldenrod, new Vector2(Main.rand.NextFloat(0.7f, 1f)), offset.ToRotation(), 0.96f, 30, true));
            }
        }
        public override void FindFrame(int frameHeight)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;

            NPC.frame = new Rectangle(0, currentFrame * frameHeight, tex.Width, frameHeight - 2);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;
            Color color = Color.Lerp(drawColor, Color.White, 0.2f);
            Vector2 origin = NPC.frame.Size() * 0.5f;
            SpriteEffects spriteEffects = NPC.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            if (deadTime > 0)
            {
                modNPC.drawCenter = Vector2.Zero;
                modNPC.drawCenter.X += Main.rand.NextFloat(-2f, 2f) * ((deadTime + 50) / 200f);
                if (deathGodRays.Count > 0)
                {
                    StartAdditiveSpritebatch();
                    for (int i = 0; i < deathGodRays.Count; i++)
                    {
                        int direction = i == 0 ? -1 : 1;
                        GodRay ray = deathGodRays[i];
                        float rotation = ray.rotation;
                        Vector2 scale = ray.scale;
                        int time = ray.time;
                        rotation += (deadTime - time) * 0.0033f * direction * ((i + 5) / 5);
                        float opacity = MathHelper.Clamp(MathHelper.Lerp(1f, 0.5f, (deadTime - time) / 60f), 0.5f, 1f);
                        Main.EntitySpriteDraw(godRayTex, NPC.Center - Main.screenPosition, null, Color.Goldenrod * opacity, rotation, new Vector2(0, godRayTex.Height * 0.5f), scale, SpriteEffects.None);
                    }
                    StartVanillaSpritebatch();
                }
            }

            Main.EntitySpriteDraw(tex, NPC.Center + modNPC.drawCenter - Main.screenPosition, NPC.frame, color, NPC.rotation, origin, NPC.scale, spriteEffects);

            bool drawEyes = NPC.localAI[0] >= -85;
            if (drawEyes)
            {
                Vector2 eyePos = NPC.Center + modNPC.drawCenter;
                for (int j = 0; j < 5; j++)
                {
                    Main.EntitySpriteDraw(eyeTex, eyePos - Main.screenPosition + Main.rand.NextVector2CircularEdge(2f, 2f) * NPC.scale, null, Color.White * 0.4f, NPC.rotation, origin, NPC.scale, SpriteEffects.None);
                }
                Main.EntitySpriteDraw(eyeTex, eyePos - Main.screenPosition, null, Color.White, NPC.rotation, origin, NPC.scale, SpriteEffects.None);
            }
            bool drawLights = NPC.localAI[0] >= -160;
            if (drawLights)
            {
                float lightsOpacity = 1f;
                if (NPC.localAI[0] < -100)
                    lightsOpacity = (NPC.localAI[0] + 130) / 30f;

                Main.EntitySpriteDraw(lightTex, NPC.Center + modNPC.drawCenter - Main.screenPosition, null, Color.White * lightsOpacity, NPC.rotation, origin, NPC.scale, spriteEffects);

                if (NPC.ai[0] == DartTrap.Id && NPC.ai[1] < 60)
                {
                    float completion = NPC.ai[1] / 60;
                    float radius = (float)Math.Pow(completion, 2f) * 240;
                    float rotOff = completion * 6;
                    float opacity = 0.65f * (1f - completion);
                    Color extraColor = Color.White * opacity;
                    extraColor.A = 0;
                    for (int i = 0; i < 16; i++)
                    {
                        Main.EntitySpriteDraw(lightTex, NPC.Center + modNPC.drawCenter - Main.screenPosition + ((rotOff + i * MathHelper.PiOver4 * 0.5f) * (i % 2 == 0 ? -1 : 1)).ToRotationVector2() * radius, null, extraColor, NPC.rotation, origin, NPC.scale, spriteEffects);
                    }
                }
            }

            if (deadTime > 0)
            {
                StartAdditiveSpritebatch();

                float deathGlowOpacity = MathHelper.Clamp(deadTime / (deathCutsceneDuration - 40f), 0, 1) * 0.8f;
                Color deathGlowColor = Color.Goldenrod * deathGlowOpacity;
                Vector3 colorHSL = Main.rgbToHsl(deathGlowColor);

                GameShaders.Misc["TerRoguelike:BasicTint"].UseOpacity(1f);
                GameShaders.Misc["TerRoguelike:BasicTint"].UseColor(Main.hslToRgb(1 - colorHSL.X, colorHSL.Y, colorHSL.Z));
                GameShaders.Misc["TerRoguelike:BasicTint"].Apply();

                Main.EntitySpriteDraw(tex, NPC.Center + modNPC.drawCenter - Main.screenPosition, NPC.frame, color, NPC.rotation, origin, NPC.scale, spriteEffects);

                StartVanillaSpritebatch();
            }
            return false;
        }
    }
}
