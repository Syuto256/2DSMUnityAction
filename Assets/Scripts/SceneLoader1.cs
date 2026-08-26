using UnityEngine.SceneManagement;

// ============================================================
// SceneLoader
// シーンの切り替えをまとめて管理します。
//
// 【重要】
//  このクラスはオブジェクトに付けません（static クラスのため）。
//  他のスクリプトから SceneLoader.LoadClear(); のように直接呼びます。
//
// 【セットアップ手順】
//  1. 下の SceneName に書いた名前と、実際のシーン名を一致させる
//  2. File → Build Profiles（またはBuild Settings）で
//     4つのシーンをすべてリストに追加する
//     → これを忘れると「シーンが読み込めない」エラーになります
// ============================================================

public static class SceneLoader
{
    // --- シーン名をここで一括管理する ---
    // 名前を変えたいときは、ここだけ直せば全部に反映されます
    public const string TitleScene = "Title";
    public const string GameScene = "Game";
    public const string ClearScene = "Clear";
    public const string GameOverScene = "GameOver";

    public static void LoadTitle()
    {
        Load(TitleScene);
    }

    public static void LoadGame()
    {
        // ゲームを始めるときはスコアを 0 に戻す
        // （リトライのときも通るので、ここに書けば1箇所で済みます）
        ScoreManager.ResetScore();
        Load(GameScene);
    }

    public static void LoadClear()
    {
        Load(ClearScene);
    }

    public static void LoadGameOver()
    {
        Load(GameOverScene);
    }

    // 今いるシーンをもう一度読み込む（リトライ用）
    public static void ReloadCurrentScene()
    {
        Load(SceneManager.GetActiveScene().name);
    }

    private static void Load(string sceneName)
    {
        // 時間を止めていた場合に備えて元に戻す
        UnityEngine.Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}
