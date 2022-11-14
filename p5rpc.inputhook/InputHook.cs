using p5rpc.inputhook.Configuration;
using p5rpc.inputhook.interfaces;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Hooks.Definitions.X64;
using Reloaded.Memory.Pointers;
using Reloaded.Memory.Sigscan.Definitions.Structs;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Memory.Sources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static p5rpc.inputhook.interfaces.Inputs;
using static Reloaded.Hooks.Definitions.X64.FunctionAttribute;

namespace p5rpc.inputhook
{
    public class InputHook : IInputHook
    {
        private IReloadedHooks _hooks;

        private Config _configuration;

        private IAsmHook _keyPressedHook;

        private List<Key> _keysToSend = new();

        private bool _needToSendKeys = false;

        private IReverseWrapper<KeyPressedFunction> _keyPressedReverseWrapper;

        private IMemory _memory;


        public InputHook(IStartupScanner startupScanner, IReloadedHooks hooks, Config configuration)
        {
            _hooks = hooks;
            _configuration = configuration;
            _memory = new Memory();
            startupScanner.AddMainModuleScan("0F 28 74 24 ?? 48 8D 34 85 00 00 00 00", InitKeyboardHook);
        }

        private unsafe void InitKeyboardHook(PatternScanResult result)
        {
            if (!result.Found)
            {
                Utils.LogError("Unable to find keyboard hook function, aborting initialisation");
                return;
            }

            Utils.LogDebug($"Found keyboard hook function at 0x{result.Offset + Utils.BaseAddress:X}");

            string[] function =
            {
                "use64",
                $"{_hooks.Utilities.GetAbsoluteCallMnemonics(KeyPressed, out _keyPressedReverseWrapper)}",
            };
            _keyPressedHook = _hooks.CreateAsmHook(function, result.Offset + Utils.BaseAddress, AsmHookBehaviour.ExecuteFirst).Activate();
        }

        private int KeyPressed(int numKeys, nuint pressedKeyInfo, bool needToSendKeys)
        {
            if (numKeys == 0 && !_needToSendKeys)
                return numKeys;
            FixedArrayPtr<Key> pressedKeys = new(pressedKeyInfo, numKeys);
            Utils.LogDebug($"Pressed keys: {string.Join(", ", pressedKeys)}");
            List<Key> pressedKeysList = pressedKeys.ToList();

            if (_needToSendKeys)
            {
                pressedKeysList.AddRange(_keysToSend);
                _keysToSend.Clear();
                _needToSendKeys = false;
            }

            if (OnInputIntercept?.GetInvocationList().Length != 0)
            {
                OnInputIntercept?.Invoke(pressedKeysList);
                _memory.Write(pressedKeyInfo, pressedKeysList.ToArray());
            }

            Task.Run(() => OnInput?.Invoke(pressedKeysList));

            return pressedKeysList.Count;
        }

        public void SendKey(Key key)
        {
            _keysToSend.Add(key);
            _needToSendKeys = true;
        }

        public event OnInputEvent OnInput;

        public event OnInputInterceptEvent OnInputIntercept;

        [Function(new Register[] { Register.rax, Register.r13, Register.rbx }, Register.rax, false)]
        private delegate int KeyPressedFunction(int numKeys, nuint pressedKeyInfo, bool needToSendKeys);
    }
}
