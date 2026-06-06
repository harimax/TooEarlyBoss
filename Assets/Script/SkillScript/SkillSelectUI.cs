using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;

public class SkillSelectUI : MonoBehaviour
{
    public static SkillSelectUI Instance { get; private set; }
    [Header("UI References")]
    [SerializeField] private GameObject skillCardPrefab;
    [SerializeField] private Transform cardParent;  // 配置先のUI（GridLayoutGroup推奨）

    [Header("Game References")]
    [SerializeField] private List<SkillBase> allSkills;  // 全スキル（ScriptableObject）
    [SerializeField] private SkillManager skillManager;  // プレイヤーのSkillManager参照
    [SerializeField] private BattleManager battleManager; // ←インスペクタで設定
    [SerializeField] private GameObject discardDialogPrefab;
    [Header("Focus")]
    [SerializeField] private Selectable fallbackSelectable; // フォーカスが外れたときの予備選択肢
    bool skillChosen = false;
    [SerializeField] private int skillChoiceCount = 3; // 選択肢のスキル枚数    
    private void Awake()
    {
        if (skillManager == null)
        {
            var player = GameObject.FindWithTag("Player");
            skillManager = player.GetComponent<SkillManager>();
            Debug.Log("SkillManagerを自動取得しました," + skillManager);
        }
        // シングルトン初期化
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        LoadAllSkills();
    }

    /// <summary>
    /// Resources/Skill から全スキルをロード
    /// </summary>
    private void LoadAllSkills()
    {
        // Resources/SkillsフォルダからSkillBaseをすべてロード
        SkillBase[] loadedSkills = Resources.LoadAll<SkillBase>("Skill");
        allSkills = new List<SkillBase>(loadedSkills);
    }
    /// <summary>
    //ミッションクリア後にスキルカードを表示するメソッド
    /// <summary>
    public void ShowRandomSkillChoices()
    {

        skillChosen = false; // ここで毎回リセット！

        //既に獲得しているスキルは表示対象外にする
        var unacquiredSkills = allSkills
        .Where(skill => !skillManager.HasSkill(skill)).ToList();

        int choiceCount = Mathf.Min(skillChoiceCount, unacquiredSkills.Count); // ← 修正ポイント

        //まだ獲得していないスキルからランダムに4つ選ぶ
        var selectedSkills = unacquiredSkills.OrderBy(x => UnityEngine.Random.value).Take(choiceCount).ToList();

        // 表示をクリア
        ClearCardUI();

        //カードを表示させる
        foreach (var skill in selectedSkills)
        {
            CreateSkillCard(skill);
        }
        // 最初のボタンにフォーカスを移動
        FocusFirstSkillButton().Forget();
    }
    /// <summary>
    /// UI上のスキルカードをすべて削除する
    /// </summary>
    void ClearCardUI()
    {
        foreach (Transform child in cardParent)
        {
            Destroy(child.gameObject);
        }
    }
    /// <summary>
    /// 破棄候補をリストで受け取り、ユーザーに選ばせるポップアップを表示
    /// </summary>
    public void ShowDiscardDialog(
        List<SkillBase> candidates,
        Action<SkillBase> onDiscarded)
    {
        var dialog = Instantiate(discardDialogPrefab, transform);
        dialog.GetComponent<SkillDiscardDialog>()
              .Initialize(candidates, onDiscarded);
    }

    /// <summary>
    /// 1枚のスキルカードを生成してセットアップ
    /// </summary>
    private void CreateSkillCard(SkillBase skill)
    {
        var capturedSkill = skill; // ループの中でコピーを作る
        var card = Instantiate(skillCardPrefab, cardParent);
        card.SetActive(true);

        var skillSet = card.GetComponent<SkillSet>();
        skillSet.Setup(capturedSkill, () =>
        {
            if (skillChosen) return;
            skillChosen = true;

            skillManager.AcquireSkill(
                capturedSkill,
                () =>
                {
                    Debug.Log("【DEBUG】 onAcquired コールバック発火");
                    battleManager.ReturnPlayerToInitialPosition();
                    ClearCardUI();
                }
            );
        }
    );
    }
    /// <summary>
    /// スキル選択UIの最初の選択肢にフォーカスを移動する
    /// </summary>
    /// <returns></returns>
    private async UniTaskVoid FocusFirstSkillButton()
    {
        Debug.Log("FocusFirstSkillButton called");
        if (EventSystem.current == null) return;
        Debug.Log("EventSystem.current is valid");

        // LayoutGroup / Instantiate 完了待ち
        await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);

        // cardParent 配下の Button を取得
        var buttons = cardParent.GetComponentsInChildren<Button>(true);

        // 最初に選択可能なボタン
        var first = buttons
            .Select(b => (Selectable)b)
            .FirstOrDefault(b =>
                b != null &&
                b.IsInteractable() &&
                b.gameObject.activeInHierarchy);

        var target = first ?? fallbackSelectable;
        if (target == null) return;

        EventSystem.current.SetSelectedGameObject(null);
        target.Select();
    }

}
