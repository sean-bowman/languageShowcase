// =============================================================================
// EARTH VISUALIZATION
// =============================================================================
//
// GRAPHICS CONCEPT: 3D Sphere with Material Properties
// =====================================================
//
// In 3D graphics, visible objects are "Meshes" - combinations of:
//
//   Mesh = Geometry + Material
//
// GEOMETRY defines the shape:
//   - SphereGeometry: sphere defined by radius and segment count
//   - More segments = smoother sphere (but more triangles to render)
//   - 64x64 segments is good balance of quality vs performance
//
// MATERIAL defines the appearance:
//   - Color: base color of the surface
//   - Emissive: self-illumination (glows without light source)
//   - Specular: highlight color from reflections
//   - Shininess: how focused the specular highlight is
//
// PHONG SHADING:
// --------------
// MeshPhongMaterial uses the Phong reflection model:
//
//   Final Color = Ambient + Diffuse + Specular
//
//   Ambient:  Constant base illumination (emissive in Three.js)
//   Diffuse:  Light scattered based on surface angle to light
//   Specular: Bright highlight where light reflects toward camera
//
// ATMOSPHERE EFFECT:
// -----------------
// The atmosphere glow is created using a slightly larger sphere
// rendered with BackSide (inside-out), creating a rim light effect.
//
// =============================================================================
// JAVASCRIPT CONCEPTS IN THIS FILE
// =============================================================================
//
// 1. CLASS METHODS
//    `create()`, `update()`, `addAtmosphere()`
//    Methods are functions attached to a class.
//
// 2. THIS KEYWORD
//    `this.mesh`, `this.scene`
//    Refers to the current instance of the class.
//
// 3. CHAINED METHOD CALLS
//    `this.mesh.rotation.y += ...`
//    Accessing nested properties.
//
// 4. METHOD ORGANIZATION
//    Breaking complex initialization into helper methods
//    (`create`, `addAtmosphere`, `addEquator`, `addAxis`).
//
// =============================================================================

import * as THREE from 'three';
import { SCALE, COLORS } from '../utils/constants.js';

/**
 * 3D Earth visualization component.
 *
 * @description
 * Creates a 3D representation of Earth at the center of the scene.
 * Includes:
 * - Main sphere with Phong shading
 * - Atmospheric glow effect
 * - Equator line for reference
 * - Rotation axis indicator
 *
 * The Earth slowly rotates for visual interest.
 */
export class Earth {
    /**
     * Create Earth visualization.
     *
     * @param {THREE.Scene} scene - The Three.js scene to add Earth to
     */
    constructor(scene) {
        // Store reference to the scene
        this.scene = scene;

        // These will be set in create()
        this.mesh = null;        // Main Earth mesh
        this.atmosphere = null;  // Atmospheric glow mesh

        // Rotation speed (radians per frame at 60fps)
        // AEROSPACE: Earth actually rotates once per 24 hours
        // This is much faster for visual effect
        this.rotationSpeed = 0.001;

        // Create the Earth visualization
        this.create();
    }

    /**
     * Create the main Earth mesh and add features.
     *
     * @description
     * THREE.JS: Mesh creation follows this pattern:
     * 1. Create Geometry (shape)
     * 2. Create Material (appearance)
     * 3. Combine into Mesh
     * 4. Add to scene
     */
    create() {
        // =====================================================================
        // GEOMETRY
        // =====================================================================
        // THREE.JS: SphereGeometry(radius, widthSegments, heightSegments)
        // More segments = smoother sphere
        // 64x64 is good for a hero object like Earth
        const geometry = new THREE.SphereGeometry(
            SCALE.EARTH_VISUAL,  // Radius (from constants)
            64,                   // Width segments (longitude)
            64                    // Height segments (latitude)
        );

        // =====================================================================
        // MATERIAL
        // =====================================================================
        // THREE.JS: MeshPhongMaterial supports lighting effects
        //
        // GRAPHICS: Phong shading calculates lighting per-pixel:
        // - color: base diffuse color
        // - emissive: self-illumination (adds to final color)
        // - specular: color of shiny highlights
        // - shininess: focus of highlights (higher = smaller, sharper)
        const material = new THREE.MeshPhongMaterial({
            color: 0x4080ff,      // Blue (ocean color)
            emissive: 0x112244,   // Slight blue glow
            specular: 0x333333,   // Gray highlights
            shininess: 25         // Medium shininess
        });

        // =====================================================================
        // MESH CREATION
        // =====================================================================
        // THREE.JS: Mesh combines geometry and material
        this.mesh = new THREE.Mesh(geometry, material);

        // Add to scene (makes it visible)
        this.scene.add(this.mesh);

        // =====================================================================
        // ADD FEATURES
        // =====================================================================
        // These helper methods add additional visual elements
        this.addAtmosphere();  // Glowing rim
        this.addEquator();     // Reference line
        this.addAxis();        // Rotation axis indicator
    }

    /**
     * Add atmospheric glow effect.
     *
     * @description
     * GRAPHICS TECHNIQUE: BackSide rendering for rim glow
     *
     * The atmosphere is a slightly larger sphere rendered "inside-out"
     * (BackSide). This creates a rim lighting effect because:
     *
     * 1. Camera is OUTSIDE the atmosphere sphere
     * 2. BackSide renders the inside faces
     * 3. We only see where the edge (rim) is visible
     * 4. Transparency makes it a subtle glow
     *
     * This is a common technique for halos, glows, and outlines.
     */
    addAtmosphere() {
        // Slightly larger sphere (102% of Earth radius)
        const atmosphereGeometry = new THREE.SphereGeometry(
            SCALE.EARTH_VISUAL * 1.02,  // 2% larger than Earth
            64,
            64
        );

        // THREE.JS: MeshBasicMaterial ignores lighting (always full brightness)
        // THREE.BackSide renders the inside of the sphere
        const atmosphereMaterial = new THREE.MeshBasicMaterial({
            color: 0x4a90d9,      // Light blue
            transparent: true,    // Enable transparency
            opacity: 0.15,        // Very transparent
            side: THREE.BackSide  // Render inside faces only
        });

        this.atmosphere = new THREE.Mesh(atmosphereGeometry, atmosphereMaterial);
        this.scene.add(this.atmosphere);
    }

    /**
     * Add equator line for reference.
     *
     * @description
     * AEROSPACE: The equator is important because:
     * - Geostationary orbits must be above the equator
     * - Launching from the equator gives "free" velocity from Earth rotation
     * - Orbital inclination is measured relative to the equator
     *
     * THREE.JS: Lines are created from BufferGeometry containing vertex positions.
     * Each vertex is 3 floats (x, y, z). For a line, vertices are connected in order.
     */
    addEquator() {
        const segments = 128;  // Number of line segments
        const geometry = new THREE.BufferGeometry();
        const positions = [];

        // Radius slightly above Earth surface to prevent z-fighting
        // Z-FIGHTING: When two surfaces are at the exact same depth,
        // the GPU can't decide which to render, causing flickering.
        const radius = SCALE.EARTH_VISUAL * 1.001;

        // Generate circle vertices in the XZ plane (Y = 0)
        for (let i = 0; i <= segments; i++) {
            const theta = (i / segments) * Math.PI * 2;
            positions.push(
                radius * Math.cos(theta),  // X
                0,                          // Y (equator is at Y=0)
                radius * Math.sin(theta)   // Z
            );
        }

        // THREE.JS: BufferAttribute stores vertex data
        geometry.setAttribute(
            'position',
            new THREE.Float32BufferAttribute(positions, 3)
        );

        // Semi-transparent line material
        const material = new THREE.LineBasicMaterial({
            color: 0x6ab0ff,
            transparent: true,
            opacity: 0.5
        });

        const equator = new THREE.Line(geometry, material);

        // THREE.JS: Adding to mesh (not scene) makes it a child
        // Child objects inherit parent's transformations (rotation, position)
        // So when Earth rotates, the equator rotates with it
        this.mesh.add(equator);
    }

    /**
     * Add rotation axis indicator.
     *
     * @description
     * AEROSPACE: Earth's rotation axis is tilted 23.4 degrees from orbital plane.
     * In this visualization, we show a vertical axis for simplicity.
     *
     * The axis indicator helps users understand Earth's orientation
     * and that it's rotating.
     */
    addAxis() {
        const axisGeometry = new THREE.BufferGeometry();
        const axisLength = SCALE.EARTH_VISUAL * 1.5;  // Extends beyond Earth

        // Simple line from -Y to +Y (vertical axis)
        axisGeometry.setAttribute('position', new THREE.Float32BufferAttribute([
            0, -axisLength, 0,  // Bottom point
            0, axisLength, 0    // Top point
        ], 3));

        const axisMaterial = new THREE.LineBasicMaterial({
            color: 0xffffff,     // White
            transparent: true,
            opacity: 0.3         // Very subtle
        });

        const axis = new THREE.Line(axisGeometry, axisMaterial);

        // Add as child of mesh (rotates with Earth)
        this.mesh.add(axis);
    }

    /**
     * Update Earth rotation.
     *
     * @description
     * Called each animation frame (~60fps).
     *
     * THREE.JS: rotation.y is the rotation around the Y axis (vertical).
     * Rotation is in radians. A full rotation = 2*PI radians.
     *
     * The `delta * 60` factor normalizes for frame rate:
     * - At 60fps, delta ~ 1/60, so delta * 60 ~ 1
     * - At 30fps, delta ~ 1/30, so delta * 60 ~ 2 (moves twice as much)
     * This keeps animation speed consistent regardless of frame rate.
     *
     * @param {number} delta - Time since last frame (seconds)
     */
    update(delta) {
        if (this.mesh) {
            // Rotate around Y axis (vertical)
            // JAVASCRIPT: += adds to current value
            this.mesh.rotation.y += this.rotationSpeed * delta * 60;
        }
    }

    /**
     * Set Earth rotation speed.
     *
     * @param {number} speed - Rotation speed (radians per normalized frame)
     */
    setRotationSpeed(speed) {
        this.rotationSpeed = speed;
    }
}
