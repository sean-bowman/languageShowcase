#ifndef HOHMANN_ORBIT_HPP
#define HOHMANN_ORBIT_HPP

/*
 * orbit.hpp - Represents an orbit around a celestial body
 */

#include "celestial_body.hpp"

namespace hohmann {

/*
 * Orbit class - Represents a circular orbit around a celestial body
 *
 * For Hohmann transfers, we work with circular orbits. The orbit is
 * defined by its radius (distance from body center) and the parent body.
 */
class Orbit {
public:
    /*
     * Construct an orbit from radius
     *
     * Parameters:
     *   body - The celestial body being orbited
     *   radius - Orbital radius from body center in meters
     */
    Orbit(const CelestialBody& body, double radius);

    /*
     * Construct an orbit from altitude above surface
     *
     * Parameters:
     *   body - The celestial body being orbited (must have radius)
     *   altitude - Altitude above surface in meters
     *
     * Returns:
     *   Orbit object
     *
     * Throws:
     *   std::invalid_argument if body has no defined radius
     */
    static Orbit fromAltitude(const CelestialBody& body, double altitude);

    // Accessors
    [[nodiscard]] const CelestialBody& body() const { return m_body; }
    [[nodiscard]] double radius() const { return m_radius; }

    /*
     * Get altitude above body surface (if body has defined radius)
     *
     * Returns:
     *   Altitude in meters, or nullopt if body has no radius
     */
    [[nodiscard]] std::optional<double> altitude() const;

    /*
     * Calculate orbital velocity
     *
     * Returns:
     *   Velocity in m/s
     */
    [[nodiscard]] double velocity() const;

    /*
     * Calculate orbital period
     *
     * Returns:
     *   Period in seconds
     */
    [[nodiscard]] double period() const;

    /*
     * Calculate orbital period in human-readable units
     *
     * Returns:
     *   Period as hours (for Earth orbits) or days
     */
    [[nodiscard]] double periodHours() const;

    // Common Earth orbits
    static Orbit LEO(const CelestialBody& earth);  // Low Earth Orbit (~400 km)
    static Orbit ISS(const CelestialBody& earth);  // ISS orbit (~420 km)
    static Orbit GEO(const CelestialBody& earth);  // Geostationary (~35,786 km)
    static Orbit GPS(const CelestialBody& earth);  // GPS constellation (~20,200 km)

private:
    CelestialBody m_body;
    double m_radius;  // Orbital radius from body center [m]
};

} // namespace hohmann

#endif // HOHMANN_ORBIT_HPP
