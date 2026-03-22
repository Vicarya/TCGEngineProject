using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TCG.Weiss.UI
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(LayoutElement))]
    public class PaginationUI : MonoBehaviour
    {
        public Button PrevButton { get; private set; }
        public Button NextButton { get; private set; }
        public IReadOnlyList<Button> PageButtons => _pageButtons;
        public IReadOnlyList<TextMeshProUGUI> PageButtonTexts => _pageButtonTexts;

        private const float HorizontalPadding = 12f;
        private const float VerticalPadding = 8f;
        private const float CenterGap = 8f;
        private const int MaxPageButtons = 7;
        private const string EditorNotoFontPath = "Assets/Fonts/NoteSansJP/Noto_Sans_JP/static/NotoSansJP-Regular SDF.asset";

        private readonly List<Button> _pageButtons = new List<Button>();
        private readonly List<TextMeshProUGUI> _pageButtonTexts = new List<TextMeshProUGUI>();

        private RectTransform _rectTransform;
        private RectTransform _prevButtonRect;
        private RectTransform _nextButtonRect;
        private RectTransform _centerRootRect;
        private readonly List<RectTransform> _pageButtonRects = new List<RectTransform>();
        private TMP_FontAsset _paginationFontAsset;

        private void Awake()
        {
            _paginationFontAsset = ResolvePaginationFont();

            _rectTransform = GetComponent<RectTransform>();
            _rectTransform.anchorMin = new Vector2(0f, 0f);
            _rectTransform.anchorMax = new Vector2(1f, 1f);
            _rectTransform.pivot = new Vector2(0.5f, 0.5f);
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;

            LayoutElement layoutElement = GetComponent<LayoutElement>();
            layoutElement.flexibleWidth = 1f;
            layoutElement.flexibleHeight = 1f;
            layoutElement.minWidth = 1f;
            layoutElement.minHeight = 1f;

            Image background = gameObject.GetComponent<Image>();
            if (background == null)
            {
                background = gameObject.AddComponent<Image>();
            }
            background.color = new Color(1f, 1f, 1f, 0.08f);

            PrevButton = CreateButton("PrevButton", "Prev", out _prevButtonRect, out _);
            CreateCenterRoot();
            for (int i = 0; i < MaxPageButtons; i++)
            {
                Button pageButton = CreateButton($"PageButton{i}", "-", out RectTransform pageRect, out TextMeshProUGUI pageText, _centerRootRect);
                _pageButtons.Add(pageButton);
                _pageButtonTexts.Add(pageText);
                _pageButtonRects.Add(pageRect);
            }
            NextButton = CreateButton("NextButton", "Next", out _nextButtonRect, out _);

            UpdateLayout();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (_rectTransform == null)
            {
                return;
            }

            UpdateLayout();
        }

        private void CreateCenterRoot()
        {
            GameObject centerRoot = new GameObject("PageButtons", typeof(RectTransform));
            centerRoot.transform.SetParent(transform, false);
            _centerRootRect = centerRoot.GetComponent<RectTransform>();
        }

        private void UpdateLayout()
        {
            float parentHeight = _rectTransform.rect.height;
            float parentWidth = _rectTransform.rect.width;
            if (parentHeight <= 0f && _rectTransform.parent is RectTransform parentRect)
            {
                parentHeight = parentRect.rect.height;
                if (parentWidth <= 0f)
                {
                    parentWidth = parentRect.rect.width;
                }
            }

            float sideButtonSize = Mathf.Max(24f, parentHeight - (VerticalPadding * 2f));
            float sideMargin = HorizontalPadding + sideButtonSize + 16f;
            float availableCenterWidth = Mathf.Max(24f, parentWidth - (sideMargin * 2f));
            float pageButtonWidth = Mathf.Max(
                24f,
                (availableCenterWidth - ((_pageButtonRects.Count - 1) * CenterGap)) / Mathf.Max(1, _pageButtonRects.Count)
            );

            if (_prevButtonRect != null)
            {
                _prevButtonRect.anchorMin = new Vector2(0f, 0.5f);
                _prevButtonRect.anchorMax = new Vector2(0f, 0.5f);
                _prevButtonRect.pivot = new Vector2(0f, 0.5f);
                _prevButtonRect.anchoredPosition = new Vector2(HorizontalPadding, 0f);
                _prevButtonRect.sizeDelta = new Vector2(sideButtonSize, sideButtonSize);
            }

            if (_nextButtonRect != null)
            {
                _nextButtonRect.anchorMin = new Vector2(1f, 0.5f);
                _nextButtonRect.anchorMax = new Vector2(1f, 0.5f);
                _nextButtonRect.pivot = new Vector2(1f, 0.5f);
                _nextButtonRect.anchoredPosition = new Vector2(-HorizontalPadding, 0f);
                _nextButtonRect.sizeDelta = new Vector2(sideButtonSize, sideButtonSize);
            }

            if (_centerRootRect != null)
            {
                _centerRootRect.anchorMin = new Vector2(0f, 0f);
                _centerRootRect.anchorMax = new Vector2(1f, 1f);
                _centerRootRect.offsetMin = new Vector2(sideMargin, VerticalPadding);
                _centerRootRect.offsetMax = new Vector2(-sideMargin, -VerticalPadding);
            }

            LayoutCenterButtons(pageButtonWidth, sideButtonSize);
        }

        private void LayoutCenterButtons(float buttonWidth, float buttonHeight)
        {
            if (_centerRootRect == null || _pageButtonRects.Count == 0)
            {
                return;
            }

            float totalWidth = (_pageButtonRects.Count * buttonWidth) + ((_pageButtonRects.Count - 1) * CenterGap);
            float startX = -totalWidth * 0.5f + buttonWidth * 0.5f;

            for (int i = 0; i < _pageButtonRects.Count; i++)
            {
                RectTransform rect = _pageButtonRects[i];
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(startX + i * (buttonWidth + CenterGap), 0f);
                rect.sizeDelta = new Vector2(buttonWidth, buttonHeight);
            }
        }

        private Button CreateButton(string name, string label, out RectTransform rectTransform, out TextMeshProUGUI labelText, RectTransform parentOverride = null)
        {
            GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parentOverride != null ? parentOverride : transform, false);

            rectTransform = buttonObject.GetComponent<RectTransform>();

            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.88f, 0.9f, 0.94f, 0.95f);

            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = image.color;
            colors.highlightedColor = new Color(0.96f, 0.97f, 0.99f, 1f);
            colors.pressedColor = new Color(0.78f, 0.82f, 0.9f, 1f);
            colors.disabledColor = new Color(0.55f, 0.57f, 0.62f, 0.45f);
            button.colors = colors;

            GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer));
            textObject.transform.SetParent(buttonObject.transform, false);

            labelText = textObject.AddComponent<TextMeshProUGUI>();
            ConfigureText(labelText, label, 24f);

            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return button;
        }

        private void ConfigureText(TextMeshProUGUI text, string value, float fontSize)
        {
            TMP_FontAsset fontAsset = _paginationFontAsset != null ? _paginationFontAsset : ResolvePaginationFont();
            if (fontAsset != null)
            {
                text.font = fontAsset;
            }

            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = fontSize;
            text.color = new Color(0.1f, 0.12f, 0.16f, 1f);
            text.enableWordWrapping = false;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.text = value;
        }

        public void SetPageButtonState(int index, string label, bool active, bool interactable, bool isCurrent)
        {
            if (index < 0 || index >= _pageButtons.Count)
            {
                return;
            }

            Button button = _pageButtons[index];
            TextMeshProUGUI text = _pageButtonTexts[index];

            button.gameObject.SetActive(active);
            if (!active)
            {
                return;
            }

            text.text = label;
            button.interactable = interactable;

            Image image = button.GetComponent<Image>();
            image.color = isCurrent
                ? new Color(0.63f, 0.76f, 0.97f, 1f)
                : new Color(0.88f, 0.9f, 0.94f, 0.95f);
            text.color = new Color(0.1f, 0.12f, 0.16f, 1f);
        }

        private TMP_FontAsset ResolvePaginationFont()
        {
#if UNITY_EDITOR
            TMP_FontAsset editorFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(EditorNotoFontPath);
            if (editorFont != null)
            {
                return editorFont;
            }
#endif
            TMP_FontAsset defaultFont = TMP_Settings.defaultFontAsset;
            if (defaultFont != null)
            {
                return defaultFont;
            }

            return Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        }
    }
}
