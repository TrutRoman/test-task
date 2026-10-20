using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

// Builds the low-poly character under the player object: feet at y = 0, facing +Z.
// Every joint is an empty pivot that CharacterAnimator rotates; meshes hang below their pivots.
public static class CharacterBuilder
{
    const float ThighLength = 0.4f;
    const float ShinLength = 0.4f;
    const float LegLength = ThighLength + ShinLength;
    const float HipSpacing = 0.11f;
    const float ShoulderSpacing = 0.27f;
    const float ShoulderHeight = 0.5f;
    const float NeckHeight = 0.6f;

    // A limb mesh points up from its base; turned upside down it hangs from the pivot.
    static readonly Quaternion HangDown = Quaternion.Euler(180f, 0f, 0f);

    sealed class Meshes
    {
        public Mesh Torso, Pelvis, Limb, Head, Hand, Eye, Scarf, Beanie, Pom;
    }

    public static CharacterAnimator Build(GameObject player, ArtMaterials materials)
    {
        Meshes meshes = CreateMeshes();
        Transform model = CreatePivot(player.transform, "Model", Vector3.zero);
        Transform hips = CreatePivot(model, "Hips", new Vector3(0f, LegLength, 0f));
        CreatePart(hips, "Pelvis", meshes.Pelvis, materials.CharacterPants, new Vector3(0f, -0.08f, 0f), Quaternion.identity, Vector3.one);

        (Transform thighLeft, Transform shinLeft) = CreateLeg(hips, "Left", -1f, meshes, materials);
        (Transform thighRight, Transform shinRight) = CreateLeg(hips, "Right", 1f, meshes, materials);

        Transform torso = CreatePivot(hips, "Torso", new Vector3(0f, 0.02f, 0f));
        CreatePart(torso, "Body", meshes.Torso, materials.CharacterJacket, Vector3.zero, Quaternion.identity, Vector3.one);
        Transform armLeft = CreateArm(torso, "Left", -1f, meshes, materials);
        Transform armRight = CreateArm(torso, "Right", 1f, meshes, materials);
        Transform scarfTail = CreateScarf(torso, meshes, materials);
        Transform head = CreateHead(torso, meshes, materials);

        var animator = player.AddComponent<CharacterAnimator>();
        EditorWiring.SetReference(animator, "hips", hips);
        EditorWiring.SetReference(animator, "torso", torso);
        EditorWiring.SetReference(animator, "head", head);
        EditorWiring.SetReference(animator, "thighLeft", thighLeft);
        EditorWiring.SetReference(animator, "thighRight", thighRight);
        EditorWiring.SetReference(animator, "shinLeft", shinLeft);
        EditorWiring.SetReference(animator, "shinRight", shinRight);
        EditorWiring.SetReference(animator, "armLeft", armLeft);
        EditorWiring.SetReference(animator, "armRight", armRight);
        EditorWiring.SetReference(animator, "scarfTail", scarfTail);
        EditorWiring.SetFloat(animator, "legLength", LegLength);

        CreateBlobShadow(player.transform, materials);
        return animator;
    }

    static Meshes CreateMeshes()
    {
        return new Meshes
        {
            Torso = Save(LowPolyMeshes.Frustum("Character Torso", 8, 0.25f, 0.2f, 0.58f)),
            Pelvis = Save(LowPolyMeshes.Frustum("Character Pelvis", 8, 0.2f, 0.25f, 0.1f)),
            // Unit length, scaled per limb.
            Limb = Save(LowPolyMeshes.Frustum("Character Limb", 6, 0.075f, 0.065f, 1f)),
            Head = Save(LowPolyMeshes.Sphere("Character Head", 1, 0.22f, 0f, 0)),
            Hand = Save(LowPolyMeshes.Sphere("Character Hand", 0, 0.065f, 0f, 0)),
            Eye = Save(LowPolyMeshes.Sphere("Character Eye", 0, 0.032f, 0f, 0)),
            Scarf = Save(LowPolyMeshes.Frustum("Character Scarf", 8, 0.21f, 0.17f, 0.1f)),
            Beanie = Save(LowPolyMeshes.Frustum("Character Beanie", 8, 0.205f, 0.1f, 0.13f)),
            Pom = Save(LowPolyMeshes.Sphere("Character Pom", 0, 0.055f, 0.1f, 5)),
        };
    }

    static Mesh Save(Mesh mesh)
    {
        return GeneratedAssets.Save(mesh, $"Meshes/{mesh.name}.asset");
    }

    static (Transform thigh, Transform shin) CreateLeg(Transform hips, string side, float direction, Meshes meshes, ArtMaterials materials)
    {
        Transform thigh = CreatePivot(hips, $"Thigh {side}", new Vector3(direction * HipSpacing, 0f, 0f));
        CreatePart(thigh, "Thigh Mesh", meshes.Limb, materials.CharacterPants, Vector3.zero, HangDown, new Vector3(1.45f, ThighLength, 1.45f));

        Transform shin = CreatePivot(thigh, $"Shin {side}", new Vector3(0f, -ThighLength, 0f));
        CreatePart(shin, "Shin Mesh", meshes.Limb, materials.CharacterPants, Vector3.zero, HangDown, new Vector3(1.2f, ShinLength - 0.06f, 1.2f));
        CreateBox(shin, "Foot", new Vector3(0f, -ShinLength + 0.035f, 0.05f), new Vector3(0.15f, 0.07f, 0.26f), materials.CharacterDark);
        return (thigh, shin);
    }

    static Transform CreateArm(Transform torso, string side, float direction, Meshes meshes, ArtMaterials materials)
    {
        Transform arm = CreatePivot(torso, $"Arm {side}", new Vector3(direction * ShoulderSpacing, ShoulderHeight, 0f));
        CreatePart(arm, "Arm Mesh", meshes.Limb, materials.CharacterJacket, Vector3.zero, HangDown, new Vector3(1.1f, 0.46f, 1.1f));
        CreatePart(arm, "Hand", meshes.Hand, materials.CharacterSkin, new Vector3(0f, -0.5f, 0f), Quaternion.identity, Vector3.one);
        return arm;
    }

    // A ring around the neck and a tail on the back that flutters while running.
    static Transform CreateScarf(Transform torso, Meshes meshes, ArtMaterials materials)
    {
        CreatePart(torso, "Scarf", meshes.Scarf, materials.CharacterScarf, new Vector3(0f, 0.53f, 0f), Quaternion.identity, Vector3.one);
        Transform tail = CreatePivot(torso, "Scarf Tail", new Vector3(0.07f, 0.58f, -0.17f));
        CreateBox(tail, "Scarf Tail Mesh", new Vector3(0f, -0.16f, 0f), new Vector3(0.12f, 0.32f, 0.05f), materials.CharacterScarf);
        return tail;
    }

    static Transform CreateHead(Transform torso, Meshes meshes, ArtMaterials materials)
    {
        Transform head = CreatePivot(torso, "Head", new Vector3(0f, NeckHeight, 0f));
        CreatePart(head, "Face", meshes.Head, materials.CharacterSkin, new Vector3(0f, 0.21f, 0f), Quaternion.identity, Vector3.one);

        Vector3 eyeScale = new(0.8f, 1.25f, 0.6f);
        CreatePart(head, "Eye Left", meshes.Eye, materials.CharacterDark, new Vector3(-0.08f, 0.24f, 0.195f), Quaternion.identity, eyeScale);
        CreatePart(head, "Eye Right", meshes.Eye, materials.CharacterDark, new Vector3(0.08f, 0.24f, 0.195f), Quaternion.identity, eyeScale);

        Vector3 cheekScale = new(1.1f, 0.6f, 0.4f);
        CreatePart(head, "Cheek Left", meshes.Eye, materials.CharacterScarf, new Vector3(-0.13f, 0.17f, 0.17f), Quaternion.identity, cheekScale);
        CreatePart(head, "Cheek Right", meshes.Eye, materials.CharacterScarf, new Vector3(0.13f, 0.17f, 0.17f), Quaternion.identity, cheekScale);

        CreatePart(head, "Beanie", meshes.Beanie, materials.CharacterScarf, new Vector3(0f, 0.3f, 0f), Quaternion.identity, Vector3.one);
        CreatePart(head, "Pom", meshes.Pom, materials.CharacterScarf, new Vector3(0f, 0.45f, 0f), Quaternion.identity, Vector3.one);
        return head;
    }

    // The Unity quad primitive faces -Z; BlobShadow turns it onto the ground every frame.
    static void CreateBlobShadow(Transform player, ArtMaterials materials)
    {
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "Blob Shadow";
        Object.DestroyImmediate(quad.GetComponent<Collider>());
        quad.transform.SetParent(player, false);
        quad.transform.SetLocalPositionAndRotation(new Vector3(0f, 0.02f, 0f), Quaternion.Euler(90f, 0f, 0f));

        var renderer = quad.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = materials.BlobShadow;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;

        EditorWiring.SetReference(quad.AddComponent<BlobShadow>(), "target", player);
    }

    static Transform CreatePivot(Transform parent, string name, Vector3 position)
    {
        Transform pivot = new GameObject(name).transform;
        pivot.SetParent(parent, false);
        pivot.localPosition = position;
        return pivot;
    }

    // The blob shadow replaces real shadows, so character parts do not cast any.
    static void CreatePart(Transform parent, string name, Mesh mesh, Material material, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        var part = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
        part.transform.SetParent(parent, false);
        part.transform.SetLocalPositionAndRotation(position, rotation);
        part.transform.localScale = scale;
        part.GetComponent<MeshFilter>().sharedMesh = mesh;

        var renderer = part.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
    }

    static void CreateBox(Transform parent, string name, Vector3 position, Vector3 size, Material material)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        Object.DestroyImmediate(box.GetComponent<Collider>());
        box.transform.SetParent(parent, false);
        box.transform.localPosition = position;
        box.transform.localScale = size;

        var renderer = box.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
    }
}
