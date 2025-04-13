using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
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
            BufferMetadata meta = new BufferMetadata();
            string csvData = file.text;
            string[] rows = csvData.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            Debug.Log("reading: " + file.name);
            int recordStartRow = 3;
            int[] counts = rows[0].Split(',').Select(x => int.Parse(x)).ToArray();
            meta.tensionCount = counts[0];
            meta.imuCount = counts[1];
            meta.keys = rows[1].Split(',').ToList();
            meta.useRaw = bool.Parse(rows[2]);
            if (meta.useRaw)
            {
                recordStartRow = 4;
                meta.tensionCalibrations = new List<Vector2>();
                float[] calibrations = rows[3].Split(',').Select(x => float.Parse(x)).ToArray();
                for (int i = 0; i < meta.tensionCount; i++)
                {
                    meta.tensionCalibrations.Add(new Vector2(calibrations[i * 2], calibrations[i * 2 + 1]));
                }
            }
            for (int j = recordStartRow; j < rows.Length; j++)
            {
                string row = rows[j];
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
            recording.AddBufferData(meta);
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
            csvData += recording.BufferData.tensionCount + "," + recording.BufferData.imuCount + "\n";
            for (int i = 0; i < recording.BufferData.keys.Count; i++)
            {
                csvData += recording.BufferData.keys[i];
                if (i < recording.BufferData.keys.Count - 1) csvData += ",";
            }
            csvData += "\n";
            csvData += recording.BufferData.useRaw;
            csvData += "\n";
            if (recording.BufferData.useRaw)
            {
                for (int i = 0; i < recording.BufferData.tensionCalibrations.Count; i++)
                {
                    csvData += recording.BufferData.tensionCalibrations[i].x + "," + recording.BufferData.tensionCalibrations[i].y;
                    if (i < recording.BufferData.tensionCalibrations.Count - 1) csvData += ",";
                }
                csvData += "\n";
            }
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