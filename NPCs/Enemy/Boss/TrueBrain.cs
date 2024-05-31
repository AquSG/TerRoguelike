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
using static TerRoguelike.Managers.TextureManager;
using static TerRoguelike.Schematics.SchematicManager;
using static TerRoguelike.Systems.MusicSystem;
using static TerRoguelike.Systems.RoomSystem;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using static TerRoguelike.Systems.EnemyHealthBarSystem;
using Terraria.GameContent.Shaders;
using Terraria.Graphics.Effects;

namespace TerRoguelike.NPCs.Enemy.Boss
{
    public class TrueBrain : BaseRoguelikeNPC
    {
        public Entity target;
        public Vector2 spawnPos;
        public bool ableToHit = true;
        public bool canBeHit = true;
        public override int modNPCID => ModContent.NPCType<TrueBrain>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Lunar"] };
        public override int CombatStyle => -1;
        public int currentFrame = 0;
        public SlotId TeleportSlot;
        public Texture2D eyeTex, innerEyeTex;
        public Vector2 eyeVector = Vector2.Zero;
        public Vector2 eyePosition { get { return new Vector2(0, -18) + modNPC.drawCenter; } }
        public Vector2 innerEyePosition { get { return new Vector2(0, -20) + modNPC.drawCenter; } }

        public int deadTime = 0;
        public int cutsceneDuration = 120;
        public int deathCutsceneDuration = 120;

        public static int teleportTime = 40;
        public static int teleportMoveTimestamp = 20;
        public Vector2 teleportTargetPos = new Vector2(-1);
        public List<Vector2> phantomPositions = [];

        public static Attack None = new Attack(0, 0, 90);
        public static Attack TeleportBolt = new Attack(1, 30, 340);
        public static Attack ProjCharge = new Attack(2, 30, 180);
        public static Attack FakeCharge = new Attack(3, 30, 180);
        public static Attack CrossBeam = new Attack(4, 30, 180);
        public static Attack SpinBeam = new Attack(5, 30, 180);
        public static Attack Teleport = new Attack(6, 30, 100);
        public static Attack Summon = new Attack(7, 18, 180);
        public int TeleportBoltCycleTime = 90;
        public int TeleportBoltTelegraph = 20;
        public int TeleportBoltFireRate = 8;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 5;
            NPCID.Sets.MustAlwaysDraw[Type] = true;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 240;
            NPC.height = 150;
            NPC.aiStyle = -1;
            NPC.damage = 36;
            NPC.lifeMax = 60000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.knockBackResist = 0f;
            modNPC.IgnoreRoomWallCollision = true;
            modNPC.OverrideIgniteVisual = true;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            modNPC.AdaptiveArmorEnabled = true;
            modNPC.AdaptiveArmorAddRate = 50;
            innerEyeTex = TexDict["MoonLordInnerEye"];
            eyeTex = TexDict["TrueBrainEye"];
            modNPC.drawCenter = new Vector2(0, 32);
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
        }
        public override void PostAI()
        {
            if (NPC.localAI[0] >= -(cutsceneDuration + 30))
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
                    modPlayer.moonLordSkyEffect = true;
                }
            }

            if (NPC.ai[3] > 0)
            {
                if (NPC.ai[3] == 5)
                {
                    TeleportSlot = SoundEngine.PlaySound(CrimsonVessel.TeleportSound with { Volume = 0.4f, MaxInstances = 2 }, NPC.Center);
                }
                else if (NPC.ai[3] == teleportMoveTimestamp)
                {
                    if (teleportTargetPos == new Vector2(-1))
                    {
                        Vector2 targetPos = target != null ? target.Center : spawnPos;
                        bool found = false;
                        for (int i = 0; i < 100; i++)
                        {
                            Vector2 wantedPos = targetPos + Main.rand.NextVector2CircularEdge(400, 340);
                            if (ParanoidTileRetrieval(wantedPos.ToTileCoordinates()).IsTileSolidGround(true))
                                continue;

                            if (modNPC.isRoomNPC)
                            {
                                if (!RoomList[modNPC.sourceRoomListID].GetRect().Contains(wantedPos.ToPoint()))
                                    continue;
                            }
                            if ((wantedPos - NPC.Center).Length() < 150f)
                                continue;

                            teleportTargetPos = wantedPos;
                            found = true;
                            break;
                        }
                        if (!found)
                        {
                            teleportTargetPos = targetPos + Main.rand.NextVector2CircularEdge(400, 340);
                        }
                    }

                    NPC.Center = teleportTargetPos;
                    if (SoundEngine.TryGetActiveSound(TeleportSlot, out var teleportSound) && teleportSound.IsPlaying)
                    {
                        teleportSound.Position = NPC.Center;
                        teleportSound.Update();
                    }
                }
                else if (NPC.ai[3] >= teleportTime)
                {
                    NPC.ai[3] = 0;
                    teleportTargetPos = new Vector2(-1);
                }
            }

            bool eyeCenter = false;
            float rate = 0.15f;
            if (eyeCenter)
            {
                eyeVector = Vector2.Lerp(eyeVector, Vector2.Zero, rate);
            }
            else
            {
                Vector2 targetPos = target != null ? target.Center : spawnPos;
                float maxEyeOffset = 12;

                Vector2 targetVect = targetPos - (NPC.Center + innerEyePosition.RotatedBy(NPC.rotation));
                if (targetVect.Length() > maxEyeOffset)
                    targetVect = targetVect.SafeNormalize(Vector2.UnitY) * maxEyeOffset;
                eyeVector = Vector2.Lerp(eyeVector, targetVect, rate);
            }
        }
        public override void AI()
        {
            if (NPC.ai[3] != 0)
            {
                NPC.ai[3]++;
            }

            NPC.rotation = 0f;
            NPC.frameCounter += 0.16d;
            if (deadTime > 0)
            {
                CheckDead();
                return;
            }

            ableToHit = NPC.localAI[0] >= 0 && NPC.ai[3] == 0 && deadTime == 0;

            if (NPC.localAI[0] < 0)
            {
                target = modNPC.GetTarget(NPC);

                if (NPC.localAI[0] == -cutsceneDuration)
                {
                    CutsceneSystem.SetCutscene(NPC.Center + eyePosition, cutsceneDuration, 30, 30, 2.5f);
                }
                NPC.localAI[0]++;


                if (NPC.localAI[0] == -30)
                {
                    NPC.localAI[1] = 0;
                    NPC.immortal = false;
                    NPC.dontTakeDamage = false;
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
            target = modNPC.GetTarget(NPC);
            NPC.ai[1]++;

            if (NPC.ai[0] == None.Id)
            {
                if (NPC.ai[1] >= None.Duration)
                {
                    ChooseAttack();
                }
                else
                {
                    DefaultMovement();
                    if (NPC.ai[1] == None.Duration - teleportMoveTimestamp + 1)
                    {
                        teleportTargetPos = new Vector2(-1);
                        NPC.ai[3] = 1;
                    }
                }
            }

            if (NPC.ai[0] == TeleportBolt.Id)
            {
                Vector2 targetPos = target != null ? target.Center : spawnPos;
                NPC.velocity = Vector2.Zero;
                int time = (int)NPC.ai[1] % TeleportBoltCycleTime;
                int attackStartTime = teleportMoveTimestamp + TeleportBoltTelegraph;
                List<Vector2> projSpawnRefPositions = [NPC.Center + innerEyePosition];
                if (phantomPositions.Count > 0)
                {
                    for (int i = 0; i < phantomPositions.Count; i++)
                    {
                        projSpawnRefPositions.Add(phantomPositions[i] + new Vector2(0, -20));
                    }
                }

                if (time < attackStartTime)
                {
                    if (time == teleportMoveTimestamp)
                    {
                        for (int i = 0; i < projSpawnRefPositions.Count; i++)
                        {
                            SoundEngine.PlaySound(SoundID.Item13 with { Volume = 1f / (projSpawnRefPositions.Count * 0.7f), Pitch = 0.2f, PitchVariance = 0, MaxInstances = 4 }, projSpawnRefPositions[i]);
                        }
                    }
                    if (time >= teleportMoveTimestamp && time % 3 == 0)
                    {
                        float maxLength = 12;
                        float range = MathHelper.PiOver4 * 1.5f;
                        for (int i = 0; i < projSpawnRefPositions.Count; i++)
                        {
                            Vector2 anchorPos = projSpawnRefPositions[i];
                            Vector2 vectToTarget = (targetPos - anchorPos);
                            if (vectToTarget.Length() > maxLength)
                                vectToTarget = vectToTarget.SafeNormalize(Vector2.UnitY) * maxLength;
                            Vector2 particleSpawnPos = anchorPos + vectToTarget * new Vector2(0.35f, 1f);

                            Vector2 offset = (Main.rand.NextFloat(-range, range) + vectToTarget.ToRotation()).ToRotationVector2() * Main.rand.NextFloat(32);

                            ParticleManager.AddParticle(new Ball(
                                particleSpawnPos + offset, -offset * 0.1f,
                                20, Color.Lerp(Color.Teal, Color.Cyan, Main.rand.NextFloat(0.25f, 0.75f)), new Vector2(0.25f), 0, 0.96f, 10));
                        }
                    }
                }
                else if (NPC.ai[3] == 0 && (time - attackStartTime) % TeleportBoltFireRate == 0)
                {
                    if ((time - attackStartTime) / TeleportBoltFireRate % 2 == 0)
                    {
                        float pitch = Main.rand.NextFloat(-0.05f, 0.05f);
                        for (int i = 0; i < projSpawnRefPositions.Count; i++)
                        {
                            SoundEngine.PlaySound(SoundID.Item125 with { Volume = 0.7f / (projSpawnRefPositions.Count * 0.66f), Pitch = pitch, PitchVariance = 0, MaxInstances = 8, SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest}, projSpawnRefPositions[i]);
                        }
                    }
                    float maxLength = 12;
                    for (int i = 0; i < projSpawnRefPositions.Count; i++)
                    {
                        Vector2 anchorPos = projSpawnRefPositions[i];
                        Vector2 vectToTarget = (targetPos - anchorPos);
                        if (vectToTarget.Length() > maxLength)
                            vectToTarget = vectToTarget.SafeNormalize(Vector2.UnitY) * maxLength;
                        Vector2 projSpawnPos = anchorPos + vectToTarget * new Vector2(0.35f, 1f);

                        Projectile.NewProjectile(NPC.GetSource_FromThis(), projSpawnPos, vectToTarget.SafeNormalize(Vector2.UnitY) * 15, ModContent.ProjectileType<PhantasmalBolt>(), NPC.damage, 0);
                    }
                }
                else if (time == TeleportBoltCycleTime - 20)
                {
                    teleportTargetPos = new Vector2(-1);
                    NPC.ai[3] = 1;
                }
                else if (time == TeleportBoltCycleTime - 1)
                {
                    phantomPositions.Add(NPC.Center + modNPC.drawCenter);
                }

                if (NPC.ai[1] == TeleportBolt.Duration)
                {
                    teleportTargetPos = new Vector2(-1);
                    NPC.ai[3] = 1;
                }
                if (NPC.ai[1] >= TeleportBolt.Duration + teleportMoveTimestamp - 1)
                {
                    phantomPositions.Clear();
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = TeleportBolt.Id;
                }
            }
            else if (NPC.ai[0] == ProjCharge.Id)
            {
                if (NPC.ai[1] == ProjCharge.Duration)
                {
                    teleportTargetPos = new Vector2(-1);
                    NPC.ai[3] = 1;
                }
                if (NPC.ai[1] >= ProjCharge.Duration + teleportMoveTimestamp - 1)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = ProjCharge.Id;
                }
            }
            else if (NPC.ai[0] == FakeCharge.Id)
            {
                if (NPC.ai[1] == FakeCharge.Duration)
                {
                    teleportTargetPos = new Vector2(-1);
                    NPC.ai[3] = 1;
                }
                if (NPC.ai[1] >= FakeCharge.Duration + teleportMoveTimestamp - 1)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = FakeCharge.Id;
                }
            }
            else if (NPC.ai[0] == CrossBeam.Id)
            {
                if (NPC.ai[1] == CrossBeam.Duration)
                {
                    teleportTargetPos = new Vector2(-1);
                    NPC.ai[3] = 1;
                }
                if (NPC.ai[1] >= CrossBeam.Duration + teleportMoveTimestamp - 1)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = CrossBeam.Id;
                }
            }
            else if (NPC.ai[0] == SpinBeam.Id)
            {
                if (NPC.ai[1] == SpinBeam.Duration)
                {
                    teleportTargetPos = new Vector2(-1);
                    NPC.ai[3] = 1;
                }
                if (NPC.ai[1] >= SpinBeam.Duration + teleportMoveTimestamp - 1)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = SpinBeam.Id;
                }
            }
            else if (NPC.ai[0] == Teleport.Id)
            {
                DefaultMovement();
                if (NPC.ai[3] == 0)
                {
                    teleportTargetPos = new Vector2(-1);
                    NPC.ai[3] = 1;
                }
                if (NPC.ai[1] == Teleport.Duration)
                {
                    teleportTargetPos = new Vector2(-1);
                    NPC.ai[3] = 1;
                }
                if (NPC.ai[1] >= Teleport.Duration + teleportMoveTimestamp - 1)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Teleport.Id;
                }
            }
            else if (NPC.ai[0] == Summon.Id)
            {
                if (NPC.ai[1] == Summon.Duration)
                {
                    teleportTargetPos = new Vector2(-1);
                    NPC.ai[3] = 1;
                }
                if (NPC.ai[1] >= Summon.Duration + teleportMoveTimestamp - 1)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Summon.Id;
                }
            }

            void DefaultMovement()
            {
                Vector2 targetPos = target != null ? target.Center : spawnPos;

                NPC.velocity = (targetPos - NPC.Center).SafeNormalize(Vector2.UnitY) * 1f;
                float distance = targetPos.Distance(NPC.Center);
                if (distance < 120)
                {
                    NPC.velocity *= distance / 120;
                }
                else
                {
                    NPC.velocity *= (distance - 120) * 0.5f / 120f + 1;
                }
            }
        }
        public void ChooseAttack()
        {
            NPC.ai[1] = 0;
            int chosenAttack = 0;

            List<Attack> potentialAttacks = new List<Attack>() { TeleportBolt, ProjCharge, FakeCharge, CrossBeam, SpinBeam, Teleport, Summon };
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
            chosenAttack = TeleportBolt.Id;
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
                ExtraSoundSystem.ForceStopAllExtraSounds();
                enemyHealthBar.ForceEnd(0);
                NPC.velocity = Vector2.Zero;
                NPC.rotation = 0;
                modNPC.ignitedStacks.Clear();
                phantomPositions.Clear();

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
                    int d = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Vortex, hit.HitDirection, -1f, 0, default, 0.9f);
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
            return ableToHit;
        }
        public override bool CanHitNPC(NPC target)
        {
            return ableToHit;
        }
        public override void ModifyHoverBoundingBox(ref Rectangle boundingBox)
        {
            boundingBox = NPC.Hitbox;
        }
        public override void FindFrame(int frameHeight)
        {
            var tex = TextureAssets.Npc[Type].Value;

            currentFrame = (int)NPC.frameCounter % (Main.npcFrameCount[Type] - 1) + 1;
            if (!NPC.active)
                currentFrame = 0;
            NPC.frame = new Rectangle(0, frameHeight * currentFrame, tex.Width, frameHeight - 2);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            var tex = TextureAssets.Npc[Type].Value;
            Color npcColor = Color.White;
            Color phantomColor = Color.Lerp(Color.DarkBlue, Color.Cyan, 0.4f) * 0.65f;
            Vector2 scale = new Vector2(NPC.scale);
            Vector2 drawOff = -Main.screenPosition;

            if (phantomPositions.Count > 0)
            {
                Vector2 targetPos = target != null ? target.Center : spawnPos;
                bool center = false;
                float teleportInterpolant = NPC.ai[3] / teleportTime;
                for (int i = 0; i < phantomPositions.Count; i++)
                {
                    Vector2 pos = phantomPositions[i];
                    var phantDraws = TerRoguelikeWorld.GetTrueBrainDrawList(pos, center ? Vector2.Zero : (targetPos - pos), scale, phantomColor, (int)NPC.frameCounter, teleportInterpolant);
                    for (int d = 0; d < phantDraws.Count; d++)
                    {
                        phantDraws[d].Draw(drawOff);
                    }
                }
            }

            
            if (NPC.ai[3] > 0)
            {
                float interpolant = NPC.ai[3] < teleportMoveTimestamp ? NPC.ai[3] / (teleportMoveTimestamp) : 1f - ((NPC.ai[3] - teleportMoveTimestamp) / (teleportTime - teleportMoveTimestamp));
                float horizInterpolant = MathHelper.Lerp(1f, 2f, 0.5f + (0.5f * -(float)Math.Cos(interpolant * MathHelper.TwoPi)));
                float verticInterpolant = MathHelper.Lerp(0.5f + (0.5f * (float)Math.Cos(interpolant * MathHelper.TwoPi)), 8f, interpolant * interpolant);
                scale.X *= horizInterpolant;
                scale.Y *= verticInterpolant;

                scale *= 1f - interpolant;
            }

            List<StoredDraw> draws = [];

            Vector2 bodyOff = modNPC.drawCenter.RotatedBy(NPC.rotation);
            Vector2 eyeoff = eyePosition.RotatedBy(NPC.rotation);
            Vector2 innerEyeOff = innerEyePosition.RotatedBy(NPC.rotation) + eyeVector * new Vector2(0.35f, 1f);
            draws.Add(new(tex, NPC.Center + bodyOff * scale, NPC.frame, npcColor, NPC.rotation, NPC.frame.Size() * 0.5f, scale, SpriteEffects.None));
            draws.Add(new(eyeTex, NPC.Center + eyeoff * scale, null, npcColor, NPC.rotation, eyeTex.Size() * 0.5f, scale, SpriteEffects.None));
            draws.Add(new(innerEyeTex, NPC.Center + innerEyeOff * scale, null, npcColor, 0, innerEyeTex.Size() * 0.5f, scale, SpriteEffects.None));

            if (modNPC.ignitedStacks.Count > 0)
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
