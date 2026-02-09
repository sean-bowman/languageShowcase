// =============================================================================
// PHYSICAL AND ORBITAL CONSTANTS
// =============================================================================
//
// AEROSPACE CONCEPT: Gravitational Parameters and Standard Orbits
// ================================================================
//
// The gravitational parameter (mu = GM) is the product of the gravitational
// constant (G) and the mass of a celestial body (M). It's more accurate to
// use mu directly than G and M separately because mu can be measured more
// precisely through orbital observations.
//
// GRAVITATIONAL PARAMETER VALUES:
// ------------------------------
//   Body    |    mu (km^3/s^2)    |  Precision Source
//   --------|---------------------|--------------------
//   Earth   |    398,600.4418     |  GPS satellite orbits
//   Sun     | 132,712,440,018     |  Planetary ephemerides
//   Moon    |      4,902.8        |  Lunar laser ranging
//
// STANDARD EARTH ORBITS:
// ---------------------
//
//   Altitude (km)
//        |
//   400,000 + - - - - - - Moon orbit (384,400 km)
//        |
//    35,786 + - - - - - - GEO (Geostationary - 24hr period)
//        |               Communications satellites, weather satellites
//    20,200 + - - - - - - MEO (Medium Earth Orbit)
//        |               GPS constellation (~12hr period)
//       400 + - - - - - - LEO (Low Earth Orbit)
//        |               ISS, Hubble, most Earth observation
//         0 +--------------+ Earth surface
//
// =============================================================================
// JAVASCRIPT CONCEPTS IN THIS FILE
// =============================================================================
//
// 1. ES6 MODULES (export)
//    `export const GM = { ... }`
//    Makes constants available to other files via import.
//    This is the modern JavaScript module system.
//
// 2. OBJECT LITERALS
//    `{ EARTH: 398600.4418, SUN: 132712440018 }`
//    Objects group related values with named keys.
//    Access via dot notation: GM.EARTH or bracket: GM['EARTH']
//
// 3. CONST DECLARATION
//    `const GM = ...`
//    Creates a read-only reference. The object itself can be mutated,
//    but the variable cannot be reassigned.
//
// 4. HEXADECIMAL COLOR LITERALS
//    `0x4a90d9` is equivalent to the hex color #4a90d9
//    JavaScript supports hex, octal (0o), and binary (0b) number literals.
//
// =============================================================================

/**
 * Gravitational parameters (mu = GM) for celestial bodies.
 * Units: km^3/s^2
 *
 * @description
 * AEROSPACE: The gravitational parameter mu is fundamental to orbital mechanics.
 * It appears in every orbital equation:
 * - Circular velocity: v = sqrt(mu/r)
 * - Orbital period: T = 2*pi*sqrt(a^3/mu)
 * - Vis-viva: v^2 = mu*(2/r - 1/a)
 *
 * @type {Object.<string, number>}
 */
export const GM = {
    EARTH: 398600.4418,    // Determined from satellite tracking
    SUN: 132712440018,     // Determined from planetary ephemerides
    MOON: 4902.8           // Determined from lunar laser ranging
};

/**
 * Mean equatorial radii of celestial bodies.
 * Units: km
 *
 * @description
 * AEROSPACE: These are reference radii for altitude calculations.
 * Actual bodies are oblate spheroids, but mean radius is used for
 * first-order orbital calculations.
 *
 * IMPORTANT: Orbital altitude = distance from surface, not center!
 *   radius_orbital = RADIUS + altitude
 *
 * @type {Object.<string, number>}
 */
export const RADIUS = {
    EARTH: 6371,           // Mean radius (equatorial = 6378, polar = 6357)
    SUN: 696340,           // Photospheric radius
    MOON: 1737             // Mean radius
};

/**
 * Standard orbital altitudes above Earth's surface.
 * Units: km
 *
 * @description
 * AEROSPACE: These represent common operational orbits:
 *
 * LEO (Low Earth Orbit) - 400 km:
 *   - ISS operates at ~420 km
 *   - Period: ~92 minutes
 *   - Velocity: ~7.7 km/s
 *   - Atmospheric drag still significant (needs reboost)
 *
 * MEO (Medium Earth Orbit) - 20,200 km:
 *   - GPS satellites (31 operational)
 *   - Period: ~12 hours
 *   - 6 orbital planes, 55 deg inclination
 *
 * GEO (Geostationary) - 35,786 km:
 *   - Period: exactly 24 hours (sidereal)
 *   - Appears stationary from Earth
 *   - Communications, weather observation
 *   - Only possible in equatorial plane
 *
 * @type {Object.<string, number>}
 */
export const ORBITS = {
    LEO: 400,              // Low Earth Orbit (ISS altitude)
    MEO: 20200,            // Medium Earth Orbit (GPS)
    GEO: 35786,            // Geostationary Orbit
    MOON: 384400           // Lunar distance (semi-major axis)
};

/**
 * Visualization scale factors for Three.js rendering.
 *
 * @description
 * JAVASCRIPT/THREE.JS: 3D graphics use arbitrary "units".
 * We need to map real-world kilometers to Three.js units
 * so that orbits are visible and proportional.
 *
 * The challenge: LEO (400 km) and GEO (35,786 km) differ by 90x.
 * Linear scaling would make LEO invisible or GEO too large.
 * Solution: Use logarithmic scaling for orbit visualization.
 *
 * @type {Object.<string, number>}
 */
export const SCALE = {
    // Scale factor for distance conversion
    // 1 Three.js unit = 1000 km in real space
    DISTANCE: 1000,

    // Earth radius in visualization units
    // This sets the visual size of Earth in the scene
    EARTH_VISUAL: 6.371,

    // Minimum orbit radius (ensures visibility)
    MIN_ORBIT_VISUAL: 1.5
};

/**
 * Color scheme for orbital visualization.
 * Uses hexadecimal color codes (0xRRGGBB format).
 *
 * @description
 * JAVASCRIPT: Hex color literals start with 0x prefix.
 * THREE.JS: Colors can be specified as hex numbers, strings, or Color objects.
 *
 * Color scheme rationale:
 * - Blue for initial orbit (starting point)
 * - Green for final orbit (destination - "go" color)
 * - Orange for transfer (action, transition)
 * - Red for spacecraft (attention, tracking)
 *
 * @type {Object.<string, number>}
 */
export const COLORS = {
    INITIAL_ORBIT: 0x4a90d9,    // Blue - represents starting orbit
    FINAL_ORBIT: 0x4aff4a,      // Green - represents destination orbit
    TRANSFER_ORBIT: 0xff9040,   // Orange - the Hohmann ellipse
    SPACECRAFT: 0xff4040,       // Red - easy to track visually
    EARTH: 0x4080ff,            // Earth blue
    GRID: 0x1e3a5f              // Dark blue grid for depth reference
};
