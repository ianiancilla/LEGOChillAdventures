using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.LEGO.Minifig;

public class Player_WaterCollision : MonoBehaviour
{
    [SerializeField] int maxPositionsQueueLength = 10;
    [SerializeField] float waterHeight = 29.999f;

    const string WATER_TAG = "Water";

    // member variables
    List<Vector3> previousPositionsQueue;
    bool calledRespawn = false;

    // cache
    MinifigController minifigController;

    // Start is called before the first frame update
    void Awake()
    {
        // initialise variables
        previousPositionsQueue = new List<Vector3>();

        // cache
        minifigController = GetComponent<MinifigController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (calledRespawn) { return; }

        if (IsDrowning())
        {
            // Explode old player
            minifigController.Explode();
            // Spawn and setup new player, and destroy old one, after delay
            FindObjectOfType<PlayerRespawner>().Respawn(this.gameObject,
                                                        previousPositionsQueue[0],
                                                        this.transform.rotation);
            calledRespawn = true;
            return;
        }

        if (IsOnDryLand())
        {
            UpdatePositionQueue();
        }
    }

    private void UpdatePositionQueue()
    {
        // if queue is full, delete oldest
        if (previousPositionsQueue.Count >= maxPositionsQueueLength)
        {
            previousPositionsQueue.RemoveAt(0);
        }
        previousPositionsQueue.Add(this.transform.position);
    }
    private bool IsDrowning()
    {
        float playerHeight = this.gameObject.transform.position.y;

        return (playerHeight <= waterHeight);
    }

    private bool IsOnDryLand()
    {
        // Bit shift to get a bit mask of layers 4(Water) 8(Terrain)
        int layerMask = (1 << 4) | (1 << 8);

        RaycastHit hit;
        // Does the ray intersect any objects on those two layers?
        if (Physics.Raycast(transform.position + new Vector3(0, 100, 0),
                            Vector3.down,
                            out hit,
                            Mathf.Infinity,
                            layerMask))
        {
            if (hit.collider.gameObject.tag != WATER_TAG)
            {
                return true;
            }
        }
        return false;

    }

}

