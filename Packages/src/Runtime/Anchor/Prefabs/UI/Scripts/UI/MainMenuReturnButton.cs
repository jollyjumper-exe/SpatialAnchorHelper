using UnityEngine;

public class MainMenuReturnButton : MonoBehaviour
{
    public void Return()
    {
        MainMenuManager.Instance.ReturnToLast();
    }
}
