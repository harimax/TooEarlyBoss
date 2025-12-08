using System.Collections;
using UnityEngine;
using Invector;
using Cinemachine;
using Invector.vMelee;
using Invector.vCharacterController;
using Cysharp.Threading.Tasks;
using System;
public class GrowPhaseUIController : MonoBehaviour
{
    private enum Mode
    {
        Training,
        Battle
    }
    [Header("References")]
    [SerializeField] private GameObject MainCameraObject;//Invectorのやつをアタッチ
    [SerializeField] private GameObject trainingCameraObject;
    [SerializeField] private GameObject trainingUI;
    [SerializeField] private GameObject gameUI;
    [SerializeField] private GameObject brainCameraObject;//Brainのやつをアタッチ
    [SerializeField] private BattleStartController battleStartController;
    [Header("Camera Objects")]
    private CinemachineVirtualCamera trainingCameraVirtual;
    private CinemachineVirtualCamera mainCameraVirtual;
    private CinemachineBrain brainCameraObj;
    [Header("UI")]
    private bool IsGameUI;
    private bool IstrainingUI;
    private void Awake()
    {
        //カメラが存在しないときは直接探してとる
        if(MainCameraObject == null)
        {
            MainCameraObject = GameObject.FindWithTag("MainCamera");
            Debug.Log(mainCameraVirtual);
        }

        AttachPlayerData();
    }
    /// <summary>
    /// 戦闘準備モードへ移行する
    /// </summary>
    public void PrepareMissionButton()
    {
        // Debug.Log("戦闘移行");
        trainingCameraVirtual.Priority = 5;
        Debug.Log("プレイヤーカメラ"+mainCameraVirtual.Priority);
        SetAllUIInactive();
        IsGameUI = true;
        IstrainingUI = false;
        AttachPlayerData();
        _ = SwitchCameraAsync(); // ← ここもUniTaskに変更
    }
    /// <summary>
    /// ミッション開始ボタン
    /// </summary>
    public void StartMissionButton()
    {
        //バトル開始メソッドを呼び出す
        battleStartController.StartBattle();
        // UI非表示
        SetAllUIInactive();
    }

    /// <summary>
    /// 修行モードに戻るボタン
    /// </summary>
    public void ReturntTrainingButton()
    {
        // Debug.Log("修行に戻る");
        trainingCameraVirtual.Priority = 30;
        Debug.Log("プレイヤーカメラ"+mainCameraVirtual.Priority);
        SetAllUIInactive();
        IsGameUI = false;
        IstrainingUI = true;
        _ = SwitchCameraAsync(); // ← ここもUniTaskに変更
    }
    // カメラ遷移完了まで待ってからUIとCanMoveを切り替える
    private async UniTask SwitchCameraAsync()
    {
        // カメラの優先度変更反映待ち（1フレーム待つ）
        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

        // ブレンド完了待ち
        await UniTask.WaitUntil(() => !brainCameraObj.IsBlending);
        //戦闘シーンの際はGameUIを起動
        if (IsGameUI)
        {
            gameUI.SetActive(true);
            IsGameUI = false;
        }
        //修行シーンの際はtrainingUIを起動
        if (IstrainingUI)
        {
            trainingUI.SetActive(true);
            IstrainingUI = false;
        }
    }

    /// <summary>
    /// UIをすべて非表示にする
    /// </summary>
    private void SetAllUIInactive()
    {
        trainingUI.SetActive(false);
        gameUI.SetActive(false);
    }
    /// <summary>
    /// ミッション開始に必要なデータを格納
    /// </summary>
    public void AttachPlayerData()
    {
        // カメラ関連
        var MainCameraObject = GameObject.FindWithTag("MainCamera");
        mainCameraVirtual = MainCameraObject.GetComponent<CinemachineVirtualCamera>();
        brainCameraObj = brainCameraObject.GetComponent<CinemachineBrain>();
        trainingCameraVirtual = trainingCameraObject.GetComponentInChildren<CinemachineVirtualCamera>();
    }
}
