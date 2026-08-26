# CameraFollow

> 前提知識は `00_共通の基礎知識.md` を参照。

---

## 1. 役割

カメラがプレイヤーを追いかける。ステージの外が映らないよう範囲を制限する。

`HpUI` と同じ「**相手を見に行くだけ**」のスクリプト。

---

## 2. セットアップ

1. Main Camera に `CameraFollow` を付ける
2. **Target に Player** をドラッグ
3. **まず Use Limit のチェックを外して再生**し、追従だけ確認する
4. Main Camera の **Size** で見える範囲を調整（BE2 なら 5〜7 が目安）
5. 再生を止め、Use Limit にチェックを入れて範囲を設定

> **いきなり範囲制限を設定しない。** 値が合っていないとカメラが動かず、
> 「壊れた」と勘違いする。

### 範囲の決め方

| 項目 | 目安 |
|---|---|
| Min Position X | ステージ左端 ＋ Size × 1.78 |
| Max Position X | ステージ右端 − Size × 1.78 |
| Min Position Y | 地面より少し上 |

`1.78` は 16:9 の横幅補正。
**Main Camera を選択すると赤い枠が出る**ので、それを見ながら微調整するのが確実。

> **Y の設定を忘れると、落下時にカメラが地面の下まで潜る。**

---

## 3. コード解説

### 3-1. なぜ LateUpdate なのか

```csharp
void LateUpdate()
```

**プレイヤーが動き終わった「後」にカメラを動かすため。**

`Update` に書くと、カメラが動いた後にプレイヤーが動くことがあり、
**1フレームずれてガタガタ揺れる。**

| メソッド | 呼ばれる順番 |
|---|---|
| `FixedUpdate` | 物理 |
| `Update` | 通常の処理 |
| **`LateUpdate`** | **全部終わった後** |

**「他のものを追いかける処理は LateUpdate」** と覚える。

### 3-2. Mathf.Clamp

```csharp
goalX = Mathf.Clamp(goalX, minPosition.x, maxPosition.x);
```

`Clamp` は「**指定した範囲に収める**」命令。

- 範囲より小さければ最小値にする
- 範囲より大きければ最大値にする
- 範囲内ならそのまま

これでステージの外が映らなくなる。

### 3-3. SmoothDamp

```csharp
transform.position = Vector3.SmoothDamp(
    transform.position, goalPosition, ref currentVelocity, smoothTime);
```

**目標地点へなめらかに近づける**命令。
急に飛ぶのではなく、じわっと追いかける。

`ref currentVelocity` は、この命令が内部で使う作業用の変数。
**中身を気にする必要はない**が、必ず用意して渡す必要がある。

`smoothTime` が小さいほど機敏、大きいほどゆったり。**0.15〜0.3 が自然。**

### 3-4. カメラの Z 座標

```csharp
Vector3 goalPosition = new Vector3(goalX, goalY, transform.position.z);
```

**Z を現在の値のまま維持している。**
2Dカメラの Z は `-10` である必要があり、動かすと何も映らなくなる。

---

## 4. 出た質問と回答

（未記入）

---

## 5. よくある詰まり

| 症状 | 原因 | 対処 |
|---|---|---|
| **カメラが動かない** | Target 未設定 | Player をドラッグ |
| 同上 | Use Limit の範囲が狭すぎる | 一度チェックを外して確認 |
| **画面が真っ暗** | Z 座標が -10 以外 | -10 に戻す |
| ガタガタ揺れる | Smooth Time が小さすぎる | 0.15〜0.3 に |
| 同上 | Player の Interpolate 未設定 | Rigidbody2D で Interpolate に |
| ステージ外が映る | 範囲未設定 | 赤い枠を見ながら調整 |
| **マップを広げたら途中で止まる** | **範囲の更新忘れ** | Min/Max を再設定 |

> **マップ作成後は必ず Min/Max を設定し直す。** 忘れやすい。

---

## 6. 教えるときのコツ

**Smooth Time を 1 にして再生**すると、カメラが大きく遅れて付いてくる。
**数値の意味が体感で分かる。**

### 覚えてもらうルール

> **他のものを追いかける処理は LateUpdate。**

---

## 7. 追加の質問メモ

（未記入）
