using Unity.Netcode;
using UnityEngine;

public class PlayerAvatar : NetworkBehaviour
{
    public GameObject[] avatarModels; // Array of avatar models to choose from
    private NetworkVariable<int> selectedAvatarIndex = new NetworkVariable<int>(0);

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            ChooseAvatarServerRpc(Random.Range(0, avatarModels.Length)); // Choose a random avatar at start
        }
        UpdateAvatarModel(selectedAvatarIndex.Value);
    }

    [ServerRpc]
    public void ChooseAvatarServerRpc(int index)
    {
        selectedAvatarIndex.Value = index;
        UpdateAvatarModel(index);
    }

    private void UpdateAvatarModel(int index)
    {
        for (int i = 0; i < avatarModels.Length; i++)
        {
            avatarModels[i].SetActive(i == index);
        }
    }
}
