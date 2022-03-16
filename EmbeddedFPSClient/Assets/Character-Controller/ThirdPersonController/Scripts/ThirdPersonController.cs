using DarkRift;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
	[RequireComponent(typeof(CharacterController))]
	public class ThirdPersonController : MonoBehaviour
	{
		[Header("Player")]
		[Tooltip("Move speed of the character in m/s")]
		public float moveSpeed = 2.0f;
		[Tooltip("Sprint speed of the character in m/s")]
		public float sprintSpeed = 5.335f;
		[Tooltip("How fast the character turns to face movement direction")]
		[Range(0.0f, 0.3f)]
		public float rotationSmoothTime = 0.12f;
		[Tooltip("Acceleration and deceleration")]
		public float speedChangeRate = 10.0f;

		[Space(10)]
		[Tooltip("The height the player can jump")]
		public float jumpHeight = 1.2f;
		[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		public float gravity = -15.0f;

		[Space(10)]
		[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
		public float jumpTimeout = 0.50f;
		[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		public float fallTimeout = 0.15f;

		[Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		public bool grounded = true;
		[Tooltip("Useful for rough ground")]
		public float groundedOffset = -0.14f;
		[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		public float groundedRadius = 0.28f;
		[Tooltip("What layers the character uses as ground")]
		public LayerMask groundLayers;

		[Header("Cinemachine")]
		[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
		public GameObject cinemachineCameraTarget;
		[Tooltip("How far in degrees can you move the camera up")]
		public float topClamp = 70.0f;
		[Tooltip("How far in degrees can you move the camera down")]
		public float bottomClamp = -30.0f;
		[Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
		public float cameraAngleOverride = 0.0f;
		[Tooltip("For locking the camera position on all axis")]
		public bool lockCameraPosition = false;

		// cinemachine
		protected float cinemachineTargetYaw;
		protected float cinemachineTargetPitch;

		// player
		protected float targetRotation = 0.0f;
		private float rotationVelocity;
		private float verticalVelocity;
		private float terminalVelocity = 53.0f;
		private Vector3 playerVector;

		// animation IDs
		protected int animIDSpeedHorizontal;
		protected int animIDSpeedVertical;
		protected int animIDGrounded;
		protected int animIDJump;
		protected int animIDFreeFall;
		protected int animIDMotionSpeed;

		protected Animator animator;
		protected CharacterController controller;


		protected StarterAssetsInputs input;
		protected GameObject mainCamera;

		protected const float threshold = 0.05f;

		protected bool hasAnimator;

		[HideInInspector]
		public int speedModifier=1;

		protected virtual void Awake()
		{
			// get a reference to our main camera

		}

        protected virtual void OnEnable()
        {
			if (mainCamera == null)
			{
				mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			}
		}

        protected virtual void Start()
		{
			hasAnimator = TryGetComponent(out animator);
			controller = GetComponent<CharacterController>();
			input = FindObjectOfType<StarterAssetsInputs>();
			AssignAnimationIDs();
		}


		protected virtual void Update()
		{
			hasAnimator = TryGetComponent(out animator);

			if (animator.GetCurrentAnimatorStateInfo(0).IsName("JumpStart"))
				animator.ResetTrigger("Jump");

			Jump();
			GroundedCheck();
			Move();
			//controller.Move(playerVector * Time.deltaTime);
		}

		public void Jump()
		{
			playerVector = new Vector3();
			if (grounded &&Input.GetButtonDown("Jump"))
			{
				verticalVelocity = Mathf.Sqrt(jumpHeight * -2.0f * Physics.gravity.y);
				animator.SetTrigger(animIDJump);
			}
			if (!grounded)
				verticalVelocity += Physics.gravity.y * Time.deltaTime;
			else
				verticalVelocity = 0;
			playerVector.y = verticalVelocity;
		}

		protected virtual void LateUpdate()
		{
			CameraRotation();
			UpdateAnimator();
		}

		protected void AssignAnimationIDs()
		{
			animIDSpeedHorizontal = Animator.StringToHash("Horizontal");
			animIDSpeedVertical = Animator.StringToHash("Vertical");
			animIDGrounded = Animator.StringToHash("Grounded");
			animIDJump = Animator.StringToHash("Jump");
			animIDFreeFall = Animator.StringToHash("FreeFall");
			animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
		}

		protected void GroundedCheck()
		{
			// set sphere position, with offset
			Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z);
			grounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);

			// update animator if using character
			if (hasAnimator)
			{
				animator.SetBool(animIDGrounded, grounded);
			}
		}

		private void CameraRotation()
		{
			// if there is an input and camera position is not fixed
			if (input.look.sqrMagnitude >= threshold && !lockCameraPosition)
			{
				cinemachineTargetYaw += input.look.x * Time.deltaTime;
				cinemachineTargetPitch += input.look.y * Time.deltaTime;
			}

			// clamp our rotations so our values are limited 360 degrees
			cinemachineTargetYaw = ClampAngle(cinemachineTargetYaw, float.MinValue, float.MaxValue);
			cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, bottomClamp, topClamp);

			// Cinemachine will follow this target
			cinemachineCameraTarget.transform.rotation = Quaternion.Euler(cinemachineTargetPitch + cameraAngleOverride, cinemachineTargetYaw, 0.0f);
		}

        private void Move()
		{
			float targetSpeed = input.sprint ? sprintSpeed : moveSpeed;
			speedModifier = input.sprint ? 2 : 1;
			if (input.move == Vector2.zero) targetSpeed = 0.0f;

			float inputVertical = input.move.y;
			float inputHorizontal = input.move.x;

			Vector3 inputDirection = transform.right * input.move.x + transform.forward * input.move.y;
			targetRotation = mainCamera.transform.eulerAngles.y;
			float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, rotationSmoothTime);

			transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
			inputDirection = inputDirection.normalized * targetSpeed;
			playerVector.x = inputDirection.x;
			playerVector.z = inputDirection.z;
		}

		void UpdateAnimator()
        {
			if (hasAnimator)
			{
				animator.SetFloat(animIDSpeedHorizontal, input.move.x * speedModifier, Time.deltaTime, Time.deltaTime);
				animator.SetFloat(animIDSpeedVertical, input.move.y * speedModifier, Time.deltaTime, Time.deltaTime);
				animator.SetFloat(animIDMotionSpeed, new Vector2(input.move.y, input.move.x).normalized.magnitude);
			}
		}

		protected static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

		private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;
			
			// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
			Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z), groundedRadius);
		}
	}
}