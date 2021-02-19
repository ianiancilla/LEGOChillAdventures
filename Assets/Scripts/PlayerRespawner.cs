using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Unity.LEGO.Minifig;


public class PlayerRespawner : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab;
    [SerializeField] float respawnWait = 3f;

    const string CAMERA_NAME = "Third Person Free Look Camera";

    public void Respawn(GameObject oldPlayer,
                        Vector3 position, Quaternion rotation)
    {
        StartCoroutine(RespawnCoroutine(oldPlayer, position, rotation));
    }

    private IEnumerator RespawnCoroutine(GameObject oldPlayer,
                                         Vector3 position, Quaternion rotation)
    {
        yield return new WaitForSeconds(respawnWait);
        Debug.Log("before destroy");
        Destroy(oldPlayer);
        Debug.Log("after destroy");

        GameObject newPlayer = Instantiate(playerPrefab,
                                           position,
                                           rotation);
        newPlayer.transform.parent = this.transform;
        newPlayer.GetComponent<MinifigController>().SetInputEnabled(true);

        CinemachineFreeLook camera = GameObject.Find(CAMERA_NAME).GetComponent<CinemachineFreeLook>();
        camera.Follow = newPlayer.transform;
        camera.LookAt = newPlayer.transform;
    }

}
