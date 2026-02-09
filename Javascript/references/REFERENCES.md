# Orbital Visualization - References

## Three.js Resources

### Official Documentation
- **Three.js Docs:** https://threejs.org/docs/
- **Three.js Manual:** https://threejs.org/manual/#en/fundamentals
- **Three.js Examples:** https://threejs.org/examples/

### Key Concepts Used

#### Scene, Camera, Renderer
- https://threejs.org/docs/#api/en/scenes/Scene
- https://threejs.org/docs/#api/en/cameras/PerspectiveCamera
- https://threejs.org/docs/#api/en/renderers/WebGLRenderer

#### Geometries
- https://threejs.org/docs/#api/en/geometries/SphereGeometry
- https://threejs.org/docs/#api/en/geometries/ConeGeometry
- https://threejs.org/docs/#api/en/core/BufferGeometry

#### Materials
- https://threejs.org/docs/#api/en/materials/MeshPhongMaterial
- https://threejs.org/docs/#api/en/materials/LineBasicMaterial

#### Controls
- https://threejs.org/docs/#examples/en/controls/OrbitControls

### Tutorials
- **Three.js Journey:** https://threejs-journey.com/
- **Discover Three.js:** https://discoverthreejs.com/
- **Three.js Fundamentals:** https://threejsfundamentals.org/

## Orbital Mechanics

### Theory
- **Hohmann Transfer Orbit:** https://orbital-mechanics.space/orbital-maneuvers/hohmann-transfer.html
- **Vis-Viva Equation:** https://en.wikipedia.org/wiki/Vis-viva_equation
- **Orbital Mechanics - NASA:** https://www.grc.nasa.gov/www/k-12/airplane/orbit.html

### Reference Values
- **Earth Gravitational Parameter (μ):** 398,600.4418 km³/s²
- **Earth Radius:** 6,371 km
- **Standard Gravity (g₀):** 9.80665 m/s²

### Common Orbits
| Type | Altitude | Description |
|------|----------|-------------|
| LEO | 200-2000 km | Low Earth Orbit, ISS at 400 km |
| MEO | 2000-35786 km | Medium Earth Orbit, GPS at 20,200 km |
| GEO | 35,786 km | Geostationary, satellites appear stationary |
| HEO | Variable | Highly Elliptical Orbit, Molniya orbits |

### Equations Reference

**Circular Orbital Velocity:**
```
v = √(μ/r)
```

**Vis-Viva Equation:**
```
v² = μ(2/r - 1/a)
```

**Orbital Period:**
```
T = 2π√(a³/μ)
```

**Hohmann Transfer Delta-v:**
```
Δv₁ = √(μ/r₁) × (√(2r₂/(r₁+r₂)) - 1)
Δv₂ = √(μ/r₂) × (1 - √(2r₁/(r₁+r₂)))
```

**Transfer Time:**
```
t = π√(a_transfer³/μ)
```

## Vite Build Tool

- **Vite Documentation:** https://vitejs.dev/
- **Vite Configuration:** https://vitejs.dev/config/
- **ES Modules:** https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Modules

## WebGL Resources

- **WebGL Fundamentals:** https://webglfundamentals.org/
- **MDN WebGL Tutorial:** https://developer.mozilla.org/en-US/docs/Web/API/WebGL_API/Tutorial
- **GPU Performance:** https://webgl2fundamentals.org/webgl/lessons/webgl-performance.html

## Aerospace Visualization Examples

### Similar Projects
- **NASA Eyes:** https://eyes.nasa.gov/
- **Celestia:** https://celestia.space/
- **Orbiter Space Flight Simulator:** http://orbit.medphys.ucl.ac.uk/

### Web-Based Orbit Visualizers
- **Stuffin.space (Satellite Tracker):** https://stuffin.space/
- **Orbital Mechanics Calculator:** https://www.omnicalculator.com/physics/orbital-velocity

## Color Palette

The visualization uses an aerospace-inspired theme:

| Element | Color | Hex |
|---------|-------|-----|
| Background | Dark Blue | #0a0a1a |
| Initial Orbit | Blue | #4a90d9 |
| Final Orbit | Green | #4aff4a |
| Transfer Orbit | Orange | #ff9040 |
| Spacecraft | Red | #ff4040 |
| Earth | Ocean Blue | #4080ff |
| Grid | Dark Navy | #1e3a5f |
