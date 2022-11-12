using p5rpc.inputhook.interfaces;
using Reloaded.Mod.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p5rpc.inputhook
{
    public class Exports : IExports
    {
        public Type[] GetTypes() => new[] { typeof(IInputHook) };
    }
}
