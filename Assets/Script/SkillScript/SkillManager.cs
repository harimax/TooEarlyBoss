using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using NUnit.Framework;
public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance { get; private set; }

    public const int MaxTotalSkills = 7;
    public const int MaxSpecialSkills = 2;
    /// <summary>取得済みスキル一覧</summary>
    public List<SkillBase> acquiredSkills = new List<SkillBase>();
    [SerializeField] private Transform player;
    private void Awake()
    {
        // シングルトンパターンの実装
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning(
                $"複数の SkillManager が存在しています。古い方({Instance.name})を残し、新しい方({name})を破棄します。");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);   // ここで本当に1体だけが残る
    }

    //ミッション開始時に常時発動するスキルを適応させる
    public void ActivePassiveSkill()
    {
        foreach (var skill in acquiredSkills)
        {
            skill.ApplyEffect(gameObject);
        }
    }
    //ゲーム中に条件を満たすと発動する
    void Update()
    {
        // Debug.Log("スキル確認中");
        foreach (var skill in acquiredSkills)
        {
            skill.UpdateConditional(gameObject);
        }
    }
    //スキルを獲得する処理
    public void AcquireSkill(SkillBase newSkill, Action onAcquired = null)
    {
        //重複取得防止
        if (acquiredSkills.Contains(newSkill)) return;

        //特殊スキル上限チェック
        if (newSkill.SpcialSkill && IsSpecialSkillLimited())
        {

            var specialSkills = GetSpecialSkills();
            ShowDiscardDialogAndAdd(specialSkills, newSkill, onAcquired);
            return;
        }
        //スキルを限度で取得しているか確認
        if (IsSkillLimitReached())
        {
            ShowDiscardDialogAndAdd(acquiredSkills, newSkill, onAcquired);
            return;
        }
        // 通常取得
        InternalAddSkill(newSkill);
        onAcquired?.Invoke();
    }
    /// <summary>
    /// スキルをリストから削除する
    /// </summary>
    private void RemoveSkill(SkillBase skill)
    {
        //もし削除したスキルが炎を打つものなら
        if (skill.skillName == "ダークボール")
        {
            player.GetComponent<ShotFire>().hasSkill = false;
        }
        acquiredSkills.Remove(skill);
        Debug.Log($"スキル '{skill.skillName}' を破棄しました。");
    }

    /// <summary>
    /// スキルを内部リストに追加
    /// </summary>
    private void InternalAddSkill(SkillBase newSkill)
    {
        acquiredSkills.Add(newSkill);
        Debug.Log($"スキル '{newSkill.skillName}' を取得しました。");
        foreach (var skill in acquiredSkills)
        {
            Debug.Log($"現在のスキル: {skill.skillName}");
        }
    }

    /// <summary>
    /// 特殊スキルが上限に達しているか
    /// </summary>
    private bool IsSpecialSkillLimited()
    {
        int specialCount = acquiredSkills.Count(s => s.SpcialSkill);
        return specialCount >= MaxSpecialSkills;
    }
    /// <summary>
    /// 特殊スキルのみ抽出
    /// </summary>
    private List<SkillBase> GetSpecialSkills()
    {
        return acquiredSkills.Where(s => s.SpcialSkill).ToList();
    }

    /// <summary>
    /// 総スキル数の上限に達しているか
    /// </summary>
    private bool IsSkillLimitReached() => acquiredSkills.Count >= MaxTotalSkills;

    /// <summary>
    /// 破棄ダイアログを出し、選択後に新スキルを追加する共通処理
    /// </summary>
    private void ShowDiscardDialogAndAdd(
        List<SkillBase> candidates,
        SkillBase newSkill,
        Action onAcquired)
    {
        SkillSelectUI.Instance.ShowDiscardDialog(
            candidates,
            discardedSkill =>
            {
                RemoveSkill(discardedSkill);
                InternalAddSkill(newSkill);
                onAcquired?.Invoke();
            });
    }
}
