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
using Terraria.GameContent.Shaders;
using Terraria.Graphics.Effects;
using static TerRoguelike.MainMenu.TerRoguelikeMenu;

namespace TerRoguelike.NPCs.Enemy.Boss
{
    public class MoonLord : BaseRoguelikeNPC
    {
        public Entity target;
        public Vector2 spawnPos;
        public bool ableToHit = true;
        public bool canBeHit = true;
        public override int modNPCID => ModContent.NPCType<MoonLord>();
        public int handType = ModContent.NPCType<MoonLordHand>();
        public int headType = ModContent.NPCType<MoonLordHead>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Lunar"] };
        public SlotId DeathraySlot;
        public override int CombatStyle => -1;
        public bool CollisionPass = false;
        public bool goreProc = false;
        public int coreCurrentFrame = 0;
        public int mouthCurrentFrame = 0;
        public int headEyeCurrentFrame = 0;
        public int leftHandCurrentFrame = 0;
        public int rightHandCurrentFrame = 0;
        public int emptyEyeCurrentFrame = 0;
        public int headCurrentFrame = 0;
        public Rectangle coreFrame;
        public Rectangle mouthFrame;
        public Rectangle headEyeFrame;
        public Rectangle leftHandFrame;
        public Rectangle rightHandFrame;
        public Rectangle emptyEyeFrame;
        public Rectangle headFrame;
        public int headOverlayFrameCounter;
        public Vector2 headPos;
        public Vector2 leftHandPos;
        public Vector2 rightHandPos;
        public Vector2 headEyeVector;
        public Vector2 leftEyeVector;
        public Vector2 rightEyeVector;
        public Vector2 leftHandAnchor;
        public Vector2 leftHandTargetPos;
        public float leftHandMoveInterpolant;
        public Vector2 rightHandAnchor;
        public Vector2 rightHandTargetPos;
        public float rightHandMoveInterpolant;
        public float maxHandAnchorDistance = 480;
        public int idleCounter = 0;

        public static readonly SoundStyle Break = new SoundStyle("TerRoguelike/Sounds/GlassBreak");
        public static readonly SoundStyle MoonLordDeath = new SoundStyle("TerRoguelike/Sounds/MoonLordDeath");
        public Texture2D coreTex, coreCrackTex, emptyEyeTex, innerEyeTex, lowerArmTex, upperArmTex, mouthTex, sideEyeTex, topEyeTex, topEyeOverlayTex, headTex, handTex, bodyTex, perlinTex;
        public int leftHandWho = -1; // yes, technically moon lord's "Left" is not the same as the left for the viewer, and vice versa for right hand. I do not care. internally it will be based on viewer perspective.
        public int rightHandWho = -1;
        public int headWho = -1;

        public int deadTime = 0;
        public int cutsceneDuration = 160;
        public int deathCutsceneDuration = 480;
        public int deathBlackWhiteStartTime = 200;
        public int deathBlackWhiteStopTime = 340;

        public static Attack None = new Attack(0, 0, 130);
        public static Attack PhantSpin = new Attack(1, 30, 260);
        public static Attack PhantBolt = new Attack(2, 30, 360);
        public static Attack PhantSphere = new Attack(3, 30, 179);
        public static Attack TentacleCharge = new Attack(4, 30, 600);
        public static Attack Deathray = new Attack(5, 30, 300);
        public static Attack Summon = new Attack(6, 18, 77);
        public static int phantSpinWindup = 50;
        public static int phantBoltWindup = 30;
        public static int phantBoltFireRate = 8;
        public static int phantBoltFiringDuration = 90;
        public static int phantSphereFireRate = 20;
        public static int tentacleWindup = 90;
        public static int deathrayWindup = 90;
        public static int summonWindup = 30;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1;
            NPCID.Sets.MustAlwaysDraw[Type] = true;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            modNPC.TerRoguelikeBoss = true;
            NPC.width = 60;
            NPC.height = 88;
            NPC.aiStyle = -1;
            NPC.damage = 36;
            NPC.lifeMax = 16500;
            NPC.knockBackResist = 0f;
            modNPC.drawCenter = new Vector2(0, 0);
            modNPC.IgnoreRoomWallCollision = true;
            modNPC.OverrideIgniteVisual = true;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            NPC.behindTiles = true;
            coreTex = TexDict["MoonLordCore"];
            coreCrackTex = TexDict["MoonLordCoreCracks"];
            emptyEyeTex = TexDict["MoonLordEmptyEye"];
            innerEyeTex = TexDict["MoonLordInnerEye"];
            lowerArmTex = TexDict["MoonLordLowerArm"];
            upperArmTex = TexDict["MoonLordUpperArm"];
            mouthTex = TexDict["MoonLordMouth"];
            sideEyeTex = TexDict["MoonLordSideEye"];
            topEyeTex = TexDict["MoonLordTopEye"];
            topEyeOverlayTex = TexDict["MoonLordTopEyeOverlay"];
            headTex = TexDict["MoonLordHead"];
            handTex = TexDict["MoonLordHand"];
            bodyTex = TexDict["MoonLordBodyHalf"];
            perlinTex = TexDict["Perlin"];
            modNPC.AdaptiveArmorEnabled = true;
        }
        public override void DrawBehind(int index)
        {
            if (deadTime >= deathBlackWhiteStartTime && deadTime < deathBlackWhiteStopTime)
            {
                NPC.behindTiles = false;
                Main.instance.DrawCacheNPCsOverPlayers.Add(index);
            }
            else
            {
                NPC.behindTiles = true;
            }
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.immortal = true;
            NPC.dontTakeDamage = true;
            NPC.localAI[0] = -(cutsceneDuration + 30);
            NPC.direction = -1;
            NPC.spriteDirection = -1;
            spawnPos = NPC.Center;
            NPC.ai[2] = None.Id;
            ableToHit = false;
            rightEyeVector = leftEyeVector = headEyeVector = Vector2.Zero;

            for (int i = -1; i <= 1; i += 2)
            {
                Vector2 handSpawnPos = new Vector2(464 * i, -60) + NPC.Center;
                int whoAmI = NPC.NewNPC(NPC.GetSource_FromThis(), (int)handSpawnPos.X, (int)handSpawnPos.Y, handType);
                NPC hand = Main.npc[whoAmI];
                hand.direction = hand.spriteDirection = i;
                if (i == -1)
                {
                    leftHandWho = whoAmI;
                    leftHandPos = leftHandAnchor = leftHandTargetPos = handSpawnPos;
                }
                else
                {
                    rightHandWho = whoAmI;
                    rightHandPos = rightHandAnchor = rightHandTargetPos = handSpawnPos;
                }
            }
            Vector2 headSpawnPos = new Vector2(0, -395) + NPC.Center;
            headWho = NPC.NewNPC(NPC.GetSource_FromThis(), (int)headSpawnPos.X, (int)headSpawnPos.Y, headType);
            headPos = headSpawnPos;
        }
        public override void PostAI()
        {
            if (NPC.localAI[0] > -90 && deadTime < deathBlackWhiteStartTime - 80)
            {
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player player = Main.player[i];
                    if (!player.active)
                        continue;
                    var modPlayer = player.ModPlayer();
                    if (modPlayer == null)
                        continue;

                    modPlayer.moonLordVisualEffect = true;
                }
            }

            leftHandPos = Vector2.Lerp(leftHandPos, leftHandTargetPos, leftHandMoveInterpolant);
            if (leftHandPos.Distance(leftHandAnchor) > maxHandAnchorDistance)
                leftHandPos = leftHandAnchor + (leftHandPos - leftHandAnchor).SafeNormalize(Vector2.UnitY) * maxHandAnchorDistance;

            rightHandPos = Vector2.Lerp(rightHandPos, rightHandTargetPos, rightHandMoveInterpolant);
            if (rightHandPos.Distance(rightHandAnchor) > maxHandAnchorDistance)
                rightHandPos = rightHandAnchor + (rightHandPos - rightHandAnchor).SafeNormalize(Vector2.UnitY) * maxHandAnchorDistance;

            headPos = NPC.Center + new Vector2(0, -395); // do NOT touch this one

            NPC leftHand = leftHandWho >= 0 ? Main.npc[leftHandWho] : null;
            NPC rightHand = rightHandWho >= 0 ? Main.npc[rightHandWho] : null;
            NPC head = headWho >= 0 ? Main.npc[headWho] : null;

            NPC.immortal = true;
            NPC.dontTakeDamage = true;
            canBeHit = false;
            bool enableHitBox = true;

            float rate = 0.15f;
            bool eyeCenter = NPC.ai[0] == PhantSpin.Id || NPC.ai[0] == PhantSphere.Id;
            bool eyeDeathray = NPC.ai[0] == Deathray.Id && NPC.ai[1] > deathrayWindup - 30;
            Vector2 deathrayConvergePos = new Vector2(NPC.localAI[1], NPC.localAI[2]);
            if (deadTime != 0)
            {
                eyeCenter = true;
                InnerEyePositionUpdate(ref headEyeVector, headPos);
            }
            if (eyeCenter)
                rate = 0.075f;
            if (eyeDeathray)
            {
                if (NPC.ai[1] >= deathrayWindup)
                    rate = 0.5f;
                else
                    rate = 0.075f;
            }
            if (leftHand != null)
            {
                if (leftHand.life > 1)
                {
                    enableHitBox = false;
                    leftHand.Center = leftHandPos;
                    InnerEyePositionUpdate(ref leftEyeVector, leftHandPos);
                }
            }
            if (rightHand != null)
            {
                if(rightHand.life > 1)
                {
                    enableHitBox = false;
                    rightHand.Center = rightHandPos;
                    InnerEyePositionUpdate(ref rightEyeVector, rightHandPos);
                }
            }
            if (NPC.ai[0] == PhantSphere.Id)
                eyeCenter = false;
            if (head != null)
            {
                if (head.life > 1)
                {
                    enableHitBox = false;
                    head.Center = headPos;
                    InnerEyePositionUpdate(ref headEyeVector, headPos);
                }
                else
                {
                    if (deadTime < 130)
                    {
                        headOverlayFrameCounter++;
                    }
                    else
                    {
                        if (headOverlayFrameCounter > 25)
                            headOverlayFrameCounter = 25;
                        if (headOverlayFrameCounter > 0)
                            headOverlayFrameCounter--;
                    }
                }
            }
            
            if (enableHitBox && deadTime == 0)
            {
                if (!goreProc)
                {
                    SoundEngine.PlaySound(SoundID.NPCDeath1 with { Volume = 1f }, NPC.Center);
                    SoundEngine.PlaySound(SoundID.NPCHit57 with { Volume = 0.5f, Pitch = 0.1f }, NPC.Center + new Vector2(0, -300));
                    int[] goreIds = [GoreID.MoonLordHeart1, GoreID.MoonLordHeart2, GoreID.MoonLordHeart3, GoreID.MoonLordHeart4];
                    for (int i = 0; i < 20; i++)
                    {
                        int goreId = goreIds[(i / 5) % 4];
                        Gore.NewGore(NPC.GetSource_Death(), Main.rand.NextVector2Circular(NPC.width * 0.5f, NPC.height * 0.5f) + NPC.position + new Vector2(NPC.width * 0.25f, NPC.height * 0.25f), Main.rand.NextVector2CircularEdge(5, 3) - Vector2.UnitY * 0.5f, goreId, NPC.scale);
                    }
                    goreProc = true;
                }
                NPC.immortal = false;
                NPC.dontTakeDamage = false;
                canBeHit = true;
            }

            if (SoundEngine.TryGetActiveSound(DeathraySlot, out var sound) && sound.IsPlaying)
            {
                sound.Position = deathrayConvergePos;
            }

            void InnerEyePositionUpdate(ref Vector2 eyeVector, Vector2 basePosition)
            {
                if (eyeCenter)
                {
                    eyeVector = Vector2.Lerp(eyeVector, Vector2.Zero, rate);
                }
                else if (eyeDeathray)
                {
                    Vector2 deathrayVect = deathrayConvergePos - basePosition;
                    float maxEyeOffset = 16;
                    if (deathrayVect.Length() > maxEyeOffset)
                        deathrayVect = deathrayVect.SafeNormalize(Vector2.UnitY) * maxEyeOffset;
                    eyeVector = Vector2.Lerp(eyeVector, deathrayVect, rate);
                }
                else
                {
                    Vector2 targetPos = target != null ? target.Center : spawnPos + new Vector2(0, -80);
                    float maxEyeOffset = 16;

                    Vector2 targetVect = targetPos - basePosition;
                    if (targetVect.Length() > maxEyeOffset)
                        targetVect = targetVect.SafeNormalize(Vector2.UnitY) * maxEyeOffset;
                    eyeVector = Vector2.Lerp(eyeVector, targetVect, rate);
                }
            }
        }
        public override void AI()
        {
            if (NPC.localAI[3] == 0)
            {
                if (modNPC.isRoomNPC)
                {
                    Room parentRoom = modNPC.GetParentRoom();
                    if (parentRoom.awake)
                        NPC.localAI[3] = 1;
                }
                else
                    NPC.localAI[3] = 1;
            }

            NPC leftHand = leftHandWho >= 0 ? Main.npc[leftHandWho] : null;
            NPC rightHand = rightHandWho >= 0 ? Main.npc[rightHandWho] : null;
            NPC head = headWho >= 0 ? Main.npc[headWho] : null;

            if (leftHand != null)
            {
                if (leftHand.type != handType)
                {
                    leftHandWho = -1;
                    leftHand = null;
                }
            }
            if (rightHand != null)
            {
                if (rightHand.type != handType)
                {
                    rightHandWho = -1;
                    rightHand = null;
                }
            }
            if (head != null)
            {
                if (head.type != headType)
                {
                    headWho = -1;
                    head = null;
                }
            }

            leftHandMoveInterpolant = rightHandMoveInterpolant = 0.05f; //default move interpolant
            NPC.frameCounter += 0.2d;
            if (deadTime > 0)
            {
                CheckDead();
                return;
            }

            ableToHit = NPC.localAI[0] >= 0 && deadTime == 0;
            if (NPC.localAI[3] != 1)
            {
                if (NPC.localAI[1] > 0)
                    NPC.localAI[1]--;

                idleCounter++;
                float leftPeriod = (float)Math.Cos(idleCounter * 0.008f);
                float RightPeriod = (float)Math.Cos(idleCounter * 0.008f + 0.6f);

                leftHandTargetPos = leftHandAnchor + new Vector2(100, 0) + new Vector2(-8, 16) * leftPeriod;
                rightHandTargetPos = rightHandAnchor + new Vector2(-100, 0) + new Vector2(8, 16) * RightPeriod;
                target = modNPC.GetTarget(NPC);

                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player player = Main.player[i];
                    if (!player.active)
                        continue;
                    var modPlayer = player.ModPlayer();
                    if (modPlayer == null)
                        continue;

                    if (player.Center.Distance(NPC.Center + new Vector2(0, -80)) < 440)
                    {
                        modPlayer.brainSucked = true;
                        if (Main.rand.NextBool(3))
                        {
                            modPlayer.brainSucklerTime--;
                        }
                    }
                }
                return;
            }
                

            if (modNPC.isRoomNPC && NPC.localAI[0] == -(cutsceneDuration + 30))
            {
                SetBossTrack(FinalBoss1Theme);
            }

            if (NPC.localAI[0] < 0)
            {
                target = modNPC.GetTarget(NPC);

                if (NPC.localAI[0] == -cutsceneDuration)
                {
                    CutsceneSystem.SetCutscene(NPC.Center + new Vector2(0, -200), cutsceneDuration, 30, 30, 1.25f, CutsceneSystem.CutsceneSource.Boss);
                }
                NPC.localAI[0]++;

                if (NPC.localAI[0] == -120)
                {
                    SoundEngine.PlaySound(SoundID.NPCHit57 with { Volume = 0.6f, Pitch = -0.15f, PitchVariance = 0 }, NPC.Center + new Vector2(0, -80));
                }
                if (NPC.localAI[0] == -90)
                {
                    SoundEngine.PlaySound(SoundID.NPCDeath58 with { Volume = 1f, Pitch = 0f }, NPC.Center + new Vector2(0, -80));
                }
                if (NPC.localAI[0] > -90)
                {
                    BossAI();
                    if (NPC.localAI[0] == -80)
                    {
                        SoundEngine.PlaySound(Break with { Volume = 0.3f, Pitch = -0.2f }, NPC.Center + new Vector2(0, -80));
                        SoundEngine.PlaySound(SoundID.Shatter with { Volume = 0.15f, Pitch = -0.5f }, NPC.Center + new Vector2(0, -80));
                        float radius = 470;
                        Vector2 basePos = NPC.Center + new Vector2(0, -80);
                        for (int d = 0; d < 16; d++)
                        {
                            float radiusCompletion = (float)Math.Pow(d / 16f, 0.65f);
                            Color color = Color.Lerp(Color.Teal, Color.LightBlue, radiusCompletion) * MathHelper.Lerp(0.6f, 0.7f, radiusCompletion) * 0.8f;
                            for (int i = 0; i < 26; i++)
                            {
                                float completion = i / 26f;
                                float baseRot = MathHelper.TwoPi * (completion);
                                baseRot += Main.rand.NextFloat(-0.2f, 0.2f);
                                Vector2 particleSpawnPos = basePos + baseRot.ToRotationVector2() * radiusCompletion * radius;
                                Vector2 particleVel = baseRot.ToRotationVector2() * Main.rand.NextFloat(4f, 10f) * MathHelper.Lerp(0.75f, 1f, radiusCompletion);
                                particleVel.X *= 1.3f;
                                particleVel.Y -= 2f;
                                ParticleManager.AddParticle(new Shard(
                                    particleSpawnPos, particleVel,
                                    Main.rand.Next(60, 90), color, new Vector2(MathHelper.Lerp(0.2f, 0.3f, completion)), Main.rand.Next(4), Main.rand.NextFloat(MathHelper.TwoPi), 0.99f,
                                    60, 0.1f, Main.rand.NextBool() ? SpriteEffects.None : SpriteEffects.FlipHorizontally, true));
                                ParticleManager.AddParticle(new Square(
                                    particleSpawnPos, particleVel, Main.rand.Next(30, 60), color * 1.4f, new Vector2(1.5f), baseRot, 0.96f, 60, true));
                            }
                        }
                    }
                }
                if (NPC.localAI[0] == -30)
                {
                    NPC.localAI[1] = 0;
                    NPC.immortal = false;
                    NPC.dontTakeDamage = false;
                    if (!TerRoguelikeWorld.escape)
                        enemyHealthBar = new EnemyHealthBar([NPC.whoAmI, headWho, leftHandWho, rightHandWho], NPC.GivenOrTypeName);
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
            NPC.ai[1]++;

            NPC leftHand = leftHandWho >= 0 ? Main.npc[leftHandWho] : null;
            NPC rightHand = rightHandWho >= 0 ? Main.npc[rightHandWho] : null;
            NPC head = headWho >= 0 ? Main.npc[headWho] : null;

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

                    leftHandTargetPos = leftHandAnchor;
                    rightHandTargetPos = rightHandAnchor;
                }
            }

            if (NPC.ai[0] == PhantSpin.Id)
            {
                int start = head.life > 1 ? 0 : 1;

                if (NPC.ai[1] < phantSpinWindup)
                {
                    bool beginning = NPC.ai[1] == 0;
                    if (beginning)
                    {
                        leftHandTargetPos = leftHandAnchor + new Vector2(-32, 0);
                        rightHandTargetPos = rightHandAnchor + new Vector2(32, 0);
                        SoundEngine.PlaySound(SoundID.Zombie93 with { Volume = 0.9f }, NPC.Center + new Vector2(0, -100));
                        if (head != null)
                        {
                            head.direction = Main.rand.NextBool() ? -1 : 1;
                        }
                    }
                    if (NPC.ai[1] % 2 == 0)
                    {
                        for (int i = start; i < 3; i++)
                        {
                            NPC npc = i switch
                            {
                                1 => leftHand,
                                2 => rightHand,
                                _ => head,
                            };
                            if (npc == null)
                                continue;

                            Vector2 offset = Main.rand.NextVector2Circular(48, 48);
                            ParticleManager.AddParticle(new Ball(
                                npc.Center + offset, -offset * 0.1f + npc.velocity,
                                20, Color.Lerp(Color.Teal, Color.Cyan, 0.5f), new Vector2(0.25f), 0, 0.96f, 10));
                        }
                    }
                }
                else
                {
                    if (NPC.ai[1] % 10 == 0)
                    {
                        float projSpeed = 8;
                        for (int i = start; i < 3; i++)
                        {
                            NPC npc = i switch
                            {
                                1 => leftHand,
                                2 => rightHand,
                                _ => head,
                            };
                            if (npc == null)
                                continue;

                            if (npc.life > 1)
                            {
                                float shootBaseRot = NPC.ai[1] * 0.015f * npc.direction + i * 0.4f;
                                for (int j = 0; j < 4; j++)
                                {
                                    float shootRot = shootBaseRot + j * MathHelper.PiOver2;
                                    Vector2 shootRotVector = shootRot.ToRotationVector2();
                                    Projectile.NewProjectile(NPC.GetSource_FromThis(), npc.Center + shootRotVector * npc.width * 0.3f, shootRotVector * projSpeed, ModContent.ProjectileType<PhantasmalEye>(), NPC.damage, 0);
                                }
                            }
                            else
                            {
                                float shootBaseRot = npc.rotation;
                                for (int j = 0; j < 2; j++)
                                {
                                    float shootRot = shootBaseRot + j * MathHelper.Pi;
                                    Vector2 shootRotVector = shootRot.ToRotationVector2();
                                    Projectile.NewProjectile(NPC.GetSource_FromThis(), npc.Center + shootRotVector * npc.width * 0.3f, shootRotVector * projSpeed, ModContent.ProjectileType<PhantasmalEye>(), NPC.damage, 0);
                                }
                            }
                            SoundEngine.PlaySound(SoundID.Item12 with { Volume = 0.4f, MaxInstances = 24 }, npc.Center);
                        }
                    }
                }
                if (NPC.ai[1] >= PhantSpin.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = PhantSpin.Id;
                }
            }
            else if (NPC.ai[0] == PhantBolt.Id)
            {
                Vector2 targetPos = target != null ? target.Center : spawnPos + new Vector2(0, -80);

                if (NPC.ai[1] == 0)
                {
                    SoundEngine.PlaySound(SoundID.Zombie94 with { Volume = 0.8f }, NPC.Center + new Vector2(0, -100));
                    float closest = 2000;
                    if (head != null)
                        head.direction = Main.rand.NextBool() ? -1 : 1;
                    for (int i = 0; i < 3; i++)
                    {
                        NPC npc = i switch
                        {
                            1 => leftHand,
                            2 => rightHand,
                            _ => head,
                        };
                        if (npc == null || (i == 0 && head.life <= 1))
                            continue;

                        float distance = npc.Center.Distance(targetPos);
                        if (distance < closest)
                        {
                            closest = distance;
                            NPC.ai[3] = i;
                            NPC.direction = npc.direction;
                        }
                    }
                }
                if (NPC.ai[3] > -1)
                {
                    bool comboAttack = NPC.ai[1] >= PhantBolt.Duration / 3 * 2;

                    int time = (int)NPC.ai[1] % (phantBoltWindup + phantBoltFiringDuration);
                    int[] order = NPC.ai[3] switch
                    {
                        1 => [1, 2, 0],
                        2 => [2, 1, 0],
                        _ => NPC.direction > 0 ? [0, 2, 1] : [0, 1, 2],
                    };
                    bool played = false;
                    for (int i = 0; i < order.Length; i++)
                    {
                        int n = order[i];
                        NPC npc = n switch
                        {
                            1 => leftHand,
                            2 => rightHand,
                            _ => head,
                        };
                        if (npc == null || (n == 0 && head.life <= 1))
                            continue;

                        float rot = (targetPos - npc.Center).ToRotation();
                        Vector2 projSpawnPos = npc.Center + rot.ToRotationVector2() * 25;

                        int myFiringDuration = comboAttack ? phantBoltFiringDuration : phantBoltFiringDuration / 3;
                        int delay = comboAttack ? 0 : i * myFiringDuration;
                        int effectiveTime = time - delay;
                        int firingTime = effectiveTime - phantBoltWindup;
                        if (effectiveTime >= 0)
                        {
                            if (effectiveTime == 0)
                            {
                                SoundEngine.PlaySound(SoundID.Item13 with { Volume = comboAttack ? 0.66f : 1f, Pitch = 0.2f, MaxInstances = 3 }, npc.Center);
                                if (n == 1)
                                {
                                    leftHandTargetPos = leftHandAnchor + (leftHandAnchor - targetPos).SafeNormalize(Vector2.UnitY) * 80 + Main.rand.NextVector2Circular(80, 80);
                                }
                                else if (n == 2)
                                {
                                    rightHandTargetPos = rightHandAnchor + (rightHandAnchor - targetPos).SafeNormalize(Vector2.UnitY) * 80 + Main.rand.NextVector2Circular(80, 80);
                                }
                            }
                            if (effectiveTime < phantBoltWindup && PhantBolt.Duration - NPC.ai[1] > 10)
                            {
                                if (effectiveTime % 3 == 0)
                                {
                                    Vector2 particlePos = projSpawnPos;
                                    Vector2 addedParticleVel = npc.velocity * 1.2f;
                                    if (npc.life <= 1)
                                        particlePos += rot.ToRotationVector2() * 13;
                                    else
                                    {
                                        if (n == 1)
                                            addedParticleVel = (leftHandTargetPos - leftHandPos) * 0.05f;
                                        else if (n == 2)
                                            addedParticleVel = (rightHandTargetPos - rightHandPos) * 0.05f;
                                    }
                                    float range = MathHelper.PiOver4 * 1.5f;
                                    Vector2 offset = (Main.rand.NextFloat(-range, range) + rot).ToRotationVector2() * Main.rand.NextFloat(32);
                                    ParticleManager.AddParticle(new Ball(
                                        particlePos + offset, -offset * 0.1f + addedParticleVel,
                                        20, Color.Lerp(Color.Teal, Color.Cyan, Main.rand.NextFloat(0.25f, 0.75f)), new Vector2(0.25f), 0, 0.96f, 10));
                                }
                            }
                            else if (firingTime % phantBoltFireRate == 0 && firingTime < myFiringDuration)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), projSpawnPos, rot.ToRotationVector2() * 15, ModContent.ProjectileType<PhantasmalBolt>(), NPC.damage, 0);
                            }
                            if (effectiveTime >= phantBoltWindup && firingTime % 12 == 0 && firingTime < myFiringDuration)
                            {
                                if (!comboAttack || !played)
                                {
                                    played = true;
                                    Vector2 soundPos = npc.Center;
                                    if (comboAttack)
                                    {
                                        int addCount = 0;
                                        soundPos = Vector2.Zero;
                                        if (head != null && head.life > 1)
                                        {
                                            soundPos += head.position;
                                            addCount++;
                                        }
                                        if (leftHand != null)
                                        {
                                            soundPos += leftHand.position;
                                            addCount++;
                                        }
                                        if (rightHand != null)
                                        {
                                            soundPos += rightHand.position;
                                            addCount++;
                                        }
                                        if (addCount == 0)
                                            soundPos = npc.Center;
                                        else
                                            soundPos /= addCount;
                                        soundPos += (Main.LocalPlayer.Center - soundPos) * 0.5f;
                                    }
                                    SoundEngine.PlaySound(SoundID.Item125 with { Volume = comboAttack ? 0.94f : 0.8f, MaxInstances = 6, SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest }, soundPos);
                                }
                                    
                            }
                        }
                    }
                }
                if (NPC.ai[1] >= PhantBolt.Duration || NPC.ai[3] == -1)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = PhantBolt.Id;
                }
            }
            else if (NPC.ai[0] == PhantSphere.Id)
            {
                if (NPC.ai[1] == 0)
                {
                    SoundEngine.PlaySound(SoundID.Zombie96 with { Volume = 0.8f }, NPC.Center + new Vector2(0, -80));
                }
                float completion = NPC.ai[1] / PhantSphere.Duration;
                if (NPC.ai[1] % phantSphereFireRate == 0 && PhantSphere.Duration - NPC.ai[1] > 30)
                {
                    for (int i = 1; i <= 2; i++)
                    {
                        NPC npc = i switch
                        {
                            2 => rightHand,
                            _ => leftHand,
                        };
                        if (npc == null)
                            continue;

                        if (i == 1)
                        {
                            leftHandTargetPos = leftHandAnchor + new Vector2(-32, -64);
                            leftHandTargetPos += new Vector2(Main.rand.NextFloat(-96, 96), completion * 184);
                        }
                        else
                        {
                            rightHandTargetPos = rightHandAnchor + new Vector2(32, -64);
                            rightHandTargetPos += new Vector2(Main.rand.NextFloat(-96, 96), completion * 184);
                        }

                        SoundEngine.PlaySound(SoundID.DD2_DarkMageCastHeal with { Volume = 1f, MaxInstances = 10 }, npc.Center);
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), npc.Center, (-Vector2.UnitY * 10).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)), ModContent.ProjectileType<PhantasmalSphere>(), NPC.damage, 0, -1, npc.whoAmI);
                    }
                }
                if (NPC.ai[1] >= PhantSphere.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = -60;
                    NPC.ai[2] = PhantSphere.Id;
                }
            }
            else if (NPC.ai[0] == TentacleCharge.Id)
            {
                Vector2 targetPos = target != null ? target.Center : NPC.Center + new Vector2(0, -80);
                Vector2 projSpawnPos = headPos + new Vector2(0, 209);
                float startSmoothing = 1f;
                if (NPC.ai[1] < tentacleWindup)
                {
                    startSmoothing *= NPC.ai[1] / tentacleWindup;
                    if (NPC.ai[1] == 10)
                    {
                        SoundEngine.PlaySound(SoundID.Zombie98 with { Volume = 0.9f, Pitch = -0.2f }, NPC.Center + new Vector2(0, -80));
                        SoundEngine.PlaySound(SoundID.NPCDeath10 with { Volume = 0.3f, Pitch = -0.3f }, NPC.Center + new Vector2(0, -80));
                    }
                    if (NPC.ai[1] > 10 && (NPC.ai[1] < 60 ? NPC.ai[1] % 16 == 0 : NPC.ai[1] % 9 == 0 ))
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            Color outlineColor = Color.Lerp(Color.Teal, Color.Cyan, 0.4f);
                            Color fillColor = Color.Lerp(outlineColor, Color.Black, 0.6f);
                            Vector2 particleVel = Vector2.UnitY.RotatedBy(Main.rand.NextFloat(-0.7f, 0.7f) * 2);
                            if (i == 0)
                                particleVel.Y *= 2f;
                            ParticleManager.AddParticle(new BallOutlined(
                                projSpawnPos + new Vector2(Main.rand.NextFloat(-4, 4), Main.rand.NextFloat(-5, -8)), particleVel,
                                30, outlineColor, fillColor, new Vector2(0.2f), 4, 0, 0.96f, 30));
                        }
                    }
                }
                else
                {
                    if (NPC.ai[1] == tentacleWindup)
                    {
                        ExtraSoundSystem.ExtraSounds.Add(new ExtraSound(SoundEngine.PlaySound(SoundID.DD2_BetsyDeath with { Volume = 1f, Pitch = -1f, Variants = [0] }, NPC.Center + new Vector2(0, -80)), 2.2f)); // shove it into extra sound system to amplify it beyond 1f
                        for (int i = -3; i <= 3; i += 2)
                        {
                            float shootRot = MathHelper.PiOver2 + MathHelper.PiOver4 * 0.5f * i;
                            Vector2 shootRotVect = shootRot.ToRotationVector2();
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), projSpawnPos + shootRotVect * 3, shootRotVect * 6.75f, ModContent.ProjectileType<Tentacle>(), NPC.damage, 0);
                        }
                    }
                }
                if (TentacleCharge.Duration - NPC.ai[1] > 180)
                {
                    leftHandTargetPos = leftHandPos + (leftHandPos - targetPos).SafeNormalize(Vector2.UnitY) * 4 * startSmoothing;
                    rightHandTargetPos = rightHandPos + (rightHandPos - targetPos).SafeNormalize(Vector2.UnitY) * 4 * startSmoothing;
                }
                else if (TentacleCharge.Duration - NPC.ai[1] < 90)
                {
                    leftHandTargetPos = leftHandAnchor;
                    rightHandTargetPos = rightHandAnchor;
                }
                if (NPC.ai[1] >= TentacleCharge.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = TentacleCharge.Id;
                }
            }
            else if (NPC.ai[0] == Deathray.Id)
            {
                int timeToEnd = Deathray.Duration - (int)NPC.ai[1];
                Vector2 targetPos = target != null ? target.Center : NPC.Center + new Vector2(0, -80);
                Vector2 deathrayConvergePos = new Vector2(NPC.localAI[1], NPC.localAI[2]);
                if (NPC.ai[1] < deathrayWindup)
                {
                    if (NPC.ai[1] == 0)
                    {
                        SoundEngine.PlaySound(WallOfFlesh.HellBeamCharge with { Volume = 0.9f, Pitch = 0.32f }, NPC.Center + new Vector2(0, -80));
                        SoundEngine.PlaySound(SoundID.Zombie95 with { Volume = 0.45f }, NPC.Center + new Vector2(0, -80));
                    }
                    if (NPC.ai[1] < deathrayWindup - 30)
                    {
                        deathrayConvergePos = targetPos;
                    }
                    if (NPC.ai[1] < deathrayWindup - 20)
                    {
                        if (NPC.ai[1] % 2 == 0)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                NPC npc = i switch
                                {
                                    1 => leftHand,
                                    2 => rightHand,
                                    _ => head,
                                };
                                if (npc == null || npc.life <= 1)
                                    continue;

                                Vector2 eyeVector = i switch
                                {
                                    1 => leftEyeVector,
                                    2 => rightEyeVector,
                                    _ => headEyeVector
                                };

                                Vector2 offset = Main.rand.NextVector2Circular(12, 12);
                                offset += eyeVector.SafeNormalize(Vector2.UnitY) * 12;
                                ParticleManager.AddParticle(new Ball(
                                    npc.Center + offset + eyeVector, -offset * 0.1f + npc.velocity,
                                    20, Color.Lerp(Color.White, Color.Cyan, 0.75f), new Vector2(0.25f), 0, 0.96f, 10));
                            }
                        }
                    }
                }
                else
                {
                    int time = (int)NPC.ai[1] - deathrayWindup;
                    if (RuinedMoonActive)
                    {
                        if (time > 90 && timeToEnd > 20 && Main.rand.NextBool(9))
                        {
                            time -= 5;
                            NPC.ai[1] -= 5;
                        }
                    }
                    if (NPC.ai[1] == deathrayWindup)
                    {
                        DeathraySlot = SoundEngine.PlaySound(SoundID.Zombie104 with { Volume = 0.7f, Pitch = -0.25f }, NPC.Center + new Vector2(0, -80));
                    }

                    float deathraySpeed = 5.5f;
                    if (time < 60)
                    {
                        deathraySpeed *= (float)Math.Pow(time / 60f, 0.5f);
                    }

                    if (target != null)
                    {
                        Vector2 addedVector = targetPos - deathrayConvergePos;
                        if (addedVector.Length() > deathraySpeed)
                            addedVector = addedVector.SafeNormalize(Vector2.UnitY) * deathraySpeed;
                        deathrayConvergePos += addedVector;
                    }

                    if (NPC.ai[1] % 5 == 0)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), deathrayConvergePos, Vector2.Zero, ModContent.ProjectileType<PhantasmalResidue>(), NPC.damage, 0);
                    }
                    if (timeToEnd >= 3)
                    {
                        ParticleManager.AddParticle(new Glow(
                            deathrayConvergePos, Main.rand.NextVector2Circular(6, 6), 5, Color.Cyan, new Vector2(0.25f), 0, 0.98f, 5, true));
                        ParticleManager.AddParticle(new Ball(
                            deathrayConvergePos, Main.rand.NextVector2Circular(6, 6), 5, Color.White * 0.18f, new Vector2(3f), 0, 0.96f, 5, false));
                    }

                    for (int i = 0; i < 3; i++)
                    {
                        NPC npc = i switch
                        {
                            1 => leftHand,
                            2 => rightHand,
                            _ => head
                        };
                        if (npc == null || (i == 0 && npc.life <= 1))
                            continue;

                        Vector2 basePos = npc.Center;
                        float particleRot = (deathrayConvergePos - basePos).ToRotation();
                        float randRot = Main.rand.NextFloat(-0.7f, 0.7f);
                        Vector2 particleVel = (Vector2.UnitX * Main.rand.NextFloat(1, 2)).RotatedBy(randRot + Math.Sign(randRot) * 1.2f + particleRot);
                        ParticleManager.AddParticle(new BallOutlined(
                            basePos + particleRot.ToRotationVector2() * (npc.life > 1 ? 18 : 24), particleVel + npc.velocity, 30, Color.Lerp(Color.Teal, Color.Cyan, 0.5f), Color.White * 0.5f, new Vector2(0.2f), 4, 0, 0.96f, 15));
                    }
                    if (timeToEnd > 20)
                    {
                        Vector2 pseudoVelocity = deathrayConvergePos - new Vector2(NPC.localAI[1], NPC.localAI[2]);
                        for (int i = 0; i < 3; i++)
                        {
                            Vector2 offset = Main.rand.NextVector2CircularEdge(26, 26) * Main.rand.NextFloat(0.7f, 1f);
                            ParticleManager.AddParticle(new Ball(
                                 deathrayConvergePos + offset, offset * 0.15f, 40, Color.White * 0.5f, new Vector2(0.3f, 0.1f), offset.ToRotation(), 0.98f, 20, false));
                        }
                    }
                }

                NPC.localAI[1] = deathrayConvergePos.X;
                NPC.localAI[2] = deathrayConvergePos.Y;

                if (NPC.ai[1] >= Deathray.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Deathray.Id;
                }
            }
            else if (NPC.ai[0] == Summon.Id)
            {
                if (NPC.ai[1] < summonWindup)
                {
                    if (NPC.ai[1] == 0)
                    {
                        NPC.direction = Main.rand.NextBool() ? -1 : 1;
                        SoundEngine.PlaySound(SoundID.Zombie99 with { Volume = 0.6f, Pitch = -0.1f }, NPC.Center + new Vector2(0, -80));
                        SoundEngine.PlaySound(SoundID.DD2_DarkMageCastHeal with { Volume = 1f, Pitch = -0.4f }, NPC.Center + new Vector2(0, -80));
                    }
                }
                else
                {
                    int time = (int)NPC.ai[1] - summonWindup;
                    int summonTimeBetween = 12;
                    if (time % summonTimeBetween == 0)
                    {
                        int summonCounter = time / summonTimeBetween;
                        Vector2 summonPos = headPos + new Vector2(0, 80);
                        float xOff = 50;
                        float yOff = 25;
                        Vector2 summonOffset = new Vector2(summonCounter <= 1 ? -xOff : xOff, summonCounter % 3 == 0 ? yOff : -yOff);
                        summonOffset.X *= NPC.direction;

                        summonPos += summonOffset;
                        NPC spawnedNPC = NPC.NewNPCDirect(NPC.GetSource_FromThis(), summonPos, ModContent.NPCType<TrueServant>(), 0, 0, -60);
                        spawnedNPC.velocity = summonOffset.SafeNormalize(Vector2.UnitY) * 2;
                        spawnedNPC.rotation = summonOffset.ToRotation() + MathHelper.PiOver2;

                        SoundEngine.PlaySound(SoundID.DD2_WitherBeastAuraPulse with { Volume = 1f, MaxInstances = 2 }, summonPos);
                        SoundEngine.PlaySound(SoundID.DD2_SkeletonSummoned with { Volume = 1f, MaxInstances = 2 }, summonPos);
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
            int chosenAttack = 0;

            List<Attack> potentialAttacks = new List<Attack>() { PhantSpin, PhantBolt, PhantSphere, TentacleCharge, Deathray, Summon };
            potentialAttacks.RemoveAll(x => x.Id == (int)NPC.ai[2]);

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
        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            return canBeHit ? null : false;
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
            NPC.life = 1;
            NPC.immortal = true;
            NPC.dontTakeDamage = true;
            NPC.active = true;
            ableToHit = false;
            canBeHit = false;
            modNPC.CleanseDebuffs();

            if (deadTime == 0)
            {
                leftHandTargetPos = leftHandAnchor;
                rightHandTargetPos = rightHandAnchor;
                SoundEngine.PlaySound(SoundID.NPCHit57 with { Volume = 0.6f }, NPC.Center + new Vector2(0, -300));
                SoundEngine.PlaySound(SoundID.NPCDeath62 with { Volume = 0.8f }, NPC.Center);
                SoundEngine.PlaySound(MoonLordDeath with { Volume = 0.45f, SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest }, NPC.Center + new Vector2(0, -100));
                ExtraSoundSystem.ForceStopAllExtraSounds();
                enemyHealthBar.ForceEnd(0);
                NPC.velocity = Vector2.Zero;
                NPC.rotation = 0;

                if (modNPC.isRoomNPC)
                {
                    if (ActiveBossTheme != null)
                        ActiveBossTheme.endFlag = true;
                    Room room = RoomList[modNPC.sourceRoomListID];
                    room.bossDead = true;
                    ClearChildren();
                }

                Room.ClearSpecificProjectiles(); // clear projectiles early since the death anim is long and I want nothing getting in the way

                if (headWho >= 0)
                {
                    Main.npc[headWho].ai[0] = 1;
                }
                if (leftHandWho >= 0)
                {
                    Main.npc[leftHandWho].ai[0] = 1;
                }
                if (rightHandWho >= 0)
                {
                    Main.npc[rightHandWho].ai[0] = 1;
                }
                CutsceneSystem.SetCutscene(NPC.Center + new Vector2(0, -200), deathCutsceneDuration, 30, 30, 1.000001f, CutsceneSystem.CutsceneSource.Boss);
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
            deadTime++;

            if (deadTime < deathBlackWhiteStartTime)
            {
                if (deadTime % 3 == 0) // kinda vanilla moonlord death anim explosion spawning code, ported over
                {
                    Vector2 randPos = Main.rand.NextVector2Circular(1, 1);
                    randPos *= 20 + Main.rand.NextFloat() * 450;
                    randPos += NPC.Center;
                    ParticleManager.AddParticle(new MoonExplosion(randPos, Main.rand.Next(14, 22), Color.White, new Vector2(1f)));

                    float dustCount = Main.rand.Next(6, 19);
                    float offetPerLoop = MathHelper.TwoPi / dustCount;
                    float randAngleOffset = Main.rand.NextFloat(MathHelper.TwoPi);
                    float randOffset = 1f + Main.rand.NextFloat(2f);
                    float randScale = 1f + Main.rand.NextFloat();
                    float fadeIn = 0.4f + Main.rand.NextFloat();

                    for (float i = 0f; i < dustCount * 2f; i++)
                    {
                        Dust dust = Dust.NewDustDirect(randPos, 0, 0, DustID.Vortex, 0f, 0f, 0, default);
                        dust.noGravity = true;
                        dust.position = randPos;
                        double rotateBy = randAngleOffset + offetPerLoop * i;
                        dust.velocity = Vector2.UnitY.RotatedBy(rotateBy) * randOffset * (Main.rand.NextFloat() * 1.6f + 1.6f);
                        dust.fadeIn = fadeIn;
                        dust.scale = randScale;
                    }
                }
                if (deadTime % 15 == 0) // same deal
                {
                    Vector2 randPos = Utils.RandomVector2(Main.rand, -1f, 1f);
                    randPos *= 20f + Main.rand.NextFloat() * 400f;
                    Vector2 projPos = NPC.Center + randPos;
                    Vector2 spinningPoint = new Vector2(0f, (0f - Main.rand.NextFloat()) * 0.5f - 0.5f);
                    double randRot = (float)(Main.rand.Next(4) < 2).ToDirectionInt() * ((float)Math.PI / 8f + (float)Math.PI / 4f * Main.rand.NextFloat());
                    Vector2 projVel = Utils.RotatedBy(spinningPoint, randRot) * 6f;
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), projPos.X, projPos.Y, projVel.X, projVel.Y, 622, 0, 0f, Main.myPlayer);
                }
                
                if (deadTime >= deathBlackWhiteStartTime - 60)
                {
                    float glowScaleMulti = MathHelper.Clamp((deadTime - (deathBlackWhiteStartTime - 60)) / 60f, 0, 1);
                    glowScaleMulti = (float)Math.Pow(glowScaleMulti, 2);
                    ParticleManager.AddParticle(new Glow(NPC.Center, Vector2.Zero, 60, Color.White * 0.3f, new Vector2(5.5f, 4.4f) * glowScaleMulti, Main.rand.NextFloat(MathHelper.TwoPi), 0.96f, 60, true));
                }
            }

            if (deadTime >= deathBlackWhiteStartTime && deadTime < deathBlackWhiteStopTime)
            {
                headCurrentFrame = 1;
                if (deadTime == deathBlackWhiteStartTime)
                {
                    ExtraSoundSystem.ExtraSounds.Add(new(SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Volume = 1f, Pitch = -1f, Variants = [0]}, headPos), 2f));
                    SoundEngine.PlaySound(SoundID.Zombie103 with { Volume = 0.4f, Pitch = -0.5f, PitchVariance = 0 }, headPos);
                    SoundEngine.PlaySound(SoundID.Item73 with { Volume = 0.4f, Pitch = -0.5f, PitchVariance = 0 }, headPos);
                    SoundEngine.PlaySound(SoundID.NPCDeath12 with { Volume = 0.4f, Pitch = -0.8f }, headPos);
                    SoundEngine.PlaySound(SoundID.Zombie96 with { Volume = 0.4f, Pitch = -0.1f }, headPos);
                }
                float headRaiseInterpolant = MathHelper.Clamp((deadTime - deathBlackWhiteStartTime) / 140f, 0, 1);
                headRaiseInterpolant = (float)Math.Pow(headRaiseInterpolant - 1, 4);
                CutsceneSystem.cameraTargetCenter.Y += headRaiseInterpolant * -17f;
            }
            else if (deadTime == deathBlackWhiteStopTime)
            {
                Color goreColor = Color.Lerp(Color.LightGray, Color.White, 0.35f);
                int direction = Main.rand.NextBool() ? -1 : 1;
                ParticleManager.AddParticle(new BigGore(TexDict["MoonDeadSpine"], NPC.Center + new Vector2(32, 240), Vector2.UnitX * direction * 2 - Vector2.UnitY * Main.rand.NextFloat(3.6f, 4.4f), 500,
                    goreColor, new Vector2(1f), 0.25f, 20, 0, direction, SpriteEffects.None, 0.015f),
                    ParticleManager.ParticleLayer.BehindTiles);
                direction = Main.rand.NextBool() ? -1 : 1;
                ParticleManager.AddParticle(new BigGore(TexDict["MoonDeadShoulder"], NPC.Center + new Vector2(84, -152), Vector2.UnitX * direction * 2 - Vector2.UnitY * Main.rand.NextFloat(3.6f, 4.4f), 500,
                    goreColor, new Vector2(1f), 0.25f, 20, 0, direction, SpriteEffects.None, 0.015f),
                    ParticleManager.ParticleLayer.BehindTiles);
                direction = Main.rand.NextBool() ? -1 : 1;
                ParticleManager.AddParticle(new BigGore(TexDict["MoonDeadTorso"], NPC.Center + new Vector2(0, 48), Vector2.UnitX * direction * 2 - Vector2.UnitY * Main.rand.NextFloat(3.6f, 4.4f), 500,
                    goreColor, new Vector2(1f), 0.25f, 20, 0, direction, SpriteEffects.None, 0.015f),
                    ParticleManager.ParticleLayer.BehindTiles);
                direction = Main.rand.NextBool() ? -1 : 1;
                ParticleManager.AddParticle(new BigGore(TexDict["MoonDeadHead"], headPos + new Vector2(0, 90), Vector2.UnitX * direction * 2 - Vector2.UnitY * Main.rand.NextFloat(3.6f, 4.4f), 500,
                    goreColor, new Vector2(1f), 0.25f, 20, 0, direction, SpriteEffects.None, 0.015f),
                    ParticleManager.ParticleLayer.BehindTiles);
            }
            else if (deadTime == deathBlackWhiteStopTime + 30)
            {
                SoundEngine.PlaySound(SoundID.Zombie102 with { Volume = 0.2f, Pitch = -0.5f, SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest }, NPC.Center + new Vector2(0, -400));
            }
            else if (deadTime == deathBlackWhiteStopTime + 60)
            {
                SoundEngine.PlaySound(SoundID.Zombie101 with { Volume = 0.3f, Pitch = -0.1f }, NPC.Center + new Vector2(0, -400));
            }
            if (deadTime >= deathBlackWhiteStopTime - 60 && deadTime < deathBlackWhiteStopTime)
            {
                float glowScaleMulti = MathHelper.Clamp(1f - ((deadTime - (deathBlackWhiteStopTime)) / 60f), 0, 1);
                glowScaleMulti = (float)Math.Pow(glowScaleMulti, 2);
                ParticleManager.AddParticle(new Glow(NPC.Center, Vector2.Zero, 60, Color.White * 0.3f, new Vector2(3f, 2.4f) * glowScaleMulti, Main.rand.NextFloat(MathHelper.TwoPi), 0.96f, 60, true));
            }
            if (deadTime == deathBlackWhiteStopTime - 5)
            {
                SoundEngine.PlaySound(SoundID.NPCHit57 with { Volume = 0.4f, Pitch = 0.5f, PitchVariance = 0, SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest }, headPos);
            }

            if (deadTime > deathBlackWhiteStopTime + 60)
            {
                float brainRaiseInterpolant = MathHelper.Clamp((deadTime - deathBlackWhiteStartTime) / 140f, 0, 1);
                brainRaiseInterpolant = 1 + -(float)Math.Pow(brainRaiseInterpolant - 1, 4);
                float brainMoveRightInterpolant = MathHelper.Clamp((deadTime - (deathBlackWhiteStopTime + 30)) / 60f, 0, 1);
                brainMoveRightInterpolant = 1 + -(float)Math.Pow(brainMoveRightInterpolant - 1, 2);
                float brainMoveLeftInterpolant = 0;
                if (deadTime - deathBlackWhiteStopTime - 60 >= 0)
                {
                    brainMoveLeftInterpolant = Math.Max((float)Math.Pow((deadTime - deathBlackWhiteStopTime - 60) / 180f, 2f), 0);
                }

                int amount = deadTime > deathBlackWhiteStopTime + 90 ? 3 : 1;
                for (int i = 0; i < amount; i++)
                {
                    Vector2 trueBrainPos = headPos + new Vector2(0, 0) + new Vector2(0, -280) * brainRaiseInterpolant;
                    trueBrainPos += brainMoveRightInterpolant * new Vector2(360, 0);
                    trueBrainPos += brainMoveLeftInterpolant * new Vector2(-20000, 0);

                    trueBrainPos.X += Main.rand.NextFloat(80, 120);
                    trueBrainPos.Y += Main.rand.NextFloat(-80, 80) - 24;

                    Color particleColor = Color.Lerp(Color.Teal, Color.White, 0.2f);
                    ParticleManager.AddParticle(new Wriggler(
                        trueBrainPos, Vector2.UnitX * -5,
                        26, particleColor, new Vector2(0.5f), Main.rand.Next(4), MathHelper.Pi + Main.rand.NextFloat(-0.2f, 0.2f), 0.98f, 16,
                        Main.rand.NextBool() ? SpriteEffects.None : SpriteEffects.FlipVertically));

                    Vector2 offset = Main.rand.NextVector2Circular(2, 2);
                    ParticleManager.AddParticle(new Ball(
                        trueBrainPos + offset, offset,
                        20, Color.Teal, new Vector2(0.25f), 0, 0.96f, 10));
                }   
            }
            if (deadTime >= deathCutsceneDuration - 40)
            {
                modNPC.ignoreForRoomClearing = true;
            }
            if (deadTime >= deathCutsceneDuration)
            {
                NPC.immortal = false;
                NPC.dontTakeDamage = false;
                NPC.life = 0;
            }

            return deadTime >= cutsceneDuration;
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0 && deadTime == 0)
            {
                SoundEngine.PlaySound(SoundID.NPCHit1 with { Volume = 1f }, NPC.Center);
                for (int i = 0; (double)i < hit.Damage / (double)NPC.lifeMax * 1000.0; i++)
                {
                    int d = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Vortex, hit.HitDirection, -1f);
                    Main.dust[d].noLight = true;
                    Main.dust[d].noLightEmittence = true;
                }
            }
        }
        public override void OnKill()
        {
            
        }
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            bool topEyeCanHit = headWho >= 0 && Main.npc[headWho].life > 1;
            float eyeHitRadius = 30;
            for (int i = topEyeCanHit ? 0 : 1; i < 3; i++)
            {
                var checkPos = i switch
                {
                    0 => headPos,
                    1 => leftHandPos,
                    _ => rightHandPos,
                };

                Vector2 targetVectorToPos = (target.getRect().ClosestPointInRect(checkPos) - checkPos);
                targetVectorToPos.X *= 1.6f; // simulates an oval-like shape

                if (targetVectorToPos.Length() <= eyeHitRadius)
                {
                    CollisionPass = ableToHit;
                    return ableToHit;
                }
            }
            float trueEyeHitRadius = 36;
            if (leftHandWho >= 0 && Main.npc[leftHandWho].life == 1)
            {
                Vector2 checkPos = Main.npc[leftHandWho].Center;
                Vector2 targetVectorToPos = target.getRect().ClosestPointInRect(checkPos) - checkPos;
                if (targetVectorToPos.Length() <= trueEyeHitRadius)
                {
                    CollisionPass = ableToHit;
                    return ableToHit;
                }
            }
            if (rightHandWho >= 0 && Main.npc[rightHandWho].life == 1)
            {
                Vector2 checkPos = Main.npc[rightHandWho].Center;
                Vector2 targetVectorToPos = target.getRect().ClosestPointInRect(checkPos) - checkPos;
                if (targetVectorToPos.Length() <= trueEyeHitRadius)
                {
                    CollisionPass = ableToHit;
                    return ableToHit;
                }
            }

            if (NPC.ai[0] == Deathray.Id && NPC.ai[1] >= deathrayWindup)
            {
                Vector2 deathrayConvergePos = new Vector2(NPC.localAI[1], NPC.localAI[2]);
                float deathrayRadius = 18;
                for (int i = topEyeCanHit ? 0 : 1; i < 3; i++)
                {
                    NPC npc = i switch
                    {
                        1 => leftHandWho >= 0 ? Main.npc[leftHandWho] : null,
                        2 => rightHandWho >= 0 ? Main.npc[rightHandWho] : null,
                        _ => headWho >= 0 ? Main.npc[headWho] : null
                    };
                    if (npc == null)
                        continue;

                    Vector2 startCheckPos = npc.Center;
                    Vector2 deathrayVect = deathrayConvergePos - startCheckPos;
                    float deathrayLength = deathrayVect.Length();
                    Vector2 deathrayNormalVect = deathrayVect.SafeNormalize(Vector2.UnitY);
                    for (int d = 24; d < deathrayLength; d += 18)
                    {
                        Vector2 checkPos = startCheckPos + deathrayNormalVect * d;
                        Vector2 closestPoint = target.getRect().ClosestPointInRect(checkPos);
                        if (checkPos.Distance(closestPoint) < deathrayRadius)
                        {
                            CollisionPass = false;
                            if (ableToHit)
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), closestPoint, Vector2.Zero, ModContent.ProjectileType<PhantasmalDeathray>(), NPC.damage, 0);
                            return false;
                        }
                    }
                }
            }
            CollisionPass = false;
            return false;
        }
        public override bool CanHitNPC(NPC target)
        {
            bool leftHandAlive = leftHandWho >= 0 && Main.npc[leftHandWho].life > 1;
            bool rightHandAlive = rightHandWho >= 0 && Main.npc[rightHandWho].life > 1;
            bool topEyeCanHit = headWho >= 0 && Main.npc[headWho].life > 1;
            float eyeHitRadius = 30;
            for (int i = topEyeCanHit ? 0 : 1; i < 3; i++)
            {
                var checkPos = i switch
                {
                    0 => headPos,
                    1 => leftHandPos,
                    _ => rightHandPos,
                };

                Vector2 targetVectorToPos = (target.Hitbox.ClosestPointInRect(checkPos) - checkPos);
                targetVectorToPos.X *= 1.6f; // simulates an oval-like shape

                if (targetVectorToPos.Length() <= eyeHitRadius)
                {
                    CollisionPass = ableToHit;
                    return ableToHit;
                }
            }
            float trueEyeHitRadius = 36;
            if (leftHandWho >= 0 && Main.npc[leftHandWho].life == 1)
            {
                Vector2 checkPos = Main.npc[leftHandWho].Center;
                Vector2 targetVectorToPos = target.Hitbox.ClosestPointInRect(checkPos) - checkPos;
                if (targetVectorToPos.Length() <= trueEyeHitRadius)
                {
                    CollisionPass = ableToHit;
                    return ableToHit;
                }
            }
            if (rightHandWho >= 0 && Main.npc[rightHandWho].life == 1)
            {
                Vector2 checkPos = Main.npc[rightHandWho].Center;
                Vector2 targetVectorToPos = target.Hitbox.ClosestPointInRect(checkPos) - checkPos;
                if (targetVectorToPos.Length() <= trueEyeHitRadius)
                {
                    CollisionPass = ableToHit;
                    return ableToHit;
                }
            }

            if (NPC.ai[0] == Deathray.Id && NPC.ai[1] >= deathrayWindup)
            {
                Vector2 deathrayConvergePos = new Vector2(NPC.localAI[1], NPC.localAI[2]);
                float deathrayRadius = 18;
                for (int i = topEyeCanHit ? 0 : 1; i < 3; i++)
                {
                    NPC npc = i switch
                    {
                        1 => leftHandWho >= 0 ? Main.npc[leftHandWho] : null,
                        2 => rightHandWho >= 0 ? Main.npc[rightHandWho] : null,
                        _ => headWho >= 0 ? Main.npc[headWho] : null
                    };
                    if (npc == null)
                        continue;

                    Vector2 startCheckPos = npc.Center;
                    Vector2 deathrayVect = deathrayConvergePos - startCheckPos;
                    float deathrayLength = deathrayVect.Length();
                    Vector2 deathrayNormalVect = deathrayVect.SafeNormalize(Vector2.UnitY);
                    for (int d = 24; d < deathrayLength; d += 18)
                    {
                        Vector2 checkPos = startCheckPos + deathrayNormalVect * d;
                        Vector2 closestPoint = target.getRect().ClosestPointInRect(checkPos);
                        if (checkPos.Distance(closestPoint) < deathrayRadius)
                        {
                            CollisionPass = false;
                            if (ableToHit)
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), closestPoint, Vector2.Zero, ModContent.ProjectileType<PhantasmalDeathray>(), NPC.damage, 0);
                            return false;
                        }
                    }
                }
            }
            CollisionPass = (leftHandAlive || rightHandAlive || topEyeCanHit) ? false : NPC.getRect().Intersects(target.Hitbox);
            return false;
        }
        public override bool ModifyCollisionData(Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox)
        {
            if (CollisionPass)
            {
                npcHitbox = new Rectangle(0, 0, Main.maxTilesX * 16, Main.maxTilesY * 16);
            }
            return CollisionPass;
        }
        public override void ModifyHoverBoundingBox(ref Rectangle boundingBox)
        {
            boundingBox = NPC.Hitbox;
        }
        public override void FindFrame(int frameHeight)
        {
            var baseTex = TextureAssets.Npc[Type];
            NPC.frame = baseTex.Frame(sizeOffsetY: -180);

            bool hardMode = (int)difficulty >= (int)Difficulty.BloodMoon;

            bool leftHandAlive = leftHandWho >= 0 && Main.npc[leftHandWho].life > 1;
            bool rightHandAlive = rightHandWho >= 0 && Main.npc[rightHandWho].life > 1;
            bool headAlive = headWho >= 0 && Main.npc[headWho].life > 1;

            int headForceFrame = -1;
            int leftHandForceFrame = -1;
            int rightHandForceFrame = -1;
            int mouthForceFrame = 0;

            int sharedFrame = 0;
            if (NPC.localAI[3] == 0)
            {
                sharedFrame = (int)(idleCounter % 680 * 0.1f);
            }
            else if (NPC.ai[0] == None.Id)
            {
                float rate = 0.125f;
                if (hardMode)
                    rate *= 0.5f;
                sharedFrame = (int)(NPC.ai[1] * rate);
            }
            else if (NPC.ai[0] == PhantBolt.Id && NPC.ai[1] >= (phantBoltWindup + phantBoltFiringDuration) * 2)
            {
                sharedFrame = (int)((NPC.ai[1] % (phantBoltWindup + phantBoltFiringDuration)) * 0.2f);
            }
            if (NPC.ai[0] == PhantSphere.Id)
            {
                leftHandForceFrame = 1;
                rightHandForceFrame = 1;
            }
            if (NPC.ai[0] == TentacleCharge.Id)
            {
                if (NPC.ai[1] < 15 || TentacleCharge.Duration - NPC.ai[1] < 15)
                    mouthForceFrame = 1;
                else
                    mouthForceFrame = 2;
            }
            if (NPC.localAI[0] < 0)
            {
                if (NPC.localAI[0] > -110)
                {
                    if (NPC.localAI[0] <= -100)
                    {
                        mouthForceFrame = 1;
                    }
                    else if (NPC.localAI[0] <= -70)
                    {
                        mouthForceFrame = 2;
                    }
                    else if (NPC.localAI[0] <= -62)
                    {
                        mouthForceFrame = 1;
                    }
                }
            }
            if (sharedFrame > 6)
                sharedFrame = 0;
            else
            {
                switch (sharedFrame)
                {
                    default:
                    case 0:
                    case 6:
                        sharedFrame = 0;
                        break;
                    case 1:
                    case 5:
                        sharedFrame = 1;
                        break;
                    case 2:
                    case 4:
                        sharedFrame = 2;
                        break;
                    case 3:
                        sharedFrame = 3;
                        break;
                }
            }

            frameHeight = coreTex.Height / 5;
            coreCurrentFrame = leftHandAlive || rightHandAlive || headAlive ? 0 : (int)NPC.frameCounter % 4 + 1;
            coreFrame = new Rectangle(0, coreCurrentFrame * frameHeight, coreTex.Width, frameHeight - 2);

            frameHeight = mouthTex.Height / 3;
            mouthCurrentFrame = mouthForceFrame;
            mouthFrame = new Rectangle(0, mouthCurrentFrame * frameHeight, mouthTex.Width, frameHeight - 2);

            frameHeight = topEyeOverlayTex.Height / 4;
            headEyeCurrentFrame = headOverlayFrameCounter == 0 ? (headForceFrame >= 0 ? headForceFrame : sharedFrame) : Math.Min(headOverlayFrameCounter / 8 + 1, 3);
            headEyeFrame = new Rectangle(0, headEyeCurrentFrame * frameHeight, topEyeOverlayTex.Width, frameHeight - 2);

            frameHeight = handTex.Height / 4;
            leftHandCurrentFrame = leftHandForceFrame >= 0 ? leftHandForceFrame : sharedFrame;
            leftHandFrame = new Rectangle(0, leftHandCurrentFrame * frameHeight, handTex.Width, frameHeight - 2);

            rightHandCurrentFrame = rightHandForceFrame >= 0 ? rightHandForceFrame : sharedFrame;
            rightHandFrame = new Rectangle(0, rightHandCurrentFrame * frameHeight, handTex.Width, frameHeight - 2);

            frameHeight = emptyEyeTex.Height / 4;
            emptyEyeCurrentFrame = (int)NPC.frameCounter % 4;
            emptyEyeFrame = new Rectangle(0, emptyEyeCurrentFrame * frameHeight, emptyEyeTex.Width, frameHeight - 2);

            frameHeight = headTex.Height / 2;
            headFrame = new Rectangle(0, headCurrentFrame * frameHeight, headTex.Width, frameHeight - 2);
        }
        public static void DrawDeathrayForNPC(NPC npc, NPC parent)
        {
            if (parent.ai[0] == Deathray.Id && parent.ai[1] >= deathrayWindup - 30)
            {
                var deathrayTex = TextureAssets.Projectile[ModContent.ProjectileType<PhantasmalDeathray>()].Value;
                int timeToEnd = Deathray.Duration - (int)parent.ai[1];

                Vector2 deathRayConvergePos = new Vector2(parent.localAI[1], parent.localAI[2]);

                Vector2 deathrayVector = deathRayConvergePos - npc.Center;
                float deathrayLength = deathrayVector.Length();

                Vector2 deathrayNormalVect = deathrayVector.SafeNormalize(Vector2.UnitY);
                float deathrayRot = deathrayVector.ToRotation();
                Vector2 deathrayStartPos = npc.Center + deathrayNormalVect * (npc.life > 1 ? 18 : 24);

                float verticalScale = 1f;
                if (timeToEnd < 5)
                {
                    verticalScale *= timeToEnd / 5f;
                }
                else if (parent.ai[1] < deathrayWindup)
                {
                    if (parent.ai[1] < deathrayWindup - 5)
                        verticalScale *= MathHelper.Clamp((parent.ai[1] - (deathrayWindup - 30)) / 10f, 0, 1) * 0.2f;
                    else
                        verticalScale *= (parent.ai[1] - (deathrayWindup - 5)) / 5f;
                }

                Main.EntitySpriteDraw(deathrayTex, deathrayStartPos - Main.screenPosition, null, Color.White, deathrayRot, new Vector2(0, deathrayTex.Height * 0.5f), new Vector2(1f, verticalScale), SpriteEffects.None);

                float middleScale = deathrayLength - deathrayTex.Width * 2;
                if (middleScale >= 1)
                    Main.EntitySpriteDraw(deathrayTex, deathrayStartPos + deathrayNormalVect * deathrayTex.Width - Main.screenPosition, new Rectangle(deathrayTex.Width - 1, 0, 1, deathrayTex.Height), Color.White, deathrayRot, new Vector2(0, deathrayTex.Height * 0.5f), new Vector2(middleScale, verticalScale), SpriteEffects.None);

                Main.EntitySpriteDraw(deathrayTex, deathRayConvergePos - Main.screenPosition, null, Color.White, deathrayRot, new Vector2(1, deathrayTex.Height * 0.5f), new Vector2(1f, verticalScale), SpriteEffects.FlipHorizontally);
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            float colorLerp = MathHelper.Clamp(MathHelper.Lerp(1f, 0.35f, -NPC.localAI[0] / 90), 0.35f, 1f);
            Color MoonLordColor = Color.Lerp(Color.LightGray, Color.White, colorLerp);

            bool leftHandAlive = leftHandWho >= 0 && Main.npc[leftHandWho].life > 1;
            bool rightHandAlive = rightHandWho >= 0 && Main.npc[rightHandWho].life > 1;
            bool headAlive = headWho >= 0 && Main.npc[headWho].life > 1;

            Vector2 bodyDrawPos = NPC.Center + new Vector2(0, 43);
            Vector2 shoulderAnchor = new Vector2(220, -98);
            float upperArmLength = upperArmTex.Height * 0.8f;
            float lowerArmLength = lowerArmTex.Height * 1f;
            float upperArmLengthRatio = 1 / ((upperArmLength + lowerArmLength) / upperArmLength);

            Vector2 leftUpperArmOrigin = upperArmTex.Size() * new Vector2(0.4477f, 0.17f);
            Vector2 rightUpperArmOrigin = upperArmTex.Size() * new Vector2(1 - 0.4477f, 0.17f);
            Vector2 lowerArmOrigin = lowerArmTex.Size() * new Vector2(0.5f, 0.9f);

            Vector2 leftShoulderPos = bodyDrawPos + shoulderAnchor * new Vector2(-1, 1);
            Vector2 rightShoulderPos = bodyDrawPos + shoulderAnchor * new Vector2(1, 1);

            Vector2 leftHandBottomPos = leftHandPos + new Vector2(0, 32);
            Vector2 rightHandBottomPos = rightHandPos + new Vector2(0, 32);
            Vector2 leftShoulderHandVect = leftHandBottomPos - leftShoulderPos;
            Vector2 rightShoulderHandVect = rightHandBottomPos - rightShoulderPos;

            float leftUpperArmRot = (float)Math.Asin((leftShoulderHandVect * upperArmLengthRatio).Length() / upperArmLength) + leftShoulderHandVect.ToRotation() - MathHelper.Pi;
            Vector2 leftElbowPos = (leftUpperArmRot + MathHelper.PiOver2).ToRotationVector2() * upperArmLength + leftShoulderPos;
            float rightUpperArmRot = (float)Math.Asin((rightShoulderHandVect * upperArmLengthRatio).Length() / upperArmLength) * -1 + rightShoulderHandVect.ToRotation();
            Vector2 rightElbowPos = (rightUpperArmRot + MathHelper.PiOver2).ToRotationVector2() * upperArmLength + rightShoulderPos;

            float leftLowerArmRot = (leftHandBottomPos - leftElbowPos).ToRotation() + MathHelper.PiOver2;
            float rightLowerArmRot = (rightHandBottomPos - rightElbowPos).ToRotation() + MathHelper.PiOver2;


            List<StoredDraw> draws = [];
            draws.Add(new StoredDraw(upperArmTex, leftShoulderPos, null, MoonLordColor, leftUpperArmRot, leftUpperArmOrigin, NPC.scale, SpriteEffects.None));
            draws.Add(new StoredDraw(upperArmTex, rightShoulderPos, null, MoonLordColor, rightUpperArmRot, rightUpperArmOrigin, NPC.scale, SpriteEffects.FlipHorizontally));

            for (int i = -1; i <= 1; i += 2)
            {
                draws.Add(new StoredDraw(bodyTex, bodyDrawPos, null, MoonLordColor, 0, bodyTex.Size() * new Vector2(i == -1 ? 1 : 0, 0.5f), NPC.scale, i == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally));
            }
            draws.Add(new StoredDraw(coreCrackTex, NPC.Center + new Vector2(2, -11), null, MoonLordColor, 0, coreCrackTex.Size() * 0.5f, NPC.scale, SpriteEffects.None));
            draws.Add(new StoredDraw(coreTex, NPC.Center + new Vector2(-1, 0), coreFrame, MoonLordColor, 0, coreFrame.Size() * 0.5f, NPC.scale, SpriteEffects.None));

            draws.Add(new StoredDraw(lowerArmTex, leftElbowPos, null, MoonLordColor, leftLowerArmRot, lowerArmOrigin, NPC.scale, SpriteEffects.None));
            draws.Add(new StoredDraw(lowerArmTex, rightElbowPos, null, MoonLordColor, rightLowerArmRot, lowerArmOrigin, NPC.scale, SpriteEffects.FlipHorizontally));

            draws.Add(new StoredDraw(emptyEyeTex, leftHandPos + new Vector2(0, -2), emptyEyeFrame, MoonLordColor, 0, emptyEyeFrame.Size() * 0.5f, NPC.scale, SpriteEffects.None));
            if (leftHandAlive)
            {
                draws.Add(new StoredDraw(sideEyeTex, leftHandPos, null, MoonLordColor, 0, sideEyeTex.Size() * 0.5f, NPC.scale, SpriteEffects.None));
                draws.Add(new StoredDraw(innerEyeTex, leftHandPos + leftEyeVector, null, MoonLordColor, 0, innerEyeTex.Size() * 0.5f, NPC.scale, SpriteEffects.None));
            }
            draws.Add(new StoredDraw(handTex, leftHandPos + new Vector2(2, -49), leftHandFrame, MoonLordColor, 0, leftHandFrame.Size() * 0.5f, NPC.scale, SpriteEffects.None));

            draws.Add(new StoredDraw(emptyEyeTex, rightHandPos + new Vector2(0, -2), emptyEyeFrame, MoonLordColor, 0, emptyEyeFrame.Size() * 0.5f, NPC.scale, SpriteEffects.FlipHorizontally));
            if (rightHandAlive)
            {
                draws.Add(new StoredDraw(sideEyeTex, rightHandPos, null, MoonLordColor, 0, sideEyeTex.Size() * 0.5f, NPC.scale, SpriteEffects.FlipHorizontally));
                draws.Add(new StoredDraw(innerEyeTex, rightHandPos + rightEyeVector, null, MoonLordColor, 0, innerEyeTex.Size() * 0.5f, NPC.scale, SpriteEffects.None));
            }
            draws.Add(new StoredDraw(handTex, rightHandPos + new Vector2(-2, -49), rightHandFrame, MoonLordColor, 0, rightHandFrame.Size() * 0.5f, NPC.scale, SpriteEffects.FlipHorizontally));

            draws.Add(new StoredDraw(headTex, headPos + new Vector2(0, 4), headFrame, MoonLordColor, 0, headFrame.Size() * new Vector2(0.5f, 0.25f), NPC.scale, SpriteEffects.None));
            draws.Add(new StoredDraw(mouthTex, headPos + new Vector2(1, 212), mouthFrame, MoonLordColor, 0, mouthFrame.Size() * 0.5f, NPC.scale, SpriteEffects.None));

            draws.Add(new StoredDraw(emptyEyeTex, headPos, emptyEyeFrame, MoonLordColor, 0, emptyEyeFrame.Size() * 0.5f, NPC.scale, SpriteEffects.None));
            if (headWho >= 0)
            {
                draws.Add(new StoredDraw(topEyeTex, headPos, null, MoonLordColor, 0, topEyeTex.Size() * 0.5f, NPC.scale, SpriteEffects.None));
                draws.Add(new StoredDraw(innerEyeTex, headPos + headEyeVector, null, MoonLordColor, 0, innerEyeTex.Size() * 0.5f, NPC.scale, SpriteEffects.None));
                draws.Add(new StoredDraw(topEyeOverlayTex, headPos + new Vector2(0, 4), headEyeFrame, MoonLordColor, 0, headEyeFrame.Size() * 0.5f, NPC.scale, SpriteEffects.None));
            }

            Vector2 drawOff = -Main.screenPosition;

            if (modNPC.ignitedStacks.Count > 0 || (leftHandAlive && Main.npc[leftHandWho].ModNPC().ignitedStacks.Count > 0) || (rightHandAlive && Main.npc[rightHandWho].ModNPC().ignitedStacks.Count > 0) || (headAlive && Main.npc[headWho].ModNPC().ignitedStacks.Count > 0))
            {
                StartAlphaBlendSpritebatch();

                Color color = Color.Lerp(Color.Yellow, Color.OrangeRed, Main.rand.NextFloat(0.4f, 0.6f + float.Epsilon) + 0.2f + (0.2f * (float)Math.Cos((Main.GlobalTimeWrappedHourly * 20f)))) * 0.8f;
                Vector3 colorHSL = Main.rgbToHsl(color);
                GameShaders.Misc["TerRoguelike:BasicTint"].UseOpacity(1f);
                GameShaders.Misc["TerRoguelike:BasicTint"].UseColor(Main.hslToRgb(1 - colorHSL.X, colorHSL.Y, colorHSL.Z));
                GameShaders.Misc["TerRoguelike:BasicTint"].Apply();

                for (int i = 0; i < draws.Count; i++)
                {
                    var draw = draws[i];
                    if (draw.texture.Width < 100) // every small texture here is covered up by a bigger texture. no point in wasting time drawing ignite textures for things that would have no effect
                        continue;
                    for (int j = 0; j < 8; j++)
                    {
                        draw.Draw(drawOff + Vector2.UnitX.RotatedBy(j * MathHelper.PiOver4 + draw.rotation) * 2);
                    }
                }

                StartVanillaSpritebatch();
            }

            if (deadTime >= deathBlackWhiteStartTime)
            {
                float brainRaiseInterpolant = MathHelper.Clamp((deadTime - deathBlackWhiteStartTime) / 140f, 0, 1);
                brainRaiseInterpolant = 1 + -(float)Math.Pow(brainRaiseInterpolant - 1, 4);
                float brainMoveRightInterpolant = MathHelper.Clamp((deadTime - (deathBlackWhiteStopTime + 30)) / 60f, 0, 1);
                brainMoveRightInterpolant = 1 + -(float)Math.Pow(brainMoveRightInterpolant - 1, 2);
                float brainMoveLeftInterpolant = 0;
                if (deadTime - deathBlackWhiteStopTime - 60 >= 0)
                {
                    brainMoveLeftInterpolant = Math.Max((float)Math.Pow((deadTime - deathBlackWhiteStopTime - 60) / 180f, 2f), 0);
                }


                Vector2 trueBrainPos = headPos + new Vector2(0, 0) + new Vector2(0, -280) * brainRaiseInterpolant;
                Vector2 brainScale = Vector2.One;
                brainScale.X *= MathHelper.Clamp(MathHelper.Lerp(0.9f, 1f, brainRaiseInterpolant * 1.3f), 0.9f, 1f);
                trueBrainPos += brainMoveRightInterpolant * new Vector2(360, 0);
                trueBrainPos += brainMoveLeftInterpolant * new Vector2(-20000, 0);

                Vector2 wantedEyeVector = Main.LocalPlayer == null ? Vector2.Zero : Main.LocalPlayer.Center - trueBrainPos;
                wantedEyeVector = wantedEyeVector.SafeNormalize(Vector2.UnitY);
                wantedEyeVector = Vector2.Lerp(wantedEyeVector, -Vector2.UnitX, brainMoveRightInterpolant);
                wantedEyeVector *= 16;

                var trueBrainDrawList = TerRoguelikeWorld.GetTrueBrainDrawList(trueBrainPos, wantedEyeVector, brainScale, Color.White, (int)(Main.GlobalTimeWrappedHourly * 8));
                if (!NPC.behindTiles)
                {
                    postDrawAllBlack = true;
                    for (int i = 0; i < trueBrainDrawList.Count; i++)
                    {
                        var draw = trueBrainDrawList[i];
                        draws.Add(draw);
                    }
                    for (int i = 0; i < draws.Count; i++)
                    {
                        postDrawEverythingCache.Add(draws[i]);
                    }
                }
                else
                {
                    draws.Clear();
                    for (int i = 0; i < trueBrainDrawList.Count; i++)
                    {
                        postDrawEverythingCache.Add(trueBrainDrawList[i]);
                    }
                }
            }

            if (NPC.behindTiles)
            {
                for (int i = 0; i < draws.Count; i++)
                {
                    draws[i].Draw(drawOff);
                }
            }

            if (NPC.localAI[0] < 0)
            {
                DrawForcefield();
            }
            return false;
        }
        public void DrawForcefield()
        {
            Vector2 drawpos = NPC.Center - Main.screenPosition;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin((SpriteSortMode)1, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.Transform);
            float shieldExpandIntensity = (float)Math.Cos(Main.GlobalTimeWrappedHourly * 0.7f) * 0.15f + 0.15f;
            float shieldImpulseTime = NPC.localAI[1];
            float shieldStrength = ((float)Math.Cos(Main.GlobalTimeWrappedHourly * 0.7f) * 0.25f + 0.5f) * TerRoguelikeWorld.chainList.Count * 0.25f;
            Color shieldColor = Color.Lerp(Color.White, Color.Teal, 0.5f);
            shieldColor.A = 180;
            if (shieldImpulseTime > 0f && shieldImpulseTime <= 120f)
            {
                shieldExpandIntensity += 0.8f * (shieldImpulseTime / 120f);
                shieldStrength += 0.2f * (shieldImpulseTime / 120f);
            }
            if (NPC.localAI[0] >= -80)
            {
                int time = (int)NPC.localAI[0] + 80;
                float completion = time / 80f;
                shieldExpandIntensity += 20 * completion;
                shieldStrength -= completion * 0.8f;
                shieldColor = Color.Lerp(shieldColor, Color.White, completion);
            }
            Filters.Scene["Vortex"].GetShader().UseIntensity(1f + shieldExpandIntensity).UseProgress(0f);
            Rectangle frame = new Rectangle(0, 0, 2000, 1300);
            DrawData drawdata = new DrawData(perlinTex, drawpos + new Vector2(0, -72f), frame, shieldColor * (shieldStrength * 0.8f + 0.2f), NPC.rotation, frame.Size() * 0.5f, NPC.scale * (1f + shieldExpandIntensity * 0.05f), SpriteEffects.None, 0f);
            GameShaders.Misc["ForceField"].UseColor(new Vector3(1f + shieldExpandIntensity * 0.5f));
            GameShaders.Misc["ForceField"].Apply(drawdata);
            drawdata.Draw(Main.spriteBatch);
            StartVanillaSpritebatch();
        }
    }
}
