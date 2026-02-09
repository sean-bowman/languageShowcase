/*
 * orbit.cpp - Implementation of the Orbit class for circular orbital mechanics
 *
 * ==============================================================================
 * AEROSPACE CONCEPT: Circular Orbits
 * ==============================================================================
 *
 * A circular orbit is a special case where a spacecraft maintains constant
 * distance from the central body. The gravitational force exactly balances
 * the centripetal acceleration needed for circular motion.
 *
 * KEY PROPERTIES OF CIRCULAR ORBITS:
 *
 *   1. CONSTANT RADIUS: Unlike elliptical orbits, r never changes
 *   2. CONSTANT VELOCITY: v = sqrt(mu/r) at all points
 *   3. ORBITAL PERIOD: T = 2*pi*sqrt(r^3/mu) - how long one orbit takes
 *
 * ALTITUDE vs RADIUS:
 * -------------------
 *          ^
 *          |  <-- Altitude (h) = distance above surface
 *    ------*------  <-- Surface of planet
 *          |
 *          |  <-- Body radius (R)
 *          |
 *          O  <-- Center of planet
 *
 *   Orbital Radius = Body Radius + Altitude
 *          r       =      R      +    h
 *
 * For Earth:
 *   R_earth = 6,371 km
 *   ISS altitude h = 420 km
 *   ISS orbital radius r = 6,371 + 420 = 6,791 km
 *
 * COMMON EARTH ORBITAL REGIMES:
 * -----------------------------
 * | Name | Altitude | Period | Use Case |
 * |------|----------|--------|----------|
 * | LEO  | 200-2000 km | 90 min | Space stations, Earth observation |
 * | MEO  | 2000-35786 km | 2-24 hrs | GPS (~20,200 km, 12 hr period) |
 * | GEO  | 35,786 km | 24 hrs | Communications (appears stationary) |
 * | HEO  | Variable | Variable | Molniya orbits for high latitude coverage |
 *
 * ==============================================================================
 * C++ CONCEPTS DEMONSTRATED IN THIS FILE
 * ==============================================================================
 *
 * 1. STATIC FACTORY METHODS (fromAltitude, LEO, GEO, etc.)
 *    - Alternative to constructors for creating objects
 *    - Can have descriptive names (fromAltitude vs just Orbit)
 *    - Can return different types or perform validation differently
 *    - Called without an instance: Orbit::fromAltitude(...)
 *
 * 2. std::optional<T> (C++17)
 *    - Represents a value that may or may not exist
 *    - Safer than null pointers or magic sentinel values (-1, NaN)
 *    - Methods: has_value(), value(), value_or(default), operator*
 *    - std::nullopt represents "no value"
 *
 * 3. CONST REFERENCES AS PARAMETERS
 *    - `const CelestialBody& body` passes by reference (no copy)
 *    - `const` promises we won't modify the passed object
 *    - More efficient than pass-by-value for objects larger than ~16 bytes
 *
 * 4. MEMBER STORAGE BY VALUE vs REFERENCE
 *    - m_body is stored BY VALUE (a copy), not by reference
 *    - This means the Orbit owns its CelestialBody data
 *    - Alternative: store reference/pointer if sharing is desired
 *
 * See also:
 *   celestial_body.hpp for CelestialBody class
 *   constants.hpp for physical constants
 */

#include "hohmann/orbit.hpp"
#include "hohmann/constants.hpp"
#include <stdexcept>    // std::invalid_argument
#include <cmath>        // std::sqrt (though not directly used here)

namespace hohmann {

// =============================================================================
// CONSTRUCTORS
// =============================================================================

/**
 * Construct an orbit from orbital radius.
 *
 * AEROSPACE NOTE: Radius vs Altitude
 * -----------------------------------
 * Radius is measured from the CENTER of the planet, not the surface.
 * This is what orbital mechanics equations use internally.
 *
 * For most users, altitude (height above surface) is more intuitive.
 * Use fromAltitude() for that case instead.
 *
 * C++ CONCEPT: Constructor Validation
 * ------------------------------------
 * The constructor validates input immediately. This is called
 * "failing fast" - it's better to throw an exception here than
 * to have mysterious errors later when we try to calculate with
 * invalid data (like taking sqrt of negative number).
 *
 * Parameters:
 *   body - The celestial body being orbited (stored by copy)
 *   radius - Orbital radius from body CENTER in meters
 *
 * Throws:
 *   std::invalid_argument if radius is not positive
 */
Orbit::Orbit(const CelestialBody& body, double radius)
    : m_body(body), m_radius(radius) {
    // Validate: orbital radius must be positive
    // (In reality, it should also be greater than the body's radius,
    // but some theoretical calculations might use "underground" orbits)
    if (radius <= 0) {
        throw std::invalid_argument("Orbital radius must be positive");
    }
}

// =============================================================================
// STATIC FACTORY METHODS
// =============================================================================

/**
 * Create an orbit from altitude above the body's surface.
 *
 * C++ CONCEPT: Static Factory Methods
 * ------------------------------------
 * A "static factory method" is a static function that creates and returns
 * an object. Advantages over constructors:
 *
 *   1. NAMED: fromAltitude() is clearer than Orbit(body, ???)
 *   2. FLEXIBLE: Can perform complex validation before construction
 *   3. OPTIONABLE: Could return std::optional<Orbit> instead of throwing
 *
 * The `static` keyword means this belongs to the CLASS, not an instance.
 * Called as: Orbit::fromAltitude(earth, 400e3)
 * NOT as:    myOrbit.fromAltitude(...)
 *
 * C++ CONCEPT: std::optional Usage
 * ---------------------------------
 * body.radius() returns std::optional<double> because not all celestial
 * bodies have a defined radius (e.g., the Sun for heliocentric orbits
 * might not need a surface radius).
 *
 *   .has_value() - returns true if a value exists
 *   *optional    - dereferences to get the value (undefined behavior if empty!)
 *
 * Always check has_value() or use value_or(default) before dereferencing.
 *
 * Parameters:
 *   body - The celestial body (must have a defined radius)
 *   altitude - Height above surface in meters
 *
 * Returns:
 *   A new Orbit object
 *
 * Throws:
 *   std::invalid_argument if body has no radius
 */
Orbit Orbit::fromAltitude(const CelestialBody& body, double altitude) {
    // Get the body's radius - returns std::optional<double>
    auto radius = body.radius();

    // Check if the optional has a value
    if (!radius.has_value()) {
        throw std::invalid_argument(
            "Cannot create orbit from altitude: body has no defined radius"
        );
    }

    // Calculate orbital radius = body radius + altitude
    // *radius dereferences the optional to get the double value
    return Orbit(body, *radius + altitude);
}

// =============================================================================
// ORBITAL PROPERTY CALCULATIONS
// =============================================================================

/**
 * Calculate altitude above the body's surface.
 *
 * C++ CONCEPT: Returning std::optional
 * -------------------------------------
 * This method returns std::optional<double> because altitude only makes
 * sense if the body has a defined surface radius.
 *
 * For example, when calculating heliocentric orbits (around the Sun),
 * the Sun's "radius" isn't relevant - we care about the distance from
 * its center, not "altitude above the Sun's surface."
 *
 * Returning std::optional lets the caller decide how to handle the
 * "no altitude" case instead of forcing a crash or magic value.
 *
 * Usage by caller:
 *   if (auto alt = orbit.altitude()) {
 *       std::cout << "Altitude: " << *alt << " m\n";
 *   } else {
 *       std::cout << "Altitude not applicable\n";
 *   }
 *
 * Returns:
 *   Altitude in meters, or std::nullopt if body has no radius
 */
std::optional<double> Orbit::altitude() const {
    // Get the body's radius
    auto body_radius = m_body.radius();

    // If no radius defined, we can't calculate altitude
    if (!body_radius.has_value()) {
        return std::nullopt;  // C++ way of saying "no value"
    }

    // Altitude = orbital radius - body radius
    return m_radius - *body_radius;
}

/**
 * Calculate orbital velocity for this circular orbit.
 *
 * AEROSPACE CONCEPT: Circular Orbit Velocity
 * ------------------------------------------
 * For a circular orbit, velocity is constant and given by:
 *
 *   v = sqrt(mu / r)
 *
 * Where:
 *   v  = orbital velocity [m/s]
 *   mu = GM = gravitational parameter [m^3/s^2]
 *   r  = orbital radius [m]
 *
 * This comes from balancing gravitational force with centripetal force:
 *   F_gravity = F_centripetal
 *   GMm/r^2   = mv^2/r
 *   GM/r      = v^2
 *   v         = sqrt(GM/r)
 *
 * Example velocities:
 *   ISS (420 km):  ~7.66 km/s
 *   GEO (35,786 km): ~3.07 km/s
 *
 * Note: Higher orbits are SLOWER, not faster!
 *
 * Returns:
 *   Orbital velocity in m/s
 */
double Orbit::velocity() const {
    // Delegate to CelestialBody which has the formula
    return m_body.circularVelocity(m_radius);
}

/**
 * Calculate orbital period (time for one complete orbit).
 *
 * AEROSPACE CONCEPT: Orbital Period (Kepler's Third Law)
 * ------------------------------------------------------
 * The period T is how long it takes to complete one orbit:
 *
 *   T = 2 * pi * sqrt(r^3 / mu)
 *
 * This is Kepler's Third Law in its modern form. Key insight:
 * Period depends ONLY on semi-major axis (= radius for circular orbits),
 * not on the mass of the orbiting object!
 *
 * A feather and a bowling ball at the same altitude orbit in the same time.
 *
 * Example periods:
 *   ISS (420 km):     ~92 minutes
 *   GPS (20,200 km):  ~12 hours (2 orbits per day)
 *   GEO (35,786 km):  ~24 hours (stationary over one spot)
 *
 * Returns:
 *   Orbital period in seconds
 */
double Orbit::period() const {
    return m_body.orbitalPeriod(m_radius);
}

/**
 * Get orbital period in hours for easier human readability.
 *
 * C++ NOTE: Unit Conversion
 * -------------------------
 * This is a simple convenience method. The magic number 3600 is
 * seconds per hour. In production code, you might use a constants
 * file or a units library to avoid magic numbers.
 *
 * Returns:
 *   Orbital period in hours
 */
double Orbit::periodHours() const {
    return period() / 3600.0;  // 3600 seconds per hour
}

// =============================================================================
// PRESET ORBITS: Common Earth Orbital Regimes
// =============================================================================
// These static factory methods create orbits for well-known altitudes.
// They make the code more readable:
//   Orbit::LEO(earth)  vs  Orbit::fromAltitude(earth, 400e3)
//
// C++ CONCEPT: Named Constants via Factory Methods
// -------------------------------------------------
// Instead of #define LEO_ALTITUDE 400000, we use factory methods.
// Advantages:
//   - Type-safe (returns Orbit, not just a number)
//   - Encapsulates the altitude values
//   - Self-documenting at call site
// =============================================================================

/**
 * Create a Low Earth Orbit (LEO) at standard altitude.
 *
 * AEROSPACE: Low Earth Orbit (LEO)
 * --------------------------------
 * LEO is generally defined as 200-2000 km altitude.
 * We use 400 km as a standard reference, which is:
 *   - Near the ISS altitude (~420 km)
 *   - In the "sweet spot" that balances:
 *     - Low enough for frequent Earth observation
 *     - High enough to minimize atmospheric drag
 *
 * Objects in LEO travel at ~7.7 km/s and orbit in ~90 minutes.
 * They experience some atmospheric drag and will eventually decay
 * without periodic reboosting (ISS does this every few months).
 *
 * Parameters:
 *   earth - The Earth celestial body (must have radius defined)
 *
 * Returns:
 *   Orbit at 400 km altitude
 */
Orbit Orbit::LEO(const CelestialBody& earth) {
    return Orbit::fromAltitude(earth, 400e3);  // 400 km
}

/**
 * Create an orbit at ISS altitude.
 *
 * AEROSPACE: International Space Station Orbit
 * --------------------------------------------
 * The ISS orbits at approximately 420 km altitude. This altitude
 * is chosen to:
 *   - Minimize fuel needed for supply missions from Earth
 *   - Stay above most atmospheric drag (but still needs reboosting)
 *   - Remain below the Van Allen radiation belts
 *
 * The ISS is inclined at 51.6 degrees to the equator, allowing
 * launches from both US (Kennedy) and Russian (Baikonur) sites.
 *
 * Parameters:
 *   earth - The Earth celestial body
 *
 * Returns:
 *   Orbit at 420 km altitude
 */
Orbit Orbit::ISS(const CelestialBody& earth) {
    return Orbit::fromAltitude(earth, 420e3);  // 420 km
}

/**
 * Create a Geostationary Earth Orbit (GEO).
 *
 * AEROSPACE: Geostationary Orbit
 * ------------------------------
 * GEO is a VERY special orbit where the satellite appears to hover
 * motionless over a single point on Earth's equator.
 *
 * Requirements:
 *   1. Orbital period = 24 hours (sidereal day, actually 23h 56m 4s)
 *   2. Circular orbit (eccentricity = 0)
 *   3. Equatorial (inclination = 0)
 *
 * Solving T = 2*pi*sqrt(r^3/mu) for T = 86164 seconds gives:
 *   r = 42,164 km from Earth's center
 *   altitude = 42,164 - 6,371 = 35,786 km
 *
 * Applications:
 *   - Communications satellites (DirecTV, etc.)
 *   - Weather satellites (GOES series)
 *   - Early warning systems
 *
 * The "geostationary belt" is actually quite crowded with satellites!
 *
 * Parameters:
 *   earth - The Earth celestial body
 *
 * Returns:
 *   Orbit at 35,786 km altitude
 */
Orbit Orbit::GEO(const CelestialBody& earth) {
    return Orbit::fromAltitude(earth, 35786e3);  // 35,786 km
}

/**
 * Create a GPS constellation orbit.
 *
 * AEROSPACE: GPS Medium Earth Orbit
 * ---------------------------------
 * The GPS constellation consists of ~31 satellites at:
 *   - Altitude: 20,200 km (MEO - Medium Earth Orbit)
 *   - Period: ~12 hours (satellites pass overhead twice daily)
 *   - Inclination: 55 degrees (good coverage of populated areas)
 *   - 6 orbital planes with 4+ satellites each
 *
 * The 12-hour period is carefully chosen:
 *   - Satellites repeat ground tracks daily (predictable coverage)
 *   - Each satellite is visible for ~4-6 hours at a time
 *   - At least 4 satellites visible anywhere on Earth (for 3D position + time)
 *
 * Fun fact: GPS satellites are so precisely tracked that they're used
 * to verify Einstein's relativity! Their clocks run ~38 microseconds
 * fast per day due to gravitational time dilation being less at high
 * altitude (they're farther from Earth's gravity well).
 *
 * Parameters:
 *   earth - The Earth celestial body
 *
 * Returns:
 *   Orbit at 20,200 km altitude
 */
Orbit Orbit::GPS(const CelestialBody& earth) {
    return Orbit::fromAltitude(earth, 20200e3);  // 20,200 km
}

} // namespace hohmann
