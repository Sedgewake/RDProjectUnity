using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController characterController;
    public MovementType currentMovement = MovementType.Walk;

    void Update()
    {
        switch (currentMovement)
        {
            case MovementType.Walk:
                Walk();
                if (Input.GetKeyDown(KeyCode.F)) currentMovement = MovementType.Swim;
                break;
            case MovementType.Swim:
                Swim();
                if (Input.GetKeyDown(KeyCode.G)) currentMovement = MovementType.Walk;
                break;
            case MovementType.Ladder:
                ClimbLadder();
                break;
            case MovementType.Vehicle:
                RideVehicle();
                break;
        }
    }

    void Walk()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        characterController.Move(move * Time.deltaTime * 5f);
    }

    void Swim()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        float y = Input.GetAxis("Jump") - (Input.GetKey(KeyCode.LeftShift) ? 1 : 0);
        Vector3 swim = new Vector3(x, y, z);
        characterController.Move(swim * Time.deltaTime * 2f);
    }

    void ClimbLadder()
    {
        Vector3 up = transform.up * Input.GetAxis("Vertical");
        characterController.Move(up * Time.deltaTime * 3f);
    }

    void RideVehicle()
    {
        // Placeholder for vehicle logic
    }
}