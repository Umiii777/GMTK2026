using UnityEngine;

public class FailureUI : MonoBehaviour
{
    private OldTelevision oldTelevision;

    private void Start()
    {
        oldTelevision = Camera.main.GetComponent<OldTelevision>();

        oldTelevision.warpVertical = true;
        OldTelevision.stabilityOne = 0.1f;
        OldTelevision.stabilityTwo = 1f;
    }

    public void OnClickRestart()
    {
        ResetOldTelevision();
    }

    public void OnClickMainMenu()
    {
        ResetOldTelevision();
    }

    private void ResetOldTelevision()
    {
        oldTelevision.warpVertical = false;
        OldTelevision.stabilityOne = 6f;
        OldTelevision.stabilityTwo = 50f;
    }
}
