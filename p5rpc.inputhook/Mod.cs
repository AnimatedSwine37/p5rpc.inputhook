using p5rpc.inputhook.Configuration;
using p5rpc.inputhook.Template;
using Reloaded.Hooks.Definitions.X64;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Memory.Sigscan.Definitions.Structs;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using System.Diagnostics;
using System.Drawing;

namespace p5rpc.inputhook
{
    /// <summary>
    /// Your mod logic goes here.
    /// </summary>
    public unsafe class Mod : ModBase // <= Do not Remove.
    {
        /// <summary>
        /// Provides access to the mod loader API.
        /// </summary>
        private readonly IModLoader _modLoader;

        /// <summary>
        /// Provides access to the Reloaded.Hooks API.
        /// </summary>
        /// <remarks>This is null if you remove dependency on Reloaded.SharedLib.Hooks in your mod.</remarks>
        private readonly IReloadedHooks? _hooks;

        /// <summary>
        /// Provides access to the Reloaded logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Entry point into the mod, instance that created this class.
        /// </summary>
        private readonly IMod _owner;

        /// <summary>
        /// Provides access to this mod's configuration.
        /// </summary>
        private Config _configuration;

        /// <summary>
        /// The configuration of the currently executing mod.
        /// </summary>
        private readonly IModConfig _modConfig;

        private GetKeyboardInputInfoFunction GetKeyboardInputInfo;

        private Timer _inputTimer;
        
        public Mod(ModContext context)
        {
            _modLoader = context.ModLoader;
            _hooks = context.Hooks;
            _logger = context.Logger;
            _owner = context.Owner;
            _configuration = context.Configuration;
            _modConfig = context.ModConfig;


            // For more information about this template, please see
            // https://reloaded-project.github.io/Reloaded-II/ModTemplate/

            // If you want to implement e.g. unload support in your mod,
            // and some other neat features, override the methods in ModBase.

            // TODO: Implement some mod logic

            Utils.Initialise(_logger, _configuration);
            
            var startupScannerController = _modLoader.GetController<IStartupScanner>();
            if (startupScannerController == null || !startupScannerController.TryGetTarget(out var startupScanner))
            {
                _logger.WriteLine($"[Input Hook] Unable to get controller for Reloaded SigScan Library, aborting initialisation", Color.Red);
                return;
            }

            startupScanner.AddMainModuleScan("48 83 EC 28 33 D2 E8 ?? ?? ?? ?? 48 83 C0 60", InitKeyboardHook);

        }

        private void InitKeyboardHook(PatternScanResult result)
        {
            if(!result.Found)
            {
                Utils.LogError("Unable to find keyboard hook function, aborting initialisation");
                return;
            }

            Utils.LogDebug($"Found keyboard hook function at 0x{result.Offset + Utils.BaseAddress:X}");
            GetKeyboardInputInfo = _hooks.CreateWrapper<GetKeyboardInputInfoFunction>(result.Offset + Utils.BaseAddress, out _);
            Debugger.Launch();
            _inputTimer = new Timer(PollInput, null, TimeSpan.FromMilliseconds(10000), TimeSpan.FromMilliseconds(10));
        }

        private int GetKeyboardInput()
        {
            int** inputInfo = GetKeyboardInputInfo(0, 0);
            if (*(inputInfo + 1) != (int*)0)
            {
                return **inputInfo;
            }
            return 0;
        }

        private void PollInput(object? state)
        {
            var input = GetKeyboardInput();
            if (input == 0)
                return;

            Utils.LogDebug($"Input: 0x{input:X}");
        }

        [Function(CallingConventions.Microsoft)]
        public delegate int** GetKeyboardInputInfoFunction(int param1, int param2);


        #region Standard Overrides
        public override void ConfigurationUpdated(Config configuration)
        {
            // Apply settings from configuration.
            // ... your code here.
            _configuration = configuration;
            _logger.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");
        }
        #endregion

        #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Mod() { }
#pragma warning restore CS8618
        #endregion
    }
}