using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class SceneController : Singleton<SceneController>
{
    [SerializeField] private GameObject playerPrefab;
    private GameObject player;
    NavMeshAgent playerAgent;
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
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
        if(SceneManager.GetActiveScene().name != sceneName)
        {
            yield return SceneManager.LoadSceneAsync(sceneName);
            yield return Instantiate(playerPrefab, GetDestination(destinationTag).transform.position, GetDestination(destinationTag).transform.rotation);
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
}
