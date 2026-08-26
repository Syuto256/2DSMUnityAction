using System.Collections;
using UnityEngine;

// ============================================================
// PlayerHealth
// プレイヤーのHP・被弾・無敵時間・死亡を担当します。
//
// 【セットアップ】
//  Player オブジェクトに付けるだけ。
//  PlayerController と同じオブジェクトに付けてください。
// ============================================================

[RequireComponent(typeof(PlayerController))]
public class PlayerHealth : MonoBehaviour
{
    [Header("体力")]
    [Tooltip("最大HP。今回は3")]
    [SerializeField] private int maxHp = 3;

    [Header("無敵時間")]
    [Tooltip("ダメージを受けた後、何秒間無敵になるか")]
    [SerializeField] private float invincibleTime = 1.0f;

    [Tooltip("無敵中の点滅の速さ（秒）")]
    [SerializeField] private float blinkInterval = 0.1f;

    [Header("死亡時")]
    [Tooltip("死んでからゲームオーバー画面に移るまでの秒数")]
    [SerializeField] private float deadWaitTime = 1.5f;

    // --- 他のスクリプトから読み取れるようにする ---
    public int CurrentHp => currentHp;
    public int MaxHp => maxHp;
    public bool IsDead => isDead;

    private int currentHp;
    private bool isInvincible;   // 無敵中か
    private bool isDead;         // 死亡済みか

    private PlayerController playerController;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    void Start()
    {
        currentHp = maxHp;
        playerController = GetComponent<PlayerController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    // ------------------------------------------------------------
    // ダメージを受ける（敵やトゲから呼ばれる）
    // ------------------------------------------------------------
    public void TakeDamage(int damage)
    {
        // 死亡済み or 無敵中なら何もしない
        if (isDead || isInvincible) return;

        currentHp -= damage;
        Debug.Log("ダメージ！ 残りHP: " + currentHp);

        if (currentHp <= 0)
        {
            currentHp = 0;
            Die();
        }
        else
        {
            // まだ生きている → 無敵時間を開始
            StartCoroutine(InvincibleRoutine());
        }
    }

    // ------------------------------------------------------------
    // 即死（穴に落ちたときなど）
    // ------------------------------------------------------------
    public void InstantDeath()
    {
        if (isDead) return;

        currentHp = 0;
        Die();
    }

    // ------------------------------------------------------------
    // 無敵時間の処理
    // コルーチン = 「時間をかけて少しずつ進む処理」を書ける仕組み
    // ------------------------------------------------------------
    private IEnumerator InvincibleRoutine()
    {
        isInvincible = true;

        float timer = 0f;
        while (timer < invincibleTime)
        {
            // 絵の表示 / 非表示を切り替えて点滅させる
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
            }

            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }

        // 点滅を終えたら必ず表示状態に戻す
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        isInvincible = false;
    }

    // ------------------------------------------------------------
    // 死亡処理
    // ------------------------------------------------------------
    private void Die()
    {
        isDead = true;
        Debug.Log("プレイヤー死亡");

        // 操作を止める
        playerController.SetControl(false);

        if (animator != null)
        {
            animator.SetTrigger("Dead");
        }

        StartCoroutine(GameOverRoutine());
    }

    private IEnumerator GameOverRoutine()
    {
        yield return new WaitForSeconds(deadWaitTime);
        SceneLoader.LoadGameOver();
    }
}
