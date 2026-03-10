using UnityEngine;
using System.Collections.Generic;

public class TestMousePosition : MonoBehaviour
{
    private Vector3 offset;
    private Camera mainCamera;
    private static TestMousePosition currentDraggedObject; // Статическая ссылка на перетаскиваемый объект
    private ScaleObject scaleObject; // Ссылка на компонент масштабирования

    [Header("Magnet Settings")]
    [SerializeField] private LayerMask buildingsLayer; // Слой Buildings (второй объект)
    [SerializeField] private LayerMask roofsLayer; // Слой Roofs (первый объект)
    [SerializeField] private float magnetDistance = 2f; // Дистанция для магнитной привязки
    [SerializeField] private bool showMagnetGizmo = true; // Показывать ли гизмо
    [SerializeField] private bool snapWhileDragging = true; // Включить/выключить привязку при перетаскивании

    [Header("Attachment Point Settings")]
    [SerializeField] private AttachmentPointType attachmentPointType = AttachmentPointType.Custom;
    [SerializeField] private Vector3 customAttachmentPoint = Vector3.zero; // Точка крепления относительно центра объекта
    [SerializeField] private bool showAttachmentGizmo = true; // Показывать ли точку крепления
    [SerializeField] private Color attachmentGizmoColor = Color.blue; // Цвет точки крепления

    // Типы точек крепления
    public enum AttachmentPointType
    {
        Custom,         // Пользовательская точка
        BottomCenter,   // Центр низа коллайдера
        TopCenter       // Центр верха коллайдера
    }

    // Компоненты для сортировки
    private SpriteRenderer spriteRenderer;
    private Canvas canvas;
    private Collider2D objectCollider;

    // Точка крепления (возвращает актуальную позицию в локальных координатах)
    public Vector3 AttachmentPoint
    {
        get
        {
            switch (attachmentPointType)
            {
                case AttachmentPointType.BottomCenter:
                    return GetBottomCenterLocalPoint();
                case AttachmentPointType.TopCenter:
                    return GetTopCenterLocalPoint();
                case AttachmentPointType.Custom:
                default:
                    return customAttachmentPoint;
            }
        }
        set
        {
            if (attachmentPointType == AttachmentPointType.Custom)
                customAttachmentPoint = value;
        }
    }

    // Мировая позиция точки крепления
    public Vector3 WorldAttachmentPoint
    {
        get { return transform.position + AttachmentPoint; }
    }

    void Start()
    {
        mainCamera = Camera.main;
        scaleObject = GetComponent<ScaleObject>();

        // Получаем коллайдер
        objectCollider = GetComponent<Collider2D>();
        if (objectCollider == null)
        {
            Debug.LogWarning($"No Collider2D found on {gameObject.name}. Attachment points may not work correctly.");
        }

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

        // Обрабатываем перетаскивание только для текущего объекта и только если не идет масштабирование
        if (Input.GetMouseButton(0) && currentDraggedObject == this)
        {
            // Проверяем, не идет ли масштабирование
            if (scaleObject != null && !scaleObject.IsScaling)
            {
                HandleMouseDrag();
            }
        }

        // Обрабатываем отпускание мыши
        if (Input.GetMouseButtonUp(0) && currentDraggedObject == this)
        {
            if (scaleObject != null)
                scaleObject.IsActive = false;
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

            if (scaleObject != null)
                scaleObject.IsActive = true;

            // Сохраняем текущую точку крепления в момент начала перетаскивания
            Vector3 currentAttachmentPoint = WorldAttachmentPoint;
            offset = currentAttachmentPoint - mousePos;

            // Небольшая вибрация или эффект для обратной связи
            Debug.Log($"Started dragging {gameObject.name} with Order in Layer: {(spriteRenderer != null ? spriteRenderer.sortingOrder : 0)}");
        }
    }

    private void HandleMouseDrag()
    {
        Vector3 targetPosition = GetMouseWorldPosition() + offset;

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

            targetPosition = magnetizedAttachmentPoint;
        }

        // Вычисляем новую позицию объекта
        // ВАЖНО: Используем AttachmentPoint, который автоматически обновляется при масштабировании
        Vector3 newObjectPosition = targetPosition - AttachmentPoint;

        // Сохраняем Z координату
        newObjectPosition.z = transform.position.z;

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

    // Получить локальную точку центра низа коллайдера с учетом масштаба
    private Vector3 GetBottomCenterLocalPoint()
    {
        if (objectCollider == null)
        {
            objectCollider = GetComponent<Collider2D>();
            if (objectCollider == null) return Vector3.zero;
        }

        // Получаем границы коллайдера в локальных координатах
        Bounds bounds = GetColliderLocalBounds();

        // Центр низа: по X - центр, по Y - минимум
        return new Vector3(bounds.center.x, bounds.min.y, 0);
    }

    // Получить локальную точку центра верха коллайдера с учетом масштаба
    private Vector3 GetTopCenterLocalPoint()
    {
        if (objectCollider == null)
        {
            objectCollider = GetComponent<Collider2D>();
            if (objectCollider == null) return Vector3.zero;
        }

        // Получаем границы коллайдера в локальных координатах
        Bounds bounds = GetColliderLocalBounds();

        // Центр верха: по X - центр, по Y - максимум
        return new Vector3(bounds.center.x, bounds.max.y, 0);
    }

    // Получить границы коллайдера в локальных координатах (с учетом масштаба)
    private Bounds GetColliderLocalBounds()
    {
        if (objectCollider == null)
        {
            return new Bounds(Vector3.zero, Vector3.zero);
        }

        if (objectCollider is BoxCollider2D boxCollider)
        {
            // Для BoxCollider2D: размер умножаем на локальный масштаб
            Vector3 size = Vector3.Scale(boxCollider.size, transform.localScale);
            Vector3 center = boxCollider.offset;

            // Создаем bounds с учетом масштаба
            return new Bounds(center, size);
        }
        else if (objectCollider is CircleCollider2D circleCollider)
        {
            // Для CircleCollider2D: радиус умножаем на максимальный масштаб
            float maxScale = Mathf.Max(transform.localScale.x, transform.localScale.y);
            float radius = circleCollider.radius * maxScale;
            Vector3 center = circleCollider.offset;

            return new Bounds(center, new Vector3(radius * 2, radius * 2, 0));
        }
        else if (objectCollider is PolygonCollider2D polygonCollider)
        {
            // Для PolygonCollider2D используем bounds с учетом масштаба
            Bounds bounds = polygonCollider.bounds;

            // Конвертируем мировые границы в локальные
            Vector3 localMin = transform.InverseTransformPoint(bounds.min);
            Vector3 localMax = transform.InverseTransformPoint(bounds.max);
            Vector3 localCenter = (localMin + localMax) / 2f;
            Vector3 localSize = localMax - localMin;

            return new Bounds(localCenter, localSize);
        }
        else if (objectCollider is CapsuleCollider2D capsuleCollider)
        {
            // Для CapsuleCollider2D
            Vector3 size = Vector3.Scale(capsuleCollider.size, transform.localScale);
            Vector3 center = capsuleCollider.offset;

            return new Bounds(center, size);
        }
        else if (objectCollider is EdgeCollider2D edgeCollider)
        {
            // Для EdgeCollider2D вычисляем bounds по точкам
            Vector2[] points = edgeCollider.points;
            if (points.Length == 0) return new Bounds(Vector3.zero, Vector3.zero);

            float minX = float.MaxValue, maxX = float.MinValue;
            float minY = float.MaxValue, maxY = float.MinValue;

            foreach (Vector2 point in points)
            {
                Vector2 worldPoint = edgeCollider.transform.TransformPoint(point);
                Vector2 localPoint = transform.InverseTransformPoint(worldPoint);

                minX = Mathf.Min(minX, localPoint.x);
                maxX = Mathf.Max(maxX, localPoint.x);
                minY = Mathf.Min(minY, localPoint.y);
                maxY = Mathf.Max(maxY, localPoint.y);
            }

            Vector3 localCenter2 = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, 0);
            Vector3 localSize2 = new Vector3(maxX - minX, maxY - minY, 0);

            return new Bounds(localCenter2, localSize2);
        }

        // Если тип коллайдера не поддерживается, пробуем получить bounds через коллайдер
        Bounds worldBounds = objectCollider.bounds;
        Vector3 worldMin = worldBounds.min;
        Vector3 worldMax = worldBounds.max;

        // Конвертируем в локальные координаты
        Vector3 localMinWorld = transform.InverseTransformPoint(worldMin);
        Vector3 localMaxWorld = transform.InverseTransformPoint(worldMax);
        Vector3 localCenterWorld = (localMinWorld + localMaxWorld) / 2f;
        Vector3 localSizeWorld = localMaxWorld - localMinWorld;

        return new Bounds(localCenterWorld, localSizeWorld);
    }

    // Вспомогательный метод для обновления attachmentPoint в инспекторе
    public void UpdateAttachmentPointType(AttachmentPointType newType)
    {
        attachmentPointType = newType;

        // Если выбран не Custom, обновляем customAttachmentPoint для визуализации
        if (newType != AttachmentPointType.Custom)
        {
            // Это просто для отображения в инспекторе, реальное значение будет браться из свойств
            customAttachmentPoint = newType == AttachmentPointType.BottomCenter ?
                GetBottomCenterLocalPoint() : GetTopCenterLocalPoint();
        }
    }

    // Метод для получения позиции точки крепления в локальных координатах
    public Vector3 GetAttachmentPointLocalPosition()
    {
        return AttachmentPoint;
    }

    // Метод для получения позиции точки крепления в мировых координатах
    public Vector3 GetAttachmentPointWorldPosition()
    {
        return WorldAttachmentPoint;
    }

    // Отрисовка гизмо
    private void OnDrawGizmos()
    {
        // В режиме редактора обновляем коллайдер
        if (!Application.isPlaying)
        {
            if (objectCollider == null)
                objectCollider = GetComponent<Collider2D>();
        }

        if (!showMagnetGizmo) return;

        // Получаем ссылку на камеру если её нет
        if (mainCamera == null)
            mainCamera = Camera.main;

        // Рисуем точку крепления (синяя)
        if (showAttachmentGizmo)
        {
            // Получаем актуальную мировую позицию точки крепления
            Vector3 worldAttachmentPoint = WorldAttachmentPoint;

            Gizmos.color = attachmentGizmoColor;
            Gizmos.DrawSphere(worldAttachmentPoint, 0.2f);

            // Рисуем линию от центра объекта до точки крепления
            Gizmos.color = new Color(attachmentGizmoColor.r, attachmentGizmoColor.g, attachmentGizmoColor.b, 0.5f);
            Gizmos.DrawLine(transform.position, worldAttachmentPoint);

#if UNITY_EDITOR
            UnityEditor.Handles.color = attachmentGizmoColor;
            UnityEditor.Handles.Label(worldAttachmentPoint + Vector3.up * 0.3f,
                $"Attachment Point: {attachmentPointType}");
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
                else if (collider is CapsuleCollider2D capsuleCollider)
                {
                    Vector3 center = capsuleCollider.bounds.center;
                    Vector3 size = capsuleCollider.bounds.size;
                    Gizmos.DrawWireCube(center, size);
                }
                else if (collider is EdgeCollider2D edgeCollider)
                {
                    // Для EdgeCollider2D рисуем линию по точкам
                    Vector2[] points = edgeCollider.points;
                    if (points.Length > 1)
                    {
                        for (int i = 0; i < points.Length - 1; i++)
                        {
                            Vector3 worldPoint1 = edgeCollider.transform.TransformPoint(points[i]);
                            Vector3 worldPoint2 = edgeCollider.transform.TransformPoint(points[i + 1]);
                            Gizmos.DrawLine(worldPoint1, worldPoint2);
                        }
                    }
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
        Vector3 worldAttachmentPoint = WorldAttachmentPoint;
        Gizmos.color = attachmentGizmoColor;
        Gizmos.DrawSphere(worldAttachmentPoint, 0.25f);
        Gizmos.DrawLine(transform.position, worldAttachmentPoint);

        // Более яркий радиус
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(worldAttachmentPoint, magnetDistance);

        // Визуализируем границы коллайдера для отладки
        if (objectCollider != null)
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawWireCube(objectCollider.bounds.center, objectCollider.bounds.size);

            // Показываем центр низа и верха
            Vector3 bottomCenter = transform.position + GetBottomCenterLocalPoint();
            Vector3 topCenter = transform.position + GetTopCenterLocalPoint();

            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(bottomCenter, 0.15f);

            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(topCenter, 0.15f);
        }
    }
}