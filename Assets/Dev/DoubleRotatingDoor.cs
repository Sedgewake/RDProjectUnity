using UnityEngine;

public class DoubleRotatingDoor : MonoBehaviour
{
    public Transform leftDoor;    // Left door panel
    public Transform rightDoor;   // Right door panel
    public float openAngle = 90f; // Degrees each door opens (usually 90)
    public float openSpeed = 3f;
    public float activationDistance = 3f;
    public KeyCode openKey = KeyCode.E;

    private Quaternion leftInitialRot;
    private Quaternion rightInitialRot;
    private Quaternion leftTargetRot;
    private Quaternion rightTargetRot;

    private bool isOpen = false;
    private bool isAnimating = false;

    private Transform player;

    void Start()
    {
        if (leftDoor == null || rightDoor == null)
        {
            Debug.LogError("Assign both left and right door panels.");
            enabled = false;
            return;
        }

        leftInitialRot = leftDoor.localRotation;
        rightInitialRot = rightDoor.localRotation;
        player = Camera.main.transform;
    }

    void Update()
    {
        if (isAnimating)
        {
            leftDoor.localRotation = Quaternion.Slerp(leftDoor.localRotation, leftTargetRot, Time.deltaTime * openSpeed);
            rightDoor.localRotation = Quaternion.Slerp(rightDoor.localRotation, rightTargetRot, Time.deltaTime * openSpeed);

            float leftAngle = Quaternion.Angle(leftDoor.localRotation, leftTargetRot);
            float rightAngle = Quaternion.Angle(rightDoor.localRotation, rightTargetRot);
            if (leftAngle < 0.5f && rightAngle < 0.5f)
            {
                leftDoor.localRotation = leftTargetRot;
                rightDoor.localRotation = rightTargetRot;
                isAnimating = false;
            }
            return;
        }

        if (Vector3.Distance(player.position, transform.position) <= activationDistance && Input.GetKeyDown(openKey))
        {
            ToggleDoor();
        }
    }

    void ToggleDoor()
    {
        Vector3 doorForward = transform.forward;
        Vector3 toPlayer = player.position - transform.position;
        float dot = Vector3.Dot(doorForward, toPlayer);

        float direction = dot > 0 ? -1f : 1f; // open away from player

        if (!isOpen)
        {
            leftTargetRot = Quaternion.Euler(0, direction * openAngle, 0) * leftInitialRot;
            rightTargetRot = Quaternion.Euler(0, -direction * openAngle, 0) * rightInitialRot;
        }
        else
        {
            leftTargetRot = leftInitialRot;
            rightTargetRot = rightInitialRot;
        }

        isOpen = !isOpen;
        isAnimating = true;
    }
}
