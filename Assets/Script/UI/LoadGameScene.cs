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
    private const string BossSceneTitle = "SmileBossScene";
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

        Debug.Log("ゲームシーンに遷移します");

        // シーン読み込み後イベントを一度だけ登録
        SceneManager.sceneLoaded += OnSceneLoaded;

        // フェード後にシーン遷移
        fade.FadeIn(1f, () => SceneManager.LoadScene(BossSceneTitle));
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
        meleeManager.defaultDamage = new vDamage(Mathf.RoundToInt(trainingButton.PlayerPower) + 10);
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
    /// <summary>
    /// シーン読み込み完了時に実行される処理
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // プレイヤーとスタート地点を探す
        GameObject player = GameObject.FindWithTag("Player");
        GameObject startPoint = GameObject.FindWithTag("StartPoint");

        if (player != null && startPoint != null)
        {
            player.transform.position = startPoint.transform.position;
        }

        // イベント登録解除（多重呼び出し防止）
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
