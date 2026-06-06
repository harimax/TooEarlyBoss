using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 表示中 UI のボタン選択を共通化するヘルパー。
/// </summary>
public static class UISelectionUtility
{
    /// <summary>
    /// 指定ルート配下の最初の Button を取得。
    /// </summary>
    /// <param name="root">検索対象の UI ルート。</param>
    /// <returns>見つかった Button。なければ null。</returns>
    public static Button FindFirstButton(GameObject root)
    {
        return root != null ? root.GetComponentInChildren<Button>(true) : null;
    }

    /// <summary>
    /// UI ルートが表示中なら、指定ボタンを選択。
    /// </summary>
    /// <param name="root">表示状態を確認する UI ルート。</param>
    /// <param name="button">選択候補の Button。</param>
    /// <param name="lastSelected">直前に選択した GameObject。</param>
    /// <returns>選択を行った場合は true。</returns>
    public static bool SelectButtonIfRootActive(GameObject root, Button button, ref GameObject lastSelected)
    {
        if (root == null || !root.activeSelf || button == null)
        {
            return false;
        }

        return SelectIfNeeded(button.gameObject, ref lastSelected);
    }

    /// <summary>
    /// 既に選択済みでなければ EventSystem の選択先を更新。
    /// </summary>
    /// <param name="target">選択対象。</param>
    /// <param name="lastSelected">直前に選択した GameObject。</param>
    /// <returns>選択を行った場合は true。</returns>
    public static bool SelectIfNeeded(GameObject target, ref GameObject lastSelected)
    {
        if (EventSystem.current == null || target == null || lastSelected == target)
        {
            return false;
        }

        lastSelected = target;
        EventSystem.current.SetSelectedGameObject(target);
        return true;
    }
}
