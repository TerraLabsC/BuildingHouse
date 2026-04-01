using UnityEngine;

public class BackgroundSetter : MonoBehaviour
{
    public void SetBackgroundForNextScene(int index)
    {
        BackgroundChanger.SetGlobalBackground(index);
    }
}