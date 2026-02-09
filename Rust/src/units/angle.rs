//! # Angle Unit Type
//!
//! Stores angle internally in radians (SI derived unit).
//!
//! =============================================================================
//! AEROSPACE CONCEPT: Angles in Aviation and Spaceflight
//! =============================================================================
//!
//! Angles are fundamental to aerospace:
//!
//! AIRCRAFT ATTITUDE ANGLES:
//! -------------------------
//! ```text
//!                 Yaw (Heading)
//!                      |
//!                      v
//!                    .-'-.
//!                   /     \
//!     Roll <------[Aircraft]------> Roll
//!                   \     /
//!                    '-.-'
//!                      |
//!                      v
//!                    Pitch
//! ```
//! - Pitch: Nose up/down (rotation about lateral axis)
//! - Roll (Bank): Wings tilting (rotation about longitudinal axis)
//! - Yaw: Nose left/right (rotation about vertical axis)
//!
//! NAVIGATION ANGLES:
//! ------------------
//! - Heading: Direction aircraft nose points (0-360 deg, 0=North)
//! - Track: Actual path over ground (may differ from heading due to wind)
//! - Bearing: Direction TO a point from current position
//!
//! GEOGRAPHIC COORDINATES:
//! -----------------------
//! - Latitude: -90 to +90 degrees (equator = 0)
//! - Longitude: -180 to +180 degrees (Prime Meridian = 0)
//! - Subdivisions: 1 degree = 60 arcminutes = 3600 arcseconds
//!
//! Example: New York JFK Airport
//!   40deg 38' 23" N, 73deg 46' 44" W
//!   = 40.6397deg N, 73.7789deg W
//!
//! ANGLE UNITS:
//! ------------
//! | Unit | Symbol | Relation to radians | Usage |
//! |------|--------|---------------------|-------|
//! | Radian | rad | 1 | Mathematics, physics |
//! | Degree | deg | pi/180 | Navigation, common use |
//! | Arcminute | ' | pi/10800 | Precise positions |
//! | Arcsecond | " | pi/648000 | Very precise positions |
//! | Revolution | rev | 2*pi | Rotational systems |
//!
//! WHY RADIANS?
//! ------------
//! Radians are "natural" for calculus:
//! - d/dx[sin(x)] = cos(x) only when x is in radians
//! - Arc length = radius * angle (when angle in radians)
//! - Small angle: sin(x) ~ x (when x in radians)
//!
//! =============================================================================
//! RUST CONCEPT: Importing Standard Constants
//! =============================================================================
//!
//! `use std::f64::consts::PI;`
//!
//! Rust provides mathematical constants in std::f64::consts:
//! - PI: 3.14159265358979...
//! - E: Euler's number 2.71828...
//! - FRAC_PI_2: pi/2
//! - TAU: 2*pi (full circle)
//!
//! These are compile-time constants with full f64 precision.
//!
//! =============================================================================
//! RUST CONCEPT: The Neg Trait for Unary Minus
//! =============================================================================
//!
//! Besides Add, Sub, Mul, Div, there's also Neg for unary negation:
//! `impl Neg for Angle` enables `-angle` syntax.

use std::f64::consts::PI;
use std::fmt;
use std::ops::{Add, Div, Mul, Neg, Sub};

// =============================================================================
// ANGLE STRUCT
// =============================================================================
/// Angle quantity - stores value in radians internally.
///
/// # Why Radians?
///
/// Radians are the natural unit for angles in mathematics:
/// - Derivatives of trig functions are simple (d/dx sin(x) = cos(x))
/// - Arc length = radius * angle
/// - Small angle approximations work directly
///
/// All conversions are done through radians as the common base.
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub struct Angle {
    radians: f64,
}

impl Angle {
    // =========================================================================
    // CONSTRUCTORS
    // =========================================================================

    /// Create an Angle from radians.
    ///
    /// # Example
    /// ```
    /// use aerospace_units::prelude::*;
    /// use std::f64::consts::PI;
    /// let right_angle = Angle::from_radians(PI / 2.0);
    /// ```
    pub fn from_radians(rad: f64) -> Self {
        Self { radians: rad }
    }

    /// Create an Angle from degrees.
    ///
    /// AEROSPACE: Degrees are the primary unit for pilots and navigation.
    /// Headings, bearings, and lat/lon are all in degrees.
    ///
    /// Conversion: 1 degree = pi/180 radians
    pub fn from_degrees(deg: f64) -> Self {
        Self {
            radians: deg * PI / 180.0,
        }
    }

    /// Create an Angle from arcminutes.
    ///
    /// AEROSPACE: Used in precise navigation and coordinates.
    /// 1 arcminute of latitude = 1 nautical mile (approximately)!
    ///
    /// Conversion: 1 arcminute = 1/60 degree = pi/10800 radians
    pub fn from_arcminutes(arcmin: f64) -> Self {
        Self {
            radians: arcmin * PI / (180.0 * 60.0),
        }
    }

    /// Create an Angle from arcseconds.
    ///
    /// AEROSPACE: Used for very precise positioning.
    /// GPS accuracy is often quoted in arcseconds.
    ///
    /// Conversion: 1 arcsecond = 1/3600 degree = pi/648000 radians
    pub fn from_arcseconds(arcsec: f64) -> Self {
        Self {
            radians: arcsec * PI / (180.0 * 3600.0),
        }
    }

    /// Create an Angle from revolutions (turns).
    ///
    /// AEROSPACE: Useful for rotational systems like gyroscopes.
    /// Also called "turns" or "cycles".
    ///
    /// Conversion: 1 revolution = 2*pi radians = 360 degrees
    pub fn from_revolutions(rev: f64) -> Self {
        Self {
            radians: rev * 2.0 * PI,
        }
    }

    // =========================================================================
    // ACCESSORS
    // =========================================================================

    /// Get value in radians.
    pub fn as_radians(&self) -> f64 {
        self.radians
    }

    /// Get value in degrees.
    ///
    /// AEROSPACE: Use this for displaying headings, bearings, and coordinates.
    pub fn as_degrees(&self) -> f64 {
        self.radians * 180.0 / PI
    }

    /// Get value in arcminutes.
    pub fn as_arcminutes(&self) -> f64 {
        self.radians * 180.0 * 60.0 / PI
    }

    /// Get value in arcseconds.
    pub fn as_arcseconds(&self) -> f64 {
        self.radians * 180.0 * 3600.0 / PI
    }

    /// Get value in revolutions.
    pub fn as_revolutions(&self) -> f64 {
        self.radians / (2.0 * PI)
    }

    // =========================================================================
    // TRIGONOMETRIC METHODS
    // =========================================================================
    // RUST CONCEPT: Method Forwarding
    //
    // These methods delegate to f64's built-in trig functions.
    // The angle is already in radians (what f64::sin expects), so we
    // just pass it through. This is "method forwarding" - our type
    // provides a convenient interface to the underlying operations.

    /// Compute the sine of this angle.
    ///
    /// AEROSPACE: Used extensively in force resolution.
    /// Example: Vertical component of lift = Lift * sin(bank_angle)
    pub fn sin(&self) -> f64 {
        self.radians.sin()
    }

    /// Compute the cosine of this angle.
    ///
    /// AEROSPACE: Used in coordinate transformations and projections.
    /// Example: Horizontal component of lift = Lift * cos(bank_angle)
    pub fn cos(&self) -> f64 {
        self.radians.cos()
    }

    /// Compute the tangent of this angle.
    ///
    /// AEROSPACE: Used in glide slope and climb/descent calculations.
    /// Glide angle: tan(gamma) = descent_rate / ground_speed
    pub fn tan(&self) -> f64 {
        self.radians.tan()
    }

    // =========================================================================
    // UTILITY METHODS
    // =========================================================================

    /// Normalize angle to [0, 2*pi) range.
    ///
    /// AEROSPACE: Headings are always 0-360 (or 0 to 2*pi).
    /// This ensures angles wrap correctly.
    ///
    /// Example: 450 degrees -> 90 degrees
    ///          -90 degrees -> 270 degrees
    pub fn normalize(&self) -> Self {
        let mut rad = self.radians % (2.0 * PI);
        if rad < 0.0 {
            rad += 2.0 * PI;
        }
        Self { radians: rad }
    }

    /// Normalize angle to [-pi, pi) range.
    ///
    /// AEROSPACE: Useful when you need signed angles.
    /// Example: Error from target heading (turn left vs right)
    ///
    /// Example: 270 degrees -> -90 degrees
    ///          190 degrees -> -170 degrees
    pub fn normalize_signed(&self) -> Self {
        let mut rad = self.radians % (2.0 * PI);
        if rad >= PI {
            rad -= 2.0 * PI;
        } else if rad < -PI {
            rad += 2.0 * PI;
        }
        Self { radians: rad }
    }

    /// Get absolute value of angle.
    pub fn abs(&self) -> Self {
        Self {
            radians: self.radians.abs(),
        }
    }
}

// =============================================================================
// OPERATOR IMPLEMENTATIONS
// =============================================================================

/// Angle + Angle = Angle
impl Add for Angle {
    type Output = Self;

    fn add(self, other: Self) -> Self {
        Self {
            radians: self.radians + other.radians,
        }
    }
}

/// Angle - Angle = Angle
///
/// AEROSPACE: Angle differences are used for:
/// - Heading error (current heading - desired heading)
/// - Phase angles in orbital mechanics
impl Sub for Angle {
    type Output = Self;

    fn sub(self, other: Self) -> Self {
        Self {
            radians: self.radians - other.radians,
        }
    }
}

/// -Angle (unary negation)
///
/// RUST CONCEPT: The Neg Trait
/// ---------------------------
/// Implementing Neg allows using the unary minus operator.
/// `let opposite = -angle;` works just like with numbers.
impl Neg for Angle {
    type Output = Self;

    fn neg(self) -> Self {
        Self {
            radians: -self.radians,
        }
    }
}

/// Angle * scalar = Angle
impl Mul<f64> for Angle {
    type Output = Self;

    fn mul(self, scalar: f64) -> Self {
        Self {
            radians: self.radians * scalar,
        }
    }
}

/// Angle / scalar = Angle
impl Div<f64> for Angle {
    type Output = Self;

    fn div(self, scalar: f64) -> Self {
        Self {
            radians: self.radians / scalar,
        }
    }
}

/// Angle / Angle = dimensionless ratio
impl Div<Angle> for Angle {
    type Output = f64;

    fn div(self, other: Angle) -> f64 {
        self.radians / other.radians
    }
}

/// Display implementation showing degrees (more human-readable).
///
/// Note: We show degrees with the degree symbol for readability,
/// even though we store radians internally.
impl fmt::Display for Angle {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(f, "{:.2} deg", self.as_degrees())
    }
}

// =============================================================================
// UNIT TESTS
// =============================================================================
#[cfg(test)]
mod tests {
    use super::*;

    /// Test degree/radian conversions.
    #[test]
    fn test_conversions() {
        // 180 degrees = pi radians
        let angle = Angle::from_degrees(180.0);
        assert!((angle.as_radians() - PI).abs() < 0.0001);

        // 90 degrees should have sin=1, cos=0
        let right_angle = Angle::from_degrees(90.0);
        assert!((right_angle.sin() - 1.0).abs() < 0.0001);
        assert!(right_angle.cos().abs() < 0.0001);
    }

    /// Test angle normalization.
    ///
    /// AEROSPACE: Heading wraparound is critical for navigation.
    #[test]
    fn test_normalize() {
        // 450 degrees -> 90 degrees
        let angle = Angle::from_degrees(450.0);
        let normalized = angle.normalize();
        assert!((normalized.as_degrees() - 90.0).abs() < 0.0001);

        // -90 degrees -> 270 degrees
        let negative = Angle::from_degrees(-90.0);
        let norm_neg = negative.normalize();
        assert!((norm_neg.as_degrees() - 270.0).abs() < 0.0001);
    }

    /// Test arcsecond conversion.
    ///
    /// AEROSPACE: Important for GPS and precise navigation.
    #[test]
    fn test_arcseconds() {
        // 1 degree = 3600 arcseconds
        let deg = Angle::from_degrees(1.0);
        assert!((deg.as_arcseconds() - 3600.0).abs() < 0.0001);
    }
}
