namespace HexaEngine.Core.Input
{
    /// <summary>
    /// Represents the available buttons on a gamepad controller.
    /// </summary>
    public enum GamepadButton
    {
        Invalid = -1,

        //
        // Summary:
        //     Bottom face button (e.g. Xbox A button)
        South = 0,

        //
        // Summary:
        //     Right face button (e.g. Xbox B button)
        East = 1,

        //
        // Summary:
        //     Left face button (e.g. Xbox X button)
        West = 2,

        //
        // Summary:
        //     Top face button (e.g. Xbox Y button)
        North = 3,

        Back = 4,

        Guide = 5,

        Start = 6,

        LeftStick = 7,

        RightStick = 8,

        LeftShoulder = 9,

        RightShoulder = 0xA,

        DpadUp = 0xB,

        DpadDown = 0xC,

        DpadLeft = 0xD,

        DpadRight = 0xE,

        //
        // Summary:
        //     Additional button (e.g. Xbox Series X share button, PS5 microphone button, Nintendo
        //     Switch Pro capture button, Amazon Luna microphone button, Google Stadia capture
        //     button)
        Misc1 = 0xF,

        //
        // Summary:
        //     Upper or primary paddle, under your right hand (e.g. Xbox Elite paddle P1)
        RightPaddle1 = 0x10,

        //
        // Summary:
        //     Upper or primary paddle, under your left hand (e.g. Xbox Elite paddle P3)
        LeftPaddle1 = 0x11,

        //
        // Summary:
        //     Lower or secondary paddle, under your right hand (e.g. Xbox Elite paddle P2)
        RightPaddle2 = 0x12,

        //
        // Summary:
        //     Lower or secondary paddle, under your left hand (e.g. Xbox Elite paddle P4)
        LeftPaddle2 = 0x13,

        //
        // Summary:
        //     PS4/PS5 touchpad button
        Touchpad = 0x14,

        //
        // Summary:
        //     Additional button
        Misc2 = 0x15,

        //
        // Summary:
        //     Additional button

        Misc3 = 0x16,

        //
        // Summary:
        //     Additional button
        Misc4 = 0x17,

        //
        // Summary:
        //     Additional button
        Misc5 = 0x18,

        //
        // Summary:
        //     Additional button
        Misc6 = 0x19,

        Count = 0x1A
    }
}