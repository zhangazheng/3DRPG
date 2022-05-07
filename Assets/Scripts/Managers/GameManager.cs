using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public CharacterStats playerStats;
    List<IEndGameObserver> endGameObservers = new List<IEndGameObserver>();
    public void RegisterPlayer(CharacterStats stats)
    {
        playerStats = stats;
    }
    public void AddObserver(IEndGameObserver observer)
    {
        endGameObservers.Add(observer);
    }
    public void RemoveObserver(IEndGameObserver observer)
    {
        endGameObservers.Remove(observer);
    }
    public void NotifyObservers()
    {
        foreach (var ob in endGameObservers)
        {
            ob.EndNotify();
        }
    }
}
