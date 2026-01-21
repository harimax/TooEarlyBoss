using System;
using UnityEngine;

/// <summary>
/// Back / Escape 入力を監視してポーズ要求を通知するクラス。
/// 入力の検知のみに責務を限定する。
/// </summary>
public class PauseInputHandler : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private string backButtonName = "Back";
    [SerializeField] private bool allowEscapeKey = true;
    
    public static PauseInputHandler Instance { get; private set; }

    /// <summary>
    /// ポーズ入力が押されたときに呼ばれるイベント。
    /// </summary>
    public event Action PauseRequested;
    void Awake()
    {
        // シングルトン処理
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (!IsPauseInputPressed()) return;

        PauseRequested?.Invoke();
    }

    /// <summary>
    /// ポーズ入力が押されたかどうか判定する。
    /// </summary>
    private bool IsPauseInputPressed()
    {
        if (!string.IsNullOrEmpty(backButtonName) && Input.GetButtonDown(backButtonName))
        {
            return true;
        }

        return allowEscapeKey && Input.GetKeyDown(KeyCode.Escape);
    }
}
