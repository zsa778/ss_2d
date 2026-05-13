using UnityEditor;
using UnityEngine;
using SliceShoot.Monster;

public static class DogPrefabSetup
{
    [MenuItem("SliceShoot/Setup Dog Prefab")]
    static void Setup()
    {
        const string prefabPath = "Assets/Prefabs/Monsters/Dog.prefab";
        const string dataPath = "Assets/Scriptable Objects/Monsters/Common/Dog.asset";

        var prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefabAsset == null) { Debug.LogError("Dog.prefab not found at " + prefabPath); return; }

        var dogData = AssetDatabase.LoadAssetAtPath<MonsterData>(dataPath);
        if (dogData == null) { Debug.LogError("Dog.asset not found at " + dataPath); return; }

        bool changed = false;
        using (var scope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
        {
            var root = scope.prefabContentsRoot;

            if (root.GetComponent<Monster>() == null)
            {
                root.AddComponent<Monster>();
                changed = true;
            }
        }

        if (changed)
            Debug.Log("Dog.prefab: added Monster component.");

        var savedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        var monsterComp = savedPrefab.GetComponent<Monster>();
        if (monsterComp == null) { Debug.LogError("Monster component not found after save."); return; }

        dogData.prefab = monsterComp;
        EditorUtility.SetDirty(dogData);
        AssetDatabase.SaveAssets();

        Debug.Log("Dog.asset.prefab linked to Dog.prefab Monster component.");
    }
}
