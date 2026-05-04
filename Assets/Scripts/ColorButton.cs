using UnityEngine;
using UnityEngine.UI;

public class ColorButton : MonoBehaviour
{
    public SectionTypeColor color;
    private Button button;

    public Image BackGroundImage;
    public Image RedLine;

    public Color backGroundColor;
    public Color _color; // цвет кнопки при активном выделении

    private void Awake() => button = GetComponent<Button>();

    private void OnEnable()
    {
        if (InstanceObjects.Instance != null)
        {
            InstanceObjects.Instance.OnObjectColorChanged += OnObjectColorChanged;
            InstanceObjects.Instance.OnSelectedObjectChanged += OnSelectedObjectChanged;
        }
        if (SelectionManager.Instance != null)
            SelectionManager.Instance.OnSectionChanged += OnSectionChanged;

        UpdateState();
    }

    private void OnDisable()
    {
        if (InstanceObjects.Instance != null)
        {
            InstanceObjects.Instance.OnObjectColorChanged -= OnObjectColorChanged;
            InstanceObjects.Instance.OnSelectedObjectChanged -= OnSelectedObjectChanged;
        }
        if (SelectionManager.Instance != null)
            SelectionManager.Instance.OnSectionChanged -= OnSectionChanged;
    }

    private void OnObjectColorChanged(SectionType changedSection, SectionTypeColor newColor) => UpdateState();
    private void OnSectionChanged(SectionType newSection) => UpdateState();
    private void OnSelectedObjectChanged(GameObject newSelected) => UpdateState();

    private void Update()
    {
        UpdateState(); // для гарантии синхронизации в каждом кадре (можно убрать, если события покрывают)
    }

    private void UpdateState()
    {
        if (button == null || SelectionManager.Instance == null || InstanceObjects.Instance == null)
        {
            if (button != null) button.interactable = false;
            if (BackGroundImage != null) BackGroundImage.enabled = false;
            GetComponent<Image>().color = backGroundColor;
            if (RedLine != null) RedLine.color = Color.darkRed;
            return;
        }

        // Целевой объект — выделенный объект (только если его секция совпадает с текущей секцией UI)
        GameObject targetObject = GetTargetObject();
        bool objExists = targetObject != null;

        button.interactable = objExists;

        if (BackGroundImage != null)
        {
            if (objExists)
            {
                var colorObj = targetObject.GetComponent<ColorObjects>();
                bool isCurrentColor = (colorObj != null && colorObj.sectionColorSprite == color);

                BackGroundImage.enabled = isCurrentColor;
                GetComponent<Image>().color = isCurrentColor ? _color : backGroundColor;
                if (RedLine != null)
                    RedLine.color = isCurrentColor ? Color.red : Color.darkRed;
            }
            else
            {
                BackGroundImage.enabled = false;
                GetComponent<Image>().color = backGroundColor;
                if (RedLine != null) RedLine.color = Color.darkRed;
            }
        }
    }

    /// <summary>Возвращает объект, к которому будет применён цвет</summary>
    private GameObject GetTargetObject()
    {
        var selected = InstanceObjects.Instance.SelectedObject;
        if (selected == null) return null;

        // Проверяем, что выделенный объект принадлежит текущей секции
        var notifier = selected.GetComponent<SpawnedObjectNotifier>();
        if (notifier != null && notifier.section == SelectionManager.Instance.CurrentSection)
            return selected;

        return null;
    }

    public void ApplyColor()
    {
        GameObject obj = GetTargetObject();
        if (obj == null)
        {
            Debug.LogWarning("[ColorButton] Нет подходящего выделенного объекта для смены цвета");
            return;
        }

        var colorObj = obj.GetComponent<ColorObjects>();
        if (colorObj != null)
            colorObj.SetColor(color);
        else
            Debug.LogWarning($"[ColorButton] На объекте {obj.name} нет компонента ColorObjects");
    }
}