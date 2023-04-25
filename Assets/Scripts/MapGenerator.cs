using UnityEngine;

public abstract class MapGenerator : MonoBehaviour
{
    public abstract void Generate();

    public abstract bool InstantiatesAgentsOnReset();
    public abstract bool InstantiatesBoxesOnReset();
}
