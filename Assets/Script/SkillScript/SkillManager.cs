using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance { get; private set; }

    public const int MaxTotalSkills = 7;
    public const int MaxSpecialSkills = 2;
    //得たスキルを格納する
    public List<SkillBase> acquiredSkills = new List<SkillBase>();
    [SerializeField] private Transform player;
    private void Awake()
    {
        foreach (var skill in acquiredSkills)
        {
            Debug.Log($"現在のスキル: {skill.skillName}");
        }
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
            // Debug.Log(skill);
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
    public void AcquireSkill(SkillBase newSkill,Action onAcquired = null)
    {
        Debug.Log($"[AcquireSkill] 要求: {newSkill.skillName}, " +
              $"Count={acquiredSkills.Count}, " +
              $"Contains={acquiredSkills.Contains(newSkill)}, " +
              $"Special={newSkill.SpcialSkill}");
        //重複取得防止
        if (acquiredSkills.Contains(newSkill)) return;

        //特殊スキル
        if (newSkill.SpcialSkill)
        {
            int specialCount = acquiredSkills.Count(s => s.SpcialSkill);
            //特殊スキルが限界数を超えていれば削除ダイアログを表示する
            if (specialCount >= MaxSpecialSkills)
            {
                // ダイアログを開いて、破棄候補の特殊スキルを選ばせる
                SkillSelectUI.Instance.ShowDiscardDialog(
                    acquiredSkills.Where(s => s.SpcialSkill).ToList(),
                    discardedSkill =>
                    {
                        RemoveSkill(discardedSkill);
                        // 選択完了後、新スキルを追加
                        InternalAddSkill(newSkill);
                        onAcquired?.Invoke();    // ← コールバック呼び出し
                    });
                return;
            }
        }
        //スキルを限度で取得しているか確認
        if (acquiredSkills.Count >= MaxTotalSkills)
        {
            // 全スキルを候補に破棄ダイアログを表示
            SkillSelectUI.Instance.ShowDiscardDialog(
                acquiredSkills,
                discardedSkill =>
                {
                    RemoveSkill(discardedSkill);
                    InternalAddSkill(newSkill);
                    onAcquired?.Invoke();    // ← コールバック呼び出し
                });
            return;
        }
        //通常時のスキル取得
        if (!acquiredSkills.Contains(newSkill))
        {
            InternalAddSkill(newSkill);
            onAcquired?.Invoke();    // ← コールバック呼び出し

        }
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
}
