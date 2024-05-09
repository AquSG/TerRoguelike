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
using Terraria.GameContent;
using static TerRoguelike.Managers.TextureManager;

namespace TerRoguelike.Projectiles
{
    public class BoneSpear : ModProjectile, ILocalizedModType
    {
        public static readonly SoundStyle BoneSpearActivate = new("TerRoguelike/Sounds/BoneSpear");
        public static readonly SoundStyle BoneSpearAppear = new("TerRoguelike/Sounds/BoneAppear");
        public static readonly SoundStyle BoneSpearNoise = new("TerRoguelike/Sounds/BoneNoise");
        public int maxTimeLeft;
        public Texture2D tipTex;
        public Texture2D squareTex;
        public float currentLength = 0;
        public Vector2 endPosition;
        public float maxLength = 300;
        public int telegraphDuration = 45;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 1600;
        }
        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = maxTimeLeft = 390;
            Projectile.penetrate = -1;
            Projectile.hide = true;
            Projectile.ModProj().killOnRoomClear = true;
            tipTex = TexDict["BoneSpearTip"].Value;
            squareTex = TexDict["Square"].Value;
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.velocity = Vector2.Zero;
            endPosition = TerRoguelikeUtils.TileCollidePositionInLine(Projectile.Center, Projectile.Center + Projectile.rotation.ToRotationVector2() * maxLength);
            endPosition -= Projectile.rotation.ToRotationVector2() * 12;
            maxLength = Projectile.Center.Distance(endPosition);

            //SoundEngine.PlaySound(BoneSpearAppear with { Volume = 0.25f, MaxInstances = 3 }, Projectile.Center + Projectile.rotation.ToRotationVector2() * 80);
        }
        public override void AI()
        {
            int time = maxTimeLeft - Projectile.timeLeft;
            if (time < telegraphDuration)
            {
                if (time % 6 == 0)
                {
                    SoundEngine.PlaySound(BoneSpearNoise with { Volume = 0.3f, SoundLimitBehavior = SoundLimitBehavior.IgnoreNew }, Projectile.Center + Projectile.rotation.ToRotationVector2() * 80);
                }
            }
            else
            {
                if (time == telegraphDuration)
                    SoundEngine.PlaySound(BoneSpearActivate with { Volume = 0.6f, MaxInstances = 6 }, Projectile.Center + Projectile.rotation.ToRotationVector2() * 80);

                if (Projectile.timeLeft > 30)
                {
                    if (currentLength < maxLength)
                        currentLength += 20f;
                    if (currentLength > maxLength)
                        currentLength = maxLength;
                }
                else
                {
                    if (currentLength > 0)
                        currentLength -= 25f;
                    if (currentLength < 0)
                        currentLength = 0;
                }
            }
        }
        public override void OnKill(int timeLeft)
        {
            
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (maxTimeLeft - Projectile.timeLeft < telegraphDuration || currentLength == 0)
                return false;

            float radius = 5;
            for (int i = 5; i < currentLength; i += 10)
            {
                Vector2 checkPos = Projectile.Center + Projectile.rotation.ToRotationVector2() * i;
                if (targetHitbox.ClosestPointInRect(checkPos).Distance(checkPos) <= radius)
                    return true;
            }

            return false;
        }
        public override bool? CanDamage()
        {
            var modProj = Projectile.ModProj();
            if (modProj.npcOwner >= 0)
            {
                NPC npc = Main.npc[modProj.npcOwner];
                var modNPC = npc.ModNPC();
                if (modNPC.isRoomNPC)
                {
                    if (RoomSystem.RoomList[modNPC.sourceRoomListID].bossDead)
                        return false;
                }
            }

            return null;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            int time = maxTimeLeft - Projectile.timeLeft;
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            int maxFrameWidth = tex.Width;
            Vector2 rotVect = Projectile.rotation.ToRotationVector2();

            Vector2 currentEndPos = Projectile.Center + rotVect * currentLength;
            float currentLengthExtend = currentLength + 8;

            if (time < telegraphDuration)
            {
                Color telegraphColor = time % 10 < 5 ? Color.Red : Color.Orange;
                int vertiOffset = 6;

                Main.EntitySpriteDraw(squareTex, endPosition - Main.screenPosition, null, telegraphColor, Projectile.rotation, squareTex.Size() * 0.5f, new Vector2(0.5f, 3f), SpriteEffects.None);
                Main.EntitySpriteDraw(squareTex, Projectile.Center + (-rotVect * 8) - Main.screenPosition, null, telegraphColor, Projectile.rotation, squareTex.Size() * 0.5f, new Vector2(0.5f, 3f), SpriteEffects.None);
                for (int j = -1; j <= 1; j += 2)
                {
                    Main.EntitySpriteDraw(squareTex, endPosition + new Vector2(0, j * vertiOffset).RotatedBy(Projectile.rotation) - Main.screenPosition, null, telegraphColor, Projectile.rotation, new Vector2(squareTex.Width, squareTex.Height * 0.5f), new Vector2((maxLength + 8) * 0.25f, 0.5f), SpriteEffects.None);
                }

                return false;
            }

            if (currentLength <= 0)
                return false;

            int i = 0;
            while (i < currentLengthExtend)
            {
                int frameLength = (int)MathHelper.Clamp(currentLengthExtend - i, 1, 10);
                Rectangle boneFrame = new Rectangle(i % maxFrameWidth, 0, frameLength, tex.Height);
                Vector2 drawPos = currentEndPos - rotVect * i;
                Main.EntitySpriteDraw(tex, drawPos - Main.screenPosition, boneFrame, Lighting.GetColor(drawPos.ToTileCoordinates()), Projectile.rotation, new Vector2(frameLength, tex.Height * 0.5f), 1f, SpriteEffects.None);

                i += frameLength;
            }
            Main.EntitySpriteDraw(tipTex, currentEndPos - Main.screenPosition, null, Lighting.GetColor(currentEndPos.ToTileCoordinates()), Projectile.rotation, new Vector2(0, tipTex.Height * 0.5f), 1f, SpriteEffects.None);

            return false;
        }
    }
}
