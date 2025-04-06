using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalStageUIManager : MonoBehaviour
{
    public static NormalStageUIManager Instance;

    [SerializeField] private GameObject startText;
    [SerializeField] private GameObject clearText;

    void Awake()
    {
        Instance = this;
        startText.SetActive(true);
        clearText.SetActive(false);
    }

    public void HideStartText() => startText.SetActive(false);
    public void ShowClearText() => clearText.SetActive(true);
}
