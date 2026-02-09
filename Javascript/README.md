# Orbital Mechanics Visualization - Three.js

Interactive 3D visualization of Hohmann transfer orbits using Three.js and WebGL.

## Overview

This project demonstrates orbital mechanics concepts through an interactive web-based visualization:
- **3D Earth model** with atmospheric glow
- **Circular orbit visualization** for initial and final orbits
- **Hohmann transfer ellipse** showing the optimal transfer path
- **Animated spacecraft** following the transfer trajectory
- **Real-time orbital calculations** displaying delta-v requirements

## Why JavaScript/Three.js?

Three.js provides powerful 3D visualization capabilities in the browser:
- **WebGL-powered**: Hardware-accelerated graphics
- **Cross-platform**: Works on any modern browser
- **Interactive**: Real-time camera controls and animations
- **Accessible**: No installation required for end users
- **Rich ecosystem**: Extensive examples and community support

## Prerequisites

- **Node.js 18+** from [nodejs.org](https://nodejs.org)
- **npm** (included with Node.js)
- Modern browser with WebGL support (Chrome, Firefox, Edge, Safari)

## Getting Started

### Installation

```bash
cd Javascript

# Install dependencies
npm install

# Start development server
npm run dev
```

The application will open automatically in your browser at `http://localhost:5173`.

### Production Build

```bash
npm run build
npm run preview
```

## Features

### Visualization Controls
- **Mouse drag**: Rotate view around Earth
- **Scroll wheel**: Zoom in/out
- **Right-click drag**: Pan view

### Input Parameters
- **Initial Altitude**: Starting orbit altitude in km (e.g., 400 km for ISS)
- **Final Altitude**: Target orbit altitude in km (e.g., 35,786 km for GEO)
- **Animation Speed**: Control simulation playback speed

### Displayed Information
- Orbital velocities at each altitude
- Orbital periods
- Delta-v for each burn (Dv1, Dv2)
- Total delta-v requirement
- Transfer time

## Project Structure

```
Javascript/
├── package.json           # npm configuration
├── vite.config.js         # Build configuration
├── index.html             # Entry HTML with UI
├── README.md
├── src/
│   ├── main.js            # Application entry point
│   ├── scene/
│   │   └── SceneManager.js    # Three.js scene setup
│   ├── bodies/
│   │   ├── Earth.js           # Earth visualization
│   │   └── Spacecraft.js      # Animated spacecraft
│   ├── orbits/
│   │   └── OrbitRenderer.js   # Orbit visualization
│   └── utils/
│       ├── constants.js       # Physical constants
│       └── orbital-math.js    # Orbital calculations
└── references/
    └── REFERENCES.md
```

## Key Three.js Concepts Demonstrated

### 1. Scene Setup
```javascript
// Create scene, camera, and renderer
this.scene = new THREE.Scene();
this.camera = new THREE.PerspectiveCamera(60, aspect, 0.1, 10000);
this.renderer = new THREE.WebGLRenderer({ antialias: true });
```

### 2. Geometry Creation
```javascript
// Create Earth sphere
const geometry = new THREE.SphereGeometry(radius, 64, 64);
const material = new THREE.MeshPhongMaterial({ color: 0x4080ff });
const earth = new THREE.Mesh(geometry, material);
```

### 3. Orbit Path Drawing
```javascript
// Create orbit path using BufferGeometry
const geometry = new THREE.BufferGeometry();
geometry.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3));
const orbit = new THREE.Line(geometry, material);
```

### 4. Animation Loop
```javascript
animate() {
    requestAnimationFrame(() => this.animate());
    this.controls.update();
    this.renderer.render(this.scene, this.camera);
}
```

### 5. OrbitControls
```javascript
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';
this.controls = new OrbitControls(this.camera, this.renderer.domElement);
this.controls.enableDamping = true;
```

## Orbital Mechanics Background

### Hohmann Transfer
The most fuel-efficient two-impulse maneuver to transfer between circular coplanar orbits:

1. **First Burn (Dv1)**: At periapsis of initial orbit, burn prograde to enter transfer ellipse
2. **Coast**: Follow elliptical transfer orbit for half a period
3. **Second Burn (Dv2)**: At apoapsis, burn prograde to circularize into final orbit

### Key Equations

**Circular Velocity:**
```
v = sqrt(μ/r)
```

**Vis-Viva Equation:**
```
v² = μ(2/r - 1/a)
```

**Delta-v Calculations:**
```
Dv1 = sqrt(μ/r1) × (sqrt(2r2/(r1+r2)) - 1)
Dv2 = sqrt(μ/r2) × (1 - sqrt(2r1/(r1+r2)))
```

**Transfer Time:**
```
t = π × sqrt(a³/μ)
```

Where:
- μ = Earth's gravitational parameter (398,600.4 km³/s²)
- r = orbital radius
- a = semi-major axis

## Common Orbit Examples

| Orbit | Altitude | Velocity | Period |
|-------|----------|----------|--------|
| ISS (LEO) | 400 km | 7.67 km/s | 92.6 min |
| GPS (MEO) | 20,200 km | 3.87 km/s | 12 hr |
| GEO | 35,786 km | 3.07 km/s | 24 hr |

## Dependencies

- **three** (^0.160.0): 3D graphics library
- **vite** (^5.0.0): Build tool and development server

## References

See [references/REFERENCES.md](references/REFERENCES.md) for technical resources.

- [Three.js Documentation](https://threejs.org/docs/)
- [Orbital Mechanics - Wikipedia](https://en.wikipedia.org/wiki/Orbital_mechanics)
- [Hohmann Transfer Orbit](https://en.wikipedia.org/wiki/Hohmann_transfer_orbit)
