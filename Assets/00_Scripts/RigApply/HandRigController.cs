using UnityEngine;

namespace Burk
{
    public class HandRigController : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        private void OnDrawGizmos()
        {
            Transform root = animator.avatarRoot;
            Gizmos.DrawSphere(root.position, 0.1f);
            CheckTransform(root, 0);
        }

        private bool CheckTransform(Transform r, int l)
        {
            if (r.childCount == 0 || l > 5) return false;
            for (int i = 0; i < r.childCount; i++)
            {
                Transform t = r.GetChild(i);
                Gizmos.DrawLine(r.position, t.position);
                if (!CheckTransform(t, l + 1))
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(t.position, 0.1f / (l + 1));
                    Gizmos.color = Color.white;
                }
            }
            return true;
        }
    }
}
