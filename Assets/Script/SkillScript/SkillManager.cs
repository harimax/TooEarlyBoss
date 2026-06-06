using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Unity 側のスキル入口を担当。
/// 所持管理、取得判定、効果実行は専用クラスへ委譲。
/// </summary>
public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance { get; private set; }

    public const int MaxTotalSkills = 7;
    public const int MaxSpecialSkills = 2;

    /// <summary>
    /// 取得済みスキル一覧。既存 UI 互換のため public を維持。
    /// </summary>
    public List<SkillBase> acquiredSkills = new List<SkillBase>();

    [SerializeField] private Transform player;

    private SkillInventory inventory;
    private SkillAcquisitionService acquisitionService;
    private SkillEffectRunner effectRunner;

    /// <summary>
    /// 外部参照向けの読み取り専用スキル一覧。
    /// </summary>
    public IReadOnlyList<SkillBase> AcquiredSkills => inventory?.Skills ?? acquiredSkills;

    /// <summary>
    /// Singleton とスキル用サービスを初期化。
    /// </summary>
    private void Awake()
    {
        // SkillManager が複数生成された場合、先に存在する方を正とする
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning(
                $"Duplicate SkillManager found. Keep existing ({Instance.name}) and destroy new ({name}).");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeServices();
    }

    /// <summary>
    /// 戦闘開始時に常時発動スキルを適用。
    /// </summary>
    public void ActivePassiveSkill()
    {
        EnsureServices();
        effectRunner.ApplyPassiveSkills(gameObject, inventory.Skills);
    }

    /// <summary>
    /// 条件付きスキルの発動判定を毎フレーム更新。
    /// </summary>
    private void Update()
    {
        EnsureServices();
        effectRunner.UpdateConditionalSkills(gameObject, inventory.Skills);
    }

    /// <summary>
    /// 新しいスキルの取得処理を実行。
    /// </summary>
    /// <param name="newSkill">取得対象のスキル。</param>
    /// <param name="onAcquired">取得完了後のコールバック。</param>
    public void AcquireSkill(SkillBase newSkill, Action onAcquired = null)
    {
        EnsureServices();

        SkillAcquisitionResult result = acquisitionService.Resolve(newSkill);

        switch (result.Type)
        {
            case SkillAcquisitionResultType.Invalid:
            case SkillAcquisitionResultType.AlreadyOwned:
                return;

            case SkillAcquisitionResultType.AddDirectly:
                AddSkill(newSkill);
                onAcquired?.Invoke();
                return;

            case SkillAcquisitionResultType.RequireDiscard:
                ShowDiscardDialogAndAdd(result.DiscardCandidates, newSkill, onAcquired);
                return;
        }
    }

    /// <summary>
    /// 指定スキルを取得済みか確認。
    /// </summary>
    /// <param name="skill">確認対象のスキル。</param>
    /// <returns>取得済みなら true。</returns>
    public bool HasSkill(SkillBase skill)
    {
        EnsureServices();
        return inventory.Contains(skill);
    }

    /// <summary>
    /// スキル所持管理用サービスを初期化。
    /// </summary>
    private void InitializeServices()
    {
        inventory = new SkillInventory(acquiredSkills, MaxTotalSkills, MaxSpecialSkills);
        acquisitionService = new SkillAcquisitionService(inventory);
        effectRunner = new SkillEffectRunner();
    }

    /// <summary>
    /// Unity のライフサイクル外から呼ばれた場合でもサービスを利用可能にする保険。
    /// </summary>
    private void EnsureServices()
    {
        if (inventory == null || acquisitionService == null || effectRunner == null)
        {
            InitializeServices();
        }
    }

    /// <summary>
    /// スキルを一覧から削除。
    /// </summary>
    /// <param name="skill">削除対象のスキル。</param>
    private void RemoveSkill(SkillBase skill)
    {
        // 特定スキルの副作用解除は、今後の仕様変更で専用解除処理へ移す候補
        if (skill != null && skill.skillName == "繝繝ｼ繧ｯ繝懊・繝ｫ" && player != null)
        {
            ShotFire shotFire = player.GetComponent<ShotFire>();
            if (shotFire != null)
            {
                shotFire.hasSkill = false;
            }
        }

        if (inventory.Remove(skill))
        {
            Debug.Log($"Removed skill '{skill.skillName}'.");
        }
    }

    /// <summary>
    /// スキルを一覧へ追加。
    /// </summary>
    /// <param name="newSkill">追加対象のスキル。</param>
    private void AddSkill(SkillBase newSkill)
    {
        if (!inventory.Add(newSkill))
        {
            return;
        }

        Debug.Log($"Acquired skill '{newSkill.skillName}'.");

        foreach (var skill in inventory.Skills)
        {
            if (skill == null)
            {
                continue;
            }

            Debug.Log($"Current skill: {skill.skillName}");
        }
    }

    /// <summary>
    /// 破棄ダイアログを表示し、選択後に新スキルを追加。
    /// </summary>
    /// <param name="candidates">破棄候補スキル。</param>
    /// <param name="newSkill">取得予定のスキル。</param>
    /// <param name="onAcquired">取得完了後のコールバック。</param>
    private void ShowDiscardDialogAndAdd(
        IReadOnlyList<SkillBase> candidates,
        SkillBase newSkill,
        Action onAcquired)
    {
        // UI が未生成の場合は取得を保留せず中断
        if (SkillSelectUI.Instance == null)
        {
            return;
        }

        SkillSelectUI.Instance.ShowDiscardDialog(
            new List<SkillBase>(candidates),
            discardedSkill =>
            {
                RemoveSkill(discardedSkill);
                AddSkill(newSkill);
                onAcquired?.Invoke();
            });
    }
}
