using UnityEngine;
using Invector.vCharacterController;

/// <summary>
/// ミッション中のプレイヤーの制御を行うクラス
/// </summary>
public class BattlePlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject player;
    [SerializeField] private GrowPhaseUIController growUIController;
    [SerializeField] private TrianingButton trianingButton;
    private vThirdPersonController vPersonController;
    private Vector3 initPosition;
    private void Start()
    {
        // ゲームマネージャーから各種のコンポーネントを取得
        growUIController = gameObject.GetComponent<GrowPhaseUIController>();
        trianingButton = gameObject.GetComponent<TrianingButton>();

        // Inspector 未設定時の保険
        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
        }
        if (player != null)
        {
            initPosition = player.transform.position;
            vPersonController = player.GetComponent<vThirdPersonController>();
        }
    }

    /// <summary>
    /// ミッション終了後、プレイヤーをトレーニング状態に戻す
    /// </summary>
    public void ResetPlayerToInitialPosition()
    {
        if (player == null || vPersonController == null) return;

        // 位置を戻す
        player.transform.position = initPosition;
        player.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);

        // 修行 UI に戻す
        if (growUIController != null)
        {
            growUIController.ReturntTrainingButton();
        }
        // トレーニングボタン再有効化
        if (trianingButton != null)
        {
            trianingButton.SetButtonsInteractable();
        }
        // 体力・死亡フラグをリセット
        vPersonController._currentHealth = vPersonController.maxHealth;
        vPersonController.isDead = false;

        // 移動停止＋リジッドボディリセット
        DisableMovePlayer();
        ResetPlayerRigidbody();
    }
    /// <summary>
    /// プレイヤーを動けなくする
    /// </summary>
    public void DisableMovePlayer()
    {
        IsPlayerMove.GetInstance().CanMove = false;
    }
    /// <summary>
    //プレイヤーのRigidBodyで移動を止める
    /// <summary>
    public void ResetPlayerRigidbody()
    {
        var rb = player.GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;

        vPersonController.enabled = false;
        vPersonController.enabled = true;
    }
}
