using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace MiniIT.ARKANOID
{
    public class BallLauncher : MonoBehaviour
    {
        private const float MinimumLaunchUpwardComponent = 0.35f;
        private const float DefaultAimLineLength = 2.5f;
        private const float DefaultAimLineWidth = 0.08f;

        [SerializeField]
        private Ball ball = null;

        [SerializeField]
        private Vector2 attachOffset = Vector2.zero;

        [SerializeField]
        private LineRenderer aimLine = null;

        [SerializeField]
        private float aimLineLength = DefaultAimLineLength;

        [SerializeField]
        private float aimLineWidth = DefaultAimLineWidth;

        [SerializeField]
        private Color aimLineColor = new Color(1.0f, 1.0f, 1.0f, 0.9f);

        private IInputService inputService = null;
        private SignalBus signalBus = null;
        private BallCoordinator ballCoordinator = null;
        private bool awaitingLaunch = false;
        private bool isMazeActive = false;
        private Material runtimeAimLineMaterial = null;

        [Inject]
        public void Construct(IInputService inputService, SignalBus signalBus, BallCoordinator ballCoordinator)
        {
            this.inputService = inputService;
            this.signalBus = signalBus;
            this.ballCoordinator = ballCoordinator;
        }

        private void Awake()
        {
            EnsureAimLine();
            SetAimLineVisible(false);
        }

        private void OnEnable()
        {
            SubscribeSignals();
        }

        private void OnDisable()
        {
            SetAimLineVisible(false);
            UnsubscribeSignals();
        }

        private void OnDestroy()
        {
            if (runtimeAimLineMaterial != null)
            {
                Destroy(runtimeAimLineMaterial);
            }
        }

        private void Start()
        {
            ballCoordinator?.SetPrimaryBall(ball);
            ResetBallToLaunchPoint();
        }

        private void Update()
        {
            if (!awaitingLaunch)
            {
                return;
            }

            if (isMazeActive)
            {
                SetAimLineVisible(false);
                return;
            }

            UpdateAimLine();

            if (inputService != null && inputService.IsLaunchRequested())
            {
                LaunchBall();
            }
        }

        private void ResetBallToLaunchPoint()
        {
            if (ball == null)
            {
                return;
            }

            awaitingLaunch = true;
            ball.Stop();
            ball.ResetPosition(GetLaunchPosition());
            UpdateAimLine();
        }
        private IEnumerator DestObj()
        {
            yield return new WaitForSeconds(2);
            ball.ResetPosition(GetLaunchPosition());
        }


        private void LaunchBall()
        {
            if (ball == null)
            {
                return;
            }

            awaitingLaunch = false;
            SetAimLineVisible(false);

            Vector2 launchDirection = ResolveLaunchDirection();
            ball.Launch(launchDirection);
        }

        private void OnBallLost()
        {
            ResetBallToLaunchPoint();
        }

        private void OnLevelReset()
        {
            ResetBallToLaunchPoint();
        }

        private void SubscribeSignals()
        {
            if (signalBus == null)
            {
                return;
            }

            signalBus.Subscribe<BallLostSignal>(OnBallLost);
            signalBus.Subscribe<LevelResetSignal>(OnLevelReset);
            signalBus.Subscribe<MazeStartedSignal>(OnMazeStarted);
            signalBus.Subscribe<MazeCompletedSignal>(OnMazeEnded);
            signalBus.Subscribe<MazeFailedSignal>(OnMazeEnded);
        }

        private void UnsubscribeSignals()
        {
            if (signalBus == null)
            {
                return;
            }

            signalBus.Unsubscribe<BallLostSignal>(OnBallLost);
            signalBus.Unsubscribe<LevelResetSignal>(OnLevelReset);
            signalBus.Unsubscribe<MazeStartedSignal>(OnMazeStarted);
            signalBus.Unsubscribe<MazeCompletedSignal>(OnMazeEnded);
            signalBus.Unsubscribe<MazeFailedSignal>(OnMazeEnded);
        }

        private void OnMazeStarted()
        {
            isMazeActive = true;
            SetAimLineVisible(false);
        }

        private void OnMazeEnded()
        {
            isMazeActive = false;

            if (awaitingLaunch)
            {
                UpdateAimLine();
            }
        }

        private Vector2 GetLaunchPosition()
        {
            return (Vector2)transform.position + attachOffset;
        }

        private Vector2 ResolveLaunchDirection()
        {
            Vector2 inputDirection = inputService != null
                ? inputService.GetMoveInput()
                : Vector2.zero;

            if (inputDirection.sqrMagnitude <= Mathf.Epsilon)
            {
                return Vector2.up;
            }

            Vector2 launchDirection = inputDirection.normalized;
            if (launchDirection.y < MinimumLaunchUpwardComponent)
            {
                launchDirection.y = MinimumLaunchUpwardComponent;
            }

            return launchDirection.normalized;
        }

        private void UpdateAimLine()
        {
            if (!awaitingLaunch || isMazeActive || ball == null || aimLine == null)
            {
                SetAimLineVisible(false);
                return;
            }

            Vector3 startPoint = ball.transform.position;
            Vector3 endPoint = startPoint + (Vector3)(ResolveLaunchDirection() * Mathf.Max(aimLineLength, 0.0f));

            aimLine.SetPosition(0, startPoint);
            aimLine.SetPosition(1, endPoint);
            SetAimLineVisible(true);
        }

        private void EnsureAimLine()
        {
            if (aimLine != null)
            {
                ConfigureAimLine(aimLine);
                return;
            }

            GameObject aimLineObject = new GameObject("AimLine");
            aimLineObject.transform.SetParent(transform, false);

            aimLine = aimLineObject.AddComponent<LineRenderer>();
            ConfigureAimLine(aimLine);
        }

        private void ConfigureAimLine(LineRenderer lineRenderer)
        {
            if (lineRenderer == null)
            {
                return;
            }

            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;
            lineRenderer.alignment = LineAlignment.View;
            lineRenderer.textureMode = LineTextureMode.Stretch;
            lineRenderer.numCapVertices = 6;
            lineRenderer.numCornerVertices = 2;
            lineRenderer.startWidth = aimLineWidth;
            lineRenderer.endWidth = aimLineWidth;
            lineRenderer.startColor = aimLineColor;
            lineRenderer.endColor = aimLineColor;
            lineRenderer.sortingOrder = 10;

            if (lineRenderer.sharedMaterial == null)
            {
                Shader spriteShader = Shader.Find("Sprites/Default");
                if (spriteShader != null)
                {
                    runtimeAimLineMaterial = new Material(spriteShader);
                    lineRenderer.material = runtimeAimLineMaterial;
                }
            }
        }

        private void SetAimLineVisible(bool isVisible)
        {
            if (aimLine == null)
            {
                return;
            }

            aimLine.enabled = isVisible;
        }
    }
}
