# Computational Nozzle Geometry - PicoGK

Rocket nozzle geometry generation using PicoGK, LEAP 71's voxel-based computational engineering kernel.

## Overview

This project demonstrates computational geometry for aerospace engineering by creating a parametric convergent-divergent (de Laval) rocket nozzle. The nozzle geometry is generated programmatically and exported to STL for 3D printing or CAD visualization.

## Why PicoGK?

PicoGK ("peacock") is used by LEAP 71 to design 3D-printed rocket engines:
- **Voxel-based**: Avoids numerical instabilities of traditional CAD
- **Robust booleans**: Add, subtract, intersect operations that always work
- **Manufacturing-ready**: Designed for additive manufacturing workflows
- **Open-source**: Apache 2.0 license

## Prerequisites

### 1. Install .NET SDK
Download and install .NET 9.0 SDK from [dotnet.microsoft.com](https://dotnet.microsoft.com/download)

### 2. Install Visual Studio Code (Optional)
For the best development experience, install VS Code with the C# Dev Kit extension.

### 3. Clone PicoGK
```bash
cd PicoGK
git clone https://github.com/leap71/PicoGK.git
```

Your folder structure should look like:
```
PicoGK/
├── NozzleShowcase.sln
├── README.md
├── NozzleShowcase/
│   ├── NozzleShowcase.csproj
│   ├── Program.cs
│   └── ...
├── PicoGK/                 ← Cloned from GitHub
│   ├── PicoGK.csproj
│   └── ...
└── Output/                 ← Generated STL files
```

### 4. Install PicoGK Runtime
Follow the PicoGK documentation at [picogk.org/doc/setup.html](https://picogk.org/doc/setup.html) to install the native runtime for your platform.

## Building and Running

```bash
cd PicoGK/NozzleShowcase

# Restore dependencies
dotnet restore

# Build
dotnet build

# Run (generates STL files in Output folder)
dotnet run
```

### Command Line Options

```bash
# Default: simple conical nozzle
dotnet run

# Higher resolution (smaller voxels)
dotnet run --voxel 0.25

# Bell contour nozzle (parabolic profile)
dotnet run --type bell

# Fast preview (larger voxels)
dotnet run --type bell --voxel 1.0

# Help
dotnet run --help
```

## Output Files

The program generates STL files in the `Output/` folder:
- `simple_nozzle.stl` - The hollow nozzle structure
- `nozzle_flow_path.stl` - The inner flow volume (for CFD or visualization)
- `bell_nozzle.stl` - Parabolic bell contour variant (if --type bell)

## Nozzle Types

### Simple Nozzle (Default)
Uses straight conical sections for convergent and divergent portions. Simple to understand and fast to generate.

### Bell Nozzle
Uses a parabolic bell contour in the divergent section, similar to real rocket engines. Provides better flow expansion characteristics.

## Project Structure

```
PicoGK/
├── NozzleShowcase.sln          # Visual Studio solution
├── README.md
├── NozzleShowcase/
│   ├── NozzleShowcase.csproj
│   ├── Program.cs              # Entry point
│   ├── Nozzles/
│   │   ├── NozzleParameters.cs # Parametric configuration
│   │   ├── SimpleNozzle.cs     # Conical nozzle
│   │   └── BellNozzle.cs       # Parabolic bell nozzle
│   └── Utils/
│       └── Constants.cs        # Physical constants
├── Output/                     # Generated STL files
└── references/
    └── REFERENCES.md
```

## Key PicoGK Concepts

### Voxels
Three-dimensional pixels at configurable resolution. Smaller voxel sizes give higher detail but slower computation.

```csharp
// Initialize PicoGK with 0.5mm voxel size
Library.Go(0.5f, SimpleNozzle.Task);
```

### Lattice
Collection of geometric primitives (beams, spheres) that define shapes:

```csharp
Lattice lat = new();

// Add a conical beam (cylinder with different end radii)
lat.AddBeam(
    new Vector3(0, 0, 0),    // Start point
    new Vector3(0, 0, 30),   // End point
    25f,                      // Start radius
    10f,                      // End radius
    false                     // No rounded caps
);

// Convert to voxels
Voxels vox = new(lat);
```

### Boolean Operations
Robust operations for combining geometry:

```csharp
// Create outer shell by offsetting
Voxels voxOuter = new(voxInner);
voxOuter.Offset(3f);  // Add 3mm wall thickness

// Subtract inner to make hollow
voxOuter.BoolSubtract(voxInner);
```

### Mesh Export
Convert voxels to triangle mesh for STL export:

```csharp
Mesh mesh = new(voxels);
mesh.SaveToStlFile("nozzle.stl");
```

## Nozzle Parameters

| Parameter | Default | Description |
|-----------|---------|-------------|
| Inlet Diameter | 50 mm | Combustion chamber interface |
| Throat Diameter | 20 mm | Minimum flow area (choke point) |
| Exit Diameter | 60 mm | Nozzle exit |
| Convergent Length | 30 mm | Inlet to throat |
| Divergent Length | 50 mm | Throat to exit |
| Wall Thickness | 3 mm | Structural wall |
| Voxel Size | 0.5 mm | Geometry resolution |

### Computed Values
- **Expansion Ratio**: 9.0 (exit area / throat area)
- **Contraction Ratio**: 6.25 (inlet area / throat area)
- **Total Length**: 80 mm

## Rocket Nozzle Theory

### Convergent-Divergent Design
The de Laval nozzle accelerates gas flow through two stages:
1. **Convergent section**: Flow accelerates subsonically
2. **Throat**: Flow reaches Mach 1 (sonic velocity)
3. **Divergent section**: Flow accelerates supersonically

### Expansion Ratio
The ratio of exit area to throat area determines the pressure ratio and exit velocity:
- **Low expansion** (~2-3): Sea-level engines
- **Medium expansion** (~10-15): First stage engines
- **High expansion** (>30): Vacuum-optimized engines

## Dependencies

- **PicoGK**: Geometry kernel (clone from GitHub)
- **.NET 9.0**: Runtime and SDK

## Troubleshooting

### "Could not load PicoGK runtime"
Ensure you've installed the PicoGK native runtime for your platform. See [picogk.org/doc/setup.html](https://picogk.org/doc/setup.html).

### "Project reference not found"
Make sure the PicoGK folder is in the correct location relative to NozzleShowcase. The .csproj expects it at `../PicoGK/PicoGK.csproj`.

### Slow generation
Increase voxel size for faster (but lower resolution) output:
```bash
dotnet run --voxel 1.0
```

## References

See [references/REFERENCES.md](references/REFERENCES.md) for technical resources.

- [PicoGK GitHub](https://github.com/leap71/PicoGK)
- [PicoGK Documentation](https://picogk.org/)
- [LEAP 71 Gallery](https://leap71.com/gallery/)
- [Rocket Nozzle Theory](https://en.wikipedia.org/wiki/De_Laval_nozzle)
