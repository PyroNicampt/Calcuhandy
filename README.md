# Calcuhandy
 A desktop equation evaluator powered by [Avalonia](https://github.com/avaloniaui/avalonia), inspired by the live equation parsing in most browser consoles and the Quick Search's calculator functionality in the game [Satisfactory](https://www.satisfactorygame.com/).

## Compiling
Requires [.NET CLI](https://learn.microsoft.com/en-us/dotnet/core/install/)

build with `dotnet cake`, output will be in `releases/Calcuhandy-xxx.zip`.
test with `dotnet cake --target Debug`.

## Running
Currently requires [.NET 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) to run. This may change in the future.

## Using
- Press Win+C to open the calculator's entry field at the cursor.
- Escape or click out to exit.
- Enter copies the output to the clipboard.

| Operator | Action |
| --- | --- |
| + | Add |
| - | Subtract (or negative) |
| * | Multiply |
| / | Divide |
| % | Remainder |
| ^ | Power |
| \( \) \, | Grouping |

| Function | Description |
| --- | --- |
| | ***Basic Functions*** |
| abs(x) | Absolute value |
| sqrt(x) | Square Root |
| cbrt(x) | Cube Root |
| mod(x,y) | Modulo of x by y |
| max(x,y) | Largest of x and y |
| min(x,y) | Smallest of x and y |
| round(x) | Round to nearest integer |
| floor(x) | Round down to integer |
| ceil(x) | Round up to integer |
| trunc(x) | Round towards zero |
| sign(x) | 1 if x greater than zero, -1 if x less than zero, 0 if x is zero |
| clamp(x,y,z) | Clamp x between y and z inclusive |
| inverse(x) | multiplies by -1. The same as prefixing with a - |
| | ***Trigonometry*** |
| sin(x) | Sine |
| cos(x) | Cosine |
| tan(x) | Tangent |
| asin(x) | Arcsine |
| acos(x) | Arccosine |
| atan(x) | Arctangent |
| atan2(x,y) | Arctangent2 |
| sinh(x) | Hyperbolic Sine |
| cosh(x) | Hyperbolic Cosine |
| tanh(x) | Hyperbolic Tangent |
| asinh(x) | Hyperbolic Arcsine |
| acosh(x) | Hyperbolic Arccosine |
| atanh(x) | Hyperbolic Arctangent |
| | ***Logarithms*** |
| log10(x) | Base 10 Logarithm of x |
| log2(x) | Base 2 Logarithm of x |
| log(x) | Logarithm of x |

| Constant | Description |
| --- | --- |
| pi | 3.14159265358979... |
| tau | 6.28318530717958... |
| phi | The golden ratio (1.6180339...) |
| e | Euler's number (2.718281...) |
| ln2 | Natural log of 2 (0.693147...) |
| ln10 | Natural log of 10 (2.30258...) |
| deg2rad | Multiplier for degrees to radians (tau/360) |
| rad2deg | Multiplier for radians to degrees (360/tau) |
| nan | Not a Number |
| infinity | Infinity |
