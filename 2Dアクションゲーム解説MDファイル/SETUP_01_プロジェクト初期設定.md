# プロジェクト初期設定

> **このファイルは講師が事前に行う作業。**
> ここを済ませた状態のプロジェクトを配布すれば、当日30分以上節約できる。

---

## なぜ事前にやるのか

以下の設定は**すべて「忘れてもエラーが出ない」**という共通点がある。
設定漏れの症状が「なんとなく動かない」という形で現れるため、
**初心者が自力で原因にたどり着くのはほぼ不可能。**

当日はプログラムの理解に時間を使いたいので、環境まわりは潰しておく。

---

## 0. プロジェクトの作成

Unity Hub → New project → **2D (Built-In Render Pipeline)** または **Universal 2D**

| 項目 | 推奨 |
|---|---|
| Unity バージョン | Unity 6 系 |
| テンプレート | **2D** |

> **3Dテンプレートで作らないこと。** 2D用のパッケージが入らない。

**Unity 2022 以前を使う場合**、スクリプト内の以下を置換する必要がある。

| Unity 6 | Unity 2022 以前 |
|---|---|
| `rb.linearVelocity` | `rb.velocity` |
| `FindFirstObjectByType<T>()` | `FindObjectOfType<T>()` |

該当箇所はスクリプト内にコメントで明記してある。

---

## 1. 入力方式（最優先）

**Edit → Project Settings → Player → Other Settings → Active Input Handling**

**`Both` に変更する。**

> **変更後は Unity の再起動が必要。**

### なぜ必要か

Unity 6 の既定は `Input System Package (New)`。
このままだと `Input.GetAxisRaw()` が**起動直後にエラーで止まる。**

今回は旧 Input Manager を使う方針のため、`Both` にして両方使えるようにする。

**当日これを忘れると、プレイヤーが1歩も動かない。**

---

## 2. Layer の作成

Inspector 右上の **Layer → Add Layer** から追加する。

| 追加する Layer |
|---|
| `Ground` |
| `Player` |
| `Enemy` |

### なぜ必要か

`PlayerController` の接地判定が `Ground` レイヤーを見ている。

```csharp
Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
```

**Layer が Default のままだとジャンプが一切できない。** 最頻出の詰まり。

---

## 3. Physics Material 2D（摩擦ゼロ）

1. Project ウィンドウ右クリック → **Create → 2D → Physics Material 2D**
2. 名前を **`NoFriction`** に
3. Inspector で設定

| 項目 | 値 |
|---|---|
| **Friction** | **0** |
| **Bounciness** | **0** |

### なぜ必要か

**縦長のキャラを velocity で動かすと、摩擦でトルクが発生して転ぶ。**

Freeze Rotation Z で回転を止めると、今度は摩擦の逃げ場がなくなり
**ターンする場所で張り付いて止まる。**

摩擦をゼロにすれば両方解決する。

> **プレイヤーと敵の両方に付ける。** 壁への張り付きも防げる。

---

## 4. Physics 2D の設定（任意）

**Edit → Project Settings → Physics 2D**

| 項目 | 推奨 | 理由 |
|---|---|---|
| Gravity Y | -9.81（既定） | 触らなくてよい |
| Default Contact Offset | 0.01（既定） | 触らなくてよい |

**基本は既定のままでよい。** 触るとかえって不具合の原因になる。

---

## 5. Game ビューの解像度

**Game ビュー上部の解像度ドロップダウン**を設定する。

1. ドロップダウン下部の **`+`** を押す
2. Label: `Fixed 1920x1080`
3. Type: **`Fixed Resolution`**
4. Width: **1920** / Height: **1080**

さらに **Scale スライダーを一番左（1x）** に。

### なぜ必要か

`Free Aspect` だとウィンドウサイズがそのまま解像度になり、
**731×411 のような奇数解像度**になることがある。

奇数解像度では**ドット絵のタイルの境界に隙間**が出る。
Pixel Perfect Camera を使う場合は警告も出る。

---

## 6. ビルド設定（シーン作成後）

**File → Build Settings（Unity 6 は Build Profiles → Scene List）**

シーンを4つ作り、**すべて登録する。**

| 順 | シーン名 |
|---|---|
| 0 | `Title` |
| 1 | `Game` |
| 2 | `Clear` |
| 3 | `GameOver` |

> **シーン名は大文字小文字まで正確に。** `SceneLoader.cs` の定数と一致させる。
> **登録漏れがあるとシーン遷移が必ずエラーになる。**

---

## 7. TextMeshPro の導入

**Window → TextMeshPro → Import TMP Essential Resources**

### なぜ必要か

`ScoreUI.cs` が TextMeshPro を使っている。
未導入だと `The type or namespace name 'TMPro' could not be found` というエラーが出る。

初回に一度だけ実行すればよい。

### 日本語を使う場合

TextMeshPro は初期状態で日本語フォントを持たないため **□（豆腐）** になる。

1. 日本語フォント（.ttf / .otf）を Assets に入れる
2. **Window → TextMeshPro → Font Asset Creator**
3. Source Font File にフォントを指定
4. **Character Set を `Custom Characters`** にして、使う文字だけ貼り付ける
5. Generate → Save し、Text の Font Asset に設定

> **Character Set を全文字にすると生成に数分〜十数分かかる。** 必ず使う文字だけに絞る。

**ライセンス注意**：フォントによっては再配布や埋め込みが禁止されている。
**Noto Sans JP や M PLUS などのオープンライセンス（SIL OFL）** を選べば安全。
Google Fonts から無料で入手できる。

> **当日は英数字のタイトルにするのが最も安全。**

---

## 8. スクリプトの配置

配布された `.cs` ファイル **15本** を Assets 内の `Scripts` フォルダに入れる。

| スクリプト | オブジェクトに付ける？ |
|---|---|
| PlayerController | ⭕ Player |
| PlayerHealth | ⭕ Player |
| EnemyPatrol | ⭕ Enemy |
| EnemyHealth | ⭕ Enemy |
| EnemyAttack | ⭕ Enemy |
| StompZone | ⭕ Enemy の子 |
| Goal | ⭕ Goal |
| DamageZone | ⭕ DeathZone / トゲ |
| **SceneLoader** | **❌ 置くだけ（static）** |
| SceneButton | ⭕ ボタン |
| HpUI | ⭕ Canvas |
| CameraFollow | ⭕ Main Camera |
| **ScoreManager** | **❌ 置くだけ（static）** |
| Coin | ⭕ コイン |
| ScoreUI | ⭕ Canvas |

> **`: MonoBehaviour` が付いているものだけオブジェクトに付けられる。**
> static クラスを付けようとすると
> `The script class can't be abstract!` というエラーが出るが、**これは正常。**

### 15本すべて入れること

C# は**1つでもエラーがあると全スクリプトが使えなくなる。**

たとえば `SceneLoader.cs` は `ScoreManager.ResetScore()` を呼んでいるので、
**`ScoreManager.cs` がないと `Can't add script` エラーになる。**

---

## 9. 設定チェックリスト

配布前に以下を確認する。

- [ ] Active Input Handling が **`Both`**
- [ ] Layer に `Ground` `Player` `Enemy` がある
- [ ] `NoFriction` マテリアルがある（Friction 0）
- [ ] Game ビューが **Fixed Resolution 1920×1080**
- [ ] TMP Essential Resources が導入済み
- [ ] スクリプト15本が入っていて **Console にエラーがない**
- [ ] BE2 アセットが導入済み（`SETUP_02` 参照）
- [ ] シーン4つ＋Build Settings 登録（作成後）

---

## 10. 配布時の注意

### プロジェクトごと配布する場合の問題

**Asset Store のライセンスは購入者アカウントに紐づく。**
アセットを含んだプロジェクトをそのまま配ると、厳密には規約違反になり得る。

**推奨：アセットを除いた状態で配布し、参加者各自に導入してもらう。**
（手順は `SETUP_02_アセットの導入.md`）

### 容量

大型アセットを入れるとプロジェクトが数GBになる。
USBメモリで配る場合は容量に注意。

**`Library` フォルダは配布不要**（Unity が自動生成する）。
削除すると容量が大幅に減る。
