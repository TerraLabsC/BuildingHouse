using UnityEngine;

/// <summary>
/// Компонент для автоматического перемещения объекта в центр экрана,
/// если он выходит за пределы видимости камеры.
/// </summary>
public class ScreenBoundaryKeeper : MonoBehaviour
{
    [Header("Настройки камеры")]
    [Tooltip("Камера, по которой определяется видимость. Если не задана, используется Camera.main.")]
    public Camera targetCamera;

    [Header("Параметры проверки")]
    [Tooltip("Если true, проверяются границы рендерера (с учётом MeshRenderer или SpriteRenderer). Иначе проверяется только позиция объекта.")]
    public bool useRendererBounds = false;

    [Tooltip("Если true, объекты позади камеры (Z < 0 в пространстве камеры) считаются невидимыми.")]
    public bool considerDepth = true;

    [Header("Поведение")]
    [Tooltip("При старте принудительно проверить и при необходимости телепортировать.")]
    public bool teleportOnStart = true;

    [Tooltip("Смещение от центра экрана в мировых единицах (применяется после вычисления центра).")]
    public Vector2 offsetFromCenter = Vector2.zero;

    // Кэш рендерера для оптимизации
    private Renderer cachedRenderer;

    private void Start()
    {
        // Если камера не назначена, используем основную
        if (targetCamera == null)
            targetCamera = Camera.main;

        // Если нужно, сразу телепортируем при старте
        if (teleportOnStart)
            TeleportIfOffscreen();
    }

    private void LateUpdate()
    {
        TeleportIfOffscreen();
    }

    /// <summary>
    /// Проверяет, находится ли объект в кадре. Если нет – перемещает в центр экрана.
    /// </summary>
    private void TeleportIfOffscreen()
    {
        if (targetCamera == null)
        {
            Debug.LogWarning("ScreenBoundaryKeeper: камера не задана и Camera.main не найдена.", this);
            return;
        }

        if (!IsObjectVisible())
        {
            TeleportToScreenCenter();
        }
    }

    /// <summary>
    /// Определяет, виден ли объект камерой.
    /// </summary>
    private bool IsObjectVisible()
    {
        if (useRendererBounds && TryGetRendererBounds(out Bounds bounds))
        {
            // Проверяем, пересекаются ли границы объекта с frustum камеры
            Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(targetCamera);
            return GeometryUtility.TestPlanesAABB(frustumPlanes, bounds);
        }
        else
        {
            // Проверяем позицию объекта в экранных координатах
            Vector3 viewportPos = targetCamera.WorldToViewportPoint(transform.position);

            // Проверяем глубину, если нужно
            if (considerDepth && viewportPos.z < 0)
                return false;

            // Проверяем, что координаты в пределах [0,1] по X и Y
            return viewportPos.x >= 0 && viewportPos.x <= 1 &&
                   viewportPos.y >= 0 && viewportPos.y <= 1;
        }
    }

    /// <summary>
    /// Пытается получить bounding box объекта через рендерер.
    /// </summary>
    private bool TryGetRendererBounds(out Bounds bounds)
    {
        if (cachedRenderer == null)
            cachedRenderer = GetComponent<Renderer>();

        if (cachedRenderer != null)
        {
            bounds = cachedRenderer.bounds;
            return true;
        }

        bounds = new Bounds();
        return false;
    }

    /// <summary>
    /// Перемещает объект в центр экрана (с учётом смещения).
    /// Сохраняет мировую глубину (Z) объекта относительно камеры.
    /// </summary>
    private void TeleportToScreenCenter()
    {
        // Получаем позицию объекта в пространстве камеры
        Vector3 camSpacePos = targetCamera.transform.InverseTransformPoint(transform.position);

        // Вычисляем направление от камеры к центру экрана на той же глубине
        // Центр экрана в пространстве камеры — это (0, 0, camSpacePos.z)
        Vector3 centerCamSpace = new Vector3(0, 0, camSpacePos.z);

        // Добавляем смещение (offsetFromCenter) в мировом пространстве, но переводим в пространство камеры
        // Смещение задано в мировых единицах вдоль осей X и Y (локальных для камеры)
        // Преобразуем смещение в пространство камеры: смещение по горизонтали и вертикали
        Vector3 offsetCamSpace = new Vector3(offsetFromCenter.x, offsetFromCenter.y, 0);

        // Итоговая позиция в пространстве камеры
        Vector3 newCamSpacePos = centerCamSpace + offsetCamSpace;

        // Конвертируем обратно в мировые координаты
        Vector3 newWorldPos = targetCamera.transform.TransformPoint(newCamSpacePos);

        // Перемещаем объект
        transform.position = newWorldPos;
    }

    /// <summary>
    /// Публичный метод для ручной телепортации в центр экрана (можно вызвать из другого скрипта).
    /// </summary>
    public void TeleportToCenter()
    {
        TeleportToScreenCenter();
    }
}