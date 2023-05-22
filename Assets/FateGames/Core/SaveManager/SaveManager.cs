using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;

namespace FateGames.Core
{
    public class SaveManager
    {
        private SaveDataVariable saveData, overrideSaveData;

        public SaveManager(SaveDataVariable saveData, SaveDataVariable overrideSaveData)
        {
            this.saveData = saveData;
            this.overrideSaveData = overrideSaveData;
        }

        public void SaveToDevice(SaveData data)
        {
            BinaryFormatter formatter = new();
            string path = Application.persistentDataPath + "/saveData.fate";
            FileStream stream = new(path, FileMode.Create);
            formatter.Serialize(stream, data);
            stream.Close();
            Debug.Log("Saved");
        }

        public void Load(bool overrideSave = false)
        {
            if (!overrideSave)
            {
                string path = Application.persistentDataPath + "/saveData.fate";
                if (File.Exists(path))
                {
                    BinaryFormatter formatter = new();
                    FileStream stream = new(path, FileMode.Open);
                    stream.Position = 0;
                    SaveData data = formatter.Deserialize(stream) as SaveData;
                    stream.Close();
                    saveData.Value = data;
                }
                else
                {
                    SaveData data = new();
                    saveData.Value = data;
                    SaveToDevice(data);
                }
            }
            else
            {
                SaveData data;
                if (overrideSaveData == null) data = new();
                else data = CloneSave(overrideSaveData.Value);

                saveData.Value = data;
            }
        }

        public SaveData CloneSave(SaveData original)
        {
            // Serialize
            BinaryFormatter formatter = new();
            string path = Application.persistentDataPath + "/cloneSaveData.fate";
            FileStream serializationStream = new(path, FileMode.Create);
            formatter.Serialize(serializationStream, original);
            serializationStream.Close();

            // Deserialize
            FileStream deserializationStream = new(path, FileMode.Open);
            deserializationStream.Position = 0;
            SaveData clone = formatter.Deserialize(deserializationStream) as SaveData;
            deserializationStream.Close();
            if (File.Exists(path))
                File.Delete(path);
            return clone;
        }
#if UNITY_EDITOR
        [MenuItem("Fate/Delete Player Data")]
        static void DeletePlayerData()
        {
            string path = Application.persistentDataPath + "/saveData.fate";
            if (File.Exists(path))
                File.Delete(path);
        }
#endif
    }
}
