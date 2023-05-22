using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FateGames.Core
{
    public abstract class Variable<T> : ScriptableObject
    {
        [SerializeField] private T value = default;
        public T Value
        {
            get => value;
            set
            {
                T previousValue = this.value;
                this.value = value;
                OnValueChanged.Invoke(previousValue, value);
            }
        }
        [SerializeField] public UnityEvent<T, T> OnValueChanged;
    }
}
