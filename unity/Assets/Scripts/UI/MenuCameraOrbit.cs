using UnityEngine;

// Slowly circles the main menu camera around the level.
public class MenuCameraOrbit : MonoBehaviour
{
    [SerializeField] Vector3 center = new(0f, 1f, 2f);
    [SerializeField] float radius = 17f;
    [SerializeField] float height = 6f;
    [SerializeField] float degreesPerSecond = 4f;
    [SerializeField] float startAngle = -30f;

    float angle;

    void Awake()
    {
        MoveToStart();
    }

    void LateUpdate()
    {
        angle += degreesPerSecond * Time.deltaTime;
        ApplyAngle();
    }

    // Public so the scene builder can save the camera at its starting pose.
    public void MoveToStart()
    {
        angle = startAngle;
        ApplyAngle();
    }

    void ApplyAngle()
    {
        Vector3 offset = Quaternion.Euler(0f, angle, 0f) * new Vector3(0f, height, -radius);
        Vector3 position = center + offset;
        transform.SetPositionAndRotation(position, Quaternion.LookRotation(center - position));
    }
}
