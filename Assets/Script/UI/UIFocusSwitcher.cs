using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class UIFocusSwitcher : MonoBehaviour
{
    [Header("育成ボタン群（無効化対象）")]
    [SerializeField] private Selectable[] trainingSelectables;

    [Header("育成が無効になった時にフォーカスさせるボタン")]
    [SerializeField] private Selectable battleButton;

    /// <summary>
    /// 育成ボタンを無効化する前後で、フォーカスが育成側に残っていたら戦闘へ移す
    /// </summary>
    public async UniTaskVoid MoveFocusToBattleIfTrainingSelected()
    {
        if (EventSystem.current == null || battleButton == null) return;

        var current = EventSystem.current.currentSelectedGameObject;
        if (current == null) return;

        // 現在フォーカスが育成ボタン群にあるかチェック
        bool isTrainingSelected = false;
        foreach (var s in trainingSelectables)
        {
            if (s != null && s.gameObject == current)
            {
                isTrainingSelected = true;
                break;
            }
        }

        if (!isTrainingSelected) return;

        // UIのinteractable変更が反映されるのを待つ（重要）
        await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);

        EventSystem.current.SetSelectedGameObject(null);
        battleButton.Select();
    }
}
