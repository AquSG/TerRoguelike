using Microsoft.CodeAnalysis;
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

namespace TerRoguelike.NPCs.Enemy.Boss
{
    public class IceQueen : BaseRoguelikeNPC
    {
        public Entity target;
        public Vector2 spawnPos;
        public bool ableToHit = true;
        public bool canBeHit = true;
        public override int modNPCID => ModContent.NPCType<IceQueen>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Snow"] };
        public override int CombatStyle => -1;
        public int currentFrame;
        public bool CollisionPass = false;
        public List<ExtraHitbox> hitboxes = new List<ExtraHitbox>()
        {
            new ExtraHitbox(new Point(80, 80), new Vector2(0)),
            new ExtraHitbox(new Point(40, 40), new Vector2(-55, -12)),
            new ExtraHitbox(new Point(40, 40), new Vector2(55, -12)),
            new ExtraHitbox(new Point(34, 34), new Vector2(-90, -20)),
            new ExtraHitbox(new Point(34, 34), new Vector2(90, -20)),
            new ExtraHitbox(new Point(45, 45), new Vector2(0, 59)),
            new ExtraHitbox(new Point(40, 40), new Vector2(0, -84)),
            new ExtraHitbox(new Point(26, 26), new Vector2(0, -54)),
        };
        Texture2D glowTex;
        Texture2D squareTex;

        public int deadTime = 0;
        public int cutsceneDuration = 120;
        public int deathCutsceneDuration = 120;

        public static Attack None = new Attack(0, 0, 300);
        public static Attack IceWave = new Attack(1, 0, 480);
        public static Attack Snowflake = new Attack(2, 0, 300);
        public static Attack Spin = new Attack(3, 0, 300);
        public static Attack IceRain = new Attack(4, 0, 300);
        public static Attack IceFog = new Attack(5, 0, 300);
        public static Attack Summon = new Attack(6, 0, 300);
        public float defaultMaxSpeed = 16f;
        public float defaultAcceleration = 0.2f;
        public float defaultDeceleration = 0.95f;


        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 6;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 140;
            NPC.height = 140;
            NPC.aiStyle = -1;
            NPC.damage = 30;
            NPC.lifeMax = 30000;
            NPC.HitSound = SoundID.NPCHit7;
            NPC.DeathSound = SoundID.NPCDeath5;
            NPC.knockBackResist = 0f;
            modNPC.drawCenter = new Vector2(0, 0);
            modNPC.IgnoreRoomWallCollision = true;
            modNPC.SpecialProjectileCollisionRules = true;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            glowTex = TexDict["IceQueenGlow"].Value;
            squareTex = TexDict["Square"].Value;
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
            ableToHit = false;
        }
        public override void PostAI()
        {
            switch (currentFrame)
            {
                default:
                case 0:
                    hitboxes[3].active = true;
                    hitboxes[4].active = true;
                    break;
                case 1:
                    hitboxes[3].active = false;
                    hitboxes[4].active = true;
                    break;
                case 2:
                    hitboxes[3].active = true;
                    hitboxes[4].active = false;
                    break;
                case 3:
                case 4:
                case 5:
                    hitboxes[3].active = false;
                    hitboxes[4].active = false;
                    break;
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
                SetBossTrack(CorruptionParasiteTheme);
            }

            ableToHit = NPC.localAI[0] >= 0;
            canBeHit = true;

            NPC.frameCounter += 0.13d;
            if (NPC.localAI[0] < 0)
            {
                target = modNPC.GetTarget(NPC, false, false);

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
                NPC.rotation = NPC.rotation.AngleLerp(MathHelper.Clamp(NPC.velocity.X * 0.05f, -MathHelper.PiOver2 * 0.33f, MathHelper.PiOver2 * 0.33f), 0.25f);
            }
        }
        public void BossAI()
        {
            target = modNPC.GetTarget(NPC, false, false);

            NPC.ai[1]++;
            NPC.velocity *= 0.985f;

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

            if (NPC.ai[0] == IceWave.Id)
            {
                bool outsideRoom = false;
                if (modNPC.isRoomNPC)
                {
                    
                    if (Math.Abs(NPC.ai[3]) > 10)
                    {
                        Room room = RoomList[modNPC.sourceRoomListID];
                        Vector2 checkPos = new Vector2(NPC.Center.X, room.RoomPosition16.Y + room.RoomCenter16.Y);
                        if (!room.GetRect().Contains((int)checkPos.X, (int)checkPos.Y))
                            outsideRoom = true;
                    }
                }
                if (Math.Abs(NPC.ai[3]) >= 60 || outsideRoom)
                {
                    NPC.direction *= -1;
                    NPC.ai[3] = 0;
                }
                Vector2 targetPos = target == null ? spawnPos : target.Center;
                if (NPC.ai[3] == 0)
                    NPC.ai[3] += NPC.direction;
                else if (Math.Sign(targetPos.X - NPC.Center.X) == -Math.Sign(NPC.ai[3]))
                {
                    NPC.ai[3] += Math.Sign(NPC.ai[3]);
                }
                if (NPC.ai[1] >= IceWave.Duration && Math.Abs(NPC.ai[3]) != 2)
                    NPC.ai[1]--;

                Vector2 wantedPos = targetPos + new Vector2(0, -282);
                wantedPos.X = NPC.Center.X + Math.Sign(NPC.ai[3]) * 250;

                if (NPC.velocity.Length() < defaultMaxSpeed)
                    NPC.velocity += (wantedPos - NPC.Center).SafeNormalize(Vector2.UnitY) * defaultAcceleration;
                if (NPC.velocity.Length() > defaultMaxSpeed)
                    NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * defaultMaxSpeed;

                if (NPC.ai[1] >= IceWave.Duration)
                {
                    currentFrame = 0;
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = IceWave.Id;
                    NPC.ai[3] = 0;
                }
            }
            else if (NPC.ai[0] == Snowflake.Id)
            {
                if (NPC.ai[1] >= Snowflake.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Snowflake.Id;
                }
            }
            else if (NPC.ai[0] == Spin.Id)
            {
                if (NPC.ai[1] >= Spin.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Spin.Id;
                }
            }
            else if (NPC.ai[0] == IceRain.Id)
            {
                if (NPC.ai[1] >= IceRain.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = IceRain.Id;
                }
            }
            else if (NPC.ai[0] == IceFog.Id)
            {
                if (NPC.ai[1] >= IceFog.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = IceFog.Id;
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

            bool defaultMovement = NPC.ai[0] == None.Id;
            bool sineWaveVelocity = true;
            if (defaultMovement)
            {
                if (target != null)
                {
                    Vector2 targetPos = target.Center + new Vector2(0, -250);
                    float targetRadius = 80f;
                    if (NPC.Center.Distance(targetPos) > targetRadius)
                    {
                        if (NPC.velocity.Length() < defaultMaxSpeed)
                        {
                            NPC.velocity += (targetPos - NPC.Center).SafeNormalize(Vector2.UnitY) * defaultAcceleration;
                        }
                    }
                    else
                    {
                        NPC.velocity *= 0.98f;
                    }

                    if (NPC.velocity.Length() > defaultMaxSpeed)
                    {
                        NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * defaultMaxSpeed;
                    }
                }
            }
            if (sineWaveVelocity)
            {
                NPC.velocity.Y += (float)Math.Cos(NPC.localAI[0] * 0.05f) * 0.033f;
            }
        }

        public void ChooseAttack()
        {
            NPC.ai[1] = 0;
            int chosenAttack = 0;

            List<Attack> potentialAttacks = new List<Attack>() { IceWave, Snowflake, Spin, IceRain, IceFog, Summon };
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
            chosenAttack = IceWave.Id;
            NPC.ai[0] = chosenAttack;
        }

        public override bool? CanFallThroughPlatforms()
        {
            return true;
        }
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            Rectangle targetBox = target.Hitbox;
            for (int i = 0; i < hitboxes.Count; i++)
            {
                if (!hitboxes[i].active)
                    continue;

                bool pass = targetBox.Intersects(hitboxes[i].GetHitbox(NPC.Center, NPC.rotation));
                if (pass)
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
            Rectangle targetBox = target.Hitbox;
            for (int i = 0; i < hitboxes.Count; i++)
            {
                if (!hitboxes[i].active)
                    continue;

                bool pass = targetBox.Intersects(hitboxes[i].GetHitbox(NPC.Center, NPC.rotation));
                if (pass)
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

            for (int i = 0; i < hitboxes.Count; i++)
            {
                if (!hitboxes[i].active)
                    continue;

                bool pass = projectile.Colliding(projectile.getRect(), hitboxes[i].GetHitbox(NPC.Center, NPC.rotation));
                if (pass)
                {
                    projectile.ModProj().ultimateCollideOverride = true;
                    return canBeHit ? null : false;
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

            modNPC.OverrideIgniteVisual = true;
            NPC.life = 1;
            NPC.immortal = true;
            NPC.dontTakeDamage = true;
            NPC.active = true;


            if (deadTime == 0)
            {
                NPC.velocity *= 0;
                NPC.rotation = 0;
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
            if (NPC.life > 0)
            {

            }
            else
            {
                
            }
        }
        public override void OnKill()
        {
            
        }
        public override void FindFrame(int frameHeight)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;

            NPC.frame = new Rectangle(0, currentFrame * frameHeight, tex.Width, frameHeight);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;
            Color color = Color.Lerp(drawColor, Color.White, 0.2f);

            Main.EntitySpriteDraw(tex, NPC.Center - Main.screenPosition, NPC.frame, color, NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
            int trailLength = (int)MathHelper.Clamp(NPC.velocity.Length() * 0.4f, 0, 3);
            for (int i = trailLength; i >= 0; i--)
            {
                Vector2 offset = (NPC.oldPosition - NPC.position).SafeNormalize(Vector2.UnitY) * 2 * i;
                Main.EntitySpriteDraw(glowTex, NPC.Center + offset - Main.screenPosition, NPC.frame, i == 0 ? Color.White : (Color.White * 0.4f), NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
            }
            


            bool drawHitboxes = false;
            if (drawHitboxes)
            {
                for (int i = 0; i < hitboxes.Count; i++)
                {
                    if (!hitboxes[i].active)
                        continue;

                    Rectangle hitbox = hitboxes[i].GetHitbox(NPC.Center, NPC.rotation);
                    for (int d = 0; d <= 1; d++)
                    {
                        for (int x = 0; x < hitbox.Width; x++)
                        {
                            Main.EntitySpriteDraw(squareTex, hitbox.Location.ToVector2() + new Vector2(x, hitbox.Height * d) - Main.screenPosition, null, Color.Red, 0, squareTex.Size() * 0.5f, 0.5f, SpriteEffects.None);
                        }
                        for (int y = 0; y < hitbox.Height; y++)
                        {
                            Main.EntitySpriteDraw(squareTex, hitbox.Location.ToVector2() + new Vector2(hitbox.Width * d, y) - Main.screenPosition, null, Color.Red, 0, squareTex.Size() * 0.5f, 0.5f, SpriteEffects.None);
                        }
                    }
                }
            }

            return false;
        }
    }
    public class ExtraHitbox
    {
        public Point dimensions;
        public Vector2 offset;
        public bool active;
        public ExtraHitbox(Point Dimensions, Vector2 Offset, bool Active = true)
        {
            dimensions = Dimensions;
            offset = Offset;
            active = Active;
        }
        public Rectangle GetHitbox(Vector2 origin, float rotation)
        {
            Point hitboxPos = (offset.RotatedBy(rotation) + origin).ToPoint() - new Point(dimensions.X / 2, dimensions.Y / 2);
            return new Rectangle(hitboxPos.X, hitboxPos.Y, dimensions.X, dimensions.Y);
        }
    }
}
