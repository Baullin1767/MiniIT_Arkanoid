using System;
using Data;
using UnityEngine;
using Zenject;

namespace MiniIT.ARKANOID
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    public class Paddle : MonoBehaviour
    {
        private float speed;

        private float limitX;

        [SerializeField]
        private Rigidbody2D body = null;

        private IInputService inputService = null;
        private SignalBus signalBus = null;
        
        private Vector2 basePosition = new Vector2(0, 0);

        [Inject]
        public void Construct(IInputService inputService, SignalBus signalBus, GameSettings gameSettings)
        {
            this.inputService = inputService;
            this.signalBus = signalBus;
            
            speed = gameSettings.speed;
            limitX = gameSettings.limitX;
        }

        private void Awake()
        {
            if (body == null)
            {
                body = GetComponent<Rigidbody2D>();
                basePosition = body.position;
            }
            signalBus.Subscribe<LevelResetSignal>(ResetPosition);
        }

        private void FixedUpdate()
        {
            if (inputService == null)
            {
                return;
            }

            float input = inputService.GetMovement();

            Move(input);
        }

        private void Move(float input)
        {
            if (body == null)
            {
                return;
            }

            Vector2 position = body.position;

            position.x += input * speed * Time.fixedDeltaTime;
            position.x = Mathf.Clamp(position.x, -limitX, limitX);

            body.MovePosition(position);
        }

        private void ResetPosition()
        {
            body.position = basePosition;
        }
    }
}
