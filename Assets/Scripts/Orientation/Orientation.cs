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
            Debug.Log("Переключение на горизонтальный режим");

            gameObject.transform.position = PositionCameraHorizontal.transform.position;
        }
        else
        {
            Debug.Log("Переключение на вертикальный режим");

            gameObject.transform.position = PositionCameraVertical.transform.position;
        }
    }
}
