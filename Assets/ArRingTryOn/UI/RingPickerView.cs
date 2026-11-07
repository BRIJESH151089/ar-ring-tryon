using System.Collections.Generic;
using ArRingTryOn.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ArRingTryOn.UI
{
    /// <summary>A row of buttons, one per ring in the catalog. Tapping one selects that ring.</summary>
    public sealed class RingPickerView : MonoBehaviour
    {
        [SerializeField] private Button _buttonTemplate;
        [SerializeField] private float _bottomMargin = 48f;
        [SerializeField] private Color _idleColor = new Color(0f, 0f, 0f, 0.55f);
        [SerializeField] private Color _selectedColor = new Color(0.96f, 0.78f, 0.36f, 0.95f);
        [SerializeField] private Color _idleTextColor = Color.white;
        [SerializeField] private Color _selectedTextColor = new Color(0.12f, 0.1f, 0.08f);

        private readonly List<(Image Background, Text Label)> _buttons = new List<(Image, Text)>();
        private RingSelection _selection;

        public void Bind(RingCatalog catalog, RingSelection selection)
        {
            _selection = selection;
            _buttonTemplate.gameObject.SetActive(false);

            for (var i = 0; i < catalog.Count; i++)
            {
                var design = catalog[i];
                var button = Instantiate(_buttonTemplate, _buttonTemplate.transform.parent);
                button.name = design.DisplayName;
                button.gameObject.SetActive(true);

                var label = button.GetComponentInChildren<Text>();
                label.text = design.DisplayName;
                button.onClick.AddListener(() => selection.TrySelect(design.Id));
                _buttons.Add((button.GetComponent<Image>(), label));
            }

            selection.Changed += OnSelectionChanged;
            Highlight(selection.CurrentIndex);
            KeepAboveSystemBar();
        }

        private void OnSelectionChanged(RingDesign design)
        {
            Highlight(_selection.CurrentIndex);
        }

        private void Highlight(int selectedIndex)
        {
            for (var i = 0; i < _buttons.Count; i++)
            {
                var selected = i == selectedIndex;
                _buttons[i].Background.color = selected ? _selectedColor : _idleColor;
                _buttons[i].Label.color = selected ? _selectedTextColor : _idleTextColor;
            }
        }

        // The app draws edge to edge, so lift the row clear of the gesture or navigation bar.
        private void KeepAboveSystemBar()
        {
            var rectTransform = (RectTransform)transform;
            var scaleFactor = GetComponentInParent<Canvas>().rootCanvas.scaleFactor;
            var inset = Screen.safeArea.yMin / scaleFactor;
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, _bottomMargin + inset);
        }

        private void OnDestroy()
        {
            if (_selection != null)
            {
                _selection.Changed -= OnSelectionChanged;
            }
        }
    }
}
