//! # Mass Unit Type
//!
//! Stores mass internally in kilograms (SI base unit).
//!
//! =============================================================================
//! AEROSPACE CONCEPT: Mass vs Weight
//! =============================================================================
//!
//! CRITICAL DISTINCTION:
//! - MASS: Amount of matter (intrinsic property, constant everywhere)
//! - WEIGHT: Force due to gravity (varies with location)
//!
//! ```text
//! Weight = Mass x Gravitational acceleration
//! W = m * g
//!
//! On Earth surface: g = 9.80665 m/s^2 (standard gravity)
//! On Moon surface:  g = 1.62 m/s^2 (~1/6 Earth)
//! On Mars surface:  g = 3.72 m/s^2 (~38% Earth)
//! In orbit (free fall): Apparent g = 0 (weightlessness, but mass unchanged!)
//! ```
//!
//! An astronaut with mass 70 kg:
//! - Weighs 686 N on Earth
//! - Weighs 113 N on Moon
//! - Weighs 0 N in orbit (but still has 70 kg of mass!)
//!
//! AEROSPACE MASS UNITS:
//! ---------------------
//! | Unit | Symbol | Definition | Usage |
//! |------|--------|------------|-------|
//! | Kilogram | kg | SI base unit | Scientific, most of world |
//! | Pound-mass | lbm | 0.453592 kg | US aviation/aerospace |
//! | Metric tonne | t | 1000 kg | Large masses (aircraft, rockets) |
//! | Slug | slug | 14.5939 kg | US engineering (force = slug * ft/s^2) |
//!
//! THE POUND CONFUSION:
//! --------------------
//! "Pound" can mean two things:
//! 1. Pound-mass (lbm): A unit of MASS (= 0.453592 kg)
//! 2. Pound-force (lbf): A unit of FORCE (= 4.44822 N)
//!
//! This is a historical mess. In US customary units:
//! - 1 lbm on Earth surface weighs approximately 1 lbf
//! - But they're NOT the same thing!
//!
//! Newton's 2nd law in SI: F = m * a (neat!)
//! In US customary: F = m * a / gc where gc = 32.174 lbm*ft/(lbf*s^2)
//!
//! This library keeps them strictly separate to avoid confusion.
//!
//! AEROSPACE EXAMPLES:
//! ------------------
//! - Boeing 787-9 max takeoff mass: 254,000 kg
//! - A380 fuel capacity: ~253,000 kg of jet fuel
//! - Saturn V fully fueled: 2,970,000 kg (2,970 tonnes)
//! - Space Shuttle dry mass: ~78,000 kg
//!
//! =============================================================================
//! RUST CONCEPT: Same Pattern, Different Types
//! =============================================================================
//!
//! This file follows the exact same pattern as length.rs:
//! 1. Newtype struct with private field
//! 2. Constructor methods (from_*)
//! 3. Accessor methods (as_*)
//! 4. Operator trait implementations
//! 5. Display trait implementation
//! 6. Unit tests
//!
//! This consistency makes the codebase predictable and easy to understand.

use std::fmt;
use std::ops::{Add, Div, Mul, Sub};

// =============================================================================
// MASS STRUCT
// =============================================================================
/// Mass quantity - stores value in kilograms internally.
///
/// # Important: Mass != Weight
///
/// This type represents MASS (amount of matter), not weight (gravitational force).
/// To get weight, multiply mass by gravitational acceleration:
/// ```rust,ignore
/// let mass_kg = 70.0;
/// let g_earth = 9.80665;  // m/s^2
/// let weight_newtons = mass_kg * g_earth;  // 686 N
/// ```
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub struct Mass {
    kilograms: f64,
}

impl Mass {
    // =========================================================================
    // CONSTRUCTORS
    // =========================================================================

    /// Create a Mass from kilograms (SI base unit).
    ///
    /// # Example
    /// ```
    /// use aerospace_units::prelude::*;
    /// let fuel_mass = Mass::from_kilograms(50000.0);  // 50 tonnes
    /// ```
    pub fn from_kilograms(kg: f64) -> Self {
        Self { kilograms: kg }
    }

    /// Create a Mass from grams.
    ///
    /// Conversion: 1 g = 0.001 kg
    pub fn from_grams(g: f64) -> Self {
        Self { kilograms: g / 1000.0 }
    }

    /// Create a Mass from metric tonnes (megagrams).
    ///
    /// AEROSPACE: The preferred unit for large masses like aircraft
    /// and rockets. A tonne is exactly 1000 kg.
    ///
    /// Note: "Tonne" (metric) vs "Ton" (US short ton = 907 kg, UK long ton = 1016 kg)
    /// We use metric tonnes here.
    pub fn from_tonnes(t: f64) -> Self {
        Self { kilograms: t * 1000.0 }
    }

    /// Create a Mass from pounds-mass (avoirdupois).
    ///
    /// AEROSPACE: Common in US aviation. Be careful not to confuse
    /// with pounds-force (lbf)!
    ///
    /// Conversion: 1 lbm = 0.45359237 kg (exact, by international agreement)
    /// We use 0.453592 for practical calculations.
    pub fn from_pounds(lb: f64) -> Self {
        Self {
            kilograms: lb * 0.453592,
        }
    }

    /// Create a Mass from slugs.
    ///
    /// AEROSPACE: The slug is the US customary unit of mass where
    /// force = mass * acceleration works without conversion factors.
    /// 1 slug = 1 lbf * s^2 / ft
    ///
    /// Conversion: 1 slug = 14.593903 kg
    ///
    /// The slug is rarely used today except in some legacy calculations.
    pub fn from_slugs(slugs: f64) -> Self {
        Self {
            kilograms: slugs * 14.5939,
        }
    }

    // =========================================================================
    // ACCESSORS
    // =========================================================================

    /// Get value in kilograms.
    pub fn as_kilograms(&self) -> f64 {
        self.kilograms
    }

    /// Get value in grams.
    pub fn as_grams(&self) -> f64 {
        self.kilograms * 1000.0
    }

    /// Get value in metric tonnes.
    ///
    /// AEROSPACE: Useful for displaying large masses like aircraft weight.
    pub fn as_tonnes(&self) -> f64 {
        self.kilograms / 1000.0
    }

    /// Get value in pounds-mass.
    pub fn as_pounds(&self) -> f64 {
        self.kilograms / 0.453592
    }

    /// Get value in slugs.
    pub fn as_slugs(&self) -> f64 {
        self.kilograms / 14.5939
    }

    // =========================================================================
    // UTILITY METHODS
    // =========================================================================

    /// Check if this mass is positive.
    ///
    /// Mass should always be positive in physical systems.
    /// Negative mass would violate conservation laws!
    pub fn is_positive(&self) -> bool {
        self.kilograms > 0.0
    }
}

// =============================================================================
// OPERATOR IMPLEMENTATIONS
// =============================================================================

/// Mass + Mass = Mass
impl Add for Mass {
    type Output = Self;

    fn add(self, other: Self) -> Self {
        Self {
            kilograms: self.kilograms + other.kilograms,
        }
    }
}

/// Mass - Mass = Mass
///
/// AEROSPACE: Mass subtraction is common in rocket calculations:
/// - Propellant mass = Initial mass - Dry mass
/// - Fuel burned = Initial fuel - Remaining fuel
impl Sub for Mass {
    type Output = Self;

    fn sub(self, other: Self) -> Self {
        Self {
            kilograms: self.kilograms - other.kilograms,
        }
    }
}

/// Mass * scalar = Mass
impl Mul<f64> for Mass {
    type Output = Self;

    fn mul(self, scalar: f64) -> Self {
        Self {
            kilograms: self.kilograms * scalar,
        }
    }
}

/// Mass / scalar = Mass
impl Div<f64> for Mass {
    type Output = Self;

    fn div(self, scalar: f64) -> Self {
        Self {
            kilograms: self.kilograms / scalar,
        }
    }
}

/// Mass / Mass = dimensionless ratio (mass ratio)
///
/// AEROSPACE: THE MASS RATIO is crucial in rocket science!
///
/// The Tsiolkovsky rocket equation:
/// ```text
/// dv = ve * ln(m0 / mf)
///
/// where:
///   dv = change in velocity (delta-v)
///   ve = exhaust velocity
///   m0 = initial mass (with propellant)
///   mf = final mass (dry, propellant expended)
///   m0/mf = mass ratio
/// ```
///
/// Higher mass ratio = more delta-v capability.
/// Saturn V had mass ratio of ~23 for the first stage!
impl Div<Mass> for Mass {
    type Output = f64;

    /// Dividing two masses gives a dimensionless ratio (mass ratio).
    fn div(self, other: Mass) -> f64 {
        self.kilograms / other.kilograms
    }
}

/// Display implementation for human-readable output.
impl fmt::Display for Mass {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(f, "{:.2} kg", self.kilograms)
    }
}

// =============================================================================
// UNIT TESTS
// =============================================================================
#[cfg(test)]
mod tests {
    use super::*;

    /// Test basic unit conversions.
    #[test]
    fn test_conversions() {
        // 1 pound = 0.453592 kg
        let mass = Mass::from_pounds(1.0);
        assert!((mass.as_kilograms() - 0.453592).abs() < 0.0001);

        // 1 tonne = 1000 kg
        let tonnes = Mass::from_tonnes(1.0);
        assert!((tonnes.as_kilograms() - 1000.0).abs() < 0.0001);
    }

    /// Test mass ratio calculation.
    ///
    /// AEROSPACE: This test demonstrates the mass ratio for Saturn V.
    /// The first stage (S-IC) had:
    /// - Wet mass: ~2,290,000 kg
    /// - Dry mass: ~131,000 kg
    /// - Mass ratio: ~17.5 (for first stage alone)
    ///
    /// Total vehicle mass ratio was even higher!
    #[test]
    fn test_mass_ratio() {
        // Approximate Saturn V total mass ratio
        let initial = Mass::from_tonnes(2970.0);  // Fully fueled
        let final_mass = Mass::from_tonnes(130.0);  // Approximate dry mass
        let ratio: f64 = initial / final_mass;
        assert!((ratio - 22.85).abs() < 0.1);
    }
}
