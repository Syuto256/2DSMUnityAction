using UnityEngine;

// ============================================================
// CameraFollow
// カメラがプレイヤーを追いかけます。
// ステージの外が映らないように移動範囲を制限できます。
//
// 【セットアップ手順】
//  1. Main Camera に このスクリプトを付ける
//  2. Target に Player をドラッグする
//  3. Use Limit にチェックを入れ、Scene ビューの赤い枠を見ながら
//     Min / Max を調整してステージの端に合わせる
// ============================================================

public class CameraFollow : MonoBehaviour
{
    [Header("追いかける対象")]
    [Tooltip("Player をドラッグ")]
    [SerializeField] private Transform target;

    [Header("追従の設定")]
    [Tooltip("小さいほどゆっくり付いてくる。0.1〜0.3くらいが自然")]
    [SerializeField] private float smoothTime = 0.15f;

    [Tooltip("プレイヤーからどれだけずらすか（Yを少し上げると見やすい）")]
    [SerializeField] private Vector2 offset = new Vector2(0f, 1f);

    [Header("移動範囲の制限")]
    [Tooltip("チェックを入れるとステージの外が映らなくなる")]
    [SerializeField] private bool useLimit = true;

    [SerializeField] private Vector2 minPosition = new Vector2(0f, 0f);
    [SerializeField] private Vector2 maxPosition = new Vector2(50f, 10f);

    private Vector3 currentVelocity; // SmoothDamp が内部で使う値

    // ------------------------------------------------------------
    // LateUpdate を使う理由：
    //   プレイヤーが動き終わった「後」にカメラを動かすため。
    //   Update に書くとカメラがガタガタ揺れます。
    // ------------------------------------------------------------
    void LateUpdate()
    {
        if (target == null) return;

        // 目標の位置を計算する
        float goalX = target.position.x + offset.x;
        float goalY = target.position.y + offset.y;

        // 範囲からはみ出さないように制限する
        if (useLimit)
        {
            goalX = Mathf.Clamp(goalX, minPosition.x, maxPosition.x);
            goalY = Mathf.Clamp(goalY, minPosition.y, maxPosition.y);
        }

        // カメラのZは -10 のまま維持する（動かすと何も映らなくなる）
        Vector3 goalPosition = new Vector3(goalX, goalY, transform.position.z);

        // なめらかに近づける
        transform.position = Vector3.SmoothDamp(
            transform.position,
            goalPosition,
            ref currentVelocity,
            smoothTime);
    }

    // ------------------------------------------------------------
    // Scene ビューにカメラの移動範囲を赤い枠で表示する
    // ------------------------------------------------------------
    private void OnDrawGizmosSelected()
    {
        if (!useLimit) return;

        Vector3 center = new Vector3(
            (minPosition.x + maxPosition.x) * 0.5f,
            (minPosition.y + maxPosition.y) * 0.5f,
            0f);

        Vector3 size = new Vector3(
            maxPosition.x - minPosition.x,
            maxPosition.y - minPosition.y,
            0f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center, size);
    }
}
