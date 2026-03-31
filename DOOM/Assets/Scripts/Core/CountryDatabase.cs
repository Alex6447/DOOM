using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using DOOM.Data;

namespace DOOM.Core
{
    /// <summary>
    /// DOOM-1.4 / DOOM-2.1 — Загрузка справочника стран из StreamingAssets/countries.json.
    /// Офлайн-режим: данные упакованы в приложение.
    /// </summary>
    public class CountryDatabase : MonoBehaviour
    {
        public static CountryDatabase Instance { get; private set; }

        public IReadOnlyList<Country> Countries => _countries;
        private List<Country> _countries = new();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start() => StartCoroutine(LoadCountries());

        private IEnumerator LoadCountries()
        {
            string path = Path.Combine(Application.streamingAssetsPath, "countries.json");

#if UNITY_ANDROID && !UNITY_EDITOR
            // На Android StreamingAssets читается через UnityWebRequest
            using var request = UnityEngine.Networking.UnityWebRequest.Get(path);
            yield return request.SendWebRequest();
            if (request.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[CountryDB] Ошибка загрузки: {request.error}");
                yield break;
            }
            ParseJson(request.downloadHandler.text);
#else
            if (!File.Exists(path))
            {
                Debug.LogError($"[CountryDB] Файл не найден: {path}");
                yield break;
            }
            ParseJson(File.ReadAllText(path));
            yield return null;
#endif
        }

        private void ParseJson(string json)
        {
            var wrapper = JsonUtility.FromJson<CountryListWrapper>(json);
            _countries = wrapper?.countries ?? new List<Country>();
            _countries.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.CurrentCulture));
            Debug.Log($"[CountryDB] Загружено {_countries.Count} стран.");
        }

        public Country GetById(string id) => _countries.Find(c => c.id == id);

        [System.Serializable]
        private class CountryListWrapper { public List<Country> countries; }
    }
}
