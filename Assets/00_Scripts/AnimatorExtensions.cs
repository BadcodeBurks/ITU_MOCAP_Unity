using UnityEngine;

public static class AnimatorExtensions
{
    public static bool TryGetNameHash(this Animator animator, string name, out int hash)
    {
        AnimatorControllerParameter[] parameters = animator.parameters;
        for (byte i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].name == name)
            {
                hash = parameters[i].nameHash;
                return true;
            }
        }
        hash = 0;
        return false;
    }
}