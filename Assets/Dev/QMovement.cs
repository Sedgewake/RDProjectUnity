//#define USE_CROUCH
#define MOUSE_SMOOTHING
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QMovement : MonoBehaviour
{
	public Camera playerCamera;
	public float moveSpeed = 5.0f;
	public float rotationSpeed = 3.0f;

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

	// Grabbing
	public float grabRange = 3f;
	public float holdDistance = 2f;
	public LayerMask grabbableLayer;
	public Transform holdPoint;

	private Rigidbody grabbedObject;
	private Vector3 grabVelocity;

	void Start () 
	{
		player_rotation = transform.rotation;
		controller = GetComponent<CharacterController>();
		controller.skinWidth = 0.03f;
		controller.height = PLAYER_HEIGHT;
		controller.radius = 0.35f;		
		controller.minMoveDistance = 0;
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
		look_input = Input.GetAxis("Mouse Y") * rotationSpeed * 0.9f;
		look_y -= look_input;
		look_y = Mathf.Clamp(look_y, -90.0f, 90.0f);
		player_rotation *= Quaternion.Euler(0, rotation_input, 0);
		move_direction = player_rotation * move_input;

		if (controller.isGrounded)
		{
			if (Physics.Raycast(transform.position, vector_down, out hit_surface, 1.5f))
			{
				surface_normal = hit_surface.normal;
				move_direction = ProjectOnPlane(move_direction, surface_normal);
			}
			move_direction.Normalize();
			#if USE_CROUCH
			move_speed = move_direction.magnitude * (moveSpeed - crouch_value_s * 3.0f);
			#else
			move_speed = move_direction.magnitude * moveSpeed;
			#endif
			dot = Vector3.Dot(move_vector, move_direction);
			speed = new Vector2(move_vector.x, move_vector.z).magnitude;
			speed_mul = speed - (speed < DECC_SPEED ? DECC_SPEED : speed) * FRICTION * frame_time;
			if (speed_mul < 0) speed_mul = 0;
			if (speed > 0) speed_mul /= speed;
			move_vector *= speed_mul;
			vel_add = move_speed - dot;
			vel_mul = ACC_SPEED * frame_time * move_speed;
			if (vel_mul > vel_add) vel_mul = vel_add;
			move_vector += move_direction * vel_mul;
			if (move_vector.y > -0.5f) move_vector.y = -0.5f;
			if (Input.GetButtonDown("Jump") && surface_normal.y > 0.5f)
			{
				move_vector *= JUMP_ACC;
				move_vector.y = JUMP_VEL;
			}
		}
		else
		{
			move_direction.Normalize();
			move_speed = move_direction.magnitude * moveSpeed;
			dot = Vector3.Dot(move_vector, move_direction);
			vel_add = move_speed - dot;
			vel_mul = ACC_SPEED_AIR * frame_time * move_speed;
			if (vel_mul > vel_add) vel_mul = vel_add;
			if (vel_mul > 0) move_vector += move_direction * vel_mul;
			move_vector.y -= GRAVITY_ADD * frame_time;
		}

		#if USE_CROUCH
		// Crouch logic unchanged
		#endif

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

		// Grabbing input
		if (Input.GetMouseButtonDown(1)) // Right click
		{
			if (grabbedObject == null)
				TryGrabObject();
			else
				DropObject();
		}
	}

	void FixedUpdate()
	{
		if (grabbedObject != null)
		{
			Vector3 targetPosition = holdPoint.position;
			Vector3 toTarget = targetPosition - grabbedObject.position;
			grabVelocity = toTarget / Time.fixedDeltaTime;
			grabbedObject.velocity = grabVelocity;
		}
	}

	void TryGrabObject()
	{
		Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
		if (Physics.Raycast(ray, out RaycastHit hit, grabRange, grabbableLayer))
		{
			Rigidbody rb = hit.collider.attachedRigidbody;
			if (rb != null && !rb.isKinematic)
			{
				grabbedObject = rb;
				grabbedObject.useGravity = false;
				grabbedObject.drag = 10f;
			}
		}
	}

	void DropObject()
	{
		if (grabbedObject != null)
		{
			grabbedObject.useGravity = true;
			grabbedObject.drag = 0f;
			grabbedObject = null;
		}
	}

	Vector3 ProjectOnPlane(Vector3 vector, Vector3 normal)
	{
		return vector - normal * Vector3.Dot(vector, normal);
	}
}
