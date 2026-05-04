using UnityEngine;

public class CornerDragHandler : MonoBehaviour
{
    public enum CornerType { TopLeft, TopRight, BottomRight, BottomLeft }
    [HideInInspector] public CornerType cornerType = CornerType.TopLeft;

    [SerializeField] private Transform targetObject;
    [SerializeField] private Camera cam;                  // если не указана, используется Camera.main
    [SerializeField] private LayerMask cornerLayerMask;   // слой, на котором находятся все уголки

    private CircleCollider2D col;
    public bool isDragging;
    private Vector3 oppositeWorldOnGrab;
    private Vector3 initialHalfSize;
    private Vector3 initialScale;

    private void Awake()
    {
        if (cam == null)
            cam = Camera.main;

        cornerLayerMask = 1 << 13;

        col = GetComponent<CircleCollider2D>();

        if (targetObject == null)
        {
            // предполагаемая структура: Corner > Container > TargetObject
            if (transform.parent != null && transform.parent.parent != null)
                targetObject = transform.parent.parent;
            else
                Debug.LogError($"Угол {name}: целевой объект не найден.", this);
        }
    }

    private void Update()
    {
        if (cam == null || col == null || targetObject == null)
            return;

        // Луч из камеры через мышь, проверяем ТОЛЬКО слой уголков
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, cornerLayerMask);

        // Попали ли именно в этот уголок?
        bool hitThisCorner = hit.collider != null && hit.collider == col;

        if (Input.GetMouseButtonDown(0) && hitThisCorner)
        {
            StartDrag();
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            DragCorner(GetMouseWorldPoint());
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
        }
    }

    private Vector3 GetMouseWorldPoint()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = cam.WorldToScreenPoint(oppositeWorldOnGrab).z;
        return cam.ScreenToWorldPoint(mousePos);
    }

    private void StartDrag()
    {
        isDragging = true;

        // Противоположный угол (центр объекта считается точкой (0,0) в локальных координатах)
        Vector3 cornerWorld = transform.position;
        Vector3 cornerLocal = targetObject.InverseTransformPoint(cornerWorld);
        Vector3 oppositeLocal = -cornerLocal;
        oppositeWorldOnGrab = targetObject.TransformPoint(oppositeLocal);

        initialHalfSize = (cornerWorld - oppositeWorldOnGrab) * 0.5f;
        initialScale = targetObject.localScale;
    }

    private void DragCorner(Vector3 mouseWorld)
    {
        mouseWorld.z = oppositeWorldOnGrab.z;

        Vector3 offset = mouseWorld - oppositeWorldOnGrab;
        Vector3 direction = initialHalfSize.normalized;

        // Проекция смещения на исходную диагональ
        float projectedDiagonal = Vector3.Dot(offset, direction);
        Vector3 newHalfSize = direction * (projectedDiagonal / 2f);

        float minScale = 0.05f;
        float scaleFactor = Mathf.Max(newHalfSize.magnitude / initialHalfSize.magnitude, minScale);

        // Чтобы избежать «перескока» знака, масштабируем исходный halfSize
        newHalfSize = direction * (initialHalfSize.magnitude * scaleFactor);

        targetObject.position = oppositeWorldOnGrab + newHalfSize;
        targetObject.localScale = new Vector3(
            initialScale.x * scaleFactor,
            initialScale.y * scaleFactor,
            initialScale.z
        );
    }
}