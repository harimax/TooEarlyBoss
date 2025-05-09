using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector;
using Unity.VisualScripting;
using Cinemachine;
using UnityEngine.UIElements;
using Invector.vMelee;
using Invector.vCharacterController;
    public class StartMission : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject MainCameraObject;//Invectorのやつをアタッチ
        [SerializeField] private GameObject trainingUI;
        [SerializeField] private GameObject gameUI;
        [SerializeField] private GameObject brainCameraObject;//Brainのやつをアタッチ
        [SerializeField] private MissionManager missionManager;

        private static StartMission _instance;
        private CinemachineVirtualCamera mainCameraVirtual;
        private CinemachineBrain brain;
        private vThirdPersonController vPersonController;
        private bool IsGameUI;
        private bool IstrainingUI;
        private GameObject player;
        private GameObject gameManager;
        private TrianingButton trainingButton;
        private vMeleeManager _vMeleeManager; 
        private EnemyGenerator enemyGenerator;

        // プレイヤーの修行値
        private float tempPlayerPower = 1f;
        private int tempPlayerHealth = 1;
        private float tempPlayerStamina = 1f;
        private float tempPlayerSpecial = 1f;

        public bool CanMove { get; set; } = false;  // 読み取り専用にしておくと安全 
        private bool isMissionActive = false; // ミッションが進行中かどうか
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            player = GameObject.FindWithTag("Player"); // プレイヤーにTagがあると便利！
            gameManager = GameObject.Find("GameManager"); // オブジェクト名に合わせて
            
            mainCameraVirtual = MainCameraObject.GetComponent<CinemachineVirtualCamera>();
            brain = brainCameraObject.GetComponent<CinemachineBrain>();
            
            enemyGenerator = GameObject.Find("EnemyGenerator").GetComponent<EnemyGenerator>();
            vPersonController = player.GetComponent<vThirdPersonController>();
            _vMeleeManager = player.GetComponent<vMeleeManager>();
            trainingButton = gameManager.GetComponent<TrianingButton>();
        }
        /// <summary>
        /// 戦闘準備モードへ移行する
        /// </summary>
        public void StartDungeonMode()
        {
            // Debug.Log("戦闘移行");
            mainCameraVirtual.Priority = 20;

            tempPlayerStamina = trainingButton.PlayerStamina;
            tempPlayerHealth = trainingButton.PlayerHealth;
            tempPlayerPower = trainingButton.PlayerPower;

            setAllFalseUI();
            IsGameUI = true;
            IstrainingUI = false;
            StartCoroutine(WaitForCameraTransition());
        }
        /// <summary>
        /// ミッション開始ボタン
        /// </summary>
        public void StartMissionButton()
        {
            //修行したパラメータを加算させる
            vPersonController.AddMaxStamina(tempPlayerStamina);
            vPersonController.AddMaxHealth(tempPlayerHealth);
            _vMeleeManager.defaultDamage = new vDamage(Mathf.RoundToInt(tempPlayerPower) + 10);
            // Debug.Log("ダンジョンスタート");
            CanMove = true;
            setAllFalseUI();
            enemyGenerator.GenerateEnemy();
            //ミッション開始メソッドが呼ばれる
            missionManager.StartMission();
        }
        
        /// <summary>
        /// 修行モードに戻るボタン
        /// </summary>
        public void ReturntTrainingButton()
        {
            // Debug.Log("修行に戻る");
            mainCameraVirtual.Priority = 5;
            setAllFalseUI();
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

        /// <summary>
        /// UIをすべて非表示にする
        /// </summary>
        private void setAllFalseUI()
        {
            trainingUI.SetActive(false);
            gameUI.SetActive(false);
        }
        //StartDugeonクラスを受け取る
        public static StartMission GetInstance()
        {
            return _instance;
        }
    }
