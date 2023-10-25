using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
    private VideoPlayer player;

    void Awake()
    {
        player = GetComponent<VideoPlayer>();

        player.loopPointReached += LoadNextScene;
        player.targetTexture.Release();
    }

    private void Start()
    {
        GlobalGameManager.Instance.LoadSceneIn((float)player.clip.length, 1);
    }

    private void LoadNextScene(VideoPlayer player)
    {
        //SceneManager.LoadScene(1);
    }
}
