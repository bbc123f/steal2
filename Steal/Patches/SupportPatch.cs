using GorillaNetworking;
using HarmonyLib;

namespace Steal.GorillaOS.Patchers
{
    [HarmonyPatch(typeof(GorillaComputer), "ProcessSupportState", MethodType.Normal)]
    public class SupportPatch
    {
        public static int focusedModual = 1;

        static bool Prefix(GorillaKeyboardButton buttonPressed)
        {
            focusedModual = 1;
            if (int.TryParse(buttonPressed.characterString, out int result))
            {
                if (result != 0)
                {
                    focusedModual = result;
                }
            }

            if (buttonPressed.characterString== "enter")
            {
                Steal.GorillaOS.GorillaOS.Moduals.ToArray()[focusedModual - 1].enabled = !Steal.GorillaOS.GorillaOS.Moduals.ToArray()[focusedModual - 1].enabled;
            }
           Steal.GorillaOS.GorillaOS.instance.Refresh();
            return true;
        }
    }
}
