// =============================================================================
// ORBITAL MECHANICS VISUALIZATION - MAIN ENTRY POINT
// =============================================================================
//
// AEROSPACE CONCEPT: Interactive Orbital Visualization
// =====================================================
//
// This application visualizes Hohmann transfer orbits - the most fuel-efficient
// way to transfer between two circular orbits around a central body.
//
// WHAT THIS VISUALIZATION SHOWS:
// -----------------------------
//
//   1. INITIAL ORBIT (Blue)
//      - Starting circular orbit (e.g., LEO at 400 km)
//      - Spacecraft travels at circular velocity v = sqrt(mu/r)
//
//   2. TRANSFER ORBIT (Orange)
//      - Elliptical path connecting the two orbits
//      - Periapsis (closest point) touches initial orbit
//      - Apoapsis (farthest point) touches final orbit
//
//   3. FINAL ORBIT (Green)
//      - Destination circular orbit (e.g., GEO at 35,786 km)
//      - Lower velocity than initial orbit (counterintuitive!)
//
// THE HOHMANN MANEUVER SEQUENCE:
// -----------------------------
//
//   Phase 1: Coast in initial orbit
//      |
//      v
//   BURN #1: Increase velocity --> Enter transfer ellipse
//      |
//      v
//   Phase 2: Coast along transfer ellipse (half orbit)
//      |
//      v
//   BURN #2: Increase velocity --> Circularize at final orbit
//      |
//      v
//   Phase 3: Coast in final orbit
//
// =============================================================================
// JAVASCRIPT CONCEPTS IN THIS FILE
// =============================================================================
//
// 1. ES6 MODULES - NAMED IMPORTS
//    `import { SceneManager } from './scene/SceneManager.js';`
//    Imports specific exports by name.
//
// 2. ES6 CLASS
//    `class OrbitalVisualization { ... }`
//    Modern syntax for object-oriented programming.
//
// 3. DOM MANIPULATION
//    `document.getElementById('id')`, `element.textContent`, `element.value`
//    Interacting with HTML elements.
//
// 4. EVENT LISTENERS
//    `element.addEventListener('click', () => { ... })`
//    Responding to user interactions.
//
// 5. DOMCONTENTLOADED EVENT
//    `document.addEventListener('DOMContentLoaded', () => { ... })`
//    Ensures DOM is ready before running code.
//
// 6. INPUT VALIDATION
//    `parseFloat()`, `isNaN()`, type checking
//    Validating user input before processing.
//
// =============================================================================

import { SceneManager } from './scene/SceneManager.js';
import { Earth } from './bodies/Earth.js';
import { Spacecraft } from './bodies/Spacecraft.js';
import { OrbitRenderer } from './orbits/OrbitRenderer.js';
import { COLORS, ORBITS } from './utils/constants.js';
import {
    hohmannTransfer,
    circularVelocity,
    orbitalPeriod,
    formatTime,
    formatVelocity,
    formatAltitude
} from './utils/orbital-math.js';

/**
 * Main application class for orbital visualization.
 *
 * @description
 * This class orchestrates all components of the visualization:
 * - SceneManager: Three.js scene, camera, renderer
 * - Earth: 3D Earth model at the center
 * - OrbitRenderer: Draws circular and elliptical orbits
 * - Spacecraft: Animated object following orbital paths
 *
 * DESIGN PATTERN: This is a "Mediator" pattern - this class coordinates
 * communication between components that don't know about each other.
 */
class OrbitalVisualization {
    /**
     * Initialize the visualization application.
     *
     * @description
     * JAVASCRIPT: Class constructors are called when using the `new` keyword.
     * The constructor should initialize all instance properties and call
     * any necessary setup methods.
     */
    constructor() {
        // =====================================================================
        // COMPONENT REFERENCES
        // =====================================================================
        // These will be initialized in init()
        // JAVASCRIPT: null indicates "intentionally empty"
        this.sceneManager = null;
        this.earth = null;
        this.spacecraft = null;
        this.orbitRenderer = null;

        // =====================================================================
        // ORBITAL PARAMETERS
        // =====================================================================
        // AEROSPACE: Default to LEO->GEO transfer (a common mission profile)
        // LEO (Low Earth Orbit): 400 km - ISS altitude
        // GEO (Geostationary): 35,786 km - communications satellites
        this.initialAltitude = ORBITS.LEO;    // 400 km
        this.finalAltitude = ORBITS.GEO;      // 35,786 km

        // Store calculated transfer parameters
        this.transferParams = null;

        // =====================================================================
        // ANIMATION STATE
        // =====================================================================
        this.isAnimating = false;

        // Initialize the application
        this.init();
    }

    /**
     * Initialize all application components.
     *
     * @description
     * INITIALIZATION ORDER:
     * 1. SceneManager (must be first - provides the scene)
     * 2. Earth (adds to scene)
     * 3. OrbitRenderer (needs scene reference)
     * 4. Spacecraft (needs orbitRenderer for positioning)
     * 5. Initial calculation and visualization
     * 6. UI controls setup
     * 7. Animation callback registration
     */
    init() {
        // Get the container element from HTML
        // JAVASCRIPT: getElementById returns the DOM element or null
        const container = document.getElementById('canvas-container');

        // Create the Three.js scene manager
        this.sceneManager = new SceneManager(container);

        // Create Earth at the center of the scene
        // JAVASCRIPT: Passing scene reference allows Earth to add itself
        this.earth = new Earth(this.sceneManager.scene);

        // Create the orbit rendering component
        this.orbitRenderer = new OrbitRenderer(this.sceneManager.scene);

        // Create the animated spacecraft
        this.spacecraft = new Spacecraft(this.sceneManager.scene);

        // Connect spacecraft to orbit renderer for position queries
        this.spacecraft.setOrbitRenderer(this.orbitRenderer);

        // Calculate transfer and create initial visualization
        this.calculateAndVisualize();

        // Setup UI control event handlers
        this.setupControls();

        // Register our update function with the animation loop
        // JAVASCRIPT: Arrow function preserves `this` context
        this.sceneManager.addAnimationCallback((delta) => this.update(delta));
    }

    /**
     * Calculate Hohmann transfer and update visualization.
     *
     * @description
     * This method:
     * 1. Calculates transfer parameters (delta-v, time, etc.)
     * 2. Clears existing orbit visualizations
     * 3. Creates new orbit visualizations
     * 4. Positions spacecraft at start of initial orbit
     * 5. Updates the info panel with calculated values
     */
    calculateAndVisualize() {
        // Calculate transfer parameters using orbital mechanics
        // This returns an object with all the velocities, delta-v values, etc.
        this.transferParams = hohmannTransfer(this.initialAltitude, this.finalAltitude);

        // Clear any existing orbit visualizations
        this.orbitRenderer.clearAllOrbits();

        // =====================================================================
        // CREATE ORBIT VISUALIZATIONS
        // =====================================================================

        // Initial circular orbit (blue)
        this.orbitRenderer.createCircularOrbit(
            'initial',                   // Unique ID for this orbit
            this.initialAltitude,        // Altitude in km
            COLORS.INITIAL_ORBIT,        // Blue color
            { opacity: 0.8 }             // Options object
        );

        // Final circular orbit (green)
        this.orbitRenderer.createCircularOrbit(
            'final',
            this.finalAltitude,
            COLORS.FINAL_ORBIT,
            { opacity: 0.8 }
        );

        // Transfer ellipse (orange)
        // halfOrbit: true shows only the transfer portion (not the unused half)
        this.orbitRenderer.createTransferOrbit(
            'transfer',
            this.initialAltitude,
            this.finalAltitude,
            COLORS.TRANSFER_ORBIT,
            { opacity: 0.6, halfOrbit: true }
        );

        // Position spacecraft at the start of the initial orbit
        this.spacecraft.reset();
        const initialPos = this.orbitRenderer.getPositionOnOrbit('initial', 0);
        this.spacecraft.setPosition(initialPos);

        // Update the info panel with calculated values
        this.updateInfoPanel();
    }

    /**
     * Update the HTML info panel with current orbital parameters.
     *
     * @description
     * JAVASCRIPT: DOM manipulation using getElementById and textContent.
     *
     * This displays:
     * - Initial orbit: altitude, velocity, period
     * - Final orbit: altitude, velocity, period
     * - Transfer: delta-v for each burn, total delta-v, transfer time
     */
    updateInfoPanel() {
        const params = this.transferParams;

        // =====================================================================
        // INITIAL ORBIT INFORMATION
        // =====================================================================
        // JAVASCRIPT: getElementById gets element, textContent sets its text
        document.getElementById('initial-altitude').textContent =
            formatAltitude(this.initialAltitude);
        document.getElementById('initial-velocity').textContent =
            formatVelocity(params.vCircular1);
        document.getElementById('initial-period').textContent =
            formatTime(orbitalPeriod(this.initialAltitude));

        // =====================================================================
        // FINAL ORBIT INFORMATION
        // =====================================================================
        document.getElementById('final-altitude').textContent =
            formatAltitude(this.finalAltitude);
        document.getElementById('final-velocity').textContent =
            formatVelocity(params.vCircular2);
        document.getElementById('final-period').textContent =
            formatTime(orbitalPeriod(this.finalAltitude));

        // =====================================================================
        // TRANSFER PARAMETERS
        // =====================================================================
        // Delta-v values are what rockets actually care about!
        document.getElementById('delta-v1').textContent =
            formatVelocity(params.deltaV1);
        document.getElementById('delta-v2').textContent =
            formatVelocity(params.deltaV2);
        document.getElementById('total-delta-v').textContent =
            formatVelocity(params.totalDeltaV);
        document.getElementById('transfer-time').textContent =
            formatTime(params.transferTime);
    }

    /**
     * Setup UI control event handlers.
     *
     * @description
     * JAVASCRIPT: Event-driven programming using addEventListener.
     *
     * UI CONTROLS:
     * - Input fields for initial/final altitude
     * - Calculate button: recalculates with new values
     * - Animate button: starts/stops spacecraft animation
     * - Reset button: resets camera and spacecraft
     * - Speed slider: adjusts animation speed
     */
    setupControls() {
        // Get references to UI elements
        const inputInitial = document.getElementById('input-initial');
        const inputFinal = document.getElementById('input-final');
        const speedSlider = document.getElementById('speed-slider');

        const btnCalculate = document.getElementById('btn-calculate');
        const btnAnimate = document.getElementById('btn-animate');
        const btnReset = document.getElementById('btn-reset');

        // Set initial input values
        // JAVASCRIPT: Input elements have a .value property (always a string)
        inputInitial.value = this.initialAltitude;
        inputFinal.value = this.finalAltitude;

        // =====================================================================
        // CALCULATE BUTTON
        // =====================================================================
        // JAVASCRIPT: addEventListener attaches a callback to an event
        btnCalculate.addEventListener('click', () => {
            // Parse input values
            // JAVASCRIPT: parseFloat converts string to number
            const newInitial = parseFloat(inputInitial.value);
            const newFinal = parseFloat(inputFinal.value);

            // Input validation
            // JAVASCRIPT: isNaN (Not a Number) checks for invalid numbers
            if (isNaN(newInitial) || isNaN(newFinal)) {
                alert('Please enter valid numbers');
                return;  // Exit early
            }

            if (newInitial <= 0 || newFinal <= 0) {
                alert('Altitudes must be positive');
                return;
            }

            if (newInitial === newFinal) {
                alert('Initial and final altitudes must be different');
                return;
            }

            // Update altitudes and recalculate
            this.initialAltitude = newInitial;
            this.finalAltitude = newFinal;

            // Stop any running animation
            this.isAnimating = false;
            btnAnimate.textContent = 'Start Animation';
            btnAnimate.classList.remove('active');

            // Recalculate and redraw
            this.calculateAndVisualize();
        });

        // =====================================================================
        // ANIMATE BUTTON
        // =====================================================================
        // Toggle animation on/off
        btnAnimate.addEventListener('click', () => {
            // JAVASCRIPT: ! is the logical NOT operator (toggles boolean)
            this.isAnimating = !this.isAnimating;

            if (this.isAnimating) {
                // Start animation from beginning of initial orbit
                this.spacecraft.startAnimation('initial', 0);
                btnAnimate.textContent = 'Stop Animation';
                // JAVASCRIPT: classList.add adds a CSS class
                btnAnimate.classList.add('active');
            } else {
                this.spacecraft.stopAnimation();
                btnAnimate.textContent = 'Start Animation';
                btnAnimate.classList.remove('active');
            }
        });

        // =====================================================================
        // RESET BUTTON
        // =====================================================================
        btnReset.addEventListener('click', () => {
            // Reset camera to default view
            this.sceneManager.resetCamera();

            // Reset spacecraft to initial position
            this.spacecraft.reset();

            // Stop animation
            this.isAnimating = false;
            btnAnimate.textContent = 'Start Animation';
            btnAnimate.classList.remove('active');
        });

        // =====================================================================
        // SPEED SLIDER
        // =====================================================================
        // JAVASCRIPT: 'input' event fires continuously as slider moves
        speedSlider.addEventListener('input', (e) => {
            // e.target is the element that triggered the event
            // parseFloat converts the string value to a number
            this.spacecraft.setAnimationSpeed(parseFloat(e.target.value));
        });
    }

    /**
     * Update callback called each animation frame.
     *
     * @description
     * This is called ~60 times per second by the SceneManager's animation loop.
     * It updates any animated objects in the scene.
     *
     * @param {number} delta - Time since last frame (in seconds, approximate)
     */
    update(delta) {
        // Update Earth rotation (slow spin for visual effect)
        this.earth.update(delta);

        // Update spacecraft position if animating
        if (this.isAnimating) {
            this.spacecraft.update(delta, this.transferParams);
        }
    }
}

// =============================================================================
// APPLICATION ENTRY POINT
// =============================================================================
//
// JAVASCRIPT: DOMContentLoaded fires when the HTML document has been fully
// parsed (but before images, stylesheets, etc. have loaded).
//
// This ensures we don't try to access DOM elements before they exist.
// Alternative: Place <script> tag at end of <body>.

document.addEventListener('DOMContentLoaded', () => {
    // Create the application instance
    // JAVASCRIPT: `new` creates an instance and calls the constructor
    new OrbitalVisualization();
});
