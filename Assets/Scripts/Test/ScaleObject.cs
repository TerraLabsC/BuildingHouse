using UnityEngine;

public class ScaleObject : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 3f;
    [SerializeField] private float zoomSpeed = 0.01f;
    [SerializeField] private float mouseZoomSpeed = 0.005f;

    private Camera mainCamera;
    private float initialDistance;
    private Vector3 initialScale;
    private bool isMouseScaling = false;
    private Vector2 lastMousePosition;

    public bool IsActive = false;
    public bool IsScaling { get; private set; } = false; // Флаг, указывающий что идет масштабирование

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (IsActive)
        {
            // Проверяем наличие сенсорного ввода
            if (Input.touchSupported)
            {
                // Мультитач для устройств с поддержкой касаний (включая Windows с сенсорным экраном)
                if (Input.touchCount == 2)
                {
                    IsScaling = true;
                    HandleTouchScaling();
                }
                // Если касаний нет или их меньше двух, проверяем эмуляцию мышью
                else
                {
                    HandleMouseEmulation();
                }
            }
            else
            {
                // Если сенсорный ввод не поддерживается, используем только эмуляцию мышью
                HandleMouseEmulation();
            }
        }
        else
        {
            IsScaling = false;
        }
    }

    private void HandleTouchScaling()
    {
        Touch touch1 = Input.GetTouch(0);
        Touch touch2 = Input.GetTouch(1);

        if (touch2.phase == TouchPhase.Began)
        {
            initialDistance = Vector2.Distance(touch1.position, touch2.position);
            initialScale = transform.localScale;
        }
        else if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
        {
            float currentDistance = Vector2.Distance(touch1.position, touch2.position);

            if (Mathf.Approximately(initialDistance, 0)) return;

            float scaleFactor = currentDistance / initialDistance;
            Vector3 newScale = initialScale * scaleFactor;

            // Ограничиваем размер
            newScale.x = Mathf.Clamp(newScale.x, minScale, maxScale);
            newScale.y = Mathf.Clamp(newScale.y, minScale, maxScale);
            newScale.z = 1f; // Для 2D объектов z обычно остается 1

            transform.localScale = newScale;
        }

        // Если пальцев меньше 2, масштабирование закончено
        if (Input.touchCount < 2)
        {
            IsScaling = false;
        }
    }

    private void HandleMouseEmulation()
    {
        // Начало масштабирования (зажали Ctrl + ЛКМ)
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButtonDown(0))
        {
            isMouseScaling = true;
            IsScaling = true;
            initialScale = transform.localScale;
            lastMousePosition = Input.mousePosition;
        }

        // Процесс масштабирования (Ctrl зажат и ЛКМ зажата)
        if (isMouseScaling && Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButton(0))
        {
            Vector2 currentMousePosition = Input.mousePosition;
            float deltaY = currentMousePosition.y - lastMousePosition.y;

            // Изменяем масштаб в зависимости от движения мыши вверх/вниз
            float scaleChange = deltaY * mouseZoomSpeed;

            Vector3 newScale = transform.localScale + new Vector3(scaleChange, scaleChange, 0);

            // Ограничиваем размер
            newScale.x = Mathf.Clamp(newScale.x, minScale, maxScale);
            newScale.y = Mathf.Clamp(newScale.y, minScale, maxScale);
            newScale.z = 1f;

            transform.localScale = newScale;

            lastMousePosition = currentMousePosition;
        }

        // Завершение масштабирования (отпустили ЛКМ или Ctrl)
        if (isMouseScaling && (!Input.GetMouseButton(0) || !Input.GetKey(KeyCode.LeftControl)))
        {
            isMouseScaling = false;
            IsScaling = false;
        }
    }

    // Дополнительный метод для сброса масштаба к начальному
    public void ResetScale()
    {
        transform.localScale = Vector3.one;
    }

    // Дополнительный метод для установки конкретного масштаба
    public void SetScale(float newScale)
    {
        float clampedScale = Mathf.Clamp(newScale, minScale, maxScale);
        transform.localScale = new Vector3(clampedScale, clampedScale, 1f);
    }
}