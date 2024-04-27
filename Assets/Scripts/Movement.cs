using UnityEngine;

[DisallowMultipleComponent]
public class Movement : MonoBehaviour
{
    [SerializeField] private CharacterController controller;

    [Min(0)][SerializeField] private float inputMoveSpeed = 1;
    [SerializeField] private Vector3 inputMoveDirection;
    [SerializeField] private Vector3 additionalMoveForce;
    [SerializeField] private bool useLocalUpdate = false;

    public CharacterController Controller => controller;
    public float InputMoveSpeed => inputMoveSpeed;
    public Vector3 InputMoveDirection => inputMoveDirection;
    public Vector3 AdditionalMoveForce => additionalMoveForce;

    private void Update()
    {
        if(useLocalUpdate) MovementСalculation();
    }

    public void SetMoveValues(Vector3 inputDirection, float moveSpeed)
    {
        inputMoveDirection = inputDirection;
        inputMoveSpeed = moveSpeed;
    }

    public void SetMoveValues(Vector3 inputDirection, float moveSpeed, Vector3 additionalDirection)
    {
        additionalMoveForce = additionalDirection;
        SetMoveValues(inputDirection, moveSpeed);
    }

    public void MovementСalculation()
    {
        Vector3 inputDirection = NormalizeVectorMoreOne(inputMoveDirection) * inputMoveSpeed;
        Vector3 additionalDirection = additionalMoveForce;
        Vector3 moveDirection = inputDirection + additionalDirection;

        if (moveDirection == Vector3.zero) return;
        controller.Move(moveDirection * Time.deltaTime);
    }

    private Vector3 NormalizeVectorMoreOne(Vector3 vector)
    {
        return vector.magnitude > 1 ? vector.normalized : vector;
    }
}
