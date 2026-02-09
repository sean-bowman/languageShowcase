# ISA Calculator - FORTRAN

International Standard Atmosphere (ISA) Calculator implementing the ISO 2533:1975 standard atmosphere model.

## Overview

This tool computes atmospheric properties at any geometric altitude from sea level up to 86 km:

- **Temperature** (K, °C)
- **Pressure** (Pa, kPa, atm)
- **Density** (kg/m³)
- **Speed of Sound** (m/s, knots)

## Why FORTRAN?

FORTRAN (Formula Translation) has been NASA's workhorse for scientific computing since the 1950s. It excels at:

- Numerical computation with native array operations
- High-performance computing (still used for weather/climate models)
- Scientific formula implementation (reads like math)

Modern Fortran (90+) supports modules, derived types, and other structured programming features.

## Prerequisites

### Option 1: Fortran Package Manager (fpm) - Recommended

```bash
# Install fpm (Fortran Package Manager)
# On Windows with MSYS2:
pacman -S mingw-w64-x86_64-fpm

# Or download from: https://github.com/fortran-lang/fpm/releases
```

### Option 2: Manual compilation with gfortran

```bash
# Install gfortran via MSYS2:
pacman -S mingw-w64-x86_64-gcc-fortran
```

## Building and Running

### With fpm (recommended):

```bash
cd FORTRAN

# Build the project
fpm build

# Run the calculator (displays atmosphere table)
fpm run

# Run with a specific altitude (in meters)
fpm run -- 10000

# Run unit tests
fpm test
```

### Manual compilation:

```bash
cd FORTRAN/src

# Compile modules first (order matters!)
gfortran -c constants_module.f90
gfortran -c isa_module.f90

# Compile and link main program
gfortran -c ../app/main.f90
gfortran -o isa_calculator constants_module.o isa_module.o main.o

# Run
./isa_calculator
./isa_calculator 10000
```

## Usage Examples

### Display atmosphere table:

```bash
$ fpm run

==============================================================
     International Standard Atmosphere (ISA) Calculator
              Based on ISO 2533:1975
==============================================================

Sea Level Reference Values:
  Temperature:        288.15 K  (15.00 C)
  Pressure:        101325.0 Pa (101.325 kPa)
  Density:           1.2250 kg/m^3

Standard Atmosphere Table
=========================

  Altitude      Temperature      Pressure        Density       Sound Speed
    [km]           [K]            [Pa]          [kg/m^3]         [m/s]
  --------      -----------      --------       ---------      -----------
       0.0         288.15      1.0132E+05      1.2250E+00         340.29
       1.0         281.65      8.9875E+04      1.1117E+00         336.43
      ...
```

### Calculate at specific altitude:

```bash
$ fpm run -- 35000

Atmospheric Properties:
------------------------
  Altitude:            35000.0 m
                          35.0 km
                      114829.4 ft

  Temperature:          236.51 K
                        -36.64 C

  Pressure:         2.3882E+03 Pa
                          2.3882 kPa
                          0.0236 atm

  Density:          3.5185E-02 kg/m^3
                          0.0287 (ratio to sea level)

  Speed of Sound:       308.30 m/s
                       1109.87 km/h
                        599.21 knots
```

## Project Structure

```none
FORTRAN/
├── fpm.toml                    # Package manager configuration
├── README.md                   # This file
├── app/
│   └── main.f90               # CLI application
├── src/
│   ├── constants_module.f90   # Physical constants
│   └── isa_module.f90         # ISA calculations
├── test/
│   └── test_isa.f90           # Unit tests
└── references/
    └── REFERENCES.md          # Technical references
```

## Key Fortran Concepts Demonstrated

1. **Modules** - Encapsulate constants and procedures
2. **Derived Types** - `AtmosphericProperties` struct-like type
3. **Pure Functions** - Side-effect-free calculations
4. **Implicit None** - Explicit variable declarations
5. **Kind Parameters** - Double precision (`selected_real_kind`)
6. **Array Operations** - Batch calculations

## References

See [references/REFERENCES.md](references/REFERENCES.md) for detailed technical references.

Primary source: ISO 2533:1975 "Standard Atmosphere"
