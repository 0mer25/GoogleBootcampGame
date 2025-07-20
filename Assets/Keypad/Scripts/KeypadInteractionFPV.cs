using UnityEngine;

namespace NavKeypad
{
    public class KeypadInteractionFPV : MonoBehaviour
    {
        private Camera cam;
        [SerializeField] private Keypad activeKeypad;

        private void Awake() => cam = Camera.main;

        private void Update()
        {
            var ray = cam.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 2f, Color.red); // ekrana ray göster
            if (Input.GetMouseButtonDown(0))
            {
                if (Physics.Raycast(ray, out var hit))
                {
                    if (hit.collider.TryGetComponent(out KeypadButton btn))
                    {
                        btn.PressButton();
                    }
                }
            }

            if (activeKeypad != null)
            {
                for (int i = 0; i <= 9; i++)
                {
                    if (Input.GetKeyDown(i.ToString()))
                    {
                        activeKeypad.AddInput(i.ToString());
                    }
                }

                if (Input.GetKeyDown(KeyCode.Return))
                {
                    activeKeypad.AddInput("enter");
                }
            }
        }

        public void SetActiveKeypad(Keypad keypad)
        {
            activeKeypad = keypad;
        }
    }
}
