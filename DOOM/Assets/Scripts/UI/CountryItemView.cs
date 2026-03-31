using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DOOM.Data;

namespace DOOM.UI
{
    /// <summary>
    /// DOOM-2.2 — Элемент списка стран в ScrollView.
    /// </summary>
    public class CountryItemView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI flagText;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI populationText;
        [SerializeField] private Button selectButton;
        [SerializeField] private GameObject selectedHighlight;

        private Country _country;
        private System.Action<Country> _onSelected;

        public void Init(Country country, System.Action<Country> onSelected)
        {
            _country = country;
            _onSelected = onSelected;

            flagText.text = country.flag;
            nameText.text = country.name;
            populationText.text = FormatPopulation(country.population);

            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() => _onSelected?.Invoke(_country));
            SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            if (selectedHighlight != null)
                selectedHighlight.SetActive(selected);
        }

        private string FormatPopulation(long pop)
        {
            if (pop >= 1_000_000_000) return $"{pop / 1_000_000_000f:0.#} млрд";
            if (pop >= 1_000_000)     return $"{pop / 1_000_000f:0.#} млн";
            return $"{pop / 1000f:0.#} тыс";
        }
    }
}
