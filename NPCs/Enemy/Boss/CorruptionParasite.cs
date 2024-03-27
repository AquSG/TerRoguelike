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
using TerRoguelike.Systems;
using static TerRoguelike.Managers.TextureManager;
using static TerRoguelike.Schematics.SchematicManager;
using static TerRoguelike.Systems.MusicSystem;
using static TerRoguelike.Systems.RoomSystem;
using static TerRoguelike.Utilities.TerRoguelikeUtils;

namespace TerRoguelike.NPCs.Enemy.Boss
{
    public class CorruptionParasite : BaseRoguelikeNPC
    {
        public Entity target;
        public Vector2 spawnPos;
        public bool ableToHit = true;
        public bool canBeHit = true;
        public override int modNPCID => ModContent.NPCType<CorruptionParasite>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Corrupt"] };
        public override int CombatStyle => -1;
        public int currentFrame;
        public Texture2D headTex;
        public Texture2D bodyTex;
        public Texture2D tailTex;
        public bool CollisionPass = false;

        public int deadTime = 0;
        public int cutsceneDuration = 120;
        public int deathCutsceneDuration = 120;

        public static Attack None = new Attack(0, 0, 180);
        public static Attack Charge = new Attack(1, 30, 480);
        public static Attack WaveTunnel = new Attack(2, 30, 180);
        public static Attack Vomit = new Attack(3, 30, 180);
        public static Attack ProjCharge = new Attack(4, 30, 180);
        public static Attack Summon = new Attack(5, 30, 180);
        public float defaultMinVelociy = 2;
        public float defaultMaxVelociy = 7;
        public float defaultLookingAtThreshold = MathHelper.PiOver4 * 0.5f;
        public float defaultAcceleration = 0.04f;
        public float defaultDecelertaion = 0.02f;
        public float defaultMinRotation = 0.015f;
        public float defaultMaxRotation = 0.05f;
        public int defaultLookingAtBuffer = 0;
        public float segmentRotationInterpolant = 0.95f;
        public float chargeDesiredDist = 1000;
        public Vector2 chargeDesiredPos;
        public int chargeCount = 3;
        public int chargeTelegraph = 40;
        public int chargingDuration = 120;
        public float chargeSpeed = 11f;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 1;
            NPCID.Sets.MustAlwaysDraw[modNPCID] = true;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 38;
            NPC.height = 38;
            NPC.aiStyle = -1;
            NPC.damage = 35;
            NPC.lifeMax = 30000;
            NPC.knockBackResist = 0f;
            modNPC.drawCenter = new Vector2(0, 0);
            modNPC.IgnoreRoomWallCollision = true;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            modNPC.OverrideIgniteVisual = true;
            NPC.behindTiles = true;
            headTex = TexDict["CorruptionParasiteHead"].Value;
            bodyTex = TexDict["CorruptionParasiteBody"].Value;
            tailTex = TexDict["CorruptionParasiteTail"].Value;
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
            NPC.ai[1] = -48;
            ableToHit = false;

            int segCount = 50;
            int segmentHeight = 38;
            NPC.position.Y += 320;
            NPC.velocity.Y = -2;
            for (int i = 0; i < segCount; i++)
            {
                modNPC.Segments.Add(new WormSegment(NPC.Center + (Vector2.UnitY * segmentHeight * i), MathHelper.PiOver2 * 3f, i == 0 ? NPC.height : segmentHeight));
            }
        }
        public override void PostAI()
        {
            NPC.rotation = NPC.velocity.ToRotation();
            modNPC.UpdateWormSegments(NPC, segmentRotationInterpolant);
        }
        public override void AI()
        {
            chargeDesiredDist = 1300;
            chargingDuration = 135;
            Charge = new Attack(1, 30, (chargingDuration + chargeTelegraph) * chargeCount);
            chargeSpeed = 20f;
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

            NPC.frameCounter += 0.13d;
            if (NPC.localAI[0] < 0)
            {
                if (NPC.localAI[0] == -cutsceneDuration)
                {
                    CutsceneSystem.SetCutscene(spawnPos, cutsceneDuration, 30, 30, 2.5f);
                }
                NPC.localAI[0]++;

                if (NPC.localAI[0] == -30)
                {
                    NPC.hide = false;
                    NPC.immortal = false;
                    NPC.dontTakeDamage = false;
                    NPC.ai[1] = 0;
                    NPC.ai[3] = 0;
                }
                if (NPC.localAI[0] < -30)
                    NPC.ai[1]++;
            }
            else
            {
                NPC.localAI[0]++;
                BossAI();
            }
        }
        public void BossAI()
        {
            target = modNPC.GetTarget(NPC, false, false);

            NPC.ai[1]++;

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

            if (NPC.ai[0] == Charge.Id)
            {
                int chargeProgress = (int)NPC.ai[1] % (chargeTelegraph + chargingDuration);
                if (chargeProgress == 0)
                {
                    ChooseChargeLocation();
                }
                if (chargeProgress <= 3)
                {
                    Vector2 targetPos = chargeDesiredPos;
                    float rotToTarget = (targetPos - NPC.Center).ToRotation();

                    float potentialRot = NPC.velocity.ToRotation().AngleTowards(rotToTarget, defaultMaxRotation * (NPC.velocity.Length() / 10f));

                    if (NPC.velocity.Length() < defaultMaxVelociy * 2)
                    {
                        NPC.velocity += NPC.velocity.SafeNormalize(Vector2.UnitY) * defaultAcceleration * 2;
                        if (NPC.velocity.Length() > defaultMaxVelociy * 2)
                            NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * defaultMaxVelociy * 2;
                    }

                    NPC.velocity = (Vector2.UnitX * NPC.velocity.Length()).RotatedBy(potentialRot);

                    if (chargeProgress == 3)
                    {
                        if ((NPC.Center - chargeDesiredPos).Length() > 70)
                        {
                            NPC.ai[1]--;
                            chargeProgress--;
                        }
                    }
                }
                if (chargeProgress >= 3)
                {
                    Vector2 targetPos = target == null ? spawnPos : target.Center;

                    if (chargeProgress == 3)
                    {
                        
                    }

                    if (chargeProgress < chargeTelegraph)
                    {
                        float rotToTarget = (targetPos - NPC.Center).ToRotation();
                        float potentialRot = NPC.velocity.ToRotation().AngleTowards(rotToTarget, 0.3f);
                        NPC.velocity = (Vector2.UnitX * NPC.velocity.Length()).RotatedBy(potentialRot) * 0.95f;
                    }
                    else
                    {
                        float completion = 1f - ((float)(chargeProgress - chargeTelegraph) / (chargingDuration));
                        NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * chargeSpeed * (completion * 0.75f + 0.25f);
                    }
                }

                if (NPC.ai[1] >= Charge.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Charge.Id;
                }
            }
            else if (NPC.ai[0] == WaveTunnel.Id)
            {
                if (NPC.ai[1] >= WaveTunnel.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = WaveTunnel.Id;
                }
            }
            else if (NPC.ai[0] == Vomit.Id)
            {
                if (NPC.ai[1] >= Vomit.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Vomit.Id;
                }
            }
            else if (NPC.ai[0] == ProjCharge.Id)
            {
                if (NPC.ai[1] >= ProjCharge.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = ProjCharge.Id;
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

            defaultLookingAtThreshold = 0.1f;
            bool defaultMovement = NPC.ai[0] != Charge.Id;
            if (defaultLookingAtBuffer > 0)
                defaultLookingAtBuffer--;

            if (defaultMovement)
            {
                Vector2 targetPos = target == null ? NPC.Center - NPC.velocity : target.Center;
                float rotToTarget = (targetPos - NPC.Center).ToRotation();
                float angleBetween = Math.Abs(AngleSizeBetween(NPC.rotation, rotToTarget));
                if (angleBetween < defaultLookingAtThreshold)
                    defaultLookingAtBuffer = 10;
                else if (angleBetween > MathHelper.PiOver2 && targetPos.Distance(NPC.Center) < 48f)
                    defaultLookingAtBuffer = 30;
                bool lookingAtTarget = defaultLookingAtBuffer > 0;
                float potentialRot = NPC.velocity.ToRotation().AngleTowards(rotToTarget, lookingAtTarget ? defaultMinRotation : defaultMaxRotation);

                if (lookingAtTarget)
                    NPC.velocity += NPC.velocity.SafeNormalize(Vector2.UnitY) * defaultAcceleration;
                else if (NPC.velocity.Length() > defaultMinVelociy)
                {
                    NPC.velocity -= NPC.velocity.SafeNormalize(Vector2.UnitY) * defaultDecelertaion;
                    if (NPC.velocity.Length() < defaultMinVelociy)
                        NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * defaultMinVelociy;
                }
                if (NPC.velocity.Length() > defaultMaxVelociy)
                    NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * defaultMaxVelociy;

                NPC.velocity = (Vector2.UnitX * NPC.velocity.Length()).RotatedBy(potentialRot);
            }
        }

        public void ChooseAttack()
        {
            NPC.ai[1] = 0;
            int chosenAttack = 0;


            List<Attack> potentialAttacks = new List<Attack>() { Charge };
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
        public void ChooseChargeLocation()
        {
            if (target != null)
                chargeDesiredPos = target.Center;
            else
                chargeDesiredPos = spawnPos;

            float rot = (NPC.Center - chargeDesiredPos).ToRotation();

            rot += Main.rand.NextFloat(-MathHelper.PiOver2, MathHelper.PiOver2 + float.Epsilon);
            chargeDesiredPos += rot.ToRotationVector2() * chargeDesiredDist;
        }
        public override bool? CanFallThroughPlatforms()
        {
            return true;
        }
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            for (int i = 0; i < modNPC.Segments.Count; i++)
            {
                WormSegment segment = modNPC.Segments[i];
                float radius = i == 0 ? (NPC.height < NPC.width ? NPC.height / 2 : NPC.width / 2) : segment.Height / 2;
                if (segment.Position.Distance(target.getRect().ClosestPointInRect(segment.Position)) <= radius)
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
            for (int i = 0; i < modNPC.Segments.Count; i++)
            {
                WormSegment segment = modNPC.Segments[i];
                float radius = i == 0 ? (NPC.height < NPC.width ? NPC.height / 2 : NPC.width / 2) : segment.Height / 2;
                if (segment.Position.Distance(target.getRect().ClosestPointInRect(segment.Position)) <= radius)
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
        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            if ((projectile.hostile && !NPC.friendly) || (projectile.friendly && NPC.friendly))
                return false;

            for (int i = 0; i < modNPC.Segments.Count; i++)
            {
                WormSegment segment = modNPC.Segments[i];
                bool pass = projectile.Colliding(projectile.getRect(), new Rectangle((int)(segment.Position.X - ((i == 0 ? NPC.width : segment.Height) / 2)), (int)(segment.Position.Y - ((i == 0 ? NPC.height : segment.Height) / 2)), NPC.width, NPC.height));
                if (pass)
                {
                    projectile.ModProj().ultimateCollideOverride = true;
                    modNPC.hitSegment = i;
                    return null;
                }
            }
            return false;
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
            SoundEngine.PlaySound(SoundID.NPCHit1, modNPC.Segments[modNPC.hitSegment].Position);
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / 25.0; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 5, hit.HitDirection, -1f);
                }
            }
            else
            {
                SoundEngine.PlaySound(SoundID.NPCDeath1, modNPC.Segments[modNPC.hitSegment].Position);
            }
        }
        public override void OnKill()
        {

        }
        public override void FindFrame(int frameHeight)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;
            int frameCount = Main.npcFrameCount[Type];

            currentFrame = 0;

            NPC.frame = new Rectangle(0, currentFrame * frameHeight, tex.Width, frameHeight);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (modNPC.OverrideIgniteVisual && modNPC.ignitedStacks.Any())
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                for (int i = modNPC.Segments.Count - 1; i >= 0; i--)
                {
                    Texture2D texture;
                    WormSegment segment = modNPC.Segments[i];
                    if (i == 0)
                        texture = headTex;
                    else if (i == modNPC.Segments.Count - 1)
                        texture = tailTex;
                    else
                        texture = bodyTex;

                    Color color = Color.Lerp(Color.Yellow, Color.OrangeRed, Main.rand.NextFloat(0.4f, 0.6f + float.Epsilon) + 0.2f + (0.2f * (float)Math.Cos((Main.GlobalTimeWrappedHourly * 20f)))) * 0.8f;
                    Vector3 colorHSL = Main.rgbToHsl(color);
                    float outlineThickness = 1f;
                    SpriteEffects spriteEffects = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

                    GameShaders.Misc["TerRoguelike:BasicTint"].UseOpacity(1f);
                    GameShaders.Misc["TerRoguelike:BasicTint"].UseColor(Main.hslToRgb(1 - colorHSL.X, colorHSL.Y, colorHSL.Z));
                    GameShaders.Misc["TerRoguelike:BasicTint"].Apply();

                    Vector2 position = segment.Position + (Vector2.UnitY * NPC.gfxOffY);
                    for (float j = 0; j < 1; j += 0.125f)
                    {
                        spriteBatch.Draw(texture, position + (j * MathHelper.TwoPi + segment.Rotation + MathHelper.PiOver2).ToRotationVector2() * outlineThickness - Main.screenPosition, null, color, segment.Rotation + MathHelper.PiOver2, texture.Size() * 0.5f, NPC.scale, spriteEffects, 0f);
                    }
                }
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            for (int i = modNPC.Segments.Count - 1; i >= 0; i--)
            {
                Texture2D texture;
                WormSegment segment = modNPC.Segments[i];
                if (i == 0)
                    texture = headTex;
                else if (i == modNPC.Segments.Count - 1)
                    texture = tailTex;
                else
                    texture = bodyTex;

                Color color = modNPC.ignitedStacks.Any() ? Color.Lerp(Color.White, Color.OrangeRed, 0.4f) : Lighting.GetColor(new Point((int)(segment.Position.X / 16), (int)(segment.Position.Y / 16)));
                spriteBatch.Draw(texture, segment.Position - screenPos, null, color, segment.Rotation + MathHelper.PiOver2, texture.Size() * 0.5f, 1f, SpriteEffects.None, 0);
            }
            return false;
        }
    }
}
