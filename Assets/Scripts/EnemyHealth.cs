using System.Collections;
using UnityEngine;

// ============================================================
// EnemyHealth
// 敵のHPと、倒されたときの処理を担当します。
//
// 【セットアップ】
//  Enemy オブジェクトに付けるだけ。
// ============================================================

public class EnemyHealth : MonoBehaviour
{
    [Header("体力")]
    [Tooltip("敵のHP。1なら一回踏むと倒せる")]
    [SerializeField] private int maxHp = 1;

    [Header("倒されたとき")]
    [Tooltip("倒れてから消えるまでの秒数")]
    [SerializeField] private float destroyDelay = 0.5f;

    public bool IsDead => isDead;

    private int currentHp;
    private bool isDead;

    private EnemyPatrol enemyPatrol;
    private Animator animator;

    void Start()
    {
        currentHp = maxHp;
        enemyPatrol = GetComponent<EnemyPatrol>();
        animator = GetComponent<Animator>();
    }

    // ------------------------------------------------------------
    // ダメージを受ける（StompZone から呼ばれる）
    // ------------------------------------------------------------
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHp -= damage;

        if (currentHp <= 0)
        {
            Die();
        }
    }

    // ------------------------------------------------------------
    // 倒される
    // ------------------------------------------------------------
    private void Die()
    {
        isDead = true;
        Debug.Log(gameObject.name + " を倒した");

        // 動きを止める
        if (enemyPatrol != null)
        {
            enemyPatrol.StopMove();
        }

        // すべての当たり判定を切る
        // → これをしないと倒した瞬間にプレイヤーがダメージを受けてしまう
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }

        // 物理を止めて落ちないようにする
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        if (animator != null)
        {
            animator.SetTrigger("Dead");
        }

        StartCoroutine(DestroyRoutine());
    }

    private IEnumerator DestroyRoutine()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }
}
