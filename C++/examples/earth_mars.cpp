/*
 * earth_mars.cpp - Example: Interplanetary transfer from Earth to Mars
 *
 * ==============================================================================
 * AEROSPACE CONCEPT: Interplanetary Hohmann Transfer
 * ==============================================================================
 *
 * This example calculates a simplified Hohmann transfer between Earth's orbit
 * and Mars' orbit around the Sun. This is a "heliocentric" calculation -
 * we treat Earth and Mars as points on circular orbits around the Sun.
 *
 * WHY MARS?
 * ---------
 * Mars is humanity's primary target for exploration beyond the Moon because:
 *   - It's relatively close (varies from 55 to 400 million km)
 *   - It has a thin atmosphere (useful for aerobraking)
 *   - Day length is similar to Earth (24h 37m)
 *   - Water ice exists at the poles and underground
 *   - Surface gravity is manageable (38% of Earth's)
 *
 * HELIOCENTRIC vs GEOCENTRIC
 * --------------------------
 * This calculation shows the transfer from Earth's orbit to Mars' orbit,
 * treating both as circles around the Sun:
 *
 *                        Mars' orbit
 *                      /            \
 *                     /   Transfer   \
 *                    /     ellipse    \
 *                   |        /\        |
 *      Earth's orbit|       /  \       |
 *                   |      /    \      |
 *                    \    / Sun  \    /
 *                     \  /   *   \  /
 *                      \/        \/
 *
 * The transfer ellipse is tangent to both orbits:
 *   - Perihelion (closest to Sun) at Earth's orbit
 *   - Aphelion (farthest from Sun) at Mars' orbit
 *
 * CRITICAL SIMPLIFICATIONS:
 * -------------------------
 * This calculation does NOT include:
 *   1. Escaping Earth's gravity (requires ~3.6 km/s from LEO)
 *   2. Entering Mars orbit (requires ~2.1 km/s)
 *   3. Elliptical actual orbits (eccentricity ~0.017 Earth, ~0.093 Mars)
 *   4. Orbital inclinations (Mars is tilted 1.85 deg to ecliptic)
 *   5. Gravity assists from other planets
 *
 * For a COMPLETE Mars mission delta-v budget, you need:
 *   - LEO to Earth escape: ~3.6 km/s
 *   - Heliocentric transfer: ~2.9 km/s (this calculation)
 *   - Mars orbit insertion: ~2.1 km/s
 *   - Mars landing: ~4.1 km/s (most done by atmosphere)
 *
 * ==============================================================================
 * AEROSPACE CONCEPT: Launch Windows and Phase Angles
 * ==============================================================================
 *
 * You can't launch to Mars anytime - the planets must be aligned correctly.
 *
 * PHASE ANGLE:
 * When launching, Mars must be AHEAD of Earth by a specific angle (about 44 deg).
 * This ensures that when the spacecraft arrives at Mars' orbital radius,
 * Mars will actually BE there!
 *
 * SYNODIC PERIOD:
 * The time between launch windows (when Earth-Mars alignment repeats):
 *
 *   T_synodic = (T_earth * T_mars) / |T_mars - T_earth|
 *             = (1 year * 1.88 years) / (0.88 years)
 *             = ~2.14 years = ~26 months
 *
 * This is why Mars missions cluster together - everyone launches during
 * the same window! (Mars 2020, Tianwen-1, Hope all launched July-Aug 2020)
 *
 * ==============================================================================
 * C++ CONCEPTS DEMONSTRATED
 * ==============================================================================
 *
 * 1. USING NAMED CONSTANTS
 *    `orbitalRadius::earth` from constants.hpp
 *    Better than magic numbers like 1.496e11
 *
 * 2. UNIT CONVERSIONS
 *    Dividing by 1e9 to convert meters to million km
 *    Dividing by 86400*365.25 to convert seconds to years
 *
 * 3. COMPOUND CALCULATIONS
 *    Synodic period formula using results from multiple objects
 *
 * 4. ASTRONOMICAL UNITS (AU)
 *    1 AU = Earth-Sun distance = 1.496e11 meters
 *    Mars orbits at ~1.52 AU
 *
 * See also:
 *   orbit.hpp for Orbit class
 *   constants.hpp for orbital_radius values
 */

#include "hohmann/celestial_body.hpp"
#include "hohmann/orbit.hpp"
#include "hohmann/hohmann_transfer.hpp"
#include "hohmann/constants.hpp"

#include <iostream>
#include <iomanip>

using namespace hohmann;

/**
 * Main entry point - calculates Earth-Mars heliocentric transfer.
 *
 * This example demonstrates interplanetary mission planning at a basic level.
 */
int main() {
    std::cout << "================================================\n";
    std::cout << "     Earth to Mars Transfer (Heliocentric)\n";
    std::cout << "================================================\n\n";

    // =========================================================================
    // CENTRAL BODY: THE SUN
    // =========================================================================
    // For interplanetary transfers, the Sun is the central body.
    // Earth and Mars are treated as points orbiting the Sun.
    //
    // SCALE OF THE SUN:
    //   GM = 1.327e20 m^3/s^2  (333,000x Earth's GM)
    //   Mass = 1.989e30 kg (contains 99.86% of solar system mass!)
    // =========================================================================
    auto sun = CelestialBody::Sun();

    std::cout << "Central Body: " << sun.name() << "\n";
    std::cout << "  GM = " << std::scientific << sun.gm() << " m^3/s^2\n\n";

    // =========================================================================
    // PLANETARY ORBITS (CIRCULAR APPROXIMATION)
    // =========================================================================
    // We approximate Earth and Mars as having circular orbits.
    //
    // REALITY CHECK:
    //   Earth eccentricity: 0.017 (very nearly circular)
    //   Mars eccentricity: 0.093 (noticeably elliptical)
    //
    // Mars' elliptical orbit means:
    //   - Perihelion (closest): 1.38 AU
    //   - Aphelion (farthest): 1.67 AU
    //
    // Launch windows when Mars is near perihelion require less delta-v
    // and are preferred for crewed missions.
    //
    // ORBITAL RADII FROM constants.hpp:
    //   EARTH = 1.496e11 m = 1 AU (Astronomical Unit definition)
    //   MARS  = 2.279e11 m = 1.524 AU
    // =========================================================================

    // Create orbits using the Orbit constructor directly (not from_altitude)
    // because we're specifying the orbital RADIUS, not altitude above surface
    auto earth_orbit = Orbit(sun, orbitalRadius::earth);
    auto mars_orbit = Orbit(sun, orbitalRadius::mars);

    std::cout << std::fixed;  // Switch from scientific to fixed notation

    // -------------------------------------------------------------------------
    // EARTH'S ORBIT
    // -------------------------------------------------------------------------
    // Earth orbits at 1 AU = 149.6 million km from the Sun
    // This distance defines the Astronomical Unit (AU)
    //
    // Orbital velocity: ~29.78 km/s (107,000 km/h!)
    // Period: 1 year (365.25 days - this defines our year)
    // -------------------------------------------------------------------------
    std::cout << "=== Earth's Orbit ===\n";
    std::cout << std::setprecision(3);

    // Convert meters to "million km" for human-readable distances
    // 1e9 = 1,000,000,000 = conversion from m to million km
    std::cout << "  Radius: " << earth_orbit.radius() / 1e9 << " million km\n";
    std::cout << "          (1.000 AU)\n";  // By definition

    std::cout << std::setprecision(2);
    // Convert m/s to km/s by dividing by 1000
    std::cout << "  Velocity: " << earth_orbit.velocity() / 1000.0 << " km/s\n";

    // Convert seconds to years: 86400 seconds/day * 365.25 days/year
    std::cout << "  Period: " << earth_orbit.period() / (86400.0 * 365.25) << " years\n\n";

    // -------------------------------------------------------------------------
    // MARS' ORBIT
    // -------------------------------------------------------------------------
    // Mars orbits at ~1.524 AU = 228 million km from the Sun
    //
    // Orbital velocity: ~24.1 km/s (slower than Earth - farther from Sun)
    // Period: ~1.88 years (687 Earth days)
    //
    // Mars' longer year means Earth "laps" Mars every ~26 months
    // -------------------------------------------------------------------------
    std::cout << "=== Mars' Orbit ===\n";
    std::cout << std::setprecision(3);
    std::cout << "  Radius: " << mars_orbit.radius() / 1e9 << " million km\n";

    // Calculate and display distance in AU
    // AU = orbital_radius / Earth's orbital radius
    std::cout << "          (" << mars_orbit.radius() / orbitalRadius::earth << " AU)\n";

    std::cout << std::setprecision(2);
    std::cout << "  Velocity: " << mars_orbit.velocity() / 1000.0 << " km/s\n";
    std::cout << "  Period: " << mars_orbit.period() / (86400.0 * 365.25) << " years\n\n";

    // =========================================================================
    // HOHMANN TRANSFER CALCULATION
    // =========================================================================
    // The HohmannTransfer class calculates the transfer ellipse that connects
    // Earth's orbit (perihelion) to Mars' orbit (aphelion).
    //
    // TRANSFER ELLIPSE PROPERTIES:
    //   Perihelion = Earth's orbit radius = 1.0 AU
    //   Aphelion = Mars' orbit radius = 1.524 AU
    //   Semi-major axis = (1.0 + 1.524) / 2 = 1.262 AU
    // =========================================================================
    HohmannTransfer transfer(earth_orbit, mars_orbit);
    auto result = transfer.result();

    std::cout << "=== Hohmann Transfer ===\n";
    std::cout << std::setprecision(3);
    std::cout << "  Transfer orbit semi-major axis: "
              << result.semiMajorAxis / 1e9 << " million km\n\n";

    // -------------------------------------------------------------------------
    // BURN 1: Trans-Mars Injection (TMI)
    // -------------------------------------------------------------------------
    // This is the delta-v needed to leave Earth's orbital velocity and
    // enter the transfer ellipse.
    //
    // IMPORTANT: This is the HELIOCENTRIC delta-v only!
    // A real spacecraft starting from LEO needs additional delta-v to:
    //   1. Escape Earth's gravity well (~11.2 km/s escape velocity)
    //   2. But we get credit for Earth's orbital velocity (~29.8 km/s)
    //
    // The actual LEO departure burn is ~3.6 km/s
    // -------------------------------------------------------------------------
    std::cout << std::setprecision(2);
    std::cout << "  BURN 1 (at Earth's orbit):\n";
    std::cout << "    dv1 = " << result.deltaV1 / 1000.0 << " km/s\n";
    std::cout << "    This is the velocity change needed to enter\n";
    std::cout << "    the transfer orbit from Earth's orbital velocity.\n\n";

    // -------------------------------------------------------------------------
    // COAST PHASE: The Long Journey
    // -------------------------------------------------------------------------
    // After TMI, the spacecraft coasts along the transfer ellipse.
    // No propulsion needed - just falling around the Sun.
    //
    // Transfer time is half the period of the transfer ellipse:
    //   T_transfer = 0.5 * 2*pi*sqrt(a^3/GM_sun)
    //
    // For Earth-Mars: ~259 days (~8.5 months)
    //
    // During this time:
    //   - Mars travels ~135 deg around its orbit
    //   - Earth travels ~255 deg around its orbit
    //   - The spacecraft travels 180 deg (half the ellipse)
    // -------------------------------------------------------------------------
    std::cout << "  COAST PHASE:\n";
    double days = result.transferTimeDays();
    std::cout << "    Transfer time: " << days << " days\n";
    std::cout << "                   (" << days / 30.44 << " months)\n";  // ~30.44 days/month
    std::cout << "    The spacecraft coasts along the transfer ellipse.\n\n";

    // -------------------------------------------------------------------------
    // BURN 2: Mars Orbit Insertion (MOI)
    // -------------------------------------------------------------------------
    // When arriving at Mars' orbital radius, the spacecraft is moving slower
    // than Mars (it's at aphelion of its ellipse). This burn speeds it up
    // to match Mars' orbital velocity.
    //
    // Again, this is HELIOCENTRIC only. To actually orbit Mars, you need
    // additional delta-v to be captured by Mars' gravity.
    // -------------------------------------------------------------------------
    std::cout << "  BURN 2 (at Mars' orbit):\n";
    std::cout << "    dv2 = " << result.deltaV2 / 1000.0 << " km/s\n";
    std::cout << "    This matches Mars' orbital velocity.\n\n";

    // =========================================================================
    // SUMMARY
    // =========================================================================
    std::cout << "=== Summary ===\n";
    std::cout << "  Total heliocentric dv: " << result.totalDeltaV / 1000.0 << " km/s\n";
    std::cout << "  Transfer time: " << days << " days (~"
              << std::setprecision(1) << days / 30.44 << " months)\n\n";

    // -------------------------------------------------------------------------
    // PHASE ANGLE: When to Launch
    // -------------------------------------------------------------------------
    // The phase angle tells us where Mars needs to be relative to Earth
    // at the moment of launch.
    //
    // CALCULATION:
    //   Mars travels an angle theta_mars during transfer time
    //   Spacecraft travels 180 deg (half orbit)
    //   So Mars must start at: 180 - theta_mars degrees ahead of Earth
    //
    // For Earth-Mars: ~44 degrees
    //
    // If Mars isn't at this angle, you either:
    //   - Wait for the next launch window (~26 months)
    //   - Use more fuel for a faster transfer (rarely done)
    // -------------------------------------------------------------------------
    double phase_deg = transfer.phaseAngle() * 180.0 / math::pi;  // radians to degrees
    std::cout << "  Launch phase angle: " << std::setprecision(1)
              << phase_deg << " deg\n";
    std::cout << "  (Mars should be this far ahead of Earth at launch)\n\n";

    // =========================================================================
    // REAL MISSION CONTEXT
    // =========================================================================
    // This heliocentric calculation is just one piece of mission planning.
    // A complete Mars mission delta-v budget looks like:
    //
    // FROM EARTH:
    //   LEO insertion:     ~9.4 km/s  (from Earth's surface)
    //   Trans-Mars injection: ~3.6 km/s (from LEO)
    //
    // HELIOCENTRIC (this calculation):
    //   Transfer dv: ~2.9 km/s (already accounted for in TMI)
    //
    // AT MARS:
    //   Mars orbit insertion: ~2.1 km/s (to enter Mars orbit)
    //   Landing: ~4.1 km/s (mostly aerobraking if atmosphere used)
    //
    // TOTAL (Earth surface to Mars surface): ~11-16 km/s
    // This is why Mars missions are so challenging!
    // =========================================================================
    std::cout << "=== Important Notes ===\n";
    std::cout << "  This is a SIMPLIFIED heliocentric calculation.\n\n";
    std::cout << "  A real Mars mission also needs:\n";
    std::cout << "  - Earth departure burn: ~3.6 km/s from LEO\n";
    std::cout << "  - Mars orbit insertion: ~2.1 km/s\n";
    std::cout << "  - Mars landing (if applicable): ~4.1 km/s\n\n";
    std::cout << "  Launch windows occur every ~26 months when\n";
    std::cout << "  Earth and Mars are properly aligned.\n";

    // -------------------------------------------------------------------------
    // SYNODIC PERIOD CALCULATION
    // -------------------------------------------------------------------------
    // The synodic period is the time between identical Earth-Mars alignments.
    //
    // DERIVATION:
    //   Earth angular velocity: omega_E = 2*pi / T_E
    //   Mars angular velocity:  omega_M = 2*pi / T_M
    //
    //   Relative angular velocity: omega_rel = omega_E - omega_M
    //   (Earth catches up to Mars at this rate)
    //
    //   Time for Earth to "lap" Mars: T_synodic = 2*pi / omega_rel
    //
    //   Simplifying: T_synodic = (T_E * T_M) / |T_M - T_E|
    //
    // For Earth-Mars:
    //   T_synodic = (1.0 * 1.88) / (1.88 - 1.0) = 2.14 years = 26 months
    // -------------------------------------------------------------------------
    double T_earth = earth_orbit.period();  // in seconds
    double T_mars = mars_orbit.period();    // in seconds

    // Calculate synodic period using the formula
    double synodic_period = std::abs((T_earth * T_mars) / (T_mars - T_earth));

    std::cout << "\n  Synodic period (time between launch windows):\n";
    std::cout << "    " << synodic_period / (86400.0 * 30.44) << " months\n";
    // Convert seconds to months: 86400 sec/day * 30.44 days/month

    return 0;  // Success
}
