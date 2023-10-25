using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GroupManager : MonoBehaviour
{
    [SerializeField] private List<NPCAIStateManager> _npcs = new List<NPCAIStateManager>();
    public Room currentRoom;

    [SerializeField] private int groupCapMin;
    [SerializeField] private int groupCapMax;

    private int _groupCapacity = 8;
    public int groupCapacity { get { return _groupCapacity; } }
    public List<NPCAIStateManager> npcs { get { return _npcs; } }

    // Start is called before the first frame update
    void Start()
    {
        // add to room in npcmanager
        currentRoom.AddGroup(this);
        _groupCapacity = Random.Range(groupCapMin, groupCapMax);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void JoinGroup(NPCAIStateManager npc)
    {
        npcs.Add(npc);
    }

    public void LeaveGroup(NPCAIStateManager npc)
    {
        npcs.Remove(npc);
        if(npcs.Count <= 0)
        {
            // remove from room in npcmanager
            currentRoom.RemoveGroup(this);
            Destroy(gameObject);
        }
    }

    public void MoveGroup(Vector3 pos)
    {
        currentRoom.RemoveGroup(this);
        transform.position = pos;
        foreach (NPCAIStateManager npc in npcs)
        {
            NavMeshPath path = new NavMeshPath();
            npc.agent.CalculatePath(pos, path);
            npc.nextPath = path;
        }
        foreach (NPCAIStateManager npc in npcs)
        {
            npc.followGroup = true;
        }
    }
}
