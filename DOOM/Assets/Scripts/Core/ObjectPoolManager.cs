using System.Collections.Generic;
using UnityEngine;

namespace DOOM.Core
{
    /// <summary>
    /// DOOM-1.5 — Менеджер пулов объектов (враги, снаряды, эффекты).
    /// Pre-warm при загрузке сцены.
    /// </summary>
    public class ObjectPoolManager : MonoBehaviour
    {
        [System.Serializable]
        public class PoolConfig
        {
            public string key;
            public GameObject prefab;
            public int initialSize;
        }

        [SerializeField] private List<PoolConfig> poolConfigs;

        private Dictionary<string, Queue<GameObject>> pools = new();
        private Dictionary<string, GameObject> prefabMap = new();

        public static ObjectPoolManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start() => PreWarm();

        private void PreWarm()
        {
            foreach (var cfg in poolConfigs)
            {
                prefabMap[cfg.key] = cfg.prefab;
                var queue = new Queue<GameObject>();
                for (int i = 0; i < cfg.initialSize; i++)
                    queue.Enqueue(CreateNew(cfg.key, cfg.prefab));
                pools[cfg.key] = queue;
            }
        }

        private GameObject CreateNew(string key, GameObject prefab)
        {
            var obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            obj.name = key;
            return obj;
        }

        public GameObject Spawn(string key, Vector3 position, Quaternion rotation)
        {
            if (!pools.ContainsKey(key))
            {
                Debug.LogWarning($"[Pool] Ключ '{key}' не зарегистрирован.");
                return null;
            }

            GameObject obj;
            if (pools[key].Count > 0)
                obj = pools[key].Dequeue();
            else
                obj = CreateNew(key, prefabMap[key]);

            obj.transform.SetPositionAndRotation(position, rotation);
            obj.SetActive(true);
            obj.GetComponent<IPoolable>()?.OnSpawn();
            return obj;
        }

        public void Despawn(string key, GameObject obj)
        {
            obj.GetComponent<IPoolable>()?.OnDespawn();
            obj.SetActive(false);
            obj.transform.SetParent(transform);
            pools[key].Enqueue(obj);
        }
    }
}
