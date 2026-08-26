# SceneLoader / SceneButton

> 前提知識は `00_共通の基礎知識.md` を参照。

---

## 1. 役割

| スクリプト | 役割 | オブジェクトに付ける？ |
|---|---|---|
| **SceneLoader** | シーン切り替えの本体 | **付けない**（static クラス） |
| **SceneButton** | UIボタンとの橋渡し | ボタンに付ける |

### なぜ2つに分かれているか

**Unity の UI ボタンは static メソッドを直接呼べないため。**

`SceneLoader` は static クラスなので、ボタンの On Click にドラッグできない。
その橋渡しをするのが `SceneButton`。

### なぜ名前が SceneManager ではないのか

**Unity 標準の `UnityEngine.SceneManagement.SceneManager` と衝突するため。**

同名にすると `SceneManager.LoadScene()` がエラーになり、
初心者が原因不明で止まる典型パターンになる。

---

## 2. セットアップ

### SceneLoader

**Assets フォルダに置いておくだけ。** オブジェクトには付けない。

```csharp
public static class ScoreManager       // ← 付けられない
public class Coin : MonoBehaviour      // ← 付けられる
```

**`: MonoBehaviour` が付いているかどうかが判断基準。**

### 必須の準備

1. **シーンを4つ作る**：`Title` `Game` `Clear` `GameOver`
   （**大文字小文字まで正確に**）
2. **File → Build Settings（Build Profiles）に4つとも登録**

> **登録を忘れるとシーン遷移が必ずエラーになる。** 最頻出。

### SceneButton

1. ボタンを作る（UI → Button - TextMeshPro）
2. そのボタンに `SceneButton` を付ける
3. Inspector の **On Click ()** の `+` を押す
4. 左の欄に**ボタン自身**をドラッグ
5. 右のドロップダウンから `SceneButton → OnClickGameStart()` などを選ぶ

> **「No Function」のままだと押しても何も起きない。** 最大の詰まりポイント。

### 各シーンのボタン割り当て

| シーン | 表示 | メソッド |
|---|---|---|
| Title | START | `OnClickGameStart()` |
| Clear | タイトルへ | `OnClickTitle()` |
| GameOver | リトライ | `OnClickRetry()` |

---

## 3. コード解説

### 3-1. シーン名を一箇所で管理

```csharp
public const string TitleScene = "Title";
public const string GameScene = "Game";
```

`const` は「**書き換えられない値**」。

**名前を変えたくなったら、ここだけ直せば全部に反映される。**
各所に文字列を直接書くと、変更漏れでバグる。

### 3-2. 共通処理をまとめる

```csharp
private static void Load(string sceneName)
{
    UnityEngine.Time.timeScale = 1f;   // 時間を止めていた場合に備える
    SceneManager.LoadScene(sceneName);
}
```

すべての遷移がこの1つを通る。
**「毎回やるべきこと」を1箇所に書ける**のが利点。

### 3-3. スコアのリセット

```csharp
public static void LoadGame()
{
    ScoreManager.ResetScore();
    Load(GameScene);
}
```

**スタートもリトライも `LoadGame()` を通る**ので、ここに書けば両方に効く。

> static の値はシーンをまたいでも消えない。
> リセットを忘れると前回のスコアが加算され続ける。

---

## 4. 出た質問と回答

### Q1. ScoreManager をオブジェクトに付けようとするとエラーが出る

**質問の原文（スコア機能の作業中に発生）**

> `Can't add script behaviour 'ScoreManager'. The script class can't be abstract!`

**回答**

**エラーではなく、正しい動作。**

`ScoreManager` も `SceneLoader` も **static クラス**なので、
オブジェクトに付けることができない。

**static クラスとは「世界に1つしかない道具箱」。**
実体を持たないので、オブジェクトに貼り付けるという概念がない。

`Physics2D` と同じ性質。

```csharp
Physics2D.OverlapCircle(...);   // オブジェクトに付けない
ScoreManager.AddScore(1);       // 同じく付けない
SceneLoader.LoadGame();         // 同じく付けない
```

**呼びたいときに、どこからでも名前を書けば使える。**
だからシーンに置く必要がない。

**見分け方**

```csharp
public static class ScoreManager                 // ← 付けられない
public class Coin : MonoBehaviour                // ← 付けられる
```

**`: MonoBehaviour` が付いているかどうか。**

**参加者への説明**

> **「`MonoBehaviour` って書いてあるスクリプトだけオブジェクトに付けられる。
> 書いてないやつは置くだけでOK」**

**当日も同じ質問が来ると思っておく。**

---

## 5. よくある詰まり

| 症状 | 原因 | 対処 |
|---|---|---|
| **`Scene couldn't be loaded`** | **Build Settings 登録漏れ** | 4シーンすべて登録 |
| 同上 | シーン名の綴り違い | 大文字小文字まで一致させる |
| **ボタンを押しても無反応** | On Click が No Function | 3-2 の手順を再確認 |
| ボタンが押せない | EventSystem が消えている | Canvas 作成時に自動生成されるもの |
| **script class can't be abstract** | static クラスを付けようとした | Q1 参照。付けなくてよい |

---

## 6. 教えるときのコツ

### 覚えてもらうルール

> **`MonoBehaviour` と書いてあるものだけオブジェクトに付ける。**

> **シーンを増やしたら Build Settings に登録する。**

この2つは当日確実に必要になる。

---

## 7. 追加の質問メモ

（未記入）
