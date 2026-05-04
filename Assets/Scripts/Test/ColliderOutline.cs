using System;
using UnityEngine;
using UnityEngine.UI;

public class ColliderOutline : MonoBehaviour
{
    public enum Face { Front, Back, Left, Right, Top, Bottom }
    [SerializeField] private Face faceToDraw = Face.Front;
    [SerializeField] private float lineWidth = 0.03f;
    [SerializeField] private Material lineMaterial;
    [SerializeField] private GameObject cornerPrefab;
    [SerializeField] private float inset = 0.1f;

    private Renderer sourceRenderer;
    private LineRenderer[] lines;
    private Transform[] corners;
    private BoxCollider box;
    private GameObject holder;

    // Публичные свойства для доступа из DragObject3D
    public LineRenderer[] Lines => lines;
    public Transform[] Corners => corners;
    public GameObject Holder => holder;

    void Awake()
    {
        box = GetComponent<BoxCollider>();
        sourceRenderer = GetComponent<Renderer>();
        CreateHolder();
        CreateLineRenderers();
        CreateCorners();
        ApplySortingOrder();
    }

    void CreateHolder()
    {
        holder = new GameObject("ColliderOutlineHolder");
        holder.transform.SetParent(transform, false);
    }

    void CreateLineRenderers()
    {
        lines = new LineRenderer[4];
        string[] names = { "TopEdge", "BottomEdge", "LeftEdge", "RightEdge" };

        for (int i = 0; i < 4; i++)
        {
            GameObject lineObj = new GameObject(names[i]);
            lineObj.transform.SetParent(holder.transform, false);

            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;
            lr.useWorldSpace = true;
            lr.positionCount = 2;
            lr.material = lineMaterial != null ? lineMaterial : new Material(Shader.Find("Sprites/Default"));

            lines[i] = lr;
        }
    }

    void CreateCorners()
    {
        corners = new Transform[4];
        string[] cornerNames = { "Corner_TL", "Corner_TR", "Corner_BR", "Corner_BL" };
        CornerDragHandler.CornerType[] cornerTypes = {
            CornerDragHandler.CornerType.TopLeft,
            CornerDragHandler.CornerType.TopRight,
            CornerDragHandler.CornerType.BottomRight,
            CornerDragHandler.CornerType.BottomLeft
        };

        LineRenderer[] cornerLines = { lines[0], lines[0], lines[1], lines[1] };
        int[] cornerPointIndices = { 0, 1, 1, 0 };

        for (int i = 0; i < 4; i++)
        {
            if (cornerPrefab == null)
            {
                corners[i] = null;
                continue;
            }

            GameObject instance = Instantiate(cornerPrefab, holder.transform);
            instance.name = cornerNames[i];

            instance.gameObject.layer = 13;

            CircleCollider2D col = instance.GetComponent<CircleCollider2D>();
            if (col == null)
            {
                col = instance.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
            }

            CornerDragHandler dragHandler = instance.GetComponent<CornerDragHandler>();
            if (dragHandler == null)
                dragHandler = instance.AddComponent<CornerDragHandler>();
            dragHandler.cornerType = cornerTypes[i];

            CornerPositionTracker tracker = instance.GetComponent<CornerPositionTracker>();
            if (tracker == null)
                tracker = instance.AddComponent<CornerPositionTracker>();
            tracker.Initialize(cornerLines[i], cornerPointIndices[i]);

            corners[i] = instance.transform;
        }
    }

    void ApplySortingOrder()
    {
        if (sourceRenderer == null)
            return;

        int order = sourceRenderer.sortingOrder;
        int layerID = sourceRenderer.sortingLayerID;

        foreach (LineRenderer lr in lines)
        {
            if (lr != null)
            {
                lr.sortingOrder = order;
                lr.sortingLayerID = layerID;
            }
        }

        foreach (Transform corner in corners)
        {
            if (corner == null) continue;
            SpriteRenderer[] sprites = corner.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer sr in sprites)
            {
                sr.sortingOrder = order;
                sr.sortingLayerID = layerID;
            }
        }
    }

    void Update()
    {
        DrawFaceOutline();
        ApplySortingOrder();
    }

    void DrawFaceOutline()
    {
        Vector3 c = box.center;
        Vector3 s = box.size * 0.5f;
        Transform t = transform;

        Vector3 a, b, c1, d;
        GetFaceCorners(faceToDraw, c, s, inset, out a, out b, out c1, out d);

        Vector3 aW = t.TransformPoint(a);
        Vector3 bW = t.TransformPoint(b);
        Vector3 c1W = t.TransformPoint(c1);
        Vector3 dW = t.TransformPoint(d);

        lines[0].SetPositions(new[] { aW, bW }); // верх
        lines[1].SetPositions(new[] { dW, c1W }); // низ
        lines[2].SetPositions(new[] { aW, dW });  // лево
        lines[3].SetPositions(new[] { bW, c1W }); // право
    }

    void GetFaceCorners(Face face, Vector3 center, Vector3 halfExtents, float inset,
        out Vector3 topLeft, out Vector3 topRight, out Vector3 bottomRight, out Vector3 bottomLeft)
    {
        Vector3 up, right;
        switch (face)
        {
            case Face.Front:
                up = Vector3.up; right = Vector3.right;
                break;
            case Face.Back:
                up = Vector3.up; right = -Vector3.right;
                break;
            case Face.Left:
                up = Vector3.up; right = -Vector3.forward;
                break;
            case Face.Right:
                up = Vector3.up; right = Vector3.forward;
                break;
            case Face.Top:
                up = Vector3.forward; right = Vector3.right;
                break;
            case Face.Bottom:
                up = -Vector3.forward; right = Vector3.right;
                break;
            default:
                up = Vector3.up; right = Vector3.right;
                break;
        }

        Vector3 normal = Vector3.Cross(right, up).normalized;
        Vector3 faceCenter = center + normal * Vector3.Dot(normal, halfExtents);

        float halfUp = Mathf.Abs(Vector3.Dot(up, halfExtents));
        float halfRight = Mathf.Abs(Vector3.Dot(right, halfExtents));

        halfUp = Mathf.Max(0, halfUp - inset);
        halfRight = Mathf.Max(0, halfRight - inset);

        topLeft = faceCenter + up * halfUp - right * halfRight;
        topRight = faceCenter + up * halfUp + right * halfRight;
        bottomRight = faceCenter - up * halfUp + right * halfRight;
        bottomLeft = faceCenter - up * halfUp - right * halfRight;
    }
}