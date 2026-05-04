using UnityEngine;

public class ChildScript : MonoBehaviour
{
    void Update()
    {
        // Получаем родительский Transform
        Transform parentTransform = transform.parent;
        if (parentTransform != null)
        {
            // Берём SpriteRenderer с родителя
            SpriteRenderer parentRenderer = parentTransform.GetComponent<SpriteRenderer>();
            if (parentRenderer != null)
            {
                int parentOrder = parentRenderer.sortingOrder;
            }
        }
    }
}