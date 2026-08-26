# EnemyPatrol

> 前提知識は `00_共通の基礎知識.md` を参照。

---

## 1. 役割

敵が決められた範囲を左右に往復する。

- 一定速度で横移動
- 端まで来たら折り返す
- 進行方向に合わせて絵を反転
- 倒されたら停止する

`PlayerController` と同じ「パターン1：自分だけで完結する」スクリプト。
他人を呼ばず、毎フレーム自分を動かすだけ。

---

## 2. セットアップ

| コンポーネント | 設定 |
|---|---|
| Sprite Renderer | BE2 の敵画像 |
| Rigidbody2D | **Freeze Rotation Z にチェック**<br>Collision Detection: Continuous |
| Capsule Collider 2D | Is Trigger は**オフ**（体の当たり判定）<br>**Material に NoFriction** |
| EnemyPatrol | Move Speed, Move Range |

### 設定値

| 項目 | 値 | 備考 |
|---|---|---|
| Move Speed | 2 | |
| Move Range | 3 | 置いた位置から左右に何マス動くか |
| Start Moving Right | お好みで | 最初に右へ動くか |

**往復範囲は「置いた場所が中心」。** Scene ビューに黄色い線で表示される。

---

## 3. コード解説

### 3-1. 折り返し地点は Start で決まる

```csharp
leftLimitX = transform.position.x - moveRange;
rightLimitX = transform.position.x + moveRange;
```

**再生した瞬間の位置**を中心に、左右の端を計算している。

つまり**シーンに置いた位置が往復の中心**になる。
別オブジェクトで地点を指定する方式より、設定ミスが起きにくい。

### 3-2. 折り返しの判定

```csharp
if (direction > 0 && transform.position.x >= rightLimitX)
{
    direction = -1;
    UpdateFlip();
}
```

`direction` は `1`（右）か `-1`（左）のどちらか。

**`direction > 0 &&` の部分が重要。**
「右へ進んでいる最中に、右端を超えたら」という意味。
この条件がないと、右端の外側にいる間ずっと反転し続けて震える。

### 3-3. 移動

```csharp
rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
```

`PlayerController` の横移動と同じ形。
**縦の速度をそのまま維持している**ので、重力で地面に乗る。

### 3-4. 三項演算子

```csharp
direction = startMovingRight ? 1 : -1;
```

`条件 ? 真のとき : 偽のとき` という書き方。

```csharp
if (startMovingRight) direction = 1;
else                  direction = -1;
```

上記と同じ意味。**1行で書ける短縮形。**

### 3-5. 絵の反転

```csharp
spriteRenderer.flipX = !(direction < 0);
```

> **注意：この行は素材の向きによって書き換えが必要。**
> 元絵が右向きなら `(direction < 0)`、左向きなら `!(direction < 0)` になる。
> 実際に動かして、進行方向を向いているか確認すること。

### 3-6. Gizmo で範囲を表示

```csharp
private void OnDrawGizmos()
```

`OnDrawGizmos` は**Scene ビューに線を描くための特別なメソッド。**
ゲームの動作には一切影響しない。デバッグ専用。

`PlayerController` の緑の円（`OnDrawGizmosSelected`）との違いは、
**`Selected` が付くと選択時のみ表示**される点。
`OnDrawGizmos` は常に表示されるので、敵が複数いても全部の範囲が見える。

---

## 4. 出た質問と回答

> 当日出た質問をここに追記する。

（未記入）

---

## 5. よくある詰まり

| 症状 | 原因 | 対処 |
|---|---|---|
| **敵が90度倒れる** | **摩擦でトルクが発生** | Freeze Rotation Z ＋ **NoFriction マテリアル** |
| **ターンする場所で止まる** | **摩擦の逃げ場がない** | **NoFriction マテリアル**（Friction 0） |
| 敵が足場から落ちる | Move Range が足場より広い | 範囲を狭める |
| 進行方向と逆を向く | 素材の向き | 3-5 参照 |
| 敵がプレイヤーに押される | Dynamic の押し合い | Kinematic にする（重力が効かなくなる点に注意） |
| タイルの継ぎ目で引っかかる | Box Collider の角 | Capsule Collider 2D に変更 |

### NoFriction マテリアルの作り方

1. Project 右クリック → **Create → 2D → Physics Material 2D**
2. 名前を `NoFriction` に
3. **Friction: 0** / **Bounciness: 0**
4. Enemy のコライダーの **Material 欄**にドラッグ

**プレイヤーにも付けると壁への張り付きを防げる。**

---

## 6. 教えるときのコツ

### 実演すると効果的なもの

**① Move Range を変えて Gizmo を見せる**

数値を変えると黄色い線の長さが変わる。**設定値と挙動の対応が目で見える。**

**② 摩擦の実演**

NoFriction を外して再生すると、敵が転ぶ。付け直すと直る。
**「見えない摩擦が悪さをしている」**ことが体感できる。

### 覚えてもらうルール

> **縦長のキャラを velocity で動かすと必ず転ぶ。Freeze Rotation Z と摩擦ゼロはセット。**

---

## 7. 追加の質問メモ

（未記入）
