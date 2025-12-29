namespace Game.DataJson.Loader
{
    using UnityEngine;
    using System.IO;
    using Game.DataJson.Schema;

    public class JsonDataLoader : MonoBehaviour
    {
        [Header("Loading Path")]
        [SerializeField] private DataLoadingPath loadingPath = DataLoadingPath.StreamingAssets;

        [Header("File Names")]
        [SerializeField] private string monstersFileName = "monsters.json";

        public MonstersData LoadMonsters()
        {
            string path = GetFullPath(monstersFileName);

            if (!File.Exists(path))
            {
                Debug.LogError($"[JsonDataLoader] File not found: {path}");
                return null;
            }

            try
            {
                string json = File.ReadAllText(path);
                MonstersData data = JsonUtility.FromJson<MonstersData>(json);

                Debug.Log($"[JsonDataLoader] Loaded {data.monsters.Count} monsters from {path}");
                return data;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[JsonDataLoader] Failed to load monsters: {ex.Message}");
                return null;
            }
        }

        private string GetFullPath(string fileName)
        {
            switch (loadingPath)
            {
                case DataLoadingPath.StreamingAssets:
                    return Path.Combine(Application.streamingAssetsPath, "GameData", fileName);

                case DataLoadingPath.Resources:
                    string resourceName = Path.GetFileNameWithoutExtension(fileName);
                    TextAsset textAsset = Resources.Load<TextAsset>($"GameData/{resourceName}");
                    if (textAsset != null)
                    {
                        string tempPath = Path.Combine(Application.temporaryCachePath, fileName);
                        File.WriteAllText(tempPath, textAsset.text);
                        return tempPath;
                    }
                    return null;

                default:
                    return Path.Combine(Application.streamingAssetsPath, "GameData", fileName);
            }
        }
    }

    public enum DataLoadingPath
    {
        StreamingAssets,
        Resources
    }
}
