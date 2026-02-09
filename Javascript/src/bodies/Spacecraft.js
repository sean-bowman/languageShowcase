// =============================================================================
// SPACECRAFT VISUALIZATION
// =============================================================================
//
// AEROSPACE CONCEPT: Orbital Animation and Phase Transitions
// ===========================================================
//
// This spacecraft animates through a complete Hohmann transfer sequence:
//
// ANIMATION PHASES:
// ----------------
//
//   Phase 1: INITIAL ORBIT
//   ----------------------
//   - Spacecraft circles in the initial (lower) orbit
//   - Completes one full orbit (2*PI radians)
//   - Represents the starting condition before transfer
//
//   Phase 2: TRANSFER ORBIT
//   -----------------------
//   - Spacecraft follows the transfer ellipse
//   - Only travels half the ellipse (PI radians)
//   - This is the Hohmann transfer trajectory
//   - Slower angular speed (conservation of angular momentum)
//
//   Phase 3: FINAL ORBIT
//   --------------------
//   - Spacecraft circles in the final (higher) orbit
//   - Completes one orbit, then restarts
//   - Represents the destination orbit after circularization
//
// ANGULAR MOMENTUM INSIGHT:
// ------------------------
// In real orbital mechanics, spacecraft move faster near periapsis
// (closest point) and slower near apoapsis (farthest point).
// This is Kepler's Second Law: equal areas in equal times.
//
// For visualization simplicity, we use constant angular speed
// (constant angle/time) which is not physically accurate but
// makes the animation easier to follow.
//
// =============================================================================
// JAVASCRIPT CONCEPTS IN THIS FILE
// =============================================================================
//
// 1. THREE.JS VECTOR3
//    `new THREE.Vector3()`, `vector.copy()`, `vector.clone()`
//    3D vectors for positions and directions.
//
// 2. TYPED ARRAYS
//    `new Float32Array(size)`
//    Fixed-size arrays for GPU-efficient data.
//
// 3. STRING STATES
//    `this.animationPhase = 'initial'`
//    Using strings as state machine states.
//
// 4. CONDITIONAL PHASE LOGIC
//    `if (this.animationPhase === 'initial' && ...)`
//    State machine transitions.
//
// 5. CLONE VS COPY
//    `clone()` creates new object, `copy()` modifies existing object.
//    Important distinction for memory management.
//
// =============================================================================

import * as THREE from 'three';
import { COLORS } from '../utils/constants.js';

/**
 * Animated spacecraft that follows orbital paths.
 *
 * @description
 * The spacecraft is visualized as a small cone pointing in the direction
 * of motion (prograde). It leaves a trail showing its recent path.
 *
 * The animation cycles through three phases:
 * 1. Initial orbit (one full revolution)
 * 2. Transfer orbit (half revolution)
 * 3. Final orbit (one full revolution)
 * Then repeats.
 */
export class Spacecraft {
    /**
     * Create spacecraft visualization.
     *
     * @param {THREE.Scene} scene - The Three.js scene to add spacecraft to
     */
    constructor(scene) {
        this.scene = scene;

        // 3D mesh representing the spacecraft
        this.mesh = null;

        // Trail line showing recent path
        this.trail = null;

        // Trail position history (array of Vector3)
        this.trailPositions = [];

        // Maximum number of trail points (older points are removed)
        this.maxTrailLength = 100;

        // =====================================================================
        // POSITION AND ANIMATION STATE
        // =====================================================================

        // Current position (THREE.Vector3 for 3D coordinates)
        this.position = new THREE.Vector3();

        // Current angle along orbit (radians from starting point)
        this.angle = 0;

        // Is animation currently running?
        this.isAnimating = false;

        // Current phase: 'initial', 'transfer', or 'final'
        // JAVASCRIPT: Strings as enum-like state values
        this.animationPhase = 'initial';

        // Speed multiplier (controlled by UI slider)
        this.animationSpeed = 1;

        // =====================================================================
        // ORBIT REFERENCE
        // =====================================================================

        // Reference to OrbitRenderer for getting positions
        this.orbitRenderer = null;

        // ID of current orbit being followed
        this.currentOrbitId = null;

        // Create the spacecraft mesh and trail
        this.create();
    }

    /**
     * Create spacecraft mesh and trail.
     *
     * @description
     * The spacecraft is a cone (ConeGeometry) rotated to point forward.
     * A point light is attached to make it glow.
     *
     * THREE.JS: ConeGeometry creates a cone shape.
     * Parameters: (radius, height, radialSegments)
     */
    create() {
        // =====================================================================
        // SPACECRAFT MESH
        // =====================================================================

        // Create cone geometry (like a rocket or arrow)
        const geometry = new THREE.ConeGeometry(
            0.3,  // Radius at base
            1,    // Height
            8     // Number of sides (octagonal)
        );

        // Rotate geometry so cone points along +Z axis (forward)
        // THREE.JS: rotateX/Y/Z modify the geometry itself (permanent)
        geometry.rotateX(Math.PI / 2);

        // Phong material for lighting response
        const material = new THREE.MeshPhongMaterial({
            color: COLORS.SPACECRAFT,  // Red
            emissive: 0x441111,        // Slight red glow
            specular: 0x666666,        // Gray highlights
            shininess: 30
        });

        this.mesh = new THREE.Mesh(geometry, material);
        this.scene.add(this.mesh);

        // Add a point light to the spacecraft (makes it glow)
        // THREE.JS: PointLight(color, intensity, distance)
        const light = new THREE.PointLight(COLORS.SPACECRAFT, 0.5, 10);
        // Adding light as child of mesh - moves with spacecraft
        this.mesh.add(light);

        // Create the trail visualization
        this.createTrail();
    }

    /**
     * Create trail line geometry.
     *
     * @description
     * The trail uses a BufferGeometry with pre-allocated vertex buffer.
     * As the spacecraft moves, we update the vertex positions.
     *
     * THREE.JS: setDrawRange(start, count) controls how many vertices
     * are actually drawn. This is more efficient than recreating geometry.
     */
    createTrail() {
        const geometry = new THREE.BufferGeometry();

        // Pre-allocate vertex buffer (3 floats per vertex: x, y, z)
        // JAVASCRIPT: Float32Array is a typed array (fixed size, 32-bit floats)
        const positions = new Float32Array(this.maxTrailLength * 3);

        geometry.setAttribute(
            'position',
            new THREE.BufferAttribute(positions, 3)
        );

        // Initially draw nothing (0 vertices)
        geometry.setDrawRange(0, 0);

        const material = new THREE.LineBasicMaterial({
            color: COLORS.SPACECRAFT,
            transparent: true,
            opacity: 0.5
        });

        this.trail = new THREE.Line(geometry, material);
        this.scene.add(this.trail);
    }

    /**
     * Set reference to orbit renderer.
     *
     * @param {OrbitRenderer} orbitRenderer - The orbit renderer to use
     */
    setOrbitRenderer(orbitRenderer) {
        this.orbitRenderer = orbitRenderer;
    }

    /**
     * Set spacecraft position and orient along orbit tangent.
     *
     * @description
     * AEROSPACE: In real orbital mechanics, spacecraft naturally orient
     * themselves due to gravity gradient torques. Here we manually
     * orient the spacecraft to point along the velocity vector (prograde).
     *
     * MATH: The tangent to a circle at point (x, 0, z) is (-z, 0, x).
     * This is perpendicular to the radius vector.
     *
     * @param {THREE.Vector3} position - New position
     */
    setPosition(position) {
        // Update internal position
        // THREE.JS: copy() modifies this vector to match the argument
        this.position.copy(position);

        // Move mesh to new position
        this.mesh.position.copy(position);

        // =====================================================================
        // ORIENT SPACECRAFT ALONG ORBIT (PROGRADE)
        // =====================================================================
        // Calculate tangent vector (perpendicular to radius in XZ plane)
        // For point (x, y, z), tangent in counter-clockwise direction is (-z, y, x)
        const tangent = new THREE.Vector3(
            -position.z,  // Perpendicular X
            0,            // Stay in orbital plane
            position.x    // Perpendicular Z
        ).normalize();    // Make unit length

        // THREE.JS: lookAt() orients the object to face a point
        // We look at a point ahead on our path
        this.mesh.lookAt(position.clone().add(tangent));

        // Update trail
        this.updateTrail();
    }

    /**
     * Update trail with current position.
     *
     * @description
     * JAVASCRIPT: shift() removes first element of array.
     * push() adds to end. Together they create a sliding window.
     *
     * THREE.JS: After modifying BufferAttribute data, set needsUpdate = true
     * to tell Three.js to upload new data to GPU.
     */
    updateTrail() {
        // Only update trail when animating
        if (!this.isAnimating) return;

        // Add current position to trail history
        // THREE.JS: clone() creates a copy (otherwise we'd store references)
        this.trailPositions.push(this.position.clone());

        // Remove oldest point if trail is too long
        if (this.trailPositions.length > this.maxTrailLength) {
            // JAVASCRIPT: shift() removes and returns first element
            this.trailPositions.shift();
        }

        // Update geometry with new positions
        const positions = this.trail.geometry.attributes.position.array;

        for (let i = 0; i < this.trailPositions.length; i++) {
            // Each position is 3 floats (x, y, z)
            positions[i * 3] = this.trailPositions[i].x;
            positions[i * 3 + 1] = this.trailPositions[i].y;
            positions[i * 3 + 2] = this.trailPositions[i].z;
        }

        // Tell Three.js to update GPU data
        this.trail.geometry.attributes.position.needsUpdate = true;

        // Only draw the vertices we're using
        this.trail.geometry.setDrawRange(0, this.trailPositions.length);
    }

    /**
     * Clear the trail.
     */
    clearTrail() {
        // JAVASCRIPT: Empty array by setting length to 0 or reassigning
        this.trailPositions = [];
        this.trail.geometry.setDrawRange(0, 0);
    }

    /**
     * Start animation on specified orbit.
     *
     * @param {string} orbitId - ID of orbit to follow ('initial', 'transfer', 'final')
     * @param {number} [startAngle=0] - Starting angle in radians
     */
    startAnimation(orbitId, startAngle = 0) {
        this.currentOrbitId = orbitId;
        this.angle = startAngle;
        this.isAnimating = true;
        this.clearTrail();
    }

    /**
     * Stop animation.
     */
    stopAnimation() {
        this.isAnimating = false;
    }

    /**
     * Update animation - called each frame.
     *
     * @description
     * STATE MACHINE: The animation progresses through phases:
     *
     *   'initial' --[angle >= 2*PI]--> 'transfer'
     *   'transfer' --[angle >= PI]--> 'final'
     *   'final' --[angle >= 3*PI]--> 'initial' (restart)
     *
     * AEROSPACE: In a real Hohmann transfer:
     * - Initial orbit: spacecraft completes orbit while waiting for window
     * - Transfer: spacecraft coasts along ellipse (half orbit)
     * - Final orbit: spacecraft circularized at destination
     *
     * @param {number} delta - Time since last frame
     * @param {Object} transferParams - Transfer orbit parameters (unused here)
     */
    update(delta, transferParams = null) {
        if (!this.isAnimating || !this.orbitRenderer) return;

        // =====================================================================
        // ANGULAR SPEED
        // =====================================================================
        // Base angular speed (radians per normalized frame)
        let angularSpeed = 0.02 * this.animationSpeed;

        // Slow down during transfer for visual clarity
        // AEROSPACE: In reality, transfer orbit speed varies (Kepler's 2nd Law)
        if (this.animationPhase === 'transfer') {
            angularSpeed *= 0.5;
        }

        // Update angle (normalize by frame rate)
        this.angle += angularSpeed * delta * 60;

        // =====================================================================
        // PHASE TRANSITIONS (STATE MACHINE)
        // =====================================================================

        if (this.animationPhase === 'initial' && this.angle >= Math.PI * 2) {
            // Completed initial orbit -> start transfer
            // AEROSPACE: This is where burn #1 would occur
            this.animationPhase = 'transfer';
            this.currentOrbitId = 'transfer';
            this.angle = 0;  // Reset angle for transfer
        }
        else if (this.animationPhase === 'transfer' && this.angle >= Math.PI) {
            // Completed transfer (half orbit) -> enter final orbit
            // AEROSPACE: This is where burn #2 would occur
            this.animationPhase = 'final';
            this.currentOrbitId = 'final';
            this.angle = Math.PI;  // Continue from opposite side
        }
        else if (this.animationPhase === 'final' && this.angle >= Math.PI * 3) {
            // Completed final orbit -> restart animation
            this.animationPhase = 'initial';
            this.currentOrbitId = 'initial';
            this.angle = 0;
            this.clearTrail();  // Clear trail for new cycle
        }

        // =====================================================================
        // POSITION UPDATE
        // =====================================================================
        // Get position on current orbit at current angle
        const position = this.orbitRenderer.getPositionOnOrbit(
            this.currentOrbitId,
            this.angle
        );

        // Update spacecraft position
        this.setPosition(position);
    }

    /**
     * Reset animation to initial state.
     */
    reset() {
        this.stopAnimation();
        this.animationPhase = 'initial';
        this.currentOrbitId = 'initial';
        this.angle = 0;
        this.clearTrail();

        // Position at start of initial orbit
        if (this.orbitRenderer) {
            const position = this.orbitRenderer.getPositionOnOrbit('initial', 0);
            this.setPosition(position);
        }
    }

    /**
     * Set animation speed multiplier.
     *
     * @param {number} speed - Speed multiplier (1 = normal)
     */
    setAnimationSpeed(speed) {
        this.animationSpeed = speed;
    }
}
