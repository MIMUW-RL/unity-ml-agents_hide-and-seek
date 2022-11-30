using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; } = null;

    [SerializeField] private float gracePeriod = 10f;

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


    private bool AreAllHidersHidden()
    {
        bool retval = true;
        foreach (AgentActions seeker in seekers)
        {
            foreach (AgentActions hider in hiders)
            {
                Vector3 direction = hider.transform.position - seeker.transform.position;
                Ray ray = new Ray(seeker.transform.position, direction);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (hit.collider.gameObject == hider.gameObject)
                    {
                        Debug.DrawLine(seeker.transform.position, hit.point, Color.red);
                        retval = false;
                    }
                }
            }
        }
        return retval;
    }
}
