#ifndef HOHMANN_TRANSFER_HPP
#define HOHMANN_TRANSFER_HPP

/*
 * hohmann_transfer.hpp - Calculates Hohmann transfer orbits between two circular orbits
 */

#include "orbit.hpp"

namespace hohmann {

/*
 * TransferResult struct - Contains the results of a Hohmann transfer calculation
 */
struct TransferResult {
    double deltaV1;        ///< First burn delta-v [m/s] (departure)
    double deltaV2;        ///< Second burn delta-v [m/s] (arrival)
    double totalDeltaV;   ///< Total delta-v required [m/s]
    double transferTime;   ///< Transfer time [seconds]
    double semiMajorAxis; ///< Transfer orbit semi-major axis [m]

    // Derived values
    [[nodiscard]] double transferTimeHours() const { return transferTime / 3600.0; }
    [[nodiscard]] double transferTimeDays() const { return transferTime / 86400.0; }
};

/*
 * HohmannTransfer class - Calculates Hohmann transfer parameters between two circular orbits
 *
 * A Hohmann transfer is the most fuel-efficient two-impulse maneuver to
 * transfer between two coplanar circular orbits. It uses an elliptical
 * transfer orbit that is tangent to both the initial and final orbits.
 *
 * The transfer consists of:
 * 1. First burn (delta_v1): At periapsis of transfer orbit, raises apoapsis
 * 2. Coast along transfer ellipse (half orbital period)
 * 3. Second burn (delta_v2): At apoapsis, circularizes the orbit
 */
class HohmannTransfer {
public:
    /*
     * Construct a Hohmann transfer between two orbits
     *
     * Parameters:
     *   initial - The starting circular orbit
     *   final_orbit - The target circular orbit
     *
     * Throws:
     *   std::invalid_argument if orbits are around different bodies
     */
    HohmannTransfer(const Orbit& initial, const Orbit& final_orbit);

    // Accessors
    [[nodiscard]] const Orbit& initialOrbit() const { return m_initial; }
    [[nodiscard]] const Orbit& finalOrbit() const { return m_final; }
    [[nodiscard]] const TransferResult& result() const { return m_result; }

    /*
     * Check if this is a "raise" maneuver (going to higher orbit)
     *
     * Returns:
     *   true if final orbit is higher than initial
     */
    [[nodiscard]] bool isRaising() const;

    /*
     * Calculate phase angle for rendezvous
     *
     * For rendezvous with a target in the destination orbit, launch must
     * occur when the target is at a specific phase angle ahead/behind.
     *
     * Returns:
     *   Phase angle in radians
     */
    [[nodiscard]] double phaseAngle() const;

    /* Print a summary of the transfer to stdout */
    void printSummary() const;

private:
    Orbit m_initial;
    Orbit m_final;
    TransferResult m_result;

    /* Calculate all transfer parameters */
    void calculate();
};

} // namespace hohmann

#endif // HOHMANN_TRANSFER_HPP
