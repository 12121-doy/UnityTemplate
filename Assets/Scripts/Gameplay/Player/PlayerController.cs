using Guisha.Core;
using Guisha.Framework;
using UnityEngine;
namespace Guisha.Gameplay
{
    [RequireComponent(typeof(CharacterController), typeof(PlayerStatus))]
    public sealed class PlayerController : MonoBehaviour
    {
        public Transform cameraTransform;
        [SerializeField] private float walkSpeed = 3, runSpeed = 5, crouchSpeed = 1.5f, gravity = -20;
        [SerializeField] private LayerMask ceilingMask = ~0;
        private CharacterController controller;
        private PlayerStatus status;
        private PlayerEntity entity;
        private float vertical, standingHeight, nextStep;
        private Vector3 standingCenter;
        private bool crouched;
        public bool IsCrouched => crouched;
        private void Awake()
        {
            controller = GetComponent<CharacterController>(); status = GetComponent<PlayerStatus>(); entity = GetComponent<PlayerEntity>();
            standingHeight = controller.height; standingCenter = controller.center;
        }
        private void Update()
        {
            Cursor.lockState = GameSession.InputEnabled ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !GameSession.InputEnabled;
            if (!GameSession.InputEnabled || !entity.IsAlive) return;
            if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
            bool wantsCrouch = Input.GetKey(KeyCode.LeftControl);
            if (wantsCrouch != crouched)
            {
                bool blocked = false;
                if (!wantsCrouch)
                {
                    var bottom = transform.position + standingCenter + Vector3.down * (standingHeight * 0.5f - controller.radius);
                    var top = bottom + Vector3.up * (standingHeight - 2 * controller.radius);
                    foreach (var hit in Physics.OverlapCapsule(bottom, top, controller.radius * 0.95f, ceilingMask, QueryTriggerInteraction.Ignore))
                        if (!hit.transform.IsChildOf(transform)) { blocked = true; break; }
                }
                if (!blocked)
                {
                    crouched = wantsCrouch; controller.height = standingHeight * (crouched ? 0.55f : 1);
                    controller.center = standingCenter - Vector3.up * (standingHeight - controller.height) * 0.5f;
                }
            }
            Vector3 input = Vector3.ClampMagnitude(new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")), 1);
            Vector3 forward = cameraTransform != null ? MathUtility.Planar(cameraTransform.forward).normalized : Vector3.forward;
            Vector3 right = Vector3.Cross(Vector3.up, forward);
            Vector3 move = forward * input.z + right * input.x;
            bool running = !crouched && input.sqrMagnitude > 0 && Input.GetKey(KeyCode.LeftShift) && status.ConsumeStamina(20 * Time.deltaTime);
            float speed = crouched ? crouchSpeed : running ? runSpeed : walkSpeed;
            if (status.Injured) speed *= 0.7f;
            if (controller.isGrounded && vertical < 0) vertical = -2;
            vertical += gravity * Time.deltaTime;
            controller.Move((move * speed + Vector3.up * vertical) * Time.deltaTime);
            if (move.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(move), 12 * Time.deltaTime);
                if (controller.isGrounded && Time.time >= nextStep)
                { nextStep = Time.time + (running ? 0.3f : 0.55f); EventBus.Publish(new NoiseEmitted(transform.position, crouched ? 2 : running ? 12 : 6, gameObject)); }
            }
        }
        private void OnDisable() { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
    }
}
