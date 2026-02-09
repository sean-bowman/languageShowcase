# Personal Code Styling Guidelines

This document provides soft guidelines for coding practices to help you write code that integrates naturally with existing codebases. These are not strict rules—use your judgment, prioritize clarity, and ask questions when uncertain.

The goal is consistency and readability, not rigid enforcement. These conventions apply across all languages in this repository.

---

## Naming Conventions

### Variables and Functions → camelCase

```python
chamberPressure = 15.0
massFlowRate = 2.5
userInputs = {}

def calculatePressure():
def convertTemperatureToKelvin(temperature, unit):
```

### Classes → PascalCase

```python
class Nozzle:
class EngineState:
class UnitConversions:
```

### Private Members → _leadingUnderscore (then camelCase)

```python
def _validateInputs(self):
def _calculateAllocation(self, userPosition):
_privateVariable = 10
```

### Constants → camelCase

We don't use SCREAMING_CASE for constants.

```python
maxCpuCores = 16
sessionTimeout = 300
minChannelRadius = 0.75e-3
```

---

## String Conventions

**Use single quotes for all strings.** This is one of the more consistent patterns in the codebase.

```python
# Preferred
fuel = 'HDPE'
oxidizer = 'O2'
message = f'Chamber pressure: {pressure} Pa'

# Avoid double quotes
fuel = "HDPE"  # not preferred
```

**Exception:** Strings containing single quotes may use double quotes to avoid escaping:

```python
message = "Can't find the file"  # acceptable
```

However, you can still use single quotes with the escape character (`\`) to correctly render quotes inside of single quote strings:

```python
message = 'Can\'t find the file' # preferred
```

**Docstrings use triple single quotes:**

```python
'''
This is a docstring.
'''
```

---

## Documentation

### File Headers

Every Python file should start with a title comment and docstring:

```python
# -- Nozzle Class Definition -- #

'''

Brief description of what this file contains.

More detailed explanation if needed.

[Your Name] - [MM/DD/YYYY]

'''
```

### Class Docstrings

Classes should have comprehensive docstrings that explain purpose, parameters, and usage:

```python
class EngineState:
    '''
    Engine operating states based on chamber pressure relative to ambient.

    States:
    -------
    OFF : Pc < 2x ambient - No choked flow
    TRANSITION : 2x ambient <= Pc but combustion not converged
    COMBUSTION : Full combustion model converged
    FAILED : Could not achieve viable operating condition

    Sean Bowman - [12/30/2025]
    '''
```

#### VSCode Docstring Snippet (Recommended)

For comprehensive class documentation, add the following snippet to your VSCode user snippets (`File > Preferences > Configure User Snippets > python.json`). This provides a standardized template that you can insert by typing `docs` and pressing Tab:

```json
{
    "docstring": {
        "prefix": "docs",
        "body": [
            "'''",
            "",
            "${1:Brief description.}",
            "",
            "Core Capabilities:",
            "------------------",
            "1. ${2:Does mathy things}",
            "",
            "Primary Input Properties:",
            "-------------------------",
            "${3:prop1} : ${4:dataType}",
            "    ${5:Description of prop1}",
            "",
            "Key Output Properties:",
            "----------------------",
            "${6:prop3} : ${7:dataType}",
            "    ${8:Description of prop3}",
            "",
            "Public Methods:",
            "---------------",
            "${9:methodName}(${10:params})",
            "    ${11:Method description}",
            "",
            "Typical Workflow:",
            "-----------------",
            "1. Instantiate: ${12:example} = ${13:ClassName}()",
            "2. Set inputs: ${12:example}.setInputs(configPath) or set properties directly",
            "3. Generate math: ${12:example}.${9:methodName}(${10:params})",
            "",
            "Examples:",
            "---------",
            "",
            "Manual attribute-based setup:",
            "",
            ">>> ${12:example} = ${13:ClassName}()",
            ">>> ${12:example}.${3:prop1} = ${14:value}",
            ">>> ${12:example}.${9:methodName}(${10:params})",
            ">>> print(${12:example}.${6:prop3})",
            "",
            "Notes:",
            "------",
            "- ${15:Notes about usage, limitations, or special considerations.}",
            "",
            "See Also:",
            "---------",
            "${16:relatedClass} : ${17:Class with a related functionality}",
            "",
            "${18:AuthorName} - [${19:MM/DD/YYYY}]",
            "",
            "'''"
        ],
        "description": "Insert standard class docstring template"
    }
}
```

This snippet uses tab stops (`$1`, `$2`, etc.) so you can quickly fill in each section by pressing Tab to move through the template.

### Method Docstrings

Public methods should document parameters and return values:

```python
def convertTemperatureToKelvin(self, temperature: float, unit: str) -> float:
    '''
    Converts a temperature value from a specified unit to Kelvin.

    Parameters:
    -----------
    temperature : float
        The temperature value to convert
    unit : str
        The unit of the input temperature ('K', 'C', 'F')

    Returns:
    --------
    float : The temperature value in Kelvin

    '''
```

### Section Break Comments

Use these to organize major sections within files:

```python
# ---------------------------------------------------------------------- #
# -- Background Job Factory Functions -- #
# ---------------------------------------------------------------------- #
```

Or the shorter form for subsections:

```python
# -- Nozzle Contour Properties -- #
```

### Inline Comments

Add unit annotations when storing physical quantities:

```python
self.chamberPressure = []    # [Pa]
self.thrust = []             # [N]
self.massFlowRate = []       # [kg/s]
self.temperature = []        # [K]
self.maxVelocity = []        # [m/s]
```

---

## Documentation by Language

The documentation principles above apply across all languages. Here's how they translate:

### Python

```python
'''
Brief description of module/class/function.

Parameters:
-----------
altitude : float
    The altitude in meters

Returns:
--------
float : The temperature in Kelvin
'''
```

### C# (XML Documentation)

```csharp
/// <summary>
/// Brief description of class/method.
/// </summary>
/// <param name="altitude">The altitude in meters</param>
/// <returns>The temperature in Kelvin</returns>
```

### C++

```cpp
/*
 * Brief description of function.
 *
 * Parameters:
 *   altitude - The altitude in meters
 *
 * Returns:
 *   The temperature in Kelvin
 */
```

### Rust

```rust
/// Brief description of function.
///
/// # Arguments
/// * `altitude` - The altitude in meters
///
/// # Returns
/// The temperature in Kelvin
```

### FORTRAN

```fortran
!------------------------------------------------------------------------------
! Brief description of subroutine/function.
!
! Parameters:
!   altitude - The altitude in meters
!
! Returns:
!   The temperature in Kelvin
!------------------------------------------------------------------------------
```

### JavaScript (JSDoc)

```javascript
/**
 * Brief description of function.
 * @param {number} altitude - The altitude in meters
 * @returns {number} The temperature in Kelvin
 */
```

---

## Type Hints

Use type hints for function parameters and returns. This helps with readability and IDE support:

```python
def setInputs(self, inputsPath: str | dict, debugMode: bool = False) -> None:

def calculateAreaRatio(machNumber: float) -> float:

def validateDesignFolder(designPath: str) -> tuple:
```

---

## Import Organization

Organize imports in this order with blank lines between groups:

```python
# Standard library imports
import os
import sys
import copy
import warnings
from typing import Optional, Dict

# Third-party imports
import numpy as np
import pandas as pd
import streamlit as st
from scipy.optimize import fsolve

# Local imports
from myutils import helpers
from mypackage.utils import UnitConversions
```

For modules that may be used both as standalone and as part of a package:

```python
try:
    # Absolute imports - used when running the file directly
    # These work when the module directory is on Python's sys.path
    from myutils import helpers
    from MyClass import MyClass
except ImportError:
    # Relative imports - used when the module is imported as part of a package
    # The dot (.) means 'from the same package/directory as this file'
    # Required when calling the class externally (e.g., from mypackage import MyClass)
    from .myutils import helpers
    from .MyClass import MyClass
```

---

## Code Structure Patterns

### Class Organization

```python
class Example:
    '''Class docstring...'''

    # -- Constructor -- #
    def __init__(self):
        self.prop1 = []
        self.prop2 = []

    # -- Private Helper Methods -- #
    def _validateInputs(self):
        '''Private validation logic.'''
        pass

    # -- Public Methods -- #
    def setInputs(self, inputs: dict) -> None:
        '''Public method for setting inputs.'''
        pass

    def calculate(self) -> float:
        '''Main calculation method.'''
        pass
```

### Helper Functions Inside Methods

For complex calculations, define helper functions within the method that uses them:

```python
def convergingSection(self, raoThroatAngle: float = 'default'):
    '''Generate the converging section geometry.'''

    def calculateAreaRatio(machNumber) -> float:
        '''Calculate area ratio for given Mach number.'''
        areaRatio = (1 / machNumber) * (...)
        return areaRatio

    def calculateFlowProperties(x, r) -> np.ndarray:
        '''Calculate flow properties along the section.'''
        # Uses calculateAreaRatio internally
        pass

    # Main method logic using helpers
    result = calculateFlowProperties(xCoords, rCoords)
```

---

## Error Handling

Error handling ensures code fails gracefully with informative messages. Use two complementary approaches:

### Input Validation with Private Methods

Create a private `_validateInputs()` method to check inputs before expensive calculations. Call it early in public methods.

```python
def _validateInputs(self) -> None:
    '''
    Validate input properties before calculations.

    Raises:
    -------
    TypeError : If inputs are not the expected type
    ValueError : If inputs are outside valid ranges
    '''

    # Type validation - use isinstance() for type checking
    if not isinstance(self.prop1, (int, float)):
        raise TypeError(f'prop1 must be numeric, got {type(self.prop1).__name__}')

    # Value validation - check ranges and constraints
    if self.prop1 < 0:
        raise ValueError(f'prop1 must be non-negative, got {self.prop1}')

def _validateMathyParams(self, mathyParams: dict) -> None:
    '''
    Validate that mathyParams matches the expected format exactly.

    Expected format: {'param1': <numeric>, 'param2': <numeric>}
    '''

    requiredKeys = {'param1', 'param2'}

    # Check that input is a dictionary
    if not isinstance(mathyParams, dict):
        raise TypeError(f'mathyParams must be a dictionary, got {type(mathyParams).__name__}')

    # Check for exact key match (no missing, no extra)
    providedKeys = set(mathyParams.keys())
    if providedKeys != requiredKeys:
        missing = requiredKeys - providedKeys
        extra = providedKeys - requiredKeys
        errorParts = []
        if missing:
            errorParts.append(f'missing keys: {missing}')
        if extra:
            errorParts.append(f'unexpected keys: {extra}')
        raise ValueError(f'mathyParams format error - {", ".join(errorParts)}')

    # Validate that both values are numeric
    for key in requiredKeys:
        value = mathyParams[key]
        if not isinstance(value, (int, float)):
            raise TypeError(f'{key} must be numeric, got {type(value).__name__}')
```

**When to use:**

- Before any calculation that depends on user-provided values
- At the start of public methods that accept parameters
- When inputs come from external sources (files, APIs, user input)

### Try-Except for Runtime Errors

Use try-except blocks for operations that may fail at runtime (file I/O, external calls, complex calculations).

```python
def loadConfiguration(self, configPath: str) -> dict:
    '''Load configuration from file with proper error handling.'''

    try:
        with open(configPath, 'r') as f:
            config = json.load(f)
        return config

    except FileNotFoundError:
        raise FileNotFoundError(f'Configuration file not found: {configPath}')

    except json.JSONDecodeError as e:
        raise ValueError(f'Invalid JSON in {configPath}: {e}') from e
```

**Best practices:**

- Catch specific exceptions, not bare `except:`
- Use `from e` to preserve the original traceback
- Add context to error messages (file paths, parameter values)
- Use `finally` for cleanup that must always run

```python
try:
    result = performCalculation()
except ConvergenceError as e:
    # Re-raise with additional context
    raise ConvergenceError(f'Failed at iteration {i}: {e}') from e
finally:
    # Always runs - use for cleanup
    cleanupResources()
```

### Custom Exceptions

For domain-specific errors, consider defining custom exception classes:

| Exception Type               | Use Case                                         |
| ---------------------------- | ------------------------------------------------ |
| `InvalidInputError`        | Input validation failures with parameter context |
| `ConvergenceFailureError`  | Iterative solver failures                        |
| `GeometricConstraintError` | Geometry constraint violations                   |

Example custom exception with context:

```python
class InvalidInputError(ValueError):
    '''Custom exception for input validation with context.'''
    def __init__(self, message, parameterName=None, value=None, validRange=None):
        self.parameterName = parameterName
        self.value = value
        self.validRange = validRange
        super().__init__(message)

if chamberPressure < 0:
    raise InvalidInputError(
        message='Chamber pressure must be positive',
        parameterName='chamberPressure',
        value=chamberPressure,
        validRange='> 0'
    )
```

**When to use custom vs standard exceptions:**

- Use `TypeError` / `ValueError` for simple validation in utility code
- Use custom exceptions when you need to track additional context (iteration count, parameter ranges, etc.)
- Use custom exceptions in core calculation classes where detailed error reporting aids debugging

---

## Formatting

**Line Length:** Aim for 120 characters max, but don't stress about it. Readability matters more.

**Spacing:** Use spaces around assignment operators:

```python
# Preferred
chamberPressure = 15.0
massFlowRate = 2.5

# Also acceptable for alignment in related variables
self.fuel     = 'HDPE'
self.oxidizer = 'O2'
```

---

## Language-Specific Naming Conventions

While this repository generally follows camelCase naming for consistency, **some languages enforce snake_case through compiler warnings or strong community conventions**. In these cases, we follow the language's idiomatic style to avoid friction.

### Rust → snake_case (Enforced)

Rust enforces snake_case through compiler warnings (`non_snake_case`). Using camelCase generates warnings and goes against the Rust API Guidelines.

**Rationale:**
- The Rust compiler issues warnings for any non-snake_case names
- The entire Rust ecosystem uses snake_case (std library, all crates)
- Fighting this creates unnecessary warning noise
- Rust's official style guide mandates snake_case for functions and variables

**Examples:**
```rust
// Correct (snake_case)
fn calculate_delta_v(wet_mass: f64, dry_mass: f64) -> f64 { }
let chamber_pressure = 15.0;

// Incorrect (generates compiler warnings)
fn calculateDeltaV(wetMass: f64, dryMass: f64) -> f64 { }  // warning: non_snake_case
let chamberPressure = 15.0;  // warning: non_snake_case
```

### FORTRAN → snake_case (Conventional)

FORTRAN is case-insensitive (all names are converted to uppercase internally), but the community convention is snake_case for readability.

**Rationale:**
- FORTRAN compilers treat `MyVariable`, `myvariable`, and `MYVARIABLE` as identical
- Modern FORTRAN code (2003/2008) conventionally uses lowercase snake_case
- This improves readability in a case-insensitive environment
- Mixed-case names serve no functional purpose

**Examples:**
```fortran
! Conventional (lowercase snake_case)
function isa_temperature(altitude) result(temp)
real :: chamber_pressure

! Technically valid but unconventional
function IsaTemperature(Altitude) result(Temp)  ! Works, but not idiomatic
REAL :: CHAMBER_PRESSURE  ! Old FORTRAN 77 style
```

### Other Languages → camelCase

All other languages in this repository (Python, C++, C#, JavaScript) follow the camelCase conventions outlined above.

---

## Quick Reference

| Element    | Convention     | Example               | Exceptions |
| ---------- | -------------- | --------------------- | ---------- |
| Variables  | camelCase      | `chamberPressure`   | Rust, FORTRAN: snake_case |
| Functions  | camelCase      | `calculateThrust()` | Rust, FORTRAN: snake_case |
| Classes    | PascalCase     | `EngineState`       | |
| Private    | _camelCase     | `_validateInputs()` | |
| Constants  | camelCase      | `maxCpuCores`       | |
| Strings    | single quotes  | `'HDPE'`            | |
| Docstrings | triple single  | `'''...'''`         | |
| Units      | inline comment | `# [Pa]`            | |

---

## Philosophy

1. **Clarity over cleverness** — Write code that's easy to read and maintain
2. **Document the "why"** — Comments should explain intent, not repeat what the code does
3. **Consistency within a file** — If you're editing an existing file, match its style
4. **Ask when uncertain** — When patterns are ambiguous, ask the team
