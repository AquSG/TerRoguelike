using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using TerRoguelike.Managers;
using TerRoguelike.Particles;
using TerRoguelike.Projectiles;
using TerRoguelike.Systems;
using static TerRoguelike.Managers.TextureManager;
using static TerRoguelike.Schematics.SchematicManager;
using static TerRoguelike.Systems.MusicSystem;
using static TerRoguelike.Systems.RoomSystem;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using static TerRoguelike.Systems.EnemyHealthBarSystem;
using static TerRoguelike.MainMenu.TerRoguelikeMenu;
using TerRoguelike.World;

namespace TerRoguelike.NPCs.Enemy.Boss
{
    public class BrambleHollow : BaseRoguelikeNPC
    {
        public Entity target;
        public Vector2 spawnPos;
        public bool ableToHit = true;
        public bool canBeHit = true;
        public override int modNPCID => ModContent.NPCType<BrambleHollow>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Forest"] };
        public override int CombatStyle => -1;
        public int currentFrame;
        public int horizontalFrame;
        public Texture2D lightTex;
        public Texture2D ballTex;
        public Texture2D fireTex;
        public List<TallFireDraw> fireDraws = new List<TallFireDraw>();
        float oldOpacity = 0;
        public int deadTime = 0;
        public int cutsceneDuration = 195;
        public int cutsceneBurrowFinish = -60;
        public int deathCutsceneDuration = 270;
        public int deathBurnStartTime = 90;
        public int deathDisintegrateStartTime = 150;
        public SoundStyle BramblePunch = new SoundStyle("TerRoguelike/Sounds/BramblePunch");
        public SoundStyle BurrowSound = new SoundStyle("TerRoguelike/Sounds/Shockwave", 3);

        public Attack None = new Attack(0, 0, 120);
        public Attack Burrow = new Attack(1, 60, 360);
        public Attack VineWall = new Attack(2, 40, 240);
        public Attack RootLift = new Attack(3, 40, 180);
        public Attack SeedBarrage = new Attack(4, 40, 90);
        public Attack Summon = new Attack(5, 30, 150);
        public int ballAttack1 = 60;
        public int ballAttack2 = 120;
        public Vector2 ballAttack1Pos;
        public Vector2 ballAttack2Pos;
        public int rootAttack1 = 90;
        public int rootAttack2 = 150;
        public Vector2[] SummonSpawnPositions = new Vector2[] { new Vector2(-1), new Vector2(-1) };
        public int summonTelegraph = 60;
        public int vineWallCooldown = 0;
        public int desiredEnemy;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 10;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            modNPC.TerRoguelikeBoss = true;
            NPC.width = 128;
            NPC.height = 128;
            NPC.aiStyle = -1;
            NPC.damage = 30;
            NPC.lifeMax = 25000;
            NPC.HitSound = SoundID.NPCHit7;
            NPC.DeathSound = SoundID.NPCDeath5;
            NPC.knockBackResist = 0f;
            modNPC.drawCenter = new Vector2(0, 240);
            lightTex = TexDict["BrambleHollowGlow"];
            ballTex = TexDict["LeafBall"];
            fireTex = TexDict["TallFire"];
            NPC.behindTiles = true;
            NPC.noGravity = true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            //NPC ai 3 is used for the current cardinal direction the npc is. 0 - down, 1 - left, -1 - right, 2 - up 
            NPC.ai[3] = 0;

            NPC.immortal = NPC.dontTakeDamage = !TerRoguelikeWorld.escape;
            currentFrame = 0;
            horizontalFrame = 0;
            NPC.localAI[0] = -(cutsceneDuration + 30);
            NPC.direction = -1;
            NPC.spriteDirection = -1;
            spawnPos = NPC.Center;
            NPC.ai[2] = Burrow.Id;
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
                SetBossTrack(BrambleHollowTheme);
            }

            ableToHit = !(NPC.ai[0] == Burrow.Id && NPC.ai[1] > Burrow.Duration * 0.5f) && deadTime == 0;
            canBeHit = !(NPC.ai[0] == Burrow.Id && Math.Abs(NPC.ai[1] - (Burrow.Duration * 0.5f)) < 60);

            NPC.velocity += new Vector2(0, 0.1f).RotatedBy(MathHelper.PiOver2 * NPC.ai[3]);
            NPC.frameCounter += 0.18d;

            if (NPC.localAI[0] < 0)
            {
                spawnPos = NPC.Center;
                if (NPC.localAI[0] == -cutsceneDuration)
                {
                    CutsceneSystem.SetCutscene(NPC.Center, cutsceneDuration, 30, 30, 2.5f, CutsceneSystem.CutsceneSource.Boss);
                    SoundEngine.PlaySound(BurrowSound with { Volume = 0.1f, Pitch = 0.6f, MaxInstances = 2 }, NPC.Center);
                }
                NPC.localAI[0]++;
                if (NPC.localAI[0] < cutsceneBurrowFinish)
                {
                    BurrowEffect();
                }
                else if (NPC.localAI[0] == cutsceneBurrowFinish)
                {
                    //SoundEngine.PlaySound(SoundID.DeerclopsScream with { Volume = 0.4f, Pitch = -0.7f }, NPC.Center);
                    SoundEngine.PlaySound(SoundID.DeerclopsScream with { Volume = 0.4f, Pitch = -0.9f }, NPC.Center);
                }
                else if (NPC.localAI[0] == -30)
                {
                    NPC.immortal = false;
                    NPC.dontTakeDamage = false;
                    if (!TerRoguelikeWorld.escape)
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
            bool hardMode = (int)difficulty >= (int)Difficulty.BloodMoon;

            target = modNPC.GetTarget(NPC);

            if (vineWallCooldown > 0)
                vineWallCooldown--;
            NPC.ai[1]++;

            ballAttack1Pos = NPC.Center + modNPC.drawCenter + new Vector2(100, 0).RotatedBy(NPC.rotation);
            ballAttack2Pos = NPC.Center + modNPC.drawCenter + new Vector2(-100, 0).RotatedBy(NPC.rotation);

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
                    NPC.direction = 1;

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

            if (NPC.ai[0] == Burrow.Id)
            {
                if (NPC.ai[1] == 0)
                {
                    SoundEngine.PlaySound(BurrowSound with { Volume = 0.1f, Pitch = 0.5f, MaxInstances = 2 }, NPC.Center);
                }
                if (NPC.ai[1] < 80 || NPC.ai[1] > (int)(Burrow.Duration * 0.5f) + 1)
                {
                    BurrowEffect();
                }
                if (NPC.ai[1] == (int)(Burrow.Duration * 0.5f))
                {
                    NPC.velocity = Vector2.Zero;
                    List<int> directions = new List<int>() { -1, 0, 1, 2 };
                    directions.RemoveAll(x => x == (int)NPC.ai[3]);
                    int newDir = directions[Main.rand.Next(directions.Count)];
                    NPC.ai[3] = newDir;
                    if (newDir % 2 != 0)
                    {
                        newDir *= -1;
                        Room room = RoomList[modNPC.sourceRoomListID];
                        Vector2 checkPos = room.RoomPosition16 + room.RoomCenter16 + (new Vector2(((room.RoomDimensions16.X * 0.5f) - 16) * newDir, room.RoomDimensions16.Y * 0.17f));
                        Point tilePos = checkPos.ToTileCoordinates();
                        if (ParanoidTileRetrieval(tilePos.X, tilePos.Y).IsTileSolidGround(true))
                        {
                            for (int i = 1; i < 25; i++)
                            {
                                if (!ParanoidTileRetrieval(tilePos.X + (i * -newDir), tilePos.Y).IsTileSolidGround(true))
                                {
                                    tilePos.X += (i * -newDir);
                                    break;
                                }
                            }
                        }
                        checkPos = tilePos.ToVector2() * 16f;
                        if (newDir == 1)
                        {
                            checkPos.X += 16;
                            NPC.Right = checkPos;
                        }
                        else
                        {
                            NPC.Left = checkPos;
                        }
                    }
                    else
                    {
                        newDir -= 1;
                        newDir *= -1;
                        Room room = RoomList[modNPC.sourceRoomListID];
                        Vector2 checkPos = room.RoomPosition16 + room.RoomCenter16 + ((new Vector2(0, (room.RoomDimensions16.Y * 0.5f) - 16) * newDir));
                        Point tilePos = checkPos.ToTileCoordinates();
                        if (ParanoidTileRetrieval(tilePos.X, tilePos.Y).IsTileSolidGround(true))
                        {
                            for (int i = 1; i < 25; i++)
                            {
                                if (!ParanoidTileRetrieval(tilePos.X, tilePos.Y + (i * -newDir)).IsTileSolidGround(true))
                                {
                                    tilePos.Y += (i * -newDir);
                                    break;
                                }
                            }
                        }
                        checkPos = tilePos.ToVector2() * 16f;
                        if (newDir == 1)
                        {
                            checkPos.Y += 16;
                            NPC.Bottom = checkPos;
                        }
                        else
                        {
                            NPC.Top = checkPos;
                        }
                    }
                    SoundEngine.PlaySound(BurrowSound with { Volume = 0.11f, Pitch = 0.5f, MaxInstances = 2 }, NPC.Center);
                }
                if (NPC.ai[1] >= Burrow.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = None.Duration - 60;
                    NPC.ai[2] = Burrow.Id;
                }
            }
            else if (NPC.ai[0] == VineWall.Id)
            {
                float attackHorizSpeedMulti = (int)NPC.ai[3] % 2 == 0 ? 3f : 0.6f;
                if (NPC.ai[1] == 0)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        Dust.NewDust(ballAttack1Pos, 1, 1, DustID.Grass);
                        Dust.NewDust(ballAttack2Pos, 1, 1, DustID.Grass);
                    }
                    SoundEngine.PlaySound(SoundID.Item76 with { Volume = 1f }, NPC.Center);
                }
                else if (NPC.ai[1] == ballAttack1)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), ballAttack1Pos, new Vector2(attackHorizSpeedMulti, -6).RotatedBy(NPC.rotation), ModContent.ProjectileType<LeafBall>(), NPC.damage, 0f, -1, NPC.ai[3]);
                }
                else if (NPC.ai[1] == ballAttack2)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), ballAttack2Pos, new Vector2(-attackHorizSpeedMulti, -6).RotatedBy(NPC.rotation), ModContent.ProjectileType<LeafBall>(), NPC.damage, 0f, -1, NPC.ai[3]);
                }
                else if (NPC.ai[1] >= VineWall.Duration)
                {
                    vineWallCooldown = RuinedMoonActive ? 0 : 830;
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = VineWall.Id;
                }
            }
            else if (NPC.ai[0] == RootLift.Id)
            {
                if (NPC.ai[1] == 0)
                {
                    SoundEngine.PlaySound(SoundID.DeerclopsScream with { Volume = 0.4f, Pitch = -0.7f }, NPC.Center);
                }
                if (NPC.ai[1] == rootAttack1 - 35 || NPC.ai[1] == rootAttack2 - 35)
                {
                    float rotateBy = (NPC.ai[3] * MathHelper.PiOver2) + MathHelper.Pi;
                    SoundEngine.PlaySound(BramblePunch with { Volume = 0.7f, MaxInstances = 2 }, NPC.Center);
                    for (int i = 0; i < 10; i++)
                    {
                        Vector2 particlePos = NPC.ai[1] < rootAttack2 - 35 ? new Vector2(108, 24).RotatedBy(rotateBy) : new Vector2(-108, 24).RotatedBy(rotateBy);
                        particlePos += NPC.Center;
                        Vector2 particleVel = new Vector2(0, -2).RotatedBy(rotateBy + (i / 10f * MathHelper.TwoPi) + Main.rand.NextFloat(-0.08f, 0.08f));
                        Vector2 scale = new Vector2(0.07f);
                        particlePos += particleVel * -10f;
                        ParticleManager.AddParticle(new ThinSpark(particlePos, particleVel, 25, Color.SandyBrown * 0.60f, scale, particleVel.ToRotation(), true));
                    }
                }
                if (NPC.ai[1] == rootAttack1 || NPC.ai[1] == rootAttack2)
                {
                    float rotateBy = (NPC.ai[3] * MathHelper.PiOver2) + MathHelper.Pi;
                    Vector2 checkDirection = new Vector2(0, -16).RotatedBy(rotateBy);
                    Vector2 targetPos = target != null ? target.Center : NPC.Center + new Vector2(NPC.ai[1] == rootAttack1 ? -160f : 160f, 0).RotatedBy(NPC.rotation);
                    for (int i = 0; i < 100; i++)
                    {
                        Vector2 offsetTargetPos = (targetPos + (checkDirection * i));
                        Point targetTilePos = offsetTargetPos.ToTileCoordinates();
                        if (ParanoidTileRetrieval(targetTilePos.X, targetTilePos.Y).IsTileSolidGround(true))
                        {
                            float ai0 = (int)NPC.ai[3] % 2 != 0 ? -NPC.ai[3] : (NPC.ai[3] == 2 ? 0 : 2);
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), targetTilePos.ToWorldCoordinates() + new Vector2(0, -32).RotatedBy(rotateBy), new Vector2(0, 12).RotatedBy(rotateBy), ModContent.ProjectileType<RootPillar>(), NPC.damage, 0f, -1, ai0);
                            break;
                        }
                    }
                    for (int i = -10; i <= 10; i++)
                    {
                        Vector2 particlePos = NPC.ai[1] < rootAttack2 ? new Vector2(48, -64).RotatedBy(rotateBy) : new Vector2(-48, -64).RotatedBy(rotateBy);
                        particlePos += NPC.Center;
                        Vector2 particleVel = new Vector2(0, 2).RotatedBy(rotateBy + (0.1f * i) + Main.rand.NextFloat(-0.08f, 0.08f));
                        Vector2 scale = new Vector2(0.05f);
                        particlePos += particleVel * 7.5f;
                        ParticleManager.AddParticle(new ThinSpark(particlePos, particleVel, 30, Color.SandyBrown * 0.65f, scale, particleVel.ToRotation(), true));
                    }
                }
                else if (NPC.ai[1] >= RootLift.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = RootLift.Id;
                }
            }
            else if (NPC.ai[0] == SeedBarrage.Id)
            {
                int attackStart = 30;
                bool shoot = NPC.ai[1] % 5 == 0;
                float rotOff = MathHelper.Pi * 0.20f;
                int direction = NPC.direction * (NPC.ai[3] == 2 ? -1 : 1);
                if (NPC.ai[1] >= 15 && NPC.ai[1] <= 75 && shoot)
                {
                    float progress = (NPC.ai[1] - 15) / (60);
                    Vector2 pos = new Vector2(0, -48).RotatedBy(((-rotOff + (progress * rotOff * 2)) * direction) + NPC.rotation);
                    Vector2 velocity = pos.SafeNormalize(Vector2.UnitY) * 3f;
                    pos += NPC.Center;
                    SoundEngine.PlaySound(SoundID.Item5 with { Volume = 0.6f }, pos);
                    Vector2 scale = new Vector2(0.1f);
                    ParticleManager.AddParticle(new ThinSpark(pos, velocity, 30, Color.Green, scale, velocity.ToRotation(), true));
                }
                if (NPC.ai[1] >= attackStart && shoot)
                {

                    float progress = (NPC.ai[1] - attackStart) / (SeedBarrage.Duration - attackStart);
                    Vector2 pos = new Vector2(0, -72).RotatedBy(((-rotOff + (progress * rotOff * 2)) * direction) + NPC.rotation);
                    Vector2 velocity = pos.SafeNormalize(Vector2.UnitY) * 7f;
                    pos += NPC.Center;
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), pos, velocity, ModContent.ProjectileType<GreenPetal>(), NPC.damage, 0);
                }
                if (NPC.ai[1] >= SeedBarrage.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = SeedBarrage.Id;
                }
            }
            else if (NPC.ai[0] == Summon.Id)
            {
                if (NPC.ai[1] == 0)
                {
                    bool onWall = NPC.ai[3] % 2 != 0;
                    desiredEnemy = onWall ? ModContent.NPCType<Tumbletwig>() : ModContent.NPCType<SeedLobber>();
                    SoundEngine.PlaySound(SoundID.Item44 with { Volume = 0.5f, Pitch = -0.2f }, NPC.Center);

                    NPC dummyNPC = new NPC();
                    dummyNPC.type = desiredEnemy;
                    dummyNPC.SetDefaults(desiredEnemy);
                    Rectangle dummyRect = new Rectangle(0, 0, dummyNPC.width, dummyNPC.height);
                    for (int i = 0; i < SummonSpawnPositions.Length; i++)
                    {
                        int direction = i == 0 ? -1 : 1;
                        float rotateBy = onWall ? NPC.rotation : 0;
                        float distanceBeside = 164f * direction;
                        Vector2 potentialSpawnPos = NPC.Center + new Vector2(distanceBeside, 0).RotatedBy(rotateBy);

                        Vector2 checkDirection = new Vector2(0, 16).RotatedBy(rotateBy);
                        for (int b = 0; b < 25; b++)
                        {
                            Point tilePos = (potentialSpawnPos + (checkDirection * b)).ToTileCoordinates();
                            if (ParanoidTileRetrieval(tilePos.X, tilePos.Y).IsTileSolidGround(onWall))
                            {
                                potentialSpawnPos = tilePos.ToWorldCoordinates() - (checkDirection * 0.5f);
                                if (onWall)
                                {
                                    potentialSpawnPos += new Vector2(((dummyNPC.width * 0.5f)) * NPC.ai[3], -dummyNPC.height * 0.5f);
                                }
                                else
                                {
                                    potentialSpawnPos += new Vector2(0, -dummyNPC.height + 10);
                                }
                                SummonSpawnPositions[i] = potentialSpawnPos;
                                break;
                            }
                        }
                    }
                }
                if (NPC.ai[1] < summonTelegraph)
                {
                    int time = 20;
                    float completion = NPC.ai[1] / summonTelegraph;
                    float curveMultiplier = 1f - (float)Math.Pow(Math.Abs(completion - 0.5f) * 2, 2);
                    for (int i = 0; i < SummonSpawnPositions.Length; i++)
                    {
                        int dir = i == 0 ? -1 : 1;
                        if (NPC.ai[3] == 2)
                            dir *= -1;

                        Vector2 startPos = NPC.Center + new Vector2(100 * dir, -16).RotatedBy(NPC.rotation);

                        Vector2 endPos = SummonSpawnPositions[i];
                        if (endPos == new Vector2(-1))
                            continue;
                        endPos += new Vector2(0, 16);

                        Vector2 particlePos = startPos + ((endPos - startPos) * completion) + (Vector2.UnitY * -32 * curveMultiplier).RotatedBy(NPC.rotation);
                        particlePos += new Vector2(Main.rand.NextFloat(-10, 10), Main.rand.NextFloat(-10, 10));

                        ParticleManager.AddParticle(new Square(particlePos, Vector2.Zero, time, Color.HotPink, new Vector2((4f + Main.rand.NextFloat(-0.5f, 0.5f)) * 0.45f), 0, 0.96f, time, true));
                    }
                }
                else if (NPC.ai[1] == summonTelegraph)
                {
                    SoundEngine.PlaySound(SoundID.DD2_WitherBeastAuraPulse with { Volume = 1f }, NPC.Center);
                    SoundEngine.PlaySound(SoundID.DD2_SkeletonSummoned with { Volume = 1f }, NPC.Center);
                    for (int i = 0; i < SummonSpawnPositions.Length; i++)
                    {
                        Vector2 pos = SummonSpawnPositions[i];
                        if (pos == new Vector2(-1))
                            continue;

                        SpawnManager.SpawnEnemy(desiredEnemy, pos, modNPC.sourceRoomListID, 60, 0.45f);

                        SummonSpawnPositions[i] = new Vector2(-1);
                    }
                }
                if (NPC.ai[1] >= Summon.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Summon.Id;
                }
            }
        }
        public void ChooseAttack()
        {
            NPC.ai[1] = 0;
            List<Attack> potentialAttacks = new List<Attack>() { Burrow, VineWall, RootLift, SeedBarrage, Summon };
            potentialAttacks.RemoveAll(x => x.Id == (int)NPC.ai[2]);
            if (vineWallCooldown > 0)
                potentialAttacks.RemoveAll(x => x.Id == VineWall.Id);
            if (!modNPC.isRoomNPC)
            {
                potentialAttacks.RemoveAll(x => x.Id == Burrow.Id);
            }

            int totalWeight = 0;
            for (int i = 0; i < potentialAttacks.Count; i++)
            {
                totalWeight += potentialAttacks[i].Weight;
            }
            int chosenRandom = Main.rand.Next(totalWeight);
            int chosenAttack = 0;
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
        public void BurrowEffect()
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 pos = new Vector2(Main.rand.NextFloat(-NPC.width * 0.5f, NPC.width * 0.5f), NPC.height * 0.5f).RotatedBy(NPC.rotation) + NPC.Center;
                Dust d = Dust.NewDustPerfect(pos, DustID.WoodFurniture, null, 0, default, 1);
            }
        }
        public override bool? CanFallThroughPlatforms()
        {
            return true;
        }
        public override bool CanHitNPC(NPC target)
        {
            return ableToHit;
        }
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            return ableToHit;
        }
        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            return projectile.Colliding(projectile.getRect(), ModifiedHitbox()) && canBeHit ? null : false;
        }
        public override bool? CanBeHitByItem(Player player, Item item)
        {
            return item.Hitbox.Intersects(ModifiedHitbox()) && canBeHit ? null : false;
        }
        public override bool ModifyCollisionData(Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox)
        {
            npcHitbox = ModifiedHitbox();

            return false;
        }
        public Rectangle ModifiedHitbox()
        {
            if (NPC.ai[0] != Burrow.Id)
                return NPC.Hitbox;
            Rectangle npcHitbox = NPC.Hitbox;
            int lesser = npcHitbox.Width < npcHitbox.Height ? npcHitbox.Width : npcHitbox.Height;
            int offset = (int)GetOffset();
            if (offset > lesser)
                offset = lesser;

            if (NPC.ai[3] == 0)
            {
                npcHitbox.Y += offset;
                npcHitbox.Height -= offset;
            }
            else if (NPC.ai[3] == 2)
            {
                npcHitbox.Height -= offset;
            }
            else if (NPC.ai[3] == 1)
            {
                npcHitbox.Width -= offset;
            }
            else if (NPC.ai[3] == -1)
            {
                npcHitbox.Width -= -offset;
                npcHitbox.X += offset;
            }
            return npcHitbox;
        }
        public override bool CheckDead()
        {
            if (deadTime >= deathCutsceneDuration)
            {
                return true;
            }
            NPC.ai[0] = None.Id;
            NPC.ai[1] = 1;

            modNPC.OverrideIgniteVisual = true;
            NPC.velocity *= 0;
            NPC.life = 1;
            NPC.immortal = true;
            NPC.dontTakeDamage = true;
            NPC.active = true;
            if (deadTime == 0)
            {
                enemyHealthBar.ForceEnd(0);
                SoundEngine.PlaySound(SoundID.DD2_OgreHurt with { Volume = 1f, Pitch = -0.5f }, NPC.Center);
                NPC.HitSound = SoundID.Item1 with { Volume = 0f };
                NPC.DeathSound = SoundID.Item1 with { Volume = 0f };
                CutsceneSystem.SetCutscene(NPC.Center, deathCutsceneDuration, 30, 30, 2.5f, CutsceneSystem.CutsceneSource.Boss);
                if (modNPC.isRoomNPC)
                {
                    if (ActiveBossTheme != null)
                        ActiveBossTheme.endFlag = true;
                    RoomList[modNPC.sourceRoomListID].bossDead = true;
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
            }
            deadTime++;

            if (deadTime == 40)
            {
                Vector2 pos = NPC.Center + new Vector2(11, -17).RotatedBy(NPC.rotation);
                SoundEngine.PlaySound(SoundID.Item20 with { Volume = 0.7f, MaxInstances = 2, Pitch = 0.3f }, pos);
                fireDraws.Add(new TallFireDraw(pos, Main.rand.Next(17), false));
                SoundEngine.PlaySound(new SoundStyle("TerRoguelike/Sounds/FireCrackle") with { Volume = 1f, Pitch = -0.6f }, NPC.Center);
            }
            else if (deadTime == 65)
            {
                Vector2 pos = NPC.Center + new Vector2(-11, -17).RotatedBy(NPC.rotation);
                SoundEngine.PlaySound(SoundID.Item20 with { Volume = 0.7f, MaxInstances = 2, Pitch = 0.3f }, pos);
                fireDraws.Add(new TallFireDraw(pos, Main.rand.Next(17), false));
                SoundEngine.PlaySound(new SoundStyle("TerRoguelike/Sounds/FireCrackle") with { Volume = 0.8f, Pitch = -0.6f }, NPC.Center);
            }
            else if (deadTime == deathBurnStartTime)
            {
                SoundEngine.PlaySound(new SoundStyle("TerRoguelike/Sounds/FireExtinguish") with { Volume = 0.25f, Pitch = 0 }, NPC.Center);
                SoundEngine.PlaySound(new SoundStyle("TerRoguelike/Sounds/FireCrackle") with { Volume = 0.4f, Pitch = -0.6f }, NPC.Center);
            }
            else if (deadTime > deathDisintegrateStartTime)
            {
                int frameHeight = TextureAssets.Npc[Type].Value.Height / Main.npcFrameCount[Type];
                float interpolant = (deadTime - deathDisintegrateStartTime) / (deathCutsceneDuration - (float)deathDisintegrateStartTime);
                float offset = MathHelper.Lerp(0, frameHeight * 0.96f, interpolant);
                Vector2 basePos = NPC.Center + new Vector2(0, (-frameHeight * 0.5f) + offset - 18).RotatedBy(NPC.rotation);
                float halfFrameWidth = (NPC.width) * 0.8f * MathHelper.SmoothStep(1f, 0.6f, Math.Abs(interpolant - 0.5f) * 2f);
                int count = 6;
                if (interpolant > 0.51f && interpolant < 0.57f)
                {
                    halfFrameWidth *= 1.28f;
                    count += 2;
                }
                for (int i = 0; i < count; i++)
                {
                    Vector2 particlePos = basePos + new Vector2(Main.rand.NextFloat(-halfFrameWidth, halfFrameWidth), 0).RotatedBy(NPC.rotation);
                    Vector2 scale = new Vector2(Main.rand.NextFloat(0.24f, 1.5f) + MathHelper.Lerp(0, 0.3f, interpolant));
                    float randInterpolant = Main.rand.NextFloat(0.85f, 1f);
                    Color color = Color.Lerp(Color.Brown, Color.Black, randInterpolant);
                    ParticleManager.AddParticle(new Ash(particlePos, Vector2.Zero, 270, color, scale, 0, 0.96f, 0.04f, 30, Main.rand.Next(20, 40), false));
                }
            }
            else if (deadTime > deathBurnStartTime)
            {
                if (deadTime % 3 == 0)
                {
                    float width = NPC.width * 0.235f;
                    Vector2 pos = NPC.Center + new Vector2(Main.rand.NextFloat(-width, width) + 1, -34).RotatedBy(NPC.rotation);
                    fireDraws.Add(new TallFireDraw(pos, Main.rand.Next(17), true));
                }
            }

            if (deadTime >= deathCutsceneDuration)
            {
                NPC.immortal = false;
                NPC.dontTakeDamage = false;
                NPC.StrikeInstantKill();
            }

            return deadTime >= cutsceneDuration;
        }
        public override void FindFrame(int frameHeight)
        {
            NPC.rotation = MathHelper.PiOver2 * NPC.ai[3];
            modNPC.drawCenter = (NPC.Bottom - NPC.Center + new Vector2(0, (-frameHeight * 0.5f) + 2)).RotatedBy(NPC.rotation);

            int frameCount = Main.npcFrameCount[Type];
            int frameWidth = TextureAssets.Npc[Type].Value.Width / 3;
            currentFrame = (int)NPC.frameCounter % (frameCount - 5);
            horizontalFrame = 0;

            if (NPC.ai[0] == Burrow.Id || NPC.localAI[0] < cutsceneBurrowFinish)
                currentFrame += 5;
            else if (NPC.ai[0] == VineWall.Id && NPC.ai[1] < ballAttack2 + 48)
            {
                horizontalFrame = 1;
                if (NPC.ai[1] < ballAttack1)
                    currentFrame = 0;
                else if (NPC.ai[1] < ballAttack1 + 12)
                    currentFrame = 1;
                else if (NPC.ai[1] < ballAttack1 + 24)
                    currentFrame = 2;
                else if (NPC.ai[1] < ballAttack2)
                    currentFrame = 0;
                else if (NPC.ai[1] < ballAttack2 + 12)
                    currentFrame = 3;
                else if (NPC.ai[1] < ballAttack2 + 24)
                    currentFrame = 4;
                else
                    currentFrame = 0;

                NPC.frameCounter = 3;
            }
            else if (NPC.ai[0] == RootLift.Id)
            {
                int animLength = 36;
                int anim1Start = rootAttack1 - animLength;
                int anim2Start = rootAttack2 - animLength;
                if (NPC.ai[1] < anim1Start)
                {
                    horizontalFrame = 1;
                    currentFrame = 5;
                }
                else if (NPC.ai[1] < rootAttack1)
                {
                    horizontalFrame = 2;
                    currentFrame = NPC.ai[1] - anim1Start < 27 ? 0 : ((int)NPC.ai[1] - anim1Start - 27) / 3;
                }
                else if (NPC.ai[1] < anim2Start)
                {
                    horizontalFrame = 2;
                    currentFrame = NPC.ai[1] - rootAttack1 < 17 ? 3 : 1;
                }
                else if (NPC.ai[1] < rootAttack2)
                {
                    horizontalFrame = 2;
                    currentFrame = (NPC.ai[1] - anim2Start < 27 ? 0 : ((int)NPC.ai[1] - anim2Start - 27) / 3) + 4;
                }
                else
                {
                    horizontalFrame = 2;
                    currentFrame = NPC.ai[1] - rootAttack2 < 20 ? 7 : 5;
                }
            }
            else if (NPC.ai[0] == Summon.Id && NPC.ai[1] < 90)
            {
                horizontalFrame = 1;
                currentFrame = 0;
            }
            else if (NPC.localAI[0] < 0 && NPC.localAI[1] >= cutsceneBurrowFinish)
            {
                horizontalFrame = 1;
                currentFrame = 5;
            }
            if (deadTime > 0)
            {
                horizontalFrame = 1;
                currentFrame = 5;
            }

            NPC.frame = new Rectangle(horizontalFrame * frameWidth, currentFrame * frameHeight, frameWidth, frameHeight);

            if (NPC.ai[0] == Burrow.Id || NPC.localAI[0] < cutsceneBurrowFinish)
            {
                float offset = GetOffset();
                int rectCutoff = (int)offset - 28;
                if (rectCutoff > NPC.frame.Height)
                    rectCutoff = NPC.frame.Height;
                if (rectCutoff > 0)
                {
                    NPC.frame.Height -= rectCutoff;
                }


                modNPC.drawCenter += (Vector2.UnitY * offset * 0.5f).RotatedBy(NPC.rotation);
            }
            else if (deadTime > deathDisintegrateStartTime)
            {
                float offset = MathHelper.Lerp(0, frameHeight, (deadTime - deathDisintegrateStartTime) / (deathCutsceneDuration - (float)deathDisintegrateStartTime));
                int rectCutoff = (int)offset;
                if (rectCutoff > NPC.frame.Height)
                    rectCutoff = NPC.frame.Height;
                if (rectCutoff > 0)
                {
                    NPC.frame.Y += rectCutoff;
                    NPC.frame.Height -= rectCutoff;
                }

                modNPC.drawCenter += (Vector2.UnitY * offset * 0.5f).RotatedBy(NPC.rotation);
            }
        }
        public float GetOffset()
        {
            int halfTime = (int)(Burrow.Duration * 0.5f);
            float maxOffset = 240;
            float offset;
            if (NPC.ai[0] == Burrow.Id)
            {
                if (NPC.ai[1] < halfTime)
                {
                    offset = MathHelper.SmoothStep(0, maxOffset, NPC.ai[1] / halfTime);
                }
                else
                {
                    offset = MathHelper.SmoothStep(maxOffset, 0, (NPC.ai[1] - halfTime) / halfTime);
                }
            }
            else
            {
                offset = MathHelper.SmoothStep(0, maxOffset, -(NPC.localAI[0] - cutsceneBurrowFinish) / (cutsceneDuration + 30 + cutsceneBurrowFinish));
            }
            return offset;
        }
        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            position += modNPC.drawCenter + new Vector2(0, 28);
            if (NPC.ai[0] == Burrow.Id)
            {
                float halfTime = (Burrow.Duration * 0.5f);
                scale = MathHelper.Clamp(MathHelper.SmoothStep(0, 1f, Math.Abs((NPC.ai[1] - halfTime) / (halfTime))), 0, 1f);
            }
            return true;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;
            bool deadBurnt = deadTime > deathBurnStartTime;

            float deathDisintegrateCompletion = 1f - ((deadTime - deathDisintegrateStartTime) * 2.5f / (deathCutsceneDuration - (float)deathDisintegrateStartTime));
            int fireFrameHeight = fireTex.Height / 17;
            for (int i = 0; i < fireDraws.Count; i++)
            {
                TallFireDraw fire = fireDraws[i];
                if (!fire.drawBehind)
                    continue;

                int currentFrame = ((int)((Main.GlobalTimeWrappedHourly * 17) + fire.animOffset) % 17);
                Rectangle frame = new Rectangle(0, currentFrame * fireFrameHeight, fireTex.Width, fireFrameHeight);
                Main.EntitySpriteDraw(fireTex, fire.position - Main.screenPosition, frame, Color.White * deathDisintegrateCompletion, NPC.rotation, new Vector2(frame.Width * 0.5f, frame.Height * 0.85f), 0.65f, SpriteEffects.None);
            }

            if ((int)NPC.ai[0] == VineWall.Id && NPC.ai[1] < 120)
            {
                float interpolant = MathHelper.Clamp(MathHelper.SmoothStep(0, 1f, NPC.ai[1] / ballAttack1), 0, 1f);
                float offset = -32f * (1f - interpolant);
                if (NPC.ai[1] < ballAttack1)
                {
                    Main.EntitySpriteDraw(ballTex, ballAttack1Pos + new Vector2(0, offset).RotatedBy(NPC.rotation) - Main.screenPosition, null, Color.White * interpolant, NPC.rotation, ballTex.Size() * 0.5f, 1f, SpriteEffects.None);
                }
                Main.EntitySpriteDraw(ballTex, ballAttack2Pos + new Vector2(0, offset).RotatedBy(NPC.rotation) - Main.screenPosition, null, Color.White * interpolant, NPC.rotation, ballTex.Size() * 0.5f, 1f, SpriteEffects.None);
            }

            Vector2 drawPos = NPC.Center + modNPC.drawCenter;
            float newOpacity = 0.7f + (float)(Math.Cos(Main.GlobalTimeWrappedHourly) * 0.2f); ;
            if (NPC.ai[0] == Burrow.Id)
            {
                float halfBurrowTime = (Burrow.Duration * 0.5f);
                newOpacity = MathHelper.Lerp(-0.75f, newOpacity, Math.Abs((NPC.ai[1] - halfBurrowTime) / halfBurrowTime));
            }
            else if ((NPC.ai[0] == RootLift.Id && NPC.ai[1] < rootAttack1 - 20) || (NPC.localAI[0] < 0 && NPC.localAI[1] >= cutsceneBurrowFinish) || deadTime > 0)
            {
                newOpacity = 1f;
            }
            else if (NPC.localAI[0] < cutsceneBurrowFinish)
            {
                newOpacity = MathHelper.Lerp(0, newOpacity, -(NPC.ai[1] - cutsceneBurrowFinish) / (cutsceneDuration + 30 + cutsceneBurrowFinish));
            }

            float glowOpacity = MathHelper.Lerp(oldOpacity, newOpacity, 0.1f);
            oldOpacity = glowOpacity;
            if (deadBurnt)
            {
                StartAlphaBlendSpritebatch();
                Color color = Color.Black;
                Vector3 colorHSL = Main.rgbToHsl(color);
                GameShaders.Misc["TerRoguelike:BasicTint"].UseOpacity(1f);
                GameShaders.Misc["TerRoguelike:BasicTint"].UseColor(Main.hslToRgb(1 - colorHSL.X, colorHSL.Y, colorHSL.Z));
                GameShaders.Misc["TerRoguelike:BasicTint"].Apply();
            }

            Main.EntitySpriteDraw(tex, drawPos - Main.screenPosition, NPC.frame, Lighting.GetColor(drawPos.ToTileCoordinates()), NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection > 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            if (!deadBurnt)
                Main.EntitySpriteDraw(lightTex, drawPos - Main.screenPosition, NPC.frame, Color.White * glowOpacity, NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection > 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            else
            {
                StartVanillaSpritebatch();
            }
            for (int i = 0; i < fireDraws.Count; i++)
            {
                TallFireDraw fire = fireDraws[i];
                if (fire.drawBehind)
                    continue;

                int currentFrame = ((int)((Main.GlobalTimeWrappedHourly * 17) + fire.animOffset) % 17);
                Rectangle frame = new Rectangle(0, currentFrame * fireFrameHeight, fireTex.Width, fireFrameHeight);
                Main.EntitySpriteDraw(fireTex, fire.position - Main.screenPosition, frame, Color.White * deathDisintegrateCompletion, NPC.rotation, new Vector2(frame.Width * 0.5f, frame.Height * 0.85f), 0.65f, SpriteEffects.None);
            }
            return false;
        }
    }
    public class TallFireDraw
    {
        public Vector2 position;
        public int animOffset;
        public bool drawBehind;
        public TallFireDraw(Vector2 Position, int AnimOffset, bool DrawBehind)
        {
            position = Position;
            animOffset = AnimOffset;
            drawBehind = DrawBehind;
        }
    }
}
