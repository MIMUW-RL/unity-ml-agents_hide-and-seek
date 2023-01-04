using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; } = null;

    [SerializeField] private float gracePeriod = 10f;
    [SerializeField] private float coneAngle = 0.375f * 180f;

    [SerializeField] private TMPro.TextMeshProUGUI textMeshReward = null;

    private float graceTimer = 0f;
    private List<AgentActions> hiders;
    private List<AgentActions> seekers;

    public int hidersReward { get; private set; } = 0;
    public bool GracePeriodEnded
    {
        get { return graceTimer <= 0f; }
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
        graceTimer = gracePeriod;
        hiders = FindObjectsOfType<AgentActions>().Where((AgentActions a) => a.IsHiding).ToList();
        seekers = FindObjectsOfType<AgentActions>().Where((AgentActions a) => !a.IsHiding).ToList();
    }

    private void Update()
    {
        if (graceTimer > 0f)
        {
            graceTimer -= Time.deltaTime;
            return;
        }

        hidersReward = AreAllHidersHidden() ? 1 : -1;
        textMeshReward.text = "Hiders reward: " + hidersReward;
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
