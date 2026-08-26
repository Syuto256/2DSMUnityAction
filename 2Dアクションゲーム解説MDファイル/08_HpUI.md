# HpUI

> 前提知識は `00_共通の基礎知識.md` を参照。

---

## 1. 役割

画面にハートを並べて残りHPを表示する。

全体像で言う「**パターン3：相手を見に行くだけ**」のスクリプト。
毎フレーム `PlayerHealth` の HP を**読む**だけで、変更はしない。

---

## 2. セットアップ

1. Hierarchy → UI → **Canvas** を作る
2. Canvas Scaler を **`Scale With Screen Size`** / Reference Resolution **1920×1080**
3. Canvas の子に **Image を3つ**作り、ハートの絵を設定して横に並べる
4. Canvas に `HpUI` を付ける
5. **Hearts の Size を 3** にして、Image を**左から順に**ドラッグ
6. Player Health に Player をドラッグ

### レイアウトの目安

| 項目 | 値 |
|---|---|
| アンカー | 左上（Alt を押しながら選ぶと位置ごと移動） |
| Width / Height | 64 × 64 |
| Pos X | 60 / 130 / 200 |
| Pos Y | -60 |

**ドット絵なら Preserve Aspect にチェック。** 引き伸ばしを防ぐ。

> BE2 にハートの絵がない場合は、白い四角の Image を赤くするだけでも十分。

---

## 3. コード解説

### 3-1. 毎フレーム読みに行く

```csharp
void Update()
{
    if (playerHealth == null) return;
    UpdateHearts(playerHealth.CurrentHp);
}
```

`PlayerHealth` 側から通知させる方法（イベント）もあるが、**あえて単純にしている。**

| | 毎フレーム読む（現状） | イベント通知 |
|---|---|---|
| 分かりやすさ | ⭕ | △（新概念が必要） |
| 負荷 | 無視できる | わずかに軽い |
| 更新漏れ | 起きない | **登録忘れで起きる** |

**今回の規模では単純な方が正解。**

### 3-2. 配列とインデックス

```csharp
for (int i = 0; i < hearts.Length; i++)
{
    bool isAlive = (i < currentHp);
    hearts[i].enabled = isAlive;
}
```

`hearts` は **Image を3つ入れた配列**。`hearts[0]` `hearts[1]` `hearts[2]` で個別に触れる。

**`i < currentHp` が判定の中身。**

| HP | i=0 | i=1 | i=2 |
|---|---|---|---|
| 3 | 表示 | 表示 | 表示 |
| 2 | 表示 | 表示 | 非表示 |
| 1 | 表示 | 非表示 | 非表示 |

**左から順に消える**のはこのため。ドラッグの順番が重要になる理由でもある。

### 3-3. 自動で探す保険

```csharp
if (playerHealth == null)
{
    playerHealth = FindFirstObjectByType<PlayerHealth>();
}
```

ドラッグし忘れても動くようにしている。
ただし**探す処理は重い**ので、`Start()` で1回だけ。

---

## 4. 出た質問と回答

（未記入）

---

## 5. よくある詰まり

| 症状 | 原因 | 対処 |
|---|---|---|
| **ハートが消えない** | Hearts の Size が 3 でない | Size を 3 に |
| 減る順番がおかしい | ドラッグの順番が逆 | 左から順に入れる |
| ハートが巨大 | サイズ未指定 | Set Native Size か手動指定 |
| 画面に見えない | Render Mode | `Screen Space - Overlay` を確認 |
| 画面サイズを変えると崩れる | Canvas Scaler 未設定 | `Scale With Screen Size` に |

---

## 6. 教えるときのコツ

**Hearts の順番を入れ替えて見せる**と、配列のインデックスが何をしているか分かる。

### 覚えてもらうルール

> **UI は Canvas の中にしか置けない。**

---

## 7. 追加の質問メモ

（未記入）
