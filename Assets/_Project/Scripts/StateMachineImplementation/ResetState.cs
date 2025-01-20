using UnityEngine;
using UnityEngine.UI;

public class ResetState : ARState
{
    public ResetState(StateManager manager) : base(manager) { }

    public override void Enter()
    {
        // Hide all other UI elements
        arManager.scanFloorInstruction.SetActive(false);
        arManager.setHeightInstruction.SetActive(false);
        arManager.setDoorwindowInstruction.SetActive(false);
        arManager.setTapmessagefirst.SetActive(false);
        arManager.setTapmessagesecond.SetActive(false);
        arManager.screenCenter.SetActive(false);
        arManager.upheight.SetActive(false);
        arManager.UndoButton.SetActive(false);
        arManager.FinishFloorButton.SetActive(false);
        arManager.SetHeightButton.SetActive(false);
        arManager.FinishWindowsButton.SetActive(false);
        arManager.windowTypePanel.SetActive(false);
        arManager.VisualizeButton.SetActive(false);

        // Show only reset-related UI elements
        arManager.ResetButton.SetActive(true);
        arManager.NoButton.SetActive(true);
        arManager.ResetInstruction.SetActive(true);
    }

    public override void Exit()
    {
        arManager.ResetButton.SetActive(false);
        arManager.NoButton.SetActive(false);
        arManager.ResetInstruction.SetActive(false);
    }
}