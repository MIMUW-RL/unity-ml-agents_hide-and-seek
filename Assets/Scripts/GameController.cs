using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; } = null;

    [SerializeField] private int episodeSteps = 2000;
    [SerializeField] private float gracePeriodFraction = 0.4f;
    [SerializeField] private float coneAngle = 0.375f * 180f;

    [SerializeField] private TMPro.TextMeshProUGUI textMeshReward = null;

    private int episodeTimer = 0;
    private List<AgentActions> hiders;
    private List<AgentActions> seekers;
    private SimpleMultiAgentGroup hidersGroup;
    private SimpleMultiAgentGroup seekersGroup;
    private List<BoxHolding> holdObjects;

    public int HidersReward { get; private set; } = 0;
    public bool GracePeriodEnded
    {
        get { return episodeTimer >= episodeSteps * gracePeriodFraction; }
    }



    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple GameController instances found in a scene");
            Destroy(this);
        }
    }

    private void Start()
    {
        hiders = FindObjectsOfType<AgentActions>().Where((AgentActions a) => a.IsHiding).ToList();
        seekers = FindObjectsOfType<AgentActions>().Where((AgentActions a) => !a.IsHiding).ToList();
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
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            ResetScene();
        }

        if (GracePeriodEnded)
        {
            HidersReward = AreAllHidersHidden() ? 1 : -1;
            textMeshReward.text = "Hiders reward: " + HidersReward;
        }
        else
        {
            HidersReward = 0;
            textMeshReward.text = "Grace period";
        }
    }

    private void FixedUpdate()
    {
        episodeTimer++;

        if (episodeTimer >= episodeSteps)
        {
            hidersGroup.EndGroupEpisode();
            seekersGroup.EndGroupEpisode();
            ResetScene();
        }
        else if (GracePeriodEnded)
        {
            hidersGroup.AddGroupReward(HidersReward);
            seekersGroup.AddGroupReward(-HidersReward);
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


    public void ResetScene()
    {
        episodeTimer = 0;
        foreach (AgentActions agent in hiders)
        {
            agent.ResetAgent();
        }
        foreach (AgentActions agent in seekers)
        {
            agent.ResetAgent();
        }
        foreach (BoxHolding holdObject in holdObjects)
        {
            holdObject.Reset();
        }
    }


    private bool AreAllHidersHidden()
    {
        bool retval = true;
        foreach (AgentActions seeker in seekers)
        {
            foreach (AgentActions hider in hiders)
            {
                if (AgentSeesAgent(seeker, hider, out RaycastHit hit))
                {
                    Debug.DrawLine(seeker.transform.position, hit.point, Color.red);
                    retval = false;
                }
            }
        }
        return retval;
    }
}
