/*
 * leo_to_geo.cpp - Example: LEO to GEO transfer around Earth
 *
 * ==============================================================================
 * AEROSPACE CONCEPT: LEO to GEO Transfer
 * ==============================================================================
 *
 * This example demonstrates one of the most common satellite maneuvers in
 * commercial spaceflight: transferring from Low Earth Orbit (LEO) to
 * Geostationary Earth Orbit (GEO).
 *
 * WHY GEO IS SPECIAL:
 * -------------------
 * Geostationary orbit has a unique property: satellites there appear to
 * "hover" motionless over a single point on Earth's equator.
 *
 *   Period = 23 hours, 56 minutes, 4 seconds (one sidereal day)
 *   Altitude = 35,786 km
 *   Velocity = 3,075 m/s
 *
 * This happens because the satellite's orbital period exactly matches
 * Earth's rotation. From the ground, the satellite never moves!
 *
 * GEO APPLICATIONS:
 *   - Communications (DirecTV, satellite phones, internet)
 *   - Weather monitoring (GOES satellites)
 *   - Early warning systems
 *   - Broadcasting
 *
 * WHY NOT LAUNCH DIRECTLY TO GEO?
 * -------------------------------
 * Rockets don't go directly to GEO because:
 *   1. It requires too much fuel for a single burn
 *   2. Intermediate orbit allows checkout of spacecraft systems
 *   3. Launch vehicle can be smaller/cheaper by doing less work
 *
 * Instead, missions typically follow this sequence:
 *   1. Launch to LEO or directly to GTO (Geostationary Transfer Orbit)
 *   2. Coast to apoapsis (highest point)
 *   3. Fire onboard engine to circularize at GEO
 *
 * A GTO is essentially the Hohmann transfer ellipse - one end at LEO altitude,
 * the other at GEO altitude.
 *
 * ==============================================================================
 * C++ CONCEPTS DEMONSTRATED
 * ==============================================================================
 *
 * 1. STANDARD LIBRARY INCLUDES
 *    - <iostream> for console output
 *    - <iomanip> for number formatting
 *
 * 2. AUTO TYPE DEDUCTION
 *    `auto earth = CelestialBody::Earth();`
 *    The compiler figures out that `earth` is type `CelestialBody`
 *
 * 3. SCIENTIFIC VS FIXED NOTATION
 *    std::scientific -> 3.986e+14
 *    std::fixed      -> 398600000000000.00
 *
 * 4. DEREFERENCING std::optional
 *    `*earth.radius()` - the * gets the value from the optional
 *    Only safe because we know Earth has a defined radius!
 *
 * 5. METHOD CHAINING ON RESULTS
 *    `result.transferTimeHours()` calls a method on the returned struct
 *
 * See also:
 *   orbit.hpp for Orbit class and factory methods
 *   hohmann_transfer.hpp for transfer calculations
 */

#include "hohmann/celestial_body.hpp"
#include "hohmann/orbit.hpp"
#include "hohmann/hohmann_transfer.hpp"

#include <iostream>
#include <iomanip>

using namespace hohmann;

/**
 * Main entry point - calculates and displays a LEO to GEO transfer.
 *
 * This example shows step-by-step how a Hohmann transfer works,
 * with detailed output explaining each phase of the maneuver.
 */
int main() {
    // =========================================================================
    // HEADER - Set up the display
    // =========================================================================
    std::cout << "================================================\n";
    std::cout << "    LEO to GEO Transfer - Detailed Example\n";
    std::cout << "================================================\n\n";

    // =========================================================================
    // CENTRAL BODY SETUP
    // =========================================================================
    // Create Earth using the static factory method.
    // This gives us accurate values for GM and radius.
    //
    // C++ CONCEPT: auto
    // -----------------
    // `auto` tells the compiler to deduce the type automatically.
    // Here, CelestialBody::Earth() returns CelestialBody, so:
    //   auto earth = ...  becomes  CelestialBody earth = ...
    //
    // Benefits of auto:
    //   - Less typing for long type names
    //   - Code adapts if return type changes
    //   - Required for some types (lambdas, iterators)
    //
    // Drawbacks:
    //   - Can hide what type you're working with
    //   - Overuse makes code harder to read
    // =========================================================================
    auto earth = CelestialBody::Earth();

    // Display Earth's properties
    // Notice: We use two different number formats below
    std::cout << "Central Body: " << earth.name() << "\n";

    // C++ CONCEPT: std::scientific
    // ----------------------------
    // std::scientific displays numbers in scientific notation: 3.986e+14
    // This is better for very large numbers like GM (398,600,000,000,000)
    std::cout << "  GM = " << std::scientific << earth.gm() << " m^3/s^2\n";

    // C++ CONCEPT: std::fixed and setprecision
    // ----------------------------------------
    // std::fixed displays numbers in fixed-point notation: 6371.00
    // setprecision(0) means no decimal places
    //
    // IMPORTANT: These are "sticky" - they affect ALL subsequent output
    // until you change them again!
    std::cout << "  Radius = " << std::fixed << std::setprecision(0)
              << *earth.radius() / 1000.0 << " km\n\n";
    // Note: *earth.radius() dereferences the std::optional<double>
    // We divide by 1000 to convert meters to kilometers for readability

    // =========================================================================
    // DEFINE THE ORBITS
    // =========================================================================
    // LEO (Low Earth Orbit): 400 km altitude
    // - This is near the ISS altitude (~420 km)
    // - Typical for initial satellite deployment
    // - Starlink satellites start here before raising to ~550 km
    //
    // GEO (Geostationary Earth Orbit): 35,786 km altitude
    // - 24-hour orbital period matches Earth's rotation
    // - Satellite appears stationary from the ground
    // =========================================================================

    // Create LEO orbit from altitude (400 km = 400,000 m = 400e3 m)
    // The `e3` notation means "times 10^3" - it's scientific notation in code
    auto leo = Orbit::fromAltitude(earth, 400e3);  // 400 km in meters

    // Create GEO orbit using the preset factory method
    // This uses the exact GEO altitude (35,786 km)
    auto geo = Orbit::GEO(earth);

    // =========================================================================
    // DISPLAY INITIAL ORBIT (LEO)
    // =========================================================================
    std::cout << "=== Initial Orbit (LEO) ===\n";
    std::cout << std::setprecision(0);  // No decimal places for km values

    // *leo.altitude() dereferences the optional to get the double value
    // We know LEO has a valid altitude because Earth has a defined radius
    std::cout << "  Altitude: " << *leo.altitude() / 1000.0 << " km\n";

    // Radius is measured from Earth's CENTER, not surface
    // radius = Earth's radius + altitude = 6371 + 400 = 6771 km
    std::cout << "  Radius:   " << leo.radius() / 1000.0 << " km\n";

    std::cout << std::setprecision(2);  // 2 decimal places for velocity/period
    // LEO velocity is about 7.67 km/s - this is FAST!
    // For reference, a bullet travels at about 1 km/s
    std::cout << "  Velocity: " << leo.velocity() << " m/s\n";

    // LEO period is about 1.5 hours - ISS orbits Earth ~16 times per day
    std::cout << "  Period:   " << leo.periodHours() << " hours\n\n";

    // =========================================================================
    // DISPLAY TARGET ORBIT (GEO)
    // =========================================================================
    std::cout << "=== Target Orbit (GEO) ===\n";
    std::cout << std::setprecision(0);
    std::cout << "  Altitude: " << *geo.altitude() / 1000.0 << " km\n";
    std::cout << "  Radius:   " << geo.radius() / 1000.0 << " km\n";

    std::cout << std::setprecision(2);
    // GEO velocity is about 3.07 km/s - SLOWER than LEO!
    // Counterintuitive: higher orbits are slower, not faster
    std::cout << "  Velocity: " << geo.velocity() << " m/s\n";

    // GEO period is exactly 24 hours (actually 23h 56m 4s sidereal)
    std::cout << "  Period:   " << geo.periodHours() << " hours\n\n";

    // =========================================================================
    // CALCULATE THE HOHMANN TRANSFER
    // =========================================================================
    // The HohmannTransfer class calculates everything we need:
    //   - Both delta-v burns
    //   - Transfer orbit parameters
    //   - Time of flight
    //
    // AEROSPACE: The Two Burns
    // ------------------------
    // BURN 1 (at LEO, periapsis of transfer):
    //   - Increases velocity to enter transfer ellipse
    //   - Raises apoapsis (far point) to GEO altitude
    //   - "Prograde" = in the direction of travel
    //
    // BURN 2 (at GEO, apoapsis of transfer):
    //   - Increases velocity to circularize
    //   - At apoapsis, the spacecraft is moving too slowly for a circular orbit
    //   - This burn speeds it up to GEO circular velocity
    // =========================================================================
    HohmannTransfer transfer(leo, geo);

    // Get all results at once
    // C++ CONCEPT: Structured Result
    // ------------------------------
    // result is a TransferResult struct containing multiple values:
    //   - delta_v1, delta_v2, total_delta_v
    //   - semi_major_axis, transfer_time
    // This is better than having 5 separate getter methods
    auto result = transfer.result();

    std::cout << "=== Hohmann Transfer ===\n";

    // The transfer ellipse has its center of mass at Earth's center
    // Semi-major axis = (LEO radius + GEO radius) / 2
    std::cout << "  Transfer orbit semi-major axis: "
              << std::setprecision(0) << result.semiMajorAxis / 1000.0 << " km\n\n";

    // -------------------------------------------------------------------------
    // BURN 1: Departure from LEO
    // -------------------------------------------------------------------------
    // This is where the spacecraft fires its engine to leave LEO
    // "Prograde" means burning in the direction of travel (speeding up)
    std::cout << "  BURN 1 (at LEO periapsis):\n";
    std::cout << std::setprecision(2);
    std::cout << "    dv1 = " << result.deltaV1 << " m/s (prograde)\n";
    std::cout << "    This raises apoapsis to GEO altitude\n\n";

    // -------------------------------------------------------------------------
    // COAST PHASE: Transfer orbit
    // -------------------------------------------------------------------------
    // After burn 1, the spacecraft is on an elliptical orbit
    // It coasts (no engine firing) from periapsis to apoapsis
    // This takes about 5 hours for LEO-GEO
    std::cout << "  COAST PHASE:\n";
    std::cout << "    Time to reach apoapsis: " << result.transferTimeHours() << " hours\n";
    std::cout << "    Distance traveled: half of transfer ellipse\n\n";

    // -------------------------------------------------------------------------
    // BURN 2: Arrival at GEO
    // -------------------------------------------------------------------------
    // At apoapsis, the spacecraft is at GEO altitude but moving too slowly
    // for a circular orbit. This burn adds the remaining velocity needed.
    std::cout << "  BURN 2 (at GEO apoapsis):\n";
    std::cout << "    dv2 = " << result.deltaV2 << " m/s (prograde)\n";
    std::cout << "    This circularizes the orbit at GEO\n\n";

    // =========================================================================
    // SUMMARY
    // =========================================================================
    std::cout << "=== Summary ===\n";
    std::cout << "  Total dv required: " << result.totalDeltaV << " m/s\n";
    std::cout << "  Total transfer time: " << result.transferTimeHours() << " hours\n";

    // =========================================================================
    // REAL-WORLD CONTEXT
    // =========================================================================
    // In practice, commercial launches to GEO work slightly differently:
    //
    // 1. DIRECT TO GTO (Geostationary Transfer Orbit):
    //    The rocket inserts the satellite directly into the transfer ellipse,
    //    essentially performing Burn 1 as part of the launch sequence.
    //
    // 2. SATELLITE PERFORMS BURN 2:
    //    The satellite uses its onboard propulsion (often electric or
    //    bipropellant) to circularize at GEO. This can take hours (chemical)
    //    or months (electric propulsion).
    //
    // 3. STATION KEEPING:
    //    Once at GEO, small burns are needed periodically to maintain
    //    position against perturbations (Moon, Sun, solar pressure).
    // =========================================================================
    std::cout << "\n=== Context ===\n";
    std::cout << "  For reference, a typical GTO (Geostationary Transfer Orbit)\n";
    std::cout << "  insertion by a launch vehicle provides most of dv1.\n";
    std::cout << "  The satellite's onboard propulsion then completes\n";
    std::cout << "  the circularization burn (dv2) at apoapsis.\n";

    return 0;  // Success
}
