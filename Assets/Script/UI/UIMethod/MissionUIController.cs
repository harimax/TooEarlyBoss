using UnityEngine;
using TMPro;
using UnityEngine.UI;
//// <summary>
/// ミッションUIの制御を行うクラス  
/// </summary>
public class MissionUIController : MonoBehaviour
{
    [Header("Result Text")]
    [SerializeField] private TextMeshProUGUI resultText;

    /// <summary>
    /// ミッション結果テキストを消す処理    
    /// </summary>
    public void ResetText()
    {
        if (resultText != null)
        {
            resultText.text = "";
        }
    }
    /// <summary>
    /// クリア演出用のテキストを表示
    /// </summary>
    public void ShowClear()
    {
        if (resultText != null)
        {
            resultText.text = "クリア";
        }
    }
    /// <summary>
    /// 失敗（死亡）時のテキストを表示
    /// </summary>
    public void ShowFailed()
    {
        if (resultText != null)
        {
            resultText.text = "死んだぜ";
        }
    }
    
}
