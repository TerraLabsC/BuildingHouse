using UnityEngine;
using System.Collections.Generic;

public class TestMousePosition : MonoBehaviour
{
    private Vector3 offset;
    private Camera mainCamera;
    private static TestMousePosition currentDraggedObject; // Статическая ссылка на перетаскиваемый объект

    [Header("Magnet Settings")]
    [SerializeField] private LayerMask buildingsLayer; // Слой Buildings (второй объект)
    [SerializeField] private LayerMask roofsLayer; // Слой Roofs (первый объект)
    [SerializeField] private float magnetDistance = 2f; // Дистанция для магнитной привязки
    [SerializeField] private bool showMagnetGizmo = true; // Показывать ли гизмо
    [SerializeField] private bool snapWhileDragging = true; // Включить/выключить привязку при перетаскивании

    [Header("Attachment Point")]
    [SerializeField] private Vector3 attachmentPoint = Vector3.zero; // Точка крепления относительно центра объекта
    [SerializeField] private bool showAttachmentGizmo = true; // Показывать ли точку крепления
    [SerializeField] private Color attachmentGizmoColor = Color.blue; // Цвет точки крепления

    // Компоненты для сортировки
    private SpriteRenderer spriteRenderer;
    private Canvas canvas;

    // Мировая позиция точки крепления
    public Vector3 WorldAttachmentPoint
    {
        get { return transform.position + attachmentPoint; }
    }

    void Start()
    {
        mainCamera = Camera.main;
        // Устанавливаем слои по умолчанию
        if (buildingsLayer == 0)
            buildingsLayer = LayerMask.GetMask("Buildings");
        if (roofsLayer == 0)
            roofsLayer = LayerMask.GetMask("Roofs");

        // Получаем компоненты для сортировки
        spriteRenderer = GetComponent<SpriteRenderer>();
        canvas = GetComponent<Canvas>();
    }

    void Update()
    {
        // Обрабатываем нажатие мыши вручную, чтобы учитывать сортировку
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseDown();
        }

        // Обрабатываем перетаскивание только для текущего объекта
        if (Input.GetMouseButton(0) && currentDraggedObject == this)
        {
            HandleMouseDrag();
        }

        // Обрабатываем отпускание мыши
        if (Input.GetMouseButtonUp(0) && currentDraggedObject == this)
        {
            currentDraggedObject = null;
        }
    }

    private void HandleMouseDown()
    {
        Vector3 mousePos = GetMouseWorldPosition();

        // Находим все объекты под мышью
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);

        if (hits.Length == 0) return;

        // Сортируем объекты по приоритету (сначала UI, потом по Order in Layer)
        System.Array.Sort(hits, (a, b) => {
            // Получаем компоненты для сортировки
            SpriteRenderer spriteA = a.collider.GetComponent<SpriteRenderer>();
            SpriteRenderer spriteB = b.collider.GetComponent<SpriteRenderer>();
            Canvas canvasA = a.collider.GetComponent<Canvas>();
            Canvas canvasB = b.collider.GetComponent<Canvas>();

            // Приоритет у Canvas (UI)
            if (canvasA != null && canvasB == null) return -1;
            if (canvasA == null && canvasB != null) return 1;

            // Сортируем по Order in Layer (чем больше - тем выше)
            int orderA = spriteA != null ? spriteA.sortingOrder : 0;
            int orderB = spriteB != null ? spriteB.sortingOrder : 0;

            // Сортируем по убыванию (больший order сверху)
            return orderB.CompareTo(orderA);
        });

        // Проверяем, есть ли наш объект среди hits и является ли он самым верхним
        bool isTopmost = false;
        foreach (var hit in hits)
        {
            if (hit.collider.gameObject == gameObject)
            {
                isTopmost = true;
                break;
            }
            // Если встретили другой объект с коллайдером до нашего - наш не сверху
            else if (hit.collider.gameObject != gameObject)
            {
                // Проверяем, есть ли у этого объекта коллайдер и он не триггер
                if (hit.collider.enabled)
                {
                    // Если этот объект имеет больший или равный приоритет - наш не сверху
                    SpriteRenderer otherSprite = hit.collider.GetComponent<SpriteRenderer>();
                    SpriteRenderer thisSprite = GetComponent<SpriteRenderer>();

                    int otherOrder = otherSprite != null ? otherSprite.sortingOrder : 0;
                    int thisOrder = thisSprite != null ? thisSprite.sortingOrder : 0;

                    if (otherOrder >= thisOrder)
                    {
                        isTopmost = false;
                        break;
                    }
                }
            }
        }

        // Если наш объект самый верхний - начинаем перетаскивание
        if (isTopmost)
        {
            currentDraggedObject = this;
            offset = WorldAttachmentPoint - mousePos;

            // Небольшая вибрация или эффект для обратной связи
            Debug.Log($"Started dragging {gameObject.name} with Order in Layer: {(spriteRenderer != null ? spriteRenderer.sortingOrder : 0)}");
        }
    }

    private void HandleMouseDrag()
    {
        Vector3 targetPosition = GetMouseWorldPosition() + offset;

        // Вычисляем новую позицию объекта с учетом точки крепления
        Vector3 newObjectPosition = targetPosition - attachmentPoint;

        // Проверяем магнитную привязку к слоям
        if (snapWhileDragging)
        {
            // Сначала пробуем примагнититься к Roofs
            Vector3 magnetizedAttachmentPoint = GetMagnetizedPosition(targetPosition, roofsLayer, false);

            // Если не нашли точку на Roofs, пробуем примагнититься к Buildings
            if (magnetizedAttachmentPoint == targetPosition)
            {
                magnetizedAttachmentPoint = GetMagnetizedPosition(targetPosition, buildingsLayer, false);
            }

            // Пересчитываем позицию объекта с учетом примагниченной точки крепления
            newObjectPosition = magnetizedAttachmentPoint - attachmentPoint;
        }

        transform.position = newObjectPosition;
    }

    private Vector3 GetMagnetizedPosition(Vector3 position, LayerMask targetLayer, bool useGlobalSearch = false)
    {
        float searchDistance = useGlobalSearch ? Mathf.Infinity : magnetDistance;

        // Поиск ближайшей точки на целевом слое
        Collider2D[] targets;

        if (useGlobalSearch)
        {
            // Ищем все коллайдеры на сцене
            targets = FindObjectsOfType<Collider2D>();
            // Фильтруем только нужный слой
            List<Collider2D> filteredTargets = new List<Collider2D>();
            int layerNumber = GetLayerNumber(targetLayer);

            foreach (var target in targets)
            {
                if (target.gameObject.layer == layerNumber)
                {
                    filteredTargets.Add(target);
                }
            }
            targets = filteredTargets.ToArray();
        }
        else
        {
            targets = Physics2D.OverlapCircleAll(position, searchDistance, targetLayer);
        }

        if (targets.Length > 0)
        {
            float closestDistance = float.MaxValue;
            Vector3 closestPoint = position;

            foreach (var target in targets)
            {
                // Получаем ближайшую точку на коллайдере
                Vector3 closestPointOnTarget = target.ClosestPoint(position);
                float distance = Vector3.Distance(position, closestPointOnTarget);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPoint = closestPointOnTarget;
                }
            }

            // Если нашли точку в пределах magnetDistance или используем глобальный поиск
            if (useGlobalSearch || closestDistance <= magnetDistance)
            {
                // Учитываем Z координату для правильного позиционирования
                closestPoint.z = transform.position.z;
                return closestPoint;
            }
        }

        return position;
    }

    private int GetLayerNumber(LayerMask layerMask)
    {
        // Конвертируем LayerMask в номер слоя
        int layerNumber = 0;
        int layer = layerMask.value;
        while (layer > 1)
        {
            layer = layer >> 1;
            layerNumber++;
        }
        return layerNumber;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Mathf.Abs(mainCamera.transform.position.z - transform.position.z);
        return mainCamera.ScreenToWorldPoint(mousePosition);
    }

    // Получить порядок сортировки объекта
    private int GetSortingOrder()
    {
        if (spriteRenderer != null)
            return spriteRenderer.sortingOrder;
        if (canvas != null)
            return canvas.sortingOrder;
        return 0;
    }

    // Отрисовка гизмо
    private void OnDrawGizmos()
    {
        if (!showMagnetGizmo) return;

        // Получаем ссылку на камеру если её нет
        if (mainCamera == null)
            mainCamera = Camera.main;

        // Рисуем точку крепления (синяя)
        if (showAttachmentGizmo)
        {
            Gizmos.color = attachmentGizmoColor;
            Gizmos.DrawSphere(WorldAttachmentPoint, 0.2f);

            // Рисуем линию от центра объекта до точки крепления
            Gizmos.color = new Color(attachmentGizmoColor.r, attachmentGizmoColor.g, attachmentGizmoColor.b, 0.5f);
            Gizmos.DrawLine(transform.position, WorldAttachmentPoint);

#if UNITY_EDITOR
            UnityEditor.Handles.color = attachmentGizmoColor;
            UnityEditor.Handles.Label(WorldAttachmentPoint + Vector3.up * 0.3f, "Attachment Point");
#endif
        }

        // Рисуем радиус магнитной привязки от точки крепления
        Gizmos.color = new Color(0, 1, 0, 0.1f);
        Gizmos.DrawWireSphere(WorldAttachmentPoint, magnetDistance);

        // Визуализация всех объектов на слоях
        VisualizeLayer(roofsLayer, Color.yellow, "Roofs");
        VisualizeLayer(buildingsLayer, Color.green, "Buildings");

        // Отображаем порядок сортировки для отладки
#if UNITY_EDITOR
        UnityEditor.Handles.color = Color.white;
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, $"Order: {GetSortingOrder()}");
#endif
    }

    private void VisualizeLayer(LayerMask targetLayer, Color gizmoColor, string layerName)
    {
        if (targetLayer == 0) return;

        Collider2D[] allColliders = FindObjectsOfType<Collider2D>();
        int layerNumber = GetLayerNumber(targetLayer);

        foreach (var collider in allColliders)
        {
            if (collider != null && collider.gameObject.layer == layerNumber)
            {
                // Рисуем контур объектов
                Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.2f);

                if (collider is BoxCollider2D boxCollider)
                {
                    Vector3 center = boxCollider.bounds.center;
                    Vector3 size = boxCollider.bounds.size;
                    Gizmos.DrawWireCube(center, size);
                }
                else if (collider is CircleCollider2D circleCollider)
                {
                    Gizmos.DrawWireSphere(circleCollider.bounds.center, circleCollider.radius);
                }
                else if (collider is PolygonCollider2D polygonCollider)
                {
                    // Для полигональных коллайдеров рисуем приблизительный контур
                    Vector3 center = polygonCollider.bounds.center;
                    Vector3 size = polygonCollider.bounds.size;
                    Gizmos.DrawWireCube(center, size);
                }

                // Показываем ближайшую точку к позиции мыши (для наглядности)
                if (Application.isPlaying && mainCamera != null)
                {
                    Vector3 mousePos = GetMouseWorldPosition();
                    Vector3 closestPoint = collider.ClosestPoint(mousePos);
                    float distanceToMouse = Vector3.Distance(mousePos, closestPoint);

                    Gizmos.color = distanceToMouse <= magnetDistance ? gizmoColor : Color.gray;
                    Gizmos.DrawSphere(closestPoint, 0.1f);
                }
            }
        }
    }

    // Для отрисовки в редакторе когда объект выбран
    private void OnDrawGizmosSelected()
    {
        if (!showMagnetGizmo) return;

        // Точка крепления
        Gizmos.color = attachmentGizmoColor;
        Gizmos.DrawSphere(WorldAttachmentPoint, 0.25f);
        Gizmos.DrawLine(transform.position, WorldAttachmentPoint);

        // Более яркий радиус
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(WorldAttachmentPoint, magnetDistance);
    }
}