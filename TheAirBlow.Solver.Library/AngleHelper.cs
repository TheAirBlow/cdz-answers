using Jint.Native;
using Jint.Native.Array;
using Jint.Native.Object;

namespace TheAirBlow.Solver.Library;

public static class AngleHelper
{
    /// <summary>
    /// Converts ArrayInstance to JsValue[]
    /// </summary>
    /// <param name="input">Input</param>
    /// <returns>Output</returns>
    public static JsValue[] NativeToArray(ArrayInstance input)
    {
        var list = input.GetOwnProperties().Select(
            p => p.Value.Value);
        return list as JsValue[] ?? list.ToArray();
    }
    
    /// <summary>
    /// Converts ArrayInstance to JsValue[]
    /// </summary>
    /// <param name="input">Input</param>
    /// <returns>Output</returns>
    public static JsValue[] NativeToObject(ObjectInstance input)
    {
        var list = input.GetOwnProperties().Select(
            p => p.Value.Value);
        return list as JsValue[] ?? list.ToArray();
    }
}