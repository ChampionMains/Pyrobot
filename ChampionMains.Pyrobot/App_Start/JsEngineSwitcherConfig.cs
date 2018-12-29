using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.V8;

namespace ChampionMains.Pyrobot
{
    public class JsEngineSwitcherConfig
    {
        public static void Configure(IJsEngineSwitcher engineSwitcher)
        {
            engineSwitcher.EngineFactories.AddV8();
            engineSwitcher.DefaultEngineName = V8JsEngine.EngineName;
        }
    }
}