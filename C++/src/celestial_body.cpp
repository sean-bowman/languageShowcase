/*
 * celestial_body.cpp - Implementation of the CelestialBody class for planetary data
 *
 * ==============================================================================
 * AEROSPACE CONCEPT: Celestial Bodies and Gravitational Parameters
 * ==============================================================================
 *
 * In orbital mechanics, celestial bodies are defined primarily by their
 * GRAVITATIONAL PARAMETER (mu or GM), not their mass. This is a crucial
 * distinction that often surprises newcomers.
 *
 * WHY GM INSTEAD OF MASS?
 * -----------------------
 * The gravitational parameter mu = G * M combines:
 *   G = Universal gravitational constant = 6.674 x 10^-11 m^3/(kg*s^2)
 *   M = Mass of the body [kg]
 *
 * We use GM together because:
 *   1. PRECISION: GM is measured directly (from satellite tracking) with
 *      10+ significant figures. G alone is only known to ~4 figures!
 *   2. PRACTICALITY: Every orbital equation uses GM, never G or M separately
 *   3. HISTORY: Before satellites, we couldn't measure planetary masses well
 *
 * COMPARISON OF PRECISION:
 *   Earth's GM: 3.986004418 x 10^14 m^3/s^2  (10 significant figures)
 *   G constant: 6.674 x 10^-11 m^3/(kg*s^2)  (4 significant figures)
 *   Earth's M:  5.972 x 10^24 kg             (4 significant figures)
 *
 * If we calculated GM from G*M, we'd lose precision!
 *
 * GRAVITATIONAL PARAMETERS OF SOLAR SYSTEM BODIES:
 * ------------------------------------------------
 * | Body    | GM (m^3/s^2)        | Notes                          |
 * |---------|---------------------|--------------------------------|
 * | Sun     | 1.327 x 10^20       | Dominates solar system         |
 * | Earth   | 3.986 x 10^14       | Standard reference body        |
 * | Moon    | 4.905 x 10^12       | ~1/81 of Earth's GM            |
 * | Mars    | 4.283 x 10^13       | ~1/9 of Earth's GM             |
 * | Jupiter | 1.267 x 10^17       | ~318 x Earth's GM              |
 *
 * ==============================================================================
 * C++ CONCEPTS DEMONSTRATED IN THIS FILE
 * ==============================================================================
 *
 * 1. DEFAULT PARAMETERS
 *    std::optional<double> radius = std::nullopt
 *    - Provides default value if argument not supplied
 *    - Makes the radius parameter truly optional in the API
 *
 * 2. std::string HANDLING
 *    - const std::string& for input (efficient, no copy)
 *    - std::string member variable (owns the data)
 *    - Return by const reference from getter (no copy)
 *
 * 3. STATIC FACTORY METHODS FOR PRESET DATA
 *    - CelestialBody::Earth() returns a pre-configured object
 *    - Encapsulates physical constants
 *    - Makes code readable: Earth() vs CelestialBody("Earth", 3.986e14, ...)
 *
 * 4. CONSTEXPR CONSTANTS (in constants.hpp)
 *    - gm::earth, bodyRadius::earth, etc.
 *    - Compile-time constants for physical values
 *    - Organized in namespaces to avoid naming conflicts
 *
 * See also:
 *   constants.hpp for physical constant definitions
 *   orbit.hpp for using CelestialBody in orbital calculations
 */

#include "hohmann/celestial_body.hpp"
#include "hohmann/constants.hpp"
#include <cmath>    // std::sqrt, std::pow

namespace hohmann {

// =============================================================================
// CONSTRUCTOR
// =============================================================================

/**
 * Construct a celestial body with given properties.
 *
 * C++ CONCEPT: Default Parameter Values
 * -------------------------------------
 * The declaration has: std::optional<double> radius = std::nullopt
 *
 * This means you can call:
 *   CelestialBody("Sun", 1.327e20)          // radius defaults to nullopt
 *   CelestialBody("Earth", 3.986e14, 6.371e6)  // radius specified
 *
 * Default values are specified in the DECLARATION (header), not definition.
 * The compiler substitutes the default when the argument is omitted.
 *
 * C++ CONCEPT: std::string Parameter Handling
 * -------------------------------------------
 * We take `const std::string& name` (const reference) for efficiency:
 *   - No copy is made when passing the argument
 *   - `const` prevents accidental modification
 *
 * But we STORE `m_name` by VALUE (std::string, not reference):
 *   - The object owns its own copy of the string
 *   - Safe even if the original string is destroyed
 *
 * Parameters:
 *   name - Display name (e.g., "Earth", "Mars")
 *   gm - Gravitational parameter GM in m^3/s^2
 *   radius - Optional mean radius in meters (for altitude calculations)
 */
CelestialBody::CelestialBody(const std::string& name, double gm,
                             std::optional<double> radius)
    : m_name(name), m_gm(gm), m_radius(radius) {}

// =============================================================================
// ORBITAL MECHANICS CALCULATIONS
// =============================================================================

/**
 * Calculate the velocity needed for a circular orbit at given radius.
 *
 * AEROSPACE CONCEPT: Circular Orbit Velocity
 * ------------------------------------------
 * The velocity formula v = sqrt(GM/r) comes from balancing forces:
 *
 *   Gravitational force = Centripetal force
 *         GMm/r^2       =      mv^2/r
 *           GM/r        =       v^2
 *             v         =   sqrt(GM/r)
 *
 * Notice that the orbiting object's mass 'm' cancels out!
 * This is why all objects at the same orbital radius move at the same speed,
 * regardless of their mass (satellites, space stations, astronauts floating
 * outside - all move at exactly the same velocity).
 *
 * EXAMPLE VALUES:
 *   At 400 km above Earth (LEO): v = sqrt(3.986e14 / 6.771e6) = 7,672 m/s
 *   At 35,786 km above Earth (GEO): v = sqrt(3.986e14 / 42.157e6) = 3,075 m/s
 *
 * INTUITION: Higher orbits = SLOWER velocity (counterintuitive!)
 *
 * Parameters:
 *   orbital_radius - Distance from body CENTER in meters
 *
 * Returns:
 *   Circular orbital velocity in m/s
 */
double CelestialBody::circularVelocity(double orbital_radius) const {
    // v = sqrt(GM / r)
    // This is the simplified vis-viva equation for circular orbits (r = a)
    return std::sqrt(m_gm / orbital_radius);
}

/**
 * Calculate the escape velocity at a given distance from the body.
 *
 * AEROSPACE CONCEPT: Escape Velocity
 * -----------------------------------
 * Escape velocity is the minimum speed needed to completely escape a body's
 * gravitational influence (reach "infinity" with zero remaining velocity).
 *
 * DERIVATION (from energy conservation):
 *   At escape, total energy = 0 (just barely escapes)
 *   Kinetic + Potential = 0
 *   (1/2)mv^2 - GMm/r = 0
 *   v^2 = 2GM/r
 *   v_escape = sqrt(2GM/r)
 *
 * KEY RELATIONSHIP:
 *   v_escape = sqrt(2) * v_circular  (at any given radius)
 *   v_escape = 1.414... * v_circular
 *
 * To escape from Earth's surface (r = 6,371 km):
 *   v_escape = sqrt(2 * 3.986e14 / 6.371e6) = 11,186 m/s = 11.2 km/s
 *
 * This is why rockets need so much fuel - they must reach nearly escape
 * velocity just to get to orbit!
 *
 * TERMINOLOGY NOTE: "Escape velocity" is actually "escape SPEED" - direction
 * doesn't matter as long as you're not pointing straight down!
 *
 * Parameters:
 *   distance - Distance from body CENTER in meters
 *
 * Returns:
 *   Escape velocity in m/s
 */
double CelestialBody::escapeVelocity(double distance) const {
    // v_escape = sqrt(2 * GM / r)
    // The factor of 2 compared to circular velocity is NOT a coincidence:
    // it represents the extra kinetic energy needed to reach infinity.
    return std::sqrt(2.0 * m_gm / distance);
}

/**
 * Calculate the orbital period for a circular orbit.
 *
 * AEROSPACE CONCEPT: Kepler's Third Law
 * -------------------------------------
 * The orbital period T is related to the orbital radius by:
 *
 *   T = 2 * pi * sqrt(r^3 / GM)
 *
 * Or equivalently (Kepler's original formulation):
 *   T^2 / r^3 = constant (for orbits around the same body)
 *
 * DERIVATION:
 *   Circumference of orbit = 2 * pi * r
 *   Velocity = sqrt(GM/r)
 *   Time = Distance / Speed
 *   T = 2*pi*r / sqrt(GM/r) = 2*pi*r * sqrt(r/GM) = 2*pi*sqrt(r^3/GM)
 *
 * KEY INSIGHT: Period depends ONLY on semi-major axis (or radius for circular),
 * NOT on the orbiting object's mass. A feather and a truck at the same
 * altitude have identical orbital periods!
 *
 * EXAMPLE PERIODS AROUND EARTH:
 *   LEO (400 km):  T = 2*pi*sqrt((6.771e6)^3 / 3.986e14) = 5,553 s = 92.5 min
 *   GEO (35,786 km): T = 2*pi*sqrt((42.157e6)^3 / 3.986e14) = 86,164 s = 23h 56m
 *
 * The GEO period matches Earth's rotation (sidereal day), which is why
 * geostationary satellites appear to hover over one spot!
 *
 * Parameters:
 *   orbital_radius - Distance from body CENTER in meters
 *
 * Returns:
 *   Orbital period in seconds
 */
double CelestialBody::orbitalPeriod(double orbital_radius) const {
    // T = 2*pi * sqrt(r^3 / GM)
    // math::twoPi is defined in constants.hpp as a constexpr double
    return math::twoPi * std::sqrt(
        std::pow(orbital_radius, 3) / m_gm
    );
}

// =============================================================================
// STATIC FACTORY METHODS: Pre-defined Celestial Bodies
// =============================================================================
// These methods return CelestialBody objects with accurate physical data.
// Using factory methods instead of global constants:
//   - Ensures objects are properly constructed
//   - Allows lazy initialization (created when needed)
//   - Provides clear, readable API: CelestialBody::Earth()
//
// All values are from NASA/JPL reference data.
// =============================================================================

/**
 * Create the Sun celestial body.
 *
 * AEROSPACE: The Sun
 * ------------------
 * The Sun contains 99.86% of the solar system's mass. Its gravitational
 * parameter is used for all interplanetary trajectory calculations.
 *
 * Key data:
 *   GM:     1.327 x 10^20 m^3/s^2
 *   Radius: 6.957 x 10^8 m (696,700 km)
 *   Mass:   1.989 x 10^30 kg (but we don't use this directly!)
 *
 * The Sun's GM dominates trajectory calculations so much that planetary
 * gravity is often treated as a perturbation (the "patched conic" method).
 *
 * Returns:
 *   CelestialBody representing the Sun
 */
CelestialBody CelestialBody::Sun() {
    return CelestialBody("Sun", gm::sun, bodyRadius::sun);
}

/**
 * Create the Earth celestial body.
 *
 * AEROSPACE: Earth
 * ----------------
 * Earth is the reference body for most orbital mechanics education and
 * the starting point for most missions.
 *
 * Key data:
 *   GM:     3.986004418 x 10^14 m^3/s^2 (very precisely known!)
 *   Radius: 6.371 x 10^6 m (6,371 km mean radius)
 *   Note: Earth is slightly oblate (equatorial radius > polar radius)
 *
 * Earth's GM is known to 10 significant figures thanks to decades of
 * precise satellite tracking. This precision enables GPS accuracy!
 *
 * Returns:
 *   CelestialBody representing Earth
 */
CelestialBody CelestialBody::Earth() {
    return CelestialBody("Earth", gm::earth, bodyRadius::earth);
}

/**
 * Create the Moon celestial body.
 *
 * AEROSPACE: The Moon
 * -------------------
 * Earth's only natural satellite. Important for:
 *   - Lunar missions (Apollo, Artemis)
 *   - Gravity assists for deep space missions
 *   - Tidal calculations
 *
 * Key data:
 *   GM:     4.905 x 10^12 m^3/s^2 (~1/81 of Earth)
 *   Radius: 1.737 x 10^6 m (1,737 km)
 *
 * The Moon's GM is small enough that spacecraft can orbit it with
 * relatively low delta-v, but large enough to provide meaningful
 * gravity assists.
 *
 * Returns:
 *   CelestialBody representing the Moon
 */
CelestialBody CelestialBody::Moon() {
    return CelestialBody("Moon", gm::moon, bodyRadius::moon);
}

/**
 * Create the Mars celestial body.
 *
 * AEROSPACE: Mars
 * ---------------
 * The primary target for human exploration beyond the Moon. Mars missions
 * use Hohmann-like transfers from Earth, taking about 7-9 months.
 *
 * Key data:
 *   GM:     4.283 x 10^13 m^3/s^2 (~1/9 of Earth)
 *   Radius: 3.390 x 10^6 m (3,390 km)
 *
 * Mars has enough gravity to retain a thin atmosphere but not enough
 * to prevent significant loss over geological time. Its lower gravity
 * makes surface-to-orbit much easier than Earth (about 40% of Earth's
 * surface gravity).
 *
 * Returns:
 *   CelestialBody representing Mars
 */
CelestialBody CelestialBody::Mars() {
    return CelestialBody("Mars", gm::mars, bodyRadius::mars);
}

/**
 * Create the Jupiter celestial body.
 *
 * AEROSPACE: Jupiter
 * ------------------
 * The largest planet, often used for gravity assists to reach the outer
 * solar system (Voyager, Cassini, New Horizons missions).
 *
 * Key data:
 *   GM:     1.267 x 10^17 m^3/s^2 (~318 x Earth)
 *   Radius: ~69,911 km (no solid surface - we use cloud top altitude)
 *
 * Note: Jupiter's radius is set to nullopt here because for gas giants,
 * the "radius" is somewhat arbitrary (cloud top altitude varies).
 * For most orbital calculations around Jupiter, we care about GM only.
 *
 * Jupiter's massive gravity makes it excellent for gravity assists:
 *   - Can significantly speed up or slow down spacecraft
 *   - Can redirect trajectories with minimal fuel
 *   - Voyager 2 used Jupiter to reach Saturn, Uranus, and Neptune!
 *
 * Returns:
 *   CelestialBody representing Jupiter (no radius defined)
 */
CelestialBody CelestialBody::Jupiter() {
    // Note: std::nullopt for radius because gas giants don't have
    // a well-defined surface. The radius would only be needed for
    // altitude calculations, which don't apply here.
    return CelestialBody("Jupiter", gm::jupiter, std::nullopt);
}

} // namespace hohmann
