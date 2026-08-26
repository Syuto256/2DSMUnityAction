# PlayerHealth

> 前提知識は `00_共通の基礎知識.md` を参照。

---

## 1. 役割

プレイヤーの体力まわりを担当する。

- HPの管理
- 被弾処理
- 無敵時間と点滅
- 死亡処理とゲームオーバーへの遷移

### なぜ PlayerController と分けたか

**性質が違うため。**

| | PlayerController | PlayerHealth |
|---|---|---|
| いつ動くか | **毎フレーム動き続ける** | **呼ばれたときだけ動く** |
| `Update()` | ある | **ない** |

`PlayerHealth` には `Update()` がない。これは「自分からは何も始めない」ことを意味する。
全体像の図で言えば、**矢印を受ける側**。

---

## 2. セットアップ

Player オブジェクトに付けるだけ。**PlayerController と同じオブジェクト**に付ける。

### 設定値

| 項目 | 値 | 備考 |
|---|---|---|
| Max Hp | 3 | |
| Invincible Time | 1.0 | 被弾後の無敵秒数 |
| Blink Interval | 0.1 | 点滅の速さ |
| Dead Wait Time | 1.5 | ゲームオーバー画面までの間 |

`[RequireComponent(typeof(PlayerController))]` が付いているので、
PlayerController がない状態では付けられない。

---

## 3. コード解説

### 3-1. 外からの入り口は2つ

```csharp
public void TakeDamage(int damage)  // 敵・トゲから（EnemyAttack, DamageZone）
public void InstantDeath()          // 穴に落ちたとき（DamageZone）
```

`public` にしているのは、他のスクリプトから呼んでもらう必要があるため。
それ以外は全て `private`。

### 3-2. 早期リターン

```csharp
public void TakeDamage(int damage)
{
    if (isDead || isInvincible) return;
    // ここから下が本編
}
```

- `||` … 「または」
- `return` … 「このメソッドをここで終わりにする」

**「死んでいる、または無敵中なら、何もせず終わる」** と読む。

**なぜこの書き方をするか**

条件を逆にしても動く。

```csharp
if (!isDead && !isInvincible)
{
    // 本編が全部この中に入る
}
```

動きは同じだが、**本編が丸ごとインデントの中に入るため読みにくい。**
条件が3つ4つと増えると階層が深くなって追えなくなる。

**「弾く条件を先に書いて、通ったものだけ本編へ」** は実務でも定番の書き方。
`PlayerController` の `Flip()` にも同じ形がある（`if (spriteRenderer == null) return;`）。

### 3-3. コルーチン（新概念）

```csharp
private IEnumerator InvincibleRoutine()
{
    isInvincible = true;
    // ...
    yield return new WaitForSeconds(blinkInterval);   // ここで0.1秒待つ
    // ...
    isInvincible = false;
}
```

**普通のメソッドは途中で止まれない。** 上から下まで一気に実行されて終わる。

「1秒待つ」を普通に書こうとすると、`Update()` の中で時間を数える変数を用意して、
毎フレーム加算して、閾値を超えたら…と非常に面倒になる。

**コルーチンは「途中で一時停止して、後で続きから再開できるメソッド」。**

> **たとえ：しおりを挟んだ本。**
> `yield return` のところにしおりを挟んで中断し、時間が来たらそのページから再開する。
> 普通のメソッドは最後まで一気に読み切るしかない。

**3つの決まりごと**

| 決まり | 書き方 | 忘れると |
|---|---|---|
| 戻り値の型 | `IEnumerator` | **エラーが出る** |
| 待つ命令 | `yield return 〜` | ただのメソッドになる（待たない） |
| 呼び出し方 | **`StartCoroutine(〜)`** | **エラーが出ないのに何も起きない** ← 最凶 |

### 3-4. 点滅の中身

```csharp
float timer = 0f;
while (timer < invincibleTime)
{
    spriteRenderer.enabled = !spriteRenderer.enabled;   // 表示を反転
    yield return new WaitForSeconds(blinkInterval);     // 0.1秒待つ
    timer += blinkInterval;                             // 経過時間を足す
}

spriteRenderer.enabled = true;   // 必ず表示に戻す
```

「表示を反転 → 0.1秒待つ」を1秒間繰り返している。これが点滅。

**最後の1行が重要。**
ループの回数によっては非表示の状態で終わる可能性がある。
明示的に `true` に戻さないと、**プレイヤーが透明のままになる。**

`!` は「反対にする」記号。`enabled` が `true` なら `false` に、`false` なら `true` に。

### 3-5. 無敵時間がなぜ必要か

**設計判断として説明する価値がある部分。**

無敵時間がないと、敵に触れている間ずっとダメージが入る。
`OnCollisionStay2D` は毎フレーム呼ばれるので、**HP3が0.05秒で消し飛ぶ。**

「1秒間無敵」は**理不尽さを消すための仕組み。**
マリオでもロックマンでも、被弾後は必ず点滅して無敵になっている。

**点滅は装飾ではなく、「今は無敵ですよ」という情報表示。**
だから点滅が終わるタイミングと無敵が切れるタイミングを一致させている。

### 3-6. 死亡処理

```csharp
private void Die()
{
    isDead = true;
    playerController.SetControl(false);   // 操作を止める
    animator.SetTrigger("Dead");          // 死亡アニメ
    StartCoroutine(GameOverRoutine());    // 1.5秒後にシーン遷移
}
```

**すぐにシーンを切り替えていないのがポイント。**
即座に切り替えると、プレイヤーが「何が起きたか」を認識できない。

1.5秒の余白があることで、死亡アニメを見せ、状況を理解する時間が生まれる。
**「間を置く」のはゲームデザインとして重要。**

> BE2 には死亡用のスプライトがないため、`Dead` トリガーに対応するアニメを
> 作らなくてもエラーにはならない（Animator が無視するだけ）。

### 3-7. 読み取り専用の公開

```csharp
public int CurrentHp => currentHp;
public int MaxHp => maxHp;
public bool IsDead => isDead;
```

`HpUI` から HP を読むために公開しているが、**書き換えはさせない。**
詳細は Q1 を参照。

---

## 4. 出た質問と回答

### Q1. `=>` は何？

**質問の原文**

> ```csharp
> MaxHp => maxHp;
> ```
> =>は何？

**回答**

**「ラムダ演算子」**と呼ぶ。読み方は「アロー」または「ラムダ」。
意味は **「〜を返す」**。

```csharp
public int MaxHp => maxHp;
```

「`MaxHp` と聞かれたら、`maxHp` を返す」という意味。

**元の書き方**

```csharp
public int MaxHp
{
    get { return maxHp; }
}
```

**5行が1行になっている。** 中身が「返すだけ」のときに使える短縮記法。

**なぜこんな回りくどいことをするのか**

「`maxHp` を `public` にすれば済むのでは？」という疑問が自然に湧く。

| | 読める | 書き換えられる |
|---|---|---|
| `public int maxHp` | ⭕ | **⭕ ← 困る** |
| `private int maxHp` + `public int MaxHp => maxHp` | ⭕ | ❌ |

`=>` を使った書き方は、**読み取り専用の覗き窓**を作っている。
`HpUI` から HP を読みたいが、外から勝手に書き換えられては困る。だから窓だけ開ける。

これがカプセル化の実例（`00_共通の基礎知識.md` の「4」参照）。

**命名の慣習**

```csharp
private int maxHp;              // 内部用（小文字始まり）
public int MaxHp => maxHp;      // 公開用（大文字始まり）
```

**大文字始まりが公開用、小文字始まりが内部用。** C# の慣習。
この対応関係が見えると、コードの意図が読めるようになる。

**メソッドにも使える**

```csharp
public void Reset() => currentHp = maxHp;
```

ただし**中身が1行のときだけ。** 複数行なら `{ }` が必要。

---

### Q2. `IEnumerator` の意味は？

**質問の原文**

> ```csharp
> IEnumerator
> ```
> これの意味は

**回答**

**コルーチンを書くための「決まった型」。**

```csharp
private IEnumerator InvincibleRoutine()
```

**なぜこの型でなければいけないのか**

普通のメソッドは上から下まで一気に実行されて終わる。**途中で止まれない。**

でもコルーチンは「0.1秒待つ」ために**途中で中断する**必要がある。
そのために Unity は、**「このメソッドは途中で止まる可能性がある」という目印**を要求する。
それが `IEnumerator`。

**目印がないと、Unity は中断のさせ方が分からない。**

**名前の意味**

| 部分 | 意味 |
|---|---|
| `I` | Interface（インターフェース）の頭文字 |
| `Enumerator` | 列挙するもの、順番に取り出すもの |

「**順番に1つずつ取り出せる仕組み**」という意味。

コルーチンは実際、「`yield return` で区切られた処理を、1つずつ順番に実行していく」
という動き方をしている。だからこの型が使われる。

**3点セットで覚える**

```csharp
private IEnumerator MyRoutine()          // ① 型は IEnumerator
{
    yield return new WaitForSeconds(1f); // ② yield return で待つ
}

StartCoroutine(MyRoutine());             // ③ StartCoroutine で呼ぶ
```

**どれか1つでも欠けると動かない。**

**参加者への説明**

深く理解する必要はない。こう伝えれば十分。

> **「コルーチンを書くときの決まり文句。`void` の代わりに `IEnumerator` と書く、と覚えればいい」**

`I` の意味やインターフェースの話に踏み込むと初心者には重すぎる。
**「そういう書き方をするもの」で通す。**

理屈を知りたがる参加者には「途中で止まれるメソッドだよ、という目印」とだけ答える。

---

### Q3. なぜ `StartCoroutine` を前に書く必要がある？

**質問の原文**

> ```csharp
> StartCoroutine(InvincibleRoutine());
> ```
> これは何？
> 無敵時間を開始しているのはわかるが、
> なぜ前にStartCoroutineを書く必要があるかわからない

**回答**

**コルーチンは普通のメソッドとして呼んでも動かないため。**

**そのまま呼ぶとどうなるか**

```csharp
InvincibleRoutine();   // ← 何も起きない
```

エラーは出ない。でも**点滅もしないし、無敵にもならない。**

この行がやっているのは「実行」ではなく、**「実行の手順書を作っただけ」。**

**`IEnumerator` の正体がここで効いてくる**

`IEnumerator` を返すメソッドは、呼ばれても中身が実行されない。
代わりに「**こういう順番で処理する手順書**」を作って返すだけ。

```csharp
IEnumerator memo = InvincibleRoutine();   // 手順書ができただけ
```

> **たとえ：レシピを書き出しただけで、まだ火をつけていない状態。**

**`StartCoroutine` が火をつける役**

その手順書を Unity に渡して**「これを毎フレーム少しずつ進めてください」と依頼する命令。**

Unity 側が手順書を預かり、以下を管理する。

- 今どこまで進んだか
- 次に再開するのはいつか
- `WaitForSeconds` の時間を数える

**この管理役がいないと、`yield return` で止まった後、誰も再開してくれない。**

**分けて書くと分かりやすい**

```csharp
IEnumerator routine = InvincibleRoutine();   // 手順書を作る
StartCoroutine(routine);                     // 実行を依頼する
```

普段は1行にまとめているだけで、中では2段階のことが起きている。

```csharp
StartCoroutine( InvincibleRoutine() );
//     ↑              ↑
//   実行を依頼    手順書を作る（内側が先に動く）
```

**なぜ Unity に預ける必要があるのか**

「自分で待てばいいのでは？」と思うかもしれないが、
**メソッドの中で1秒止まるとゲーム全体が1秒フリーズする。**

画面も止まる、入力も効かない、敵も動かない。それでは使い物にならない。

**コルーチンは「ゲームを止めずに、この処理だけ待たせる」仕組み。**
そのためにはゲーム全体を管理している Unity 側に預ける必要がある。

**たとえるなら**

| | たとえ |
|---|---|
| `InvincibleRoutine()` | やることリストを書く |
| `StartCoroutine(...)` | **そのリストを係の人に渡す** |
| Unity | 毎フレーム進捗を見て、時間が来たら次へ進める係 |

リストを書いただけでは誰も動かない。**係の人に渡して初めて実行が始まる。**

**途中で止めることもできる**

預けているからこそ、後からキャンセルもできる。

```csharp
StopCoroutine(...);      // 特定のものを止める
StopAllCoroutines();     // 全部止める
```

「無敵中に死んだら点滅を止める」といった処理が書ける。
**Unity が管理しているから外から介入できる。**

---

## 5. よくある詰まり

| 症状 | 原因 | 対処 |
|---|---|---|
| **点滅も無敵もしない** | `StartCoroutine` を付け忘れ | `StartCoroutine(〜)` で呼ぶ |
| **プレイヤーが透明のまま** | 点滅が非表示で終わった | ループ後に `enabled = true` |
| HPが一瞬で0になる | 無敵時間が機能していない | `isInvincible` の判定を確認 |
| ダメージを受けない | Is Trigger の設定漏れ（相手側） | 相手のコライダーを確認 |
| ゲームオーバーに遷移しない | Build Settings 登録漏れ | 4シーンすべて登録 |
| `yield` でエラー | 戻り値が `void` になっている | `IEnumerator` に変更 |

---

## 6. 教えるときのコツ

### 実演すると効果的なもの

**① `StartCoroutine` を外してみせる**

```csharp
InvincibleRoutine();   // StartCoroutine を外す
```

敵に当たると**点滅せず、無敵にもならず、HPが一瞬で消し飛ぶ。エラーは1つも出ない。**

**「エラーが出ないのに動かない」という体験**は当日必ず遭遇するパターン。
先に見せておく価値が高い。

**② 無敵時間を長くする**

`Invincible Time` を 5 秒にして再生すると、点滅が長く続いて仕組みが見える。
コルーチンが「時間をかけて少しずつ進んでいる」ことが体感できる。

**③ 無敵をゼロにする**

`Invincible Time` を 0 にすると、敵に触れた瞬間にHPが消し飛ぶ。
**無敵時間が「理不尽さを消すための仕組み」だと一発で伝わる。**

### 言い換えのストック

| 概念 | 言い換え |
|---|---|
| コルーチン | しおりを挟んだ本 |
| `IEnumerator` | 途中で止まれるメソッドの目印 |
| `InvincibleRoutine()` だけ呼ぶ | レシピを書いただけ。火をつけていない |
| `StartCoroutine` | やることリストを係の人に渡す |
| `=>` | 読み取り専用の覗き窓 |
| 早期リターン | 弾く条件を先に書いて、通ったものだけ本編へ |

### 覚えてもらうルール

> **コルーチンは `StartCoroutine` を付けないと動かない。決まり文句だと思っていい。**

「なぜ？」と聞かれたら：

> **呼ぶだけだと「やることリスト」ができるだけ。それを Unity に渡して初めて実行される。**

> **`IEnumerator` は、`void` の代わりに書く決まり文句。**

---

## 7. 追加の質問メモ

> 当日出た質問をここに追記する。

（未記入）
