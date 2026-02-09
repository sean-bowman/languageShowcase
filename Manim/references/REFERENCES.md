# Aerospace Animations - References

## Manim Resources

### Official Documentation
- **Manim Community Docs:** https://docs.manim.community/en/stable/
- **Manim Community GitHub:** https://github.com/ManimCommunity/manim
- **3Blue1Brown's Original Manim:** https://github.com/3b1b/manim

### Tutorials
- **Quickstart Guide:** https://docs.manim.community/en/stable/tutorials/quickstart.html
- **Building Blocks:** https://docs.manim.community/en/stable/tutorials/building_blocks.html
- **Theorem of Beethoven (YouTube):** https://www.youtube.com/c/TheoremofBeethoven

### Key Concepts

#### Mobjects (Mathematical Objects)
- https://docs.manim.community/en/stable/reference/manim.mobject.mobject.Mobject.html
- Geometric: Circle, Rectangle, Line, Arrow, Polygon
- Text: Text, Tex, MathTex
- Groups: VGroup, Group

#### Animations
- https://docs.manim.community/en/stable/reference/manim.animation.animation.Animation.html
- Creation: Create, Write, FadeIn, FadeOut
- Movement: MoveAlongPath, Rotate, Shift
- Transformation: Transform, ReplacementTransform

#### Scene Types
- Scene: Standard 2D scene
- ThreeDScene: 3D visualization with camera control
- MovingCameraScene: Animated camera movement

## Aerospace Physics References

### ISA Standard Atmosphere
- **ISO 2533:1975:** International Standard Atmosphere specification
- **Engineering LibreTexts:** https://eng.libretexts.org/Bookshelves/Aerospace_Engineering/Fundamentals_of_Aerospace_Engineering_(Arnedo)/02:_Generalities/2.03:_Standard_atmosphere

#### ISA Layer Data
| Layer | Altitude (km) | Lapse Rate (K/km) | Base Temp (K) |
|-------|--------------|-------------------|---------------|
| Troposphere | 0-11 | -6.5 | 288.15 |
| Tropopause | 11-20 | 0 | 216.65 |
| Stratosphere | 20-32 | +1.0 | 216.65 |
| Stratosphere 2 | 32-47 | +2.8 | 228.65 |
| Stratopause | 47-51 | 0 | 270.65 |
| Mesosphere | 51-71 | -2.8 | 270.65 |
| Mesosphere 2 | 71-85 | -2.0 | 214.65 |

### Orbital Mechanics
- **Hohmann Transfer:** https://orbital-mechanics.space/orbital-maneuvers/hohmann-transfer.html
- **Vis-Viva Equation:** https://en.wikipedia.org/wiki/Vis-viva_equation
- **Orbital Elements:** https://en.wikipedia.org/wiki/Orbital_elements

#### Key Equations

**Circular Orbital Velocity:**
```
v_circ = sqrt(μ/r)
```

**Escape Velocity:**
```
v_esc = sqrt(2μ/r) = sqrt(2) × v_circ
```

**Vis-Viva Equation:**
```
v² = μ(2/r - 1/a)
```

**Hohmann Transfer:**
```
Δv₁ = sqrt(μ/r₁) × (sqrt(2r₂/(r₁+r₂)) - 1)
Δv₂ = sqrt(μ/r₂) × (1 - sqrt(2r₁/(r₁+r₂)))
t = π × sqrt(a³/μ)
```

### Rocket Propulsion
- **Tsiolkovsky Equation:** https://en.wikipedia.org/wiki/Tsiolkovsky_rocket_equation
- **Specific Impulse:** https://www.grc.nasa.gov/www/k-12/airplane/specimp.html

#### Rocket Equation
```
Δv = v_e × ln(m₀/m_f)
Δv = I_sp × g₀ × ln(m₀/m_f)
```

Where:
- v_e = Exhaust velocity
- I_sp = Specific impulse (seconds)
- g₀ = Standard gravity (9.80665 m/s²)
- m₀ = Initial mass
- m_f = Final (dry) mass

## Physical Constants

| Constant | Value | Unit |
|----------|-------|------|
| Earth GM (μ) | 3.986004418 × 10¹⁴ | m³/s² |
| Earth Radius | 6,371 | km |
| Standard Gravity (g₀) | 9.80665 | m/s² |
| Gas Constant (R_air) | 287.058 | J/(kg·K) |
| Specific Heat Ratio (γ) | 1.4 | - |
| ISA Sea Level Temp | 288.15 | K |
| ISA Sea Level Pressure | 101,325 | Pa |

## Video Production

### FFmpeg
- **Official Site:** https://ffmpeg.org/
- **Documentation:** https://ffmpeg.org/documentation.html

### Quality Settings
| Flag | Resolution | FPS | Use Case |
|------|-----------|-----|----------|
| -ql | 480p | 15 | Quick preview |
| -qm | 720p | 30 | Medium quality |
| -qh | 1080p | 60 | High quality |
| -qk | 2160p | 60 | 4K production |

## LaTeX for Equations

### Common Symbols
```latex
\mu          % Greek mu (gravitational parameter)
\Delta v     % Delta-v
\sqrt{}      % Square root
\frac{a}{b}  % Fraction
\ln          % Natural logarithm
\exp         % Exponential
```

### Resources
- **LaTeX Math Symbols:** https://oeis.org/wiki/List_of_LaTeX_mathematical_symbols
- **Overleaf Documentation:** https://www.overleaf.com/learn

## Color Palette

The animations use an aerospace-inspired dark theme:

| Element | Color | Hex |
|---------|-------|-----|
| Background | Dark Navy | #0d1b2a |
| Troposphere | Blue | #4a90d9 |
| Stratosphere | Green | #4aff4a |
| Mesosphere | Orange | #ff9040 |
| Highlight | Yellow | #ffff00 |
| Earth | Blue | #4080ff |
| Rocket | Red | #ff4040 |

## Similar Educational Content

### 3Blue1Brown Videos
- Essence of Linear Algebra
- Essence of Calculus
- Physics animations

### Aerospace Education
- **NASA Glenn Research Center:** https://www.grc.nasa.gov/www/k-12/
- **ESA Education:** https://www.esa.int/Education
