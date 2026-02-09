//! # Pressure Unit Type
//!
//! Stores pressure internally in Pascals (SI derived unit).
//!
//! =============================================================================
//! AEROSPACE CONCEPT: Pressure in Aviation
//! =============================================================================
//!
//! Pressure is EVERYWHERE in aerospace:
//!
//! 1. ATMOSPHERIC PRESSURE
//!    - Decreases with altitude (barometric formula)
//!    - Standard sea level: 101,325 Pa = 1013.25 hPa = 29.92 inHg
//!    - At FL350 (35,000 ft): ~23,842 Pa (~24% of sea level)
//!    - Used for altimeter setting and weather
//!
//! 2. DYNAMIC PRESSURE
//!    - q = 0.5 * rho * v^2 (rho = air density, v = velocity)
//!    - Determines aerodynamic loads on the aircraft
//!    - Airspeed indicators measure this!
//!
//! 3. CABIN PRESSURE
//!    - Aircraft cabins are pressurized for passenger comfort
//!    - Typically maintained at ~8,000 ft equivalent (~75 kPa)
//!    - Differential pressure: 8-9 psi between cabin and outside
//!
//! 4. ENGINE/ROCKET PRESSURES
//!    - Combustion chamber pressure: 7-25 MPa for rockets
//!    - Higher pressure = better efficiency (usually)
//!
//! PRESSURE UNITS USED IN AVIATION:
//! --------------------------------
//! | Unit | Symbol | Value | Usage |
//! |------|--------|-------|-------|
//! | Hectopascal | hPa | 100 Pa | ICAO standard (same as mbar) |
//! | Millibar | mbar | 100 Pa | Weather, outside USA |
//! | Inches of Mercury | inHg | 3386.39 Pa | USA altimeter setting |
//! | Pounds per Square Inch | psi | 6894.76 Pa | US engineering |
//! | Standard Atmosphere | atm | 101,325 Pa | Reference |
//!
//! ALTIMETER SETTING:
//! ------------------
//! Pilots set their altimeter to the local barometric pressure (QNH).
//! "Altimeter 29.92" means set to standard pressure (29.92 inHg).
//! Above transition altitude, everyone uses 29.92 (standard pressure altitude).
//!
//! =============================================================================
//! RUST CONCEPT: Static Factory Methods
//! =============================================================================
//!
//! `Pressure::seaLevel()` is a static factory method that returns a
//! pre-defined value. This is useful for common reference values:
//!
//! ```rust,ignore
//! impl Pressure {
//!     pub fn sea_level() -> Self {
//!         Self::from_atmospheres(1.0)
//!     }
//! }
//! ```

use std::fmt;
use std::ops::{Add, Div, Mul, Sub};

// =============================================================================
// PRESSURE STRUCT
// =============================================================================
/// Pressure quantity - stores value in Pascals internally.
///
/// # Pascal: The SI Unit of Pressure
///
/// 1 Pascal = 1 Newton per square meter = 1 kg/(m*s^2)
///
/// This is a relatively small unit (atmospheric pressure is ~101,325 Pa),
/// so kilopascals (kPa) and hectopascals (hPa) are commonly used.
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub struct Pressure {
    pascals: f64,
}

impl Pressure {
    // =========================================================================
    // CONSTRUCTORS
    // =========================================================================

    /// Create a Pressure from Pascals (SI derived unit).
    pub fn from_pascals(pa: f64) -> Self {
        Self { pascals: pa }
    }

    /// Create a Pressure from kilopascals.
    ///
    /// Common in general engineering. 1 atm = 101.325 kPa.
    pub fn from_kilopascals(kpa: f64) -> Self {
        Self {
            pascals: kpa * 1000.0,
        }
    }

    /// Create a Pressure from megapascals.
    ///
    /// AEROSPACE: Used for high-pressure systems like rocket combustion chambers.
    /// RS-25 (Space Shuttle Main Engine) runs at about 20.6 MPa chamber pressure.
    pub fn from_megapascals(mpa: f64) -> Self {
        Self {
            pascals: mpa * 1_000_000.0,
        }
    }

    /// Create a Pressure from bar.
    ///
    /// 1 bar = 100,000 Pa (close to 1 atm, easier to work with).
    /// Common in European weather reports and diving.
    pub fn from_bar(bar: f64) -> Self {
        Self {
            pascals: bar * 100_000.0,
        }
    }

    /// Create a Pressure from standard atmospheres.
    ///
    /// AEROSPACE: The reference pressure for standard atmosphere.
    /// 1 atm = 101,325 Pa exactly (by definition).
    ///
    /// Note: This is "standard atmosphere" as a unit, not to be confused
    /// with the International Standard Atmosphere (ISA) model.
    pub fn from_atmospheres(atm: f64) -> Self {
        Self {
            pascals: atm * 101_325.0,
        }
    }

    /// Create a Pressure from pounds per square inch.
    ///
    /// AEROSPACE: Common in US engineering, especially for:
    /// - Tire pressures
    /// - Cabin differential pressure
    /// - Hydraulic systems
    ///
    /// Conversion: 1 psi = 6894.757 Pa
    pub fn from_psi(psi: f64) -> Self {
        Self {
            pascals: psi * 6894.757,
        }
    }

    /// Create a Pressure from inches of mercury.
    ///
    /// AEROSPACE: THE standard for altimeter settings in the USA.
    /// Pilots hear "Altimeter two niner niner two" (29.92 inHg) constantly.
    ///
    /// Conversion: 1 inHg = 3386.389 Pa
    ///
    /// Historical note: Mercury barometers were invented in 1643 by Torricelli.
    /// Standard atmospheric pressure supports a column of mercury 29.92 inches
    /// (760 mm) high. This is why these units persist in aviation.
    pub fn from_inches_hg(inhg: f64) -> Self {
        Self {
            pascals: inhg * 3386.389,
        }
    }

    /// Create a Pressure from millimeters of mercury (torr).
    ///
    /// Named after Torricelli. 1 torr = 1 mmHg (approximately).
    /// 760 mmHg = 1 atm (exactly, by definition of torr).
    pub fn from_mmhg(mmhg: f64) -> Self {
        Self {
            pascals: mmhg * 133.322,
        }
    }

    // =========================================================================
    // ACCESSORS
    // =========================================================================

    /// Get value in Pascals.
    pub fn as_pascals(&self) -> f64 {
        self.pascals
    }

    /// Get value in kilopascals.
    pub fn as_kilopascals(&self) -> f64 {
        self.pascals / 1000.0
    }

    /// Get value in megapascals.
    pub fn as_megapascals(&self) -> f64 {
        self.pascals / 1_000_000.0
    }

    /// Get value in bar.
    pub fn as_bar(&self) -> f64 {
        self.pascals / 100_000.0
    }

    /// Get value in standard atmospheres.
    pub fn as_atmospheres(&self) -> f64 {
        self.pascals / 101_325.0
    }

    /// Get value in pounds per square inch.
    pub fn as_psi(&self) -> f64 {
        self.pascals / 6894.757
    }

    /// Get value in inches of mercury.
    ///
    /// AEROSPACE: Use this for altimeter settings and weather reports in USA.
    pub fn as_inches_hg(&self) -> f64 {
        self.pascals / 3386.389
    }

    /// Get value in millimeters of mercury.
    pub fn as_mmhg(&self) -> f64 {
        self.pascals / 133.322
    }

    // =========================================================================
    // UTILITY METHODS
    // =========================================================================

    /// Check if this pressure is positive.
    ///
    /// Absolute pressure should always be positive (vacuum = 0).
    /// Gauge pressure can be negative (below atmospheric).
    pub fn is_positive(&self) -> bool {
        self.pascals > 0.0
    }

    /// Standard sea level pressure (1 standard atmosphere).
    ///
    /// AEROSPACE: This is the reference for the International Standard
    /// Atmosphere (ISA) model. It's used as the baseline for pressure
    /// altitude calculations.
    ///
    /// Value: 101,325 Pa = 1013.25 hPa = 29.92126 inHg
    pub fn sea_level() -> Self {
        Self::from_atmospheres(1.0)
    }
}

// =============================================================================
// OPERATOR IMPLEMENTATIONS
// =============================================================================

/// Pressure + Pressure = Pressure
impl Add for Pressure {
    type Output = Self;

    fn add(self, other: Self) -> Self {
        Self {
            pascals: self.pascals + other.pascals,
        }
    }
}

/// Pressure - Pressure = Pressure
///
/// AEROSPACE: Pressure difference is crucial for:
/// - Cabin differential pressure (inside - outside)
/// - Dynamic pressure calculations
/// - Barometric altitude changes
impl Sub for Pressure {
    type Output = Self;

    fn sub(self, other: Self) -> Self {
        Self {
            pascals: self.pascals - other.pascals,
        }
    }
}

/// Pressure * scalar = Pressure
impl Mul<f64> for Pressure {
    type Output = Self;

    fn mul(self, scalar: f64) -> Self {
        Self {
            pascals: self.pascals * scalar,
        }
    }
}

/// Pressure / scalar = Pressure
impl Div<f64> for Pressure {
    type Output = Self;

    fn div(self, scalar: f64) -> Self {
        Self {
            pascals: self.pascals / scalar,
        }
    }
}

/// Pressure / Pressure = dimensionless ratio (pressure ratio)
///
/// AEROSPACE: Pressure ratios are important for:
/// - Compressor performance (pressure ratio across compressor)
/// - Supersonic flow analysis (static/total pressure ratio)
/// - Altitude calculations (pressure at altitude / sea level pressure)
impl Div<Pressure> for Pressure {
    type Output = f64;

    fn div(self, other: Pressure) -> f64 {
        self.pascals / other.pascals
    }
}

/// Display implementation for human-readable output.
impl fmt::Display for Pressure {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        if self.pascals >= 1000.0 {
            write!(f, "{:.2} kPa", self.as_kilopascals())
        } else {
            write!(f, "{:.2} Pa", self.pascals)
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
        // 1 atm = 101325 Pa
        let p = Pressure::from_atmospheres(1.0);
        assert!((p.as_pascals() - 101325.0).abs() < 0.1);

        // 1 atm = 1.01325 bar
        assert!((p.as_bar() - 1.01325).abs() < 0.0001);

        // 1 atm = 14.696 psi
        assert!((p.as_psi() - 14.696).abs() < 0.01);
    }

    /// Test sea level standard pressure.
    ///
    /// AEROSPACE: These values must be accurate for altimeter calculations!
    #[test]
    fn test_sea_level() {
        let sea_level = Pressure::sea_level();

        // Standard values
        assert!((sea_level.as_pascals() - 101325.0).abs() < 0.1);
        assert!((sea_level.as_inches_hg() - 29.92).abs() < 0.01);
    }
}
