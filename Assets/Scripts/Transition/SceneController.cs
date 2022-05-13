using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class SceneController : Singleton<SceneController>, IEndGameObserver
{
    [SerializeField] private GameObject playerPrefab;
    private GameObject player;
    NavMeshAgent playerAgent;
    [SerializeField] private SceneFader fadePrefab;
    bool fadeFinished;
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
        fadeFinished = true;
    }
    void Start()
    {
        GameManager.Instance.AddObserver(this);
    }
    public void TransitionToDestination(TransitionPoint transitionPoint)
    {
        if(transitionPoint.transitionType == TransitionPoint.TransitionType.SameScene)
        {
            StartCoroutine(Transition(SceneManager.GetActiveScene().name, transitionPoint.destinationTag));
        } 
        else if (transitionPoint.transitionType == TransitionPoint.TransitionType.DifferentScene)
        {
            StartCoroutine(Transition(transitionPoint.sceneName, transitionPoint.destinationTag));
        }
    }
    IEnumerator Transition(string sceneName, TransitionDestination.DestinationTag destinationTag)
    {
        // todo: ±£´æÊý¾Ý
        SaveManager.Instance.SavePlayerData();
        if(SceneManager.GetActiveScene().name != sceneName)
        {
            yield return SceneManager.LoadSceneAsync(sceneName);
            yield return Instantiate(playerPrefab, GetDestination(destinationTag).transform.position, GetDestination(destinationTag).transform.rotation);
            SaveManager.Instance.LoadPlayerData();
            yield break;
        } 
        else
        {
            player = GameManager.Instance.playerStats.gameObject;
            playerAgent = player.GetComponent<NavMeshAgent>();
            playerAgent.enabled = false;
            player.transform.SetPositionAndRotation(GetDestination(destinationTag).transform.position, GetDestination(destinationTag).transform.rotation);
            playerAgent.enabled = true;
            yield return null;
        }
    }
    private TransitionDestination GetDestination (TransitionDestination.DestinationTag destinationTag)
    {
        var ent = FindObjectsOfType<TransitionDestination>();
        foreach (var e in ent)
        {
            if(e.destinationTag == destinationTag)
            {
                return e;
            }
        }
        return null;
    }
    public void TransitionToFirstLevel()
    {
        StartCoroutine(LoadLevel("Level1"));
    }
    public void TransitionToLoadGame()
    {
        StartCoroutine(LoadLevel(SaveManager.Instance.SceneName));
    }
    public void TransitionToMain()
    {
        StartCoroutine(LoadMain());
    }
    IEnumerator LoadLevel(string sceneName)
    {
        SceneFader fade = Instantiate(fadePrefab);
        if (!string.IsNullOrEmpty(sceneName))
        {
            yield return StartCoroutine(fade.FadeOut(2.5f));
            yield return SceneManager.LoadSceneAsync(sceneName);
            yield return Instantiate(playerPrefab, GameManager.Instance.GetEntrance().position, GameManager.Instance.GetEntrance().rotation);
            SaveManager.Instance.SavePlayerData();
            yield return StartCoroutine(fade.FadeIn(2.5f));
            yield break;
        }
    }
    IEnumerator LoadMain()
    {
        SceneFader fade = Instantiate(fadePrefab);
        yield return StartCoroutine(fade.FadeOut(2.5f));
        yield return SceneManager.LoadSceneAsync("Main");
        yield return StartCoroutine(fade.FadeIn(2.5f));
        yield break;
    }

    public void EndNotify()
    {
        if(fadeFinished)
        {
            fadeFinished = false;
            StartCoroutine(LoadMain());
        }
    }
}
