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
    private Vector2 lastMousePosition;

    // Флаги состояния
    private bool isScaling = false;
    private bool isMouseScaling = false;

    // Ссылка на компонент перемещения
    private TestMousePosition mousePositionComponent;

    // Публичные свойства
    public bool IsScaling
    {
        get { return isScaling; }
        private set { isScaling = value; }
    }

    public bool IsActive { get; set; } = false;

    void Start()
    {
        mainCamera = Camera.main;

        // Пытаемся найти компонент перемещения на этом же объекте
        mousePositionComponent = GetComponent<TestMousePosition>();
        if (mousePositionComponent == null)
        {
            Debug.LogWarning($"TestMousePosition component not found on {gameObject.name}. ScaleObject will work independently.");
        }

        Debug.Log($"ScaleObject initialized on {gameObject.name}");
    }

    void Update()
    {
        // Для отладки - показываем состояние объекта
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log($"{gameObject.name} - IsActive: {IsActive}, IsScaling: {isScaling}, IsMouseScaling: {isMouseScaling}");
        }

        // Проверяем наличие сенсорного ввода
        if (Input.touchSupported)
        {
            HandleTouchInput();
        }
        else
        {
            HandleMouseInput();
        }
    }

    private void HandleTouchInput()
    {
        // Мультитач для устройств с поддержкой касаний
        if (Input.touchCount == 2)
        {
            // Проверяем, касаются ли пальцы этого объекта
            if (IsTouchingThisObject())
            {
                if (!isScaling)
                {
                    StartScaling("touch");
                }

                HandleTouchScaling();
            }
            else if (isScaling)
            {
                // Если пальцы не на этом объекте, но мы в режиме масштабирования - выходим
                StopScaling();
            }
        }
        else
        {
            // Если не масштабируем, но были в режиме масштабирования - завершаем
            if (isScaling)
            {
                StopScaling();
            }

            // Проверяем эмуляцию мышью
            HandleMouseInput();
        }
    }

    private bool IsTouchingThisObject()
    {
        if (Input.touchCount < 2) return false;

        // Проверяем, касается ли хотя бы один из пальцев этого объекта
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            Vector3 touchPos = mainCamera.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, Mathf.Abs(mainCamera.transform.position.z - transform.position.z)));

            Collider2D hit = Physics2D.OverlapPoint(touchPos);
            if (hit != null && hit.gameObject == gameObject)
            {
                return true;
            }
        }

        return false;
    }

    private void HandleMouseInput()
    {
        // Для отладки координат мыши
        if (Input.GetKeyDown(KeyCode.M))
        {
            Vector3 mousePos = GetMouseWorldPosition();
            Collider2D hit = Physics2D.OverlapPoint(mousePos);
            Debug.Log($"Mouse at {mousePos}, hit: {(hit != null ? hit.gameObject.name : "none")}");
        }

        // Начало масштабирования (зажали Ctrl + ЛКМ)
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButtonDown(0))
        {
            // Проверяем, есть ли под мышкой этот объект
            Vector3 mousePos = GetMouseWorldPosition();
            Collider2D hit = Physics2D.OverlapPoint(mousePos);

            Debug.Log($"MouseDown - Ctrl held, checking object under mouse: {(hit != null ? hit.gameObject.name : "none")}");

            if (hit != null && hit.gameObject == gameObject)
            {
                Debug.Log($"Starting mouse scaling on {gameObject.name}");

                // Начинаем масштабирование мышью
                if (!isScaling)
                {
                    StartScaling("mouse");
                }

                isMouseScaling = true;
                initialScale = transform.localScale;
                lastMousePosition = Input.mousePosition;
            }
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

            Debug.Log($"Scaling {gameObject.name} - new scale: {newScale.x}");
        }

        // Завершение масштабирования (отпустили ЛКМ или Ctrl)
        if (isMouseScaling && (!Input.GetMouseButton(0) || !Input.GetKey(KeyCode.LeftControl)))
        {
            Debug.Log($"Mouse scaling ended on {gameObject.name}");
            isMouseScaling = false;

            // Если нет активных касаний - завершаем масштабирование
            if (Input.touchCount < 2)
            {
                StopScaling();
            }
        }
    }

    private void StartScaling(string source)
    {
        isScaling = true;

        Debug.Log($"Started scaling {gameObject.name} via {source}, IsActive: {IsActive}");
    }

    private void StopScaling()
    {
        if (isScaling)
        {
            Debug.Log($"Stopped scaling {gameObject.name}");

            isScaling = false;
            isMouseScaling = false;

            // Уведомляем компонент перемещения, что масштабирование закончилось
            if (mousePositionComponent != null)
            {
                //mousePositionComponent.OnStopScaling();
            }
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
            Debug.Log($"Touch scaling started on {gameObject.name}, initial distance: {initialDistance}");
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
            newScale.z = 1f;

            transform.localScale = newScale;

            Debug.Log($"Touch scaling {gameObject.name} - factor: {scaleFactor}, new scale: {newScale.x}");
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Mathf.Abs(mainCamera.transform.position.z - transform.position.z);
        return mainCamera.ScreenToWorldPoint(mousePosition);
    }

    public void ResetScale()
    {
        transform.localScale = Vector3.one;
    }

    public void SetScale(float newScale)
    {
        float clampedScale = Mathf.Clamp(newScale, minScale, maxScale);
        transform.localScale = new Vector3(clampedScale, clampedScale, 1f);
    }

    public void ForceStopScaling()
    {
        if (isScaling)
        {
            Debug.Log($"Force stopped scaling {gameObject.name}");
            isScaling = false;
            isMouseScaling = false;
        }
    }

    private void OnDisable()
    {
        ForceStopScaling();
    }

    private void OnMouseEnter()
    {
        // Для отладки - показываем когда мышь над объектом
        // Debug.Log($"Mouse entered {gameObject.name}");
    }

    private void OnMouseExit()
    {
        // Для отладки - показываем когда мышь покидает объект
        // Debug.Log($"Mouse exited {gameObject.name}");
    }
}