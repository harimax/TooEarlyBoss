using System.Threading;
using Cinemachine;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 育成フェーズ内の Cinemachine カメラ優先度とブレンド待機を担当。
/// </summary>
public sealed class GrowPhaseCameraController
{
    private const int PriorityLow = 0;
    private const int PriorityHigh = 20;

    private readonly CinemachineVirtualCamera trainingCameraVirtual;
    private readonly CinemachineVirtualCamera mainCameraVirtual;
    private readonly CinemachineVirtualCamera battlePrepareCameraVirtual;
    private readonly CinemachineBrain brainCamera;

    /// <summary>
    /// カメラ用 GameObject から必要な Cinemachine 参照を取得。
    /// </summary>
    /// <param name="playerCameraObject">プレイヤー追従カメラの GameObject。</param>
    /// <param name="trainingCameraObject">育成画面カメラの GameObject。</param>
    /// <param name="battlePrepareCameraObject">戦闘準備カメラの GameObject。</param>
    /// <param name="brainCameraObject">CinemachineBrain を持つカメラ GameObject。</param>
    public GrowPhaseCameraController(
        GameObject playerCameraObject,
        GameObject trainingCameraObject,
        GameObject battlePrepareCameraObject,
        GameObject brainCameraObject)
    {
        // Inspector 未設定時は MainCamera タグから追従カメラを補完
        if (playerCameraObject == null)
        {
            playerCameraObject = GameObject.FindWithTag("MainCamera");
        }

        // Brain 未設定時は Camera.main を優先して補完
        if (brainCameraObject == null && Camera.main != null)
        {
            brainCameraObject = Camera.main.gameObject;
        }

        mainCameraVirtual = playerCameraObject != null
            ? playerCameraObject.GetComponent<CinemachineVirtualCamera>()
            : null;

        trainingCameraVirtual = trainingCameraObject != null
            ? trainingCameraObject.GetComponentInChildren<CinemachineVirtualCamera>()
            : null;

        battlePrepareCameraVirtual = battlePrepareCameraObject != null
            ? battlePrepareCameraObject.GetComponent<CinemachineVirtualCamera>()
            : null;

        brainCamera = brainCameraObject != null
            ? brainCameraObject.GetComponent<CinemachineBrain>()
            : null;
    }

    /// <summary>
    /// 指定したモードのカメラだけ優先度を上げる。
    /// </summary>
    /// <param name="mode">切り替え先カメラモード。</param>
    public void SetMode(GrowPhaseCameraMode mode)
    {
        SetPriority(trainingCameraVirtual, PriorityLow);
        SetPriority(battlePrepareCameraVirtual, PriorityLow);
        SetPriority(mainCameraVirtual, PriorityLow);

        switch (mode)
        {
            case GrowPhaseCameraMode.Training:
                SetPriority(trainingCameraVirtual, PriorityHigh);
                break;

            case GrowPhaseCameraMode.BattlePrepare:
                SetPriority(battlePrepareCameraVirtual, PriorityHigh);
                break;

            case GrowPhaseCameraMode.Follow:
                SetPriority(mainCameraVirtual, PriorityHigh);
                break;
        }
    }

    /// <summary>
    /// Cinemachine のブレンドが終わるまで待機。
    /// </summary>
    /// <param name="token">画面破棄時のキャンセルトークン。</param>
    public async UniTask WaitForBlendAsync(CancellationToken token)
    {
        // 優先度変更が Cinemachine に反映されるフレームまで待機
        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, token);

        // Brain 未設定時はブレンド待ちを省略
        if (brainCamera == null)
        {
            return;
        }

        await UniTask.WaitUntil(() => !brainCamera.IsBlending, cancellationToken: token);
    }

    /// <summary>
    /// カメラ参照がある場合だけ Priority を変更。
    /// </summary>
    /// <param name="camera">対象の CinemachineVirtualCamera。</param>
    /// <param name="priority">設定する Priority 値。</param>
    private static void SetPriority(CinemachineVirtualCamera camera, int priority)
    {
        // シーン差分で未設定カメラがあっても他カメラの切替を継続
        if (camera != null)
        {
            camera.Priority = priority;
        }
    }
}
