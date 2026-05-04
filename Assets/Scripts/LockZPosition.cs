using UnityEngine;

/// <summary>
/// Фиксирует позицию объекта по оси Z на значении, которое было при старте.
/// </summary>
public class LockZPosition : MonoBehaviour
{
    private float initialZ;

    private void Start()
    {
        // Запоминаем исходную Z-координату
        initialZ = transform.position.z;
    }

    private void Update()
    {
        // Каждый кадр возвращаем Z к исходному значению
        Vector3 pos = transform.position;
        if (!Mathf.Approximately(pos.z, initialZ))
        {
            pos.z = initialZ;
            transform.position = pos;
        }
    }
}