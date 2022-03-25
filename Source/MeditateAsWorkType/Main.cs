using System.Reflection;
using HarmonyLib;
using Verse;

namespace MeditateAsWorkType;

[StaticConstructorOnStartup]
internal class Main
{
    static Main()
    {
        //Log.Message("Hello from Harmony in scope: com.github.harmony.rimworld.maarx.meditateasworktype");
        var harmony = new Harmony("com.github.harmony.rimworld.maarx.meditateasworktype");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }
}

//[HarmonyPatch]
//class Patch_ToilFailConditions_FailOn
//{
//    static System.Reflection.MethodBase TargetMethod()
//    {
//        return typeof(ToilFailConditions).GetMethods().FirstOrDefault(
//            x => x.Name.Equals("FailOn", StringComparison.OrdinalIgnoreCase) &&
//            x.IsGenericMethod)?.MakeGenericMethod(typeof(Toil));
//    }
//    static bool Prefix(ref Func<bool> condition)
//    {
//        Log.Message("Hello from Patch_ToilFailConditions_FailOn: " + condition.ToString());
//        return true;
//    }
//}