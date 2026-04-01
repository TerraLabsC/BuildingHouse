using UnityEngine;

public class OrientationMenu : MonoBehaviour
{
    private Camera _camera;
    private bool _isHorizontal = false;
    private bool _isInitialized = false;

    public GameObject Text;
    public Vector3 PositionTextH;
    public Vector3 PositionTextV;

    public RectTransform buttons;

    [Header("Settings")]
    public float horizontalWidth = 1366.903f;
    public float verticalWidth = 675f;

    private void Start()
    {
        _camera = Camera.main;
        Time.timeScale = 1f;

        UpdateOrientation(true);
    }

    private void Update()
    {
        if (_camera == null) return;

        bool shouldBeHorizontal = Screen.width > Screen.height;

        UpdateOrientation(shouldBeHorizontal);
    }

    private void UpdateOrientation(bool isHorizontal)
    {
        _isHorizontal = isHorizontal;
        _isInitialized = true;

        if (isHorizontal)
        {
            Text.transform.localPosition = PositionTextH;

            buttons.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, horizontalWidth);
        }
        else
        {
            Text.transform.localPosition = PositionTextV;

            buttons.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, verticalWidth);
        }
    }
}
