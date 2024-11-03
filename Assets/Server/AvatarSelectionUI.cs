using UnityEngine;
using UnityEngine.UI;

public class AvatarSelectionUI : MonoBehaviour
{
    public PlayerAvatar playerAvatar;
    public Button[] avatarButtons;

    private void Start()
    {
        for (int i = 0; i < avatarButtons.Length; i++)
        {
            int index = i;
            avatarButtons[i].onClick.AddListener(() => playerAvatar.ChooseAvatarServerRpc(index));
        }
    }
}
