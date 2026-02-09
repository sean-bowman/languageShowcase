//! # Velocity Unit Type
//!
//! Stores velocity internally in meters per second (SI derived unit).
//!
//! =============================================================================
//! AEROSPACE CONCEPT: Velocity and Airspeed
//! =============================================================================
//!
//! Velocity in aerospace is surprisingly complex. There are multiple "speeds":
//!
//! | Type | Abbrev | What It Is |
//! |------|--------|------------|
//! | Indicated Airspeed | IAS | What the cockpit instrument shows |
//! | Calibrated Airspeed | CAS | IAS corrected for instrument/position error |
//! | Equivalent Airspeed | EAS | CAS corrected for compressibility (>Mach 0.3) |
//! | True Airspeed | TAS | Actual speed through the air mass |
//! | Ground Speed | GS | Speed relative to ground (TAS +/- wind) |
//!
//! WHY SO MANY?
//! -----------
//! Aircraft instruments measure DYNAMIC PRESSURE (q = 0.5 * rho * v^2).
//! At altitude, air density (rho) decreases, so the same IAS means higher TAS.
//! A Boeing 737 at FL350 might show IAS = 280 kts but actually TAS = 480 kts!
//!
//! VELOCITY UNITS:
//! ---------------
//! - Knots (kts): 1 nm/hr = 1.852 km/hr = 0.514444 m/s
//!   THE standard unit in aviation worldwide
//! - Mach: Ratio to local speed of sound (varies with temperature!)
//!   Sea level, 15degC: Mach 1 = 340.29 m/s = 661.5 kts
//!   At FL350 (-56.5degC): Mach 1 = 295 m/s = 573 kts
//! - km/h: Ground operations, some Russian/Chinese aviation
//! - ft/s: Rocket propulsion, some engineering calculations
//!
//! IMPORTANT SPEEDS IN AVIATION:
//! - V1: Takeoff decision speed (commit to takeoff)
//! - Vr: Rotation speed (pull back on stick)
//! - V2: Takeoff safety speed (minimum climb speed with one engine failed)
//! - Vne: Never exceed speed (structural limit)
//! - Mmo: Maximum operating Mach number
//!
//! =============================================================================
//! RUST CONCEPT: Derived Units
//! =============================================================================
//!
//! Velocity is a "derived unit" in SI - it's length/time (m/s).
//! We could theoretically compute it from Length and Time types:
//! ```rust,ignore
//! let velocity = length / time;  // Would need: impl Div<Time> for Length
//! ```
//!
//! For simplicity, we treat Velocity as its own fundamental type here.
//! A more sophisticated library might use dimensional analysis.

use std::fmt;
use std::ops::{Add, Div, Mul, Sub};

// =============================================================================
// VELOCITY STRUCT
// =============================================================================
/// Velocity quantity - stores value in m/s internally.
///
/// # Internal Representation
///
/// Meters per second (m/s) is the SI derived unit for velocity.
/// All conversions are done through m/s as the common base.
///
/// # Common Aerospace Velocities
///
/// - Walking speed: ~1.4 m/s (5 km/h)
/// - Highway driving: ~30 m/s (110 km/h)
/// - Commercial jet cruise: ~250 m/s (485 kts TAS, Mach 0.85)
/// - Speed of sound (sea level): 340.29 m/s
/// - SR-71 Blackbird: ~980 m/s (Mach 3.2)
/// - ISS orbital velocity: ~7,660 m/s
/// - Earth escape velocity: ~11,186 m/s
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub struct Velocity {
    meters_per_second: f64,
}

impl Velocity {
    // =========================================================================
    // CONSTRUCTORS
    // =========================================================================

    /// Create a Velocity from meters per second (SI derived unit).
    ///
    /// # Example
    /// ```
    /// use aerospace_units::prelude::*;
    /// let orbital_velocity = Velocity::from_meters_per_second(7660.0);
    /// ```
    pub fn from_meters_per_second(mps: f64) -> Self {
        Self {
            meters_per_second: mps,
        }
    }

    /// Create a Velocity from kilometers per second.
    ///
    /// AEROSPACE: Used for orbital mechanics and escape velocities.
    /// Earth escape velocity is ~11.2 km/s.
    pub fn from_kilometers_per_second(kmps: f64) -> Self {
        Self {
            meters_per_second: kmps * 1000.0,
        }
    }

    /// Create a Velocity from kilometers per hour.
    ///
    /// AEROSPACE: Used for ground operations and some international aviation.
    ///
    /// Conversion: 1 km/h = 1000m / 3600s = 1/3.6 m/s
    pub fn from_kilometers_per_hour(kmph: f64) -> Self {
        Self {
            meters_per_second: kmph / 3.6,
        }
    }

    /// Create a Velocity from feet per second.
    ///
    /// AEROSPACE: Used in rocket propulsion and some US engineering.
    /// Saturn V exhaust velocity: ~2,580 m/s = 8,465 ft/s
    pub fn from_feet_per_second(fps: f64) -> Self {
        Self {
            meters_per_second: fps * 0.3048,
        }
    }

    /// Create a Velocity from knots (nautical miles per hour).
    ///
    /// AEROSPACE: THE standard velocity unit in aviation worldwide.
    /// - ATC assigns speeds in knots
    /// - Flight plans use knots
    /// - Weather reports use knots for wind
    ///
    /// Conversion: 1 knot = 1852 m / 3600 s = 0.514444 m/s
    ///
    /// Historical note: "Knot" comes from the practice of measuring ship
    /// speed by counting knots passing through sailors' hands on a rope
    /// attached to a floating log (the "chip log").
    pub fn from_knots(kts: f64) -> Self {
        Self {
            meters_per_second: kts * 0.514444,
        }
    }

    /// Create a Velocity from miles per hour.
    ///
    /// Conversion: 1 mph = 1609.344 m / 3600 s = 0.44704 m/s
    pub fn from_miles_per_hour(mph: f64) -> Self {
        Self {
            meters_per_second: mph * 0.44704,
        }
    }

    /// Create a Velocity from Mach number given speed of sound.
    ///
    /// AEROSPACE: Mach number is velocity divided by local speed of sound.
    ///
    /// CRITICAL: Speed of sound varies with temperature, NOT altitude directly!
    ///   a = sqrt(gamma * R * T) where:
    ///   - gamma = 1.4 for air
    ///   - R = 287 J/(kg*K) specific gas constant for air
    ///   - T = absolute temperature in Kelvin
    ///
    /// At sea level (15degC): 340.29 m/s
    /// At FL350 (-56.5degC): ~295 m/s
    ///
    /// # Parameters
    /// - `mach`: Mach number (dimensionless ratio)
    /// - `speed_of_sound_mps`: Local speed of sound in m/s
    ///
    /// # Example
    /// ```
    /// use aerospace_units::prelude::*;
    /// let sea_level_sos = 340.29;
    /// let supersonic = Velocity::from_mach(1.5, sea_level_sos);
    /// ```
    pub fn from_mach(mach: f64, speed_of_sound_mps: f64) -> Self {
        Self {
            meters_per_second: mach * speed_of_sound_mps,
        }
    }

    // =========================================================================
    // ACCESSORS
    // =========================================================================

    /// Get value in meters per second (SI derived unit).
    pub fn as_meters_per_second(&self) -> f64 {
        self.meters_per_second
    }

    /// Get value in kilometers per second.
    pub fn as_kilometers_per_second(&self) -> f64 {
        self.meters_per_second / 1000.0
    }

    /// Get value in kilometers per hour.
    pub fn as_kilometers_per_hour(&self) -> f64 {
        self.meters_per_second * 3.6
    }

    /// Get value in feet per second.
    pub fn as_feet_per_second(&self) -> f64 {
        self.meters_per_second / 0.3048
    }

    /// Get value in knots.
    ///
    /// AEROSPACE: Use this for flight planning, ATC communications,
    /// and standard aviation operations.
    pub fn as_knots(&self) -> f64 {
        self.meters_per_second / 0.514444
    }

    /// Get value in miles per hour.
    pub fn as_miles_per_hour(&self) -> f64 {
        self.meters_per_second / 0.44704
    }

    /// Get Mach number given the local speed of sound.
    ///
    /// AEROSPACE: For high-speed flight, Mach number matters more than
    /// indicated airspeed because aerodynamic effects (shock waves,
    /// compressibility) depend on Mach number.
    ///
    /// # Parameters
    /// - `speed_of_sound_mps`: Local speed of sound in m/s
    ///
    /// # Returns
    /// Mach number (dimensionless). M < 1 is subsonic, M > 1 is supersonic.
    pub fn as_mach(&self, speed_of_sound_mps: f64) -> f64 {
        self.meters_per_second / speed_of_sound_mps
    }

    // =========================================================================
    // UTILITY METHODS
    // =========================================================================

    /// Check if this velocity is positive.
    ///
    /// Note: Velocity is a vector quantity (has direction), but we store
    /// only magnitude here. "Negative" velocity might represent backward
    /// motion in some coordinate systems.
    pub fn is_positive(&self) -> bool {
        self.meters_per_second > 0.0
    }

    /// Get the absolute value of this velocity.
    pub fn abs(&self) -> Self {
        Self {
            meters_per_second: self.meters_per_second.abs(),
        }
    }
}

// =============================================================================
// OPERATOR IMPLEMENTATIONS
// =============================================================================
// See length.rs for detailed explanation of operator overloading.

/// Velocity + Velocity = Velocity
///
/// AEROSPACE: Adding velocities (vector addition) is essential for:
/// - Ground speed = TAS + wind
/// - Relative velocity calculations
/// - Delta-v budgets in orbital mechanics
impl Add for Velocity {
    type Output = Self;

    fn add(self, other: Self) -> Self {
        Self {
            meters_per_second: self.meters_per_second + other.meters_per_second,
        }
    }
}

/// Velocity - Velocity = Velocity
///
/// AEROSPACE: Velocity differences are used for:
/// - Delta-v (change in velocity) for orbital maneuvers
/// - Closing speed between aircraft
/// - Relative velocity for docking operations
impl Sub for Velocity {
    type Output = Self;

    fn sub(self, other: Self) -> Self {
        Self {
            meters_per_second: self.meters_per_second - other.meters_per_second,
        }
    }
}

/// Velocity * scalar = Velocity
impl Mul<f64> for Velocity {
    type Output = Self;

    fn mul(self, scalar: f64) -> Self {
        Self {
            meters_per_second: self.meters_per_second * scalar,
        }
    }
}

/// Velocity / scalar = Velocity
impl Div<f64> for Velocity {
    type Output = Self;

    fn div(self, scalar: f64) -> Self {
        Self {
            meters_per_second: self.meters_per_second / scalar,
        }
    }
}

/// Velocity / Velocity = dimensionless ratio
///
/// AEROSPACE: Velocity ratios appear in:
/// - Mach number (velocity / speed of sound)
/// - Mass ratio equation: dv = ve * ln(m0/mf), rearranged
impl Div<Velocity> for Velocity {
    type Output = f64;

    fn div(self, other: Velocity) -> f64 {
        self.meters_per_second / other.meters_per_second
    }
}

/// Display implementation for human-readable output.
impl fmt::Display for Velocity {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(f, "{:.2} m/s", self.meters_per_second)
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
        // 100 knots in m/s
        let v = Velocity::from_knots(100.0);
        assert!((v.as_meters_per_second() - 51.4444).abs() < 0.001);

        // 100 km/h in m/s
        let v2 = Velocity::from_kilometers_per_hour(100.0);
        assert!((v2.as_meters_per_second() - 27.778).abs() < 0.01);
    }

    /// Test Mach number calculations.
    ///
    /// AEROSPACE: This test uses sea level standard conditions.
    /// At 15degC, speed of sound = 340.29 m/s.
    #[test]
    fn test_mach() {
        let speed_of_sound = 340.29; // m/s at sea level, 15degC
        let v = Velocity::from_mach(2.0, speed_of_sound);

        // Mach 2 = 680.58 m/s
        assert!((v.as_meters_per_second() - 680.58).abs() < 0.01);

        // Convert back to Mach
        assert!((v.as_mach(speed_of_sound) - 2.0).abs() < 0.0001);
    }
}
