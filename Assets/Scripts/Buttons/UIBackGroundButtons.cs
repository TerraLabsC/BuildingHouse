using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIBackGroundButtons : MonoBehaviour
{
    public Image ButtonMain;
    public Image BackGround;
    public TextMeshProUGUI TextMeshProUGUI;

    public Color ButtonMainColor;
    public Color BackroundColor;
    public Color TextColor;

    private Color ButtonMainColorCurrent;
    private Color BackroundColorCurrent;
    private Color TextColorCurrent;

    public bool IsActive = false;

    private void Start()
    {
        ButtonMainColorCurrent = ButtonMain.color;
        BackroundColorCurrent = BackGround.color;
        TextColorCurrent = TextMeshProUGUI.color;

        if (IsActive)
        {
            OnButtonColor();
        }
    }

    public void OnButtonColor()
    {
        ButtonMain.color = ButtonMainColor;
        BackGround.color = BackroundColor;
        TextMeshProUGUI.color = TextColor;
    }

    public void OffButtonColor()
    {
        ButtonMain.color = ButtonMainColorCurrent;
        BackGround.color = BackroundColorCurrent;
        TextMeshProUGUI.color = TextColorCurrent;
    }
}
