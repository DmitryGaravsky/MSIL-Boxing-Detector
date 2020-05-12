# MSIL Boxing Detector

MSIL-level boxing detector tool for .Net (powered by [ILReader](https://github.com/DmitryGaravsky/ILReader))


### Boxing patterns detected:

1) Typical errors in string operations:

```cs
int a = 42;
string answer = string.Format("{0}", a); // boxing here
```

```cs
int a = 2; int b = 3;
string question = string.Concat(a," + ", b); // boxing here
```

2) Boxing with the `Enum.HasValue` method call:

```cs
[Flags]
enum State { One, Two }
//...
var state = State.One | State.Two;
bool isOne = state.HasFlag(State.One); // boxing here
```


### Usage scenarios:

1) To process all the assemblies within the MSIL Boxing Detector **current folder**
add all the specific assemblies into this folder and run the `BoxingDetector.exe` without arguments.

2) To process the specific assemblies use the following snippet:

```cmd
BoxingDetector.exe AssemblyXXX.dll AssemblyYYY.dll AssemblyZZZ.dll
```
