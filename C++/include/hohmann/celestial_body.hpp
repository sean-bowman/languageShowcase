#ifndef HOHMANN_CELESTIAL_BODY_HPP
#define HOHMANN_CELESTIAL_BODY_HPP

/*
 * celestial_body.hpp - Represents a celestial body with gravitational properties
 */

#include <string>
#include <optional>

namespace hohmann {

/*
 * CelestialBody class - Represents a celestial body that can be orbited
 *
 * A celestial body is defined by its gravitational parameter (GM) and
 * optionally its physical radius. Pre-defined bodies are available
 * through static factory methods.
 */
class CelestialBody {
public:
    /*
     * Construct a celestial body
     *
     * Parameters:
     *   name - Display name of the body
     *   gm - Gravitational parameter (GM) in m³/s²
     *   radius - Optional mean radius in meters
     */
    CelestialBody(const std::string& name, double gm,
                  std::optional<double> radius = std::nullopt);

    // Accessors
    [[nodiscard]] const std::string& name() const { return m_name; }
    [[nodiscard]] double gm() const { return m_gm; }
    [[nodiscard]] std::optional<double> radius() const { return m_radius; }

    /*
     * Calculate circular orbital velocity at given radius
     *
     * Parameters:
     *   orbitalRadius - Distance from body center in meters
     *
     * Returns:
     *   Orbital velocity in m/s
     */
    [[nodiscard]] double circularVelocity(double orbitalRadius) const;

    /*
     * Calculate escape velocity at given radius
     *
     * Parameters:
     *   distance - Distance from body center in meters
     *
     * Returns:
     *   Escape velocity in m/s
     */
    [[nodiscard]] double escapeVelocity(double distance) const;

    /*
     * Calculate orbital period for circular orbit
     *
     * Parameters:
     *   orbitalRadius - Distance from body center in meters
     *
     * Returns:
     *   Orbital period in seconds
     */
    [[nodiscard]] double orbitalPeriod(double orbitalRadius) const;

    // Pre-defined celestial bodies
    static CelestialBody Sun();
    static CelestialBody Earth();
    static CelestialBody Moon();
    static CelestialBody Mars();
    static CelestialBody Jupiter();

private:
    std::string m_name;
    double m_gm;  // Gravitational parameter [m³/s²]
    std::optional<double> m_radius;  // Mean radius [m]
};

} // namespace hohmann

#endif // HOHMANN_CELESTIAL_BODY_HPP
