# Language Showcase: Aerospace Engineering Tools

A collection of aerospace engineering tools implemented in six different programming languages, designed to build proficiency while creating practical utilities.

## Project Summary

| Language | Project | Description |
|----------|---------|-------------|
| **FORTRAN** | ISA Atmosphere Calculator | Compute atmospheric properties at any altitude |
| **Rust** | Type-Safe Unit Library | Prevent unit conversion errors at compile time |
| **C++** | Hohmann Transfer Calculator | Calculate orbital maneuver requirements |
| **C#** | Aerospace Dashboard (WPF) | Visual desktop application with charts |
| **JavaScript** | Three.js Orbital Visualization | Interactive 3D web visualization of orbits |
| **Python** | Manim Aerospace Animations | Mathematical animations for education |
| **C# (PicoGK)** | Computational Nozzle Geometry | Voxel-based 3D-printable rocket nozzle |

---

## FORTRAN: International Standard Atmosphere Calculator

**Directory:** [FORTRAN/](FORTRAN/)

### Purpose
Computes atmospheric properties based on the ISO 2533:1975 standard atmosphere model, used worldwide in aviation and aerospace.

### Why FORTRAN?
- NASA's workhorse for scientific computing since the 1950s
- Native array operations ideal for numerical work
- Still powers climate models, CFD simulations, and trajectory optimization

### Features
- Temperature, pressure, density, speed of sound calculations
- Supports altitudes from sea level to 86 km
- Handles all atmospheric layers (troposphere through mesosphere)
- Batch processing capability for multiple altitudes

### Key Formulas
```
Temperature:     T = T0 + L * h  (gradient layers)
Pressure:        P = P0 * (T/T0)^(-g/LR)
Density:         rho = P / (R * T)
Speed of Sound:  a = sqrt(gamma * R * T)
```

### Build & Run
```bash
cd FORTRAN
fpm build && fpm run
fpm run -- 10000  # Calculate at 10 km altitude
```

---

## Rust: Type-Safe Aerospace Unit Conversion Library

**Directory:** [Rust/](Rust/)

### Purpose
Prevents unit conversion errors (like the $327M Mars Climate Orbiter loss) through compile-time type safety. Incompatible unit operations won't compile.

### Why Rust?
- Memory safety without garbage collection
- Zero-cost abstractions - type safety with no runtime overhead
- Adopted for safety-critical aerospace systems (Ferrocene compiler qualified for ISO 26262)
- Used in satellite software (KubOS)

### Features
- Strongly-typed quantities: Length, Velocity, Mass, Force, Pressure, Angle
- Specific Impulse (Isp) calculations
- Operator overloading for natural syntax
- Compile-time prevention of unit mismatches

### Example
```rust
let thrust = Force::from_newtons(25000.0);
let mass_flow = MassFlowRate::from_kg_per_s(80.0);
let isp = thrust.specific_impulse(mass_flow);

// This won't compile - type mismatch!
// let bad = thrust + altitude;
```

### Build & Run
```bash
cd Rust
cargo build
cargo run --example rocket_equation
cargo run --example unit_demo
```

---

## C++: Hohmann Transfer Orbit Calculator

**Directory:** [C++/](C++/)

### Purpose
Calculates delta-v requirements and transfer times for Hohmann transfer orbits - the most fuel-efficient two-impulse maneuver between circular orbits.

### Why C++?
- Industry standard for flight dynamics simulation
- JSBSim (open-source FDM) is written in C++
- High performance for real-time applications
- Object-oriented design fits orbital mechanics modeling

### Features
- Calculates first and second burn delta-v
- Transfer time computation
- Phase angle for rendezvous
- Pre-defined celestial bodies (Earth, Moon, Mars, Sun)
- Common orbit presets (LEO, GEO, GPS)

### Key Equations
```
Vis-viva:    v^2 = mu * (2/r - 1/a)
Delta-v:     dv1 = sqrt(mu/r1) * (sqrt(2*r2/(r1+r2)) - 1)
Transfer:    t = pi * sqrt(a^3/mu)
```

### Build & Run
```bash
cd C++
mkdir build && cd build
cmake .. && cmake --build .
./hohmann 400 35786  # LEO to GEO transfer
./leo_to_geo         # Detailed example
./earth_mars         # Interplanetary example
```

---

## C#: Aerospace Mission Dashboard (WPF)

**Directory:** [C#/](C#/)

### Purpose
A Windows desktop application combining ISA calculations, Hohmann transfers, and unit conversion with visual charts and a modern dark UI.

### Why C#/WPF?
- Standard for aerospace ground control software
- ArduPilot's Mission Planner is written in C#
- Rich UI capabilities with XAML
- MVVM pattern for maintainable code

### Features
- ISA Calculator panel with real-time updates
- Hohmann Transfer Calculator for orbit planning
- Unit Converter for common aerospace units
- Temperature vs Altitude chart
- Pressure vs Altitude chart
- Aerospace-themed dark UI

### Architecture
```
MVVM Pattern:
├── Models/       -> Data structures (Orbit, CelestialBody)
├── ViewModels/   -> Presentation logic (MainViewModel)
├── Views/        -> XAML UI (MainWindow)
└── Services/     -> Calculations (IsaCalculator, HohmannCalculator)
```

### Build & Run
```bash
cd C#/AerospaceDashboard
dotnet restore
dotnet run

# Or open AerospaceDashboard.sln in Visual Studio and press F5
```

---

## JavaScript: Three.js Orbital Visualization

**Directory:** [Javascript/](Javascript/)

### Purpose
Interactive 3D web visualization of orbital mechanics, showing Earth, circular orbits, and Hohmann transfer animations directly in the browser.

### Why JavaScript/Three.js?
- WebGL-powered hardware-accelerated 3D graphics
- Cross-platform - works on any modern browser
- No installation required for end users
- Rich ecosystem with extensive examples
- Ideal for interactive educational tools

### Features
- 3D Earth model with atmospheric glow
- Circular orbit visualization (LEO, GEO)
- Hohmann transfer ellipse animation
- Animated spacecraft following transfer path
- Interactive camera controls (orbit, zoom, pan)
- Real-time delta-v calculation display

### Key Three.js Concepts
```javascript
// Scene setup
const scene = new THREE.Scene();
const camera = new THREE.PerspectiveCamera(60, aspect, 0.1, 10000);
const renderer = new THREE.WebGLRenderer({ antialias: true });

// Create geometry
const earth = new THREE.Mesh(
    new THREE.SphereGeometry(radius, 64, 64),
    new THREE.MeshPhongMaterial({ color: 0x4080ff })
);

// Animation loop
function animate() {
    requestAnimationFrame(animate);
    controls.update();
    renderer.render(scene, camera);
}
```

### Build & Run
```bash
cd Javascript
npm install
npm run dev      # Start development server
npm run build    # Production build
```

---

## Python: Manim Aerospace Animations

**Directory:** [Manim/](Manim/)

### Purpose
Create mathematical animations explaining aerospace concepts using Manim - the animation library created by Grant Sanderson (3Blue1Brown) for his educational YouTube videos.

### Why Python/Manim?
- Same tool used by 3Blue1Brown for millions of learners
- LaTeX integration for precise mathematical equations
- Programmatic animations that are reproducible and version-controlled
- Ideal for explaining complex aerospace derivations
- Smooth animations that build intuitive understanding

### Features
- ISA Atmosphere layer visualization and temperature profiles
- Hohmann transfer derivation with animated equations
- Rocket equation visualization with propellant depletion
- Orbital velocity and escape velocity derivations
- Orbital elements explanation

### Animation Scenes
| Scene | Description |
|-------|-------------|
| `ISALayersScene` | Visualize atmospheric layers |
| `ISAProfileScene` | Animated temperature vs altitude |
| `HohmannVisualizationScene` | Animated transfer maneuver |
| `HohmannDerivationScene` | Step-by-step equation derivation |
| `RocketEquationIntroScene` | Tsiolkovsky equation |
| `MassRatioExponentialScene` | Why rockets are hard |
| `OrbitalVelocityScene` | Circular velocity derivation |

### Example Scene
```python
from manim import *

class HohmannScene(Scene):
    def construct(self):
        # Create orbits
        inner = Circle(radius=1.5, color=BLUE)
        outer = Circle(radius=3.5, color=GREEN)

        # Animate
        self.play(Create(inner), Create(outer))

        # Show equation
        equation = MathTex(r"\Delta v_1 = \sqrt{\frac{\mu}{r_1}}")
        self.play(Write(equation))
```

### Build & Run
```bash
cd Manim
pip install -r requirements.txt

# Render animations
manim -pql scenes/hohmann_transfer.py HohmannVisualizationScene  # Low quality preview
manim -pqh scenes/hohmann_transfer.py HohmannVisualizationScene  # High quality
```

---

## C# (PicoGK): Computational Nozzle Geometry

**Directory:** [PicoGK/](PicoGK/)

### Purpose
Create 3D-printable rocket nozzle geometry using PicoGK's voxel-based computational engineering approach. Demonstrates how LEAP 71 builds aerospace components programmatically.

### Why PicoGK?

- Used by LEAP 71 to design 3D-printed rocket engines
- Voxel-based approach avoids CAD numerical instabilities
- Boolean operations (add, subtract, intersect) work robustly
- Designed for additive manufacturing workflows
- Open-source under Apache 2.0 license

### Features

- Convergent-divergent (de Laval) nozzle geometry
- Hollow structure with configurable wall thickness
- Parametric design (throat diameter, expansion ratio)
- Parabolic bell contour option for optimized flow
- STL export for 3D printing or visualization

### Key Concepts

```csharp
// Create flow path using lattice beams
Lattice latFlowPath = new();
latFlowPath.AddBeam(inlet, throat, inletRadius, throatRadius, false);
latFlowPath.AddBeam(throat, exit, throatRadius, exitRadius, false);

// Convert to voxels and create hollow structure
Voxels voxFlowPath = new(latFlowPath);
Voxels voxNozzle = new(voxFlowPath);
voxNozzle.Offset(wallThickness);      // Add wall
voxNozzle.BoolSubtract(voxFlowPath);  // Hollow out
```

### Build & Run
```bash
cd PicoGK/NozzleShowcase

# Requires PicoGK cloned alongside this project
dotnet restore
dotnet build
dotnet run                    # Default nozzle
dotnet run -- --type bell     # Parabolic bell contour
```

---

## Learning Path

### Recommended Order
1. **FORTRAN** - Simplest project, good numerical computing introduction
2. **Rust** - Learn type system concepts through practical application
3. **C++** - Object-oriented design with orbital mechanics
4. **C#** - Combines GUI with calculation services
5. **JavaScript** - Web-based 3D visualization
6. **Python** - Mathematical animations and education
7. **C# (PicoGK)** - Computational geometry for additive manufacturing

### What You'll Learn

| Language | Key Concepts |
|----------|-------------|
| FORTRAN | Modules, pure functions, array operations, fpm build system |
| Rust | Ownership, traits, operator overloading, cargo, zero-cost abstractions |
| C++ | Classes, CMake, header/source separation, std::optional, [[nodiscard]] |
| C# | MVVM, data binding, XAML, INotifyPropertyChanged, async patterns |
| JavaScript | ES6 modules, Three.js scene/camera/renderer, WebGL, animation loops |
| Python | Manim scenes, LaTeX equations, Mobjects, animation composition |
| C# (PicoGK) | Voxel geometry, Lattice beams, Boolean operations, STL export |

---

## References

Each project includes a `references/` directory with:
- Primary technical sources
- Formula derivations
- Validation data
- Language-specific documentation

### Key Sources
- [ISO 2533:1975 Standard Atmosphere](https://www.iso.org/standard/7472.html)
- [Hohmann Transfer - Orbital Mechanics](https://orbital-mechanics.space/orbital-maneuvers/hohmann-transfer.html)
- [Rust in Safety-Critical Space Systems](https://arxiv.org/html/2405.18135v1)
- [NASA FORTRAN for Trajectory Optimization](https://ntrs.nasa.gov/api/citations/20180000413/downloads/20180000413.pdf)
- [JSBSim Flight Dynamics Model](https://github.com/JSBSim-Team/jsbsim)
- [Three.js Documentation](https://threejs.org/docs/)
- [Manim Community Documentation](https://docs.manim.community/en/stable/)
- [PicoGK Documentation](https://picogk.org/)
- [LEAP 71 Gallery](https://leap71.com/gallery/)

---

## Prerequisites

### FORTRAN
- gfortran (via MinGW-w64 or MSYS2)
- fpm (Fortran Package Manager)

### Rust
- rustup from [rustup.rs](https://rustup.rs)

### C++
- Visual Studio 2022 with C++ workload, or
- MinGW-w64 + CMake

### C#
- Visual Studio 2022 with .NET desktop development workload
- .NET 8.0 SDK

### JavaScript
- Node.js 18+ from [nodejs.org](https://nodejs.org)
- npm (included with Node.js)
- Modern browser with WebGL support

### Python
- Python 3.8+ from [python.org](https://python.org)
- FFmpeg for video rendering
- LaTeX (optional, for equation rendering)

### C# (PicoGK)

- .NET 9.0 SDK from [dotnet.microsoft.com](https://dotnet.microsoft.com/download)
- PicoGK library (clone from [GitHub](https://github.com/leap71/PicoGK))
- Visual Studio Code with C# Dev Kit extension

---

## Project Status

| Project | Status | Tests | Documentation |
|---------|--------|-------|---------------|
| FORTRAN ISA | Complete | Yes | README + References |
| Rust Units | Complete | Yes | README + References |
| C++ Hohmann | Complete | Examples | README + References |
| C# Dashboard | Complete | N/A | README + References |
| JS Three.js | Complete | N/A | README + References |
| Python Manim | Complete | N/A | README + References |
| C# PicoGK | Complete | N/A | README + References |

---

*Created as a language proficiency exercise with practical aerospace engineering applications.*
