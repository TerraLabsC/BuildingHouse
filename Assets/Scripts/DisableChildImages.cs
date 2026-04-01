using UnityEngine;
using UnityEngine.UI;

public class DisableChildImages : MonoBehaviour
{
    void Start()
    {
        DisableAllImages();
    }

    public void DisableAllImages()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            Image image = child.GetComponent<Image>();

            if (image != null)
            {
                image.enabled = false;
            }
        }
    }
}
