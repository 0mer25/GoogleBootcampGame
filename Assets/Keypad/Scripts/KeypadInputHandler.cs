using UnityEngine;

namespace NavKeypad
{
    public class KeypadInputHandler : MonoBehaviour
    {
        private Keypad keypad;

        public void Initialize(Keypad targetKeypad)
        {
            keypad = targetKeypad;
            Debug.Log("Keypad input handler initialized");
        }

        private void Update()
        {
            if (keypad == null) return;

            // Sayý tuþlarý
            if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0))
                keypad.AddInput("0");
            else if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
                keypad.AddInput("1");
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
                keypad.AddInput("2");
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
                keypad.AddInput("3");
            else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
                keypad.AddInput("4");
            else if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
                keypad.AddInput("5");
            else if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6))
                keypad.AddInput("6");
            else if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7))
                keypad.AddInput("7");
            else if (Input.GetKeyDown(KeyCode.Alpha8) || Input.GetKeyDown(KeyCode.Keypad8))
                keypad.AddInput("8");
            else if (Input.GetKeyDown(KeyCode.Alpha9) || Input.GetKeyDown(KeyCode.Keypad9))
                keypad.AddInput("9");

            // Enter tuþlarý
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                keypad.AddInput("enter");

            // Backspace veya Delete ile temizleme
            else if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Delete))
                keypad.AddInput("clear");
        }
    }
}