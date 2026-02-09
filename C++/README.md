# Hohmann Transfer Calculator - C++

A C++ application for calculating Hohmann transfer orbits between circular orbits.

## Overview

This calculator computes the delta-v requirements and transfer times for Hohmann transfer maneuvers - the most fuel-efficient two-impulse method to transfer between two coplanar circular orbits.

**How a Hohmann Transfer Works:**
1. **First burn (Δv₁)**: At the initial orbit, burn prograde to raise the apoapsis to the target orbit
2. **Coast phase**: Travel along the transfer ellipse (half an orbital period)
3. **Second burn (Δv₂)**: At apoapsis, burn prograde to circularize at the target orbit

## Why C++?

C++ is the industry standard for flight dynamics simulation:
- **JSBSim**: Open-source flight dynamics model used by FlightGear and NASA
- **High performance**: Critical for real-time simulation
- **Object-oriented design**: Natural fit for modeling physical objects (orbits, bodies)

## Prerequisites

### Windows (Visual Studio)
1. Install Visual Studio 2022 with "Desktop development with C++" workload
2. CMake is included with Visual Studio

### Windows (MinGW/MSYS2)
```bash
pacman -S mingw-w64-x86_64-gcc mingw-w64-x86_64-cmake
```

## Building

```bash
cd C++

# Create build directory
mkdir build
cd build

# Configure with CMake
cmake ..

# Build
cmake --build .

# Or on Windows with Visual Studio:
cmake --build . --config Release
```

## Running

```bash
# Default: shows common transfers and LEO-GEO example
./hohmann

# Custom transfer (altitudes in km)
./hohmann 400 20200     # LEO to GPS orbit
./hohmann 420 35786     # ISS to GEO

# Run examples
./leo_to_geo
./earth_mars
```

## Example Output

```
========================================
       Common Earth Orbit Transfers
========================================

LEO (400 km) → GEO (35,786 km):
  Total Δv: 3935.46 m/s
  Time:     5.26 hours

LEO (400 km) → GPS (20,200 km):
  Total Δv: 3574.89 m/s
  Time:     3.53 hours

...
```

## Project Structure

```
C++/
├── CMakeLists.txt           # Build configuration
├── README.md                # This file
├── include/hohmann/
│   ├── constants.hpp        # Physical constants (GM values, etc.)
│   ├── celestial_body.hpp   # CelestialBody class
│   ├── orbit.hpp            # Orbit class
│   └── hohmann_transfer.hpp # HohmannTransfer class
├── src/
│   ├── main.cpp             # CLI application
│   ├── celestial_body.cpp   # CelestialBody implementation
│   ├── orbit.cpp            # Orbit implementation
│   └── hohmann_transfer.cpp # Transfer calculations
├── examples/
│   ├── leo_to_geo.cpp       # Detailed LEO-GEO example
│   └── earth_mars.cpp       # Interplanetary transfer
└── references/
    └── REFERENCES.md        # Technical references
```

## Key C++ Concepts Demonstrated

1. **Classes and OOP** - CelestialBody, Orbit, HohmannTransfer
2. **Header/Source Separation** - Include guards, declarations vs definitions
3. **`std::optional`** - For optional body radius
4. **`[[nodiscard]]`** - Modern C++ attribute for return values
5. **CMake** - Cross-platform build system
6. **Namespace Organization** - `hohmann::` namespace

## Key Equations

### Vis-Viva Equation
```
v² = μ(2/r - 1/a)
```
Where:
- v = orbital velocity
- μ = GM (gravitational parameter)
- r = current orbital radius
- a = semi-major axis

### Hohmann Transfer Delta-v
```
Δv₁ = √(μ/r₁) × (√(2r₂/(r₁+r₂)) - 1)
Δv₂ = √(μ/r₂) × (1 - √(2r₁/(r₁+r₂)))
```

### Transfer Time
```
t = π × √(a³/μ)
```
Where a = (r₁ + r₂) / 2 (semi-major axis of transfer ellipse)

## References

See [references/REFERENCES.md](references/REFERENCES.md) for detailed technical references.

- [Hohmann Transfer - Wikipedia](https://en.wikipedia.org/wiki/Hohmann_transfer_orbit)
- [Orbital Mechanics - orbital-mechanics.space](https://orbital-mechanics.space/orbital-maneuvers/hohmann-transfer.html)
- [JSBSim Flight Dynamics Model](https://github.com/JSBSim-Team/jsbsim)
