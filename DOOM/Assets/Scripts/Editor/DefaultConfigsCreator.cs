using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using DOOM.Game;
using DOOM.Core;

namespace DOOM.Editor
{
    /// <summary>
    /// DOOM/4. Create Default Configs — создаёт EnemyConfig, WaveConfig, BossConfig,
    /// prefab-ы и прописывает всё в ObjectPoolManager, WaveController, BossSpawner.
    /// </summary>
    public static class DefaultConfigsCreator
    {
        const string SO_PATH    = "Assets/ScriptableObjects";
        const string PREFAB_PATH = "Assets/Prefabs";

        [MenuItem("DOOM/4. Create Default Configs + Prefabs")]
        public static void CreateAll()
        {
            EnsureDir(SO_PATH);
            EnsureDir(PREFAB_PATH);

            // ── 1. EnemyConfigs ──────────────────────────────────────
            var enemyTypes = new[]
            {
                (EnemyType.Zombie,   "Zombie",   50f,  5f, 1.5f, 10,  new Color(0.4f,0.7f,0.3f)),
                (EnemyType.Dog,      "Dog",      30f,  8f, 3.0f, 15,  new Color(0.7f,0.5f,0.2f)),
                (EnemyType.Lizard,   "Lizard",   60f,  6f, 2.0f, 20,  new Color(0.2f,0.6f,0.4f)),
                (EnemyType.Monster,  "Monster", 120f, 12f, 1.2f, 40,  new Color(0.6f,0.1f,0.1f)),
                (EnemyType.Guard,    "Guard",    80f, 10f, 2.5f, 30,  new Color(0.3f,0.3f,0.7f)),
            };

            var enemyConfigs = new Dictionary<EnemyType, EnemyConfig>();
            var enemyPrefabs  = new Dictionary<EnemyType, GameObject>();

            foreach (var (type, tname, hp, dmg, spd, reward, color) in enemyTypes)
            {
                var cfg = GetOrCreate<EnemyConfig>($"{SO_PATH}/Enemy_{tname}.asset");
                cfg.enemyType = type;
                cfg.health    = hp;
                cfg.damage    = dmg;
                cfg.speed     = spd;
                cfg.reward    = reward;
                EditorUtility.SetDirty(cfg);
                enemyConfigs[type] = cfg;

                string poolKey = $"enemy_{tname.ToLower()}";
                enemyPrefabs[type] = GetOrCreatePrefab(poolKey, color, new Vector2(0.5f, 0.8f),
                    typeof(EnemyController), typeof(Rigidbody2D), typeof(CircleCollider2D));
            }

            // ── 2. BossConfigs + prefabs ─────────────────────────────
            var bossData = new[]
            {
                (1, "Мелкий чиновник",  200f, 15f, 1.0f, SpecialAbilityType.None,        new Color(0.5f,0.5f,0.5f)),
                (2, "Пропагандист",     350f, 20f, 1.2f, SpecialAbilityType.SpawnMinions, new Color(0.6f,0.4f,0.1f)),
                (3, "Силовик",          500f, 25f, 1.5f, SpecialAbilityType.Shield,       new Color(0.2f,0.2f,0.6f)),
                (4, "Олигарх",          700f, 30f, 1.8f, SpecialAbilityType.Charge,       new Color(0.7f,0.6f,0.0f)),
                (5, "Генерал",         1000f, 40f, 2.0f, SpecialAbilityType.SpawnMinions, new Color(0.5f,0.0f,0.0f)),
                (6, "Президент",       2000f, 60f, 2.5f, SpecialAbilityType.MultiPhase,   new Color(0.8f,0.2f,0.0f)),
            };

            var bossConfigs = new List<BossConfig>();
            foreach (var (lvl, dname, hp, dmg, spd, ability, color) in bossData)
            {
                var cfg = GetOrCreate<BossConfig>($"{SO_PATH}/Boss_Level{lvl}.asset");
                cfg.bossLevel     = lvl;
                cfg.displayName   = dname;
                cfg.health        = hp;
                cfg.damage        = dmg;
                cfg.speed         = spd;
                cfg.specialAbility = ability;
                cfg.abilityInterval = 5f;
                cfg.shieldDuration  = 2f;
                cfg.minionCount     = 3;
                EditorUtility.SetDirty(cfg);
                bossConfigs.Add(cfg);

                GetOrCreatePrefab($"boss_{lvl}", color, new Vector2(1.0f, 1.4f),
                    typeof(BossController));
            }

            // ── 3. Player unit prefab ────────────────────────────────
            GetOrCreatePrefab("player_unit", new Color(0.8f, 0.7f, 0.5f), new Vector2(0.4f, 0.7f),
                typeof(PlayerUnit));

            // ── 4. Bullet prefab ─────────────────────────────────────
            GetOrCreatePrefab("bullet", new Color(1f, 0.9f, 0.2f), new Vector2(0.15f, 0.15f),
                typeof(Bullet));

            // ── 5. Barrel prefab ─────────────────────────────────────
            GetOrCreatePrefab("barrel", new Color(0.6f, 0.3f, 0.1f), new Vector2(0.5f, 0.6f),
                typeof(Barrel));

            // ── 6. WaveConfigs ───────────────────────────────────────
            var waveConfigs = new List<WaveConfig>();
            var waveEnemy = new[]
            {
                new[]{(EnemyType.Zombie, 5)},
                new[]{(EnemyType.Zombie, 5), (EnemyType.Dog, 3)},
                new[]{(EnemyType.Zombie, 4), (EnemyType.Dog, 3), (EnemyType.Lizard, 2)},
                new[]{(EnemyType.Dog, 4), (EnemyType.Lizard, 4), (EnemyType.Guard, 2)},
                new[]{(EnemyType.Lizard, 3), (EnemyType.Guard, 4), (EnemyType.Monster, 2)},
                new[]{(EnemyType.Guard, 3), (EnemyType.Monster, 5)},
            };

            for (int w = 0; w < 6; w++)
            {
                var wc = GetOrCreate<WaveConfig>($"{SO_PATH}/Wave_{w+1}.asset");
                wc.waveNumber     = w + 1;
                wc.spawnInterval  = 0.5f;
                wc.barrelCount    = 2;
                wc.enemies        = new List<EnemySpawnEntry>();
                foreach (var (type, count) in waveEnemy[w])
                {
                    wc.enemies.Add(new EnemySpawnEntry
                    {
                        enemyConfig = enemyConfigs[type],
                        count       = count
                    });
                }
                EditorUtility.SetDirty(wc);
                waveConfigs.Add(wc);
            }

            AssetDatabase.SaveAssets();

            // ── 7. Прописать всё в сцену ─────────────────────────────
            WireIntoScene(enemyTypes, enemyPrefabs, bossConfigs, waveConfigs);

            Debug.Log("<color=green>[DOOM] ✅ Конфиги, prefab-ы и привязка в сцену — готово!</color>");
        }

        // ─────────────────────────────────────────────────────────────
        static void WireIntoScene(
            (EnemyType type, string name, float hp, float dmg, float spd, int reward, Color color)[] enemyTypes,
            Dictionary<EnemyType, GameObject> enemyPrefabs,
            List<BossConfig> bossConfigs,
            List<WaveConfig> waveConfigs)
        {
            // ObjectPoolManager
            var poolMgr = Object.FindFirstObjectByType<ObjectPoolManager>();
            if (poolMgr != null)
            {
                var so = new SerializedObject(poolMgr);
                var list = so.FindProperty("poolConfigs");
                list.ClearArray();

                int idx = 0;

                void AddPool(string key, GameObject prefab, int size)
                {
                    list.arraySize++;
                    var el = list.GetArrayElementAtIndex(idx++);
                    el.FindPropertyRelative("key").stringValue       = key;
                    el.FindPropertyRelative("prefab").objectReferenceValue = prefab;
                    el.FindPropertyRelative("initialSize").intValue  = size;
                }

                AddPool("player_unit", LoadPrefab("player_unit"), 20);
                AddPool("bullet",      LoadPrefab("bullet"),       50);
                AddPool("barrel",      LoadPrefab("barrel"),        6);
                foreach (var (type, tname, _, _, _, _, _) in enemyTypes)
                    AddPool($"enemy_{tname.ToLower()}", LoadPrefab($"enemy_{tname.ToLower()}"), 10);
                for (int i = 1; i <= 6; i++)
                    AddPool($"boss_{i}", LoadPrefab($"boss_{i}"), 1);

                so.ApplyModifiedProperties();
                Debug.Log("[DOOM] ObjectPoolManager — пулы прописаны.");
            }
            else Debug.LogWarning("[DOOM] ObjectPoolManager не найден в сцене.");

            // WaveController
            var wavCtrl = Object.FindFirstObjectByType<WaveController>();
            if (wavCtrl != null)
            {
                var so = new SerializedObject(wavCtrl);
                var list = so.FindProperty("waveConfigs");
                list.ClearArray();
                for (int i = 0; i < waveConfigs.Count; i++)
                {
                    list.arraySize++;
                    list.GetArrayElementAtIndex(i).objectReferenceValue = waveConfigs[i];
                }
                so.ApplyModifiedProperties();
                Debug.Log("[DOOM] WaveController — 6 волн прописаны.");
            }
            else Debug.LogWarning("[DOOM] WaveController не найден в сцене.");

            // BossSpawner
            var bossSpawner = Object.FindFirstObjectByType<BossSpawner>();
            if (bossSpawner != null)
            {
                var so = new SerializedObject(bossSpawner);
                var list = so.FindProperty("bossConfigs");
                list.ClearArray();
                for (int i = 0; i < bossConfigs.Count; i++)
                {
                    list.arraySize++;
                    list.GetArrayElementAtIndex(i).objectReferenceValue = bossConfigs[i];
                }
                so.ApplyModifiedProperties();
                Debug.Log("[DOOM] BossSpawner — 6 боссов прописаны.");
            }
            else Debug.LogWarning("[DOOM] BossSpawner не найден в сцене.");

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }

        // ─────────────────────────────────────────────────────────────
        static GameObject GetOrCreatePrefab(string key, Color color, Vector2 size,
            params System.Type[] components)
        {
            string path = $"{PREFAB_PATH}/{key}.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = new GameObject(key);

            // Sprite renderer с цветным квадратом
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = MakeSquareSprite(color);

            go.transform.localScale = new Vector3(size.x, size.y, 1f);

            foreach (var t in components)
                if (go.GetComponent(t) == null) go.AddComponent(t);

            // Rigidbody2D — убрать gravity
            var rb = go.GetComponent<Rigidbody2D>();
            if (rb != null) { rb.gravityScale = 0; rb.constraints = RigidbodyConstraints2D.FreezeRotation; }

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        static GameObject LoadPrefab(string key) =>
            AssetDatabase.LoadAssetAtPath<GameObject>($"{PREFAB_PATH}/{key}.prefab");

        static T GetOrCreate<T>(string path) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null) return existing;
            var obj = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(obj, path);
            return obj;
        }

        static Sprite MakeSquareSprite(Color color)
        {
            var tex = new Texture2D(16, 16);
            var pixels = new Color[256];
            for (int i = 0; i < 256; i++) pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 16, 16), Vector2.one * 0.5f, 16f);
        }

        static void EnsureDir(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                var parts = path.Split('/');
                string cur = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    string next = cur + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(next))
                        AssetDatabase.CreateFolder(cur, parts[i]);
                    cur = next;
                }
            }
        }
    }
}
