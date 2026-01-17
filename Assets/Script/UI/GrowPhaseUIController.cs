
using UnityEngine;
using Cinemachine;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class GrowPhaseUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject playerCameraObject;//Invectorのやつをアタッチ
    [SerializeField] private GameObject trainingCameraObject;
    [SerializeField] private GameObject BattlePrepareCameraObject;//Brainのやつをアタッチ
    [SerializeField] private GameObject trainingUI;
    [SerializeField] private GameObject gameUI;
    [SerializeField] private GameObject brainCameraObject;//Brainのやつをアタッチ
    [SerializeField] private Transform battleStartPoint;
    [SerializeField] private GameObject player;
    [SerializeField] private BattleStartController battleStartController;
    [Header("Camera Objects")]
    private CinemachineVirtualCamera trainingCameraVirtual;
    private CinemachineVirtualCamera mainCameraVirtual;
    private CinemachineVirtualCamera battlePrepareCameraVirtual;
    private CinemachineBrain brainCamera;
    [Header("UI")]
    private bool IsGameUI;
    private bool IstrainingUI;
    [Header("UI Focus")]
    [SerializeField] private Selectable trainingDefaultSelectable; // 例：Attackボタン
    [SerializeField] private Selectable gameDefaultSelectable;     // GameUI側の最初ボタン

    // Priorityは「絶対値」で固定
    private const int PRI_LOW = 0;
    private const int PRI_MID = 10;
    private const int PRI_HIGH = 20;

    private CameraMode _mode;

    private enum CameraMode
    {
        Training,       // 育成カメラ
        BattlePrepare,  // 戦闘準備カメラ
        Follow          // 戦闘中（プレイヤー追従カメラ）
    }


    private void Awake()
    {
        SetAllUIInactive();
        CacheCameraReferences();//カメラやBrainの参照を1度だけキャッシュ
        trainingUI?.SetActive(true); // 初期状態が修行UIなら
        SetPlayerGravity(false); // 育成中は重力をオフにする
        // Inspector 未設定時の保険
        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
        }

    }

    /// <summary>
    /// 戦闘準備モードへ移行する
    /// </summary>
    public void PrepareMissionButton()
    {
        SetAllUIInactive();
        SetCameraMode(CameraMode.BattlePrepare);
        IsGameUI = true;
        IstrainingUI = false;
        SetPlayerGravity(false); // 戦闘開始前は重力を切って育成待機状態にする
        SwitchCameraAsync().Forget();
    }
    /// <summary>
    /// ミッション開始ボタン
    /// </summary>
    public void StartMissionButton()
    {
        // UI非表示
        SetAllUIInactive();
        SetCameraMode(CameraMode.Follow);
        SetPlayerGravity(true); // バトル開始時は重力をオンにする
        player.transform.position = battleStartPoint.position;
        battleStartController.StartBattle();//バトル開始メソッドを呼び出す
    }

    /// <summary>
    /// 修行モードに戻るボタン
    /// </summary>
    public void ReturntTrainingButton()
    {
        SetCameraMode(CameraMode.Training);
        SetAllUIInactive();
        IsGameUI = false;
        IstrainingUI = true;
        SetPlayerGravity(false); // 育成に戻る時は重力をオフにする
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
            await SelectUIFocusAsync(gameDefaultSelectable);
        }
        //修行シーンの際はtrainingUIを起動
        if (IstrainingUI)
        {
            trainingUI.SetActive(true);
            IstrainingUI = false;
            await SelectTrainingUIFocusAsync();
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
        if (playerCameraObject == null)
        {
            playerCameraObject = GameObject.FindWithTag("MainCamera");
        }
        if (playerCameraObject != null)
        {
            mainCameraVirtual = playerCameraObject.GetComponent<CinemachineVirtualCamera>();
        }

        // 修行カメラ
        if (trainingCameraObject != null)
        {
            trainingCameraVirtual = trainingCameraObject.GetComponentInChildren<CinemachineVirtualCamera>();
            Debug.Log(trainingCameraVirtual);
        }
        // 戦闘準備カメラ
        if (BattlePrepareCameraObject != null)
        {
            battlePrepareCameraVirtual = BattlePrepareCameraObject.GetComponent<CinemachineVirtualCamera>();
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
    /// <summary>
    /// 修行UIのデフォルト選択肢にフォーカスを移動する
    /// </summary>
    private async UniTask SelectTrainingUIFocusAsync()
    {
        // EventSystemや参照が無いなら何もしない
        if (EventSystem.current == null || trainingDefaultSelectable == null) return;

        // UIの有効化＆レイアウト反映を待つ（これが超重要）
        await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);

        // もし対象ボタンが非表示/非活性なら選択できないのでガード
        if (!trainingDefaultSelectable.gameObject.activeInHierarchy) return;
        if (!trainingDefaultSelectable.IsInteractable()) return;

        // 既存選択を一度クリアしてからSelect（これが安定）
        EventSystem.current.SetSelectedGameObject(null);
        trainingDefaultSelectable.Select();
    }
    /// <summary>
    /// 指定UIのデフォルト選択肢にフォーカスを移動する
    /// </summary>
    private async UniTask SelectUIFocusAsync(Selectable target)
    {
        if (EventSystem.current == null || target == null) return;

        // SetActive(true) 直後はレイアウト更新中のことがあるので、UI反映を待つ
        await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);

        // 選択可能状態チェック
        if (!target.gameObject.activeInHierarchy) return;
        if (!target.IsInteractable()) return;

        // 既存選択をクリアしてからSelect（安定）
        EventSystem.current.SetSelectedGameObject(null);
        target.Select();
    }
    /// <summary>
    /// カメラモードを設定する
    /// </summary>
    /// <param name="mode"></param>
    private void SetCameraMode(CameraMode mode)
    {
        _mode = mode;

        // 毎回3台すべてを確定させる（ここが重要）
        trainingCameraVirtual.Priority = PRI_LOW;
        battlePrepareCameraVirtual.Priority = PRI_LOW;
        mainCameraVirtual.Priority = PRI_LOW;

        switch (mode)
        {
            case CameraMode.Training:
                trainingCameraVirtual.Priority = PRI_HIGH;
                break;

            case CameraMode.BattlePrepare:
                battlePrepareCameraVirtual.Priority = PRI_HIGH;
                break;

            case CameraMode.Follow:
                mainCameraVirtual.Priority = PRI_HIGH; // プレイヤー追従のvcamをこれに
                break;
        }
    }

    /// <summary>
    /// プレイヤーの重力をオン/オフする。
    /// </summary>
    /// <param name="enabled">オンにする場合はtrue</param>
    private void SetPlayerGravity(bool enabled)
    {
        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
        }
        if (player == null) return;

        var rb = player.GetComponent<Rigidbody>();
        if (rb == null) return;

        rb.useGravity = enabled;
    }

}
