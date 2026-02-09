# ISA Calculator - References

## Primary Standards

### ISO 2533:1975 - Standard Atmosphere
The International Organization for Standardization (ISO) Standard Atmosphere defines the atmospheric model used in this calculator.

- **Standard:** ISO 2533:1975 "Standard Atmosphere"
- **Publisher:** International Organization for Standardization
- **Status:** Active standard (with addenda)
- **Purchase:** https://www.iso.org/standard/7472.html

### ICAO Standard Atmosphere
The International Civil Aviation Organization uses the same model:
- **Document:** ICAO Doc 7488/3 "Manual of the ICAO Standard Atmosphere"

## Online References

### Engineering LibreTexts - ISA Equations
Comprehensive derivation of ISA equations with worked examples.
- **URL:** https://eng.libretexts.org/Bookshelves/Aerospace_Engineering/Fundamentals_of_Aerospace_Engineering_(Arnedo)/02:_Generalities/2.03:_Standard_atmosphere/2.3.03:_ISA_equations
- **Archived:** 2024

### Embry-Riddle Aeronautical University - ISA Introduction
Introduction to atmospheric properties and the ISA model for flight vehicles.
- **URL:** https://eaglepubs.erau.edu/introductiontoaerospaceflightvehicles/chapter/international-standard-atmosphere-isa/

### AeroToolbox - Standard Atmosphere Calculator
Online calculator and reference for ISA calculations with detailed explanations.
- **URL:** https://aerotoolbox.com/atmcalc/

## Key Values Used

### Sea Level Conditions
| Property | Value | Unit |
|----------|-------|------|
| Temperature | 288.15 | K |
| Pressure | 101325 | Pa |
| Density | 1.225 | kg/m³ |
| Speed of Sound | 340.29 | m/s |

### Physical Constants
| Constant | Value | Unit |
|----------|-------|------|
| Gas constant (R) | 287.05287 | J/(kg·K) |
| Gravity (g₀) | 9.80665 | m/s² |
| Ratio of specific heats (γ) | 1.4 | - |

### Atmospheric Layers
| Layer | Altitude Range | Lapse Rate |
|-------|---------------|------------|
| Troposphere | 0 - 11 km | -6.5 K/km |
| Tropopause | 11 - 20 km | 0.0 K/km |
| Lower Stratosphere | 20 - 32 km | +1.0 K/km |
| Upper Stratosphere | 32 - 47 km | +2.8 K/km |
| Lower Mesosphere | 47 - 51 km | 0.0 K/km |
| Mid Mesosphere | 51 - 71 km | -2.8 K/km |
| Upper Mesosphere | 71 - 86 km | -2.0 K/km |

## Formula Summary

### Gradient Layers (non-zero lapse rate)
```
T = T_base + L × (h - h_base)
P = P_base × (T / T_base)^(-g₀ / (L × R))
```

### Isothermal Layers (zero lapse rate)
```
T = T_base (constant)
P = P_base × exp(-g₀ × (h - h_base) / (R × T))
```

### Derived Properties
```
ρ = P / (R × T)                    (Ideal gas law)
a = √(γ × R × T)                   (Speed of sound)
```

## Validation Data Sources

ICAO publishes tabulated values at standard altitudes that can be used to validate implementations. Values at key altitudes:

| Altitude (m) | Temperature (K) | Pressure (Pa) | Density (kg/m³) |
|-------------|-----------------|---------------|-----------------|
| 0 | 288.15 | 101325 | 1.2250 |
| 1000 | 281.65 | 89874.6 | 1.1117 |
| 5000 | 255.65 | 54019.9 | 0.7361 |
| 10000 | 223.15 | 26436.3 | 0.4127 |
| 11000 | 216.65 | 22632.1 | 0.3639 |
| 20000 | 216.65 | 5474.9 | 0.0880 |
| 32000 | 228.65 | 868.0 | 0.0132 |
