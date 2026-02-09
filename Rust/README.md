# Aerospace Units - Rust

A type-safe unit conversion library for aerospace engineering that prevents unit errors at compile time.

## Overview

This library demonstrates Rust's type system by creating strongly-typed unit quantities. Operations between incompatible units (like adding Force to Length) result in compile-time errors, not runtime bugs.

**Why does this matter?** The Mars Climate Orbiter was lost in 1999 due to a unit conversion error between metric and imperial units - a $327 million mistake that proper type safety could have prevented.

## Why Rust?

Rust is increasingly adopted for safety-critical aerospace systems:
- **Memory safety** without garbage collection
- **Zero-cost abstractions** - type safety with no runtime overhead
- **Ferrocene** compiler qualified for ISO 26262 (ASIL D), IEC 61508 (SIL 4)
- Used in satellite software (KubOS) and embedded systems

## Prerequisites

Install Rust via [rustup](https://rustup.rs):

```bash
# Windows (PowerShell):
winget install Rustlang.Rustup

# Or download from https://rustup.rs
```

## Building and Running

```bash
cd Rust

# Build the library
cargo build

# Run tests
cargo test

# Run examples
cargo run --example unit_demo
cargo run --example rocket_equation
```

## Usage

```rust
use aerospace_units::prelude::*;

// Create quantities with explicit units
let altitude = Length::from_feet(35000.0);
println!("Altitude: {} m", altitude.as_meters());  // 10668 m

let thrust = Force::from_kilonewtons(845.0);
let mass_flow = MassFlowRate::from_kg_per_s(270.0);
let isp = thrust.specific_impulse(mass_flow);
println!("Isp: {} s", isp.as_seconds());

// Type-safe arithmetic
let delta_altitude = Length::from_meters(1000.0);
let new_altitude = altitude + delta_altitude;  // OK: Length + Length

// This won't compile - incompatible types!
// let bad = thrust + altitude;  // Error: cannot add Force and Length
```

## Unit Types

| Type | Internal Unit | Supported Conversions |
|------|--------------|----------------------|
| `Length` | meters | km, feet, miles, nautical miles |
| `Velocity` | m/s | km/s, km/h, ft/s, knots, mph, Mach |
| `Mass` | kg | g, tonnes, lb, slugs |
| `Force` | N | kN, MN, lbf, klbf |
| `Pressure` | Pa | kPa, MPa, bar, atm, psi, inHg, mmHg |
| `Angle` | radians | degrees, arcmin, arcsec, revolutions |
| `MassFlowRate` | kg/s | lb/s, tonnes/s |
| `SpecificImpulse` | seconds | m/s (exhaust velocity) |

## Project Structure

```
Rust/
├── Cargo.toml              # Package manifest
├── README.md               # This file
├── src/
│   ├── lib.rs             # Library entry point
│   └── units/
│       ├── mod.rs         # Module declarations
│       ├── length.rs      # Length type
│       ├── velocity.rs    # Velocity type
│       ├── mass.rs        # Mass type
│       ├── force.rs       # Force/thrust type
│       ├── pressure.rs    # Pressure type
│       ├── angle.rs       # Angle type
│       ├── mass_flow_rate.rs
│       └── specific_impulse.rs
├── examples/
│   ├── unit_demo.rs       # Basic conversion demo
│   └── rocket_equation.rs # Tsiolkovsky equation
└── references/
    └── REFERENCES.md      # Technical references
```

## Key Rust Concepts Demonstrated

1. **Newtype Pattern** - Wrapping primitives for type safety
2. **Operator Overloading** - `std::ops` traits (Add, Sub, Mul, Div)
3. **Method Chaining** - Fluent API design
4. **Unit Tests** - `#[cfg(test)]` modules
5. **Documentation Comments** - `///` for public API docs
6. **Modules** - Organizing code into logical units

## References

See [references/REFERENCES.md](references/REFERENCES.md) for technical references.

- [Mars Climate Orbiter Mishap Investigation](https://llis.nasa.gov/lesson/384)
- [Ferrocene - Qualified Rust Compiler](https://ferrous-systems.com/ferrocene/)
- [Rust in Safety-Critical Space Systems](https://arxiv.org/html/2405.18135v1)
