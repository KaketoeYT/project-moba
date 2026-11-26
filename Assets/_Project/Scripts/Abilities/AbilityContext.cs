using UnityEngine;

// Everything an ability needs to know about one cast.
// A struct, so casting never allocates memory.
public readonly struct AbilityContext
{
    public readonly AbilityCaster Caster;

    // World point being aimed at (usually under the mouse)
    public readonly Vector3 AimPoint;

    // Flat (XZ) normalized direction from the cast origin to the aim point.
    // Vector3.zero if the aim point is right on top of the caster.
    public readonly Vector3 AimDirection;

    public AbilityContext(AbilityCaster caster, Vector3 aimPoint)
    {
        Caster = caster;
        AimPoint = aimPoint;

        Vector3 flat = aimPoint - caster.CastOrigin.position;
        flat.y = 0f;
        AimDirection = flat.sqrMagnitude < 0.0001f ? Vector3.zero : flat.normalized;
    }

    public GameObject Owner => Caster.gameObject;
    public Vector3 Origin => Caster.CastOrigin.position;
}