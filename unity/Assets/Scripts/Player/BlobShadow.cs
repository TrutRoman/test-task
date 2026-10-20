using UnityEngine;

// A soft round shadow on the ground under the character (the character itself casts no real shadow).
[RequireComponent(typeof(MeshRenderer))]
public class BlobShadow : MonoBehaviour
{
    // The ray starts inside the character's own collider, which raycasts ignore.
    const float RayStartHeight = 0.5f;

    [SerializeField] Transform target;
    [SerializeField] float size = 1.1f;
    [SerializeField] float maxHeight = 4f;
    [SerializeField] float surfaceOffset = 0.02f;

    MeshRenderer shadowRenderer;

    void Awake()
    {
        if (target == null) throw new MissingReferenceException("BlobShadow: target is not assigned.");

        shadowRenderer = GetComponent<MeshRenderer>();
    }

    void LateUpdate()
    {
        Vector3 origin = target.position + Vector3.up * RayStartHeight;
        bool hasGround = Physics.Raycast(origin, Vector3.down, out RaycastHit hit, maxHeight + RayStartHeight, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        shadowRenderer.enabled = hasGround;
        if (!hasGround) return;

        // The quad faces -Z, so -Z is turned to the ground normal. Higher above the ground means a smaller shadow.
        float height = hit.distance - RayStartHeight;
        transform.SetPositionAndRotation(hit.point + hit.normal * surfaceOffset, Quaternion.FromToRotation(Vector3.back, hit.normal));
        transform.localScale = Vector3.one * size * Mathf.Lerp(1f, 0.4f, height / maxHeight);
    }
}
