using UnityEngine;
using Mirror;
using System.Collections;
using UnityEngine.SceneManagement;
using System.IO;
using Unity.VisualScripting;

public class TransitionToScene : NetworkBehaviour
{
    private MyNetworkManager myNetworkManager;
    private FadeInOutScreen fadeInOutScreen;

    [Scene]
    public string transitionToSceneName;
    public string sceneSpawnPos;

    private void Awake()
    {
        if (myNetworkManager == null)
        {
            myNetworkManager = FindObjectOfType<MyNetworkManager>();
            fadeInOutScreen = FindObjectOfType<FadeInOutScreen>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerMove>())
        {
            if (collision.TryGetComponent<PlayerMove>(out PlayerMove playerMove))
            {
                playerMove.enabled = false;
            }
            if (isServer)
            {
                StartCoroutine(SendPlayerToNewScene(collision.gameObject));
            }
        }
    }

    [ServerCallback]

    IEnumerator SendPlayerToNewScene(GameObject player)
    {
        if (player.TryGetComponent<NetworkIdentity>(out NetworkIdentity identity))
        {
            NetworkConnectionToClient conn = identity.connectionToClient;
            if (conn == null)
            {
                yield break;
            }

            conn.Send(new SceneMessage { sceneName = this.gameObject.scene.path, sceneOperation = SceneOperation.UnloadAdditive, customHandling = true });

            yield return new WaitForSeconds((fadeInOutScreen.speed * 0.1f));

            NetworkServer.RemovePlayerForConnection(conn, false);

            NetworkStartPosition[] allStartPos = FindObjectsOfType<NetworkStartPosition>();

            Transform start = myNetworkManager.GetStartPosition();

            foreach (var item in allStartPos)
            {
                if (item.gameObject.scene.name == Path.GetFileNameWithoutExtension(transitionToSceneName) && item.name == sceneSpawnPos)
                {
                    start = item.transform;
                }

                player.transform.position = start.position;

                SceneManager.MoveGameObjectToScene(player, SceneManager.GetSceneByPath(transitionToSceneName));

                conn.Send(new SceneMessage { sceneName = transitionToSceneName, sceneOperation = SceneOperation.LoadAdditive, customHandling = true });

                NetworkServer.AddPlayerForConnection(conn, player);

                if (NetworkClient.localPlayer != null && NetworkClient.localPlayer.TryGetComponent<PlayerMove>(out PlayerMove playerMove))
                {
                    playerMove.enabled = true;
                }
            }
        }
    }
}
