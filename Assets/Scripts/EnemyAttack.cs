using UnityEngine;

// ============================================================
// EnemyAttack
// 敵がプレイヤーに触れたときにダメージを与える処理です。
//
// 【セットアップ】
//  Enemy オブジェクト（体のコライダーがある方）に付けます。
//  StompZone の子オブジェクトには付けません。
// ============================================================

public class EnemyAttack : MonoBehaviour
{
    [Header("攻撃")]
    [Tooltip("プレイヤーに与えるダメージ量")]
    [SerializeField] private int attackPower = 1;

    private EnemyHealth enemyHealth;

    void Start()
    {
        enemyHealth = GetComponent<EnemyHealth>();
    }

    // ------------------------------------------------------------
    // 触れている「間ずっと」呼ばれる
    // Enter（触れた瞬間）ではなく Stay を使う理由：
    //   無敵時間が終わった後、まだ敵に密着していれば
    //   もう一度ダメージを受けてほしいためです
    // ------------------------------------------------------------
    private void OnCollisionStay2D(Collision2D collision)
    {
        DealDamage(collision.gameObject);
    }

    private void DealDamage(GameObject target)
    {
        // 倒された敵は攻撃してこない
        if (enemyHealth != null && enemyHealth.IsDead) return;

        // 相手が PlayerHealth を持っていればプレイヤーだと判断する
        PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackPower);
        }
    }
}
