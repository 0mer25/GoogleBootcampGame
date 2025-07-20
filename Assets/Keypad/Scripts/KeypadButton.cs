using System.Collections;
using UnityEngine;

namespace NavKeypad
{
    public class KeypadButton : MonoBehaviour
    {
        [SerializeField] private string value;
        [SerializeField] private float moveDist = 0.005f;
        [SerializeField] private float pressSpeed = 0.1f;
        [SerializeField] private Keypad keypad;

        private bool moving = false;

        public void PressButton()
        {
            if (!moving)
            {
                keypad.AddInput(value);
                StartCoroutine(ButtonAnimation());
            }
        }

        private IEnumerator ButtonAnimation()
        {
            moving = true;
            Vector3 start = transform.localPosition;
            Vector3 end = start + new Vector3(0, 0, moveDist);

            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime / pressSpeed;
                transform.localPosition = Vector3.Lerp(start, end, t);
                yield return null;
            }

            yield return new WaitForSeconds(0.05f);

            t = 0;
            while (t < 1)
            {
                t += Time.deltaTime / pressSpeed;
                transform.localPosition = Vector3.Lerp(end, start, t);
                yield return null;
            }

            moving = false;
        }
    }
}
