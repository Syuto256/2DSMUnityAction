using UnityEngine;

// ============================================================
// SceneButton
// UIボタンからシーンを切り替えるためのスクリプトです。
//
// 【なぜ必要？】
//  SceneLoader は static クラスなので、ボタンの OnClick に
//  直接ドラッグできません。その橋渡しをするのがこのスクリプトです。
//
// 【セットアップ手順】
//  1. ボタンを作る（Hierarchy → UI → Button）
//  2. そのボタンにこのスクリプトを付ける
//  3. ボタンの Inspector → On Click () の「+」を押す
//  4. 左の欄にボタン自身をドラッグ
//  5. 右のドロップダウンから SceneButton → 使いたいメソッドを選ぶ
// ============================================================

public class SceneButton : MonoBehaviour
{
    // ボタンの OnClick から選べるようにするため、すべて public にします

    public void OnClickTitle()
    {
        SceneLoader.LoadTitle();
    }

    public void OnClickGameStart()
    {
        SceneLoader.LoadGame();
    }

    public void OnClickRetry()
    {
        SceneLoader.LoadGame();
    }

    public void OnClickQuit()
    {
        Debug.Log("ゲーム終了（エディタ上では終了しません）");
        Application.Quit();
    }
}
