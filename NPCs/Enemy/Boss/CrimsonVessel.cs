using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace TerRoguelike.NPCs.Enemy.Boss
{
    public class CrimsonVessel : BaseRoguelikeNPC
    {
        public Entity target;
        public Vector2 spawnPos;
        public bool ableToHit = true;
        public bool canBeHit = true;
        public override int modNPCID => ModContent.NPCType<CrimsonVessel>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Crimson"] };
        public override int CombatStyle => -1;
        public int currentFrame;

        public int deadTime = 0;
        public int cutsceneDuration = 150;
        public int deathCutsceneDuration = 150;

        public Attack None = new Attack(0, 0, 180);
        public Attack Teleport = new Attack(1, 20, 40);
        public Attack Heal = new Attack(2, 0, 440);
        public Attack BouncyBall = new Attack(3, 20, 180);
        public Attack BloodSpread = new Attack(4, 20, 90);
        public Attack Charge = new Attack(5, 20, 150);
        public Attack BloodTrail = new Attack(6, 20, 150);
        public int teleportTime = 40;
        public int teleportMoveTimestamp = 20;
        List<TrackedSeer> trackedSeers = new List<TrackedSeer>();
        public int healCountdown = 1500;
        public int setHealTime = 1500;
        public int seerSpawnTime = 100;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 8;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 156;
            NPC.height = 112;
            NPC.aiStyle = -1;
            NPC.damage = 35;
            NPC.lifeMax = 25000;
            NPC.HitSound = SoundID.NPCHit9;
            NPC.DeathSound = SoundID.NPCDeath11;
            NPC.knockBackResist = 0f;
            modNPC.drawCenter = new Vector2(0, 24);
            modNPC.IgnoreRoomWallCollision = true;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            modNPC.OverrideIgniteVisual = true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.immortal = true;
            NPC.dontTakeDamage = true;
            currentFrame = 0;
            NPC.localAI[0] = -(cutsceneDuration + 30);
            NPC.direction = -1;
            NPC.spriteDirection = -1;
            spawnPos = NPC.Center;
            NPC.ai[2] = None.Id;
            NPC.ai[1] = Heal.Duration - seerSpawnTime;
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

            ableToHit = NPC.ai[3] == 0;
            canBeHit = true;
            trackedSeers.RemoveAll(x => !Main.npc[x.whoAmI].active);

            NPC.frameCounter += 0.13d;
            if (NPC.localAI[0] < 0)
            {
                spawnPos = NPC.Center;
                if (NPC.localAI[0] == -cutsceneDuration)
                {
                    CutsceneSystem.SetCutscene(NPC.Center, cutsceneDuration, 30, 30, 2.5f);
                }

                NPC.localAI[0]++;
                if (NPC.localAI[0] < -30 && NPC.localAI[0] >= -30 - seerSpawnTime)
                {
                    SpawnSeers();
                    NPC.ai[1]++;
                }
                if (NPC.localAI[0] == -30)
                {
                    NPC.immortal = false;
                    NPC.dontTakeDamage = false;
                    NPC.ai[1] = 0;
                }
            }
            else
            {
                NPC.localAI[0]++;
                BossAI();
            }

            if (trackedSeers.Any())
            {
                for (int i = 0; i < trackedSeers.Count; i++)
                {
                    TrackedSeer seer = trackedSeers[i];
                    NPC npc = Main.npc[seer.whoAmI];
                    npc.ai[3] = NPC.ai[3];
                    CrimsonSeer.UpdateCrimsonSeer(npc, seer.seerId, trackedSeers.Count);
                }
            }
        }
        public void BossAI()
        {
            target = modNPC.GetTarget(NPC, false, false);

            NPC.ai[1]++;
            if (NPC.localAI[0] == 1)
            {
                SpawnSeers();
            }

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
                    
                }
            }

            if (NPC.ai[0] == Teleport.Id)
            {
                TeleportAI();
                if (NPC.ai[1] >= Teleport.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = None.Duration - 30;
                    NPC.ai[2] = Teleport.Id;
                    NPC.ai[3] = 0;
                }
            }
            else if (NPC.ai[0] == Heal.Id)
            {
                if (NPC.ai[1] <= teleportTime)
                    TeleportAI(spawnPos);
                if (NPC.ai[1] >= teleportMoveTimestamp)
                    NPC.velocity = Vector2.Zero;
                if (NPC.ai[1] == teleportTime + 1)
                {
                    if (!trackedSeers.Any())
                        NPC.ai[1] = Heal.Duration - seerSpawnTime;
                }
                if (NPC.ai[1] > teleportTime && NPC.ai[1] < Heal.Duration - 100)
                {

                }
                else if (NPC.ai[1] >= Heal.Duration - seerSpawnTime)
                {
                    SpawnSeers();
                }

                if (NPC.ai[1] >= Heal.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Heal.Id;
                    NPC.ai[3] = 0;
                }
            }
            else if (NPC.ai[0] == BouncyBall.Id)
            {
                if (NPC.ai[1] <= teleportTime)
                    TeleportAI();

                if (NPC.ai[1] >= BouncyBall.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = BouncyBall.Id;
                    NPC.ai[3] = 0;
                }
            }
            else if (NPC.ai[0] == BloodSpread.Id)
            {
                if (NPC.ai[1] <= teleportTime)
                    TeleportAI();

                if (NPC.ai[1] >= BloodSpread.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = BloodSpread.Id;
                    NPC.ai[3] = 0;
                }
            }
            else if (NPC.ai[0] == Charge.Id)
            {
                if (NPC.ai[1] <= teleportTime)
                    TeleportAI();

                if (NPC.ai[1] >= Charge.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Charge.Id;
                    NPC.ai[3] = 0;
                }
            }
            else if (NPC.ai[0] == BloodTrail.Id)
            {
                if (NPC.ai[1] <= teleportTime)
                    TeleportAI();

                if (NPC.ai[1] >= BloodTrail.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = BloodTrail.Id;
                    NPC.ai[3] = 0;
                }
            }

            bool defaultMovement = (NPC.ai[0] == 0 || NPC.ai[1] <= teleportTime) && !(NPC.ai[0] == Heal.Id && NPC.ai[1] >= teleportMoveTimestamp);
            if (defaultMovement)
            {
                if (target != null)
                {
                    if ((target.Center - NPC.Center).Length() > 64)
                        NPC.velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * 0.34f;
                    else
                        NPC.velocity *= 0.98f;
                }
                else
                    NPC.velocity *= 0.98f;
            }
        }
        public void TeleportAI(Vector2? forcedLocation = null)
        {
            NPC.ai[3]++;

            if (NPC.ai[3] == teleportMoveTimestamp)
            {
                Vector2 teleportLocation = -Vector2.One;
                if (target != null && forcedLocation == null)
                {
                    for (int i = 0; i < 100; i++)
                    {
                        Vector2 randPos = target.Center + Main.rand.NextVector2CircularEdge(240, 240);
                        Point randTilePos = randPos.ToTileCoordinates();

                        if (!ParanoidTileRetrieval(randTilePos.X, randTilePos.Y).IsTileSolidGround(true))
                        {
                            if (modNPC.isRoomNPC)
                            {
                                if (!RoomList[modNPC.sourceRoomListID].GetRect().Contains(randPos.ToPoint()))
                                    continue;
                            }
                            if ((randPos - NPC.Center).Length() < 120f)
                                continue;

                            teleportLocation = randPos;
                            break;
                        }
                    }
                }

                if (forcedLocation != null)
                    teleportLocation = (Vector2)forcedLocation;
                else if (teleportLocation == -Vector2.One)
                    teleportLocation = spawnPos + Main.rand.NextVector2CircularEdge(240, 240);

                NPC.Center = teleportLocation;
            }

            if (NPC.ai[3] == teleportTime)
            {
                NPC.ai[3] = 0;
            }
        }
        public void SpawnSeers()
        {
            int seerCount = 8;
            int seerRate = seerSpawnTime / (seerCount - 1);
            if ((NPC.ai[1] - (Heal.Duration - seerSpawnTime)) % seerRate == 0)
            {
                int currentSeer = (int)((NPC.ai[1] - (Heal.Duration - seerSpawnTime)) / seerRate);
                float rotation = ((float)currentSeer / seerCount * MathHelper.TwoPi) - MathHelper.PiOver2;
                Vector2 pos = NPC.Center + (rotation.ToRotationVector2() * 50);
                int whoAmI = NPC.NewNPC(NPC.GetSource_FromThis(), (int)pos.X, (int)pos.Y, ModContent.NPCType<CrimsonSeer>());
                NPC seer = Main.npc[whoAmI];
                seer.rotation = rotation;
                seer.ai[3] = NPC.ai[3];
                seer.localAI[2] = -(Heal.Duration - NPC.ai[1]);
                seer.ModNPC().isRoomNPC = modNPC.isRoomNPC;
                seer.ModNPC().sourceRoomListID = modNPC.sourceRoomListID;
                trackedSeers.Add(new TrackedSeer(currentSeer, whoAmI));
            }
        }
        public void ChooseAttack()
        {
            NPC.ai[1] = 0;
            NPC.ai[3] = 0;
            int chosenAttack = 0;

            if (healCountdown <= 0)
            {
                chosenAttack = Heal.Id;
                healCountdown = setHealTime;
            }
            else
            {
                List<Attack> potentialAttacks = new List<Attack>() { Teleport, BouncyBall, BloodSpread, Charge, BloodTrail };
                potentialAttacks.RemoveAll(x => x.Id == (int)NPC.ai[2]);
                if (!trackedSeers.Any())
                    potentialAttacks.RemoveAll(x => x.Id == BouncyBall.Id || x.Id == BloodSpread.Id);

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
                chosenAttack = Teleport.Id;
            }
            
            NPC.ai[0] = chosenAttack;
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
        public override bool? CanBeHitByProjectile(Projectile projectile) => canBeHit ? null : false;
        public override bool? CanBeHitByItem(Player player, Item item) => canBeHit ? null : false;

        public override bool CheckDead()
        {
            if (deadTime >= deathCutsceneDuration - 30)
            {
                return true;
            }
            NPC.ai[0] = None.Id;
            NPC.ai[1] = 1;
            NPC.ai[3] = 0;

            modNPC.OverrideIgniteVisual = true;
            NPC.velocity *= 0;
            NPC.life = 1;
            NPC.immortal = true;
            NPC.dontTakeDamage = true;
            NPC.active = true;

            if (deadTime == 0)
            {
                CutsceneSystem.SetCutscene(NPC.Center, deathCutsceneDuration, 30, 30, 2.5f);
                if (modNPC.isRoomNPC)
                {
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
                for (int i = 0; (double)i < hit.Damage / 25.0; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 5, hit.HitDirection, -1f);
                }
            }
        }
        public override void OnKill()
        {
            for (int i = 0; i < 150; i++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, 5, Main.rand.Next(-2, 2), -2f);
            }
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, new Vector2((float)Main.rand.Next(-30, 31) * 0.2f, (float)Main.rand.Next(-30, 31) * 0.2f), 396);
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, new Vector2((float)Main.rand.Next(-30, 31) * 0.2f, (float)Main.rand.Next(-30, 31) * 0.2f), 397);
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, new Vector2((float)Main.rand.Next(-30, 31) * 0.2f, (float)Main.rand.Next(-30, 31) * 0.2f), 398);
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, new Vector2((float)Main.rand.Next(-30, 31) * 0.2f, (float)Main.rand.Next(-30, 31) * 0.2f), 399);
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, new Vector2((float)Main.rand.Next(-30, 31) * 0.2f, (float)Main.rand.Next(-30, 31) * 0.2f), 400);
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, new Vector2((float)Main.rand.Next(-30, 31) * 0.2f, (float)Main.rand.Next(-30, 31) * 0.2f), 401);
        }
        public override void FindFrame(int frameHeight)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;

            int frameCount = Main.npcFrameCount[Type];
            
            currentFrame = (int)NPC.frameCounter % (frameCount - 4);

            NPC.frame = new Rectangle(0, currentFrame * frameHeight, tex.Width, frameHeight);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;
            Vector2 drawPos = NPC.Center + modNPC.drawCenter;
            Color color = Color.Lerp(Color.White, Lighting.GetColor(drawPos.ToTileCoordinates()), 0.6f);
            Vector2 scale = new Vector2(NPC.scale);
            if (NPC.ai[3] > 0)
            {
                float interpolant = NPC.ai[3] < teleportMoveTimestamp ? NPC.ai[3] / (teleportMoveTimestamp) : 1f - ((NPC.ai[3] - teleportMoveTimestamp) / (teleportTime - teleportMoveTimestamp));
                float horizInterpolant = MathHelper.Lerp(1f, 2f, 0.5f + (0.5f * -(float)Math.Cos(interpolant * MathHelper.TwoPi)));
                float verticInterpolant = MathHelper.Lerp(0.5f + (0.5f * (float)Math.Cos(interpolant * MathHelper.TwoPi)), 8f, interpolant * interpolant);
                scale.X *= horizInterpolant;
                scale.Y *= verticInterpolant;

                scale *= 1f - interpolant;
            }
            if (modNPC.ignitedStacks.Any())
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                Color fireColor = Color.Lerp(Color.Yellow, Color.OrangeRed, Main.rand.NextFloat(0.4f, 0.6f + float.Epsilon) + 0.2f + (0.2f * (float)Math.Cos((Main.GlobalTimeWrappedHourly * 20f)))) * 0.8f;
                Vector3 colorHSL = Main.rgbToHsl(fireColor);
                float outlineThickness = 1f;

                GameShaders.Misc["TerRoguelike:BasicTint"].UseOpacity(1f);
                GameShaders.Misc["TerRoguelike:BasicTint"].UseColor(Main.hslToRgb(1 - colorHSL.X, colorHSL.Y, colorHSL.Z));
                GameShaders.Misc["TerRoguelike:BasicTint"].Apply();

                for (float j = 0; j < 1; j += 0.125f)
                {
                    spriteBatch.Draw(tex, drawPos - Main.screenPosition + ((j * MathHelper.TwoPi + NPC.rotation).ToRotationVector2() * outlineThickness), NPC.frame, color, NPC.rotation, NPC.frame.Size() * 0.5f, scale, NPC.spriteDirection > 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
                }
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            Main.EntitySpriteDraw(tex, drawPos - Main.screenPosition, NPC.frame, color, NPC.rotation, NPC.frame.Size() * 0.5f, scale, NPC.spriteDirection > 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            
            return false;
        }
    }
    public class TrackedSeer
    {
        public int seerId;
        public int whoAmI;
        public TrackedSeer(int SeerId, int WhoAmI)
        {
            seerId = SeerId;
            whoAmI = WhoAmI;
        }
    }
}
