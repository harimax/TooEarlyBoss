using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;


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
    }
}
