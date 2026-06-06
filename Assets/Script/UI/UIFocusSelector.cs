using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UI 表示直後の既定フォーカス設定を担当。
/// </summary>
public static class UIFocusSelector
{
    /// <summary>
    /// 指定した Selectable が選択可能な場合だけフォーカスを移動。
    /// </summary>
    /// <param name="target">フォーカス先の Selectable。</param>
    /// <param name="token">画面破棄時のキャンセルトークン。</param>
    public static async UniTask SelectAsync(Selectable target, CancellationToken token)
    {
        // EventSystem や選択先がない画面ではフォーカス処理不要
        if (EventSystem.current == null || target == null)
        {
            return;
        }

        // SetActive 直後のレイアウト反映を待ってから選択
        await UniTask.Yield(PlayerLoopTiming.PostLateUpdate, token);

        // 非表示、または操作不可の UI は選択対象外
        if (!target.gameObject.activeInHierarchy || !target.IsInteractable())
        {
            return;
        }

        EventSystem.current.SetSelectedGameObject(null);
        target.Select();
    }
}
