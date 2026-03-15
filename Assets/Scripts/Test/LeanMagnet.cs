using UnityEngine;
using System.Collections.Generic;

public class LeanMagnet : MonoBehaviour
{
    [Header("Magnet Settings")]
    [SerializeField] private LayerMask buildingsLayer;
    [SerializeField] private LayerMask roofsLayer;
    [SerializeField] private float magnetDistance = 2f;
    [SerializeField] private bool snapWhileDragging = true;
    [SerializeField] private bool showMagnetGizmo = true;

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

    private Collider2D objectCollider;
    private Transform cachedTransform;
    private Camera mainCamera;
    private bool isBeingDragged = false;

    // Свойства для доступа из других скриптов
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
        get { return cachedTransform.position + AttachmentPoint; }
    }

    private void Awake()
    {
        cachedTransform = transform;
        objectCollider = GetComponent<Collider2D>();
        mainCamera = Camera.main;

        if (buildingsLayer == 0)
            buildingsLayer = LayerMask.GetMask("Buildings");
        if (roofsLayer == 0)
            roofsLayer = LayerMask.GetMask("Roofs");
    }

    private void LateUpdate()
    {
        // Проверяем, перетаскивается ли объект (по нажатой мыши)
        CheckIfBeingDragged();

        // Если объект перетаскивается, применяем магнит
        if (isBeingDragged && snapWhileDragging)
        {
            ApplyMagnet();
        }
    }

    private void CheckIfBeingDragged()
    {
        isBeingDragged = false;

        // Проверяем для мобильных устройств (касания)
        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                Vector3 worldPos = GetTouchWorldPosition(touch);
                RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

                if (hit.collider != null && hit.collider.gameObject == gameObject)
                {
                    isBeingDragged = true;
                    break;
                }
            }
        }
        // Проверяем для мыши (в редакторе или на ПК)
        else if (Input.GetMouseButton(0))
        {
            Vector3 mousePos = GetMouseWorldPosition();
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                isBeingDragged = true;
            }
        }
    }

    private Vector3 GetTouchWorldPosition(Touch touch)
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        Vector3 screenPos = new Vector3(touch.position.x, touch.position.y,
            Mathf.Abs(mainCamera.transform.position.z - transform.position.z));

        return mainCamera.ScreenToWorldPoint(screenPos);
    }

    private Vector3 GetMouseWorldPosition()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Mathf.Abs(mainCamera.transform.position.z - transform.position.z);
        return mainCamera.ScreenToWorldPoint(mousePosition);
    }

    private void ApplyMagnet()
    {
        Vector3 worldAttachmentPoint = WorldAttachmentPoint;

        // Ищем ближайшую точку для магнита
        Vector3 magnetizedPoint = GetMagnetizedPosition(worldAttachmentPoint);

        if (magnetizedPoint != worldAttachmentPoint)
        {
            // Вычисляем смещение для объекта
            Vector3 offset = magnetizedPoint - worldAttachmentPoint;
            cachedTransform.position += offset;
        }
    }

    /// <summary>
    /// Публичный метод для получения позиции с учетом магнита
    /// </summary>
    public Vector3 GetMagnetizedPosition(Vector3 worldPosition)
    {
        // Сначала проверяем слой крыш
        Vector3 magnetizedPosition = GetMagnetizedPositionForLayer(worldPosition, roofsLayer);

        // Если не нашли на крышах, проверяем здания
        if (magnetizedPosition == worldPosition)
        {
            magnetizedPosition = GetMagnetizedPositionForLayer(worldPosition, buildingsLayer);
        }

        return magnetizedPosition;
    }

    private Vector3 GetMagnetizedPositionForLayer(Vector3 position, LayerMask targetLayer)
    {
        Collider2D[] targets = Physics2D.OverlapCircleAll(position, magnetDistance, targetLayer);

        if (targets.Length > 0)
        {
            float closestDistance = float.MaxValue;
            Vector3 closestPoint = position;

            foreach (var target in targets)
            {
                // Пропускаем свой собственный коллайдер
                if (target.gameObject == gameObject) continue;

                Vector3 closestPointOnTarget = target.ClosestPoint(position);
                float distance = Vector3.Distance(position, closestPointOnTarget);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPoint = closestPointOnTarget;
                }
            }

            if (closestDistance <= magnetDistance)
            {
                closestPoint.z = cachedTransform.position.z;
                return closestPoint;
            }
        }

        return position;
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
            Vector3 size = Vector3.Scale(boxCollider.size, cachedTransform.localScale);
            Vector3 center = boxCollider.offset;
            return new Bounds(center, size);
        }
        else if (objectCollider is CircleCollider2D circleCollider)
        {
            float maxScale = Mathf.Max(cachedTransform.localScale.x, cachedTransform.localScale.y);
            float radius = circleCollider.radius * maxScale;
            Vector3 center = circleCollider.offset;
            return new Bounds(center, new Vector3(radius * 2, radius * 2, 0));
        }
        else if (objectCollider is PolygonCollider2D polygonCollider)
        {
            Bounds bounds = polygonCollider.bounds;
            Vector3 localMin = cachedTransform.InverseTransformPoint(bounds.min);
            Vector3 localMax = cachedTransform.InverseTransformPoint(bounds.max);
            Vector3 localCenter = (localMin + localMax) / 2f;
            Vector3 localSize = localMax - localMin;
            return new Bounds(localCenter, localSize);
        }
        else if (objectCollider is CapsuleCollider2D capsuleCollider)
        {
            Vector3 size = Vector3.Scale(capsuleCollider.size, cachedTransform.localScale);
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
                Vector2 localPoint = cachedTransform.InverseTransformPoint(worldPoint);

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

        Vector3 localMinWorld = cachedTransform.InverseTransformPoint(worldMin);
        Vector3 localMaxWorld = cachedTransform.InverseTransformPoint(worldMax);
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

    private void OnDrawGizmos()
    {
        if (!showMagnetGizmo) return;

        if (objectCollider == null && !Application.isPlaying)
            objectCollider = GetComponent<Collider2D>();

        if (cachedTransform == null)
            cachedTransform = transform;

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (showAttachmentGizmo)
        {
            Vector3 worldAttachmentPoint = WorldAttachmentPoint;

            Gizmos.color = attachmentGizmoColor;
            Gizmos.DrawSphere(worldAttachmentPoint, 0.2f);

            Gizmos.color = new Color(attachmentGizmoColor.r, attachmentGizmoColor.g, attachmentGizmoColor.b, 0.5f);
            Gizmos.DrawLine(cachedTransform.position, worldAttachmentPoint);

#if UNITY_EDITOR
            UnityEditor.Handles.color = attachmentGizmoColor;
            UnityEditor.Handles.Label(worldAttachmentPoint + Vector3.up * 0.3f,
                $"Attachment Point: {attachmentPointType}");
#endif
        }

        // Рисуем радиус магнита
        Gizmos.color = new Color(0, 1, 0, 0.1f);
        Gizmos.DrawWireSphere(WorldAttachmentPoint, magnetDistance);
    }

    private void OnDrawGizmosSelected()
    {
        if (!showMagnetGizmo) return;

        Vector3 worldAttachmentPoint = WorldAttachmentPoint;

        // Детальная визуализация при выделении
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