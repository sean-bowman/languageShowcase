// =============================================================================
// INTERNATIONAL STANDARD ATMOSPHERE (ISA) CALCULATOR
// =============================================================================
//
// AEROSPACE CONCEPT: The Standard Atmosphere Model
// =================================================
//
// The International Standard Atmosphere (ISA) is a model of how Earth's
// atmospheric properties vary with altitude. It provides reference values
// for aircraft performance, altimeter calibration, and engineering.
//
// Based on ISO 2533:1975 and ICAO standard atmosphere.
//
// WHY WE NEED A STANDARD:
// ----------------------
// Real atmospheric conditions vary by weather, location, and season.
// But aircraft performance specs and regulations need consistent values.
// "Standard day" conditions let us compare apples to apples.
//
// ATMOSPHERIC LAYERS:
// ------------------
//
//     50 km +-----------------------------------------+
//           |  Stratosphere (upper)                   |
//           |  Temperature RISES with altitude        |
//           |  Lapse rate: +1.0 K/km                  |
//     32 km +-----------------------------------------+
//           |  Stratosphere (lower)                   |
//           |  Temperature RISES with altitude        |
//           |  Lapse rate: +1.0 K/km                  |
//     20 km +-----------------------------------------+
//           |  Tropopause (isothermal layer)          |
//           |  Temperature CONSTANT: 216.65 K (-56.5C)|
//           |  Lapse rate: 0 K/km                     |
//     11 km +-----------------------------------------+ <- Typical cruise alt
//           |  Troposphere                            |
//           |  Temperature FALLS with altitude        |
//           |  Lapse rate: -6.5 K/km                  |
//           |  Where weather happens                  |
//      0 km +-----------------------------------------+ Sea Level
//           T = 288.15 K (15 C), P = 101325 Pa
//
// KEY EQUATIONS:
// -------------
//
// TROPOSPHERE (h <= 11 km, temperature gradient layer):
//   T(h) = T0 + L * h
//   P(h) = P0 * (T/T0)^(-g0/(L*R))
//
// TROPOPAUSE (11-20 km, isothermal layer):
//   T(h) = 216.65 K (constant)
//   P(h) = P11 * exp(-g0 * (h - h11) / (R * T))
//
// where:
//   T = temperature [K]
//   P = pressure [Pa]
//   h = altitude [m]
//   L = lapse rate [K/m] (negative for cooling)
//   R = specific gas constant for air = 287.05 J/(kg*K)
//   g0 = standard gravity = 9.80665 m/s^2
//
// DENSITY from ideal gas law:
//   rho = P / (R * T)
//
// SPEED OF SOUND:
//   a = sqrt(gamma * R * T)
//   gamma = 1.4 for air (ratio of specific heats)
//
// AEROSPACE APPLICATIONS:
// ----------------------
// - Altimetry: Altimeters measure pressure and convert to altitude using ISA
// - Performance: Engine thrust and wing lift depend on density
// - Flight planning: Fuel burn varies with altitude (cruise at optimum)
// - Mach number: True airspeed / speed of sound (varies with altitude!)
//
// =============================================================================
// C# CONCEPTS IN THIS FILE
// =============================================================================
//
// 1. CONST FIELDS
//    `public const double T0 = 288.15;`
//    Compile-time constants. Value is substituted at compile time.
//    More efficient than readonly, but must be known at compile time.
//
// 2. PRIVATE CONST FIELDS
//    `private const double H_Troposphere = 11000.0;`
//    Internal implementation details hidden from consumers.
//    Encapsulation: users don't need to know layer boundaries.
//
// 3. EXPRESSION-BODIED METHODS
//    For single-line methods, you can use arrow syntax:
//    `public static double Density(double alt) => P / (R * T);`
//    We use full method bodies here for clarity with complex logic.
//
// 4. XML DOCUMENTATION COMMENTS
//    /// <summary>, /// <param>, /// <returns>
//    These generate IntelliSense tooltips and API documentation.
//    Best practice for public APIs.
//
// =============================================================================

using System;
using System.Collections.Generic;
using AerospaceDashboard.Models;

namespace AerospaceDashboard.Services;

/// <summary>
/// International Standard Atmosphere (ISA) Calculator.
/// </summary>
/// <remarks>
/// <para>
/// Implements the ISA model per ISO 2533:1975.
/// Valid from sea level to approximately 50 km altitude.
/// </para>
/// <para>
/// C# CONCEPT: Static Class with Constants
/// This class serves as a namespace for related constants and functions.
/// All public constants are accessible as IsaCalculator.T0, etc.
/// </para>
/// </remarks>
public static class IsaCalculator
{
    // =========================================================================
    // SEA LEVEL REFERENCE VALUES
    // =========================================================================
    // C# CONCEPT: Public Constants
    // 'const' values are implicitly static and must be compile-time constants.
    // The value is literally substituted into calling code at compile time.
    // Use 'readonly' if value needs to be computed at runtime.

    /// <summary>Sea level temperature [K] = 15 deg C</summary>
    /// <remarks>
    /// AEROSPACE: "Standard day" temperature. Actual temps vary hugely:
    /// - Hot day (ISA+20): 35 deg C - engines produce less thrust
    /// - Cold day (ISA-20): -5 deg C - better performance but ice risk
    /// </remarks>
    public const double T0 = 288.15;

    /// <summary>Sea level pressure [Pa] = 1013.25 hPa = 29.92 inHg</summary>
    /// <remarks>
    /// AEROSPACE: The "standard atmosphere" reference pressure.
    /// Pilots set altimeter to local pressure for accurate altitude.
    /// "Altimeter 29.92" means set to standard pressure.
    /// </remarks>
    public const double P0 = 101325.0;

    /// <summary>Sea level density [kg/m^3]</summary>
    /// <remarks>
    /// AEROSPACE: Density directly affects lift and drag:
    ///   Lift = 0.5 * rho * V^2 * S * Cl
    /// Lower density at altitude = less drag = fuel efficient cruise.
    /// </remarks>
    public const double Rho0 = 1.225;

    // =========================================================================
    // PHYSICAL CONSTANTS
    // =========================================================================

    /// <summary>Specific gas constant for dry air [J/(kg*K)]</summary>
    /// <remarks>
    /// R = R_universal / M_air = 8314.46 / 28.9647 = 287.05 J/(kg*K)
    /// Used in ideal gas law: P = rho * R * T
    /// </remarks>
    public const double R = 287.05287;

    /// <summary>Ratio of specific heats (cp/cv) for air</summary>
    /// <remarks>
    /// AEROSPACE: gamma determines how air behaves in compression.
    /// For diatomic gases like N2 and O2: gamma = 7/5 = 1.4
    /// Appears in speed of sound: a = sqrt(gamma * R * T)
    /// </remarks>
    public const double Gamma = 1.4;

    /// <summary>Standard gravity [m/s^2]</summary>
    /// <remarks>
    /// The official value used in aerospace calculations.
    /// Actual gravity varies with latitude and altitude.
    /// </remarks>
    public const double G0 = 9.80665;

    // =========================================================================
    // ATMOSPHERIC LAYER BOUNDARIES
    // =========================================================================
    // C# CONCEPT: Private Constants
    // Implementation details that users don't need to access.
    // Encapsulation: we could change layer handling without breaking API.

    /// <summary>Top of troposphere [m]</summary>
    private const double H_Troposphere = 11000.0;

    /// <summary>Top of tropopause (isothermal layer) [m]</summary>
    private const double H_Tropopause = 20000.0;

    /// <summary>Top of lower stratosphere [m]</summary>
    private const double H_Stratosphere1 = 32000.0;

    // =========================================================================
    // TEMPERATURE LAPSE RATES
    // =========================================================================
    // Lapse rate = change in temperature per meter of altitude [K/m]
    // Negative = cooling with altitude (troposphere)
    // Positive = warming with altitude (stratosphere)
    // Zero = isothermal (tropopause)

    /// <summary>Troposphere lapse rate [K/m] (-6.5 deg C per km)</summary>
    private const double L_Troposphere = -0.0065;

    /// <summary>Lower stratosphere lapse rate [K/m] (+1.0 deg C per km)</summary>
    private const double L_Stratosphere1 = 0.001;

    // =========================================================================
    // LAYER BOUNDARY VALUES (pre-computed for efficiency)
    // =========================================================================

    /// <summary>Temperature at 11 km (tropopause base) [K]</summary>
    private const double T11 = 216.65;  // -56.5 deg C

    /// <summary>Temperature at 20 km [K]</summary>
    private const double T20 = 216.65;  // Still -56.5 deg C (isothermal layer)

    /// <summary>Pressure at 11 km [Pa]</summary>
    private const double P11 = 22632.06;  // About 22% of sea level

    /// <summary>Pressure at 20 km [Pa]</summary>
    private const double P20 = 5474.889;  // About 5% of sea level

    // =========================================================================
    // TEMPERATURE CALCULATION
    // =========================================================================

    /// <summary>
    /// Calculate atmospheric temperature at a given altitude.
    /// </summary>
    /// <param name="altitude">Geometric altitude above sea level [m]</param>
    /// <returns>Static air temperature [K]</returns>
    /// <remarks>
    /// <para>
    /// AEROSPACE: Temperature determines air density (via ideal gas law)
    /// and speed of sound. Pilots fly at altitude where temperature is
    /// cold enough for efficient engines but warm enough to avoid icing.
    /// </para>
    /// <para>
    /// The temperature profile is piecewise linear:
    /// - Troposphere: T decreases 6.5 K per km (weather layer)
    /// - Tropopause: T constant at 216.65 K (jets cruise here)
    /// - Stratosphere: T increases 1 K per km (ozone heating)
    /// </para>
    /// </remarks>
    public static double Temperature(double altitude)
    {
        if (altitude <= H_Troposphere)
        {
            // TROPOSPHERE: Linear temperature decrease
            // T = T0 + L * h = 288.15 + (-0.0065) * h
            return T0 + L_Troposphere * altitude;
        }
        else if (altitude <= H_Tropopause)
        {
            // TROPOPAUSE: Isothermal layer (constant temperature)
            // Temperature "pause" at the boundary between troposphere and stratosphere.
            // AEROSPACE: Jets cruise here because:
            // - Low temperature = efficient engines
            // - Above most weather
            // - Constant conditions
            return T11;
        }
        else if (altitude <= H_Stratosphere1)
        {
            // STRATOSPHERE: Temperature increases with altitude
            // Ozone (O3) absorbs UV radiation and heats this layer.
            // T = T20 + L_strat * (h - h20)
            return T20 + L_Stratosphere1 * (altitude - H_Tropopause);
        }
        else
        {
            // Above 32 km: Simplified extension of stratosphere model
            // Real atmosphere has more layers (mesosphere, thermosphere)
            return T20 + L_Stratosphere1 * (altitude - H_Tropopause);
        }
    }

    // =========================================================================
    // PRESSURE CALCULATION
    // =========================================================================

    /// <summary>
    /// Calculate atmospheric pressure at a given altitude.
    /// </summary>
    /// <param name="altitude">Geometric altitude above sea level [m]</param>
    /// <returns>Static air pressure [Pa]</returns>
    /// <remarks>
    /// <para>
    /// AEROSPACE: Pressure decreases exponentially with altitude.
    /// At cruise altitude (35,000 ft = 10.7 km), pressure is only 23% of sea level.
    /// This is why aircraft cabins must be pressurized.
    /// </para>
    /// <para>
    /// The equation differs for gradient vs isothermal layers:
    /// - Gradient (troposphere): P = P0 * (T/T0)^(-g0/(L*R))
    /// - Isothermal: P = P_base * exp(-g0 * dh / (R * T))
    /// </para>
    /// </remarks>
    public static double Pressure(double altitude)
    {
        double T = Temperature(altitude);

        if (altitude <= H_Troposphere)
        {
            // TROPOSPHERE (gradient layer):
            // P/P0 = (T/T0)^(g0 / (L * R))
            //
            // The exponent comes from integrating the hydrostatic equation
            // dP/dh = -rho * g with the ideal gas law rho = P/(R*T).
            return P0 * Math.Pow(T / T0, -G0 / (L_Troposphere * R));
        }
        else if (altitude <= H_Tropopause)
        {
            // TROPOPAUSE (isothermal layer):
            // P = P11 * exp(-g0 * (h - h11) / (R * T))
            //
            // For constant temperature, pressure decays exponentially.
            // This is the "barometric formula" for isothermal atmosphere.
            return P11 * Math.Exp(-G0 * (altitude - H_Troposphere) / (R * T11));
        }
        else
        {
            // STRATOSPHERE (gradient layer):
            // Same formula as troposphere but with different base values.
            return P20 * Math.Pow(T / T20, -G0 / (L_Stratosphere1 * R));
        }
    }

    // =========================================================================
    // DENSITY CALCULATION
    // =========================================================================

    /// <summary>
    /// Calculate air density at a given altitude.
    /// </summary>
    /// <param name="altitude">Geometric altitude above sea level [m]</param>
    /// <returns>Air density [kg/m^3]</returns>
    /// <remarks>
    /// AEROSPACE: Density is what really matters for aircraft performance!
    /// - Lift proportional to density: L = 0.5 * rho * V^2 * S * Cl
    /// - Drag proportional to density: D = 0.5 * rho * V^2 * S * Cd
    /// - Engine thrust decreases with density (less mass flow)
    ///
    /// At 35,000 ft, density is about 31% of sea level.
    /// Aircraft compensate by flying faster (higher TAS).
    /// </remarks>
    public static double Density(double altitude)
    {
        double T = Temperature(altitude);
        double P = Pressure(altitude);

        // IDEAL GAS LAW: P = rho * R * T
        // Rearranging: rho = P / (R * T)
        return P / (R * T);
    }

    // =========================================================================
    // SPEED OF SOUND
    // =========================================================================

    /// <summary>
    /// Calculate speed of sound at a given altitude.
    /// </summary>
    /// <param name="altitude">Geometric altitude above sea level [m]</param>
    /// <returns>Speed of sound [m/s]</returns>
    /// <remarks>
    /// <para>
    /// AEROSPACE: Speed of sound depends ONLY on temperature, not pressure!
    ///   a = sqrt(gamma * R * T)
    ///
    /// At sea level (T=288K): a = 340 m/s = 661 knots
    /// At cruise (T=217K):    a = 295 m/s = 573 knots
    /// </para>
    /// <para>
    /// Mach number = TAS / a (true airspeed divided by local speed of sound)
    /// A jet at Mach 0.85 at cruise is going:
    ///   0.85 * 295 = 251 m/s = 487 knots TAS
    /// </para>
    /// </remarks>
    public static double SpeedOfSound(double altitude)
    {
        double T = Temperature(altitude);

        // Speed of sound in ideal gas: a = sqrt(gamma * R * T)
        // Derived from thermodynamics of isentropic compression waves.
        return Math.Sqrt(Gamma * R * T);
    }

    // =========================================================================
    // COMPOSITE PROPERTIES
    // =========================================================================

    /// <summary>
    /// Get all atmospheric properties at a given altitude.
    /// </summary>
    /// <param name="altitude">Geometric altitude above sea level [m]</param>
    /// <returns>Record containing T, P, rho, and speed of sound</returns>
    /// <remarks>
    /// C# CONCEPT: Object Initializer with Record
    /// Creates an AtmosphericProperties record with all values set.
    /// More efficient than calling individual methods since Temperature()
    /// is only computed once and reused.
    /// </remarks>
    public static AtmosphericProperties GetProperties(double altitude)
    {
        return new AtmosphericProperties
        {
            Altitude = altitude,
            Temperature = Temperature(altitude),
            Pressure = Pressure(altitude),
            Density = Density(altitude),
            SpeedOfSound = SpeedOfSound(altitude)
        };
    }

    /// <summary>
    /// Generate atmospheric profile data for charting.
    /// </summary>
    /// <param name="minAltitude">Starting altitude [m]</param>
    /// <param name="maxAltitude">Ending altitude [m]</param>
    /// <param name="points">Number of data points</param>
    /// <returns>List of atmospheric properties at regular intervals</returns>
    /// <remarks>
    /// C# CONCEPT: Optional Parameters with Default Values
    /// If caller doesn't specify min/max/points, defaults are used.
    /// Makes the API flexible without requiring overloads.
    /// </remarks>
    public static List<AtmosphericProperties> GenerateProfile(
        double minAltitude = 0,
        double maxAltitude = 50000,
        int points = 100)
    {
        var profile = new List<AtmosphericProperties>();
        double step = (maxAltitude - minAltitude) / (points - 1);

        for (int i = 0; i < points; i++)
        {
            double alt = minAltitude + i * step;
            profile.Add(GetProperties(alt));
        }

        return profile;
    }

    /// <summary>
    /// Get atmospheric layer definitions for reference.
    /// </summary>
    /// <returns>List of atmospheric layers with their properties</returns>
    /// <remarks>
    /// AEROSPACE: Standard atmosphere layer information.
    /// Useful for displaying layer boundaries in visualizations.
    /// </remarks>
    public static List<AtmosphericLayer> GetLayers()
    {
        return new List<AtmosphericLayer>
        {
            new AtmosphericLayer
            {
                Name = "Troposphere",
                BaseAltitude = 0,
                TopAltitude = 11000,
                LapseRate = -0.0065,  // Cooling 6.5 K/km
                BaseTemperature = 288.15  // 15 deg C
            },
            new AtmosphericLayer
            {
                Name = "Tropopause",
                BaseAltitude = 11000,
                TopAltitude = 20000,
                LapseRate = 0,  // Isothermal
                BaseTemperature = 216.65  // -56.5 deg C
            },
            new AtmosphericLayer
            {
                Name = "Stratosphere",
                BaseAltitude = 20000,
                TopAltitude = 50000,
                LapseRate = 0.001,  // Warming 1 K/km
                BaseTemperature = 216.65  // -56.5 deg C at base
            }
        };
    }
}
