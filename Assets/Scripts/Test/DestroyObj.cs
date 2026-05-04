using UnityEngine;

public class DestroyObj : MonoBehaviour
{
    private GameObject destroy;

    public GameObject FingerLesson;

    private void Start()
    {
        GameObject foundObject = GameObject.FindWithTag("DestroyObjects");

        if (foundObject != null)
        {
            destroy = foundObject;

            destroy.GetComponent<DestroyObject>().Object = gameObject;
        }
    }

    public void ActiveDestroy()
    {
        if (!destroy) return;

        destroy.GetComponent<DestroyObject>().Object = gameObject;

        DestroyFinger();
    }

    public void DestroyFinger()
    {
        if (FingerLesson != null)
        {
            Destroy(FingerLesson);
        }
    }

    public void DeactiveDestroy()
    {
        //destroy.GetComponent<DestroyObject>().IsObjectInTopRightCorner();

        //destroy.GetComponent<DestroyObject>().Object = null;
    }
}
