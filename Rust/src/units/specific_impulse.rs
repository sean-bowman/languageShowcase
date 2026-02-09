//! # Specific Impulse (Isp) Unit Type
//!
//! Stores specific impulse internally in seconds (standard aerospace convention).
//! Can also represent as effective exhaust velocity in m/s.
//!
//! =============================================================================
//! AEROSPACE CONCEPT: Specific Impulse - THE Rocket Efficiency Metric
//! =============================================================================
//!
//! Specific impulse (Isp) is THE most important metric for rocket engine
//! efficiency. It tells you how much "punch" you get from each unit of
//! propellant.
//!
//! DEFINITION:
//! -----------
//! Isp = Thrust / (Mass_flow_rate * g0)
//!
//! Where g0 = 9.80665 m/s^2 (standard gravity)
//!
//! Units: seconds (yes, time! It's how long 1 kg of propellant can produce
//! 1 Newton of thrust, or equivalently, 1 lb of propellant producing 1 lbf)
//!
//! PHYSICAL MEANING:
//! -----------------
//! Isp is directly related to exhaust velocity:
//!
//!   v_e = Isp * g0
//!
//! Higher exhaust velocity = propellant leaves faster = more momentum transfer
//!
//! WHY Isp MATTERS (The Rocket Equation):
//! --------------------------------------
//! ```text
//! dv = v_e * ln(m0/mf) = Isp * g0 * ln(m0/mf)
//!
//! where:
//!   dv = change in velocity (delta-v)
//!   v_e = effective exhaust velocity
//!   m0 = initial mass
//!   mf = final mass (after propellant burned)
//!   ln = natural logarithm
//! ```
//!
//! This is Tsiolkovsky's rocket equation, and it shows that delta-v is
//! proportional to Isp. Double the Isp = double the delta-v capability!
//!
//! TYPICAL Isp VALUES:
//! ------------------
//! | Engine Type | Propellants | Isp (s) | Notes |
//! |-------------|-------------|---------|-------|
//! | Solid rocket | APCP | 250-290 | Simple, reliable |
//! | Kerosene/LOX | RP-1/LOX | 260-310 | Merlin, F-1 |
//! | Methane/LOX | CH4/LOX | 320-360 | Raptor (SpaceX) |
//! | Hydrogen/LOX | LH2/LOX | 360-450 | RS-25, J-2, Vulcain |
//! | Hypergolic | N2O4/MMH | 290-310 | Storable, used in space |
//! | Ion thruster | Xenon | 1500-5000 | Very efficient, low thrust |
//! | Nuclear thermal | H2 + reactor | 800-900 | Theoretical advantage |
//!
//! WHY THE VARIATION?
//! ------------------
//! Isp depends on:
//! 1. EXHAUST MOLECULAR WEIGHT: Lighter = faster = higher Isp
//!    (That's why H2/LOX beats RP-1/LOX - water vapor is lighter than CO2)
//! 2. COMBUSTION TEMPERATURE: Hotter = faster = higher Isp
//! 3. NOZZLE EXPANSION: Better expansion = more velocity = higher Isp
//!
//! VACUUM vs SEA LEVEL:
//! --------------------
//! Isp is lower at sea level because atmospheric pressure pushes back on
//! the exhaust. In vacuum, the exhaust can expand fully.
//!
//! Example: RS-25 (Space Shuttle Main Engine)
//!   Sea level: 366 s
//!   Vacuum: 452 s
//!
//! =============================================================================
//! RUST CONCEPT: Associated Constants
//! =============================================================================
//!
//! `pub const G0: f64 = 9.80665;`
//!
//! This defines a constant associated with the SpecificImpulse type.
//! Benefits:
//! - Namespaced: `SpecificImpulse::G0` is unambiguous
//! - Always available when the type is in scope
//! - Compile-time constant (zero runtime cost)

use std::fmt;
use std::ops::{Add, Div, Mul, Sub};

// =============================================================================
// SPECIFIC IMPULSE STRUCT
// =============================================================================
/// Specific Impulse quantity - stores value in seconds internally.
///
/// # Why Seconds?
///
/// Seconds are the traditional unit because:
/// 1. It's the same in metric and imperial systems (a rare convenience!)
/// 2. Easy to compare engines regardless of unit system
/// 3. Directly meaningful: "how long can 1 unit mass produce 1 unit thrust"
///
/// # Exhaust Velocity Relationship
///
/// v_e = Isp * g0
///
/// So 300 s Isp = 2942 m/s exhaust velocity
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub struct SpecificImpulse {
    seconds: f64,
}

impl SpecificImpulse {
    /// Standard gravity constant [m/s^2].
    ///
    /// AEROSPACE: This is the EXACT value by definition.
    /// It's used to convert between Isp (seconds) and exhaust velocity (m/s).
    ///
    /// Historical note: This was originally the measured gravitational
    /// acceleration at sea level at 45 degrees latitude. Now it's just
    /// a defined constant for unit conversion.
    ///
    /// RUST CONCEPT: Associated Constant
    /// ---------------------------------
    /// `pub const` inside an impl block creates an associated constant.
    /// Access it as: `SpecificImpulse::G0`
    pub const G0: f64 = 9.80665;

    // =========================================================================
    // CONSTRUCTORS
    // =========================================================================

    /// Create a SpecificImpulse from seconds (standard aerospace unit).
    ///
    /// # Example
    /// ```
    /// use aerospace_units::prelude::*;
    /// let merlin_isp = SpecificImpulse::from_seconds(311.0);  // Merlin 1D vacuum
    /// ```
    pub fn from_seconds(s: f64) -> Self {
        Self { seconds: s }
    }

    /// Create a SpecificImpulse from effective exhaust velocity in m/s.
    ///
    /// AEROSPACE: Sometimes you know exhaust velocity from analysis
    /// (e.g., from the ideal rocket equation or CFD simulation).
    ///
    /// # Example
    /// ```
    /// use aerospace_units::prelude::*;
    /// let isp = SpecificImpulse::from_exhaust_velocity(3000.0);  // 3 km/s
    /// // isp is about 306 seconds
    /// ```
    pub fn from_exhaust_velocity(ve_mps: f64) -> Self {
        Self {
            seconds: ve_mps / Self::G0,
        }
    }

    // =========================================================================
    // ACCESSORS
    // =========================================================================

    /// Get value in seconds (the aerospace standard).
    pub fn as_seconds(&self) -> f64 {
        self.seconds
    }

    /// Get value as effective exhaust velocity in m/s.
    ///
    /// AEROSPACE: This is what goes into the rocket equation:
    /// dv = v_e * ln(m0/mf)
    pub fn as_exhaust_velocity(&self) -> f64 {
        self.seconds * Self::G0
    }

    /// Get value as effective exhaust velocity in km/s.
    ///
    /// AEROSPACE: Convenient for orbital mechanics where km/s is common.
    /// Earth escape velocity is about 11.2 km/s for reference.
    pub fn as_exhaust_velocity_kmps(&self) -> f64 {
        self.seconds * Self::G0 / 1000.0
    }

    // =========================================================================
    // UTILITY METHODS
    // =========================================================================

    /// Check if this Isp is positive.
    ///
    /// Isp should always be positive for a functioning engine.
    /// Negative would mean... the propellant is somehow slowing down?
    pub fn is_positive(&self) -> bool {
        self.seconds > 0.0
    }
}

// =============================================================================
// OPERATOR IMPLEMENTATIONS
// =============================================================================
// Note: Adding/subtracting specific impulses isn't physically meaningful
// in most cases, but it's sometimes useful for error calculations or
// weighted averages.

/// SpecificImpulse + SpecificImpulse = SpecificImpulse
impl Add for SpecificImpulse {
    type Output = Self;

    fn add(self, other: Self) -> Self {
        Self {
            seconds: self.seconds + other.seconds,
        }
    }
}

/// SpecificImpulse - SpecificImpulse = SpecificImpulse
///
/// AEROSPACE: Isp difference might be used to compare engines.
/// "How much better is the RS-25 than the F-1? 452 - 263 = 189 seconds better!"
impl Sub for SpecificImpulse {
    type Output = Self;

    fn sub(self, other: Self) -> Self {
        Self {
            seconds: self.seconds - other.seconds,
        }
    }
}

/// SpecificImpulse * scalar = SpecificImpulse
impl Mul<f64> for SpecificImpulse {
    type Output = Self;

    fn mul(self, scalar: f64) -> Self {
        Self {
            seconds: self.seconds * scalar,
        }
    }
}

/// SpecificImpulse / scalar = SpecificImpulse
impl Div<f64> for SpecificImpulse {
    type Output = Self;

    fn div(self, scalar: f64) -> Self {
        Self {
            seconds: self.seconds / scalar,
        }
    }
}

/// Display implementation for human-readable output.
impl fmt::Display for SpecificImpulse {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(f, "{:.1} s", self.seconds)
    }
}

// =============================================================================
// UNIT TESTS
// =============================================================================
#[cfg(test)]
mod tests {
    use super::*;

    /// Test exhaust velocity conversion.
    ///
    /// AEROSPACE: Verifying v_e = Isp * g0
    #[test]
    fn test_exhaust_velocity_conversion() {
        // 300 s Isp = 2942 m/s exhaust velocity
        let isp = SpecificImpulse::from_seconds(300.0);
        assert!((isp.as_exhaust_velocity() - 2942.0).abs() < 1.0);
    }

    /// Test round-trip conversion (exhaust velocity -> Isp -> exhaust velocity).
    #[test]
    fn test_round_trip() {
        let ve = 3000.0; // m/s
        let isp = SpecificImpulse::from_exhaust_velocity(ve);
        assert!((isp.as_exhaust_velocity() - ve).abs() < 0.01);
    }

    /// Test typical real-world engine values.
    ///
    /// AEROSPACE: These are actual engine specifications!
    #[test]
    fn test_typical_values() {
        // SpaceX Merlin 1D vacuum: ~311 s
        let merlin = SpecificImpulse::from_seconds(311.0);
        assert!((merlin.as_exhaust_velocity_kmps() - 3.05).abs() < 0.05);

        // RS-25 (SSME) vacuum: ~452 s - one of the most efficient chemical engines
        let rs25 = SpecificImpulse::from_seconds(452.0);
        assert!((rs25.as_exhaust_velocity_kmps() - 4.43).abs() < 0.05);
    }
}
