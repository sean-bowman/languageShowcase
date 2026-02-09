// =============================================================================
// ORBITAL MECHANICS CALCULATIONS
// =============================================================================
//
// AEROSPACE CONCEPT: Hohmann Transfer Orbits
// ===========================================
//
// A Hohmann transfer is the most fuel-efficient way to move between two
// circular orbits in the same plane. It uses an elliptical transfer orbit
// that is tangent to both the initial and final orbits.
//
// HOHMANN TRANSFER DIAGRAM:
// -------------------------
//
//                        * Apoapsis (far point)
//                       /|\
//                      / | \
//                     /  |  \  Final circular orbit
//        ___________/    |   \_____________________________
//       /                |      burn #2: circularize       \
//      /                 |        dv2 = v_circ - v_transfer \
//     |                  |                                   |
//     |        Earth     |                                   |
//     |          O       |  Transfer ellipse                 |
//     |                  |  (half orbit shown)               |
//      \                 |                                  /
//       \_______burn #1__|_________________________________/
//               dv1      |
//        Periapsis       |
//       (close point)    |
//                        |
//         Initial circular orbit
//
// KEY EQUATIONS:
// -------------
//
// 1. CIRCULAR VELOCITY:
//    v_circ = sqrt(mu / r)
//    - Speed needed to maintain circular orbit at radius r
//    - Higher orbits = slower speeds (counterintuitive but true!)
//
// 2. VIS-VIVA EQUATION:
//    v^2 = mu * (2/r - 1/a)
//    - Gives velocity at any point on ANY orbit
//    - r = current distance from center
//    - a = semi-major axis (average of periapsis and apoapsis)
//
// 3. TRANSFER ELLIPSE:
//    a_transfer = (r1 + r2) / 2
//    - Semi-major axis is average of the two orbit radii
//    - Periapsis tangent to inner orbit, apoapsis tangent to outer
//
// 4. TRANSFER TIME:
//    t = pi * sqrt(a^3 / mu)
//    - Half the period of the transfer ellipse
//    - From Kepler's Third Law: T^2 proportional to a^3
//
// =============================================================================
// JAVASCRIPT CONCEPTS IN THIS FILE
// =============================================================================
//
// 1. ES6 MODULES (import/export)
//    `import { GM, RADIUS } from './constants.js';`
//    `export function circularVelocity(...) { }`
//    Named imports/exports allow selective loading.
//
// 2. DEFAULT PARAMETERS
//    `function circularVelocity(altitude, body = 'EARTH')`
//    Parameters with defaults are optional when calling.
//
// 3. TEMPLATE LITERALS
//    `\`${seconds.toFixed(1)} s\``
//    Backticks allow embedded expressions via ${}.
//
// 4. ARROW FUNCTIONS (implicit in callbacks)
//    Short syntax: `(x) => x * 2` equals `function(x) { return x * 2; }`
//
// 5. OBJECT SHORTHAND
//    `return { r1, r2, aTransfer }` equals `return { r1: r1, r2: r2, ... }`
//    When key matches variable name, you can omit the colon.
//
// 6. MATH OBJECT
//    `Math.sqrt()`, `Math.pow()`, `Math.PI`, `Math.abs()`, `Math.cos()`
//    JavaScript's built-in math functions (no import needed).
//
// =============================================================================

import { GM, RADIUS } from './constants.js';

/**
 * Calculate circular orbital velocity.
 *
 * @description
 * AEROSPACE: Circular velocity is the speed needed to maintain a circular
 * orbit at a given altitude. Derived from balancing gravitational force
 * with centripetal acceleration:
 *
 *   F_gravity = F_centripetal
 *   G*M*m/r^2 = m*v^2/r
 *   v = sqrt(G*M/r) = sqrt(mu/r)
 *
 * INTERESTING FACT: Higher orbits have SLOWER velocities!
 *   LEO (400 km):  7.67 km/s
 *   GEO (35,786 km): 3.07 km/s
 *
 * @param {number} altitude - Altitude above Earth surface [km]
 * @param {string} [body='EARTH'] - Central body name
 * @returns {number} Orbital velocity [km/s]
 */
export function circularVelocity(altitude, body = 'EARTH') {
    // AEROSPACE: radius = body radius + altitude above surface
    const r = RADIUS[body] + altitude;

    // JAVASCRIPT: Object property access with computed key
    const mu = GM[body];

    // v = sqrt(mu/r) - the fundamental circular orbit equation
    return Math.sqrt(mu / r);
}

/**
 * Calculate orbital period using Kepler's Third Law.
 *
 * @description
 * AEROSPACE: Kepler's Third Law states that the square of the orbital
 * period is proportional to the cube of the semi-major axis:
 *
 *   T^2 = (4*pi^2/mu) * a^3
 *   T = 2*pi * sqrt(a^3 / mu)
 *
 * For circular orbits, a = r (semi-major axis equals radius).
 *
 * EXAMPLES:
 *   LEO (400 km):   T = 92.4 min = 1.54 hr
 *   GEO (35,786 km): T = 23.93 hr (nearly 24 hours!)
 *
 * @param {number} altitude - Altitude above Earth surface [km]
 * @param {string} [body='EARTH'] - Central body name
 * @returns {number} Orbital period [seconds]
 */
export function orbitalPeriod(altitude, body = 'EARTH') {
    const r = RADIUS[body] + altitude;
    const mu = GM[body];

    // For circular orbit, semi-major axis equals radius
    const a = r;

    // T = 2*pi*sqrt(a^3/mu) from Kepler's Third Law
    // Math.pow(a, 3) computes a^3 (a cubed)
    return 2 * Math.PI * Math.sqrt(Math.pow(a, 3) / mu);
}

/**
 * Calculate Hohmann transfer orbit parameters.
 *
 * @description
 * AEROSPACE: The Hohmann transfer is a two-impulse maneuver:
 *
 * BURN #1 (at periapsis):
 *   - Accelerate from circular velocity to transfer ellipse velocity
 *   - dv1 = v_transfer_periapsis - v_circular_initial
 *   - This raises apoapsis to the final orbit altitude
 *
 * COAST PHASE:
 *   - Spacecraft follows elliptical path (no fuel used)
 *   - Takes half the transfer orbit period
 *
 * BURN #2 (at apoapsis):
 *   - Accelerate from transfer ellipse velocity to circular velocity
 *   - dv2 = v_circular_final - v_transfer_apoapsis
 *   - This circularizes the orbit at the new altitude
 *
 * TOTAL DELTA-V:
 *   dv_total = |dv1| + |dv2|
 *   This determines fuel requirements via Tsiolkovsky equation.
 *
 * @param {number} altitudeInitial - Initial orbit altitude [km]
 * @param {number} altitudeFinal - Final orbit altitude [km]
 * @param {string} [body='EARTH'] - Central body name
 * @returns {Object} Transfer parameters object
 */
export function hohmannTransfer(altitudeInitial, altitudeFinal, body = 'EARTH') {
    // Calculate orbital radii (from planet CENTER, not surface)
    // AEROSPACE: Always work with radii, not altitudes, in orbital equations
    const r1 = RADIUS[body] + altitudeInitial;  // Initial radius [km]
    const r2 = RADIUS[body] + altitudeFinal;    // Final radius [km]
    const mu = GM[body];                         // Gravitational parameter [km^3/s^2]

    // =========================================================================
    // TRANSFER ELLIPSE GEOMETRY
    // =========================================================================
    // The transfer ellipse is tangent to both orbits:
    // - Periapsis (closest point) touches inner orbit at r1
    // - Apoapsis (farthest point) touches outer orbit at r2
    // Semi-major axis is the average of periapsis and apoapsis radii
    const aTransfer = (r1 + r2) / 2;

    // =========================================================================
    // VELOCITIES AT INITIAL ORBIT (periapsis of transfer)
    // =========================================================================
    // Circular velocity: v = sqrt(mu/r)
    const vCircular1 = Math.sqrt(mu / r1);

    // Transfer velocity at periapsis using vis-viva equation:
    // v^2 = mu * (2/r - 1/a)
    // At r1, with semi-major axis aTransfer
    const vTransfer1 = Math.sqrt(mu * (2 / r1 - 1 / aTransfer));

    // =========================================================================
    // VELOCITIES AT FINAL ORBIT (apoapsis of transfer)
    // =========================================================================
    // Circular velocity at final orbit
    const vCircular2 = Math.sqrt(mu / r2);

    // Transfer velocity at apoapsis
    // Note: This will be SLOWER than periapsis (conservation of angular momentum)
    const vTransfer2 = Math.sqrt(mu * (2 / r2 - 1 / aTransfer));

    // =========================================================================
    // DELTA-V CALCULATIONS
    // =========================================================================
    // First burn: accelerate from circular to transfer ellipse
    // AEROSPACE: We use abs() because dv is a magnitude (always positive)
    const deltaV1 = Math.abs(vTransfer1 - vCircular1);

    // Second burn: accelerate from transfer ellipse to circular
    // At apoapsis, transfer velocity < circular velocity, so we speed up
    const deltaV2 = Math.abs(vCircular2 - vTransfer2);

    // =========================================================================
    // TRANSFER TIME
    // =========================================================================
    // Time = half the period of the transfer ellipse
    // T/2 = pi * sqrt(a^3/mu)
    // AEROSPACE: This is derived from Kepler's Third Law
    const transferTime = Math.PI * Math.sqrt(Math.pow(aTransfer, 3) / mu);

    // =========================================================================
    // ECCENTRICITY OF TRANSFER ELLIPSE
    // =========================================================================
    // e = (r_apoapsis - r_periapsis) / (r_apoapsis + r_periapsis)
    // For transfer orbit: e = (r2 - r1) / (r2 + r1)
    // Note: This assumes r2 > r1 (transferring to higher orbit)
    const eccentricity = (r2 - r1) / (r2 + r1);

    // JAVASCRIPT: Object shorthand - { r1 } equals { r1: r1 }
    return {
        r1,                              // Initial radius [km]
        r2,                              // Final radius [km]
        aTransfer,                       // Transfer semi-major axis [km]
        eccentricity,                    // Transfer orbit eccentricity (0-1)
        vCircular1,                      // Initial circular velocity [km/s]
        vCircular2,                      // Final circular velocity [km/s]
        vTransfer1,                      // Velocity at transfer periapsis [km/s]
        vTransfer2,                      // Velocity at transfer apoapsis [km/s]
        deltaV1,                         // First burn magnitude [km/s]
        deltaV2,                         // Second burn magnitude [km/s]
        totalDeltaV: deltaV1 + deltaV2,  // Total delta-v budget [km/s]
        transferTime                     // Coast time [seconds]
    };
}

/**
 * Format time in human-readable units.
 *
 * @description
 * JAVASCRIPT: Template literals (backticks) allow embedded expressions.
 * The ${expression} syntax evaluates and inserts the result.
 *
 * Example: `${seconds.toFixed(1)} s` might produce "92.4 s"
 *
 * @param {number} seconds - Time in seconds
 * @returns {string} Formatted time string with appropriate units
 */
export function formatTime(seconds) {
    // Choose appropriate units based on magnitude
    if (seconds < 60) {
        // Less than a minute: show seconds
        return `${seconds.toFixed(1)} s`;
    } else if (seconds < 3600) {
        // Less than an hour: show minutes
        return `${(seconds / 60).toFixed(1)} min`;
    } else if (seconds < 86400) {
        // Less than a day: show hours
        return `${(seconds / 3600).toFixed(2)} hr`;
    } else {
        // Days (for long transfers like Earth-Mars)
        return `${(seconds / 86400).toFixed(2)} days`;
    }
}

/**
 * Format velocity with units.
 *
 * @param {number} velocity - Velocity in km/s
 * @returns {string} Formatted velocity string
 */
export function formatVelocity(velocity) {
    // JAVASCRIPT: toFixed(2) rounds to 2 decimal places and returns a string
    return `${velocity.toFixed(2)} km/s`;
}

/**
 * Format altitude with units.
 *
 * @description
 * JAVASCRIPT: toLocaleString() adds thousands separators based on locale.
 * Example: 35786 becomes "35,786" in en-US locale.
 *
 * @param {number} altitude - Altitude in km
 * @returns {string} Formatted altitude string
 */
export function formatAltitude(altitude) {
    if (altitude < 10000) {
        // Small numbers: no thousands separator needed
        return `${altitude.toFixed(0)} km`;
    } else {
        // Large numbers: add thousands separators for readability
        return `${altitude.toLocaleString()} km`;
    }
}

/**
 * Calculate position on an ellipse at a given true anomaly.
 *
 * @description
 * AEROSPACE: True anomaly (theta) is the angle from periapsis to the
 * current position, measured from the focus (central body).
 *
 * The orbit equation (polar form):
 *   r = a(1 - e^2) / (1 + e*cos(theta))
 *
 * Where:
 *   r = distance from focus (central body)
 *   a = semi-major axis
 *   e = eccentricity
 *   theta = true anomaly
 *
 * Converting to Cartesian:
 *   x = r * cos(theta)
 *   y = r * sin(theta)
 *
 * @param {number} a - Semi-major axis
 * @param {number} e - Eccentricity (0 = circle, 0-1 = ellipse)
 * @param {number} theta - True anomaly [radians]
 * @returns {Object} Position {x, y}
 */
export function ellipsePosition(a, e, theta) {
    // AEROSPACE: This is the conic section equation in polar form
    // p = a(1-e^2) is called the semi-latus rectum
    // r = p / (1 + e*cos(theta))
    const r = a * (1 - e * e) / (1 + e * Math.cos(theta));

    // Convert polar (r, theta) to Cartesian (x, y)
    return {
        x: r * Math.cos(theta),
        y: r * Math.sin(theta)
    };
}

/**
 * Generate an array of points along an elliptical arc.
 *
 * @description
 * AEROSPACE: This is used to draw orbital paths for visualization.
 * Points are evenly spaced in true anomaly (angle), not in arc length.
 *
 * JAVASCRIPT: Arrays are dynamic and created with `[]`.
 * The `push()` method appends elements to the end.
 *
 * @param {number} a - Semi-major axis
 * @param {number} e - Eccentricity
 * @param {number} thetaStart - Starting true anomaly [radians]
 * @param {number} thetaEnd - Ending true anomaly [radians]
 * @param {number} [segments=100] - Number of line segments
 * @returns {Array<{x: number, y: number}>} Array of position objects
 */
export function generateEllipsePoints(a, e, thetaStart, thetaEnd, segments = 100) {
    // JAVASCRIPT: Empty array literal
    const points = [];

    // Angular step size
    const dTheta = (thetaEnd - thetaStart) / segments;

    // Generate points along the arc
    // JAVASCRIPT: For loop with <= to include the endpoint
    for (let i = 0; i <= segments; i++) {
        const theta = thetaStart + i * dTheta;
        // JAVASCRIPT: push() adds element to end of array
        points.push(ellipsePosition(a, e, theta));
    }

    return points;
}
