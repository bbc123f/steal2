using System;
using System.Collections.Generic;
using System.Text;

namespace Steal.Attributes
{
    internal class Button
    {
        public string buttontext { get; set; }
        public bool isToggle { get; set; }
        public Action OnExecute { get; set; }
        public Action OnDisable { get; set; }
        public bool Active { get; set; }

        public Button(string text, bool isToggle, Action onExecute, Action onDisable = null, bool nigger = false)
        {
            buttontext = text;
            this.isToggle = isToggle;
            OnExecute = onExecute;
            OnDisable = onDisable;
            Active = nigger;
        }

        public override string ToString()
        {
            return $"[Name {buttontext} : isToggle {isToggle} : Active {Active}]";
        }
    }
}
