using UnityEngine;

/// <summary>
/// Привязывает объект к заданной точке LineRenderer.
/// Использует LateUpdate, чтобы всегда идти за уже обновлённой линией.
/// </summary>
public class CornerPositionTracker : MonoBehaviour
{
    private LineRenderer targetLine;
    private int pointIndex;

    /// <summary>
    /// Вызывается из ColliderOutline для передачи линии и индекса точки.
    /// </summary>
    public void Initialize(LineRenderer line, int index)
    {
        targetLine = line;
        pointIndex = index;
    }

    void LateUpdate()
    {
        if (targetLine != null && pointIndex >= 0 && pointIndex < targetLine.positionCount)
        {
            transform.position = targetLine.GetPosition(pointIndex);
        }
    }
}