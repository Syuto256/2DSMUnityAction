# PlayerController

> 前提知識は `00_共通の基礎知識.md` を参照。

---

## 1. 役割

プレイヤーの以下を1つのスクリプトで担当する。

- 横移動
- ジャンプ
- 接地判定
- 見た目の左右反転
- Animator への値の受け渡し

### なぜ1ファイルにまとめたか

初心者が「操作が変だ」と思ったとき、**開くファイルが1つで済む**ようにするため。
機能ごとにファイルを分けると設計としては綺麗だが、処理を追うのが難しくなる。

今回の規模（9時間・1ステージ）ではこの構成が最適。

---

## 2. セットアップ

### Player オブジェクトに付けるもの

| コンポーネント | 設定 |
|---|---|
| Sprite Renderer | BE2 のプレイヤー画像 |
| Rigidbody2D | **Freeze Rotation Z にチェック**<br>Gravity Scale: 3<br>Collision Detection: Continuous<br>Interpolate: Interpolate |
| Capsule Collider 2D | Direction: Vertical |
| PlayerController | 下記参照 |
| PlayerHealth | — |

Layer は `Player` に設定。

### 子オブジェクト GroundCheck

Player を右クリック → Create Empty で作成し、**足元より少し下**（Y = -0.5 程度）に配置。

### PlayerController の設定値

| 項目 | 値 | 備考 |
|---|---|---|
| Move Speed | 6 | 素材サイズに応じて調整 |
| Jump Power | 13 | Gravity Scale とセットで調整 |
| Ground Check | GroundCheck をドラッグ | **必須** |
| Ground Check Radius | 0.15 | 大きすぎると壁でジャンプできる |
| Ground Layer | **Ground のみ**にチェック | **必須** |
| Bounce Power | 8 | 敵を踏んだときの跳ね |

**Ground Layer の設定を忘れるとジャンプが一切できない。** 最頻出のミス。

---

## 3. コード解説

### 3-1. 入り口は2つ

```csharp
void Update()      // 毎フレーム：入力と見た目
void FixedUpdate() // 一定間隔：物理（実際に動かす）
```

役割の違いは `00_共通の基礎知識.md` の「3. Update と FixedUpdate の使い分け」を参照。

### 3-2. 入力を受け取る

```csharp
inputX = Input.GetAxisRaw("Horizontal");
```

`Horizontal` は Unity にあらかじめ登録された名前。**←→キーと A/D キー**が割り当て済み。

返る値は3種類だけ。

| 状態 | 値 |
|---|---|
| 左を押している | `-1` |
| 何も押していない | `0` |
| 右を押している | `1` |

**`GetAxisRaw` の `Raw` は「加工しない」という意味。**
`GetAxis`（Raw なし）だと 0 から 1 へじわっと変化する。滑らかだが、
キーを離した後もしばらく動き続けるため、アクションゲームでは操作感が悪い。

### 3-3. ジャンプの「予約」

ここが最もトリッキーな部分。**2段階に分かれている。**

```csharp
// Update：予約するだけ
if (Input.GetButtonDown("Jump") && isGround)
{
    jumpRequest = true;
}
```

```csharp
// FixedUpdate：実行して予約を消す
if (jumpRequest)
{
    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
    jumpRequest = false;
}
```

**なぜ2段階なのか**

- 押した瞬間（`GetButtonDown`）は `Update` でしか拾えない
- 物理（`rb`）は `FixedUpdate` でしか動かしたくない

そこで **`Update` で「ジャンプしたい」とメモを残し、`FixedUpdate` がそれを見て実行**する。

**`jumpRequest = false` を消すとメモが残り続け、永久にジャンプし続ける。**

### 3-4. 横移動

```csharp
rb.linearVelocity = new Vector2(inputX * moveSpeed, rb.linearVelocity.y);
```

`linearVelocity` は**速度**。`Vector2` は「横と縦、2つの数字のセット」。

| | 中身 | 意味 |
|---|---|---|
| 横（x） | `inputX * moveSpeed` | 右なら `1 × 6 = 6`、左なら `-6`、停止なら `0` |
| 縦（y） | `rb.linearVelocity.y` | **今の値をそのまま維持** |

**縦を「今の値のまま」にしているのが重要。**
ここに `0` を書くと重力が打ち消され、空中で止まる。ジャンプも落下もできなくなる。

> 参加者が「浮いたまま動かない」と言ってきたら、まずここを疑う。

### 3-5. 接地判定

```csharp
isGround = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
```

日本語にすると：

> **`groundCheck` の位置に、半径 `groundCheckRadius` の円を仮定する。
> その円が `groundLayer` のものと重なっていれば true。**

引数の役割：

| 引数 | 役割 | 値 |
|---|---|---|
| `groundCheck.position` | **どこを**調べるか | 足元の座標 |
| `groundCheckRadius` | **どのくらいの範囲**を調べるか | 0.15 |
| `groundLayer` | **何を**探すか | Ground レイヤー |

`OnDrawGizmosSelected()` に書いた緑の円は、**まさにこの判定範囲を可視化したもの**。
位置調整はこれを見ながら行う。

### 3-6. 見た目の反転

```csharp
if (inputX > 0f)       spriteRenderer.flipX = false;
else if (inputX < 0f)  spriteRenderer.flipX = true;
```

**`else` ではなく `else if` にしている理由**

`inputX` が 0 のとき何もしないため。`else` にすると、止まった瞬間に必ず右を向く。
「止まったら向きを保つ」という自然な挙動のための工夫。

**元絵が左向きの素材を使う場合は `true` / `false` が逆になる。**（Q5 参照）

### 3-7. 外部から呼ばれるメソッド

```csharp
public void Bounce()          // 敵を踏んだとき（StompZone から）
public void SetControl(bool)  // 操作の停止／再開（PlayerHealth, Goal から）
```

`public` にしているのは、**他のスクリプトから呼んでもらう必要があるため。**
それ以外は全て `private`。

---

## 4. 出た質問と回答

### Q1. Update に `rb.linearVelocity = ...` を書くと何が困る？

**質問の原文**

> Updateに
> ```csharp
> rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
> ```
> を入れるとどういった不便なことが起こるの？

**回答**

一見動いてしまうが、3つの問題が起きる。**3つとも「エラーが出ない」のが共通点。**

| | 症状 | 気づきやすさ |
|---|---|---|
| ① | ジャンプの高さが安定しない | **ほぼ気づけない** |
| ② | 後から壊れる時限爆弾 | 原因不明になる |
| ③ | 見た目がカクつく | 何となく気持ち悪い |

**① PCの性能でジャンプの高さが変わる**

`Update` と `FixedUpdate` は同じリズムで動いていない。速度を書いた直後に
物理が回るか、しばらく回らないかが毎回変わる。その間、重力は `FixedUpdate` の
タイミングでしか働かない。

- 直後に物理が回れば → 設定どおりの高さ
- 間が空けば → 重力に削られてわずかに低い

数センチの差だが、**「あと少しで足場に届かない」場面で理不尽な失敗**が起きる。
参加者は原因が分からず「操作ミス」だと思い込む。

**② 同じフレームで2回書くと打ち消し合う**

現状は `FixedUpdate` 内で「横移動 → ジャンプ」の順が保証されている。
`Update` に移すとこの順番が崩れ、**横移動の行を `0` に書き換えた瞬間に
ジャンプが効かなくなる。** しかもエラーは出ない。

**③ 動きがカクつく**

Interpolate は「前回と今回の物理位置の間」を補間している。
そこへ想定外のタイミングで速度が変わると、立ち上がりが一瞬ガクッとする。

**教えるときは①だけ話せば十分。** 実害が想像しやすく納得されやすい。

---

### Q2. `Physics2D.OverlapCircle` は何をしている？

**質問の原文**

> ```csharp
> Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
> ```
> この処理が何をしているのかがわからない。
> サークル（groundCheck）を作ってそれがgroundLayerに触れているとき、
> isGroundが作れて、それでジャンプできるようになる感じ？

**この認識の答え合わせ**

| 認識 | 判定 |
|---|---|
| groundLayer に触れていると isGround になる | ⭕ **合っている** |
| ジャンプできるようになる | ⭕ **合っている** |
| サークルを「作る」 | ❌ 作られない。その瞬間だけ調べて消える |
| groundCheck が円 | ❌ 円ではなく「調べる位置」を示す空オブジェクト |

**大筋は正しい。ズレているのは「円が存在し続けている」というイメージの部分。**

**回答**

**円というオブジェクトが作られるわけではない。**

その一瞬だけ「ここに円があったとしたら、何かに重なる？」と計算して、
答えを返して消える命令。

- ❌ 足元に見えない円盤を貼り付けている
- ⭕ **足元に懐中電灯を一瞬当てて、何か照らされたか確認している**

`FixedUpdate` の中にあるので、**1秒に50回この確認を繰り返している。**

**`groundCheck` は円ではなく「位置」**

ただの空オブジェクトで、役割は「どこを調べるか」という座標を教えることだけ。
Player の子にしておけば、本体が動けば足元の座標も一緒に動く。

**動作の流れ**

```
足元に地面がある   → 「あった」を返す   → isGround = true  → ジャンプできる
ジャンプして空中へ → 「なかった」を返す → isGround = false → 押しても無反応
着地              → 「あった」を返す   → isGround = true  → またジャンプできる
```

これが**二段ジャンプを防いでいる仕組み。**

**`groundLayer` の役割**

3つ目の引数がなければ、**プレイヤー自身のコライダーまで「地面」と判定される。**
GroundCheck は Player の内部にあるため、自分自身に必ず重なるから。

`groundLayer` で「Ground レイヤーだけを見る」と指定することで、それ以外は無視される。
**だから Ground レイヤーの設定を忘れると永久に `false` になる。**

**補足：戻り値の正体**

厳密には `true` / `false` ではなく、**見つかったコライダーそのもの**を返す。
見つからなければ `null`。C# が「何か入っていれば true、null なら false」と
変換してくれている。

```csharp
// どの地面に乗っているか知りたい場合
Collider2D ground = Physics2D.OverlapCircle(...);
if (ground != null) { Debug.Log(ground.gameObject.name); }
```

---

### Q3. `Physics2D.OverlapCircle` という名前の意味は？

**質問の原文**

> Physics2D.OverlapCircleってどういう意味？

**回答**

名前を3つに分解する。

| 部分 | 意味 |
|---|---|
| `Physics2D` | 2Dの物理を扱う道具箱 |
| `Overlap` | 重なっている |
| `Circle` | 円 |

**「2D物理の道具箱にある、円が重なっているか調べる機能」**

**`Physics2D` と `Physics`（2Dなし）は別物。**
`Physics` は3D用なので、2Dゲームで使うと何も検出されない。

**`Overlap` の性質**

| | 動き方 |
|---|---|
| `Overlap` | **こちらから聞きに行く**（今どうなってる？） |
| `OnCollisionEnter2D` | **向こうから知らせが来る**（ぶつかりました！） |

`Overlap` は好きなタイミングで好きな場所を調べられる。
だから足元という「プレイヤー本体とは違う場所」を調べられる。

**`Circle` の仲間**

| 命令 | 形 | 使いどころ |
|---|---|---|
| `OverlapCircle` | 円 | **足元の接地判定**、爆発の範囲 |
| `OverlapBox` | 四角 | 壁ぎわの判定、細長い範囲 |
| `OverlapPoint` | 点 | マウスクリックの位置 |

**接地判定に円を使う理由**は、角がないので段差に引っかかりにくいから。
四角だと角が地面の継ぎ目に反応して判定がチラつく。

**別系統：`Raycast`**

一点から線を飛ばして最初に当たったものを調べる。
`Overlap` が「その場を照らす」なら、`Raycast` は「レーザーポインタを向ける」。
線なので細く、足の端が地面にかかっている状態を拾えないことがある。

---

### Q4. `SpriteRenderer` は bool で見た目を変えられるの？

**質問の原文**

> ```csharp
> spriteRenderer.flipX = false;
> ```
> SpriteRendererってboolのようにtrueやfalseで見た目を変えることが出来るの？

**回答**

**主語が1つズレている。bool なのは `SpriteRenderer` ではなく `flipX` の方。**

```csharp
spriteRenderer.flipX = false;
//     ↑             ↑
//  部品そのもの    その部品が持つ「設定項目」の1つ
```

`SpriteRenderer` は「絵を表示する部品」で、その中にたくさんの設定項目がある。
`flipX` はそのうちの1つで、型が `bool`。

**インスペクターと同じものを見ている**

Player を選択して Inspector の Sprite Renderer を見ると、`Flip` の
`X` `Y` チェックボックスがある。`spriteRenderer.flipX = true;` は
**そのチェックボックスにチェックを入れる**という意味。

項目ごとに型が違う：

| 書き方 | インスペクターの項目 | 型 |
|---|---|---|
| `spriteRenderer.flipX` | Flip X | `bool` |
| `spriteRenderer.color` | Color | `Color` |
| `spriteRenderer.enabled` | 左上のチェック | `bool` |
| `spriteRenderer.sortingOrder` | Order in Layer | `int` |

`PlayerHealth` の点滅も同じ仕組み：

```csharp
spriteRenderer.enabled = !spriteRenderer.enabled;
```

**なぜ反転で向きが変わるのか**

`flipX` は絵を左右にひっくり返して表示する機能。画像を用意し直していない。
**左向き用の画像を別途用意する必要がないので、素材が半分で済む。**

**注意点**

子オブジェクト（武器・攻撃判定など）は `flipX` では反転しない。
絵だけがひっくり返り、子は元の位置に残る。
その場合は `transform.localScale` の X をマイナスにする方法に切り替える。

> 今回の `GroundCheck` は中央にあるので影響しない。

---

### Q5. 元絵が左向きのキャラだと逆にならない？

**質問の原文**

> もともと左向きのキャラ画像の時は、右に移動するとき後ろを向いてしまうのではないか？

> **この指摘は完全に正しい。** コードは「元絵が右向き」を前提にしているため、
> 左向き素材ではそのまま逆になる。

**回答**

**なる。** 現在のコードは「元絵が右向き」を前提にしている。

**対処法A：`true` / `false` を入れ替える**

```csharp
if (inputX > 0f)       spriteRenderer.flipX = true;
else if (inputX < 0f)  spriteRenderer.flipX = false;
```

`EnemyPatrol` も同様（`<` を `>` に変える）。

```csharp
spriteRenderer.flipX = (direction > 0);
```

**対処法B：インスペクターで切り替えられるようにする（推奨）**

素材が変わるたびにコードを直すのは筋が悪い。フラグを1つ増やす。

```csharp
[Header("見た目")]
[Tooltip("元の絵が左向きならチェックを入れる")]
[SerializeField] private bool spriteFacesLeft = false;

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
}
```

`!` は「反対にする」記号。

**利点：向きが逆だと気づいた参加者が、コードを読まずに自分で直せる。**

これは `moveSpeed` や `jumpPower` を `[SerializeField]` にしているのと同じ考え方。
詳細は `00_共通の基礎知識.md` の「7. 設計の指針」を参照。

---

### Q6. なぜ `rb.linearVelocity` と書く？ `linearVelocity` だけではダメ？

**質問の原文**

> ```csharp
> rb.linearVelocity
> ```
> これはどうしてrb.〇〇と書いているの？
> linearVelocityと書くだけじゃダメなの？
> rb.と書くことでGetComponent&lt;Rigidbody2D&gt;();の情報があるというイメージ？

**この認識の答え合わせ**

> **⭕ イメージは合っている。**
> `Awake()` で `GetComponent<Rigidbody2D>()` した結果を `rb` に入れてあるので、
> `rb.` と書くと「その Rigidbody2D の」という意味になる。

**回答**

シーンの中には Rigidbody2D を持つオブジェクトが**複数ある**（プレイヤー、敵など）。

`linearVelocity = ...` とだけ書いたら、Unity は「**誰の速度？**」となる。
`rb.` を付けることで「**この Rigidbody2D の**速度」と特定できる。

> クラスに田中くんが3人いたら「1組の田中くん」と言う必要がある、のと同じ。

`rb` の正体、`GetComponent` との関係、毎回呼んではいけない理由は
`00_共通の基礎知識.md` の「2. GetComponent と変数の関係」を参照。

**`Physics2D.` との違い**

| | 何を指すか | 前に付くもの |
|---|---|---|
| `Physics2D.〜` | **機能そのもの**（誰のものでもない） | クラス名（固定） |
| `rb.〜` | **特定のオブジェクトの部品** | 変数名（自分で決める） |

`Physics2D` は世界に1つしかない共通の道具箱なので特定不要。
`rb` は「たくさんある中のこれ」なので変数で指す必要がある。

---

### Q7. なぜ `canControl = enable;` と写している？ 片方ではダメ？

**質問の原文**

> ```csharp
> public void SetControl(bool enable)
> {
>     canControl = enable;
>
>     if (!enable)
>     {
>         rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
>     }
> }
> ```
> なぜ、canControlにenableを入れているの。
> どちらか片方ではだめなの？

**回答**

**片方だけでは成立しない。役割がまったく違う。**

| | `enable` | `canControl` |
|---|---|---|
| 何者か | メソッドの**引数** | クラスの**変数（フィールド）** |
| 寿命 | メソッドが終わると**消える** | ゲーム中ずっと**残る** |
| 誰が決める | 呼び出した側が渡す | このスクリプトが保持する |

**`enable` が消えるという話**

```
Goal が SetControl(false) を呼ぶ
   ↓
enable に false が入る
   ↓
canControl = enable;   ← 掲示板に書き写す
   ↓
メソッドが終わる
   ↓
enable は消える   ← ここが重要
```

`Update()` は毎フレーム `canControl` を参照し続ける必要がある。
**`enable` しかなければ、`Update()` から見に行くものが存在しない。**

**`canControl` だけでもダメな理由**

```csharp
public void SetControl()   // 引数なし
{
    canControl = false;
}
```

止めることはできるが、**再開できない。**
引数があることで、同じメソッドで停止と再開の両方ができる。

```csharp
player.SetControl(false);  // 止める
player.SetControl(true);   // 再開する
```

**なぜ `canControl` を `public` にしないのか**

```csharp
canControl = enable;

if (!enable)
{
    rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);  // ← 後始末
}
```

**止めるときは「速度もゼロにする」という後始末がセット**になっている。

外から `canControl = false` と直接書かれると、入力は止まるのに速度が残り、
キーを離しているのにキャラが滑り続ける。

**メソッドにしておけば、呼ぶ側は後始末を知らなくていい。**

この考え方（カプセル化）と、引数とフィールドの名前を変える理由は
`00_共通の基礎知識.md` の「4」「5」を参照。

---

## 5. よくある詰まり

| 症状 | 原因 | 対処 |
|---|---|---|
| **ジャンプできない** | Ground Layer 未設定 | Ground Layer で Ground にチェック |
| ジャンプできない | GroundCheck の位置が高い | 緑の円が地面に少し埋まる位置へ |
| **二段ジャンプできてしまう** | Ground Check Radius が大きすぎる | 0.15 程度に |
| その場でクルクル回る | Freeze Rotation Z 未チェック | Rigidbody2D で設定 |
| 地面をすり抜ける | 地面のコライダー忘れ | Box Collider 2D を追加 |
| 空中で止まる・落ちない | `linearVelocity.y` を 0 にしている | 現在の y を維持する |
| 勝手に高く飛ぶように見える | **Jump Power が大きすぎる** | Gravity Scale とセットで調整 |
| 向きが逆 | 元絵の向きが違う | Q5 の対処法B |
| 動きがガタつく | Interpolate 未設定 | Rigidbody2D の Interpolate を設定 |
| `Input.GetAxisRaw` でエラー | Active Input Handling | Project Settings で `Both` に |

---

## 6. 教えるときのコツ

### 実演すると効果的なもの

**① 接地判定の可視化**

1. Player を選択させ、Scene ビューで足元の緑の円を見せる
2. 再生して、地面に乗っているとき／ジャンプ中を見比べさせる
3. 「この円が地面に触っているときだけジャンプできる」と伝える

さらに **Ground Check Radius を 3 くらいに大きくして再生**させると、
円が巨大になり空中でもジャンプし放題になる。
**「判定の大きさ」が挙動を決めていることが体感で分かる。**

**② インスペクターとコードの対応**

再生中に Inspector の Flip X を手でクリックさせる。キャラが左右にひっくり返る。
「これをコードから自動でやっているのが `spriteRenderer.flipX = true;`」と説明すると
一発で腑に落ちる。

**③ 数値のバラつきを見せる**

`Update` に物理を書くとどうなるか試したがる参加者には、
`Debug.Log(rb.linearVelocity.y)` を出させると数値のバラつきが見える。
体感より説得力がある。

### 言い換えのストック

| 概念 | 言い換え |
|---|---|
| Update と FixedUpdate | 目で見て判断する／体を動かす |
| `rb.` を付ける理由 | 田中くんが3人いたら組を言う |
| `OverlapCircle` | 懐中電灯を一瞬当てて確認する |
| 引数とフィールド | 届いた手紙／書き写した掲示板 |
| `GetComponent` | 同じオブジェクトの別部品を名前を付けて手元に持ってくる |

### 覚えてもらうルール

> **Rigidbody を触る命令は、全部 `FixedUpdate` に置く。**

理由を聞かれたら「PCの性能でジャンプの高さが変わっちゃうから」だけ答える。

> **知らない命令が出てきたら、まず名前を英語のまま分解して読む。**

これはプロも実際にやっている。以降の学習速度が変わる。

---

## 7. 追加の質問メモ

> 当日出た質問をここに追記する。

（未記入）
