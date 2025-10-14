using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[Serializable]
public struct StateToPrefab
{
    public string state;
    public GameObject prefab;
}

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance { get; private set; }
    public string currentState { get; private set; }
    public string lastState { get; private set; }

    [SerializeField] private List<StateToPrefab> stateToPrefabList;
    [SerializeField] private GameObject canvas;
    private Dictionary<string, GameObject> stateToPrefabDict = new Dictionary<string, GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (stateToPrefabList != null)
        {
            foreach (StateToPrefab stateToPrefab in stateToPrefabList)
            {
                stateToPrefabDict[stateToPrefab.state] = stateToPrefab.prefab;
            }
            
            string firstState = new List<string>(stateToPrefabDict.Keys)[0];
            SwitchTo(firstState);
        }
    }

    public void SwitchTo(string newState)
    {
        if (!stateToPrefabDict.ContainsKey(newState) || stateToPrefabDict[newState] == null) return;

        lastState = currentState;
        currentState = newState;

        foreach (Transform child in canvas.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        Instantiate(stateToPrefabDict[newState], canvas.transform);
    }

    public void ReturnToLast()
    {
        SwitchTo(lastState);
    }
}
