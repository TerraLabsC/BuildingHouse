using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class BackgroundMenuSelector : MonoBehaviour
{
    [Header("Button Settings")]
    [SerializeField] private Button[] menuButtons; // Массив кнопок меню

    [Header("Separate Button Settings")]
    [SerializeField] private Button separateButton; // Отдельная кнопка
    [SerializeField] private TextMeshProUGUI separateButtonText; // Текст отдельной кнопки

    [Header("Color Settings")]
    [SerializeField] private Color normalColor = Color.white; // Обычный цвет (для активной кнопки)
    [SerializeField] private Color darkColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Темный цвет (для неактивных кнопок)

    [Header("Animation Settings")]
    [SerializeField] private float transitionDuration = 0.3f; // Длительность анимации
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Кривая анимации

    private GameObject[] buttonImagesParents; // Для хранения родительских объектов Image (первый уровень)
    private int currentActiveButtonIndex = -1; // Текущая активная кнопка

    // Структура для хранения компонентов каждой кнопки
    private class ButtonComponents
    {
        public Image firstLevelImage;
        public Image secondLevelImage;
        public Image thirdLevelImage;
        public TextMeshProUGUI textComponent;
        public Coroutine currentAnimation;
    }

    private ButtonComponents[] buttonComponents; // Компоненты для каждой кнопки
    private Coroutine separateButtonAnimation; // Анимация отдельной кнопки

    // Компоненты отдельной кнопки
    private TextMeshProUGUI separateTextComponent;
    private Image separateImageComponent;

    private void Start()
    {
        // Инициализируем массивы
        buttonImagesParents = new GameObject[menuButtons.Length];
        buttonComponents = new ButtonComponents[menuButtons.Length];

        // Находим для каждой кнопки ее дочерний объект "0" (первый уровень) и все компоненты
        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (menuButtons[i] != null)
            {
                // Ищем дочерний объект с именем "0"
                Transform childZero = menuButtons[i].transform.Find("0");
                if (childZero != null)
                {
                    buttonImagesParents[i] = childZero.gameObject;
                    buttonComponents[i] = new ButtonComponents();

                    // Получаем все компоненты для плавной анимации
                    buttonComponents[i].firstLevelImage = childZero.GetComponent<Image>();

                    // Находим второй уровень
                    Transform secondLevelZero = childZero.Find("0");
                    if (secondLevelZero != null)
                    {
                        buttonComponents[i].secondLevelImage = secondLevelZero.GetComponent<Image>();

                        // Находим третий уровень
                        Transform thirdLevelZero = secondLevelZero.Find("0");
                        if (thirdLevelZero != null)
                        {
                            buttonComponents[i].thirdLevelImage = thirdLevelZero.GetComponent<Image>();
                        }

                        // Находим текст
                        Transform textObject = secondLevelZero.Find("1");
                        if (textObject != null)
                        {
                            buttonComponents[i].textComponent = textObject.GetComponent<TextMeshProUGUI>();
                        }
                        else
                        {
                            buttonComponents[i].textComponent = secondLevelZero.GetComponentInChildren<TextMeshProUGUI>();
                        }
                    }
                    else
                    {
                        buttonComponents[i].textComponent = childZero.GetComponentInChildren<TextMeshProUGUI>();
                    }
                }
                else
                {
                    Debug.LogWarning($"У кнопки {menuButtons[i].name} не найден дочерний объект '0'");
                }
            }
        }

        // Получаем компоненты отдельной кнопки
        if (separateButton != null)
        {
            separateTextComponent = separateButtonText != null ? separateButtonText : separateButton.GetComponentInChildren<TextMeshProUGUI>();
            separateImageComponent = separateButton.GetComponent<Image>();
        }

        // Добавляем обработчики для всех кнопок меню
        for (int i = 0; i < menuButtons.Length; i++)
        {
            int index = i; // Локальная копия для замыкания
            if (menuButtons[index] != null)
            {
                menuButtons[index].onClick.AddListener(() => OnMenuButtonClick(index));
            }
        }

        // Добавляем обработчик для отдельной кнопки
        if (separateButton != null)
        {
            separateButton.onClick.AddListener(OnSeparateButtonClick);
        }

        // Устанавливаем начальное состояние (можно раскомментировать при необходимости)
        // if (menuButtons.Length > 0)
        // {
        //     SetActiveButton(0);
        // }
    }

    private void OnMenuButtonClick(int clickedIndex)
    {
        SetActiveButton(clickedIndex);
    }

    private void OnSeparateButtonClick()
    {
        Debug.Log("Отдельная кнопка нажата");
        if (currentActiveButtonIndex >= 0)
        {
            Debug.Log($"Выбран фон с индексом: {currentActiveButtonIndex}");
        }
    }

    public void SetActiveButton(int buttonIndex)
    {
        if (buttonIndex < 0 || buttonIndex >= menuButtons.Length) return;

        currentActiveButtonIndex = buttonIndex;

        // Запускаем анимацию для всех кнопок
        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (buttonComponents[i] != null)
            {
                bool isActive = (i == buttonIndex);
                AnimateMenuButton(i, isActive);
            }
        }

        // Анимируем отдельную кнопку
        AnimateSeparateButton(true);
    }

    private void AnimateMenuButton(int buttonIndex, bool isActive)
    {
        ButtonComponents components = buttonComponents[buttonIndex];
        if (components == null) return;

        // Останавливаем текущую анимацию, если она есть
        if (components.currentAnimation != null)
        {
            StopCoroutine(components.currentAnimation);
        }

        // Запускаем новую анимацию
        components.currentAnimation = StartCoroutine(AnimateMenuButtonCoroutine(components, isActive));
    }

    private IEnumerator AnimateMenuButtonCoroutine(ButtonComponents components, bool isActive)
    {
        float elapsedTime = 0f;

        // Сохраняем начальные и конечные значения
        bool startFirstLevelEnabled = components.firstLevelImage != null ? components.firstLevelImage.enabled : false;
        bool endFirstLevelEnabled = isActive;

        Color startSecondLevelColor = components.secondLevelImage != null ? components.secondLevelImage.color : normalColor;
        Color endSecondLevelColor = isActive ? normalColor : darkColor;

        Color startThirdLevelColor = components.thirdLevelImage != null ? components.thirdLevelImage.color : normalColor;
        Color endThirdLevelColor = isActive ? normalColor : darkColor;

        Color startTextColor = components.textComponent != null ? components.textComponent.color : normalColor;
        Color endTextColor = isActive ? normalColor : darkColor;

        while (elapsedTime < transitionDuration)
        {
            float t = transitionCurve.Evaluate(elapsedTime / transitionDuration);

            // Плавно меняем состояние первого уровня (Image.enabled)
            if (components.firstLevelImage != null && startFirstLevelEnabled != endFirstLevelEnabled)
            {
                // Для boolean используем пороговое значение
                components.firstLevelImage.enabled = t > 0.5f ? endFirstLevelEnabled : startFirstLevelEnabled;
            }

            // Плавно меняем цвета
            if (components.secondLevelImage != null)
            {
                components.secondLevelImage.color = Color.Lerp(startSecondLevelColor, endSecondLevelColor, t);
            }

            if (components.thirdLevelImage != null)
            {
                components.thirdLevelImage.color = Color.Lerp(startThirdLevelColor, endThirdLevelColor, t);
            }

            if (components.textComponent != null)
            {
                components.textComponent.color = Color.Lerp(startTextColor, endTextColor, t);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Устанавливаем конечные значения
        if (components.firstLevelImage != null)
        {
            components.firstLevelImage.enabled = endFirstLevelEnabled;
        }

        if (components.secondLevelImage != null)
        {
            components.secondLevelImage.color = endSecondLevelColor;
        }

        if (components.thirdLevelImage != null)
        {
            components.thirdLevelImage.color = endThirdLevelColor;
        }

        if (components.textComponent != null)
        {
            components.textComponent.color = endTextColor;
        }

        components.currentAnimation = null;
    }

    private void AnimateSeparateButton(bool isActive)
    {
        // Останавливаем текущую анимацию
        if (separateButtonAnimation != null)
        {
            StopCoroutine(separateButtonAnimation);
        }

        // Запускаем новую анимацию
        separateButtonAnimation = StartCoroutine(AnimateSeparateButtonCoroutine(isActive));
    }

    private IEnumerator AnimateSeparateButtonCoroutine(bool isActive)
    {
        if (separateButton == null) yield break;

        float elapsedTime = 0f;

        // Сохраняем начальные и конечные значения
        bool startButtonEnabled = separateButton.enabled;
        bool endButtonEnabled = isActive;

        Color startTextColor = separateTextComponent != null ? separateTextComponent.color : normalColor;
        Color endTextColor = isActive ? normalColor : darkColor;

        Color startImageColor = separateImageComponent != null ? separateImageComponent.color : normalColor;
        Color endImageColor = isActive ? normalColor : darkColor;

        while (elapsedTime < transitionDuration)
        {
            float t = transitionCurve.Evaluate(elapsedTime / transitionDuration);

            // Плавно меняем состояние Button.enabled
            if (startButtonEnabled != endButtonEnabled)
            {
                separateButton.enabled = t > 0.5f ? endButtonEnabled : startButtonEnabled;
            }

            // Плавно меняем цвет текста
            if (separateTextComponent != null)
            {
                separateTextComponent.color = Color.Lerp(startTextColor, endTextColor, t);
            }

            // Плавно меняем цвет фона кнопки
            if (separateImageComponent != null)
            {
                separateImageComponent.color = Color.Lerp(startImageColor, endImageColor, t);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Устанавливаем конечные значения
        separateButton.enabled = endButtonEnabled;

        if (separateTextComponent != null)
        {
            separateTextComponent.color = endTextColor;
        }

        if (separateImageComponent != null)
        {
            separateImageComponent.color = endImageColor;
        }

        separateButtonAnimation = null;

        Debug.Log($"Отдельная кнопка {(isActive ? "активирована" : "деактивирована")}");
    }

    // Метод для немедленного изменения состояния (без анимации)
    public void SetActiveButtonImmediate(int buttonIndex)
    {
        if (buttonIndex < 0 || buttonIndex >= menuButtons.Length) return;

        currentActiveButtonIndex = buttonIndex;

        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (buttonComponents[i] != null)
            {
                bool isActive = (i == buttonIndex);
                SetMenuButtonStateImmediate(i, isActive);
            }
        }

        SetSeparateButtonStateImmediate(true);
    }

    private void SetMenuButtonStateImmediate(int buttonIndex, bool isActive)
    {
        ButtonComponents components = buttonComponents[buttonIndex];
        if (components == null) return;

        if (components.firstLevelImage != null)
        {
            components.firstLevelImage.enabled = isActive;
        }

        if (components.secondLevelImage != null)
        {
            components.secondLevelImage.color = isActive ? normalColor : darkColor;
        }

        if (components.thirdLevelImage != null)
        {
            components.thirdLevelImage.color = isActive ? normalColor : darkColor;
        }

        if (components.textComponent != null)
        {
            components.textComponent.color = isActive ? normalColor : darkColor;
        }
    }

    private void SetSeparateButtonStateImmediate(bool isActive)
    {
        if (separateButton == null) return;

        separateButton.enabled = isActive;

        if (separateTextComponent != null)
        {
            separateTextComponent.color = isActive ? normalColor : darkColor;
        }

        if (separateImageComponent != null)
        {
            separateImageComponent.color = isActive ? normalColor : darkColor;
        }
    }

    // Метод для получения текущей активной кнопки
    public int GetCurrentActiveButtonIndex()
    {
        return currentActiveButtonIndex;
    }

    // Метод для изменения длительности анимации
    public void SetTransitionDuration(float duration)
    {
        transitionDuration = Mathf.Max(0.01f, duration);
    }

    // Метод для изменения кривой анимации
    public void SetTransitionCurve(AnimationCurve curve)
    {
        transitionCurve = curve;
    }
}