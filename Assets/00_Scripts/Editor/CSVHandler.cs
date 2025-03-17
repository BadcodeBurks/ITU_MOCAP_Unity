using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace Burk
{
    public static class RecordsHandler
    {
        const string C_RECORDS_FOLDER_NAME = "MocapRecords";
        static string folderPath = Path.Combine("Assets", C_RECORDS_FOLDER_NAME);
        public class RecordWrapper
        {
            public string name;
            public string path;
            public BufferRecording recording;
        }
        static Dictionary<string, RecordWrapper> _recordWrappers = new Dictionary<string, RecordWrapper>();
        public static List<string> RecordNames => _recordWrappers.Keys.ToList();


        [InitializeOnLoadMethod]
        static void Init()
        {
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                CreateDirectoryInProject();
                AssetDatabase.Refresh();
            }
            LoadAllRecords();
        }

        static void CreateDirectoryInProject()
        {
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.Log("Creating folder: " + folderPath);
                AssetDatabase.CreateFolder("Assets", C_RECORDS_FOLDER_NAME);
            }
        }

        public static string[] GetRecordPaths()
        {
            string[] guids = AssetDatabase.FindAssets("", new[] { folderPath });

            if (guids.Length == 0)
            {
                Debug.Log("No recording files found.");
                return null;
            }
            List<string> paths = new List<string>();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                paths.Add(path);
            }
            return paths.OrderBy(x => x).ToArray();
        }

        public static List<BufferRecording> LoadAllRecords()
        {
            string[] paths = GetRecordPaths();
            if (paths == null) return null;
            BufferRecording[] recordings = new BufferRecording[paths.Length];
            _recordWrappers.Clear();
            for (int i = 0; i < paths.Length; i++)
            {
                recordings[i] = LoadRecording(paths[i]);
                _recordWrappers.Add(recordings[i].Name, new RecordWrapper() { name = recordings[i].Name, recording = recordings[i], path = paths[i] });
            }
            return recordings.ToList();
        }

        public static BufferRecording LoadRecording(string path)
        {
            TextAsset file = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            if (file == null) return null;
            BufferRecording recording = new BufferRecording(file.name);
            string csvData = file.text;
            string[] rows = csvData.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string row in rows)
            {
                // Split each row by commas
                string[] values = row.Split(',');

                if (values.Length > 1)
                {
                    // Parse timestamp (first value)
                    double timestamp = double.Parse(values[0]);

                    // Parse buffer values (the remaining values)
                    float[] bufferValues = new float[values.Length - 1];
                    for (int i = 1; i < values.Length; i++)
                    {
                        bufferValues[i - 1] = float.Parse(values[i]);
                    }
                    recording.AddRecordFrame(bufferValues, timestamp);
                }
            }
            return recording;
        }

        public static string GetProjectPath() // No Longer used, delete if unnecessary
        {
            // This gives the path to the 'Assets' folder in the project
            string projectPath = Application.dataPath;
            // This gives the root path of the project
            string fullProjectPath = projectPath.Substring(0, projectPath.Length - "/Assets".Length);

            Debug.Log("Project Path: " + fullProjectPath);

            return fullProjectPath;
        }

        private static string GetRecordingString(BufferRecording recording)
        {
            string csvData = "";
            for (int i = 0; i < recording.GetFrameCount(); i++)
            {
                csvData += recording.GetTimeStamp(i);
                for (int j = 0; j < recording.GetValues(i).Length; j++)
                {
                    csvData += "," + recording.GetValues(i)[j];
                }
                csvData += "\n";
            }
            return csvData;
        }

        public static void SaveRecordingAsCSV(BufferRecording recording, string fileName)
        {
            string filePath = Path.Combine(folderPath, fileName + ".asset");
            string csvData = GetRecordingString(recording);
            TextAsset textAsset = new TextAsset(csvData);
            AssetDatabase.CreateAsset(textAsset, filePath);
            AssetDatabase.Refresh();
            LoadAllRecords();
        }

        public static bool CheckFileExists(string fileName)
        {
            string filePath = Path.Combine(folderPath, fileName + ".csv");
            return AssetDatabase.LoadAssetAtPath<TextAsset>(filePath) != null;
        }

        internal static BufferRecording GetRecording(string recordName)
        {
            return _recordWrappers[recordName].recording;
        }

        public static void ExtractAllRecords()
        {
            string path = EditorUtility.SaveFolderPanel("Select Folder", "", "New Recording Folder");
            if (string.IsNullOrEmpty(path)) return;
            foreach (RecordWrapper record in _recordWrappers.Values)
            {
                string recordFilePath = Path.Combine(path, record.name + ".csv");
                string csvData = GetRecordingString(record.recording);
                File.WriteAllText(recordFilePath, csvData);
            }
            Debug.Log("Extracted all records to " + path);
        }
    }
}