using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static UnityMainThreadDispatcher _instance = null;
    private static readonly Queue<Action> _queue = new Queue<Action>();

    public static UnityMainThreadDispatcher Instance()
    {
        if (_instance == null)
        {
            throw new Exception("UnityMainThreadDispatcher not found. Add it to a GameObject in the scene.");
        }
        return _instance;
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public void Enqueue(Action action)
    {
        lock (_queue)
        {
            _queue.Enqueue(action);
        }
    }

    void Update()
    {
        lock (_queue)
        {
            while (_queue.Count > 0)
            {
                _queue.Dequeue().Invoke();
            }
        }
    }
}