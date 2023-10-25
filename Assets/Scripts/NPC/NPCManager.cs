using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public class Room
{
    public Transform center;
    [HideInInspector] public Transform perimeter;
    public bool isCircle;
    [HideInInspector] public float rngWeight;
    public bool isTargetNPCRoom;
    public bool isInGate;
    [HideInInspector] public int targetNPCAmount;

    private List<GroupManager> _groups = new List<GroupManager>();

    public List<GroupManager> groups { get { return _groups; } }

    public float GetRadius()
    {
        return Mathf.Abs(center.position.x - perimeter.position.x);
    }

    public float GetArea()
    {
        if(isCircle) return Mathf.Pow(GetRadius(), 2) * Mathf.PI;
        else
        {
            float x = 2 * Mathf.Abs(center.position.x - perimeter.position.x);
            float z = 2 * Mathf.Abs(center.position.z - perimeter.position.z);
            //Debug.Log(x);
            //Debug.Log(z);
            return x*z;
        }
    }

    public void AddGroup(GroupManager group)
    {
        groups.Add(group);
    }

    public void RemoveGroup(GroupManager group)
    {
        groups.Remove(group);
    }
}

public class NPCManager : MonoBehaviour
{
    [SerializeField] private GameObject NPCPrefab;
    [SerializeField] private GameObject JanitorPrefab;
    [SerializeField] private GameObject PolicePrefab;
    [SerializeField] private GameObject SecurityPrefab;
    [SerializeField] private GameObject TargetPrefab;
    [SerializeField] private GameObject BossPrefab;

    [SerializeField] private List<Room> _rooms = new List<Room>();
    public List<Room> rooms { get { return _rooms; } }

    private List<NPCAIStateManager> _npcs = new List<NPCAIStateManager>();
    private List<NPCAIStateManager> _janitors = new List<NPCAIStateManager>();
    private List<NPCAIStateManager> _police = new List<NPCAIStateManager>();
    private List<NPCAIStateManager> _security = new List<NPCAIStateManager>();
    private List<NPCAIStateManager> _targets = new List<NPCAIStateManager>();
    private NPCAIStateManager _boss;

    public List<NPCAIStateManager> police { get { return _police; } }

    [SerializeField] private bool _spawnNPCs;
    public bool spawnNPCs { get { return _spawnNPCs; } set { _spawnNPCs = value; } }
    [SerializeField] private int npcCount;
    [SerializeField] private int janitorCount;
    //[SerializeField] private int policeCount;
    [SerializeField] private int securityPairCount;
    [SerializeField] private int targetsCount;
    //[SerializeField] private bool spawnBoss;

    [SerializeField] private GameObject NPCParent;
    [SerializeField] private GameObject _GroupParent;

    public GameObject GroupParent { get { return _GroupParent; } }

    //public Room tutorialCafeteria = new Room();

    // Start is called before the first frame update
    void Start()
    {
        AllocateRoomWeights();
        if(spawnNPCs) ResetNPCs();
        //else tutorialCafeteria.perimeter = tutorialCafeteria.center.GetChild(0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Get Random Point on a Navmesh surface
    public static Vector3 GetRandomPoint(Room room)
    {
        NavMeshHit hit; // NavMesh Sampling Info Container
        bool foundPosition;
        // from randomPos find a nearest point on NavMesh surface in range of maxDistance
        if (room.isCircle) 
        {
            foundPosition = NavMesh.SamplePosition(room.center.position + UnityEngine.Random.insideUnitSphere * room.GetRadius(), out hit, room.GetRadius(), 1); 
        }
        else 
        {
            Vector3 randomPos;
            randomPos.x = UnityEngine.Random.Range(room.perimeter.position.x, room.perimeter.position.x + 2*Mathf.Abs(room.center.position.x - room.perimeter.position.x));
            randomPos.y = room.center.position.y;
            randomPos.z = UnityEngine.Random.Range(room.perimeter.position.z, room.perimeter.position.z + 2*Mathf.Abs(room.center.position.z - room.perimeter.position.z));
            foundPosition = NavMesh.SamplePosition(randomPos, out hit, room.GetRadius(), 1);
        }

        if (!foundPosition) return GetRandomPoint(room);
        // throw exception, return false, retry, some other way to handle failed sample
        
        /*
        NavMeshPath path = new NavMeshPath();
        this.navmeshAgent.CalculatePath(navMeshHit.position, path);
        canReachPoint = path.status == NavMeshPathStatus.PathComplete;
        */

        return hit.position;
    }

    [ContextMenu("ResetNPCs")]
    public void ResetNPCs()
    {
        foreach (Transform NPC in NPCParent.transform) Destroy(NPC.gameObject);

        for (int i = 0; i < npcCount; i++)
        {
            Room room = WeightedRandomRoom();
            GameObject npc = Instantiate(NPCPrefab, GetRandomPoint(room), Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0), NPCParent.transform);
            npc.GetComponent<NPCAIStateManager>().currentRoom = room;
            _npcs.Add(npc.GetComponent<NPCAIStateManager>());
            if (i <= (npcCount/3)) npc.GetComponent<NPCAIStateManager>().selectedAction = NPCAIStateManager.NPCAction.walkToRoom;
            else if(i <= (npcCount*2/3)) npc.GetComponent<NPCAIStateManager>().selectedAction = NPCAIStateManager.NPCAction.walk;

            AssignAgentLayerMasks(room, npc.GetComponent<NavMeshAgent>());
        }

        for (int i = 0; i < janitorCount; i++)
        {
            Room room = WeightedRandomRoom();
            GameObject npc = Instantiate(JanitorPrefab, GetRandomPoint(room), Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0), NPCParent.transform);
            npc.GetComponent<NPCAIStateManager>().currentRoom = room;
            _janitors.Add(npc.GetComponent<NPCAIStateManager>());
            npc.GetComponent<NPCAIStateManager>().selectedAction = NPCAIStateManager.NPCAction.walk;

            AssignAgentLayerMasks(room, npc.GetComponent<NavMeshAgent>());
        }

        /*for (int i = 0; i < policeCount; i++)
        {
            Room room = WeightedRandomRoom();
            GameObject npc = Instantiate(PolicePrefab, GetRandomPoint(room), Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0), NPCParent.transform);
            npc.GetComponent<NPCAIStateManager>().currentRoom = room;
            _police.Add(npc.GetComponent<NPCAIStateManager>());

            AssignAgentLayerMasks(room, npc.GetComponent<NavMeshAgent>());
        }*/

        for (int i = 0; i < securityPairCount; i++)
        {
            Room room = WeightedRandomRoom();
            GameObject npc = Instantiate(SecurityPrefab, GetRandomPoint(room), Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0), NPCParent.transform);
            GameObject npc2 = Instantiate(SecurityPrefab, npc.transform.position+(Vector3.one/2), npc.transform.rotation, NPCParent.transform);
            npc.GetComponent<NPCAIStateManager>().patrolPartner = npc2;
            npc2.GetComponent<NPCAIStateManager>().patrolPartner = npc;
            npc.GetComponent<NPCAIStateManager>().currentRoom = room;
            npc2.GetComponent<NPCAIStateManager>().currentRoom = room;
            _security.Add(npc.GetComponent<NPCAIStateManager>());
            _security.Add(npc2.GetComponent<NPCAIStateManager>());
            npc.GetComponent<NPCAIStateManager>().isLeadingPatrol = true;
            
            
            //npc2.GetComponent<NavMeshAgent>().speed += npc2.GetComponent<NPCAIStateManager>().securitySpeedInc;

            //AssignAgentLayerMasks(room, npc.GetComponent<NavMeshAgent>());
            //AssignAgentLayerMasks(room, npc2.GetComponent<NavMeshAgent>());
        }

        foreach(Room room in _rooms)
        {
            for (int i = 0; i < room.targetNPCAmount; i++)
            {
                GameObject npc = Instantiate(TargetPrefab, GetRandomPoint(room), Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0), NPCParent.transform);
                npc.GetComponent<NPCAIStateManager>().currentRoom = room;
                _targets.Add(npc.GetComponent<NPCAIStateManager>());

                AssignAgentLayerMasks(room, npc.GetComponent<NavMeshAgent>());
            }
        }

        /*if (spawnBoss)
        {
            Room room = WeightedRandomRoom();
            GameObject npc = Instantiate(BossPrefab, GetRandomPoint(room), Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0), NPCParent.transform);
            npc.GetComponent<NPCAIStateManager>().currentRoom = room;
            _boss = npc.GetComponent<NPCAIStateManager>();
        }*/
    }

    private void AllocateRoomWeights()
    {
        float totalArea = 0;
        int targetRooms = 0;
        foreach(Room room in _rooms)
        {
            room.perimeter = room.center.GetChild(0);
            totalArea += room.GetArea();
            if (room.isTargetNPCRoom) targetRooms++;

            room.center.GetComponent<RoomTrigger>().room = room;
        }

        int targetPerRoom = targetsCount / targetRooms;
        int remainder = targetsCount % targetRooms;

        _rooms.Shuffle();

        foreach(Room room in _rooms)
        {
            room.rngWeight =  room.GetArea() / totalArea;
            if (room.isTargetNPCRoom)
            {
                room.targetNPCAmount = targetPerRoom;
                if (remainder > 0)
                {
                    room.targetNPCAmount++;
                    remainder--;
                }
            }
        }

        _rooms.Sort((r1, r2)=>r1.rngWeight.CompareTo(r2.rngWeight));
    }

    public Room WeightedRandomRoom()
    {
        float rng = UnityEngine.Random.Range(0f, 1f);
        float weights = 0;
        foreach(Room room in _rooms)
        {
            weights += room.rngWeight;
            if (weights > rng)
            {
                return room;
            }
        }

        return null;
    }

    public Vector3 GetRandomLocation()
    {
        Room room = WeightedRandomRoom();
        return GetRandomPoint(room);
    }

    public Vector3 GetRandomLocationInRoom(Room room)
    {
        return GetRandomPoint(room);
    }

    public GameObject CreateNPC(Vector3 pos, Quaternion rot, int meshIndexP1, int meshIndexP2, Room room)
    {
        GameObject newNPC = Instantiate(NPCPrefab, new Vector3(pos.x, 0, pos.z), rot, NPCParent.transform);
        NPCAIStateManager npcAI = newNPC.GetComponent<NPCAIStateManager>();
        npcAI.startIndexP1 = meshIndexP1;
        npcAI.startIndexP2 = meshIndexP2;
        npcAI.startGeneration = false;
        npcAI.currentRoom = room;
        return newNPC;
    }

    public void PlayerAssigned()
    {
        foreach (NPCAIStateManager security in _security)
        {
            security.AddPoliceWatch();
        }
    }

    private void AssignAgentLayerMasks(Room room, NavMeshAgent agent)
    {
        if (room.isInGate)
        {
            agent.areaMask &= ~(1 << 5);
            agent.areaMask += (1 << 6);
        }
        else
        {
            agent.areaMask &= ~(1 << 6);
            agent.areaMask += (1 << 5);
        }
    }

    public void SendAllToMainHall()
    {
        foreach (NPCAIStateManager npc in _npcs)
        {
            npc.currentState = npc.states.GoToBoss();
            npc.currentState.EnterState();
        }
        foreach (NPCAIStateManager npc in _security)
        {
            npc.currentState = npc.states.GoToBoss();
            npc.currentState.EnterState();
        }
        foreach (NPCAIStateManager npc in _janitors)
        {
            npc.currentState = npc.states.GoToBoss();
            npc.currentState.EnterState();
        }
        foreach (NPCAIStateManager npc in _targets)
        {
            npc.currentState = npc.states.GoToBoss();
            npc.currentState.EnterState();
        }
    }

    public void FormACult(PlayerStateManager player)
    {
        foreach (NPCAIStateManager npc in _npcs)
        {
            npc.FollowPlayer(player);
        }
        foreach (NPCAIStateManager npc in _security)
        {
            npc.FollowPlayer(player);
        }
        foreach (NPCAIStateManager npc in _janitors)
        {
            npc.FollowPlayer(player);
        }
        foreach (NPCAIStateManager npc in _targets)
        {
            npc.FollowPlayer(player);
        }
    }
}
