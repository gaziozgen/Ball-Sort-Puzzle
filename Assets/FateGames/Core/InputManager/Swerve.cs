using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace FateGames.Core
{
    public class Swerve : MonoBehaviour
    {
        [SerializeField] protected int Size;
        public Vector2 AnchorPosition { get; protected set; } = Vector2.zero;
        public Vector2 MousePosition { get; protected set; } = Vector2.zero;
        public Vector2 Difference { get => MousePosition - AnchorPosition; }
        public float Distance { get => Difference.magnitude; }
        public float Rate { get => Distance / Size; }
        public float XRate { get => Difference.x / Size; }
        public float YRate { get => Difference.y / Size; }
        public readonly UnityEvent OnStart = new();
        public readonly UnityEvent OnSwerve = new();
        public readonly UnityEvent OnRelease = new();


        protected virtual void OnMouseButtonDown(InputAction.CallbackContext context)
        {
            print("Touch Down");
            MousePosition = Mouse.current.position.ReadValue();
            AnchorPosition = MousePosition;
            OnStart.Invoke();
        }

        protected virtual void OnMouseButton(InputAction.CallbackContext context)
        {
            print("Touch");
            Vector2 mousePosition = context.ReadValue<Vector2>();
            Vector2 direction = (mousePosition - AnchorPosition).normalized;
            MousePosition = AnchorPosition + direction * Mathf.Clamp((mousePosition - AnchorPosition).magnitude, 0, Size);
            OnSwerve.Invoke();
        }

        protected virtual void OnMouseButtonUp(InputAction.CallbackContext context)
        {
            print("Touch Up");
            OnRelease.Invoke();
        }

    }
}

