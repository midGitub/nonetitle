using System.Collections;
using CitrusFramework;
using UnityEngine;
using UnityEngine.UI;

public enum MoreSlotsAnim
{
    ShowBoard,
    HideBoard
}

public class MapMoreSlotsBoardController : MonoBehaviour
{
    public MapCurtainsUiController MapCurtainsUiCtrl;
    public Animator AnimCtrl;
    public Button BoardButton;
    public float DuringTime;

    void Start()
    {
        BoardButton.onClick.AddListener(ShowMoreSlots);
    }

    void OnDestroy()
    {
        BoardButton.onClick.RemoveListener(ShowMoreSlots);
    }

    public void PlayBoardAnim(MoreSlotsAnim anim)
    {
        AnimCtrl.SetTrigger(anim.ToString());
    }

    void ShowMoreSlots()
    {
        MapCurtainsAnim animType = MapCurtainsUiCtrl.AlreadySwitched
            ? MapCurtainsAnim.MoveInFromLeft
            : MapCurtainsAnim.MoveInFromRight;

        MapCurtainsUiCtrl.PlayMoreSlotsAnim(animType);
    }
}
