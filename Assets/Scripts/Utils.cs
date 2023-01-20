using System.IO;

using UnityEditor;
using UnityEngine;

public class Utils {
    public static void SaveGameObjectAsPrefab(GameObject go, string name)
    {
        // TODO: Does this need to be disabled for built applications?
        if (!Directory.Exists("Assets/Generated"))
        {
            AssetDatabase.CreateFolder("Assets", "Generated");
        }
        string prefabPath = Application.dataPath + $"/Generated/{name}.prefab";
        bool prefabSavingSuccess;
        PrefabUtility.SaveAsPrefabAssetAndConnect(go, prefabPath, InteractionMode.UserAction, out prefabSavingSuccess);
        if (prefabSavingSuccess == true)
        {
            Debug.Log("Prefab was saved successfully");
        } else {
            Debug.LogWarning("Prefab failed to save to " + prefabPath);
        }
    }
}
