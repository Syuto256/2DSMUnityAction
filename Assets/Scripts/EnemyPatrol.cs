using UnityEngine;

// ============================================================
// EnemyPatrol
// 敵が決められた範囲を左右に往復する動きを担当します。
//
// 【セットアップ手順】
//  1. Enemy オブジェクトに以下を付ける
//       - Sprite Renderer
//       - Rigidbody2D … Freeze Rotation Z にチェック
//       - Box Collider 2D（体の当たり判定。IsTrigger は オフ）
//       - このスクリプト
//  2. Move Range に「中心から左右何マス動くか」を入れる
//
//  ※ 往復範囲は「置いた場所を中心」に自動で決まります。
//     Scene ビューに黄色い線で表示されるので確認してください。
// ============================================================

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPatrol : MonoBehaviour
{
    [Header("移動")]
    [Tooltip("移動する速さ")]
    [SerializeField] private float moveSpeed = 2f;

    [Tooltip("最初の位置から左右にどれだけ動くか（マス数）")]
    [SerializeField] private float moveRange = 3f;

    [Tooltip("最初に右へ動くなら true、左なら false")]
    [SerializeField] private bool startMovingRight = true;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private float leftLimitX;   // 左端のX座標
    private float rightLimitX;  // 右端のX座標
    private int direction = 1;  // 1 = 右へ, -1 = 左へ
    private bool isStopped;     // 倒されたら true

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        // 置かれた位置を中心に、左右の折り返し地点を計算する
        leftLimitX = transform.position.x - moveRange;
        rightLimitX = transform.position.x + moveRange;

        direction = startMovingRight ? 1 : -1;
        UpdateFlip();
    }

    void FixedUpdate()
    {
        if (isStopped) return;

        // --- 折り返しの判定 ---
        if (direction > 0 && transform.position.x >= rightLimitX)
        {
            direction = -1;
            UpdateFlip();
        }
        else if (direction < 0 && transform.position.x <= leftLimitX)
        {
            direction = 1;
            UpdateFlip();
        }

        // --- 移動 ---
        // 縦の速度はそのまま残す（重力で地面に乗るため）
        // ※ Unity 2022 以前は linearVelocity → velocity に置き換え
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(moveSpeed));
        }
    }

    // 進む向きに合わせて絵を反転させる
    private void UpdateFlip()
    {
        if (spriteRenderer == null) return;

        // 元の絵が「右向き」の場合の設定です。
        // 逆になったら true / false を入れ替えてください
        spriteRenderer.flipX = (direction < 0);
    }

    // ------------------------------------------------------------
    // 動きを止める（EnemyHealth から呼ばれる）
    // ------------------------------------------------------------
    public void StopMove()
    {
        isStopped = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    // ------------------------------------------------------------
    // Scene ビューに往復範囲を黄色い線で表示する（デバッグ用）
    // ------------------------------------------------------------
    private void OnDrawGizmos()
    {
        // 再生前は現在位置、再生中は計算済みの端を使う
        float centerX = Application.isPlaying
            ? (leftLimitX + rightLimitX) * 0.5f
            : transform.position.x;

        Vector3 left = new Vector3(centerX - moveRange, transform.position.y, 0f);
        Vector3 right = new Vector3(centerX + moveRange, transform.position.y, 0f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(left, right);
        Gizmos.DrawWireSphere(left, 0.15f);
        Gizmos.DrawWireSphere(right, 0.15f);
    }
}
