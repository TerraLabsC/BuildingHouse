using UnityEngine;

public class OrientationBackGround : MonoBehaviour
{
    private Camera _camera;
    private bool _isHorizontal = false;
    private bool _isInitialized = false;

    public Canvas BackGroundHorizontal;
    public Canvas BackGroundVertical;

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

        if (shouldBeHorizontal != _isHorizontal || !_isInitialized)
        {
            UpdateOrientation(shouldBeHorizontal);
        }
    }

    private void UpdateOrientation(bool isHorizontal)
    {
        _isHorizontal = isHorizontal;
        _isInitialized = true;

        if (isHorizontal)
        {
            BackGroundHorizontal.enabled = true;
            BackGroundVertical.enabled = false;
        }
        else
        {
            BackGroundHorizontal.enabled = false;
            BackGroundVertical.enabled = true;
        }
    }
}
