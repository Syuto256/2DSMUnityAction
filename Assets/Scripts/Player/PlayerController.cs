using UnityEngine;

// ============================================================
// PlayerController
// プレイヤーの「横移動・ジャンプ・接地判定・向きの反転・アニメ更新」を担当します。
//
// 【セットアップ手順】
//  1. Player オブジェクトに以下を付ける
//       - Sprite Renderer
//       - Rigidbody2D  … Constraints の Freeze Rotation Z に必ずチェック
//       - Capsule Collider 2D（足元が丸いと段差に引っかかりにくい）
//       - このスクリプト
//  2. Player の子に空オブジェクト「GroundCheck」を作り、足元より少し下に置く
//  3. インスペクターの Ground Check にその GroundCheck をドラッグする
//  4. 地面の Tilemap の Layer を「Ground」にする
//  5. インスペクターの Ground Layer で「Ground」だけにチェックを入れる
//
//  ※ 4と5を忘れると「ジャンプが一切できない」状態になります。最初に確認！
// ============================================================

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("移動")]
    [Tooltip("横に動く速さ。大きいほど速い")]
    [SerializeField] private float moveSpeed = 6f;

    [Header("ジャンプ")]
    [Tooltip("ジャンプの強さ。大きいほど高く跳ぶ")]
    [SerializeField] private float jumpPower = 12f;

    [Header("接地判定")]
    [Tooltip("足元に置いた空オブジェクトをここにドラッグ")]
    [SerializeField] private Transform groundCheck;

    [Tooltip("接地を調べる円の大きさ。大きすぎると壁でジャンプできてしまう")]
    [SerializeField] private float groundCheckRadius = 0.15f;

    [Tooltip("「地面」とみなすレイヤー。Ground だけにチェックを入れる")]
    [SerializeField] private LayerMask groundLayer;

    [Header("敵を踏んだとき")]
    [Tooltip("敵を踏んだときに跳ねる強さ")]
    [SerializeField] private float bouncePower = 8f;

    [Header("見た目")]
    [Tooltip("元の絵が左向きならチェックを入れる")]
    [SerializeField] private bool spriteFacesLeft = false;

    // --- 他のスクリプトから状態を知りたいとき用 ---
    public bool IsGround => isGround;

    // --- 内部で使う変数 ---
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private float inputX;        // 横入力（-1 = 左, 0 = 停止, 1 = 右）
    private bool jumpRequest;    // 「ジャンプしたい」という予約フラグ
    private bool isGround;       // 地面に足がついているか
    private bool canControl = true; // 操作を受け付けるか（死亡時などに false）

    void Awake()
    {
        // 必要な部品をあらかじめ取っておく（毎フレーム取ると重いため）
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        // 設定忘れをその場で気づけるようにする
        if (groundCheck == null)
        {
            Debug.LogError("GroundCheck が設定されていません！ インスペクターを確認してください", this);
        }
    }

    // ------------------------------------------------------------
    // Update … 毎フレーム呼ばれる。「入力」と「見た目」はこちらで扱う
    //
    // なぜ入力を Update に書くのか？
    //   GetButtonDown（押した瞬間）は Update でしか正しく拾えないためです。
    //   FixedUpdate に書くと、押したのにジャンプしないことがあります。
    // ------------------------------------------------------------
    void Update()
    {
        if (canControl)
        {
            // 左右キー / A・Dキー を -1〜1 の数値で受け取る
            inputX = Input.GetAxisRaw("Horizontal");

            // スペースキーを押した & 地面にいる → ジャンプを予約する
            // （ここでは予約だけ。実際に跳ぶのは FixedUpdate）
            if (Input.GetButtonDown("Jump") && isGround)
            {
                Debug.Log("ジャンプ入力を受け取りました");
                jumpRequest = true;
            }
        }
        else
        {
            inputX = 0f;
            jumpRequest = false;
        }

        Flip();
        UpdateAnimator();
    }

    // ------------------------------------------------------------
    // FixedUpdate … 一定間隔で呼ばれる。「物理（実際に動かす処理）」はこちら
    //
    // なぜ分けるのか？
    //   Rigidbody を動かす処理を Update に書くと、
    //   PCの性能によって移動速度が変わったり、動きがガタつきます。
    // ------------------------------------------------------------
    void FixedUpdate()
    {
        // --- 接地判定 ---
        // GroundCheck の位置に小さな円を描き、Ground レイヤーに触れていれば true
        if (groundCheck != null)
        {
            isGround = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        // --- 横移動 ---
        // 縦の速度（y）はそのまま残す。ここを 0 にすると落下しなくなるので注意
        // ※ Unity 2022 以前を使う場合は linearVelocity → velocity に置き換えてください
        rb.linearVelocity = new Vector2(inputX * moveSpeed, rb.linearVelocity.y);

        // --- ジャンプ ---
        if (jumpRequest)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
            jumpRequest = false; // 予約を使い切る
        }
    }

    // ------------------------------------------------------------
    // 進む向きに合わせて絵を左右反転させる
    // ------------------------------------------------------------
    private void Flip()
    {
        if (spriteRenderer == null) return;

        if (inputX > 0f)
        {
            spriteRenderer.flipX = spriteFacesLeft;
        }
        else if (inputX < 0f)
        {
            spriteRenderer.flipX = !spriteFacesLeft;
        }
        // inputX が 0 のときは何もしない（止まったら向きを保つ）
    }

    // ------------------------------------------------------------
    // Animator に今の状態を伝える
    // Animator をまだ作っていなくてもエラーにならないようにしてあります
    // ------------------------------------------------------------
    private void UpdateAnimator()
    {
        if (animator == null) return;

        // Speed … 0 なら待機、0より大きければ走り
        animator.SetFloat("Speed", Mathf.Abs(inputX));

        // IsGround … false ならジャンプ中
        animator.SetBool("IsGround", isGround);
    }

    // ------------------------------------------------------------
    // 敵を踏んだときに、敵側から呼んでもらう小ジャンプ
    // ------------------------------------------------------------
    public void Bounce()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, bouncePower);
    }

    // ------------------------------------------------------------
    // 操作を止める / 再開する（死亡時・ゲームクリア時に使う）
    // ------------------------------------------------------------
    public void SetControl(bool enable)
    {
        canControl = enable;

        if (!enable)
        {
            // 横の動きだけ止める（縦はそのままにして重力を効かせる）
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
    }

    // ------------------------------------------------------------
    // Sceneビューで接地判定の円を緑色で表示する（デバッグ用）
    // Playerを選択すると見えます。位置調整に使ってください
    // ------------------------------------------------------------
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
