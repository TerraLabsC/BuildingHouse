using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DragObject3D : MonoBehaviour
{
    public static event System.Action<DragObject3D, bool> OnIsActiveChanged;

    private Camera mainCamera;
    private Vector3 offset;
    private Plane dragPlane;
    private bool isDragging = false;
    private int dragFingerId = -1;

    [SerializeField] private Color outlineColor = Color.white;
    private ColliderOutline outline;

    [SerializeField] private bool isActive = false;

    // НОВОЕ: флаг, разрешающий перетаскивание (управляется из CornerDragHandler)
    [HideInInspector] public bool allowDrag = true;

    public bool IsActive
    {
        get => isActive;
        set
        {
            if (isActive == value) return;
            isActive = value;
            ApplyOutlineState();
            OnIsActiveChanged?.Invoke(this, isActive);

            if (!isActive)
            {
                isDragging = false;
                dragFingerId = -1;
            }
        }
    }

    private void Awake()
    {
        outline = GetComponent<ColliderOutline>();
        if (outline == null)
            outline = GetComponentInChildren<ColliderOutline>();
    }

    private void Start()
    {
        mainCamera = Camera.main;
        ApplyOutlineState();
        OnIsActiveChanged?.Invoke(this, isActive);
    }

    private void ApplyOutlineState()
    {
        if (outline == null) return;

        Color targetColor = outlineColor;
        targetColor.a = isActive ? 1f : 0f;

        LineRenderer[] lines = outline.Lines;
        if (lines != null)
        {
            foreach (LineRenderer lr in lines)
            {
                if (lr != null)
                {
                    lr.enabled = isActive;
                    if (lr.material != null)
                        lr.material.color = targetColor;
                }
            }
        }

        Transform[] corners = outline.Corners;
        if (corners != null)
        {
            foreach (Transform corner in corners)
            {
                if (corner == null) continue;
                SpriteRenderer[] sprites = corner.GetComponentsInChildren<SpriteRenderer>();
                foreach (SpriteRenderer sr in sprites)
                {
                    sr.enabled = isActive;
                    sr.color = targetColor;
                }
            }
        }

        ActiveCollidersHolder();
    }

    public void SetActive(bool active)
    {
        IsActive = active;
    }

    private void OnValidate()
    {
        ApplyOutlineState();
    }

    public void ActiveCollidersHolder()
    {
        if (outline == null || outline.Holder == null) return;

        CircleCollider2D[] colliders = outline.Holder.GetComponentsInChildren<CircleCollider2D>(true);
        foreach (var col in colliders)
        {
            col.enabled = isActive;
        }
    }

    private void Update()
    {
        Transform child0 = transform.GetChild(0);
        if (child0 != null)
        {
            SpriteRenderer sr = child0.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.enabled = isActive;
        }

        ApplyOutlineState();

        if (!isActive) return;

        SpriteRenderer parentSr = GetComponent<SpriteRenderer>();
        if (parentSr != null && child0 != null)
        {
            SpriteRenderer childSr = child0.GetComponent<SpriteRenderer>();
            if (childSr != null)
                childSr.sortingOrder = parentSr.sortingOrder - 1;
        }

        // НОВОЕ: не начинать драг, если запрещено (например, идёт скейлинг)
        if (!isDragging)
        {
            if (allowDrag)
                TryStartDrag();
        }
        else
        {
            TryUpdateDrag();
        }
    }

    private void TryStartDrag()
    {
        Vector2 inputPos;
        bool inputBegan = false;

        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began)
                {
                    inputPos = touch.position;
                    inputBegan = true;
                    Ray ray = mainCamera.ScreenPointToRay(inputPos);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject)
                    {
                        dragFingerId = touch.fingerId;
                        StartDrag(ray);
                        return;
                    }
                }
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            inputPos = Input.mousePosition;
            inputBegan = true;
            Ray ray = mainCamera.ScreenPointToRay(inputPos);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject)
            {
                dragFingerId = -1;
                StartDrag(ray);
            }
        }
    }

    private void TryUpdateDrag()
    {
        bool inputHeld = false;
        Vector2 inputPos = Vector2.zero;

        if (dragFingerId >= 0 && Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.fingerId == dragFingerId)
                {
                    if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                    {
                        inputHeld = true;
                        inputPos = touch.position;
                    }
                    else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        isDragging = false;
                        dragFingerId = -1;
                        return;
                    }
                    break;
                }
            }
        }
        else if (dragFingerId == -1 && Input.GetMouseButton(0))
        {
            inputHeld = true;
            inputPos = Input.mousePosition;
        }
        else
        {
            isDragging = false;
            dragFingerId = -1;
            return;
        }

        if (inputHeld)
        {
            Ray ray = mainCamera.ScreenPointToRay(inputPos);
            float distance;
            if (dragPlane.Raycast(ray, out distance))
            {
                Vector3 targetPosition = ray.GetPoint(distance) + offset;
                transform.position = targetPosition;
            }
        }
    }

    private void StartDrag(Ray initialRay)
    {
        dragPlane = new Plane(mainCamera.transform.forward, transform.position);

        float distance;
        if (dragPlane.Raycast(initialRay, out distance))
        {
            Vector3 hitPoint = initialRay.GetPoint(distance);
            offset = transform.position - hitPoint;
        }
        else
        {
            offset = Vector3.zero;
        }

        isDragging = true;
    }
}