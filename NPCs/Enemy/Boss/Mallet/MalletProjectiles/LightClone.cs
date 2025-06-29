using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerRoguelike.Managers;
using TerRoguelike.Systems;
using TerRoguelike.TerPlayer;
using Microsoft.Xna.Framework.Graphics;
using TerRoguelike.Items.Common;
using TerRoguelike.Utilities;
using Terraria.DataStructures;
using TerRoguelike.Particles;
using static TerRoguelike.Managers.TextureManager;
using Steamworks;
using ReLogic.Utilities;
using Microsoft.Build.Construction;
using System.Security.Policy;
using System.IO;

namespace TerRoguelike.NPCs.Enemy.Boss.Mallet.MalletProjectiles
{
    public class LightClone : ModProjectile, ILocalizedModType
    {
        public int maxTimeLeft;
        public ref float zDepth => ref Projectile.ai[0];
        public float oldZDepth = 1;
        public int targetPlayer = -1;
        public int targetNPC = -1;
        public Vector2 spawnPos = Vector2.Zero;
        public Vector2 defaultPos = Vector2.Zero;
        public Vector2 fakePos
        {
            get
            {
                return new Vector2(Projectile.localAI[0], Projectile.localAI[1]);
            }
            set
            {
                Projectile.localAI[0] = value.X;
                Projectile.localAI[1] = value.Y;
            }
        }

        Entity target
        {
            get
            {
                if (targetPlayer >= 0)
                {
                    return Main.player[targetPlayer];
                }
                else if (targetNPC >= 0)
                {
                    return Main.npc[targetNPC];
                }
                else
                    return null;
            }
        }
        public override string Texture => "TerRoguelike/Projectiles/InvisibleProj";
        public Texture2D CloneTexture = null;
        public List<ExtraHitbox> hitboxes = [];
        public SlotId DashSlot;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;
        }
        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = maxTimeLeft = 90;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.netImportant = true; //makes projectile not able to be replaced if at the projectile cap 
            CloneTexture = TexDict["LightClone"];
        }
        public override void OnSpawn(IEntitySource source)
        {
            spawnPos = Projectile.Center;
            defaultPos = Projectile.velocity;
            Projectile.velocity = Vector2.Zero;
            if (source is EntitySource_Parent parentSource)
            {
                if (parentSource.Entity is NPC)
                {
                    NPC parent = Main.npc[parentSource.Entity.whoAmI];
                    var modparent = parent.ModNPC();
                    if (modparent != null)
                    {
                        targetNPC = modparent.targetNPC;
                        targetPlayer = modparent.targetPlayer;
                    }
                }
            }

            fakePos = Projectile.Center;
            OOBCheck();

            Projectile.localAI[2]++;
            DashSlot = SoundEngine.PlaySound(Mallet.BackgroundDash with { Volume = 0.5f, Variants = [2], MaxInstances = 3, Pitch = -0.25f }, (fakePos - Main.Camera.Center) * zDepth + Main.Camera.Center);
            Vector2 targetPos = target == null || Projectile.ai[1] == 1 ? defaultPos : target.Center;
            for (int i = 0; i < 10; i++)
            {
                ParticleManager.AddParticle(new ThinSpark(targetPos, Vector2.Zero, 20, Color.Yellow, new Vector2(0.35f, 0.5f) * 1, MathHelper.PiOver2 * i, true, false));
            }
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            if (zDepth < 1)
            {
                Projectile.hide = true;
                behindNPCsAndTiles.Add(index);
            }
            else if (zDepth == 1)
            {
                Projectile.hide = false;
            }
            else
            {
                Projectile.hide = true;
                overPlayers.Add(index);
            }
        }
        public override bool PreAI()
        {
            hitboxes = new List<ExtraHitbox>()
                    {
                        new ExtraHitbox(new Point(80, 80), new Vector2(0)),
                        new ExtraHitbox(new Point(80, 80), new Vector2(30, 10), true, false),
                        new ExtraHitbox(new Point(60, 60), new Vector2(20, 55), true, false),
                        new ExtraHitbox(new Point(60, 60), new Vector2(-60, 30), true, false),
                        new ExtraHitbox(new Point(60, 60), new Vector2(80, 30), true, false),
                        new ExtraHitbox(new Point(80, 80), new Vector2(-120, 30), true, false),
                        new ExtraHitbox(new Point(80, 80), new Vector2(140, 30), true, false),
                        new ExtraHitbox(new Point(80, 80), new Vector2(-180, 30), true, false),
                        new ExtraHitbox(new Point(80, 80), new Vector2(200, 34), true, false),
                        new ExtraHitbox(new Point(80, 80), new Vector2(-240, 20), true, false),
                        new ExtraHitbox(new Point(80, 80), new Vector2(260, 20), true, false),
                        new ExtraHitbox(new Point(60, 60), new Vector2(-280, 10), true, false),
                        new ExtraHitbox(new Point(60, 60), new Vector2(300, 10), true, false),
                    };

            if (target != null)
            {
                if (targetPlayer >= 0)
                {
                    var player = Main.player[targetPlayer];
                    if (!player.active || player.dead)
                        targetPlayer = -1;
                }
                else if (targetNPC >= 0)
                {
                    var npc = Main.npc[targetNPC];
                    if (!npc.active)
                        targetNPC = -1;
                }
            }
            return true;
        }
        public override void AI()
        {
            oldZDepth = zDepth;
            Vector2 targetPos = target == null || Projectile.ai[1] == 1 ? defaultPos : target.Center;

            Projectile.velocity = Vector2.Zero;
            zDepth *= 1.1f;
            
            fakePos += (targetPos - fakePos) * 10f / 66;

            if (zDepth > 1 && oldZDepth >= 1)
                fakePos += Vector2.UnitY * -1.5f * (zDepth - 1);

            if (Projectile.localAI[2] == 0)
            {
                Projectile.localAI[2]++;
                DashSlot = SoundEngine.PlaySound(Mallet.BackgroundDash with { Volume = 1f, Variants = [2] }, (fakePos - Main.Camera.Center) * zDepth + Main.Camera.Center);
                for (int i = 0; i < 2; i++)
                {
                    ParticleManager.AddParticle(new ThinSpark(targetPos, Vector2.Zero, 30, Color.Yellow, new Vector2(0.35f, 0.5f) * 10, MathHelper.PiOver2 * i, true, false));
                }
                
            }

            OOBCheck();

            if (SoundEngine.TryGetActiveSound(DashSlot, out var sound))
            {
                sound.Position = (fakePos - Main.Camera.Center) * zDepth + Main.Camera.Center;
            }
        }
        public void OOBCheck() // projectile is effectively super far away and is only visible because of zdepth. if it spawns above the world the projectile just dies immediately.
        {
            Projectile.Center = fakePos;
            if (Projectile.position.X <= Main.leftWorld || Projectile.position.X + (float)Projectile.width >= Main.rightWorld || Projectile.position.Y <= Main.topWorld || Projectile.position.Y + (float)Projectile.height >= Main.bottomWorld)
                Projectile.Center = defaultPos;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            for (int i = 0; i < hitboxes.Count; i++)
            {
                if (!hitboxes[i].active)
                    continue;

                bool pass = targetHitbox.Intersects(hitboxes[i].GetHitbox(fakePos, Projectile.rotation, Projectile.scale));
                if (pass)
                {
                    return true;
                }
            }
            return false;
        }
        public override bool CanHitPlayer(Player target)
        {
            Rectangle targetHitbox = target.Hitbox;
            for (int i = 0; i < hitboxes.Count; i++)
            {
                if (!hitboxes[i].active)
                    continue;

                bool pass = targetHitbox.Intersects(hitboxes[i].GetHitbox(fakePos, Projectile.rotation, Projectile.scale));
                if (pass)
                {
                    target.AddBuff(ModContent.BuffType<Retribution>(), 20);
                    return true;
                }
            }
            return false;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Color npcColor = Color.Yellow * 0.75f;
            npcColor.A = 100;
            if (zDepth > 1)
            {
                npcColor *= 1 - ((zDepth - 1) * 0.14f);
            }
            float scale = Projectile.scale * zDepth;
            Vector2 drawPos = (fakePos - Main.Camera.Center) * zDepth + Main.Camera.Center;

            int frameCounter = 7;
            var bgFlyFrame = CloneTexture.Frame(2, 4, frameCounter / 4, frameCounter % 4);

            Main.EntitySpriteDraw(CloneTexture, drawPos - Main.screenPosition, bgFlyFrame, npcColor, Projectile.rotation, bgFlyFrame.Size() * 0.5f - new Vector2(40, 0), scale, SpriteEffects.None);

            return false;
        }
        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            Mallet.InflictRetribution(target);
        }
        public override bool? CanDamage() => (zDepth >= 1 && oldZDepth < 1) ? null : false;
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.WriteVector2(fakePos);
            writer.WriteVector2(defaultPos);
            writer.WriteVector2(spawnPos);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            fakePos = reader.ReadVector2();
            defaultPos = reader.ReadVector2();
            spawnPos = reader.ReadVector2();
        }
    }
}
