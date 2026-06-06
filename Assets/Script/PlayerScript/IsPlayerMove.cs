using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤー移動を止める理由。
/// bool の上書きではなく理由単位で管理し、複数要因の競合を防ぐ。
/// </summary>
public enum PlayerMoveBlockReason
{
    Legacy,
    Training,
    BattlePreparing,
    BattleFinished,
    BossClear,
    GameOver,
    Pause,
    Cutscene,
    BossEvent,
    RideEvent
}

/// <summary>
/// プレイヤー移動可否を管理。
/// 移動禁止理由が 1 つもない場合だけ移動可能。
/// </summary>
public class IsPlayerMove : MonoBehaviour
{
    private static IsPlayerMove instance;

    private readonly HashSet<PlayerMoveBlockReason> blockReasons = new HashSet<PlayerMoveBlockReason>();

    [SerializeField] private bool debugCanMove;

    /// <summary>
    /// 既存コード互換用の移動可否プロパティ。
    /// 新規コードでは Block / Unblock の利用を優先。
    /// </summary>
    public bool CanMove
    {
        get => blockReasons.Count == 0;
        set
        {
            if (value)
            {
                ClearAllBlocks();
                return;
            }

            Block(PlayerMoveBlockReason.Legacy);
        }
    }

    /// <summary>
    /// Inspector で現在の移動可否を確認するための表示値。
    /// </summary>
    public bool DebugCanmove;

    /// <summary>
    /// 移動管理のインスタンスを登録し、初期移動可否を設定。
    /// </summary>
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            return;
        }

        instance = this;

        if (debugCanMove || DebugCanmove)
        {
            ClearAllBlocks();
            return;
        }

        // 初期状態は従来と同じく移動不可
        Block(PlayerMoveBlockReason.Legacy);
    }

    private void OnDestroy()
    {
        if (instance != this)
        {
            return;
        }

        // 破棄済みインスタンスを static 参照に残さない
        instance = null;
    }

    /// <summary>
    /// 既存コード向けのグローバル取得口。
    /// </summary>
    /// <returns>現在の移動管理インスタンス。</returns>
    public static IsPlayerMove GetInstance()
    {
        return instance;
    }

    /// <summary>
    /// 指定理由でプレイヤー移動を禁止。
    /// </summary>
    /// <param name="reason">移動禁止理由。</param>
    public void Block(PlayerMoveBlockReason reason)
    {
        blockReasons.Add(reason);
        SyncDebugValue();
    }

    /// <summary>
    /// 指定理由の移動禁止を解除。
    /// </summary>
    /// <param name="reason">解除する移動禁止理由。</param>
    public void Unblock(PlayerMoveBlockReason reason)
    {
        blockReasons.Remove(reason);
        SyncDebugValue();
    }

    /// <summary>
    /// すべての移動禁止理由を解除。
    /// 戦闘開始など、明確に操作可能へ戻す場面で利用。
    /// </summary>
    public void ClearAllBlocks()
    {
        blockReasons.Clear();
        SyncDebugValue();
    }

    /// <summary>
    /// 指定理由で移動が止められているか確認。
    /// </summary>
    /// <param name="reason">確認する移動禁止理由。</param>
    /// <returns>指定理由が登録済みなら true。</returns>
    public bool IsBlockedBy(PlayerMoveBlockReason reason)
    {
        return blockReasons.Contains(reason);
    }

    /// <summary>
    /// Inspector 確認用の表示値を現在の移動可否に同期。
    /// </summary>
    private void SyncDebugValue()
    {
        DebugCanmove = CanMove;
    }
}
