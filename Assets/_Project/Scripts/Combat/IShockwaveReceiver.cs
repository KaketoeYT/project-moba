using UnityEngine;

namespace _Project.Scripts.Combat
{
    // Implement on anything that should react to a shockwave (walls, barrels...)
    public interface IShockwaveReceiver
    {
        // point: where the shockwave started.
        // strength: 1 at the centre of the blast, 0 at the edge of its radius.
        void OnShockwave(Vector3 point, float strength);
    }
}