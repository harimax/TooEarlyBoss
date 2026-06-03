using UnityEngine;
using System.Collections.Generic;

public class TrainingEvent : MonoBehaviour
{
    // =========================
    // ■ イベント各自の確率
    // =========================
    // イベントが起きた場合の内訳（重み）
    private const float P_KAKUHEN = 0.10f;          // 確変 10%
    private const float P_FAILUP = 0.15f;           // 失敗率 15%
    private const float P_RANDOM = 0.45f;           // ランダム上昇 45%
    private const float P_ALL = 0.05f;              // 全能力アップ 5%
    private const float P_EVENT_KAKUHEN = 0.05f;    // イベント確変 5%
    private const float P_TRAIN_BOOST = 0.20f;      // 育成二倍(1.5倍) 20%
    // =========================
    // 調整パラメータ（Inspector）
    // =========================

    [Header("Event Trigger")]
    [Range(0f, 1f)]
    [SerializeField] private float eventTriggerRate = 0.25f; // まずイベントが起きる確率

    [Header("Failure")]
    [Range(0f, 1f)]
    [SerializeField] private float baseFailRate = 0.05f;     // 通常失敗率（上昇なし）

    [Header("Kakuhen (倍率)")]
    [Range(1f, 5f)]
    [SerializeField] private float kakuhenMultiplier = 2f;
    [SerializeField] private int kakuhenTurns = 3;
    [SerializeField] private int eventKakuhenTurns = 3;      // イベント確変の持続回数

    [Header("FailUp (失敗率上昇)")]
    [Range(0f, 1f)]
    [SerializeField] private float failUpBonus = 0.15f;
    [SerializeField] private int failUpTurns = 3;


    [Header("Bonus")]
    [SerializeField] private int randomBonus = 10;
    [SerializeField] private int allBonus = 15;
    [SerializeField] private float trainingBoostMul = 1.5f;  // 育成二倍(仕様は1.5倍)

    //イベント識別型
    public enum TrainingEventType
    {
        None,           // イベントなし（通常）
        Fail,           // 失敗（上昇なし）
        KakuhenStart,   // 確変開始(育成値が2倍)
        FailUpStart,    // 失敗率UP開始
        RandomStatUp,   // ランダム能力UP
        AllStatUp,      // 全能力UP
        EventKakuhenStart,  // イベント確変：イベント発生率2倍状態開始
        TrainingBoostStart  // 育成二倍：次の育成時の上昇が1.5倍（ワンショット）開始
    }

    // =========================
    // 持続状態（Stateの責務）
    // =========================
    private int kakuhenRemain = 0;// 上昇倍率の残り回数
    private int failUpRemain = 0;// 失敗率UPの残り回数
    private int eventKakuhenRemain = 0;   // イベント発生率2倍の残り回数
    private bool nextTrainingBoost = false; // 次回育成1.5倍（ワンショット）

    /// <summary>
    /// ボタン押下時に呼ばれる唯一の公開メソッド
    /// </summary>
    public (PlayerGrowParameters addParams, TrainingEventType eventType) Apply(PlayerGrowParameters baseAdd)
    {
        // 前回のイベントで予約された育成倍率は、この育成結果にだけ適用する。
        bool applyBoostThisTime = nextTrainingBoost;

        if (applyBoostThisTime)
        {
            nextTrainingBoost = false; // 今回で消費する（成功した場合に適用）
        }

        //失敗判定
        float failRate = baseFailRate;
        if (failUpRemain > 0)
        {
            failRate += failUpBonus;
        }
        failRate = Mathf.Clamp01(failRate);
        if (Random.value < failRate)
        {
            //失敗
            ConsumeTurns();
            return (new PlayerGrowParameters(0, 0, 0, 0),
                    TrainingEventType.Fail);
        }

        //イベント処理---------------------------------------------------
        float currentTriggerRate = eventTriggerRate;
        if (eventKakuhenRemain > 0)
        {
            // イベント確変中はイベント発生率だけを上げ、元の設定値は変更しない。
            currentTriggerRate *= 2f;
        }
        currentTriggerRate = Mathf.Clamp01(currentTriggerRate);
        //イベント発生抽選
        bool eventOccurs = Random.value < currentTriggerRate;
        if (!eventOccurs)
        {
            // イベントなし（確変が残っていれば倍率だけ適用）
            PlayerGrowParameters result = BounsApply(baseAdd, applyBoostThisTime);
            return (result, TrainingEventType.None);
        }

        // どのイベントを発生させるか
        TrainingEventType picked = RollWeightedEvent();

        PlayerGrowParameters finalAdd;

        switch (picked)
        {
            case TrainingEventType.KakuhenStart: // 確変開始
                kakuhenRemain = kakuhenTurns;
                finalAdd = BounsApply(baseAdd, applyBoostThisTime);
                return (finalAdd, TrainingEventType.KakuhenStart);

            case TrainingEventType.FailUpStart: // 失敗率UP開始
                failUpRemain = failUpTurns;
                finalAdd = BounsApply(baseAdd, applyBoostThisTime);
                return (finalAdd, TrainingEventType.FailUpStart);

            case TrainingEventType.RandomStatUp: // ランダム1能力UP
                finalAdd = baseAdd.AddParameters(RandomBonus());
                finalAdd = BounsApply(finalAdd, applyBoostThisTime);
                return (finalAdd, TrainingEventType.RandomStatUp);

            case TrainingEventType.AllStatUp: // 全能力UP
                finalAdd = baseAdd.AddParameters(
                    new PlayerGrowParameters(allBonus, allBonus, allBonus, allBonus));
                finalAdd = BounsApply(finalAdd, applyBoostThisTime);
                return (finalAdd, TrainingEventType.AllStatUp);

            case TrainingEventType.EventKakuhenStart:// イベント確変開始
                eventKakuhenRemain = eventKakuhenTurns;
                finalAdd = BounsApply(baseAdd, applyBoostThisTime);
                return (finalAdd, TrainingEventType.EventKakuhenStart);

            case TrainingEventType.TrainingBoostStart:  // 育成二倍開始
                // “次の育成時に1.5倍” を付与（今回には適用しない）
                nextTrainingBoost = true;
                finalAdd = BounsApply(baseAdd, applyBoostThisTime);
                return (finalAdd, TrainingEventType.TrainingBoostStart);
        }

        ConsumeTurns();
        return (baseAdd, TrainingEventType.None);
    }
    // =====================
    // 抽選（P_*** 合計=1.0 前提）
    // =====================
    private TrainingEventType RollWeightedEvent()
    {
        // acc に確率を積み上げ、乱数が最初に下回ったイベントを採用する。
        float r = Random.value; // 0.0〜1.0未満
        float acc = 0f;

        acc += P_KAKUHEN;
        if (r < acc) return TrainingEventType.KakuhenStart;

        acc += P_FAILUP;
        if (r < acc) return TrainingEventType.FailUpStart;

        acc += P_RANDOM;
        if (r < acc) return TrainingEventType.RandomStatUp;

        acc += P_ALL;
        if (r < acc) return TrainingEventType.AllStatUp;

        acc += P_EVENT_KAKUHEN;
        if (r < acc) return TrainingEventType.EventKakuhenStart;

        return TrainingEventType.TrainingBoostStart; // 残り
    }

    // =====================
    // 補助メソッド
    // =====================

    /// <summary>
    /// 確変反映とターン消費の共通化メソッド
    /// </summary>
    private PlayerGrowParameters BounsApply(PlayerGrowParameters add, bool applyBoostThisTime)
    {
        // 確変倍率と次回育成ブーストをまとめて反映し、最後に継続ターンを1つ消費する。
        var kakuhenAdd = ApplyKakuhen(add);
        if (applyBoostThisTime)
        {
            kakuhenAdd = Multiply(kakuhenAdd, trainingBoostMul);
        }
        ConsumeTurns();
        return kakuhenAdd;
    }

    /// <summary>
    /// 確変が残っていれば倍率をかける
    /// </summary>
    private PlayerGrowParameters ApplyKakuhen(PlayerGrowParameters add)
    {
        if (kakuhenRemain <= 0) return add;

        return new PlayerGrowParameters(
            Mathf.RoundToInt(add.PlayerPower * kakuhenMultiplier),
            Mathf.RoundToInt(add.PlayerHealth * kakuhenMultiplier),
            Mathf.RoundToInt(add.PlayerStamina * kakuhenMultiplier),
            Mathf.RoundToInt(add.PlayerSpecial * kakuhenMultiplier)
        );
    }
    /// <summary>
    /// 育成1.5倍（実仕様：1.5倍）など、任意倍率を増分に適用する
    /// </summary>
    private PlayerGrowParameters Multiply(PlayerGrowParameters add, float mul)
    {
        return new PlayerGrowParameters(
        Mathf.RoundToInt(add.PlayerHealth * mul),
        Mathf.RoundToInt(add.PlayerPower * mul),
        Mathf.RoundToInt(add.PlayerStamina * mul),
        Mathf.RoundToInt(add.PlayerSpecial * mul)
        );
    }

    /// <summary>
    /// ランダム能力ボーナス
    /// </summary>
    private PlayerGrowParameters RandomBonus()
    {
        int r = Random.Range(0, 4);

        return r switch
        {
            0 => new PlayerGrowParameters(randomBonus, 0, 0, 0),
            1 => new PlayerGrowParameters(0, randomBonus, 0, 0),
            2 => new PlayerGrowParameters(0, 0, randomBonus, 0),
            _ => new PlayerGrowParameters(0, 0, 0, randomBonus),
        };
    }

    /// <summary>
    /// 育成1回分、持続ターンを消費する
    /// </summary>
    private void ConsumeTurns()
    {
        if (kakuhenRemain > 0) kakuhenRemain--;
        if (failUpRemain > 0) failUpRemain--;
        if (eventKakuhenRemain > 0) eventKakuhenRemain--;
    }
}
