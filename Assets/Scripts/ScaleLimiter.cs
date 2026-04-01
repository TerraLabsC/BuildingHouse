using UnityEngine;

/// <summary>
/// Компонент для ограничения масштаба объекта.
/// Позволяет задать минимальный и максимальный масштаб по каждой оси отдельно.
/// </summary>
public class ScaleLimiter : MonoBehaviour
{
    [Header("Ограничения по осям")]
    [Tooltip("Минимально допустимый масштаб (по каждой оси)")]
    public Vector3 minScale = new Vector3(0.1f, 0.1f, 0.1f);

    [Tooltip("Максимально допустимый масштаб (по каждой оси)")]
    public Vector3 maxScale = new Vector3(2f, 2f, 2f);

    [Header("Настройки")]
    [Tooltip("Если включено, масштаб будет принудительно выравниваться по самой ограниченной оси (равномерное масштабирование)")]
    public bool uniformScaling = false;

    // Опционально: принудительно применить ограничения при старте
    private void Start()
    {
        ApplyScaleConstraints();
    }

    // Применяем ограничения каждый кадр после всех изменений
    private void LateUpdate()
    {
        ApplyScaleConstraints();
    }

    /// <summary>
    /// Применяет ограничения к локальному масштабу объекта.
    /// </summary>
    private void ApplyScaleConstraints()
    {
        Vector3 currentScale = transform.localScale;
        Vector3 clampedScale = currentScale;

        if (uniformScaling)
        {
            // Для равномерного масштабирования выбираем коэффициент,
            // который не выходит за границы ни по одной из осей
            float maxComponent = Mathf.Max(
                Mathf.Max(currentScale.x, currentScale.y),
                currentScale.z
            );
            float minComponent = Mathf.Min(
                Mathf.Min(currentScale.x, currentScale.y),
                currentScale.z
            );

            // Определяем желаемый единый множитель
            float targetFactor = currentScale.x; // Например, берём X как основу
            // Ограничиваем его с учётом min и max по каждой оси
            targetFactor = Mathf.Clamp(targetFactor, minScale.x, maxScale.x);
            targetFactor = Mathf.Clamp(targetFactor, minScale.y, maxScale.y);
            targetFactor = Mathf.Clamp(targetFactor, minScale.z, maxScale.z);

            clampedScale = Vector3.one * targetFactor;
        }
        else
        {
            // Ограничиваем каждую ось независимо
            clampedScale.x = Mathf.Clamp(currentScale.x, minScale.x, maxScale.x);
            clampedScale.y = Mathf.Clamp(currentScale.y, minScale.y, maxScale.y);
            clampedScale.z = Mathf.Clamp(currentScale.z, minScale.z, maxScale.z);
        }

        // Применяем, только если изменилось, чтобы избежать лишних вызовов
        if (clampedScale != currentScale)
        {
            transform.localScale = clampedScale;
        }
    }

    // Метод для ручного вызова ограничений (например, после изменения масштаба кодом)
    public void EnforceConstraints()
    {
        ApplyScaleConstraints();
    }
}