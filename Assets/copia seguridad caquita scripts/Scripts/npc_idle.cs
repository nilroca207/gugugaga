using UnityEngine;
using System.Collections;

public class npc_idle : MonoBehaviour
{
    public float walkRadius;
    
    public UnityEngine.AI.NavMeshAgent agent;
    private bool isWalking;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isWalking = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (isWalking) 
        {
            Move();
            StartCoroutine("WaitingForMove");
        }
    }
    void Move(){
        Vector3 randomDirection = Random.insideUnitSphere * walkRadius;
        randomDirection += transform.position;
        UnityEngine.AI.NavMeshHit hit;
        UnityEngine.AI.NavMesh.SamplePosition(randomDirection, out hit, walkRadius, 1);
        agent.SetDestination(hit.position);
    }
    IEnumerator WaitingForMove()
    {
        isWalking = false;
        yield return new WaitForSeconds(UnityEngine.Random.Range(1f,4f));
        isWalking = true;
    }
}
