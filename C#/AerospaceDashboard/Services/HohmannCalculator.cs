// =============================================================================
// HOHMANN TRANSFER CALCULATOR
// =============================================================================
//
// AEROSPACE CONCEPT: Hohmann Transfer Orbits
// ===========================================
//
// A Hohmann transfer is the most fuel-efficient way to move between two
// circular orbits in the same plane. Discovered by Walter Hohmann in 1925,
// it uses an elliptical transfer orbit tangent to both the initial and
// final orbits.
//
// The Maneuver (Two Burns):
// -------------------------
//
//                         Final Orbit (r2)
//                    .-'               '-.
//                  ,'    Transfer        `.
//         dv2 -> *'      Ellipse           \
//               /.                          .
//              /  .                          .
//             |    .                          |
//             |    |    Initial Orbit (r1)    |
//             |    |        .---.             |
//             |    |       /     \            |
//             |    .      |   *   |           |
//              \   .       \  |  /           .
//               \  .        '-+-'           /
//                `.:..........+...........,
//                             ^
//                             dv1 (prograde burn)
//
// 1. FIRST BURN (dv1): At periapsis of transfer orbit
//    - Fire engines prograde (in direction of motion)
//    - Raises apoapsis to final orbit altitude
//
// 2. COAST PHASE: Travel along transfer ellipse
//    - Takes exactly half the orbital period of the ellipse
//    - No fuel consumed during coast
//
// 3. SECOND BURN (dv2): At apoapsis of transfer orbit
//    - Fire engines prograde again
//    - Circularizes orbit at new altitude
//
// KEY EQUATIONS:
// -------------
// Vis-viva equation (velocity at any point in orbit):
//   v = sqrt(mu * (2/r - 1/a))
//
// where:
//   v  = orbital velocity [m/s]
//   mu = gravitational parameter (GM) [m^3/s^2]
//   r  = current radius from body center [m]
//   a  = semi-major axis of orbit [m]
//
// For circular orbit (a = r):
//   v_circular = sqrt(mu / r)
//
// Transfer orbit semi-major axis:
//   a_transfer = (r1 + r2) / 2
//
// Transfer time:
//   t = pi * sqrt(a^3 / mu)  (half the orbital period)
//
// REAL-WORLD EXAMPLES:
// -------------------
// - LEO to GEO: ~4.2 km/s total delta-v, ~5.2 hours transfer time
// - Earth to Mars (simplified): ~3.6 km/s from LEO, ~259 days transfer
// - Apollo to Moon: ~3.2 km/s trans-lunar injection, ~3 days transfer
//
// =============================================================================
// C# CONCEPTS IN THIS FILE
// =============================================================================
//
// 1. FILE-SCOPED NAMESPACE (C# 10+)
//    `namespace AerospaceDashboard.Services;`
//    No braces needed - entire file belongs to this namespace.
//    Reduces indentation and boilerplate.
//
// 2. RECORDS (C# 9+)
//    `public record TransferResult { ... }`
//    Records are immutable reference types with value-based equality.
//    Great for data transfer objects (DTOs) and calculation results.
//    Compiler generates: GetHashCode, Equals, ToString, deconstructor.
//
// 3. INIT-ONLY SETTERS
//    `public double DeltaV1 { get; init; }`
//    Properties can be set during object initialization but not after.
//    Enables object initializer syntax while maintaining immutability.
//
// 4. EXPRESSION-BODIED PROPERTIES
//    `public double TransferTimeHours => TransferTime / 3600.0;`
//    Read-only computed property with arrow syntax (no `return` keyword).
//    Clean syntax for simple calculations.
//
// 5. STATIC CLASSES
//    `public static class HohmannCalculator`
//    Cannot be instantiated; all members must be static.
//    Used for utility/helper functions that don't need instance state.
//
// 6. COLLECTION EXPRESSIONS (C# 12+ style dictionary initializer)
//    `new() { ["key"] = value, ... }`
//    Compact syntax for initializing Dictionary with indexer.
//
// =============================================================================

using System;
using System.Collections.Generic;
using AerospaceDashboard.Models;

namespace AerospaceDashboard.Services;

/// <summary>
/// Results of a Hohmann transfer calculation.
/// </summary>
/// <remarks>
/// <para>
/// RECORD TYPE: This is a C# record, which provides:
/// - Immutability (properties set once at creation)
/// - Value-based equality (two TransferResults with same values are Equal)
/// - Auto-generated ToString() for debugging
/// </para>
/// <para>
/// UNITS: All values use SI units (meters, seconds) internally.
/// Conversion properties are provided for convenience.
/// </para>
/// </remarks>
public record TransferResult
{
    // =========================================================================
    // PRIMARY RESULTS (stored values)
    // =========================================================================

    /// <summary>First burn delta-v [m/s] - raises apoapsis</summary>
    /// <remarks>
    /// AEROSPACE: This is the "injection burn" that kicks us onto the
    /// transfer ellipse. Always in prograde direction for raising orbit.
    /// </remarks>
    public double DeltaV1 { get; init; }

    /// <summary>Second burn delta-v [m/s] - circularizes orbit</summary>
    /// <remarks>
    /// AEROSPACE: This "circularization burn" completes the transfer.
    /// Fired at apoapsis of the transfer ellipse.
    /// </remarks>
    public double DeltaV2 { get; init; }

    /// <summary>Total delta-v required [m/s]</summary>
    /// <remarks>
    /// AEROSPACE: The "delta-v budget" - how much velocity change needed.
    /// Directly relates to propellant mass via the Tsiolkovsky equation:
    ///   dv = Isp * g0 * ln(m_wet / m_dry)
    /// </remarks>
    public double TotalDeltaV { get; init; }

    /// <summary>Transfer duration [seconds]</summary>
    /// <remarks>
    /// AEROSPACE: This is exactly half the orbital period of the transfer
    /// ellipse. For a LEO-to-GEO transfer, this is about 5.2 hours.
    /// </remarks>
    public double TransferTime { get; init; }

    /// <summary>Semi-major axis of transfer ellipse [m]</summary>
    /// <remarks>
    /// AEROSPACE: The semi-major axis determines the orbital period and
    /// energy. For a Hohmann transfer: a = (r1 + r2) / 2
    /// </remarks>
    public double SemiMajorAxis { get; init; }

    // =========================================================================
    // COMPUTED PROPERTIES (unit conversions)
    // =========================================================================
    // C# CONCEPT: Expression-bodied members using '=>' arrow syntax.
    // These are read-only properties that compute on access.
    // No backing field - value is calculated each time.

    /// <summary>Transfer time in hours (more human-readable)</summary>
    public double TransferTimeHours => TransferTime / 3600.0;

    /// <summary>Transfer time in days (for interplanetary transfers)</summary>
    public double TransferTimeDays => TransferTime / 86400.0;
}

/// <summary>
/// Calculates Hohmann transfer orbits between circular orbits.
/// </summary>
/// <remarks>
/// <para>
/// C# CONCEPT: Static Class
/// ------------------------
/// Marked with 'static' keyword, this class:
/// - Cannot be instantiated (no 'new HohmannCalculator()')
/// - All members must be static
/// - Perfect for stateless utility functions
/// - Similar to a namespace with functions (like in other languages)
/// </para>
/// <para>
/// AEROSPACE: This calculator assumes circular initial and final orbits.
/// For elliptical orbits, more complex bi-elliptic transfers may be
/// more efficient in some cases.
/// </para>
/// </remarks>
public static class HohmannCalculator
{
    /// <summary>
    /// Calculate Hohmann transfer between two circular orbits.
    /// </summary>
    /// <param name="initial">Starting circular orbit</param>
    /// <param name="final">Target circular orbit</param>
    /// <returns>Transfer parameters including delta-v and timing</returns>
    /// <remarks>
    /// ALGORITHM: Implements the classical Hohmann transfer equations.
    /// Works for both raising (r2 &gt; r1) and lowering (r2 &lt; r1) orbits.
    /// </remarks>
    public static TransferResult Calculate(Orbit initial, Orbit final)
    {
        // Extract orbital parameters
        double r1 = initial.Radius;  // Initial orbital radius [m]
        double r2 = final.Radius;    // Final orbital radius [m]
        double mu = initial.Body.GM; // Gravitational parameter [m^3/s^2]

        // =====================================================================
        // STEP 1: Calculate transfer orbit geometry
        // =====================================================================
        // The transfer ellipse is tangent to both orbits:
        // - Periapsis at r1 (if raising orbit) or r2 (if lowering)
        // - Apoapsis at r2 (if raising orbit) or r1 (if lowering)
        //
        // Semi-major axis is the average of the two radii:
        //   a = (r_periapsis + r_apoapsis) / 2 = (r1 + r2) / 2
        double aTransfer = (r1 + r2) / 2.0;

        // =====================================================================
        // STEP 2: Calculate circular orbit velocities
        // =====================================================================
        // For a circular orbit, velocity is constant and given by:
        //   v = sqrt(mu / r)
        //
        // This comes from the vis-viva equation with a = r (circular orbit):
        //   v^2 = mu * (2/r - 1/a) = mu * (2/r - 1/r) = mu/r
        double v1 = Math.Sqrt(mu / r1);  // Velocity in initial orbit [m/s]
        double v2 = Math.Sqrt(mu / r2);  // Velocity in final orbit [m/s]

        // =====================================================================
        // STEP 3: Calculate transfer orbit velocities using vis-viva equation
        // =====================================================================
        // VIS-VIVA EQUATION (the fundamental equation of orbital mechanics):
        //   v^2 = mu * (2/r - 1/a)
        //
        // This relates velocity (v) to position (r) and orbital shape (a).
        // Energy is conserved, so this works at any point in the orbit.

        // Velocity at periapsis of transfer ellipse (at r1)
        double vTransferPeriapsis = Math.Sqrt(mu * (2.0 / r1 - 1.0 / aTransfer));

        // Velocity at apoapsis of transfer ellipse (at r2)
        double vTransferApoapsis = Math.Sqrt(mu * (2.0 / r2 - 1.0 / aTransfer));

        // =====================================================================
        // STEP 4: Calculate delta-v for each burn
        // =====================================================================
        // Delta-v is the difference between current and required velocity.
        // Direction matters: raising orbit requires prograde burns (add speed).

        double dv1, dv2;
        if (r2 > r1)
        {
            // RAISING ORBIT: Both burns are prograde (add velocity)
            // Burn 1: Speed up from v1 to vTransferPeriapsis
            // Burn 2: Speed up from vTransferApoapsis to v2
            dv1 = vTransferPeriapsis - v1;
            dv2 = v2 - vTransferApoapsis;
        }
        else
        {
            // LOWERING ORBIT: Both burns are retrograde (subtract velocity)
            // Burn 1: Slow down from v1 to vTransferPeriapsis
            // Burn 2: Slow down from vTransferApoapsis to v2
            dv1 = v1 - vTransferPeriapsis;
            dv2 = vTransferApoapsis - v2;
        }

        // =====================================================================
        // STEP 5: Calculate transfer time
        // =====================================================================
        // Transfer time is half the orbital period of the transfer ellipse.
        //
        // Kepler's Third Law gives orbital period:
        //   T = 2 * pi * sqrt(a^3 / mu)
        //
        // So half-period (transfer time) is:
        //   t = pi * sqrt(a^3 / mu)
        double transferTime = Math.PI * Math.Sqrt(Math.Pow(aTransfer, 3) / mu);

        // =====================================================================
        // STEP 6: Package results
        // =====================================================================
        // C# CONCEPT: Object Initializer Syntax
        // Instead of a constructor with many parameters, we can initialize
        // properties inline. The 'init' keyword allows this but prevents
        // later modification.
        return new TransferResult
        {
            DeltaV1 = Math.Abs(dv1),
            DeltaV2 = Math.Abs(dv2),
            TotalDeltaV = Math.Abs(dv1) + Math.Abs(dv2),
            TransferTime = transferTime,
            SemiMajorAxis = aTransfer
        };
    }

    /// <summary>
    /// Calculate transfer from altitude to altitude (defaults to Earth).
    /// </summary>
    /// <param name="initialAltitude">Starting altitude above surface [m]</param>
    /// <param name="finalAltitude">Target altitude above surface [m]</param>
    /// <returns>Transfer parameters</returns>
    /// <remarks>
    /// AEROSPACE: Altitude is measured from the surface; radius from the
    /// body's center. Radius = Body.Radius + Altitude.
    ///
    /// C# CONCEPT: Method Overloading
    /// This method has the same name but different parameters than the
    /// other Calculate method. The compiler chooses the right one based
    /// on the argument types.
    /// </remarks>
    public static TransferResult Calculate(double initialAltitude, double finalAltitude)
    {
        // Create orbit objects from altitudes (assumes Earth)
        var initial = Orbit.FromAltitude(CelestialBody.Earth, initialAltitude);
        var final = Orbit.FromAltitude(CelestialBody.Earth, finalAltitude);
        return Calculate(initial, final);
    }

    /// <summary>
    /// Common Earth orbit presets for quick selection in UI.
    /// </summary>
    /// <remarks>
    /// <para>
    /// AEROSPACE: Common orbital altitudes and their purposes:
    /// - LEO (400 km): ISS, crew missions, Earth observation
    /// - ISS (420 km): International Space Station's actual altitude
    /// - GPS (20,200 km): Navigation satellite constellation (12-hour period)
    /// - GEO (35,786 km): Geostationary orbit (24-hour period, stays over one spot)
    /// - Lunar Transfer: Earth-Moon distance (not an orbit, but common target)
    /// </para>
    /// <para>
    /// C# CONCEPT: Expression-bodied property returning a new Dictionary.
    /// The `new()` syntax infers the Dictionary type from the property type.
    /// Indexer initializer `["key"] = value` is cleaner than `{ "key", value }`.
    /// </para>
    /// </remarks>
    public static Dictionary<string, double> CommonOrbits => new()
    {
        ["LEO (400 km)"] = 400e3,
        ["ISS (420 km)"] = 420e3,
        ["GPS (20,200 km)"] = 20200e3,
        ["GEO (35,786 km)"] = 35786e3,
        ["Lunar Transfer"] = 384400e3  // Earth-Moon distance (km)
    };
}
