using UnityEngine;

// ============================================================
// StompZone
// 敵の頭に置く「踏みつけ判定」です。
//
// 【セットアップ手順】
//  1. Enemy の子に空オブジェクト「StompZone」を作る
//  2. Box Collider 2D を付け、Is Trigger に必ずチェック
//  3. 敵の頭の上に、体のコライダーより少し上へはみ出す位置に置く
//       → はみ出させないと、踏む前に体に当たってダメージを受けます
//  4. このスクリプトを付ける
// ============================================================

public class StompZone : MonoBehaviour
{
    [Header("踏みつけ")]
    [Tooltip("踏んだときに敵へ与えるダメージ")]
    [SerializeField] private int stompDamage = 1;

    [Tooltip("落下中のときだけ踏んだ判定にする（下から突き上げても倒れない）")]
    [SerializeField] private bool requireFalling = true;

    private EnemyHealth enemyHealth;

    void Start()
    {
        // 親オブジェクトから EnemyHealth を探す
        enemyHealth = GetComponentInParent<EnemyHealth>();

        if (enemyHealth == null)
        {
            Debug.LogError("StompZone の親に EnemyHealth がありません", this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (enemyHealth == null || enemyHealth.IsDead) return;

        // プレイヤーかどうか確認する
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        // 落下中かどうかを確認する
        if (requireFalling)
        {
            Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();

            // ※ Unity 2022 以前は linearVelocity → velocity に置き換え
            if (playerRb != null && playerRb.linearVelocity.y > 0f)
            {
                // 上に向かっている＝下から当たった → 踏んだ扱いにしない
                return;
            }
        }

        // 敵にダメージを与えて、プレイヤーを跳ねさせる
        enemyHealth.TakeDamage(stompDamage);
        player.Bounce();
    }
}
