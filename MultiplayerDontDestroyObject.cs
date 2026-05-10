using System;
using System.Collections.Generic;
using System.Text;
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
