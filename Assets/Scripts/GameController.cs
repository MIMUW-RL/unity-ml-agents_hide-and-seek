using System.Collections.Generic;
using System.Linq;
using Unity.Barracuda;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private int episodeSteps = 240;
    [SerializeField] private float gracePeriodFraction = 0.4f;
    [SerializeField] private float coneAngle = 0.375f * 180f;

    [SerializeField] private MapGenerator mapGenerator = null;

    public enum IndividualRewardType
    { 
        None,  // no reward
        Solo,  // +1/-1 based on whether individual agent hides from/sees the other team, given per step
        Team   // +1/-1 based on whether the team is hidden from/sees the other team, given per step
    };
    public enum GroupRewardType
    {
        None,         // no reward
        PerStep,      // +1/-1 per step, based on whether the team is hidden/sees the other team
        WinCondition  // +1/-1 at the end of episode, based on how much time all hiders were hidden
    };
    [Header("Rewards")]
    [SerializeField] private IndividualRewardType individualRewardType = IndividualRewardType.None;
    [SerializeField] private float individualRewardMultiplier = 1.0f;
    [SerializeField] private GroupRewardType groupRewardType = GroupRewardType.None;
    [SerializeField] private float groupRewardMultiplier = 1.0f;
    [SerializeField] private float winConditionThreshold = 1.0f;

    [Header("Coplay")]
    [SerializeField] private bool useCoplay = false;
    [SerializeField] private int numberOfCoplayAgents = 1;
    [SerializeField] private float selfPlayRatio = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool debugDrawBoxHold = true;
    [SerializeField] private bool debugDrawVisibility = true;
    [SerializeField] private bool debugDrawIndividualReward = true;
    [SerializeField] private bool debugLogPlatformParams = true;
    [SerializeField] private bool debugLogMatchResult = false;
    [SerializeField] private TMPro.TextMeshProUGUI textMeshReward = null;


    private int episodeTimer = 0;
    private List<AgentActions> hiders;
    private List<AgentActions> seekers;
    private SimpleMultiAgentGroup hidersGroup;
    private SimpleMultiAgentGroup seekersGroup;
    private bool[,] visibilityMatrix;
    private List<BoxHolding> holdObjects;

    private int stepsHidden = 0;
    private bool hidersPerfectGame = true;
    private StatsRecorder statsRecorder = null;

    public int HidersGroupReward { get; private set; } = 0;
    public bool GracePeriodEnded
    {
        get { return episodeTimer >= episodeSteps * gracePeriodFraction; }
    }

    public bool DebugDrawBoxHold => debugDrawBoxHold;
    public bool DebugDrawVisibility => debugDrawVisibility;
    public bool DebugDrawIndividualReward => debugDrawIndividualReward;
    public bool DebugLogMatchResult => debugLogMatchResult;



    private void Start()
    {
        if (debugLogPlatformParams)
        {
            System.Console.WriteLine(gameObject.name);
            System.Console.WriteLine(JsonUtility.ToJson(this, true));
            if (mapGenerator != null)
            {
                System.Console.WriteLine(JsonUtility.ToJson(mapGenerator, true));
            }
            System.Console.WriteLine();
        }

        mapGenerator?.Generate();

        AgentActions[] allAgents = GetComponentsInChildren<AgentActions>();
        System.Array.ForEach(allAgents, (AgentActions agent) => agent.GameController = this);
        hiders = allAgents.Where((AgentActions a) => a.IsHiding).ToList();
        seekers = allAgents.Where((AgentActions a) => !a.IsHiding).ToList();
        visibilityMatrix = new bool[hiders.Count, seekers.Count];
        holdObjects = FindObjectsOfType<BoxHolding>().ToList();

        hidersGroup = new SimpleMultiAgentGroup();
        foreach (AgentActions hider in hiders)
        {
            hidersGroup.RegisterAgent(hider.GetComponent<HideAndSeekAgent>());
        }

        seekersGroup = new SimpleMultiAgentGroup();
        foreach (AgentActions seeker in seekers)
        {
            seekersGroup.RegisterAgent(seeker.GetComponent<HideAndSeekAgent>());
        }

        statsRecorder = Academy.Instance.StatsRecorder;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            ResetScene();
        }

        if (GracePeriodEnded)
        {
            if (textMeshReward != null)
            {
                textMeshReward.text = "Hiders reward: " + HidersGroupReward;
            }
        }
        else
        {
            if (textMeshReward != null)
            {
                textMeshReward.text = "Grace period";
            }
        }
    }

    private void FixedUpdate()
    {
        episodeTimer++;

        Vector3 seekersMeanPosition = Vector3.zero;
        foreach (AgentActions seeker in seekers)
        {
            seekersMeanPosition += seeker.transform.position - transform.position;
        }
        seekersMeanPosition /= seekers.Count;
        statsRecorder.Add("Environment/SeekersMeanX", seekersMeanPosition.x);
        statsRecorder.Add("Environment/SeekersMeanZ", seekersMeanPosition.z);

        Vector3 hidersMeanPosition = Vector3.zero;
        foreach (AgentActions hider in hiders)
        {
            hidersMeanPosition += hider.transform.position - transform.position;
        }
        hidersMeanPosition /= hiders.Count;
        statsRecorder.Add("Environment/HidersMeanX", hidersMeanPosition.x);
        statsRecorder.Add("Environment/HidersMeanZ", hidersMeanPosition.z);

        if (episodeTimer > episodeSteps)
        {
            float timeHidden = stepsHidden / (episodeSteps * (1f - gracePeriodFraction));
            bool hidersWon = winConditionThreshold >= 1f ? hidersPerfectGame : timeHidden > winConditionThreshold;
            if (debugLogMatchResult)
            {
                Debug.LogFormat("Team {0} won; Time percentage hidden - {1}", hidersWon ? "hiders" : "seekers", timeHidden * 100f);
            }
            statsRecorder.Add("Environment/TimeHidden", timeHidden);

            if (groupRewardType == GroupRewardType.WinCondition)
            {
                hidersGroup.AddGroupReward(hidersWon ? groupRewardMultiplier : -groupRewardMultiplier);
                seekersGroup.AddGroupReward(hidersWon ? -groupRewardMultiplier : groupRewardMultiplier);
            }
            statsRecorder.Add("Environment/HiderWinRatio", hidersWon ? 1 : 0);

            hidersGroup.EndGroupEpisode();
            seekersGroup.EndGroupEpisode();
            ResetScene();
        }
        else if (GracePeriodEnded)
        {
            FillVisibilityMatrix();
            HidersGroupReward = AreAllHidersHidden() ? 1 : -1;
            stepsHidden += HidersGroupReward > 0 ? 1 : 0;
            if (HidersGroupReward < 0)
            {
                hidersPerfectGame = false;
            }
            if (groupRewardType == GroupRewardType.PerStep)
            {
                hidersGroup.AddGroupReward(HidersGroupReward * groupRewardMultiplier);
                seekersGroup.AddGroupReward(-HidersGroupReward * groupRewardMultiplier);
            }
        }
        else
        {
            HidersGroupReward = 0;
        }
    }


    public IEnumerable<AgentActions> GetHiders()
    {
        foreach (AgentActions agent in hiders)
        {
            yield return agent;
        }
    }

    public IEnumerable<AgentActions> GetSeekers()
    {
        foreach (AgentActions agent in seekers)
        {
            yield return agent;
        }
    }

    public bool AgentSeesAgent(AgentActions agent1, AgentActions agent2, out RaycastHit hit)
    {
        Vector3 direction = agent2.transform.position - agent1.transform.position;
        if (Vector3.Angle(direction, agent1.transform.forward) > coneAngle)
        {
            hit = new RaycastHit();
            return false;
        }

        Ray ray = new Ray(agent1.transform.position, direction);
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject == agent2.gameObject)
            {
                return true;
            }
        }
        return false;
    }

    public float GetIndividualReward(AgentActions agent)
    {
        if (!GracePeriodEnded)
        {
            return 0;
        }

        if (individualRewardType == IndividualRewardType.None)
        {
            return 0;
        }
        if (individualRewardType == IndividualRewardType.Team)
        {
            return AreAllHidersHidden() == agent.IsHiding
                ?  individualRewardMultiplier
                : -individualRewardMultiplier;
        }

        for (int i = 0; i < hiders.Count; i++)
        {
            if (agent.GetInstanceID() == hiders[i].GetInstanceID())
            {
                for (int j = 0; j < seekers.Count; j++)
                {
                    if (visibilityMatrix[i, j])
                    {
                        return -individualRewardMultiplier;
                    }
                }
                return individualRewardMultiplier;
            }
        }

        for (int j = 0; j < seekers.Count; j++)
        {
            if (agent.GetInstanceID() == seekers[j].GetInstanceID())
            {
                for (int i = 0; i < hiders.Count; i++)
                {
                    if (visibilityMatrix[i, j])
                    {
                        return individualRewardMultiplier;
                    }
                }
                return -individualRewardMultiplier;
            }
        }

        return 0;
    }

    public void ResetScene()
    {
        stepsHidden = 0;
        hidersPerfectGame = true;
        HidersGroupReward = 0;
        episodeTimer = 0;
        if (mapGenerator == null || !mapGenerator.InstantiatesAgentsOnReset())
        {
            foreach (AgentActions agent in hiders)
            {
                agent.ResetAgent();
            }
            foreach (AgentActions agent in seekers)
            {
                agent.ResetAgent();
            }
        }

        if (mapGenerator == null || !mapGenerator.InstantiatesBoxesOnReset())
        {
            foreach (BoxHolding holdObject in holdObjects)
            {
                holdObject.Reset();
            }
        }

        mapGenerator?.Generate();

        if (mapGenerator != null && mapGenerator.InstantiatesAgentsOnReset())
        {
            hiders = ((MapGeneratorSimple)mapGenerator).GetInstantiatedHiders();
            seekers = ((MapGeneratorSimple)mapGenerator).GetInstantiatedSeekers();
            hiders.ForEach((AgentActions agent) => agent.GameController = this);
            seekers.ForEach((AgentActions agent) => agent.GameController = this);
            visibilityMatrix = new bool[hiders.Count, seekers.Count];

            hidersGroup = new SimpleMultiAgentGroup();
            foreach (AgentActions hider in hiders)
            {
                hidersGroup.RegisterAgent(hider.GetComponent<HideAndSeekAgent>());
            }

            seekersGroup = new SimpleMultiAgentGroup();
            foreach (AgentActions seeker in seekers)
            {
                seekersGroup.RegisterAgent(seeker.GetComponent<HideAndSeekAgent>());
            }
        }
        else
        {
            for (int i = 0; i < visibilityMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < visibilityMatrix.GetLength(1); j++)
                {
                    visibilityMatrix[i, j] = false;
                }
            }
        }

        if (useCoplay)
        {
            CoplayManager.Instance.Rescan();
            foreach (AgentActions agent in hiders)
            {
                agent.SwitchToTraining();
            }
            foreach (AgentActions agent in seekers)
            {
                agent.SwitchToTraining();
            }

            if (Random.Range(0f, 1f) < selfPlayRatio)
            {
                for (int i = 0; i < numberOfCoplayAgents; i++)
                {
                    NNModel model = CoplayManager.Instance.GetRandomModel();
                    if(model is not null){
                        (CoplayManager.TrainedTeamID == 0 ? hiders[i] : seekers[i]).SwitchToInference(model);
                        Debug.Log(gameObject.name.Split(' ').Last() + " set agent " + i + " (team" + CoplayManager.TrainedTeamID + ") model=" + model.name.Split("/").Last());
                    }
                }
            }else{
                //Debug.Log(gameObject.name + " not using coplay for team" + CoplayManager.TrainedTeamID);
            }
        }
    }


    private void FillVisibilityMatrix()
    {
        for (int i = 0; i < hiders.Count; i++)
        {
            for (int j = 0; j < seekers.Count; j++)
            {
                if (AgentSeesAgent(seekers[j], hiders[i], out RaycastHit hit))
                {
                    visibilityMatrix[i, j] = true;
                    if (debugDrawVisibility)
                    {
                        Debug.DrawLine(seekers[j].transform.position, hit.point, Color.red);
                    }
                }
                else
                {
                    visibilityMatrix[i, j] = false;
                }
            }
        }
    }

    private bool AreAllHidersHidden()
    {
        for (int i = 0; i < hiders.Count; i++)
        {
            for (int j = 0; j < seekers.Count; j++)
            {
                if (visibilityMatrix[i, j])
                {
                    return false;
                }
            }
        }
        return true;
    }
}
