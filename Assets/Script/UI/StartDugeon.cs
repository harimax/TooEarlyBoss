using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector;
using Unity.VisualScripting;
using Cinemachine;
using UnityEngine.UIElements;
namespace Invector.vCharacterController
{
    public class StartDugeon : MonoBehaviour
    {
        [SerializeField] private GameObject MainCamera;
        [SerializeField] private GameObject trainingUI;
        [SerializeField] private GameObject gameUI;
        [SerializeField] private GameObject brainCamera;
        private static StartDugeon _instance;
        private CinemachineVirtualCamera mainCameraVirtual;
        private CinemachineBrain brain;
        private vThirdPersonController vPersonController;
        private bool IsGameUI;
        private bool IstrainingUI;
        private GameObject player;
        private GameObject gameManager;
        private TrianingButton trianingButton;
        private float tempPlayerPower = 1f;
        private float tempPlayerHealth = 1f;
        private float tempPlayerStamina = 1f;
        private float tempPlayerSpecial = 1f;

        public bool CanMove { get; private set; } = false;  // 読み取り専用にしておくと安全 


        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject); // シングルトンパターンに従い、重複したインスタンスを破棄
            }
            else
            {
                _instance = this;
            }
            mainCameraVirtual = MainCamera.GetComponent<CinemachineVirtualCamera>();
            brain = brainCamera.GetComponent<CinemachineBrain>();
            player = GameObject.FindWithTag("Player"); // プレイヤーにTagがあると便利！
            // Debug.Log(player);
            vPersonController = player.GetComponent<vThirdPersonController>();
            gameManager = GameObject.Find("GameManager"); // オブジェクト名に合わせて
            trianingButton = gameManager.GetComponent<TrianingButton>();


        }
        //戦闘モードに移動
        public void StartDungeonMode()
        {
            // Debug.Log("戦闘移行");
            mainCameraVirtual.Priority = 20;
            tempPlayerStamina=trianingButton.PlayerStamina;
            Debug.Log(tempPlayerStamina);
            setFalseUI();
            IsGameUI = true;
            IstrainingUI = false;
            StartCoroutine(WaitForCameraTransition());
        }
        //始めるボタン
        public void StartMissionButton()
        {
            Debug.Log(vPersonController);
            vPersonController.AddMaxStamina(tempPlayerStamina);
            // Debug.Log("ダンジョンスタート");
            CanMove = true;
            setFalseUI();

            //修行したパラメータを加算させる
        }
        //戻るボタン
        public void ReturntTrainingButton()
        {
            // Debug.Log("修行に戻る");
            mainCameraVirtual.Priority = 5;
            setFalseUI();
            IsGameUI = false;
            IstrainingUI = true;

            StartCoroutine(WaitForCameraTransition());
        }
        // カメラ遷移完了まで待ってからUIとCanMoveを切り替える
        private IEnumerator WaitForCameraTransition()
        {
            yield return new WaitForEndOfFrame(); // カメラの優先度変更反映待ち
            yield return new WaitUntil(() => !brain.IsBlending); // 遷移完了待ち
            //戦闘シーンの際はGameUIを起動
            if (IsGameUI)
            {
                gameUI.SetActive(true);
                IsGameUI = false;
            }
            //修行シーンの際はtrainingUIを起動
            if (IstrainingUI)
            {
                trainingUI.SetActive(true);
                IstrainingUI = false;
            }
        }
        //UIを一度falseにするメソッド
        private void setFalseUI()
        {
            trainingUI.SetActive(false);
            gameUI.SetActive(false);
        }
        //StartDugeonクラスを受け取る
        public static StartDugeon GetInstance()
        {
            return _instance;
        }
    }

}
