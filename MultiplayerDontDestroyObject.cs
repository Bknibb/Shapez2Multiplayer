using UnityEngine;

namespace Shapez2Multiplayer
{
    public class MultiplayerDontDestroyObject : MonoBehaviour
    {
        public void Update()
        {
            MultiplayerCore.Update();
        }
    }
}
