using UnityEngine;

public class Orientation : MonoBehaviour
{
    private Camera _camera;
    private bool _isHorizontal = false;
    private bool _isInitialized = false;

    [SerializeField] private GameObject PositionCameraHorizontal;
    [SerializeField] private GameObject PositionCameraVertical;

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
            gameObject.transform.position = PositionCameraHorizontal.transform.position;
        }
        else
        {
            gameObject.transform.position = PositionCameraVertical.transform.position;
        }
    }
}
