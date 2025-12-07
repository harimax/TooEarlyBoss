using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using Invector;
using UnityEngine.EventSystems;
using System.Threading.Tasks;
using Invector.vMelee;
using Invector.vCharacterController;

public class LoadGameScene : MonoBehaviour
{
    [SerializeField] Fade fade;
    [SerializeField] private RectTransform buttonTransform;
    [SerializeField] private string[] BossSceneTitle;
    private TrianingButton trainingButton;
    private vThirdPersonController vPersonController;
    private vMeleeManager meleeManager;
    private SkillManager skillManager;


    public async void OnClickStartButton()
    {
        await AnimateButton();

        // プレイヤー情報取得 & ステータス適用
        if (!TryGetPlayerComponents()) return;
        ApplyPlayerTrainingStats();
        trainingButton = GetComponent<TrianingButton>();
        trainingButton.SaveParameter();

        Debug.Log("ゲームシーンに遷移します");
        if (fade != null)
        {
            // フェード後にシーン遷移
            fade.FadeIn(1f, () => SceneManager.LoadScene(BossSceneTitle[ProcessManager.Instance.CurrentCycle - 1]));
        }
        else
        {
            SceneManager.LoadScene(BossSceneTitle[ProcessManager.Instance.CurrentCycle - 1]);
        }

    }
    /// <summary>
    /// ボタンのアニメーション（DOTween）
    /// </summary>
    private async Task AnimateButton()
    {
        await buttonTransform.DOScale(0.9f, 0.5f).SetEase(Ease.OutCubic).AsyncWaitForCompletion();
        await buttonTransform.DOScale(1.0f, 0.24f).SetEase(Ease.OutCubic).SetDelay(0.05f).AsyncWaitForCompletion();
    }
    /// <summary>
    /// プレイヤーのトレーニング値をステータスに反映
    /// </summary>
    private void ApplyPlayerTrainingStats()
    {
        vPersonController.ResetMaxHealth();
        vPersonController.ResetMaxStamina();
        //修了したパラメータを加算させる
        vPersonController.AddMaxStamina(trainingButton.PlayerStamina);
        vPersonController.AddMaxHealth(trainingButton.PlayerHealth);
        meleeManager.defaultDamage.damageValue = Mathf.RoundToInt(trainingButton.PlayerPower) + meleeManager.defaultDamage.damageValue;
        meleeManager.Init();
    }
    /// <summary>
    /// プレイヤーや必要なコンポーネントの取得
    /// </summary>
    private bool TryGetPlayerComponents()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("プレイヤーがシーン内に見つかりませんでした。");
            return false;
        }

        trainingButton = GetComponent<TrianingButton>();
        vPersonController = player.GetComponent<vThirdPersonController>();
        meleeManager = player.GetComponent<vMeleeManager>();
        skillManager = player.GetComponent<SkillManager>();

        if (trainingButton == null || vPersonController == null || meleeManager == null || skillManager == null)
        {
            Debug.LogWarning("必要なコンポーネントのいずれかが見つかりません。");
            return false;
        }

        return true;
    }
}
