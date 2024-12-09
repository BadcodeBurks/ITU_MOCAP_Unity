using UnityEditor;

namespace Burk
{
    public static class CreateScriptTemplates
    {
        [MenuItem("Assets/Create/Template/MonoBehaviour", priority = 40)]
        public static void CreateMonoBehaviourMenuItem()
        {
            string templatePath = "Assets/Editor/Templates/MonoBehaviour.cs.txt";

            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, "NewScript.cs");
        }
    }
}
