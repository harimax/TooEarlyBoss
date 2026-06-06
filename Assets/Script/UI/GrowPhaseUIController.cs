using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 育成フェーズの画面遷移、UI 表示、バトル開始操作を担当。
/// カメラ切替は <see cref="GrowPhaseCameraController"/>、UI フォーカスは <see cref="UIFocusSelector"/> に委譲。
/// </summary>
public class GrowPhaseUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject playerCameraObject;
    [SerializeField] private GameObject trainingCameraObject;
    [SerializeField] private GameObject BattlePrepareCameraObject;
    [SerializeField] private GameObject trainingUI;
    [SerializeField] private GameObject gameUI;
    [SerializeField] private GameObject brainCameraObject;
    [SerializeField] private Transform battleStartPoint;
    [SerializeField] private GameObject player;
    [SerializeField] private BattleStartController battleStartController;

    [Header("UI Focus")]
    [SerializeField] private Selectable trainingDefaultSelectable;
    [SerializeField] private Selectable gameDefaultSelectable;

    private GrowPhaseCameraController cameraController;
    private PendingUI pendingUI;

    /// <summary>
    /// カメラ切替完了後に表示する UI 種別。
    /// </summary>
    private enum PendingUI
    {
        None,
        Training,
        Game
    }

    /// <summary>
    /// 初期表示と、カメラ制御用サービスの準備を実行。
    /// </summary>
    private void Awake()
    {
        cameraController = new GrowPhaseCameraController(
            playerCameraObject,
            trainingCameraObject,
            BattlePrepareCameraObject,
            brainCameraObject);

        SetAllUIInactive();
        SetUIActive(trainingUI, true);

        // Inspector 未設定時だけタグ検索でプレイヤーを補完
        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
        }
    }

    /// <summary>
    /// 戦闘準備画面へ遷移。
    /// </summary>
    public void PrepareMissionButton()
    {
        SetAllUIInactive();
        pendingUI = PendingUI.Game;
        cameraController.SetMode(GrowPhaseCameraMode.BattlePrepare);
        SwitchCameraAsync().Forget();
    }

    /// <summary>
    /// 戦闘開始位置へプレイヤーを移動し、バトル開始処理を呼び出し。
    /// </summary>
    public void StartMissionButton()
    {
        SetAllUIInactive();
        pendingUI = PendingUI.None;
        cameraController.SetMode(GrowPhaseCameraMode.Follow);

        if (player != null && battleStartPoint != null)
        {
            player.transform.position = battleStartPoint.position;
        }

        // バトル開始演出や敵起動は専用コンポーネントへ委譲
        if (battleStartController != null)
        {
            battleStartController.StartBattle();
        }
    }

    /// <summary>
    /// 育成画面へ戻る。
    /// </summary>
    public void ReturntTrainingButton()
    {
        SetAllUIInactive();
        pendingUI = PendingUI.Training;
        cameraController.SetMode(GrowPhaseCameraMode.Training);
        SwitchCameraAsync().Forget();
    }

    /// <summary>
    /// カメラブレンド完了後、遷移先 UI を表示して既定フォーカスを設定。
    /// </summary>
    private async UniTask SwitchCameraAsync()
    {
        await cameraController.WaitForBlendAsync(this.GetCancellationTokenOnDestroy());

        switch (pendingUI)
        {
            case PendingUI.Game:
                SetUIActive(gameUI, true);
                pendingUI = PendingUI.None;
                await UIFocusSelector.SelectAsync(gameDefaultSelectable, this.GetCancellationTokenOnDestroy());
                break;

            case PendingUI.Training:
                SetUIActive(trainingUI, true);
                pendingUI = PendingUI.None;
                await UIFocusSelector.SelectAsync(trainingDefaultSelectable, this.GetCancellationTokenOnDestroy());
                break;
        }
    }

    /// <summary>
    /// 育成 UI と戦闘準備 UI をまとめて非表示。
    /// </summary>
    private void SetAllUIInactive()
    {
        SetUIActive(trainingUI, false);
        SetUIActive(gameUI, false);
    }

    /// <summary>
    /// UI 参照が設定済みの場合だけ表示状態を変更。
    /// </summary>
    /// <param name="ui">表示状態を変更する UI オブジェクト。</param>
    /// <param name="active">表示する場合は true。</param>
    private static void SetUIActive(GameObject ui, bool active)
    {
        // シーン差分で UI が未設定でも遷移処理を止めない
        if (ui != null)
        {
            ui.SetActive(active);
        }
    }
}
