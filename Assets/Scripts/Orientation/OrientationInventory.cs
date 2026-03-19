using UnityEngine;
using UnityEngine.UI;

public class OrientationInventory : MonoBehaviour
{
    private Camera _camera;
    private bool _isHorizontal = false;
    private bool _isInitialized = false;

    public GameObject Buttons;
    public RectTransform TransformButtons;

    public VerticalLayoutGroup verticalLayoutGroup;

    public RectTransform filterContent;

    public RectTransform ButtonsObjectsWidth;

    public float WidthHorizontal = 1417f;
    public float WidthVertical = 1061f;

    private void Start()
    {
        _camera = Camera.main;
        Time.timeScale = 1f;

        UpdateOrientation(true);
    }

    private void Update()
    {
        if (_camera == null) return;

        bool shouldBeHorizontal = Screen.width > Screen.height;

        if (shouldBeHorizontal != _isHorizontal || !_isInitialized)
        {
            UpdateOrientation(shouldBeHorizontal);
        }
    }

    private void UpdateOrientation(bool isHorizontal)
    {
        _isHorizontal = isHorizontal;
        _isInitialized = true;

        if (isHorizontal)
        {
            verticalLayoutGroup.padding.left = 69;
            verticalLayoutGroup.padding.top = 69;

            if (Buttons != null && TransformButtons != null)
            {
                Buttons.transform.SetParent(TransformButtons, false);
                ButtonsObjectsWidth.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, WidthHorizontal);
            }
        }
        else
        {
            verticalLayoutGroup.padding.left = 33;
            verticalLayoutGroup.padding.top = 39;

            if (Buttons != null && filterContent != null)
            {
                Buttons.transform.SetParent(filterContent, false);
                Buttons.transform.SetSiblingIndex(1); // Второй объект (индекс 1)
                ButtonsObjectsWidth.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, WidthVertical);
            }
        }

        // Принудительно перестраиваем layout
        if (filterContent != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(filterContent);
        }

        // Также перестраиваем TransformButtons, если это RectTransform
        if (TransformButtons != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(TransformButtons);
        }
    }
}