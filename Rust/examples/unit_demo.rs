//! # Unit Conversion Demo
//!
//! Demonstrates basic unit conversions and type safety features.
//!
//! =============================================================================
//! AEROSPACE CONCEPT: Why Unit Safety Matters
//! =============================================================================
//!
//! On September 23, 1999, NASA lost the $327.6 million Mars Climate Orbiter
//! because of a unit conversion error:
//!
//! - Lockheed Martin's software sent thruster telemetry in POUND-SECONDS
//! - NASA's navigation software expected NEWTON-SECONDS
//! - The spacecraft entered Mars atmosphere at 57 km instead of 150 km
//! - It was destroyed
//!
//! This library prevents such errors at COMPILE TIME. If you try to add
//! Force to Length, the Rust compiler will refuse to build your code.
//! No runtime checks needed - the error is caught before the code ever runs!
//!
//! =============================================================================
//! RUST CONCEPT: Running Examples
//! =============================================================================
//!
//! Run this example with:
//!
//! ```bash
//! cargo run --example unit_demo
//! ```
//!
//! Examples are compiled and run separately from the main library.
//! They're great for documentation and testing usage patterns.
//!
//! =============================================================================
//! RUST CONCEPT: The Prelude Pattern
//! =============================================================================
//!
//! `use aerospace_units::prelude::*;`
//!
//! This imports all commonly-used types with a single line:
//! - Length, Velocity, Mass, Force, Pressure, Angle
//! - SpecificImpulse, MassFlowRate
//!
//! Without the prelude, you'd need:
//! ```rust
//! use aerospace_units::units::length::Length;
//! use aerospace_units::units::velocity::Velocity;
//! use aerospace_units::units::mass::Mass;
//! // ... and so on for each type
//! ```

use aerospace_units::prelude::*;

/// Main entry point - demonstrates unit conversions and type safety.
fn main() {
    println!("==========================================");
    println!("    Aerospace Units - Type Safety Demo");
    println!("==========================================\n");

    // =========================================================================
    // LENGTH CONVERSIONS
    // =========================================================================
    // AEROSPACE: Length units in aviation
    // - Altitude: feet (USA/intl) or meters (Russia/China)
    // - Horizontal distance: nautical miles
    // - Runway length: feet or meters
    println!("Length Conversions:");
    println!("-------------------");

    // Create a length from feet - common for altitude
    let cruise_altitude = Length::from_feet(35000.0);

    // Access the same value in different units
    // The internal representation is always meters, but we can
    // convert to any unit on demand.
    println!("  Cruise altitude: {} ft", cruise_altitude.as_feet());
    println!("                   {} m", cruise_altitude.as_meters());
    println!("                   {} km", cruise_altitude.as_kilometers());
    println!();

    // =========================================================================
    // VELOCITY CONVERSIONS
    // =========================================================================
    // AEROSPACE: Velocity units are complex!
    // - Knots: THE standard for ATC and flight planning
    // - Mach: For high-speed flight (varies with temperature!)
    // - m/s or km/h: Ground operations
    println!("Velocity Conversions:");
    println!("---------------------");

    let ground_speed = Velocity::from_knots(450.0);

    // Same velocity, different representations
    println!("  Ground speed: {} kts", ground_speed.as_knots());
    println!("                {} mph", ground_speed.as_miles_per_hour());
    println!("                {} km/h", ground_speed.as_kilometers_per_hour());
    println!("                {} m/s", ground_speed.as_meters_per_second());

    // AEROSPACE: Mach number depends on local speed of sound
    // At 11 km altitude (typical cruise), temperature is about -56.5 degC
    // Speed of sound = sqrt(gamma * R * T) = ~295 m/s
    let speed_of_sound = 295.07; // m/s at 11 km altitude
    println!("  Mach number:  {:.2} (at 11 km altitude)",
             ground_speed.as_mach(speed_of_sound));
    println!();

    // =========================================================================
    // FORCE/THRUST CONVERSIONS
    // =========================================================================
    // AEROSPACE: Thrust is measured in:
    // - Kilonewtons (kN): Most of the world
    // - Pounds-force (lbf): US aerospace industry
    println!("Force/Thrust Conversions:");
    println!("-------------------------");

    // GE90-115B: One of the most powerful commercial jet engines
    let engine_thrust = Force::from_kilonewtons(845.0);

    println!("  Engine thrust: {} kN", engine_thrust.as_kilonewtons());
    println!("                 {} lbf", engine_thrust.as_pounds_force());
    println!("                 {} N", engine_thrust.as_newtons());
    println!();

    // =========================================================================
    // PRESSURE CONVERSIONS
    // =========================================================================
    // AEROSPACE: Pressure units vary by context
    // - Altimeter setting: inHg (USA), hPa/mbar (ICAO)
    // - Cabin pressure: psi (engineering)
    // - Atmospheric: Pa, kPa, atm
    println!("Pressure Conversions:");
    println!("---------------------");

    // Typical cabin pressure at cruise altitude
    // (equivalent to about 8000 ft altitude)
    let cabin_pressure = Pressure::from_psi(11.8);

    println!("  Cabin pressure: {:.1} psi", cabin_pressure.as_psi());
    println!("                  {:.1} kPa", cabin_pressure.as_kilopascals());
    println!("                  {:.2} atm", cabin_pressure.as_atmospheres());
    println!("                  {:.1} inHg", cabin_pressure.as_inches_hg());
    println!();

    // =========================================================================
    // ANGLE CONVERSIONS
    // =========================================================================
    // AEROSPACE: Angles are everywhere!
    // - Navigation: degrees (0-360)
    // - Coordinates: degrees/minutes/seconds
    // - Physics calculations: radians
    println!("Angle Conversions:");
    println!("------------------");

    // Bank angle for a coordinated turn
    let bank_angle = Angle::from_degrees(30.0);

    println!("  Bank angle: {:.0} deg", bank_angle.as_degrees());
    println!("              {:.4} rad", bank_angle.as_radians());

    // Trigonometry methods work directly on the Angle type
    // No need to convert to radians first!
    println!("  sin(30 deg) = {:.3}", bank_angle.sin());
    println!("  cos(30 deg) = {:.3}", bank_angle.cos());
    println!();

    // =========================================================================
    // SPECIFIC IMPULSE
    // =========================================================================
    // AEROSPACE: Isp is THE metric for rocket engine efficiency
    // Higher Isp = more delta-v per kg of propellant
    println!("Specific Impulse:");
    println!("-----------------");

    // RS-25 (Space Shuttle Main Engine) - one of the most efficient
    // chemical rocket engines ever built
    let rs25_isp = SpecificImpulse::from_seconds(452.0);

    println!("  RS-25 Isp: {} s", rs25_isp.as_seconds());
    println!("             {} m/s (exhaust velocity)", rs25_isp.as_exhaust_velocity());
    println!("             {:.2} km/s", rs25_isp.as_exhaust_velocity_kmps());
    println!();

    // =========================================================================
    // TYPE SAFETY DEMONSTRATION
    // =========================================================================
    // This is the KEY feature of this library: compile-time unit checking
    println!("Type Safety Demonstration:");
    println!("--------------------------");

    println!("  The following operations are ALLOWED:");

    // Create two lengths to demonstrate valid operations
    let alt1 = Length::from_meters(1000.0);
    let alt2 = Length::from_meters(500.0);

    // RUST CONCEPT: Operator Overloading
    // ----------------------------------
    // These operations work because we implemented the
    // Add, Mul<f64>, and Div<Length> traits for Length.
    println!("    Length + Length = {}", alt1 + alt2);  // Add trait
    println!("    Length * 2.0 = {}", alt1 * 2.0);      // Mul<f64> trait
    println!("    Length / Length = {} (ratio)", alt1 / alt2);  // Div<Length> trait
    println!();

    // RUST CONCEPT: Compile-Time Type Checking
    // ----------------------------------------
    // The following code would NOT compile if uncommented:
    //
    // let force = Force::from_newtons(100.0);
    // let length = Length::from_meters(10.0);
    // let bad = force + length;  // ERROR: no implementation for `Force + Length`
    //
    // The Rust compiler catches unit errors BEFORE the code runs!
    println!("  The following would NOT COMPILE (type mismatch):");
    println!("    // Force + Length      <- Error: incompatible types");
    println!("    // Velocity + Pressure <- Error: incompatible types");
    println!("    // Mass + Angle        <- Error: incompatible types");
    println!();
    println!("  This prevents errors like the Mars Climate Orbiter mishap,");
    println!("  where unit confusion led to a $327 million loss.");
}
