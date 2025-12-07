using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class SkillSelectUI : MonoBehaviour
{
    public static SkillSelectUI Instance { get; private set; }
    [Header("UI References")]
    [SerializeField] private GameObject skillCardPrefab;
    [SerializeField] private Transform cardParent;  // 配置先のUI（GridLayoutGroup推奨）

    [Header("Game References")]
    [SerializeField] private List<SkillBase> allSkills;  // 全スキル（ScriptableObject）
    [SerializeField] private SkillManager skillManager;  // プレイヤーのSkillManager参照
    [SerializeField] private MissionManager missionManager; // ←インスペクタで設定
    [SerializeField] private GameObject discardDialogPrefab;
    bool skillChosen = false;
    private void Awake()
    {
        if(skillManager == null)
        {
            var player = GameObject.FindWithTag("Player");
            skillManager =player.GetComponent<SkillManager>();
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

    private void LoadAllSkills()
    {
        // Resources/SkillsフォルダからSkillBaseをすべてロード
        SkillBase[] loadedSkills = Resources.LoadAll<SkillBase>("Skill");
        allSkills = new List<SkillBase>(loadedSkills);

        // Debug.Log($"スキルを {allSkills.Count} 個ロードしました。");
    }

    //ミッションクリア後にスキルカードを表示するメソッド
    public void ShowRandomSkillChoices()
    {

        skillChosen = false; // ←★ ここで毎回リセット！

        //既に獲得しているスキルは表示対象外にする
        var unacquiredSkills = allSkills
        .Where(skill => !skillManager.acquiredSkills.Contains(skill)).ToList();

        // 選択数の制限（2つ）
        int choiceCount = Mathf.Min(4, unacquiredSkills.Count); // ← 修正ポイント

        //まだ獲得していないスキルからランダムに4つ選ぶ
        var selectedSkills = unacquiredSkills.OrderBy(x => UnityEngine.Random.value).Take(choiceCount).ToList();

        // 表示をクリア
        foreach (Transform child in cardParent)
        {
            Destroy(child.gameObject);
        }

        //カードを表示させる
        foreach (var skill in selectedSkills)
        {
            var capturedSkill = skill; // ループの中でコピーを作る
            GameObject card = Instantiate(skillCardPrefab, cardParent);
            SkillSet skillSet = card.GetComponent<SkillSet>();
            card.SetActive(true);
            //setUpメソッド(ボタン押下時)に初期値に戻る処理とプレイヤーにスキルをセットする処理、スキルカードUIをクリアする処理を追加させる
            skillSet.Setup(capturedSkill, () =>
            {
                Debug.Log("スキル選択ボタンにリスナー登録");
                if (skillChosen) return;
                skillChosen = true;

                // AcquireSkill の onAcquired コールバックに後処理を渡す
                skillManager.AcquireSkill(
                    capturedSkill,
                    // このラムダは「真に取得したとき」にだけ呼ばれる
                    () =>
                    {
                        Debug.Log("【DEBUG】 onAcquired コールバック発火");
                        missionManager.ReturnPlayerToInitialPosition();
                        ClearCardUI();
                    }
                );
            });
        }
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
}
