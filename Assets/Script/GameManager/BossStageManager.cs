using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Invector.vMelee;
using Invector.vCharacterController;
using TMPro;
using UnityEngine.UI;

public class BossStageManager : MonoBehaviour
{
    public GameObject StartButton;
    [SerializeField] private TextMeshProUGUI ClearText;
    // Start is called before the first frame update
    void Start()
    {
        ClearText.text = "";
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void BossStartButton()
    {
        IsPlayerMove.GetInstance().CanMove = true;
        StartButton.SetActive(false);

    }
    public void DefeatBoss()
    {
        ClearText.text = "倒したぜ";
        Debug.Log("敵を倒した");
        
    }
}
