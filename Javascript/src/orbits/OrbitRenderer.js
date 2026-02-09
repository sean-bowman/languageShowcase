// =============================================================================
// ORBIT RENDERER
// =============================================================================
//
// AEROSPACE CONCEPT: Orbital Path Visualization
// ================================================
//
// Orbits are paths objects follow under gravitational influence.
// This renderer visualizes two types:
//
// 1. CIRCULAR ORBITS (e = 0)
// --------------------------
// A circle centered on the central body.
//
//        * * *
//      *       *
//     *    O    *     O = central body (Earth)
//      *       *      r = constant radius
//        * * *
//
//   Parametric equation:
//     x = r * cos(theta)
//     y = 0  (orbital plane)
//     z = r * sin(theta)
//
//   Where theta goes from 0 to 2*PI.
//
// 2. ELLIPTICAL ORBITS (0 < e < 1)
// --------------------------------
// An ellipse with the central body at one focus.
//
//         Apoapsis
//            *
//          / | \
//         /  |  \      a = semi-major axis (half of longest diameter)
//        /   |   \     b = semi-minor axis (half of shortest diameter)
//       *----O----*    c = distance from center to focus
//        \   |   /     e = eccentricity = c/a
//         \  |  /
//          \ | /       For Hohmann transfer:
//            *         e = (r2 - r1) / (r2 + r1)
//        Periapsis
//
//   Parametric equation (from center, not focus):
//     x = a * cos(theta) - c     (c = offset to focus)
//     y = 0
//     z = b * sin(theta)
//
// VISUALIZATION CHALLENGE: LOGARITHMIC SCALING
// --------------------------------------------
// LEO is ~400 km, GEO is ~36,000 km (90x difference).
// Linear scaling makes LEO invisible or GEO enormous.
// Solution: Use logarithmic scaling for visual balance.
//
// =============================================================================
// JAVASCRIPT CONCEPTS IN THIS FILE
// =============================================================================
//
// 1. ES6 MAP
//    `this.orbits = new Map()`
//    Key-value collection with any type as key.
//    Methods: set(), get(), has(), delete(), forEach()
//
// 2. DESTRUCTURING ASSIGNMENT
//    `const { segments = 128, opacity = 0.8 } = options`
//    Extract properties with default values.
//
// 3. OBJECT METHODS
//    `geometry.dispose()`, `material.dispose()`
//    Explicit cleanup for WebGL resources.
//
// 4. MATH.LOG10
//    `Math.log10(x)` returns log base 10
//    Useful for scaling large value ranges.
//
// 5. CONDITIONAL (TERNARY) OPERATOR
//    `condition ? valueIfTrue : valueIfFalse`
//    Compact if-else expression.
//
// =============================================================================

import * as THREE from 'three';
import { SCALE, COLORS, RADIUS } from '../utils/constants.js';

/**
 * Renders circular and elliptical orbits in 3D.
 *
 * @description
 * This class manages orbit visualizations:
 * - Creates circular orbits at specified altitudes
 * - Creates elliptical transfer orbits between two altitudes
 * - Stores orbit data for position queries
 * - Handles cleanup when orbits are removed
 *
 * Orbits are stored by ID in a Map for easy lookup.
 */
export class OrbitRenderer {
    /**
     * Create orbit renderer.
     *
     * @param {THREE.Scene} scene - The Three.js scene to add orbits to
     */
    constructor(scene) {
        this.scene = scene;

        // Store orbits by ID for lookup
        // JAVASCRIPT: Map is a key-value collection
        // Unlike Object, Map preserves insertion order and allows any key type
        this.orbits = new Map();
    }

    /**
     * Convert real altitude to visualization radius.
     *
     * @description
     * VISUALIZATION PROBLEM:
     * Real orbital radii span huge ranges (6,771 km for LEO to 42,157 km for GEO).
     * Linear scaling makes close orbits indistinguishable.
     *
     * SOLUTION: Logarithmic scaling
     * - log10(1) = 0, log10(10) = 1, log10(100) = 2
     * - Compresses large differences into manageable range
     * - LEO and GEO become visually distinct
     *
     * FORMULA:
     * visual_radius = log10(actual_radius / earth_radius) * 20 + earth_visual_radius
     *
     * @param {number} altitude - Altitude above Earth surface [km]
     * @returns {number} Visual radius in Three.js units
     */
    altitudeToVisual(altitude) {
        // Calculate actual orbital radius (from Earth center)
        const actualRadius = RADIUS.EARTH + altitude;

        // Base visual radius (Earth's size in scene)
        const baseRadius = SCALE.EARTH_VISUAL;

        // Logarithmic scaling:
        // - log10(actualRadius/RADIUS.EARTH) is small for LEO, larger for GEO
        // - Multiply by 20 to spread out the visualization
        // - Add baseRadius to start from Earth's surface
        const scaleFactor = Math.log10(actualRadius / RADIUS.EARTH) * 20 + baseRadius;

        // Ensure minimum visible radius
        // JAVASCRIPT: Math.max returns the larger of two values
        return Math.max(scaleFactor, baseRadius + 1);
    }

    /**
     * Create a circular orbit visualization.
     *
     * @description
     * Circular orbits are simple: constant radius from center.
     * We generate points around a circle and connect them with lines.
     *
     * @param {string} id - Unique identifier for this orbit
     * @param {number} altitude - Altitude above Earth surface [km]
     * @param {number} color - Hex color (e.g., 0x4a90d9)
     * @param {Object} options - Rendering options
     * @returns {THREE.Line} The created line object
     */
    createCircularOrbit(id, altitude, color, options = {}) {
        // =====================================================================
        // DESTRUCTURING WITH DEFAULTS
        // =====================================================================
        // JAVASCRIPT: Destructuring extracts properties from an object.
        // Default values are used if property is undefined.
        const {
            segments = 128,    // Number of line segments (more = smoother)
            opacity = 0.8,     // Transparency (0 = invisible, 1 = solid)
            lineWidth = 2,     // (not actually used in WebGL - line width is limited)
            dashed = false     // Use dashed line style?
        } = options;

        // Remove existing orbit with same ID (if any)
        this.removeOrbit(id);

        // Convert altitude to visual radius
        const radius = this.altitudeToVisual(altitude);

        // =====================================================================
        // GENERATE CIRCLE VERTICES
        // =====================================================================
        const geometry = new THREE.BufferGeometry();
        const positions = [];

        // Generate points around circle
        for (let i = 0; i <= segments; i++) {
            // Angle from 0 to 2*PI (full circle)
            const theta = (i / segments) * Math.PI * 2;

            // Parametric circle in XZ plane (Y = 0)
            // MATH: x = r*cos(theta), z = r*sin(theta)
            positions.push(
                radius * Math.cos(theta),  // X
                0,                          // Y (orbital plane)
                radius * Math.sin(theta)   // Z
            );
        }

        geometry.setAttribute(
            'position',
            new THREE.Float32BufferAttribute(positions, 3)
        );

        // =====================================================================
        // CREATE MATERIAL
        // =====================================================================
        let material;

        if (dashed) {
            // THREE.JS: LineDashedMaterial creates dashed lines
            material = new THREE.LineDashedMaterial({
                color: color,
                transparent: true,
                opacity: opacity,
                dashSize: 0.5,   // Length of dashes
                gapSize: 0.3    // Length of gaps
            });
        } else {
            // Standard solid line
            material = new THREE.LineBasicMaterial({
                color: color,
                transparent: true,
                opacity: opacity
            });
        }

        // =====================================================================
        // CREATE LINE AND ADD TO SCENE
        // =====================================================================
        const line = new THREE.Line(geometry, material);

        // Dashed lines need line distances computed
        if (dashed) {
            line.computeLineDistances();
        }

        this.scene.add(line);

        // Store orbit data for later queries
        // JAVASCRIPT: Map.set(key, value) adds or updates an entry
        this.orbits.set(id, {
            line,              // The Three.js line object
            altitude,          // Original altitude [km]
            radius,            // Visual radius
            type: 'circular'   // Orbit type for position calculations
        });

        return line;
    }

    /**
     * Create a transfer (elliptical) orbit visualization.
     *
     * @description
     * AEROSPACE: A Hohmann transfer orbit is an ellipse with:
     * - Periapsis (closest point) at the initial orbit radius
     * - Apoapsis (farthest point) at the final orbit radius
     *
     * ELLIPSE GEOMETRY:
     * - a = semi-major axis = (r1 + r2) / 2
     * - c = focal distance = |r2 - r1| / 2
     * - b = semi-minor axis = sqrt(a^2 - c^2)
     * - Focus is offset from center by c
     *
     * @param {string} id - Unique identifier
     * @param {number} altitudeInitial - Initial orbit altitude [km]
     * @param {number} altitudeFinal - Final orbit altitude [km]
     * @param {number} color - Hex color
     * @param {Object} options - Rendering options
     * @returns {THREE.Line} The created line object
     */
    createTransferOrbit(id, altitudeInitial, altitudeFinal, color, options = {}) {
        const {
            segments = 128,
            opacity = 0.8,
            halfOrbit = true  // Only show the transfer half (not return path)
        } = options;

        // Remove existing orbit with same ID
        this.removeOrbit(id);

        // Convert altitudes to visual radii
        const r1 = this.altitudeToVisual(altitudeInitial);  // Periapsis radius
        const r2 = this.altitudeToVisual(altitudeFinal);    // Apoapsis radius

        // =====================================================================
        // ELLIPSE PARAMETERS
        // =====================================================================
        // MATH: For an ellipse with foci offset from center:
        // a = semi-major axis (half of longest diameter)
        // b = semi-minor axis (half of shortest diameter)
        // c = distance from center to focus
        // Relationship: a^2 = b^2 + c^2 (Pythagorean!)

        const a = (r1 + r2) / 2;                    // Semi-major axis
        const c = Math.abs(r2 - r1) / 2;            // Focal distance
        const b = Math.sqrt(a * a - c * c);         // Semi-minor axis

        // The center of the ellipse is offset from the focus (Earth)
        // If r1 < r2 (transferring outward), focus is to the left (-x)
        // JAVASCRIPT: Ternary operator for conditional value
        const centerOffset = r1 < r2 ? c : -c;

        // =====================================================================
        // GENERATE ELLIPSE VERTICES
        // =====================================================================
        const geometry = new THREE.BufferGeometry();
        const positions = [];

        // Angle range: full ellipse or just transfer half
        const startAngle = halfOrbit ? 0 : 0;
        const endAngle = halfOrbit ? Math.PI : Math.PI * 2;
        const angleRange = endAngle - startAngle;

        for (let i = 0; i <= segments; i++) {
            // Parametric angle
            const t = i / segments;
            const theta = startAngle + t * angleRange;

            // Parametric ellipse equation (centered at origin):
            // x = a * cos(theta)
            // z = b * sin(theta)
            // Then offset x by centerOffset to place focus at origin
            const x = a * Math.cos(theta) - centerOffset;
            const z = b * Math.sin(theta);

            positions.push(x, 0, z);
        }

        geometry.setAttribute(
            'position',
            new THREE.Float32BufferAttribute(positions, 3)
        );

        const material = new THREE.LineBasicMaterial({
            color: color,
            transparent: true,
            opacity: opacity
        });

        const line = new THREE.Line(geometry, material);
        this.scene.add(line);

        // Store orbit data including ellipse parameters
        this.orbits.set(id, {
            line,
            altitudeInitial,
            altitudeFinal,
            r1,
            r2,
            a,               // Semi-major axis (for position calculations)
            b,               // Semi-minor axis
            centerOffset,    // Offset from focus to center
            type: 'transfer'
        });

        return line;
    }

    /**
     * Remove an orbit by ID.
     *
     * @description
     * THREE.JS: Proper cleanup requires:
     * 1. Remove object from scene
     * 2. Dispose of geometry (frees GPU memory)
     * 3. Dispose of material (frees GPU memory)
     *
     * Failing to dispose causes memory leaks in WebGL applications.
     *
     * @param {string} id - Orbit identifier
     */
    removeOrbit(id) {
        // JAVASCRIPT: Map.has() checks if key exists
        if (this.orbits.has(id)) {
            const orbit = this.orbits.get(id);

            // Remove from Three.js scene
            this.scene.remove(orbit.line);

            // Dispose of GPU resources
            orbit.line.geometry.dispose();
            orbit.line.material.dispose();

            // Remove from our tracking Map
            // JAVASCRIPT: Map.delete() removes entry by key
            this.orbits.delete(id);
        }
    }

    /**
     * Remove all orbits.
     */
    clearAllOrbits() {
        // JAVASCRIPT: Map.forEach() iterates over all entries
        this.orbits.forEach((orbit, id) => {
            this.scene.remove(orbit.line);
            orbit.line.geometry.dispose();
            orbit.line.material.dispose();
        });

        // JAVASCRIPT: Map.clear() removes all entries
        this.orbits.clear();
    }

    /**
     * Get orbit data by ID.
     *
     * @param {string} id - Orbit identifier
     * @returns {Object|null} Orbit data or null if not found
     */
    getOrbit(id) {
        // JAVASCRIPT: || operator for default value
        return this.orbits.get(id) || null;
    }

    /**
     * Calculate position on orbit at given angle.
     *
     * @description
     * Returns the 3D position on the orbit for a given true anomaly (angle).
     * Used by Spacecraft to follow the orbital path.
     *
     * CIRCULAR ORBIT:
     *   position = (r * cos(angle), 0, r * sin(angle))
     *
     * ELLIPTICAL ORBIT:
     *   position = (a * cos(angle) - centerOffset, 0, b * sin(angle))
     *   Note: This uses parametric angle, not true anomaly.
     *   True anomaly would require solving Kepler's equation.
     *
     * @param {string} id - Orbit identifier
     * @param {number} angle - Angle in radians
     * @returns {THREE.Vector3} Position on orbit
     */
    getPositionOnOrbit(id, angle) {
        const orbit = this.orbits.get(id);

        // Return origin if orbit not found
        if (!orbit) return new THREE.Vector3();

        if (orbit.type === 'circular') {
            // Simple circle: x = r*cos, z = r*sin
            return new THREE.Vector3(
                orbit.radius * Math.cos(angle),
                0,
                orbit.radius * Math.sin(angle)
            );
        }
        else if (orbit.type === 'transfer') {
            // Ellipse (parametric form, offset to focus at origin)
            const x = orbit.a * Math.cos(angle) - orbit.centerOffset;
            const z = orbit.b * Math.sin(angle);
            return new THREE.Vector3(x, 0, z);
        }

        // Fallback
        return new THREE.Vector3();
    }
}
