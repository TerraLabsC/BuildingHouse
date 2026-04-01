using UnityEngine;

public class OrientationBool : MonoBehaviour
{
    private Camera _camera;
    private bool _isHorizontal = false;
    private bool _isInitialized = false;

    public GameObject BackGroundHorizontal;
    public GameObject BackGroundVertical;

    public bool IsActive = false;

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

    public void BoolActive()
    {
        IsActive = true;
    }

    private void UpdateOrientation(bool isHorizontal)
    {
        Debug.Log("sdf0");

        _isHorizontal = isHorizontal;
        _isInitialized = true;

        if (IsActive)
        {
            if (isHorizontal)
            {
                BackGroundHorizontal.SetActive(true);
                BackGroundVertical.SetActive(false);
            }
            else
            {
                BackGroundHorizontal.SetActive(false);
                BackGroundVertical.SetActive(true);
            }
        }
    }
}
