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
        private bool isMazeActive = false;
        
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
        }

        private void OnEnable()
        {
            if (signalBus == null)
            {
                return;
            }

            signalBus.Subscribe<LevelResetSignal>(ResetPosition);
            signalBus.Subscribe<MazeStartedSignal>(OnMazeStarted);
            signalBus.Subscribe<MazeCompletedSignal>(OnMazeEnded);
            signalBus.Subscribe<MazeFailedSignal>(OnMazeEnded);
        }

        private void OnDisable()
        {
            if (signalBus == null)
            {
                return;
            }

            signalBus.Unsubscribe<LevelResetSignal>(ResetPosition);
            signalBus.Unsubscribe<MazeStartedSignal>(OnMazeStarted);
            signalBus.Unsubscribe<MazeCompletedSignal>(OnMazeEnded);
            signalBus.Unsubscribe<MazeFailedSignal>(OnMazeEnded);
        }

        private void FixedUpdate()
        {
            if (inputService == null || isMazeActive)
            {
                return;
            }

            float input = inputService.GetMoveInput().x;

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

        private void OnMazeStarted()
        {
            isMazeActive = true;
        }

        private void OnMazeEnded()
        {
            isMazeActive = false;
        }
    }
}
