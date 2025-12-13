using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillSet : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI skillDescriptionText;
    [SerializeField] private Button skillButton;

    //スキルカードを表示する際に呼び出すメソッド　
    //スキル名とボタンクリック時に起こす処理を引数とする
    public void Setup(SkillBase skill, System.Action onClick)
    {
        // 表示更新
        skillNameText.text = skill.skillName;
        skillDescriptionText.text = skill.description;

        // ボタンイベントを差し替え
        skillButton.onClick.RemoveAllListeners();
        skillButton.onClick.AddListener(() => onClick.Invoke());
    }
}

