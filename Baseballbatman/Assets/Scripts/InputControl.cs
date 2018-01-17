using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InputControl
{
	public float MoveX { get; private set; }
	public float MoveY { get; private set; }
	public Vector2 Move { get { return Vector2.ClampMagnitude(new Vector2 (MoveX, MoveY), 1f); } }

	public bool Attack { get; private set; }
	public bool Dodge { get; private set; }
	public bool DodgeLeft { get; private set; }

	private enum ControlDevice { xbox360, joyConUpwards, joyConSideways, mouseAndKeyboard }
	[SerializeField] private ControlDevice controldevice;

	private KeyCode keyboardAttackKey = KeyCode.Mouse0;
	private KeyCode keyboardDodgeKey = KeyCode.Space;

	private KeyCode xbox360AttackKey = KeyCode.JoystickButton0;
	private KeyCode xbox360DodgeKey = KeyCode.JoystickButton5;

	private JoyConRKey joyConUpwardsAttackKey = JoyConRKey.R;
	private JoyConRKey joyConUpwardsDodgeKey = JoyConRKey.ZR;

	private JoyConRKey joyConSidewaysAttackKey = JoyConRKey.B;
	private JoyConRKey joyConSidewaysDodgeKey = JoyConRKey.Y;


	public void Update ()
	{
		switch (controldevice) {
		case ControlDevice.mouseAndKeyboard:
			GetMouseAndKeyboardInput ();
			break;
		case ControlDevice.xbox360:
			GetXbox360Input ();
			break;
		case ControlDevice.joyConSideways:
			GetJoyConSideWaysInput ();
			break;
		case ControlDevice.joyConUpwards:
			GetJoyConUpwardsInput ();
			break;
		}

		/*
		for (KeyCode i = 0; i <= KeyCode.Joystick8Button19; i++)
		{
			if (Input.GetKey(i))
				Debug.Log(i);
		}
		*/
	}

	private void GetMouseAndKeyboardInput ()
	{
		Vector2 move = Vector2.ClampMagnitude (
			               new Vector2 (
				               Input.GetAxis ("Horizontal"),
				               Input.GetAxis ("Vertical")
			               ), 1f
		               );
		MoveX = move.x;
		MoveY = move.y;

		Attack = Input.GetKeyDown (KeyCode.Space);
		Dodge = Input.GetKeyDown (KeyCode.RightShift);
	}

	private void GetXbox360Input ()
	{
		MoveX = Input.GetAxis ("Horizontal");
		MoveY = Input.GetAxis ("Vertical");

		Attack = Input.GetKeyDown (xbox360AttackKey);
		Dodge = Input.GetKeyDown (xbox360DodgeKey);
	}

	private void GetJoyConUpwardsInput ()
	{
		MoveX = Input.GetAxis ("JoyCon Horizontal");
		MoveY = Input.GetAxis ("JoyCon Vertical");

		Attack = Input.GetKeyDown ((KeyCode)joyConUpwardsAttackKey);
		Dodge = Input.GetKeyDown ((KeyCode)joyConUpwardsDodgeKey);
	}

	private void GetJoyConSideWaysInput ()
	{
		MoveX = Input.GetAxis ("JoyCon Vertical");
		MoveY = -1 * Input.GetAxis ("JoyCon Horizontal");

		Attack = Input.GetKeyDown ((KeyCode)joyConSidewaysAttackKey);
		Dodge = Input.GetKeyDown ((KeyCode)joyConSidewaysDodgeKey);
	}

}
	public enum JoyConRKey
	{
		None = KeyCode.None,

		A = KeyCode.JoystickButton0,
		B = KeyCode.JoystickButton2,
		X = KeyCode.JoystickButton1,
		Y = KeyCode.JoystickButton3,
		Plus = KeyCode.JoystickButton9,
		Home = KeyCode.JoystickButton12,

		R = KeyCode.JoystickButton14,
		ZR = KeyCode.JoystickButton15,

		SR = KeyCode.JoystickButton5,
		SL = KeyCode.JoystickButton4,
		
		// These are when joycon is held upwards in one hand
		// Joystick Horizontal = Joystick Axis 10.
		// Joystick Vertical = Joystick Axis 9.
	}