using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
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
using TerRoguelike.Utilities;
using static TerRoguelike.Managers.TextureManager;
using static TerRoguelike.Schematics.SchematicManager;
using static TerRoguelike.Systems.MusicSystem;
using static TerRoguelike.Systems.RoomSystem;
using static TerRoguelike.Utilities.TerRoguelikeUtils;

namespace TerRoguelike.NPCs.Enemy.Boss
{
    public class Skeletron : BaseRoguelikeNPC
    {
        public Entity target;
        public Vector2 spawnPos;
        public bool ableToHit = true;
        public bool canBeHit = true;
        public override int modNPCID => ModContent.NPCType<Skeletron>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Dungeon"] };
        public override int CombatStyle => -1;
        public int currentFrame;
        public bool CollisionPass = false;
        public Texture2D eyeTex;

        public int deadTime = 0;
        public int cutsceneDuration = 120;
        public int deathCutsceneDuration = 120;
        bool skipEyeParticles = false;

        public static Attack None = new Attack(0, 0, 100);
        public static Attack Charge = new Attack(1, 30, 270);
        public static Attack SoulBurst = new Attack(2, 30, 180);
        public static Attack BoneSpear = new Attack(3, 30, 180);
        public static Attack SoulTurret = new Attack(4, 30, 180);
        public static Attack TeleportDash = new Attack(5, 30, 180);
        public static Attack Summon = new Attack(6, 30, 180);
        public int chargeWindup = 60;
        public int chargeFireRate = 60;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 2;
            NPCID.Sets.TrailCacheLength[Type] = 2;
            NPCID.Sets.TrailingMode[Type] = 3;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 80;
            NPC.height = 80;
            NPC.aiStyle = -1;
            NPC.damage = 35;
            NPC.lifeMax = 29000;
            NPC.HitSound = SoundID.NPCHit2;
            NPC.knockBackResist = 0f;
            modNPC.drawCenter = new Vector2(0, 0);
            modNPC.IgnoreRoomWallCollision = true;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            eyeTex = TexDict["SkeletronEye"].Value;
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
        }
        public override void PostAI()
        {
            if (skipEyeParticles)
                skipEyeParticles = false;
            else
            {
                var positions = EyePositions(NPC.Center + NPC.velocity, NPC.rotation);
                var oldPositions = EyePositions(NPC.oldPos[1] + new Vector2(NPC.width, NPC.height) * 0.5f, NPC.oldRot[1]);
                Color particleColor = Color.Lerp(Color.Cyan, Color.Blue, 0.15f);

                for (int i = 0; i < positions.Count; i++)
                {
                    Vector2 offset = oldPositions[i] - positions[i];

                    for (int j = 0; j < 8; j++)
                    {
                        int time = 6 + Main.rand.Next(8);
                        float completion = j / 8f;
                        bool switchup = Main.rand.NextBool(20);
                        Vector2 pos = positions[i] + offset * completion;
                        Vector2 velocity = switchup ? -Vector2.UnitY * 1.25f + NPC.velocity : Vector2.Zero;
                        Vector2 scale = switchup ? new Vector2(0.075f) : new Vector2(0.1f);
                        ParticleManager.AddParticle(new Ball(
                            pos - Vector2.UnitY * 2, velocity + Main.rand.NextVector2Circular(0.25f, 0.25f),
                            time, particleColor, scale, 0, 0.96f, time, false));
                    }
                }
            }
        }
        public override void AI()
        {
            NPC.frameCounter += 0.25d;

            if (deadTime > 0)
            {
                CheckDead();
                return;
            }
            if (modNPC.isRoomNPC && NPC.localAI[0] == -(cutsceneDuration + 30))
            {
                SetBossTrack(QueenBeeTheme);
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
                    NPC.immortal = false;
                    NPC.dontTakeDamage = false;
                    NPC.ai[1] = 0;
                }
            }
            else
            {
                NPC.localAI[0]++;
                BossAI();
                bool defaultRotation = NPC.ai[0] == None.Id || NPC.ai[0] == BoneSpear.Id || NPC.ai[0] == SoulTurret.Id || NPC.ai[0] == Summon.Id;
                if (defaultRotation)
                {
                    float rotBound = MathHelper.PiOver2 * 0.6f;
                    NPC.rotation = NPC.rotation.AngleTowards(MathHelper.Clamp(NPC.velocity.X * 0.1f, -rotBound, rotBound), 0.2f);
                }
                    
            }
        }
        public void BossAI()
        {
            target = modNPC.GetTarget(NPC);

            NPC.ai[1]++;
            NPC.velocity *= 0.98f;

            if (NPC.ai[0] == None.Id)
            {
                UpdateDirection();
                if (NPC.ai[1] == None.Duration)
                {
                    Room room = modNPC.isRoomNPC ? RoomList[modNPC.sourceRoomListID] : null;
                    if (room != null)
                    {
                        if (!room.GetRect().Contains(NPC.getRect()))
                        {
                            NPC.velocity += (room.GetRect().ClosestPointInRect(NPC.Center) - NPC.Center).SafeNormalize(Vector2.UnitY) * 0.1f;
                            NPC.ai[1]--;
                        }
                    }
                }

                if (NPC.ai[1] >= None.Duration)
                {
                    ChooseAttack();
                }
                else
                {
                    DefaultMovement();
                }

            }
            if (NPC.ai[0] == Charge.Id)
            {
                Vector2 targetPos = target != null ? target.Center : NPC.velocity.ToRotation().AngleTowards((spawnPos - NPC.Center).ToRotation(), 0.03f).ToRotationVector2() * 180 + NPC.Center;
                float magnitude = MathHelper.Clamp(targetPos.Distance(NPC.Center) * 0.015f, 3f, 8f);
                NPC.velocity = (targetPos - NPC.Center).SafeNormalize(Vector2.UnitY) * magnitude;
                NPC.rotation += 0.2f * NPC.direction;

                if (NPC.ai[1] < chargeWindup)
                {
                    float startupCompletion = NPC.ai[1] / chargeWindup;
                    NPC.velocity *= (float)Math.Pow(startupCompletion, 4);
                    if (NPC.ai[1] == 0)
                    {
                        SoundEngine.PlaySound(SoundID.Roar with { Volume = 1f }, NPC.Center);
                    }
                }
                else
                {
                    int time = (int)NPC.ai[1] - chargeWindup;
                    if (time % chargeFireRate == 0)
                    {
                        SoundEngine.PlaySound(SoundID.NPCHit36 with { Volume = 0.3f, MaxInstances = 2 }, NPC.Center);
                        float fireDirection = (-NPC.velocity).ToRotation();
                        for (float i = -3; i <= 3; i += 2)
                        {
                            Vector2 projVelDir = (fireDirection + 0.5f * i).ToRotationVector2();
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + projVelDir * 28, projVelDir * 12, ModContent.ProjectileType<SeekingSoulBlast>(), NPC.damage, 0, -1, targetPos.X, targetPos.Y, NPC.velocity.ToRotation());
                        }
                    }
                }
                if (NPC.Center.Distance(NPC.Center + NPC.velocity) >= NPC.Center.Distance(targetPos))
                {
                    NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY * NPC.Center.Distance(targetPos));
                }
                
                if (NPC.ai[1] >= Charge.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Charge.Id;
                }
            }
            else if (NPC.ai[0] == SoulBurst.Id)
            {
                if (NPC.ai[1] >= SoulBurst.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = SoulBurst.Id;
                }
            }
            else if (NPC.ai[0] == BoneSpear.Id)
            {
                DefaultMovement();

                if (NPC.ai[1] >= BoneSpear.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = BoneSpear.Id;
                }
            }
            else if (NPC.ai[0] == SoulTurret.Id)
            {
                if (NPC.ai[1] >= SoulTurret.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = SoulTurret.Id;
                }
            }
            else if (NPC.ai[0] == TeleportDash.Id)
            {
                if (NPC.ai[1] >= TeleportDash.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = TeleportDash.Id;
                }
            }
            else if (NPC.ai[0] == Summon.Id)
            {
                if (NPC.ai[1] >= Summon.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Summon.Id;
                }
            }

            void DefaultMovement()
            {
                if (target != null)
                {
                    Vector2 targetPos = target.Center + new Vector2(0, -160);
                    targetPos = TileCollidePositionInLine(target.Center + new Vector2(0, -80), targetPos);
                    if (NPC.velocity.Length() < 10)
                        NPC.velocity += (targetPos - NPC.Center).SafeNormalize(Vector2.UnitY) * 0.15f;
                    if (NPC.velocity.Length() > 10)
                        NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * 10;
                }
            }
            void UpdateDirection()
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
            }
        }
        public void ChooseAttack()
        {
            NPC.ai[1] = 0;
            int chosenAttack = 0;

            List<Attack> potentialAttacks = new List<Attack>() { Charge, SoulBurst, BoneSpear, SoulTurret, TeleportDash, Summon };
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

            chosenAttack = Charge.Id;
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
            skipEyeParticles = true;


            if (deadTime == 0)
            {
                NPC.velocity = Vector2.Zero;
                NPC.rotation = 0;
                modNPC.ignitedStacks.Clear();
                if (modNPC.isRoomNPC)
                {
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
                for (int i = 0; (double)i < hit.Damage * 0.025d; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 26, hit.HitDirection, -1f);
                }
            }
        }
        public override void OnKill()
        {
            SoundEngine.PlaySound(SoundID.NPCDeath2 with { Volume = 1f }, NPC.Center);

            for (int i = 0; i < 150; i++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, 26, Main.rand.NextFloat(-2.5f, 2.5f), -2.5f);
            }
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 54);
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 55);
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

            Main.EntitySpriteDraw(tex, NPC.Center + modNPC.drawCenter - Main.screenPosition, NPC.frame, color, NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);

            var positions = EyePositions(NPC.Center, NPC.rotation);
            for (int i = 0; i < positions.Count; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    float scaleoff = Main.rand.NextFloat(0.2f);
                    Vector2 scale = new Vector2(1 - scaleoff, 1 + scaleoff);
                    Main.EntitySpriteDraw(eyeTex, positions[i] - Main.screenPosition + Main.rand.NextVector2CircularEdge(1.5f, 2f) * NPC.scale + (-Vector2.UnitY * scaleoff * eyeTex.Height * NPC.scale * 0.8f), null, Color.White * 0.4f, 0, eyeTex.Size() * 0.5f, scale * NPC.scale, SpriteEffects.None);
                }
                Main.EntitySpriteDraw(eyeTex, positions[i] - Main.screenPosition, null, Color.White, 0, eyeTex.Size() * 0.5f, NPC.scale, SpriteEffects.None);
                
            }
            return false;
        }
        public List<Vector2> EyePositions(Vector2 center, float rotation)
        {
            List<Vector2> positions = [];
            for (int i = -1; i <= 1; i += 2)
            {
                Vector2 offset = (new Vector2(18 * i, -12) * NPC.scale).RotatedBy(rotation);
                positions.Add(center + offset);
            }
            return positions;
        }
    }
}
