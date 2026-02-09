//! # Unit Type Modules
//!
//! Each module defines a strongly-typed quantity that prevents
//! mixing incompatible units at compile time.
//!
//! =============================================================================
//! AEROSPACE CONCEPT: Type-Safe Units
//! =============================================================================
//!
//! The Mars Climate Orbiter disaster ($327.6 million lost!) was caused by
//! a simple unit mismatch: one team used pound-seconds, another expected
//! newton-seconds. The software compiled and ran - but gave wrong results.
//!
//! This library prevents such errors by making incompatible units into
//! different TYPES. The Rust compiler won't even let you compile code
//! that mixes them incorrectly:
//!
//! ```rust,ignore
//! let altitude = Length::from_feet(35000.0);
//! let airspeed = Velocity::from_knots(450.0);
//! // let bad = altitude + airspeed;  // WON'T COMPILE - different types!
//! ```
//!
//! =============================================================================
//! RUST CONCEPT: Module Organization
//! =============================================================================
//!
//! This file (mod.rs) serves as the "root" of the `units` module.
//! When you write `mod units;` in lib.rs, Rust looks for:
//!   - `units.rs` (a single file), OR
//!   - `units/mod.rs` (a directory with submodules)
//!
//! We use the directory form because units has multiple submodules.
//!
//! DIRECTORY STRUCTURE:
//! ```text
//! src/
//! |-- lib.rs           (declares: pub mod units;)
//! |-- units/
//!     |-- mod.rs       (THIS FILE - declares submodules)
//!     |-- length.rs    (defines Length type)
//!     |-- velocity.rs  (defines Velocity type)
//!     |-- mass.rs      (defines Mass type)
//!     |-- force.rs     (defines Force type)
//!     |-- ...
//! ```
//!
//! ## pub mod vs mod
//!
//! - `pub mod name;` - Public module, accessible from outside this crate
//! - `mod name;` - Private module, only accessible within this crate
//!
//! All our unit modules are public so users can import them directly
//! if they prefer specific imports over the prelude.
//!
//! ## Module Visibility
//!
//! Each `pub mod` line makes the entire module public, but only the
//! items marked `pub` INSIDE those modules are actually accessible.
//! So `pub mod length;` exposes `length::Length` because Length is
//! declared as `pub struct Length`.

// =============================================================================
// SUBMODULE DECLARATIONS
// =============================================================================
// Each line below tells Rust to include that module.
// The compiler looks for `{name}.rs` in this directory.

/// Angle measurements (radians, degrees).
///
/// AEROSPACE: Used for heading, bank angle, pitch, and geographic coordinates.
pub mod angle;

/// Force measurements (newtons, pounds-force).
///
/// AEROSPACE: Used for thrust, lift, drag, and weight calculations.
pub mod force;

/// Length/distance measurements (meters, feet, nautical miles).
///
/// AEROSPACE: Used for altitude, range, runway length, visibility.
pub mod length;

/// Mass measurements (kilograms, pounds-mass).
///
/// AEROSPACE: Used for fuel mass, payload, aircraft weight.
/// Note: Mass != Weight! Mass is constant, weight depends on gravity.
pub mod mass;

/// Mass flow rate (kg/s, lb/s).
///
/// AEROSPACE: Used for engine fuel consumption and propellant flow in rockets.
pub mod mass_flow_rate;

/// Pressure measurements (pascals, millibars, inches of mercury).
///
/// AEROSPACE: Used for atmospheric pressure, altimeter settings, cabin pressure.
pub mod pressure;

/// Specific impulse (seconds).
///
/// AEROSPACE: The key metric for rocket engine efficiency.
/// Higher Isp = more delta-v per unit of propellant.
pub mod specific_impulse;

/// Velocity measurements (m/s, knots, Mach).
///
/// AEROSPACE: Used for airspeed, groundspeed, climb rate, orbital velocity.
pub mod velocity;
