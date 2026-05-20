using UnityEngine;
using UnityEngine.UI;

public class SelectLight : MonoBehaviour
{
    public Sprite RedSpriteShadow;
    public Sprite RedSprite;
    public Sprite GreenSpriteShadow;
    public Sprite GreenSprite;

    public ColorButton colorButton;

    private Image mainImage;
    private Image shadowImage;

    private void Start()
    {
        mainImage = GetComponent<Image>();
        if (transform.childCount > 0)
            shadowImage = transform.GetChild(0).GetComponent<Image>();
    }

    private void Update()
    {
        if (colorButton == null)
            return;

        bool active = colorButton.IsActiveColor;

        if (mainImage != null)
            mainImage.sprite = active ? GreenSprite : RedSprite;
            mainImage.rectTransform.sizeDelta = new Vector2(50, 50);

        if (shadowImage != null)
            shadowImage.sprite = active ? GreenSpriteShadow : RedSpriteShadow;
            shadowImage.rectTransform.sizeDelta = new Vector2(220, 220);
    }
}