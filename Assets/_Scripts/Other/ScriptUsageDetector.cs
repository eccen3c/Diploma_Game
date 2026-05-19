#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class ScriptUsageDetector : EditorWindow
{
    [MenuItem("Tools/Найти неиспользуемые скрипты")]
    public static void ShowWindow()
    {
        // 1. Ищем все .cs файлы в проекте
        string[] allScriptPaths = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
        List<string> allScriptNames = new List<string>();

        foreach (string path in allScriptPaths)
        {
            // Исключаем системные скрипты и этот же скрипт
            string name = Path.GetFileNameWithoutExtension(path);
            if (name != "ScriptUsageDetector" && !path.Contains("Editor"))
            {
                allScriptNames.Add(name);
            }
        }

        // 2. Ищем используемые скрипты (На сцене и в Префабах)
        HashSet<string> usedScripts = new HashSet<string>();

        // А) Сканируем текущую сцену
        MonoBehaviour[] sceneObjects = FindObjectsOfType<MonoBehaviour>(true);
        foreach (var script in sceneObjects)
        {
            if (script != null) usedScripts.Add(script.GetType().Name);
        }

        // Б) Сканируем ВСЕ префабы в проекте
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab != null)
            {
                MonoBehaviour[] prefabScripts = prefab.GetComponentsInChildren<MonoBehaviour>(true);
                foreach (var script in prefabScripts)
                {
                    if (script != null) usedScripts.Add(script.GetType().Name);
                }
            }
        }

        // В) Учитываем ScriptableObjects (UnitData)
        // Они не висят на объектах, но они используются системой
        usedScripts.Add("UnitData");

        // 3. Сравниваем и выводим результат
        Debug.Log("--- ОТЧЕТ ПО СКРИПТАМ ---");

        foreach (string scriptName in allScriptNames)
        {
            if (usedScripts.Contains(scriptName))
            {
                // Если хочешь видеть и используемые, раскомментируй строку ниже:
                // Debug.Log($"? ИСПОЛЬЗУЕТСЯ: {scriptName}");
            }
            else
            {
                Debug.LogError($"? НЕ ИСПОЛЬЗУЕТСЯ (Кандидат на удаление): {scriptName}");
            }
        }

        Debug.Log("--- КОНЕЦ ОТЧЕТА ---");
    }
}
#endif