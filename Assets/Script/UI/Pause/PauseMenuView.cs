using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// ポーズ画面に表示する情報を更新するビュークラス。
/// ステータスと取得スキルの描画のみを担当する。
/// </summary>
public class PauseMenuView : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject rootPanel;

    [Header("Status UI")]
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Skill UI")]
    [SerializeField] private TextMeshProUGUI skillText;
    [SerializeField] private SkillManager skillManager;

    private const string EmptySkillLabel = "未取得";

    private void Awake()
    {
        if (rootPanel != null)
        {
            rootPanel.SetActive(false);
        }
    }

    /// <summary>
    /// ポーズ画面を表示する。
    /// </summary>
    public void Show()
    {
        if (rootPanel == null) return;

        rootPanel.SetActive(true);
    }

    /// <summary>
    /// ポーズ画面を非表示にする。
    /// </summary>
    public void Hide()
    {
        if (rootPanel == null) return;

        rootPanel.SetActive(false);
    }

    /// <summary>
    /// ポーズ画面の表示内容を最新化する。
    /// </summary>
    public void Refresh()
    {
        UpdateStatusText();
        UpdateSkillText();
    }

    /// <summary>
    /// 現在の成長ステータスを表示する。
    /// </summary>
    private void UpdateStatusText()
    {
        if (statusText == null) return;

        if (!PlayerGrowRepository.LoadParameters(out var growth) || growth == null)
        {
            statusText.text = "ステータス情報がありません";
            return;
        }

        var builder = new StringBuilder();
        builder.AppendLine("ステータス");
        builder.AppendLine($"Power : {growth.PlayerPower}");
        builder.AppendLine($"Health: {growth.PlayerHealth}");
        builder.AppendLine($"Stamina: {growth.PlayerStamina}");
        builder.AppendLine($"Special: {growth.PlayerSpecial}");

        statusText.text = builder.ToString();
    }

    /// <summary>
    /// 取得済みスキルを一覧表示する。
    /// </summary>
    private void UpdateSkillText()
    {
        if (skillText == null) return;

        EnsureSkillManager();

        if (skillManager == null || skillManager.acquiredSkills == null || skillManager.acquiredSkills.Count == 0)
        {
            skillText.text = $"取得スキル\n{EmptySkillLabel}";
            return;
        }

        var builder = new StringBuilder();
        builder.AppendLine("取得スキル");

        foreach (var skill in skillManager.acquiredSkills)
        {
            if (skill == null) continue;
            builder.AppendLine($"・{skill.skillName}");
        }

        skillText.text = builder.ToString();
    }

    /// <summary>
    /// SkillManager 参照が未設定の場合はプレイヤーから取得する。
    /// </summary>
    private void EnsureSkillManager()
    {
        if (skillManager != null) return;

        var player = GameObject.FindWithTag("Player");
        if (player == null) return;

        skillManager = player.GetComponent<SkillManager>();
    }
}
