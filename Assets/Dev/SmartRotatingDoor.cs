using UnityEngine;

public class SmartRotatingDoor : MonoBehaviour
{
    public Transform hinge; // Pivot point (usually the door itself or a child)
    public float openAngle = 90f;
    public float openSpeed = 3f;
    public float activationDistance = 3f;
    public KeyCode openKey = KeyCode.E;

    private bool isOpen = false;
    private bool isAnimating = false;
    private Quaternion targetRotation;
    private Quaternion initialRotation;

    private Transform player;

    void Start()
    {
        if (hinge == null)
            hinge = transform;

        initialRotation = hinge.rotation;
        player = Camera.main.transform;
    }

    void Update()
    {
        if (isAnimating)
        {
            hinge.rotation = Quaternion.Slerp(hinge.rotation, targetRotation, Time.deltaTime * openSpeed);
            if (Quaternion.Angle(hinge.rotation, targetRotation) < 0.1f)
            {
                hinge.rotation = targetRotation;
                isAnimating = false;
            }
            return;
        }

        if (Vector3.Distance(player.position, hinge.position) <= activationDistance && Input.GetKeyDown(openKey))
        {
            ToggleDoor();
        }
    }

    void ToggleDoor()
    {
        if (!isOpen)
        {
            // Determine if player is in front or behind the door
            Vector3 doorForward = hinge.forward;
            Vector3 toPlayer = player.position - hinge.position;
            float dot = Vector3.Dot(doorForward, toPlayer);

            float angle = dot > 0 ? -openAngle : openAngle; // open away from player
            targetRotation = Quaternion.Euler(0, angle, 0) * hinge.rotation;
        }
        else
        {
            targetRotation = initialRotation;
        }

        isOpen = !isOpen;
        isAnimating = true;
    }
}
