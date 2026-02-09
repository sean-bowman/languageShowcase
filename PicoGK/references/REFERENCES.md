# Computational Nozzle Geometry - References

## PicoGK Resources

### Official Documentation
- **PicoGK GitHub:** https://github.com/leap71/PicoGK
- **PicoGK Documentation:** https://picogk.org/
- **Setup Guide:** https://picogk.org/doc/setup.html
- **Coding for Engineers Book:** https://picogk.org/coding-for-engineers/

### Key Tutorials
- **First Steps in PicoGK:** https://picogk.org/coding-for-engineers/8-first-steps-in-picogk.html
- **Computational Geometry:** https://picogk.org/coding-for-engineers/14-computational-geometry-part2.html

### Related Libraries
- **LEAP71 ShapeKernel:** https://github.com/leap71/LEAP71_ShapeKernel
- **LEAP71 LatticeLibrary:** https://github.com/leap71/LEAP71_LatticeLibrary

### LEAP 71 Resources
- **LEAP 71 Website:** https://leap71.com/
- **LEAP 71 Gallery:** https://leap71.com/gallery/
- **Noyron CEM Platform:** https://leap71.com/noyron/

## Rocket Nozzle Theory

### Fundamentals
- **De Laval Nozzle:** https://en.wikipedia.org/wiki/De_Laval_nozzle
- **Rocket Propulsion Elements:** Sutton & Biblarz (textbook)
- **NASA Rocket Nozzles:** https://www.grc.nasa.gov/www/k-12/airplane/nozzle.html

### Key Equations

**Choked Flow (Throat Condition):**
```
A* / A = [2 / (γ + 1)]^((γ+1)/(2(γ-1))) * M * [1 + ((γ-1)/2) * M²]^(-(γ+1)/(2(γ-1)))
```

**Expansion Ratio:**
```
ε = Ae / At = [2 / (γ + 1)]^((γ+1)/(2(γ-1))) * (1 / Me) * [1 + ((γ-1)/2) * Me²]^((γ+1)/(2(γ-1)))
```

**Exit Velocity:**
```
Ve = √[2 * γ / (γ-1) * R * Tc / M * (1 - (Pe/Pc)^((γ-1)/γ))]
```

Where:
- γ = Ratio of specific heats (1.4 for air, ~1.2 for combustion gases)
- M = Mach number
- A* = Throat area
- Ae = Exit area
- R = Gas constant
- Tc = Chamber temperature
- Pc, Pe = Chamber and exit pressure

### Nozzle Contours

**Conical Nozzle:**
- Simple to manufacture
- 15° half-angle typical
- Some thrust loss due to divergent flow

**Bell (Parabolic) Nozzle:**
- Optimized for parallel exit flow
- 80% bell is common (80% of 15° cone length)
- Better specific impulse

**Rao Optimum Contour:**
- Method-of-characteristics design
- Maximum thrust for given length
- Complex manufacturing

### Typical Expansion Ratios

| Application | Expansion Ratio | Environment |
|-------------|-----------------|-------------|
| Sea-level booster | 10-20 | 1 atm ambient |
| Upper stage | 40-80 | Near-vacuum |
| Vacuum optimized | 100-300 | Space |
| RCS thrusters | 50-100 | Space |

## Voxel-Based Geometry

### Theory
- **OpenVDB:** https://www.openvdb.org/ (PicoGK's foundation)
- **Level Sets:** https://en.wikipedia.org/wiki/Level-set_method
- **Marching Cubes:** https://en.wikipedia.org/wiki/Marching_cubes

### Benefits for Manufacturing
- No manifold geometry issues
- Robust boolean operations
- Direct path to 3D printing slicers
- Handles complex internal structures

## Additive Manufacturing

### Metal 3D Printing
- **LPBF (Laser Powder Bed Fusion):** Used for aerospace components
- **EBM (Electron Beam Melting):** Alternative for reactive metals
- **DED (Directed Energy Deposition):** Large-scale parts

### Design for AM
- Minimum wall thickness considerations
- Support structure requirements
- Powder removal from internal channels
- Post-processing (HIP, heat treatment)

### Aerospace Applications
- Rocket engine thrust chambers
- Injector face plates
- Regenerative cooling channels
- Turbopump components

## Physical Constants

| Constant | Value | Unit |
|----------|-------|------|
| Universal Gas Constant | 8.314 | J/(mol·K) |
| Air γ (sea level) | 1.4 | - |
| Combustion γ (typical) | 1.2 | - |
| Standard Pressure | 101,325 | Pa |
| Standard Temperature | 288.15 | K |

## .NET and C# Resources

### Documentation
- **.NET Documentation:** https://docs.microsoft.com/en-us/dotnet/
- **C# Language Reference:** https://docs.microsoft.com/en-us/dotnet/csharp/
- **System.Numerics.Vector3:** https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector3

### Development Tools
- **Visual Studio Code:** https://code.visualstudio.com/
- **C# Dev Kit:** https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit

## File Formats

### STL (STereoLithography)
- Standard format for 3D printing
- Binary and ASCII variants
- Triangulated surface mesh

### OpenVDB
- Sparse volumetric data format
- Used by PicoGK internally
- Industry standard for VFX/simulation
