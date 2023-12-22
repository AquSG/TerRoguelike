using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TerRoguelike;
using TerRoguelike.TerPlayer;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.GameContent;
using TerRoguelike.NPCs;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using TerRoguelike.Projectiles;
using Terraria.Graphics.Shaders;

namespace TerRoguelike.NPCs.Enemy
{
    public class SandWorm : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<SandWorm>();
        public override List<int> associatedFloors => new List<int>() { 2 };
        public override Vector2 DrawCenterOffset => new Vector2(0, -6);
        public override int CombatStyle => 0;
        public List<WormSegment> Segments = new List<WormSegment>();
        public Texture2D headTex = ModContent.Request<Texture2D>("TerRoguelike/NPCs/Enemy/SandWormHead").Value;
        public Texture2D bodyTex = ModContent.Request<Texture2D>("TerRoguelike/NPCs/Enemy/SandWormBody").Value;
        public Texture2D tailTex = ModContent.Request<Texture2D>("TerRoguelike/NPCs/Enemy/SandWormTail").Value;
        public bool CollisionPass = false;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 1;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 34;
            NPC.height = 34;
            NPC.aiStyle = -1;
            NPC.damage = 35;
            NPC.lifeMax = 1000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.behindTiles = true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.velocity = Vector2.UnitY * 2f;
            int segCount = 10;
            for (int i = 0; i < segCount; i++)
            {
                Segments.Add(new WormSegment(NPC.Center + (Vector2.UnitY * NPC.height * i), MathHelper.PiOver2 * 3f, NPC.height));
            }
            modNPC.OverrideIgniteVisual = true;
        }
        public override void AI()
        {
            modNPC.RogueWormAI(NPC, 16f, MathHelper.Pi / 60f, 480);
            NPC.rotation = NPC.velocity.ToRotation();
            modNPC.UpdateWormSegments(ref Segments, NPC);
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
            {
                for (int i = 0; i < Segments.Count; i++)
                {
                    WormSegment segment = Segments[i];
                    if (i == 0)
                    {
                        Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, GoreID.DuneSplicerHead, NPC.scale);
                        continue;
                    }
                    if (i == Segments.Count - 1)
                    {
                        Gore.NewGore(NPC.GetSource_Death(), segment.Position, segment.Position - segment.OldPosition, GoreID.DuneSplicerTail, NPC.scale);
                        continue;
                    }
                    Gore.NewGore(NPC.GetSource_Death(), segment.Position, segment.Position - segment.OldPosition, GoreID.DuneSplicerBody, NPC.scale);
                }
            }
            
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (modNPC.OverrideIgniteVisual && modNPC.ignitedStacks.Any())
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                for (int i = Segments.Count - 1; i >= 0; i--)
                {
                    Texture2D texture;
                    WormSegment segment = Segments[i];
                    if (i == 0)
                        texture = headTex;
                    else if (i == Segments.Count - 1)
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
            for (int i = Segments.Count - 1; i >= 0; i--)
            {
                Texture2D texture;
                WormSegment segment = Segments[i];
                if (i == 0)
                    texture = headTex;
                else if (i == Segments.Count - 1)
                    texture = tailTex;
                else
                    texture = bodyTex;

                Color color = modNPC.ignitedStacks.Any() ? Color.Lerp(Color.White, Color.OrangeRed, 0.4f) : Lighting.GetColor(new Point((int)(segment.Position.X / 16), (int)(segment.Position.Y / 16)));
                spriteBatch.Draw(texture, segment.Position - screenPos, null, color, segment.Rotation + MathHelper.PiOver2, headTex.Size() * 0.5f, 1f, SpriteEffects.None, 0);
            }
            return false;
        }
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            float radius = NPC.height < NPC.width ? NPC.height / 2 : NPC.width / 2;
            for (int i = 0; i < Segments.Count; i++)
            {
                WormSegment segment = Segments[i];
                if (segment.Position.Distance(target.getRect().ClosestPointInRect(segment.Position)) <= radius)
                {
                    CollisionPass = true;
                    return true;
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
            float radius = NPC.height < NPC.width ? NPC.height / 2 : NPC.width / 2;
            for (int i = 0; i < Segments.Count; i++)
            {
                WormSegment segment = Segments[i];
                if (projectile.Colliding(projectile.getRect(), new Rectangle((int)(segment.Position.X - (NPC.width / 2)), (int)(segment.Position.Y - (NPC.height / 2)), NPC.width, NPC.height)))
                {
                    projectile.GetGlobalProjectile<TerRoguelikeGlobalProjectile>().ultimateCollideOverride = true;
                    return null;
                }
            }
            return false;
        }
    }
}
