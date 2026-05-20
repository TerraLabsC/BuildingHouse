using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIBackGroundButtons : MonoBehaviour
{
    public GameObject RedLight;
    public GameObject GreenLight;

    public bool IsActive = false;

    private void Start()
    {
        if (IsActive)
        {
            OnButtonColor();
        }
    }

    public void OnButtonColor()
    {
        RedLight.SetActive(false);
        GreenLight.SetActive(true);
    }

    public void OffButtonColor()
    {
        RedLight.SetActive(true);
        GreenLight.SetActive(false);
    }
}
