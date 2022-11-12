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
        /// With this event you only read the input, if you want to edit it use <see cref="OnInputIntercept"/>
        /// This occurs after both <see cref="SendKey"/> and <see cref="OnInput"/>
        /// </summary>
        public event OnInputEvent OnInput;

        /// <summary>
        /// This event occurs whenever the game recieves an input just before it is processed
        /// With this event you can edit the input before it is processed, for just reading inputs use <see cref="OnInput"/>
        /// This occurs before <see cref="OnInput"/> and after <see cref="SendKey"/>
        /// </summary>
        public event OnInputInterceptEvent OnInputIntercept;

        /// <summary>
        /// Queues a key to be sent to the game the next time it polls for inputs
        /// This is added before the <see cref="OnInput"/> and <see cref="OnInputIntercept"/> events are run so it can be modified by other consumers
        /// </summary>
        /// <param name="key"></param>
        public void SendKey(Key key);

    }

    /// <summary>
    /// A delegate for an event that occurs whenever the game recieves an input just before it is processed
    /// Ran asynchronously so use for reading inputs before they are processed
    /// </summary>
    /// <param name="inputs">The rising edge inputs that were recieved</param>
    public delegate void OnInputEvent(List<Key> inputs);

    /// <summary>
    /// A delegate for an event that occurs whenever the game recieves an input just before it is processed
    /// Ran synchronously so use for editing inputs before they are processed
    /// </summary>
    /// <param name="inputs">The rising edge inputs that were recieved</param>
    /// <returns></returns>
    public delegate void OnInputInterceptEvent(List<Key> inputs);
}
