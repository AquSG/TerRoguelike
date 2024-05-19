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
using System.Collections;

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
        public override int CombatStyle => -1;
        public bool CollisionPass = false;
        public bool goreProc = false;
        public int coreCurrentFrame = 0;
        public int mouthCurrentFrame = 0;
        public int headEyeCurrentFrame = 0;
        public int leftHandCurrentFrame = 0;
        public int rightHandCurrentFrame = 0;
        public int emptyEyeCurrentFrame = 0;
        public Rectangle coreFrame;
        public Rectangle mouthFrame;
        public Rectangle headEyeFrame;
        public Rectangle leftHandFrame;
        public Rectangle rightHandFrame;
        public Rectangle emptyEyeFrame;
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


        public Texture2D coreTex, coreCrackTex, emptyEyeTex, innerEyeTex, lowerArmTex, upperArmTex, mouthTex, sideEyeTex, topEyeTex, topEyeOverlayTex, headTex, handTex, bodyTex;
        public int leftHandWho = -1; // yes, technically moon lord's "Left" is not the same as the left for the viewer, and vice versa for right hand. I do not care. internally it will be based on viewer perspective.
        public int rightHandWho = -1;
        public int headWho = -1;

        public int deadTime = 0;
        public int cutsceneDuration = 120;
        public int deathCutsceneDuration = 120;

        public static Attack None = new Attack(0, 0, 130);
        public static Attack PhantSpin = new Attack(1, 30, 260);
        public static Attack PhantBolt = new Attack(2, 30, 360);
        public static Attack PhantSphere = new Attack(3, 30, 179);
        public static Attack TentacleCharge = new Attack(4, 30, 600);
        public static Attack Deathray = new Attack(5, 30, 180);
        public static Attack PhantSpawn = new Attack(6, 30, 180);
        public static int phantSpinWindup = 50;
        public static int phantBoltWindup = 30;
        public static int phantBoltFireRate = 8;
        public static int phantBoltFiringDuration = 90;
        public static int phantSphereFireRate = 20;
        public static int tentacleWindup = 90;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 60;
            NPC.height = 88;
            NPC.aiStyle = -1;
            NPC.damage = 36;
            NPC.lifeMax = 25000;
            NPC.HitSound = SoundID.NPCHit1;
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
            if (eyeCenter)
                rate = 0.075f;
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
                    headOverlayFrameCounter++;
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

            void InnerEyePositionUpdate(ref Vector2 eyeVector, Vector2 basePosition)
            {
                if (eyeCenter)
                {
                    eyeVector = Vector2.Lerp(eyeVector, Vector2.Zero, rate);
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

                if (NPC.localAI[0] == -30)
                {
                    NPC.immortal = false;
                    NPC.dontTakeDamage = false;
                    NPC.ai[1] = 0;
                    enemyHealthBar = new EnemyHealthBar([NPC.whoAmI, headWho, leftHandWho, rightHandWho], NPC.FullName);
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
                        SoundEngine.PlaySound(SoundID.Zombie98 with { Volume = 0.7f, Pitch = -0.2f }, NPC.Center + new Vector2(0, -80));
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
                        SoundEngine.PlaySound(SoundID.DD2_BetsyDeath with { Volume = 1f, Pitch = -1f, Variants = [0] }, NPC.Center + new Vector2(0, -80));
                        for (int i = -3; i <= 3; i += 2)
                        {
                            float shootRot = MathHelper.PiOver2 + MathHelper.PiOver4 * 0.5f * i;
                            Vector2 shootRotVect = shootRot.ToRotationVector2();
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), projSpawnPos + shootRotVect * 3, shootRotVect * 7.5f, ModContent.ProjectileType<Tentacle>(), NPC.damage, 0);
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
                if (NPC.ai[1] >= Deathray.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Deathray.Id;
                }
            }
            else if (NPC.ai[0] == PhantSpawn.Id)
            {
                if (NPC.ai[1] >= PhantSpawn.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = PhantSpawn.Id;
                }
            }
        }
        public void ChooseAttack()
        {
            NPC.ai[1] = 0;
            int chosenAttack = 0;

            //List<Attack> potentialAttacks = new List<Attack>() { PhantSpin, PhantBolt, PhantSphere, Tentacle, Deathray, PhantSpawn };
            List<Attack> potentialAttacks = new List<Attack>() { PhantSpin, PhantBolt, PhantSphere, TentacleCharge };
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

            chosenAttack = TentacleCharge.Id;
            NPC.ai[0] = chosenAttack;
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
            ableToHit = false;
            canBeHit = false;

            if (deadTime == 0)
            {
                enemyHealthBar.ForceEnd(0);
                NPC.velocity = Vector2.Zero;
                NPC.rotation = 0;
                modNPC.ignitedStacks.Clear();

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
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage * 0.01d; i++)
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
            CollisionPass = false;
            return false;
        }
        public override bool CanHitNPC(NPC target)
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
            CollisionPass = false;
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
            bool leftHandAlive = leftHandWho >= 0 && Main.npc[leftHandWho].life > 1;
            bool rightHandAlive = rightHandWho >= 0 && Main.npc[rightHandWho].life > 1;
            bool headAlive = headWho >= 0 && Main.npc[headWho].life > 1;

            int headForceFrame = -1;
            int leftHandForceFrame = -1;
            int rightHandForceFrame = -1;
            int mouthForceFrame = 0;

            int sharedFrame = 0;
            if (NPC.ai[0] == None.Id)
            {
                sharedFrame = (int)(NPC.ai[1] * 0.125f);
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
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
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
            draws.Add(new StoredDraw(upperArmTex, leftShoulderPos, null, Color.White, leftUpperArmRot, leftUpperArmOrigin, NPC.scale, SpriteEffects.None));
            draws.Add(new StoredDraw(upperArmTex, rightShoulderPos, null, Color.White, rightUpperArmRot, rightUpperArmOrigin, NPC.scale, SpriteEffects.FlipHorizontally));

            for (int i = -1; i <= 1; i += 2)
            {
                draws.Add(new StoredDraw(bodyTex, bodyDrawPos, null, Color.White, 0, bodyTex.Size() * new Vector2(i == -1 ? 1 : 0, 0.5f), NPC.scale, i == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally));
            }
            draws.Add(new StoredDraw(coreCrackTex, NPC.Center + new Vector2(2, -11), null, Color.White, 0, coreCrackTex.Size() * 0.5f, NPC.scale, SpriteEffects.None));
            draws.Add(new StoredDraw(coreTex, NPC.Center + new Vector2(-1, 0), coreFrame, Color.White, 0, coreFrame.Size() * 0.5f, NPC.scale, SpriteEffects.None));

            draws.Add(new StoredDraw(lowerArmTex, leftElbowPos, null, Color.White, leftLowerArmRot, lowerArmOrigin, NPC.scale, SpriteEffects.None));
            draws.Add(new StoredDraw(lowerArmTex, rightElbowPos, null, Color.White, rightLowerArmRot, lowerArmOrigin, NPC.scale, SpriteEffects.FlipHorizontally));

            draws.Add(new StoredDraw(emptyEyeTex, leftHandPos + new Vector2(0, -2), emptyEyeFrame, Color.White, 0, emptyEyeFrame.Size() * 0.5f, NPC.scale, SpriteEffects.None));
            if (leftHandAlive)
            {
                draws.Add(new StoredDraw(sideEyeTex, leftHandPos, null, Color.White, 0, sideEyeTex.Size() * 0.5f, NPC.scale, SpriteEffects.None));
                draws.Add(new StoredDraw(innerEyeTex, leftHandPos + leftEyeVector, null, Color.White, 0, innerEyeTex.Size() * 0.5f, NPC.scale, SpriteEffects.None));
            }
            draws.Add(new StoredDraw(handTex, leftHandPos + new Vector2(2, -49), leftHandFrame, Color.White, 0, leftHandFrame.Size() * 0.5f, NPC.scale, SpriteEffects.None));

            draws.Add(new StoredDraw(emptyEyeTex, rightHandPos + new Vector2(0, -2), emptyEyeFrame, Color.White, 0, emptyEyeFrame.Size() * 0.5f, NPC.scale, SpriteEffects.FlipHorizontally));
            if (rightHandAlive)
            {
                draws.Add(new StoredDraw(sideEyeTex, rightHandPos, null, Color.White, 0, sideEyeTex.Size() * 0.5f, NPC.scale, SpriteEffects.FlipHorizontally));
                draws.Add(new StoredDraw(innerEyeTex, rightHandPos + rightEyeVector, null, Color.White, 0, innerEyeTex.Size() * 0.5f, NPC.scale, SpriteEffects.None));
            }
            draws.Add(new StoredDraw(handTex, rightHandPos + new Vector2(-2, -49), rightHandFrame, Color.White, 0, rightHandFrame.Size() * 0.5f, NPC.scale, SpriteEffects.FlipHorizontally));

            draws.Add(new StoredDraw(headTex, headPos + new Vector2(0, 4), null, Color.White, 0, headTex.Size() * new Vector2(0.5f, 0.25f), NPC.scale, SpriteEffects.None));
            draws.Add(new StoredDraw(mouthTex, headPos + new Vector2(1, 212), mouthFrame, Color.White, 0, mouthFrame.Size() * 0.5f, NPC.scale, SpriteEffects.None));

            draws.Add(new StoredDraw(emptyEyeTex, headPos, emptyEyeFrame, Color.White, 0, emptyEyeFrame.Size() * 0.5f, NPC.scale, SpriteEffects.None));
            if (headWho >= 0)
            {
                draws.Add(new StoredDraw(topEyeTex, headPos, null, Color.White, 0, topEyeTex.Size() * 0.5f, NPC.scale, SpriteEffects.None));
                draws.Add(new StoredDraw(innerEyeTex, headPos + headEyeVector, null, Color.White, 0, innerEyeTex.Size() * 0.5f, NPC.scale, SpriteEffects.None));
                draws.Add(new StoredDraw(topEyeOverlayTex, headPos + new Vector2(0, 4), headEyeFrame, Color.White, 0, headEyeFrame.Size() * 0.5f, NPC.scale, SpriteEffects.None));
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

            for (int i = 0; i < draws.Count; i++)
            {
                draws[i].Draw(drawOff);
            }
            return false;
        }
    }
}
