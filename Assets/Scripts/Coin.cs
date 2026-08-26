using UnityEngine;

// ============================================================
// Coin
// プレイヤーが触れるとスコアが増えて消えるコインです。
//
// 【セットアップ手順】
//  1. コインのスプライトをシーンにドラッグする
//  2. Circle Collider 2D を付け、Is Trigger に必ずチェック
//  3. このスクリプトを付ける
//  4. Score に点数を入れる（銅=1, 銀=5, 金=10 など）
//  5. 完成したら Project フォルダにドラッグしてプレハブ化する
//     → 以降はプレハブをドラッグするだけで量産できます
// ============================================================

public class Coin : MonoBehaviour
{
    [Header("スコア")]
    [Tooltip("拾ったときに増える点数")]
    [SerializeField] private int score = 1;

    private bool isTaken; // 二重に取得しないようにする

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isTaken) return;

        // 相手が PlayerController を持っていればプレイヤーだと判断する
        if (other.GetComponent<PlayerController>() == null) return;

        isTaken = true;

        // スコアを増やす
        ScoreManager.AddScore(score);

        // 自分を消す
        Destroy(gameObject);
    }
}
