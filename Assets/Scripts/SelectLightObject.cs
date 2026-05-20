using UnityEngine;
using UnityEngine.UI;

public class SelectLightObject : MonoBehaviour
{
    public Sprite RedSpriteShadow;
    public Sprite RedSprite;
    public Sprite GreenSpriteShadow;
    public Sprite GreenSprite;

    [Tooltip("Перетащите сюда кнопку, на которой висит SpawnObjects нужной секции")]
    public SpawnObjects sectionButton;

    [Tooltip("Фиксированный масштаб для дочернего объекта (тень)")]
    public Vector3 shadowFixedScale = new Vector3(2.137171f, 2.137171f, 2.137171f);

    private Image mainImage;
    private Image shadowImage;
    private PrefabInstanceTracker tracker;

    // Сохраняем исходные размеры основного изображения
    private Vector2 mainImageInitialSize;

    private void Start()
    {
        mainImage = GetComponent<Image>();
        if (transform.childCount > 0)
            shadowImage = transform.GetChild(0).GetComponent<Image>();

        // Запоминаем размеры главного Image (только для mainImage)
        if (mainImage != null)
            mainImageInitialSize = mainImage.rectTransform.sizeDelta;

        tracker = PrefabInstanceTracker.Instance;
        if (tracker == null)
        {
            Debug.LogError("SelectLightObject: PrefabInstanceTracker не найден в сцене!");
            return;
        }

        if (sectionButton == null)
        {
            Debug.LogError("SelectLightObject: Не назначена ссылка на кнопку (sectionButton)!");
            return;
        }

        tracker.OnCountChanged.AddListener(OnPrefabCountChanged);
        UpdateState();
    }

    private void OnDestroy()
    {
        if (tracker != null)
            tracker.OnCountChanged.RemoveListener(OnPrefabCountChanged);
    }

    private void OnPrefabCountChanged(GameObject changedPrefab, int newCount)
    {
        if (sectionButton != null && changedPrefab == sectionButton.PrefabObject)
            UpdateState();
    }

    private void UpdateState()
    {
        if (sectionButton == null || tracker == null)
            return;

        bool hasAny = tracker.GetCount(sectionButton.PrefabObject) > 0;

        // Главное изображение
        if (mainImage != null)
        {
            mainImage.sprite = hasAny ? GreenSprite : RedSprite;
            mainImage.rectTransform.sizeDelta = mainImageInitialSize; // фиксируем размер
        }

        // Дочернее изображение (тень) с фиксированным масштабом
        if (shadowImage != null)
        {
            shadowImage.sprite = hasAny ? GreenSpriteShadow : RedSpriteShadow;
            shadowImage.transform.localScale = shadowFixedScale; // принудительный масштаб
        }
    }
}