# Goal

> 前提知識は `00_共通の基礎知識.md` を参照。

---

## 1. 役割

プレイヤーが触れるとゲームクリアになる場所。

**このゲームで最も短いスクリプト。** トリガー判定の基本形として読むとよい。

---

## 2. セットアップ

1. 空オブジェクト `Goal` を作り、ステージの終点に置く
2. **Box Collider 2D** を追加し、**Is Trigger にチェック**
3. **Size を縦に大きめ**（例：X=1, Y=3）にする
4. `Goal` スクリプトを付ける
5. 見た目が欲しければ Sprite Renderer で旗などを設定

**判定が小さいとジャンプ中に飛び越えてしまう。** 縦に長くしておく。

---

## 3. コード解説

### 3-1. トリガー判定の基本形

```csharp
private void OnTriggerEnter2D(Collider2D other)
{
    if (isCleared) return;

    PlayerController player = other.GetComponent<PlayerController>();
    if (player == null) return;

    // 本編
}
```

**この形は `DamageZone` `Coin` `StompZone` でも全く同じ。**
1つ理解すれば全部読める。

| 行 | 意味 |
|---|---|
| `OnTriggerEnter2D` | Is Trigger のコライダーに何かが入った |
| `other` | 入ってきた相手 |
| `GetComponent<PlayerController>()` | 相手がプレイヤーか確認 |
| `== null` なら `return` | プレイヤーでなければ何もしない |

### 3-2. なぜタグを使わないのか

`other.CompareTag("Player")` という書き方もある。

**あえて `GetComponent` を使っている理由：**

- タグは**設定を忘れやすく、忘れてもエラーが出ない**
- `GetComponent` なら「PlayerController が付いているか」で判断でき、
  **付け忘れていれば他の場所でも動かないので気づける**

### 3-3. 二重発動の防止

```csharp
private bool isCleared;
```

トリガーは、コライダーの形によっては短時間に複数回入ることがある。
**フラグで1回だけに制限している。**

`Coin` の `isTaken` も同じ役割。

### 3-4. 間を置いてから遷移

```csharp
player.SetControl(false);          // 操作を止める
StartCoroutine(ClearRoutine());    // 1秒後にシーン遷移
```

即座に切り替えると、プレイヤーが「クリアした」と認識できない。
**`PlayerHealth` の死亡処理と同じ設計。**

---

## 4. 出た質問と回答

（未記入）

---

## 5. よくある詰まり

| 症状 | 原因 | 対処 |
|---|---|---|
| **触れても何も起きない** | **Is Trigger のチェック漏れ** | 最頻出。まずここを疑う |
| ジャンプで飛び越える | 判定が小さい | Size の Y を大きく |
| `Scene 'Clear' couldn't be loaded` | Build Settings 登録漏れ | 4シーンすべて登録 |
| Scene ビューで場所が分からない | 見た目がない | Sprite Renderer を付けるか緑の枠を頼りに |

---

## 6. 教えるときのコツ

**Goal は最初に読ませるスクリプトとして最適。**
10行程度で、トリガー判定の型が全部入っている。

`DamageZone` `Coin` も同じ構造だと伝えると、**3つまとめて理解できる。**

### 覚えてもらうルール

> **「触れたら何かが起きる」ものは、全部 Is Trigger にチェック。**

---

## 7. 追加の質問メモ

（未記入）
