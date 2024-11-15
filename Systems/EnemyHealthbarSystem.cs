using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics;
using Terraria.ModLoader;
using TerRoguelike.TerPlayer;

namespace TerRoguelike.Systems
{
    public class EnemyHealthBarSystem : ModSystem
    {
        public static EnemyHealthBar enemyHealthBar = new EnemyHealthBar([], "");
        public override void ClearWorld()
        {
            enemyHealthBar = new EnemyHealthBar([], "");
        }
        public override void Unload()
        {
            enemyHealthBar = null;
        }
        public override void PostUpdateEverything()
        {
            enemyHealthBar.Update();
        }
        public class EnemyHealthBar
        {
            public List<int> TrackedEnemies;
            public List<int> TrackedEnemyTypes;
            public int CurrentHealth;
            public int MaxHealth;
            public float MainBar;
            public float ExtraBar;
            public string Name;
            public float Opacity;
            public EnemyHealthBar(List<int> trackedEnemies, string name)
            {
                TrackedEnemies = [];
                TrackedEnemyTypes = [];
                for (int i = 0; i < trackedEnemies.Count; i++)
                {
                    AddEnemy(trackedEnemies[i]);
                }
                Name = name;
                MainBar = 1;
                ExtraBar = 1;
                Opacity = 0;
                CurrentHealth = 0;
                MaxHealth = 0;
                Update();
            }
            public void AddEnemy(int npc)
            {
                TrackedEnemies.Add(npc);
                TrackedEnemyTypes.Add(Main.npc[npc].type);
            }
            public void ForceEnd(int displayedHealth)
            {
                CurrentHealth = displayedHealth;
                TrackedEnemies.Clear();
                TrackedEnemyTypes.Clear();
            }
            public void Update()
            {
                if (TrackedEnemies.Count > 0)
                {
                    CurrentHealth = 0;
                    int storedHealth = MaxHealth;
                    MaxHealth = 0;
                    Opacity += 0.016f;
                    for (int i = 0; i < TrackedEnemies.Count; i++)
                    {
                        int storedType = TrackedEnemyTypes[i];
                        NPC npc = Main.npc[TrackedEnemies[i]];
                        bool clear = npc.type != storedType || !npc.active || npc.life == 0;
                        if (clear)
                        {
                            TrackedEnemies.RemoveAt(i);
                            TrackedEnemyTypes.RemoveAt(i);
                            i--;
                            continue;
                        }

                        storedHealth = 0;
                        if (npc.life < npc.lifeMax)
                            MaxHealth += npc.lifeMax;
                        else
                            MaxHealth += npc.life;
                        CurrentHealth += npc.life;
                    }
                    if (storedHealth > 0)
                    {
                        MaxHealth = storedHealth;
                    }
                }
                if (TrackedEnemies.Count == 0)
                {
                    Opacity *= 0.99f;
                    Opacity -= 0.01f;
                }
                Opacity = MathHelper.Clamp(Opacity, 0, 1);
                if (Opacity == 0)
                    return;

                if (MaxHealth == 0)
                    MaxHealth = 1;

                MainBar = CurrentHealth / (float)MaxHealth;
                MainBar = MathHelper.Clamp(MainBar, 0, 1);

                ExtraBar = MathHelper.Lerp(MainBar, ExtraBar, 0.965f);
                ExtraBar -= 0.00001f;
                ExtraBar = MathHelper.Clamp(ExtraBar, MainBar - 0.001f, 1);

            }
        }
    }
}
