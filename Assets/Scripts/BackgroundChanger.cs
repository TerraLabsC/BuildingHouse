using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundChanger : MonoBehaviour
{
    [Header("Компоненты")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Список фонов")]
    [SerializeField] private List<Sprite> backgrounds = new List<Sprite>();

    [Header("Список граунда")]
    [SerializeField] private List<GameObject> grounds;

    [Header("Настройки анимации")]
    [SerializeField] private float fadeDuration = 0.5f;

    // Статическая переменная для хранения текущего индекса между сценами
    private static int globalBackgroundIndex = 0;

    private int currentIndex = 0;
    private bool isTransitioning = false;

    void Awake()
    {
        // Применяем сохраненный индекс при загрузке сцены
        currentIndex = globalBackgroundIndex;
    }

    void Start()
    {
        InitializeBackground();
    }

    private void InitializeBackground()
    {
        if (backgrounds.Count > 0 && backgroundImage != null)
        {
            backgroundImage.sprite = backgrounds[currentIndex];

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

        ActivateGroundByIndex(currentIndex);

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

        // Затемнение
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;

        // Смена фона и граунда
        backgroundImage.sprite = backgrounds[currentIndex];
        ActivateGroundByIndex(currentIndex);

        // Сохраняем глобальный индекс
        globalBackgroundIndex = currentIndex;

        // Появление
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

    private void ActivateGroundByIndex(int index)
    {
        if (grounds.Count == 0) return;

        foreach (GameObject ground in grounds)
        {
            if (ground != null)
            {
                ground.SetActive(false);
            }
        }

        if (index >= 0 && index < grounds.Count && grounds[index] != null)
        {
            grounds[index].SetActive(true);
        }
        else
        {
            Debug.LogWarning($"Граунд с индексом {index} не существует или равен null");

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

    public void SetBackgroundWithoutTransition(int index)
    {
        if (index >= 0 && index < backgrounds.Count)
        {
            currentIndex = index;
            globalBackgroundIndex = currentIndex;

            if (backgroundImage != null)
            {
                backgroundImage.sprite = backgrounds[currentIndex];
            }

            ActivateGroundByIndex(currentIndex);
        }
    }

    public void ActivateGround(int index)
    {
        ActivateGroundByIndex(index);
    }

    public int GetCurrentIndex()
    {
        return currentIndex;
    }

    // Статический метод для установки фона из любого скрипта
    public static void SetGlobalBackground(int index)
    {
        globalBackgroundIndex = index;
    }

    // Метод для принудительного обновления фона (вызвать после загрузки сцены)
    public void RefreshBackground()
    {
        if (backgrounds.Count > 0 && backgroundImage != null)
        {
            currentIndex = globalBackgroundIndex;
            backgroundImage.sprite = backgrounds[currentIndex];
            ActivateGroundByIndex(currentIndex);
        }
    }
}