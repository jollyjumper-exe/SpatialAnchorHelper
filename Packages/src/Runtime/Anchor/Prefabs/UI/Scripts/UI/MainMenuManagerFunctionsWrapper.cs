using UnityEngine;

public class MainMenuManagerFunctionsWrapper : MonoBehaviour
{
    private MainMenuManager mainMenuManager; 

    void Awake(){
        if(MainMenuManager.Instance != null){
            mainMenuManager = MainMenuManager.Instance;
        }
    }

    public void SwitchTo(string newState){
        if(mainMenuManager == null) return;

        mainMenuManager.SwitchTo(newState);
    }

    public void ReturnToLast(){
        if(mainMenuManager == null) return;

        mainMenuManager.ReturnToLast();
    }
}
