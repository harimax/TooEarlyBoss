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
    [Header("References")]
    [SerializeField] private GameObject mainCameraObject;//Invectorのやつをアタッチ
    [SerializeField] private GameObject trainingCameraObject;
    [SerializeField] private GameObject trainingUI;
    [SerializeField] private GameObject gameUI;
    [SerializeField] private GameObject brainCameraObject;//Brainのやつをアタッチ
    [SerializeField] private BattleStartController battleStartController;
    [Header("Camera Objects")]
    private CinemachineVirtualCamera trainingCameraVirtual;
    private CinemachineVirtualCamera mainCameraVirtual;
    private CinemachineBrain brainCamera;
    [Header("UI")]
    private bool IsGameUI;
    private bool IstrainingUI;
    private void Awake()
    {
        SetAllUIInactive();
        trainingUI?.SetActive(true); // 初期状態が修行UIなら
        CacheCameraReferences();//カメラやBrainの参照を1度だけキャッシュ
    }
    /// <summary>
    /// 戦闘準備モードへ移行する
    /// </summary>
    public void PrepareMissionButton()
    {
        trainingCameraVirtual.Priority = 5;
        SetAllUIInactive();
        IsGameUI = true;
        IstrainingUI = false;
        SwitchCameraAsync().Forget(); 
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
        trainingCameraVirtual.Priority = 30;
        SetAllUIInactive();
        IsGameUI = false;
        IstrainingUI = true;
        SwitchCameraAsync().Forget();
    }
    // カメラ遷移完了まで待ってからUIとCanMoveを切り替える
    private async UniTask SwitchCameraAsync()
    {
        // カメラの優先度変更反映待ち（1フレーム待つ）
        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

        // ブレンド完了待ち
        await UniTask.WaitUntil(() => !brainCamera.IsBlending);
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
        if (trainingUI) trainingUI.SetActive(false);
        if (gameUI) gameUI.SetActive(false);
    }
    /// <summary>
    /// カメラやBrainの参照を1度だけキャッシュ
    /// </summary>
    private void CacheCameraReferences()
    {
        // メインカメラ
        if (mainCameraObject == null)
        {
            mainCameraObject = GameObject.FindWithTag("MainCamera");
        }
        if (mainCameraObject != null)
        {
            mainCameraVirtual = mainCameraObject.GetComponent<CinemachineVirtualCamera>();
        }

        // 修行カメラ
        if (trainingCameraObject != null)
        {
            trainingCameraVirtual = trainingCameraObject.GetComponentInChildren<CinemachineVirtualCamera>();
        }

        // Brain
        if (brainCameraObject == null && Camera.main != null)
        {
            brainCameraObject = Camera.main.gameObject;
        }
        if (brainCameraObject != null)
        {
            brainCamera = brainCameraObject.GetComponent<CinemachineBrain>();
        }
    }
}
