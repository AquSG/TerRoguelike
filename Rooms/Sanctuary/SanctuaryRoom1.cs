using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerRoguelike.Managers;
using TerRoguelike.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using TerRoguelike.World;
using static TerRoguelike.Schematics.SchematicManager;
using static TerRoguelike.Systems.MusicSystem;
using Terraria.Audio;
using Terraria.ModLoader;
using TerRoguelike.NPCs.Enemy;
using Microsoft.Xna.Framework.Graphics;
using static TerRoguelike.Managers.TextureManager;
using static TerRoguelike.Systems.RoomSystem;
using TerRoguelike.Utilities;
using TerRoguelike.TerPlayer;
using TerRoguelike.NPCs.Enemy.Boss;
using TerRoguelike.Items;
using TerRoguelike.Particles;
using Terraria.Utilities.Terraria.Utilities;
using Terraria.ModLoader.IO;
using Terraria.GameContent;
using Microsoft.CodeAnalysis;
using ReLogic.Utilities;
using TerRoguelike.Packets;
using static TerRoguelike.Packets.TeleportToPositionPacket;

namespace TerRoguelike.Rooms
{
    public class SanctuaryRoom1 : Room
    {
        public override int AssociatedFloor => FloorDict["Sanctuary"];
        public override string Key => "SanctuaryRoom1";
        public override string Filename => "Schematics/RoomSchematics/SanctuaryRoom1.csch";
        public override bool IsStartRoom => true;
        public override bool IsSanctuary => true;
        public override bool ActivateNewFloorEffects => false;
        public override bool AllowSettingPlayerCurrentRoom => true;
        public bool lunarGambitGranted = false;
        public SlotId lunarGambitSlot;
        public override void InitializeRoom()
        {
            if (!initialized)
            {
                for (int i = 0; i < TerRoguelikeWorld.itemBasins.Count; i++)
                {
                    var basin = TerRoguelikeWorld.itemBasins[i];
                    basin.itemDisplay = ItemManager.ChooseItemUnbiased((int)basin.tier);
                    if (Main.netMode == NetmodeID.SinglePlayer)
                        basin.GenerateItemOptions(Main.LocalPlayer);
                }
                BasinPacket.Send();
            }
            initialized = true;
        }
        public override void Update()
        {
            if (!awake)
                initialized = false;

            active = false;
            base.Update();
            awake = false;

            bool fadeSound = !lunarGambitGranted;
            if (!lunarGambitGranted)
            {
                foreach(Player player in Main.ActivePlayers)
                {
                    var modPlayer = player.ModPlayer();
                    if (modPlayer != null)
                    {
                        if (modPlayer.darkSanctuaryTime > 0)
                        {
                            fadeSound = false;

                            Vector2 itemPosition = RoomPosition16 + RoomCenter16 + new Vector2(-328, -48);
                            if (modPlayer.darkSanctuaryTime == 35)
                                lunarGambitSlot = SoundEngine.PlaySound(TerRoguelikeWorld.LunarGambitSpawn with { Volume = 1f }, itemPosition);

                            if (modPlayer.darkSanctuaryTime >= 180)
                            {
                                int i = Item.NewItem(Item.GetSource_None(), itemPosition, ModContent.ItemType<LunarGambit>());
                                Main.item[i].velocity = Vector2.Zero;

                                ParticleManager.AddParticle(new Glow(itemPosition, Vector2.Zero, 20, Color.Cyan * 0.40f, new Vector2(0.3f), 0, 0.96f, 20, true));
                                ParticleManager.AddParticle(new Glow(itemPosition, Vector2.Zero, 20, Color.White * 0.60f, new Vector2(0.15f), 0, 0.96f, 20, true));

                                lunarGambitGranted = true;
                            }
                            else
                            {
                                float completion = modPlayer.darkSanctuaryTime / 180f;
                                Vector2 randVect = Main.rand.NextVector2Circular(20, 20) * Main.rand.NextFloat(0.5f, 1f);
                                ParticleManager.AddParticle(new Ball(
                                    itemPosition + randVect * 2, -randVect * 0.2f, 20, 
                                    Color.Lerp(Color.White, Color.Cyan, Main.rand.NextFloat(0.3f, 1f)) * completion, 
                                    new Vector2(Main.rand.NextFloat(0.5f, 1f) * 0.12f), 
                                    0, 0.96f, 20, true));
                                ParticleManager.AddParticle(new Glow(itemPosition, Vector2.Zero, 20, Color.Cyan * 0.20f, new Vector2(0.19f) * (float)Math.Pow(completion, 2), 0, 0.96f, 20, true));
                                ParticleManager.AddParticle(new Glow(itemPosition, Vector2.Zero, 20, Color.White * 0.30f, new Vector2(0.1f) * (float)Math.Pow(completion, 3), 0, 0.96f, 20, true));

                                if (Main.rand.NextBool(3))
                                {
                                    int lifetime = 40;
                                    float scaleMulti = 1f * MathHelper.Lerp(0.25f, 1f, completion);

                                    Color color = Color.Lerp(Color.Cyan, Color.White, Main.rand.NextFloat());
                                    ParticleManager.AddParticle(new Smoke(
                                            itemPosition, Main.rand.NextVector2CircularEdge(1.3f, 1.3f) * Main.rand.NextFloat(0.6f, 0.86f) * scaleMulti, lifetime, color * 0.4f, new Vector2(0.18f) * scaleMulti,
                                            Main.rand.Next(15), Main.rand.NextFloat(MathHelper.TwoPi), Main.rand.NextBool() ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0.98f, 20));
                                }
                                
                            }
                            break;
                        }
                    }
                }
            }
            if (SoundEngine.TryGetActiveSound(lunarGambitSlot, out var sound) && sound.IsPlaying)
            {
                if (fadeSound)
                {
                    sound.Volume *= 0.8f;
                    sound.Pitch -= 0.05f;
                }
            }
        }
        public override bool CanDescend(Player player, TerRoguelikePlayer modPlayer)
        {
            modPlayer.noThrow = true;
            return !modPlayer.escaped && player.position.X + player.width >= ((RoomPosition.X + RoomDimensions.X) * 16f) - 22f && !player.dead;
        }
        public override Vector2 DescendTeleportPosition()
        {
            return RoomPosition16 + RoomDimensions16 * new Vector2(0.125f, 0.6f);
        }
        public override bool CanAscend(Player player, TerRoguelikePlayer modPlayer)
        {
            modPlayer.enableCampfire = true;
            return player.position.X <= (RoomPosition.X * 16f) + 22f && !player.dead && modPlayer.escaped;
        }
        public override void Ascend(Player player)
        {
            if (!TerRoguelike.mpClient)
            {
                var modPlayer = player.ModPlayer();
                var finalRoom = RoomID[FloorID[FloorDict["Surface"]].StartRoomID];
                player.Center = finalRoom.RoomPosition16 + finalRoom.RoomDimensions16 * new Vector2(0.5f, 0.66f);
                TeleportToPositionPacket.Send(player.Center, TeleportContext.TrueBrain, ID);
                finalRoom.AddBoss(finalRoom.bossSpawnPos, ModContent.NPCType<TrueBrain>());

                SetBossTrack(FinalBoss2Theme);
                ResetRoomID(finalRoom.ID);
                RoomPacket.Send(finalRoom.ID);


                NewFloorEffects(finalRoom, modPlayer);
            }
        }

        public override void PreResetRoom()
        {
            lunarGambitGranted = false;
        }

        public override void PostDrawTilesRoom()
        {
            base.PostDrawTilesRoom();
            if (!lunarGambitGranted)
            {
                foreach (Player player in Main.ActivePlayers)
                {
                    var modPlayer = player.ModPlayer();
                    if (modPlayer != null)
                    {
                        if (modPlayer.darkSanctuaryTime > 0 && modPlayer.darkSanctuaryTime < 180)
                        {
                            float completion = modPlayer.darkSanctuaryTime / 180f;
                            Vector2 itemPosition = RoomPosition16 + RoomCenter16 + new Vector2(-328, -48);
                            var tex = TextureAssets.Item[ModContent.ItemType<LunarGambit>()].Value;
                            Color color = Color.White * 0.3f;
                            color.A = 0;
                            for (int i = 0; i < 5; i++)
                            {
                                postDrawEverythingCache.Add(new(tex, itemPosition, null, color * completion, 0, tex.Size() * 0.5f, (float)Math.Pow((completion - 0.5f) * 2, 0.25f), SpriteEffects.None));
                            }
                            break;
                        }
                    }
                }
            }
        }
    }
}
