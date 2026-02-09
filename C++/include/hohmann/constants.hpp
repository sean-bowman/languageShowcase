#ifndef HOHMANN_CONSTANTS_HPP
#define HOHMANN_CONSTANTS_HPP

/*
 * constants.hpp - Physical constants for orbital mechanics calculations
 */

namespace hohmann {

/// Mathematical constants
namespace math {
    constexpr double pi = 3.14159265358979323846;
    constexpr double twoPi = 2.0 * pi;
}

/// Physical constants
namespace physics {
    /// Universal gravitational constant [m³/(kg·s²)]
    constexpr double G = 6.67430e-11;

    /// Standard gravity at Earth's surface [m/s²]
    constexpr double g0 = 9.80665;
}

/// Gravitational parameters (GM) for various bodies [m³/s²]
/// Source: NASA JPL planetary fact sheets
namespace gm {
    constexpr double sun = 1.32712440018e20;
    constexpr double mercury = 2.2032e13;
    constexpr double venus = 3.24859e14;
    constexpr double earth = 3.986004418e14;
    constexpr double moon = 4.9048695e12;
    constexpr double mars = 4.282837e13;
    constexpr double jupiter = 1.26686534e17;
    constexpr double saturn = 3.7931187e16;
    constexpr double uranus = 5.793939e15;
    constexpr double neptune = 6.836529e15;
}

/// Mean orbital radii [m]
/// Source: NASA JPL planetary fact sheets
namespace orbitalRadius {
    constexpr double mercury = 5.791e10;
    constexpr double venus = 1.082e11;
    constexpr double earth = 1.496e11;  // 1 AU
    constexpr double mars = 2.279e11;
    constexpr double jupiter = 7.785e11;
    constexpr double saturn = 1.432e12;
    constexpr double uranus = 2.867e12;
    constexpr double neptune = 4.515e12;
}

/// Body radii [m]
namespace bodyRadius {
    constexpr double sun = 6.9634e8;
    constexpr double earth = 6.371e6;
    constexpr double moon = 1.7374e6;
    constexpr double mars = 3.3895e6;
}

} // namespace hohmann

#endif // HOHMANN_CONSTANTS_HPP
