//! # Mass Flow Rate Unit Type
//!
//! Stores mass flow rate internally in kg/s (SI derived unit).
//!
//! =============================================================================
//! AEROSPACE CONCEPT: Mass Flow Rate in Propulsion
//! =============================================================================
//!
//! Mass flow rate (mdot, pronounced "m-dot") is the rate at which mass passes
//! through a system. In propulsion, it's how fast we're consuming propellant.
//!
//! SYMBOL: mdot = dm/dt (derivative of mass with respect to time)
//!
//! KEY EQUATION FOR ROCKET THRUST:
//! -------------------------------
//! ```text
//! F = mdot * v_e
//!
//! where:
//!   F = thrust [N]
//!   mdot = mass flow rate [kg/s]
//!   v_e = effective exhaust velocity [m/s]
//! ```
//!
//! This is Newton's Second Law applied to a rocket: The thrust equals the
//! rate of momentum being expelled (mass flow times velocity).
//!
//! MASS FLOW IN DIFFERENT ENGINES:
//! -------------------------------
//! | Engine | mdot (kg/s) | Thrust (kN) | Isp (s) |
//! |--------|-------------|-------------|---------|
//! | SpaceX Merlin 1D (vac) | 280 | 981 | 348 |
//! | Saturn V F-1 | 2,578 | 6,770 | 263 |
//! | RS-25 (SSME) | 468 | 2,090 | 452 |
//! | Raptor 2 | 650 | 2,300 | 350 |
//!
//! WHY DOES MASS FLOW MATTER?
//! --------------------------
//! - Higher mdot = more thrust (good for liftoff)
//! - Higher mdot = faster propellant consumption (less burn time)
//! - For a given thrust, lower mdot means higher Isp (more efficient)
//!
//! BURN TIME CALCULATION:
//! ----------------------
//! ```text
//! t_burn = m_propellant / mdot
//!
//! Example: F-1 engine
//!   Propellant: 2,077,000 kg (LOX + RP-1)
//!   mdot: 2,578 kg/s
//!   Burn time: 2,077,000 / 2,578 = 805 seconds (actually ~150s per engine,
//!              but 5 engines total, so stages add up)
//! ```
//!
//! =============================================================================
//! RUST CONCEPT: Rate Types
//! =============================================================================
//!
//! Mass flow rate is a "rate type" - it has time in the denominator.
//! This pattern (quantity per time) appears often in physics:
//! - Mass flow: kg/s
//! - Volume flow: m^3/s
//! - Angular velocity: rad/s
//!
//! We could theoretically compute it as Mass / Time if we had a Time type:
//! ```rust,ignore
//! impl Div<Time> for Mass {
//!     type Output = MassFlowRate;
//! }
//! ```
//!
//! For simplicity, we treat MassFlowRate as its own fundamental type.

use std::fmt;
use std::ops::{Add, Div, Mul, Sub};

// =============================================================================
// MASS FLOW RATE STRUCT
// =============================================================================
/// Mass flow rate quantity - stores value in kg/s internally.
///
/// # Notation
///
/// In engineering, mass flow rate is often written as:
/// - mdot (with a dot over m): dm/dt
/// - m-dot (spoken as "m-dot")
///
/// The dot notation comes from Newton's notation for derivatives.
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub struct MassFlowRate {
    kg_per_s: f64,
}

impl MassFlowRate {
    // =========================================================================
    // CONSTRUCTORS
    // =========================================================================

    /// Create a MassFlowRate from kilograms per second (SI derived unit).
    ///
    /// # Example
    /// ```
    /// use aerospace_units::prelude::*;
    /// let mdot = MassFlowRate::from_kg_per_s(2578.0);  // F-1 engine
    /// ```
    pub fn from_kg_per_s(kgps: f64) -> Self {
        Self { kg_per_s: kgps }
    }

    /// Create a MassFlowRate from pounds per second.
    ///
    /// AEROSPACE: Common in US aerospace for engine specifications.
    pub fn from_lb_per_s(lbps: f64) -> Self {
        Self {
            kg_per_s: lbps * 0.453592,
        }
    }

    /// Create a MassFlowRate from tonnes per second.
    ///
    /// AEROSPACE: Useful for very large engines or combined flows.
    /// The Saturn V first stage had about 2.6 tonnes/s of propellant flow!
    pub fn from_tonnes_per_s(tps: f64) -> Self {
        Self {
            kg_per_s: tps * 1000.0,
        }
    }

    /// Create a MassFlowRate from kilograms per minute.
    ///
    /// Sometimes used for smaller systems or fuel consumption rates.
    pub fn from_kg_per_min(kgpm: f64) -> Self {
        Self {
            kg_per_s: kgpm / 60.0,
        }
    }

    // =========================================================================
    // ACCESSORS
    // =========================================================================

    /// Get value in kilograms per second.
    pub fn as_kg_per_s(&self) -> f64 {
        self.kg_per_s
    }

    /// Get value in pounds per second.
    pub fn as_lb_per_s(&self) -> f64 {
        self.kg_per_s / 0.453592
    }

    /// Get value in tonnes per second.
    pub fn as_tonnes_per_s(&self) -> f64 {
        self.kg_per_s / 1000.0
    }

    /// Get value in kilograms per minute.
    pub fn as_kg_per_min(&self) -> f64 {
        self.kg_per_s * 60.0
    }

    // =========================================================================
    // UTILITY METHODS
    // =========================================================================

    /// Check if this mass flow rate is positive.
    ///
    /// Mass flow should be positive in normal operation
    /// (mass leaving the system, not entering).
    pub fn is_positive(&self) -> bool {
        self.kg_per_s > 0.0
    }
}

// =============================================================================
// OPERATOR IMPLEMENTATIONS
// =============================================================================

/// MassFlowRate + MassFlowRate = MassFlowRate
///
/// AEROSPACE: Add flow rates from multiple sources.
/// Example: Total propellant flow = oxidizer_flow + fuel_flow
impl Add for MassFlowRate {
    type Output = Self;

    fn add(self, other: Self) -> Self {
        Self {
            kg_per_s: self.kg_per_s + other.kg_per_s,
        }
    }
}

/// MassFlowRate - MassFlowRate = MassFlowRate
impl Sub for MassFlowRate {
    type Output = Self;

    fn sub(self, other: Self) -> Self {
        Self {
            kg_per_s: self.kg_per_s - other.kg_per_s,
        }
    }
}

/// MassFlowRate * scalar = MassFlowRate
///
/// AEROSPACE: Throttle settings scale mass flow.
/// At 50% throttle, mdot_actual = mdot_full * 0.5
impl Mul<f64> for MassFlowRate {
    type Output = Self;

    fn mul(self, scalar: f64) -> Self {
        Self {
            kg_per_s: self.kg_per_s * scalar,
        }
    }
}

/// MassFlowRate / scalar = MassFlowRate
impl Div<f64> for MassFlowRate {
    type Output = Self;

    fn div(self, scalar: f64) -> Self {
        Self {
            kg_per_s: self.kg_per_s / scalar,
        }
    }
}

/// Display implementation for human-readable output.
impl fmt::Display for MassFlowRate {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(f, "{:.2} kg/s", self.kg_per_s)
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
        // 1 lb/s = 0.453592 kg/s
        let mdot = MassFlowRate::from_lb_per_s(1.0);
        assert!((mdot.as_kg_per_s() - 0.453592).abs() < 0.0001);
    }

    /// Test F-1 engine mass flow rate.
    ///
    /// AEROSPACE: The F-1 had enormous propellant consumption.
    /// It burned through ~2,600 kg of propellant every second!
    #[test]
    fn test_f1_engine_mass_flow() {
        // F-1 engine: ~2578 kg/s propellant flow
        let mdot = MassFlowRate::from_kg_per_s(2578.0);
        // Convert to lb/s for verification
        assert!((mdot.as_lb_per_s() - 5683.0).abs() < 10.0);
    }
}
