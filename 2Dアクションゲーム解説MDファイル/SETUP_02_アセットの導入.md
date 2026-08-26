# アセットの導入（BE2）

> **参加者各自が自分の Unity ID で行う作業。**
> ライセンスは購入者アカウントに紐づくため、プロジェクトごとの配布は避ける。

使用アセット：**Simple 2D Platformer Assets Pack（BE2）**

---

## 1. Asset Store で入手する

### 手順

**① ブラウザで探す**

`assetstore.unity.com` にアクセスし、Unity ID でログインする。

> **Asset Store はブラウザ専用。** Unity 2020 以降、エディタ内のストアは廃止された。

**② マイアセットに追加**

アセットページで **Add to My Assets（マイアセットに追加）** を押し、
ライセンス条項に同意する。

> この時点ではまだダウンロードされていない。**アカウントに紐づいただけ。**

> **無料アセットでも後から有料化されることがある。**
> 一度マイアセットに追加しておけば、その後有料化されても無料のまま使い続けられる。
> 候補は早めに追加しておくのが有利。

**③ Package Manager を開く**

Unity エディタで **Window → Package Manager**

左上のドロップダウンから **`My Assets`** を選ぶ。
出てこない場合は右上の更新ボタンを押す。

**④ Download → Import**

対象のアセットを選び、右下の **Download** を押す。
完了すると **Import** に変わるので、それを押す。

**⑤ 取り込む中身を選ぶ**

`Import Unity Package` ウィンドウが開く。

| 項目 | 要否 | 内容 |
|---|---|---|
| **Sprites/** | **必須** | Player・Enemy・Platforms・Objects・Coins |
| Demo/Demo.unity | 推奨 | 完成見本。組み方の参考になる |
| ReadMe/ | **推奨** | **ライセンス表記の条件が書かれている** |
| App Icon.png | 不要 | ストア掲載用 |

**基本は全部チェックのまま Import で問題ない。** 軽量なパックのため。

> **ReadMe は必ず残して、Import 後に中身を読む。**
> クレジット表記が必須かどうかがここに書かれている。

---

## 2. 「Missing Signature」の警告について

Import 時に以下が出ることがある。

> `This project contains at least one package that doesn't have a signature.`

**Close を押して問題ない。**

これは「ウイルスを検知した」という警告ではなく、
**「Unity 公式の電子署名が付いていない」**という通知。
Asset Store の個人・小規模パブリッシャーの作品はほぼ全て署名なし。

### 補足

BE2 には `ReadmeEditorBE2.cs` というエディタ拡張スクリプトが含まれる。
エディタ拡張は Unity 上で実行されるコードなので、原理的には注意対象。

ただし Asset Store の審査を通っており、ReadMe を見やすく表示するだけの一般的なもの。

**気になる場合は `Scripts/Editor` のチェックを外せば回避できる。**
素材の利用には一切影響しない。

---

## 3. スプライトの設定

### BE2 は分割済み

**BE2 のスプライトは最初から Slice 済みで名前も付いている。**
`Player.png` を展開すると `Idle 0` `Run 0` などが並んでいる。

**手動での Slice 作業は不要。** 素材側の設定をそのまま使う。

### 確認しておく項目

各画像を選択し、Inspector で以下を確認する。

| 項目 | 値 | 理由 |
|---|---|---|
| **Filter Mode** | **`Point (no filter)`** | ドット絵がぼやけない |
| **Compression** | **`None`** | 色が滲まない |
| **Generate Mip Maps** | **チェックを外す** | **タイルの隙間の原因** |
| Wrap Mode | `Clamp` | 端の滲み防止 |
| Max Size | 元画像より大きい値 | 縮小されると隙間の原因 |

変更後は必ず **Apply** を押す。

> **Generate Mip Maps は見落とされやすい。**
> 有効だとカメラ距離に応じて縮小版に差し替わり、隣のタイルの色が滲み出す。
> 2Dのドット絵では不要な機能。

### Pixels Per Unit を統一する

**Player・Enemy・Platforms・Coins ですべて同じ値にする。**

揃っていないと**キャラだけ巨大／極小**になる。

**確認方法：`Demo.unity` を開いて Player を選択する。**
作者が意図したサイズ感がそのまま見られるので、その値に合わせるのが最も早い。

---

## 4. 素材の内訳

### Player.png（9コマ）

| アニメ | コマ | 枚数 |
|---|---|---|
| Idle | Idle 0〜1 | 2 |
| Jump | Jump 0〜2 | 3 |
| Run | Run 0〜3 | 4 |

**死亡用のスプライトはない。**
`Dead` トリガーに対応するアニメを作らなくてもエラーにはならない（Animator が無視する）。

### Coins.png（3種 × 4コマ）

**銅・銀・金の3種類、各4コマの回転アニメ。**

同じ `Coin` スクリプトで、点数だけ変えて作り分けられる。

| 種類 | 推奨 Score |
|---|---|
| 銅 | 1 |
| 銀 | 5 |
| 金 | 10 |

### その他

| ファイル | 用途 |
|---|---|
| `Platforms.png` | 地面・足場のタイル |
| `Enemy.png` | 敵 |
| `Objects.png` | 旗・トゲなどの小物 |

---

## 5. Tile Palette の作成

**Window → 2D → Tile Palette**

1. **Create New Palette** で新規作成し、保存先フォルダを指定
2. `Platforms.png` の分割済みスプライトを**パレットへドラッグ**
3. Tile アセットが自動生成される

### Tile Palette が開けない場合

**`2D Tilemap Editor` パッケージが必要。**
2Dテンプレートで作成していれば導入済み。
なければ Package Manager の Unity Registry から追加する。

---

## 6. 自分で用意した画像を使う場合

**Project ウィンドウの Assets フォルダに直接ドラッグ&ドロップするだけ。**

その後は「3. スプライトの設定」以降を同様に行う。

> エクスプローラーから直接コピーしても認識されるが、
> Unity 起動中は反映が遅れることがある。エディタ上へのドラッグを推奨。

---

## 7. トラブル対処

| 症状 | 原因 | 対処 |
|---|---|---|
| **タイルに隙間が見える** | Generate Mip Maps が有効 | チェックを外す |
| 同上 | Game ビューが奇数解像度 | Fixed Resolution 1920×1080 に |
| 同上 | アンチエイリアス | Project Settings → Quality で Disabled |
| **キャラだけ巨大／極小** | Pixels Per Unit 不一致 | 全素材で統一 |
| 絵がぼやける | Filter Mode | `Point (no filter)` に |
| マテリアルがピンク色 | パイプライン不一致 | 2Dスプライトなら無視してよい |
| Import が終わらない | 大型アセット | **事前に済ませておく** |
| Tile Palette が開けない | パッケージ未導入 | 2D Tilemap Editor を追加 |

### タイルの隙間が消えない場合

上から順に試す。**多くは①か②で解決する。**

1. Game ビューを **Fixed Resolution 1920×1080**、Scale を 1x に
2. **Generate Mip Maps を外す** / Filter Mode を Point / Compression を None
3. Project Settings → Quality → **Anti Aliasing を Disabled**
4. Main Camera に **Pixel Perfect Camera** を追加
   - Assets Pixels Per Unit をスプライトと一致させる
   - Reference Resolution を `320×180` など 16:9 の整数比に
   - **カメラの Size が上書きされるので、CameraFollow の Min/Max を再調整すること**
5. **Sprite Atlas** を作り、Padding を 4 以上に

---

## 8. ライセンス・費用の注意

### 金銭面

- **BE2 は無料アセット。追加費用は発生しない。**
- ただし**無料アセットは後から有料化されることがある。**
  早めにマイアセットに追加しておけば、以後も無料で使い続けられる。

### ライセンス

- **無料でもクレジット表記が必須なものがある。**
  ページの Description とライセンス欄、Import した ReadMe を必ず確認する。
- 多くのアセットは「**ゲームに組み込んで配布はOK、素材単体の再配布はNG**」。
- **Asset Store のライセンスは購入者アカウントに紐づく。**
  厳密には参加者それぞれが自分のアカウントで入手する必要がある。
  **プロジェクトファイルごと配布するのはグレー。**

### 成果物を公開する場合

- クレジット表記が必要なら、タイトル画面などに記載する
- 再配布条件を再確認する

### 容量

大型アセットを入れるとプロジェクトが数GBになる。ストレージに余裕を持たせる。

---

## 9. 事前準備チェックリスト

- [ ] Unity ID でログインし、BE2 を **マイアセットに追加**
- [ ] Package Manager から **Download → Import**
- [ ] **ReadMe を読んでライセンス条件を確認**
- [ ] 全スプライトの **Filter Mode / Compression / Mip Maps** を設定
- [ ] **Pixels Per Unit を統一**（Demo シーンで正解値を確認）
- [ ] Tile Palette を作成
- [ ] タイルの隙間が出ないことを確認

> **Import は数分〜10分かかることがある。**
> 低スペックPCでは固まったように見えるため、**当日ではなく事前に済ませておく。**
