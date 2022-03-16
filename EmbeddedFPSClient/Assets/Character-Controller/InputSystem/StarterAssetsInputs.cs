using UnityEngine;

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[SerializeField]
		float speedMultiplier=5;
		float lookSpeed = 50;

		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
		public bool shoot;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

        public void OnMove()
		{
			Vector2 movementVector = Vector2.zero;
			movementVector.x = Input.GetAxis("Horizontal")*speedMultiplier;
			movementVector.y = Input.GetAxis("Vertical") * speedMultiplier ;
			MoveInput(movementVector);
		}

		public void OnLook()
		{
			Vector2 mouseLook = Vector2.zero;
			mouseLook.x = Input.GetAxis("Mouse X") * lookSpeed;
			mouseLook.y = -Input.GetAxis("Mouse Y") *  lookSpeed;
			if (cursorInputForLook)
			{
				LookInput(mouseLook);
			}
		}

        private void Update()
        {
			OnMove();
			OnLook();
			OnSprint();
			OnShoot();
		}

		public void OnSprint()
		{
			SprintInput(Input.GetButton("Sprint"));
		}

		public void OnShoot()
        {
			ShootInput(Input.GetButtonDown("Fire1"));
        } 

		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
			if (sprint)
				speedMultiplier = Mathf.Lerp(speedMultiplier,2,Time.deltaTime);
			else
				speedMultiplier = Mathf.Lerp(speedMultiplier,1,Time.deltaTime);
		}

		public void ShootInput(bool shootState)
        {
			shoot = shootState;
        }

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
}