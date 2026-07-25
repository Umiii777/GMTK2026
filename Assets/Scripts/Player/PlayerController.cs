using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "PlayerController", menuName = "InputController/PlayerController")]
public class PlayerController : InputController
{

    public override bool RetrieveJumpInput()
    {
        return Input.GetButtonDown("Jump");
    }

    public override float RetrieveMoveInput()
    {
        return Input.GetAxisRaw("Horizontal");
    }

    public override bool RetrieveDashInput()
    {
        // Left Shift（Unity 默认 Fire3）；也可改成自己的按键
        return Input.GetButtonDown("Fire3") || Input.GetKeyDown(KeyCode.LeftShift);
    }
    public override bool RetrieveScaleInput()
    {
        return Input.GetKeyDown(KeyCode.Tab);
    }

}
