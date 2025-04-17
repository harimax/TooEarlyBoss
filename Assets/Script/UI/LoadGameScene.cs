using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening; 
using UnityEngine.EventSystems; 
using System.Threading.Tasks;

public class LoadGameScene : MonoBehaviour
{
    private string mainSceneTitle="DunegonScene";
    // [SerializeField] private GameObject player;
    [SerializeField] Fade fade;
    // [SerializeField] private GameObject explorosion;
    private Rigidbody rb;
    void Start() 
    {
        // rb=player.GetComponent<Rigidbody>();

    }

    public async void OnClickStartButton()
    {
        // ボタンのスケールを縮小するアニメーションを実行
        await this.transform.DOScale(0.9f, 0.5f).SetEase(Ease.OutCubic).AsyncWaitForCompletion();
        // Debug.Log("ボタンが押されました");
        await this.transform.DOScale(1.0f, 0.24f).SetEase(Ease.OutCubic).SetDelay(0.05f).AsyncWaitForCompletion();


        Debug.Log("ゲームシーンに遷移します");
        // Destroy(explorosionPrehab,2.0f);
        //fade.FadeIn(時間, ()=>完了した時にやりたいこと)
        fade.FadeIn(1f,()=>SceneManager.LoadScene(mainSceneTitle));
    }
}
