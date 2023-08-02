using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Barracuda;
using Unity.Barracuda.ONNX;
using UnityEngine;


public class CoplayManager : MonoBehaviour
{
    public static CoplayManager Instance { get; private set; } = null;

    public static int TrainedTeamID { get; private set; } = 0;

    private string trainerStatusPath = null;
    private string modelsPath = null;
    private List<NNModel> checkpoints = null;
    private HashSet<string> checkpointNames = null;


    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("Multiple CoplayManagers present in scene!");
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        foreach (string arg in System.Environment.GetCommandLineArgs())
        {
            if (arg.StartsWith("trainer_status="))
            {
                trainerStatusPath = arg[15..];
            }
            if (arg.StartsWith("models_path="))
            {
                modelsPath = arg[12..];
            }
        }
        
        if (trainerStatusPath == null)
        {
            Debug.LogWarning("No trainer_status env arg provided!");
        }
        else
        {
            Debug.Log("trainer_status: " + trainerStatusPath);
        }

        if (modelsPath == null)
        {
            Debug.LogWarning("No models_path evn arg provided!");
        }
        else
        {
            Debug.Log("models_path: " + modelsPath);
        }

        checkpoints = new List<NNModel>();
        checkpointNames = new HashSet<string>();
    }


    public void Rescan()
    {
        string[] trainerStatusData = File.ReadAllLines(trainerStatusPath);
        foreach (string line in trainerStatusData)
        {
            if (line.StartsWith("learningTeamId: "))
            {
                TrainedTeamID = int.Parse(line[16..]);
            }
        }

        string[] files = Directory.GetFiles(modelsPath, "*.onnx");
        bool appendedNew = false;
        foreach (string file in files)
        {
            if (!checkpointNames.Contains(file))
            {
                checkpointNames.Add(file);
                
                NNModel model = ScriptableObject.CreateInstance<NNModel>();
                model.modelData = ScriptableObject.CreateInstance<NNModelData>();
                ONNXModelConverter conv = new ONNXModelConverter(true);
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        ModelWriter.Save(writer, conv.Convert(File.ReadAllBytes(file)));
                        model.modelData.Value = stream.ToArray();
                    }
                }
                model.modelData.hideFlags = HideFlags.HideInHierarchy;
                model.modelData.name = "Data";
                model.name = file;
                Debug.Log("Found new model in modelPath, appending to checkpoints");
                Debug.Log(file);                
                appendedNew = true;
                checkpoints.Add(model);
                Debug.Log(string.Concat("checkpoints size = ", checkpoints.Count));
            }
        }
    }

    public NNModel GetRandomModel()
    {
        if (checkpoints.Count > 0)
        {
            return checkpoints[Random.Range(0, checkpoints.Count)];
        }
        return null;
    }
}
