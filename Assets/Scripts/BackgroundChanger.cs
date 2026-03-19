using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundChanger : MonoBehaviour
{
    [Header("Компоненты")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private CanvasGroup canvasGroup; // Только для фона

    [Header("Список фонов")]
    [SerializeField] private List<Sprite> backgrounds = new List<Sprite>();

    [Header("Список граунда")]
    [SerializeField] private List<GameObject> grounds; // Теперь это GameObject, а не Sprite

    [Header("Настройки анимации")]
    [SerializeField] private float fadeDuration = 0.5f; // Длительность затухания только для фона

    private int currentIndex = 0;
    private bool isTransitioning = false;

    void Start()
    {
        // Инициализация фона
        if (backgrounds.Count > 0 && backgroundImage != null)
        {
            backgroundImage.sprite = backgrounds[0];

            if (canvasGroup == null)
            {
                canvasGroup = backgroundImage.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = backgroundImage.gameObject.AddComponent<CanvasGroup>();
                }
            }
            canvasGroup.alpha = 1f;
        }

        // Инициализация граунда - включаем первый, остальные выключаем
        ActivateGroundByIndex(0);

        // Проверка соответствия размеров списков
        if (backgrounds.Count != grounds.Count)
        {
            Debug.LogWarning($"Количество фонов ({backgrounds.Count}) и граундов ({grounds.Count}) не совпадает!");
        }
    }

    public void NextBackground()
    {
        if (!isTransitioning && backgrounds.Count > 0)
        {
            currentIndex = (currentIndex + 1) % backgrounds.Count;
            StartCoroutine(TransitionBackground());
        }
    }

    public void PreviousBackground()
    {
        if (!isTransitioning && backgrounds.Count > 0)
        {
            currentIndex--;
            if (currentIndex < 0)
            {
                currentIndex = backgrounds.Count - 1;
            }
            StartCoroutine(TransitionBackground());
        }
    }

    private IEnumerator TransitionBackground()
    {
        isTransitioning = true;

        // Затухание только фона
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;

        // Смена фона
        backgroundImage.sprite = backgrounds[currentIndex];

        // МГНОВЕННАЯ СМЕНА ГРАУНДА - включаем нужный, выключаем все остальные
        ActivateGroundByIndex(currentIndex);

        // Появление фона
        elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        isTransitioning = false;
    }

    // Метод для включения конкретного граунда и выключения всех остальных
    private void ActivateGroundByIndex(int index)
    {
        if (grounds.Count == 0) return;

        // Сначала выключаем все граунды
        foreach (GameObject ground in grounds)
        {
            if (ground != null)
            {
                ground.SetActive(false);
            }
        }

        // Включаем нужный граунд, если индекс существует
        if (index >= 0 && index < grounds.Count && grounds[index] != null)
        {
            grounds[index].SetActive(true);
        }
        else
        {
            Debug.LogWarning($"Граунд с индексом {index} не существует или равен null");

            // Если индекс не подходит, включаем первый или последний
            if (grounds.Count > 0 && grounds[0] != null)
            {
                grounds[0].SetActive(true);
            }
        }
    }

    public void SetBackground(int index)
    {
        if (!isTransitioning && index >= 0 && index < backgrounds.Count)
        {
            currentIndex = index;
            StartCoroutine(TransitionBackground());
        }
    }

    // Метод для ручного включения конкретного граунда
    public void ActivateGround(int index)
    {
        ActivateGroundByIndex(index);
    }

    // Метод для получения текущего индекса
    public int GetCurrentIndex()
    {
        return currentIndex;
    }
}