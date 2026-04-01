using UnityEngine;

public class DestroyObj : MonoBehaviour
{
    private GameObject destroy;

    private void Start()
    {
        // Если у объекта есть тег
        GameObject foundObject = GameObject.FindWithTag("DestroyObjects");

        if (foundObject != null)
        {
            destroy = foundObject;
        }
    }

    public void ActiveDestroy()
    {
        destroy.GetComponent<DestroyObject>().Object = gameObject;
    }

    public void DeactiveDestroy()
    {
        destroy.GetComponent<DestroyObject>().Object = null;
    }
}
