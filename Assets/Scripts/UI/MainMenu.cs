using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    Button newGame, continueGame, quitGame;
    PlayableDirector director;
    private void Awake()
    {
        newGame = transform.GetChild(1).GetComponent<Button>();
        continueGame = transform.GetChild(2).GetComponent<Button>();
        quitGame = transform.GetChild(3).GetComponent<Button>();
        director = FindObjectOfType<PlayableDirector>();
        director.stopped += (v) => NewGame();

        newGame.onClick.AddListener(PlayTimeline);
        continueGame.onClick.AddListener(ContinueGame);
        quitGame.onClick.AddListener(QuitGame);
    }

    private void QuitGame()
    {
        Application.Quit();
    }

    private void ContinueGame()
    {
        SceneController.Instance.TransitionToLoadGame();
    }

    void PlayTimeline()
    {
        director.Play();
    }
    void NewGame()
    {
        PlayerPrefs.DeleteAll();
        SceneController.Instance.TransitionToFirstLevel();
    }
}
