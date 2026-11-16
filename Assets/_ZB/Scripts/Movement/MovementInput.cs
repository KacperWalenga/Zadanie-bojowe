using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ZB.Maze
{
    public class MovementInput : MonoBehaviour
    {
        private const float MinDirectionMagnitudeSquared = 0.5f;
        private const float MinStrafeMagnitude = 0.5f;

        [SerializeField] private Movement movement;
        [SerializeField] private InputActionReference move;
        [SerializeField] private InputActionReference strafe;

        private readonly List<InputAction> enabledActions = new();

        private static void Unsubscribe(InputAction action, Action<InputAction.CallbackContext> callback)
        {
            if (action != null)
                action.performed -= callback;
        }

        private void OnEnable()
        {
            Subscribe(move.action, OnMove);
            Subscribe(strafe.action, OnStrafe);
        }

        private void OnDisable()
        {
            Unsubscribe(move.action, OnMove);
            Unsubscribe(strafe.action, OnStrafe);

            foreach (var action in enabledActions)
                action.Disable();

            enabledActions.Clear();
        }

        private void Subscribe(InputAction action, Action<InputAction.CallbackContext> callback)
        {
            action.performed += callback;

            if (action.enabled)
                return;

            action.Enable();
            enabledActions.Add(action);
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            var direction = context.ReadValue<Vector2>();

            if (direction.sqrMagnitude < MinDirectionMagnitudeSquared)
                return;

            if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x))
            {
                if (direction.y > 0f)
                    movement.MoveForward();
                else
                    movement.MoveBackward();

                return;
            }

            if (direction.x > 0f)
                movement.TurnRight();
            else
                movement.TurnLeft();
        }

        private void OnStrafe(InputAction.CallbackContext context)
        {
            var horizontal = context.ReadValue<Vector2>().x;

            if (Mathf.Abs(horizontal) < MinStrafeMagnitude)
                return;

            if (horizontal < 0f)
                movement.MoveLeft();
            else
                movement.MoveRight();
        }
    }
}
