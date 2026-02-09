//! # Length/Distance Unit Type
//!
//! Stores length internally in meters (SI base unit).
//!
//! =============================================================================
//! AEROSPACE CONCEPT: Length Units in Aviation
//! =============================================================================
//!
//! Aviation uses a confusing mix of length units depending on context:
//!
//! | Measurement | Unit Used | Why? |
//! |-------------|-----------|------|
//! | Altitude    | Feet (USA/intl), Meters (Russia/China) | Historical standardization |
//! | Runway length | Feet or Meters | Depends on country |
//! | Visibility  | Statute miles (USA), Meters (ICAO) | Local conventions |
//! | Distance    | Nautical miles | 1 nm = 1 minute of latitude |
//! | Wavelengths | Meters | Radio navigation (VOR, ILS) |
//!
//! CONVERSION FACTORS (exact definitions):
//! - 1 foot = 0.3048 meters (exact, by international agreement 1959)
//! - 1 nautical mile = 1852 meters (exact, by international agreement)
//! - 1 statute mile = 1609.344 meters (exact, = 5280 feet)
//! - 1 kilometer = 1000 meters (exact, SI definition)
//!
//! COMMON AEROSPACE ALTITUDES:
//! - Cruising altitude: 35,000-40,000 ft (10,668-12,192 m)
//! - Transition altitude: 18,000 ft (USA), varies by country
//! - Mount Everest: 29,032 ft (8,849 m)
//! - Karman line (space): 100 km = 328,084 ft
//!
//! =============================================================================
//! RUST CONCEPTS IN THIS FILE
//! =============================================================================
//!
//! ## 1. The Newtype Pattern
//!
//! The core of this file is the "newtype" pattern:
//! ```rust,ignore
//! pub struct Length { meters: f64 }
//! ```
//!
//! This WRAPS an f64 in a new type. Why not just use f64?
//!
//! **Problem with raw f64:**
//! ```rust,ignore
//! let altitude: f64 = 10000.0;  // Is this feet? Meters? Nautical miles?
//! let distance: f64 = 500.0;    // Same problem
//! let result = altitude + distance;  // Compiles! But is it meaningful?
//! ```
//!
//! **Solution with newtype:**
//! ```rust,ignore
//! let altitude = Length::from_feet(10000.0);
//! let distance = Length::from_nautical_miles(500.0);
//! let result = altitude + distance;  // Converts properly, always meters internally
//! ```
//!
//! The newtype pattern is ZERO-COST at runtime - the struct is compiled away,
//! leaving just an f64. All the safety is enforced at compile time!
//!
//! ## 2. Derive Macros
//!
//! `#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]`
//!
//! This line auto-implements several traits:
//! - Debug: Enables `{:?}` formatting for debugging
//! - Clone: Enables `.clone()` to create copies
//! - Copy: Enables implicit copying (like primitives)
//! - PartialEq: Enables `==` and `!=` comparisons
//! - PartialOrd: Enables `<`, `>`, `<=`, `>=` comparisons
//!
//! "Partial" (vs total) ordering is needed because f64 has NaN,
//! and NaN comparisons are undefined (NaN != NaN is true!)
//!
//! ## 3. Operator Overloading via Traits
//!
//! Rust uses traits to implement operators:
//! - `impl Add for Length` enables `length_a + length_b`
//! - `impl Sub for Length` enables `length_a - length_b`
//! - `impl Mul<f64> for Length` enables `length * scalar`
//! - `impl Div<f64> for Length` enables `length / scalar`
//! - `impl Div<Length> for Length` enables `length_a / length_b` -> ratio
//!
//! ## 4. Associated Types in Traits
//!
//! `type Output = Self;`
//!
//! The Add trait looks like:
//! ```rust,ignore
//! trait Add<Rhs = Self> {
//!     type Output;  // <-- Associated type
//!     fn add(self, rhs: Rhs) -> Self::Output;
//! }
//! ```
//!
//! We must specify what type the operation returns. For Length + Length,
//! the output is Length. For Length / Length, the output is f64 (a ratio).
//!
//! ## 5. The Display Trait
//!
//! `impl fmt::Display for Length` lets us use `{}` formatting:
//! ```rust,ignore
//! let alt = Length::from_meters(10000.0);
//! println!("{}", alt);  // Prints: "10000.00 m"
//! ```

use std::fmt;
use std::ops::{Add, Div, Mul, Sub};

// =============================================================================
// THE NEWTYPE STRUCT
// =============================================================================
/// Length quantity - stores value in meters internally.
///
/// # Why Meters as Internal Representation?
///
/// Meters are the SI base unit for length. By storing everything in meters
/// internally, we:
/// 1. Avoid repeated conversions during calculations
/// 2. Have a single source of truth
/// 3. Match scientific convention
///
/// # The Private Field Pattern
///
/// Notice `meters: f64` has NO `pub` keyword - it's PRIVATE.
/// This is intentional encapsulation:
/// - Users can't write: `let len = Length { meters: 5.0 };`
/// - They MUST use constructors: `Length::from_meters(5.0)`
/// - This ensures all values go through our conversion logic
///
/// # Derive Attributes Explained
///
/// ```text
/// #[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
///          ^      ^      ^     ^         ^
///          |      |      |     |         +-- Comparison: <, >, <=, >=
///          |      |      |     +-- Equality: ==, !=
///          |      |      +-- Implicit copy (like i32, f64)
///          |      +-- Explicit .clone() method
///          +-- Debug printing with {:?}
/// ```
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub struct Length {
    meters: f64,
}

// =============================================================================
// IMPLEMENTATION BLOCK
// =============================================================================
/// RUST CONCEPT: impl Block
///
/// The `impl` block defines methods associated with a type.
/// Methods inside can be:
/// - Associated functions (no self): `Length::from_meters(5.0)`
/// - Instance methods (&self): `length.as_feet()`
///
/// Self vs self:
/// - `Self` (capital) = the type (Length)
/// - `self` (lowercase) = the instance
/// - `&self` = borrowed reference to instance
/// - `&mut self` = mutable borrowed reference
impl Length {
    // =========================================================================
    // CONSTRUCTORS (Associated Functions)
    // =========================================================================
    // These are "associated functions" - they don't take self.
    // Called with :: syntax: Length::from_meters(100.0)
    //
    // RUST CONCEPT: Why Self { meters: m } works
    // ------------------------------------------
    // Inside an impl block, `Self` is an alias for the type being implemented.
    // So `Self { meters: m }` is equivalent to `Length { meters: m }`.
    // Using Self makes refactoring easier if we rename the type.

    /// Create a Length from meters (SI base unit).
    ///
    /// # Example
    /// ```
    /// use aerospace_units::prelude::*;
    /// let altitude = Length::from_meters(10668.0);  // Typical cruise altitude
    /// ```
    pub fn from_meters(m: f64) -> Self {
        Self { meters: m }
    }

    /// Create a Length from kilometers.
    ///
    /// AEROSPACE: Used for visibility (in ICAO/metric countries)
    /// and some altitude displays.
    ///
    /// Conversion: 1 km = 1000 m (exact, SI definition)
    pub fn from_kilometers(km: f64) -> Self {
        Self { meters: km * 1000.0 }
    }

    /// Create a Length from feet.
    ///
    /// AEROSPACE: THE primary altitude unit in most of the world's airspace.
    /// Flight levels (FL350 = 35,000 ft) are always in feet.
    ///
    /// Conversion: 1 ft = 0.3048 m (exact, by 1959 international agreement)
    ///
    /// The exact value 0.3048 comes from: 1 yard = 0.9144 m, 1 ft = 1/3 yard
    pub fn from_feet(ft: f64) -> Self {
        Self { meters: ft * 0.3048 }
    }

    /// Create a Length from nautical miles.
    ///
    /// AEROSPACE: THE standard unit for horizontal distance in aviation.
    /// - ATC gives distances in nm
    /// - Charts show distances in nm
    /// - Flight plans use nm
    ///
    /// Conversion: 1 nm = 1852 m (exact, by international agreement)
    ///
    /// Why 1852? A nautical mile was originally defined as one minute of arc
    /// of latitude. Earth's circumference / (360 degrees x 60 minutes) gives
    /// approximately 1852 m. The value was later standardized exactly.
    pub fn from_nautical_miles(nm: f64) -> Self {
        Self { meters: nm * 1852.0 }
    }

    /// Create a Length from statute miles.
    ///
    /// AEROSPACE: Used for visibility in the USA.
    /// "Visibility 10 statute miles" means clear conditions.
    ///
    /// Conversion: 1 mi = 1609.344 m (exact, = 5280 ft x 0.3048 m/ft)
    pub fn from_miles(mi: f64) -> Self {
        Self { meters: mi * 1609.344 }
    }

    // =========================================================================
    // ACCESSORS (Instance Methods)
    // =========================================================================
    // These are "instance methods" - they take &self (borrowed reference).
    // Called with . syntax: length.as_meters()
    //
    // RUST CONCEPT: &self Borrow
    // --------------------------
    // `&self` means we BORROW the instance, not consume it.
    // The caller can still use the Length after calling these methods.
    //
    // If we used `self` (not &self), the method would MOVE (consume) the value,
    // and the caller couldn't use it again. For getters, we always want &self.

    /// Get value in meters (the internal representation).
    ///
    /// Since meters are stored directly, this is a simple field access.
    pub fn as_meters(&self) -> f64 {
        self.meters
    }

    /// Get value in kilometers.
    pub fn as_kilometers(&self) -> f64 {
        self.meters / 1000.0
    }

    /// Get value in feet.
    ///
    /// AEROSPACE: Use this for altitude displays and flight level calculations.
    pub fn as_feet(&self) -> f64 {
        self.meters / 0.3048
    }

    /// Get value in nautical miles.
    ///
    /// AEROSPACE: Use this for distance/range calculations.
    pub fn as_nautical_miles(&self) -> f64 {
        self.meters / 1852.0
    }

    /// Get value in statute miles.
    pub fn as_miles(&self) -> f64 {
        self.meters / 1609.344
    }

    // =========================================================================
    // UTILITY METHODS
    // =========================================================================

    /// Check if this length is positive.
    ///
    /// Useful for validation - most aerospace lengths should be positive.
    pub fn is_positive(&self) -> bool {
        self.meters > 0.0
    }

    /// Get the absolute value.
    ///
    /// RUST CONCEPT: Returning Self
    /// ----------------------------
    /// This method returns a NEW Length (by value), it doesn't modify
    /// the existing one. This is idiomatic Rust - prefer immutable
    /// operations that return new values.
    pub fn abs(&self) -> Self {
        Self {
            meters: self.meters.abs(),
        }
    }
}

// =============================================================================
// OPERATOR OVERLOADING: ARITHMETIC TRAITS
// =============================================================================
// RUST CONCEPT: Operator Overloading
//
// Rust doesn't have "magic" operators. Instead, operators like + and -
// are defined by traits in std::ops. To make `a + b` work, you implement
// the Add trait for your type.
//
// Trait definition (simplified):
// ```
// pub trait Add<Rhs = Self> {
//     type Output;
//     fn add(self, rhs: Rhs) -> Self::Output;
// }
// ```
//
// Note: Operators CONSUME their operands by default (take self, not &self).
// This is fine for Copy types like Length - they're implicitly copied.
// =============================================================================

/// Length + Length = Length
///
/// Adding two lengths makes physical sense - you get another length.
///
/// RUST CONCEPT: impl Trait for Type
/// ---------------------------------
/// `impl Add for Length` means "implement the Add trait for the Length type."
/// This enables: `let total = length_a + length_b;`
impl Add for Length {
    /// The result type of the addition operation.
    /// Length + Length = Length (not f64, not something else).
    type Output = Self;

    /// Perform the addition.
    ///
    /// RUST CONCEPT: self vs &self in operators
    /// ----------------------------------------
    /// The Add trait takes `self` (not `&self`), meaning it MOVES the value.
    /// However, since Length is Copy, Rust automatically copies it.
    /// So `a + b` doesn't actually consume a or b - they remain usable.
    fn add(self, other: Self) -> Self {
        Self {
            meters: self.meters + other.meters,
        }
    }
}

/// Length - Length = Length
///
/// Subtracting lengths gives the difference (also a length).
impl Sub for Length {
    type Output = Self;

    fn sub(self, other: Self) -> Self {
        Self {
            meters: self.meters - other.meters,
        }
    }
}

/// Length * scalar = Length
///
/// Multiplying a length by a dimensionless number scales it.
/// Example: `half_distance = distance * 0.5`
///
/// RUST CONCEPT: Generic Type Parameter <f64>
/// ------------------------------------------
/// `impl Mul<f64> for Length` means Length can be multiplied by f64.
/// The <f64> specifies what the right-hand side type is.
///
/// We could also implement `impl Mul<Length> for f64` to allow `2.0 * length`,
/// but that's not done here for simplicity.
impl Mul<f64> for Length {
    type Output = Self;

    fn mul(self, scalar: f64) -> Self {
        Self {
            meters: self.meters * scalar,
        }
    }
}

/// Length / scalar = Length
///
/// Dividing a length by a number shrinks it.
/// Example: `half_distance = distance / 2.0`
impl Div<f64> for Length {
    type Output = Self;

    fn div(self, scalar: f64) -> Self {
        Self {
            meters: self.meters / scalar,
        }
    }
}

/// Length / Length = ratio (f64)
///
/// Dividing two lengths gives a dimensionless ratio.
/// Example: `let scale = actual_distance / model_distance;`
///
/// RUST CONCEPT: Different Output Types
/// ------------------------------------
/// Notice `type Output = f64` here, not `Self`.
/// When Length is divided by Length, the units cancel:
/// meters / meters = dimensionless
///
/// This is DIFFERENT from `impl Div<f64>` above, where Output = Self.
/// Rust allows multiple impl blocks with different type parameters!
impl Div<Length> for Length {
    type Output = f64;

    /// Dividing two lengths gives a dimensionless ratio.
    fn div(self, other: Length) -> f64 {
        self.meters / other.meters
    }
}

// =============================================================================
// DISPLAY TRAIT: HUMAN-READABLE OUTPUT
// =============================================================================
/// RUST CONCEPT: The Display Trait
///
/// Display is the trait for user-facing output (like Python's __str__).
/// Implementing it allows using {} in format strings:
///
/// ```rust,ignore
/// let alt = Length::from_meters(1000.0);
/// println!("{}", alt);  // Prints: "1000.00 m"
/// ```
///
/// vs Debug (from #[derive(Debug)]) which uses {:?} and is for programmers:
/// ```rust,ignore
/// println!("{:?}", alt);  // Prints: "Length { meters: 1000.0 }"
/// ```
///
/// RUST CONCEPT: fmt::Formatter
/// ----------------------------
/// The Formatter controls output formatting (width, precision, alignment).
/// We use write!() macro to write formatted output to it.
impl fmt::Display for Length {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        // {:.2} means 2 decimal places
        write!(f, "{:.2} m", self.meters)
    }
}

// =============================================================================
// UNIT TESTS
// =============================================================================
/// RUST CONCEPT: Test Module
///
/// Tests are placed in a submodule marked with #[cfg(test)].
/// They have access to private fields/methods of the parent module.
///
/// Run with: `cargo test`
/// Run specific test: `cargo test test_conversions`
#[cfg(test)]
mod tests {
    // Import everything from the parent module (Length, etc.)
    use super::*;

    /// Test that unit conversions are accurate.
    ///
    /// AEROSPACE: These conversion factors are internationally standardized.
    /// Errors here could cause serious navigation problems!
    #[test]
    fn test_conversions() {
        // 1 foot = 0.3048 meters (exact)
        let length = Length::from_feet(1.0);
        assert!((length.as_meters() - 0.3048).abs() < 0.0001);

        // 1 kilometer = 1000 meters (exact)
        let km = Length::from_kilometers(1.0);
        assert!((km.as_meters() - 1000.0).abs() < 0.0001);

        // 1 nautical mile = 1852 meters (exact)
        let nm = Length::from_nautical_miles(1.0);
        assert!((nm.as_meters() - 1852.0).abs() < 0.0001);
    }

    /// Test arithmetic operations on lengths.
    #[test]
    fn test_arithmetic() {
        let a = Length::from_meters(100.0);
        let b = Length::from_meters(50.0);

        // Length + Length = Length
        assert!((a + b).as_meters() - 150.0 < 0.0001);

        // Length - Length = Length
        assert!((a - b).as_meters() - 50.0 < 0.0001);

        // Length * scalar = Length
        assert!((a * 2.0).as_meters() - 200.0 < 0.0001);

        // Length / scalar = Length
        assert!((a / 2.0).as_meters() - 50.0 < 0.0001);
    }

    /// Test that dividing two lengths gives a ratio.
    #[test]
    fn test_ratio() {
        let a = Length::from_meters(100.0);
        let b = Length::from_meters(50.0);

        // Length / Length = dimensionless ratio
        let ratio: f64 = a / b;
        assert!((ratio - 2.0).abs() < 0.0001);
    }
}
