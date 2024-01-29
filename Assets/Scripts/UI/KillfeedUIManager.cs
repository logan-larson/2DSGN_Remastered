using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillfeedUIManager : NetworkBehaviour
{
    [SerializeField]
    private GameObject _killfeedItemPrefab;

    [SerializeField]
    private Transform _killfeedItemsParent;

    private void Start()
    {
        if (PlayersManager.Instance == null)
            return;

        PlayersManager.Instance.OnPlayerKilled.AddListener(OnPlayerKilled);
    }

    private void OnDestroy()
    {
        if (PlayersManager.Instance == null)
            return;

        PlayersManager.Instance.OnPlayerKilled.RemoveAllListeners();
    }

    private void OnPlayerKilled(Player playerKilled, Player killer, WeaponInfo weaponInfo)
    {
        GameObject killfeedItem = Instantiate(_killfeedItemPrefab, _killfeedItemsParent);

        KillfeedItemUI killfeedItemUI = killfeedItem.GetComponent<KillfeedItemUI>();

        var killerUsername = killer != null ? killer.Username : null;
        var weaponSpritePath = weaponInfo != null ? weaponInfo.SpritePath : null;

        killfeedItemUI.Setup(playerKilled.Username, killerUsername, weaponSpritePath);

        Destroy(killfeedItem, 5f);

        OnPlayerKilledObserversRpc(playerKilled, killer, weaponInfo);
    }

    [ObserversRpc]
    private void OnPlayerKilledObserversRpc(Player playerKilled, Player killer, WeaponInfo weaponInfo)
    {
        if (base.IsServer)
            return;

        GameObject killfeedItem = Instantiate(_killfeedItemPrefab, _killfeedItemsParent);

        KillfeedItemUI killfeedItemUI = killfeedItem.GetComponent<KillfeedItemUI>();

        var killerUsername = killer != null ? killer.Username : null;
        var weaponSpritePath = weaponInfo != null ? weaponInfo.SpritePath : null;

        killfeedItemUI.Setup(playerKilled.Username, killerUsername, weaponSpritePath);

        Destroy(killfeedItem, 5f);
    }
}
