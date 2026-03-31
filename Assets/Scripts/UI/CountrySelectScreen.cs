using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DOOM.Core;
using DOOM.Data;

namespace DOOM.UI
{
    /// <summary>
    /// DOOM-2.2 / DOOM-2.3 / DOOM-2.4 / DOOM-2.5 — Экран выбора страны.
    /// </summary>
    public class CountrySelectScreen : MonoBehaviour
    {
        [Header("List")]
        [SerializeField] private Transform listContainer;
        [SerializeField] private CountryItemView itemPrefab;
        [SerializeField] private TMP_InputField searchField;

        [Header("Selection Info")]
        [SerializeField] private GameObject selectionInfoPanel;
        [SerializeField] private TextMeshProUGUI selectedFlagText;
        [SerializeField] private TextMeshProUGUI selectedCapitalText;
        [SerializeField] private TextMeshProUGUI selectedTargetText;

        [Header("Actions")]
        [SerializeField] private Button playButton;
        [SerializeField] private string gameSceneName = "GameScene";

        private List<CountryItemView> _itemViews = new();
        private Country _selectedCountry;
        private CountryItemView _selectedView;

        private IEnumerator Start()
        {
            playButton.interactable = false;
            selectionInfoPanel.SetActive(false);

            // Ждём загрузки базы стран
            yield return new WaitUntil(() => CountryDatabase.Instance?.Countries?.Count > 0);
            BuildList(CountryDatabase.Instance.Countries);

            if (searchField != null)
                searchField.onValueChanged.AddListener(FilterList);

            playButton.onClick.AddListener(OnPlayClicked);
        }

        private void BuildList(IReadOnlyList<Country> countries)
        {
            foreach (Transform child in listContainer) Destroy(child.gameObject);
            _itemViews.Clear();

            foreach (var country in countries)
            {
                var view = Instantiate(itemPrefab, listContainer);
                view.Init(country, OnCountrySelected);
                _itemViews.Add(view);
            }
        }

        private void FilterList(string query)
        {
            foreach (var view in _itemViews)
            {
                bool match = string.IsNullOrEmpty(query) ||
                             view.name.Contains(query, System.StringComparison.OrdinalIgnoreCase);
                view.gameObject.SetActive(match);
            }
        }

        private void OnCountrySelected(Country country)
        {
            _selectedCountry = country;

            // Сбросить предыдущий выбор
            _selectedView?.SetSelected(false);
            _selectedView = _itemViews.Find(v => v.gameObject.activeSelf &&
                                                  v.name == country.id);
            _selectedView?.SetSelected(true);

            // Показать инфо
            selectionInfoPanel.SetActive(true);
            selectedFlagText.text = country.flag;
            selectedCapitalText.text = $"Столица: {country.capital}";
            selectedTargetText.text = $"Цель: {country.targetName}";

            // Сохранить в сессию
            if (GameManager.Instance != null)
                GameManager.Instance.StartNewGame(country.id);

            playButton.interactable = true;
        }

        private void OnPlayClicked()
        {
            if (_selectedCountry == null) return;
            // Анимация перехода может быть добавлена через Animator или DOTween
            SceneManager.LoadScene(gameSceneName);
        }
    }
}
