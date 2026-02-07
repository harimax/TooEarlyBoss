using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;
using System.Linq;


public class SkillDiscardDialog : MonoBehaviour
{
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject buttonPrefab;

    private Action<SkillBase> onDiscarded;

    /// <summary>
    /// 候補リストと破棄コールバックを受け取り、ボタンを生成
    /// </summary>
    public void Initialize(List<SkillBase> candidates, Action<SkillBase> onDiscarded)
    {
        this.onDiscarded = onDiscarded;
        foreach (var skill in candidates)
        {
            var btnObj = Instantiate(buttonPrefab, contentParent);
            var text = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            text.text = skill.skillName;
            var btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                this.onDiscarded(skill);
                Destroy(gameObject);
            });
        }
        // 最初のボタンにフォーカスを移動
        FocusFirstDiscardButton().Forget();
    }
    /// <summary>
    /// 削除スキルUIの最初の選択肢にフォーカスを移動する
    /// </summary>
    /// <returns></returns>
    private async UniTaskVoid FocusFirstDiscardButton()
    {
        if (EventSystem.current == null) return;

        await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);

        var buttons = contentParent.GetComponentsInChildren<Button>(true);
        var first = buttons
            .Select(button => (Selectable)button)
            .FirstOrDefault(button =>
                button != null &&
                button.IsInteractable() &&
                button.gameObject.activeInHierarchy);

        if (first == null) return;

        EventSystem.current.SetSelectedGameObject(null);
        first.Select();
    }
}
