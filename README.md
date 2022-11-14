# p5rpc.inputhook
A [Reloaded-II](https://reloaded-project.github.io/Reloaded-II/) library that allows mods to read, alter, and send inputs directly to Persona 5 Royal (Steam).

Note that currently this only supports inputs for keyboard and only rising edge inputs (when a button is first pressed). Buttons being held or falling edge presses cannot be detected or sent currently, although in the future this may be added if it's needed.

## Using In Your Mods
Firstly, you'll need to add the [p5rpc.inputhook.interfaces nuget package](https://www.nuget.org/packages/p5rpc.inputhook.interfaces) to your project. Then add `p5rpc.inputhook` as a dependency in your `ModConfig.json`.

Finally, you'll need to access `IInputHook` by adding the following to your Mod.cs
``` C#
var inputController = _modLoader.GetController<IInputHook>();
if (inputController == null || !inputController.TryGetTarget(out var inputHook))
{
    // Tell the user that you couldn't access inputhook so stuff won't work
}
```

Once you have an instance of InputHook there are three main things you can do with it: read inputs, intercept inputs, and send inputs.

### Reading Inputs
To read inputs given to the game subscribe to the `OnInput` event which will give a List of all pressed keys whenever a key is pressed (only rising edge inputs).
``` C#
_inputHook.OnInput += InputHappened;

private void InputHappened(List<Key> inputs)
{
    // Do something with the inputs
}

```
Note that changing the `inputs` list will not effect the inputs that the game processes, if you want to do that look below at Intercepting Inputs and Sending Inputs.

### Intercepting Inputs
Intercepting inputs works similarly to reading them except any changes you make to the `inputs` list will change the inputs that the game processes. So, you can remove, add or change inputs before the game processes them.

To do so subscribe to the `OnInputIntercept` event like below
``` C#
_inputHook.OnInputIntercept += InputIntercepted;

private void InputIntercepted(List<Key> inputs)
{
    // Read and change the inputs and those changes will change what the game processes!
}
```

Note that whilst intercepts could be used to simply read inputs it adds extra processing so please use `OnInput` if you are only reading.

### Sending Inputs
Finally, you can send brand new inputs using the `SendKeys` method in IInputHook like below.
``` C#
inputHook.SendKey(Key.H);
```
Note that this doesn't send the input immediately, rather it adds it to a list of inputs that will be appended to any other inputs the game recieved the next time it polls for inputs (which happens very frequently). 
This delay shouldn't ever be noticeable, however, one case where it is useful to remember is when you are sending inputs from an intercept event. In that case the input will be added on the next input poll, not the current one.

### Supported Inputs
Currently only keyboard inputs are supported. All of the keys on a standard 100% keyboard should be correctly mapped however, if you find one that isn't or have a different type of keyboard that you want to add support for you can do so by adding to the `Key` enum that is inside `Inputs.cs` of `p5rpc.inputhook.interfaces`. 

You can see the numerical value of every key printed to the Reloaded console if you enable Debug mode in InputHook's configuration in Reloaded. 
The reason it is like this is because as far as I can tell the key codes used in the game (at the point I've hooked) are a non-standard format so I've manually mapped them to the enum for easy use.
