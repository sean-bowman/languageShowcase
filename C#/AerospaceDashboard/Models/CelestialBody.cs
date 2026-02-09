// =============================================================================
// CELESTIAL BODY MODEL
// =============================================================================
//
// AEROSPACE CONCEPT: The Gravitational Parameter (mu = GM)
// ========================================================
//
// In orbital mechanics, we almost never use mass (M) and the gravitational
// constant (G) separately. Instead, we use the gravitational parameter:
//
//   mu = G * M  [m^3/s^2]
//
// Why? Several reasons:
//
// 1. PRECISION: We can measure mu very precisely by tracking satellites,
//    but G and M individually are much harder to measure.
//
//    Earth's mu is known to 9 significant figures: 3.986004418 x 10^14
//    But G is only known to 5 figures: 6.674 x 10^-11
//
// 2. CONVENIENCE: All orbital equations use mu, not G and M separately.
//
//   Circular velocity:  v = sqrt(mu / r)
//   Orbital period:     T = 2 * pi * sqrt(r^3 / mu)
//   Vis-viva equation:  v^2 = mu * (2/r - 1/a)
//   Escape velocity:    v_esc = sqrt(2 * mu / r)
//
// GRAVITATIONAL PARAMETERS:
// ------------------------
// | Body   | mu (m^3/s^2)        | Mass (kg)           |
// |--------|---------------------|---------------------|
// | Sun    | 1.327 x 10^20       | 1.989 x 10^30       |
// | Earth  | 3.986 x 10^14       | 5.972 x 10^24       |
// | Moon   | 4.905 x 10^12       | 7.342 x 10^22       |
// | Mars   | 4.283 x 10^13       | 6.417 x 10^23       |
//
// =============================================================================
// C# CONCEPTS IN THIS FILE
// =============================================================================
//
// 1. IMMUTABLE PROPERTIES (get-only)
//    `public string Name { get; }`
//    Properties with only a getter cannot be changed after construction.
//    This makes the class effectively immutable (thread-safe, predictable).
//
// 2. NULLABLE VALUE TYPES
//    `public double? Radius { get; }`
//    The `?` makes double nullable. Null means "no value" (some bodies
//    might not have a defined radius, like barycenters).
//
// 3. OPTIONAL PARAMETERS
//    `double? radius = null`
//    The radius parameter has a default value, making it optional.
//
// 4. STATIC FACTORY PROPERTIES
//    `public static CelestialBody Earth => new(...)`
//    Static properties that return pre-configured instances.
//    Cleaner than having many static fields.
//
// 5. PRIMARY CONSTRUCTOR PATTERN (manual)
//    The class has a single constructor that sets all properties.
//    C# 12 has primary constructors that make this more concise.
//
// =============================================================================

using System;

namespace AerospaceDashboard.Models;

/// <summary>
/// Represents a celestial body with gravitational properties.
/// </summary>
/// <remarks>
/// <para>
/// AEROSPACE: A celestial body is any naturally occurring object in space:
/// planets, moons, stars, asteroids. For orbital mechanics, the key property
/// is the gravitational parameter (mu = GM), not mass alone.
/// </para>
/// <para>
/// C# CONCEPT: Immutable Class
/// All properties are get-only, set via constructor. Once created, the
/// object cannot be modified. This is safer for multi-threaded code.
/// </para>
/// </remarks>
public class CelestialBody
{
    // =========================================================================
    // PROPERTIES
    // =========================================================================

    /// <summary>Human-readable name of the body</summary>
    public string Name { get; }

    /// <summary>Gravitational parameter (mu = G*M) [m^3/s^2]</summary>
    /// <remarks>
    /// AEROSPACE: This is THE key parameter for orbital calculations.
    /// More precise than using G and M separately.
    /// </remarks>
    public double GM { get; }

    /// <summary>Mean radius of the body [m], or null if not applicable</summary>
    /// <remarks>
    /// C# CONCEPT: Nullable Value Type (double?)
    /// The ? makes this a nullable type. null means "not defined".
    /// Some celestial bodies (like barycenters) don't have a radius.
    /// </remarks>
    public double? Radius { get; }

    // =========================================================================
    // CONSTRUCTOR
    // =========================================================================

    /// <summary>
    /// Create a new celestial body.
    /// </summary>
    /// <param name="name">Display name</param>
    /// <param name="gm">Gravitational parameter [m^3/s^2]</param>
    /// <param name="radius">Optional mean radius [m]</param>
    /// <remarks>
    /// C# CONCEPT: Optional Parameter
    /// The `radius = null` makes this parameter optional.
    /// Callers can omit it: `new CelestialBody("Sun", 1.3e20)`
    /// </remarks>
    public CelestialBody(string name, double gm, double? radius = null)
    {
        Name = name;
        GM = gm;
        Radius = radius;
    }

    // =========================================================================
    // ORBITAL CALCULATIONS
    // =========================================================================

    /// <summary>
    /// Calculate circular orbital velocity at a given radius.
    /// </summary>
    /// <param name="orbitalRadius">Distance from body center [m]</param>
    /// <returns>Orbital velocity [m/s]</returns>
    /// <remarks>
    /// <para>
    /// AEROSPACE: The vis-viva equation for circular orbits (a = r):
    ///   v = sqrt(mu / r)
    /// </para>
    /// <para>
    /// Example velocities around Earth:
    /// - LEO (400 km):    7.67 km/s
    /// - GPS (20,200 km): 3.87 km/s
    /// - GEO (35,786 km): 3.07 km/s
    /// - Moon (384,400 km): 1.02 km/s
    ///
    /// Notice: Higher orbits are SLOWER. This is counterintuitive!
    /// </para>
    /// </remarks>
    public double CircularVelocity(double orbitalRadius)
    {
        // v = sqrt(mu / r)
        // From vis-viva with a = r (circular orbit)
        return Math.Sqrt(GM / orbitalRadius);
    }

    /// <summary>
    /// Calculate orbital period for a circular orbit.
    /// </summary>
    /// <param name="orbitalRadius">Distance from body center [m]</param>
    /// <returns>Orbital period [seconds]</returns>
    /// <remarks>
    /// <para>
    /// AEROSPACE: Kepler's Third Law:
    ///   T = 2 * pi * sqrt(r^3 / mu)
    /// </para>
    /// <para>
    /// Example periods around Earth:
    /// - ISS (420 km):    92 minutes
    /// - GPS (20,200 km): 12 hours (exactly half a day - intentional!)
    /// - GEO (35,786 km): 24 hours (appears stationary from ground)
    /// - Moon:           27.3 days
    /// </para>
    /// </remarks>
    public double OrbitalPeriod(double orbitalRadius)
    {
        // T = 2 * pi * sqrt(r^3 / mu)
        // Kepler's Third Law derived from Newtonian gravity
        return 2 * Math.PI * Math.Sqrt(Math.Pow(orbitalRadius, 3) / GM);
    }

    // =========================================================================
    // PRE-DEFINED BODIES
    // =========================================================================
    // C# CONCEPT: Static Factory Properties
    // These are expression-bodied properties that create new instances.
    // Each access creates a new object (unlike static fields which are shared).
    // Trade-off: simpler code vs. slightly more allocations.

    /// <summary>Earth with standard gravitational parameter</summary>
    /// <remarks>
    /// AEROSPACE: Earth's mu is THE most precisely known value in astrodynamics.
    /// Measured by tracking thousands of satellites over decades.
    /// </remarks>
    public static CelestialBody Earth => new("Earth", 3.986004418e14, 6.371e6);

    /// <summary>Earth's Moon</summary>
    /// <remarks>
    /// AEROSPACE: The Moon's mu is about 1/81 of Earth's.
    /// Lunar orbit is complex due to Earth-Sun perturbations.
    /// </remarks>
    public static CelestialBody Moon => new("Moon", 4.9048695e12, 1.7374e6);

    /// <summary>Mars (the Red Planet)</summary>
    /// <remarks>
    /// AEROSPACE: Mars has about 1/10 of Earth's mu.
    /// Popular destination for robotic and future human missions.
    /// </remarks>
    public static CelestialBody Mars => new("Mars", 4.282837e13, 3.3895e6);

    /// <summary>The Sun (our star)</summary>
    /// <remarks>
    /// AEROSPACE: The Sun contains 99.86% of the Solar System's mass.
    /// Its mu dominates interplanetary trajectory calculations.
    /// </remarks>
    public static CelestialBody Sun => new("Sun", 1.32712440018e20, 6.9634e8);
}
