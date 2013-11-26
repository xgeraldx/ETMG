

using UnityEngine;
using System.Collections;



public class QController : MonoBehaviour {
	public AnimationClip idleAnimation;
	public AnimationClip walkAnimation;
	public AnimationClip runAnimation;
	public AnimationClip standAttackSingleShot;
	public AnimationClip standAttackRapidFire;
	public AnimationClip runAttackSingleShot;
	public AnimationClip runAttackRapidFire;
	public AnimationClip forwardRoll;
	public AnimationClip death;
	public AnimationClip takeDamage;

	public float walkMaxAnimationSpeed = 0.75;
	public float trotMaxAnimationSpeed = 1.0;
	public float runMaxAnimationSpeed  = 1.0;
	public float jumpAnimationSpeed  = 1.15;
	public float landAnimationSpeed  = 1.0;
	
	private Animation _animation;

	enum CharacterState {
		Idle = 0,
		Walking = 1,
		Trotting = 2,
		Running = 3,
		Jumping = 4,
		RunShootSingle = 5,
		RunShootRapid = 6,
		StandShootSingle = 7,
		StandShootRapid = 8,
		TakeDamage = 9,
		Death = 10,
		ForwardRoll = 11
	}

	private CharacterState _characterState;

	// The speed when walking
	public float walkSpeed = 2.0f;
	// after trotAfterSeconds of walking we trot with trotSpeed
	public float trotSpeed = 4.0f;
	// when pressing "Fire3" button (cmd) we start running
	public float runSpeed = 6.0f;
	
	public float inAirControlAcceleration = 3.0f;
	
	// How high do we jump when pressing jump and letting go immediately
	public float jumpHeight = 0.5f;
	
	// The gravity for the character
	public float gravity = 20.0f;
	// The gravity in controlled descent mode
	public float speedSmoothing = 10.0f;
	public float rotateSpeed = 500.0f;
	public float trotAfterSeconds = 3.0f;
	
	public bool canJump = true;

	private float jumpRepeatTime = 0.05f;
	private float jumpTimeout = 0.15f;
	private float groundedTimeout = 0.25f;

	#region Non-Android
	// The current move direction in x-z
	private float moveDirection = Vector3.zero;
	// The current vertical speed
	private float verticalSpeed = 0.0f;
	// The current x-z move speed
	private float moveSpeed = 0.0f;
	#endregion
	public Joystick moveJoystick;
	public Joystick rotateJoystick;

	public Transform cameraPivot;

	public float forwardSpeed = 4f;
	public float backwardSpeed = 1f;
	public float sidestepSpeed = 1f;
	public float jumpSpeed = 8f;
	public float inAirMultiplier = .25f;
	Vector2 rotationSpeed = new Vector2(50,25);

	private Transform thisTransform;
	private CharacterController character;
	private Vector3 cameraVelocity;
	private Vector3 velocity;

	// Use this for initialization
	void Start () {
		thisTransform = GetComponent<Transform>();
		character = GetComponent<CharacterController>();
#if UNITY_EDITOR
		moveJoystick.Disable();
		rotateJoystick.Disable();
#endif

	}

	void OnEndGame()
	{
		moveJoystick.Disable();
		rotateJoystick.Disable();
		this.enabled = false;
	}
	// Update is called once per frame
	void Update () 
	{
	
	#if UNITY_ANDROID_API
		Debug.Log("Android");
		UpdateMobileMovement();
	#endif

	#if UNITY_EDITOR
		Debug.Log("Editor");
		UpdateComputerMovement();
	#endif
	
	#if UNITY_STANDALONE_WIN
		Debug.log("Win");
		UpdateComputerMovement();
	#endif

	}

	void UpdateMobileMovement()
	{
		Vector3 movement = thisTransform.TransformDirection( new Vector3( moveJoystick.position.x, 0, moveJoystick.position.y ) );

		// We only want horizontal movement
		movement.y = 0f;
		movement.Normalize();
		
		Vector3 cameraTarget = Vector3.zero;
		
		// Apply movement from move joystick
		Vector2 absJoyPos = new Vector2( Mathf.Abs( moveJoystick.position.x ), Mathf.Abs( moveJoystick.position.y ) );	
		if ( absJoyPos.y > absJoyPos.x )
		{
			if ( moveJoystick.position.y > 0f )
				movement *= forwardSpeed * absJoyPos.y;
			else
			{
				movement *= backwardSpeed * absJoyPos.y;
				cameraTarget.z = moveJoystick.position.y * 0.75f;
			}
		}
		else
		{
			movement *= sidestepSpeed * absJoyPos.x;
			
			// Let's move the camera a bit, so the character isn't stuck under our thumb
			cameraTarget.x = -moveJoystick.position.x * 0.5f;
		}
		
		// Check for jump
		if ( character.isGrounded )
		{
			if ( rotateJoystick.tapCount == 2 )
			{
				// Apply the current movement to launch velocity		
				velocity = character.velocity;
				velocity.y = jumpSpeed;			
			}
		}
		else
		{			
			// Apply gravity to our velocity to diminish it over time
			velocity.y += Physics.gravity.y * Time.deltaTime;
			
			// Move the camera back from the character when we jump
			cameraTarget.z = -jumpSpeed * 0.25f;
			
			// Adjust additional movement while in-air
			movement.x *= inAirMultiplier;
			movement.z *= inAirMultiplier;
		}
		
		movement += velocity;	
		movement += Physics.gravity;
		movement *= Time.deltaTime;
		
		// Actually move the character	
		character.Move( movement );
		
		if ( character.isGrounded )
			// Remove any persistent velocity after landing	
			velocity = Vector3.zero;
		
		// Seek camera towards target position
		var pos = cameraPivot.localPosition;
		pos.x = Mathf.SmoothDamp( pos.x, cameraTarget.x, ref cameraVelocity.x, 0.3f );
		pos.z = Mathf.SmoothDamp( pos.z, cameraTarget.z, ref cameraVelocity.z, 0.5f );
		cameraPivot.localPosition = pos;
		
		// Apply rotation from rotation joystick
		if ( character.isGrounded )
		{
			var camRotation = rotateJoystick.position;
			camRotation.x *= rotationSpeed.x;
			camRotation.y *= rotationSpeed.y;
			camRotation *= Time.deltaTime;
			
			// Rotate the character around world-y using x-axis of joystick
			thisTransform.Rotate( 0f, camRotation.x, 0f, Space.World );
			
			// Rotate only the camera with y-axis input
			cameraPivot.Rotate( camRotation.y, 0f, 0f );
		}

	}

	void UpdateComputerMovement()
	{

	}
}
