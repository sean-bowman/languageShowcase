# Aerospace Animations - Manim

Mathematical animations explaining aerospace engineering concepts using Manim (the library created by 3Blue1Brown).

## Overview

This project creates educational animations covering:
- **ISA Atmosphere**: Temperature and pressure profiles through atmospheric layers
- **Hohmann Transfers**: Orbital maneuver derivation and visualization
- **Rocket Equation**: Tsiolkovsky equation and mass ratio concepts
- **Orbital Basics**: Velocity equations, escape velocity, orbital elements

## Why Manim?

Manim (Mathematical Animation Engine) is ideal for aerospace education:
- **Mathematical rigor**: LaTeX integration for precise equations
- **Visual clarity**: Smooth animations that build understanding
- **Programmatic**: Reproducible, version-controlled animations
- **Popular**: Same tool used by 3Blue1Brown for millions of learners

## Prerequisites

### Required
- **Python 3.8+** from [python.org](https://python.org)
- **FFmpeg** for video rendering

### Installation

```bash
cd Manim

# Create virtual environment (recommended)
python -m venv venv
venv\Scripts\activate  # Windows
# source venv/bin/activate  # Linux/Mac

# Install dependencies
pip install -r requirements.txt
```

### FFmpeg Installation (Windows)
1. Download from [ffmpeg.org](https://ffmpeg.org/download.html)
2. Extract to `C:\ffmpeg`
3. Add `C:\ffmpeg\bin` to system PATH

## Rendering Animations

### Basic Usage

```bash
# Low quality preview (fast)
manim -pql scenes/hohmann_transfer.py HohmannVisualizationScene

# High quality render
manim -pqh scenes/hohmann_transfer.py HohmannVisualizationScene

# 4K quality
manim -qk scenes/hohmann_transfer.py HohmannVisualizationScene
```

### Available Scenes

#### ISA Atmosphere (`scenes/isa_atmosphere.py`)
- `ISAIntroScene` - Introduction to standard atmosphere
- `ISALayersScene` - Visualize atmospheric layers
- `ISAEquationsScene` - Temperature and pressure equations
- `ISAProfileScene` - Animated temperature profile

#### Hohmann Transfer (`scenes/hohmann_transfer.py`)
- `HohmannIntroScene` - Introduction to transfer orbits
- `HohmannVisualizationScene` - Animated transfer maneuver
- `HohmannDerivationScene` - Derive delta-v equations
- `HohmannExampleScene` - LEO to GEO numerical example

#### Rocket Equation (`scenes/rocket_equation.py`)
- `RocketEquationIntroScene` - Tsiolkovsky equation introduction
- `RocketVisualScene` - Propellant depletion visualization
- `MassRatioExponentialScene` - Exponential nature of rockets
- `StagingConceptScene` - Why staging helps

#### Orbital Basics (`scenes/orbital_basics.py`)
- `OrbitalVelocityScene` - Circular velocity derivation
- `EscapeVelocityScene` - Escape velocity derivation
- `OrbitalElementsScene` - Classical orbital elements
- `OrbitTypesScene` - Orbits by eccentricity

### Render All Scenes

```bash
# Render all scenes in a file
manim -pql scenes/isa_atmosphere.py --write_all
```

## Project Structure

```
Manim/
├── requirements.txt        # Python dependencies
├── manim.cfg               # Manim configuration
├── README.md
├── scenes/
│   ├── isa_atmosphere.py   # ISA animations
│   ├── hohmann_transfer.py # Transfer orbit animations
│   ├── rocket_equation.py  # Rocket equation animations
│   └── orbital_basics.py   # Orbital mechanics basics
├── components/
│   ├── orbit.py            # Orbit Mobjects
│   ├── atmosphere.py       # Atmosphere visuals
│   ├── equations.py        # Common equations
│   └── spacecraft.py       # Spacecraft graphics
├── data/
│   └── constants.py        # Physical constants
├── media/                  # Output directory (generated)
│   └── videos/
└── references/
    └── REFERENCES.md
```

## Key Manim Concepts Demonstrated

### 1. Scene Structure
```python
class MyScene(Scene):
    def construct(self):
        # All animations go here
        title = Text("Hello Aerospace")
        self.play(Write(title))
        self.wait(1)
```

### 2. Mathematical Typesetting
```python
equation = MathTex(r"v = \sqrt{\frac{\mu}{r}}")
self.play(Write(equation))
```

### 3. Geometric Objects
```python
orbit = Circle(radius=2, color=BLUE)
earth = Dot(ORIGIN, color=BLUE)
self.play(Create(orbit), Create(earth))
```

### 4. Animations
```python
# Transform one object into another
self.play(Transform(circle, square))

# Move along a path
self.play(MoveAlongPath(spacecraft, orbit))

# Fade in/out
self.play(FadeIn(text, shift=UP))
```

### 5. Value Trackers for Dynamic Updates
```python
tracker = ValueTracker(0)
number = DecimalNumber(0)
number.add_updater(lambda m: m.set_value(tracker.get_value()))
self.play(tracker.animate.set_value(100))
```

## Configuration

Edit `manim.cfg` to customize:
- Output quality and frame rate
- Background color
- Output directory
- Preview settings

```ini
[CLI]
quality = medium_quality
frame_rate = 60
background_color = #0d1b2a
preview = True
```

## Output

Rendered videos are saved to `media/videos/`:
- `480p15/` - Low quality
- `720p30/` - Medium quality
- `1080p60/` - High quality
- `2160p60/` - 4K quality

## Aerospace Concepts Covered

### ISA Standard Atmosphere
- Temperature lapse rates by layer
- Pressure calculation (gradient vs isothermal)
- Density from ideal gas law

### Orbital Mechanics
- Circular and escape velocities
- Vis-viva equation
- Hohmann transfer delta-v
- Classical orbital elements

### Rocket Propulsion
- Tsiolkovsky rocket equation
- Mass ratio implications
- Staging advantages

## Dependencies

- **manim** (>=0.18.0): Animation engine
- **numpy**: Numerical calculations
- **scipy**: Scientific computing

## References

See [references/REFERENCES.md](references/REFERENCES.md) for technical resources.

- [Manim Community Documentation](https://docs.manim.community/)
- [3Blue1Brown YouTube](https://www.youtube.com/c/3blue1brown)
- [Manim GitHub](https://github.com/ManimCommunity/manim)
