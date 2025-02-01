using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerRoguelike.Managers;
using TerRoguelike.NPCs;
using Terraria.Chat;
using TerRoguelike.Systems;
using Microsoft.Xna.Framework.Graphics;
using TerRoguelike.Schematics;
using TerRoguelike.World;
using TerRoguelike.Projectiles;
using TerRoguelike.Utilities;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using TerRoguelike.Particles;
using Terraria.Audio;
using Steamworks;

namespace TerRoguelike.Managers
{
    public class Floor
    {
        public int ID = -1;
        public virtual int StartRoomID => -1;
        public virtual List<int> BossRoomIDs => new List<int>();
        public virtual int Stage => -1;
        public virtual bool InHell => false;
        public virtual string Name => "";
        public virtual FloorSoundtrack Soundtrack => MusicSystem.BaseTheme;

        #region Jstc
        public int jstc = 0;
        public int targetJstc = 0;
        public JstcProgress jstcProgress = JstcProgress.Start;
        public enum JstcProgress
        {
            Start = 0,
            Enemies = 1,
            EnemyPortal = 2,
            Boss = 3,
            BossDeath = 4,
            BossPortal = 5,
            Jstc = 6,
        }
        public void jstcUpdate()
        {
            if (!TerRoguelikeWorld.escape)
                return;

            if (jstcProgress == JstcProgress.Start)
            {
                bool allow = true;

                int start = SchematicManager.RoomID[StartRoomID].myRoom;
                int count = jstc;
                int boss = 0;
                for (int i = start; i < RoomSystem.RoomList.Count; i++)
                {
                    Room room = RoomSystem.RoomList[i];
                    if (room.IsBossRoom)
                    {
                        boss = i;
                        break;
                    }
                    if (room.IsStartRoom || room.TransitionDirection != -1) continue;

                    if (!room.awake)
                    {
                        allow = false;
                    }
                    for (int n = 0; n < room.NotSpawned.Length; n++)
                    {
                        if (room.NotSpawned[n] && room.AssociatedWave[n] == 0)
                            count++;
                    }
                }
                for (int i = 0; i < SpawnManager.pendingEnemies.Count; i++)
                {
                    var enemy = SpawnManager.pendingEnemies[i];
                    if (enemy.RoomListID < 0) continue;
                    if (RoomSystem.RoomList[enemy.RoomListID].AssociatedFloor == ID)
                    {
                        count++;
                    }
                }
                foreach (NPC npc in Main.ActiveNPCs)
                {
                    var modNPC = npc.ModNPC();
                    if (modNPC == null || !modNPC.isRoomNPC || modNPC.sourceRoomListID < 0) continue;
                    if (RoomSystem.RoomList[modNPC.sourceRoomListID].AssociatedFloor == ID)
                    {
                        count++;
                    }
                }

                if (count > targetJstc)
                    targetJstc = count;

                if (allow)
                {
                    if (jstc >= targetJstc)
                    {
                        int furthest = -1;
                        foreach (Player player in Main.ActivePlayers)
                        {
                            Rectangle pRect = player.getRect();
                            for (int i = boss; i >= SchematicManager.RoomID[StartRoomID].myRoom; i--)
                            {
                                Room room = RoomSystem.RoomList[i];
                                if (room.GetRect().Intersects(pRect))
                                {
                                    if (i > furthest)
                                        furthest = i;
                                    break;
                                }
                            }
                            if (furthest == -1 || furthest == boss)
                            {
                                allow = false;
                            }
                        }
                        if (allow)
                        {
                            Room targetRoom = RoomSystem.RoomList[furthest];
                            Room nextRoom = RoomSystem.RoomList[furthest + 1];
                            int doorDir = 0;
                            bool down = nextRoom.Key.Contains("Down");
                            bool up = nextRoom.Key.Contains("Up");
                            if (!up && !down)
                                doorDir = 0;
                            else if (down)
                                doorDir = 1;
                            else if (up)
                                doorDir = 2;

                            allow = false;
                            Point checkStart;
                            Point airStart;
                            switch (doorDir)
                            {
                                default:
                                case 0:
                                    checkStart = targetRoom.RoomPosition.ToPoint() + new Point((int)targetRoom.RoomDimensions.X - 1, 0);
                                    airStart = new Point(-1, -1);

                                    for (int i = 0; i < targetRoom.RoomDimensions.Y; i++)
                                    {
                                        Point checkTile = checkStart + new Point(0, i);
                                        if (airStart == new Point(-1, -1))
                                        {
                                            if (!ParanoidTileRetrieval(checkTile).IsTileSolidGround(true))
                                            {
                                                airStart = checkTile;
                                            }
                                        }
                                        else
                                        {
                                            if (ParanoidTileRetrieval(checkTile).IsTileSolidGround(true))
                                            {
                                                Vector2 doorTop = airStart.ToWorldCoordinates(8, 0);
                                                Vector2 doorBottom = checkTile.ToWorldCoordinates(8, 0);
                                                TerRoguelikeWorld.jstcPortalPos = Vector2.Lerp(doorTop, doorBottom, 0.5f);
                                                float doorHeight = doorBottom.Y - doorTop.Y;
                                                doorHeight = Math.Max(doorHeight, 96);
                                                TerRoguelikeWorld.jstcPortalScale = new Vector2(doorHeight * 0.2f, doorHeight);
                                                TerRoguelikeWorld.jstcPortalRot = 0;
                                                allow = true;
                                                break;
                                            }
                                        }
                                        
                                    }
                                    break;
                                case 1:
                                case 2:
                                    checkStart = targetRoom.RoomPosition.ToPoint() + new Point(0, doorDir == 1 ? ((int)targetRoom.RoomDimensions.Y - 1) : 0);
                                    airStart = new Point(-1, -1);

                                    for (int i = 0; i < targetRoom.RoomDimensions.X; i++)
                                    {
                                        Point checkTile = checkStart + new Point(i, 0);
                                        if (airStart == new Point(-1, -1))
                                        {
                                            if (!ParanoidTileRetrieval(checkTile).IsTileSolidGround(true))
                                            {
                                                airStart = checkTile;
                                            }
                                        }
                                        else
                                        {
                                            if (ParanoidTileRetrieval(checkTile).IsTileSolidGround(true))
                                            {
                                                Vector2 doorLeft = airStart.ToWorldCoordinates(0, 8);
                                                Vector2 doorRight = checkTile.ToWorldCoordinates(0, 8);
                                                TerRoguelikeWorld.jstcPortalPos = Vector2.Lerp(doorLeft, doorRight, 0.5f);
                                                float doorHeight = doorRight.X - doorLeft.X;
                                                doorHeight = Math.Max(doorHeight, 96);
                                                TerRoguelikeWorld.jstcPortalScale = new Vector2(doorHeight, doorHeight * 0.2f);
                                                TerRoguelikeWorld.jstcPortalRot = doorDir == 1 ? MathHelper.PiOver2 : -MathHelper.PiOver2;
                                                allow = true;
                                                break;
                                            }
                                        }
                                    }
                                    break;
                            }
                            if (allow)
                            {
                                TerRoguelikeWorld.jstcPortalTime = 1;
                                jstcProgress = JstcProgress.Enemies;
                                SoundEngine.PlaySound(TerRoguelikeWorld.JstcSpawn with { Volume = 1f, Pitch = -0.2f }, TerRoguelikeWorld.jstcPortalPos);
                            }
                        }
                    }
                }
            }
            if (jstcProgress == JstcProgress.Enemies)
            {
                Vector2 portalScale = TerRoguelikeWorld.jstcPortalScale;
                bool portalRot = portalScale.X > portalScale.Y;
                Vector2 portalPos = TerRoguelikeWorld.jstcPortalPos;
                Vector2 portalDimensions = new Vector2(14, portalRot ? portalScale.X : portalScale.Y);
                if (portalRot)
                    portalDimensions = new Vector2(portalDimensions.Y, portalDimensions.X);

                Point portalTopLeft = (portalPos - (portalDimensions * 0.5f)).ToPoint();
                Rectangle portalRect = portalRot ? new Rectangle(portalTopLeft.X, portalTopLeft.Y, (int)portalDimensions.X, (int)portalDimensions.Y) : new Rectangle(portalTopLeft.X, portalTopLeft.Y, 100, (int)portalDimensions.Y) ;

                foreach (Player player in Main.ActivePlayers)
                {
                    if (player.getRect().Intersects(portalRect))
                    {
                        int boss = 0;
                        for (int i = SchematicManager.RoomID[StartRoomID].myRoom; i < RoomSystem.RoomList.Count; i++)
                        {
                            if (RoomSystem.RoomList[i].IsBossRoom)
                            {
                                boss = i;
                                break;
                            }
                        }
                        Room bossRoom = RoomSystem.RoomList[boss];
                        int doorDir = bossRoom.HasTransition ? RoomSystem.RoomList[boss - 1].TransitionDirection : 0;

                        Point startTile;
                        Point airTile = new Point(-1, -1);
                        Vector2 finalTargetPos = Vector2.Zero;
                        switch (doorDir)
                        {
                            default:
                            case 0:
                                startTile = bossRoom.RoomPosition.ToPoint();
                                for (int i = 0; i < bossRoom.RoomDimensions.Y; i++)
                                {
                                    Point checkTile = startTile + new Point(0, i);
                                    if (airTile == new Point(-1, -1))
                                    {
                                        if (!ParanoidTileRetrieval(checkTile).IsTileSolidGround(true))
                                        {
                                            airTile = checkTile;
                                        }
                                    }
                                    else
                                    {
                                        if (ParanoidTileRetrieval(checkTile).IsTileSolidGround(true))
                                        {
                                            Vector2 topPos = airTile.ToWorldCoordinates(8, 0);
                                            Vector2 bottomPos = checkTile.ToWorldCoordinates(8, 0);
                                            finalTargetPos = Vector2.Lerp(topPos, bottomPos, 0.5f);
                                            finalTargetPos = TileCollidePositionInLine(finalTargetPos, finalTargetPos + Vector2.UnitX * 100);
                                            finalTargetPos = TileCollidePositionInLine(finalTargetPos, finalTargetPos + Vector2.UnitY * 20);
                                            break;
                                        }
                                    }
                                }
                                break;
                            case 1:
                            case 2:
                                startTile = bossRoom.RoomPosition.ToPoint() + new Point(0, doorDir == 2 ? (int)bossRoom.RoomDimensions.Y - 1 : 0);
                                for (int i = 0; i < bossRoom.RoomDimensions.X; i++)
                                {
                                    Point checkTile = startTile + new Point(i, 0);
                                    if (airTile == new Point(-1, -1))
                                    {
                                        if (!ParanoidTileRetrieval(checkTile).IsTileSolidGround(true))
                                        {
                                            airTile = checkTile;
                                        }
                                    }
                                    else
                                    {
                                        if (ParanoidTileRetrieval(checkTile).IsTileSolidGround(true))
                                        {
                                            Vector2 leftPos = airTile.ToWorldCoordinates(0, 8);
                                            Vector2 RightPos = checkTile.ToWorldCoordinates(0, 8);
                                            finalTargetPos = Vector2.Lerp(leftPos, RightPos, 0.5f);
                                            if (doorDir == 1)
                                            {
                                                finalTargetPos += Vector2.UnitY * 68;
                                            }
                                            else
                                            {
                                                finalTargetPos -= Vector2.UnitY * 16;
                                            }
                                            break;
                                        }
                                    }
                                }
                                break;
                        }

                        foreach (Player player2 in Main.ActivePlayers)
                        {
                            player2.velocity = Vector2.Zero;
                            var modPlayer = player2.ModPlayer();
                            modPlayer.jstcTeleportTime = 1;
                            modPlayer.jstcTeleportStart = player2.Center;
                            modPlayer.jstcTeleportEnd = finalTargetPos - (Vector2.UnitY * player2.height);
                        }
                        jstcProgress = JstcProgress.EnemyPortal;
                        TerRoguelikeWorld.jstcPortalTime = -60;
                        CutsceneSystem.SetCutscene(player.Center, 180, 60, 30, 1.5f);
                        SoundEngine.PlaySound(TerRoguelikeWorld.WorldTeleport with { Volume = 0.2f, Variants = [2], Pitch = -0.25f });
                        break;
                    }
                }
            }
            if (jstcProgress == JstcProgress.EnemyPortal)
            {
                CutsceneSystem.cameraTargetCenter = Vector2.Lerp(Main.LocalPlayer.Center + Vector2.UnitY * Main.LocalPlayer.gfxOffY, Main.Camera.Center, 0.6f);
                bool end = false;
                Main.SetCameraLerp(0, 0);
                foreach (Player player in Main.ActivePlayers)
                {
                    var modPlayer = player.ModPlayer();
                    if (modPlayer == null) continue;

                    float completion = modPlayer.jstcTeleportTime / 180f;
                    float interpolant = MathHelper.Hermite(0, 0f, 1, 0f, completion);
                    player.Center = Vector2.Lerp(modPlayer.jstcTeleportStart, modPlayer.jstcTeleportEnd, interpolant);

                    SpawnParticles(player);

                    if (modPlayer.jstcTeleportTime >= 180)
                    {
                        end = true;
                    }
                    modPlayer.jstcTeleportTime++;
                }
                if (end)
                {
                    foreach (Player player in Main.ActivePlayers)
                    {
                        player.velocity = player.position - player.oldPosition;

                        var modPlayer = player.ModPlayer();
                        if (modPlayer == null) continue;

                        player.Center = modPlayer.jstcTeleportEnd;
                        modPlayer.jstcTeleportTime = 0;
                    }
                    jstcProgress = JstcProgress.Boss;
                }
            }
            if (jstcProgress == JstcProgress.Boss)
            {
                foreach(Player player in Main.ActivePlayers)
                {
                    var modPlayer = player.ModPlayer();
                    if (modPlayer == null || modPlayer.currentRoom == -1) continue;

                    Room room = RoomSystem.RoomList[modPlayer.currentRoom];

                    if (room.IsBossRoom && room.bossDead)
                    {
                        int doorDir = room.HasTransition ? RoomSystem.RoomList[room.myRoom - 1].TransitionDirection : 0;

                        bool allow = false;
                        Point startTile;
                        Point airTile = new Point(-1, -1);
                        switch (doorDir)
                        {
                            default:
                            case 0:
                                startTile = room.RoomPosition.ToPoint();
                                for (int i = 0; i < room.RoomDimensions.Y; i++)
                                {
                                    Point checkTile = startTile + new Point(0, i);
                                    if (airTile == new Point(-1, -1))
                                    {
                                        if (!ParanoidTileRetrieval(checkTile).IsTileSolidGround(true))
                                        {
                                            airTile = checkTile;
                                        }
                                    }
                                    else
                                    {
                                        if (ParanoidTileRetrieval(checkTile).IsTileSolidGround(true))
                                        {
                                            Vector2 doorTop = airTile.ToWorldCoordinates(8, 0);
                                            Vector2 doorBottom = checkTile.ToWorldCoordinates(8, 0);
                                            TerRoguelikeWorld.jstcPortalPos = Vector2.Lerp(doorTop, doorBottom, 0.5f);
                                            float doorHeight = doorBottom.Y - doorTop.Y;
                                            doorHeight = Math.Max(doorHeight, 96);
                                            TerRoguelikeWorld.jstcPortalScale = new Vector2(doorHeight * 0.2f, doorHeight);
                                            TerRoguelikeWorld.jstcPortalRot = MathHelper.Pi;
                                            allow = true;
                                            break;
                                        }
                                    }
                                }
                                break;
                            case 1:
                            case 2:
                                startTile = room.RoomPosition.ToPoint() + new Point(0, doorDir == 2 ? (int)room.RoomDimensions.Y - 1 : 0);
                                for (int i = 0; i < room.RoomDimensions.X; i++)
                                {
                                    Point checkTile = startTile + new Point(i, 0);
                                    if (airTile == new Point(-1, -1))
                                    {
                                        if (!ParanoidTileRetrieval(checkTile).IsTileSolidGround(true))
                                        {
                                            airTile = checkTile;
                                        }
                                    }
                                    else
                                    {
                                        if (ParanoidTileRetrieval(checkTile).IsTileSolidGround(true))
                                        {
                                            Vector2 leftPos = airTile.ToWorldCoordinates(0, 8);
                                            Vector2 RightPos = checkTile.ToWorldCoordinates(0, 8);
                                            TerRoguelikeWorld.jstcPortalPos = Vector2.Lerp(leftPos, RightPos, 0.5f);
                                            float doorHeight = RightPos.X - leftPos.X;
                                            doorHeight = Math.Max(doorHeight, 96);
                                            TerRoguelikeWorld.jstcPortalScale = new Vector2(doorHeight, doorHeight * 0.2f);
                                            TerRoguelikeWorld.jstcPortalRot = doorDir == 2 ? MathHelper.PiOver2 : -MathHelper.PiOver2;
                                            allow = true;
                                            break;
                                        }
                                    }
                                }
                                break;
                        }

                        if (allow)
                        {
                            TerRoguelikeWorld.jstcPortalTime = 1;
                            jstcProgress = JstcProgress.BossDeath;
                            SoundEngine.PlaySound(TerRoguelikeWorld.JstcSpawn with { Volume = 1f, Pitch = -0.2f }, TerRoguelikeWorld.jstcPortalPos);
                            for (int f = Stage; f < 5; f++)
                            {
                                if (SchematicManager.FloorID[RoomManager.FloorIDsInPlay[f]].jstcProgress < JstcProgress.BossDeath)
                                    allow = false;
                            }
                            if (allow)
                                SoundEngine.PlaySound(new SoundStyle("TerRoguelike/Sounds/weird") with { Volume = 0.25f });
                            break;
                        }
                    }
                }
            }
            if (jstcProgress == JstcProgress.BossDeath)
            {
                Vector2 portalScale = TerRoguelikeWorld.jstcPortalScale;
                bool portalRot = portalScale.X > portalScale.Y;
                Vector2 portalPos = TerRoguelikeWorld.jstcPortalPos;
                Vector2 portalDimensions = new Vector2(14, portalRot ? portalScale.X : portalScale.Y);
                if (portalRot)
                    portalDimensions = new Vector2(portalDimensions.Y, portalDimensions.X);

                Point portalTopLeft = (portalPos - (portalDimensions * 0.5f)).ToPoint();
                Rectangle portalRect = portalRot ? new Rectangle(portalTopLeft.X, portalTopLeft.Y, (int)portalDimensions.X, (int)portalDimensions.Y) : new Rectangle(portalTopLeft.X - 100, portalTopLeft.Y, 114, (int)portalDimensions.Y);

                foreach (Player player in Main.ActivePlayers)
                {
                    if (player.getRect().Intersects(portalRect))
                    {
                        Room startRoom = RoomSystem.RoomList[SchematicManager.RoomID[StartRoomID].myRoom];
                        Vector2 finalTargetPos = startRoom.RoomPosition16 + startRoom.RoomCenter16;

                        foreach (Player player2 in Main.ActivePlayers)
                        {
                            player2.velocity = Vector2.Zero;
                            var modPlayer = player2.ModPlayer();
                            modPlayer.jstcTeleportTime = 1;
                            modPlayer.jstcTeleportStart = player2.Center;
                            modPlayer.jstcTeleportEnd = finalTargetPos;
                        }
                        jstcProgress = JstcProgress.BossPortal;
                        TerRoguelikeWorld.jstcPortalTime = -60;
                        CutsceneSystem.SetCutscene(player.Center, 180, 60, 30, 1.5f);
                        SoundEngine.PlaySound(TerRoguelikeWorld.WorldTeleport with { Volume = 0.2f, Variants = [2], Pitch = -0.25f });
                        break;
                    }
                }
            }
            if (jstcProgress == JstcProgress.BossPortal)
            {
                CutsceneSystem.cameraTargetCenter = Vector2.Lerp(Main.LocalPlayer.Center + Vector2.UnitY * Main.LocalPlayer.gfxOffY, Main.Camera.Center, 0.6f);
                bool end = false;
                Main.SetCameraLerp(0, 0);
                foreach (Player player in Main.ActivePlayers)
                {
                    var modPlayer = player.ModPlayer();
                    if (modPlayer == null) continue;

                    float completion = modPlayer.jstcTeleportTime / 180f;
                    float interpolant = MathHelper.Hermite(0, 0f, 1, 0f, completion);
                    player.Center = Vector2.Lerp(modPlayer.jstcTeleportStart, modPlayer.jstcTeleportEnd, interpolant);

                    SpawnParticles(player);
                    

                    if (modPlayer.jstcTeleportTime >= 180)
                    {
                        end = true;
                    }
                    modPlayer.jstcTeleportTime++;
                }
                if (end)
                {
                    foreach (Player player in Main.ActivePlayers)
                    {
                        player.velocity = player.position - player.oldPosition;

                        var modPlayer = player.ModPlayer();
                        if (modPlayer == null) continue;

                        player.Center = modPlayer.jstcTeleportEnd;
                        modPlayer.jstcTeleportTime = 0;
                    }
                    jstcProgress = JstcProgress.Jstc;
                }
            }
            if (TerRoguelikeWorld.jstcPortalTime != 0)
            {
                TerRoguelikeWorld.jstcPortalTime++;
            }
            void SpawnParticles(Player player)
            {
                int inbetweenCount = 8;
                for (int i = 0; i < inbetweenCount; i++)
                {
                    if (!Main.rand.NextBool(4))
                        continue;

                    float inbetweenComp = i / (float)inbetweenCount;
                    Vector2 playerPos = Vector2.Lerp(player.position, player.oldPosition, inbetweenComp);

                    Vector2 particlePos = Main.rand.NextVector2FromRectangle(new Rectangle((int)playerPos.X, (int)playerPos.Y, player.width, player.height));
                    Vector2 particleVel = Main.rand.NextVector2Circular(0.5f, 0.5f);
                    Color particleCol = Color.Lerp(Color.Yellow, Color.White, Main.rand.NextFloat(0.5f));
                    Vector2 particleScale = new Vector2(Main.rand.NextFloat(1f, 2f)) * 0.03f;
                    for (int p = 0; p < 2; p++)
                    {
                        float particleRot = p * MathHelper.PiOver2;
                        ParticleManager.AddParticle(new ThinSpark(particlePos, particleVel, 30, particleCol, particleScale, particleRot, true, false), ParticleManager.ParticleLayer.AfterProjectiles);
                    }
                }
            }
        }
        public virtual void Reset()
        {
            jstc = 0;
            targetJstc = 0;
            jstcProgress = JstcProgress.Start;
        }
        #endregion
    }
}
