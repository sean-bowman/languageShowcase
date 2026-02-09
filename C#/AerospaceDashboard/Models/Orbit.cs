// =============================================================================
// CIRCULAR ORBIT MODEL
// =============================================================================
//
// AEROSPACE CONCEPT: Orbital Mechanics Basics
// ============================================
//
// An orbit is the curved path of an object around a celestial body due to
// gravity. Circular orbits are the simplest case: constant altitude and
// constant velocity.
//
// KEY ORBITAL PARAMETERS:
// ----------------------
//
//                         Apoapsis (highest point)
//                              *
//                          .--' '--.
//                       .-'    |    '-.
//                     ,'       |       `.
//                    /    Semi-major     \
//                   |      axis (a)       |
//    Periapsis ----*---------*------------*
//   (lowest point) |     Center          |
//                   |                     |
//                    \                   /
//                     `.               ,'
//                       '-.         .-'
//                          '--. .--'
//                              *
//
// For circular orbits: periapsis = apoapsis = radius (r)
//                      semi-major axis (a) = r
//
// RADIUS vs ALTITUDE:
// ------------------
// - Radius: Distance from body CENTER (used in equations)
// - Altitude: Distance from body SURFACE (what humans care about)
//
//   Radius = Body.Radius + Altitude
//
// For Earth:
//   ISS at 420 km altitude has radius = 6371 + 420 = 6791 km from center
//
// COMMON EARTH ORBITS:
// -------------------
// | Type | Altitude | Period | Use |
// |------|----------|--------|-----|
// | LEO  | 200-2000 km | 90-127 min | ISS, Earth obs |
// | MEO  | 2000-35786 km | 2-24 hr | GPS, navigation |
// | GEO  | 35,786 km | 24 hr | Communications |
//
// =============================================================================
// C# CONCEPTS IN THIS FILE
// =============================================================================
//
// 1. STATIC FACTORY METHOD
//    `public static Orbit FromAltitude(...)`
//    An alternative to constructors for creating objects.
//    Can have descriptive names and perform validation.
//
// 2. PATTERN MATCHING WITH NULLABLE
//    `if (body.Radius is null)`
//    The `is null` pattern is cleaner than `== null` for nullables.
//
// 3. EXPRESSION-BODIED PROPERTIES
//    `public double Velocity => Body.CircularVelocity(Radius);`
//    Computed properties that delegate to methods.
//    Calculated on each access (not cached).
//
// 4. NULL-CONDITIONAL WITH TERNARY
//    `Body.Radius.HasValue ? ... : null`
//    Checks if nullable has value, returns computed result or null.
//
// =============================================================================

using System;

namespace AerospaceDashboard.Models;

/// <summary>
/// Represents a circular orbit around a celestial body.
/// </summary>
/// <remarks>
/// <para>
/// AEROSPACE: Circular orbits are the simplest orbital type. The spacecraft
/// maintains constant altitude and constant velocity. Real orbits are often
/// slightly elliptical, but circular is a useful approximation.
/// </para>
/// <para>
/// C# CONCEPT: Immutable Model Class
/// Properties are set at construction and cannot be changed.
/// Computed properties derive values from the primary data.
/// </para>
/// </remarks>
public class Orbit
{
    // =========================================================================
    // PRIMARY PROPERTIES (stored data)
    // =========================================================================

    /// <summary>The celestial body being orbited</summary>
    public CelestialBody Body { get; }

    /// <summary>Orbital radius from body CENTER [m]</summary>
    /// <remarks>
    /// AEROSPACE: NOT the same as altitude!
    ///   Radius = Body.Radius + Altitude
    /// All orbital equations use radius, not altitude.
    /// </remarks>
    public double Radius { get; }

    // =========================================================================
    // CONSTRUCTOR
    // =========================================================================

    /// <summary>
    /// Create an orbit with a given radius.
    /// </summary>
    /// <param name="body">The celestial body to orbit</param>
    /// <param name="radius">Orbital radius from body center [m]</param>
    public Orbit(CelestialBody body, double radius)
    {
        Body = body;
        Radius = radius;
    }

    // =========================================================================
    // STATIC FACTORY METHOD
    // =========================================================================

    /// <summary>
    /// Create an orbit from altitude above the body's surface.
    /// </summary>
    /// <param name="body">The celestial body to orbit</param>
    /// <param name="altitude">Altitude above surface [m]</param>
    /// <returns>An orbit at the specified altitude</returns>
    /// <exception cref="ArgumentException">If body has no defined radius</exception>
    /// <remarks>
    /// <para>
    /// AEROSPACE: Altitude is how we typically think about orbits
    /// ("the ISS is at 420 km altitude"), but equations need radius.
    /// This factory method handles the conversion.
    /// </para>
    /// <para>
    /// C# CONCEPT: Static Factory Method
    /// - Named "FromAltitude" to describe what input is expected
    /// - Can validate preconditions before creating object
    /// - Returns null or throws if invalid (we throw here)
    ///
    /// Compare to constructor which just takes radius.
    /// </para>
    /// </remarks>
    public static Orbit FromAltitude(CelestialBody body, double altitude)
    {
        // C# CONCEPT: Pattern Matching with 'is null'
        // Cleaner and more readable than body.Radius == null
        if (body.Radius is null)
            throw new ArgumentException("Body has no defined radius");

        // Convert altitude to radius
        // C# CONCEPT: .Value accessor for nullable
        // We already checked it's not null, so .Value is safe
        return new Orbit(body, body.Radius.Value + altitude);
    }

    // =========================================================================
    // COMPUTED PROPERTIES (derived data)
    // =========================================================================
    // C# CONCEPT: Expression-Bodied Properties
    // These are read-only properties with an arrow (=>) instead of braces.
    // The expression is evaluated each time the property is accessed.
    // No backing field - computed on demand.

    /// <summary>
    /// Get altitude above surface, if body has a defined radius.
    /// </summary>
    /// <remarks>
    /// C# CONCEPT: Nullable Return with Ternary
    /// Returns null if we can't compute altitude (body has no radius).
    /// This is more expressive than throwing an exception.
    /// </remarks>
    public double? Altitude => Body.Radius.HasValue ? Radius - Body.Radius.Value : null;

    /// <summary>Orbital velocity [m/s]</summary>
    /// <remarks>
    /// AEROSPACE: For circular orbits: v = sqrt(mu / r)
    /// Delegates to CelestialBody.CircularVelocity for the calculation.
    /// </remarks>
    public double Velocity => Body.CircularVelocity(Radius);

    /// <summary>Orbital period [seconds]</summary>
    /// <remarks>
    /// AEROSPACE: Time for one complete orbit.
    /// From Kepler's Third Law: T = 2 * pi * sqrt(r^3 / mu)
    /// </remarks>
    public double Period => Body.OrbitalPeriod(Radius);

    /// <summary>Orbital period in hours (more human-readable)</summary>
    public double PeriodHours => Period / 3600.0;

    // =========================================================================
    // COMMON ORBIT PRESETS
    // =========================================================================
    // C# CONCEPT: Static Factory Properties
    // Pre-configured orbits for common use cases.
    // Each access creates a new Orbit instance.

    /// <summary>Low Earth Orbit at typical altitude (400 km)</summary>
    /// <remarks>
    /// AEROSPACE: LEO is 200-2000 km altitude. 400 km is typical for
    /// crewed spacecraft (ISS is at ~420 km). Period is about 93 minutes.
    /// </remarks>
    public static Orbit LEO => FromAltitude(CelestialBody.Earth, 400e3);

    /// <summary>Geostationary Earth Orbit (35,786 km)</summary>
    /// <remarks>
    /// AEROSPACE: At exactly 35,786 km altitude, orbital period = 24 hours.
    /// Satellite appears stationary from ground. Used for communications,
    /// weather satellites. Must be over equator.
    /// </remarks>
    public static Orbit GEO => FromAltitude(CelestialBody.Earth, 35786e3);

    /// <summary>GPS satellite orbit (20,200 km)</summary>
    /// <remarks>
    /// AEROSPACE: GPS satellites orbit at 20,200 km with 12-hour period.
    /// This means each satellite passes over the same ground tracks daily.
    /// The constellation has 31 active satellites in 6 orbital planes.
    /// </remarks>
    public static Orbit GPS => FromAltitude(CelestialBody.Earth, 20200e3);
}
