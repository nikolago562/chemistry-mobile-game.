using UnityEngine;
using UnityEngine.Rendering;

namespace ChemistryGame
{
    // Keep the character behind tables when walking along their back edge.
    public sealed class PersonalLabDepth : MonoBehaviour
    {
        private PersonalLab lab;
        private SortingGroup group;
        private int classroomOrder;
        // Below classroom furniture (6+) but above floors, rugs and wall tiles (0-3).
        private const int BehindFurnitureOrder = 5;
        public void Initialize(PersonalLab owner)
        {
            lab = owner; group = GetComponent<SortingGroup>();
            if (group != null) classroomOrder = group.sortingOrder;
        }
        private void LateUpdate()
        {
            if (group == null || lab == null) return;
            int order = lab.Inside ? -50 - Mathf.RoundToInt((transform.position.y - lab.RoomOrigin.y + 0.08f) * 10)
                : DepthOccluders.Behind(transform.position) ? BehindFurnitureOrder : classroomOrder;
            if (group.sortingOrder != order) group.sortingOrder = order;
        }
    }
}
