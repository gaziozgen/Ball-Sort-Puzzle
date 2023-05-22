using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FateGames.Core
{
    public class FateMonoBehaviour : MonoBehaviour
    {
        private Transform _transform = null;
#pragma warning disable CS0108
        public Transform transform
#pragma warning restore CS0108
        {
            get
            {
                if (_transform == null)
                    _transform = base.transform;
                return _transform;
            }
        }
        public void Activate()
        {
            gameObject.SetActive(true);
            OnActivated.Invoke();
        }
        public void Deactivate()
        {
            gameObject.SetActive(false);
            OnDeactivated.Invoke();
        }
        public UnityEvent OnActivated { get; private set; } = new();
        public UnityEvent OnDeactivated { get; private set; } = new();
    }

}
