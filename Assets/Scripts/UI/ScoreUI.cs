using TMPro;
using UnityEngine;

// ============================================================
// ScoreUI
// 画面にスコアを表示します。
//
// 【セットアップ手順】
//  1. Canvas の子に Text - TextMeshPro を作る
//  2. 画面の右上あたりに配置する
//  3. Canvas（または作った Text）にこのスクリプトを付ける
//  4. Score Text に、作った Text をドラッグする
//
//  ※ クリア画面でも同じ手順で置けば、最終スコアを表示できます
// ============================================================

public class ScoreUI : MonoBehaviour
{
    [Header("参照")]
    [Tooltip("スコアを表示する TextMeshPro をドラッグ")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("表示")]
    [Tooltip("数字の前に付ける文字")]
    [SerializeField] private string prefix = "SCORE ";

    [Tooltip("数字を何桁で表示するか（3なら 007 のようになる）")]
    [SerializeField] private int digits = 3;

    void Start()
    {
        if (scoreText == null)
        {
            Debug.LogError("Score Text が設定されていません", this);
        }
    }

    void Update()
    {
        if (scoreText == null) return;

        // 例）digits が 3 なら 7 → "007"
        scoreText.text = prefix + ScoreManager.CurrentScore.ToString("D" + digits);
    }
}
