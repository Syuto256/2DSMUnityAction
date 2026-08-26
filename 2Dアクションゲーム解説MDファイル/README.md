# 2Dアクションゲーム 解説資料

Unity初心者向け合同制作（2日間・約9時間）用のスクリプト解説集。

---

## ファイル一覧

### セットアップ（当日より前に読む）

| ファイル | 内容 | 誰が |
|---|---|---|
| **`SETUP_01_プロジェクト初期設定.md`** | Layer・入力方式・マテリアル等 | **講師（事前）** |
| **`SETUP_02_アセットの導入.md`** | BE2 の入手と画像設定 | **参加者各自** |

### スクリプト解説

| ファイル | 内容 | 密度 |
|---|---|---|
| **`00_共通の基礎知識.md`** | 全スクリプト共通の知識。**最初に読む** | 詳細 |
| `01_PlayerController.md` | 移動・ジャンプ・接地判定 | 詳細 |
| `02_PlayerHealth.md` | HP・無敵時間・コルーチン | 詳細 |
| `03_EnemyPatrol.md` | 敵の往復移動 | 簡易 |
| `04_敵の撃破処理.md` | EnemyHealth / EnemyAttack / StompZone | 簡易 |
| `05_Goal.md` | ゴール判定（**最も短い。最初に読ませるのに最適**） | 簡易 |
| `06_DamageZone.md` | 穴・トゲ | 簡易 |
| `07_SceneLoader.md` | シーン遷移 / SceneButton | 簡易 |
| `08_HpUI.md` | HP表示 | 簡易 |
| `09_CameraFollow.md` | カメラ追従 | 簡易 |
| `10_スコア機能.md` | ScoreManager / Coin / ScoreUI | 簡易 |
| `_template.md` | 新規追加用の雛形 | — |

---

## 読む順番

### 教える側の準備として

1. `00_共通の基礎知識.md`
2. `01_PlayerController.md` ← **ここが理解できれば全体の6割**
3. `02_PlayerHealth.md`
4. 残りは必要に応じて

### 参加者に読ませるなら

1. `05_Goal.md` ← 10行程度でトリガー判定の型が全部入っている
2. `01_PlayerController.md`

---

## スクリプトの3パターン

このゲームのスクリプトは3種類しかない。

| パターン | 内容 | 該当 |
|---|---|---|
| **1. 自分だけで完結** | 毎フレーム自分を動かす | PlayerController, EnemyPatrol |
| **2. ぶつかった相手を呼ぶ** | `OnTriggerEnter2D` など | Goal, DamageZone, Coin, StompZone, EnemyAttack |
| **3. 相手を見に行くだけ** | 毎フレーム読むだけ | HpUI, CameraFollow, ScoreUI |

**スクリプトを読むときは、まず処理の入り口を探す。**

| 起点 | メソッド |
|---|---|
| 毎フレーム | `Update()` / `FixedUpdate()` |
| ぶつかった | `OnTriggerEnter2D()` など |
| 押された | ボタンの `OnClick` |

---

## 当日の最重要チェックリスト

以下は**設定を忘れてもエラーが出ない**ため、原因不明の不具合になる。

- [ ] Active Input Handling が **`Both`**（Project Settings → Player）
- [ ] 地面の Layer が **`Ground`**
- [ ] PlayerController の **Ground Layer** で Ground にチェック
- [ ] Rigidbody2D の **Freeze Rotation Z**
- [ ] 敵とプレイヤーに **NoFriction マテリアル**
- [ ] Tilemap に **Composite Collider 2D**（Used By Composite にチェック）
- [ ] Build Settings に **4シーンすべて登録**
- [ ] 「触れたら何か起きる」ものは **Is Trigger にチェック**
- [ ] Animator の遷移で **Has Exit Time のチェックを外す**
- [ ] Any State → Jump の **Can Transition To Self を外す**
- [ ] Animator のパラメータ名は **大文字小文字まで一致**

---

## 実際に遭遇したトラブル

当日も再発する可能性が高いもの。

| 症状 | 原因 | 参照 |
|---|---|---|
| プレイヤーが勝手に飛ぶように見える | Jump Power 過大 | `01` |
| タイルの隙間が見える | 画像設定 / Game ビュー解像度が奇数 | — |
| Animator が空で `+` が押せない | Animator Controller 未作成 | — |
| 敵が90度倒れる／ターンで止まる | 摩擦 | `03` |
| `Can't add script` | 他スクリプトのコンパイルエラー | `10` |
| `script class can't be abstract` | static クラスを付けようとした | `07` Q1 |
