using UnityEngine;

// ============================================================
// DamageZone
// トゲや落下death（穴）など、触れるとダメージを受ける場所です。
//
// 【セットアップ手順】
//  ■ 落下deathとして使う場合
//    1. 空オブジェクト「DeathZone」を作る
//    2. Box Collider 2D を付け、Is Trigger にチェック
//    3. ステージの一番下に、横に長く伸ばして置く
//    4. Instant Death にチェックを入れる
//
//  ■ トゲとして使う場合
//    Instant Death のチェックを外し、Damage を 1 にする
// ============================================================

public class DamageZone : MonoBehaviour
{
    [Header("ダメージ設定")]
    [Tooltip("チェックすると、HPに関係なく一撃で死亡する（穴に落ちたとき用）")]
    [SerializeField] private bool instantDeath = true;

    [Tooltip("Instant Death がオフのときに与えるダメージ量")]
    [SerializeField] private int damage = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth == null) return;

        if (instantDeath)
        {
            playerHealth.InstantDeath();
        }
        else
        {
            playerHealth.TakeDamage(damage);
        }
    }
}
