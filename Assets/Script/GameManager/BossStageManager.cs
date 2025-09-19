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
    [SerializeField] private GameObject Boss;
    [SerializeField] private GameObject BossCamera;
    [SerializeField] private GameObject ClearButton;
    private GameObject player;
    private IBossController bossController;
    // Start is called before the first frame update
    void Start()
    {
        ClearText.text = "";
        bossController = Boss.GetComponent<IBossController>();
        bossController.PauseBoss();
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void BossStartButton()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        var skillManager = player.GetComponent<SkillManager>();

        IsPlayerMove.GetInstance().CanMove = true;
        StartButton.SetActive(false);
        BossCamera.SetActive(false);
        bossController.ResumeBoss();

        if (skillManager != null)
        {
            skillManager.ActivePassiveSkill();
        }
    
    }
    public void DefeatBoss()
    {
        ClearText.text = "倒したぜ";
        Debug.Log("敵を倒した");
        ClearButton.SetActive(true);
    }
}
