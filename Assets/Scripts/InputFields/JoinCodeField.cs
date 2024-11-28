using UnityEngine;
using System.Text.RegularExpressions;

namespace Tankito.InputFields
{
    public class JoinCodeField : InputFieldHandler
    {
        [SerializeField]
        private int maxLength = 6;

        protected override void Start()
        {
            base.Start();

            if (inputField != null)
            {
                inputField.onValueChanged.AddListener(ValidateInput);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (inputField != null)
            {
                inputField.onValueChanged.RemoveListener(ValidateInput);
            }
        }

        private void ValidateInput(string input)
        {
            string filteredInput = Regex.Replace(input, @"[^a-zA-Z0-9]", "");
            filteredInput = filteredInput.ToUpper();

            if (filteredInput.Length > maxLength)
            {
                filteredInput = filteredInput.Substring(0, maxLength);
            }

            if (inputField.text != filteredInput)
            {
                inputField.text = filteredInput;
                inputField.caretPosition = filteredInput.Length;
            }
        }
    }
}


