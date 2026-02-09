//! # Tsiolkovsky Rocket Equation Example
//!
//! Demonstrates using the aerospace_units library to calculate
//! delta-v for a rocket using the Tsiolkovsky rocket equation.
//!
//! =============================================================================
//! AEROSPACE CONCEPT: The Tsiolkovsky Rocket Equation
//! =============================================================================
//!
//! The most important equation in astronautics, derived by Konstantin
//! Tsiolkovsky in 1903 (before powered flight!):
//!
//! ```text
//!     dv = v_e * ln(m0 / mf)
//!
//!     or equivalently:
//!
//!     dv = Isp * g0 * ln(m0 / mf)
//!
//! Where:
//!   dv  = delta-v (change in velocity) [m/s]
//!   v_e = effective exhaust velocity [m/s]
//!   Isp = specific impulse [s]
//!   g0  = standard gravity (9.80665 m/s^2)
//!   m0  = initial mass (wet mass, with propellant) [kg]
//!   mf  = final mass (dry mass, propellant expended) [kg]
//!   ln  = natural logarithm
//! ```
//!
//! KEY INSIGHTS:
//! -------------
//! 1. Delta-v is LOGARITHMIC with mass ratio. To double your delta-v,
//!    you need to SQUARE your mass ratio (exponentially more propellant).
//!
//! 2. Higher Isp (exhaust velocity) gives linear improvement.
//!    This is why H2/LOX engines are preferred despite complexity.
//!
//! 3. The "tyranny of the rocket equation" - most of a rocket's mass
//!    must be propellant. Saturn V was 85% propellant by mass!
//!
//! DELTA-V BUDGET EXAMPLES:
//! ------------------------
//! | Maneuver | Typical dv |
//! |----------|------------|
//! | Earth surface to LEO | ~9,400 m/s |
//! | LEO to GEO | ~4,000 m/s |
//! | LEO to Earth escape | ~3,200 m/s |
//! | LEO to Mars transfer | ~3,600 m/s |
//! | LEO to Moon landing | ~5,900 m/s |
//!
//! =============================================================================
//! RUST CONCEPT: Running Examples
//! =============================================================================
//!
//! Files in the `examples/` directory are standalone programs.
//! Run this example with:
//!
//! ```bash
//! cargo run --example rocket_equation
//! ```
//!
//! The Cargo.toml specifies dependencies, and the example gets
//! access to the main library crate (aerospace_units).
//!
//! =============================================================================
//! RUST CONCEPT: The Prelude Import
//! =============================================================================
//!
//! `use aerospace_units::prelude::*;`
//!
//! This single line imports all commonly-used types from our library:
//! - Length, Velocity, Mass, Force, Pressure, Angle
//! - SpecificImpulse, MassFlowRate
//!
//! The `*` (glob import) is usually discouraged in Rust, but preludes
//! are the exception - that's exactly what they're designed for!

use aerospace_units::prelude::*;

/// Main entry point - demonstrates the rocket equation with real examples.
///
/// RUST CONCEPT: fn main()
/// -----------------------
/// Every executable Rust program needs a main() function.
/// It takes no arguments and returns () (unit type) by default.
///
/// Command-line args are accessed via std::env::args() if needed.
fn main() {
    println!("==============================================");
    println!("    Tsiolkovsky Rocket Equation Calculator");
    println!("==============================================\n");

    // =========================================================================
    // EXAMPLE 1: Saturn V First Stage (S-IC)
    // =========================================================================
    // The S-IC was the most powerful rocket stage ever flown.
    // It used 5 F-1 engines burning RP-1/LOX.
    //
    // AEROSPACE NOTE: Mass Ratio
    // --------------------------
    // Mass ratio = m0/mf = wet/dry = 2290/131 = 17.5
    // This is an EXCELLENT mass ratio for a first stage!
    // Most of the stage's mass was propellant.
    println!("Example 1: Saturn V S-IC Stage");
    println!("------------------------------");

    // RUST CONCEPT: Type Inference
    // ----------------------------
    // Rust infers these are Mass types from the function called.
    // We could also write: let s1c_wet_mass: Mass = Mass::from_tonnes(2290.0);
    let s1c_wet_mass = Mass::from_tonnes(2290.0);
    let s1c_dry_mass = Mass::from_tonnes(131.0);
    let f1_isp = SpecificImpulse::from_seconds(263.0); // sea level Isp

    // Calculate delta-v using our helper function
    let delta_v_s1c = calculate_delta_v(f1_isp, s1c_wet_mass, s1c_dry_mass);

    // RUST CONCEPT: println! Formatting
    // ---------------------------------
    // {:.0} = 0 decimal places
    // {:.2} = 2 decimal places
    // {} uses the Display trait implementation
    println!("  Wet mass:    {:.0} tonnes ({:.0} lb)",
             s1c_wet_mass.as_tonnes(),
             s1c_wet_mass.as_pounds());
    println!("  Dry mass:    {:.0} tonnes ({:.0} lb)",
             s1c_dry_mass.as_tonnes(),
             s1c_dry_mass.as_pounds());
    println!("  Mass ratio:  {:.2}", s1c_wet_mass.as_kilograms() / s1c_dry_mass.as_kilograms());
    println!("  Isp:         {} ({:.0} m/s exhaust velocity)",
             f1_isp,
             f1_isp.as_exhaust_velocity());
    println!("  Delta-v:     {:.0} m/s ({:.2} km/s)",
             delta_v_s1c.as_meters_per_second(),
             delta_v_s1c.as_kilometers_per_second());
    println!();

    // =========================================================================
    // EXAMPLE 2: Falcon 9 Second Stage
    // =========================================================================
    // SpaceX's workhorse upper stage uses a single Merlin Vacuum engine.
    //
    // AEROSPACE NOTE: Vacuum vs Sea Level Isp
    // ---------------------------------------
    // The Merlin Vacuum has much higher Isp (348s) than sea level engines
    // because there's no atmospheric backpressure on the exhaust.
    // The nozzle can be optimized for vacuum expansion.
    println!("Example 2: Falcon 9 Second Stage");
    println!("---------------------------------");

    let f9_s2_wet_mass = Mass::from_tonnes(111.5);
    let f9_s2_dry_mass = Mass::from_tonnes(4.5);
    let merlin_vac_isp = SpecificImpulse::from_seconds(348.0);

    let delta_v_f9 = calculate_delta_v(merlin_vac_isp, f9_s2_wet_mass, f9_s2_dry_mass);

    println!("  Wet mass:    {:.1} tonnes", f9_s2_wet_mass.as_tonnes());
    println!("  Dry mass:    {:.1} tonnes", f9_s2_dry_mass.as_tonnes());
    println!("  Mass ratio:  {:.2}", f9_s2_wet_mass.as_kilograms() / f9_s2_dry_mass.as_kilograms());
    println!("  Isp:         {} (Merlin Vacuum)", merlin_vac_isp);
    println!("  Delta-v:     {:.0} m/s ({:.2} km/s)",
             delta_v_f9.as_meters_per_second(),
             delta_v_f9.as_kilometers_per_second());
    println!();

    // =========================================================================
    // EXAMPLE 3: Ion Thruster Deep Space Mission
    // =========================================================================
    // Ion thrusters have VERY high Isp (3000+ seconds) but low thrust.
    // They're used for long-duration missions where efficiency matters
    // more than thrust (like Dawn spacecraft, Starlink satellites).
    //
    // AEROSPACE NOTE: The Isp Advantage
    // ---------------------------------
    // With only 300 kg of propellant, an ion thruster can achieve
    // 6.5 km/s of delta-v! A chemical rocket would need ~1000 kg
    // of propellant for the same delta-v.
    //
    // The catch: ion thrusters produce only millinewtons of thrust,
    // so burns take months instead of minutes.
    println!("Example 3: Ion Thruster Deep Space Mission");
    println!("-------------------------------------------");

    let spacecraft_wet = Mass::from_kilograms(1500.0);
    let spacecraft_dry = Mass::from_kilograms(1200.0);
    let ion_isp = SpecificImpulse::from_seconds(3000.0);

    let delta_v_ion = calculate_delta_v(ion_isp, spacecraft_wet, spacecraft_dry);

    // RUST CONCEPT: Operator Overloading in Action
    // --------------------------------------------
    // (spacecraft_wet - spacecraft_dry) works because we implemented
    // the Sub trait for Mass. The result is another Mass.
    println!("  Wet mass:    {:.0} kg", spacecraft_wet.as_kilograms());
    println!("  Dry mass:    {:.0} kg", spacecraft_dry.as_kilograms());
    println!("  Propellant:  {:.0} kg", (spacecraft_wet - spacecraft_dry).as_kilograms());
    println!("  Isp:         {} (Ion thruster)", ion_isp);
    println!("  Delta-v:     {:.0} m/s ({:.2} km/s)",
             delta_v_ion.as_meters_per_second(),
             delta_v_ion.as_kilometers_per_second());
    println!();

    // Reference information
    println!("Reference: Typical Delta-v Requirements");
    println!("---------------------------------------");
    println!("  LEO insertion:      ~9,400 m/s");
    println!("  LEO to GEO:         ~4,000 m/s");
    println!("  Earth escape:       ~3,200 m/s (from LEO)");
    println!("  Mars transfer:      ~3,600 m/s (from LEO)");
}

/// Calculate delta-v using the Tsiolkovsky rocket equation.
///
/// # The Equation
///
/// ```text
/// dv = v_e * ln(m0 / mf)
/// ```
///
/// # Parameters
/// - `isp`: Specific impulse of the engine
/// - `wet_mass`: Initial mass (with propellant)
/// - `dry_mass`: Final mass (propellant expended)
///
/// # Returns
/// Delta-v as a Velocity
///
/// # RUST CONCEPT: Function Parameters with Custom Types
///
/// Notice how the function signature enforces correct units:
/// - `isp: SpecificImpulse` - can't accidentally pass a Mass
/// - `wet_mass: Mass` - can't accidentally pass a Velocity
/// - Returns `Velocity` - the result is typed, not just a raw f64
///
/// This is the power of type-safe units!
fn calculate_delta_v(isp: SpecificImpulse, wet_mass: Mass, dry_mass: Mass) -> Velocity {
    // Mass ratio: m0/mf (must be > 1 for positive delta-v)
    let mass_ratio = wet_mass.as_kilograms() / dry_mass.as_kilograms();

    // Apply the rocket equation: dv = v_e * ln(mass_ratio)
    // RUST NOTE: .ln() is a method on f64 for natural logarithm
    let delta_v_mps = isp.as_exhaust_velocity() * mass_ratio.ln();

    // Wrap the result in our Velocity type
    Velocity::from_meters_per_second(delta_v_mps)
}
