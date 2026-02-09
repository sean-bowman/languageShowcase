// =============================================================================
// ATMOSPHERIC MODELS
// =============================================================================
//
// AEROSPACE CONCEPT: Atmospheric Properties and Layers
// =====================================================
//
// Earth's atmosphere is divided into distinct layers based on temperature
// behavior. Each layer has different characteristics important for flight.
//
// ATMOSPHERIC STRUCTURE:
// ---------------------
//
//   Altitude (km)
//        |
//    100 + - - - - - - - - - - - - - - - Karman Line (edge of space)
//        |   Thermosphere (T rises dramatically)
//     85 + - - - - - - - - - - - - - - - Mesopause
//        |   Mesosphere (T falls)
//     50 + - - - - - - - - - - - - - - - Stratopause
//        |   Stratosphere (T rises - ozone heating)
//     20 + - - - - - - - - - - - - - - - Tropopause
//        |   *** TROPOPAUSE (T constant) ***
//     11 + - - - - - - - - - - - - - - - Commercial cruise altitude
//        |   Troposphere (T falls 6.5 K/km)
//        |   Weather, clouds, most aircraft
//      0 +-------------------------------+ Sea level
//        150   200   250   300   Temperature (K)
//
// WHY IT MATTERS FOR AVIATION:
// ---------------------------
// 1. AIR DENSITY: Affects lift, drag, and engine thrust
//    - rho = P / (R * T) from ideal gas law
//    - At 35,000 ft: only 31% of sea level density
//
// 2. TEMPERATURE: Affects engine efficiency and speed of sound
//    - Jet engines more efficient in cold air
//    - Mach number = TAS / sqrt(gamma * R * T)
//
// 3. PRESSURE: Used by altimeters for altitude measurement
//    - All aircraft altimeters are just barometers
//    - Must be calibrated to local pressure
//
// =============================================================================
// C# CONCEPTS IN THIS FILE
// =============================================================================
//
// 1. RECORDS (C# 9+)
//    `public record AtmosphericProperties { ... }`
//    Records are immutable reference types with value equality.
//    Perfect for data transfer objects (DTOs).
//
// 2. INIT-ONLY SETTERS
//    `public double Temperature { get; init; }`
//    Can be set during object initialization but not after.
//    Combines mutability (at creation) with immutability (after).
//
// 3. EXPRESSION-BODIED PROPERTIES
//    `public double TemperatureCelsius => Temperature - 273.15;`
//    Computed properties with arrow syntax. Read-only.
//
// 4. STRING PROPERTY WITH DEFAULT
//    `public string Name { get; init; } = string.Empty;`
//    Non-nullable string with empty default avoids null issues.
//
// 5. MULTIPLE TYPES IN ONE FILE
//    C# allows multiple types per file. Related records are often
//    grouped together for convenience.
//
// =============================================================================

namespace AerospaceDashboard.Models;

/// <summary>
/// Atmospheric properties at a specific altitude.
/// </summary>
/// <remarks>
/// <para>
/// AEROSPACE: These are the key properties pilots and engineers care about:
/// - Temperature: affects engine performance and speed of sound
/// - Pressure: used by altimeters, affects aerodynamics
/// - Density: the "real" property that determines lift and drag
/// - Speed of Sound: reference for Mach number
/// </para>
/// <para>
/// C# CONCEPT: Record Type
/// Records are reference types with:
/// - Value-based equality (two records with same properties are Equal)
/// - Immutability by default
/// - Auto-generated ToString, GetHashCode, Equals
/// - Built-in deconstruction support
///
/// Perfect for "data holder" types like this.
/// </para>
/// </remarks>
public record AtmosphericProperties
{
    // =========================================================================
    // PRIMARY PROPERTIES (stored in SI units)
    // =========================================================================
    // C# CONCEPT: Init-Only Setters
    // The 'init' accessor allows setting during object initialization:
    //   new AtmosphericProperties { Temperature = 288.15 }
    // But prevents modification afterward:
    //   props.Temperature = 300; // Compiler error!

    /// <summary>Geometric altitude [m]</summary>
    /// <remarks>
    /// AEROSPACE: Height above mean sea level.
    /// Different from pressure altitude (what altimeters show).
    /// </remarks>
    public double Altitude { get; init; }

    /// <summary>Static air temperature [K]</summary>
    /// <remarks>
    /// AEROSPACE: The ambient temperature of still air.
    /// "Static" means not affected by motion (vs total temperature
    /// which includes kinetic heating at high speeds).
    /// </remarks>
    public double Temperature { get; init; }

    /// <summary>Static air pressure [Pa]</summary>
    /// <remarks>
    /// AEROSPACE: Ambient atmospheric pressure.
    /// Decreases roughly exponentially with altitude.
    /// At 18 km: half sea level. At 5.5 km: half sea level.
    /// </remarks>
    public double Pressure { get; init; }

    /// <summary>Air density [kg/m^3]</summary>
    /// <remarks>
    /// AEROSPACE: This is THE property for aerodynamics.
    /// Lift = 0.5 * rho * V^2 * S * Cl
    /// Drag = 0.5 * rho * V^2 * S * Cd
    /// </remarks>
    public double Density { get; init; }

    /// <summary>Speed of sound [m/s]</summary>
    /// <remarks>
    /// AEROSPACE: a = sqrt(gamma * R * T)
    /// Depends ONLY on temperature, not pressure!
    /// At sea level: 340 m/s. At cruise (11 km): 295 m/s.
    /// </remarks>
    public double SpeedOfSound { get; init; }

    // =========================================================================
    // COMPUTED PROPERTIES (unit conversions for display)
    // =========================================================================
    // C# CONCEPT: Expression-Bodied Properties
    // Read-only properties with arrow syntax.
    // Value is computed each time the property is accessed.

    /// <summary>Temperature in Celsius (for human display)</summary>
    /// <remarks>
    /// Conversion: Celsius = Kelvin - 273.15
    /// Sea level ISA: 288.15 K = 15 deg C
    /// Tropopause: 216.65 K = -56.5 deg C
    /// </remarks>
    public double TemperatureCelsius => Temperature - 273.15;

    /// <summary>Pressure in kilopascals (more readable than Pa)</summary>
    /// <remarks>
    /// Sea level: 101.325 kPa
    /// Cruise altitude: ~23 kPa
    /// </remarks>
    public double PressureKPa => Pressure / 1000.0;

    /// <summary>Altitude in kilometers (more readable than meters)</summary>
    public double AltitudeKm => Altitude / 1000.0;
}

/// <summary>
/// Defines an atmospheric layer with its properties.
/// </summary>
/// <remarks>
/// <para>
/// AEROSPACE: The atmosphere is divided into layers based on temperature
/// behavior. Each layer has a characteristic temperature gradient (lapse rate).
/// </para>
/// <para>
/// STANDARD ATMOSPHERE LAYERS:
/// - Troposphere (0-11 km): T falls 6.5 K/km. Weather happens here.
/// - Tropopause (11-20 km): T constant. Jets cruise here.
/// - Stratosphere (20-50 km): T rises. Ozone layer lives here.
/// - Mesosphere (50-85 km): T falls again.
/// - Thermosphere (85+ km): T rises dramatically. ISS orbits here.
/// </para>
/// </remarks>
public record AtmosphericLayer
{
    /// <summary>Layer name (e.g., "Troposphere")</summary>
    /// <remarks>
    /// C# CONCEPT: String with Default Value
    /// Using `= string.Empty` ensures the property is never null.
    /// This is a common pattern to avoid null-reference exceptions.
    /// </remarks>
    public string Name { get; init; } = string.Empty;

    /// <summary>Altitude at bottom of layer [m]</summary>
    public double BaseAltitude { get; init; }

    /// <summary>Altitude at top of layer [m]</summary>
    public double TopAltitude { get; init; }

    /// <summary>Temperature lapse rate [K/m]</summary>
    /// <remarks>
    /// AEROSPACE: Lapse rate is dT/dh (temperature change per altitude).
    /// - Negative: temperature decreases with altitude (troposphere)
    /// - Zero: isothermal (tropopause)
    /// - Positive: temperature increases with altitude (stratosphere)
    ///
    /// Troposphere lapse rate of -0.0065 K/m = -6.5 deg C per km
    /// </remarks>
    public double LapseRate { get; init; }

    /// <summary>Temperature at base of layer [K]</summary>
    public double BaseTemperature { get; init; }
}
