using UnityEngine;
using System.Collections.Generic;

public class TestMousePosition : MonoBehaviour
{
    private Vector3 offset;
    private Camera mainCamera;
    private static TestMousePosition currentDraggedObject; // Оставляем статическим для отслеживания перетаскиваемого объекта

    [Header("Magnet Settings")]
    [SerializeField] private LayerMask buildingsLayer;
    [SerializeField] private LayerMask roofsLayer;
    [SerializeField] private float magnetDistance = 2f;
    [SerializeField] private bool showMagnetGizmo = true;
    [SerializeField] private bool snapWhileDragging = true;

    [Header("Attachment Point Settings")]
    [SerializeField] private AttachmentPointType attachmentPointType = AttachmentPointType.Custom;
    [SerializeField] private Vector3 customAttachmentPoint = Vector3.zero;
    [SerializeField] private bool showAttachmentGizmo = true;
    [SerializeField] private Color attachmentGizmoColor = Color.blue;

    public enum AttachmentPointType
    {
        Custom,
        BottomCenter,
        TopCenter
    }

    private SpriteRenderer spriteRenderer;
    private Canvas canvas;
    private Collider2D objectCollider;

    // Флаг, указывающий, что сейчас идет масштабирование (управляется ScaleObject)
    private bool isScaling = false;

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

    public Vector3 WorldAttachmentPoint
    {
        get { return transform.position + AttachmentPoint; }
    }

    void Start()
    {
        mainCamera = Camera.main;
        objectCollider = GetComponent<Collider2D>();

        if (objectCollider == null)
        {
            Debug.LogWarning($"No Collider2D found on {gameObject.name}");
        }

        if (buildingsLayer == 0)
            buildingsLayer = LayerMask.GetMask("Buildings");
        if (roofsLayer == 0)
            roofsLayer = LayerMask.GetMask("Roofs");

        spriteRenderer = GetComponent<SpriteRenderer>();
        canvas = GetComponent<Canvas>();
    }

    void Update()
    {

        // Обрабатываем нажатие мыши
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseDown();
        }

        // Обрабатываем перетаскивание только для текущего объекта и только если не идет масштабирование
        if (Input.GetMouseButton(0) && currentDraggedObject == this)
        {
            // Проверяем, не идет ли масштабирование и зажат ли Ctrl
            bool ctrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

            if (!isScaling && !ctrlHeld)
            {
                HandleMouseDrag();
            }
            else if (isScaling)
            {
                // Если идет масштабирование - не двигаем объект
                // Можно добавить визуальный эффект
            }
        }

        // Обрабатываем отпускание мыши
        if (Input.GetMouseButtonUp(0) && currentDraggedObject == this)
        {
            StopDragging();
        }

    }

    private void HandleMouseDown()
    {
        Vector3 mousePos = GetMouseWorldPosition();

        // Находим все объекты под мышью
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);

        if (hits.Length == 0) return;

        // Сортируем объекты по приоритету
        System.Array.Sort(hits, (a, b) => {
            SpriteRenderer spriteA = a.collider.GetComponent<SpriteRenderer>();
            SpriteRenderer spriteB = b.collider.GetComponent<SpriteRenderer>();
            Canvas canvasA = a.collider.GetComponent<Canvas>();
            Canvas canvasB = b.collider.GetComponent<Canvas>();

            if (canvasA != null && canvasB == null) return -1;
            if (canvasA == null && canvasB != null) return 1;

            int orderA = spriteA != null ? spriteA.sortingOrder : 0;
            int orderB = spriteB != null ? spriteB.sortingOrder : 0;

            return orderB.CompareTo(orderA);
        });

        // Проверяем, является ли наш объект самым верхним
        bool isTopmost = false;
        foreach (var hit in hits)
        {
            if (hit.collider.gameObject == gameObject)
            {
                isTopmost = true;
                break;
            }
            else if (hit.collider.gameObject != gameObject && hit.collider.enabled)
            {
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

        if (isTopmost)
        {
            StartDragging(mousePos);
        }
    }

    private void StartDragging(Vector3 mousePos)
    {
        // Если есть другой перетаскиваемый объект, завершаем его перетаскивание
        if (currentDraggedObject != null && currentDraggedObject != this)
        {
            Debug.Log($"Another object {currentDraggedObject.gameObject.name} is being dragged, stopping it");
            currentDraggedObject.StopDragging();
        }

        currentDraggedObject = this;

        Vector3 currentAttachmentPoint = WorldAttachmentPoint;
        offset = currentAttachmentPoint - mousePos;

        Debug.Log($"Started dragging {gameObject.name}");
    }

    private void StopDragging()
    {
        if (currentDraggedObject == this)
        {
            currentDraggedObject = null;
        }

        Debug.Log($"Stopped dragging {gameObject.name}");
    }

    private void HandleMouseDrag()
    {
        Vector3 targetPosition = GetMouseWorldPosition() + offset;

        if (snapWhileDragging)
        {
            Vector3 magnetizedAttachmentPoint = GetMagnetizedPosition(targetPosition, roofsLayer, false);

            if (magnetizedAttachmentPoint == targetPosition)
            {
                magnetizedAttachmentPoint = GetMagnetizedPosition(targetPosition, buildingsLayer, false);
            }

            targetPosition = magnetizedAttachmentPoint;
        }

        Vector3 newObjectPosition = targetPosition - AttachmentPoint;
        newObjectPosition.z = transform.position.z;

        transform.position = newObjectPosition;
    }

    // Методы, вызываемые из ScaleObject
    public void OnStartScaling()
    {
        isScaling = true;
        Debug.Log($"Scaling started on {gameObject.name} - dragging paused");
    }

    public void OnStopScaling()
    {
        isScaling = false;
        Debug.Log($"Scaling stopped on {gameObject.name} - dragging resumed");

        // Обновляем offset при возобновлении перетаскивания, чтобы учесть новый масштаб
        if (currentDraggedObject == this)
        {
            Vector3 mousePos = GetMouseWorldPosition();
            Vector3 currentAttachmentPoint = WorldAttachmentPoint;
            offset = currentAttachmentPoint - mousePos;
        }
    }

    private Vector3 GetMagnetizedPosition(Vector3 position, LayerMask targetLayer, bool useGlobalSearch = false)
    {
        float searchDistance = useGlobalSearch ? Mathf.Infinity : magnetDistance;

        Collider2D[] targets;

        if (useGlobalSearch)
        {
            targets = FindObjectsOfType<Collider2D>();
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
                Vector3 closestPointOnTarget = target.ClosestPoint(position);
                float distance = Vector3.Distance(position, closestPointOnTarget);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPoint = closestPointOnTarget;
                }
            }

            if (useGlobalSearch || closestDistance <= magnetDistance)
            {
                closestPoint.z = transform.position.z;
                return closestPoint;
            }
        }

        return position;
    }

    private int GetLayerNumber(LayerMask layerMask)
    {
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

    private int GetSortingOrder()
    {
        if (spriteRenderer != null)
            return spriteRenderer.sortingOrder;
        if (canvas != null)
            return canvas.sortingOrder;
        return 0;
    }

    private Vector3 GetBottomCenterLocalPoint()
    {
        if (objectCollider == null)
        {
            objectCollider = GetComponent<Collider2D>();
            if (objectCollider == null) return Vector3.zero;
        }

        Bounds bounds = GetColliderLocalBounds();
        return new Vector3(bounds.center.x, bounds.min.y, 0);
    }

    private Vector3 GetTopCenterLocalPoint()
    {
        if (objectCollider == null)
        {
            objectCollider = GetComponent<Collider2D>();
            if (objectCollider == null) return Vector3.zero;
        }

        Bounds bounds = GetColliderLocalBounds();
        return new Vector3(bounds.center.x, bounds.max.y, 0);
    }

    private Bounds GetColliderLocalBounds()
    {
        if (objectCollider == null)
        {
            return new Bounds(Vector3.zero, Vector3.zero);
        }

        if (objectCollider is BoxCollider2D boxCollider)
        {
            Vector3 size = Vector3.Scale(boxCollider.size, transform.localScale);
            Vector3 center = boxCollider.offset;
            return new Bounds(center, size);
        }
        else if (objectCollider is CircleCollider2D circleCollider)
        {
            float maxScale = Mathf.Max(transform.localScale.x, transform.localScale.y);
            float radius = circleCollider.radius * maxScale;
            Vector3 center = circleCollider.offset;
            return new Bounds(center, new Vector3(radius * 2, radius * 2, 0));
        }
        else if (objectCollider is PolygonCollider2D polygonCollider)
        {
            Bounds bounds = polygonCollider.bounds;
            Vector3 localMin = transform.InverseTransformPoint(bounds.min);
            Vector3 localMax = transform.InverseTransformPoint(bounds.max);
            Vector3 localCenter = (localMin + localMax) / 2f;
            Vector3 localSize = localMax - localMin;
            return new Bounds(localCenter, localSize);
        }
        else if (objectCollider is CapsuleCollider2D capsuleCollider)
        {
            Vector3 size = Vector3.Scale(capsuleCollider.size, transform.localScale);
            Vector3 center = capsuleCollider.offset;
            return new Bounds(center, size);
        }
        else if (objectCollider is EdgeCollider2D edgeCollider)
        {
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

        Bounds worldBounds = objectCollider.bounds;
        Vector3 worldMin = worldBounds.min;
        Vector3 worldMax = worldBounds.max;

        Vector3 localMinWorld = transform.InverseTransformPoint(worldMin);
        Vector3 localMaxWorld = transform.InverseTransformPoint(worldMax);
        Vector3 localCenterWorld = (localMinWorld + localMaxWorld) / 2f;
        Vector3 localSizeWorld = localMaxWorld - localMinWorld;

        return new Bounds(localCenterWorld, localSizeWorld);
    }

    public void UpdateAttachmentPointType(AttachmentPointType newType)
    {
        attachmentPointType = newType;

        if (newType != AttachmentPointType.Custom)
        {
            customAttachmentPoint = newType == AttachmentPointType.BottomCenter ?
                GetBottomCenterLocalPoint() : GetTopCenterLocalPoint();
        }
    }

    public Vector3 GetAttachmentPointLocalPosition()
    {
        return AttachmentPoint;
    }

    public Vector3 GetAttachmentPointWorldPosition()
    {
        return WorldAttachmentPoint;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            if (objectCollider == null)
                objectCollider = GetComponent<Collider2D>();
        }

        if (!showMagnetGizmo) return;

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (showAttachmentGizmo)
        {
            Vector3 worldAttachmentPoint = WorldAttachmentPoint;

            Gizmos.color = attachmentGizmoColor;
            Gizmos.DrawSphere(worldAttachmentPoint, 0.2f);

            Gizmos.color = new Color(attachmentGizmoColor.r, attachmentGizmoColor.g, attachmentGizmoColor.b, 0.5f);
            Gizmos.DrawLine(transform.position, worldAttachmentPoint);

#if UNITY_EDITOR
            UnityEditor.Handles.color = attachmentGizmoColor;
            UnityEditor.Handles.Label(worldAttachmentPoint + Vector3.up * 0.3f,
                $"Attachment Point: {attachmentPointType}");
#endif
        }

        Gizmos.color = new Color(0, 1, 0, 0.1f);
        Gizmos.DrawWireSphere(WorldAttachmentPoint, magnetDistance);

        VisualizeLayer(roofsLayer, Color.yellow, "Roofs");
        VisualizeLayer(buildingsLayer, Color.green, "Buildings");

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

    private void OnDrawGizmosSelected()
    {
        if (!showMagnetGizmo) return;

        Vector3 worldAttachmentPoint = WorldAttachmentPoint;
        Gizmos.color = attachmentGizmoColor;
        Gizmos.DrawSphere(worldAttachmentPoint, 0.25f);
        Gizmos.DrawLine(transform.position, worldAttachmentPoint);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(worldAttachmentPoint, magnetDistance);

        if (objectCollider != null)
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawWireCube(objectCollider.bounds.center, objectCollider.bounds.size);

            Vector3 bottomCenter = transform.position + GetBottomCenterLocalPoint();
            Vector3 topCenter = transform.position + GetTopCenterLocalPoint();

            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(bottomCenter, 0.15f);

            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(topCenter, 0.15f);
        }
    }
}