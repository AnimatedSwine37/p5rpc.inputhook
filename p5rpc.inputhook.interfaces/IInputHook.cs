using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static p5rpc.inputhook.interfaces.Inputs;

namespace p5rpc.inputhook.interfaces
{
    public interface IInputHook
    {
        /// <summary>
        /// This event occurs whenever the game recieves an input just before it is processed
        /// </summary>
        public event OnInputEvent OnInput;
    }

    /// <summary>
    /// A delegate for an event that occurs whenever the game recieves an input just before it is processed
    /// </summary>
    /// <param name="inputs"></param>
    public delegate void OnInputEvent(List<Key> inputs);
}
