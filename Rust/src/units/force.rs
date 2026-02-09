//! # Force/Thrust Unit Type
//!
//! Stores force internally in Newtons (SI derived unit).
//!
//! =============================================================================
//! AEROSPACE CONCEPT: Forces in Flight and Propulsion
//! =============================================================================
//!
//! The four forces of flight:
//! ```text
//!                  LIFT
//!                   ^
//!                   |
//!    DRAG <-----[Aircraft]-----> THRUST
//!                   |
//!                   v
//!                 WEIGHT
//!
//! For steady, level flight: Lift = Weight, Thrust = Drag
//! ```
//!
//! FORCE vs THRUST:
//! ----------------
//! - Force: General term (F = m * a)
//! - Thrust: Specifically the propulsive force from an engine
//! - Weight: Force due to gravity (W = m * g)
//! - Lift: Aerodynamic force perpendicular to airflow
//! - Drag: Aerodynamic force opposing motion
//!
//! FORCE UNITS:
//! ------------
//! | Unit | Symbol | Definition | Usage |
//! |------|--------|------------|-------|
//! | Newton | N | kg * m/s^2 | SI standard |
//! | Kilonewton | kN | 1000 N | Aircraft engines |
//! | Meganewton | MN | 1,000,000 N | Rocket engines |
//! | Pound-force | lbf | 4.44822 N | US aerospace |
//! | Kilopound-force | klbf | 1000 lbf = 4448.22 N | Large engines |
//!
//! AEROSPACE EXAMPLES:
//! ------------------
//! - Cessna 172 engine: ~1.2 kN (270 lbf)
//! - Boeing 737 MAX engine (CFM LEAP): 130 kN (29,000 lbf) each
//! - Boeing 777 engine (GE90): 514 kN (115,000 lbf)
//! - F-22 Raptor engine (F119): 156 kN (35,000 lbf) with afterburner
//! - SpaceX Merlin 1D (vacuum): 981 kN (220,500 lbf)
//! - Saturn V F-1 engine: 6.77 MN (1,522,000 lbf) - most powerful single-nozzle!
//! - Space Shuttle SRB: 12.5 MN (2,800,000 lbf) each
//!
//! =============================================================================
//! RUST CONCEPT: Cross-Type Operations
//! =============================================================================
//!
//! This file shows how different unit types can interact.
//! The `specificImpulse()` method uses both `Force` and `MassFlowRate`:
//!
//! ```rust,ignore
//! impl Force {
//!     pub fn specific_impulse(&self, mass_flow_rate: MassFlowRate) -> SpecificImpulse
//! }
//! ```
//!
//! This enforces that Isp is ONLY calculated from valid inputs.
//! You can't accidentally pass a mass or velocity - the types prevent it!

use std::fmt;
use std::ops::{Add, Div, Mul, Sub};

// Import related unit types for cross-type operations
use super::mass_flow_rate::MassFlowRate;
use super::specific_impulse::SpecificImpulse;

// =============================================================================
// FORCE STRUCT
// =============================================================================
/// Force quantity - stores value in Newtons internally.
///
/// # Newton: The SI Unit of Force
///
/// 1 Newton is defined as the force required to accelerate 1 kilogram
/// by 1 meter per second squared: N = kg * m/s^2
///
/// This comes directly from Newton's Second Law: F = m * a
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub struct Force {
    newtons: f64,
}

impl Force {
    // =========================================================================
    // CONSTRUCTORS
    // =========================================================================

    /// Create a Force from Newtons (SI derived unit).
    ///
    /// # Example
    /// ```
    /// use aerospace_units::prelude::*;
    /// let weight = Force::from_newtons(686.0);  // ~70 kg person on Earth
    /// ```
    pub fn from_newtons(n: f64) -> Self {
        Self { newtons: n }
    }

    /// Create a Force from kilonewtons.
    ///
    /// AEROSPACE: Common unit for aircraft engine thrust.
    pub fn from_kilonewtons(kn: f64) -> Self {
        Self { newtons: kn * 1000.0 }
    }

    /// Create a Force from meganewtons.
    ///
    /// AEROSPACE: Used for rocket engines where thrust is enormous.
    /// The Space Shuttle Main Engine produces about 2.3 MN at liftoff.
    pub fn from_meganewtons(mn: f64) -> Self {
        Self {
            newtons: mn * 1_000_000.0,
        }
    }

    /// Create a Force from pounds-force.
    ///
    /// AEROSPACE: THE standard unit in US aerospace industry.
    /// Conversion: 1 lbf = 4.4482216 N (exact: lbm * g0_ft/s2)
    ///
    /// WARNING: Don't confuse pound-force (lbf) with pound-mass (lbm)!
    /// - 1 lbf is the weight of 1 lbm on Earth at standard gravity
    /// - They are NOT the same physical quantity
    pub fn from_pounds_force(lbf: f64) -> Self {
        Self {
            newtons: lbf * 4.44822,
        }
    }

    /// Create a Force from kilopounds-force (kip).
    ///
    /// AEROSPACE: Used for large rocket engines in US units.
    /// The F-1 engine produces 1,522 klbf of thrust!
    pub fn from_kilopounds_force(klbf: f64) -> Self {
        Self {
            newtons: klbf * 4448.22,
        }
    }

    // =========================================================================
    // ACCESSORS
    // =========================================================================

    /// Get value in Newtons.
    pub fn as_newtons(&self) -> f64 {
        self.newtons
    }

    /// Get value in kilonewtons.
    pub fn as_kilonewtons(&self) -> f64 {
        self.newtons / 1000.0
    }

    /// Get value in meganewtons.
    pub fn as_meganewtons(&self) -> f64 {
        self.newtons / 1_000_000.0
    }

    /// Get value in pounds-force.
    pub fn as_pounds_force(&self) -> f64 {
        self.newtons / 4.44822
    }

    /// Get value in kilopounds-force.
    pub fn as_kilopounds_force(&self) -> f64 {
        self.newtons / 4448.22
    }

    // =========================================================================
    // AEROSPACE CALCULATIONS
    // =========================================================================

    /// Calculate specific impulse from thrust and mass flow rate.
    ///
    /// AEROSPACE CONCEPT: Specific Impulse (Isp)
    /// -----------------------------------------
    /// Specific impulse is THE key metric for rocket engine efficiency.
    /// It tells you how long 1 kg of propellant can produce 1 N of thrust.
    ///
    /// Formula: Isp = F / (mdot * g0)
    ///
    /// Where:
    /// - F = thrust [N]
    /// - mdot = mass flow rate [kg/s]
    /// - g0 = standard gravity = 9.80665 m/s^2
    /// - Isp = specific impulse [s]
    ///
    /// Higher Isp = more efficient = more delta-v per kg of propellant
    ///
    /// Typical values:
    /// - Solid rockets: 250-290 s
    /// - RP-1/LOX (Merlin, F-1): 260-310 s
    /// - LH2/LOX (RS-25, J-2): 360-450 s
    /// - Ion thrusters: 1500-5000 s (but very low thrust)
    ///
    /// # Example
    /// ```
    /// use aerospace_units::prelude::*;
    /// let thrust = Force::from_newtons(6_770_000.0);  // F-1 engine
    /// let mdot = MassFlowRate::from_kg_per_s(2578.0);
    /// let isp = thrust.specific_impulse(mdot);
    /// // isp is about 268 seconds (sea level)
    /// ```
    pub fn specific_impulse(&self, mass_flow_rate: MassFlowRate) -> SpecificImpulse {
        const G0: f64 = 9.80665; // Standard gravity [m/s^2]
        let isp_seconds = self.newtons / (mass_flow_rate.as_kg_per_s() * G0);
        SpecificImpulse::from_seconds(isp_seconds)
    }

    // =========================================================================
    // UTILITY METHODS
    // =========================================================================

    /// Check if this force is positive.
    ///
    /// In most coordinate systems, positive thrust means forward/up.
    pub fn is_positive(&self) -> bool {
        self.newtons > 0.0
    }

    /// Get the absolute value of this force.
    pub fn abs(&self) -> Self {
        Self {
            newtons: self.newtons.abs(),
        }
    }
}

// =============================================================================
// OPERATOR IMPLEMENTATIONS
// =============================================================================

/// Force + Force = Force
///
/// AEROSPACE: Adding forces (vector addition) is fundamental to flight analysis.
/// Example: Total thrust from multiple engines.
impl Add for Force {
    type Output = Self;

    fn add(self, other: Self) -> Self {
        Self {
            newtons: self.newtons + other.newtons,
        }
    }
}

/// Force - Force = Force
///
/// AEROSPACE: Net force calculation.
/// Example: Thrust - Drag = Net forward force
impl Sub for Force {
    type Output = Self;

    fn sub(self, other: Self) -> Self {
        Self {
            newtons: self.newtons - other.newtons,
        }
    }
}

/// Force * scalar = Force
impl Mul<f64> for Force {
    type Output = Self;

    fn mul(self, scalar: f64) -> Self {
        Self {
            newtons: self.newtons * scalar,
        }
    }
}

/// Force / scalar = Force
impl Div<f64> for Force {
    type Output = Self;

    fn div(self, scalar: f64) -> Self {
        Self {
            newtons: self.newtons / scalar,
        }
    }
}

/// Force / Force = dimensionless ratio (thrust-to-weight ratio)
///
/// AEROSPACE: Thrust-to-weight ratio (TWR) is critical:
/// - TWR > 1 required for vertical takeoff
/// - TWR affects acceleration capability
/// - Fighter jets: TWR ~1.0-1.4
/// - Saturn V at liftoff: TWR ~1.2
impl Div<Force> for Force {
    type Output = f64;

    fn div(self, other: Force) -> f64 {
        self.newtons / other.newtons
    }
}

/// Display implementation with automatic unit scaling.
///
/// RUST CONCEPT: Conditional Formatting
/// ------------------------------------
/// The Display implementation chooses appropriate units based on magnitude.
/// This is more readable than always showing raw Newtons.
impl fmt::Display for Force {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        if self.newtons.abs() >= 1_000_000.0 {
            write!(f, "{:.2} MN", self.as_meganewtons())
        } else if self.newtons.abs() >= 1000.0 {
            write!(f, "{:.2} kN", self.as_kilonewtons())
        } else {
            write!(f, "{:.2} N", self.newtons)
        }
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
        // 1 lbf = 4.44822 N
        let force = Force::from_pounds_force(1.0);
        assert!((force.as_newtons() - 4.44822).abs() < 0.0001);

        // 1 kN = 1000 N
        let kn = Force::from_kilonewtons(1.0);
        assert!((kn.as_newtons() - 1000.0).abs() < 0.0001);
    }

    /// Test Saturn V F-1 engine thrust conversion.
    ///
    /// AEROSPACE: The F-1 was the most powerful single-chamber liquid-fueled
    /// rocket engine ever flown. Five F-1s powered the Saturn V first stage.
    #[test]
    fn test_f1_engine() {
        // Saturn V F-1 engine thrust: 6.77 MN
        let thrust = Force::from_meganewtons(6.77);
        // Should be about 1,522,000 lbf
        assert!((thrust.as_pounds_force() - 1_522_000.0).abs() < 1000.0);
    }
}
