using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Invector.vMelee;
using Invector.vCharacterController;
using TMPro;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class BossStageManager : MonoBehaviour
{
    public GameObject StartButton;
    [SerializeField] private TextMeshProUGUI ClearText;
    [SerializeField] private GameObject Boss;
    [SerializeField] private GameObject BossCamera;
    [SerializeField] private GameObject ClearButton;
    [SerializeField] private GameObject GameOverButtons;
    [SerializeField] private GameObject[] petBoss; 
    private GameObject player;
    private IBossController bossController;
    private IBossController[] petBossControllers;
    // Start is called before the first frame update
    void Start()
    {
        ClearText.text = "";
        bossController = Boss.GetComponent<IBossController>();
        bossController.PauseBoss();
        //ステージ4のペットボスも取得して一時停止
        if (petBoss.Length > 0)
        {
            petBossControllers = new IBossController[petBoss.Length];
            for (int i = 0; i < petBoss.Length; i++)
            {
                petBossControllers[i] = petBoss[i].GetComponent<IBossController>();
                petBossControllers[i].PauseBoss();
            }
        }
    }

    public void BossStartButton()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        var skillManager = player.GetComponent<SkillManager>();

        IsPlayerMove.GetInstance().CanMove = true;
        StartButton.SetActive(false);
        BossCamera.SetActive(false);
        bossController.ResumeBoss();
        //ステージ4のペットボスも再開
        if (petBossControllers != null && petBossControllers.Length > 0)
        {
            for (int i = 0; i < petBossControllers.Length; i++)
            {
                petBossControllers[i].ResumeBoss();
            }
        }

        if (skillManager != null)
        {
            skillManager.ActivePassiveSkill();
        }
        //プレイヤーの.OnDeadにGAMEOVER処理を追加する
        var vCharacter = player.GetComponent<vThirdPersonController>(); 
        vCharacter.onDead.AddListener((GameObject deadObject) =>
        {
            GameOverDelay().Forget();
        });

    }
    public void DefeatBoss()
    {
        DefeatBossDelay().Forget();
    }
    private async UniTask DefeatBossDelay()
    {
        Time.timeScale = 0.3f;
        await UniTask.Delay(1500);
        Time.timeScale = 1f;
        ClearText.text = "倒したぜ";
        Debug.Log("敵を倒した");
        ClearButton.SetActive(true);
    }
    private async UniTask GameOverDelay()
    {
        Time.timeScale = 0.3f;
        await UniTask.Delay(1500);
        Time.timeScale = 0.0f;
         ClearText.text = "死んだぜ/nどうする？";
        GameOverButtons.SetActive(true);
    }

}
