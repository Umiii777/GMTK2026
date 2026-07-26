using UnityEngine;

[CreateAssetMenu(fileName = "PlayerController", menuName = "InputController/PlayerController")]
public class PlayerController : InputController
{
    public override bool RetrieveJumpInput()
    {
        if (PauseManager.IsPaused)
            return false;

        return Input.GetButtonDown("Jump");
    }

    public override float RetrieveMoveInput()
    {
        if (PauseManager.IsPaused)
            return 0f;

        return Input.GetAxisRaw("Horizontal");
    }

    public override bool RetrieveDashInput()
    {
        if (PauseManager.IsPaused)
            return false;

        // Left Shift（Unity 默认 Fire3）；也可改成自己的按键
        return Input.GetButtonDown("Fire3") || Input.GetKeyDown(KeyCode.LeftShift);
    }

    public override bool RetrieveScaleInput()
    {
        if (PauseManager.IsPaused)
            return false;

        return Input.GetKeyDown(KeyCode.Tab);
    }
}
