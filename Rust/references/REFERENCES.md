# Aerospace Units Library - References

## Primary Motivation

### Mars Climate Orbiter Mishap (1999)

The Mars Climate Orbiter was lost due to a unit conversion error - one team used metric units (Newton-seconds), another used imperial units (pound-force-seconds). This $327 million mission failure is a classic example of why type-safe unit systems matter.

- **NASA Lesson Learned:** https://llis.nasa.gov/lesson/384
- **Root Cause:** Software interface error between ground-based systems
- **Impact:** Complete loss of spacecraft during Mars orbit insertion

## Rust in Safety-Critical Systems

### Ferrocene - Qualified Rust Compiler

Ferrous Systems qualified their open-source Rust compiler for safety-critical applications:

- **ISO 26262 (Automotive):** ASIL D qualification
- **IEC 61508 (Industrial):** SIL 4 qualification
- **Future Plans:** DO-178C (Aerospace) qualification
- **URL:** https://ferrous-systems.com/ferrocene/

### Research Paper: Rust in Space

"Bringing Rust to Safety-Critical Systems in Space" - 2024 research paper evaluating Rust for aerospace use.

- **URL:** https://arxiv.org/html/2405.18135v1
- **Key Finding:** Rust meets DO-178C evaluation criteria for programming languages
- **Target Platform:** PowerPC (used in James Webb Space Telescope)

### KubOS - Satellite Software Stack

KubOS uses Rust for satellite flight software components.

- **URL:** https://github.com/kubos/kubos
- **Application:** CubeSat and small satellite operations

## Unit Conversion Standards

### SI Units (International System)

| Quantity | SI Base/Derived Unit | Symbol |
|----------|---------------------|--------|
| Length | meter | m |
| Mass | kilogram | kg |
| Time | second | s |
| Force | Newton (kg·m/s²) | N |
| Pressure | Pascal (N/m²) | Pa |
| Angle | radian | rad |

### Common Aerospace Conversions

| From | To | Factor |
|------|-----|--------|
| 1 foot | meters | 0.3048 |
| 1 nautical mile | meters | 1852 |
| 1 knot | m/s | 0.514444 |
| 1 pound-force | Newtons | 4.44822 |
| 1 psi | Pascals | 6894.757 |
| 1 atmosphere | Pascals | 101325 |

## Rocket Propulsion References

### Tsiolkovsky Rocket Equation

The fundamental equation for rocket propulsion:

```
Δv = Isp × g₀ × ln(m₀/mf)
```

Where:
- Δv = change in velocity (m/s)
- Isp = specific impulse (s)
- g₀ = standard gravity (9.80665 m/s²)
- m₀ = initial (wet) mass
- mf = final (dry) mass

### Typical Engine Performance

| Engine | Isp (vacuum) | Thrust | Application |
|--------|-------------|--------|-------------|
| F-1 | 263 s (SL) | 6.77 MN | Saturn V first stage |
| RS-25 (SSME) | 452 s | 2.28 MN | Space Shuttle |
| Merlin 1D | 311 s | 914 kN | Falcon 9 |
| RL-10 | 465 s | 110 kN | Centaur upper stage |
| Ion thruster | ~3000 s | ~0.1 N | Deep space missions |

## Programming References

### Rust Newtype Pattern

The "newtype" pattern wraps a primitive type to create a distinct type:

```rust
struct Length(f64);  // f64 wrapped as Length
struct Mass(f64);    // f64 wrapped as Mass

// These are now incompatible types!
// Length + Mass will not compile
```

### Operator Overloading in Rust

Rust uses traits from `std::ops` for operator overloading:

- `Add` - implements `+`
- `Sub` - implements `-`
- `Mul` - implements `*`
- `Div` - implements `/`
- `Neg` - implements unary `-`

### Zero-Cost Abstractions

Rust's type wrappers compile to the same machine code as raw primitives. The type safety is enforced at compile time with no runtime overhead.

## Additional Resources

- **uom crate:** https://github.com/iliekturtles/uom - Full dimensional analysis library
- **dimensioned crate:** https://docs.rs/dimensioned - Alternative units library
- **Rust Book:** https://doc.rust-lang.org/book/ - Official Rust documentation
