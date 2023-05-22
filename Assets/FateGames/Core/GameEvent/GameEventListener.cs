using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FateGames.Core
{
    public class GameEventListener : MonoBehaviour
    {
        [SerializeField] private GameEventTuple gameEventTuple;

        private void OnEnable()
        {
            for (int i = 0; i < gameEventTuple.Events.Length; i++)
            {
                GameEvent gameEvent = gameEventTuple.Events[i];
                gameEvent.RegisterListener(this);
            }
        }
        private void OnDisable()
        {
            for (int i = 0; i < gameEventTuple.Events.Length; i++)
            {
                GameEvent gameEvent = gameEventTuple.Events[i];
                gameEvent.UnregisterListener(this);
            }
        }
        public void OnEventRaised() => gameEventTuple.Response.Invoke();
    }

    [System.Serializable]
    public class GameEventTuple
    {
        [SerializeField] private GameEvent[] events = new GameEvent[0];
        [SerializeField] private UnityEvent response;

        public GameEvent[] Events { get => events; }
        public UnityEvent Response { get => response; }
    }

}