using UnityEngine;

public class CornerDragHandler : MonoBehaviour
{
    public enum CornerType { TopLeft, TopRight, BottomRight, BottomLeft }
    [HideInInspector] public CornerType cornerType = CornerType.TopLeft;

    [SerializeField] private Transform targetObject;
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask cornerLayerMask;

    private CircleCollider2D col;
    public bool isDragging;
    private Vector3 oppositeWorldOnGrab;
    private Vector3 initialHalfSize;
    private Vector3 initialScale;

    private int dragFingerId = -1;

    // НОВОЕ: ссылка на родительский DragObject3D
    private DragObject3D dragObject;

    private void Awake()
    {
        if (cam == null)
            cam = Camera.main;

        cornerLayerMask = 1 << 13;

        col = GetComponent<CircleCollider2D>();

        if (targetObject == null)
        {
            if (transform.parent != null && transform.parent.parent != null)
                targetObject = transform.parent.parent;
            else
                Debug.LogError($"Угол {name}: целевой объект не найден.", this);
        }

        // НОВОЕ: получаем DragObject3D с целевого объекта (родителя контейнера)
        if (targetObject != null)
            dragObject = targetObject.GetComponent<DragObject3D>();
    }

    private void Update()
    {
        if (cam == null || col == null || targetObject == null)
            return;

        if (!isDragging)
            TryStartDrag();
        else
            TryUpdateDrag();
    }

    private void TryStartDrag()
    {
        Vector2 screenPos = Vector2.zero;
        bool began = false;
        int newFingerId = -1;

        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began)
                {
                    Ray ray = cam.ScreenPointToRay(touch.position);
                    RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, cornerLayerMask);

                    if (hit.collider != null && hit.collider == col)
                    {
                        screenPos = touch.position;
                        began = true;
                        newFingerId = touch.fingerId;
                        break;
                    }
                }
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            screenPos = Input.mousePosition;
            Ray ray = cam.ScreenPointToRay(screenPos);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, cornerLayerMask);

            if (hit.collider != null && hit.collider == col)
            {
                began = true;
                newFingerId = -1;
            }
        }

        if (began)
        {
            dragFingerId = newFingerId;
            StartDrag(screenPos);
        }
    }

    private void TryUpdateDrag()
    {
        Vector2 screenPos = Vector2.zero;
        bool inputHeld = false;
        bool inputEnded = false;

        if (dragFingerId >= 0)
        {
            if (Input.touchCount > 0)
            {
                bool found = false;
                foreach (Touch touch in Input.touches)
                {
                    if (touch.fingerId == dragFingerId)
                    {
                        found = true;
                        if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                        {
                            screenPos = touch.position;
                            inputHeld = true;
                        }
                        else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                        {
                            inputEnded = true;
                        }
                        break;
                    }
                }
                if (!found)
                    inputEnded = true;
            }
            else
            {
                inputEnded = true;
            }
        }
        else
        {
            if (Input.GetMouseButton(0))
            {
                screenPos = Input.mousePosition;
                inputHeld = true;
            }
            else
            {
                inputEnded = true;
            }
        }

        if (inputEnded)
        {
            isDragging = false;
            dragFingerId = -1;
            // НОВОЕ: разрешаем перетаскивание родительскому DragObject3D
            if (dragObject != null)
                dragObject.allowDrag = true;
            return;
        }

        if (inputHeld)
        {
            Vector3 worldPoint = GetWorldPointFromScreenPos(screenPos);
            DragCorner(worldPoint);
        }
    }

    private void StartDrag(Vector2 screenPos)
    {
        isDragging = true;

        // НОВОЕ: запрещаем перетаскивание родительскому DragObject3D
        if (dragObject != null)
            dragObject.allowDrag = false;

        Vector3 mouseWorld = GetWorldPointFromScreenPos(screenPos);

        Vector3 cornerWorld = transform.position;
        Vector3 cornerLocal = targetObject.InverseTransformPoint(cornerWorld);
        Vector3 oppositeLocal = -cornerLocal;
        oppositeWorldOnGrab = targetObject.TransformPoint(oppositeLocal);

        initialHalfSize = (cornerWorld - oppositeWorldOnGrab) * 0.5f;
        initialScale = targetObject.localScale;
    }

    private Vector3 GetWorldPointFromScreenPos(Vector2 screenPos)
    {
        Vector3 mousePos = screenPos;
        mousePos.z = cam.WorldToScreenPoint(oppositeWorldOnGrab).z;
        return cam.ScreenToWorldPoint(mousePos);
    }

    private void DragCorner(Vector3 mouseWorld)
    {
        mouseWorld.z = oppositeWorldOnGrab.z;

        Vector3 offset = mouseWorld - oppositeWorldOnGrab;
        Vector3 direction = initialHalfSize.normalized;

        float projectedDiagonal = Vector3.Dot(offset, direction);
        Vector3 newHalfSize = direction * (projectedDiagonal / 2f);

        float minScale = 0.05f;
        float scaleFactor = Mathf.Max(newHalfSize.magnitude / initialHalfSize.magnitude, minScale);

        newHalfSize = direction * (initialHalfSize.magnitude * scaleFactor);

        targetObject.position = oppositeWorldOnGrab + newHalfSize;
        targetObject.localScale = new Vector3(
            initialScale.x * scaleFactor,
            initialScale.y * scaleFactor,
            initialScale.z
        );
    }

    private void OnDisable()
    {
        isDragging = false;
        dragFingerId = -1;
        // НОВОЕ: при отключении угла на всякий случай разрешаем драг
        if (dragObject != null)
            dragObject.allowDrag = true;
    }
}