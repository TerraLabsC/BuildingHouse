using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DragObject3D : MonoBehaviour
{
    // Статическое событие: оповещает всех слушателей при изменении IsActive.
    // Передаётся сам объект и новое значение активности.
    public static event System.Action<DragObject3D, bool> OnIsActiveChanged;

    private Camera mainCamera;
    private Vector3 offset;
    private Plane dragPlane;

    [SerializeField] private Color outlineColor = Color.white;
    private ColliderOutline outline;

    [SerializeField] private bool isActive = false;

    public bool IsActive
    {
        get => isActive;
        set
        {
            if (isActive == value) return;   // Не вызываем событие лишний раз
            isActive = value;
            ApplyOutlineState();
            OnIsActiveChanged?.Invoke(this, isActive);
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

        // Оповещаем о начальном состоянии, если кому-то нужно.
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

    private void SetOutlineAlpha(Color color)
    {
        if (outline == null) return;

        LineRenderer[] lines = outline.Lines;
        if (lines != null)
        {
            foreach (LineRenderer lr in lines)
            {
                if (lr != null && lr.material != null)
                    lr.material.color = color;
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
                    sr.color = color;
                }
            }
        }
    }

    public void SetActive(bool active)
    {
        IsActive = active;
    }

    private void OnValidate()
    {
        // Только для редактора – не вызываем событие во избежание ошибок.
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
        transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = isActive;

        ApplyOutlineState();

        if (!isActive) return;

        transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = GetComponent<SpriteRenderer>().sortingOrder;

        dragPlane = new Plane(mainCamera.transform.forward, transform.position);

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        float distance;

        if (dragPlane.Raycast(ray, out distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            offset = transform.position - hitPoint;
        }
    }

    private void OnMouseDown()
    {
        if (!isActive) return;

        dragPlane = new Plane(mainCamera.transform.forward, transform.position);
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        float distance;

        if (dragPlane.Raycast(ray, out distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            offset = transform.position - hitPoint;
        }
    }

    private void OnMouseDrag()
    {
        if (!isActive) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        float distance;

        if (dragPlane.Raycast(ray, out distance))
        {
            Vector3 targetPosition = ray.GetPoint(distance) + offset;
            transform.position = targetPosition;
        }
    }
}