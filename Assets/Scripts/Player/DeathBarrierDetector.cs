using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathBarrierDetector : NetworkBehaviour
{
    /// <summary>
    /// Check for death barrier collisions.
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!base.IsServer)
            return;

        if (collision.gameObject.CompareTag("DeathBarrier"))
        {
            PlayersManager.Instance.DamagePlayer(base.Owner);
        }
    }
}
