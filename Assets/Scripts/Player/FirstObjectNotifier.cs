using FishNet.Object;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstObjectNotifier : NetworkBehaviour
{
    public static event Action<Transform, GameObject> OnFirstObjectSpawned;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (base.IsOwner)
        {
            OnFirstObjectSpawned?.Invoke(base.transform, base.gameObject);

            /*
            NetworkObject nob = base.LocalConnection.FirstObject;
            if (nob == base.NetworkObject)
            {
                OnFirstObjectSpawned?.Invoke(base.transform, base.gameObject);
            }
            */
        }
    }
}
