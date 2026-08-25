using System.Collections;
using UnityEngine;

// ============================================================
// Goal
// プレイヤーが触れるとゲームクリアになる場所です。
//
// 【セットアップ手順】
//  1. 空オブジェクト「Goal」を作り、ゴール地点に置く
//  2. Box Collider 2D を付け、Is Trigger に必ずチェック
//  3. このスクリプトを付ける
//  4. 見た目が欲しければ Sprite Renderer で旗などを付ける
// ============================================================

public class Goal : MonoBehaviour
{
    [Header("クリア演出")]
    [Tooltip("ゴールしてからクリア画面に移るまでの秒数")]
    [SerializeField] private float waitTime = 1.0f;

    private bool isCleared; // 二重に発動しないようにする

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCleared) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        isCleared = true;
        Debug.Log("ゲームクリア！");

        // 操作を止める
        player.SetControl(false);

        StartCoroutine(ClearRoutine());
    }

    private IEnumerator ClearRoutine()
    {
        yield return new WaitForSeconds(waitTime);
        SceneLoader.LoadClear();
    }
}
