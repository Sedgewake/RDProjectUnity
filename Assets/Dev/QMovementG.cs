using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QMovementG : MonoBehaviour
{
	public Camera playerCamera;
	public float moveSpeed = 5.0f;
	public float rotationSpeed = 3.0f;

	// Grab specific variables
	public float grabDistance = 3.0f; // Distance from which the player can grab objects
	public float carryDistance = 2.0f; // Distance at which the object is carried in front of the player
	public float throwForce = 10.0f; // Force applied when throwing the object

	private Rigidbody grabbedObject;
	private bool isGrabbing = false;
	private Vector3 grabbedObjectOriginalScale; // To store the original scale if we decide to change it while carrying

	private const float FRICTION = 5.0f;
	private const float DECC_SPEED = 3.5f;
	private const float ACC_SPEED = 10.0f;
	private const float ACC_SPEED_AIR = 1.5f;
	private const float JUMP_VEL = 5.8f;
	private const float JUMP_ACC = 1.42f;
	private const float GRAVITY_ADD = 17.0f;
	private const float CAMERA_OFFSET = 0.72f;
	private const float PLAYER_HEIGHT = 1.8f;
	
	#if MOUSE_SMOOTHING
	[RangeAttribute(0.0f, 1.0f)] public float mouseSmoothing = 0.1f;
	private Quaternion targetRotation;
	#endif
	private CharacterController controller;
	private Vector3 move_input;
	private Vector3 move_direction;
	private Vector3 move_vector;
	private Vector3 vector_down;
	private Vector3 surface_normal;
	private Vector3 camera_offset;
	private Quaternion player_rotation;
	private RaycastHit hit_surface;
	private float frame_time = 0.0f;
	private float rotation_input;
	private float look_input;
	private float look_y = 0.0f;
	private float move_speed;
	private float dot;
	private float vel_add;
	private float vel_mul;
	private float speed;
	private float speed_mul;
	#if USE_CROUCH
	private float crouch_value = 0;
	private float crouch_value_s = 0;
	private Vector3 center_offset;
	#endif

	void Start () 
	{
		player_rotation = transform.rotation;
		controller = GetComponent<CharacterController>();
		controller.skinWidth = 0.03f;
		controller.height = PLAYER_HEIGHT;
		controller.radius = 0.35f;		
		controller.minMoveDistance = 0; // This is required for CharacterController.isGrounded to always work.
		move_vector = new Vector3(0, -0.5f, 0);
		vector_down = new Vector3(0, -1.0f, 0);
		surface_normal = new Vector3(0, 1.0f, 0);
		move_input = new Vector3(0, 0, 0);
		camera_offset = new Vector3(0, CAMERA_OFFSET, 0);
		#if USE_CROUCH
		center_offset = new Vector3(0, 0, 0);
		#endif
		if (!Application.isEditor)
		{
			Cursor.visible = false;
		}
	}

	void Update () 
	{
		frame_time = Time.deltaTime;
		move_input.x = Input.GetAxisRaw("Horizontal");
		move_input.z = Input.GetAxisRaw("Vertical");
		rotation_input = Input.GetAxis("Mouse X") * rotationSpeed;
		look_input = Input.GetAxis("Mouse Y") * rotationSpeed * 0.9f; // Make vertical mouse look less sensitive.
		look_y -= look_input;
		look_y = Mathf.Clamp(look_y, -90.0f, 90.0f);
		player_rotation *= Quaternion.Euler(0, rotation_input, 0);
		move_direction = player_rotation * move_input;
		if (controller.isGrounded)
		{
			if (Physics.Raycast(transform.position, vector_down, out hit_surface, 1.5f))
			{
				surface_normal = hit_surface.normal;
				move_direction = ProjectOnPlane(move_direction, surface_normal); // Stick to the ground on slopes.
			}
			move_direction.Normalize();
			#if USE_CROUCH
			move_speed = move_direction.magnitude * (moveSpeed - crouch_value_s * 3.0f);
			#else
			move_speed = move_direction.magnitude * moveSpeed;
			#endif
			dot = move_vector.x * move_direction.x + move_vector.y * move_direction.y + move_vector.z * move_direction.z;
			speed = (float)System.Math.Sqrt(move_vector.x * move_vector.x + move_vector.z * move_vector.z);
			speed_mul = speed - (speed < DECC_SPEED ? DECC_SPEED : speed) * FRICTION * frame_time;
			if(speed_mul < 0) speed_mul = 0;
			if(speed > 0) speed_mul /= speed;
			move_vector *= speed_mul;
			vel_add = move_speed - dot;
			vel_mul = ACC_SPEED * frame_time * move_speed;
			if(vel_mul > vel_add) vel_mul = vel_add;
			move_vector += move_direction * vel_mul;
			if (move_vector.y > -0.5f) move_vector.y = -0.5f; // Make sure there is always a little gravity to keep the character on the ground.
			if(Input.GetButtonDown("Jump"))
			{
				if (surface_normal.y > 0.5f) // Do not jump on high slopes.
				{
					move_vector *= JUMP_ACC;
					move_vector.y = JUMP_VEL;
				}
			}
		}
		else // In Air
		{
			move_direction.Normalize();
			move_speed = move_direction.magnitude * moveSpeed;
			dot = move_vector.x * move_direction.x + move_vector.y * move_direction.y + move_vector.z * move_direction.z;
			vel_add = move_speed - dot;
			vel_mul = ACC_SPEED_AIR * frame_time * move_speed;
			if (vel_mul > vel_add) vel_mul = vel_add;
			if (vel_mul > 0) move_vector += move_direction * vel_mul;
			move_vector.y -= GRAVITY_ADD * frame_time;
		}
		#if USE_CROUCH
		if (Input.GetButton("Crouch"))
		{
			if (crouch_value < 1.0f)
			{
				crouch_value += frame_time * 5.7f;
				crouch_value_s = Mathf.Clamp01(crouch_value);
				center_offset.y = crouch_value_s * -0.5f;
				controller.height = PLAYER_HEIGHT - crouch_value_s;
				controller.center = center_offset;
				camera_offset.y = CAMERA_OFFSET - crouch_value_s * 0.9f;
			}
		}
		else
		{
			if (crouch_value > 0)
			{
				RaycastHit hit_up;
				if (!Physics.SphereCast(playerCamera.transform.position + vector_down * 0.25f, 0.3f, new Vector3(0, 1.0f, 0), out hit_up, 0.3f)) // Check if there is a space for player to raise
				{
					crouch_value -= frame_time * 5.0f;
					crouch_value_s = Mathf.Clamp01(crouch_value);
					center_offset.y = crouch_value_s * -0.5f;
					controller.height = PLAYER_HEIGHT - crouch_value_s;
					controller.center = center_offset;
					camera_offset.y = CAMERA_OFFSET - crouch_value_s * 0.9f;
				}
			}
		}
		#endif

		// Handle Grab/Release input
		if (Input.GetButtonDown("Use")) // Assuming "Use" is set up in your Input Manager (e.g., E key)
		{
			if (isGrabbing)
			{
				ReleaseObject();
			}
			else
			{
				TryGrabObject();
			}
		}
		
		controller.Move(move_vector * frame_time);
		#if MOUSE_SMOOTHING
		targetRotation = player_rotation * Quaternion.Euler(look_y, 0, 0);
		playerCamera.transform.rotation = Quaternion.Slerp(playerCamera.transform.rotation, targetRotation, frame_time * (1.0f - mouseSmoothing) * 50.0f);
		#else
		playerCamera.transform.rotation = player_rotation * Quaternion.Euler(look_y, 0, 0);
		#endif
		playerCamera.transform.position = transform.position + camera_offset;
		if (Input.GetKeyDown("escape"))
		{
			Application.Quit();
		}
	}

	void FixedUpdate()
	{
		if (isGrabbing && grabbedObject != null)
		{
			// Calculate the target position and rotation for the grabbed object
			Vector3 targetPosition = playerCamera.transform.position + playerCamera.transform.forward * carryDistance;
			Quaternion targetRotation = playerCamera.transform.rotation;

			// Smoothly move the object to the target position.
			// Using MovePosition and MoveRotation for Rigidbody, which is good for physics interactions.
			// However, since we're making it kinematic, directly setting transform.position and rotation in Update
			// would also work and might look smoother for purely visual carrying.
			// For this implementation, we'll keep it kinematic and directly set its transform in Update.
			// The key here is that we're essentially bypassing physics simulation for the grabbed object
			// to ensure it sticks to the player's view.
		}
	}

	void LateUpdate()
	{
		// This is where we update the grabbed object's position and rotation for visual smoothness
		// after the camera has moved.
		if (isGrabbing && grabbedObject != null)
		{
			grabbedObject.position = playerCamera.transform.position + playerCamera.transform.forward * carryDistance;
			grabbedObject.rotation = playerCamera.transform.rotation;
		}
	}

	void TryGrabObject()
	{
		RaycastHit hit;
		if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, grabDistance))
		{
			Rigidbody hitRb = hit.collider.GetComponent<Rigidbody>();
			if (hitRb != null && !hitRb.isKinematic) // Ensure it's a movable Rigidbody and not already kinematic
			{
				grabbedObject = hitRb;
				isGrabbing = true;

				// Store original properties
				grabbedObjectOriginalScale = grabbedObject.transform.localScale;

				// Make the object kinematic so it doesn't fall or get affected by physics while being carried
				grabbedObject.isKinematic = true;
				grabbedObject.useGravity = false;

				// Parent the object to the camera for easy positioning relative to the player's view.
				// This simplifies keeping it in front of the player.
				grabbedObject.transform.parent = playerCamera.transform;

				// Offset the object so it appears in front of the camera
				grabbedObject.transform.localPosition = new Vector3(0, 0, carryDistance);
				grabbedObject.transform.localRotation = Quaternion.identity; // Reset rotation relative to camera
			}
		}
	}

	void ReleaseObject()
	{
		if (grabbedObject != null)
		{
			// Unparent the object
			grabbedObject.transform.parent = null;

			// Restore original properties
			grabbedObject.isKinematic = false;
			grabbedObject.useGravity = true;
			// Add force to "throw" the object when released
			grabbedObject.AddForce(playerCamera.transform.forward * throwForce, ForceMode.VelocityChange);

			grabbedObject = null;
			isGrabbing = false;
		}
	}

	Vector3 ProjectOnPlane (Vector3 vector, Vector3 normal)
	{
		return vector - normal * (vector.x * normal.x + vector.y * normal.y + vector.z * normal.z);
	}
}