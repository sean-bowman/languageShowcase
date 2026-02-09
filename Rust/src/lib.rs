//! # Aerospace Units Library
//!
//! A type-safe unit conversion library for aerospace engineering.
//!
//! =============================================================================
//! AEROSPACE CONCEPT: The Mars Climate Orbiter Disaster
//! =============================================================================
//!
//! On September 23, 1999, NASA lost the $327.6 million Mars Climate Orbiter
//! because of a unit conversion error. The spacecraft's thruster telemetry
//! was being sent in pound-seconds by Lockheed Martin, but NASA's navigation
//! software expected newton-seconds.
//!
//! Result: The spacecraft entered Mars' atmosphere at 57 km instead of the
//! planned 150 km, and was destroyed.
//!
//! This library prevents such errors by making unit types incompatible at
//! compile time. You literally CANNOT add meters to seconds - the Rust
//! compiler will reject the code before it ever runs.
//!
//! =============================================================================
//! RUST CONCEPTS IN THIS FILE
//! =============================================================================
//!
//! ## 1. Documentation Comments (//! and ///)
//!
//! Rust has TWO types of documentation comments:
//!
//! - `//!` - Inner doc comments: Document the CONTAINING item (this module)
//!   These appear at the top of a file/module to describe the whole thing.
//!
//! - `///` - Outer doc comments: Document the FOLLOWING item (a function, struct, etc.)
//!   These appear just before a declaration.
//!
//! Documentation supports Markdown formatting and can include code examples
//! that are automatically tested with `cargo test`!
//!
//! ## 2. Module System (pub mod)
//!
//! Rust organizes code into MODULES:
//! - `mod name;` - Declares a module (looks for name.rs or name/mod.rs)
//! - `pub mod name;` - Declares a PUBLIC module (accessible from outside)
//! - `use crate::path::Item;` - Brings an item into scope
//!
//! Module tree example:
//! ```text
//! crate (lib.rs)
//! ├── units (units/mod.rs)
//! │   ├── length
//! │   ├── velocity
//! │   └── ...
//! └── prelude (inline module)
//! ```
//!
//! ## 3. The Prelude Pattern
//!
//! A "prelude" module re-exports commonly used items for convenient import:
//! ```rust
//! use aerospace_units::prelude::*;  // Import everything commonly needed
//! ```
//! Instead of:
//! ```rust
//! use aerospace_units::units::length::Length;
//! use aerospace_units::units::velocity::Velocity;
//! // ... many more imports
//! ```
//!
//! This pattern is used by Rust's standard library and many popular crates.
//!
//! ## 4. Type Safety Through Newtypes
//!
//! This library uses the "newtype pattern" - wrapping primitives in structs:
//! ```rust
//! pub struct Length(f64);   // Meters internally
//! pub struct Velocity(f64); // m/s internally
//! ```
//!
//! Even though both contain f64, they are DIFFERENT TYPES.
//! `Length + Velocity` is a compile error - no runtime checks needed!
//!
//! Compare to dynamic languages like Python:
//! ```python
//! length = 1000.0    # Is this meters? feet? km?
//! velocity = 340.0   # Is this m/s? km/h? knots?
//! result = length + velocity  # Silent bug!
//! ```
//!
//! =============================================================================
//! ## Example Usage
//!
//! ```rust
//! use aerospace_units::prelude::*;
//!
//! // Create values with explicit units - no ambiguity!
//! let altitude = Length::from_feet(35000.0);
//! println!("Altitude: {} m", altitude.as_meters());
//!
//! // Calculate specific impulse (a key rocket engine metric)
//! let thrust = Force::from_newtons(25000.0);
//! let mass_flow = MassFlowRate::from_kg_per_s(80.0);
//! let isp = thrust.specific_impulse(mass_flow);
//! println!("Specific Impulse: {} s", isp.as_seconds());
//!
//! // This won't compile - different types can't be added!
//! // let bad = thrust + altitude;  // ERROR: mismatched types
//! ```

// =============================================================================
// MODULE DECLARATIONS
// =============================================================================
/// The `units` module contains all unit type definitions.
///
/// RUST CONCEPT: pub mod
/// ----------------------
/// `pub mod units;` does two things:
///   1. Declares a module named `units`
///   2. Makes it public (accessible from outside this crate)
///
/// The compiler looks for the module in:
///   - `units.rs` (single file module), or
///   - `units/mod.rs` (directory module with submodules)
///
/// We use the directory form because `units` has its own submodules
/// (length, velocity, etc.).
pub mod units;

// =============================================================================
// PRELUDE MODULE
// =============================================================================
/// Prelude module - import this for convenient access to all unit types.
///
/// RUST CONCEPT: Inline Module with pub use
/// ----------------------------------------
/// This is an "inline" module - defined directly in this file rather than
/// in a separate file. It's useful for small modules that just re-export items.
///
/// `pub use` re-exports items, making them accessible through this module:
///   - `crate::units::length::Length` becomes...
///   - `crate::prelude::Length`
///
/// Users can then write:
/// ```rust,ignore
/// use aerospace_units::prelude::*;
/// let x = Length::from_meters(100.0);
/// ```
///
/// Instead of the verbose:
/// ```rust,ignore
/// use aerospace_units::units::length::Length;
/// let x = Length::from_meters(100.0);
/// ```
///
/// THE GLOB IMPORT (`*`)
/// ---------------------
/// `use module::*` imports ALL public items from a module.
/// This is normally discouraged in Rust (prefer explicit imports),
/// but preludes are the exception - that's their purpose!
pub mod prelude {
    // Re-export all unit types for convenient access
    // These are the types users will work with most often
    pub use crate::units::angle::Angle;
    pub use crate::units::force::Force;
    pub use crate::units::length::Length;
    pub use crate::units::mass::Mass;
    pub use crate::units::mass_flow_rate::MassFlowRate;
    pub use crate::units::pressure::Pressure;
    pub use crate::units::specific_impulse::SpecificImpulse;
    pub use crate::units::velocity::Velocity;
}

// =============================================================================
// UNIT TESTS
// =============================================================================
/// RUST CONCEPT: Test Module with #[cfg(test)]
///
/// The `#[cfg(test)]` attribute means this module is ONLY compiled when
/// running tests (`cargo test`). It's completely excluded from release builds.
///
/// This is called "conditional compilation" - code that only exists in
/// certain configurations.
///
/// Benefits:
///   - Tests don't bloat the final binary
///   - Tests can access private items (they're in the same crate)
///   - Tests are kept close to the code they test
///
/// RUNNING TESTS:
///   `cargo test`           - Run all tests
///   `cargo test length`    - Run tests with "length" in the name
///   `cargo test -- --nocapture`  - Show println! output
#[cfg(test)]
mod tests {
    // RUST CONCEPT: use super::*
    // --------------------------
    // `super` refers to the parent module (in this case, the crate root).
    // `super::prelude::*` imports everything from our prelude module.
    //
    // This is like Python's `from .. import *` but with explicit super.
    use super::prelude::*;

    // -------------------------------------------------------------------------
    // RUST CONCEPT: #[test] Attribute
    //
    // The `#[test]` attribute marks a function as a test.
    // Test functions:
    //   - Take no arguments
    //   - Return () or Result<(), E>
    //   - Pass if they don't panic
    //   - Fail if they panic or return Err
    //
    // ASSERTIONS:
    //   assert!(condition)           - Panic if false
    //   assert_eq!(left, right)      - Panic if not equal
    //   assert_ne!(left, right)      - Panic if equal
    //   assert!((a - b).abs() < tol) - Floating-point comparison
    // -------------------------------------------------------------------------

    /// Test length unit conversions.
    ///
    /// AEROSPACE: Length Units
    /// -----------------------
    /// Aviation uses a mix of units depending on context:
    ///   - Altitude: feet (USA/intl) or meters (some countries)
    ///   - Distance: nautical miles (aviation), km (metric countries)
    ///   - Runway length: feet or meters
    ///   - Visibility: meters or statute miles
    ///
    /// 1 meter = 3.28084 feet
    /// 1 nautical mile = 1852 meters
    /// 1 kilometer = 1000 meters
    #[test]
    fn test_length_conversions() {
        let length = Length::from_meters(1000.0);

        // Test feet conversion: 1000 m = 3280.84 ft
        assert!((length.as_feet() - 3280.84).abs() < 0.01);

        // Test kilometers: 1000 m = 1.0 km
        assert!((length.as_kilometers() - 1.0).abs() < 0.0001);
    }

    /// Test velocity unit conversions.
    ///
    /// AEROSPACE: Velocity Units
    /// -------------------------
    /// Airspeed is measured in multiple ways:
    ///   - Knots (nautical miles per hour) - standard in aviation
    ///   - Mach number - ratio to speed of sound
    ///   - m/s or km/h - ground operations
    ///
    /// Speed of sound at sea level: ~340.29 m/s = 661.47 knots
    /// Mach 1 = speed of sound (varies with altitude/temperature)
    #[test]
    fn test_velocity_conversions() {
        // Sea level speed of sound
        let velocity = Velocity::from_meters_per_second(340.29);

        // Test knots: 340.29 m/s = 661.47 knots (approximately)
        assert!((velocity.as_knots() - 661.47).abs() < 0.1);

        // Test Mach number: at sea level, 340.29 m/s = Mach 1.0
        assert!((velocity.as_mach(340.29) - 1.0).abs() < 0.0001);
    }

    /// Test force unit conversions.
    ///
    /// AEROSPACE: Force Units
    /// ----------------------
    /// Force in aerospace is typically measured as:
    ///   - Newtons (N) - SI unit
    ///   - Kilonewtons (kN) - large forces
    ///   - Pounds-force (lbf) - US/Imperial
    ///
    /// 1 kN = 1000 N
    /// 1 N = 0.224809 lbf
    #[test]
    fn test_force_conversions() {
        let force = Force::from_newtons(1000.0);

        // Test pounds-force: 1000 N = 224.809 lbf
        assert!((force.as_pounds_force() - 224.809).abs() < 0.01);

        // Test kilonewtons: 1000 N = 1.0 kN
        assert!((force.as_kilonewtons() - 1.0).abs() < 0.0001);
    }

    /// Test specific impulse calculation.
    ///
    /// AEROSPACE: Specific Impulse (Isp)
    /// ----------------------------------
    /// Specific impulse is THE key metric for rocket engine efficiency.
    /// It tells you how long 1 kg of propellant can produce 1 N of thrust.
    ///
    /// Isp = Thrust / (mass_flow_rate * g0)
    /// Units: seconds (a higher number is better)
    ///
    /// Typical values:
    ///   - Solid rockets: 250-290 s
    ///   - RP-1/LOX (Merlin, F-1): 260-310 s
    ///   - LH2/LOX (RS-25, J-2): 360-450 s
    ///   - Ion thrusters: 1500-5000 s (but very low thrust)
    ///
    /// This test uses the Saturn V F-1 engine parameters:
    ///   - Thrust: 6.77 MN (6,770,000 N)
    ///   - Mass flow: 2578 kg/s
    ///   - Expected Isp: ~263 s (sea level) to ~304 s (vacuum)
    #[test]
    fn test_specific_impulse_calculation() {
        // F-1 engine parameters (Saturn V first stage)
        // This engine powered humanity to the Moon!
        let thrust = Force::from_newtons(6_770_000.0);  // 6.77 MN
        let mass_flow = MassFlowRate::from_kg_per_s(2578.0);

        // Calculate specific impulse
        let isp = thrust.specific_impulse(mass_flow);

        // Expected Isp approximately 263-268 seconds at sea level
        // We use a tolerance of 5 seconds due to rounding in our calculation
        assert!((isp.as_seconds() - 268.0).abs() < 5.0);
    }
}
