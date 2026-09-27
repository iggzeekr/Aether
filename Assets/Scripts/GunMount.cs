using UnityEngine;

public static class GunMount
{
    public static Transform Socket(GameObject prefab, Transform rig, string socketName)
    {
        Transform socket = Find(rig, socketName);
        if (prefab == null || socket == null)
            return null;

        GameObject gun = Object.Instantiate(prefab, socket);
        gun.transform.localPosition = Vector3.zero;
        gun.transform.localRotation = Quaternion.identity;
        gun.transform.localScale = Vector3.one;
        Strip(gun);
        return Nose(gun.transform, socket.forward);
    }

    public static Transform Hand(GameObject prefab, Transform hand, Transform owner)
    {
        if (hand == null || owner == null)
            return null;

        GameObject gun = prefab != null
            ? Object.Instantiate(prefab, hand)
            : GameObject.CreatePrimitive(PrimitiveType.Cube);
        if (prefab == null)
        {
            gun.name = "Tufek";
            gun.transform.SetParent(hand, false);
            Object.Destroy(gun.GetComponent<Collider>());
            gun.transform.localScale = new Vector3(0.08f, 0.08f, 0.55f);
        }

        gun.transform.localPosition = Vector3.zero;
        gun.transform.localRotation = Quaternion.identity;
        if (prefab != null)
            gun.transform.localScale = Vector3.one;
        Strip(gun);

        Vector3 axis = LongestAxis(gun.transform);
        Vector3 desired = hand.InverseTransformDirection(owner.forward);
        if (desired.sqrMagnitude < 0.001f)
            desired = Vector3.forward;
        desired.Normalize();
        if (Vector3.Dot(axis, desired) < 0f)
            axis = -axis;
        gun.transform.localRotation = Quaternion.FromToRotation(axis, desired);

        Bounds world = WorldBounds(gun);
        float length = Mathf.Max(world.size.x, Mathf.Max(world.size.y, world.size.z));
        if (length > 0.05f)
            gun.transform.localScale *= 0.78f / length;

        world = WorldBounds(gun);
        if (world.size.sqrMagnitude > 0.0001f)
        {
            Vector3 forward = owner.forward;
            float half = Support(world.extents, forward);
            Vector3 rear = world.center - forward * half;
            gun.transform.position += hand.position - rear;
        }

        return Nose(gun.transform, owner.forward);
    }

    static Transform Nose(Transform gun, Vector3 forward)
    {
        var tip = new GameObject("Agiz");
        tip.transform.SetParent(gun, false);
        Bounds world = WorldBounds(gun.gameObject);
        if (forward.sqrMagnitude < 0.001f)
            forward = gun.forward;
        forward.Normalize();
        if (world.size.sqrMagnitude > 0.0001f)
            tip.transform.position = world.center + forward * Support(world.extents, forward);
        else
            tip.transform.localPosition = new Vector3(0f, 0f, 0.2f);
        return tip.transform;
    }

    static Vector3 LongestAxis(Transform gun)
    {
        Vector3 best = Vector3.forward;
        float bestLen = 0f;
        MeshFilter[] filters = gun.GetComponentsInChildren<MeshFilter>();
        for (int i = 0; i < filters.Length; i++)
        {
            Mesh mesh = filters[i].sharedMesh;
            if (mesh == null)
                continue;
            Consider(gun, filters[i].transform, mesh.bounds.size, ref best, ref bestLen);
        }

        SkinnedMeshRenderer[] skinned = gun.GetComponentsInChildren<SkinnedMeshRenderer>();
        for (int i = 0; i < skinned.Length; i++)
        {
            Mesh mesh = skinned[i].sharedMesh;
            if (mesh == null)
                continue;
            Consider(gun, skinned[i].transform, mesh.bounds.size, ref best, ref bestLen);
        }

        return bestLen < 0.001f ? Vector3.forward : best.normalized;
    }

    static void Consider(Transform gun, Transform piece, Vector3 size, ref Vector3 best, ref float bestLen)
    {
        TryAxis(gun, piece, Vector3.right, size.x, ref best, ref bestLen);
        TryAxis(gun, piece, Vector3.up, size.y, ref best, ref bestLen);
        TryAxis(gun, piece, Vector3.forward, size.z, ref best, ref bestLen);
    }

    static void TryAxis(Transform gun, Transform piece, Vector3 local, float length, ref Vector3 best, ref float bestLen)
    {
        if (length <= bestLen)
            return;
        bestLen = length;
        best = gun.InverseTransformDirection(piece.TransformDirection(local));
    }

    static Bounds WorldBounds(GameObject gun)
    {
        Renderer[] renderers = gun.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return new Bounds(gun.transform.position, Vector3.one * 0.2f);
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);
        return bounds;
    }

    static float Support(Vector3 extents, Vector3 direction)
    {
        return Mathf.Abs(extents.x * direction.x) + Mathf.Abs(extents.y * direction.y) + Mathf.Abs(extents.z * direction.z);
    }

    static void Strip(GameObject gun)
    {
        Collider[] colliders = gun.GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++)
            colliders[i].enabled = false;
        Animator[] animators = gun.GetComponentsInChildren<Animator>();
        for (int i = 0; i < animators.Length; i++)
            animators[i].enabled = false;
    }

    static Transform Find(Transform rig, string boneName)
    {
        Transform[] bones = rig.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < bones.Length; i++)
        {
            if (bones[i].name == boneName)
                return bones[i];
        }
        return null;
    }
}
