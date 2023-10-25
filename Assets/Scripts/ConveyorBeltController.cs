using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorBeltController : MonoBehaviour
{
    [SerializeField] private GameObject BeltPiece;

    void Start()
    {
        GameObject beltPiece;
        for(int i = 0; i < 200; i++)
        {
            beltPiece = Instantiate(BeltPiece, transform);
            float ratio = (float)(1 + i) / 200;
            beltPiece.GetComponent<Animator>().Play("Base Layer.Conveyor", 0, ratio);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
