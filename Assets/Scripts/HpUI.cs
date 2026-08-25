using UnityEngine;
using UnityEngine.UI;

// ============================================================
// HpUI
// 画面にハートを並べて、残りHPを表示します。
//
// 【セットアップ手順】
//  1. Hierarchy → UI → Canvas を作る
//  2. Canvas の子に Image を3つ作り、ハートの絵を設定して横に並べる
//  3. Canvas に このスクリプトを付ける
//  4. Hearts の Size を 3 にして、作った Image を順番にドラッグする
//  5. Player Health に、シーン内の Player をドラッグする
// ============================================================

public class HpUI : MonoBehaviour
{
    [Header("参照")]
    [Tooltip("ハートの Image を左から順に入れる")]
    [SerializeField] private Image[] hearts;

    [Tooltip("シーン内の Player をドラッグ。空なら自動で探します")]
    [SerializeField] private PlayerHealth playerHealth;

    [Header("表示方法")]
    [Tooltip("チェックを外すと、減ったハートを消さずに暗く表示します")]
    [SerializeField] private bool hideLostHearts = true;

    [Tooltip("暗く表示するときの色")]
    [SerializeField] private Color lostColor = new Color(1f, 1f, 1f, 0.25f);

    void Start()
    {
        // ドラッグし忘れていたら自動で探す
        // ※ Unity 2022 以前は FindFirstObjectByType → FindObjectOfType
        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
        }

        if (playerHealth == null)
        {
            Debug.LogError("PlayerHealth が見つかりません", this);
        }
    }

    void Update()
    {
        if (playerHealth == null) return;

        UpdateHearts(playerHealth.CurrentHp);
    }

    private void UpdateHearts(int currentHp)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null) continue;

            // i番目のハートが「まだ残っているか」
            bool isAlive = (i < currentHp);

            if (hideLostHearts)
            {
                hearts[i].enabled = isAlive;
            }
            else
            {
                hearts[i].enabled = true;
                hearts[i].color = isAlive ? Color.white : lostColor;
            }
        }
    }
}
