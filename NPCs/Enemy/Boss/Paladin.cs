using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TerRoguelike.Projectiles;
using TerRoguelike.Systems;
using static TerRoguelike.Managers.TextureManager;
using static TerRoguelike.Schematics.SchematicManager;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using TerRoguelike.Particles;
using TerRoguelike.Managers;

namespace TerRoguelike.NPCs.Enemy.Boss
{
    public class Paladin : BaseRoguelikeNPC
    {
        Entity target;
        Vector2 spawnPos;
        public float acceleration = 0.05f;
        public float deceleration = 0.95f;
        public float xCap = 2f;
        public float chargeSpeed = 8f;
        public int chargeTelegraph = 60;
        public float jumpVelocity = -7.9f;
        public override int modNPCID => ModContent.NPCType<Paladin>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Base"] };
        public override int CombatStyle => -1;
        public int currentFrame;
        public Texture2D hammerTex;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 16;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 32;
            NPC.height = 56;
            NPC.aiStyle = -1;
            NPC.damage = 25;
            NPC.lifeMax = 20000;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.NPCDeath6;
            NPC.knockBackResist = 0f;
            modNPC.drawCenter = new Vector2(0, -12);
            hammerTex = TexDict["PaladinHammer"];
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.immortal = true;
            NPC.dontTakeDamage = true;
            currentFrame = Main.npcFrameCount[Type] - 1;
            NPC.localAI[0] = -270;
            NPC.direction = -1;
            NPC.spriteDirection = -1;
            spawnPos = NPC.Center;
        }
        public override void AI()
        {
            if (NPC.collideY)
                NPC.localAI[1] = 0;
            else
                NPC.localAI[1]++;

            if (NPC.localAI[0] < 0)
            {
                if (NPC.localAI[0] == -210)
                {
                    CutsceneSystem.SetCutscene(NPC.Center, 210, 30, 30, 2.5f);
                    SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot with { Volume = 1f, Pitch = -1.3f }, NPC.Center);
                }
                if (NPC.localAI[0] == -150)
                {
                    SoundEngine.PlaySound(SoundID.DeerclopsRubbleAttack with { Volume = 0.75f, Pitch = 0.5f }, NPC.Center);
                    SoundEngine.PlaySound(SoundID.DeerclopsIceAttack with { Volume = 0.375f, Pitch = -0.2f }, NPC.Center);
                    SoundEngine.PlaySound(SoundID.Item70 with { Volume = 0.375f, Pitch = -0.2f }, NPC.Center);
                    for (int i = -15; i <= 15; i++)
                    {
                        Dust d = Dust.NewDustPerfect(NPC.Bottom + new Vector2(3 * i, 0), DustID.Smoke, null, 0, Color.LightGray, 1f);
                        d.velocity.Y -= 1;
                    }
                    for (int i = -15; i <= 15; i++)
                    {
                        Dust d = Dust.NewDustPerfect(NPC.Bottom + new Vector2(0.75f * i, 0), DustID.Stone, new Vector2(Main.rand.NextFloat(0.05f, 0.15f) * i + (Math.Sign(i) * 1.5f), Main.rand.NextFloat(-4f, -2f)), 0, default, 1.2f);
                    }
                    for (int i = -2; i <= 2; i++)
                    {
                        int goreid = Main.rand.NextFromCollection(new List<int>() { GoreID.Smoke1, GoreID.Smoke2, GoreID.Smoke3 });
                        Gore.NewGorePerfect(NPC.GetSource_FromThis(), NPC.Bottom + new Vector2(16 * i + (24 * NPC.direction), -16), new Vector2(Main.rand.NextFloat(-0.08f, 0.35f) * i, Main.rand.NextFloat(-1.2f, -0.7f)), goreid);
                    }
                    for (int i = -1; i <= 1; i += 2)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(0, 10), new Vector2(i * 6, 16), ModContent.ProjectileType<Shockwave>(), NPC.damage, 0f);
                    }
                }
                NPC.localAI[0]++;
                if (NPC.localAI[0] == -30)
                {
                    NPC.immortal = false;
                    NPC.dontTakeDamage = false;
                }
            }
            else
            {
                NPC.localAI[0]++;
                BossAI();
            }
        }
        public Attack None = new Attack(0, 0, 300);
        public Attack Charge = new Attack(1, 30, 180);
        public Attack Throw = new Attack(2, 30, 180);
        public Attack Slam = new Attack(3, 20, 240);
        public Attack Summon = new Attack(4, 20, 150);
        public void BossAI()
        {
            target = modNPC.GetTarget(NPC, false, false);

            acceleration = 0.05f;
            deceleration = 0.93f;
            xCap = target == null ? 0 : 2f;
            chargeSpeed = 8f;
            chargeTelegraph = 60;
            jumpVelocity = -7.9f;

            NPC.ai[1]++;

            if (NPC.ai[0] == None.Id)
            {
                if (NPC.ai[1] >= None.Duration)
                {
                    ChooseAttack();
                }
                else
                {
                    if (target != null)
                    {
                        if (NPC.Center.X < target.Center.X)
                        {
                            NPC.direction = 1;
                            NPC.spriteDirection = 1;
                        }
                        else
                        {
                            NPC.direction = -1;
                            NPC.spriteDirection = -1;
                        }
                    }
                    if (NPC.velocity.X < -xCap || NPC.velocity.X > xCap)
                    {
                        if (NPC.velocity.Y == 0f)
                            NPC.velocity *= 0.8f;
                    }
                    else if (NPC.velocity.X < xCap && NPC.direction == 1)
                    {
                        NPC.velocity.X += acceleration;
                        if (NPC.velocity.X > xCap)
                            NPC.velocity.X = xCap;
                    }
                    else if (NPC.velocity.X > -xCap && NPC.direction == -1)
                    {
                        NPC.velocity.X -= acceleration;
                        if (NPC.velocity.X < -xCap)
                            NPC.velocity.X = -xCap;
                    }
                }
                NPC.frameCounter += 0.08d * Math.Abs(NPC.velocity.X);
            }

            if (NPC.ai[0] == Charge.Id)
            {
                if (NPC.ai[1] == 0)
                {
                    if (target != null)
                    {
                        if (NPC.Center.X < target.Center.X)
                        {
                            NPC.direction = 1;
                            NPC.spriteDirection = 1;
                        }
                        else
                        {
                            NPC.direction = -1;
                            NPC.spriteDirection = -1;
                        }
                    }
                }
                if (NPC.ai[1] < chargeTelegraph)
                {
                    NPC.velocity.X *= deceleration;
                    if (NPC.ai[1] % 20 == 0 || NPC.ai[1] % 20 == 5)
                    {
                        Vector2 particleVelocity = new Vector2(-3f * NPC.direction, -0.4f);
                        Vector2 particleScale = new Vector2(0.34f, 0.34f);
                        particleScale *= Main.rand.NextFloat(0.8f, 1f);
                        int timeLeft = Main.rand.Next(20, 26);

                        ParticleManager.AddParticle(new Spark(NPC.Center + new Vector2(NPC.width * 0.6f * -NPC.direction, 8), particleVelocity, timeLeft, Color.DarkGray, particleScale, particleVelocity.ToRotation()));
                        ParticleManager.AddParticle(new Spark(NPC.Center + new Vector2(NPC.width * 0.6f * -NPC.direction, 24), particleVelocity + new Vector2(0, -0.2f), timeLeft, Color.DarkGray, particleScale, particleVelocity.ToRotation()));

                        ParticleManager.AddParticle(new Spark(NPC.Center + new Vector2(NPC.width * 0.6f * -NPC.direction, -16), particleVelocity + new Vector2(0, 0.4f), timeLeft, Color.DarkGray, particleScale, particleVelocity.ToRotation()));
                    }
                }
                else if (NPC.ai[1] >= chargeTelegraph && NPC.ai[1] <= chargeTelegraph + 1)
                {
                    NPC.ai[1] = chargeTelegraph;
                    if (NPC.collideX)
                    {
                        SoundEngine.PlaySound(SoundID.DeerclopsRubbleAttack with { Volume = 0.5f, Pitch = 0.5f }, NPC.Center);
                        SoundEngine.PlaySound(SoundID.DeerclopsIceAttack with { Volume = 0.25f, Pitch = -0.2f }, NPC.Center);
                        SoundEngine.PlaySound(SoundID.Item70 with { Volume = 0.375f, Pitch = -0.2f }, NPC.Center);
                        for (int i = -15; i <= 15; i++)
                        {
                            Dust d = Dust.NewDustPerfect(NPC.Bottom + new Vector2(3 * i, 0), DustID.Smoke, null, 0, Color.LightGray, 1f);
                            d.velocity.Y -= 1;
                            d.velocity.X += -NPC.oldVelocity.X * 0.25f;
                        }
                        for (int i = -15; i <= 15; i++)
                        {
                            Dust d = Dust.NewDustPerfect(NPC.Bottom + new Vector2(0.75f * i, 0), DustID.Stone, new Vector2(Main.rand.NextFloat(0.05f, 0.15f) * i + (Math.Sign(i) * 1.5f), Main.rand.NextFloat(-4f, -2f)), 0, default, 1.2f);
                            d.velocity.X += -NPC.oldVelocity.X * 0.25f;
                        }
                        for (int i = -1; i <= 2; i++)
                        {
                            int goreid = Main.rand.NextFromCollection(new List<int>() { GoreID.Smoke1, GoreID.Smoke2, GoreID.Smoke3 });
                            Gore g = Gore.NewGorePerfect(NPC.GetSource_FromThis(), NPC.Center + new Vector2(16 * Math.Sign(NPC.oldVelocity.X), -16 - 16 * i), new Vector2(Main.rand.NextFloat(-1.2f, -0.7f) * Math.Sign(NPC.oldVelocity.X), Main.rand.NextFloat(-0.08f, 0.08f)), goreid);
                        }

                        NPC.ai[1] += 2;
                        NPC.velocity.X = -chargeSpeed * NPC.direction * 0.55f;
                        NPC.velocity.Y = -chargeSpeed * 0.4f;
                    }
                    else
                    {
                        NPC.velocity.X = chargeSpeed * NPC.direction;
                        if ((int)NPC.localAI[0] % 9 == 0)
                        {
                            SoundEngine.PlaySound(SoundID.DeerclopsStep with { Volume = 0.35f, Pitch = -0.1f, MaxInstances = 8 }, NPC.Center);
                            SoundEngine.PlaySound(SoundID.NPCHit4 with { Volume = 0.05f, Pitch = -0.9f, PitchVariance = 0.08f, MaxInstances = 8 }, NPC.Center);
                        }
                        if ((int)NPC.localAI[0] % 3 == 0)
                        {
                            Vector2 particleVelocity = new Vector2(-3f * Math.Sign(NPC.velocity.X), -0.4f);
                            Vector2 particleScale = new Vector2(0.45f, 0.34f);
                            particleScale *= Main.rand.NextFloat(0.8f, 1f);
                            int timeLeft = Main.rand.Next(20, 26);

                            ParticleManager.AddParticle(new Spark(NPC.Center + new Vector2(NPC.width * 0.5f * -Math.Sign(NPC.velocity.X), 8), particleVelocity, timeLeft, Color.Goldenrod, particleScale, particleVelocity.ToRotation()));
                            ParticleManager.AddParticle(new Spark(NPC.Center + new Vector2(NPC.width * 0.5f * -Math.Sign(NPC.velocity.X), 24), particleVelocity + new Vector2(0, -0.2f), timeLeft, Color.Goldenrod, particleScale, particleVelocity.ToRotation()));

                            ParticleManager.AddParticle(new Spark(NPC.Center + new Vector2(NPC.width * 0.5f * -Math.Sign(NPC.velocity.X), -16), particleVelocity + new Vector2(0, 0.4f), timeLeft, Color.Goldenrod, particleScale, particleVelocity.ToRotation()));
                        }
                    }
                }
                if (NPC.ai[1] > chargeTelegraph + 1)
                {
                    NPC.velocity.X *= deceleration;
                }
                if (NPC.ai[1] >= Charge.Duration)
                {
                    NPC.ai[0] = 0;
                    NPC.ai[1] = 0;
                }
                NPC.frameCounter += 0.05d * Math.Abs(NPC.velocity.X);
            }
            else if (NPC.ai[0] == Throw.Id)
            {
                NPC.velocity.X *= deceleration;
                if (NPC.ai[1] >= Throw.Duration)
                {
                    NPC.ai[0] = 0;
                    NPC.ai[1] = 0;
                }
            }
            else if (NPC.ai[0] == Slam.Id)
            {
                NPC.velocity.X *= deceleration;
                if (NPC.ai[1] >= Slam.Duration)
                {
                    NPC.ai[0] = 0;
                    NPC.ai[1] = 0;
                }
            }
            else if (NPC.ai[0] == Summon.Id)
            {
                NPC.velocity.X *= deceleration;
                if (NPC.ai[1] >= Summon.Duration)
                {
                    NPC.ai[0] = 0;
                    NPC.ai[1] = 0;
                }
            }

            if (target != null)
            {
                if (NPC.ai[0] == None.Id && NPC.ai[1] < None.Duration - 60)
                {
                    if (NPC.velocity.Y == 0f && target.Bottom.Y < NPC.Top.Y && Math.Abs(NPC.Center.X - target.Center.X) < (float)(target.width * 3) && Collision.CanHit(NPC, target))
                    {
                        if (NPC.velocity.Y == 0f)
                        {
                            int padding = (int)(6 * (jumpVelocity / -7.9f));
                            if (target.Bottom.Y > NPC.Top.Y - (float)(padding * 16))
                            {
                                NPC.velocity.Y = jumpVelocity;
                                NPC.localAI[1] = 10;
                            }
                            else
                            {
                                int bottomtilepointx = (int)(NPC.Center.X / 16f);
                                int bottomtilepointY = (int)(NPC.Bottom.Y / 16f) - 1;
                                for (int i = bottomtilepointY; i > bottomtilepointY - padding; i--)
                                {
                                    if (Main.tile[bottomtilepointx, i].HasUnactuatedTile && TileID.Sets.Platforms[Main.tile[bottomtilepointx, i].TileType])
                                    {
                                        NPC.velocity.Y = jumpVelocity;
                                        NPC.localAI[1] = 10;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else if (NPC.velocity.Y == 0f && target.Top.Y > NPC.Bottom.Y && Math.Abs(NPC.Center.X - target.Center.X) < (float)(target.width * 3) && Collision.CanHit(NPC, target))
                    {
                        int fluff = 6;
                        int bottomtilepointx = (int)(NPC.Center.X / 16f);
                        int bottomtilepointY = (int)(NPC.Bottom.Y / 16f);
                        for (int i = bottomtilepointY; i > bottomtilepointY - fluff - 1; i--)
                        {
                            if (Main.tile[bottomtilepointx, i].HasUnactuatedTile && TileID.Sets.Platforms[Main.tile[bottomtilepointx, i].TileType])
                            {
                                NPC.position.Y += 1;
                                NPC.stairFall = true;
                                NPC.velocity.Y += 0.01f;
                                break;
                            }
                        }
                    }
                }
            }

            if (NPC.velocity.Y >= 0f)
            {
                int dir = 0;
                if (NPC.velocity.X < 0f)
                    dir = -1;
                if (NPC.velocity.X > 0f)
                    dir = 1;

                Vector2 futurePos = NPC.position;
                futurePos.X += NPC.velocity.X;
                int tileX = (int)((futurePos.X + (float)(NPC.width / 2) + (float)((NPC.width / 2 + 1) * dir)) / 16f);
                int tileY = (int)((futurePos.Y + (float)NPC.height - 1f) / 16f);
                if (WorldGen.InWorld(tileX, tileY, 4))
                {
                    if ((float)(tileX * 16) < futurePos.X + (float)NPC.width && (float)(tileX * 16 + 16) > futurePos.X && ((Main.tile[tileX, tileY].HasUnactuatedTile && !TopSlope(Main.tile[tileX, tileY]) && !TopSlope(Main.tile[tileX, tileY - 1]) && Main.tileSolid[Main.tile[tileX, tileY].TileType] && !Main.tileSolidTop[Main.tile[tileX, tileY].TileType]) || (Main.tile[tileX, tileY - 1].IsHalfBlock && Main.tile[tileX, tileY - 1].HasUnactuatedTile)) && (!Main.tile[tileX, tileY - 1].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX, tileY - 1].TileType] || Main.tileSolidTop[Main.tile[tileX, tileY - 1].TileType] || (Main.tile[tileX, tileY - 1].IsHalfBlock && (!Main.tile[tileX, tileY - 4].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX, tileY - 4].TileType] || Main.tileSolidTop[Main.tile[tileX, tileY - 4].TileType]))) && (!Main.tile[tileX, tileY - 2].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX, tileY - 2].TileType] || Main.tileSolidTop[Main.tile[tileX, tileY - 2].TileType]) && (!Main.tile[tileX, tileY - 3].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX, tileY - 3].TileType] || Main.tileSolidTop[Main.tile[tileX, tileY - 3].TileType]) && (!Main.tile[tileX - dir, tileY - 3].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX - dir, tileY - 3].TileType]))
                    {
                        float tilePosY = tileY * 16;
                        if (Main.tile[tileX, tileY].IsHalfBlock)
                            tilePosY += 8f;

                        if (Main.tile[tileX, tileY - 1].IsHalfBlock)
                            tilePosY -= 8f;

                        if (tilePosY < futurePos.Y + (float)NPC.height)
                        {
                            float difference = futurePos.Y + (float)NPC.height - tilePosY;
                            if (difference <= 16.1f)
                            {
                                NPC.gfxOffY += NPC.position.Y + (float)NPC.height - tilePosY;
                                NPC.position.Y = tilePosY - (float)NPC.height;
                            }

                            if (difference < 9f)
                                NPC.stepSpeed = 1f;
                            else
                                NPC.stepSpeed = 2f;
                        }

                    }
                }
            }
        }
        public void ChooseAttack()
        {
            NPC.ai[1] = 0;
            List<Attack> potentialAttacks = new List<Attack>() { Charge, Throw, Slam, Summon };
            potentialAttacks.RemoveAll(x => x.Id == (int)NPC.ai[0]);
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
            chosenAttack = Charge.Id;
            NPC.ai[0] = chosenAttack;
        }
        public override bool? CanFallThroughPlatforms()
        {
            if (target != null)
            {
                if (NPC.ai[0] == Charge.Id)
                {
                    if (NPC.ai[1] >= chargeTelegraph && NPC.ai[1] <= chargeTelegraph + 1)
                    {
                        if (NPC.direction == -1)
                        {
                            if ((NPC.Center.X > target.Center.X - 120f && NPC.Bottom.Y >= target.Bottom.Y))
                                return false;
                            else
                                return true;
                        }
                        else
                        {
                            if ((NPC.Center.X < target.Center.X + 120f && NPC.Bottom.Y >= target.Bottom.Y))
                                return false;
                            else
                                return true;
                        }
                    }
                }
            }
            return null;
        }
        public override void OnKill()
        {
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 10; i++)
                {
                    int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, 16, 0f, 0f, 0, default(Color), 1.5f);
                    Dust d = Main.dust[dust];
                    d.velocity *= 2f;
                    d.noGravity = true;
                }
                Vector2 pos = new Vector2(NPC.position.X, NPC.position.Y);
                Vector2 velocity = default(Vector2);
                int gore = Gore.NewGore(NPC.GetSource_Death(), pos, velocity, Main.rand.Next(11, 14), NPC.scale);
                Gore g = Main.gore[gore];
                g.velocity *= 0.5f;

                pos = new Vector2(NPC.position.X, NPC.position.Y + 20f);
                gore = Gore.NewGore(NPC.GetSource_Death(), pos, velocity, Main.rand.Next(11, 14), NPC.scale);
                g = Main.gore[gore];
                g.velocity *= 0.5f;

                pos = new Vector2(NPC.position.X, NPC.position.Y + 40f);
                gore = Gore.NewGore(NPC.GetSource_Death(), pos, velocity, Main.rand.Next(11, 14), NPC.scale);
                g = Main.gore[gore];
                g.velocity *= 0.5f;
            }
        }
        public override void FindFrame(int frameHeight)
        {
            int frameCount = Main.npcFrameCount[Type];
            int swingTime = 0;
            if (NPC.localAI[0] < -30)
            {
                if (NPC.localAI[0] < -150)
                    currentFrame = frameCount - 1;
                else if (NPC.localAI[0] < -120)
                    currentFrame = frameCount - 2;
                else if (NPC.localAI[0] >= -50)
                    currentFrame = 0;
                else if (NPC.localAI[0] >= -70)
                    currentFrame = frameCount - 3;
                else if (NPC.localAI[0] >= -90)
                    currentFrame = frameCount - 4;
            }
            else
            {
                if (NPC.ai[0] == Throw.Id)
                {
                    currentFrame = swingTime < 0 ? frameCount - 7 : (swingTime < 10 ? frameCount - 6 : frameCount - 5);
                    NPC.frameCounter = 0;
                }
                else if (NPC.ai[0] == Charge.Id)
                {
                    if (NPC.ai[1] < chargeTelegraph)
                    {
                        currentFrame = frameCount - 4; 
                    }
                    else if (NPC.ai[1] >= chargeTelegraph && NPC.ai[1] <= chargeTelegraph + 1)
                    {
                        currentFrame = ((int)NPC.frameCounter % (frameCount - 9)) + 2;
                    }
                    else
                    {
                        currentFrame = 1;
                    }
                }
                else
                {
                    currentFrame = NPC.localAI[1] < 10 ? ((int)NPC.frameCounter % (frameCount - 9)) + 2 : 1;
                }
            }

            NPC.frame = new Rectangle(0, currentFrame * frameHeight, TextureAssets.Npc[Type].Value.Width, frameHeight);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;
            int frameHeight = tex.Height / Main.npcFrameCount[Type];
            modNPC.drawCenter = new Vector2(0, -((frameHeight - NPC.height) * 0.5f) + 16);

            if (NPC.localAI[0] < -150)
            {
                modNPC.drawCenter.Y += MathHelper.Lerp(0, -1000, Math.Abs(NPC.localAI[0] + 150) / 60f);
            }
            Vector2 drawPos = NPC.Center + modNPC.drawCenter + (Vector2.UnitY * NPC.gfxOffY);
            Main.EntitySpriteDraw(tex, drawPos - Main.screenPosition, NPC.frame, Lighting.GetColor(drawPos.ToTileCoordinates()), NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            if (NPC.localAI[0] < -90)
            {
                Vector2 hammerPos = new Vector2(-8f, 0) + NPC.Bottom;
                Main.EntitySpriteDraw(hammerTex, hammerPos - Main.screenPosition, null, Lighting.GetColor(hammerPos.ToTileCoordinates()), MathHelper.Pi, hammerTex.Size() * 0.5f, 1f, SpriteEffects.None);
            }
            return false;
        }
    }
}
