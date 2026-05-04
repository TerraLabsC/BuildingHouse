using UnityEngine;

public class TouchManager : MonoBehaviour
{
    void Update()
    {
        // Проверяем, есть ли хотя бы одно касание
        if (Input.touchCount > 0)
        {
            // Берем первое касание
            Touch touch = Input.GetTouch(0);

            // Нас интересует только момент, когда палец только коснулся экрана
            if (touch.phase == TouchPhase.Began)
            {
                // Создаем луч из точки касания
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit hit;

                // Пускаем луч и проверяем, попал ли он в какой-либо объект
                if (Physics.Raycast(ray, out hit))
                {
                    // Пытаемся получить компонент нашего скрипта с объекта, в который попали
                    SpawnedObjectNotifier objectScript = hit.collider.GetComponent<SpawnedObjectNotifier>();

                    if (objectScript != null)
                    {
                        // Если скрипт найден, вызываем его публичный метод
                        objectScript.HandleTouch();
                    }
                }
            }
        }
    }
}