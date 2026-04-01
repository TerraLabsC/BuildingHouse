using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class ImageToggleGroup : MonoBehaviour
{
    [Header("Цвета для фона")]
    [SerializeField] private Color normalBackgroundColor = Color.white;
    [SerializeField] private Color darkBackgroundColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    [Header("Цвета для текста")]
    [SerializeField] private Color normalTextColor = Color.white;
    [SerializeField] private Color darkTextColor = new Color(0.7f, 0.7f, 0.7f, 1f);

    [Header("Цвета для иконок (опционально)")]
    [SerializeField] private Color normalIconColor = Color.white;
    [SerializeField] private Color darkIconColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    [Header("Компоненты")]
    [SerializeField] private List<Button> buttons;
    [SerializeField] private List<Image> buttonImages; // Фон кнопок
    [SerializeField] private List<TextMeshProUGUI> buttonTexts; // Текст кнопок
    [SerializeField] private List<Image> buttonIcons; // Иконки на кнопках

    [Header("Настройки")]
    [SerializeField] private bool selectFirstButton = true;
    [SerializeField] private int defaultSelectedIndex = 0;

    private int currentSelectedIndex = -1;

    private void Start()
    {
        // Подписываемся на все кнопки
        for (int i = 0; i < buttons.Count; i++)
        {
            int index = i;
            buttons[i].onClick.AddListener(() => SelectButton(index));
        }

        // Выделяем кнопку по умолчанию
        if (selectFirstButton && buttons.Count > 0)
        {
            if (defaultSelectedIndex >= 0 && defaultSelectedIndex < buttons.Count)
            {
                SelectButton(defaultSelectedIndex);
            }
            else
            {
                SelectButton(0);
            }
        }
    }

    public void SelectButton(int index)
    {
        if (index < 0 || index >= buttons.Count) return;
        if (currentSelectedIndex == index) return; // Не перевыделяем ту же кнопку

        currentSelectedIndex = index;

        // Обновляем цвета всех кнопок
        for (int i = 0; i < buttons.Count; i++)
        {
            bool isSelected = (i == index);
            SetButtonColors(i, isSelected);
        }
    }

    private void SetButtonColors(int index, bool isSelected)
    {
        Color targetBackgroundColor = isSelected ? normalBackgroundColor : darkBackgroundColor;
        Color targetTextColor = isSelected ? normalTextColor : darkTextColor;
        Color targetIconColor = isSelected ? normalIconColor : darkIconColor;

        // Меняем цвет фона кнопки
        if (index < buttonImages.Count && buttonImages[index] != null)
        {
            buttonImages[index].color = targetBackgroundColor;
        }
        else
        {
            // Если список buttonImages пуст, пытаемся найти Image на самой кнопке
            Image buttonImage = buttons[index].GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = targetBackgroundColor;
            }
        }

        // Меняем цвет текста кнопки
        if (index < buttonTexts.Count && buttonTexts[index] != null)
        {
            buttonTexts[index].color = targetTextColor;
        }
        else
        {
            // Если список buttonTexts пуст, пытаемся найти TextMeshProUGUI в дочерних объектах
            TextMeshProUGUI buttonText = buttons[index].GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.color = targetTextColor;
            }
        }

        // Меняем цвет иконки
        if (index < buttonIcons.Count && buttonIcons[index] != null)
        {
            buttonIcons[index].color = targetIconColor;
        }
        else
        {
            // Если список buttonIcons пуст, пытаемся найти Image в дочерних объектах
            foreach (Transform child in buttons[index].transform)
            {
                Image icon = child.GetComponent<Image>();
                if (icon != null && icon != buttons[index].GetComponent<Image>())
                {
                    icon.color = targetIconColor;
                    break;
                }
            }
        }

        // Настраиваем цветовую схему самой кнопки
        ColorBlock colors = buttons[index].colors;
        colors.normalColor = targetBackgroundColor;
        colors.selectedColor = targetBackgroundColor;
        colors.highlightedColor = isSelected ? normalBackgroundColor : darkBackgroundColor;
        colors.pressedColor = isSelected ? normalBackgroundColor : darkBackgroundColor;
        buttons[index].colors = colors;
    }

    // Метод для установки цвета фона для конкретной кнопки
    public void SetButtonBackgroundColor(int index, Color color)
    {
        if (index < 0 || index >= buttons.Count) return;

        if (index < buttonImages.Count && buttonImages[index] != null)
        {
            buttonImages[index].color = color;
        }
        else
        {
            Image buttonImage = buttons[index].GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = color;
            }
        }
    }

    // Метод для установки цвета текста для конкретной кнопки
    public void SetButtonTextColor(int index, Color color)
    {
        if (index < 0 || index >= buttons.Count) return;

        if (index < buttonTexts.Count && buttonTexts[index] != null)
        {
            buttonTexts[index].color = color;
        }
        else
        {
            TextMeshProUGUI buttonText = buttons[index].GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.color = color;
            }
        }
    }

    // Метод для установки цвета иконки для конкретной кнопки
    public void SetButtonIconColor(int index, Color color)
    {
        if (index < 0 || index >= buttons.Count) return;

        if (index < buttonIcons.Count && buttonIcons[index] != null)
        {
            buttonIcons[index].color = color;
        }
        else
        {
            foreach (Transform child in buttons[index].transform)
            {
                Image icon = child.GetComponent<Image>();
                if (icon != null && icon != buttons[index].GetComponent<Image>())
                {
                    icon.color = color;
                    break;
                }
            }
        }
    }

    // Получить текущий выбранный индекс
    public int GetSelectedIndex()
    {
        return currentSelectedIndex;
    }

    // Получить выбранную кнопку
    public Button GetSelectedButton()
    {
        if (currentSelectedIndex >= 0 && currentSelectedIndex < buttons.Count)
        {
            return buttons[currentSelectedIndex];
        }
        return null;
    }

    // Принудительно обновить все кнопки
    public void RefreshAllButtons()
    {
        if (currentSelectedIndex >= 0)
        {
            SelectButton(currentSelectedIndex);
        }
    }

    private void OnDestroy()
    {
        // Отписываемся от событий
        for (int i = 0; i < buttons.Count; i++)
        {
            if (buttons[i] != null)
            {
                buttons[i].onClick.RemoveAllListeners();
            }
        }
    }
}