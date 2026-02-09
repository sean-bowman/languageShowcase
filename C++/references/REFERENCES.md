# Hohmann Transfer Calculator - References

## Primary Sources

### Hohmann Transfer Orbit

Named after Walter Hohmann, who published the concept in 1925 in "Die Erreichbarkeit der Himmelskörper" (The Attainability of Celestial Bodies).

- **Wikipedia:** https://en.wikipedia.org/wiki/Hohmann_transfer_orbit
- **Orbital Mechanics Textbook:** https://orbital-mechanics.space/orbital-maneuvers/hohmann-transfer.html

### Key Properties

A Hohmann transfer is:
- The most fuel-efficient two-impulse maneuver between coplanar circular orbits
- An elliptical orbit tangent to both initial and final orbits
- Uses two impulsive burns (instantaneous velocity changes)

## Orbital Mechanics Equations

### Vis-Viva Equation

The fundamental equation relating velocity, position, and orbit shape:

```
v² = μ(2/r - 1/a)
```

Where:
- v = orbital velocity [m/s]
- μ = GM = gravitational parameter [m³/s²]
- r = distance from center of mass [m]
- a = semi-major axis [m]

### Circular Orbit Velocity

For a circular orbit (r = a):

```
v = √(μ/r)
```

### Hohmann Transfer Equations

For transfer from radius r₁ to r₂:

**Semi-major axis of transfer ellipse:**
```
a_transfer = (r₁ + r₂) / 2
```

**First burn delta-v (at r₁):**
```
Δv₁ = √(μ/r₁) × (√(2r₂/(r₁+r₂)) - 1)
```

**Second burn delta-v (at r₂):**
```
Δv₂ = √(μ/r₂) × (1 - √(2r₁/(r₁+r₂)))
```

**Transfer time (half the ellipse period):**
```
t = π × √(a³/μ)
```

### Phase Angle for Rendezvous

To rendezvous with a target in the destination orbit:

```
θ = π × (1 - (1/(2√2)) × √((r₁/r₂ + 1)³))
```

## Gravitational Parameters

Source: NASA JPL Planetary Fact Sheets (https://nssdc.gsfc.nasa.gov/planetary/factsheet/)

| Body | GM (m³/s²) | Radius (km) |
|------|-----------|-------------|
| Sun | 1.327×10²⁰ | 696,340 |
| Earth | 3.986×10¹⁴ | 6,371 |
| Moon | 4.905×10¹² | 1,737 |
| Mars | 4.283×10¹³ | 3,390 |
| Jupiter | 1.267×10¹⁷ | 69,911 |

## Common Earth Orbits

| Orbit | Altitude (km) | Radius (km) | Period |
|-------|--------------|-------------|--------|
| ISS | 420 | 6,791 | 92 min |
| LEO (typical) | 400 | 6,771 | 92 min |
| GPS | 20,200 | 26,571 | 12 hr |
| GEO | 35,786 | 42,157 | 24 hr |

## C++ Implementation References

### JSBSim

Open-source flight dynamics model in C++, used by NASA for validation:
- **GitHub:** https://github.com/JSBSim-Team/jsbsim
- **Paper:** "JSBSim: An Open Source Flight Dynamics Model in C++"

### AIAA Aerospace Simulations

"Building Aerospace Simulations in C++" by Peter Zipfel covers:
- Six degrees of freedom (6-DOF) modeling
- Object-oriented design for aerospace
- CADAC++ simulation framework

### Modern C++ Features Used

| Feature | Standard | Usage |
|---------|----------|-------|
| `std::optional` | C++17 | Optional body radius |
| `[[nodiscard]]` | C++17 | Warn if return value ignored |
| Structured bindings | C++17 | Destructuring returns |
| `constexpr` | C++11 | Compile-time constants |
| Namespace | C++98 | Code organization |

## Validation Data

### LEO to GEO Transfer

Expected values (Earth, 400 km to 35,786 km):
- Δv₁ ≈ 2,426 m/s
- Δv₂ ≈ 1,469 m/s
- Total Δv ≈ 3,935 m/s
- Transfer time ≈ 5.26 hours

### Earth-Mars Transfer (Heliocentric)

Expected values (simplified circular orbits):
- Δv₁ ≈ 2.94 km/s (at Earth orbit)
- Δv₂ ≈ 2.65 km/s (at Mars orbit)
- Total Δv ≈ 5.59 km/s
- Transfer time ≈ 259 days (~8.5 months)

## Additional Resources

- **NASA Glenn Research Center - Orbit Mechanics:** https://www1.grc.nasa.gov/beginners-guide-to-aeronautics/orbit-mechanics/
- **Orbital Mechanics for Engineering Students** by Howard Curtis
- **GMAT (General Mission Analysis Tool):** https://software.nasa.gov/software/GSC-17177-1
