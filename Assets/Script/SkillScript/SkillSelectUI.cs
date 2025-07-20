using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class SkillSelectUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject skillCardPrefab;
    [SerializeField] private Transform cardParent;  // 配置先のUI（GridLayoutGroup推奨）

    [Header("Game References")]
    [SerializeField] private List<SkillBase> allSkills ;  // 全スキル（ScriptableObject）
    [SerializeField] private SkillManager skillManager;  // プレイヤーのSkillManager参照
    [SerializeField] private MissionManager missionManager; // ←インスペクタで設定
    bool skillChosen = false;
    private void Awake()
    {
        LoadAllSkills();
    }

    private void LoadAllSkills()
    {
        // Resources/SkillsフォルダからSkillBaseをすべてロード
        SkillBase[] loadedSkills = Resources.LoadAll<SkillBase>("Skill");
        allSkills = new List<SkillBase>(loadedSkills);

        Debug.Log($"スキルを {allSkills.Count} 個ロードしました。");
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
        var selectedSkills = unacquiredSkills.OrderBy(x => Random.value).Take(choiceCount).ToList();

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

                skillManager.AcquireSkill(capturedSkill);
                missionManager.ReturnPlayerToInitialPosition(); // ← ここで呼び出す！
                ClearCardUI();
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
}
