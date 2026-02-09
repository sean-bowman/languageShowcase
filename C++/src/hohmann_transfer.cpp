/*
 * hohmann_transfer.cpp - Implementation of the Hohmann Transfer Orbit Calculator
 *
 * ==============================================================================
 * AEROSPACE CONCEPT: Hohmann Transfer Orbit
 * ==============================================================================
 *
 * A Hohmann transfer is an orbital maneuver that moves a spacecraft between
 * two circular orbits using two engine burns. Named after German engineer
 * Walter Hohmann who described it in his 1925 book "Die Erreichbarkeit der
 * Himmelskorper" (The Attainability of Celestial Bodies).
 *
 * WHY IT MATTERS:
 * - Most fuel-efficient two-impulse transfer between coplanar circular orbits
 * - Used for nearly all GEO satellite deployments (LEO -> GEO)
 * - Foundation for understanding interplanetary mission design
 * - Trade-off: Fuel efficiency vs transfer time (slower than direct transfers)
 *
 * THE MANEUVER (for raising orbit):
 *
 *                          * Apoapsis (highest point)
 *                       *     *
 *                    *    2nd    *
 *                 *     burn       *  <-- Final circular orbit
 *              *                     *
 *           *                          *
 *          *                            *
 *         *     Transfer                 *
 *         *      Ellipse                 *
 *          *                            *
 *           *    1st burn             *
 *            *     here ->          *
 *              *      *   *   *   *  <-- Initial circular orbit
 *                 *  *  *  *  *
 *                    [Earth]
 *
 * Step 1: At periapsis, fire engines PROGRADE (forward) to enter transfer orbit
 * Step 2: Coast along the transfer ellipse (takes ~5 hours for LEO->GEO)
 * Step 3: At apoapsis, fire engines PROGRADE again to circularize
 *
 * KEY EQUATIONS:
 *   - Transfer semi-major axis: a = (r1 + r2) / 2
 *   - Vis-viva equation: v^2 = mu * (2/r - 1/a)
 *   - Transfer time: T = pi * sqrt(a^3 / mu)
 *
 * Where:
 *   r1 = initial orbit radius [m]
 *   r2 = final orbit radius [m]
 *   mu = GM = gravitational parameter [m^3/s^2]
 *   a  = semi-major axis [m]
 *   v  = orbital velocity [m/s]
 *
 * ==============================================================================
 * C++ CONCEPTS DEMONSTRATED IN THIS FILE
 * ==============================================================================
 *
 * 1. CLASSES AND ENCAPSULATION
 *    - HohmannTransfer bundles data (orbits, results) with behavior (calculate)
 *    - Private members (m_initial, m_final) hide implementation details
 *    - Public methods provide controlled access to functionality
 *
 * 2. MEMBER INITIALIZER LISTS (line 78)
 *    Constructor syntax `: m_initial(initial), m_final(final_orbit)`
 *    - More efficient than assignment in constructor body
 *    - Required for const members and references
 *    - Initializes members in declaration order (not list order!)
 *
 * 3. CONST CORRECTNESS
 *    - `const Orbit&` parameters: promise not to modify the argument
 *    - `const` after method name: promise method won't modify object state
 *    - Enables compiler optimizations and prevents accidental modifications
 *
 * 4. [[nodiscard]] ATTRIBUTE (C++17)
 *    - Compiler warning if return value is ignored
 *    - Useful for getters and calculations where ignoring result is likely a bug
 *
 * 5. NAMESPACES
 *    - `namespace hohmann { }` groups related code
 *    - Prevents naming conflicts with other libraries
 *    - Access via `hohmann::HohmannTransfer` or `using namespace hohmann`
 *
 * 6. std::optional (seen in other files)
 *    - C++17 feature for values that may or may not exist
 *    - Safer than null pointers or sentinel values
 *
 * See also:
 *   orbit.hpp for the Orbit class
 *   celestial_body.hpp for CelestialBody
 *   constants.hpp for mathematical constants
 */

#include "hohmann/hohmann_transfer.hpp"
#include "hohmann/constants.hpp"
#include <cmath>        // std::sqrt, std::pow, std::abs - mathematical functions
#include <stdexcept>    // std::invalid_argument - exception type
#include <iostream>     // std::cout - console output
#include <iomanip>      // std::fixed, std::setprecision - output formatting

namespace hohmann {

// =============================================================================
// CONSTRUCTOR
// =============================================================================

/**
 * Construct a Hohmann transfer calculator.
 *
 * C++ CONCEPT: Member Initializer List
 * ------------------------------------
 * The syntax `: m_initial(initial), m_final(final_orbit)` is a member
 * initializer list. It initializes member variables BEFORE the constructor
 * body executes.
 *
 * Why use it instead of assignment in the body?
 *   1. EFFICIENCY: Objects are constructed once, not default-constructed
 *      then assigned (which would mean constructing twice)
 *   2. REQUIRED: For const members, reference members, and members without
 *      default constructors
 *   3. ORDER: Members are initialized in DECLARATION order in the class,
 *      regardless of order in the initializer list (compiler may warn if
 *      these differ)
 *
 * Compare:
 *   GOOD (initializer list):
 *     HohmannTransfer(...) : m_initial(initial) { }
 *
 *   LESS EFFICIENT (assignment):
 *     HohmannTransfer(...) { m_initial = initial; }  // constructs then assigns
 */
HohmannTransfer::HohmannTransfer(const Orbit& initial, const Orbit& final_orbit)
    : m_initial(initial), m_final(final_orbit) {

    // =========================================================================
    // VALIDATION: Verify orbits are around the same celestial body
    // =========================================================================
    // We compare gravitational parameters (GM) because two different bodies
    // won't have the same GM (to within floating-point precision).
    //
    // PHYSICS NOTE: GM is used instead of mass because it's measured more
    // precisely. We know Earth's GM to 10 significant figures, but G and M
    // separately only to about 4 figures!
    //
    // C++ CONCEPT: std::abs vs abs
    // ----------------------------
    // Always use std::abs (from <cmath>) for floating-point numbers.
    // The C function abs() only works with integers and will truncate!
    // =========================================================================
    if (std::abs(initial.body().gm() - final_orbit.body().gm()) > 1.0) {
        throw std::invalid_argument(
            "Cannot transfer between orbits around different bodies"
        );
    }

    // Perform the Hohmann transfer calculation
    calculate();
}

// =============================================================================
// ORBIT ANALYSIS METHODS
// =============================================================================

/**
 * Determine if this transfer raises or lowers the orbit.
 *
 * C++ CONCEPT: const Methods
 * --------------------------
 * The `const` after the parameter list promises this method won't modify
 * any member variables. This allows:
 *   1. Calling on const HohmannTransfer objects
 *   2. Compiler optimizations (can cache results)
 *   3. Self-documenting code (reader knows it's a pure query)
 *
 * If you try to modify a member in a const method, compilation fails.
 */
bool HohmannTransfer::isRaising() const {
    return m_final.radius() > m_initial.radius();
}

/**
 * Calculate the phase angle for rendezvous with a target spacecraft.
 *
 * AEROSPACE CONCEPT: Phase Angle
 * ==============================
 * If you want to rendezvous with a target already in the destination orbit,
 * you can't just launch when you're directly below it. By the time you
 * arrive, it will have moved!
 *
 * The phase angle tells you how far AHEAD the target should be at launch.
 *
 * For transfers to higher orbits:
 *   - Target should be BEHIND you at launch (you'll catch up during transfer)
 *   - Phase angle is negative (or expressed as positive angle behind)
 *
 * For transfers to lower orbits:
 *   - Target should be AHEAD of you at launch
 *   - Phase angle is positive
 *
 * FORMULA:
 *   theta = pi * (1 - (1/(2*sqrt(2))) * sqrt((r1/r2 + 1)^3))
 *
 * This formula comes from:
 *   1. Calculating how far the target moves during transfer time
 *   2. Subtracting from 180 degrees (half orbit of transfer ellipse)
 *
 * C++ CONCEPT: std::pow vs multiplication
 * ---------------------------------------
 * For small integer powers, direct multiplication is faster:
 *   x * x * x  is faster than  std::pow(x, 3)
 *
 * But std::pow handles fractional exponents (like 1.5 here) which
 * multiplication cannot express simply.
 */
double HohmannTransfer::phaseAngle() const {
    double r1 = m_initial.radius();
    double r2 = m_final.radius();

    // Calculate (r1/r2 + 1)^1.5
    // The 1.5 exponent comes from Kepler's third law: T^2 proportional to a^3
    double ratio_term = std::pow((r1/r2 + 1.0), 1.5);

    // Final phase angle formula
    // math::pi is defined in constants.hpp as a constexpr double
    double angle = math::pi * (1.0 - ratio_term / (2.0 * std::sqrt(2.0)));

    return angle;  // in radians
}

// =============================================================================
// CORE CALCULATION
// =============================================================================

/**
 * Calculate all Hohmann transfer parameters.
 *
 * This is where the orbital mechanics happens. We calculate:
 *   1. The transfer orbit shape (semi-major axis)
 *   2. Velocities at each point using the vis-viva equation
 *   3. Delta-v required for each burn
 *   4. Total transfer time
 */
void HohmannTransfer::calculate() {
    // =========================================================================
    // STEP 1: Extract orbital parameters
    // =========================================================================
    double r1 = m_initial.radius();     // Initial orbit radius [m]
    double r2 = m_final.radius();       // Final orbit radius [m]
    double mu = m_initial.body().gm();  // Gravitational parameter [m^3/s^2]

    // =========================================================================
    // STEP 2: Calculate transfer orbit semi-major axis
    // =========================================================================
    // PHYSICS: Semi-Major Axis
    // ------------------------
    // For an ellipse, the semi-major axis 'a' is half the longest diameter.
    //
    // In a Hohmann transfer:
    //   - Periapsis (closest point) touches the inner orbit at r1
    //   - Apoapsis (farthest point) touches the outer orbit at r2
    //
    // From ellipse geometry:
    //   2a = r_periapsis + r_apoapsis = r1 + r2
    //   a = (r1 + r2) / 2
    //
    // This is simply the AVERAGE of the two orbital radii.
    // =========================================================================
    double a_transfer = (r1 + r2) / 2.0;

    // =========================================================================
    // STEP 3: Calculate velocities using the vis-viva equation
    // =========================================================================
    // PHYSICS: The Vis-Viva Equation
    // ------------------------------
    // The vis-viva ("living force") equation is fundamental to orbital mechanics.
    // It relates velocity to position for ANY point on ANY orbit:
    //
    //     v^2 = mu * (2/r - 1/a)
    //
    // Where:
    //   v  = orbital velocity at position r [m/s]
    //   mu = GM = gravitational parameter [m^3/s^2]
    //   r  = current distance from central body's center [m]
    //   a  = semi-major axis of the orbit [m]
    //
    // DERIVATION (simplified):
    // This comes from conservation of energy. Total orbital energy is:
    //   E = (1/2)mv^2 - GMm/r = -GMm/(2a)
    //
    // Solving for v gives the vis-viva equation.
    //
    // SPECIAL CASES:
    //   - Circular orbit (r = a):  v = sqrt(mu/r)
    //   - Parabolic escape (a->inf): v = sqrt(2*mu/r)  [escape velocity]
    //
    // C++ CONCEPT: std::sqrt
    // ----------------------
    // std::sqrt is the C++ standard library square root function.
    // It's in namespace std and requires <cmath> header.
    // For compile-time constants, you could use constexpr functions instead.
    // =========================================================================

    // Velocity in initial circular orbit
    // For circular: v = sqrt(mu/r)  [vis-viva with r = a]
    double v1 = std::sqrt(mu / r1);

    // Velocity at periapsis of transfer orbit (at radius r1)
    // Uses full vis-viva: v^2 = mu * (2/r1 - 1/a_transfer)
    double v_transfer_periapsis = std::sqrt(mu * (2.0/r1 - 1.0/a_transfer));

    // Velocity at apoapsis of transfer orbit (at radius r2)
    double v_transfer_apoapsis = std::sqrt(mu * (2.0/r2 - 1.0/a_transfer));

    // Velocity in final circular orbit
    double v2 = std::sqrt(mu / r2);

    // =========================================================================
    // STEP 4: Calculate delta-v for each burn
    // =========================================================================
    // PHYSICS: Delta-v (dv)
    // ---------------------
    // Delta-v represents the change in velocity a spacecraft must achieve.
    // It's the fundamental currency of spaceflight - you need fuel to get dv.
    //
    // The Tsiolkovsky rocket equation relates dv to fuel mass:
    //   dv = v_exhaust * ln(m_initial / m_final)
    //
    // Higher dv means more fuel, which is expensive and heavy.
    //
    // For Hohmann transfers:
    //   - Both burns are PROGRADE (in direction of motion) for raising orbits
    //   - Both burns are RETROGRADE (against motion) for lowering orbits
    //
    // We take absolute values because dv magnitude is what matters for fuel.
    // =========================================================================
    double dv1, dv2;

    if (r2 > r1) {
        // RAISING ORBIT: Both burns speed us up (prograde)
        // ------------------------------------------------
        // Burn 1: At periapsis, speed up to enter transfer orbit
        //         Current velocity: v1 (circular)
        //         Needed velocity: v_transfer_periapsis (faster, to raise apoapsis)
        dv1 = v_transfer_periapsis - v1;

        // Burn 2: At apoapsis, speed up to circularize
        //         Current velocity: v_transfer_apoapsis (slow at apoapsis)
        //         Needed velocity: v2 (circular, faster than transfer apoapsis)
        dv2 = v2 - v_transfer_apoapsis;
    } else {
        // LOWERING ORBIT: Both burns slow us down (retrograde)
        // ----------------------------------------------------
        // The math is reversed but the principle is the same.
        // At higher orbit, we slow down to drop periapsis.
        // At lower orbit, we slow down again to circularize.
        dv1 = v1 - v_transfer_periapsis;
        dv2 = v_transfer_apoapsis - v2;
    }

    // =========================================================================
    // STEP 5: Calculate transfer time
    // =========================================================================
    // PHYSICS: Transfer Time from Kepler's Third Law
    // -----------------------------------------------
    // The transfer takes exactly HALF the orbital period of the transfer ellipse.
    // (We travel from periapsis to apoapsis, which is half an orbit.)
    //
    // Kepler's third law:  T^2 = (4 * pi^2 / mu) * a^3
    // Solving for T:       T = 2 * pi * sqrt(a^3 / mu)
    // Half period:         T_transfer = pi * sqrt(a^3 / mu)
    //
    // For LEO (400 km) to GEO (35,786 km):
    //   a_transfer = (6771 + 42157) / 2 = 24,464 km
    //   T_transfer = about 5.25 hours
    //
    // This is why GEO satellite deployment takes several hours - you can't
    // rush orbital mechanics!
    // =========================================================================
    double T_transfer = math::pi * std::sqrt(std::pow(a_transfer, 3) / mu);

    // =========================================================================
    // STEP 6: Store results in the TransferResult struct
    // =========================================================================
    m_result.deltaV1 = std::abs(dv1);          // Always positive magnitude
    m_result.deltaV2 = std::abs(dv2);
    m_result.totalDeltaV = m_result.deltaV1 + m_result.deltaV2;
    m_result.transferTime = T_transfer;         // In seconds
    m_result.semiMajorAxis = a_transfer;       // In meters
}

// =============================================================================
// OUTPUT METHODS
// =============================================================================

/**
 * Print a human-readable summary of the transfer to the console.
 *
 * C++ CONCEPT: Output Stream Formatting
 * -------------------------------------
 * std::cout is the standard output stream. We control formatting with:
 *
 *   std::fixed       - Use fixed-point notation (not scientific)
 *   std::setprecision(n) - Show n digits after decimal point
 *   std::setw(n)     - Set minimum field width (for alignment)
 *
 * These "manipulators" modify stream state and persist until changed.
 * That's why we set precision to 0 for large numbers (radii) then back
 * to 2 for velocities.
 *
 * C++ CONCEPT: auto with std::optional
 * ------------------------------------
 * The line `if (auto alt = m_initial.altitude()) { ... }` demonstrates:
 *   1. `auto` type deduction (compiler figures out it's std::optional<double>)
 *   2. std::optional's boolean conversion (true if value present)
 *   3. Combined declaration and test in if statement
 *
 * Inside the if block, we dereference with `*alt` to get the double value.
 * This is safe because we only enter the block if the optional has a value.
 */
void HohmannTransfer::printSummary() const {
    // Set floating-point formatting for entire output
    std::cout << std::fixed << std::setprecision(2);

    std::cout << "\n========================================\n";
    std::cout << "      Hohmann Transfer Summary\n";
    std::cout << "========================================\n\n";

    std::cout << "Central Body: " << m_initial.body().name() << "\n\n";

    // -------------------------------------------------------------------------
    // Initial orbit information
    // -------------------------------------------------------------------------
    std::cout << "Initial Orbit:\n";
    std::cout << "  Radius:   " << std::setprecision(0)
              << m_initial.radius() / 1000.0 << " km\n";

    // Altitude is optional - only exists if the body has a defined radius
    // C++ auto keyword deduces type as std::optional<double>
    if (auto alt = m_initial.altitude()) {
        std::cout << "  Altitude: " << *alt / 1000.0 << " km\n";
    }
    std::cout << std::setprecision(2);
    std::cout << "  Velocity: " << m_initial.velocity() << " m/s\n";
    std::cout << "  Period:   " << m_initial.periodHours() << " hours\n\n";

    // -------------------------------------------------------------------------
    // Final orbit information
    // -------------------------------------------------------------------------
    std::cout << "Final Orbit:\n";
    std::cout << "  Radius:   " << std::setprecision(0)
              << m_final.radius() / 1000.0 << " km\n";
    if (auto alt = m_final.altitude()) {
        std::cout << "  Altitude: " << *alt / 1000.0 << " km\n";
    }
    std::cout << std::setprecision(2);
    std::cout << "  Velocity: " << m_final.velocity() << " m/s\n";
    std::cout << "  Period:   " << m_final.periodHours() << " hours\n\n";

    // -------------------------------------------------------------------------
    // Transfer orbit properties
    // -------------------------------------------------------------------------
    std::cout << "Transfer Orbit:\n";
    std::cout << "  Semi-major axis: " << std::setprecision(0)
              << m_result.semiMajorAxis / 1000.0 << " km\n";
    std::cout << "  Type: " << (isRaising() ? "Raising" : "Lowering") << "\n\n";

    // -------------------------------------------------------------------------
    // Delta-v requirements (the "cost" of the transfer)
    // -------------------------------------------------------------------------
    std::cout << std::setprecision(2);
    std::cout << "Delta-v Requirements:\n";
    std::cout << "  First burn (dv1):  " << m_result.deltaV1 << " m/s\n";
    std::cout << "  Second burn (dv2): " << m_result.deltaV2 << " m/s\n";
    std::cout << "  Total dv:          " << m_result.totalDeltaV << " m/s\n\n";

    // -------------------------------------------------------------------------
    // Transfer time
    // -------------------------------------------------------------------------
    std::cout << "Transfer Time:\n";
    double hours = m_result.transferTimeHours();
    if (hours < 24) {
        std::cout << "  " << hours << " hours\n";
    } else {
        std::cout << "  " << m_result.transferTimeDays() << " days\n";
        std::cout << "  (" << hours << " hours)\n";
    }

    // -------------------------------------------------------------------------
    // Phase angle for rendezvous
    // -------------------------------------------------------------------------
    double phase_deg = phaseAngle() * 180.0 / math::pi;
    std::cout << "\nPhase Angle for Rendezvous: " << phase_deg << " deg\n";

    std::cout << "\n========================================\n";
}

} // namespace hohmann
